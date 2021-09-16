Imports BluePrism.ApplicationManager.AMI

Public Class ctlApplicationInfo

    Public Event InfoChanged(ByVal sender As Object, ByVal e As EventArgs)
    Public Event AppNameChanged(ByVal sender As Object, ByVal e As EventArgs)
    Public Event InfoSaved(ByVal sender As Object, ByVal e As EventArgs)

    Public Event LaunchClick As EventHandler
    Public Event DiagnosticsClick As EventHandler
    Public Event WizardClick As EventHandler

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Friend ReadOnly Property Owner() As frmIntegrationAssistant
        Get
            Return TryCast(TopLevelControl, frmIntegrationAssistant)
        End Get
    End Property

    Public ReadOnly Property AppName() As String
        Get
            For Each param As clsApplicationParameter In Parameters
                If param.Name = "Application Name" Then Return param.Value
            Next
            Return Nothing
        End Get
    End Property

    <Category("Appearance"),
     Description("Text to appear on the Launch button"),
     DefaultValue("Launch")>
    Public Property LaunchButtonText() As String
        Get
            Return btnLaunch.Text
        End Get
        Set(ByVal value As String)
            btnLaunch.Text = value
        End Set
    End Property

    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Parameters() As ICollection(Of clsApplicationParameter)
        Get
            Dim params As New List(Of clsApplicationParameter)
            For Each ctl As Control In flowPane.FlowedControls
                params.Add(DirectCast(ctl.Tag, clsApplicationParameter))
            Next
            Return params
        End Get
        Set(ByVal value As ICollection(Of clsApplicationParameter))
            flowPane.FlowedControls.Clear()
            Dim owner As frmIntegrationAssistant = Me.Owner
            For Each param As clsApplicationParameter In value
                flowPane.FlowedControls.Add(
                 frmApplicationDefinitionCreate.CreatePageForParameter(
                  param, AddressOf HandleChange,
                  owner Is Nothing OrElse owner.ApplicationLaunched))
            Next
        End Set
    End Property

    Public ReadOnly Property HasParams() As Boolean
        Get
            Return (flowPane.FlowedControls.Count > 0)
        End Get
    End Property

    Public Property LaunchEnabled() As Boolean
        Get
            Return btnLaunch.Enabled
        End Get
        Set(ByVal value As Boolean)
            btnLaunch.Enabled = value
        End Set
    End Property

    Private Sub HandleChange(ByVal sender As Object, ByVal e As EventArgs)

        ' Check for changes to the application manager mode parameter.
        ' If the application manager mode is set to embedded, the external process 
        ' timeout parameter controls need to be disabled.
        Dim ctrl = DirectCast(sender, Control)
        If ctrl.Parent IsNot Nothing AndAlso ctrl.Parent.Tag IsNot Nothing Then

            Dim applicationParameter = DirectCast(ctrl.Parent.Tag,
                clsApplicationParameter)

            If applicationParameter.Name = "ProcessMode" Then
                For Each c As Control In flowPane.FlowedControls
                    If c.Tag Is Nothing Then Continue For
                    Dim p = DirectCast(c.Tag, clsApplicationParameter)
                    If p.Name <> "ExternalProcessTimeout" Then Continue For
                    For Each childControl As Control In c.Controls
                        If Not TypeOf childControl Is ctlProcessTimespan Then _
                            Continue For
                        Dim timespanControl = DirectCast(childControl, IProcessValue)
                        Dim isReadOnly = ctrl.Text = My.Resources.ctlApplicationInfo_EmbeddedDefault
                        timespanControl.ReadOnly = isReadOnly
                    Next
                Next
            End If
        End If

        RaiseEvent InfoChanged(Me, e)
    End Sub

    Public Sub SaveToModel()
        Dim name As String = AppName
        For Each pan As Panel In flowPane.FlowedControls
            frmApplicationDefinitionCreate.StoreParameterFromPanel(pan)
        Next
        RaiseEvent InfoSaved(Me, EventArgs.Empty)
    End Sub

    Private Sub HandleLaunchClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnLaunch.Click
        RaiseEvent LaunchClick(Me, e)
    End Sub

    Private Sub HandleDiagClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnDiag.Click
        RaiseEvent DiagnosticsClick(Me, e)
    End Sub

    Private Sub HandleWizardClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnAppWizard.Click
        RaiseEvent WizardClick(Me, e)
    End Sub

    ''' <summary>
    ''' Determines whether the user can edit the application info.
    ''' </summary>
    Public Property [ReadOnly]() As Boolean
        Get
            Return Not btnDiag.Enabled
        End Get
        Set(ByVal value As Boolean)
            For Each ctl As Control In flowPane.FlowedControls
                ctl.Enabled = Not value
            Next
            btnDiag.Enabled = Not value
        End Set
    End Property

End Class
