Imports AutomateControls.Wizard
Imports BluePrism.AutomateProcessCore

Public Class clsWebServiceWizardController
    Inherits WizardController

    Private mWebServices As Dictionary(Of String, clsWebService)
    Private mCredentialsNeeded As Boolean

    Protected Overrides Sub OnNavigateBegin(ByVal sender As Object, ByVal e As CancelEventArgs)
        'In the navigate next pressing event handler we get relevant data from the current panel
        'i.e. the panel we are just about to leave.

        Dim location As ctlChooseWebServiceLocation = TryCast(CurrentPanel, ctlChooseWebServiceLocation)
        If location IsNot Nothing Then
            ServiceDetails.URL = location.txtUrl.Text
        End If
        Dim cred As ctlChooseWebServiceCredentials = TryCast(CurrentPanel, ctlChooseWebServiceCredentials)
        If cred IsNot Nothing Then
            Try
                mCredentialsNeeded = cred.chkNeedsUsernameAndPassword.Checked
                ServiceDetails.Username = cred.txtUsername.Text
                ServiceDetails.Secret = cred.txtPassword.SecurePassword
                ServiceDetails.Certificate = cred.GetCertificate()
            Catch ex As Exception
                UserMessage.ShowExceptionMessage(ex)
                e.Cancel = True
                MyBase.OnNavigateBegin(sender, e)
                Exit Sub
            End Try
        End If
        Dim log As ctlChooseWebServiceLogging = TryCast(CurrentPanel, ctlChooseWebServiceLogging)
        If log IsNot Nothing Then
            mWebServices = log.WebServices
        End If
        Dim service As ctlChooseWebService = TryCast(CurrentPanel, ctlChooseWebService)
        If service IsNot Nothing AndAlso service.lvServices.SelectedItems.Count > 0 Then
            ServiceDetails.ServiceToUse = service.lvServices.SelectedItems(0).Text
        End If
        Dim methods As ctlChooseWebServiceMethods = TryCast(CurrentPanel, ctlChooseWebServiceMethods)
        If methods IsNot Nothing Then
            GetWebServiceMethods(methods)
        End If
        Dim timeout As ctlChooseWebServiceTimeout = TryCast(CurrentPanel, ctlChooseWebServiceTimeout)
        If timeout IsNot Nothing Then
            ServiceDetails.Timeout = Integer.Parse(timeout.txtTimeout.Text)
        End If
        Dim name As ctlChooseWebServiceName = TryCast(CurrentPanel, ctlChooseWebServiceName)
        If name IsNot Nothing Then
            ServiceDetails.FriendlyName = name.ServiceName
        End If

        MyBase.OnNavigateBegin(sender, e)
    End Sub

    Public Overrides Sub StartWizard()
        MyBase.StartWizard()

        Dim location As ctlChooseWebServiceLocation = TryCast(CurrentPanel, ctlChooseWebServiceLocation)
        If location IsNot Nothing Then
            location.txtUrl.Text = ServiceDetails.URL
        End If
    End Sub

    Protected Overrides Sub OnNavigateNextEnd(ByVal sender As Object, ByVal e As System.EventArgs)
        'In the navigate next pressed event handler we setup the new panels data, i.e. the panel 
        'we are about to show.

        Dim cred As ctlChooseWebServiceCredentials = TryCast(CurrentPanel, ctlChooseWebServiceCredentials)
        If cred IsNot Nothing Then
            Dim username As String = ServiceDetails.Username
            If Not String.IsNullOrEmpty(username) Then
                cred.chkNeedsUsernameAndPassword.Checked = True
                cred.txtUsername.Text = username
                cred.txtPassword.SecurePassword = ServiceDetails.Secret
            End If
            If ServiceDetails.Certificate IsNot Nothing Then
                cred.chkNeedsClientSSL.Checked = True
                cred.HasCertificate = True
                cred.btnChange.Visible = True
            End If
        End If
        Dim log As ctlChooseWebServiceLogging = TryCast(CurrentPanel, ctlChooseWebServiceLogging)
        If log IsNot Nothing Then
            log.ServiceDetails = ServiceDetails
        End If
        Dim service As ctlChooseWebService = TryCast(CurrentPanel, ctlChooseWebService)
        If service IsNot Nothing Then
            PopulateListOfAvailableServices(service)
        End If
        Dim methods As ctlChooseWebServiceMethods = TryCast(CurrentPanel, ctlChooseWebServiceMethods)
        If methods IsNot Nothing Then
            PopulateListOfAvailableMethods(methods)
        End If
        Dim timeout As ctlChooseWebServiceTimeout = TryCast(CurrentPanel, ctlChooseWebServiceTimeout)
        If timeout IsNot Nothing Then
            timeout.txtTimeout.Text = ServiceDetails.Timeout.ToString
        End If
        Dim name As ctlChooseWebServiceName = TryCast(CurrentPanel, ctlChooseWebServiceName)
        If name IsNot Nothing Then

            name.Setup(ServiceDetails)
        End If

        MyBase.OnNavigateNextEnd(sender, e)
    End Sub

    Protected Overrides Sub OnNavigateFinish(ByVal sender As Object, ByVal e As System.EventArgs)
        If Not mCredentialsNeeded Then
            ServiceDetails.Username = Nothing
            ServiceDetails.Secret = Nothing
        End If
        Dialog.DialogResult = DialogResult.OK
        MyBase.OnNavigateFinish(sender, e)
    End Sub

    Private ReadOnly Property Dialog() As frmWebServices
        Get
            Return CType(m_WizardDialog, frmWebServices)
        End Get
    End Property

    Private ReadOnly Property ServiceDetails() As clsWebServiceDetails
        Get
            Return Dialog.ServiceDetails
        End Get
    End Property

    Private Sub GetWebServiceMethods(ByVal methods As ctlChooseWebServiceMethods)
        Dim details As clsWebServiceDetails = ServiceDetails
        details.Actions.Clear()
        For Each item As ListViewItem In methods.lvMethods.Items
            details.Actions.Add(item.Text, item.Checked)
        Next
    End Sub

    Private Sub PopulateListOfAvailableServices(ByVal services As ctlChooseWebService)
        services.lvServices.BeginUpdate()
        services.lvServices.Items.Clear()
        Dim sServiceToUse As String = ServiceDetails.ServiceToUse
        For Each svc As String In mWebServices.Keys
            Dim item As ListViewItem = services.lvServices.Items.Add(svc)
            If svc = sServiceToUse Then
                item.Selected = True
            End If
            item.SubItems.Add(mWebServices(svc).Narrative)
        Next
        services.lvServices.Select()
        services.lvServices.EndUpdate()
    End Sub

    ''' <summary>
    ''' Takes the first selected row in the Services listview and creates a list 
    ''' of available methods provided by this service. 
    ''' 
    ''' The list is displayed in the methods listview.
    ''' </summary>
    Private Sub PopulateListOfAvailableMethods(ByVal methods As ctlChooseWebServiceMethods)
        Try

            Dim webService As clsWebService = mWebServices(ServiceDetails.ServiceToUse)
            Dim actions As IList(Of clsBusinessObjectAction) = webService.GetActions()

            methods.lvMethods.Items.Clear()
            If actions IsNot Nothing AndAlso actions.Count > 0 Then

                Try
                    'prepare listview to be populated
                    methods.lvMethods.BeginUpdate()
                    Dim details As clsWebServiceDetails = ServiceDetails
                    'fill with list of methods from web service object
                    For Each act As clsWebServiceAction In actions
                        Dim name As String = act.GetName()
                        If Not details.Actions.ContainsKey(name) Then
                            details.Actions.Add(name, True)
                        End If
                        Dim imgIndex As Integer = 0
                        Dim item As ListViewItem = methods.lvMethods.Items.Add(name, imgIndex)
                        item.SubItems.Add(act.GetNarrative())
                        item.Checked = details.ActionEnabled(name)
                    Next

                Catch ex As Exception
                    UserMessage.Show(My.Resources.clsWebServiceWizardController_UnexpectedErrorFailedToPopulateListOfAvailableMethods)
                Finally
                    methods.lvMethods.EndUpdate()
                End Try
            Else
                UserMessage.Show(My.Resources.clsWebServiceWizardController_ThereAreNoMethodsAvailableToBeListed)
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.clsWebServiceWizardController_UnexpectedError0ItAppearsThatTheOperationsProvidedByThisWebServiceAreNotSupport, ex.Message))
        End Try
    End Sub
End Class
