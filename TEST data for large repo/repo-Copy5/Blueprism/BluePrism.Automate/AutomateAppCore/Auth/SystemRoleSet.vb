Imports System.Collections.Concurrent
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.Server.Domain.Models

Namespace Auth

    ''' <summary>
    ''' A specialised roleset which is not directly modifiable.
    ''' The current system roleset can be retrieved or set using the static
    ''' <see cref="SystemRoleSet.Current"/> property.
    ''' </summary>
    Public Class SystemRoleSet : Inherits RoleSet

#Region " Class-scope declarations "

        ' The currently set roleset
        Private Shared mCurr As RoleSet

        ''' <summary>
        ''' The current roleset in the system
        ''' </summary>
        Public Shared ReadOnly Property Current() As RoleSet
            Get
                Return SystemCurrent
            End Get
        End Property

        ''' <summary>
        ''' Gets the current set of roles, populating then from the server if they
        ''' are not already set.
        ''' </summary>
        Public Shared ReadOnly Property SystemCurrent() As SystemRoleSet
            Get
                Try
                    If mCurr Is Nothing Then Init()
                Catch ex As Exception
                    Debug.Fail("Failed to retrieve roles: " & ex.ToString())
                End Try
                Return DirectCast(mCurr, SystemRoleSet)
            End Get
        End Property

        ''' <summary>
        ''' Initialises the system roles held in this class.
        ''' </summary>
        Private Shared Sub Init()
            mCurr = New SystemRoleSet(gSv.GetRoles())
        End Sub

        ''' <summary>
        ''' Resets the current system roleset, ensuring that a brand new system role
        ''' set is retrieved from the server when it is required.
        ''' </summary>
        ''' <remarks>Note that use of this method effectively removes the system
        ''' roleset, meaning that anything listening for changes on the
        ''' <see cref="Current"/> system roleset will no longer receive events.
        ''' </remarks>
        Friend Shared Sub Reset()
            Reset(False)
        End Sub

        ''' <summary>
        ''' Resets the current system roleset, ensuring that a brand new system role
        ''' set is retrieved from the server when it is required.
        ''' </summary>
        ''' <param name="loadImmediately">True to reload the roles from the database
        ''' immediately; False to wait and load them lazily when next requested.
        ''' </param>
        ''' <remarks>Note that use of this method effectively removes the system
        ''' roleset, meaning that anything listening for changes on the
        ''' <see cref="Current"/> system roleset will no longer receive events.
        ''' </remarks>
        Friend Shared Sub Reset(ByVal loadImmediately As Boolean)
            Dim curr = DirectCast(mCurr, SystemRoleSet)
            If curr IsNot Nothing Then curr.Dispose()
            mCurr = Nothing
            If loadImmediately Then Init()
        End Sub

#End Region

#Region " Member Variables "

        ' A data monitor which keeps track of when the role data has updated
        Private WithEvents mMonitor As IDataMonitor

        Private Shared mDataMonitorFactory As Func(Of IDataMonitor) = 
            Function() New TimerDataMonitor(New DatabaseMonitoredDataStore()) With {
                                                    .Interval = TimeSpan.FromSeconds(10),
                                                    .Enabled = True}

        ' A lock used to ensure that only one thread attempts to update the
        ' roles at any one time
        Private mUpdateLock As New Object()

#End Region

