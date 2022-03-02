Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Server.Domain.Models

''' Project  : Automate
''' Class    : frmManageRoles
''' 
''' <summary>
''' A wizard to manage user roles.
''' </summary>
Friend Class frmManageRoles
    Inherits frmWizard
    Implements IPermission


#Region " Windows Form Designer generated code "

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
    Private WithEvents mPermissions As AutomateUI.ctlAuth

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmManageRoles))
        Me.mPermissions = New AutomateUI.ctlAuth()
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
        Me.objBluebar.Title = Global.AutomateUI.My.Resources.frmManageRoles_CreateAndManageBluePrismRoles
        '
        'mPermissions
        '
        resources.ApplyResources(Me.mPermissions, "mPermissions")
        Me.mPermissions.EditMode = AutomateUI.AuthEditMode.ManageRoles
        Me.mPermissions.Name = "mPermissions"
        '
        'frmManageRoles
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.mPermissions)
        Me.Name = "frmManageRoles"
        Me.Title = Global.AutomateUI.My.Resources.frmManageRoles_CreateAndManageBluePrismRoles
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.mPermissions, 0)
        Me.ResumeLayout(False)

    End Sub

#End Region

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.SetMaxSteps(0)

    End Sub

    ''' <summary>
    ''' Gets the associated permission level.
    ''' </summary>
    ''' <value>The permission level</value>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System Manager")
        End Get
    End Property

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmManageRoles.htm"
    End Function

#Region " Methods "

    ''' <summary>
    ''' Moves the wizard along to the next step.
    ''' </summary>
    Protected Overrides Sub UpdatePage()
        If GetStep() = 0 Then Return

        Dim warningMessageFmt As String =
         My.Resources.frmManageRoles_WarningThereAppearsToBeAProblemWithYourActiveDirectoryUserGroups011AreYouSureYo

        Try
            mPermissions.ValidateRoles()

        Catch adce As ActiveDirectoryConfigException
            Dim resp As MsgBoxResult = _
             UserMessage.YesNo(warningMessageFmt, adce.Message, vbCrLf)
            If resp <> MsgBoxResult.Yes Then Return

        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.frmManageRoles_AnErrorOccurredWhileAttemptingToValidateTheActiveDirectoryGroups01, vbCrLf, ex.Message)
            Return

        End Try

        ' Commit the changes
        Dim roles As RoleSet = gSv.UpdateRoles(mPermissions.Roles)
        SystemRoleSet.SystemCurrent.Poll()
        DialogResult = DialogResult.OK
        Close()

    End Sub

#End Region

End Class
