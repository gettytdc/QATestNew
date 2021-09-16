Imports BluePrism.Data.DataModels.WorkQueueAnalysis

Public Class WorkQueueAnalysisAuditHelper

    Public Function ConvertSnapshotDayConfigurationToList(config As SnapshotDayConfiguration) As List(Of String)
        Dim listOfDays = New List(Of String)

        If config.Monday Then listOfDays.Add(My.Resources.DayOfTheWeek_Monday)
        If config.Tuesday Then listOfDays.Add(My.Resources.DayOfTheWeek_Tuesday)
        If config.Wednesday Then listOfDays.Add(My.Resources.DayOfTheWeek_Wednesday)
        If config.Thursday Then listOfDays.Add(My.Resources.DayOfTheWeek_Thursday)
        If config.Friday Then listOfDays.Add(My.Resources.DayOfTheWeek_Friday)
        If config.Saturday Then listOfDays.Add(My.Resources.DayOfTheWeek_Saturday)
        If config.Sunday Then listOfDays.Add(My.Resources.DayOfTheWeek_Sunday)
        Return listOfDays
    End Function

    Private Function ConvertListToString(list As IEnumerable(Of String)) As String
        Dim output = New StringBuilder()

        For Each item As String In list
            output.Append(item)
            If Not list.LastOrDefault()?.Equals(item) Then output.Append(", ")
        Next

        Return output.ToString()
    End Function

    Public Function GetDaysAdded(newDayConfiguration As SnapshotDayConfiguration, oldDayConfiguration As SnapshotDayConfiguration) As String
        Dim newConfigurationAsList = ConvertSnapshotDayConfigurationToList(newDayConfiguration)
        Dim oldConfigurationAsList = ConvertSnapshotDayConfigurationToList(oldDayConfiguration)
        Dim result = newConfigurationAsList.Except(oldConfigurationAsList)

        Return ConvertListToString(result)
    End Function

    Public Function GetDaysRemoved(newDayConfiguration As SnapshotDayConfiguration, oldDayConfiguration As SnapshotDayConfiguration) As String
        Dim newConfigurationAsList = ConvertSnapshotDayConfigurationToList(newDayConfiguration)
        Dim oldConfigurationAsList = ConvertSnapshotDayConfigurationToList(oldDayConfiguration)
        Dim result = oldConfigurationAsList.Except(newConfigurationAsList)

        Return ConvertListToString(result)
    End Function

    Public Function GetQueuesAdded(changeset As SnapshottingChangeset) As String
        Return ConvertListToString(changeset.ListQueuesToAdd)
    End Function

    Public Function GetQueueRemoved(changeset As SnapshottingChangeset) As String
        Return ConvertListToString(changeset.ListQueuesToRemove)
    End Function

    Private Function ConvertListToString(list As IEnumerable(Of Integer)) As String
        Return ConvertListToString(list.Select(Function(x) x.ToString).ToList)
    End Function
End Class