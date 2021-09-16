Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.DataAccess
Imports BluePrism.Data
Imports BluePrism.Data.DataModels.WorkQueueAnalysis

Partial Public Class clsServer

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function GetSnapshotConfigurations() As List(Of SnapshotConfiguration) Implements IServer.GetSnapshotConfigurations
        CheckPermissions()

        Using connection = GetConnection()
            Return GetDataAccess(connection).GetSnapshotConfigurations()
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function DeleteSnapshotConfiguration(id As Integer, name As String) As Integer Implements IServer.DeleteSnapshotConfiguration
        CheckPermissions()

        Using connection = GetConnection()
            Return GetDataAccess(connection).DeleteSnapshotConfiguration(id, name)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function GetWorkQueueNamesAssociatedToSnapshotConfiguration(id As Integer) As ICollection(Of String) Implements IServer.GetWorkQueueNamesAssociatedToSnapshotConfiguration
        CheckPermissions()

        Using connection = GetConnection()
            Return GetDataAccess(connection).GetWorkQueueNamesAssociatedToSnapshotConfiguration(id)
        End Using
    End Function
    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function GetWorkQueueIdentifiersAssociatedToSnapshotConfiguration(id As Integer) As ICollection(Of Integer) Implements IServer.GetWorkQueueIdentifiersAssociatedToSnapshotConfiguration
        CheckPermissions()

        Using connection = GetConnection()
            Return GetDataAccess(connection).GetWorkQueueIdentifiersAssociatedToSnapshotConfiguration(id)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function GetQueueSnapshots() As ICollection(Of QueueSnapshot) Implements IServer.GetQueueSnapshots
        CheckPermissions()

        Using connection = GetConnection()
            Return GetDataAccess(connection).GetQueueSnapshots()
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function GetQueuesWithTimezoneAndSnapshotInformation() As ICollection(Of WorkQueueSnapshotInformation) Implements IServer.GetQueuesWithTimezoneAndSnapshotInformation
        CheckPermissions()

        Using connection = GetConnection()
            Return GetDataAccess(connection).GetQueuesWithTimezoneAndSnapshotInformation()
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function SetQueuesToBeSnapshotted(queuesToSnapshot As ICollection(Of WorkQueueSnapshotInformation)) As Integer Implements IServer.SetQueuesToBeSnapshotted
        CheckPermissions()

        If Not queuesToSnapshot.Any() Then Return 0

        Using connection = GetConnection()
            Dim dataAccess = GetDataAccess(connection)

            queuesToSnapshot = dataAccess.RemoveDuplicateSnapshotTriggers(queuesToSnapshot)
            If Not queuesToSnapshot _
                .Any(Function(x) x.snapshotIdsToProcess.Any()) Then Return 0

            Return dataAccess.SetQueuesToBeSnapshotted(queuesToSnapshot)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Sub StartQueueSnapshottingProcess() Implements IServer.StartQueueSnapshottingProcess
        CheckPermissions()

        Using connection = GetConnection()
            GetDataAccess(connection).StartQueueSnapshottingProcess()
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Sub ClearOrphanedSnapshotData() Implements IServer.ClearOrphanedSnapshotData
        CheckPermissions()
        Using connection = GetConnection()
            GetDataAccess(connection).ClearOrphanedSnapshotData()
        End Using
    End Sub

    Private Function GetDataAccess(connection As IDatabaseConnection) As WorkQueueAnalysisDataAccess
        Return New WorkQueueAnalysisDataAccess(connection, Me)
    End Function


    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function GetSnapshotConfigurationByName(configName As String) As SnapshotConfiguration Implements IServer.GetSnapshotConfigurationByName

        CheckPermissions()

        Using connection = GetConnection()
            Dim dataAccess = GetDataAccess(connection)
            Return dataAccess.GetSnapshotConfigurationByName(configName)
        End Using
    End Function


    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Function SaveConfigurationAndApplyToQueues(configToSave As SnapshotConfiguration,
                                               originalConfigName As String,
                                               queuesToConfigure As List(Of Integer)) As Boolean Implements Iserver.SaveConfigurationAndApplyToQueues

        CheckPermissions()
        Using connection = GetConnection()
            Dim dataAccess = GetDataAccess(connection)
            Return dataAccess.SaveConfigurationAndApplyToQueues(configToSave, originalConfigName, queuesToConfigure)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Function ConfigurationChangesWillCauseDataDeletion(configToSave As SnapshotConfiguration,
                                               originalConfigName As String,
                                               queuesToConfigure As List(Of Integer)) As Boolean Implements Iserver.ConfigurationChangesWillCauseDataDeletion

        CheckPermissions()
        Using connection = GetConnection()
            Dim dataAccess = GetDataAccess(connection)
            Return dataAccess.ConfigurationChangesWillCauseDataDeletion(configToSave, originalConfigName, queuesToConfigure)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function ConfigurationChangesWillExceedPermittedSnapshotLimit(
                                                                         configToSave As SnapshotConfiguration,
                                                                         queuesToConfigure As List(Of Integer)) As Boolean Implements IServer.ConfigurationChangesWillExceedPermittedSnapshotLimit
        CheckPermissions()
        Using connection = GetConnection()
            Dim dataAccess = GetDataAccess(connection)
            Return dataAccess.ConfigurationChangesWillExceedPermittedSnapshotLimit(configToSave, queuesToConfigure)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function TriggerExistsInDatabase(snapshotId As Long, queueIdentifier As Integer) As Boolean Implements IServer.TriggerExistsInDatabase
        CheckPermissions()

        Using connection = GetConnection()
            Return GetDataAccess(connection).TriggerExistsInDatabase(snapshotId, queueIdentifier)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function TriggersDueToBeProcessed() As Boolean Implements IServer.TriggersDueToBeProcessed
        CheckPermissions()

        Using connection = GetConnection()
            Return GetDataAccess(connection).TriggersDueToBeProcessed()
        End Using
    End Function
End Class