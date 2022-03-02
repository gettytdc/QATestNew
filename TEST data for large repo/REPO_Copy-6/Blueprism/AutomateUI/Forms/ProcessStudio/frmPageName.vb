Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : frmPageRename
''' 
''' <summary>
''' A form to rename process pages.
''' </summary>
Friend Class frmPageName
    Inherits frmForm

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

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents tblButtons As TableLayoutPanel
    Friend WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPageName))
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.tblButtons = New System.Windows.Forms.TableLayoutPanel()
        Me.tblButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.Name = "txtName"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        '
        'tblButtons
        '
        resources.ApplyResources(Me.tblButtons, "tblButtons")
        Me.tblButtons.Controls.Add(Me.btnCancel, 1, 0)
        Me.tblButtons.Controls.Add(Me.btnOK, 0, 0)
        Me.tblButtons.Name = "tblButtons"
        '
        'frmPageName
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.tblButtons)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtName)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmPageName"
        Me.ShowInTaskbar = False
        Me.tblButtons.ResumeLayout(False)
        Me.tblButtons.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Private mobjprocess As clsProcess
    Private msOrigName As String

    Public Enum mode
        NewPage
        RenamePage
    End Enum

    Public Sub New(ByVal formmode As mode)
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        mobjprocess = Nothing
        Select Case formmode
            Case mode.NewPage
                Me.Text = My.Resources.frmPageName_NameNewPage
                Me.Label1.Text = My.Resources.frmPageName_EnterANameForThePage
            Case Else
                Me.Text = My.Resources.frmPageName_RenamePage
                Me.Label1.Text = My.Resources.frmPageName_EnterANewNameForThePageReferencesToThisPageWillAlsoBeRenamedAccordingly
        End Select
    End Sub


    ''' <summary>
    ''' Sets the current name.
    ''' </summary>
    ''' <param name="s">The name</param>
    Public Sub SetName(ByVal s As String)
        txtName.Text = s
        msOrigName = s
    End Sub

    ''' <summary>
    ''' Gets the current name.
    ''' </summary>
    ''' <returns>The name</returns>
    Public Function GetName() As String
        Return txtName.Text
    End Function

    ''' <summary>
    ''' Sets the parent process.
    ''' </summary>
    ''' <param name="objProcess">The process</param>
    Public Sub SetProcess(ByVal objProcess As clsProcess)
        mobjprocess = objProcess
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        Dim DisallowedNames As New List(Of String)
        DisallowedNames.Add(clsProcess.MainPageName.ToLower)
        DisallowedNames.Add(clsProcess.InitPageName.ToLower)

        If DisallowedNames.Contains(txtName.Text.ToLower) Then
            UserMessage.Show(My.Resources.frmPageName_ThatNameIsReservedYouMayNotUseThatNameAsTheNameForAPage)
            Exit Sub
        End If
        If txtName.Text = "" Then
            UserMessage.Show(My.Resources.frmPageName_YouMustGiveTheNewPageAName)
            Exit Sub
        End If

        If txtName.Text <> msOrigName And Not mobjprocess.GetSubSheetID(txtName.Text).Equals(Guid.Empty) Then
            UserMessage.Show(My.Resources.frmPageName_APageWithThatNameAlreadyExistsPleaseChooseAnother)
            Exit Sub

        End If
        DialogResult = System.Windows.Forms.DialogResult.OK
        Close()
    End Sub

    Private Sub txtName_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtName.KeyDown
        Select Case e.KeyCode
            Case Keys.Enter
                Me.btnOK_Click(sender, e)
        End Select
    End Sub

End Class
