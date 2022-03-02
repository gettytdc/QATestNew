''' <summary>
''' Event Arguments detailing a name change
''' </summary>
Public Class BaseNameChangeEventArgs : Inherits EventArgs

    ' The old name
    Private mOld As String

    ' The new name
    Private mNew As String

    ''' <summary>
    ''' Creates a new name change event args object.
    ''' </summary>
    ''' <param name="oldName">The name being changed from</param>
    ''' <param name="newName">The name being changed to</param>
    Public Sub New(ByVal oldName As String, ByVal newName As String)
        mOld = oldName
        mNew = newName
    End Sub

    ''' <summary>
    ''' The old name that is being changed from
    ''' </summary>
    Public ReadOnly Property OldName() As String
        Get
            Return mOld
        End Get
    End Property

    ''' <summary>
    ''' The new name that is being changed to
    ''' </summary>
    Public ReadOnly Property NewName() As String
        Get
            Return mNew
        End Get
    End Property

End Class
