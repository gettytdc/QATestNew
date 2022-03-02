''' <summary>
''' Delegate used to describe a NameChanging event using the
''' <see cref="NameChangingEventArgs"/>
''' </summary>
''' <param name="sender">The object which is firing the name changing event.</param>
''' <param name="e">The args detailing the name changing event.</param>
Public Delegate Sub NameChangingEventHandler( _
 ByVal sender As Object, ByVal e As NameChangingEventArgs)

''' <summary>
''' Cancellable event args detailing a name change event
''' </summary>
Public Class NameChangingEventArgs : Inherits BaseNameChangeEventArgs

    ' Flag indicating if the name change should be cancelled
    Private mCancel As Boolean

    ' The reason provided for the name change being cancelled
    Private mCancelReason As String

    ''' <summary>
    ''' Creates a new NameChanging event args object.
    ''' </summary>
    ''' <param name="oldName">The name being changed from</param>
    ''' <param name="newName">The name being changed to</param>
    Public Sub New(ByVal oldName As String, ByVal newName As String)
        MyBase.New(oldName, newName)
    End Sub

    ''' <summary>
    ''' Gets or sets whether the name change event has been cancelled.
    ''' </summary>
    Public Property Cancel() As Boolean
        Get
            Return mCancel
        End Get
        Set(ByVal value As Boolean)
            mCancel = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the reason that this event has been cancelled.
    ''' If <see cref="Cancel"/> is not set, this is always null.
    ''' If no reason has been set, this will return <c>"No reason specified"</c>
    ''' </summary>
    Public Property CancelReason() As String
        Get
            If Not mCancel Then Return Nothing
            If mCancelReason = "" Then Return My.Resources.NameChangingEventArgs_NoReasonSpecified
            Return mCancelReason
        End Get
        Set(ByVal value As String)
            mCancelReason = value
        End Set
    End Property

End Class
