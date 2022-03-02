#If UNITTESTS Then
' Addition of a helper method to the clsFunctions class - actually just a way of
' exposing the functions to NUnit without exposing them unnecessarily within the
' main code.
Partial Public Class clsFunctions
    Public Shared Function NUnit_GetApcFunctions() As ICollection(Of clsFunction)
        Return GenerateFunctions(Nothing)
    End Function
End Class
#End If
