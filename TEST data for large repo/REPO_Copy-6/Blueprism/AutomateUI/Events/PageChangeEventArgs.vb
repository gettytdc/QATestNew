''' <summary>
''' Class to hold details of a page change within the Application form
''' </summary>
Public Class PageChangeEventArgs
    Inherits CancelEventArgs

    ' The current control - ie. that which is being navigated away from
    Private mCurr As Control

    ' The type of new control - it may not exist as an instance yet
    Private mNew As Type

    ''' <summary>
    ''' The current control which is being changed from. 
    ''' </summary>
    Public ReadOnly Property Current() As Control
        Get
            Return DirectCast(mCurr, Control)
        End Get
    End Property

    ''' <summary>
    ''' The new type of page which is being navigated to.
    ''' </summary>
    Public ReadOnly Property NewType() As Type
        Get
            Return mNew
        End Get
    End Property

    ''' <summary>
    ''' Creates a new event arguments object for the PageChanging event.
    ''' </summary>
    ''' <param name="current">The current control which is being held in the
    ''' Application form and which is being replaced by the referenced event.</param>
    ''' <param name="newType">The new type of control which is going to be shown in
    ''' the application form.</param>
    Public Sub New(ByVal current As Control, ByVal newType As Type)
        mCurr = current
        mNew = newType
    End Sub

End Class
