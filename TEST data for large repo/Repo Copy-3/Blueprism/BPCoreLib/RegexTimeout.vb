Public Module RegexTimeout
    Public ReadOnly DefaultRegexTimeout As TimeSpan = New TimeSpan(0, 0, 10)

    Public Sub SetDefaultRegexTimeout()
        AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", DefaultRegexTimeout)
    End Sub
End Module
