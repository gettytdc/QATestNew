Imports AutomateUI.Controls.Widgets.SystemManager.WebApi
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Utility

''' <summary>
''' Panel which allows the basic details of a web api to be viewed or edited
''' </summary>
Friend Class WebApiDetailPanel : Implements IGuidanceProvider

    ''' <inheritdoc />
    Friend Event NameChanging As NameChangingEventHandler

    ''' <inheritdoc />
    Friend Event NameChanged As NameChangedEventHandler

    ' The WebApi that this panel is editing
    Private mService As WebApiDetails

    ' The name of the API when loaded
    Private mOriginalName As String

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        CollapseConfigurationSettings()
    End Sub

    ''' <summary>
    ''' Gets or sets the WebApi that this panel is concerned with.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property Service As WebApiDetails
        Get
            Return mService
        End Get
        Set(value As WebApiDetails)
            mService = value
            mOriginalName = value?.Name
            txtName.Text = value?.Name
            txtBaseUrl.Text = value?.BaseUrlString
            cbEnabled.Checked = (value IsNot Nothing AndAlso value.Enabled)
            frmConfigurationSettings.Setup(value.ConfigurationSettings, value.CommonAuthentication.Authentication.Type)
        End Set
    End Property

    ''' <summary>
    ''' Gets the guidance text for this panel.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property GuidanceText As String _
     Implements IGuidanceProvider.GuidanceText
        Get
            Return WebApi_Resources.GuidanceDetailPanel
        End Get
    End Property

    ''' <summary>
    ''' Raises the <see cref="NameChanging"/> event.
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnNameChanging(e As NameChangingEventArgs)
        RaiseEvent NameChanging(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="NameChanged"/> event.
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnNameChanged(e As NameChangedEventArgs)
        RaiseEvent NameChanged(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the validating of the action name, passing it through the
    ''' <see cref="NameChanging"/> event to allow interested parties with more
    ''' context to validate the name.
    ''' </summary>
    Private Sub HandleNameValidating(sender As Object, e As CancelEventArgs) _
     Handles txtName.Validating
        Dim nce As New NameChangingEventArgs(mService?.Name, txtName.Text)
        If Not mOriginalName.Equals(txtName.Text) Then
            OnNameChanging(nce)
        End If
        ' Not cancelled? Nothing more to do
        If Not nce.Cancel Then Return

        UserMessage.Err(nce.CancelReason)
        e.Cancel = True

    End Sub

    ''' <summary>
    ''' Handles the WebApi name being validated, ensuring that the appropriate event
    ''' is raised from this panel and that the underlying service's name is updated.
    ''' </summary>
    Private Sub HandleNameValidated(sender As Object, e As EventArgs) _
     Handles txtName.Validated
        OnNameChanged(New NameChangedEventArgs(mService?.Name, txtName.Text))
        If mService IsNot Nothing Then mService.Name = txtName.Text
    End Sub

    ''' <summary>
    ''' Handles the validating of the Base URL, ensuring that it is non-empty and is
    ''' a valid URL.
    ''' </summary>
    Private Sub HandleBaseUrlValidating(sender As Object, e As CancelEventArgs) _
        Handles txtBaseUrl.Validating

        Dim inputToValidate = txtBaseUrl.Text

        If String.IsNullOrEmpty(inputToValidate.Trim()) Then
            UserMessage.Err(WebApi_Resources.ErrorBaseUrlCannotBeEmpty)
            e.Cancel = True
            Return
        End If

        If HasBaseUrlChanged(inputToValidate) Then
            If inputToValidate.ContainsLeadingOrTrailingWhitespace() Then
                UserMessage.Show(WebApi_Resources.UrlWhitespaceWarning)
            End If
        End If
    End Sub

    Private Function HasBaseUrlChanged(baseUrl As String) As Boolean
        Return Not mService.BaseUrl.Equals(baseUrl)
    End Function

    ''' <summary>
    ''' Handles the base URL being validated, ensuring that the underlying object is
    ''' updated with the new value.
    ''' </summary>
    Private Sub HandleBaseUrlValidated(sender As Object, e As EventArgs) _
     Handles txtBaseUrl.Validated
        If mService IsNot Nothing Then mService.BaseUrlString = txtBaseUrl.Text
    End Sub

    ''' <summary>
    ''' Handles the 'enabled' checkbox being actioned, ensuring that the underlying
    ''' service is updated with the new value.
    ''' </summary>
    Private Sub HandleEnabledChanged(sender As Object, e As EventArgs) _
     Handles cbEnabled.CheckedChanged
        If mService IsNot Nothing Then mService.Enabled = cbEnabled.Checked
    End Sub

    Private Sub expandButton_Click(sender As Object, e As EventArgs) Handles btnExpandSettings.Click
        Dim shouldExpand = splitConfigurationSettings.Panel2Collapsed

        If shouldExpand Then
            ExpandConfigurationSettings()
        Else
            CollapseConfigurationSettings()
        End If

    End Sub

    Private Sub ExpandConfigurationSettings()
        splitConfigurationSettings.Panel2Collapsed = False
        btnExpandSettings.Image = MediaImages.mm_Up_16x16
    End Sub

    Private Sub CollapseConfigurationSettings()
        splitConfigurationSettings.Panel2Collapsed = True
        btnExpandSettings.Image = MediaImages.mm_Down_16x16
    End Sub

End Class
