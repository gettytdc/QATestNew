Public Class clsUserSelectedEventArgs : Inherits EventArgs
    Public Property UsersSelected As Boolean
    Public Property DeletedUser As Object
End Class

Public Delegate Sub UserSelectedEventHandler(sender As Object, args As clsUserSelectedEventArgs)
