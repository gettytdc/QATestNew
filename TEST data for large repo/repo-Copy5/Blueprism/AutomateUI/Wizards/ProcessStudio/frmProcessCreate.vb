
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore.Processes
Imports System.IO
Imports BluePrism.AutomateAppCore.Groups

''' Project  : Automate
''' Class    : frmProcessCreate
''' 
''' <summary>
''' A wizard to create a process.
''' </summary>
Friend Class frmProcessCreate : Inherits frmWizard : Implements IPermission

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
    'Friend WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    'Friend WithEvents txtDescription As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents pnlName As System.Windows.Forms.Panel
    Friend WithEvents pnlDescription As System.Windows.Forms.Panel
    Friend WithEvents lblName As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtDescription As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents pnlProcessOrObject As System.Windows.Forms.Panel
    Friend WithEvents mProcessOrObject As AutomateUI.ctlProcessOrObject
    Friend WithEvents lblSaveInFolder As Label
    Friend WithEvents gtGroups As GroupTreeControl
    Friend WithEvents chkOpenOnClose As CheckBox
    Friend WithEvents lblQuestion As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmProcessCreate))
        Me.pnlName = New System.Windows.Forms.Panel()
        Me.gtGroups = New AutomateUI.GroupTreeControl()
        Me.lblSaveInFolder = New System.Windows.Forms.Label()
        Me.lblName = New System.Windows.Forms.Label()
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.pnlDescription = New System.Windows.Forms.Panel()
        Me.chkOpenOnClose = New System.Windows.Forms.CheckBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtDescription = New AutomateControls.Textboxes.StyledTextBox()
        Me.pnlProcessOrObject = New System.Windows.Forms.Panel()
        Me.mProcessOrObject = New AutomateUI.ctlProcessOrObject()
        Me.lblQuestion = New System.Windows.Forms.Label()
        Me.pnlName.SuspendLayout()
        Me.pnlDescription.SuspendLayout()
        Me.pnlProcessOrObject.SuspendLayout()
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
        Me.btnBack.BackColor = System.Drawing.Color.White
        Me.btnBack.ForeColor = System.Drawing.SystemColors.ControlText
        resources.ApplyResources(Me.btnBack, "btnBack")
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'pnlName
        '
        resources.ApplyResources(Me.pnlName, "pnlName")
        Me.pnlName.Controls.Add(Me.gtGroups)
        Me.pnlName.Controls.Add(Me.lblSaveInFolder)
        Me.pnlName.Controls.Add(Me.lblName)
        Me.pnlName.Controls.Add(Me.txtName)
        Me.pnlName.Name = "pnlName"
        '
        'gtGroups
        '
        resources.ApplyResources(Me.gtGroups, "gtGroups")
        Me.gtGroups.Name = "gtGroups"
        '
        'lblSaveInFolder
        '
        resources.ApplyResources(Me.lblSaveInFolder, "lblSaveInFolder")
        Me.lblSaveInFolder.Name = "lblSaveInFolder"
        '
        'lblName
        '
        resources.ApplyResources(Me.lblName, "lblName")
        Me.lblName.Name = "lblName"
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.BorderColor = System.Drawing.Color.Empty
        Me.txtName.Name = "txtName"
        '
        'pnlDescription
        '
        Me.pnlDescription.Controls.Add(Me.chkOpenOnClose)
        Me.pnlDescription.Controls.Add(Me.Label2)
        Me.pnlDescription.Controls.Add(Me.txtDescription)
        resources.ApplyResources(Me.pnlDescription, "pnlDescription")
        Me.pnlDescription.Name = "pnlDescription"
        '
        'chkOpenOnClose
        '
        resources.ApplyResources(Me.chkOpenOnClose, "chkOpenOnClose")
        Me.chkOpenOnClose.Name = "chkOpenOnClose"
        Me.chkOpenOnClose.UseVisualStyleBackColor = True
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        Me.txtDescription.BorderColor = System.Drawing.Color.Empty
        Me.txtDescription.Name = "txtDescription"
        '
        'pnlProcessOrObject
        '
        Me.pnlProcessOrObject.Controls.Add(Me.mProcessOrObject)
        Me.pnlProcessOrObject.Controls.Add(Me.lblQuestion)
        resources.ApplyResources(Me.pnlProcessOrObject, "pnlProcessOrObject")
        Me.pnlProcessOrObject.Name = "pnlProcessOrObject"
        '
        'mProcessOrObject
        '
        resources.ApplyResources(Me.mProcessOrObject, "mProcessOrObject")
        Me.mProcessOrObject.Name = "mProcessOrObject"
        '
        'lblQuestion
        '
        resources.ApplyResources(Me.lblQuestion, "lblQuestion")
        Me.lblQuestion.Name = "lblQuestion"
        '
        'frmProcessCreate
        '
        resources.ApplyResources(Me, "$this")
        Me.CancelButton = Nothing
        Me.Controls.Add(Me.pnlName)
        Me.Controls.Add(Me.pnlDescription)
        Me.Controls.Add(Me.pnlProcessOrObject)
        Me.Name = "frmProcessCreate"
        Me.Title = "Start work on a completely new process"
        Me.Controls.SetChildIndex(Me.pnlProcessOrObject, 0)
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.pnlDescription, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.pnlName, 0)
        Me.pnlName.ResumeLayout(False)
        Me.pnlName.PerformLayout()
        Me.pnlDescription.ResumeLayout(False)
        Me.pnlDescription.PerformLayout()
        Me.pnlProcessOrObject.ResumeLayout(False)
        Me.pnlProcessOrObject.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Public Property OpenCreatedProcess As Boolean = False
    ''' <summary>
    ''' Default constructor. Creates process rather than object.
    ''' </summary>
    Public Sub New()
        Me.New(WizardType.Selection)
    End Sub

    ''' <summary>
    ''' Creates a new create process form for a process of the given type
    ''' </summary>
    ''' <param name="pt">The process type for the wizard to be created</param>
    Public Sub New(pt As DiagramType)
        Me.New(GetWizardType(pt))
    End Sub

    ''' <summary>
    ''' Alternate constructor. 
    ''' Requires process wizard type, process, object or user choice.
    ''' </summary>
    Public Sub New(ByVal wizardType As WizardType)
        MyBase.New()

        mWizardType = wizardType

        If mWizardType = WizardType.Selection Then
            SetStep(0)
        Else
            SetStep(1)
        End If
        SetMaxSteps(2)

        'form designer requires this
        InitializeComponent()

        mProcessOrObject.ChosenType = mWizardType
    End Sub

    ''' <summary>
    ''' Alternate constructor. 
    ''' Requires process wizard type, process, object or user choice.
    ''' </summary>
    Public Sub New(ByVal wizardType As WizardType, isSaveAs As Boolean)
        MyBase.New()


        Me.isSaveAs = isSaveAs
        mWizardType = wizardType

        If mWizardType = WizardType.Selection Then
            SetStep(0)
        Else
            SetStep(1)
        End If
        Dim maxSteps = If(isSaveAs = True, 3, 2)
        SetMaxSteps(maxSteps)

        'form designer requires this
        InitializeComponent()

        mProcessOrObject.ChosenType = mWizardType
    End Sub

    ''' <summary>
    ''' determines if the dialogue is a SaveAs
    ''' </summary>
    Private isSaveAs As Boolean

    Public Property RefreshList() As Boolean

    ''' <summary>
    ''' Gets the associated permission level.
    ''' </summary>
    ''' <value>The permission level</value>
    Public Overridable ReadOnly Property RequiredPermissions() _
     As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            If mWizardType = WizardType.BusinessObject Then
                Return Permission.ByName(Permission.ObjectStudio.CreateBusinessObject)
            Else
                Return Permission.ByName(Permission.ProcessStudio.CreateProcess)
            End If

        End Get
    End Property

    ''' <summary>
    ''' Moves the wizard along to the next step.
    ''' </summary>
    Protected Overrides Sub UpdatePage()

        Dim iPage As Integer = GetStep()

        Select Case iPage

            Case 0

                Me.Text = My.Resources.frmProcessCreate_NewProcessOrBusinessObject
                Me.Title = My.Resources.frmProcessCreate_StartWorkOnANewProcessOrBusinessObject
                mWizardType = WizardType.Selection
                UpdatePanelTextToProcessOrObject(mWizardType, pnlProcessOrObject)
                ShowPage(pnlProcessOrObject)

            Case 1

                If mWizardType = WizardType.Selection Then
                    If mProcessOrObject.rdoObject.Checked Then
                        mWizardType = WizardType.BusinessObject
                        lblName.Text = My.Resources.frmProcessCreate_EnterANameForTheObject
                    Else
                        mWizardType = WizardType.Process
                    End If
                End If

                Select Case mWizardType
                    Case WizardType.BusinessObject
                        Text = My.Resources.frmProcessCreate_NewBusinessObject
                        Title = My.Resources.frmProcessCreate_StartWorkOnANewBusinessObject
                        chkOpenOnClose.Text = My.Resources.frmProcessCreate_OpenNewBusinessObject
                        lblName.Text = My.Resources.frmProcessCreate_EnterANameForTheObject
                    Case WizardType.Process
                        Text = My.Resources.frmProcessCreate_NewProcess
                        Title = My.Resources.frmProcessCreate_StartWorkOnANewProcess
                        chkOpenOnClose.Text = My.Resources.frmProcessCreate_OpenNewProcess
                        lblName.Text = My.Resources.frmProcessCreate_EnterANameForTheProcess
                End Select
                If isSaveAs Then
                    'set the label for groups to the regional equivalent
                    Dim store As IGroupStore = GetGroupStore()
                    Dim objectsTree As IGroupTree = Nothing
                    Dim permissions As ICollection(Of String) = New List(Of String)
                    Select Case mWizardType
                        Case WizardType.Process
                            permissions.Add(Permission.ProcessStudio.CreateProcess)
                            objectsTree = store.GetTree(GroupTreeType.Processes, Nothing, Nothing, False, False, False)
                            objectsTree = objectsTree.GetFilteredView(Function(t) t.MemberType = GroupMemberType.Group, Function(g) g.Permissions.HasPermission(User.Current, permissions), False)
                            Me.lblSaveInFolder.Text = My.Resources.SaveInFolderLabelTextProcess
                        Case WizardType.BusinessObject
                            permissions.Add(Permission.ObjectStudio.CreateBusinessObject)
                            objectsTree = store.GetTree(GroupTreeType.Objects, Nothing, Nothing, False, False, False)
                            objectsTree = objectsTree.GetFilteredView(Function(t) t.MemberType = GroupMemberType.Group, Function(g) g.Permissions.HasPermission(User.Current, permissions), False)
                            Me.lblSaveInFolder.Text = My.Resources.SaveInFolderLabelTextObject
                    End Select

                    Me.gtGroups.AllowDrop = False
                    Me.gtGroups.Clear()
                    Me.gtGroups.AddTree(objectsTree)
                    Me.gtGroups.SelectedGroup = CType(objectsTree.Root.RawMembers.FirstOrDefault, IGroup)


                    'Load the groups
                Else
                    Me.gtGroups.Visible = False
                    Me.lblSaveInFolder.Visible = False
                End If

                ShowPage(pnlName)
                Me.txtName.Select()

            Case 2

                ShowDescriptionPage()

            Case 3

                OpenCreatedProcess = chkOpenOnClose.Checked
                DialogResult = DialogResult.OK
                Close()

        End Select
    End Sub

    ''' <summary>
    ''' Gets the chosen process name
    ''' </summary>
    Public Function GetChosenProcessName() As String
        Return Me.txtName.Text
    End Function

    ''' <summary>
    ''' Gets the chosen description
    ''' </summary>
    Public Function GetChosenProcessDescription() As String
        Return Me.txtDescription.Text
    End Function

    ''' <summary>
    ''' Gets the chosen GroupName
    ''' </summary>
    Public Function GetChosenGroupName() As Guid
        Return Me.gtGroups.SelectedGroup.IdAsGuid
    End Function


    ''' <summary>
    ''' Gets the chose process type
    ''' </summary>
    Public Function GetChosenProcessType() As DiagramType
        Select Case mProcessOrObject.ChosenType
            Case WizardType.Process
                Return DiagramType.Process
            Case WizardType.BusinessObject
                Return DiagramType.Object
        End Select
    End Function


    ''' <summary>
    ''' Sets the create process/object description page.
    ''' </summary>
    Private Sub ShowDescriptionPage()
        txtName.Text = txtName.Text.Trim()
        If txtName.Text = "" Then
            UserMessage.Show(My.Resources.frmProcessCreate_YouMustEnterAName)
            Rollback()
            Return
        End If
        If txtName.Text.Length > 128 Then
            UserMessage.Show(My.Resources.frmProcessCreate_TheNameCannotBeMoreThan128CharactersLong)
            Rollback()
            Return
        End If
        ' Prevents Processes being created whose names include special characters
        Dim invalidChars = Path.GetInvalidFileNameChars()
        If txtName.Text.Any(Function(c) invalidChars.Contains(c)) Then
            Dim invalidCharsStr = New String(invalidChars.Where(Function(c) Not c = vbNullChar).ToArray())
            UserMessage.Show(String.Format(String.Format(My.Resources.frmProcessCreate_TheNameCannotContainTheFollowingSpecialCharacters0, invalidCharsStr)))
            Rollback()
            Return
        End If
        Dim ConflictingProcessExists As Boolean = Not gSv.IsProcessNameUnique(Guid.Empty, txtName.Text, False)
        Dim ConflictingObjectExists As Boolean = Not gSv.IsProcessNameUnique(Guid.Empty, txtName.Text, True)
        If (ConflictingProcessExists OrElse ConflictingObjectExists) Then
            Dim Thing As String = Nothing
            If ConflictingProcessExists Then Thing = My.Resources.frmProcessCreate_Process
            If ConflictingObjectExists Then Thing = My.Resources.frmProcessCreate_BusinessObject
            RefreshList = True

            UserMessage.Show(String.Format(My.Resources.frmProcessCreate_TheNameYouHaveEnteredIsAlreadyInUseByAnExisting0PleaseChooseADifferentName, Thing))
            Rollback()
            Exit Sub
        End If
        UpdatePanelTextToProcessOrObject(mWizardType, pnlDescription)
        ShowPage(pnlDescription)
        txtDescription.Select()
    End Sub

    Private Sub TextBox_Keydown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtName.KeyDown, txtDescription.KeyDown
        Select Case e.KeyCode
            Case Keys.Enter
                If (sender Is txtName) OrElse (sender Is Me.txtDescription AndAlso Control.ModifierKeys = Keys.Control) Then NextPage()
        End Select
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmProcessCreate.htm"
    End Function

    Private gtGroupCurrentSelection As IGroup
    Private Sub gtGroups_GroupSelected(sender As Object, e As EventArgs) Handles gtGroups.GroupSelected
        'If node is root then revert the selection
        If Me.gtGroups.SelectedGroup.IsRoot Then
            Me.gtGroups.SelectedGroup = gtGroupCurrentSelection
        Else
            gtGroupCurrentSelection = Me.gtGroups.SelectedGroup
        End If
    End Sub
End Class
