Imports System.ComponentModel

''' <summary>
''' Event args detailing a name changing event which can be validated and, if
''' necessary, cancelled
''' </summary>
Public Class NameValidatingEventArgs : Inherits CancelEventArgs
    ' The name being validated
    Private mName As String

    ''' <summary>
    ''' Creates a new event args object for detailing a name changing
    ''' </summary>
    ''' <param name="name">The proposed name being changed to</param>
    Public Sub New(ByVal name As String)
        mName = name
    End Sub

    ''' <summary>
    ''' The name which is being proposed in this event.
    ''' </summary>
    Public ReadOnly Property Name() As String
        Get
            Return mName
        End Get
    End Property

End Class
