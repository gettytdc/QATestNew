
Imports AutomateControls
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateProcessCore.Processes

''' Project  : Automate
''' Class    : frmWizard
''' 
''' <summary>
''' A wizard super-class.
''' </summary>
Friend Class frmWizard : Implements IHelp, IChild

    ''' <summary>
    ''' Gets the wizard type which corresponds to the given process type
    ''' </summary>
    ''' <param name="pt">The process type for which the wizard type is required.
    ''' </param>
    ''' <returns>The 1:1 wizard type which maps directly on the process type, almost,
    ''' you might think, obviating the need for two enums really.</returns>
    Public Shared Function GetWizardType(pt As DiagramType) As WizardType
        Select Case pt
            Case DiagramType.Object : Return WizardType.BusinessObject
            Case DiagramType.Process : Return WizardType.Process
            Case Else : Return WizardType.Selection
        End Select
    End Function

#Region "Destructor"

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

#End Region

#Region "Contructor"
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New(ByVal iWizardType As WizardType)
        MyBase.New()

        mWizardType = iWizardType

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Dim objControl As Control
        Dim i As Integer = 0
        For i = Me.Controls.Count - 1 To 0 Step -1
            objControl = Me.Controls(i)
            If Not objControl Is Nothing Then
                If objControl.GetType Is GetType(Panel) Then
                    'Me.Controls.Remove(objControl)
                    objControl.Visible = False
                End If
            End If
        Next
    End Sub

    Public Sub New()
        Me.New(WizardType.Selection)
    End Sub


#End Region

#Region "Member variables"

    Protected miStep As Integer
    Protected miMaxSteps As Integer
    Protected miFirstStep As Integer

    Private Const OffsetOfPanelsFromEdges As Integer = 8
    Private Const OffsetOfLineAboveButtons As Integer = 10

    ''' <summary>
    ''' The default text to use for a next button: "Next ^gt;" by default.
    ''' </summary>
    <Category("Appearance"), DefaultValue("OK"), Description(
        "Sets the text on the Next button within the main stages of the wizard")>
    Public Property DefaultNextText As String = My.Resources.frmWizard_Next

    ''' <summary>
    ''' The default text to use for the 'Next' button on the final stage of the
    ''' wizard: "Finish" by default.
    ''' </summary>
    <Category("Appearance"), DefaultValue("OK"), Description(
        "Sets the text on the Next button for the last stage of the wizard")>
    Public Property DefaultLastStageNextText As String = My.Resources.frmWizard_Finish

    ''' <summary>
    ''' The default text to use for the 'Next' button if this is a single stage
    ''' wizard: "OK" by default.
    ''' </summary>
    <Category("Appearance"), DefaultValue("OK"), Description(
        "Sets the text on the Next button for a single stage wizard")>
    Public Property DefaultSingleStageNextText As String = My.Resources.frmWizard_OK

    ''' <summary>
    ''' Enumerates the options for starting the new, open, export clone etc wizards
    ''' </summary>
    Public Enum WizardType
        Selection = 0
        Process = 1
        BusinessObject = 2
    End Enum

    Protected mWizardType As WizardType

    ''' <summary>
    ''' Private member to store public property CurrentPage.
    ''' </summary>
    Protected mCurrentPage As Panel
    ''' <summary>
    ''' The page currently being viewed.
    ''' </summary>
    Public ReadOnly Property CurrentPage() As Panel
        Get
            Return mCurrentPage
        End Get
    End Property

#End Region

#Region "Events"

#Region "Painting"

    Private Sub frmWizard_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        Me.Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        MyBase.OnPaint(e)
        GraphicsUtil.Draw3DLine(e.Graphics, New Point(0, btnCancel.Top - 10), _
         ListDirection.LeftToRight, Width)
    End Sub

#End Region

#Region "btnCancel_Click"
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        DialogResult = System.Windows.Forms.DialogResult.Cancel
        Close()
    End Sub

#End Region

#Region "btnNext_Click"
    Private Sub btnNext_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNext.Click
        NextPage()
    End Sub

#End Region

#Region "btnBack_Click"
    Private Sub btnBack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBack.Click
        BackPage()
    End Sub

#End Region

#Region "frmWizard_Activated"
    Private Sub frmWizard_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Activated
        If DesignMode Then Return
        Static bActivated As Boolean = False
        If Not bActivated Then
            'Bug 5282 if UpdateNavigation creates a modal dialog,
            'it steals focus before bActivated can be set, when
            'that dialog is closed the activated event is fired again
            'and we get an infinite loop. Instead we set bActivated
            'before calling UpdateNavigation.
            bActivated = True
            UpdateNavigation()
        End If
    End Sub

#End Region

#End Region

#Region "SetStep"

    ''' <summary>
    ''' Sets the wizard's current step.
    ''' </summary>
    ''' <param name="i">The step</param>
    Public Sub SetStep(ByVal i As Integer)
        miStep = i
        miFirstStep = i
    End Sub

#End Region

#Region "The current step"

    ''' <summary>
    ''' Gets the wizard's current step.
    ''' </summary>
    ''' <returns>The step</returns>
    Public Function GetStep() As Integer
        Return miStep
    End Function

    ''' <summary>
    ''' The wizard's current step number. After setting the value, this will ensure
    ''' that <see cref="UpdateNavigation">the nav buttons are updated</see>.
    ''' </summary>
    Protected Property CurrentStep() As Integer
        Get
            Return miStep
        End Get
        Set(ByVal value As Integer)
            If value < 0 Then value = 0
            miStep = value
            UpdateNavigation()
        End Set
    End Property

#End Region

#Region "SetMaxSteps"
    ''' <summary>
    ''' Sets the number of steps in the wizard. This is equal to the number
    ''' of times the user may press the "Next" button - not the number of
    ''' pages in the wizard. Thus for a single page with no steps involved,
    ''' this should be set to zero.
    ''' 
    ''' </summary>
    ''' <param name="iMax">The number of steps. Only values of zero
    ''' and above are valid.</param>
    Public Sub SetMaxSteps(ByVal iMax As Integer)
        miMaxSteps = iMax
    End Sub

    ''' <summary>
    ''' Returns the maximum number of steps allowed by the
    ''' wizard.
    ''' </summary>
    ''' <remarks>See SetMaxSteps for more details.</remarks>
    Public Function GetMaxSteps() As Integer
        Return miMaxSteps
    End Function

#End Region

#Region "ShowPage"

    Protected Overridable Sub ShowPage(ByVal pg As Panel)
        Me.ShowPage(pg, True)
    End Sub

    Protected Overridable Sub ShowPage(ByVal pg As Panel, disablePanelControls As Boolean)

        For Each ctl As Control In Controls
            If TypeOf ctl Is Panel Then
                If disablePanelControls Then
                    ctl.Enabled = False
                End If
                ctl.Visible = False
            End If
        Next

        mCurrentPage = pg
        With mCurrentPage
            .Top = objBluebar.Bottom
            .Left = OffsetOfPanelsFromEdges
            .Height = btnCancel.Top - OffsetOfLineAboveButtons - 4 - objBluebar.Bottom
            .Width = btnCancel.Left + btnCancel.Width - OffsetOfPanelsFromEdges
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right
            .BringToFront()
            .Enabled = True
            .Visible = True
        End With

    End Sub

#End Region

#Region "NextPage"
    Protected Overridable Sub NextPage()
        miStep += 1
        UpdateNavigation()
    End Sub

#End Region

#Region "BackPage"
    ''' <summary>
    ''' Navigates the wizard backward by one
    ''' page.
    ''' </summary>
    ''' <remarks>No need to call UpdateNavigation,
    ''' this method does that for you.</remarks>
    Protected Overridable Sub BackPage()
        miStep -= 1
        UpdateNavigation()
    End Sub

#End Region

#Region "UpdatePage"
    Protected Overridable Sub UpdatePage()
        'do nothing
    End Sub

#End Region

#Region "UpdateNavigation"

    Protected Overridable Sub UpdateNavigation()

        btnBack.Enabled = (miStep > miFirstStep)
        UpdateNextButtonText()

        If miMaxSteps = 0 Then
            btnBack.Visible = False
            btnNext.Text = DefaultSingleStageNextText
        End If

        UpdatePage()
    End Sub

    ''' <summary>
    ''' Updates the text on the next button, according
    ''' to the current step number (see miStep)
    ''' and the number of steps in the wizard
    ''' (see miMaxSteps).
    ''' </summary>
    Protected Sub UpdateNextButtonText()
        btnNext.Text = DefaultNextText

        ' Added safety check around activeform check
        If miStep >= miMaxSteps AndAlso Not ActiveForm Is Nothing AndAlso ActiveForm.GetType = GetType(frmImportRelease) AndAlso Not FilesToImport.FileQueue.IsEmpty Then Return

        If miStep >= miMaxSteps Then btnNext.Text = DefaultLastStageNextText
    End Sub

#End Region

#Region "Rollback"

    ''' <summary>
    ''' Undoes the current step.
    ''' </summary>
    Public Overridable Sub Rollback()
        Me.BackPage()
    End Sub

    ''' <summary>
    ''' Rolls back from the current step a number of times.
    ''' </summary>
    ''' <param name="steps">The number of steps to roll back</param>
    Public Sub Rollback(ByVal steps As Integer)
        If steps > miStep Then
            miStep = 0
        Else
            miStep -= steps
        End If
        UpdateNavigation()
    End Sub

    Public Sub Rollback(ByVal sMessage As String)
        UserMessage.Show(sMessage)
        Rollback()
    End Sub

