Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.Skills

Friend Class frmStagePropertiesSkill
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
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents cmbAction As AutomateControls.StyledComboBox
    Friend WithEvents btnDetails As ctlRolloverButton
    Friend WithEvents mInputsOutputsConditions As AutomateUI.ctlInputsOutputsConditions
    Friend WithEvents mDataItemTreeView As AutomateUI.ctlDataItemTreeView
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents txtWebApiName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesSkill))
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.txtWebApiName = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.mInputsOutputsConditions = New AutomateUI.ctlInputsOutputsConditions()
        Me.btnDetails = New AutomateUI.ctlRolloverButton()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmbAction = New AutomateControls.StyledComboBox()
        Me.mDataItemTreeView = New AutomateUI.ctlDataItemTreeView()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
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
        'SplitContainer1
        '
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.txtWebApiName)
        Me.SplitContainer1.Panel1.Controls.Add(Me.Label1)
        Me.SplitContainer1.Panel1.Controls.Add(Me.mInputsOutputsConditions)
        Me.SplitContainer1.Panel1.Controls.Add(Me.btnDetails)
        Me.SplitContainer1.Panel1.Controls.Add(Me.Label4)
        Me.SplitContainer1.Panel1.Controls.Add(Me.cmbAction)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.mDataItemTreeView)
        resources.ApplyResources(Me.SplitContainer1.Panel2, "SplitContainer1.Panel2")
        '
        'txtWebApiName
        '
        resources.ApplyResources(Me.txtWebApiName, "txtWebApiName")
        Me.txtWebApiName.Name = "txtWebApiName"
        Me.txtWebApiName.ReadOnly = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'mInputsOutputsConditions
        '
        resources.ApplyResources(Me.mInputsOutputsConditions, "mInputsOutputsConditions")
        Me.mInputsOutputsConditions.Name = "mInputsOutputsConditions"
        '
        'btnDetails
        '
        resources.ApplyResources(Me.btnDetails, "btnDetails")
        Me.btnDetails.DefaultImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16
        Me.btnDetails.DisabledImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16_Disabled
        Me.btnDetails.Name = "btnDetails"
        Me.btnDetails.RolloverImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16_Hot
        Me.btnDetails.TooltipText = My.Resources.ClickToViewDocumentationForTheSelectedBusinessObject
        Me.btnDetails.TooltipTitle = My.Resources.ViewDocumentation
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'cmbAction
        '
        resources.ApplyResources(Me.cmbAction, "cmbAction")
        Me.cmbAction.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbAction.Checkable = False
        Me.cmbAction.DisabledItemColour = System.Drawing.Color.LightGray
        Me.cmbAction.DropDownBackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbAction.DropDownWidth = 392
        Me.cmbAction.Name = "cmbAction"
        Me.cmbAction.Sorted = True
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
        'frmStagePropertiesSkill
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmStagePropertiesSkill"
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

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

    End Sub


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

