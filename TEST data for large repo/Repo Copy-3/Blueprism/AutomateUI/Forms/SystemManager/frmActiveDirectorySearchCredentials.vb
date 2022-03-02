Imports AutomateControls
Imports AutomateUI.My.Resources
Imports BluePrism.Common.Security
Imports BluePrism.Core.ActiveDirectory.DirectoryServices

Public Class frmActiveDirectorySearchCredentials
    Inherits AutomateControls.Forms.AutomateForm
    Implements IEnvironmentColourManager

    Private mCredentials As DirectorySearcherCredentials
    Private mPassword As SafeString

    Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        Text = ActiveDirectoryUserSearch_Resources.UserCredentialsMainHeader
        tBar.Text = ActiveDirectoryUserSearch_Resources.UserSearcherMainHeader
        tBar.Title = ActiveDirectoryUserSearch_Resources.UserCredentialsMainHeader

        lblUserCredentialsDesc.Text = ActiveDirectoryUserSearch_Resources.UserCredentialsDescriptionLabel
        rdoUseDefaultCredentials.Text = ActiveDirectoryUserSearch_Resources.UserCredentialsUseDefaultRadioButton
        rdoUseDefaultCredentials.Checked = True
        rdoInputCustomCredentials.Text = ActiveDirectoryUserSearch_Resources.UserCredentialsInputCustomRadioButton

        lblUsername.Text = ActiveDirectoryUserSearch_Resources.UserCredentialsUsernameHeader
        lblPassword.Text = ActiveDirectoryUserSearch_Resources.UserCredentialsPasswordHeader

        btnUseCredentials.Text = ActiveDirectoryUserSearch_Resources.UserCredentialsButtonUseCredentials

    End Sub

    Private Sub frmActiveDirectoryCredentials_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not mPassword Is Nothing Then
            spPassword.SecurePassword = mPassword
        End If

    End Sub

    Private Sub btnUseCredentials_Click(sender As Object, e As EventArgs) Handles btnUseCredentials.Click
        If rdoInputCustomCredentials.Checked Then
            mPassword = New SafeString(spPassword.SecurePassword)
            mCredentials = New DirectorySearcherCredentials(tbUsername.Text, spPassword.SecurePassword)
        Else
            mCredentials = Nothing
        End If

        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub rdoInputCustomCredentials_CheckedChanged(sender As Object, e As EventArgs) Handles rdoInputCustomCredentials.CheckedChanged

        tbUsername.Enabled = rdoInputCustomCredentials.Checked
        spPassword.Enabled = rdoInputCustomCredentials.Checked

        If Not rdoInputCustomCredentials.Checked Then
            If Not mPassword Is Nothing Then
                mPassword.Clear()
            End If

            tbUsername.Clear()
            spPassword.Clear()
        End If
    End Sub

    Friend Function ShowInDialog(owner As IWin32Window) As DirectorySearcherCredentials
        Me.ShowInTaskbar = False
        Me.ShowDialog(owner)

        Return Me.mCredentials
    End Function

#Region "IEnvironmentColourManager implementation"

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return tBar.BackColor
        End Get
        Set(value As Color)
            tBar.BackColor = value
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return tBar.TitleColor
        End Get
        Set(value As Color)
            tBar.TitleColor = value
        End Set
    End Property


#End Region
End Class
