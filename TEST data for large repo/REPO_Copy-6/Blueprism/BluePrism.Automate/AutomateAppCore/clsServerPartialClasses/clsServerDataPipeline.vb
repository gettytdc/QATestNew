Imports System.Data.SqlClient
Imports System.Net.Sockets
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Data
Imports BluePrism.DataPipeline
Imports BluePrism.DataPipeline.DataPipelineOutput
Imports BluePrism.Server.Domain.Models

Partial Public Class clsServer

    <SecuredMethod(False)>
    Public Function RegisterDataPipelineProcess(name As String, tcpServer As String) As Integer Implements IServerDataPipeline.RegisterDataPipelineProcess
        CheckPermissions()
        Using con = GetConnection()
            Return GetDataAccesss(con).RegisterDataPipelineProcess(name, tcpServer)
        End Using
    End Function

    <SecuredMethod(False)>
    Public Function UpdateDataPipelineProcessorStatus(id As Integer, dataGatewayProcessState As DataGatewayProcessState, message As String) As Boolean Implements IServerDataPipeline.UpdateDataPipelineProcessStatus
        CheckPermissions()
        Using con = GetConnection()
            Return GetDataAccesss(con).UpdateProcessStatus(id, dataGatewayProcessState, message)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.DataGateways.Configuration, Permission.SystemManager.DataGateways.AdvancedConfiguration)>
    Public Sub SaveConfig(config As DataPipelineProcessConfig) Implements IServerDataPipeline.SaveConfig
        CheckPermissions()
        Using con = GetConnection()
            GetDataAccesss(con).AddOrUpdateProcessConfig(config)
        End Using
    End Sub

    <SecuredMethod(False)>
    Public Function GetConfigAssignedToDataPipelineProcess(id As Integer) As String Implements IServerDataPipeline.GetConfigForDataPipelineProcess
        CheckPermissions()
        Using con = GetConnection()
            Return GetDataAccesss(con).GetConfigForProcess(id).LogstashConfigFile
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.DataGateways.Configuration, Permission.SystemManager.DataGateways.AdvancedConfiguration)>
    Public Function GetDataPipelineConfigurations() As List(Of DataPipelineProcessConfig) Implements IServerDataPipeline.GetDataPipelineConfigurations
        CheckPermissions()
        Using con = GetConnection()
            Return GetDataAccesss(con).GetAllProcessConfigs()
        End Using
    End Function


    <SecuredMethod(Permission.SystemManager.DataGateways.Configuration, Permission.SystemManager.DataGateways.AdvancedConfiguration)>
    Public Function IsCustomConfiguration(configurationName As String) As Boolean Implements IServerDataPipeline.IsCustomConfiguration
        CheckPermissions()
        Using con = GetConnection()
            Try
                Return GetDataAccesss(con).GetConfigByName(configurationName).IsCustom
            Catch
                Return False
            End Try

        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.DataGateways.Configuration, Permission.SystemManager.DataGateways.AdvancedConfiguration)>
    Public Function GetConfigurationByName(name As String) As DataPipelineProcessConfig Implements IServerDataPipeline.GetConfigurationByName
        CheckPermissions()
        Using con = GetConnection()
            Return GetDataAccesss(con).GetConfigByName(name)
        End Using
    End Function


    <SecuredMethod(True)>
    Public Function ReEncryptDataPipelineConfigurationFiles() As Integer Implements IServerDataPipeline.ReEncryptDataPipelineConfigurationFiles
        CheckPermissions()
        Using con = GetConnection()
            Return GetDataAccesss(con).ReEncryptConfigs()
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetDataGatewayProcesses() As List(Of DataGatewayProcessStatusInformation) Implements IServerDataPipeline.GetDataGatewayProcesses
        CheckPermissions()
        Using connection = GetConnection()
            Return GetDataAccesss(connection).GetDataGatewayProcessStatuses()
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function ErrorOnDataGatewayProcess() As Boolean Implements IServerDataPipeline.ErrorOnDataGatewayProcess
        CheckPermissions()
        Using connection = GetConnection()
            Return GetDataAccesss(connection).ErrorOnDataGatewayProcess()
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetSettings() As DataPipelineSettings Implements IServerDataPipeline.GetDataPipelineSettings
        CheckPermissions()
        Using con = GetConnection()
            Return GetDataAccesss(con).GetDataPipelineSettings()
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.DataGateways.Configuration, Permission.SystemManager.DataGateways.AdvancedConfiguration)>
    Public Sub UpdateSettings(settings As DataPipelineSettings) Implements IServerDataPipeline.UpdateDataPipelineSettings
        CheckPermissions()
        Using con = GetConnection()
            GetDataAccesss(con).UpdateDataPipelineSettings(settings)
        End Using
    End Sub

    <SecuredMethod(True)>
    Public Function CheckDataGatewayConfigExists(id As Integer) As Boolean Implements IServerDataPipeline.CheckConfigExistsByID
        CheckPermissions()
        Using con = GetConnection()
            Return GetDataAccesss(con).CheckConfigExistsByID(id)
        End Using
    End Function

    <SecuredMethod(True)>
    Public Sub SendCustomDataToGateway(customData As clsCollection, sessionNumber As Integer,
                stageId As Guid, stageName As String, stageType As StageTypes,
                startDate As DateTimeOffset, processName As String,
                pageName As String, objectName As String,
                actionName As String) Implements IServerDataPipeline.SendCustomDataToGateway
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()

            Dim dataPipelineEvent = New DataPipelineEvent(EventType.CustomData) With {
                    .EventData = New Dictionary(Of String, Object) From {
                                    {"CustomDataCollection", customData},
                                    {"SessionNumber", sessionNumber},
                                    {"StageID", stageId},
                                    {"StageName", stageName},
                                    {"StageType", stageType},
                                    {"StartDate", startDate},
                                    {"ProcessName", processName},
                                    {"PageName", pageName},
                                    {"ObjectName", objectName},
                                    {"actionName", actionName}
                                    }}

            mDataPipelinePublisher.PublishToDataPipeline(con, {dataPipelineEvent})
            con.CommitTransaction()
        End Using
    End Sub

    <SecuredMethod(True)>
    Public Sub SendWqaSnapshotDataToDataGateways() Implements IServerDataPipeline.SendWqaSnapshotDataToDataGateways
        CheckPermissions()

        'Extract all WQA snap shot data from the BPMIQueueSnapshot table
        Dim command = New SqlCommand("select top 100
                                           bqs.snapshotid
                                          ,bqs.snapshotdate
                                          ,bqs.capturedatetimeutc
                                          ,bqs.totalitems
                                          ,bqs.itemspending
                                          ,bqs.itemscompleted
                                          ,bqs.itemsreferred
                                          ,bqs.newitemsdelta
                                          ,bqs.completeditemsdelta
                                          ,bqs.referreditemsdelta
                                          ,bqs.totalworktimecompleted
                                          ,bqs.totalworktimereferred
                                          ,bqs.totalidletime
                                          ,bqs.totalnewsincemidnight
                                          ,bqs.totalnewlast24hours
                                          ,bqs.averagecompletedworktime
                                          ,bqs.averagereferredworktime
                                          ,bqs.averageidletime
                                          ,bqs.senttodatagateways
                                          ,bwq.name as queuename
                                          ,bwq.id as queueid
                                   from BPMIQueueSnapshot as bqs
                                   inner join BPAWorkQueue as bwq on bqs.queueident = bwq.ident
                                   where senttodatagateways = 0
                                   order by bqs.snapshotid")

        Dim dataPipelineEvents = New List(Of DataPipelineEvent)(EventType.WqaSnapshotData)
        Dim dataPipelineEvent = New DataPipelineEvent(EventType.WqaSnapshotData)

        'Read out the data from every column of the row
        Using con = GetConnection()
            Using reader = con.ExecuteReturnDataReader(command)
                Dim provider As New ReaderDataProvider(reader)

                While reader.Read()
                    Dim snapshotid = provider.GetInt("snapshotid")
                    Dim snapshotdate = provider.GetString("snapshotdate")
                    Dim capturedatetimeutc = provider.GetString("capturedatetimeutc")
                    Dim totalitems = provider.GetInt("totalitems")
                    Dim itemspending = provider.GetInt("itemspending")
                    Dim itemscompleted = provider.GetInt("itemscompleted")
                    Dim itemsreferred = provider.GetInt("itemsreferred")
                    Dim newitemsdelta = provider.GetInt("newitemsdelta")
                    Dim completeditemsdelta = provider.GetInt("completeditemsdelta")
                    Dim referreditemsdelta = provider.GetInt("referreditemsdelta")
                    Dim totalworktimecompleted = provider.GetInt("totalworktimecompleted")
                    Dim totalworktimereferred = provider.GetInt("totalworktimereferred")
                    Dim totalidletime = provider.GetInt("totalidletime")
                    Dim totalnewsincemidnight = provider.GetInt("totalnewsincemidnight")
                    Dim totalnewlast24hours = provider.GetInt("totalnewlast24hours")
                    Dim averagecompletedworktime = provider.GetInt("averagecompletedworktime")
                    Dim averagereferredworktime = provider.GetInt("averagereferredworktime")
                    Dim averageidletime = provider.GetInt("averageidletime")
                    Dim queuename = provider.GetString("queuename")
                    Dim queueid = provider.GetGuid("queueid")

                    dataPipelineEvent = New DataPipelineEvent(EventType.WqaSnapshotData) With {
                        .EventData = New Dictionary(Of String, Object) From {
                                    {"SnapshotId", snapshotid},
                                    {"SnapshotDate", snapshotdate},
                                    {"CaptureDateTimeUtc", capturedatetimeutc},
                                    {"TotalItems", totalitems},
                                    {"Itemspending", itemspending},
                                    {"ItemsCompleted", itemscompleted},
                                    {"ItemsReferred", itemsreferred},
                                    {"NewItemsDelta", newitemsdelta},
                                    {"CompletedItemsDelta", completeditemsdelta},
                                    {"ReferredItemsDelta", referreditemsdelta},
                                    {"TotalWorkTimeCompleted", totalworktimecompleted},
                                    {"TotalWorkTimeReferred", totalworktimereferred},
                                    {"TotalIdleTime", totalidletime},
                                    {"TotalNewSinceMidnight", totalnewsincemidnight},
                                    {"TotalNewLast24Hours", totalnewlast24hours},
                                    {"AverageCompletedWorktime", averagecompletedworktime},
                                    {"AverageReferredWorktime", averagereferredworktime},
                                    {"AverageIdleTime", averageidletime},
                                    {"QueueName", queuename},
                                    {"QueueId", queueid}
                                    }}

                    'Add each row to the list to be published in a seperate connection below
                    dataPipelineEvents.Add(dataPipelineEvent)

                End While
            End Using
        End Using

        'Publish each DataPipelineEvent object and update the senttodatagateways column to true
        Using con = GetConnection()
            For Each dataPipelineEvent In dataPipelineEvents
                mDataPipelinePublisher.PublishToDataPipeline(con, {dataPipelineEvent})
                Dim snapShotId = dataPipelineEvent.EventData.Item("SnapshotId")
                Using updateCommand = New SqlCommand("update BPMIQueueSnapshot set senttodatagateways = 1 where snapshotid = @id")
                    updateCommand.Parameters.AddWithValue("@id", snapShotId)
                    con.Execute(updateCommand)
                End Using
            Next
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.DataGateways.ControlRoom)>
    Public Sub SendCommandToDatapipelineProcess(id As Integer, command As DataPipelineProcessCommand) Implements IServerDataPipeline.SendCommandToDatapipelineProcess
        CheckPermissions()
        SendCommandToLogstashProcessManager(id, command)
    End Sub

    <SecuredMethod(True)>
    Public Function GetDataPipelineOutputConfigs() As IEnumerable(Of DataPipelineOutputConfig) Implements IServerDataPipeline.GetDataPipelineOutputConfigs
        CheckPermissions()
        Using con = GetConnection()
            Return GetDataAccesss(con).GetDataPipelineOutputConfigs()
        End Using
    End Function

    <SecuredMethod(True)>
    Public Sub SaveDataPipelineOutputConfig(config As DataPipelineOutputConfig) Implements IServerDataPipeline.SaveDataPipelineOutputConfig
        CheckPermissions()
        Using con = GetConnection()
            GetDataAccesss(con).AddOrUpdateDataPipelineOutputConfig(config)
        End Using
    End Sub

    <SecuredMethod(True)>
    Public Sub RestartLogstash() Implements IServerDataPipeline.RestartLogstash
        CheckPermissions()
        Throw New NotImplementedException
    End Sub

    <SecuredMethod(True)>
    Public Function ProduceLogstashConfig() As String Implements IServerDataPipeline.ProduceLogstashConfig
        CheckPermissions()
        Using con = GetConnection()
            Dim dataGatewaySettings = GetDataAccesss(con).GetDataPipelineSettings()

            Dim input = GetLogstashConfigInputSection(dataGatewaySettings.DatabaseUserCredentialName, dataGatewaySettings.UseIntegratedSecurity, dataGatewaySettings.ServerPort)

            Dim configs = GetDataPipelineOutputConfigs()
            Dim sb = New StringBuilder

            Dim filter = GetLogstashConfigFilterSection(configs)

            sb.AppendLine(input)
            sb.AppendLine(filter)

            sb.AppendLine("output {")
            For Each c In configs
                If c.IsAdvanced Then
                    sb.AppendLine(c.AdvancedConfiguration)
                Else
                    sb.AppendLine(c.GetLogstashConfig)
                End If
            Next
            sb.Append("}")

            Return sb.ToString()

        End Using
    End Function

    <SecuredMethod(True)>
    Public Function DefaultEnctryptionSchemeValid() As Boolean Implements IServerDataPipeline.DefaultEnctryptionSchemeValid
        CheckPermissions()
        Using con = GetConnection()
            Return GetDataAccesss(con).DefaultEnctryptionSchemeValid()
        End Using
    End Function

    <SecuredMethod(True)>
    Public Sub DeleteDataPipelineOutputConfig(dataPipelineOutputConfig As DataPipelineOutputConfig) Implements IServerDataPipeline.DeleteDataPipelineOutputConfig
        CheckPermissions()
        Using con = GetConnection()
            GetDataAccesss(con).DeleteDataPipelineOutputConfig(dataPipelineOutputConfig)
        End Using
    End Sub

    Private Function GetDataAccesss(con As IDatabaseConnection) As DataPipelineDataAccess
        Return New DataPipelineDataAccess(con, Function() GetEncryptionSchemeFromDB(con, GetDefaultEncrypter(con), Nothing, True), Me)
    End Function

    Private Function GetLogstashConfigInputSection(credentialName As String, useIntegratedSecurity As Boolean, serverPort As Integer) As String
        Dim jdbcConnectionString =
             $"jdbc:sqlserver://{mDBConnectionSetting.DBServer}:{serverPort};databaseName={mDBConnectionSetting.DatabaseName};"
        Dim sqlUserConnectionSection = String.Empty

        If useIntegratedSecurity Then
            jdbcConnectionString += $"integratedSecurity=true;"
            sqlUserConnectionSection = $"jdbc_user => """"
                                    jdbc_password => """""
        Else
            Using con = GetConnection()
                Dim credential = GetCredential(con, credentialName)
                If Not credential.Type Is CredentialType.DataGatewayCredentials Then
                    Throw New BluePrismException(My.Resources.DataGatewaySqlUserInvalidCredentialType)
                End If
                sqlUserConnectionSection = $"jdbc_user => ""<%{credentialName}.username%>""{Environment.NewLine}jdbc_password => ""<%{credentialName}.password%>"""
            End Using
        End If

        Dim inputConfig =
