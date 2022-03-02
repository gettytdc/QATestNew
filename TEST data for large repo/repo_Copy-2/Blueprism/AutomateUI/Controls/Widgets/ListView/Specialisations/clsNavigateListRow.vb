Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI

Friend Class clsNavigateListRow
    Inherits clsListRow

    ''' <summary>
    ''' The application definition associated with the
    ''' elements used in this control.
    ''' </summary>
    ''' <remarks></remarks>
    Private mAppDefinition As clsApplicationDefinition

    ''' <summary>
    ''' The stage used in scope resolution, etc, when editing
    ''' this row.
    ''' </summary>
    Private mobjProcessStage As clsNavigateStage

    Public Sub New(ByVal lv As ctlListView, ByVal objProcessStage As clsNavigateStage)
        MyBase.New(lv)
        Me.mobjProcessStage = objProcessStage
        Me.mAppDefinition = Me.mobjProcessStage.Process.ApplicationDefinition
        Me.SetNullValues()
    End Sub

    Private Sub SetNullValues()
        Me.Items.Clear()
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(""))) 'element id
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(""))) 'Parameters
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(""))) 'navigate action id
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(""))) 'Inputs set column
    End Sub

    Public Sub UpdateInputsStatus()
        Dim Status As String = My.Resources.clsNavigateListRow_NA

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
                        Status = My.Resources.clsNavigateListRow_Yes
                    Else
                        Status = My.Resources.clsNavigateListRow_No
                    End If
                End If
            End If
        End If

        Me.Items(3).Value = New clsProcessValue(DataType.text, Status)
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

            If Me.mNavigationAction IsNot Nothing Then
                Me.Items.Clear()
                Dim TargetElement As clsApplicationElement = Me.mAppDefinition.FindElement(Me.mNavigationAction.ElementId)
                Dim ElementName As String = My.Resources.clsNavigateListRow_UnknownElement
                If TargetElement IsNot Nothing Then
                    ElementName = TargetElement.Name
                End If
                Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ElementName)))  'element name
                Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'Parameters
                Dim ActionName As String = ""
                If Me.mNavigationAction.Action IsNot Nothing Then
                    ActionName = Me.mNavigationAction.ActionName
                End If
                Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ActionName)))   'navigate action id
                Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'Inputs set column
            Else
                Me.SetNullValues()
            End If

            Me.Items(0).HighlightOuterColour = AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightOuter
            Me.Items(0).HighlightInnerColour = AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightInner
        End Set
    End Property

    Public Overrides Sub BeginEdit(ByVal EditRow As ctlEditableListRow)
        Dim navRow As ctlNavigateListRow = CType(EditRow, ctlNavigateListRow)
        navRow.NavigationAction = Me.NavigationAction
        navRow.Populate(Me.NavigationAction)
    End Sub

    Public Overrides Sub EndEdit(ByVal EditRow As ctlEditableListRow)
        Me.NavigationAction = CType(EditRow, ctlNavigateListRow).NavigationAction
        MyBase.EndEdit(EditRow)
    End Sub

    Public Overrides Function CreateEditRow() As ctlEditableListRow
        Dim NavRow As New ctlNavigateListRow(Me.mobjProcessStage)
        NavRow.ApplicationExplorer = Me.mApplicationExplorer
        AddHandler NavRow.ActionChanged, AddressOf HandleActionChanged
        Return NavRow
    End Function


    Private Sub HandleActionChanged(ByVal NewAction As clsNavigateStep)
        RaiseEvent ActionChanged(NewAction)
    End Sub

    ''' <summary>
    ''' Event raised when the selected action in the row is changed
    ''' during editing.
    ''' </summary>
    ''' <param name="NewAction">The newly selected action.</param>
    Public Event ActionChanged(ByVal NewAction As clsNavigateStep)

    Public Overrides Sub OnDragOver( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)

        e.Effect = DragDropEffects.None
        Me.Items(0).Highlighted = False

        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim tag As Object = CType(e.Data.GetData(GetType(TreeNode)), TreeNode).Tag
            If colIndex = 0 AndAlso TypeOf Tag Is clsApplicationElement Then
                e.Effect = DragDropEffects.Move
                Items(0).Highlighted = True
            End If
        End If

        Me.Owner.InvalidateRow(Me)
    End Sub

    Public Overrides Sub OnDragDrop( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)

        If colIndex <> 0 Then Return
        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If n IsNot Nothing AndAlso TypeOf n.Tag Is clsApplicationElement Then
            OnElementChanged(DirectCast(n.Tag, clsApplicationElement))
            Owner.CurrentEditableRow = Me
        End If

    End Sub

    Private Sub OnElementChanged(ByVal NewElement As clsApplicationElement)
        'Remember the old navigation and create a new one
        Dim oldNavigation As clsNavigateStep = mNavigationAction
        mNavigationAction = New clsNavigateStep(mobjProcessStage) With {
            .ElementId = NewElement.ID
        }

        'If there is already an action configured, see if
        'it is still available for this new element
        If oldNavigation IsNot Nothing AndAlso oldNavigation.Action IsNot Nothing Then
            Dim keepCurrentAction = False
            For Each allowedAction As clsActionTypeInfo In clsAMI.GetAllowedActions(NewElement.Type, mAppDefinition.ApplicationInfo)
                If allowedAction.ID = oldNavigation.ActionId Then
                    keepCurrentAction = True
                End If
            Next

            If keepCurrentAction Then
                CopyPropertiesToCurrentNavigationAction(oldNavigation)
            End If
        End If
    End Sub

    Private Sub CopyPropertiesToCurrentNavigationAction(oldNavigation As clsNavigateStep)
        mNavigationAction.Action = oldNavigation.Action
        For Each val As KeyValuePair(Of String, String) In oldNavigation.ArgumentValues
            mNavigationAction.ArgumentValues(val.Key) = val.Value
        Next
        For Each val As KeyValuePair(Of String, String) In oldNavigation.OutputValues
            mNavigationAction.OutputValues(val.Key) = val.Value
        Next
        mNavigationAction.Parameters.AddRange(oldNavigation.Parameters)
    End Sub

End Class