#End Region

    Private ReadOnly Property SkillStage As clsSkillStage
        Get
            Return CType(mProcessStage, clsSkillStage)
        End Get
    End Property

    Private Property Skill As Skill

    Private ReadOnly Property SelectedAction As clsBusinessObjectAction
        Get
            Dim actItem = cmbAction.SelectedComboBoxItem
            If actItem Is Nothing Then Return Nothing
            Return TryCast(actItem.Tag, clsBusinessObjectAction)
        End Get
    End Property

    Public ReadOnly Property Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return ProcessStage
        End Get
    End Property

    Protected Overrides Sub PopulateStageData()
        Try
            MyBase.PopulateStageData()

            'disable the info button beside the business object dropdown
            btnDetails.BackColor = Me.BackColor
            chkLogParametersInhibit.Visible = True

            ' Re-perform the logging options logic to ensure form controls are 
            ' in the correct state based on editability of the stage
            SetupLogParameterCheckbox(IsEditable)

            mInputsOutputsConditions.SetStage(mProcessStage, Me.ProcessViewer, mObjectRefs)
            mInputsOutputsConditions.Treeview = mDataItemTreeView
            mInputsOutputsConditions.RefreshControls(mProcessStage)
            mDataItemTreeView.Populate(mProcessStage)
            mDataItemTreeView.ProcessViewer = ProcessViewer

            RemoveHandler cmbAction.SelectedIndexChanged, AddressOf cmbAction_SelectedIndexChanged

            Skill = gSv.GetSkill(SkillStage.SkillId)
            Me.Text = String.Format(My.Resources.x0Properties, Skill.LatestVersion.Name)

            UpdateActions()
            UpdateActionDetails()
            UpdateMissing()
            CheckSkillIsAvailable()

            AddHandler cmbAction.SelectedIndexChanged, AddressOf Me.cmbAction_SelectedIndexChanged

            chkLogParametersInhibit.Checked = Not mProcessStage.LogParameters
        Catch ex As Exception
            UserMessage.Err(ex, String.Format(My.Resources.UnableToPopulateStageInformation0, ex.Message))
        End Try
    End Sub

    Private Sub CheckSkillIsAvailable()
        If Skill Is Nothing Then Return

        Dim skillAvailable = Skill.Enabled
        Dim webApiAvailable = CType(Skill.LatestVersion, WebSkillVersion).WebApiEnabled
        Dim webApiName = CType(Skill.LatestVersion, WebSkillVersion).WebApiName

        If (Not skillAvailable) Then
            IsEditable = False
            Text += My.Resources.WarningSkillIsUnavailable
            txtWebApiName.Text = webApiName
        ElseIf (Not webApiAvailable) Then
            IsEditable = False
            Text += My.Resources.WarningWebAPIIsUnavailable
            txtWebApiName.Text = String.Format(My.Resources.MissingWebAPI0, webApiName)
        Else
            txtWebApiName.Text = webApiName
        End If
    End Sub

    Private Sub UpdateMissing()
        Dim missingActionWarning As String = My.Resources.MissingAction0

        If SelectedAction Is Nothing AndAlso Not String.IsNullOrEmpty(SkillStage.ActionName) Then
            SetMissing(cmbAction, missingActionWarning, SkillStage.ActionName)
        End If
    End Sub

    Private Sub SetMissing(cmb As ComboBox, s As String, ParamArray args() As String)
        Dim Item As New AutomateControls.ComboBoxItem(String.Format(s, args), False)
        Item.Tag = Nothing
        Item.Color = Color.Red
        cmb.Sorted = False
        cmb.Items.Insert(0, Item)
        cmb.SelectedItem = Item
    End Sub

    Private mNoneComboBoxItem As AutomateControls.ComboBoxItem

    Protected Overrides Function ApplyChanges() As Boolean
        If Not MyBase.ApplyChanges() Then Return False

        'Set the parameters
        Me.mProcessStage.ClearParameters()
        Me.mProcessStage.AddParameters(mInputsOutputsConditions.GetInputParameters)
        Me.mProcessStage.AddParameters(mInputsOutputsConditions.GetOutputParameters)

        'Save the changes...
        mProcessStage.LogParameters = Not chkLogParametersInhibit.Checked

        If SelectedAction IsNot Nothing Then
            SkillStage.ActionName = SelectedAction.GetName()
            Return True
        End If

        Return True
    End Function

    ''' <summary>
    ''' Populates the actions combobox with a list of available actions based on the
    ''' selection of object in the objects combobox.
    ''' </summary>
    Private Sub UpdateActions()

        cmbAction.Items.Clear()

        Me.btnDetails.Enabled = True
        Me.btnDetails.BackColor = Me.BackColor
        cmbAction.Enabled = Me.IsEditable

        Dim webAPI = mObjectRefs.FindObjectReference(CType(Skill.LatestVersion, WebSkillVersion).WebApiName)
        If webAPI Is Nothing Then
            Return
        End If

        For Each objAct In webAPI.GetActions()
            cmbAction.Items.Add(New AutomateControls.ComboBoxItem(objAct.GetName(), objAct))
        Next

        Dim stage = SkillStage
        Dim action = webAPI.GetAction(stage.ActionName)
        If action IsNot Nothing Then
            cmbAction.SelectComboBoxByTag(action)
        ElseIf stage.ActionName = String.Empty Then
            cmbAction.SelectFirst()
        End If

        Me.mInputsOutputsConditions.RefreshControls(mProcessStage.GetInputs, mProcessStage.GetOutputs)
    End Sub

    Private Sub cmbAction_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbAction.SelectedIndexChanged
        UpdateActionDetails()
    End Sub

    Private Sub UpdateActionDetails()
        RewriteParameters()
        UpdatePreconditionsAndEndpoints()
        Me.mInputsOutputsConditions.RefreshControls(mProcessStage.GetInputs, mProcessStage.GetOutputs)
    End Sub

    ''' <summary>
    ''' Updates the user control showing the list of preconditions and endconditions
    ''' using the conditions listed in the business object.
    ''' </summary>
    Private Sub UpdatePreconditionsAndEndpoints()
        Dim preconds As New List(Of String)
        Dim postconds As New List(Of String)

        ' Build up the preconds from the referenced action's preconds/endpoint - if
        ' any part of the chain is not set (business object, action, action's
        ' preconds/endpoint) then the affected collections remain empty and the
        ' control's preconds/postconds are cleared
        Dim webAPI = mObjectRefs.FindObjectReference(CType(Skill.LatestVersion, WebSkillVersion).Name)
        If webAPI IsNot Nothing Then
            Dim action As clsBusinessObjectAction = webAPI.GetAction(cmbAction.Text)
            If action IsNot Nothing Then
                Dim endPoint As String = action.GetEndpoint()
                If endPoint IsNot Nothing Then postconds.Add(endPoint)

                Dim actionPreconds As Collection = action.GetPreConditions
                If actionPreconds IsNot Nothing Then
                    For Each objCond As String In actionPreconds
                        preconds.Add(objCond)
                    Next
                End If
            End If
        End If

        mInputsOutputsConditions.UpdatePreconditionsAndPostconditions(
         preconds, postconds)
    End Sub

    ''' <summary>
    ''' Called when the action is changed - all inputs and outputs
    ''' for the stage are rewritten accordingly. If any parameters
    ''' are common between the previous and new actions, the values
    ''' are retained.
    ''' </summary>
    Private Sub RewriteParameters()
        Dim ii As Integer
        Dim objParm As clsProcessParameter
        Dim objParm2 As clsProcessParameter

        Dim stg As clsSkillStage = SkillStage

        Dim objAct = SelectedAction
        If objAct Is Nothing Then
            mProcessStage.ClearParameters()
            Exit Sub
        End If


        'Stage 1 - remove any parameters from the process that
        '          should not be there
