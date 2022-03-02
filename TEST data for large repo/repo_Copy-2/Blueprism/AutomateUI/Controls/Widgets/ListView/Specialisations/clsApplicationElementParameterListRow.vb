Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.AMI

Friend Class clsApplicationElementParameterListRow
    Inherits clsListRow

    ''' <summary>
    ''' The stage used in scope resolution during editing of this row.
    ''' </summary>
    Private mStage As clsProcessStage

    ''' <summary>
    ''' Private member to store public property Parameter()
    ''' </summary>
    Private mParam As clsApplicationElementParameter
    ''' <summary>
    ''' The parameter represented by this listrow
    ''' </summary>
    ''' <value></value>
    Public Property Parameter() As clsApplicationElementParameter
        Get
            Return mParam
        End Get
        Set(ByVal value As clsApplicationElementParameter)
            mParam = value

            If Me.mParam IsNot Nothing Then
                Dim ident As clsIdentifierInfo = clsAMI.GetIdentifierInfo(mParam.Name)
                With Me.Items
                    .Clear()
                    If ident.Type = IdentifierType.Parent Then
                        .Add(New clsListItem(Me, String.Format(LocaleTools.Properties.GlobalResources.Parent0, ident.Name)))
                    Else
                        .Add(New clsListItem(Me, ident.Name))
                    End If
                    .Add(New clsListItem(Me, _
                      clsProcessDataTypes.GetFriendlyName(mParam.DataType)))
                    .Add(New clsListItem(Me, _
                      clsAMI.GetComparisonTypeFriendlyName(mParam.ComparisonType)))
                    .Add(New clsListItem(Me, mParam.Expression.LocalForm))
                End With
            Else
                SetNullValues()
            End If
        End Set
    End Property

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="Parameter">The parameter to be represented by this row.</param>
    ''' <param name="Stage">The stage used for scope resolution during editing of
    ''' this row.</param>
    Public Sub New(ByVal lv As ctlListView, ByVal Parameter As clsApplicationElementParameter, ByVal Stage As clsProcessStage)
        MyBase.New(lv)
        Me.Parameter = Parameter
        Me.mStage = Stage
    End Sub


    Private Sub SetNullValues()
        Me.Items.Clear()
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'Attribute name
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'data type
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'comparison type
        Me.Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, ""))) 'value
    End Sub


    Public Overrides Sub BeginEdit(ByVal EditRow As ctlEditableListRow)
        CType(EditRow, ctlApplicationElementParameterListRow).Populate(Me.mParam, Me.mStage)
    End Sub

    Public Overrides Sub EndEdit(ByVal EditRow As ctlEditableListRow)
        Me.Parameter = CType(EditRow, ctlApplicationElementParameterListRow).ElementParameter
        MyBase.EndEdit(EditRow)
    End Sub

    Public Overrides Function CreateEditRow() As ctlEditableListRow
        Return New ctlApplicationElementParameterListRow
    End Function

    ''' <summary>
    ''' Handles the dropping of an item onto this component.
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    ''' <param name="locn">The location within the component that this occurred.
    ''' </param>
    ''' <param name="colIndex">The column index within the list that this occurred.
    ''' </param>
    Public Overrides Sub OnDragDrop( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)

        ' Try getting the IDataField from the dragged treenode; if it's not a node,
        ' or it does not contain a data field tag, exit immediately
        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If n Is Nothing OrElse n.Tag Is Nothing Then Return
        Dim df As IDataField = TryCast(n.Tag, IDataField)
        If df Is Nothing Then Return

        ' bug 3358: Value should just replace whatever is already there
        Dim fldName As String = _
         "[" & CType(n.Tag, IDataField).FullyQualifiedName & "]"
        Me.Items(3).Value = fldName
        Me.Parameter.Expression = BPExpression.FromLocalised(fldName)
        Me.Owner.CurrentEditableRow = Me
    End Sub

    Public Overrides Sub OnDragOver( _
     ByVal e As DragEventArgs, ByVal locn As Point, ByVal colIndex As Integer)
        e.Effect = DragDropEffects.None
        Me.Items(0).Highlighted = False

        Dim n As TreeNode = TryCast(e.Data.GetData(GetType(TreeNode)), TreeNode)
        If Not n Is Nothing Then
            Select Case True
                Case TypeOf n.Tag Is IDataField
                    If colIndex = 3 Then
                        e.Effect = DragDropEffects.Move
                        Me.Items(3).Highlighted = True
                    End If
            End Select
        End If

        Me.Owner.InvalidateRow(Me)
    End Sub

End Class
