Imports AutomateControls
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore.Groups

''' Project  : Automate
''' Class    : frmStagePropertiesProcess
''' 
''' <summary>
''' The process properties form.
''' </summary>
Friend Class frmStagePropertiesProcess
    Inherits frmProperties
    Implements IDataItemTreeRefresher

#Region " Windows Form Designer generated code "


    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not mProc Is Nothing Then
            mProc.Dispose()
            mProc = Nothing
        End If
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
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents cmbProcess As ProcessBackedGroupMemberComboBox
    Friend WithEvents mInputsOutputsConditions As AutomateUI.ctlInputsOutputsConditions
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Private WithEvents btnDetails As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents mDataItemTreeView As AutomateUI.ctlDataItemTreeView
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesProcess))
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cmbProcess = New AutomateUI.ProcessBackedGroupMemberComboBox()
        Me.mInputsOutputsConditions = New AutomateUI.ctlInputsOutputsConditions()
        Me.mDataItemTreeView = New AutomateUI.ctlDataItemTreeView()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.btnDetails = New AutomateControls.Buttons.StandardStyledButton()
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
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.Name = "Label3"
        '
        'cmbProcess
        '
        resources.ApplyResources(Me.cmbProcess, "cmbProcess")
        Me.cmbProcess.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbProcess.DropDownBackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbProcess.DropDownWidth = 392
        Me.cmbProcess.Name = "cmbProcess"
        Me.cmbProcess.TreeType = BluePrism.AutomateAppCore.Groups.GroupTreeType.Processes
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
        'btnDetails
        '
        resources.ApplyResources(Me.btnDetails, "btnDetails")
        Me.btnDetails.FlatAppearance.BorderSize = 0
        Me.btnDetails.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(181, Byte), Integer), CType(CType(223, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.btnDetails.Image = Global.AutomateUI.My.Resources.ToolImages.Design_16x16
        Me.btnDetails.Name = "btnDetails"
        Me.ToolTip1.SetToolTip(Me.btnDetails, resources.GetString("btnDetails.ToolTip"))
        '
        'SplitContainer1
        '
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.btnDetails)
        Me.SplitContainer1.Panel1.Controls.Add(Me.mInputsOutputsConditions)
        Me.SplitContainer1.Panel1.Controls.Add(Me.Label3)
        Me.SplitContainer1.Panel1.Controls.Add(Me.cmbProcess)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.mDataItemTreeView)
        resources.ApplyResources(Me.SplitContainer1.Panel2, "SplitContainer1.Panel2")
        '
        'frmStagePropertiesProcess
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmStagePropertiesProcess"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.SplitContainer1, 0)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel1.PerformLayout()
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region


#Region "Members"

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


    Private mProc As clsProcess

