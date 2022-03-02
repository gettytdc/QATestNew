Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.Data
Imports BluePrism.Data.DataModels.WorkQueueAnalysis
Imports BluePrism.Server.Domain.Models
Imports NodaTime

Namespace clsServerPartialClasses.DataAccess
    Public Class WorkQueueAnalysisDataAccess
        Private mConnection As IDatabaseConnection
        Private ReadOnly mServer As clsServer


        Public Sub New(connection As IDatabaseConnection, server As clsServer)
            mConnection = connection
            mServer = server
        End Sub

        Public Function GetSnapshotConfigurations() As List(Of SnapshotConfiguration)
            Dim query = New StringBuilder(
                "SELECT id,
                        isenabled,
                        [name],
                        interval,
                        timezone,
                        startsecsaftermidnight,
                        endsecsaftermidnight,
                        monday,
                        tuesday,
                        wednesday,
                        thursday,
                        friday,
                        saturday,
                        sunday
                 FROM BPASnapshotConfiguration;"
                )
            Dim result = New List(Of SnapshotConfiguration)

            Using command = New SqlCommand(query.ToString())
                Using reader = mConnection.ExecuteReturnDataReader(command)
                    While reader.Read()
                        Dim id = reader.GetInt32(0)
                        Dim enabled = reader.GetBoolean(1)
                        Dim name = reader.GetString(2)
                        Dim interval = CType(reader.GetInt32(3), SnapshotInterval)
                        Dim timezone = TimeZoneInfo.FindSystemTimeZoneById(reader.GetString(4))
                        Dim startTime = LocalTime.FromSecondsSinceMidnight(reader.GetInt32(5))
                        Dim endTime = LocalTime.FromSecondsSinceMidnight(reader.GetInt32(6))
                        Dim monday = reader.GetBoolean(7)
                        Dim tuesday = reader.GetBoolean(8)
                        Dim wednesday = reader.GetBoolean(9)
                        Dim thursday = reader.GetBoolean(10)
                        Dim friday = reader.GetBoolean(11)
                        Dim saturday = reader.GetBoolean(12)
                        Dim sunday = reader.GetBoolean(13)

                        Dim daysOfTheWeek = New SnapshotDayConfiguration(monday, tuesday, wednesday, thursday, friday, saturday, sunday)
                        Dim configuration = New SnapshotConfiguration(id, enabled, name, interval, timezone, startTime, endTime, daysOfTheWeek)

                        result.Add(configuration)
                    End While
                End Using
            End Using

            Return result
        End Function

        Public Function GetWorkQueueNamesAssociatedToSnapshotConfiguration(configurationId As Integer) As ICollection(Of String)
            Const idToken = "@id"
            Dim query = New StringBuilder(
                $"SELECT [name]
                 FROM BPAWorkQueue
                 WHERE snapshotconfigurationid = {idToken};"
                )
            Dim result = New List(Of String)

            Using command = New SqlCommand(query.ToString())
                command.Parameters.AddWithValue(idToken, configurationId)

                Using reader = mConnection.ExecuteReturnDataReader(command)
                    While reader.Read()
                        result.Add(reader.GetString(0))
                    End While
                End Using
            End Using

            If result.Any() Then
                Return result
            Else
                Return Nothing
            End If
        End Function

        Public Function GetWorkQueueIdentifiersAssociatedToSnapshotConfiguration(configurationId As Integer) As ICollection(Of Integer)
            Const idToken = "@id"
            Dim query = New StringBuilder(
                $"SELECT [ident]
                 FROM BPAWorkQueue
                 WHERE snapshotconfigurationid = {idToken};"
                )
            Dim result = New List(Of Integer)()

            Using command = New SqlCommand(query.ToString())
                command.Parameters.AddWithValue(idToken, configurationId)

                Using reader = mConnection.ExecuteReturnDataReader(command)
                    While reader.Read()
                        result.Add(reader.GetInt32(0))
                    End While
                End Using
            End Using
            Return result

        End Function

        Public Function DeleteSnapshotConfiguration(id As Integer, name As String) As Integer
            Try
                mConnection.BeginTransaction()

                Dim numberOfConfigurationsDeleted = DeleteSnapshotConfigurationFromDatabase(id)
                If numberOfConfigurationsDeleted > 0 Then
                    mServer.IncrementDataVersion(mConnection, DataNames.ConfiguredSnapshots)
                End If

                mServer.AuditRecordWorkQueueAnalysisEvent(mConnection, WorkQueueAnalysisEventCode.DeleteSnapshotConfiguration, name, String.Format(My.Resources.AuditWQA_SnapshotID0SnapshotName1, id, name))
                mConnection.CommitTransaction()
                Return numberOfConfigurationsDeleted
            Catch ex As Exception
                mConnection.RollbackTransaction()
                Throw
            End Try
        End Function

        Private Function DeleteSnapshotConfigurationFromDatabase(id As Integer) As Integer
            Const idToken = "@id"
            Dim query = New StringBuilder(
                $"DELETE FROM BPASnapshotConfiguration
                  WHERE id = {idToken};"
                )

            Using command = New SqlCommand(query.ToString())
                command.Parameters.AddWithValue(idToken, id)

                Return mConnection.ExecuteReturnRecordsAffected(command)
            End Using
        End Function

        Private Function GetSnapshotConfigurationById(configId As Integer) As SnapshotConfiguration

            Dim query As New StringBuilder(
                "SELECT [id],
                        isenabled,
                        name,
                        interval,
                        timezone,
                        startsecsaftermidnight,
                        endsecsaftermidnight,
                        monday,
                        tuesday,
                        wednesday,
                        thursday,
                        friday,
                        saturday,
                        sunday
                 FROM BPASnapshotConfiguration
                WHERE id = @id;"
                )

            Dim result As SnapshotConfiguration = Nothing

            Using command = New SqlCommand(query.ToString())
                command.AddParameter("@id", configId)
                Using reader = mConnection.ExecuteReturnDataReader(command)
                    While reader.Read()
                        Dim id = reader.GetInt32(0)
                        Dim enabled = reader.GetBoolean(1)
                        Dim name = reader.GetString(2)
                        Dim interval = CType(reader.GetInt32(3), SnapshotInterval)
                        Dim timezone = TimeZoneInfo.FindSystemTimeZoneById(reader.GetString(4))
                        Dim startTime = LocalTime.FromSecondsSinceMidnight(reader.GetInt32(5))
                        Dim endTime = LocalTime.FromSecondsSinceMidnight(reader.GetInt32(6))
                        Dim monday = reader.GetBoolean(7)
                        Dim tuesday = reader.GetBoolean(8)
                        Dim wednesday = reader.GetBoolean(9)
                        Dim thursday = reader.GetBoolean(10)
                        Dim friday = reader.GetBoolean(11)
                        Dim saturday = reader.GetBoolean(12)
                        Dim sunday = reader.GetBoolean(13)

                        Dim daysOfTheWeek = New SnapshotDayConfiguration(monday, tuesday, wednesday, thursday, friday, saturday, sunday)
                        result = New SnapshotConfiguration(id, enabled, name, interval, timezone, startTime, endTime, daysOfTheWeek)
                    End While
                End Using
            End Using

            Return result
        End Function
        Public Function GetSnapshotConfigurationByName(configName As String) As SnapshotConfiguration

            Dim query As New StringBuilder(
                "SELECT id,
                        isenabled,
                        [name],
                        interval,
                        timezone,
                        startsecsaftermidnight,
                        endsecsaftermidnight,
                        monday,
                        tuesday,
                        wednesday,
                        thursday,
                        friday,
                        saturday,
                        sunday
                 FROM BPASnapshotConfiguration
                WHERE name = @name;"
                )

            Dim result As SnapshotConfiguration = Nothing

            Using command = New SqlCommand(query.ToString())
                command.AddParameter("@name", configName)
                Using reader = mConnection.ExecuteReturnDataReader(command)
                    While reader.Read()
                        Dim id = reader.GetInt32(0)
                        Dim enabled = reader.GetBoolean(1)
                        Dim name = reader.GetString(2)
                        Dim interval = CType(reader.GetInt32(3), SnapshotInterval)
                        Dim timezone = TimeZoneInfo.FindSystemTimeZoneById(reader.GetString(4))
                        Dim startTime = LocalTime.FromSecondsSinceMidnight(reader.GetInt32(5))
                        Dim endTime = LocalTime.FromSecondsSinceMidnight(reader.GetInt32(6))
                        Dim monday = reader.GetBoolean(7)
                        Dim tuesday = reader.GetBoolean(8)
                        Dim wednesday = reader.GetBoolean(9)
                        Dim thursday = reader.GetBoolean(10)
                        Dim friday = reader.GetBoolean(11)
                        Dim saturday = reader.GetBoolean(12)
                        Dim sunday = reader.GetBoolean(13)

                        Dim daysOfTheWeek = New SnapshotDayConfiguration(monday, tuesday, wednesday, thursday, friday, saturday, sunday)
                        result = New SnapshotConfiguration(id, enabled, name, interval, timezone, startTime, endTime, daysOfTheWeek)

                    End While
                End Using
            End Using

            Return result
        End Function

        Private Sub SaveSnapshotConfiguration(ByRef config As SnapshotConfiguration, originalConfigName As String)
            If config.Id = -1 Then
                Dim newId = CreateSnapshotConfiguration(config)
                config.Id = newId
                mServer.AuditRecordWorkQueueAnalysisEvent(mConnection, WorkQueueAnalysisEventCode.CreateSnapshotConfiguration, config.Name, String.Format(My.Resources.AuditWQA_SnapshotID0SnapshotName1, config.Id, config.Name))
            Else
                UpdateSnapshotConfiguration(config, originalConfigName)
            End If
        End Sub

        Private Function CreateSnapshotConfiguration(config As SnapshotConfiguration) As Integer

            If SnapshotConfigurationNameExists(config.Name) Then Throw New NameAlreadyExistsException()

            Using command As New SqlCommand
                command.CommandText = "INSERT INTO BPASnapshotConfiguration
           ([interval]
           ,[name]
           ,[timezone]
           ,[startsecsaftermidnight]
           ,[endsecsaftermidnight]
           ,[sunday]
           ,[monday]
           ,[tuesday]
           ,[wednesday]
           ,[thursday]
           ,[friday]
           ,[saturday]
           ,[isenabled])
     VALUES
           (@interval
           ,@name
           ,@timezone
           ,@startTime
           ,@endtime
           ,@sunday
           ,@monday
           ,@tuesday
           ,@wednesday
           ,@thursday
           ,@friday
           ,@saturday
           ,@isenabled);
