Imports AutomateControls
Imports AutomateUI.Controls.Widgets.SystemManager.WebApi
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore.Utility


''' <summary>
''' Callback used to test that a name is unique
''' </summary>
''' <param name="id">The ID of the web api being tested</param>
''' <param name="name">The name being tested</param>
''' <returns>True if <paramref name="name"/> is either not used as a Web API name, or
''' is only currently used in a Web API with an ID matching <paramref name="id"/>
''' </returns>
Public Delegate Function IsUniqueNameCallback(id As Guid, name As String) As Boolean

''' <summary>
''' Form used to view or edit a Web API within System Manager.
''' </summary>
Friend Class WebApiForm
    Implements IEnvironmentColourManager

    ' The callback used to ensure that a web API has a unique name
    Private mUniqueNameChecker As IsUniqueNameCallback

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        AssociatedToSkillWarningLabel.Text = WebApi_Resources.WarningLinkedToSkill

    End Sub

    ''' <summary>
    ''' Gets or sets the service in this form.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Service As WebApi
        Get
            Return apiManager.Service
        End Get
        Set(value As WebApi)
            Text = String.Format(
                WebApi_Resources.WindowTitle_Template, If(value?.Name, "?"))
            apiManager.Service = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the callback used to test that a Web API has a unique name.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property IsUniqueNameDelegate As IsUniqueNameCallback
        Get
            Return If(mUniqueNameChecker, Function(id, name) True)
        End Get
        Set(value As IsUniqueNameCallback)
            mUniqueNameChecker = value
        End Set
    End Property

    ''' <summary>
    ''' Handles the Web API name changing on the manager panel. This ensures that the
    ''' API in question has a unique name according to the
    ''' <see cref="IsUniqueNameCallback"/> configured in this form 
    ''' </summary>
    Private Sub HandleWebApiNameChanging(
     sender As Object, e As NameChangingEventArgs) _
     Handles apiManager.WebApiNameChanging
        If String.IsNullOrWhiteSpace(e.NewName) Then
            e.Cancel = True
            e.CancelReason = WebApi_Resources.ErrorEmptyWebApiName

        ElseIf Not IsUniqueNameDelegate(apiManager.ServiceId, e.NewName) Then
            e.Cancel = True
            e.CancelReason = String.Format(
                WebApi_Resources.ErrorDuplicateWebApiName_Template, e.NewName)

        End If
    End Sub

    ''' <summary>
    ''' Handles the Web API name having been changed on the manager panel. This just
    ''' updates the title bar to display the new name.
    ''' </summary>
    Private Sub HandleWebApiNameChanged(
     sender As Object, e As NameChangedEventArgs) Handles apiManager.WebApiNameChanged
        Text = String.Format(WebApi_Resources.WindowTitle_Template, e.NewName)
    End Sub

    ''' <summary>
    ''' Handles the OK button being clicked, ensuring that the data is valid and
    ''' ready for return to the caller.
    ''' </summary>
    Private Sub HandleOk(sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            apiManager.EnsureServiceValid()

            DialogResult = DialogResult.OK
            Close()

        Catch ex As Exception
            UserMessage.Err(ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the cancel button being clicked, ensuring that the appropriate
    ''' dialog result is set and the form is closed.
    ''' </summary>
    Private Sub HandleCancel(sender As Object, e As EventArgs) Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Private Sub PaintBottomStrip(ByVal Sender As Object, ByVal e As PaintEventArgs) Handles pnlBottomStrip.Paint
        GraphicsUtil.Draw3DLine(e.Graphics,
         New Point(0, 1), ListDirection.LeftToRight, pnlBottomStrip.Width)
    End Sub

    '''<inheritdoc />
    Public Overrides Function GetHelpFile() As String
        Return "Web API/HTML/configure-api-definition.htm"
    End Function

    ''' <summary>
    ''' Opens the help file whether online or offline.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    '''<inheritdoc />
    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return ctlTitleBar.BackColor
        End Get
        Set(value As Color)
            ctlTitleBar.BackColor = value
        End Set
    End Property

    '''<inheritdoc />
    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return ctlTitleBar.TitleColor
        End Get
        Set(value As Color)
            ctlTitleBar.TitleColor = value
        End Set
    End Property

    Public Sub SetWarningLabelLinkedToSkillVisibility(showWarning As Boolean)
        AssociatedToSkillWarningLabel.Visible = showWarning
    End Sub
End Class