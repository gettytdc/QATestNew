
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI

Friend Class ctlNavigateListRow
    Inherits ctlEditableListRow


    ''' <summary>
    ''' The application definition associated with the
    ''' elements used in this control.
    ''' </summary>
    ''' <remarks></remarks>
    Private mAppDefinition As clsApplicationDefinition

    ''' <summary>
    ''' The textbox in which the element is displayed.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents mElementNameField As clsApplicationElementField

    ''' <summary>
    ''' The list item that holds the textbox in which the element is displayed.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents mElementItem As ctlEditableListItem

    ''' <summary>
    ''' The button for setting parameters on the element.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents mButtonWithTickField As ButtonWithTick
    ''' <summary>
    ''' The actions combobox, the contents of which depend
    ''' on the type of element.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents mActionField As AutomateControls.ComboBoxes.MonoComboBox
    ''' <summary>
    ''' Status field indicating whether the arguments on the current
    ''' action have been set.
    ''' </summary>
    ''' <remarks></remarks>
    Private mStatusField As TextBox

    ''' <summary>
    ''' Indicates whether the control is currently being
    ''' populated.
    ''' </summary>
    Private mbPopulating As Boolean

    Private mobjProcessStage As clsProcessStage


    ''' <summary>
    ''' Instanciates a new ctlNavigateListRow
    ''' </summary>
    ''' <param name="objProcessStage">A stage associated with the current
    ''' navigation row. This stage will be used to obtain a reference
    ''' to a process (and in turn an AMI instance); in addition, this
    ''' stage will be used as a scope stage in the data item treeview
    ''' appearing on the element parameters pop up form.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal objProcessStage As clsProcessStage)
        MyBase.New()
        mobjProcessStage = objProcessStage
        Me.mAppDefinition = objProcessStage.Process.ApplicationDefinition

        Me.mElementNameField = New clsApplicationElementField
        Me.mElementNameField.ApplicationDefinition = Me.mAppDefinition
        AddHandler mElementNameField.SynchronisationRequsted, AddressOf HandleSynchRequest
        AddHandler mElementNameField.ElementChanged, AddressOf HandleElementChanged

        Me.mButtonWithTickField = New ButtonWithTick
        Me.mButtonWithTickField.ButtonEnabled = False

        Me.mActionField = New AutomateControls.ComboBoxes.MonoComboBox
        Me.mActionField.DropDownStyle = ComboBoxStyle.DropDownList
        Me.mActionField.Sorted = True
        Me.mActionField.Enabled = False

        Me.mStatusField = New TextBox
        Me.mStatusField.ReadOnly = True
        Me.mStatusField.Text = My.Resources.ctlNavigateListRow_NA

        Me.mElementItem = New ctlEditableListItem(Me.mElementNameField)
        Me.mElementItem.AllowDrop = True
        Me.mElementItem.HighlightInnerColour = AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightInner
        Me.mElementItem.HighlightOuterColour = AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightOuter

        Me.Items.Add(Me.mElementItem)
        Me.Items.Add(New ctlEditableListItem(Me.mButtonWithTickField))
        Me.Items.Add(New ctlEditableListItem(Me.mActionField))
        Me.Items.Add(New ctlEditableListItem(Me.mStatusField))

    End Sub


    ''' <summary>
    ''' Populates the row with the supplied objects.
    ''' </summary>
    ''' <param name="Navigation">The navigation to take. The target element
    ''' must not be null, the other properties may be.</param>
    Public Sub Populate(ByVal Navigation As clsNavigateStep)
        Me.NavigationAction = Navigation

        If NavigationAction IsNot Nothing Then
            Me.mbPopulating = True

            Dim objElement As clsApplicationElement = Me.mAppDefinition.FindElement(Navigation.ElementID)
            If Not objElement Is Nothing Then
                'Populate element field
                RemoveHandler mElementNameField.ElementChanged, AddressOf HandleElementChanged
                Me.mElementNameField.Element = objElement
                AddHandler mElementNameField.ElementChanged, AddressOf HandleElementChanged

                Me.mButtonWithTickField.Checked = False
                Me.mButtonWithTickField.ButtonEnabled = ElementRequiresParameters(objElement)

                'Populate actions and argument status
                Me.PopulateActions(objElement)
                If Not Navigation.Action Is Nothing Then
                    Me.mActionField.Text = Navigation.ActionName
                    Me.UpdateInputsStatus()
                End If

                Try
                    RaiseEvent ActionChanged(Me.NavigationAction)
                Catch ex As Exception
                    'Do nothing
                End Try
            Else
                Me.mElementNameField.Text = My.Resources.ctlNavigateListRow_UnknownElement
            End If
            Me.mbPopulating = False
        End If
    End Sub


    ''' <summary>
    ''' Updates the status field of the row, indicating 
    ''' whether the inputs have been set.
    ''' </summary>
    Public Sub UpdateInputsStatus()
        Dim Status As String = My.Resources.ctlNavigateListRow_NA

        If Not Me.mNavigationAction Is Nothing Then
            If (Me.mNavigationAction.Action IsNot Nothing) Then
                If (Me.mNavigationAction.Action.Arguments.Count > 0) Then
                    Dim AllSet As Boolean = True
                    For Each sArgID As String In mNavigationAction.ArgumentValues.Keys
                        If mNavigationAction.ArgumentValues(sArgID) = "" Then
                            AllSet = False
                            Exit For
                        End If
                    Next

                    If AllSet Then
                        Status = My.Resources.ctlNavigateListRow_Yes
                    Else
                        Status = My.Resources.ctlNavigateListRow_No
                    End If
                End If
            End If
        End If

        Me.mStatusField.Text = Status
    End Sub


    ''' <summary>
    ''' Populates the combobox with a list of actions available
    ''' to the supplied application element.
    ''' </summary>
    ''' <param name="E">The application element to test.</param>
    ''' <returns>Returns the number of actions populated.</returns>
    Private Function PopulateActions(ByVal E As clsApplicationElement) As Integer

        If E.Type Is Nothing Then
            UserMessage.Show(My.Resources.ctlNavigateListRow_ThisElementCanNotBeUsedBecauseItDoesNotHaveATypePleaseReturnToTheIntegrationAss)
            Exit Function
        End If

        Dim c As List(Of clsActionTypeInfo) = clsAMI.GetAllowedActions(E.Type, Me.mAppDefinition.ApplicationInfo)

        Me.mActionField.Items.Clear()
        For Each a As clsActionTypeInfo In c
            Dim item As New AutomateControls.ComboBoxes.MonoComboBoxItem(a.Name)
            item.Tag = a
            Me.mActionField.Items.Add(item)
        Next

        Me.mActionField.Enabled = (Me.mActionField.Items.Count > 0)
        Return Me.mActionField.Items.Count
    End Function

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

    Private Sub HandleSynchRequest(ByVal SenderElementID As Guid)
        If mApplicationExplorer IsNot Nothing Then
            mApplicationExplorer.SelectedMemberId = SenderElementID
        End If
    End Sub

    Private Sub ElementEditArea_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles mElementNameField.DragDrop, mElementItem.DragDrop
        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If Not n Is Nothing Then
            Select Case True
                Case (TypeOf n.Tag Is clsApplicationElement)
                    Dim objElement As clsApplicationElement = TryCast(n.Tag, clsApplicationElement)
                    If Not objElement Is Nothing Then
                        Me.OnElementChanged(objElement)
                        Me.mElementItem.IsHighlighted = False
                        Me.mElementItem.Invalidate()
                    End If
                Case Else
                    'do nothing
            End Select
        End If
    End Sub

    Private Sub ElementEditArea_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles mElementNameField.DragEnter, mElementItem.DragDrop
        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim Tag As Object = CType(e.Data.GetData(GetType(TreeNode)), TreeNode).Tag
            Select Case True
                Case (TypeOf Tag Is clsApplicationElement)
                    e.Effect = DragDropEffects.Move
                    Me.mElementItem.IsHighlighted = True
                Case Else
                    e.Effect = DragDropEffects.None
                    Me.mElementItem.IsHighlighted = False
            End Select
        End If

        Me.mElementItem.Invalidate()
    End Sub

    Private Sub mElementItem_DragLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles mElementItem.DragLeave
        Me.mElementItem.IsHighlighted = False
        Me.mElementItem.Invalidate()
    End Sub

    Private Sub HandleElementChanged()
        Me.OnElementChanged(Me.mElementNameField.Element)
    End Sub

    Private Sub OnElementChanged(ByVal NewElement As clsApplicationElement)
        'Remember the old navigation and create a new one
        Dim OldNavigation As clsNavigateStep = Me.mNavigationAction
        Me.mNavigationAction = New clsNavigateStep(DirectCast(mobjProcessStage, clsNavigateStage))
        Me.mNavigationAction.ElementID = NewElement.ID

        'If there is already an action configured, see if
        'it is still available for this new element
        If OldNavigation IsNot Nothing Then
            If OldNavigation.Action IsNot Nothing Then
                Dim KeepCurrentAction As Boolean = False
                For Each AllowedAction As clsActionTypeInfo In clsAMI.GetAllowedActions(NewElement.Type, Me.mAppDefinition.ApplicationInfo)
                    If AllowedAction.ID = OldNavigation.ActionId Then
                        KeepCurrentAction = True
                    End If
                Next

                If KeepCurrentAction Then
                    mNavigationAction.Action = OldNavigation.Action
                    For Each val As KeyValuePair(Of String, String) In OldNavigation.ArgumentValues
                        mNavigationAction.ArgumentValues(val.Key) = val.Value
                    Next
                    mNavigationAction.Parameters.AddRange(OldNavigation.Parameters)
                End If
            End If
        End If

        Me.Populate(mNavigationAction)
        Me.mActionField.Enabled = True
    End Sub








    ''' <summary>
    ''' Private member to store public property NavigationAction()
    ''' </summary>
    Private mNavigationAction As clsNavigateStep
    ''' <summary>
    ''' The navigational action that this row represents.
    ''' </summary>
    Public Property NavigationAction() As clsNavigateStep
        Get
            Return mNavigationAction
        End Get
        Set(ByVal value As clsNavigateStep)
            mNavigationAction = value
        End Set
    End Property

    ''' <summary>
    ''' Event raised when the user selects a new
    ''' action from the actions combobox.
    ''' </summary>
    Public Event ActionChanged(ByVal NewAction As clsNavigateStep)

    Private Sub mActionField_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mActionField.SelectedIndexChanged
        If Not Me.mbPopulating Then
            If Not Me.mActionField.SelectedItem Is Nothing Then
                If Not Me.mNavigationAction Is Nothing Then
                    Me.mNavigationAction.Action = CType(Me.mActionField.SelectedItem.Tag, clsActionTypeInfo)
                End If
            End If
            Me.UpdateInputsStatus()
            RaiseEvent ActionChanged(Me.NavigationAction)
        End If
    End Sub

    Private Sub mButtonWithTickField_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mButtonWithTickField.ButtonClicked
        If (Not Me.mNavigationAction Is Nothing) AndAlso (Not Me.mNavigationAction.ElementID.Equals(Guid.Empty)) Then
            Dim f As New frmElementParams
            f.SetEnvironmentColoursFromAncestor(Me)
            f.PopulateElement(mAppDefinition.FindElement(Me.mNavigationAction.ElementId), Me.mNavigationAction.Parameters, Me.mobjProcessStage)
            f.ShowInTaskbar = False
            If f.ShowDialog = DialogResult.OK Then
                Me.mNavigationAction.Parameters.Clear()
                Me.mNavigationAction.Parameters.AddRange(f.GetParameters)
            End If
            f.Dispose()
        End If
    End Sub

End Class
