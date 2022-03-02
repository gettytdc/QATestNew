Imports System.Windows.Forms

''' <summary>
''' Delegate describing a method which can handle a global key event
''' </summary>
''' <param name="e">The args detailing the event</param>
Public Delegate Sub GlobalKeyEventHandler( _
 ByVal sender As Object, ByVal e As GlobalKeyEventArgs)

''' <summary>
''' Event Args object representing a Global Key Event
''' </summary>
Public Class GlobalKeyEventArgs : Inherits EventArgs

    ' The key that is being fired
    Private mKey As Keys

    ''' <summary>
    ''' Creates a new Global Key Event Args object for the given key
    ''' </summary>
    ''' <param name="code">The key code which this event represents</param>
    Public Sub New(ByVal code As Keys)
        mKey = code
    End Sub

    ''' <summary>
    ''' The Key code which the event has detected
    ''' </summary>
    Public ReadOnly Property Key() As Keys
        Get
            Return mKey
        End Get
    End Property

End Class