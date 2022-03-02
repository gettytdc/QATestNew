Imports AutomateControls
Imports BluePrism.AutomateAppCore.Utility
Imports AutomateControls.Wizard

Public Class frmWebExpose
    Inherits StandardWizard
    Implements IEnvironmentColourManager

    Private mController As clsWebExposeWizardController
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Friend Sub Setup(ByVal mode As ProcessType)
        Dim modeName As String
        modeName = mode.ModeString
        Me.Text = String.Format(My.Resources.frmWebExpose_Expose0, modeName)

        mController = New clsWebExposeWizardController
        mController.SetDialog(Me)

        Dim pr As New ctlChooseWebExposeProcess
        pr.Setup(mode)
        mController.AddPanel(pr)

        Dim nm As New ctlChooseWebExposeName
        nm.Setup(mode)
        mController.AddPanel(nm)

        mController.StartWizard()
    End Sub

    Public ReadOnly Property Details() As clsWebExposeWizardController.ExposeDetails
        Get
            Return mController.Details
        End Get
    End Property

    Public Overrides Function GetHelpFile() As String
        Return "frmWebServiceExpose.htm"
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
