''' <summary>
''' A list of validation errors
''' </summary>
Public Class ValidationErrorList : Inherits List(Of ValidateProcessResult)

    ''' <summary>
    ''' Adds a validation result to this list.
    ''' </summary>
    ''' <param name="stg">The stage that the result is associated with</param>
    ''' <param name="checkId">The check ID for the type of validation error found.
    ''' </param>
    ''' <param name="args">The arguments for the validation message.</param>
    Public Overloads Sub Add(ByVal stg As clsProcessStage, _
     ByVal checkId As Integer, ByVal ParamArray args() As Object)
        MyBase.Add(New ValidateProcessResult(stg, checkId, args))
    End Sub

    ''' <summary>
    ''' Adds a validation result to this list.
    ''' </summary>
    ''' <param name="stg">The stage that the result is associated with</param>
    ''' <param name="helpRef">The reference within the help to register with the
    ''' validation error.</param>
    ''' <param name="checkId">The check ID for the type of validation error found.
    ''' </param>
    ''' <param name="args">The arguments for the validation message.</param>
    Public Overloads Sub Add(ByVal stg As clsProcessStage, _
     ByVal helpRef As String, ByVal checkId As Integer, _
     ByVal ParamArray args() As Object)
        MyBase.Add(New ValidateProcessResult(stg, helpRef, checkId, args))
    End Sub

End Class
