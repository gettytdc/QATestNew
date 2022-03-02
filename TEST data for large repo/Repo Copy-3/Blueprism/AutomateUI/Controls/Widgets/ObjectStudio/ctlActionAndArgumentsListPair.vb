Imports BluePrism.AutomateProcessCore
Imports BluePrism.AMI
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.ApplicationManager.AMI

''' <summary>
''' UI control to provide a pair of listviews. The first
''' listview provides a list of AMI actions, such as readactions
''' or navigate actions. The second listview provides a dynamically
''' updated list of arguments to those actions, depending on the row
''' selected in the first list.
''' </summary>
''' <remarks>Used eg in the navigate properties form and
''' read properties form.</remarks>
Friend Class ctlActionAndArgumentsListPair

    Public Event AcceptButtonToggle As EventHandler(Of ValueEventArgs(Of Boolean))

    Public Sub New()
        InitializeComponent()

        ctlActions.MinimumColumnWidth = 10
        ctlArguments.MinimumColumnWidth = 10

        ctlArguments.Columns.Add(My.Resources.ctlActionAndArgumentsListPair_Name, 170)
        ctlArguments.Columns.Add(My.Resources.ctlActionAndArgumentsListPair_Datatype, 80)
        ctlArguments.Columns.Add(My.Resources.ctlActionAndArgumentsListPair_Value, 250)
        ctlArguments.LastColumnAutoSize = True

        ctlListViewOutputs.MinimumColumnWidth = 10

        ctlListViewOutputs.Columns.Add(My.Resources.ctlActionAndArgumentsListPair_Name, 170)
        ctlListViewOutputs.Columns.Add(My.Resources.ctlActionAndArgumentsListPair_Datatype, 80)
        ctlListViewOutputs.Columns.Add(My.Resources.ctlParameterList_StoreIn, 250)
        ctlListViewOutputs.LastColumnAutoSize = True

        UpdateButtons()

        PauseAfterStepVisible = False
    End Sub

    ''' <summary>
    ''' Sets the columns used by the list of actions. This must be set
    ''' before any call to PopulateActions
    ''' </summary>
    Public WriteOnly Property ActionColumns() As List(Of ctlListColumn)
        Set(ByVal value As List(Of ctlListColumn))
            Me.ctlActions.Columns.Clear()
            For Each c As ctlListColumn In value
                Me.ctlActions.Columns.Add(c)
            Next
            Me.ctlActions.LastColumnAutoSize = True
        End Set
    End Property

    Public Sub SetTreeview(value As ctlDataItemTreeView)
        mTreeview = value
    End Sub
    Private mTreeview As ctlDataItemTreeView

    ''' <summary>
    ''' Gets the list rows corresponding to the actions in this action/args list pair
    ''' </summary>
    Friend ReadOnly Property ActionRows As IEnumerable(Of clsListRow)
        Get
            Return ctlActions.Rows
        End Get
    End Property

    ''' <summary>
    ''' Ends the editing in any lists held in this control
    ''' </summary>
    Friend Sub EndEditing()
        ctlActions.EndEditing()
        ctlArguments.EndEditing()
        ctlListViewOutputs.EndEditing()
    End Sub

    ''' <summary>
    ''' Signature definition for a delegate method used to create a new row
    ''' for the actions list. 
    ''' </summary>
    ''' <param name="S">If not null, then the data in the supplied
    ''' step should be populated into the row. Otherwise the client
    ''' should create their own blank/default data for the row.</param>
    ''' <returns>Returns a new listrow, not already contained in the actions
    ''' list whose columns match the pattern 
    ''' supplied to the constructor for this control.</returns>
    Public Delegate Function StepRowCreator(
     ByVal lv As ctlListView, ByVal s As IActionStep) As clsListRow

    ''' <summary>
    ''' Delegate to create a new row for the actions list.
    ''' This must be set before any call to PopulateActions()
    ''' </summary>
    Public RowCreator As StepRowCreator

    ' The stage used by this control for context and scope processing
    Private mStage As clsProcessStage

    ''' <summary>
    ''' The stage associated with this control, used for scope calculations.
    ''' Usually, this is the stage whose properties form is being viewed.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Stage As clsProcessStage
        Get
            Return mStage
        End Get
        Set(value As clsProcessStage)
            mStage = value
            exprPause.Stage = value
            Dim app = TryCast(value, clsAppStage)
            PauseAfterStep = If(
                app Is Nothing, "", app.PauseAfterStepExpression.LocalForm
            )
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets whether the 'Pause After Step' panel is visible in this control.
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
        "Show the 'Pause After Step' label and input field")>
    Public Property PauseAfterStepVisible As Boolean
        Get
            Return panPause.Enabled
        End Get
        Set(value As Boolean)
            panPause.Enabled = value
            panPause.Visible = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the 'PauseAfterStep' expression in this control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property PauseAfterStep As String
        Get
            Return exprPause.Text
        End Get
        Set(value As String)
            exprPause.Text = value
        End Set
    End Property

    Private mAppDefinition As clsApplicationDefinition
    ''' <summary>
    ''' The application definition used in this control.
    ''' This must be set before any call to PopulateActions()
    ''' </summary>
    Public WriteOnly Property ApplicationDefinition() As clsApplicationDefinition
        Set(ByVal value As clsApplicationDefinition)
            Me.mAppDefinition = value
        End Set
    End Property

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
            exprPause.ProcessViewer = value
        End Set
    End Property
    Private mProcessViewer As ctlProcessViewer

    ''' <summary>
    ''' Populates the actions list with the supplied steps.
    ''' </summary>
    ''' <param name="Steps">A list of steps to appear in the actions list. The
    ''' supplied steps must be compatible with the RowCreator delegate defined at
    ''' construction time.</param>
    ''' <remarks>The ActionColumns, Stage, RowCreator and ApplicationDefinition
    ''' properties must be set before calling this method.</remarks>
    Public Sub PopulateActions(ByVal steps As List(Of IActionStep))
        For Each s As IActionStep In steps
            Me.ctlActions.Rows.Add(Me.RowCreator(ctlActions, s))
        Next

        If Me.ctlActions.Rows.Count = 0 Then
            Me.AddNewRow()
        End If

        Me.ctlActions.UpdateView()

        'Select top row.
        If Me.ctlActions.Rows.Count > 0 Then
            Me.ctlActions.CurrentEditableRow = Me.ctlActions.Rows(0)
        End If
        Me.UpdateButtons()
        Me.ctlActions.LastColumnAutoSize = True
    End Sub


    ''' <summary>
    ''' Updates the enabled property on buttons in the form.
    ''' </summary>
    Private Sub UpdateButtons()
        Me.btnRemove.Enabled = Me.ctlActions.Rows.Count > 0
        Me.btnAdd.Enabled = True

        'Enable/disable move up/down buttons
        Dim SelectedRow As clsListRow = Me.ctlActions.CurrentEditableRow
        Dim Index As Integer = -1
        If SelectedRow IsNot Nothing Then
            Index = SelectedRow.Index
        End If

        Me.btnMoveUp.Enabled = (Index <> -1) AndAlso Index > 0
        Me.btnMoveDown.Enabled = (Index <> -1) AndAlso (Index < Me.ctlActions.Rows.Count - 1)
    End Sub





#Region "Button clicks"

    Private Sub btnMoveUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMoveUp.Click
        Dim SelectedRow As clsListRow = Me.ctlActions.CurrentEditableRow
        Me.ctlActions.MoveEditableRowUp()
        Me.ctlActions.UpdateView()
        Me.ctlActions.CurrentEditableRow = SelectedRow
        Me.UpdateButtons()
    End Sub

    Private Sub btnMoveDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMoveDown.Click
        Dim SelectedRow As clsListRow = Me.ctlActions.CurrentEditableRow
        Me.ctlActions.MoveEditableRowdown()
        Me.ctlActions.UpdateView()
        Me.ctlActions.CurrentEditableRow = SelectedRow
        Me.UpdateButtons()
    End Sub

    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        If Not Me.ctlActions.CurrentEditableRow Is Nothing Then
            Me.ctlActions.CurrentEditableRow.Remove()
        End If
        Me.ctlActions.UpdateView()
        Me.UpdateButtons()
    End Sub

    Private Sub btnAdd_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        Me.AddNewRow()
    End Sub

    ''' <summary>
    ''' Adds a new row to the actions list
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub AddNewRow()
        Dim R As clsListRow = Me.RowCreator(ctlActions, Nothing)
        Dim Index As Integer = Me.ctlActions.Rows.Count
        If Me.ctlActions.CurrentEditableRow IsNot Nothing Then
            Index = 1 + Me.ctlActions.CurrentEditableRow.Index
        End If
        Me.ctlActions.Rows.Insert(Index, R)
        ctlActions.UpdateView()
        Me.ctlActions.CurrentEditableRow = R
        R.EnsureVisible()
    End Sub

#End Region



    ''' <summary>
    ''' Updates the list of arguments for supplied action row
    ''' </summary>
    ''' <param name="st">The row, whose arguments are to be represented.</param>
    Public Sub RefreshArguments(ByVal st As IActionStep, Optional treeView As ctlDataItemTreeView = Nothing)

        'If the selected row has an action requiring arguments, populate them
        If Not st Is Nothing Then

            If (st.Action IsNot Nothing AndAlso st.Action.Arguments.Count > 0) OrElse st.ArgumentValues.Count > 0 Then
                PopulateArgumentsList(st)
            Else
                ClearArgumentsList()
            End If

            If (st.Action IsNot Nothing AndAlso st.Action.Outputs.Count > 0) OrElse st.OutputValues.Any() Then
                PopulateOutputsList(st, treeView)
            Else
                ClearOutputsList()
            End If
        Else
            ClearOutputsList()
            ClearArgumentsList()
        End If

    End Sub

    Public Sub RemoveOutputTab()
        TabControl2.TabPages.Remove(OutputTab)
    End Sub

    ''' <summary>
    ''' Clears the arguments list and disable it.
    ''' </summary>
    Private Sub ClearArgumentsList()
        ctlArguments.Rows.Clear()
        ctlArguments.UpdateView()
        ctlArguments.Enabled = False
    End Sub

    Private Sub ClearOutputsList()
        ctlListViewOutputs.Rows.Clear()
        ctlListViewOutputs.UpdateView()
        ctlListViewOutputs.Enabled = False
    End Sub

    ''' <summary>
    ''' Populates the arguments listview with the arguments supplied.
    ''' </summary>
    ''' <param name="st">The step in question</param>
    Private Sub PopulateArgumentsList(ByVal st As IActionStep)


        If st.Action IsNot Nothing Then

            ' Remove any existing argument values which don't belong to this action BG-147
            For Each argValKey In st.ArgumentValues.Keys.ToArray()
                If Not st.Action.Arguments.ContainsKey(argValKey) Then
                    st.ArgumentValues.Remove(argValKey)
                End If
            Next

            'Add in any system arguments not already present in user arguments
            For Each arg As clsArgumentInfo In st.Action.Arguments.Values
                If Not st.ArgumentValues.ContainsKey(arg.ID) Then
                    st.ArgumentValues.Add(arg.ID, "")
                End If
            Next
        End If

        'Repopulate list
        ctlArguments.Rows.Clear()
        If st.Action IsNot Nothing Then
            For Each info As clsArgumentInfo In st.Action.Arguments.Values
                Dim row As clsListRow = CreateArgumentRow(st, info, False)
                ctlArguments.Rows.Add(row)
            Next
        End If

        If Me.ctlArguments.Rows.Count > 0 Then
            Me.ctlArguments.CurrentEditableRow = Me.ctlArguments.Rows(0)
        End If

        ctlArguments.UpdateView()
        ctlArguments.Enabled = True

    End Sub

    Private Sub PopulateOutputsList(ByVal st As IActionStep, treeView As ctlDataItemTreeView)


        If st.Action IsNot Nothing Then

            'Remove any existing output values which don't belong to this action
            For Each argValKey In st.OutputValues.Keys.ToArray()
                If Not st.Action.Outputs.ContainsKey(argValKey) Then
                    st.OutputValues.Remove(argValKey)
                End If
            Next
        End If

        'Repopulate list
        ctlListViewOutputs.Rows.Clear()
        If st.Action IsNot Nothing Then
            For Each info As clsArgumentInfo In st.Action.Outputs.Values
                Dim row As clsListRow = CreateArgumentRow(st, info, True, treeView)
                ctlListViewOutputs.Rows.Add(row)
            Next
        End If

        If Me.ctlListViewOutputs.Rows.Count > 0 Then
            Me.ctlListViewOutputs.CurrentEditableRow = Me.ctlListViewOutputs.Rows(0)
        End If

        ctlListViewOutputs.UpdateView()
        ctlListViewOutputs.Enabled = True

    End Sub

    ''' <summary>
    ''' Create an argument row.
    ''' </summary>
    ''' <param name="st">The associated step</param>
    ''' <param name="arg">The clsArgumentInfo instance describing the argument</param>
    ''' <returns>A newsly constructed ctlParameterListRow</returns>
    Private Function CreateArgumentRow(ByVal st As IActionStep, ByVal arg As clsArgumentInfo, output As Boolean, Optional treeview As ctlDataItemTreeView = Nothing) As clsListRow

        Dim listView As ctlListView = ctlArguments
        If output Then
            listView = ctlListViewOutputs
        End If

        'Create parameter to correspond to name/datatype/value combo
        'this is a bit of a hack really, but let's us use the existing
        'ProcessExpression control
        Dim param As New clsProcessParameter
        param.Name = arg.Name
        param.SetDataType(clsProcessDataTypes.DataTypeId(arg.DataType))

        If Not output AndAlso st.ArgumentValues.ContainsKey(arg.ID) Then
            param.SetMap(st.ArgumentValues(arg.ID))
        ElseIf output AndAlso st.OutputValues.ContainsKey(arg.ID) Then
            param.SetMap(st.OutputValues(arg.ID))
        End If

        Dim row As _
         New clsParameterListRow(listView, param, ProcessViewer, False)
        param.Process = Stage.Process
        param.Direction = CType(IIf(output, ParamDirection.Out, ParamDirection.In), ParamDirection)
        row.Stage = Stage
        row.AssociatedStep = st
        row.FullyEditable = False
        row.Parameter = param
        row.Tag = arg
        row.MapTypeToApply = CType(IIf(output, MapType.Stage, MapType.Expr), MapType)

        If output And treeview IsNot Nothing Then
            row.Treeview = treeview
        End If
        If output Then
            AddHandler row.EditingEnded, AddressOf HandleOutputChanged
        Else
            AddHandler row.EditingEnded, AddressOf HandleArgumentChanged
        End If
        Return row
    End Function


    ''' <summary>
    ''' Handles any changes in an argument in the arguments list.
    ''' </summary>
    Private Sub HandleArgumentChanged(ByVal Sender As clsListRow)

        'Update the corresponding argument
        Dim p As clsProcessParameter = CType(Sender, clsParameterListRow).Parameter
        Dim a As clsArgumentInfo = CType(Sender.Tag, clsArgumentInfo)
        CType(Sender, clsParameterListRow).AssociatedStep.ArgumentValues.Item(a.ID) = p.GetMap

        'update status on corresponding row
        If TypeOf ctlActions.CurrentEditableRow Is clsNavigateListRow Then
            CType(ctlActions.CurrentEditableRow, clsNavigateListRow).UpdateInputsStatus()
        End If
    End Sub

    Private Sub HandleOutputChanged(ByVal Sender As clsListRow)

        'Update the corresponding argument
        Dim p As clsProcessParameter = CType(Sender, clsParameterListRow).Parameter
        Dim a = CType(Sender.Tag, clsArgumentInfo)
        CType(Sender, clsParameterListRow).AssociatedStep.OutputValues.Item(a.ID) = p.GetMap

    End Sub



    Private Sub ctlActions_SelectedRowChanged(
     ByVal sender As Object, ByVal e As ListRowChangedEventArgs) _
     Handles ctlActions.EditableRowChanged

        Dim newRow As clsListRow = e.NewRow

        ' We're only interested if changing to an actual row
        If newRow Is Nothing Then Return

        'Make sure any edits to the current argument are commited before changing things
        ctlArguments.EndEditing()
        ctlListViewOutputs.EndEditing()

        'Now repopulate arguments list for current row, etc ...
        Dim st As IActionStep = Nothing
        Select Case True
            Case TypeOf newRow Is clsNavigateListRow
                Dim navRow As clsNavigateListRow = CType(newRow, clsNavigateListRow)
                st = navRow.NavigationAction
                navRow.UpdateInputsStatus()
            Case TypeOf newRow Is clsReadWriteListRow
                Dim rwRow As clsReadWriteListRow = CType(newRow, clsReadWriteListRow)
                If TypeOf rwRow.ReadWriteStep Is clsReadStep Then
                    Dim ReadStep As clsReadStep = CType(rwRow.ReadWriteStep, clsReadStep)
                    st = ReadStep
                End If
            Case TypeOf newRow Is clsWaitListRow
                Dim waitRow As clsWaitListRow = CType(newRow, clsWaitListRow)
                st = waitRow.WaitChoice
            Case Else
                Throw New InvalidOperationException(My.Resources.ctlActionAndArgumentsListPair_UnsupportedRowTypeInCtlActionAndArgumentsListPair)
        End Select

        If st IsNot Nothing Then
            RefreshArguments(st, mTreeview)
        ElseIf Stage.StageType <> StageTypes.Write Then
            ClearArgumentsList()
        End If

        If TypeOf e.OldRow Is clsNavigateListRow Then
            CType(e.OldRow, clsNavigateListRow).UpdateInputsStatus()
        End If

        Me.UpdateButtons()
    End Sub


    ''' <summary>
    ''' Signature definition for methods used to retrieve help reference
    ''' in response to a press of the help button.
    ''' </summary>
    ''' <returns>Returns a string to be used as a help reference,
    ''' in the form of an AMI action ID.</returns>
    Public Delegate Function GetHelpReference() As String


    ''' <summary>
    ''' Delegate for fetching the help reference in response to
    ''' a help button click.
    ''' </summary>
    Public HelpReferenceRetriever As GetHelpReference


    Private Sub btnInfo_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnInfo.Click
        Dim ID As String = String.Empty
        If Me.HelpReferenceRetriever IsNot Nothing Then
            ID = HelpReferenceRetriever.Invoke
        End If
        clsUserInterfaceUtils.ShowHTMLDocument(Me.Stage.Process.AMI.GetDocumentation(clsAMI.DocumentFormats.HTML), String.Format(My.Resources.ctlActionAndArgumentsListPair_AMIDocumentationVersion0, clsAMI.Version.ToString), ID)
    End Sub

    Private Sub btnInfo_Enter(sender As Object, e As EventArgs) Handles btnInfo.Enter
        RaiseEvent AcceptButtonToggle(nothing, new ValueEventArgs(Of Boolean)(False))
    End Sub

    Private Sub btnInfo_Leave(sender As Object, e As EventArgs) Handles btnInfo.Leave
        RaiseEvent AcceptButtonToggle(nothing, new ValueEventArgs(Of Boolean)(True))
    End Sub

End Class
