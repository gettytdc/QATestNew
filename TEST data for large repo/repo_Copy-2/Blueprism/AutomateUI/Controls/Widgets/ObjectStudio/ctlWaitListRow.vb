Imports BluePrism.AutomateProcessCore
Imports BluePrism.AMI
Imports AutomateControls.ComboBoxes
Imports BluePrism.ApplicationManager.AMI

''' <summary>
''' This control is used to build the custom listview control seen in 
''' frmStagePropertiesWait, it allows the user to edit the properties of a 
''' wait step.
''' </summary>
Friend Class ctlWaitListRow
    Inherits ctlEditableListRow

    Public Event ConditionChanged()

    ''' <summary>
    ''' A reference to the parent process
    ''' </summary>
    Private mobjProcess As clsProcess

    ''' <summary>
    ''' Indicates whether the row is populating so suppress condition field 
    ''' updates
    ''' </summary>
    Private mbPopulating As Boolean

    ''' <summary>
    ''' Holds a reference to the wait start stage associated with the row
    ''' </summary>
    Private mobjWaitStart As Stages.clsWaitStartStage

    ''' <summary>
    ''' Holds a reference to the application definition.
    ''' </summary>
    Private mAppDefinition As clsApplicationDefinition

    ''' <summary>
    ''' Holds a reference to the step that this row relates to
    ''' </summary>
    Private mWaitChoice As clsWaitChoice

    ''' <summary>
    ''' Holds a reference to the parameters button
    ''' </summary>
    Private WithEvents mButtonWithTickField As ButtonWithTick

    ''' <summary>
    ''' Holds a reference to the textbox in which the element name is placed
    ''' </summary>
    Private WithEvents mElementNameField As clsApplicationElementField

    ''' <summary>
    ''' Holds a reference to the list item in which the elementeditarea is nested
    ''' </summary>
    Private WithEvents mElementItem As ctlEditableListItem

    ''' <summary>
    ''' Container for the comparison type combo drop-down.
    ''' </summary>
    Private WithEvents mComparisonTypeItem As ctlEditableListItem

    ''' <summary>
    ''' Combo box dropdown for the comparison types.
    ''' </summary>
    Private WithEvents mComparisonTypeDropDown As AutomateControls.ComboBoxes.MonoComboBox

    ''' <summary>
    ''' Holds a reference to the richtextbox in which the storein text is placed
    ''' </summary>
    Private WithEvents mValue As ctlExpressionEdit

    ''' <summary>
    ''' Holds a reference to the event combo box.
    ''' </summary>
    Private WithEvents mConditionField As AutomateControls.ComboBoxes.MonoComboBox

    ''' <summary>
    ''' Holds a reference to the typetextbox which the type of the element is placed
    ''' </summary>
    Private WithEvents mTypeTextBox As AutomateControls.Textboxes.StyledTextBox

    ''' <summary>
    ''' Provides access to the step that this row relates to
    ''' </summary>
    Public Property Choice() As clsWaitChoice
        Get
            UpdateWaitExpectedReply()
            Return mWaitChoice
        End Get
        Set(ByVal value As clsWaitChoice)
            mWaitChoice = value
        End Set
    End Property

    ''' <summary>
    ''' Constructs a new wait list row
    ''' </summary>
    Public Sub New(ByVal objWaitStart As Stages.clsWaitStartStage, ByVal objProcessViewer As ctlProcessViewer)
        mobjWaitStart = objWaitStart
        mobjProcess = objWaitStart.Process

        mAppDefinition = mobjProcess.ApplicationDefinition

        mValue = New ctlExpressionEdit
        mValue.Stage = objWaitStart
        mValue.ProcessViewer = objProcessViewer
        Dim StoreInItem As New ctlEditableListItem(mValue)

        mElementNameField = New clsApplicationElementField
        mElementNameField.ApplicationDefinition = Me.mAppDefinition
        AddHandler mElementNameField.SynchronisationRequsted, AddressOf HandleSynchRequest
        AddHandler mElementNameField.ElementChanged, AddressOf HandleElementChanged
        mElementItem = New ctlEditableListItem(mElementNameField)
        mElementItem.AllowDrop = True
        Me.mElementItem.HighlightInnerColour = AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightInner
        Me.mElementItem.HighlightOuterColour = AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightOuter

        mButtonWithTickField = New ButtonWithTick
        mButtonWithTickField.ButtonEnabled = False
        Dim ParamItem As New ctlEditableListItem(mButtonWithTickField)

        mTypeTextBox = New AutomateControls.Textboxes.StyledTextBox
        mTypeTextBox.BorderStyle = BorderStyle.None
        mTypeTextBox.ReadOnly = True
        mTypeTextBox.BackColor = Color.FromKnownColor(KnownColor.ControlLightLight)
        Dim DataTypeItem As New ctlEditableListItem(mTypeTextBox)

        mConditionField = New AutomateControls.ComboBoxes.MonoComboBox
        mConditionField.DropDownStyle = ComboBoxStyle.DropDownList
        mConditionField.Sorted = True
        mConditionField.Enabled = False
        Dim EventItem As New ctlEditableListItem(mConditionField)

        Me.mComparisonTypeDropDown = New AutomateControls.ComboBoxes.MonoComboBox
        Me.mComparisonTypeDropDown.DropDownStyle = ComboBoxStyle.DropDownList
        Me.mComparisonTypeDropDown.Sorted = True
        Dim ComparisonTypeItem As New ctlEditableListItem(Me.mComparisonTypeDropDown)

        Me.Items.Add(mElementItem)
        Me.Items.Add(ParamItem)
        Me.Items.Add(EventItem)
        Me.Items.Add(DataTypeItem)
        Me.Items.Add(ComparisonTypeItem)
        Me.Items.Add(StoreInItem)

    End Sub

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
    ''' Handles a sync request?
    ''' </summary>
    Private Sub HandleSynchRequest(ByVal SenderElementID As Guid)
        If mApplicationExplorer IsNot Nothing Then
            mApplicationExplorer.SelectedMemberId = SenderElementID
        End If
    End Sub

    ''' <summary>
    ''' Handles the drag drop event of the element textbox.
    ''' </summary>
    Private Sub ElementEditArea_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles mElementNameField.DragDrop, mElementItem.DragDrop
        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If Not n Is Nothing Then
            If TypeOf n.Tag Is clsApplicationElement Then
                Dim objElement As clsApplicationElement = TryCast(n.Tag, clsApplicationElement)
                If Not objElement Is Nothing Then
                    Me.OnElementChanged(objElement)
                    Me.mElementItem.IsHighlighted = False
                    Me.mElementItem.Invalidate()
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles the Dragenter event of the element textbox
    ''' </summary>
    Private Sub ElementEditArea_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles mElementNameField.DragEnter, mElementItem.DragEnter
        'Deny by default - only specific things get in
        e.Effect = DragDropEffects.None
        Me.mElementItem.IsHighlighted = False

        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim Node As TreeNode = CType(e.Data.GetData(GetType(TreeNode)), TreeNode)
            If TypeOf Node.Tag Is clsApplicationElement Then
                'We only allow elements - groupings (ie clsApplicationMembers) are irrelevant.
                e.Effect = DragDropEffects.Move
                Me.mElementItem.IsHighlighted = True
            End If
        End If

        Me.mElementItem.Invalidate()
    End Sub

    ''' <summary>
    ''' Handles the drag leave event
    ''' </summary>
    Private Sub mElementItem_DragLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles mElementNameField.DragLeave, mElementItem.DragLeave
        Me.mElementItem.IsHighlighted = False
        Me.mElementItem.Invalidate()
    End Sub

    ''' <summary>
    ''' Handles the element changing
    ''' </summary>
    Private Sub HandleElementChanged()
        Me.OnElementChanged(Me.mElementNameField.Element)
    End Sub

    ''' <summary>
    ''' Handles the element changing
    ''' </summary>
    Private Sub OnElementChanged(ByVal NewElement As clsApplicationElement)
        If Not Choice Is Nothing Then
            'This is the fancy stuff to remember old wait/choice details
            Dim oldWait As clsWaitChoice = TryCast(Me.Choice.Clone, clsWaitChoice)

            mWaitChoice.ElementID = NewElement.ID

            'If currently selected condition, then apply to new wait/choice
            If oldWait.Condition IsNot Nothing Then
                Dim KeepCondition As Boolean = False
                For Each AllowedCondition As clsConditionTypeInfo In clsAMI.GetAllowedConditions(NewElement.Type, Me.mAppDefinition.ApplicationInfo)
                    If AllowedCondition.ID = oldWait.Condition.ID Then
                        KeepCondition = True
                        Exit For
                    End If
                Next

                If KeepCondition Then
                    mWaitChoice.ExpectedReply = oldWait.ExpectedReply
                    mWaitChoice.Condition = oldWait.Condition
                    mWaitChoice.ComparisonType = oldWait.ComparisonType
                Else
                    mWaitChoice.Condition = Nothing
                End If
            End If
        Else
            'If me.choice is nothing then we are dragging a new element in
            mWaitChoice = New clsWaitChoice(mobjWaitStart)
            mWaitChoice.ElementID = NewElement.ID
            mWaitChoice.Condition = Nothing
        End If

        UpdateChoiceName()

        Me.Populate(mWaitChoice)
    End Sub

    ''' <summary>
    ''' Updates the choice name to match the element name a choice condition
    ''' </summary>
    Private Sub UpdateChoiceName()
        Dim e As clsApplicationElement = mAppDefinition.FindElement(Choice.ElementID)
        Choice.Name = e.Name & " " & If(Choice.Condition IsNot Nothing, Choice.Condition.Name, "")
    End Sub

    ''' <summary>
    ''' Populates the row based on the step provided.
    ''' </summary>
    Public Sub Populate(ByVal objWait As clsWaitChoice)
        mWaitChoice = objWait

        If objWait IsNot Nothing Then
            mbPopulating = True

            Dim objElement As clsApplicationElement = Me.mAppDefinition.FindElement(objWait.ElementID)

            If Not objElement Is Nothing Then
                'Populate element field
                RemoveHandler mElementNameField.ElementChanged, AddressOf HandleElementChanged
                Me.mElementNameField.Element = objElement
                AddHandler mElementNameField.ElementChanged, AddressOf HandleElementChanged

                Me.mButtonWithTickField.Checked = False

                Me.mButtonWithTickField.ButtonEnabled = ElementRequiresParameters(objElement)

                Me.PopulateConditions(objElement)

                Me.PopulateComparisonTypesDropDown(objWait.Condition)
                If objWait IsNot Nothing Then
                    Me.mComparisonTypeDropDown.SelectedText = clsAMI.GetComparisonTypeFriendlyName(objWait.ComparisonType)
                End If
            Else
                Me.mElementNameField.Text = My.Resources.ctlWaitListRow_UnknownElement
            End If

            Me.mValue.Text = objWait.ExpectedReply
            Me.mValue.ColourText()

            mbPopulating = False
        End If
    End Sub

    ''' <summary>
    ''' Populates the comparison types drop down with the 
    ''' types available to the supplied wait condition.
    ''' </summary>
    ''' <param name="Condition">The condition for which the
    ''' comparison types should be populated.</param>
    Private Sub PopulateComparisonTypesDropDown(ByVal Condition As clsConditionTypeInfo)
        Me.mComparisonTypeDropDown.Items.Clear()

        'Add each comparison type available to the chosen wait condition
        If Condition IsNot Nothing Then
            For Each ComparisonType As clsAMI.ComparisonTypes In clsWaitChoice.GetAllowedComparisonTypes(Condition)
                Me.mComparisonTypeDropDown.Items.Add(New AutomateControls.ComboBoxes.MonoComboBoxItem(clsAMI.GetComparisonTypeFriendlyName(ComparisonType), ComparisonType))
            Next
        End If
    End Sub

    ''' <summary>
    ''' Populates the combobox with a list of conditions available
    ''' to the supplied application element.
    ''' </summary>
    ''' <param name="elem">The application element to test.</param>
    Private Sub PopulateConditions(ByVal elem As clsApplicationElement)

        If elem.Type Is Nothing Then
            UserMessage.Show(My.Resources.ctlWaitListRow_ThisElementCanNotBeUsedBecauseItDoesNotHaveATypePleaseReturnToTheIntegrationAss)
            Exit Sub
        End If

        mConditionField.Items.Clear()
        For Each ct As clsConditionTypeInfo In
          clsAMI.GetAllowedConditions(elem.Type, mAppDefinition.ApplicationInfo)
            mConditionField.Items.Add(New MonoComboBoxItem(ct.Name) With {.Tag = ct})
        Next

        Dim cond As clsConditionTypeInfo = mWaitChoice.Condition
        If Not cond Is Nothing Then
            mConditionField.Text = cond.Name
            mTypeTextBox.Text = clsProcessDataTypes.GetFriendlyName(cond.DataType)
        End If

        mConditionField.Enabled = (mConditionField.Items.Count > 0)
    End Sub

    ''' <summary>
    ''' Determines whether the supplied element requires
    ''' parameters to be set.
    ''' </summary>
    ''' <param name="Element">The Element under scrutiny.</param>
    ''' <returns>True if the element requires parameters.</returns>
    Private Function ElementRequiresParameters(ByVal Element As clsApplicationElement) As Boolean
        For Each a As clsApplicationAttribute In Element.Attributes
            If a.Dynamic AndAlso a.InUse Then Return True
        Next

        Return False
    End Function

    ''' <summary>
    ''' Handler for the parameters button click event
    ''' </summary>
    Private Sub mButtonWithTickField_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mButtonWithTickField.ButtonClicked
        If (Not Me.mWaitChoice Is Nothing) AndAlso (Not Me.mWaitChoice.ElementID.Equals(Guid.Empty)) Then
            Dim f As New frmElementParams
            f.SetEnvironmentColoursFromAncestor(Me)
            f.PopulateElement(Me.mAppDefinition.FindElement(mWaitChoice.ElementID), Me.mWaitChoice.Parameters, Me.mobjWaitStart)
            f.ShowInTaskbar = False
            If f.ShowDialog = DialogResult.OK Then
                Me.mWaitChoice.Parameters.Clear()
                Me.mWaitChoice.Parameters.AddRange(f.GetParameters)
            End If
            f.Dispose()
        End If
    End Sub

    ''' <summary>
    ''' When the value loses focus set the expected reply text to the value of the expression text
    ''' </summary>
    Private Sub mValue_LostFocus(ByVal sender As Object, ByVal e As EventArgs) Handles mValue.LostFocus
        If mWaitChoice IsNot Nothing Then
            UpdateWaitExpectedReply()
        End If
    End Sub

    ''' <summary>
    ''' Updates the expected reply in the wait node.
    ''' </summary>
    Private Sub UpdateWaitExpectedReply()
        If Me.mWaitChoice IsNot Nothing Then
            mWaitChoice.ExpectedReply = mValue.Text
        End If
    End Sub

    ''' <summary>
    ''' When the condition is changed update the choices condition.
    ''' </summary>
    Private Sub mConditionField_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mConditionField.SelectedIndexChanged
        If Not Me.mbPopulating Then
            Dim item As AutomateControls.ComboBoxes.MonoComboBoxItem = Me.mConditionField.SelectedItem
            If Not item Is Nothing Then
                Dim cond As clsConditionTypeInfo = CType(item.Tag, clsConditionTypeInfo)
                Me.Choice.Condition = cond

                UpdateChoiceName()

                Me.mTypeTextBox.Text = clsProcessDataTypes.GetFriendlyName(cond.DataType)
                If mValue.Text = "" Then
                    Me.mValue.Text = cond.DefaultValue
                End If

                'Change the comparison type, if it is not appropriate to the new condition
                Dim currType As clsAMI.ComparisonTypes
                If Me.mComparisonTypeDropDown.SelectedItem IsNot Nothing Then
                    currType = CType(Me.mComparisonTypeDropDown.SelectedItem.Tag, clsAMI.ComparisonTypes)
                Else
                    currType = clsAMI.ComparisonTypes.Equal
                End If
                If Not clsWaitChoice.GetAllowedComparisonTypes(cond).Contains(currType) Then
                    currType = clsAMI.ComparisonTypes.Equal
                End If

                'Repopulate the comparison types dropdown, and select the desired option
                PopulateComparisonTypesDropDown(cond)
                mComparisonTypeDropDown.SelectedText = clsAMI.GetComparisonTypeFriendlyName(currType)
            End If

            RaiseEvent ConditionChanged()

        End If
    End Sub

    ''' <summary>
    ''' Handles the selected index changed event
    ''' </summary>
    Private Sub mComparisonTypeDropDown_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mComparisonTypeDropDown.SelectedIndexChanged
        If Me.mComparisonTypeDropDown.SelectedItem IsNot Nothing Then
            Dim Type As clsAMI.ComparisonTypes = CType(Me.mComparisonTypeDropDown.SelectedItem.Tag, clsAMI.ComparisonTypes)
            Me.mWaitChoice.ComparisonType = Type
        End If
    End Sub

End Class
