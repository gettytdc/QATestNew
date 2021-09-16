Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

Friend Class UserSettingsNameAndExternalId : Inherits UserDetailsControl
    
#Region " Windows Form Designer generated code "

    'UserControl overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtExternalUserId As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents controlTitle As Label
    Friend WithEvents lblExternalUserIDDescription As Label
    Friend WithEvents txtUsername As AutomateControls.Textboxes.StyledTextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UserSettingsNameAndExternalId))
        Me.txtUsername = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtExternalUserId = New AutomateControls.Textboxes.StyledTextBox()
        Me.controlTitle = New System.Windows.Forms.Label()
        Me.lblExternalUserIDDescription = New System.Windows.Forms.Label()
        Me.SuspendLayout
        '
        'txtUsername
        '
        resources.ApplyResources(Me.txtUsername, "txtUsername")
        Me.txtUsername.BorderColor = System.Drawing.Color.Empty
        Me.txtUsername.Name = "txtUsername"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'txtExternalUserId
        '
        resources.ApplyResources(Me.txtExternalUserId, "txtExternalUserId")
        Me.txtExternalUserId.BorderColor = System.Drawing.Color.Empty
        Me.txtExternalUserId.Name = "txtExternalUserId"
        '
        'controlTitle
        '
        resources.ApplyResources(Me.controlTitle, "controlTitle")
        Me.controlTitle.Name = "controlTitle"
        '
        'lblExternalUserIDDescription
        '
        resources.ApplyResources(Me.lblExternalUserIDDescription, "lblExternalUserIDDescription")
        Me.lblExternalUserIDDescription.Name = "lblExternalUserIDDescription"
        '
        'UserSettingsNameAndExternalId
        '
        Me.Controls.Add(Me.lblExternalUserIDDescription)
        Me.Controls.Add(Me.controlTitle)
        Me.Controls.Add(Me.txtExternalUserId)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtUsername)
        Me.Controls.Add(Me.Label2)
        Me.Name = "UserSettingsNameAndExternalId"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

#End Region

#Region " Member Variables "

    Private mUser As User

    Private mUsernameChanged As Boolean = False

    Private mExternalIdChanged As Boolean = False

    Private mEditingAnExistingUser As Boolean = False

#End Region

#Region " Constructors "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        mEditingAnExistingUser = 
            TryCast(FindForm(), frmExternalUserSettings) IsNot Nothing

        If mEditingAnExistingUser Then
            controlTitle.Text = 
                My.Resources.ctlUserSettingsNameAndExternalId_EditNameAndExternalIdForTheUser
        Else
            controlTitle.Text = 
                My.Resources.ctlUserSettingsNameAndExternalId_CreateUserNameAndInputTheExternalId
        End If
    End Sub

#End Region

#Region " Properties "
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overrides Property User() As User
        Get
            Return mUser
        End Get
        Set(ByVal value As User)
            mUser = value
            If mUser Is Nothing Then
                Username = ""
                ExternalUserId = ""
            Else
                Username = mUser.Name
                ExternalUserId = mUser.ExternalId
            End If
        End Set
    End Property

    Private Property Username() As String
        Get
            Return txtUsername.Text
        End Get
        Set(ByVal value As String)
            txtUsername.Text = value
            mUsernameChanged = False
        End Set
    End Property


    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property ExternalUserId() As String
        Get
            Return txtExternalUserId.Text
        End Get
        Set(ByVal value As String)
            txtExternalUserId.Text = value
            mExternalIdChanged = False
        End Set
    End Property

    Private ReadOnly Property HasExternalId As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(txtExternalUserId.Text)
        End Get
    End Property

    Private ReadOnly Property HasUsername As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(txtUsername.Text)
        End Get
    End Property

#End Region

#Region " Methods "
    Private Sub HandleUsernameChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtUsername.Validated
        If mUser IsNot Nothing Then mUser.Name = txtUsername.Text
        mUsernameChanged = True
    End Sub

    Private Sub HandleExternalIdChanged(ByVal sender As Object, ByVal e As EventArgs) _
        Handles txtExternalUserId.Validated
        If mUser IsNot Nothing Then mUser.ExternalId = txtExternalUserId.Text
        mExternalIdChanged = True
    End Sub

    Private Sub HandleUsernameEnter(ByVal sender As Object, ByVal e As EventArgs) _
        Handles txtUsername.Enter
        txtUsername.SelectAll()
    End Sub

    Public Overrides Function AllFieldsValid() As Boolean

        If mEditingAnExistingUser AndAlso HasUsername AndAlso HasExternalId Then
            If Not mUsernameChanged AndAlso Not mExternalIdChanged Then Return True
        End If

        Dim sErr As String = Nothing
        If Not Auth.User.IsValidUsername(Username, sErr) _
             Then Return UserMessage.Err(sErr)

        If Not Auth.User.IsValidExternalId(ExternalUserId, sErr) _
             Then Return UserMessage.Err(sErr)

        Return True

    End Function

    Public Sub SetUser()
        mUser.Name = Username
        mUser.ExternalId = ExternalUserId
    End Sub

    Private Sub lblExternalUserIDDescription_Click(sender As Object, e As EventArgs) Handles lblExternalUserIDDescription.Click

    End Sub

#End Region

End Class
