Imports AutomateControls
Imports AutomateControls.Wizard
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Utility

Public Class frmWebServices
    Implements IEnvironmentColourManager

    Private mController As WizardController
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mController = New clsWebServiceWizardController()
        mController.SetDialog(Me)

        Dim loc As New ctlChooseWebServiceLocation
        mController.AddPanel(loc)

        Dim cred As New ctlChooseWebServiceCredentials
        mController.AddPanel(cred)

        Dim time As New ctlChooseWebServiceTimeout
        mController.AddPanel(time)

        Dim log As New ctlChooseWebServiceLogging
        mController.AddPanel(log)

        Dim serv As New ctlChooseWebService
        mController.AddPanel(serv)

        Dim method As New ctlChooseWebServiceMethods
        mController.AddPanel(method)

        Dim name As New ctlChooseWebServiceName
        mController.AddPanel(name)
    End Sub

    Public Sub Setup(ByVal ID As Guid)
        If ID <> Guid.Empty Then
            Text = My.Resources.frmWebServices_UpdateAWebService
            mDetails = gSv.GetWebServiceDefinition(ID)
        Else
            mDetails = New clsWebServiceDetails
            mDetails.URL = "http://"
            mDetails.Timeout = 10000
        End If

        'Guid.Empty to signifies 'add new service'
        mDetails.Id = ID

        mController.StartWizard()
    End Sub

    Public Property ServiceDetails() As clsWebServiceDetails
        Get
            Return mDetails
        End Get
        Set(ByVal value As clsWebServiceDetails)
            mDetails = value
        End Set
    End Property
    Private mDetails As clsWebServiceDetails

    Public Overrides Function GetHelpFile() As String
        Return "frmWebServices.htm"
    End Function

    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return Bluebar.BackColor
        End Get
        Set(value As Color)
            Bluebar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return Bluebar.TitleColor
        End Get
        Set(value As Color)
            Bluebar.TitleColor = value
        End Set
    End Property
End Class