select cast(scope_identity() as int)"

                With command
                    .Parameters.AddWithValue("@interval", config.Interval)
                    .Parameters.AddWithValue("@name", config.Name)
                    .Parameters.AddWithValue("@timezone", config.Timezone.Id)
                    .Parameters.AddWithValue("@startTime", config.StartTime.TickOfDay / NodaConstants.TicksPerSecond)
                    .Parameters.AddWithValue("@endtime", config.EndTime.TickOfDay / NodaConstants.TicksPerSecond)
                    .Parameters.AddWithValue("@sunday", config.DaysOfTheWeek.Sunday)
                    .Parameters.AddWithValue("@monday", config.DaysOfTheWeek.Monday)
                    .Parameters.AddWithValue("@tuesday", config.DaysOfTheWeek.Tuesday)
                    .Parameters.AddWithValue("@wednesday", config.DaysOfTheWeek.Wednesday)
                    .Parameters.AddWithValue("@thursday", config.DaysOfTheWeek.Thursday)
                    .Parameters.AddWithValue("@friday", config.DaysOfTheWeek.Friday)
                    .Parameters.AddWithValue("@saturday", config.DaysOfTheWeek.Saturday)
                    .Parameters.AddWithValue("@isenabled", config.Enabled)
                End With
                Return CInt(mConnection.ExecuteReturnScalar(command))
            End Using

        End Function

        Private Function UpdateSnapshotConfiguration(config As SnapshotConfiguration, originalConfigName As String) As Boolean
            If originalConfigName <> config.Name AndAlso SnapshotConfigurationNameExists(config.Name) Then
                Throw New NameAlreadyExistsException()
            End If

            Using command As New SqlCommand
                command.CommandText = "UPDATE BPASnapshotConfiguration
   SET [interval] = @interval
      ,[name] = @name
      ,[timezone] = @timezone
      ,[startsecsaftermidnight] = @startTime
      ,[endsecsaftermidnight] = @endtime
      ,[sunday] = @sunday
      ,[monday] = @monday
      ,[tuesday] = @tuesday
      ,[wednesday] = @wednesday
      ,[thursday] = @thursday
      ,[friday] = @friday
      ,[saturday] = @saturday
      ,[isenabled] = @isenabled
 WHERE id=@id"
                With command
                    .Parameters.AddWithValue("@id", config.Id)
                    .Parameters.AddWithValue("@interval", config.Interval)
                    .Parameters.AddWithValue("@name", config.Name)
                    .Parameters.AddWithValue("@timezone", config.Timezone.Id)
                    .Parameters.AddWithValue("@startTime", config.StartTime.TickOfDay / NodaConstants.TicksPerSecond)
                    .Parameters.AddWithValue("@endtime", config.EndTime.TickOfDay / NodaConstants.TicksPerSecond)
                    .Parameters.AddWithValue("@sunday", config.DaysOfTheWeek.Sunday)
                    .Parameters.AddWithValue("@monday", config.DaysOfTheWeek.Monday)
                    .Parameters.AddWithValue("@tuesday", config.DaysOfTheWeek.Tuesday)
                    .Parameters.AddWithValue("@wednesday", config.DaysOfTheWeek.Wednesday)
                    .Parameters.AddWithValue("@thursday", config.DaysOfTheWeek.Thursday)
                    .Parameters.AddWithValue("@friday", config.DaysOfTheWeek.Friday)
                    .Parameters.AddWithValue("@saturday", config.DaysOfTheWeek.Saturday)
                    .Parameters.AddWithValue("@isenabled", config.Enabled)
                End With
                mConnection.ExecuteReturnScalar(command)
            End Using

        End Function

        Private Function SnapshotConfigurationNameExists(configName As String) As Boolean
            Return GetSnapshotConfigurationByName(configName) IsNot Nothing
        End Function

        Public Function GetQueueSnapshots() As ICollection(Of QueueSnapshot)
            Dim query = New StringBuilder(
                "SELECT snapshotid,
                        queueident,
                        timeofdaysecs,
                        [dayofweek],
                        interval,
                        eventtype
                 FROM BPMIConfiguredSnapshot;"
                )
            Dim results = New List(Of QueueSnapshot)

            Using command = New SqlCommand(query.ToString())
                Using reader = mConnection.ExecuteReturnDataReader(command)
                    While reader.Read()
                        Dim snapshotId = reader.GetInt64(0)
                        Dim queueIdentifier = reader.GetInt32(1)
                        Dim timeOfDay = LocalTime.FromSecondsSinceMidnight(reader.GetInt32(2))
                        Dim dayOfWeek = CType(reader.GetInt32(3), IsoDayOfWeek)
                        Dim interval = CType(reader.GetInt32(4), SnapshotInterval)
                        Dim eventType = CType(reader.GetInt32(5), SnapshotTriggerEventType)

                        Dim configuredSnapshot = New QueueSnapshot(snapshotId, queueIdentifier, timeOfDay, dayOfWeek, interval, eventType)
                        results.Add(configuredSnapshot)
                    End While
                End Using
            End Using

            Return results
        End Function

        Public Function GetQueuesWithTimezoneAndSnapshotInformation() As ICollection(Of WorkQueueSnapshotInformation)
            Dim query = New StringBuilder(
                "SELECT BPAWorkQueue.ident,
                        BPAWorkQueue.lastsnapshotid,
                        BPASnapshotConfiguration.timezone
                 FROM BPAWorkQueue
                 INNER JOIN BPASnapshotConfiguration
                     ON BPAWorkQueue.snapshotconfigurationid = BPASnapshotConfiguration.id;"
                )
            Dim results = New List(Of WorkQueueSnapshotInformation)

            Using command = New SqlCommand(query.ToString())
                Using reader = mConnection.ExecuteReturnDataReader(command)
                    While reader.Read()
                        Dim queueIdent = reader.GetInt32(0)
                        Dim lastSnapshotId As Long = -1
                        If Not reader.IsDBNull(1) Then
                            lastSnapshotId = reader.GetInt64(1)
                        End If
                        Dim timezone = TimeZoneInfo.FindSystemTimeZoneById(reader.GetString(2))

                        Dim workQueueInformation = New WorkQueueSnapshotInformation(queueIdent, lastSnapshotId, timezone)
                        results.Add(workQueueInformation)
                    End While
                End Using
            End Using

            Return results
        End Function

        Public Function RemoveDuplicateSnapshotTriggers(queuesToSnapshot As ICollection(Of WorkQueueSnapshotInformation)) As ICollection(Of WorkQueueSnapshotInformation)
            Dim currentTriggersInDatabase As IEnumerable(Of Tuple(Of Integer, Long)) = GetSnapshotTriggers()

            For Each queue In queuesToSnapshot
                queue.snapshotIdsToProcess _
                .RemoveAll(Function(x) currentTriggersInDatabase _
                .Any(Function(y) y.Item1 = queue.QueueIdentifier AndAlso y.Item2 = x.SnapshotId))
            Next

            Return queuesToSnapshot
        End Function

        Private Function GetSnapshotTriggers() As IEnumerable(Of Tuple(Of Integer, Long))
            Dim result = New List(Of Tuple(Of Integer, Long))
            Dim query = New StringBuilder(
                "SELECT queueident,
                        snapshotid
                 FROM BPMISnapshotTrigger;"
                )

            Using command = New SqlCommand(query.ToString())
                Using reader = mConnection.ExecuteReturnDataReader(command)
                    While reader.Read()
                        Dim snapshotTrigger = New Tuple(Of Integer, Long)(reader.GetInt32(0), reader.GetInt64(1))
                        result.Add(snapshotTrigger)
                    End While
                End Using
            End Using

            Return result
        End Function

        Public Function SetQueuesToBeSnapshotted(queuesToSnapshot As ICollection(Of WorkQueueSnapshotInformation)) As Integer
            Const snapshotTriggerTableName = "BPMISnapshotTrigger"
            Dim dataTable = GetSnapshotTriggerDataTable(queuesToSnapshot)

            return mConnection.InsertDataTable(dataTable, snapshotTriggerTableName)
        End Function

        Private Shared Function GetSnapshotTriggerDataTable(workQueueSnapshotInformations As ICollection(Of WorkQueueSnapshotInformation)) As DataTable
            Dim dataTable = CreateSnapshotTriggerDataTableSchema()

            return AddSnapshotTriggersToDataTable(dataTable, workQueueSnapshotInformations)
        End Function

        Private Shared Function AddSnapshotTriggersToDataTable(dataTable As DataTable, workQueueSnapshotInformations As ICollection(Of WorkQueueSnapshotInformation)) As DataTable
            For each queue In workQueueSnapshotInformations
                For Each snapshotTriggerInformation In queue.SnapshotIdsToProcess
                    Dim row = dataTable.NewRow()

                    row.SetField(0, queue.QueueIdentifier)
                    row.SetField(1, snapshotTriggerInformation.SnapshotId)
                    row.SetField(2, IIf(queue.LastSnapshotId <> -1, queue.LastSnapshotId, DBNull.Value))
                    row.SetField(3, snapshotTriggerInformation.EventType)
                    row.SetField(4, snapshotTriggerInformation.SnapshotTimeOffset)

                    dataTable.rows.Add(row)
                Next
            Next        
            
            Return dataTable
        End Function

        Private Shared Function CreateSnapshotTriggerDataTableSchema() As DataTable
            Dim dataTable = New DataTable()

            dataTable.Columns.Add(New DataColumn("queueident", GetType(Integer)))
            dataTable.Columns.Add(New DataColumn("snapshotid", GetType(Long)))
            dataTable.Columns.Add(New DataColumn("lastsnapshotid", GetType(Long)))
            dataTable.Columns.Add(New DataColumn("eventtype", GetType(Integer)))
            dataTable.Columns.Add(New DataColumn("snapshotdate", GetType(DateTimeOffset)))

            Return dataTable
        End Function

        Public Sub StartQueueSnapshottingProcess()
            Dim query = New StringBuilder("usp_TriggerQueueSnapshot")

            Using command = New SqlCommand(query.ToString)
                command.CommandType = CommandType.StoredProcedure
                mConnection.Execute(command)
            End Using
        End Sub

        Private Function GetQueuesAndSnapshotConfigurations() As DataTable
            Using command As New SqlCommand
                command.CommandText = "SELECT q.ident, q.snapshotconfigurationid, s.isenabled 