#End Region


    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        mProcessStage = Nothing
        mObjectRefs = Nothing


    End Sub
    Private Sub frmStageProperties_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'Make sure we have a valid index...
        Dim objProcessRef As clsSubProcessRefStage = CType(mProcessStage, clsSubProcessRefStage)
        If mProcessStage Is Nothing Then
            UserMessage.Show("Properties dialog is not properly configured")
            Exit Sub
        End If

        mInputsOutputsConditions.SetStage(mProcessStage, Me.ProcessViewer, mObjectRefs)

        'Fill in all the fields...
        MyBase.txtName.Text = mProcessStage.GetName()
        MyBase.txtDescription.Text = mProcessStage.GetNarrative()

        Me.mDataItemTreeView.ProcessViewer = Me.ProcessViewer
        Me.mInputsOutputsConditions.Treeview = Me.mDataItemTreeView

        Me.mInputsOutputsConditions.RefreshControls(mProcessStage.GetInputs, mProcessStage.GetOutputs)
        mDataItemTreeView.Populate(mProcessStage)

        Me.OriginalDataItemsTreeviewWidth = Me.mDataItemTreeView.Width
        Me.OriginalDataItemsTreeviewLeft = Me.mDataItemTreeView.Left

        Me.UpdateProcessList(objProcessRef.ReferenceId)
        Me.btnDetails.Enabled = True

    End Sub

    ''' <summary>
    ''' Repopulates the combobox list, and selects the supplied item, if requested.
    ''' </summary>
    ''' <param name="ProcessIDToSelect">The ID of the process to be selected, or guid.empty
    ''' if none.</param>
    Private Sub UpdateProcessList(ByVal ProcessIDToSelect As Guid)
        Try
            cmbProcess.TreeType = GroupTreeType.Processes
            cmbProcess.SelectedIdAsGuid = ProcessIDToSelect

            For Each item As ComboBoxItem In cmbProcess.Items
                If Not item.Enabled Then
                    item.Text = String.Format(My.Resources.frmStagePropertiesProcess_NoPermission0, item.Text)
                End If
            Next
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Update the preconditions and endpoints collection
    ''' </summary>
    Private Sub UpdatePreconditionsAndEndpoints()
        'just bail out we dont have a process yet
        If mProc Is Nothing Then Return
        mInputsOutputsConditions.UpdatePreconditionsAndPostconditions(
         mProc.Preconditions, GetSingleton.ICollection(mProc.Endpoint))
    End Sub

    ''' <summary>
    ''' Extends the validation performed in the base class.
    ''' </summary>
    ''' <returns>Returns true if validation successful, false otherwise.</returns>
    Protected Overrides Function ApplyChanges() As Boolean
        If Not MyBase.ApplyChanges() Then Return False
        ' Get the process ID for the selected process and save it back to the stage
        ProcessRefStage.ReferenceId = cmbProcess.SelectedIdAsGuid
        Return True
    End Function

    ''' <summary>
    ''' Gets the stage that this form represents as a sub process reference stage
    ''' </summary>
    Public ReadOnly Property ProcessRefStage() As clsSubProcessRefStage
        Get
            Return DirectCast(mProcessStage, clsSubProcessRefStage)
        End Get
    End Property

    Public ReadOnly Property Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return ProcessStage
        End Get
    End Property

    ''' <summary>
    ''' Called when the process is changed - all inputs and outputs
    ''' for the stage are rewritten accordingly. If any parameters
    ''' are common between the previous and new actions, the values
    ''' are retained.
    ''' </summary>
    Private Sub RewriteParameters()

        Dim sErr As String = Nothing

        'Get the process ID for the selected process...
        Dim mem = TryCast(cmbProcess.SelectedMember, ProcessBackedGroupMember)
        If mem Is Nothing Then Return

        Dim procId As Guid = mem.IdAsGuid
        If procId = Guid.Empty Then Return

        'Get the process XML for the process...
        Dim procXml As String
        Try
            procXml = gSv.GetProcessXML(procId)
        Catch ex As Exception
            Exit Sub
        End Try

        'Create a class representation of the process
        mProc = clsProcess.FromXML(Options.Instance.GetExternalObjectsInfo(), procXml, False, sErr)
        If mProc Is Nothing Then Exit Sub

        'Find the start stage, and get the parameters.
        Dim param1 As clsProcessParameter, param2 As clsProcessParameter
        Dim startStg As clsProcessStage = mProc.GetStage(mProc.GetStartStage())
        If Not startStg Is Nothing Then

            'Stage 1 - remove any parameters from the process that
            '          should not be there
InStage1:
            For i = 0 To mProcessStage.GetNumParameters() - 1
                param1 = mProcessStage.GetParameter(i)
                If param1.Direction = ParamDirection.In And startStg.GetParameter(param1.Name, param1.Direction) Is Nothing Then
                    'Remove the parameter, then start this stage
                    'again...
                    mProcessStage.RemoveParameter(param1.Name, param1.Direction)
                    GoTo InStage1
                End If
            Next

            'Stage 2 - add any parameters that aren't present, and
            '          update properties for those that are
            For j = 0 To startStg.GetNumParameters() - 1
                param1 = startStg.GetParameter(j)
                If param1.Direction = ParamDirection.In Then
                    Dim bFound As Boolean
                    bFound = False
                    For i = 0 To mProcessStage.GetNumParameters() - 1
                        param2 = mProcessStage.GetParameter(i)
                        If param2.Name = param1.Name And _
                         param2.Direction = param1.Direction Then
                            'Update the data type if it changed...
                            If param2.GetDataType() <> param1.GetDataType() Then
                                param2.SetDataType(param1.GetDataType())
                            End If
                            If param2.Narrative <> param1.Narrative Then
                                param2.Narrative = param1.Narrative
                            End If
                            If param2.FriendlyName <> param1.FriendlyName Then
                                param2.FriendlyName = param1.FriendlyName
                            End If
                            bFound = True
                        End If
                    Next
                    If Not bFound Then
                        mProcessStage.AddParameter(param1.Direction, param1.GetDataType(), param1.Name, param1.Narrative, MapType.None, "", Nothing, param1.FriendlyName)
                    End If
                End If

            Next

        End If

        'Find the end stage, and get the parameters.
        Dim objEndStage As clsProcessStage
        objEndStage = mProc.GetStage(mProc.GetEndStage())
        If Not objEndStage Is Nothing Then

            'Stage 1 - remove any parameters from the process that
            '          should not be there
OutStage1:
            For i = 0 To mProcessStage.GetNumParameters() - 1
                param1 = mProcessStage.GetParameter(i)
                If param1.Direction = ParamDirection.Out And objEndStage.GetParameter(param1.Name, param1.Direction) Is Nothing Then
                    'Remove the parameter, then start this stage
                    'again...
                    mProcessStage.RemoveParameter(param1.Name, param1.Direction)
                    GoTo OutStage1
                End If
            Next

            'Stage 2 - add any parameters that aren't present, and
            '          update properties for those that are
            For j = 0 To objEndStage.GetNumParameters() - 1
                param1 = objEndStage.GetParameter(j)
                If param1.Direction = ParamDirection.Out Then
                    Dim bFound As Boolean
                    bFound = False
                    For i = 0 To mProcessStage.GetNumParameters() - 1
                        param2 = mProcessStage.GetParameter(i)
                        If param2.Name = param1.Name And _
                         param2.Direction = param1.Direction Then
                            'Update the data type if it changed...
                            If param2.GetDataType() <> param1.GetDataType() Then
                                param2.SetDataType(param1.GetDataType())
                            End If
                            If param2.Narrative <> param1.Narrative Then
                                param2.Narrative = param1.Narrative
                            End If
                            If param2.FriendlyName <> param1.FriendlyName Then
                                param2.FriendlyName = param1.FriendlyName
                            End If
                            bFound = True
                        End If
                    Next
                    If Not bFound Then
                        mProcessStage.AddParameter(param1.Direction, param1.GetDataType(), param1.Name, param1.Narrative, MapType.None, "", Nothing, param1.FriendlyName)
                    End If
                End If
            Next
        End If
    End Sub

    Private Sub cmbProcess_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbProcess.SelectedIndexChanged
        RewriteParameters()
        UpdatePreconditionsAndEndpoints()
        Me.mInputsOutputsConditions.RefreshControls(mProcessStage.GetInputs, mProcessStage.GetOutputs)
    End Sub

    Private Sub btnDetails_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDetails.Click
        Try
            Dim mem = TryCast(cmbProcess.SelectedMember, ProcessBackedGroupMember)
            If mem Is Nothing Then
                UserMessage.Show(My.Resources.frmStagePropertiesProcess_CouldNotDisplayTheProcessSpecifiedYouMustSelectAProcessToView)
                Return
            End If

            Using view As New frmProcess(ProcessViewMode.PreviewProcess, mem.IdAsGuid, "", "")
                Dim appFrm As frmApplication = CType(mParentForm, IChild).ParentAppForm
                view.ParentAppForm = appFrm
                view.Icon = Me.Icon
                view.ShowInTaskbar = False
                view.ShowDialog()
            End Using

            Exit Sub

        Catch ex As Exception
            UserMessage.Err(ex, String.Format(My.Resources.frmStagePropertiesProcess_CouldNotDisplayTheProcessSpecified0, ex.Message))

        End Try

    End Sub

    Private Sub ShiftComponents(ByVal ShiftDistance As Integer, ByVal ShiftRight As Boolean)
        Dim HalfWidth As Integer = ShiftDistance \ 2
        Dim iTemp As Integer = 1
        If Not ShiftRight Then iTemp = -1

        Me.mDataItemTreeView.Left += ShiftDistance * iTemp
        Me.mInputsOutputsConditions.Width += ShiftDistance * iTemp
        Me.btnDetails.Left += ShiftDistance * iTemp
        Me.cmbProcess.Width += ShiftDistance * iTemp
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
        Return "frmStagePropertiesProcess.htm"
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
