Imports BluePrism.AutomateAppCore.Auth

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateProcessCore
Imports BluePrism.ClientServerResources.Core.Enums

Imports BluePrism.Server.Domain.Models

''' <summary>
''' Class used to hold a list of runner records, which provides some basic
''' methods available to search for runner records and ensures that any access
''' to it is limited to a single thread at any one time
''' </summary>
''' <remarks>Any operations implemented on this list, either methods or
''' properties should ensure that they get a lock on the instance lock object
''' before executing.</remarks>
Public Class RunnerRecordList : Implements IList(Of ListenerRunnerRecord)

#Region " Class-scope declarations "

    ''' <summary>
    ''' Light wrapper around an enumerator of runner records which ensures that
    ''' any access to it is locked against the lock in use in the collection
    ''' which is being enumerated.
    ''' </summary>
    Private Class RunnerRecordEnumerator
        Implements IEnumerator(Of ListenerRunnerRecord)

        ' The internal enumerator that we're controlling access to
        Private mEnumerator As IEnumerator(Of ListenerRunnerRecord)

        ' The lock used to control access to the enumerator
        Private mLock As Object

        ''' <summary>
        ''' Creates a new runner record enumerator using the values and lock from
        ''' the given list.
        ''' </summary>
        ''' <param name="list">The list from which to draw the runner records and
        ''' lock which controls access to them</param>
        Public Sub New(ByVal list As RunnerRecordList)
            mEnumerator = list.GetInnerEnumerator()
            mLock = list.mLock
        End Sub

        ''' <summary>
        ''' Gets the runner record in the collection at the current position of
        ''' the enumerator.
        ''' </summary>
        Public ReadOnly Property Current() As ListenerRunnerRecord _
            Implements IEnumerator(Of ListenerRunnerRecord).Current
            Get
                SyncLock mLock
                    Return mEnumerator.Current
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' Gets the runner record in the collection at the current position of
        ''' the enumerator.
        ''' </summary>
        Private ReadOnly Property NonGenericCurrent() As Object _
            Implements IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

        ''' <summary>
        ''' Advances the enumerator to the next element of the collection.
        ''' </summary>
        ''' <returns>true if the enumerator was successfully advanced to the next
        ''' element; false if the enumerator has passed the end of the collection
        ''' </returns>
        ''' <exception cref="InvalidOperationException">If the collection was
        ''' modified after the enumerator was created.</exception>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            SyncLock mLock
                Return mEnumerator.MoveNext()
            End SyncLock
        End Function

        ''' <summary>
        ''' Sets the enumerator to its initial position, which is before the
        ''' first element in the collection.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">If the collection was
        ''' modified after the enumerator was created.</exception>
        Public Sub Reset() Implements IEnumerator.Reset
            SyncLock mLock
                mEnumerator.Reset()
            End SyncLock
        End Sub

        ''' <summary>
        ''' Disposes of this enumerator, releasing any resources associated with
        ''' it.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            mEnumerator.Dispose()
        End Sub

    End Class

#End Region

#Region " Member Variables "

    ' The actual list of runner records which is being maintained by this object
    Private mList As New List(Of ListenerRunnerRecord)

    ' The lock used to control concurrent access to the list
    Private mLock As New Object()

#End Region

