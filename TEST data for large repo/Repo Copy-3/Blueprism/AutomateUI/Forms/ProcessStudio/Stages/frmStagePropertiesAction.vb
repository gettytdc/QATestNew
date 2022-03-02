Imports AutomateControls
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Extensions
Imports AutomateControls.Forms

''' Project  : Automate
''' Class    : frmStageProperties
''' 
''' <summary>
''' A stage properties super-form.
''' </summary>
Friend Class frmStagePropertiesAction
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
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents cmbObject As ComboBox
    Friend WithEvents cmbAction As ComboBox
    Friend WithEvents btnDetails As ctlRolloverButton
    Friend WithEvents mInputsOutputsConditions As AutomateUI.ctlInputsOutputsConditions
    Friend WithEvents mDataItemTreeView As AutomateUI.ctlDataItemTreeView
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents filteredBusinessObjectsListBox As ListBox
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesAction))
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.filteredBusinessObjectsListBox = New ListBox()
        Me.mInputsOutputsConditions = New AutomateUI.ctlInputsOutputsConditions()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cmbObject = New ComboBox()
        Me.btnDetails = New AutomateUI.ctlRolloverButton()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmbAction = New ComboBox()
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
        Me.SplitContainer1.Panel1.Controls.Add(Me.mInputsOutputsConditions)
        Me.SplitContainer1.Panel1.Controls.Add(Me.Label3)
        Me.SplitContainer1.Panel1.Controls.Add(Me.cmbObject)
        Me.SplitContainer1.Panel1.Controls.Add(Me.btnDetails)
        Me.SplitContainer1.Panel1.Controls.Add(Me.Label4)
        Me.SplitContainer1.Panel1.Controls.Add(Me.cmbAction)
        Me.SplitContainer1.Panel1.Controls.Add(Me.filteredBusinessObjectsListBox)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.mDataItemTreeView)
        resources.ApplyResources(Me.SplitContainer1.Panel2, "SplitContainer1.Panel2")
        '
        'filteredBusinessObjectsListBox
        '
        resources.ApplyResources(Me.filteredBusinessObjectsListBox, "filteredBusinessObjectsListBox")
        Me.filteredBusinessObjectsListBox.DrawMode = DrawMode.OwnerDrawFixed
        Me.filteredBusinessObjectsListBox.FormattingEnabled = True
        Me.filteredBusinessObjectsListBox.Name = "filteredBusinessObjectsListBox"
        Me.filteredBusinessObjectsListBox.TabStop = False
        '
        'mInputsOutputsConditions
        '
        resources.ApplyResources(Me.mInputsOutputsConditions, "mInputsOutputsConditions")
        Me.mInputsOutputsConditions.Name = "mInputsOutputsConditions"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'cmbObject
        '
        resources.ApplyResources(Me.cmbObject, "cmbObject")
        Me.cmbObject.DrawMode = DrawMode.OwnerDrawFixed
        Me.cmbObject.DropDownWidth = 392
        Me.cmbObject.MaxDropDownItems = 16
        Me.cmbObject.Name = "cmbObject"
        '
        'btnDetails
        '
        resources.ApplyResources(Me.btnDetails, "btnDetails")
        Me.btnDetails.DefaultImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16
        Me.btnDetails.DisabledImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16_Disabled
        Me.btnDetails.Name = "btnDetails"
        Me.btnDetails.RolloverImage = Global.AutomateUI.My.Resources.ToolImages.Information_16x16_Hot
        Me.btnDetails.TooltipText = "Click to view documentation for the selected business object"
        Me.btnDetails.TooltipTitle = "View Documentation"

        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'cmbAction
        '
        resources.ApplyResources(Me.cmbAction, "cmbAction")
        Me.cmbAction.BackColor = SystemColors.ControlLightLight
        Me.cmbAction.DrawMode = DrawMode.OwnerDrawFixed
        Me.cmbAction.DropDownStyle = ComboBoxStyle.DropDownList
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
        'frmStagePropertiesAction
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmStagePropertiesAction"
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

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        Me.btnDetails.TooltipText = My.Resources.ClickToViewDocumentationForTheSelectedBusinessObject
        Me.btnDetails.TooltipTitle = My.Resources.ViewDocumentation
        Me.AcceptButton = Nothing
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

    Private mHasDefaultName As Boolean = False
    Private businessObjects As New List(Of GroupedComboBoxItem)
    Private businessObjectId As Integer = 1
    Private filteredBusinessObjects As New clsSet(Of GroupedComboBoxItem)
    Private noneGroupedComboBoxItem As GroupedComboBoxItem
    Private noActionGroupedComboBoxItem As GroupedComboBoxItem = New GroupedComboBoxItem(My.Resources.NoActions, False)

