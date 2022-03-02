Imports BluePrism.AutomateProcessCore
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI

Friend Class clsWaitListRow
    Inherits clsListRow

    Public Event ConditionChanged(ByVal waitChoice As clsWaitChoice)

    ''' <summary>
    ''' Holds a reference to the step that this row relates to
    ''' </summary>
    ''' <remarks></remarks>
    Private mWaitChoice As clsWaitChoice

    ''' <summary>
    ''' Holds a reference to the wait start stage associated with the row
    ''' </summary>
    ''' <remarks></remarks>
    Private mobjWaitStart As Stages.clsWaitStartStage

    ''' <summary>
    ''' Holds a reference to the Process viewer
    ''' </summary>
    Private mobjProcessViewer As ctlProcessViewer


    Public Sub New(ByVal lv As ctlListView, ByVal objWaitStart As Stages.clsWaitStartStage, _
     ByVal objProcessViewer As ctlProcessViewer)
        MyBase.New(lv)
        mobjWaitStart = objWaitStart
        mobjProcessViewer = objProcessViewer

        Me.SetNullValues()
        Me.Items(0).HighlightOuterColour = AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightOuter
        Me.Items(0).HighlightInnerColour = AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightInner
        Me.Items(5).HighlightOuterColour = AutomateControls.ColourScheme.Default.ListViewExpressionDataDragDropHighlightOuter
        Me.Items(5).HighlightInnerColour = AutomateControls.ColourScheme.Default.ListViewExpressionDataDragDropHighlightInner
    End Sub

    Private Sub SetNullValues()
        Me.Items.Clear()
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'elementid
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'Parameters
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'condition
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'type
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'comparison type
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'expected reply
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
    ''' Provides access to the step that this row relates to
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property WaitChoice() As clsWaitChoice
        Get
            If Me.mWaitChoice Is Nothing Then
                Me.mWaitChoice = New clsWaitChoice(Me.mobjWaitStart)
            End If
            Return mWaitChoice
        End Get
        Set(ByVal value As clsWaitChoice)
            mWaitChoice = value

            Me.Items.Clear()
            If mWaitChoice IsNot Nothing Then
                Dim TargetElement As clsApplicationElement = Me.mobjWaitStart.Process.ApplicationDefinition.FindElement(Me.mWaitChoice.ElementID)
                Dim ElementName As String = "<Unknown Element>"
                If TargetElement IsNot Nothing Then
                    ElementName = TargetElement.Name
                End If
                Me.Items.Add(New clsListItem(Me, ElementName))
                Me.Items.Add(New clsListItem(Me, "")) 'Parameters
                Dim ConditionName As String = ""
                Dim dtype As DataType = DataType.unknown
                If Me.WaitChoice.Condition IsNot Nothing Then
                    ConditionName = Me.WaitChoice.Condition.Name
                    dtype = clsProcessDataTypes.DataTypeId(mWaitChoice.Condition.DataType)
                End If
                Me.Items.Add(New clsListItem(Me, ConditionName))
                Me.Items.Add(New clsListItem(Me, clsProcessDataTypes.GetFriendlyName(dtype)))
                Me.Items.Add(New clsListItem(Me, mWaitChoice.ComparisonType.ToString))
                Me.Items.Add(New clsListItem(Me, mWaitChoice.ExpectedReply))
            Else
                Me.SetNullValues()
            End If
        End Set
    End Property

    Public Overrides Sub BeginEdit(ByVal EditRow As ctlEditableListRow)
        Dim WaitEditRow As ctlWaitListRow = CType(EditRow, ctlWaitListRow)
        WaitEditRow.Choice = Me.WaitChoice
        WaitEditRow.Populate(Me.WaitChoice)
        WaitEditRow.ApplicationExplorer = Me.mApplicationExplorer
        AddHandler WaitEditRow.ConditionChanged, AddressOf HandleConditionChanged
    End Sub

    Private Sub HandleConditionChanged()
        RaiseEvent ConditionChanged(mWaitChoice)
    End Sub

    Public Overrides Sub EndEdit(ByVal EditRow As ctlEditableListRow)
        Me.mWaitChoice = CType(EditRow, ctlWaitListRow).Choice
        MyBase.EndEdit(EditRow)

        Dim dt As DataType = DataType.unknown
        If mWaitChoice.Condition IsNot Nothing Then
            dt = clsProcessDataTypes.Parse(mWaitChoice.Condition.DataType)
        End If


        Me.Items(3).Value = clsProcessDataTypes.GetFriendlyName(dt)
        Me.Items(3).TextBrush = clsListItem.DataColourTextBrushes(dt)
    End Sub

    Public Overrides Function CreateEditRow() As ctlEditableListRow
        Dim WaitRow As New ctlWaitListRow(Me.mobjWaitStart, mobjProcessViewer)
        WaitRow.ApplicationExplorer = Me.mApplicationExplorer
        Return WaitRow
    End Function

    Public Overrides Sub OnDragDrop( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)
        ' Try getting either an application element or a data field from the
        ' tree node that was dragged in. If it's not a treenode or an instance of
        ' either of those classes is in the tag of the node, do nothing
        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If n Is Nothing OrElse n.Tag Is Nothing Then Return
        Dim el As clsApplicationElement = TryCast(n.Tag, clsApplicationElement)
        Dim fld As IDataField = TryCast(n.Tag, IDataField)

        If el IsNot Nothing Then
            Me.OnElementChanged(el)
            Me.Owner.CurrentEditableRow = Me

        ElseIf fld IsNot Nothing Then
            ' Bug 3558: Value should just replace whatever is already there.
            Dim fldName As String = "[" & fld.FullyQualifiedName & "]"
            Me.Items(5).Value = fldName
            Me.WaitChoice.ExpectedReply = fldName
            Me.Owner.CurrentEditableRow = Me

        End If

    End Sub

    Public Overrides Sub OnDragOver( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)
        e.Effect = DragDropEffects.None
        Me.Items(0).Highlighted = False

        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If Not n Is Nothing Then
            Select Case True
                Case TypeOf n.Tag Is clsApplicationElement
                    If colIndex = 0 Then
                        e.Effect = DragDropEffects.Move
                        Me.Items(0).Highlighted = True
                    End If
                Case TypeOf n.Tag Is IDataField
                    If colIndex = 5 Then
                        e.Effect = DragDropEffects.Move
                        Me.Items(5).Highlighted = True
                    End If
            End Select
        End If

        Me.Owner.InvalidateRow(Me)
    End Sub

    Private Sub OnElementChanged(ByVal NewElement As clsApplicationElement)
        If Not Me.WaitChoice Is Nothing Then
            'This is the fancy stuff to remember old wait/choice details
            Dim oldWait As clsWaitChoice = TryCast(Me.WaitChoice.Clone, clsWaitChoice)

            mWaitChoice.ElementID = NewElement.ID

            'If currently selected condition, then apply to new wait/choice

            If oldWait.Condition IsNot Nothing Then
                Dim KeepCondition As Boolean = False
                For Each AllowedCondition As clsConditionTypeInfo In clsAMI.GetAllowedConditions(NewElement.Type, mobjWaitStart.Process.ApplicationDefinition.ApplicationInfo)
                    If AllowedCondition.ID = oldWait.Condition.ID Then
                        KeepCondition = True
                        Exit For
                    End If
                Next

                If KeepCondition Then
                    mWaitChoice.ExpectedReply = oldWait.ExpectedReply
                    mWaitChoice.Condition = oldWait.Condition
                    mWaitChoice.ComparisonType = oldWait.ComparisonType
                End If
            End If
        Else
            'If me.choice is nothing then we are dragging a new element in
            mWaitChoice = New clsWaitChoice(mobjWaitStart)
            mWaitChoice.ElementID = NewElement.ID
        End If
    End Sub

End Class
