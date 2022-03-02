<Serializable()>
Public Class ResourceLimitExceededException
    Inherits Exception
    
    ''' <param name="val">The number of online resources that triggered this cap</param>
    Public Sub New(msg As String, val As Integer)
        MyBase.New(msg)
        ResourcesCounted = val
    End Sub
    
    Public ReadOnly Property ResourcesCounted As Integer
End Class
