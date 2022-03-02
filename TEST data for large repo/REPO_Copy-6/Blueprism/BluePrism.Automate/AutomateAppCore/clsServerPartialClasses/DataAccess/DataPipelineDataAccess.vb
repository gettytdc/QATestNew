Imports System.Data.SqlClient
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Data
Imports BluePrism.DataPipeline
Imports BluePrism.DataPipeline.DataPipelineOutput
Imports BluePrism.Server.Domain.Models
Imports Newtonsoft.Json

Friend Class DataPipelineDataAccess
    Private ReadOnly mConnection As IDatabaseConnection
    Private ReadOnly mGetDefaultEncryptionFunc As Func(Of clsEncryptionScheme)
    Private ReadOnly mServer As clsServer
    Private ReadOnly mJsonSerializerSettings As New JsonSerializerSettings With {.TypeNameHandling = TypeNameHandling.All}

    Friend Sub New(connection As IDatabaseConnection, getDefaultEncryptionSchemeFunc As Func(Of clsEncryptionScheme), server As clsServer)
        mConnection = connection
        mGetDefaultEncryptionFunc = getDefaultEncryptionSchemeFunc
        mServer = server
    End Sub


    Friend Function RegisterDataPipelineProcess(name As String, tcpServer As String) As Integer

        Dim existingId = GetPipelineProcessId(name)
        If existingId <> -1 Then

            Dim updateCommand = New SqlCommand("update BPADataPipelineProcess set [status] = @status, tcpEndpoint = @tcpEndpoint where id = @id")
            updateCommand.Parameters.AddWithValue("@status", DataGatewayProcessState.Online)
            updateCommand.Parameters.AddWithValue("@tcpEndpoint", tcpServer)
            updateCommand.Parameters.AddWithValue("@id", existingId)
            mConnection.Execute(updateCommand)


            ' Check if process has a config assigned. If not, assign one if there is one available.
            ' This will need to change if in future we allow multiple configurations.
            If Not HasConfig(existingId) Then
                Dim defaultConfig = GetDefaultConfigId()
                If defaultConfig.HasValue Then
                    AssignConfigToPipelineProcess(existingId, defaultConfig.Value)
                End If

            End If
            Return existingId
        End If

        Dim configId = GetDefaultConfigId()
        mConnection.BeginTransaction()
        Dim insertProcessQuery = "insert into BPADataPipelineProcess([name], [status], config, tcpEndpoint) OUTPUT INSERTED.id values (@name, @status, @configId, @tcpEndpoint)"
        Dim command = New SqlCommand(insertProcessQuery)
        command.Parameters.AddWithValue("@name", name)
        command.Parameters.AddWithValue("@status", DataGatewayProcessState.Online)
        command.Parameters.AddWithValue("@configId", IIf(configId Is Nothing, DBNull.Value, configId))
        command.Parameters.AddWithValue("@tcpEndpoint", tcpServer)

        Dim id = CInt(mConnection.ExecuteReturnScalar(command))

        mConnection.CommitTransaction()

        Return id

    End Function


    Friend Function HasConfig(datapipelineProcessId As Integer) As Boolean
        Dim query = "select count(*) from BPADataPipelineProcess                       
                        where id = @id and config is not null"

        Dim command As New SqlCommand(query)
        command.Parameters.AddWithValue("@id", datapipelineProcessId)

        Dim processHasConfig = CInt(mConnection.ExecuteReturnScalar(command)) = 1
        Return processHasConfig
    End Function

    Friend Function GetConfigForProcess(datapipelineProcessId As Integer) As DataPipelineProcessConfig

        Dim query = "select dppc.[name], dppc.id, encryptid, configfile, iscustom from BPADataPipelineProcessConfig dppc
                        inner join BPADataPipelineProcess dpp on dpp.config = dppc.id
                        where dpp.id = @id"

        Dim command As New SqlCommand(query)
        command.Parameters.AddWithValue("@id", datapipelineProcessId)

        Using reader = mConnection.ExecuteReturnDataReader(command)
            Dim provider As New ReaderDataProvider(reader)
            If Not reader.Read() Then
                Throw New NoSuchElementException(My.Resources.DataPipelineDataAccess_NoConfigFoundForDataPipelineProcessWithID, datapipelineProcessId)
            End If

            Dim id = provider.GetInt("id")
            Dim encryptId = provider.GetInt("encryptid")
            Dim name = provider.GetString("name")
            Dim configFile = mServer.Decrypt(encryptId, provider.GetString("configfile"))
            Dim isCustom = provider.GetBool("iscustom")

            Return New DataPipelineProcessConfig(id, name, configFile, isCustom)
        End Using
    End Function

    Friend Function IsCustomConfiguration(configurationName As String) As Boolean

    End Function


    Friend Function GetConfigByName(name As String) As DataPipelineProcessConfig
        Dim command = New SqlCommand("select top 1 id, encryptid, configfile, iscustom from BPADataPipelineProcessConfig
                                        where name = @name")

        command.Parameters.Add(New SqlParameter("@name", name))

        Using reader = mConnection.ExecuteReturnDataReader(command)
            Dim provider As New ReaderDataProvider(reader)
            While reader.Read()
                Dim id = provider.GetInt("id")
                Dim encryptId = provider.GetInt("encryptid")
                Dim configFile = mServer.Decrypt(encryptId, provider.GetString("configfile"))
                Dim isCustom = provider.GetBool("iscustom")

                Return New DataPipelineProcessConfig(id, name, configFile, isCustom)
            End While
        End Using
        Return Nothing
    End Function

    Friend Function GetDataPipelineSettings() As DataPipelineSettings

        Const dataGatewaySettings As String = "select top 1 writesessionlogstodatabase, 
                                                            emitsessionlogstodatagateways, 
                                                            monitoringfrequency, 
                                                            sendpublisheddashboardstodatagateways, 
                                                            sendworkqueueanalysistodatagateways, 
                                                            databaseusercredentialname, 
                                                            useIntegratedSecurity,
                                                            serverPort 
                                                        from BPADataPipelineSettings"

        Dim dashboardSettings = GetPublishedDashboardIntervalSettings()

        Using dataGatewaySettingsCommand = New SqlCommand(dataGatewaySettings)
            Using reader = mConnection.ExecuteReturnDataReader(dataGatewaySettingsCommand)
                Dim provider As New ReaderDataProvider(reader)
                If reader.Read() Then
                    Return New DataPipelineSettings(provider.GetBool("writesessionlogstodatabase"),
                                                    provider.GetBool("emitsessionlogstodatagateways"),
                                                    provider.GetInt("monitoringfrequency"),
                                                    provider.GetBool("sendpublisheddashboardstodatagateways"),
                                                    dashboardSettings,
                                                    provider.GetBool("sendworkqueueanalysistodatagateways"),
                                                    provider.GetString("databaseusercredentialname"),
                                                    provider.GetBool("useIntegratedSecurity"),
                                                    provider.GetInt("serverPort"))
                End If
            End Using
        End Using
        Return New DataPipelineSettings
    End Function

    Friend Function CheckConfigExistsByID(id As Integer) As Boolean

        Dim command = New SqlCommand("select top 1 config from BPADataPipelineProcess Where id = @id")
        command.Parameters.Add(New SqlParameter("@id", id))

        Using reader = mConnection.ExecuteReturnDataReader(command)
            Dim provider As New ReaderDataProvider(reader)
            If reader.Read() Then
                Dim configID = provider.GetInt("config")
                Return configID <> 0
            End If
            Return False
        End Using

    End Function

    Friend Function DefaultEnctryptionSchemeValid() As Boolean
        Return mGetDefaultEncryptionFunc().HasValidKey
    End Function


    Private Function GetPublishedDashboardIntervalSettings() As List(Of PublishedDashboardSettings)
        Dim command = New SqlCommand("select id, name, sendeveryseconds from BPADashboard where dashtype = 2")
        Dim dashboardSettings = New List(Of PublishedDashboardSettings)()

        Using reader = mConnection.ExecuteReturnDataReader(command)
            Dim provider As New ReaderDataProvider(reader)
            While reader.Read()
                dashboardSettings.Add(New PublishedDashboardSettings(provider.GetGuid("id"), provider.GetString("name"), provider.GetInt("sendeveryseconds"), provider.GetValue("lastSent", Date.MinValue)))
            End While
        End Using

        Return dashboardSettings
    End Function


    Private Sub SetPublishedDashboardIntervalSettings(settings As List(Of PublishedDashboardSettings))

        Dim command = New SqlCommand("update BPADashboard set sendeveryseconds = @sendevery where id = @id")
        command.Parameters.Add("@sendevery", SqlDbType.Int)
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier)

        For Each setting In settings
            command.Parameters("@id").Value = setting.DashboardId
            command.Parameters("@sendevery").Value = setting.PublishToDataGatewayInterval
            If mConnection.ExecuteReturnRecordsAffected(command) <> 1 Then
                Throw New BluePrismException(My.Resources.DataPipelineDataAccess_UnableToChangeTheIntervalSettingOfPublishedDashboardWithID, setting.DashboardId)
            End If
        Next

    End Sub

    Friend Sub UpdateDataPipelineSettings(settings As DataPipelineSettings)
        settings.Validate()
        Dim command = New SqlCommand("update BPADataPipelineSettings 
                        set writesessionlogstodatabase = @writesessionlogstodatabase,
                            emitsessionlogstodatagateways = @emitsessionlogstodatagateways,
                            monitoringfrequency = @monitoringfrequency,
                            sendpublisheddashboardstodatagateways = @sendpublishedtodatagateways,
                            sendworkqueueanalysistodatagateways = @sendworkqueuetodatagateways,
                            databaseusercredentialname = @databaseusercredentialname,
                            useIntegratedSecurity = @useIntegratedSecurity, 
                            serverPort = @serverPort")

        With command.Parameters
            .AddWithValue("@writesessionlogstodatabase", settings.WriteSessionLogsToDatabase)
            .AddWithValue("@emitsessionlogstodatagateways", settings.SendSessionLogsToDataGateways)
            .AddWithValue("@monitoringfrequency", settings.MonitoringFrequency)
            .AddWithValue("@sendpublishedtodatagateways", settings.SendPublishedDashboardsToDataGateways)
            .AddWithValue("@sendworkqueuetodatagateways", settings.SendWorkQueueAnalysisToDataGateways)
            .AddWithValue("@databaseusercredentialname", settings.DatabaseUserCredentialName)
            .AddWithValue("@useIntegratedSecurity", settings.UseIntegratedSecurity)
            .AddWithValue("@serverPort", settings.ServerPort)
        End With

        Dim oldSettings = gSv.GetDataPipelineSettings()

        mConnection.BeginTransaction()
        If mConnection.ExecuteReturnRecordsAffected(command) <> 1 Then
            Throw New BluePrismException(My.Resources.DataPipelineDataAccess_UnableToUpdateTheSettings)
        End If

        SetPublishedDashboardIntervalSettings(settings.PublishedDashboardSettings)

        mServer.AuditRecordDataPipelineSettingsEvent(mConnection, DataPipelineEventCode.ModifySettings, settings, oldSettings)
        mConnection.CommitTransaction()
    End Sub

    Friend Function GetAllProcessConfigs() As List(Of DataPipelineProcessConfig)
        Dim command As New SqlCommand("select id, encryptid, name, configfile, iscustom from BPADataPipelineProcessConfig")

        Using reader = mConnection.ExecuteReturnDataReader(command)
            Dim provider As New ReaderDataProvider(reader)
            Dim configs = New List(Of DataPipelineProcessConfig)
            While reader.Read()
                Dim id = provider.GetInt("id")
                Dim encryptId = provider.GetInt("encryptid")
                Dim name = provider.GetString("name")
                Dim configFile = mServer.Decrypt(encryptId, provider.GetString("configfile"))
                Dim isCustom = provider.GetBool("iscustom")

                configs.Add(New DataPipelineProcessConfig(id, name, configFile, isCustom))
            End While
            Return configs
        End Using
    End Function

    Friend Sub AddOrUpdateProcessConfig(config As DataPipelineProcessConfig)
        If config.Id = 0 Then
            AddProcessConfig(config)
        Else
            UpdateProcessConfig(config)
        End If
    End Sub

    Private Sub AddProcessConfig(config As DataPipelineProcessConfig)

        Dim defaultEncryptionScheme = mGetDefaultEncryptionFunc()

        Dim query = "insert into BPADataPipelineProcessConfig (name, encryptid, configfile, iscustom) values (@name, @encryptid, @configfile, @iscustom)"
        Dim command = New SqlCommand(query)
        command.Parameters.AddWithValue("@name", config.Name)
        command.Parameters.AddWithValue("@encryptid", defaultEncryptionScheme.ID)
        command.Parameters.AddWithValue("@configfile", defaultEncryptionScheme.Encrypt(config.LogstashConfigFile))
        command.Parameters.AddWithValue("@iscustom", config.IsCustom)
        mConnection.BeginTransaction()
        mConnection.Execute(command)
        mServer.AuditRecordDataPipelineConfigEvent(mConnection, DataPipelineEventCode.CreateConfiguration, config)
        mConnection.CommitTransaction()
    End Sub

    Private Sub UpdateProcessConfig(config As DataPipelineProcessConfig)

        Dim defaultEncryptionScheme = mGetDefaultEncryptionFunc()

        Dim query = "update BPADataPipelineProcessConfig set name = @name, encryptid = @encryptid, configfile = @configfile, iscustom = @iscustom where id = @id"
        Dim command = New SqlCommand(query)
        command.Parameters.AddWithValue("@id", config.Id)
        command.Parameters.AddWithValue("@name", config.Name)
        command.Parameters.AddWithValue("@encryptid", defaultEncryptionScheme.ID)
        command.Parameters.AddWithValue("@configfile", defaultEncryptionScheme.Encrypt(config.LogstashConfigFile))
        command.Parameters.AddWithValue("@iscustom", config.IsCustom)

        mConnection.BeginTransaction()
        If mConnection.ExecuteReturnRecordsAffected(command) <> 1 Then
            Throw New BluePrismException(My.Resources.DataPipelineDataAccess_UnableToUpdateTheProcessorConfigWithAnid, config.Id)
        End If
        mServer.AuditRecordDataPipelineConfigEvent(mConnection, DataPipelineEventCode.ModifyConfiguration, config)
        mConnection.CommitTransaction()
    End Sub


    Friend Function GetDataPipelineProcesses() As List(Of DataPipelineProcess)


        Dim selectProcessorsQuery = "select dpp.name, dpp.status, dpp.message, dpp.lastupdated, dpc.id as configid, dpc.name as configname, dpc.encryptid, dpc.configfile dpc.iscustom from BPADataPipelineProcess dpp
                                        inner join BPADataPipelineProcessConfig dpc on dpp.config = dpc.id"
        Dim command = New SqlCommand(selectProcessorsQuery)

        Dim datapipelineProcessors = New List(Of DataPipelineProcess)

        Using reader = mConnection.ExecuteReturnDataReader(command)
            Dim provider As New ReaderDataProvider(reader)

            While reader.Read()
                Dim name = provider.GetString("name")

                Dim state = provider.GetInt("status")
                Dim message = provider.GetString("message")
                Dim lastupdated = provider.GetValue("lastupdated", Date.MinValue)

                Dim configId = provider.GetInt("configid")
                Dim configName = provider.GetString("configname")
                Dim configEncryptId = provider.GetInt("encryptid")
                Dim configfile = mServer.Decrypt(configEncryptId, provider.GetString("configfile"))
                Dim isCustom = provider.GetBool("iscustom")

                Dim config = New DataPipelineProcessConfig(configId, configName, configfile, isCustom)

                Dim status = New DataGatewayProcessStatus(DirectCast(state, DataGatewayProcessState), message, lastupdated)
                datapipelineProcessors.Add(New DataPipelineProcess(name, status, config))

            End While

            Return datapipelineProcessors
        End Using

    End Function


    Friend Function ReEncryptConfigs() As Integer

        Dim defaultEncryptionScheme = mGetDefaultEncryptionFunc()
        Dim newEncryptID = defaultEncryptionScheme.ID
        Dim ids As New List(Of Integer)

        Using cmd = New SqlCommand("select id from BPADataPipelineProcessConfig where encryptid <> @encryptid")
            cmd.Parameters.AddWithValue("@encryptid", newEncryptID)
            Using reader = mConnection.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    ids.Add(prov.GetValue("id", 0))
                End While
            End Using
        End Using
        If ids.Count = 0 Then Return -1

        Dim configfile As String
        mConnection.BeginTransaction()
        For Each id In ids
            ' Retrieve the config files (decrypting with the original scheme)
            Using cmd As New SqlCommand("select configfile, encryptid from BPADataPipelineProcessConfig where id = @id")
                cmd.Parameters.AddWithValue("@id", id)
                Using reader = mConnection.ExecuteReturnDataReader(cmd)
                    reader.Read()
                    Dim prov = New ReaderDataProvider(reader)
                    configfile = mServer.Decrypt(prov.GetInt("encryptid"), prov.GetString("configfile"))
                End Using
            End Using

            ' Update the screenshot (encrypting with the new scheme)
            Using cmd As New SqlCommand("update BPADataPipelineProcessConfig set configfile = @configfile, encryptid = @encryptid where id = @id")
                With cmd.Parameters
                    .AddWithValue("@id", id)
                    .AddWithValue("@configfile", defaultEncryptionScheme.Encrypt(configfile))
                    .AddWithValue("@encryptid", newEncryptID)
                End With
                mConnection.Execute(cmd)
            End Using
        Next
        mConnection.CommitTransaction()

        Return ids.Count
    End Function


    Friend Function UpdateProcessStatus(processId As Integer, state As DataGatewayProcessState, message As String) As Boolean
        Dim query = "update BPADataPipelineProcess
                        set status = @status, message = @message, lastupdated = GETUTCDATE()
                        where id = @id;"

        Dim cmd = New SqlCommand(query)
        cmd.Parameters.AddWithValue("@id", processId)
        cmd.Parameters.AddWithValue("@status", state)
        cmd.Parameters.AddWithValue("@message", message)

        Return mConnection.ExecuteReturnRecordsAffected(cmd) = 1
    End Function


    Friend Sub AssignConfigToPipelineProcess(datapipelineProcessId As Integer, configId As Integer)
        Dim command = New SqlCommand("update BPADataPipelineProcess set config = @config where id = @id")
        command.Parameters.AddWithValue("@config", configId)
        command.Parameters.AddWithValue("@id", datapipelineProcessId)
        mConnection.Execute(command)
    End Sub

    Friend Function GetPipelineProcessId(name As String) As Integer
        Dim command = New SqlCommand("select id from BPADataPipelineProcess where name=@name")
        command.Parameters.AddWithValue("@name", name)
        Return clsServer.IfNull(mConnection.ExecuteReturnScalar(command), -1)
    End Function


    Friend Function GetPipelineProcessTcpEndpoint(id As Integer) As String
        Dim command = New SqlCommand("select tcpEndpoint from BPADataPipelineProcess where id=@id")
        command.Parameters.AddWithValue("@id", id)
        Return CStr(mConnection.ExecuteReturnScalar(command))


    End Function

    ''' <summary>
    ''' Returns the Id of the configuration. The current implementation assumes there will only be a single config in the database which all processes will use,
    ''' so this function gets that. Returns null if there is no config.
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDefaultConfigId() As Integer?
        Dim configId As Integer? = Nothing
        Dim configs = GetAllProcessConfigs()
        If configs.Any Then
            configId = configs.First().Id
        End If

        Return configId
    End Function

    Public Function ErrorOnDataGatewayProcess() As Boolean
        Dim query = New StringBuilder(
                                      "SELECT 1 
             FROM BPADataPipelineProcess 
             WHERE [status] = @errorId;"
                                      )

        Using command = New SqlCommand(query.ToString())
            command.Parameters.AddWithValue("@errorId", DataGatewayProcessState.Error)

            Return mConnection.ExecuteReturnScalar(command) IsNot Nothing
        End Using
    End Function

    Public Function GetDataGatewayProcessStatuses() As List(Of DataGatewayProcessStatusInformation)
        Dim result = New List(Of DataGatewayProcessStatusInformation)
        Dim query = New StringBuilder(
                "SELECT id, [name], [status], message 
                 FROM BPADataPipelineProcess;"
        )

        Using command = New SqlCommand(query.ToString())
            Using reader = mConnection.ExecuteReturnDataReader(command)
                Dim provider As New ReaderDataProvider(reader)
                While reader.Read()
                    Dim process = New DataGatewayProcessStatusInformation() With {
                            .Id = provider.GetInt("id"),
                            .Name = provider.GetString("name"),
                            .Status = CType(provider.GetInt("status"), DataGatewayProcessState),
                            .ErrorMessage = provider.GetString("message")
                            }

                    result.Add(process)
                End While

                Return result
            End Using

        End Using

    End Function

    Friend Sub AddOrUpdateDataPipelineOutputConfig(config As DataPipelineOutputConfig)
        If config.Id = 0 Then
            AddDataPipelineOutputConfig(config)
        Else
            UpdateDataPipelineOutputConfig(config)
        End If
    End Sub

    Private Sub UpdateDataPipelineOutputConfig(dataPipelineConfigOutput As DataPipelineOutputConfig)

        Dim updateCommand = New SqlCommand("UPDATE [BPADataPipelineOutputConfig]
                                           SET [name] = @name
                                              ,[issessions] = @issessions
                                              ,[isdashboards] = @isdashboards
                                              ,[iscustomobjectdata] = @iscustomobjectdata
                                              ,[sessioncols] = @sessioncols
                                              ,[dashboardcols] = @dashboardcols
                                              ,[advanced] = @advanced
                                              ,[type] = @type
                                              ,[isadvanced] = @isadvanced
                                              ,[outputoptions] = @outputoptions
                                              ,[iswqasnapshotdata] = @iswqasnapshotdata
                                              ,[selecteddashboards] = @selecteddashboards
                                              ,[selectedsessionlogfields] = @selectedsessionlogfields
                                         WHERE id = @id
                                        ")

        Dim outputOptions As String = Nothing
        If dataPipelineConfigOutput.OutputOptions IsNot Nothing Then
            outputOptions = JsonConvert.SerializeObject(dataPipelineConfigOutput.OutputOptions, mJsonSerializerSettings)
        End If

        Dim selectedDashboards As String = Nothing
        If dataPipelineConfigOutput.SelectedDashboards IsNot Nothing Then
            selectedDashboards = JsonConvert.SerializeObject(dataPipelineConfigOutput.SelectedDashboards, mJsonSerializerSettings)
        End If

        Dim selectedSessionLogFields As String = Nothing
        If dataPipelineConfigOutput.SelectedSessionLogFields IsNot Nothing Then
            selectedSessionLogFields = JsonConvert.SerializeObject(dataPipelineConfigOutput.SelectedSessionLogFields, mJsonSerializerSettings)
        End If

        updateCommand.Parameters.AddWithValue("@name", dataPipelineConfigOutput.Name)
        updateCommand.Parameters.AddWithValue("@issessions", dataPipelineConfigOutput.IsSessions)
        updateCommand.Parameters.AddWithValue("@isdashboards", dataPipelineConfigOutput.IsDashboards)
        updateCommand.Parameters.AddWithValue("@iscustomobjectdata", dataPipelineConfigOutput.IsCustomObjectData)
        updateCommand.Parameters.AddWithValue("@sessioncols", GetValueOrDBNull(dataPipelineConfigOutput.SessionCols))
        updateCommand.Parameters.AddWithValue("@dashboardcols", GetValueOrDBNull(dataPipelineConfigOutput.DashboardCols))
        updateCommand.Parameters.AddWithValue("@advanced", GetValueOrDBNull(dataPipelineConfigOutput.AdvancedConfiguration))
        updateCommand.Parameters.AddWithValue("@type", dataPipelineConfigOutput.OutputType.Id)
        updateCommand.Parameters.AddWithValue("@isadvanced", dataPipelineConfigOutput.IsAdvanced)
        updateCommand.Parameters.AddWithValue("@outputoptions", GetValueOrDBNull(outputOptions))
        updateCommand.Parameters.AddWithValue("@iswqasnapshotdata", dataPipelineConfigOutput.IsWqaSnapshotData)
        updateCommand.Parameters.AddWithValue("@selecteddashboards", GetValueOrDBNull(selectedDashboards))
        updateCommand.Parameters.AddWithValue("@selectedsessionlogfields", GetValueOrDBNull(selectedSessionLogFields))
        updateCommand.Parameters.AddWithValue("@id", dataPipelineConfigOutput.Id)

        Dim oldConfig = gSv.GetDataPipelineOutputConfigs().Single(Function(c)
                                                                      Return c.UniqueReference = dataPipelineConfigOutput.UniqueReference
                                                                  End Function)

        mConnection.BeginTransaction()

        mConnection.Execute(updateCommand)
        mServer.AuditRecordDataPipelineOutputConfigEvent(mConnection, DataPipelineEventCode.ModifyOutputConfiguration, dataPipelineConfigOutput, oldConfig)

        mConnection.CommitTransaction()
    End Sub

    Public Sub AddDataPipelineOutputConfig(dataPipelineConfigOutput As DataPipelineOutputConfig)
        Dim query = "INSERT INTO [BPADataPipelineOutputConfig]
           (
            [uniquereference]
           ,[name]
           ,[issessions]
           ,[isdashboards]
           ,[iscustomobjectdata]
           ,[sessioncols]
           ,[dashboardcols]
           ,[datecreated]
           ,[advanced]
           ,[type]
           ,[isadvanced]
           ,[outputoptions]
           ,[iswqasnapshotdata]
           ,[selecteddashboards]
           ,[selectedsessionlogfields]
            )
     VALUES
           (
            @uniquereference
           ,@name
           ,@issessions
           ,@isdashboards
           ,@iscustomobjectdata
           ,@sessioncols
           ,@dashboardcols
           ,@datecreated
           ,@advanced
           ,@type
           ,@isadvanced
           ,@outputoptions
           ,@iswqasnapshotdata
           ,@selecteddashboards
           ,@selectedsessionlogfields
            )"

        Dim outputOptions = Nothing
        If dataPipelineConfigOutput.OutputOptions IsNot Nothing Then
            outputOptions = JsonConvert.SerializeObject(dataPipelineConfigOutput.OutputOptions, mJsonSerializerSettings)
        End If

        Dim selectedDashboards As String = Nothing
        If dataPipelineConfigOutput.SelectedDashboards IsNot Nothing Then
            selectedDashboards = JsonConvert.SerializeObject(dataPipelineConfigOutput.SelectedDashboards, mJsonSerializerSettings)
        End If

        Dim selectedSessionLogFields As String = Nothing
        If dataPipelineConfigOutput.SelectedSessionLogFields IsNot Nothing Then
            selectedSessionLogFields = JsonConvert.SerializeObject(dataPipelineConfigOutput.SelectedSessionLogFields, mJsonSerializerSettings)
        End If

        dataPipelineConfigOutput.UniqueReference = Guid.NewGuid()
        dataPipelineConfigOutput.DateCreated = Date.UtcNow

        Using command = New SqlCommand(query)
            command.Parameters.AddWithValue("@uniquereference", dataPipelineConfigOutput.UniqueReference)
            command.Parameters.AddWithValue("@name", dataPipelineConfigOutput.Name)
            command.Parameters.AddWithValue("@issessions", dataPipelineConfigOutput.IsSessions)
            command.Parameters.AddWithValue("@isdashboards", dataPipelineConfigOutput.IsDashboards)
            command.Parameters.AddWithValue("@iscustomobjectdata", dataPipelineConfigOutput.IsCustomObjectData)
            command.Parameters.AddWithValue("@sessioncols", GetValueOrDBNull(dataPipelineConfigOutput.SessionCols))
            command.Parameters.AddWithValue("@dashboardcols", GetValueOrDBNull(dataPipelineConfigOutput.DashboardCols))
            command.Parameters.AddWithValue("@datecreated", dataPipelineConfigOutput.DateCreated)
            command.Parameters.AddWithValue("@advanced", GetValueOrDBNull(dataPipelineConfigOutput.AdvancedConfiguration))
            command.Parameters.AddWithValue("@type", dataPipelineConfigOutput.OutputType.Id)
            command.Parameters.AddWithValue("@isadvanced", dataPipelineConfigOutput.IsAdvanced)
            command.Parameters.AddWithValue("@outputoptions", GetValueOrDBNull(outputOptions))
            command.Parameters.AddWithValue("@iswqasnapshotdata", dataPipelineConfigOutput.IsWqaSnapshotData)
            command.Parameters.AddWithValue("@selecteddashboards", GetValueOrDBNull(selectedDashboards))
            command.Parameters.AddWithValue("@selectedsessionlogfields", GetValueOrDBNull(selectedSessionLogFields))

            mConnection.BeginTransaction()

            mConnection.Execute(command)
            mServer.AuditRecordDataPipelineOutputConfigEvent(mConnection, DataPipelineEventCode.CreateOutputConfiguration, dataPipelineConfigOutput)

            mConnection.CommitTransaction()
        End Using
    End Sub

    Friend Sub DeleteDataPipelineOutputConfig(dataPipelineOutputConfig As DataPipelineOutputConfig)
        Dim deleteCommand = New SqlCommand("DELETE 
                                            FROM [BPADataPipelineOutputConfig]
                                            WHERE [uniquereference] = @configId
                                           ")

        deleteCommand.Parameters.AddWithValue("@configId", dataPipelineOutputConfig.UniqueReference)

        mConnection.BeginTransaction()

        mConnection.Execute(deleteCommand)
        mServer.AuditRecordDataPipelineOutputConfigEvent(mConnection, DataPipelineEventCode.DeleteOutputConfiguration, dataPipelineOutputConfig)

        mConnection.CommitTransaction()
    End Sub

    Public Function GetDataPipelineOutputConfigs() As IList(Of DataPipelineOutputConfig)

        Dim command As New SqlCommand("SELECT 
               [id]
              ,[uniquereference]
              ,[name]
              ,[issessions]
              ,[isdashboards]
              ,[iscustomobjectdata]
              ,[sessioncols]
              ,[dashboardcols]
              ,[datecreated]
              ,[advanced]
              ,[type]
              ,[isadvanced]
              ,[outputoptions]
              ,[iswqasnapshotdata]
              ,[selecteddashboards]
              ,[selectedsessionlogfields]
             FROM [BPADataPipelineOutputConfig]
             ORDER BY [datecreated] DESC")

        Using reader = mConnection.ExecuteReturnDataReader(command)
            Dim provider As New ReaderDataProvider(reader)
            Dim configs = New List(Of DataPipelineOutputConfig)
            While reader.Read()
                Dim id = provider.GetInt("id")
                Dim uniquereference = provider.GetGuid("uniquereference")
                Dim name = provider.GetString("name")
                Dim issessions = provider.GetBool("issessions")
                Dim isdashboards = provider.GetBool("isdashboards")
                Dim isCustomObjectData = provider.GetBool("iscustomobjectdata")
                Dim sessioncols = provider.GetString("sessioncols")
                Dim dashboardcols = provider.GetString("dashboardcols")
                Dim datecreated = provider.GetValue("datecreated", Date.MinValue)
                Dim advanced = provider.GetString("advanced")
                Dim isAdvanced = provider.GetBool("isadvanced")
                Dim type = provider.GetString("type")
                Dim outputoptions = provider.GetString("outputoptions")
                Dim iswqasnapshotdata = provider.GetBool("iswqasnapshotdata")
                Dim selecteddashboards = provider.GetString("selecteddashboards")
                Dim selectedsessionlogfields = provider.GetString("selectedsessionlogfields")

                Dim convertedOutputOptions As List(Of OutputOption) = Nothing
                Dim convertedSelectedDashboards As List(Of String) = Nothing
                Dim convertedSelectedSessionLogFields As List(Of String) = Nothing

                If outputoptions IsNot Nothing Then
                    convertedOutputOptions = JsonConvert.DeserializeObject(Of List(Of OutputOption))(outputoptions, mJsonSerializerSettings)
                End If

                If selecteddashboards IsNot Nothing Then
                    convertedSelectedDashboards = JsonConvert.DeserializeObject(Of List(Of String))(selecteddashboards, mJsonSerializerSettings)
                End If

                If selectedsessionlogfields IsNot Nothing Then
                    convertedSelectedSessionLogFields = JsonConvert.DeserializeObject(Of List(Of String))(selectedsessionlogfields, mJsonSerializerSettings)
                End If

                Dim config = New DataPipelineOutputConfig With {
                    .Id = id,
                    .UniqueReference = uniquereference,
                    .Name = name,
                    .IsSessions = issessions,
                    .IsDashboards = isdashboards,
                    .IsCustomObjectData = isCustomObjectData,
                    .SessionCols = sessioncols,
                    .DashboardCols = dashboardcols,
                    .DateCreated = datecreated,
                    .AdvancedConfiguration = advanced,
                    .OutputType = OutputTypeFactory.GetOutputType(type),
                    .OutputOptions = convertedOutputOptions,
                    .IsWqaSnapshotData = iswqasnapshotdata,
                    .SelectedDashboards = convertedSelectedDashboards,
                    .SelectedSessionLogFields = convertedSelectedSessionLogFields,
                    .IsAdvanced = isAdvanced
                }

                configs.Add(config)
            End While
            Return configs
        End Using

    End Function


    Private Function GetValueOrDBNull(obj As Object) As Object
        Return IIf(obj Is Nothing, DBNull.Value, obj)
    End Function
End Class