#Region " Specific Properties "

    ''' <summary>
    ''' Gets the lock object used by this class to ensure thread-safety for the
    ''' record list.
    ''' </summary>
    Friend ReadOnly Property Lock() As Object
        Get
            Return mLock
        End Get
    End Property

    ''' <summary>
    ''' Gets a count of the active runner records found in this list.
    ''' </summary>
    Public ReadOnly Property ActiveCount() As Integer
        Get
            SyncLock mLock
                Dim active As Integer = 0
                For Each r As ListenerRunnerRecord In mList
                    If r.IsActive Then active += 1
                Next
                Return active
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Gets a count of the pending runner records found in this list.
    ''' </summary>
    Public ReadOnly Property PendingCount() As Integer
        Get
            SyncLock mLock
                Dim pending As Integer = 0
                For Each r As ListenerRunnerRecord In mList
                    If r.Status = RunnerStatus.PENDING Then pending += 1
                Next
                Return pending
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property ActiveOrPendingCount() As Integer
        Get
            SyncLock mLock
                Return Me.Where(Function(x) x.IsActive OrElse x.IsPending).Count
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Gets a count of the running runner records found in this list.
    ''' </summary>
    Public ReadOnly Property RunningCount() As Integer
        Get
            SyncLock mLock
                Dim pending As Integer = 0
                For Each r As ListenerRunnerRecord In mList
                    If r.Status = RunnerStatus.RUNNING Then pending += 1
                Next
                Return pending
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Gets an accumulated status text for all runner records in this list. Each
    ''' record has an entry on its own line, separated from others by a CRLF
    ''' pair and each entry is preceded by a " - " string. After a status line
    ''' for each record is written, a count of the records is emitted with the
    ''' label "Total running: ".
    ''' </summary>
    ''' <seealso cref="ListenerRunnerRecord.StatusText"/>
    Public ReadOnly Property StatusText() As String
        Get
            SyncLock mLock
                Dim sb As New StringBuilder(Count * 30)
                For Each r As ListenerRunnerRecord In mList
                    sb.Append(" - ").Append(r.StatusText).AppendLine()
                Next
                sb.Append("Total running: ").Append(Count).AppendLine()
                Return sb.ToString()
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Checks if any runner records in this list are either 
    ''' <see cref="RunnerStatus.PENDING">pending</see> or
    ''' <see cref="RunnerStatus.RUNNING">running</see>, indicating that this
    ''' record list is busy.
    ''' </summary>
    Public ReadOnly Property IsBusy() As Boolean
        Get
            SyncLock mLock
                For Each r As ListenerRunnerRecord In mList
                    If r.IsBusy Then Return True
                Next
                Return False
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Gets the availability that this runner record list allows - eg. if this
    ''' record list contains an <see cref="BusinessObjectRunMode.Exclusive">exclusive</see>
    ''' <see cref="RunnerRecord.IsBusy">busy</see> runner record, there is no
    ''' scope for running any other sessions, so this will return
    ''' <see cref="BusinessObjectRunMode.None"/>
    ''' </summary>
    Public ReadOnly Property Availability() As BusinessObjectRunMode
        Get
            SyncLock mLock
                Dim avail As BusinessObjectRunMode = BusinessObjectRunMode.Exclusive

                For Each r As ListenerRunnerRecord In mList
                    ' If it's not busy, then we don't care
                    If Not r.IsBusy Then Continue For
                    Select Case r.mRunMode
                        Case BusinessObjectRunMode.Exclusive
                            ' If we have an exclusive session, then nothing else
                            ' can run, might as well return now
                            Return BusinessObjectRunMode.None

                        Case BusinessObjectRunMode.Foreground
                            ' Any foreground process means we can only run
                            ' background processes, so drop to lowest (available)
                            ' value.
                            avail = BusinessObjectRunMode.Background

                        Case BusinessObjectRunMode.Background
                            ' Any background processes indicate that exclusive
                            ' processes are not allowed, so drop the availability
                            If avail <> BusinessObjectRunMode.Background Then _
                                avail = BusinessObjectRunMode.Foreground

                    End Select
                Next
                Return avail
            End SyncLock
        End Get
    End Property

#End Region

