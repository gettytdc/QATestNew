Imports BluePrism.AutomateProcessCore

Friend Class ctlParameterList

    ''' <summary>
    ''' A reference to the map column the name of which will be changed depending on 
    ''' the maptype.
    ''' </summary>
    Private mMapColumn As ctlListColumn

    ''' <summary>
    ''' Indicates whether the parameter has a description column
    ''' </summary>
    Private mHasDescriptionColumn As Boolean

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()
        InitializeComponent()

        Me.objParamsList.MinimumColumnWidth = 30
        Me.objParamsList.Columns.Add(My.Resources.ctlParameterList_Name, 200)
        Me.objParamsList.Columns.Add(My.Resources.ctlParameterList_DataType, 105)
        mMapColumn = Me.objParamsList.Columns.Add(String.Empty, Me.Width - 285)
        UpdateMapColumnName()
        DisableMoveUpDownButtons()
    End Sub

    ''' <summary>
    ''' The direction of the parameters in this control. Newly added parameters
    ''' will have this direction set automatically.
    ''' </summary>
    Public Property ParameterDirection() As ParamDirection
        Get
            Return mParameterDirection
        End Get
        Set(ByVal value As ParamDirection)
            mParameterDirection = value
        End Set
    End Property
    Private mParameterDirection As ParamDirection

    ''' <summary>
    ''' Data types that are suppressed in the parameters list, as described in the
    ''' corresponding property on ctlProcessExpression.
    ''' </summary>
    Public Property SuppressedDataTypes() As DataType
        Get
            Return mSuppressedDataTypes
        End Get
        Set(ByVal value As DataType)
            mSuppressedDataTypes = value
            For Each R As clsParameterListRow In Me.objParamsList.Rows
                CType(R, clsParameterListRow).SuppressedDataTypes = value
            Next
        End Set
    End Property
    Private mSuppressedDataTypes As DataType

    ''' <summary>
    ''' The stage associated with this parameter list, e.g. used as scope stage.
    ''' </summary>
    Public Property Stage() As clsProcessStage
        Get
            Return mobjStage
        End Get
        Set(ByVal value As clsProcessStage)
            mobjStage = value
        End Set
    End Property
    Private mobjStage As clsProcessStage


    ''' <summary>
    ''' The treeview with which this parameter list
    ''' will be used. If set, then this list can call
    ''' a refresh on the tree.
    ''' </summary>
    Public Property Treeview() As ctlDataItemTreeView
        Get
            Return mTreeview
        End Get
        Set(ByVal value As ctlDataItemTreeView)
            mTreeview = value
        End Set
    End Property
    Private mTreeview As ctlDataItemTreeView


    ''' <summary>
    ''' Determines whether the list of parameters is to be displayed as a fully
    ''' editable list. Fully editable means that the name and data type of the
    ''' parameter may not be changed, but that the parameter mapping / expression
    ''' may still be modified.
    ''' </summary>
    Public Property FullyEditable() As Boolean
        Get
            Return mbFullyEditable
        End Get
        Set(ByVal value As Boolean)
            mbFullyEditable = value
            If mbFullyEditable Then
                Me.ShowBottomPanelButtons()
            Else
                Me.HideBottomPanelButtions()
            End If
        End Set
    End Property
    Private mbFullyEditable As Boolean

    ''' <summary>
    ''' A process viewer used to launch stage properties.
    ''' </summary>
    ''' <remarks>May be null, but if null then no stage properties can be viewed.</remarks>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property ProcessViewer() As ctlProcessViewer
        Get
            Return mProcessViewer
        End Get
        Set(ByVal value As ctlProcessViewer)
            mProcessViewer = value
        End Set
    End Property
    Private mProcessViewer As ctlProcessViewer

    ''' <summary>
    ''' Makes the 'move down', 'move up', 'add' and 'remove' buttons visible
    ''' </summary>
    Private Sub ShowBottomPanelButtons()
        Me.pnlBottomButtons.Visible = True
    End Sub

    ''' <summary>
    ''' Hides the 'move down', 'move up', 'add' and 'remove' buttons from view.
    ''' </summary>
    Private Sub HideBottomPanelButtions()
        Me.pnlBottomButtons.Visible = False
    End Sub


    ''' <summary>
    ''' If true, then no aspect of the parameter list will be editable.
    ''' </summary>
    ''' <value></value>
    Public Property [Readonly]() As Boolean
        Get
            Return mbReadonly
        End Get
        Set(ByVal value As Boolean)
            mbReadonly = value
            Me.objParamsList.Readonly = value
            If mbReadonly Then
                Me.HideBottomPanelButtions()
            Else
                Me.ShowBottomPanelButtons()
            End If
        End Set
    End Property
    Private mbReadonly As Boolean

    ''' <summary>
    ''' Relevant only when the control is not readonly.  The maptype of the
    ''' parameters in the list - i.e. the maptype given to newly created parameters.
    ''' </summary>
    Public Property MapTypeToApply() As MapType
        Get
            Return mMapType
        End Get
        Set(ByVal value As MapType)
            mMapType = value
            UpdateMapColumnName()
        End Set
    End Property
    Private mMapType As MapType

    Private Sub objParamsList_Click(ByVal sender As Object, ByVal e As ListRowChangedEventArgs) Handles objParamsList.EditableRowChanged

        If objParamsList.Rows.Count() <= 1 Then
            DisableMoveUpDownButtons()
            Return
        End If

        If objParamsList.Rows.First() Is objParamsList.CurrentEditableRow Then
            btnParamMoveDown.Enabled = True
            btnParamMoveUp.Enabled = False
            Return
        End If

        If objParamsList.LastRow Is objParamsList.CurrentEditableRow Then
            btnParamMoveDown.Enabled = False
            btnParamMoveUp.Enabled = True
            Return
        End If

        btnParamMoveDown.Enabled = True
        btnParamMoveUp.Enabled = True

    End Sub

    Private Sub DisableMoveUpDownButtons()
        btnParamMoveDown.Enabled = False
        btnParamMoveUp.Enabled = False
    End Sub

    Private Sub btnAddParam_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddParam.Click
        Dim p As New clsProcessParameter()
        p.Direction = Me.ParameterDirection
        p.Process = Stage.Process
        Dim row As clsParameterListRow = AddParameter(p, True)
        objParamsList.UpdateView()
        objParamsList.ScrollToRow(row)
    End Sub

    Private Sub btnRemoveParam_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveParam.Click
        If Me.objParamsList.Rows.Count > 0 Then
            Dim RowToRemove As clsListRow = Me.objParamsList.CurrentEditableRow
            If RowToRemove Is Nothing Then
                RowToRemove = Me.objParamsList.Rows(Me.objParamsList.Rows.Count - 1)
            End If

            Me.objParamsList.Rows.Remove(RowToRemove)
            Me.objParamsList.UpdateView()
        End If
    End Sub

    Private Sub btnParamMoveDown_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnParamMoveDown.Click

        objParamsList.MoveEditableRowdown()
        objParamsList.UpdateView()

    End Sub

    Private Sub btnParamMoveUp_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnParamMoveUp.Click

        objParamsList.MoveEditableRowUp()
        objParamsList.UpdateView()

    End Sub

    ''' <summary>
    ''' Gets a collection of parameters, as represented by the visual list.
    ''' </summary>
    ''' <param name="bRemoveUnNamed">A flag to remove any params with a blank name
    ''' </param>
    Public Function GetParameters(Optional ByVal bRemoveUnNamed As Boolean = True) As List(Of clsProcessParameter)
        Me.objParamsList.EndEditing()

        If bRemoveUnNamed Then
            RemoveUnNeededParams()
        End If

        Dim Params As New List(Of clsProcessParameter)
        For Each R As clsParameterListRow In Me.objParamsList.Rows
            Params.Add(R.Parameter)
        Next

        Return Params
    End Function

    ''' <summary>
    ''' Populates the list with the parameters supplied.
    ''' </summary>
    ''' <param name="Params">The list of parameters to display.</param>
    Public Sub Populate(ByVal Params As List(Of clsProcessParameter))
        Me.objParamsList.Rows.Clear()

        Dim RowAdded As clsParameterListRow = Nothing
        For Each p As clsProcessParameter In Params
            RowAdded = Me.AddParameter(p, False)
        Next
        Me.objParamsList.CurrentEditableRow = RowAdded

        Me.objParamsList.UpdateView()
    End Sub

    ''' <summary>
    ''' Rebuilds the columns so that an aditional Description column is inserted.
    ''' </summary>
    Public Sub ShowDescriptionColumn()
        Me.objParamsList.Columns.Clear()
        Me.objParamsList.Columns.Add(My.Resources.ctlParameterList_Name, 100)
        Me.objParamsList.Columns.Add(My.Resources.ctlParameterList_Description, 180)
        Me.objParamsList.Columns.Add(My.Resources.ctlParameterList_DataType, 85)
        mMapColumn = Me.objParamsList.Columns.Add(String.Empty, Me.Width - 365)
        UpdateMapColumnName()
        mHasDescriptionColumn = True
    End Sub

    ''' <summary>
    ''' Updates the name of the third column in the params list, according to the 
    ''' maptype and direction
    ''' of the parameters.
    ''' </summary>
    Private Sub UpdateMapColumnName()
        'Set to alternative for maptype stage
        If (MapTypeToApply = MapType.Stage) Then
            If Not mobjStage Is Nothing AndAlso mobjStage.StageType = StageTypes.End Then
                mMapColumn.Text = My.Resources.ctlParameterList_GetValueFrom
            Else
                mMapColumn.Text = My.Resources.ctlParameterList_StoreIn
            End If
        Else
            'Default value
            mMapColumn.Text = My.Resources.ctlParameterList_Value
        End If
    End Sub

    ''' <summary>
    ''' Removes the un-needed parameter boxes if they have blank names
    ''' </summary>
    Public Sub RemoveUnNeededParams()
        'Enumerate backwards, because we are removing from the collection being iterated
        For i As Integer = Me.objParamsList.Rows.Count - 1 To 0 Step -1
            If CType(objParamsList.Rows(i), clsParameterListRow).IsEmpty Then objParamsList.Rows(i).Remove()
        Next
    End Sub

    ''' <summary>
    ''' Adds a row to the list, corresponding to the supplied 
    ''' parameter object.
    ''' </summary>
    ''' <param name="parameter">The parameter to be added.</param>
    ''' <param name="MakeCurrent">If true, the new parameter row will be made the 
    ''' current row, ready for editing.</param>
    ''' <remarks>It will be necessary to call UpdateView on the list view after this
    ''' method.</remarks>
    Private Function AddParameter(ByVal parameter As clsProcessParameter, ByVal MakeCurrent As Boolean) As clsParameterListRow

        Debug.Assert(parameter.GetMapType = MapType.None OrElse parameter.GetMapType = Me.MapTypeToApply)
        Debug.Assert(parameter.Direction = Me.ParameterDirection)

        Dim ParamRow As New clsParameterListRow(
         objParamsList, parameter, ProcessViewer, mHasDescriptionColumn)
        ParamRow.Stage = Me.Stage
        ParamRow.Treeview = Me.Treeview
        ParamRow.SuppressedDataTypes = Me.SuppressedDataTypes
        ParamRow.MapTypeToApply = Me.MapTypeToApply
        ParamRow.FullyEditable = Me.FullyEditable

        Dim Index As Integer = Me.objParamsList.Rows.Count
        If Me.objParamsList.CurrentEditableRow IsNot Nothing Then
            Index = 1 + Me.objParamsList.CurrentEditableRow.Index
        End If
        Me.objParamsList.Rows.Insert(Index, ParamRow)

        If MakeCurrent Then
            Me.objParamsList.CurrentEditableRow = ParamRow
            ParamRow.EnsureVisible()
        End If
        Return ParamRow
    End Function

End Class
