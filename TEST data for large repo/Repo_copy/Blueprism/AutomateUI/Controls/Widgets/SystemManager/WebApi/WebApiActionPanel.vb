Imports AutomateUI.Controls.Widgets.SystemManager.WebApi
Imports BluePrism.BPCoreLib

''' <summary>
''' Panel which can be used to view and edit Web Api Actions.
''' </summary>
Friend Class WebApiActionPanel : Implements IGuidanceProvider

    ''' <inheritdoc />
    Friend Event NameChanging As NameChangingEventHandler

    ''' <inheritdoc />
    Friend Event NameChanged As NameChangedEventHandler

    ' The action which is being managed by this panel
    Private mAction As WebApiActionDetails

    ''' <summary>
    ''' Creates a new action panel with default values set
    ''' </summary>
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Gets the guidance text for this panel.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property GuidanceText As String _
     Implements IGuidanceProvider.GuidanceText
        Get
            Return WebApi_Resources.GuidanceActionPanel
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the action associated with this panel.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property Action As WebApiActionDetails
        Get
            Return mAction
        End Get
        Set(value As WebApiActionDetails)
            mAction = value
            txtName.Text = value?.Name
            txtDescription.Text = value?.Description
            cbEnabled.Checked = (value IsNot Nothing AndAlso value.Enabled)
            cbEnableRequestOutputParameter.Checked = If(value?.EnableRequestDataOutputParameter, False)
            cbDisableSendingOfRequest.Checked = If(value?.DisableSendingOfRequest, False)
        End Set
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
        Dim nce As New NameChangingEventArgs(mAction?.Name, txtName.Text)
        OnNameChanging(nce)
        ' Not cancelled? Nothing more to do
        If Not nce.Cancel Then Return

        UserMessage.Err(nce.CancelReason)
        e.Cancel = True

    End Sub

    ''' <summary>
    ''' Handles the name control being validated, ensuring that the appropriate event
    ''' is raised and the underlying action is updated with the new name
    ''' </summary>
    Private Sub HandleNameValidated(sender As Object, e As EventArgs) _
     Handles txtName.Validated
        OnNameChanged(New NameChangedEventArgs(mAction?.Name, txtName.Text))
        If mAction IsNot Nothing Then mAction.Name = txtName.Text
    End Sub

    ''' <summary>
    ''' Handles the description control being validated, ensuring that the underlying
    ''' action is updated with the new description
    ''' </summary>
    Private Sub HandleDescriptionValidated(sender As Object, e As EventArgs) _
     Handles txtDescription.Validated
        If mAction IsNot Nothing Then mAction.Description = txtDescription.Text
    End Sub

    ''' <summary>
    ''' Handles the 'enabled' checkbox being actioned, ensuring that the underlying
    ''' action is updated with the new value.
    ''' </summary>
    Private Sub HandleEnabledChanged(sender As Object, e As EventArgs) _
     Handles cbEnabled.CheckedChanged
        If mAction IsNot Nothing Then mAction.Enabled = cbEnabled.Checked
    End Sub

    Private Sub cbEnableRequestOutputParameter_CheckedChanged(sender As Object, e As EventArgs) _
        Handles cbEnableRequestOutputParameter.CheckedChanged
        If mAction IsNot Nothing Then mAction.EnableRequestDataOutputParameter = cbEnableRequestOutputParameter.Checked
    End Sub

    Private Sub cbDisableSendingOfRequest_CheckedChanged(sender As Object, e As EventArgs) _
        Handles cbDisableSendingOfRequest.CheckedChanged
        If mAction IsNot Nothing Then mAction.DisableSendingOfRequest = cbDisableSendingOfRequest.Checked
    End Sub

End Class
