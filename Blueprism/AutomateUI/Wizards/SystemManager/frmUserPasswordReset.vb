Imports BluePrism.Common.Security
Imports BluePrism.AutomateAppCore
Imports System.Security

''' Project  : Automate
''' Class    : frmUserPasswordReset
''' 
''' <summary>
''' A wizard to manage user passwords.
''' </summary>
Friend Class frmUserPasswordReset
    Inherits frmWizard

    Public Property UserName As String

    ''' <summary>
    ''' Gets and sets a user's current password. 
    ''' </summary>
    Public Property CurrentPassword As SecureString
        Get
            If Expired Then Return mCurrentPassword
            Return txtCurrentPassword.SecurePassword
        End Get
        Set(value As SecureString)
            mCurrentPassword = value
            If Expired Then
                txtCurrentPassword.Text = "*****"
            Else
                txtCurrentPassword.SecurePassword = value
            End If
        End Set
    End Property
    Private mCurrentPassword As SecureString

    ''' <summary>
    ''' Indicates whether or not frmUserPasswordReset has been loaded due to an expired password.
    ''' </summary>
    Public Property Expired As Boolean

    ''' <summary>
    ''' Indicates that the password has been saved.
    ''' </summary>
    Public Property Saved As Boolean

    ''' <summary>
    ''' Get the new password that was set. Only really a sensible thing to ask for
    ''' if GetSaved returns True.
    ''' </summary>
    Public ReadOnly Property NewPassword() As SafeString
        Get
            Return txtPassword.SecurePassword
        End Get
    End Property

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
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
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtPasswordConfirm As AutomateUI.ctlAutomateSecurePassword
    Friend WithEvents txtPassword As AutomateUI.ctlAutomateSecurePassword
    Friend WithEvents txtCurrentPassword As AutomateUI.ctlAutomateSecurePassword

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmUserPasswordReset))
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.txtPasswordConfirm = New AutomateUI.ctlAutomateSecurePassword()
        Me.txtPassword = New AutomateUI.ctlAutomateSecurePassword()
        Me.txtCurrentPassword = New AutomateUI.ctlAutomateSecurePassword()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Panel3.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        '
        'btnBack
        '
        resources.ApplyResources(Me.btnBack, "btnBack")
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.txtPasswordConfirm)
        Me.Panel3.Controls.Add(Me.txtPassword)
        Me.Panel3.Controls.Add(Me.txtCurrentPassword)
        Me.Panel3.Controls.Add(Me.Label3)
        Me.Panel3.Controls.Add(Me.Label2)
        Me.Panel3.Controls.Add(Me.Label1)
        resources.ApplyResources(Me.Panel3, "Panel3")
        Me.Panel3.Name = "Panel3"
        '
        'txtPasswordConfirm
        '
        resources.ApplyResources(Me.txtPasswordConfirm, "txtPasswordConfirm")
        Me.txtPasswordConfirm.Name = "txtPasswordConfirm"
        '
        'txtPassword
        '
        resources.ApplyResources(Me.txtPassword, "txtPassword")
        Me.txtPassword.Name = "txtPassword"
        '
        'txtCurrentPassword
        '
        resources.ApplyResources(Me.txtCurrentPassword, "txtCurrentPassword")
        Me.txtCurrentPassword.Name = "txtCurrentPassword"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
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
        'frmUserPasswordReset
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.Panel3)
        Me.Name = "frmUserPasswordReset"
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.Panel3, 0)
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

    ''' <summary>
    ''' Moves the wizard along to the next step.
    ''' </summary>
    Protected Overrides Sub UpdatePage()
        Dim iPage As Integer = MyBase.GetStep
        Select Case iPage
            Case 1
                ShowPage(Panel3)
                If Expired Then
                    txtPassword.Focus()
                Else
                    txtCurrentPassword.Focus()
                End If
            Case 2
                SaveAmendments()
                If Not Me.Saved Then
                    If Not Expired Then
                        txtCurrentPassword.Clear()
                    End If
                    txtPassword.Clear()
                    txtPasswordConfirm.Clear()
                    MyBase.Rollback()
                    Exit Sub
                End If
                Me.Close()
        End Select
    End Sub

    Private Sub SaveAmendments()

        Try
            gSv.UpdatePassword(UserName,
                               CurrentPassword,
                               txtPassword.SecurePassword,
                               txtPasswordConfirm.SecurePassword)
            UserMessage.Show(My.Resources.frmUserPasswordReset_ThePasswordHasBeenSuccessfullyAmended)
            Saved = True
        Catch ex As Exception
            UserMessage.Show(
             String.Format(My.Resources.frmUserPasswordReset_ThePasswordHasNotBeenUpdatedSuccessfully0, ex.Message, ex))
        End Try

    End Sub

    Private Sub frmUserPasswordReset_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Expired Then
            Me.objBluebar.Title = String.Format(My.Resources.frmUserPasswordReset_ExpiredChooseANewPasswordForUser0, UserName)
        Else
            Me.objBluebar.Title = String.Format(My.Resources.frmUserPasswordReset_ChooseANewPasswordForUser0, UserName)
        End If
        SetMaxSteps(1)
        SetStep(1)
    End Sub

    Private Sub txtFile_KeyDown(ByVal sender As System.Object,
                                ByVal e As System.Windows.Forms.KeyEventArgs) _
                            Handles txtPasswordConfirm.KeyDown, txtPassword.KeyDown

        If e.KeyCode = Keys.Enter Then
            If sender Is txtPassword Then
                txtPasswordConfirm.Select()
            Else
                If sender Is txtPasswordConfirm Then Me.btnNext.Select()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmUserPasswordReset.htm"
    End Function

End Class