#End Region

#Region "SetTitleBar"

    Protected Sub UpdatePanelTextToProcessOrObject(ByVal iWizardType As WizardType, ByVal oPanel As Panel)

        Select Case iWizardType
            Case WizardType.BusinessObject
                SetText(oPanel, My.Resources.frmWizard_ProcessOrBusinessObjectUC, My.Resources.frmWizard_BusinessObjectUC)
                SetText(oPanel, My.Resources.frmWizard_ProcessOrBusinessObjectLC, My.Resources.frmWizard_BusinessObjectLC)
                SetText(oPanel, My.Resources.frmWizard_ProcessesUC, My.Resources.frmWizard_BusinessObjectsUC)
                SetText(oPanel, My.Resources.frmWizard_ProcessesLC, My.Resources.frmWizard_BusinessObjectsLC)
                SetText(oPanel, My.Resources.frmWizard_ProcessUC, My.Resources.frmWizard_BusinessObjectUC)
                SetText(oPanel, My.Resources.frmWizard_ProcessLC, My.Resources.frmWizard_BusinessObjectLC)
            Case WizardType.Process
                SetText(oPanel, My.Resources.frmWizard_ProcessOrBusinessObjectUC, My.Resources.frmWizard_ProcessUC)
                SetText(oPanel, My.Resources.frmWizard_ProcessOrBusinessObjectLC, My.Resources.frmWizard_ProcessLC)
                SetText(oPanel, My.Resources.frmWizard_BusinessObjectsUC, My.Resources.frmWizard_ProcessesUC)
                SetText(oPanel, My.Resources.frmWizard_BusinessObjectsLC, My.Resources.frmWizard_ProcessesLC)
                SetText(oPanel, My.Resources.frmWizard_BusinessObjectUC, My.Resources.frmWizard_ProcessUC)
                SetText(oPanel, My.Resources.frmWizard_BusinessObjectLC, My.Resources.frmWizard_ProcessLC)
        End Select

        TitleToSentenceCase()

        Me.Invalidate(True)

    End Sub

    Private Sub TitleToSentenceCase()
        If Me.Text.Length > 1 Then
            Me.Text = Text.Substring(0, 1).ToUpper() + Text.Substring(1)
        ElseIf Me.Text.Length = 1 Then
            Me.Text = Me.Text.ToUpper()
        End If
    End Sub

#End Region

#Region "SetText"


    Private Sub SetText(ByVal p As Panel, ByVal s1 As String, ByVal s2 As String)

        Me.Text = Me.Text.Replace(s1, s2)
        objBluebar.Title = objBluebar.Title.Replace(s1, s2)
        ChangeText(p, s1, s2)

    End Sub

#End Region

#Region "ChangeText"

    Protected Sub ChangeText(ByVal c As Control, ByVal s1 As String, ByVal s2 As String)

        For Each child As Control In c.Controls
            ChangeText(child, s1, s2)
        Next
        c.Text = c.Text.Replace(s1, s2)

    End Sub

#End Region

#Region "SetTitleBarColours"

#End Region

    Protected mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property
End Class
