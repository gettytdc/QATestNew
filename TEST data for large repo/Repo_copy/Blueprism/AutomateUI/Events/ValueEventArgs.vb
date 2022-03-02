''' <summary>
''' EventArgs that holds a given value.
''' </summary>
''' <typeparam name="T">The type of value that this EventArgs will hold</typeparam>
Public Class ValueEventArgs(Of T)
    Inherits EventArgs

    Public ReadOnly Property Value As T

    Public Sub New(val As T)
        Value = val
    End Sub
End Class
