Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : Automate
''' Class    : frmStagePropertiesEnd
''' 
''' <summary>
''' The end stage properties form.
''' </summary>
Friend Class frmStagePropertiesEnd
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
    Friend WithEvents mInputsOutputsConditions As AutomateUI.ctlInputsOutputsConditions
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents mDataItemTreeView As AutomateUI.ctlDataItemTreeView
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesEnd))
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
        'mInputsOutputsConditions
        '
        resources.ApplyResources(Me.mInputsOutputsConditions, "mInputsOutputsConditions")
        Me.mInputsOutputsConditions.Name = "mInputsOutputsConditions"
        '
        'mDataItemTreeView
        '
        resources.ApplyResources(Me.mDataItemTreeView, "mDataItemTreeView")
        Me.mDataItemTreeView.CheckBoxes = False
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
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.mDataItemTreeView)
        '
        'frmStagePropertiesEnd
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmStagePropertiesEnd"
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

#Region " Properties "

    ''' <summary>
    ''' The specific end stage represented by this form
    ''' </summary>
    Public ReadOnly Property EndStage() As clsEndStage
        Get
            Return TryCast(mProcessStage, clsEndStage)
        End Get
    End Property

    Public ReadOnly Property Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return ProcessStage
        End Get
    End Property

#End Region

#Region " Constructors "

    Public Sub New()
        MyBase.New()
        InitializeComponent()

        mInputsOutputsConditions.Treeview = mDataItemTreeView

    End Sub

#End Region

#Region " Override methods "

    ''' <summary>
    ''' Populates the stage data set in this form into any child controls which
    ''' require updating if the stage updates.
    ''' </summary>
    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        If EndStage Is Nothing Then
            UserMessage.Show(My.Resources.frmStagePropertiesEnd_PropertiesDialogIsNotProperlyConfigured)
            Exit Sub
        End If

        mInputsOutputsConditions.SetStage(mProcessStage, Me.ProcessViewer, mObjectRefs)
        mInputsOutputsConditions.RefreshControls(mProcessStage)

        mDataItemTreeView.Populate(mProcessStage)
        mDataItemTreeView.ProcessViewer = ProcessViewer

    End Sub

    ''' <summary>
    ''' Extends the validation performed in the base class.
    ''' </summary>
    ''' <returns>Returns true if validation successful, false otherwise.</returns>
    Protected Overrides Function ApplyChanges() As Boolean

        'Commit the parameters first, so that the base
        'class can do some parameter checking
        mProcessStage.ClearParameters()
        mProcessStage.AddParameters(mInputsOutputsConditions.GetOutputParameters())

        If MyBase.ApplyChanges() Then
            EndStage.UpdateEndStages()
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesEnd.htm"
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

#End Region

End Class

