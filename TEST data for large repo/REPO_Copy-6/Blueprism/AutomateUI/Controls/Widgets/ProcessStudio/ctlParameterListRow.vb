Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : Automate
''' Class    : ctlProcessExpression
''' 
''' <summary>
''' This control is used to build the custom listview control seen in 
''' ctlInputsOutputsConditions, it allows the user to edit an simple expression
''' </summary>
Friend Class ctlParameterListRow
    Inherits ctlEditableListRow

    ''' <summary>
    ''' holds the undelying process parameter
    ''' </summary>
    Private mobjParameter As clsProcessParameter

    ''' <summary>
    ''' Holds a reference to the business objects, which is used to get collection 
    ''' info.
    ''' </summary>
    Private mobjBusinessObjects As clsGroupBusinessObject

    ''' <summary>
    ''' Holds a reference to the expression item 
    ''' </summary>
    ''' <remarks></remarks>
    Private mobjExpressionItem As ctlEditableListItem

    ''' <summary>
    ''' Holds a reference to the datatype item
    ''' </summary>
    ''' <remarks></remarks>
    Private mobjDataTypeItem As ctlEditableListItem

    ''' <summary>
    ''' Holds a reference to the name item
    ''' </summary>
    ''' <remarks></remarks>
    Private mobjNameItem As ctlEditableListItem

    ''' <summary>
    ''' Holds a reference to the description item
    ''' </summary>
    ''' <remarks></remarks>
    Private mobjDescriptionItem As ctlEditableListItem


    ''' <summary>
    ''' Holds a reference to the maptype
    ''' </summary>
    ''' <remarks></remarks>
    Private mdtMapType As MapType

    ''' <summary>
    ''' Creates a new Expression row
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New(ByVal HasDescription As Boolean, ByVal objProcessViewer As ctlProcessViewer)
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.PopulateDataTypeComboBox()

        Me.mHasDescription = HasDescription

        mobjNameItem = New ctlEditableListItem(Me.txtName)
        mobjNameItem.Width = 100

        If mHasDescription Then
            mobjDescriptionItem = New ctlEditableListItem(Me.txtDescription)
            mobjDescriptionItem.Width = 100
        End If

        mobjDataTypeItem = New ctlEditableListItem(Me.cmbDatatype)
        mobjDataTypeItem.Width = 100

        mobjExpressionEdit.ProcessViewer = objProcessViewer
        mobjExpressionItem = New ctlEditableListItem(Me.mobjExpressionEdit)
        mobjExpressionItem.Width = Me.Width - 200

        Me.Items.Add(mobjNameItem)
        If mHasDescription Then
            Me.Items.Add(mobjDescriptionItem)
        End If
        Me.Items.Add(mobjDataTypeItem)
        Me.Items.Add(mobjExpressionItem)

        Me.UpdateControls()
    End Sub

    ''' <summary>
    ''' Indicates whether the parameter has a description column
    ''' </summary>
    ''' <remarks></remarks>
    Private mHasDescription As Boolean

    ''' <summary>
    ''' Determines which colour will be used in the user interface based on which
    ''' data type we are using.
    ''' </summary>
    ''' <param name="dataType">The data type to base the color choice on</param>
    Private Sub DecideWhichColourToUse(ByVal dataType As DataType)
        Dim c As Color
        If Me.IsSelected Then
            c = Me.HighlightedForeColor
        Else
            c = DataItemColour.GetDataItemColor(dataType)
        End If

        Me.mobjExpressionItem.ForeColor = c
        Me.mobjNameItem.ForeColor = c
        Me.mobjDataTypeItem.ForeColor = c
    End Sub

    Protected Overrides Sub OnDeSelected(ByVal sender As Object, ByVal e As System.EventArgs)
        MyBase.OnDeSelected(sender, e)
        Me.DecideWhichColourToUse(Me.DataType)
    End Sub



    ''' <summary>
    ''' Populates the data types combo box with
    ''' allowed data types.
    ''' </summary>
    ''' <remarks>See also property SuppressedDataItems</remarks>
    Private Sub PopulateDataTypeComboBox()
        Me.cmbDatatype.Init(Me.SuppressedDataTypes)
    End Sub

    ''' <summary>
    ''' Determines if the supplied data type is 
    ''' to be suppressed on the current control.
    ''' </summary>
    ''' <param name="dt">The data type to consider.</param>
    ''' <returns>Returns true if the data type is to be
    ''' suppressed, false otherwise.</returns>
    ''' <remarks></remarks>
    Private Function DataTypeIsSuppressed(ByVal dt As DataType) As Boolean
        Return (dt And Me.mSuppressedDataTypes) > 0
    End Function

    ''' <summary>
    ''' The datatype set for this expression. Affects the colour used to display
    ''' the expression and the datatype label displayed in the middle column.
    ''' </summary>
    ''' <value>The data type of the expression</value>
    Public Property DataType() As DataType
        Get
            Return Me.cmbDatatype.ChosenDataType
        End Get
        Set(ByVal Value As DataType)
            cmbDatatype.ChosenDataType = Value
            Me.txtDatatype.Text = clsProcessDataTypes.GetFriendlyName(Value)
            Me.DecideWhichColourToUse(Value)
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the name of the data
    ''' </summary>
    ''' <value>A string containing the name</value>
    Public Property DataName() As String
        Get
            Return txtName.Text
        End Get
        Set(ByVal Value As String)
            txtName.Text = Value

            'Autocreated data item store-in mappings default to the name
            'of the parameter itself (when not specified by the user):
            Me.mobjStoreInEdit.AutoCreateDefault = Value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the description text
    ''' </summary>
    Public Property Description() As String
        Get
            Return txtDescription.Text
        End Get
        Set(ByVal Value As String)
            txtDescription.Text = Value

            'If we don't have a description column then display the discription as a tooltip
            If Not mHasDescription Then
                If mTooltipNarrative Is Nothing Then
                    Dim mTooltipNarrative As New ToolTip
                    mTooltipNarrative.SetToolTip(txtName, Value)
                End If
            End If
        End Set
    End Property
    Private mTooltipNarrative As ToolTip

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
            Me.PopulateDataTypeComboBox()
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
            Return Me.mobjExpressionEdit.Stage
        End Get
        Set(ByVal value As clsProcessStage)
            Me.mobjExpressionEdit.Stage = value
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
    ''' The expression that is displayed.
    ''' </summary>
    ''' <value>The expression string</value>
    Public ReadOnly Property Expression() As Control
        Get
            Select Case mdtMapType
                Case MapType.Expr
                    Return mobjExpressionEdit
                Case Else
                    Return mobjStoreInEdit
            End Select

        End Get
    End Property

    Public Property ExpressionText() As String
        Get
            Return Expression.Text
        End Get
        Set(ByVal Value As String)
            Expression.Text = Value
            mobjExpressionEdit.ColourText()
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the parameter of the expression.
    ''' </summary>
    ''' <value>The parameter</value>
    Public Property Parameter() As clsProcessParameter
        Get
            Me.CommitExpression()
            Return mobjParameter
        End Get
        Set(ByVal Value As clsProcessParameter)
            mobjParameter = Value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property FullyEditable
    ''' </summary>
    Private mbFullyEditable As Boolean
    ''' <summary>
    ''' Allows the control to be made fully editable for the purposes of start and
    ''' end stage parameters, which can have the name and data type set by the user.
    ''' </summary>
    ''' <value>A boolean indicating whether the expression should be fully editable</value>
    Public Property FullyEditable() As Boolean
        Get
            Return Me.mbFullyEditable
        End Get
        Set(ByVal Value As Boolean)
            Me.mbFullyEditable = Value

            Me.txtName.ReadOnly = Not Value
            Me.txtDatatype.Visible = Not Value
            Me.txtDatatype.TabStop = Me.txtDatatype.Visible
            Me.cmbDatatype.Visible = Value
            Me.cmbDatatype.TabStop = Me.cmbDatatype.Visible
            If Not Value Then
                Me.Expression.Select()
                Me.Items(1).NestedControl = Me.txtDatatype
            End If
        End Set
    End Property

    ''' <summary>
    ''' handles the selected index changed event for the datatype combo box.
    ''' </summary>
    ''' <param name="sender">The object that sent the event</param>
    ''' <param name="e">The event arugments</param>
    Private Sub cmbDatatype_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbDatatype.SelectedIndexChanged
        Dim iDataType As DataType = cmbDatatype.ChosenDataType
        If iDataType <> mobjParameter.GetDataType Then
            mobjParameter.SetMap(clsExpression.LocalToNormal(Me.ExpressionText))
            mobjParameter.SetMapType(MapType.None)
            mobjParameter.SetDataType(iDataType)
            Me.DataType = iDataType
        End If
    End Sub

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
            If value = MapType.Expr Then
                Me.mobjExpressionItem.NestedControl = mobjExpressionEdit
            Else
                Me.mobjExpressionItem.NestedControl = mobjStoreInEdit
            End If
        End Set
    End Property


    ''' <summary>
    ''' Private member to store public property Treeview()
    ''' </summary>
    Private mTreeview As ctlDataItemTreeView
    ''' <summary>
    ''' The treeview associated with this row for data item autocreation, etc
    ''' </summary>
    Public Property Treeview() As ctlDataItemTreeView
        Get
            Return mTreeview
        End Get
        Set(ByVal value As ctlDataItemTreeView)
            mTreeview = value
        End Set
    End Property


    Private Sub mobjStoreInEdit_AutoCreateRequested(ByVal DataItemName As String) Handles mobjStoreInEdit.AutoCreateRequested

        'Info used the last time we auto-placed a stage
        Static LastStageAdded As clsProcessStage
        Static LastRelativePosition As clsProcessStagePositioner.RelativePositions
        Dim newStage As clsDataStage = clsProcessStagePositioner.CreateDataItem(DataItemName, Me.Stage, Me.DataType, LastStageAdded, LastRelativePosition)

        If newStage IsNot Nothing Then
            'Copy across the collection's fields, when appropriate
            If newStage.GetDataType() = DataType.collection Then
                If MapTypeToApply = MapType.Stage Then
                    If Parameter IsNot Nothing AndAlso Parameter.HasDefinedCollection() Then
                        Dim stg As clsCollectionStage = DirectCast(newStage, clsCollectionStage)
                        For Each fld As clsCollectionFieldInfo In Parameter.CollectionInfo
                            stg.AddField(fld)
                        Next
                    End If
                End If
            End If

            CommitExpression()
            If Me.Treeview IsNot Nothing Then
                Me.Treeview.Repopulate(newStage)
            End If
        End If
    End Sub

    ''' <summary>
    ''' When the expression looses focus this is when we parse the input, and set the
    ''' underlying value
    ''' </summary>
    ''' <param name="sender">The object that sent the event</param>
    ''' <param name="e">The event arguments.</param>
    Private Sub txtExpression_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles mobjExpressionEdit.LostFocus, mobjStoreInEdit.LostFocus
        Me.CommitExpression()
    End Sub

    ''' <summary>
    ''' Commits the expression in the expression edit field
    ''' </summary>
    ''' <returns>Returns true if validation is successful.</returns>
    Public Function CommitExpression() As Boolean

        Dim sResult As String = Me.ExpressionText

        'Allow the user to clear the maptype
        If sResult = "" Then mobjParameter.SetMapType(MapType.None)

        Select Case MapTypeToApply
            Case MapType.Stage
                mobjParameter.SetMap(sResult.Trim("["c, "]"c))
            Case MapType.Expr
                mobjParameter.SetMap(clsExpression.LocalToNormal(sResult))
            Case Else
                mobjParameter.SetMap(sResult)
        End Select

        If mHasDescription Then
            mobjParameter.Narrative = txtDescription.Text
        End If

        mobjParameter.SetMapType(Me.MapTypeToApply)

        Return True
    End Function


    ''' <summary>
    ''' When the name control looses focus we set the name of the underlying value
    ''' to the input typed into the text box.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub txtName_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtName.LostFocus
        mobjParameter.Name = txtName.Text
    End Sub

    ''' <summary>
    ''' This handles drag-drop events, so that when the user drags a data item from 
    ''' the treeview, of the correct datatype, the underlying value gets populated 
    ''' with this data item.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub txtExpression_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles mobjExpressionEdit.DragDrop, mobjStoreInEdit.DragDrop

        Dim n As TreeNode = DirectCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        Dim fld As IDataField = TryCast(n.Tag, IDataField)

        If fld IsNot Nothing Then
            Dim dDataType As DataType = cmbDatatype.ChosenDataType
            If dDataType = DataType.unknown Then
                dDataType = fld.DataType
                Me.DataType = dDataType
            End If

            Dim name As String = fld.FullyQualifiedName
            If fld.DataType = dDataType Then

                If MapTypeToApply = MapType.Stage Then
                    mobjParameter.SetMap(name)
                    Me.ExpressionText = name
                Else
                    'The expression text has already been set by the 
                    'drag drop handler in clsExpressionRichTextBox
                    mobjParameter.SetMap(clsExpression.LocalToNormal(ExpressionText))
                End If
                mobjParameter.SetMapType(MapTypeToApply)

            Else
                ShowMessage(String.Format( _
                 My.Resources.ctlReadWriteListRow_TheRequiredDataTypeIs0ButTheDataTypeOf1Is2, _
                 clsProcessDataTypes.GetFriendlyName(dDataType), _
                 name, _
                 clsProcessDataTypes.GetFriendlyName(fld.DataType)) _
                )
            End If
        End If

    End Sub

    ''' <summary>
    ''' Updates the positions and width of all the controls based on the column widths
    ''' </summary>
    Public Overrides Sub UpdateControls()
        MyBase.UpdateControls()
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Selects the txtName control and then shows a message.
    ''' 
    ''' <remarks>This is useful because
    ''' Version 2.0 of the .NET framework will return focus to the last control
    ''' after a message box is shown. Since we perform validation on the input when the
    ''' LostFocus event is fired, this means that validation is repeatedly perfomed and the
    ''' messages never go away! See bug 1717.
    ''' </remarks>
    ''' </summary>
    ''' <param name="sMessage"></param>
    Private Sub ShowMessage(ByVal sMessage As String)
        Me.txtName.Select()
        UserMessage.Show(sMessage)
    End Sub


    Private Sub txtName_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtName.TextChanged
        Me.mobjStoreInEdit.AutoCreateDefault = Me.txtName.Text
    End Sub
End Class