#End Region

    Private ReadOnly Property ActionStage As clsActionStage
        Get
            Return CType(mProcessStage, clsActionStage)
        End Get
    End Property

    Private ReadOnly Property SelectedObject As clsBusinessObject
        Get
            Dim objItem = TryCast(cmbObject.SelectedItem, GroupedComboBoxItem)
            If objItem Is Nothing Then Return Nothing
            Return TryCast(objItem.Tag, clsBusinessObject)
        End Get
    End Property

    Private ReadOnly Property SelectedAction As clsBusinessObjectAction
        Get
            Dim actItem = TryCast(cmbAction.SelectedItem, GroupedComboBoxItem)
            If actItem Is Nothing Then Return Nothing
            Return TryCast(actItem.Tag, clsBusinessObjectAction)
        End Get
    End Property

    Public ReadOnly Property Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return ProcessStage
        End Get
    End Property

    ''' <summary>
    ''' Performs any population of the user interface based on the process stage set
    ''' in the form.
    ''' </summary>
    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        'disable the info button beside the business object dropdown
        btnDetails.Enabled = False
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


        'Bug #4337 - Make sure UpdateActions are done after the treeview has been set.
        'UpdateActions implicitly invokes cmbAction_SelectedIndexChanged
        'which in turn calls mInputsOutputsConditions.RefreshControls.
        'RefreshControls must be called after the treeview is set.
        RemoveHandler cmbObject.SelectedIndexChanged, AddressOf cmbObject_SelectedIndexChanged
        RemoveHandler cmbAction.SelectedIndexChanged, AddressOf cmbAction_SelectedIndexChanged

        UpdateObjectsList()
        UpdateActions(False)
        UpdateActionDetails()
        UpdateMissing()

        AddHandler cmbAction.SelectedIndexChanged, AddressOf Me.cmbAction_SelectedIndexChanged
        AddHandler cmbObject.SelectedIndexChanged, AddressOf cmbObject_SelectedIndexChanged

        chkLogParametersInhibit.Checked = Not mProcessStage.LogParameters

        mHasDefaultName = IsDefaultName(txtName.Text)
    End Sub

    Private Function IsDefaultName(currentName As String) As Boolean
        Dim defaultName = String.Equals(mProcessStage.InitialName, currentName)
        If Not defaultName AndAlso (SelectedObject IsNot Nothing AndAlso SelectedAction IsNot Nothing) Then
            defaultName = String.Equals(currentName, String.Format("{0}::{1}", SelectedObject.FriendlyName, SelectedAction.FriendlyName))
        End If
        Return defaultName
    End Function

    Private Sub UpdateMissing()
        Dim invalidObjectWarning As String = My.Resources.InvalidObject0
        Dim missingActionWarning As String = My.Resources.MissingAction0

        ' See acompanying note in UpdateObjectsList. If the object exists but no item is selected
        ' SelectedComboBoxItem will be nothing which can only mean we don't have permission to
        ' execute the object.
        If TryCast(cmbObject.SelectedItem, GroupedComboBoxItem) Is Nothing Then
            invalidObjectWarning = My.Resources.NoPermission0
            missingActionWarning = My.Resources.NoPermission0
        End If

        Dim sObject As String = Nothing
        Dim sAction As String = Nothing
        ActionStage.GetResource(sObject, sAction)

        If SelectedObject Is Nothing AndAlso Not String.IsNullOrEmpty(sObject) Then
            SetMissing(cmbObject, invalidObjectWarning, True, sObject)
        End If
        If SelectedAction Is Nothing AndAlso Not String.IsNullOrEmpty(sAction) Then
            SetMissing(cmbAction, missingActionWarning, False, sAction)
        End If
    End Sub

    Private Sub SelectComboBoxItemByTag(comboBox As ComboBox, businessObject As Object)
        For Each element As Object In comboBox.Items
            Dim item = CType(element, GroupedComboBoxItem)
            If item IsNot Nothing AndAlso Equals(item.Tag, businessObject) Then
                comboBox.SelectedItem = item
                comboBox.Invalidate()
                Exit For
            End If
        Next
    End Sub

    Private Sub SetMissing(cmb As ComboBox, s As String, isObject As Boolean, ParamArray args() As String)

        Dim invalidItem As New GroupedComboBoxItem(String.Format(s, args), False) With {
            .Tag = Nothing,
            .Color = Color.Red
        }

        cmb.Sorted = False

        If isObject Then
            cmb.DataSource = Nothing
            businessObjects.Insert(0, invalidItem)
            cmb.DataSource = businessObjects
        Else
            cmb.Items.Insert(0, invalidItem)
        End If

        cmb.SelectedItem = invalidItem

    End Sub



    ''' <summary>
    ''' Populates the combo box with a list of available business objects from
    ''' the mobjObjRefs member.
    ''' </summary>
    Private Sub UpdateObjectsList()

        'populate the combobox
        cmbObject.DataSource = Nothing
        noneGroupedComboBoxItem = New GroupedComboBoxItem(My.Resources.None, True) With {.Id = businessObjectId}
        businessObjects.Add(noneGroupedComboBoxItem)

        For Each obr In mObjectRefs.Children
            DescendChildren(obr, 0, True)
        Next

        cmbObject.DataSource = businessObjects

        Dim stg = ActionStage
        Dim obj = mObjectRefs.FindObjectReference(stg.ObjectName)
        If obj IsNot Nothing Then
            ' If the object exists but we end up not being able to select it in the combobox item
            ' then it must be present but not available to us, which can only mean that we don't
            ' have permission to execute the object.
            SelectComboBoxItemByTag(cmbObject, obj)
        Else
            If cmbObject.Items.Count > 0 Then cmbObject.SelectedIndex = 0
        End If

    End Sub



    Private Sub DescendChildren(obj As clsBusinessObject, indent As Integer, enabled As Boolean, Optional groupedComboBoxItem As GroupedComboBoxItem = Nothing)

        Const increment = 16
        businessObjectId += 1

        If Not obj.Valid Then Return

        Dim name As String = obj.FriendlyName

        Dim item As New GroupedComboBoxItem(name, obj)
        If Not enabled Then
            item.Tag = Nothing
            item.Enabled = False
        End If
        item.Indent = indent
        item.Id = businessObjectId
        item.Group = groupedComboBoxItem
        businessObjects.Add(item)

        If groupedComboBoxItem IsNot Nothing Then
            groupedComboBoxItem.Items.Add(item)
        End If

        Dim group = TryCast(obj, clsGroupBusinessObject)
        If group IsNot Nothing Then
            Dim details = TryCast(group.Details, clsGroupObjectDetails)
            If details IsNot Nothing AndAlso Not details.Permissions.HasPermission(
                User.Current, Permission.ObjectStudio.ExecuteBusinessObject) Then
                enabled = False
            End If

            item.Text = CStr(IIf(name.Equals("Default"), My.Resources.GroupMemberComboBox_AddEntry_Default, name))
            item.Style = FontStyle.Bold
            item.Tag = Nothing
            item.IsGroup = True

            For Each childObj In group.Children
                DescendChildren(childObj, indent + increment, True, item)
            Next

        End If

    End Sub

    ''' <summary>
    ''' Extends the validation performed in the base class.
    ''' </summary>
    ''' <returns>Returns true if validation successful, false otherwise.</returns>
    Protected Overrides Function ApplyChanges() As Boolean

        For Each param In mProcessStage.GetParameters()
            Dim result = param.ValidateParameter()
            If Not result.Item1 Then
                Dim dialogResult = BPMessageBox.ShowDialog(String.Format(My.Resources.frmStagePropertiesAction_ValidationFailedFor, param.FriendlyName, result.Item2), My.Resources.frmStagePropertiesActionDialog_ValidationWarning, MessageBoxButtons.OKCancel)
                If dialogResult = DialogResult.Cancel Then
                    Return False
                End If
            End If
        Next
        If Not MyBase.ApplyChanges() Then Return False

        'Set the parameters
        mProcessStage.ClearParameters()
        mProcessStage.AddParameters(mInputsOutputsConditions.GetInputParameters)
        mProcessStage.AddParameters(mInputsOutputsConditions.GetOutputParameters)

        'Save the changes...
        mProcessStage.LogParameters = Not chkLogParametersInhibit.Checked

        If TryCast(cmbObject.SelectedItem, GroupedComboBoxItem) Is noneGroupedComboBoxItem Then
            ActionStage.SetResource(String.Empty, String.Empty)
            Return True
        End If

        Dim obj = SelectedObject
        Dim act = SelectedAction

        If obj IsNot Nothing AndAlso act IsNot Nothing Then
            ActionStage.SetResource(obj.Name, act.GetName)
            Return True
        End If

        Return True
    End Function

    Private Sub SetActionName()
        If mHasDefaultName AndAlso (SelectedObject IsNot Nothing AndAlso SelectedAction IsNot Nothing) Then
            Me.txtName.Text = String.Format("{0}::{1}", SelectedObject.FriendlyName, SelectedAction.FriendlyName)
        End If
    End Sub

    Private Sub NameChanged() Handles txtName.TextChanged
        mHasDefaultName = IsDefaultName(txtName.Text)
    End Sub

    Private Sub cmbObject_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbObject.SelectedIndexChanged
        filteredBusinessObjectsListBox.Visible = False
        UpdateActions()
    End Sub

    ''' <summary>
    ''' Populates the actions combobox with a list of available actions based on the
    ''' selection of object in the objects combobox.
    ''' </summary>
    Private Sub UpdateActions(Optional selectFirst As Boolean = True)

        cmbAction.Items.Clear()
        Dim obr = SelectedObject

        If obr Is Nothing Then
            cmbAction.Enabled = False
            Me.btnDetails.Enabled = False
            Me.btnDetails.BackColor = Me.BackColor
            mProcessStage.ClearParameters()
        Else
            Me.btnDetails.Enabled = True
            Me.btnDetails.BackColor = Me.BackColor

            If obr.GetActions().Any Then
                cmbAction.Enabled = IsEditable

                For Each objAct In obr.GetActions()
                    cmbAction.Items.Add(New GroupedComboBoxItem(objAct.FriendlyName, objAct))
                Next
            Else
                cmbAction.Items.Add(noActionGroupedComboBoxItem)
                cmbAction.Enabled = False
                mProcessStage.ClearParameters()
            End If

            Dim stg = ActionStage
            Dim act = obr.GetAction(stg.ActionName)
            If act IsNot Nothing Then
                SelectComboBoxItemByTag(cmbAction, act)
            Else
                If selectFirst AndAlso cmbAction.Items.Count > 0 Then cmbAction.SelectedIndex = 0
            End If
        End If
        Me.mInputsOutputsConditions.RefreshControls(mProcessStage.GetInputs, mProcessStage.GetOutputs)
    End Sub

    Private Sub cmbAction_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbAction.SelectedIndexChanged
        UpdateActionDetails()
        If SelectedAction IsNot Nothing Then ProcessStage.LogInhibit = SelectedAction.DefaultLoggingInhibitMode
        UpdateStageLogging()
    End Sub

    Private Sub UpdateActionDetails()
        SetActionName()
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
        Dim obj = SelectedObject
        If obj IsNot Nothing AndAlso SelectedAction IsNot Nothing Then
            Dim action As clsBusinessObjectAction = obj.GetAction(SelectedAction.GetName)
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

        Dim stg As clsActionStage = ActionStage

        'Get the object reference...
        Dim objR = SelectedObject
        If objR Is Nothing Then
            mProcessStage.ClearParameters()
            Exit Sub
        End If

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
                mProcessStage.AddParameter(objParm.Direction, objParm.GetDataType(), objParm.Name, objParm.Narrative, mt, "", objParm.Validator, CollectionInfo, objParm.FriendlyName)
            End If
        Next

    End Sub

    Private Sub btnDetails_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDetails.Click
        Dim cur As Cursor = Me.Cursor
        Try
            Me.Cursor = System.Windows.Forms.Cursors.WaitCursor

            Dim bo = SelectedObject
            If bo Is Nothing Then Return

            clsBOD.OpenAPIDocumentation(bo)
        Finally
            Me.Cursor = cur
        End Try
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStageProperties.htm"
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

    Private Sub cmbObject_DrawItem(sender As Object, e As DrawItemEventArgs) Handles cmbObject.DrawItem

        If e.Index >= 0 AndAlso e.Index < cmbObject.Items.Count Then
            e.DrawBackground()
            Dim comboBoxItem = TryCast(cmbObject.Items(e.Index), GroupedComboBoxItem)

            Dim point = e.Bounds.Location
            point.Offset(comboBoxItem.Indent, 0)

            e.Graphics.FillRectangle(If(e.State.HasFlag(DrawItemState.Selected), SystemBrushes.Highlight, New SolidBrush(e.BackColor)), e.Bounds)
            e.Graphics.DrawString(comboBoxItem.Text, New Font(e.Font, comboBoxItem.Style), New SolidBrush(e.ForeColor), point)
        End If

    End Sub

    Private Sub cmbObject_KeyDown(sender As Object, e As KeyEventArgs) Handles cmbObject.KeyDown

        If filteredBusinessObjects.Any Then

            If e.KeyCode = Keys.Down Then
                e.Handled = True
                cmbObject.DroppedDown = False
                filteredBusinessObjectsListBox.Focus()
                Return
            End If

            If e.KeyCode = Keys.Enter Then
                Dim firstBusinessObject = filteredBusinessObjects.First(Function(x) Not x.IsGroup)
                cmbObject.SelectedItem = firstBusinessObject
                cmbObject.SelectionStart = cmbObject.Text.Length
                cmbObject.SelectionLength = 0
                filteredBusinessObjectsListBox.Visible = False
                e.SuppressKeyPress = True
            End If

        End If

    End Sub

    Private Sub cmbObject_PreviewKeyDown(sender As Object, e As PreviewKeyDownEventArgs) Handles cmbObject.PreviewKeyDown
        If e.KeyCode = Keys.Enter Then e.IsInputKey = True
    End Sub

    Private Sub cmbObject_Leave(sender As Object, e As EventArgs) Handles cmbObject.Leave
        If cmbObject.SelectedItem Is Nothing AndAlso cmbObject.SelectedIndex = -1 Then UpdateActions()
    End Sub

    Private Sub cmbObject_TextUpdate(sender As Object, e As EventArgs) Handles cmbObject.TextUpdate

        If SelectedAction IsNot Nothing Then
            cmbAction.SelectedItem = Nothing
            cmbAction.Enabled = False
        End If

        If cmbObject.Text.Trim().Length > 0 Then

            FilterBusinessObjects()
            SetListBoxSize()
            filteredBusinessObjectsListBox.Visible = True
            filteredBusinessObjectsListBox.BringToFront()
        Else
            filteredBusinessObjectsListBox.Visible = False
        End If

    End Sub

    Private Sub FilterBusinessObjects()

        filteredBusinessObjects.Clear()

        For Each businessObject In businessObjects.Where(Function(x) x.Text.Contains(cmbObject.Text.Trim(), StringComparison.CurrentCultureIgnoreCase))
            Dim currentObject = businessObject
            If filteredBusinessObjects.Contains(currentObject) Then Continue For

            If currentObject.IsGroup AndAlso currentObject.Items.Any() Then
                AddGroupItems(currentObject)
            End If

            Do
                filteredBusinessObjects.Add(currentObject)
                currentObject = currentObject.Group
            Loop While currentObject IsNot Nothing
        Next

        Dim orderedFilter = filteredBusinessObjects.OrderBy(Function(x) x.Id).ToList()
        filteredBusinessObjectsListBox.DataSource = orderedFilter
        filteredBusinessObjectsListBox.TopIndex = 0

    End Sub

    Private Sub AddGroupItems(group As GroupedComboBoxItem)
        For Each item In group.Items
            filteredBusinessObjects.Add(item)
            AddGroupItems(item)
        Next
    End Sub

    Private Sub SetListBoxSize()

        Const bottomPadding = 8

        If filteredBusinessObjectsListBox.Items.Count() = 0 Then
            filteredBusinessObjectsListBox.Height = cmbObject.ItemHeight
        ElseIf filteredBusinessObjectsListBox.Items.Count > cmbObject.MaxDropDownItems Then
            filteredBusinessObjectsListBox.Height = (filteredBusinessObjectsListBox.ItemHeight * cmbObject.MaxDropDownItems) + bottomPadding
        Else
            filteredBusinessObjectsListBox.Height = (filteredBusinessObjectsListBox.ItemHeight * filteredBusinessObjectsListBox.Items.Count) + bottomPadding
        End If

    End Sub

    Private Sub cmbObject_DropDown(sender As Object, e As EventArgs) Handles cmbObject.DropDown
        filteredBusinessObjectsListBox.Visible = False
    End Sub

    Private Sub cmbObject_LostFocus(sender As Object, e As EventArgs) Handles cmbObject.LostFocus
        If Not filteredBusinessObjectsListBox.Focused() Then filteredBusinessObjectsListBox.Visible = False
    End Sub

    Private Sub cmbAction_DrawItem(sender As Object, e As DrawItemEventArgs) Handles cmbAction.DrawItem

        If e.Index >= 0 AndAlso e.Index < cmbAction.Items.Count Then
            e.DrawBackground()
            e.Graphics.FillRectangle(If(e.State.HasFlag(DrawItemState.Selected), SystemBrushes.Highlight, New SolidBrush(e.BackColor)), e.Bounds)

            Dim comboBoxItem = TryCast(cmbAction.Items(e.Index), GroupedComboBoxItem)
            e.Graphics.DrawString(comboBoxItem.Text, e.Font, New SolidBrush(e.ForeColor), e.Bounds.Location)
        End If

    End Sub

    Private Sub filteredBusinessObjectsListBox_DrawItem(sender As Object, e As DrawItemEventArgs) Handles filteredBusinessObjectsListBox.DrawItem

        If e.Index >= 0 AndAlso e.Index < filteredBusinessObjectsListBox.Items.Count Then
            e.DrawBackground()
            Dim listBoxItem = TryCast(filteredBusinessObjectsListBox.Items(e.Index), GroupedComboBoxItem)

            Dim point = e.Bounds.Location
            point.Offset(listBoxItem.Indent, 0)

            e.Graphics.FillRectangle(If(e.State.HasFlag(DrawItemState.Selected), SystemBrushes.Highlight, New SolidBrush(e.BackColor)), e.Bounds)
            e.Graphics.DrawString(listBoxItem.Text, New Font(e.Font, listBoxItem.Style), New SolidBrush(e.ForeColor), point)
        End If

    End Sub

    Private Sub filteredBusinessObjectsListBox_KeyDown(sender As Object, e As KeyEventArgs) Handles filteredBusinessObjectsListBox.KeyDown

        If e.KeyCode = Keys.Up AndAlso filteredBusinessObjectsListBox.SelectedIndex = 0 Then
            cmbObject.Focus()
            Return
        End If

        If e.KeyCode = Keys.Up AndAlso filteredBusinessObjectsListBox.SelectedIndex + 1 <> filteredBusinessObjectsListBox.Items.Count() Then
            filteredBusinessObjectsListBox.SelectedIndex = filteredBusinessObjectsListBox.SelectedIndex
            Return
        End If

        If e.KeyCode = Keys.Down AndAlso filteredBusinessObjectsListBox.SelectedIndex + 1 <> filteredBusinessObjectsListBox.Items.Count() Then
            filteredBusinessObjectsListBox.SelectedIndex = filteredBusinessObjectsListBox.SelectedIndex
        End If

    End Sub

    Private Sub filteredBusinessObjectsListBox_PreviewKeyDown(sender As Object, e As PreviewKeyDownEventArgs) Handles filteredBusinessObjectsListBox.PreviewKeyDown

        If e.KeyCode = Keys.Enter Then
            Dim selectedBusinessObject = TryCast(filteredBusinessObjectsListBox.SelectedItem, GroupedComboBoxItem)
            SelectFilteredBusinessObject(selectedBusinessObject)
        End If

    End Sub

    Private Sub filteredBusinessObjectsListBox_MouseDown(sender As Object, e As MouseEventArgs) Handles filteredBusinessObjectsListBox.MouseDown

        If filteredBusinessObjects.Any Then
            Dim selectedBusinessObject = TryCast(TryCast(sender, ListBox).SelectedItem, GroupedComboBoxItem)
            SelectFilteredBusinessObject(selectedBusinessObject)
        End If

    End Sub

    Private Sub SelectFilteredBusinessObject(selectedBusinessObject As GroupedComboBoxItem)

        If Not selectedBusinessObject.IsGroup Then
            cmbObject.SelectedItem = selectedBusinessObject
            filteredBusinessObjectsListBox.Visible = False
        End If

    End Sub

    Public Sub Repopulate(displayStage As clsDataStage) Implements IDataItemTreeRefresher.Repopulate
        mDataItemTreeView.Repopulate(displayStage)
    End Sub

    Public Sub Remove(stage As clsDataStage) Implements IDataItemTreeRefresher.Remove
        mDataItemTreeView.RemoveDataItemTreeNode(stage)
    End Sub
End Class
