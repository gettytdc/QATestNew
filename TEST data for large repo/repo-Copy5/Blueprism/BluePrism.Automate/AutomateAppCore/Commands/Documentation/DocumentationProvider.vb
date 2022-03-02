Namespace Commands.Documentation
    Public Class DocumentationProvider
        Implements IDocumentationProvider

        Private ReadOnly mCommandFactory As ICommandFactory

        Public Sub New(commandFactory As ICommandFactory)
            mCommandFactory = commandFactory
        End Sub

        ''' <summary>
        ''' Get command documentation in Wiki format.
        ''' </summary>
        ''' <returns>A String containing the documentation.</returns>
        Public Function GetCommandWikiDocs() As String Implements IDocumentationProvider.GetCommandWikiDocs
            Dim sb As New StringBuilder()
            sb.
                Append(My.Resources.DocumentationProvider_WikiCommandReference & vbCrLf).
                Append(My.Resources.DocumentationProvider_ForEachCommandTheAuthorisationLevelRequiredToBeAbleToUseTheCommandIsShown).
                Append(My.Resources.DocumentationProvider_ThisCanBeOneOfTheFollowing & vbCrLf).
                Append(My.Resources.DocumentationProvider_AnyOpenToAnyConnection & vbCrLf).
                Append(My.Resources.DocumentationProvider_AuthedCanOnlyBeUsedByAuthenticatedUsers & vbCrLf).
                Append(My.Resources.DocumentationProvider_AuthedOrLocalCanBeUsedByAuthenticatedUsersORConnectionsFromTheLocalMachine & vbCrLf).
                Append(My.Resources.DocumentationProvider_PermissionsUserMustBeAuthenticatedANDHavePermissionsForThisResource & vbCrLf)

            For Each commandKey As String In mCommandFactory.Commands.Keys
                Dim command = mCommandFactory.Commands.Item(commandKey)
                sb.
                    Append("===" & command.Name & "===" & vbCrLf).
                    Append(My.Resources.DocumentationProvider_WikiAuthorisation & command.CommandAuthenticationRequired.ToString() & vbCrLf & vbCrLf).
                    Append(command.Help & vbCrLf)
            Next
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Get command documentation in HTML format.
        ''' </summary>
        ''' <returns>A String containing the documentation.</returns>
        Public Function GetCommandHTMLDocs() As String Implements IDocumentationProvider.GetCommandHTMLDocs
            Dim sb As New StringBuilder()
            sb.Append("<h2>" & My.Resources.DocumentationProvider_CommandReference & "</h2>")
            For Each commandKey As String In mCommandFactory.Commands.Keys
                Dim command = mCommandFactory.Commands.Item(commandKey)
                sb.Append("<h4 id=""" & command.Name & """>" & command.Name & "</h4>")
                sb.Append("<p><strong>" & My.Resources.DocumentationProvider_Authorisation & "</strong> " & command.CommandAuthenticationRequired.ToString() & "</p>")
                Dim pre = False
                For Each line As String In command.Help.Split(Chr(10))
                    While line.EndsWith(vbCr)
                        line = line.Substring(0, line.Length() - 1)
                    End While
                    If line.Length > 0 Then
                        line = line.Replace("<", "&lt;")
                        line = line.Replace(">", "&gt;")
                        If line.StartsWith(" ") Then
                            If Not pre Then
                                sb.Append("<pre>" & vbCrLf)
                                pre = True
                            End If
                            sb.Append(line.Substring(1) & vbCrLf)
                        Else
                            If pre Then
                                sb.Append("</pre>")
                                pre = False
                            End If
                            sb.Append("<p>" & line & "</p>")
                        End If
                    End If
                Next
                If pre Then
                    sb.Append("</pre>")
                End If
            Next
            sb.Append(vbCrLf)
            Return sb.ToString()
        End Function

    End Class
End NameSpace
