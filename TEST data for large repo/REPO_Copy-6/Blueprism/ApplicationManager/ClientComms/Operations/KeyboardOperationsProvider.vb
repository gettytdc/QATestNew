Imports System.Text.RegularExpressions
Imports System.Threading
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Namespace Operations

    ''' <inheritDoc/>
    Public Class KeyboardOperationsProvider : Implements IKeyboardOperationsProvider


        Public Sub SendKeys(keys As String, interval As TimeSpan) Implements IKeyboardOperationsProvider.SendKeys

            ' If we don't have an interval, we can just send what we've got and return
            If interval <= Nothing Then
                System.Windows.Forms.SendKeys.SendWait(keys)
                Return
            End If

            ' If we have any interval at all, we can't support control chars
            If Regex.IsMatch(keys, "[~{}+^%]", RegexOptions.None, RegexTimeout.DefaultRegexTimeout) Then Throw New InvalidValueException(
                My.Resources.SpecialCharactersAreNotSupportedInSendKeysIfAnIntervalValueIsProvidedSeparateCa)

            For Each c As Char In keys
                System.Windows.Forms.SendKeys.SendWait(CStr(c))
                Thread.Sleep(interval)
            Next
        End Sub


    End Class
End Namespace