Stage1:
        For ii = 0 To mProcessStage.GetNumParameters() - 1
            objParm = mProcessStage.GetParameter(ii)
            If objAct.GetParameter(objParm.Name, objParm.Direction) Is Nothing Then
                'Remove the parameter, then start this stage
                'again...
                mProcessStage.RemoveParameter(objParm.Name, objParm.Direction)
                GoTo Stage1
            End If
        Next

        'Stage 2 - add any parameters that aren't present, and
        '          update properties for those that are
        For Each objParm In objAct.GetParameters()

            'Determine the appropriate map type, which depends on whether the parameter
            'is an input or an output...
            Dim mt As MapType
            If objParm.Direction = ParamDirection.In Then
                mt = MapType.Expr
            Else
                mt = MapType.Stage
            End If

            Dim bFound As Boolean
            bFound = False
            For ii = 0 To mProcessStage.GetNumParameters() - 1
                objParm2 = mProcessStage.GetParameter(ii)
                If objParm2.Name = objParm.Name And
                 objParm2.Direction = objParm.Direction Then
                    'Update the data type if it changed...
                    If objParm2.GetDataType() <> objParm.GetDataType() Then
                        objParm2.SetDataType(objParm.GetDataType())
                    End If
                    'Update the map type if it changed...
                    If objParm2.GetMapType() <> mt Then
                        objParm2.SetMapType(mt)
                    End If
                    If objParm2.Narrative <> objParm.Narrative Then
                        objParm2.Narrative = objParm.Narrative
                    End If
                    If objParm2.FriendlyName <> objParm.FriendlyName Then
                        objParm2.FriendlyName = objParm.FriendlyName
                    End If
                    bFound = True
                End If
            Next
            If Not bFound Then
                Dim CollectionInfo As clsCollectionInfo = Nothing
                If objParm.GetDataType = DataType.collection Then
                    CollectionInfo = objParm.CollectionInfo
                End If
                mProcessStage.AddParameter(objParm.Direction, objParm.GetDataType(), objParm.Name, objParm.Narrative, mt, "", CollectionInfo, objParm.FriendlyName)
            End If
        Next

    End Sub

    Private Sub btnDetails_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDetails.Click
        Dim cur As Cursor = Me.Cursor
        Try
            Me.Cursor = System.Windows.Forms.Cursors.WaitCursor
        Finally
            Me.Cursor = cur
        End Try
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesSkill.htm"
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