#Region " Published Events "

        ''' <summary>
        ''' Event fired when the system role set is updated.
        ''' Any subscribers to this event must ensure that they unsubscribe when they
        ''' are finished with them; otherwise, this event will maintain a reference
        ''' to the subscriber object, with the potential for the resultant memory
        ''' leaks.
        ''' </summary>
        ''' <remarks>If the system roleset is reset (eg. on logout), then event
        ''' listeners will receive no further events, even if a login causes a new
        ''' system roleset to be created. That will be a separate object and thus
        ''' will not have any listeners registered to it at create time.</remarks>
        Public Event Updated(ByVal sender As Object, ByVal e As EventArgs)

#End Region

#Region " Properties "

        ''' <summary>
        ''' Checks if this role set is readonly. It is.
        ''' </summary>
        Public Overrides ReadOnly Property IsReadOnly() As Boolean
            Get
                Return True
            End Get
        End Property

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new SystemRoleSet populated from the given role set.
        ''' </summary>
        ''' <param name="roles">The roles from which to draw the roles in this
        ''' SystemRoleSet; this will actually clone the roles rather than using the
        ''' actual role objects in the provided set.</param>
        Private Sub New(ByVal roles As RoleSet)
            UpdateTo(roles)
            mMonitor = mDataMonitorFactory()
        End Sub
                

#End Region

