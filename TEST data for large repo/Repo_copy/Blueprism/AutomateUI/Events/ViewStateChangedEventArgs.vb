
''' <summary>
''' The delegate to use to handle view state changed events
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="e">The event args detailing the event</param>
Public Delegate Sub ViewStateChangedEventHandler(
    sender As Object, e As ViewStateChangedEventArgs)

''' <summary>
''' Event arguments detailing the stage committed event.
''' </summary>
Public Class ViewStateChangedEventArgs : Inherits EventArgs

    ' The view state that has been changed to
    Private mViewState As String

    ''' <summary>
    ''' Creates a new view state changed event args object
    ''' </summary>
    ''' <param name="newState">The encoded view state after the change has occurred.
    ''' </param>
    Public Sub New(ByVal newState As String)
        mViewState = newState
    End Sub

    ''' <summary>
    ''' The encoded view state after the view state change has occurred.
    ''' </summary>
    Public ReadOnly Property ViewState As String
        Get
            Return mViewState
        End Get
    End Property

End Class