FROM BPAWorkQueue q LEFT JOIN BPAsnapshotconfiguration as s ON q.snapshotconfigurationid = s.id
WHERE snapshotconfigurationid IS NOT NULL"
                Return mConnection.ExecuteReturnDataTable(command)


            End Using
        End Function

        Private Sub UpdateQueuesWithSnapshotConfigId(configurationId As Integer, listQueues As List(Of Integer))
            Using command As New SqlCommand
                command.Parameters.AddWithValue("@configId", configurationId)
                mServer.UpdateMultipleIds(mConnection, command, listQueues, "id",
                                  "update BPAWorkqueue SET snapshotconfigurationid = @configId WHERE ident IN("
                                  )

            End Using
        End Sub

        Private Sub RemoveSnapshotConfigurationFromQueues(listQueues As List(Of Integer))
            Using command As New SqlCommand
                mServer.UpdateMultipleIds(mConnection, command, listQueues, "id",
                                  "update BPAWorkqueue SET snapshotconfigurationid = NULL WHERE ident IN("
                                  )

            End Using
        End Sub

        Private Sub ApplySnapshotConfigurationToQueues(configToApply As SnapshotConfiguration,
                                                      listQueuesToConfigure As List(Of Integer),
                                                      changeset As SnapshottingChangeset)
            Dim configId = configToApply.Id
            Dim queuesToRemoveConfigFrom As New List(Of Integer)
            If configId <> -1 Then
                Dim allQueuesWithThisConfigurationCurrently = GetWorkQueueIdentifiersAssociatedToSnapshotConfiguration(configId)
                queuesToRemoveConfigFrom =
                    allQueuesWithThisConfigurationCurrently?.
                        Where(Function(q) Not listQueuesToConfigure.Contains(q)).
                        ToList
            End If

            If queuesToRemoveConfigFrom?.Count > 0 Then _
                RemoveSnapshotConfigurationFromQueues(queuesToRemoveConfigFrom)

            UpdateQueuesWithSnapshotConfigId(configId, listQueuesToConfigure)

            DeleteSnapshotAndTrendData(changeset.QueuesToDeleteSnapshotAndTrendData)

            InsertRowsIntoConfiguredSnapshotTable(changeset.QueuesAndConfigurationsToAddSnapshotRows)

        End Sub

        Private Sub DeleteSnapshotAndTrendData(queueIdentifiers As List(Of Integer))
            If queueIdentifiers IsNot Nothing AndAlso queueIdentifiers.Any Then
                DeleteFromBPMIConfiguredSnapshot(queueIdentifiers)
                DeleteFromBPMIQueueTrend(queueIdentifiers)
                DeleteFromBPMIQueueInterimSnapshot(queueIdentifiers)
                ResetLastSnapshotId(queueIdentifiers)
            End If
        End Sub

        Private Sub ResetLastSnapshotId(queueIdentifiers As List(Of Integer))
            Using command As New SqlCommand
                mServer.UpdateMultipleIds(mConnection, command, queueIdentifiers, "id",
                                  "UPDATE BPAWorkQueue SET lastsnapshotid = NULL WHERE ident IN("
                                  )
            End Using
        End Sub

        Private Sub DeleteFromBPMIQueueTrend(queueIdentifiers As List(Of Integer))
            Using command As New SqlCommand
                mServer.UpdateMultipleIds(mConnection, command, queueIdentifiers, "id",
                                  "DELETE FROM BPMIQueueTrend WHERE queueident IN("
                                  )
            End Using
        End Sub

        Private Sub DeleteFromBPMIConfiguredSnapshot(queueIdentifiers As List(Of Integer))
            Using command As New SqlCommand
                mServer.UpdateMultipleIds(mConnection, command, queueIdentifiers, "id",
                                  "DELETE FROM BPMIConfiguredSnapshot WHERE queueident IN("
                                  )
            End Using
        End Sub

        Private Sub DeleteFromBPMIQueueInterimSnapshot(queueIdentifiers As List(Of Integer))
            Using command As New SqlCommand
                mServer.UpdateMultipleIds(mConnection, command, queueIdentifiers, "id",
                                          "DELETE FROM BPMIQueueInterimSnapshot WHERE queueident IN("
                                          )
            End Using
        End Sub

        Private Sub InsertRowsIntoConfiguredSnapshotTable(queueAndConfigIds As Dictionary(Of Integer, Integer))
            If queueAndConfigIds?.Any Then
                Dim snapshotTable As DataTable = GetSnapshotTable()
                For Each queueConfigPair In queueAndConfigIds

                    Dim queueIdentifier = queueConfigPair.Key
                    Dim config = GetSnapshotConfigurationById(queueConfigPair.Value)
                    Dim snapShotTimesInSecs = config.GetSnapshotTimes()
                    Dim interimSnapshotDaysAndTimes = config.GetInterimSnapshotDaysAndTimes()
                    Dim trendCalculationDaysAndTimes = config.GetTrendCalculationDaysAndTimes()

                    For Each dayId In config.DaysOfTheWeek.GetDaysMap()
                        For Each snapshotTime In snapShotTimesInSecs
                            Dim eventType = SnapshotTriggerEventType.Snapshot
                            Dim indexOfTrendCalc = trendCalculationDaysAndTimes.FindIndex(
                                Function(x) Enumerable.SequenceEqual(x, New Integer() {dayId, snapshotTime}))
                            If indexOfTrendCalc > -1 Then
                                eventType = SnapshotTriggerEventType.Snapshot Or SnapshotTriggerEventType.Trend
                                trendCalculationDaysAndTimes.RemoveAt(indexOfTrendCalc)
                            End If
                            AddSnapshotDataTableRow(snapshotTable, queueIdentifier,
                                                    config.Interval, snapshotTime, dayId, eventType)
                        Next
                    Next

                    For Each trendCalculationDayAndTime In trendCalculationDaysAndTimes
                        AddSnapshotDataTableRow(snapshotTable, queueIdentifier,
                                                config.Interval,
                                                trendCalculationDayAndTime(1),
                                                trendCalculationDayAndTime(0),
                                                SnapshotTriggerEventType.Trend)
                    Next

                    For Each interimSnapshotDayAndTime In interimSnapshotDaysAndTimes
                        AddSnapshotDataTableRow(snapshotTable, queueIdentifier, config.Interval,
                                                interimSnapshotDayAndTime(1),
                                                interimSnapshotDayAndTime(0),
                                                SnapshotTriggerEventType.InterimSnapshot)

                    Next
                Next
                mServer.InsertDataTable(mConnection, snapshotTable, "BPMIConfiguredSnapshot")
            End If
        End Sub

        Private Sub AddSnapshotDataTableRow(ByRef snapshotTable As DataTable,
                                                    queueidentifier As Integer,
                                                    interval As SnapshotInterval,
                                                    timeOfDaySecs As Integer,
                                                    dayOfWeek As Integer,
                                                    eventType As Integer)
            Dim row = snapshotTable.NewRow()
            row("queueident") = queueidentifier
            row("interval") = interval
            row("timeofdaysecs") = timeOfDaySecs
            row("dayofweek") = dayOfWeek
            row("eventtype") = eventType
            snapshotTable.Rows.Add(row)
        End Sub

        Private Function GetSnapshotTable() As DataTable
            Dim table As New DataTable
            table.Columns.Add(New DataColumn("queueident", GetType(Integer)))
            table.Columns.Add(New DataColumn("timeofdaysecs", GetType(Integer)))
            table.Columns.Add(New DataColumn("dayofweek", GetType(Integer)))
            table.Columns.Add(New DataColumn("interval", GetType(Integer)))
            table.Columns.Add(New DataColumn("eventtype", GetType(Integer)))
            Return table
        End Function

        Public Function SaveConfigurationAndApplyToQueues(configToSave As SnapshotConfiguration,
                                                          originalConfigName As String,
                                                          queuesToConfigure As List(Of Integer)) As Boolean
            Try
                mConnection.BeginTransaction()

                Dim oldConfig As SnapshotConfiguration = Nothing
                Dim oldQueueIdentifiers As New List(Of Integer)()
                Dim currentQueueConfigurations As QueueConfigurationsDataTable = Nothing
                RefreshAllConfigDataFromDatabase(configToSave, oldConfig, oldQueueIdentifiers, currentQueueConfigurations)
                SaveSnapshotConfiguration(configToSave, originalConfigName)

                Dim changeset = New SnapshottingChangeset(oldConfig,
                                                           configToSave,
                                                           oldQueueIdentifiers,
                                                           queuesToConfigure,
                                                           currentQueueConfigurations)

                ApplySnapshotConfigurationToQueues(configToSave, queuesToConfigure, changeset)
                mServer.IncrementDataVersion(mConnection, DataNames.ConfiguredSnapshots)
                AuditSnapshotConfigurationChangeset(configToSave, oldConfig, changeset)
                mConnection.CommitTransaction()
                Return True
            Catch ex As Exception
                mConnection.RollbackTransaction()
                Throw
            End Try

        End Function

        Private Sub AuditSnapshotConfigurationChangeset(configToSave As SnapshotConfiguration, oldConfig As SnapshotConfiguration, changeset As SnapshottingChangeset)
            If oldConfig IsNot Nothing AndAlso (changeset.ConfigPropertiesHaveChanged OrElse changeset.ConfigNameOrEnabledHasChanged) Then
                mServer.AuditRecordSnapshotConfigurationChangesEvent(mConnection, WorkQueueAnalysisEventCode.EditSnapshotConfiguration, oldConfig, configToSave)
            End If

            Dim snapshotIDInfo = String.Format(My.Resources.AuditWQA_SnapshotConfigID0, configToSave.Id)

            Dim helper = New WorkQueueAnalysisAuditHelper()
            Dim comment = New StringBuilder()

            If changeset.QueuesAreBeingApplied Then
                Dim queuesAdded = helper.GetQueuesAdded(changeset)
                comment.Append(String.Format(My.Resources.AuditWQA_NewQueuesAssigned0, queuesAdded) + " ")
            End If

            If changeset.QueuesAreBeingRemoved Then
                Dim queuesRemoved = helper.GetQueueRemoved(changeset)
                comment.Append(String.Format(My.Resources.AuditWQA_QueueRemoved0, queuesRemoved))
            End If

            If comment.Length > 0 Then
                Dim auditInformation = $"{snapshotIDInfo} {comment.ToString()}"
                mServer.AuditRecordWorkQueueAnalysisEvent(mConnection, WorkQueueAnalysisEventCode.EditSnapshotConfiguration, configToSave.Name, auditInformation)
            End If
        End Sub
        Private Sub RefreshAllConfigDataFromDatabase(configToSave As SnapshotConfiguration,
                                                     ByRef oldConfig As SnapshotConfiguration,
                                                     ByRef oldQueueIdentifiers As List(Of Integer),
                                                     ByRef currentQueueConfigurations As QueueConfigurationsDataTable)
            Dim newConfigId = configToSave.Id
            If newConfigId = -1 Then
                oldConfig = Nothing
            Else
                oldConfig = GetSnapshotConfigurationById(newConfigId)
            End If

            If newConfigId <> -1 Then
                oldQueueIdentifiers = If(GetWorkQueueIdentifiersAssociatedToSnapshotConfiguration(newConfigId)?.ToList(), New List(Of Integer)())
            End If

            Dim currentConfigsTable = GetQueuesAndSnapshotConfigurations()
            Dim distinctConfigIdsInUse As List(Of Integer) =
                    currentConfigsTable.
                    AsEnumerable.
                    Select(Function(row) row.Field(Of Integer)(1)).
                    Distinct.
                    ToList

            Dim allConfigs = GetSnapshotConfigurations()
            Dim configIDsAndConfiguredSnapshotRowCount As New Dictionary(Of Integer, Integer)
            For Each configId As Integer In distinctConfigIdsInUse
                Dim config As SnapshotConfiguration = allConfigs.FirstOrDefault(Function(c) c.Id = configId)
                configIDsAndConfiguredSnapshotRowCount.Add(configId, config.ConfiguredSnapshotRowsPerQueue())
            Next

            currentQueueConfigurations = New QueueConfigurationsDataTable(
                currentConfigsTable, configIDsAndConfiguredSnapshotRowCount)

        End Sub

        Public Function ConfigurationChangesWillCauseDataDeletion(configToSave As SnapshotConfiguration,
                                                           originalConfigName As String,
                                                           queuesToConfigure As List(Of Integer)) As Boolean
            Dim oldConfig As SnapshotConfiguration = Nothing
            Dim oldQueueIdentifiers As New List(Of Integer)()
            Dim currentQueueConfigurations As QueueConfigurationsDataTable = Nothing
            RefreshAllConfigDataFromDatabase(configToSave, oldConfig, oldQueueIdentifiers, currentQueueConfigurations)

            Dim changeset = New SnapshottingChangeset(oldConfig,
                                                          configToSave,
                                                          oldQueueIdentifiers,
                                                          queuesToConfigure,
                                                          currentQueueConfigurations)

            If changeset.QueuesToDeleteSnapshotAndTrendData.Count > 0 Then Return True
            Return False
        End Function

        Public Function ConfigurationChangesWillExceedPermittedSnapshotLimit(configToSave As SnapshotConfiguration,
                                                                             queuesToConfigure As List(Of Integer)) As Boolean

            Dim oldConfig As SnapshotConfiguration = Nothing
            Dim oldQueueIdentifiers As New List(Of Integer)()
            Dim currentQueueConfigurations As QueueConfigurationsDataTable = Nothing

            RefreshAllConfigDataFromDatabase(configToSave, oldConfig, oldQueueIdentifiers, currentQueueConfigurations)

            Dim changeset = New SnapshottingChangeset(oldConfig,
                                                      configToSave,
                                                      oldQueueIdentifiers,
                                                      queuesToConfigure,
                                                      currentQueueConfigurations)

            Return changeset.WillExceedPermittedSnapshotLimit(CountCurrentSnapshotRows())
        End Function

        Private Function CountCurrentSnapshotRows() As Integer
            Using command As New SqlCommand
                command.CommandText = "SELECT COUNT(snapshotid) from BPMIConfiguredSnapshot"
                Return CInt(mConnection.ExecuteReturnScalar(command))
            End Using
        End Function
        
        Public Sub ClearOrphanedSnapshotData()
            Using command As New SqlCommand
                command.CommandText = "DELETE FROM BPMIQueueSnapshot 
    WHERE id IN (
    SELECT snapshot.id 
    FROM BPMIQueueSnapshot snapshot LEFT JOIN BPAWorkQueue as queue ON snapshot.queueident = queue.ident
    WHERE capturedatetimeutc < DATEADD(day, -28, GETUTCDATE()) AND snapshotconfigurationid IS NULL
    )"
                mConnection.Execute(command)
            End Using
        End Sub

        Public Function TriggerExistsInDatabase(snapshotId As Long, queueIdentifier As Integer) As Boolean
            Const queueIdentToken = "@queueIdent"
            Const snapshotIdToken = "@snapshotId"

            Dim query = New StringBuilder(
                $"SELECT 1
                 FROM BPMISnapshotTrigger
                 WHERE queueident = {queueIdentToken}
                    AND snapshotid = {snapshotIdToken};"
                )

            Using command = New SqlCommand(query.ToString())
                command.Parameters.AddWithValue(queueIdentToken, queueIdentifier)
                command.Parameters.AddWithValue(snapshotIdToken, snapshotId)

                Return CBool(mConnection.ExecuteReturnScalar(command))
            End Using
        End Function

        Public Function TriggersDueToBeProcessed() As Boolean
            Dim query = New StringBuilder(
                "SELECT TOP 1 1
                 FROM BPMISnapshotTrigger
                 WHERE snapshotdateutc <= GETUTCDATE();"
                )

            Using command = New SqlCommand(query.ToString())
                Return CBool(mConnection.ExecuteReturnScalar(command))
            End Using
        End Function
    End Class
End Namespace