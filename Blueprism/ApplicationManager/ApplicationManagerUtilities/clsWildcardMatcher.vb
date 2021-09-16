
Imports System.Text.RegularExpressions
Imports System.Text
Imports BluePrism.BPCoreLib

''' Project  : ApplicationManagerUtilities
''' Class    : clsWildcardMatcher
''' <summary>
''' Provides wildcard matching of patterns (uses regex internally).
''' </summary>
Public Class clsWildcardMatcher

    ''' <summary>
    ''' Matches a wildcard pattern
    ''' </summary>
    ''' <param name="input">The input string to match</param>
    ''' <param name="pattern">the wildcard pattern</param>
    Public Shared Function IsMatch(input As String, pattern As String) As Boolean
        Return Regex.IsMatch(input, InterpretWildcard(pattern),
                             RegexOptions.IgnoreCase Or
                             RegexOptions.Singleline Or
                             RegexOptions.CultureInvariant)
    End Function

    ''' <summary>
    ''' Interprets the supplied wildcard pattern and converts it to a regex pattern.
    ''' </summary>
    ''' <param name="Pattern">The pattern to be  converted.</param>
    Private Shared Function InterpretWildcard(pattern As String) As String
        Dim regexPattern = New StringBuilder() _
            .Append("^") _
            .Append(Regex.Escape(pattern)) _
            .Replace("\*", ".*") _
            .Replace("\?", ".") _
            .Replace("\#", "\d") _
            .Append("$") _
            .ToString()

        Return Regex.Replace(regexPattern, "((?<=\.\*)\.\*|\.\*(?=\.\*))", "\*", RegexOptions.None, RegexTimeout.DefaultRegexTimeout)
    End Function

End Class
