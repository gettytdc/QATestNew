Imports System.Globalization

Namespace Wizards.SystemManager.DataGateways.Helpers

    Public Module OutputTypeResourceHelper

        Public Function GetLocalizedFriendlyNameToLower(outputId As string) As String
            Dim currentCulture = CultureInfo.CurrentUICulture.Name.ToUpper
            Dim outputName As String = GetLocalizedFriendlyName(outputId)
            If currentCulture = "DE-DE" Then Return outputName
            Return outputName.ToLower
        End Function

        Public Function GetLocalizedFriendlyName(id As String) As String
            Select Case Id
                Case "database"
                    Return My.Resources.ctlChooseOutputType_Database
                Case "splunk"
                    Return My.Resources.ctlChooseOutputType_Splunk
                Case "http"
                    Return My.Resources.ctlChooseOutputType_HTTP
                Case Else
                    Return My.Resources.ctlChooseOutputType_File
            End Select
        End Function

    End Module

End Namespace

