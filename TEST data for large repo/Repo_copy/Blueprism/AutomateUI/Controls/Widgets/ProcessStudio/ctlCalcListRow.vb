Imports BluePrism.AutomateProcessCore

Friend Class ctlCalcListRow
    Inherits ctlEditableListRow

    Public Sub New(ByVal viewer As ctlProcessViewer, ByVal stage As clsProcessStage)

        mExpressionEditArea = New ctlExpressionEdit
        mExpressionEditArea.ProcessViewer = viewer
        mExpressionEditArea.Stage = stage

        mStoreInEditArea = New ctlStoreInEdit()

        Items.Add(New ctlEditableListItem(mExpressionEditArea))
        Items.Add(New ctlEditableListItem(mStoreInEditArea))

    End Sub

    ''' <summary>
    ''' Reference to the expression edit control
    ''' </summary>
    Public ReadOnly Property ExpressionEdit() As ctlExpressionEdit
        Get
            Return mExpressionEditArea
        End Get
    End Property
    Private mExpressionEditArea As ctlExpressionEdit

    ''' <summary>
    ''' Reference to the store in edit control
    ''' </summary>
    Public ReadOnly Property StoreIn() As ctlStoreInEdit
        Get
            Return mStoreInEditArea
        End Get
    End Property
    Private WithEvents mStoreInEditArea As ctlStoreInEdit

    ''' <summary>
    ''' The calculation represented by this row.
    ''' </summary>
    Public Property Calculation() As clsCalcStep
        Get
            Return mCalculation
        End Get
        Set(ByVal value As clsCalcStep)
            mCalculation = value
        End Set
    End Property
    Private mCalculation As clsCalcStep

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
    ''' Handles something being dragged / dropped into the store-in edit control
    ''' which forms part of this row.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The arguments regarding the event.</param>
    Private Sub HandleDragDrop( _
     ByVal sender As Object, ByVal e As DragEventArgs) Handles mStoreInEditArea.DragDrop

        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If n IsNot Nothing AndAlso TypeOf n.Tag Is IDataField Then
            mStoreInEditArea.Text = DirectCast(n.Tag, IDataField).FullyQualifiedName
        End If

    End Sub


    ''' <summary>
    ''' Handles the Auto Create Requested  event of the Store In Edit control
    ''' </summary>
    ''' <param name="DataItemName">The name of the data item to be created</param>
    Private Sub mResultEditArea_AutoCreateRequested(ByVal DataItemName As String) Handles mStoreInEditArea.AutoCreateRequested
        'Info used the last time we auto-placed a stage
        Static LastStageAdded As clsProcessStage
        Static LastRelativePosition As clsProcessStagePositioner.RelativePositions

        Dim DataType As DataType = clsProcessStagePositioner.DataTypeFromExpression(Calculation.Parent, mExpressionEditArea.Text)
        Dim NewDataItem As Stages.clsDataStage = clsProcessStagePositioner.CreateDataItem(DataItemName, Calculation.Parent, DataType, LastStageAdded, LastRelativePosition)

        If NewDataItem IsNot Nothing Then
            If Me.mTreeview IsNot Nothing Then
                Me.mTreeview.Repopulate(NewDataItem)
            End If
        End If
    End Sub
End Class
