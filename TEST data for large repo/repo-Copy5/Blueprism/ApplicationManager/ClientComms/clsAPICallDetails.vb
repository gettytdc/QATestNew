Imports System.Text.RegularExpressions
Imports System.Collections.Generic

Public Class clsAPICallDetails

    ''' <summary>
    ''' Indicates whether this instance contains a valid API call. "Invalid" calls include
    ''' such things as informational and debug messages, and are still queued for display
    ''' and logging purposes.
    ''' </summary>
    Public ReadOnly Property Valid() As Boolean
        Get
            Return mbValid
        End Get
    End Property
    Private mbValid As Boolean

    Public ReadOnly Property APIFunction() As String
        Get
            Return msAPIFunction
        End Get
    End Property
    Private msAPIFunction As String

    Public ReadOnly Property Parameters() As New Dictionary(Of String, String)

    Public ReadOnly Property SourceLine() As String
        Get
            Return msSourceLine
        End Get
    End Property
    Private msSourceLine As String

    ''' <summary>
    ''' Parse a line of text received from BPInjAgent.
    ''' </summary>
    ''' <param name="sText">The received line</param>
    ''' <returns>A populated clsAPICallDetails instance.</returns>
    Public Shared Function Parse(ByVal sText As String) As clsAPICallDetails

        Dim oRes As New clsAPICallDetails()
        oRes.msSourceLine = sText
        oRes.mbValid = False

        'Ignore debug messages...
        If sText.StartsWith("DEBUG:") Then Return oRes
        'And ignore general status messages...
        If sText.StartsWith("BPInjAgent:") Then Return oRes
        'And ignore anything to do with command responses:
        If sText.StartsWith("RESPONSE:") OrElse sText.StartsWith("FAILURE:") Then
            Return oRes
        End If

        Dim i = sText.IndexOf("("c)
        If i = -1 Then Return oRes

        oRes.msAPIFunction = sText.Substring(0, i)
        sText = sText.Substring(i + 1)

        If Not sText.EndsWith(")") Then Return oRes
        sText = sText.Substring(0, sText.Length - 1)
        For Each p In Split(sText, ",")
            Dim aX = Split(p, "=")
            Dim name = aX(0)
            Dim value = aX(1)
            If value.StartsWith("""") AndAlso value.EndsWith("""") Then
                value = value.Substring(1, value.Length - 2)
                value = Unescape(value)
            End If
            oRes.Parameters.Add(name, value)
        Next
        oRes.mbValid = True
        Return oRes
    End Function

    Private Shared Function Unescape(text As String) As String
        If text Is Nothing Then Return Nothing
        Return Regex.Replace(text, "\\[\\rnec]",
            Function(m)
                Select Case m.Value
                    Case "\\"
                        Return "\"
                    Case "\r"
                        Return vbCr
                    Case "\n"
                        Return vbLf
                    Case "\e"
                        Return "="
                    Case "\c"
                        Return ","
                    Case Else
                        Return m.Value
                End Select
            End Function)
    End Function

End Class
