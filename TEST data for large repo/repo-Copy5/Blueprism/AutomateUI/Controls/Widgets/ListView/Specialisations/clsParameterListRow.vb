Imports BluePrism.AutomateProcessCore

Friend Class clsParameterListRow
    Inherits clsListRow

    ''' <summary>
    ''' Indicates whether the parameter has a description column
    ''' </summary>
    Private mHasDescription As Boolean

    ''' <summary>
    ''' Private member to store public property Stage
    ''' </summary>
    Private mobjStage As clsProcessStage

    ''' <summary>
    ''' Private member to store process viewer
    ''' </summary>
    Private mProcessViewer As ctlProcessViewer

    ''' <summary>
    ''' Private member to store public property SuppressedDataItems()
    ''' </summary>
    Private mSuppressedDataTypes As DataType

    ''' <summary>
    ''' Relevant only when the control is fully editable.
    ''' Data items that will not appear in the combo box for 
    ''' selection by the user.
    ''' </summary>
    ''' <value></value>
    Public Property SuppressedDataTypes() As DataType
        Get
            Return mSuppressedDataTypes
        End Get
        Set(ByVal value As DataType)
            mSuppressedDataTypes = value
        End Set
    End Property

    ''' <summary>
    ''' The stage being worked with as the scope stage.
    ''' This value will be passed to the expression
    ''' building popup form as the scope stage for the
    ''' embedded data item treeview.
    ''' </summary>
    Public Property Stage() As clsProcessStage
        Get
            Return mobjStage
        End Get
        Set(ByVal value As clsProcessStage)
            mobjStage = value
        End Set
    End Property

    ''' <summary>
    ''' The step associated, if any.
    ''' </summary>
    Public Property AssociatedStep() As IActionStep
        Get
            Return mobjStep
        End Get
        Set(ByVal value As IActionStep)
            mobjStep = value
        End Set
    End Property
    Private mobjStep As IActionStep

    ''' <summary>
    ''' Provides access to the parameter of the expression.
    ''' </summary>
    ''' <value>The parameter</value>
    Public Property Parameter() As clsProcessParameter
        Get
            Return mParameter
        End Get
        Set(ByVal Value As clsProcessParameter)
            mParameter = Value

            If mParameter IsNot Nothing Then
                Me.Items.Clear()
                Me.Items.Add(New clsListItem(Me, CStr(IIf(mParameter.FriendlyName IsNot Nothing, mParameter.FriendlyName, mParameter.Name))))
                If Me.mHasDescription Then
                    Me.Items.Add(New clsListItem(Me, mParameter.Narrative))
                End If
                Dim DataTypeItem As New clsListItem(Me, clsProcessDataTypes.GetFriendlyName(mParameter.ParamType))
                DataTypeItem.AvailableValues = clsListItem.GetDataTypeAvailableValues
                Me.Items.Add(DataTypeItem)

                Dim mapVal As String = mParameter.GetMap()
                If mParameter.GetMapType() = MapType.Expr Then _
                 mapVal = clsExpression.NormalToLocal(mapVal)
                Items.Add(New clsListItem(Me, mapVal))

            Else
                Me.SetNullValues()
            End If

            'Set the data item colours
            Dim b As Brush = clsListItem.DataColourTextBrushes(Parameter.ParamType)
            Me.NameItem.TextBrush = b
            Me.DataTypeItem.TextBrush = b
        End Set
    End Property
    Private mParameter As clsProcessParameter

    ''' <summary>
    ''' Allows the control to be made fully editable for the purposes of start and
    ''' end stage parameters, which can have the name and data type set by the user.
    ''' </summary>
    ''' <value>A boolean indicating whether the expression should be fully editable</value>
    Public WriteOnly Property FullyEditable() As Boolean
        Set(ByVal Value As Boolean)
            Me.mbFullyEditable = Value
        End Set
    End Property
    Private mbFullyEditable As Boolean

    ''' <summary>
    ''' The maptype to be applied to the parameter when save
    ''' </summary>
    ''' <value></value>
    Public Property MapTypeToApply() As MapType
        Get
            Return Me.mdtMapType
        End Get
        Set(ByVal value As MapType)
            Me.mdtMapType = value
        End Set
    End Property
    Private mdtMapType As MapType

    ''' <summary>
    ''' The treeview associated with this row, for editing purposes
    ''' </summary>
    Public WriteOnly Property Treeview() As ctlDataItemTreeView
        Set(ByVal value As ctlDataItemTreeView)
            mTreeview = value
        End Set
    End Property
    Private mTreeview As ctlDataItemTreeView

    ''' <summary>
    ''' Provides convenient access to the Map Item
    ''' </summary>
    Private ReadOnly Property MapItem() As clsListItem
        Get
            Return Me.Items(MapIndex)
        End Get
    End Property

    ''' <summary>
    ''' Returns the correct index of the MapItem depending on whether
    ''' the ParameterListRow as a description columns or not
    ''' </summary>
    Private ReadOnly Property MapIndex() As Integer
        Get
            If Me.mHasDescription Then
                Return 3
            End If

            Return 2
        End Get
    End Property

    ''' <summary>
    ''' Provides convenient access to the Data Type Item
    ''' </summary>
    Private ReadOnly Property DataTypeItem() As clsListItem
        Get
            If Me.mHasDescription Then
                Return Me.Items(2)
            End If

            Return Me.Items(1)
        End Get
    End Property

    ''' <summary>
    ''' Provides convenient access to the Name Item
    ''' </summary>
    Private ReadOnly Property NameItem() As clsListItem
        Get
            Return Me.Items(0)
        End Get
    End Property

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="Parameter">The clsProcessParameter that this list row should
    ''' represent</param>
    ''' <param name="HasDescription">Indicates whether the parameter has a 
    ''' description column</param>
    Public Sub New(ByVal lv As ctlListView, ByVal Parameter As clsProcessParameter, _
     ByVal ProcessViewer As ctlProcessViewer, ByVal HasDescription As Boolean)
        MyBase.New(lv)
        Me.mHasDescription = HasDescription
        Me.Parameter = Parameter
        Me.mProcessViewer = ProcessViewer
    End Sub

    ''' <summary>
    ''' Sets the Rows items to empty values
    ''' </summary>
    Private Sub SetNullValues()
        Me.Items.Clear()
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'name
        If mHasDescription Then
            Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'description
        End If
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'datatype
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'value
    End Sub

    ''' <summary>
    ''' Instructs the row to populate the supplied edit row with its data, ready for
    ''' editing.
    ''' </summary>
    ''' <param name="EditRow">The edit row to be used when editing this row.</param>
    Public Overrides Sub BeginEdit(ByVal EditRow As ctlEditableListRow)
        Dim ParameterRow As ctlParameterListRow = CType(EditRow, ctlParameterListRow)
        ParameterRow.Parameter = Me.Parameter
        ParameterRow.FullyEditable = Me.mbFullyEditable
        ParameterRow.Stage = Me.Stage
        ParameterRow.Treeview = Me.mTreeview
        ParameterRow.AssociatedStep = Me.AssociatedStep
        ParameterRow.SuppressedDataTypes = Me.SuppressedDataTypes
        ParameterRow.MapTypeToApply = Me.MapTypeToApply

        ParameterRow.DataName = mParameter.FriendlyName
        ParameterRow.Description = mParameter.Narrative
        ParameterRow.DataType = mParameter.GetDataType
        Dim mapVal As String = mParameter.GetMap()
        If mParameter.GetMapType() = MapType.Expr Then _
         mapVal = clsExpression.NormalToLocal(mapVal)
        ParameterRow.ExpressionText = mapVal
        ParameterRow.mobjExpressionEdit.ColourText()
    End Sub

    ''' <summary>
    ''' Commits the data contained in the supplied edit row, following an edit 
    ''' operation.
    ''' </summary>
    ''' <param name="EditRow">The edit row containing data to be committed to this
    ''' row.</param>
    Public Overrides Sub EndEdit(ByVal EditRow As ctlEditableListRow)
        Me.Parameter = CType(EditRow, ctlParameterListRow).Parameter
        MyBase.EndEdit(EditRow)
    End Sub

    ''' <summary>
    ''' Creates an edit row suitable for editing the values in this row
    ''' </summary>
    ''' <returns>Returns an editable listview row, populated with the data in this 
    ''' row, ready to edit.</returns>
    Public Overrides Function CreateEditRow() As ctlEditableListRow
        Return New ctlParameterListRow(mHasDescription, mProcessViewer)
    End Function

    ''' <summary>
    ''' Creates a combobox picker control for items which stem from an enumeration.
    ''' </summary>
    ''' <param name="Item">This parameter is ignored.</param>
    ''' <returns>a clsDataTypesComboBox</returns>
    Protected Overrides Function CreatePickerControl(ByVal Item As clsListItem) As System.Windows.Forms.Control
        Return New clsDataTypesComboBox
    End Function

    ''' <summary>
    ''' Determines whether the current parameter row is 'empty'.
    ''' </summary>
    ''' <returns>True if the row is emtpy; false otherwise.</returns>
    Public Function IsEmpty() As Boolean
        Return String.IsNullOrEmpty(mParameter.Name)
    End Function

    ''' <summary>
    ''' Informs the row that something is being dragged over it. The row may choose
    ''' to process and accept the drop request. By default it will be rejected here
    ''' </summary>
    ''' <param name="e">Information about the proposed drag event.</param>
    ''' <param name="locn">The point at which the drag event is taking place,
    ''' in coordinates relative to the row. This should be used in preference to the
    ''' X, Y values in the event args.</param>
    ''' <param name="colIndex">The index of the column over which something is being
    ''' dragged.</param>
    Public Overrides Sub OnDragOver( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)
        e.Effect = DragDropEffects.None
        Me.MapItem.Highlighted = False

        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If colIndex = MapIndex AndAlso n IsNot Nothing AndAlso TypeOf n.Tag Is IDataField Then
            Me.MapItem.Highlighted = True
            Dim HighlightOuterColour As Color
            Dim HighlightInnerColour As Color
            Select Case Me.MapTypeToApply
                Case MapType.Stage
                    e.Effect = DragDropEffects.Move
                    HighlightOuterColour = AutomateControls.ColourScheme.Default.ListViewDataStoreInDragDropHighlightOuter
                    HighlightInnerColour = AutomateControls.ColourScheme.Default.ListViewDataStoreInDragDropHighlightInner
                Case MapType.Expr
                    e.Effect = DragDropEffects.Move
                    HighlightOuterColour = AutomateControls.ColourScheme.Default.ListViewExpressionDataDragDropHighlightOuter
                    HighlightInnerColour = AutomateControls.ColourScheme.Default.ListViewExpressionDataDragDropHighlightInner
                Case Else
                    e.Effect = DragDropEffects.None
                    HighlightOuterColour = Color.Transparent
                    HighlightInnerColour = Color.Transparent
            End Select

            Me.MapItem.HighlightOuterColour = HighlightOuterColour
            Me.MapItem.HighlightInnerColour = HighlightInnerColour
        End If

        Me.Owner.InvalidateRow(Me)
    End Sub

    ''' <summary>
    ''' Informs the row that something is being dropped onto it. The row may choose
    ''' to process and accept the drop request. By default it will be rejected here
    ''' </summary>
    ''' <param name="e">Information about the proposed drag event.</param>
    ''' <param name="locn">The point at which the drag event is taking place,
    ''' in coordinates relative to the row. This should be used in preference to the
    ''' X, Y values in the event args.</param>
    Public Overrides Sub OnDragDrop(ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)

        Dim col As ctlListColumn = Owner.Columns(MapIndex)

        If locn.X > col.Left AndAlso locn.X < col.Right Then
            Dim n As TreeNode = DirectCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
            Dim fld As IDataField = TryCast(n.Tag, IDataField)

            If fld IsNot Nothing Then
                Dim dDataType As DataType = Parameter.GetDataType()
                ' Intuit a data type from the dropped object if we don't have one already
                If dDataType = DataType.unknown Then
                    dDataType = fld.DataType
                    Parameter.SetDataType(dDataType)
                End If

                If fld.DataType = dDataType Then
                    Select Case MapTypeToApply
                        'For both expr, stage we replace existing value. See bug 3558
                        Case MapType.Stage : Parameter.SetMap(fld.FullyQualifiedName)
                        Case MapType.Expr : Parameter.SetMap("[" & fld.FullyQualifiedName & "]")
                        Case Else : Parameter.SetMap("")
                    End Select

                    MapItem.Value = New clsProcessValue(DataType.text, Parameter.GetMap)
                    Parameter.SetMapType(MapTypeToApply)
                    Owner.CurrentEditableRow = Me

                    MapItem.Highlighted = False
                    Owner.InvalidateRow(Me)

                Else
                    UserMessage.Show(String.Format( _
                        My.Resources.ctlReadWriteListRow_TheRequiredDataTypeIs0ButTheDataTypeOf1Is2, _
                     clsProcessDataTypes.GetFriendlyName(dDataType), _
                     fld.FullyQualifiedName, _
                     clsProcessDataTypes.GetFriendlyName(fld.DataType)) _
                    )
                End If

            End If

        End If

    End Sub
End Class