#Region " Mutator Overrides "

        ''' <summary>
        ''' Cannot use - this object is not modifiable
        ''' </summary>
        ''' <param name="r">Ignored</param>
        ''' <returns>Ignored</returns>
        ''' <exception cref="NotModifiableException">When called</exception>
        Public Overrides Function Add(ByVal r As Role) As Boolean
            Throw New NotModifiableException()
        End Function

        ''' <summary>
        ''' Cannot use - this object is not modifiable
        ''' </summary>
        ''' <param name="rolename">Ignored</param>
        ''' <returns>Ignored</returns>
        ''' <exception cref="NotModifiableException">When called</exception>
        Public Overrides Function Add(ByVal rolename As String) As Boolean
            Throw New NotModifiableException()
        End Function

        ''' <summary>
        ''' Cannot use - this object is not modifiable
        ''' </summary>
        ''' <returns>Ignored</returns>
        ''' <exception cref="NotModifiableException">When called</exception>
        Public Overrides Function NewRole() As Role
            Throw New NotModifiableException()
        End Function

        ''' <summary>
        ''' Cannot use - this object is not modifiable
        ''' </summary>
        ''' <param name="r">Ignored</param>
        ''' <returns>Ignored</returns>
        ''' <exception cref="NotModifiableException">When called</exception>
        Public Overrides Function Remove(ByVal r As Role) As Boolean
            Throw New NotModifiableException()
        End Function

        ''' <summary>
        ''' Cannot use - this object is not modifiable
        ''' </summary>
        ''' <param name="roleName">Ignored</param>
        ''' <returns>Ignored</returns>
        ''' <exception cref="NotModifiableException">When called</exception>
        Public Overrides Function Remove(ByVal roleName As String) As Boolean
            Throw New NotModifiableException()
        End Function

        ''' <summary>
        ''' Cannot use - this object is not modifiable
        ''' </summary>
        ''' <exception cref="NotModifiableException">When called</exception>
        Public Overrides Sub Clear()
            Throw New NotModifiableException()
        End Sub

        ''' <summary>
        ''' Cannot use - this object is not modifiable
        ''' </summary>
        ''' <param name="adminGroup">Ignored</param>
        ''' <param name="resetOthers">Ignored</param>
        ''' <exception cref="NotModifiableException">When called</exception>
        Public Overrides Sub SetSysAdminADGroup(adminGroup As String, resetOthers As Boolean)
            Throw New NotModifiableException()
        End Sub

#End Region

#Region " Other Methods "

        ''' <summary>
        ''' Disposes of this system roleset.
        ''' Note that this class does not publicise its Dispose method, or implement
        ''' the IDisposable interface - it's available to all contexts, and we don't
        ''' want any other context to be able to dispose of the system roleset.
        ''' It can only be disposed of by way of the <see cref="Reset"/> method,
        ''' which is the only place which needs to be concerned with cleaning up
        ''' this object.
        ''' </summary>
        Private Sub Dispose()
            Dim mon = mMonitor
            If mon IsNot Nothing Then mon.Dispose() : mon = Nothing
        End Sub

        ''' <summary>
        ''' Polls the database to see if the data has changed.
        ''' This is largely unnecessary unless a 'force check' is needed - eg. if the
        ''' roles have just been updated and the new ones need to be pulled from the
        ''' database.
        ''' </summary>
        Public Sub Poll()
            mMonitor.Poll()
        End Sub

        ''' <summary>
        ''' Creates the map to be used for this role set.
        ''' </summary>
        ''' <returns>An empty <see cref="ConcurrentDictionary(Of String,Role)"/> to
        ''' be used to hold the roles in this role set. This differs from the base
        ''' class, ensuring that multithreaded access to the dictionary will not
        ''' cause it to enter an invalid state.</returns>
        Protected Overrides Function CreateMap() As IDictionary(Of String, Role)
            Return New ConcurrentDictionary(Of String, Role)
        End Function

        ''' <summary>
        ''' Handles an indication that the role data has been updated by reloading
        ''' the system role set from the database.
        ''' </summary>
        Private Sub HandleRolesDataUpdated(
         sender As Object, e As MonitoredDataUpdateEventArgs) _
         Handles mMonitor.MonitoredDataUpdated
            If e.Name <> DataNames.Roles Then Return
            UpdateTo(gSv.GetRoles())
        End Sub

        ''' <summary>
        ''' Gets a modifiable copy of this roleset. No changes made to the returned
        ''' set are reflected in this object.
        ''' </summary>
        ''' <returns>A modifiable deep clone of this object.</returns>
        Public Function ModifiableCopy() As RoleSet
            Return Copy()
        End Function

        ''' <summary>
        ''' Raises the <see cref="Updated"/> event on this system roleset.
        ''' </summary>
        Protected Overridable Sub OnUpdated(ByVal e As EventArgs)
            RaiseEvent Updated(Me, e)
        End Sub

        ''' <summary>
        ''' Updates this system roleset to match the given roleset.
        ''' </summary>
        ''' <param name="rs">The roleset to take the roles from; Note that a copy of
        ''' the given roles are used in this system role set rather than the actual
        ''' objects given.</param>
        ''' <exception cref="NotModifiableException">If a role in this SystemRoleSet is
        ''' not modifyable</exception>
        ''' <exception cref="NotDeletableException">If a role in this SystemRoleSet is
        ''' not deletable</exception>
        Private Sub UpdateTo(ByVal rs As RoleSet)
            If rs Is Me Then Return ' Ignore it if it's the same...

            SyncLock mUpdateLock
                ' Check that all of the 'essential' roles in the current roleset
                ' are still the same in the given roleset
                For Each r As Role In Me
                    If Not r.CanDelete AndAlso Not rs.Contains(r.Id) Then
                        Throw New NotDeletableException(
                         "The '{0}' role cannot be deleted", _
                         r.Name)
                    End If

                    If Not r.CanRename AndAlso
                        rs.Contains(r.Id) AndAlso Not rs.Contains(r.Name) Then
                        Throw New NotModifiableException(
                         "The '{0}' role cannot be renamed", _
                         r.Name)
                    End If

                    ' The following two checks expect that the given rolset
                    ' contains the id
                    If Not rs.Contains(r.Id) Then Continue For

                    If Not r.CanChangeActiveDirectoryGroup AndAlso
                        rs(r.Id).ActiveDirectoryGroup <> r.ActiveDirectoryGroup Then
                        Throw New NotModifiableException(
                         "The '{0}' role cannot have its AD group modified", _
                         r.Name)
                    End If

                    If Not r.CanChangePermissions AndAlso
                        Not rs(r.Id).EqualsApartFromADGroup(r) Then
                        Throw New NotModifiableException( _
                         "The '{0}' role cannot have its permissions modified", _
                         r.Name)
                    End If

                Next
                RoleMap.Clear()
                For Each r As Role In rs
                    RoleMap(r.Name) = r.CloneRole()
                Next
            End SyncLock

            OnUpdated(EventArgs.Empty)

        End Sub

#End Region

    End Class

End Namespace
