
Imports BluePrism.BPCoreLib

''' <summary>
''' Class providing some licensing-related utility methods.
''' </summary>
Public Class clsLicenseQueries

    ''' <summary>
    ''' Causes the latest license to be fetched from the database.
    ''' </summary>
    ''' <returns>Nothing if successful, otherwise an error message</returns>
    Public Shared Function RefreshLicense() As String
        Try
            If Not ServerFactory.ServerAvailable Then Return "No server connection available"
            Dim keys = gSv.GetLicenseInfo()
            Licensing.SetLicenseKeys(keys)
        Catch ex As Exception
            Return ex.ToString
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Causes the current license to be replaced with the default one (e.g.
    ''' when signing out of the client)
    ''' </summary>
    Public Shared Sub UnloadLicense()
        Try
            Licensing.RestoreDefaultLicense()
        Catch
            'Ignore any exceptions here
        End Try
    End Sub

End Class

