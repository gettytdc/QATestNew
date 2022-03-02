Imports AutomateControls.ComboBoxes
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Expressions
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Represents a row in the reader and writer properties forms listview
''' </summary>
''' <remarks></remarks>
Friend Class ctlReadWriteListRow
    Inherits ctlEditableListRow

    ''' <summary>
    ''' Determines whether to layout in read mode, if false then write mode.
    ''' </summary>
    ''' <remarks></remarks>
    Private mbRead As Boolean

    ''' <summary>
    ''' A reference to the process stage to which this row
    ''' relates.
    ''' </summary>
    ''' <remarks></remarks>
    Private mobjProcessStage As clsProcessStage

    ''' <summary>
    ''' Holds a reference to the application definition.
    ''' </summary>
    ''' <remarks></remarks>
    Private mAppDefinition As clsApplicationDefinition

    ''' <summary>
    ''' Holds a reference to the step that this row relates to
    ''' </summary>
    ''' <remarks></remarks>
    Private mReadWriteStep As clsStep

    ''' <summary>
    ''' Holds a reference to the current element
    ''' </summary>
    ''' <remarks></remarks>
    Private mobjElement As clsApplicationElement

    ''' <summary>
    ''' Holds a reference to the parameters button
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents mButtonWithTickField As ButtonWithTick

    ''' <summary>
    ''' Holds a reference to the textbox in which the element name is placed
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents mElementNameField As clsApplicationElementField

    ''' <summary>
    ''' Holds a reference to the list item in which the elementeditarea is nested
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents mElementItem As ctlEditableListItem

    ''' <summary>
    ''' The combo box from which the user chooses a read type,
    ''' (when in read stage mode).
    ''' </summary>
    Private WithEvents mReadTypeCombo As AutomateControls.ComboBoxes.MonoComboBox

    ''' <summary>
    ''' Container list item for the Read Type Combo Box.
    ''' </summary>
    Private mReadTypeItem As ctlEditableListItem

    ''' <summary>
    ''' Holds a reference to the richtextbox in which the storein text is placed
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents mStoreInEdit As ctlStoreInEdit

    ''' <summary>
    ''' Holds a referelce to the ExpressionEdit field
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents mExpressionEdit As ctlExpressionEdit

    ''' <summary>
    ''' Holds a reference to the typetextbox which the type of the element is placed
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents mTypeTextBox As AutomateControls.Textboxes.StyledTextBox


    ''' <summary>
    ''' Provides access to the step that this row relates to
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ReadWriteStep() As clsStep
        Get
            If mbRead Then
                Dim read As clsReadStep = TryCast(Me.mReadWriteStep, clsReadStep)
                If Not read Is Nothing Then
                    read.Stage = mStoreInEdit.Text
                    Dim SelectedItem As AutomateControls.ComboBoxes.MonoComboBoxItem = Me.mReadTypeCombo.SelectedItem
                    If SelectedItem IsNot Nothing Then
                        read.Action = CType(SelectedItem.Tag, clsActionTypeInfo)
                    Else
                        read.Action = Nothing
                    End If
                End If
            Else
                Dim write As clsWriteStep = TryCast(Me.mReadWriteStep, clsWriteStep)
                If write IsNot Nothing Then _
                 write.Expression = BPExpression.FromLocalised(mExpressionEdit.Text)

            End If
            Return mReadWriteStep
        End Get
        Set(ByVal value As clsStep)
            mReadWriteStep = value
            If not mbRead then
                 RaiseEvent ElementChanged(CType(mReadWriteStep, clsWriteStep))
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets the data type that should be enforced in the UI, according to the
    ''' choice of element (in mobjElement) and the choice of ReadAction (as determined)
    ''' by the readaction in the ReadWriteStep
    ''' </summary>
    ''' <remarks>Relevant only for read stage mode.</remarks>
    Private ReadOnly Property DataType() As DataType
        Get
            'Get ReadAction return type, if appropriate
            Dim retType As DataType = DataType.unknown
            If TypeOf mReadWriteStep Is clsReadStep Then
                Dim act As clsActionTypeInfo =
                 CType(mReadWriteStep, clsReadStep).Action

                If act IsNot Nothing Then
                    If act.ReturnType <> "" Then
                        retType = clsProcessDataTypes.Parse(act.ReturnType)
                    Else
                        Throw New BluePrismException(
                         My.Resources.ctlReadWriteListRow_MisconfiguredAction0EncounteredNoReturnTypeSetPleaseReportThisProblemToBluePrism,
                         act.ID)
                    End If
                End If
            End If

            If TypeOf mReadWriteStep Is clsWriteStep Then
                Return CType(mReadWriteStep, clsWriteStep).ActionDataType
            End If

            Return retType
        End Get
    End Property


    ''' <summary>
    ''' Constructs a new read or write row depending on the value of bRead
    ''' </summary>
    ''' <param name="bRead">Determines whether to create a read or write row</param>
    ''' <param name="objProcessStage">A stage from which the process, AMI instance
    ''' and application definition is inferred. The read/write row created
    ''' will be associated with these objects. The stage is also used to 
    ''' populate the treeview on the popup element params form.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal bRead As Boolean, ByVal objProcessStage As clsProcessStage, ByVal objProcessViewer As ctlProcessViewer)
        mbRead = bRead
        mobjProcessStage = objProcessStage
        mAppDefinition = objProcessStage.Process.ApplicationDefinition

        mElementNameField = New clsApplicationElementField
        AddHandler mElementNameField.SynchronisationRequsted, AddressOf HandleSynchRequest
        AddHandler mElementNameField.ElementChanged, AddressOf HandleElementChanged
        mElementNameField.ApplicationDefinition = Me.mAppDefinition
        mElementItem = New ctlEditableListItem(mElementNameField)
        mElementItem.AllowDrop = True
        Me.mElementItem.HighlightInnerColour = AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightInner
        Me.mElementItem.HighlightOuterColour = AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightOuter

        mButtonWithTickField = New ButtonWithTick
        mButtonWithTickField.ButtonEnabled = False
        Dim ParamItem As New ctlEditableListItem(mButtonWithTickField)

        mReadTypeCombo = New AutomateControls.ComboBoxes.MonoComboBox()
        mReadTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList
        mReadTypeCombo.Sorted = True
        mReadTypeItem = New ctlEditableListItem(mReadTypeCombo)

        mTypeTextBox = New AutomateControls.Textboxes.StyledTextBox
        mTypeTextBox.BorderStyle = BorderStyle.None
        mTypeTextBox.ReadOnly = True
        mTypeTextBox.BackColor = Color.FromKnownColor(KnownColor.ControlLightLight)
        Dim TypeItem As New ctlEditableListItem(mTypeTextBox)

        If bRead Then
            mStoreInEdit = New ctlStoreInEdit
            Dim StoreInItem As New ctlEditableListItem(mStoreInEdit)

            Me.Items.Add(mElementItem)
            Me.Items.Add(ParamItem)
            Me.Items.Add(mReadTypeItem)
            Me.Items.Add(TypeItem)
            Me.Items.Add(StoreInItem)
        Else
            mExpressionEdit = New ctlExpressionEdit
            mExpressionEdit.ProcessViewer = objProcessViewer
            mExpressionEdit.Stage = mobjProcessStage
            Dim ValueItem As New ctlEditableListItem(mExpressionEdit)

            Me.Items.Add(ValueItem)
            Me.Items.Add(mElementItem)
            Me.Items.Add(ParamItem)
            Me.Items.Add(TypeItem)   
        End If
    End Sub

    ''' <summary>
    ''' Private member to store public property Treeview()
    ''' </summary>
    Private mTreeview As ctlDataItemTreeView
    ''' <summary>
    ''' The treeview with which this row will be used.
    ''' When set, a callback can be made from the 'store
    ''' in' control to the treeview for a refresh.
    ''' </summary>
    Public WriteOnly Property Treeview() As ctlDataItemTreeView
        Set(ByVal value As ctlDataItemTreeView)
            mTreeview = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property ApplicationExplorer()
    ''' </summary>
    Private mApplicationExplorer As ctlApplicationExplorer
    ''' <summary>
    ''' The application explorer with which this row will be used,
    ''' if any. When set a callback will be made to the explorer
    ''' when the user uses the 'show in treeview' option
    ''' </summary>
    Public WriteOnly Property ApplicationExplorer() As ctlApplicationExplorer
        Set(ByVal value As ctlApplicationExplorer)
            mApplicationExplorer = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property Stage()
    ''' </summary>
    Private mStage As clsProcessStage
    ''' <summary>
    ''' The stage owning this read/write row. Should be a reader
    ''' or writer stage.
    ''' </summary>
    ''' <remarks>This object is used when creating new stages
    ''' from the "Store In" control.</remarks>
    Public Property Stage() As clsProcessStage
        Get
            Return mStage
        End Get
        Set(ByVal value As clsProcessStage)
            mStage = value
        End Set
    End Property


    ''' <summary>
    ''' Event raised when the user selects a new
    ''' action from the actions combobox.
    ''' </summary>
    Public Event ActionChanged(ByVal NewAction As clsReadStep)

    Private Sub mReadTypeCombo_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mReadTypeCombo.SelectedIndexChanged
        Dim SelectedItem As AutomateControls.ComboBoxes.MonoComboBoxItem = Me.mReadTypeCombo.SelectedItem
        If SelectedItem IsNot Nothing Then
            Dim Action As clsActionTypeInfo = CType(SelectedItem.Tag, clsActionTypeInfo)
            If Action IsNot Nothing Then
                CType(mReadWriteStep, clsReadStep).Action = Action
                Me.UpdateUIDataType()
            Else
                UserMessage.Show(My.Resources.ctlReadWriteListRow_InternalErrorNoActionTypeInformationHasBeenAssociatedWithTheSelectedItem)
            End If
            RaiseEvent ActionChanged(CType(mReadWriteStep, clsReadStep))
        End If
    End Sub

    Private Sub HandleSynchRequest(ByVal SenderElementID As Guid)
        If mApplicationExplorer IsNot Nothing Then
            mApplicationExplorer.SelectedMemberId = SenderElementID
        End If
    End Sub

    ''' <summary>
    ''' Populates the read types combo box, based on the type of
    ''' element selected.
    ''' </summary>
    ''' <param name="ActionID">The ID of the action to be selected,
    ''' if any</param>
    ''' <remarks>mobjElement must be set to a valid element before this method
    ''' is called.</remarks>
    Private Sub PopulateReadTypesCombo(Optional ByVal ActionID As String = "")
        If mobjElement IsNot Nothing Then
            If mobjElement.Type Is Nothing Then
                UserMessage.Show(My.Resources.ctlReadWriteListRow_ThisElementCanNotBeUsedBecauseItDoesNotHaveATypePleaseReturnToTheIntegrationAss)
                Exit Sub
            End If

            Try
                mReadTypeCombo.Items.Clear()
                Dim ItemToSelect As AutomateControls.ComboBoxes.MonoComboBoxItem = Nothing
                For Each Action As clsActionTypeInfo In clsAMI.GetAllowedReadActions(mobjElement.Type, Me.mAppDefinition.ApplicationInfo)
                    Dim NewItem As New MonoComboBoxItem(Action.GetLabel(mobjElement.Type), Action)
                    mReadTypeCombo.Items.Add(NewItem)

                    'Select the newly added item if this is the correct one
                    If Not String.IsNullOrEmpty(ActionID) Then
                        If Action.ID = ActionID Then
                            ItemToSelect = NewItem
                        End If
                    End If
                Next

                If ItemToSelect IsNot Nothing Then
                    Me.mReadTypeCombo.SelectedItem = ItemToSelect
                End If
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlReadWriteListRow_UnexpectedErrorWhilstPopulatingDataComboBox0, ex.Message))
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Handles the drag drop event of the element textbox.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ElementEditArea_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles mElementNameField.DragDrop, mElementItem.DragDrop
        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If Not n Is Nothing Then
            Dim TempElement As clsApplicationElement = TryCast(n.Tag, clsApplicationElement)
            If TempElement IsNot Nothing Then
                Me.OnElementChanged(TempElement)
                Me.mElementItem.IsHighlighted = False
                Me.mElementItem.Invalidate()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles the Dragenter event of the element textbox
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ElementEditArea_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles mElementNameField.DragEnter, mElementItem.DragEnter
        Dim EditableListItem As ctlEditableListItem = Me.mElementItem

        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            e.Effect = DragDropEffects.Move
            EditableListItem.IsHighlighted = True
        Else
            e.Effect = DragDropEffects.None
            EditableListItem.IsHighlighted = False
        End If

        EditableListItem.Invalidate()
    End Sub

    Private Sub mElementItem_DragLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles mElementNameField.DragLeave, mElementItem.DragLeave
        Me.mElementItem.IsHighlighted = False
        Me.mElementItem.Invalidate()
    End Sub

    Private Sub HandleElementChanged()
        Me.OnElementChanged(Me.mElementNameField.Element)
    End Sub

    Private Sub OnElementChanged(ByVal NewElement As clsApplicationElement)
        If (Not mbRead) AndAlso NewElement.Type.Readonly Then
            UserMessage.Show(My.Resources.ctlReadWriteListRow_ThisElementIsNotSuitableForWritingPleaseChooseAnother)
            Exit Sub
        End If

        mobjElement = NewElement
        Dim objStep As clsStep
        If mbRead Then
            objStep = New clsReadStep(DirectCast(mobjProcessStage, clsReadStage))
            objStep.ElementId = mobjElement.ID
            CType(objStep, clsReadStep).Stage = Me.mStoreInEdit.Text

            Me.mStoreInEdit.AutoCreateDefault = String.Format(My.Resources.ctlReadWriteListRow_ValueFrom0, mobjElement.Name)
        Else
            objStep = New clsWriteStep(DirectCast(mobjProcessStage, clsWriteStage))
            objStep.ElementId = mobjElement.ID
            CType(objStep, clsWriteStep).Expression =
             BPExpression.FromLocalised(mExpressionEdit.Text)
            Dim actions = clsAMI.GetAllowedWriteAction(mobjElement.Type)
            If objStep.Action Is Nothing AndAlso actions.Count >0
                objStep.Action = clsAMI.GetActionTypeInfo(actions.First().ID)
            End If
             RaiseEvent ElementChanged(CType(objStep, clsWriteStep))
        End If
        Me.Populate(objStep)
    End Sub

    Public Event ElementChanged(byVal newElement As clsWriteStep)

    ''' <summary>
    ''' Populates the row based on the step provided.
    ''' </summary>
    ''' <param name="NewStep">The new step to be populated.</param>
    Public Sub Populate(ByVal NewStep As clsStep)
        Me.ReadWriteStep = NewStep

        If NewStep IsNot Nothing Then
            mobjElement = Me.mAppDefinition.FindElement(NewStep.ElementId)

            If Not mobjElement Is Nothing Then
                'Populate element field
                RemoveHandler mElementNameField.ElementChanged, AddressOf HandleElementChanged
                Me.mElementNameField.Element = mobjElement
                AddHandler mElementNameField.ElementChanged, AddressOf HandleElementChanged

                Me.mButtonWithTickField.Checked = False
                Me.mButtonWithTickField.ButtonEnabled = ElementRequiresParameters(mobjElement)

                'Set the read action type
                If TypeOf NewStep Is clsReadStep Then
                    Dim ReadStep As clsReadStep = CType(NewStep, clsReadStep)

                    'Let the new readstep inherit the current read type
                    If mReadTypeCombo.SelectedItem IsNot Nothing Then
                        ReadStep.Action = CType(Me.mReadTypeCombo.SelectedItem.Tag, clsActionTypeInfo)
                    End If

                    'If the read type is invalid then the following method simply ignores it
                    Dim ActionID As String = String.Empty
                    If ReadStep.Action IsNot Nothing Then ActionID = ReadStep.ActionId
                    Me.PopulateReadTypesCombo(ActionID)

                    'Now discard it if appropriate
                    If Me.mReadTypeCombo.SelectedItem Is Nothing Then
                        ReadStep.Action = Nothing
                    End If

                    'If only one available action then select it automatically
                    If Me.mReadTypeCombo.Items.Count = 1 Then
                        Me.mReadTypeCombo.SelectedIndex = 0
                    End If
                End If

                UpdateUIDataType()

            Else
                Me.mElementNameField.Text = My.Resources.ctlReadWriteListRow_UnknownElement
            End If

            If mbRead Then
                Dim read As clsReadStep = TryCast(mReadWriteStep, clsReadStep)
                Me.mStoreInEdit.Text = read.Stage
            Else
                Dim write As clsWriteStep = TryCast(mReadWriteStep, clsWriteStep)
                Me.mExpressionEdit.Text = write.Expression.LocalForm
                Me.mExpressionEdit.ColourText()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Updates the data type in the UI.
    ''' </summary>
    Private Sub UpdateUIDataType()
        Try
            Me.mTypeTextBox.Text = clsProcessDataTypes.GetFriendlyName(Me.DataType)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlReadWriteListRow_ErrorCouldNotUpdateDataType0, ex.Message))
            Me.mTypeTextBox.Text = My.Resources.ctlReadWriteListRow_ERROR
        End Try
    End Sub

    ''' <summary>
    ''' Determines whether the supplied element requires
    ''' parameters to be set.
    ''' </summary>
    ''' <param name="Element">The Element under scrutiny.</param>
    ''' <returns>True if the element requires parameters.</returns>
    ''' <remarks></remarks>
    Private Function ElementRequiresParameters(ByVal Element As clsApplicationElement) As Boolean
        For Each a As clsApplicationAttribute In Element.Attributes
            If a.Dynamic AndAlso a.InUse Then Return True
        Next

        Return False
    End Function

    ''' <summary>
    ''' Handler for the parameters button click event
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mButtonWithTickField_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mButtonWithTickField.ButtonClicked
        If (Not Me.mReadWriteStep Is Nothing) AndAlso (Not Me.mReadWriteStep.ElementId.Equals(Guid.Empty)) Then
            Dim f As New frmElementParams
            f.SetEnvironmentColoursFromAncestor(Me)
            f.PopulateElement(Me.mAppDefinition.FindElement(mReadWriteStep.ElementId), Me.mReadWriteStep.Parameters, mobjProcessStage)
            f.ShowInTaskbar = False
            If f.ShowDialog = DialogResult.OK Then
                Me.mReadWriteStep.Parameters.Clear()
                Me.mReadWriteStep.Parameters.AddRange(f.GetParameters)
            End If
            f.Dispose()
        End If
    End Sub

    Private Sub mStoreInEdit_AutoCreateRequested(ByVal DataItemName As String) Handles mStoreInEdit.AutoCreateRequested
        'Info used the last time we auto-placed a stage
        Static LastStageAdded As clsProcessStage
        Static LastRelativePosition As clsProcessStagePositioner.RelativePositions
        Dim NewDataItem As clsDataStage = clsProcessStagePositioner.CreateDataItem(DataItemName, mobjProcessStage, Me.DataType, LastStageAdded, LastRelativePosition)

        If Not NewDataItem Is Nothing Then
            If Not Me.mTreeview Is Nothing Then
                Me.mTreeview.Repopulate(NewDataItem)
            End If
        End If
    End Sub

    Private Sub mStoreInEdit_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles mStoreInEdit.DragDrop
        Try
            If Not mbRead Then Return
            Dim n As TreeNode = DirectCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
            Dim fld As IDataField = TryCast(n.Tag, IDataField)

            If fld IsNot Nothing Then
                Dim dtype As DataType = Me.DataType

                If fld.DataType = dtype Then
                    mStoreInEdit.Text = fld.FullyQualifiedName
                Else
                    UserMessage.Show(String.Format(
                     My.Resources.ctlReadWriteListRow_TheRequiredDataTypeIs0ButTheDataTypeOf1Is2,
                     clsProcessDataTypes.GetFriendlyName(dtype),
                     fld.FullyQualifiedName,
                     clsProcessDataTypes.GetFriendlyName(fld.DataType))
                    )
                End If
            End If

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlReadWriteListRow_UnexpectedError0, ex.Message, ex))
        End Try
    End Sub


End Class
