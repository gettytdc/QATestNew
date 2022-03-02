Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : Automate
''' Class    : frmStagePropertiesStart
''' 
''' <summary>
''' The start stage properties form.
''' </summary>
Friend Class frmStagePropertiesStart
    Inherits frmProperties
    Implements IDataItemTreeRefresher


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
    Friend WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader6 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents mInputsOutputsConditions As AutomateUI.ctlInputsOutputsConditions
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents mDataItemTreeView As AutomateUI.ctlDataItemTreeView
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesStart))
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader6 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.mInputsOutputsConditions = New AutomateUI.ctlInputsOutputsConditions()
        Me.mDataItemTreeView = New AutomateUI.ctlDataItemTreeView()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        '
        'ColumnHeader5
        '
        resources.ApplyResources(Me.ColumnHeader5, "ColumnHeader5")
        '
        'ColumnHeader4
        '
        resources.ApplyResources(Me.ColumnHeader4, "ColumnHeader4")
        '
        'ColumnHeader6
        '
        resources.ApplyResources(Me.ColumnHeader6, "ColumnHeader6")
        '
        'mInputsOutputsConditions
        '
        resources.ApplyResources(Me.mInputsOutputsConditions, "mInputsOutputsConditions")
        Me.mInputsOutputsConditions.Name = "mInputsOutputsConditions"
        '
        'mDataItemTreeView
        '
        Me.mDataItemTreeView.CheckBoxes = False
        resources.ApplyResources(Me.mDataItemTreeView, "mDataItemTreeView")
        Me.mDataItemTreeView.IgnoreScope = False
        Me.mDataItemTreeView.Name = "mDataItemTreeView"
        Me.mDataItemTreeView.Stage = Nothing
        Me.mDataItemTreeView.StatisticsMode = False
        '
        'SplitContainer1
        '
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.mInputsOutputsConditions)
        resources.ApplyResources(Me.SplitContainer1.Panel1, "SplitContainer1.Panel1")
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.mDataItemTreeView)
        resources.ApplyResources(Me.SplitContainer1.Panel2, "SplitContainer1.Panel2")
        '
        'frmStagePropertiesStart
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmStagePropertiesStart"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.SplitContainer1, 0)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region "New"

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        mProcessStage = Nothing

    End Sub

#End Region

    ''' <summary>
    ''' Used for when we maximise and then restore form to original size. We need 
    ''' to know what size it was before any resizing took place so that we can
    ''' restore the original size.
    ''' </summary>
    Private OriginalDataItemsTreeviewWidth As Integer

    ''' <summary>
    ''' Used for when we maximise and then restore form to original size. We need 
    ''' to know what location it had before any resizing took place so that we can
    ''' restore the original location.
    ''' </summary>
    Private OriginalDataItemsTreeviewLeft As Integer

    Public ReadOnly Property Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return ProcessStage
        End Get
    End Property

    Private Sub frmStagePropertiesStart_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'Make sure we have a valid index...
        If mProcessStage Is Nothing Then
            UserMessage.Show(My.Resources.frmStagePropertiesStart_PropertiesDialogIsNotProperlyConfigured)
            Exit Sub
        End If

        Me.mInputsOutputsConditions.SetStage(mProcessStage, Me.ProcessViewer, mObjectRefs)

        MyBase.txtName.Text = mProcessStage.GetName()
        MyBase.txtDescription.Text = mProcessStage.GetNarrative

        Me.mDataItemTreeView.ProcessViewer = Me.ProcessViewer
        Me.mInputsOutputsConditions.Treeview = Me.mDataItemTreeView

        Me.mInputsOutputsConditions.RefreshControls(mProcessStage.GetInputs, mProcessStage.GetOutputs)
        mDataItemTreeView.Populate(mProcessStage)

        Me.OriginalDataItemsTreeviewWidth = Me.mDataItemTreeView.Width
        Me.OriginalDataItemsTreeviewLeft = Me.mDataItemTreeView.Left
        
    End Sub

    ''' <summary>
    ''' Applies the changes.
    ''' </summary>
    ''' <returns>Returns true on success. See base class for more
    ''' info.</returns>
    ''' <remarks></remarks>
    Protected Overrides Function ApplyChanges() As Boolean
        Me.mProcessStage.ClearParameters()
        Me.mProcessStage.AddParameters(Me.mInputsOutputsConditions.GetInputParameters)

        Return MyBase.ApplyChanges()
    End Function


    Private Sub ShiftComponents(ByVal ShiftDistance As Integer, ByVal ShiftRight As Boolean)
        Dim iTemp As Integer = 1
        If Not ShiftRight Then iTemp = -1

        Me.mDataItemTreeView.Left += ShiftDistance * iTemp
        Me.mInputsOutputsConditions.Width += ShiftDistance * iTemp
        Me.txtName.Width = Me.mInputsOutputsConditions.Left + Me.mInputsOutputsConditions.Width - Me.txtName.Left
        Me.txtDescription.Width = Me.txtName.Width
    End Sub


    Private Sub frmStagePropertiesCalculation_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        Static LastWindowState As FormWindowState
        If Not (MyBase.WindowState = FormWindowState.Minimized OrElse LastWindowState = FormWindowState.Minimized) Then
            MyBase.SuspendLayout()
            Static WidthReduction As Integer
            If Me.mDataItemTreeView.Width > 350 AndAlso Me.mDataItemTreeView.Left <= Me.OriginalDataItemsTreeviewLeft + 5 Then
                WidthReduction = Me.mDataItemTreeView.Width - 350
                Me.mDataItemTreeView.Width -= WidthReduction
                ShiftComponents(WidthReduction, True)
            Else
                If Not WidthReduction = 0 Then                 'we must be restoring to default size after minimising
                    Me.mDataItemTreeView.Width = Me.OriginalDataItemsTreeviewWidth
                    ShiftComponents(WidthReduction, False)
                End If
            End If
            MyBase.ResumeLayout(True)

        End If

        LastWindowState = MyBase.WindowState
    End Sub


    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesStart.htm"
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

    Public Sub Repopulate(displayStage As clsDataStage) Implements IDataItemTreeRefresher.Repopulate
        mDataItemTreeView.Repopulate(displayStage)
    End Sub

    Public Sub Remove(stage As clsDataStage) Implements IDataItemTreeRefresher.Remove
        mDataItemTreeView.RemoveDataItemTreeNode(stage)
    End Sub
End Class