#Region " Specific Methods "

    ''' <summary>
    ''' Finds the (first) runner record with the given session ID from this
    ''' set of runner records.
    ''' </summary>
    ''' <returns>The runner record which is responsible for the given session ID,
    ''' or null if no such runner record was found.</returns>
    Public Function FindRunner(ByVal sessionId As Guid) As ListenerRunnerRecord
        SyncLock mLock
            For Each r As ListenerRunnerRecord In mList
                If r.SessionID = sessionId Then Return r
            Next
            Return Nothing
        End SyncLock
    End Function

    ''' <summary>
    ''' Finds the first runner record for a session which starts with the given
    ''' partial ID in this list of runner records.
    ''' </summary>
    ''' <param name="partialId">The partial ID to search for. Note that for any
    ''' alpha digits in the string, the search is case-insensitive. Also, any
    ''' dashes are ignored - only the actual GUID value is tested.</param>
    ''' <returns>The first runner record found whose session ID starts with the
    ''' given partial ID</returns>
    Public Function FindRunner(ByVal partialId As String) As ListenerRunnerRecord
        Return CollectionUtil.First(FindRunners(partialId))
    End Function

    ''' <summary>
    ''' Finds all runner records which start with the given partial ID.
    ''' </summary>
    ''' <param name="partialId">The partial ID to search for. Note that for any
    ''' alpha digits in the string, the search is case-insensitive. Also, any
    ''' dashes are ignored - only the actual GUID value is tested.</param>
    ''' <returns>Any runner records whose session IDs started with the given
    ''' partial ID</returns>
    Public Function FindRunners(ByVal partialId As String) As RunnerRecordList
        If partialId Is Nothing Then Throw New ArgumentNullException(NameOf(partialId))
        SyncLock mLock

            ' Guid.ToString() states that it returns lower-case alpha in its
            ' output; ensure that the partialId uses the same convention
            ' Also, we drop the dashes so that we can just test the actual number
            ' so ensure our test string has no dashes in it too.
            Dim lower As String = partialId.ToLower().Replace("-", "")

            ' Go through each runner and test if the ID starts with our value
            Dim recs As New RunnerRecordList()
            For Each r As ListenerRunnerRecord In mList
                If r.SessionID.ToString("N").StartsWith(lower) Then recs.Add(r)
            Next

            ' Return any runners whose IDs started with the specified value
            Return recs

        End SyncLock

    End Function

    ''' <summary>
    ''' Stops all the runners in this list with the given reason
    ''' </summary>
    ''' <param name="reason">The reason for stopping all the runners</param>
    Public Sub StopRunners(ByVal reason As String)
        SyncLock mLock
            For Each rr As ListenerRunnerRecord In mList
                If rr.Process IsNot Nothing Then rr.StopProcess(
                    User.CurrentName, Resources.ResourceMachine.GetName(), reason)
            Next
        End SyncLock
    End Sub

    ''' <summary>
    ''' Finds the <see cref="ListenerRunnerRecord.mAutoInstance">auto-instance</see>
    ''' runner record which is serving the process with the given ID.
    ''' </summary>
    ''' <param name="procId">The process ID for the required auto-instance runner
    ''' record</param>
    ''' <returns>The runner record in this list which is processing the process
    ''' with the given ID; or null if there is none such.</returns>
    Public Function FindAuto(ByVal procId As Guid) As ListenerRunnerRecord
        SyncLock mLock
            For Each rr As ListenerRunnerRecord In mList
                If rr.mAutoInstance AndAlso rr.ProcessId = procId Then Return rr
            Next
            Return Nothing
        End SyncLock
    End Function

    ''' <summary>
    ''' Finds or creates an <see cref="ListenerRunnerRecord.mAutoInstance">
    ''' auto-instance</see> record to serve a specified process.
    ''' </summary>
    ''' <param name="procId">The ID of the process for which an autoinstance is
    ''' required.</param>
    ''' <param name="owner">The clsListener instance which any newly created
    ''' runner record should be created by.</param>
    ''' <param name="userId">The user ID for the session to be created. Ignored
    ''' if an existing auto-instance is found.</param>
    ''' <param name="resId">The resource ID to register on any new runner record.
    ''' Ignored if an existing auto-instance is found.</param>
    ''' <returns>The runner record dealing with the specified ID.</returns>
    ''' <exception cref="OperationFailedException">If no existing autoinstance
    ''' was found for the given process and the attempt to create a new one
    ''' failed for some reason.</exception>
    ''' <remarks>This whole operation operates inside a synchronization lock on
    ''' this list, meaning that a second autoinstance record for a specified
    ''' process should never be created.</remarks>
    Public Function FindOrCreateAuto(procId As Guid, owner As clsListener, userId As Guid,
                                     resId As Guid, token As clsAuthToken) As ListenerRunnerRecord
        SyncLock mLock
            ' If one already exists, return that
            Dim newRunner As ListenerRunnerRecord = FindAuto(procId)
            If newRunner IsNot Nothing Then Return newRunner
            Try
                Dim sErr As String = Nothing
                Dim runnerRequest = New RunnerRequest() With {
                    .SessionId = Guid.NewGuid,
                    .ProcessId = procId,
                    .StarterUserId = userId,
                    .StarterResourceId = resId,
                    .RunningResourceId = resId,
                    .QueueIdent = 0,
                    .AutoInstance = True,
                    .AuthorisationToken = token}
                Return owner.CreateRunner(runnerRequest)

            Catch ex As Exception
                Throw New OperationFailedException(ex.Message)

            End Try
        End SyncLock
    End Function

    ''' <summary>
    ''' Removes a runner record for a specified session ID from this list
    ''' </summary>
    ''' <param name="sessionId">The ID of the session whose runner record should
    ''' be removed.</param>
    ''' <param name="removedRecord">The runner record that was removed, if any
    ''' was actually removed. Otherwise null.</param>
    ''' <returns>True if a runner record for the given session ID was found and
    ''' removed from this list - ie. if this list was altered as a result of this
    ''' operation; False otherwise.</returns>
    Public Function Remove(
        ByVal sessionId As Guid, ByRef removedRecord As ListenerRunnerRecord) _
        As Boolean
        SyncLock mLock
            removedRecord = FindRunner(sessionId)
            If removedRecord IsNot Nothing Then Return Remove(removedRecord)
            Return False
        End SyncLock
    End Function

    ''' <summary>
    ''' Removes the first runner record whose session ID starts with a specified
    ''' partial ID.
    ''' </summary>
    ''' <param name="partialSessId">The partial ID to search for. Note that for
    ''' any alpha digits in the string, the search is case-insensitive. Also, any
    ''' dashes are ignored - only the actual GUID value is tested.</param>
    ''' <param name="removedRecord">The runner record that was removed, if any
    ''' was actually removed. Otherwise null.</param>
    ''' <returns>True if a runner record for the given session ID was found and
    ''' removed from this list - ie. if this list was altered as a result of this
    ''' operation; False otherwise.</returns>
    Public Function Remove(
        ByVal partialSessId As String, ByRef removedRecord As ListenerRunnerRecord) _
        As Boolean
        SyncLock mLock
            removedRecord = FindRunner(partialSessId)
            If removedRecord IsNot Nothing Then Return Remove(removedRecord)
            Return False
        End SyncLock
    End Function

    ''' <summary>
    ''' Gets the inner enumerator for this record list - this is the one that
    ''' is actually used in the <see cref="RunnerRecordEnumerator"/> class when
    ''' an enumerator is requested from this object.
    ''' </summary>
    ''' <returns>An enumerator over the list of records held by this object.
    ''' </returns>
    Private Function GetInnerEnumerator() As IEnumerator(Of ListenerRunnerRecord)
        Return mList.GetEnumerator()
    End Function

#End Region

#Region " IList Properties "

    ''' <summary>
    ''' Gets a count of the number of runner records held in this list
    ''' </summary>
    Public ReadOnly Property Count() As Integer _
        Implements ICollection(Of ListenerRunnerRecord).Count
        Get
            SyncLock mLock
                Return mList.Count
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Checks if this list is readonly. It is not.
    ''' </summary>
    ''' <returns>False</returns>
    Public ReadOnly Property IsReadOnly() As Boolean _
        Implements ICollection(Of ListenerRunnerRecord).IsReadOnly
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the runner record at the specified index of this list.
    ''' </summary>
    ''' <param name="index">The index at which the item should be set or
    ''' retrieved.</param>
    Default Public Property Item(ByVal index As Integer) As ListenerRunnerRecord _
        Implements IList(Of ListenerRunnerRecord).Item
        Get
            SyncLock mLock
                Return mList(index)
            End SyncLock
        End Get
        Set(ByVal value As ListenerRunnerRecord)
            SyncLock mLock
                mList(index) = value
            End SyncLock
        End Set
    End Property

#End Region

#Region " IList Methods "

    ''' <summary>
    ''' Adds the given runner record to this list
    ''' </summary>
    ''' <param name="item">The runner record to add to this list</param>
    Public Sub Add(ByVal item As ListenerRunnerRecord) _
        Implements ICollection(Of ListenerRunnerRecord).Add
        SyncLock mLock
            mList.Add(item)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Clears all of the runner records from this list
    ''' </summary>
    Public Sub Clear() Implements ICollection(Of ListenerRunnerRecord).Clear
        SyncLock mLock
            mList.Clear()
        End SyncLock
    End Sub

    ''' <summary>
    ''' Checks if this list contains the given runner record
    ''' </summary>
    ''' <param name="item">The runner record to check for</param>
    ''' <returns>True if the given runner record was found in this list.
    ''' </returns>
    Public Function Contains(ByVal item As ListenerRunnerRecord) As Boolean _
        Implements ICollection(Of ListenerRunnerRecord).Contains
        SyncLock mLock
            Return mList.Contains(item)
        End SyncLock
    End Function

    ''' <summary>
    ''' Copies this list of runner records to the given array at the specified
    ''' index.
    ''' </summary>
    ''' <param name="arr">The array to copy to</param>
    ''' <param name="index">The index to begin copying this list.</param>
    Public Sub CopyTo(ByVal arr() As ListenerRunnerRecord, ByVal index As Integer) _
        Implements ICollection(Of ListenerRunnerRecord).CopyTo
        SyncLock mLock
            mList.CopyTo(arr, index)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Removes the given runner record from this list
    ''' </summary>
    ''' <param name="item">The runner record to remove</param>
    ''' <returns>True if the runner record was removed from this list - ie. if
    ''' this list was altered as a result of this operation; False otherwise.
    ''' </returns>
    Public Function Remove(ByVal item As ListenerRunnerRecord) As Boolean _
        Implements ICollection(Of ListenerRunnerRecord).Remove
        SyncLock mLock
            Return mList.Remove(item)
        End SyncLock
    End Function

    ''' <summary>
    ''' Gets an enumerator over the runner records in this list.
    ''' </summary>
    ''' <returns>An enumerator over the list of runner records in this list.
    ''' </returns>
    Public Function GetEnumerator() As IEnumerator(Of ListenerRunnerRecord) _
        Implements IEnumerable(Of ListenerRunnerRecord).GetEnumerator
        SyncLock mLock
            Return New RunnerRecordEnumerator(Me)
        End SyncLock
    End Function

    ''' <summary>
    ''' Gets the first index of the specified runner record in this list, or -1
    ''' if the given record was not found.
    ''' </summary>
    ''' <param name="item">The item to search for in this list</param>
    ''' <returns>The first index of the given runner record in this list, or -1
    ''' if the given runner record was not found in this list.</returns>
    Public Function IndexOf(ByVal item As ListenerRunnerRecord) As Integer _
        Implements IList(Of ListenerRunnerRecord).IndexOf
        SyncLock mLock
            Return mList.IndexOf(item)
        End SyncLock
    End Function

    ''' <summary>
    ''' Inserts a runner record into this list.
    ''' </summary>
    ''' <param name="index">The index at which the record should be inserted.
    ''' </param>
    ''' <param name="item">The runner record item to insert</param>
    Public Sub Insert(ByVal index As Integer, ByVal item As ListenerRunnerRecord) _
        Implements IList(Of ListenerRunnerRecord).Insert
        SyncLock mLock
            mList.Insert(index, item)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Removes the runner record at a specified index
    ''' </summary>
    ''' <param name="index">The index at which the runner record should be
    ''' removed currently resides.</param>
    Public Sub RemoveAt(ByVal index As Integer) _
        Implements IList(Of ListenerRunnerRecord).RemoveAt
        SyncLock mLock
            mList.RemoveAt(index)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Non-generic implementation of the
    ''' <see cref="System.Collections.IEnumerable.GetEnumerator"/> method.
    ''' </summary>
    Private Function GetNonGenericEnumerator() As IEnumerator _
        Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

#End Region

End Class
