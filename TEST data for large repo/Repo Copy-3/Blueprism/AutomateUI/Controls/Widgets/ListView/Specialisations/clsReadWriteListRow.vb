Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.clsProcessDataTypes

''' <summary>
''' Class describing a row within a listview which represents a read/write step
''' within a read/write stage.
''' </summary>
Friend Class clsReadWriteListRow : Inherits clsListRow

#Region " Column Index Constants "

    ''' <summary>
    ''' Helper class to enumerate the column names and corresponding indexes within
    ''' this row, assuming it resides within a read step
    ''' </summary>
    Private Class ReadStepColumns 'element name, params, action, ret type, dest stage
        Public Const ElementName As Integer = 0
        Public Const Params As Integer = 1
        Public Const ActionName As Integer = 2
        Public Const ReturnType As Integer = 3
        Public Const DestStage As Integer = 4
    End Class

    ''' <summary>
    ''' Helper class to enumerate the column names and corresponding indexes within
    ''' this row, assuming it resides within a write step
    ''' </summary>
    Private Class WriteStepColumns ' expression, element, params, element data type
        Public Const Expression As Integer = 0
        Public Const ElementName As Integer = 1
        Public Const Params As Integer = 2
        Public Const ElementType As Integer = 3
    End Class

#End Region

#Region " Published Events "

    ''' <summary>
    ''' Event raised when the selected action in the row is changed
    ''' during editing.
    ''' </summary>
    ''' <param name="NewAction">The newly selected action.</param>
    Public Event ActionChanged(ByVal NewAction As clsReadStep)

    Public Event ElementChanged(byVal newElement As clsWriteStep)
#End Region

#Region " Member Vars "

    ' Flag indicating the mode of this row - true=read; false=write
    Private mRead As Boolean

    ' The stage that this row is held within
    Private mStage As clsProcessStage

    ' The process viewer control with this row's stage open
    Private mProcessViewer As ctlProcessViewer

    ' The step within a read/write stage that this row represents
    Private mStep As clsStep

    ' The application definition of the VBO this row forms part of
    Private mAppDefinition As clsApplicationDefinition

    ' The element which is currently set in this row
    Private mElement As clsApplicationElement

    ' The App Explorer control displaying the app model that this row relates to
    Private mApplicationExplorer As ctlApplicationExplorer

    ' The data item treeview held in the owning form of this list row
    Private mTreeview As ctlDataItemTreeView

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new read/write step listrow.
    ''' </summary>
    ''' <param name="lv">The listview on which this row resides</param>
    ''' <param name="isReadStep">true to indicate that this row represents a read
    ''' step; false to indicate a write step</param>
    ''' <param name="stg">The stage to which this row belongs</param>
    ''' <param name="procViewer">The process viewer which is currently being used to
    ''' view the process holding this listrow.</param>
    Public Sub New(ByVal lv As ctlListView, ByVal isReadStep As Boolean, _
     ByVal stg As clsProcessStage, ByVal procViewer As ctlProcessViewer)
        MyBase.New(lv)
        mRead = isReadStep
        Stage = stg
        mProcessViewer = procViewer
        mAppDefinition = stg.Process.ApplicationDefinition

        SetNullValues()
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the stage that the step in this row belongs to as a read stage, or null
    ''' if it is not set or is not a read stage.
    ''' </summary>
    Private ReadOnly Property ReadStage() As clsReadStage
        Get
            Return TryCast(mStage, clsReadStage)
        End Get
    End Property

    ''' <summary>
    ''' Gets the stage that the step in this row belongs to as a write stage, or null
    ''' if it is not set or is not a write stage.
    ''' </summary>
    Private ReadOnly Property WriteStage() As clsWriteStage
        Get
            Return TryCast(mStage, clsWriteStage)
        End Get
    End Property

    ''' <summary>
    ''' The step that this row relates to as a read step, or null if this row
    ''' relates to a write step.
    ''' </summary>
    Private ReadOnly Property ReadStep() As clsReadStep
        Get
            Return TryCast(ReadWriteStep, clsReadStep)
        End Get
    End Property

    ''' <summary>
    ''' The step that this row relates to as a write step, or null if this row
    ''' relates to a read step.
    ''' </summary>
    Private ReadOnly Property WriteStep() As clsWriteStep
        Get
            Return TryCast(ReadWriteStep, clsWriteStep)
        End Get
    End Property

    ''' <summary>
    ''' Provides access to the step that this row relates to
    ''' </summary>
    Public Property ReadWriteStep() As clsStep
        Get
            If mStep Is Nothing Then
                If mRead _
                 Then mStep = New clsReadStep(ReadStage) _
                 Else mStep = New clsWriteStep(WriteStage)
            End If
            Return mStep
        End Get
        Set(ByVal value As clsStep)
            mStep = value

            If mStep IsNot Nothing Then
                Items.Clear()

                Dim elName As String = My.Resources.clsReadWriteListRow_UnknownElement
                Dim elDataType As DataType = DataType.unknown
                Dim targetEl As clsApplicationElement =
                 mAppDefinition.FindElement(mStep.ElementId)

                If targetEl IsNot Nothing Then
                    elName = targetEl.Name
                    elDataType = targetEl.DataType
                End If

                If mRead Then ' element name, params, action, ret type, dest stage
                    Items.Add(New clsListItem(Me, elName))
                    Items.Add(New clsListItem(Me, ""))
                    Items.Add(New clsListItem(Me, mStep.GetActionLabel(mAppDefinition)))
                    Items.Add(New clsListItem(Me, GetFriendlyName(mStep.ActionDataType)))
                    Items.Add(New clsListItem(Me, ReadStep.Stage))

                Else ' expression, element, params, element data type
                    Items.Add(New clsListItem(Me, WriteStep.Expression.LocalForm))
                    Items.Add(New clsListItem(Me, elName))
                    Items.Add(New clsListItem(Me, ""))
                    Items.Add(New clsListItem(Me, GetFriendlyName(elDataType)))

                End If

            Else
                SetNullValues()

            End If

            'Set the highlighting colour on each row
            Dim elemColInd As Integer
            If mRead _
             Then elemColInd = ReadStepColumns.ElementName _
             Else elemColInd = WriteStepColumns.ElementName

            Items(elemColInd).HighlightOuterColour =
             AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightOuter
            Items(elemColInd).HighlightInnerColour =
             AutomateControls.ColourScheme.Default.ListViewElementDragDropHighlightInner

            If mRead Then
                Items(ReadStepColumns.DestStage).HighlightOuterColour =
                 AutomateControls.ColourScheme.Default.ListViewDataStoreInDragDropHighlightOuter
                Items(ReadStepColumns.DestStage).HighlightInnerColour =
                 AutomateControls.ColourScheme.Default.ListViewDataStoreInDragDropHighlightInner

            Else
                Items(WriteStepColumns.Expression).HighlightInnerColour =
                 AutomateControls.ColourScheme.Default.ListViewExpressionDataDragDropHighlightInner
                Items(WriteStepColumns.Expression).HighlightInnerColour =
                 AutomateControls.ColourScheme.Default.ListViewExpressionDataDragDropHighlightOuter

            End If

        End Set
    End Property

    ''' <summary>
    ''' The stage owning this read/write row. Should be a reader or writer stage.
    ''' </summary>
    ''' <remarks>This object is used when creating new stages from the "Store In"
    ''' control.</remarks>
    Public Property Stage() As clsProcessStage
        Get
            Return mStage
        End Get
        Private Set(ByVal value As clsProcessStage)
            mStage = value
        End Set
    End Property

    ''' <summary>
    ''' The application explorer with which this row will be used, if any. When set a
    ''' callback will be made to the explorer when the user uses the 'show in
    ''' treeview' option
    ''' </summary>
    Public WriteOnly Property ApplicationExplorer() As ctlApplicationExplorer
        Set(ByVal value As ctlApplicationExplorer)
            mApplicationExplorer = value
        End Set
    End Property

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

#End Region

#Region " Methods "

    ''' <summary>
    ''' Sets the items in this row to be empty.
    ''' </summary>
    Private Sub SetNullValues()
        Items.Clear()
        ' read steps have 5 columns; write steps have 4.
        For i As Integer = 1 To CInt(IIf(mRead, 5, 4))
            Items.Add(New clsListItem(Me, New clsProcessValue("")))
        Next
    End Sub

    Public Overrides Sub BeginEdit(ByVal EditRow As ctlEditableListRow)
        Dim rowCtl As ctlReadWriteListRow = CType(EditRow, ctlReadWriteListRow)
        rowCtl.ReadWriteStep = Me.ReadWriteStep
        rowCtl.Stage = Me.Stage
        rowCtl.Treeview = Me.mTreeview
        rowCtl.Populate(Me.ReadWriteStep)
    End Sub

    Public Overrides Sub EndEdit(ByVal EditRow As ctlEditableListRow)
        Me.mStep = CType(EditRow, ctlReadWriteListRow).ReadWriteStep
        MyBase.EndEdit(EditRow)
    End Sub

    Public Overrides Function CreateEditRow() As ctlEditableListRow
        Dim ReadWriteRow As New ctlReadWriteListRow(mRead, mStage, mProcessViewer)
        ReadWriteRow.Treeview = Me.mTreeview
        ReadWriteRow.ApplicationExplorer = Me.mApplicationExplorer
        AddHandler ReadWriteRow.ActionChanged, AddressOf HandleActionChanged
        AddHandler ReadWriteRow.ElementChanged, addressof handleElementChanged
        Return ReadWriteRow
    End Function


    Private Sub HandleActionChanged(ByVal NewAction As clsReadStep)
        RaiseEvent ActionChanged(NewAction)
    End Sub
    Private Sub HandleElementChanged(ByVal newAction As clsStep)
        RaiseEvent ElementChanged(CType(newAction, clsWriteStep))
    End Sub

    Public Overrides Sub OnDragOver(
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal draggedColIndex As Integer)

        e.Effect = DragDropEffects.None

        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim tag As Object = DirectCast(e.Data.GetData(GetType(TreeNode)), TreeNode).Tag
            Dim col As Integer = -1
            If TypeOf tag Is clsApplicationElement Then
                ' If an app element is being dragged, use the 'element name' column
                If mRead _
                 Then col = ReadStepColumns.ElementName _
                 Else col = WriteStepColumns.ElementName

            ElseIf TypeOf tag Is IDataField Then
                ' If a data item is being dragged, it's the 'store in' stage for a
                ' read row; the 'expression' stage for a write row
                If mRead _
                 Then col = ReadStepColumns.DestStage _
                 Else col = WriteStepColumns.Expression

            End If

            If col >= 0 Then
                Dim validDrag As Boolean = (col = draggedColIndex)
                Items(col).Highlighted = validDrag
                If validDrag Then e.Effect = DragDropEffects.Move
            End If
        End If

        Owner.InvalidateRow(Me)
    End Sub

    Public Overrides Sub OnDragDrop(
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)

        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If n Is Nothing Then Return

        Dim el As clsApplicationElement = TryCast(n.Tag, clsApplicationElement)
        Dim fld As IDataField = TryCast(n.Tag, IDataField)

        If el IsNot Nothing Then
            OnElementChanged(el)
            Owner.CurrentEditableRow = Me

        ElseIf fld IsNot Nothing Then
            If mRead Then
                Dim dtype As DataType =
                 Items(ReadStepColumns.DestStage).Value.DataType

                If fld.DataType = dtype Then
                    Dim fldName As String = fld.FullyQualifiedName
                    Items(ReadStepColumns.DestStage).Value = fldName
                    ReadStep.Stage = fldName
                Else
                    UserMessage.Show(String.Format(
                     My.Resources.clsReadWriteListRow_TheRequiredDataTypeIs0ButTheDataTypeOf1Is2,
                     GetFriendlyName(dtype), fld.FullyQualifiedName,
                     GetFriendlyName(fld.DataType))
                    )
                End If

            Else
                ' bug 3558: just replace current value, despite being an expression
                Dim fldName As String = "[" & fld.FullyQualifiedName & "]"

                Items(WriteStepColumns.Expression).Value = fldName
                WriteStep.Expression = BPExpression.FromNormalised(fldName)
            End If
            Owner.CurrentEditableRow = Me
        End If

        Owner.InvalidateRow(Me)
    End Sub

    ''' <summary>
    ''' Handles the application element set in this row being changed.
    ''' </summary>
    ''' <param name="el">The new element set in this row</param>
    Private Sub OnElementChanged(ByVal el As clsApplicationElement)
        If Not mRead AndAlso el.Type.Readonly Then
            UserMessage.Show(
             My.Resources.clsReadWriteListRow_ThisElementIsNotSuitableForWritingPleaseChooseAnother)
            Exit Sub
        End If

        mElement = el

        Dim st As clsStep
        If mRead Then
            st = New clsReadStep(ReadStage)
            st.ElementId = mElement.ID
            DirectCast(st, clsReadStep).Stage = _
             CStr(Items(ReadStepColumns.DestStage).Value)

        Else
            st = New clsWriteStep(WriteStage)
            st.ElementId = mElement.ID
            DirectCast(st, clsWriteStep).Expression = BPExpression.FromLocalised( _
             CStr(Items(WriteStepColumns.Expression).Value))

        End If

        ReadWriteStep = st

    End Sub

#End Region

End Class
