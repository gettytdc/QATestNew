Public Class PropertyChangedWithValuesEventArgs
    Inherits PropertyChangedEventArgs

    Public Property OldValue As String
    Public Property NewValue As String

    Public Sub New(propertyName As String, oldValue As String, newValue As String)
        MyBase.New(propertyName)
        Me.OldValue = oldValue
        Me.NewValue = newValue
    End Sub
End Class

Public Delegate Sub PropertyChangedWithValuesEventHandler(sender As Object, e As PropertyChangedWithValuesEventArgs)

