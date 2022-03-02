Imports System.Collections.Concurrent

Namespace BackgroundJobs

    ''' <summary>
    ''' Stores and retrieves information about the progress of background jobs running
    ''' on the server. Data is stored in memory and is designed to be scoped to the 
    ''' lifetime of a server instance.
    ''' </summary>
    Public Class BackgroundJobDataStore

        Private ReadOnly mJobData As New ConcurrentDictionary(Of Guid, BackgroundJobData)

        ''' <summary>
        ''' Gets most recent data about execution of a background job
        ''' </summary>
        ''' <param name="id">Job identifier</param>
        ''' <param name="clearWhenComplete">Indicates whether information about job can be deleted on the server
        ''' if the job is complete</param>
        Public Function GetBackgroundJob(id As Guid, clearWhenComplete As Boolean) As BackgroundJobData 
            Dim result As BackgroundJobData = Nothing
            If Not mJobData.TryGetValue(id, result) Then
                result = BackgroundJobData.Unknown
            End If
            If result.IsComplete AndAlso clearWhenComplete Then
                RemoveBackgroundJob(id)
            End If
            Return result
        End Function

        ''' <summary>
        ''' Updates data stored for the specified job
        ''' </summary>
        ''' <param name="id">Job identifier</param>
        ''' <param name="data">Data about the job</param>
        Public Sub UpdateJob(id As Guid, data As BackgroundJobData )
            mJobData(id) = data
        End Sub
            
        ''' <summary>
        ''' Clears data held for a background job
        ''' </summary>
        ''' <param name="id">Job identifier</param>
        Public Sub RemoveBackgroundJob(id As Guid) 
            Dim result As BackgroundJobData = Nothing
            mJobData.TryRemove(id, result)
        End Sub

        ''' <summary>
        ''' Clears any data held for jobs that have not been updated since the
        ''' specified minimum date
        ''' </summary>
        ''' <param name="minDate">Minimum date for which updates should be retained</param>
        Public Sub RemoveExpiredJobs(minDate As Date)
            Dim expiredJobIds = From item In mJobData
                    Where item.Value.Date <= minDate
                    Select item.Key
            For Each id In expiredJobIds
                Dim result As BackgroundJobData = Nothing
                mJobData.TryRemove(id, result)
            Next
        End Sub
    End Class
End NameSpace