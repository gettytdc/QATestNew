Imports BluePrism.AutomateProcessCore

Public Class ctlChooseWebServiceLogging

    Private WithEvents mInvestigator As clsWSDLProcess
    Public ReadOnly Property WebServices() As Dictionary(Of String, clsWebService)
        Get
            Return mWebServices
        End Get
    End Property
    Private mWebServices As Dictionary(Of String, clsWebService)

    Public Property ServiceDetails() As clsWebServiceDetails
        Get
            Return mServiceDetails
        End Get
        Set(ByVal value As clsWebServiceDetails)
            mServiceDetails = value
            If mServiceDetails IsNot Nothing Then
                ImportWebService()
            End If
        End Set
    End Property
    Private mServiceDetails As clsWebServiceDetails

    ''' <summary>
    ''' Populates the list of web services available at the url in the txtUrl
    ''' texbox.
    ''' 
    ''' The list is displayed in the Services listview.
    ''' </summary>
    Private Sub ImportWebService()
        Try
            mInvestigator = New clsWSDLProcess()
            NavigateNext = False
            txtLog.Clear()
            'Need to reset the Loaded flag on this in case we've come backwards,
            'otherwise it will not add any actions. (For some reason!?)
            ServiceDetails.Loaded = False
            mWebServices = mInvestigator.Import(mServiceDetails.URL, ServiceDetails)
            NavigateNext = True
        Catch ex As System.Security.Authentication.AuthenticationException
            UserMessage.Show(My.Resources.ctlChooseWebServiceLogging_TheCredentialsOrCertificateProvidedWereNotValid)
        Catch ex As NotImplementedException
            UserMessage.Show(String.Format(My.Resources.ctlChooseWebServiceLogging_ThereWasAProblemWithTheServiceThatYouAreTryingToImport0, ex.Message))
        Catch ex As System.Net.WebException
            'eg dns error
            UserMessage.Show(String.Format(My.Resources.ctlChooseWebServiceLogging_TheSpecifiedLocationCouldNotBeFound0, ex.Message))
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlChooseWebServiceLogging_CouldNotUnderstandTheWebServiceDescribedAt0PleaseMakeSureTheUrlYouProvidedRefer, ServiceDetails.URL) & vbCrLf & vbCrLf & ex.Message)
        End Try

        UpdateNavigate()
    End Sub

    Private Sub mInvestigator_Diagnostic(ByVal message As String) Handles mInvestigator.Diagnostic
        txtLog.AppendText(message & vbCrLf)
    End Sub
End Class