$"input {{
jdbc {{
jdbc_driver_library => ""..\sqljdbc_4.2\enu\jre8\sqljdbc42.jar""
jdbc_connection_string => ""{jdbcConnectionString}""
jdbc_driver_class => ""com.microsoft.sqlserver.jdbc.SQLServerDriver"" 
{sqlUserConnectionSection}
statement => ""delete top(3000)from BPADataPipelineInput with (rowlock, readpast) output deleted.eventdata""
schedule => ""*/3 * * * * *""
}}
}}
"
        Return inputConfig
    End Function

    Private Function GetLogstashConfigFilterSection(outputConfigs As IEnumerable(Of DataPipelineOutputConfig)) As String

        Dim sb = New StringBuilder

        sb.AppendLine("filter {")
        sb.AppendLine("json {")
        sb.AppendLine("source => ""eventdata""")
        sb.AppendLine("target => ""event""")
        sb.AppendLine("}")

        Dim clones = New StringBuilder

        For Each outputConfig In outputConfigs
            If (outputConfig.IsSessionFilteringConfigRequired()) Then
                If (clones.Length = 0) Then
                    clones.Append($"""{outputConfig.Name}""")
                Else
                    clones.Append($",""{outputConfig.Name}""")
                End If
            End If
        Next

        If (clones.Length > 0) Then
            sb.AppendLine("if [event][EventType] == 1 {")
            sb.AppendLine("clone {")
            sb.AppendLine($"clones => [{clones.ToString()}]")
            sb.AppendLine("}")
            sb.AppendLine("}")
        End If

        For Each outputConfig In outputConfigs
            sb.Append(outputConfig.GetSessionFilter)
        Next

        sb.appendLine("}")

        Return sb.ToString()

    End Function

    Private Sub SendCommandToLogstashProcessManager(id As Integer, command As DataPipelineProcessCommand)

        'check config exists if the command is to start process
        If command = DataPipelineProcessCommand.StartProcess Then
            If Not CheckDataGatewayConfigExists(id) Then _
                Throw New InvalidArgumentException(My.Resources.ClsServerDataGateways_NoDataPipelineConfigurationFileForTheProcessWithID, id)
        End If

        Dim tcpEndpoint As String
        Using con = GetConnection()
            tcpEndpoint = GetDataAccesss(con).GetPipelineProcessTcpEndpoint(id)
        End Using

        If tcpEndpoint = Nothing Then
            Throw New InvalidArgumentException(My.Resources.ClsServerDataGateways_NoDataPipelineProcessWithID, id)
        End If

        Dim split = tcpEndpoint.Split(":"c)
        Dim hostname = split(0)
        Dim port = Integer.Parse(split(1))

        Using client = New TcpClient(hostname, port)
            client.SendTimeout = Options.Instance.DataPipelineProcessSendTimeout * 1000
            Dim bytesToSend = Encoding.UTF8.GetBytes(GetCommandString(command) & vbNewLine)
            client.GetStream().Write(bytesToSend, 0, bytesToSend.Length)
            client.GetStream().Flush()
        End Using
    End Sub


    Private Function GetCommandString(cmd As DataPipelineProcessCommand) As String
        Dim commandString = ""

        Select Case cmd
            Case DataPipelineProcessCommand.StartProcess
                commandString = "start"

            Case DataPipelineProcessCommand.StopProcess
                commandString = "stop"
        End Select

        Return commandString
    End Function

End Class
