Public Class clsStageTypeName
    ''' <summary>
    ''' Gets the localized friendly name For this data group according To the current culture.
    ''' </summary>
    ''' <param name="type">The stage type</param>
    ''' <returns>The name of the stage type for the current culture</returns>
    Public Shared Function GetLocalizedFriendlyName(type As String, Optional ByVal bPlural As Boolean = False) As String
        Dim result As String
        If (bPlural) Then
            result = My.Resources.Resources.ResourceManager.GetString($"StageTypes_multiple_{type}")
        Else
            result = My.Resources.Resources.ResourceManager.GetString($"StageTypes_singular_{type}")
        End If
        If (String.IsNullOrEmpty(result)) Then
            Return type
        Else
            Return result
        End If
    End Function
End Class
