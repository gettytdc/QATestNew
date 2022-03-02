Imports System.Text.RegularExpressions
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateAppCore.Utility

''' Project  : Automate
''' Class    : frmStagePropertiesCollection
''' 
''' <summary>
''' The collection properties form.
''' </summary>
Friend Class frmStagePropertiesCollection

    ''' <summary>
    ''' Edit mode of this properties dialog.
    ''' </summary>
    Private Enum EditMode

        ''' <summary>
        ''' The dialog is currently editing the stage - the field definitions within
        ''' the stage, or the top level of the stage's values.
        ''' </summary>
        Stage

        ''' <summary>
        ''' The stage is currently editing a nested collection within the stage's
        ''' initial value.
        ''' </summary>
        InitialValue

        ''' <summary>
        ''' The stage is currently editing a nested collection within the stage's
        ''' current value.
        ''' </summary>
        CurrentValue

    End Enum

    ''' <summary>
    ''' The regular expression from which a collection name can be extracted from
    ''' a listview column name - this works for both current and initial values.
    ''' </summary>
    Private Shared ReadOnly CollNameRegex As New Regex("^(.*?)\s+(?:\(.*\)|\uFF08.*\uFF09)$")

    Private ReadOnly mEditable As Boolean

    ''' <summary>
    ''' The current mode of this dialog - ie. whether this dialog is currently
    ''' editing the stage, the stage's current value or the stage's initial value.
    ''' Note that this uses the state of the UI to define the current mode, so
    ''' it can't be used with accuracy, for example, while the mode is transitioning.
    ''' </summary>
    Private ReadOnly Property CurrentMode() As EditMode
        Get
            ' We know we're in stage edit mode if we
            ' a) only have one breadcrumb (or 0 - only applies pre-initialisation), or
            ' b) the top tag is a collection definition rather than a collection object.
            If mBreadcrumbs.Count <= 1 OrElse TypeOf mBreadcrumbs.TopTag Is clsCollectionInfo Then
                Return EditMode.Stage
            End If
            ' Otherwise the top tag is a collection object, ergo we're editing a value.
            ' The current state of the tab pages will tell us which collection value we
            ' are currently modifying
            If tbCurrent.Enabled Then Return EditMode.CurrentValue
            Return EditMode.InitialValue
        End Get
    End Property

    ''' <summary>
    ''' The current collection definition manager.
    ''' If we are currently at the top level of the stage configuration, this will be
    ''' the <see cref="clsCollectionStage"/> object; if we are currently editing
    ''' fields within the stage's definition, it will be a
    ''' <see cref="clsCollectionInfo"/> object - otherwise, this will be a
    ''' <see cref="clsCollection"/> object - owned (ultimately) by either the initial
    ''' value or the current value of the stage.
    ''' </summary>
    Private ReadOnly Property Manager() As ICollectionDefinitionManager
        Get
            Return DirectCast(mBreadcrumbs.TopTag, ICollectionDefinitionManager)
        End Get
    End Property

    ''' <summary>
    ''' The default constructor initialises the component.
    ''' <param name="IsFormEditable">Flag setting to set if the form is 'readonly'</param>
    ''' </summary>
    Public Sub New(ByVal IsFormEditable As Boolean)
        MyBase.New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Can't get this to serialize in the form designer... no idea why...
        Me.LogInhibitVisible = False

        mProcessStage = Nothing
        mEditable = IsFormEditable

        lvCollectionFields.Columns.Add(My.Resources.frmStagePropertiesCollection_Name).Width = 120
        lvCollectionFields.Columns.Add(My.Resources.frmStagePropertiesCollection_Type).Width = 105
        lvCollectionFields.FillColumn = lvCollectionFields.Columns.Add(My.Resources.frmStagePropertiesCollection_Description)
        lvCollectionFields.Columns.Add(My.Resources.frmStagePropertiesCollection_Fields).Width = 80

        lvCollectionFields.Sortable = True

        ' We draw the tab headers so we can disable tabs if necessary (and draw them
        ' disabled which the tab control in and of itself doesn't)
        tabFieldsAndValues.DrawMode = TabDrawMode.OwnerDrawFixed
        tabFieldsAndValues.HotTrack = True

    End Sub

    ''' <summary>
    ''' Populates this dialog with the data from the current stage.
    ''' </summary>
    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        Dim coll As clsCollectionStage = Me.Stage

        'Make sure we have a valid index...
        If coll Is Nothing Then
            UserMessage.Show(My.Resources.frmStagePropertiesCollection_PropertiesDialogIsNotProperlyConfigured)
            Exit Sub
        End If

        ' Only one thing in the breadcrumbs, so push the primary collection name on there
        mBreadcrumbs.Push(coll.GetName(), Me.Stage)
        ' And hide it.
        mBreadcrumbs.Visible = False

        'Fill in all the fields...
        txtName.Text = coll.GetName()
        txtDescription.Text = coll.GetNarrative()
        chkPrivate.Checked = coll.IsPrivate
        chkAlwaysInit.Checked = coll.AlwaysInit
        chkSingleRow.Checked = coll.SingleRow

        ' Bring the list views up to speed
        UpdateFieldsListView()
        UpdateCurrentValueListView()
        UpdateInitialValueListView()

        tbCurrent.Enabled = ShouldShowCurrentValue()

    End Sub

    ''' <summary>
    ''' Gets the collection stage associated with this form.
    ''' </summary>
    Protected ReadOnly Property Stage() As clsCollectionStage
        Get
            Return DirectCast(mProcessStage, clsCollectionStage)
        End Get
    End Property

    ''' <summary>
    ''' Extends the validation performed in the base class.
    ''' </summary>
    ''' <returns>Returns true if validation successful, false otherwise.</returns>
    Protected Overrides Function ApplyChanges() As Boolean

        ' If we've been updating the fields, make sure that the init and curr value
        ' list views are up to date with the new field data
        Dim tp As TabPage = tabFieldsAndValues.SelectedTab
        If tp Is tbFields Then
            UpdateInitialValueListView()
            UpdateCurrentValueListView()
        Else
            lvInitialValue.EndEditing()
            lvCurrentValue.EndEditing()
        End If

        lvCollectionFields.EndEditing()

        ResetToTop() ' Get back to the stage itself.

        If MyBase.ApplyChanges() Then

            Dim stg As clsCollectionStage = Me.Stage

            'Check for illegal characters in chosen name
            Dim sErr As String = Nothing
            If Not clsDataStage.IsValidDataName(txtName.Text, sErr) Then
                UserMessage.Show(String.Format(My.Resources.frmStagePropertiesCollection_TheChosenNameForThisStageIsInvalid0, sErr))
                txtName.Focus()
                Return False
            End If

            Dim matching = stg.Process.GetConflictingDataStages(stg)

            If matching IsNot Nothing Then
                UserMessage.Show(String.Format(
                 My.Resources.frmStagePropertiesCollection_TheChosenNameForThisStageConflictsWithTheStage0OnPage1PleaseChooseAnother22Alte,
                 matching.Name, matching.GetSubSheetName(), vbCrLf)
                )
                txtName.Focus()
                Return False
            End If

            Return True
        Else
            Return False
        End If
    End Function

#Region " Handle non-list field changes "

    ''' <summary>
    ''' Handles the name text field validating, and updates the stage with the name
    ''' currently specified if the current edit mode indicates we're editing the
    ''' stage itself.
    ''' </summary>
    Private Sub HandleNameValidated(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtName.Validated
        If CurrentMode = EditMode.Stage Then
            Stage.Name = txtName.Text
            mBreadcrumbs.RootLabel = txtName.Text
        End If
    End Sub

    ''' <summary>
    ''' Handles the description field validating, updating the stage with the
    ''' currently defined description if this form's current mode indicates that the
    ''' stage is being edited.
    ''' </summary>
    Private Sub HandleDescriptionValidated(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtDescription.Validated
        If CurrentMode = EditMode.Stage Then Stage.SetNarrative(txtDescription.Text)
    End Sub

    ''' <summary>
    ''' Handles the 'always initialise' checkbox being changed, updating the stage
    ''' with the currently set value if this form's current mode indicates that the
    ''' stage is being edited.
    ''' </summary>
    Private Sub HandleAlwaysInitChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles chkAlwaysInit.CheckedChanged
        If CurrentMode = EditMode.Stage Then Stage.AlwaysInit = chkAlwaysInit.Checked
    End Sub

    ''' <summary>
    ''' Handles the 'hide from others' checkbox being changed, updating the stage
    ''' with the currently set value if this form's current mode indicates that the
    ''' stage is being edited.
    ''' </summary>
    Private Sub HandlePrivateChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles chkPrivate.CheckedChanged
        If CurrentMode = EditMode.Stage Then Stage.IsPrivate = chkPrivate.Checked
    End Sub

    ''' <summary>
    ''' Handles the 'single row' checkbox being changed - this can be set in the 
    ''' stage or when navigating the current or initial values. It sets it on the
    ''' appropriate collection definition manager.
    ''' </summary>
    Private Sub HandleSingleRowChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles chkSingleRow.CheckedChanged

        Manager.SingleRow = chkSingleRow.Checked

        ' If single-row, you can't add / remove rows in the initial / current values
        AllowAddRemoveValues = Not chkSingleRow.Checked

        ' If we're currently viewing a values tab, update it
        Dim tp As TabPage = tabFieldsAndValues.SelectedTab
        If tp Is tbCurrent Then
            UpdateCurrentValueListView()

        ElseIf tp Is tbInitValue Then
            UpdateInitialValueListView()

        End If
    End Sub

#End Region

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesCollection.htm"
    End Function

    ''' <summary>
    ''' Opens the help file whether online or offline.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the 'Add Field' button being clicked.
    ''' This adds a new row to the collection fields and creates it in the stage
    ''' represented by this form.
    ''' </summary>
    Private Sub btnAddField_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddField.Click

        'We can safely just add the field here. If this collection stage
        'has no definition (i.e. it's dynamic) that will automatically be
        'altered - a definition will be created containing the new field...
        Dim newname As String = clsCollectionStage.GenerateUniqueFieldName(My.Resources.frmStagePropertiesCollection_Field, Manager)
        Manager.AddField(newname, DataType.unknown)

        Dim newRow As New clsCollectionFieldListRow(
         lvCollectionFields, Manager.GetFieldDefinition(newname))
        Dim index As Integer
        If lvCollectionFields.CurrentEditableRow Is Nothing Then
            index = lvCollectionFields.Rows.Count
        Else
            index = 1 + lvCollectionFields.CurrentEditableRow.Index
        End If

        lvCollectionFields.Rows.Insert(index, newRow)
        lvCollectionFields.CurrentEditableRow = newRow
        newRow.EnsureVisible()
        lvCollectionFields.UpdateView()
        lvCollectionFields.FocusEditColumn(0)

        UpdateSingleRowState()

    End Sub

    ''' <summary>
    ''' Generates a field header suitable for display in
    ''' the User Interface Listview.
    ''' </summary>
    ''' <param name="sName">The field for which a header
    ''' should be generated.</param>
    ''' <param name="dtDataType">the data type of the field</param>
    ''' <returns>Returns the header generated, as a string.</returns>
    Private Function GetFieldHeader(ByVal sName As String, ByVal dtDataType As DataType) As String
        Return String.Format(My.Resources.frmStagePropertiesCollection_FieldName0DataType1, sName, clsProcessDataTypes.GetFriendlyName(dtDataType))
    End Function

    ''' <summary>
    ''' Gets the collection currently being displayed in the Initial Value tab
    ''' </summary>
    Private ReadOnly Property DisplayedInitialValue() As clsCollection
        Get
            Select Case CurrentMode
                Case EditMode.Stage
                    Return Stage.GetInitialValue().Collection

                Case EditMode.InitialValue
                    Return TryCast(mBreadcrumbs.TopTag, clsCollection)

                Case Else ' (Including EditMode.CurrentValue)
                    Return Nothing
            End Select
        End Get
    End Property

    ''' <summary>
    ''' Gets the collection currently being displayed in the Current Value tab
    ''' </summary>
    Private ReadOnly Property DisplayedCurrentValue() As clsCollection
        Get
            Select Case CurrentMode
                Case EditMode.Stage
                    Return Stage.GetValue().Collection

                Case EditMode.CurrentValue
                    Return TryCast(mBreadcrumbs.TopTag, clsCollection)


                Case Else ' (Including EditMode.InitialValue)
                    Return Nothing
            End Select
        End Get
    End Property

#Region " List View Updaters "

    ''' <summary>
    ''' Updates the collection fields with the data from the definition.
    ''' </summary>
    Private Sub UpdateFieldsListView()
        lvCollectionFields.SuspendLayout()
        lvCollectionFields.Rows.Clear()
        For Each field As clsCollectionFieldInfo In Manager.FieldDefinitions
            lvCollectionFields.Rows.Add(
             New clsCollectionFieldListRow(lvCollectionFields, field))
        Next
        lvCollectionFields.UpdateView()
        lvCollectionFields.ResumeLayout(True)
        If mEditable AndAlso CurrentMode = EditMode.Stage Then UpdateSingleRowState()
    End Sub

    ''' <summary>
    ''' Updates the state of the single row checkbox - a collection cannot contain
    ''' any rows if it has no fields defined, so this ensures that a collection is
    ''' not set as single-row without any defined fields.
    ''' </summary>
    Private Sub UpdateSingleRowState()

        If lvCollectionFields.Rows.Count = 0 AndAlso chkSingleRow.Enabled Then
            chkSingleRow.Checked = False
            mTooltip.SetToolTip(chkSingleRow, My.Resources.frmStagePropertiesCollection_ASingleRowCollectionMustContainAtLeastOneField)
            chkSingleRow.Enabled = False
            AllowAddRemoveValues = False
        ElseIf lvCollectionFields.Rows.Count > 0 AndAlso Not chkSingleRow.Enabled Then
            chkSingleRow.Enabled = True
            mTooltip.SetToolTip(chkSingleRow, Nothing)
            AllowAddRemoveValues = True
        End If
    End Sub

    ''' <summary>
    ''' Gets or sets whether the values (initial or current) can be added to or
    ''' removed from.
    ''' </summary>
    Private Property AllowAddRemoveValues() As Boolean
        Get
            Return btnAddInitialValueRow.Enabled _
             AndAlso btnRemoveInitialValueRow.Enabled _
             AndAlso btnAddCurrentValueRow.Enabled _
             AndAlso btnRemoveCurrentValueRow.Enabled
        End Get
        Set(ByVal value As Boolean)
            btnAddInitialValueRow.Enabled = value
            btnRemoveInitialValueRow.Enabled = value
            btnAddCurrentValueRow.Enabled = value
            btnRemoveCurrentValueRow.Enabled = value
        End Set
    End Property

    ''' <summary>
    ''' Updates the listview showing initial values.
    ''' </summary>
    ''' <param name="selectedIndex">The index to select. If not given, this will
    ''' leave the currently selected row selected.</param>
    Private Sub UpdateInitialValueListView(Optional ByVal selectedIndex As Integer = -1)
        UpdateCollectionListView(lvInitialValue, DisplayedInitialValue, selectedIndex)
    End Sub

    ''' <summary>
    ''' Updates the listview showing current values.
    ''' </summary>
    ''' <param name="selectedIndex">The index to select. If not given, this will
    ''' leave the currently selected row selected.</param>
    Private Sub UpdateCurrentValueListView(Optional ByVal selectedIndex As Integer = -1)
        UpdateCollectionListView(lvCurrentValue, DisplayedCurrentValue, selectedIndex)
    End Sub

    ''' <summary>
    ''' Updates the given listview with the given collection, leaving the current row
    ''' selected.
    ''' </summary>
    ''' <param name="lview">The listview to update</param>
    ''' <param name="coll">The collection to update from - if this is null, it will
    ''' have the effect of clearing the listview.</param>
    Private Sub UpdateCollectionListView(ByVal lview As ctlListView, ByVal coll As clsCollection)
        UpdateCollectionListView(lview, coll, -1)
    End Sub

    ''' <summary>
    ''' Updates the given listview with the given collection, selecting the index
    ''' specified or leaving the current row selected otherwise.
    ''' </summary>
    ''' <param name="lview">The listview to update</param>
    ''' <param name="coll">The collection to update from - if this is null, it will
    ''' have the effect of clearing the listview.</param>
    ''' <param name="selectedIndex">The index to select for editing, or -1 if it
    ''' should be left in its current state.</param>
    Private Sub UpdateCollectionListView(ByVal lview As ctlListView, ByVal coll As clsCollection, ByVal selectedIndex As Integer)

        lview.Rows.Clear()
        lview.Columns.Clear()

        If coll IsNot Nothing Then

            For Each fld As clsCollectionFieldInfo In coll.FieldDefinitions
                ' Add the column, setting its tag to the collection field
                lview.Columns.Add(GetFieldHeader(fld.Name, fld.DataType)).Tag = fld
            Next

            Dim listRow As clsListRow = Nothing
            Dim rowsToAdd As New List(Of clsListRow)

            For Each collRow As clsCollectionRow In coll.Rows
                listRow = New clsListRow(lview)
                For Each fldDefn As clsCollectionFieldInfo In coll.FieldDefinitions
                    Dim val As clsProcessValue = Nothing
                    If collRow.Contains(fldDefn.Name) Then
                        val = collRow(fldDefn.Name)
                    End If

                    If val Is Nothing Then
                        'If we do not have a value then create one
                        val = New clsProcessValue(fldDefn.DataType, "")

                    ElseIf val.DataType <> fldDefn.DataType Then
                        'If data types do not match then try casting
                        Try
                            val = val.CastInto(fldDefn.DataType)
                        Catch ex As Exception
                            'If we cannot cast then just create a new one.
                            val = New clsProcessValue(fldDefn.DataType)
                        End Try
                    End If

                    collRow(fldDefn.Name) = val
                    listRow.Items.Add(New clsListItem(listRow, val))
                Next

                'Store a reference to the row in the tag so that we 
                'can find it later.
                listRow.Tag = collRow
                rowsToAdd.Add(listRow)
            Next
            lview.Rows.AddRange(rowsToAdd)

            'Auto size the columns
            For Each c As ctlListColumn In lview.Columns
                c.Width = -3
            Next

            Dim count = 0
            For Each fld As clsCollectionFieldInfo In coll.FieldDefinitions
                Select Case fld.DataType
                    Case DataType.binary
                        lview.Columns(count).Width = 430
                    Case DataType.image
                        lview.Columns(count).Width = 400
                End Select
                count += 1
            Next

            lview.UpdateView()
            Select Case True
                Case (selectedIndex >= 0) AndAlso (selectedIndex < lview.Rows.Count)
                    lview.CurrentEditableRow = lview.Rows(selectedIndex)
                    lview.CurrentEditableRow.EnsureVisible()
                Case Else
                    If listRow IsNot Nothing Then
                        lview.CurrentEditableRow = listRow
                        listRow.EnsureVisible()
                    End If
            End Select
        End If

    End Sub

#End Region

    ''' <summary>
    ''' Makes sure the inital value listview is updated when the tabcontrol is
    ''' chnged to this tab.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleTabChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles tabFieldsAndValues.SelectedIndexChanged
        lvCurrentValue.EndEditing()
        lvInitialValue.EndEditing()
        lvCollectionFields.EndEditing()
        If tabFieldsAndValues.SelectedIndex <> 0 Then
            UpdateInitialValueListView()
            UpdateCurrentValueListView()
        End If
    End Sub

#Region " Handle Structure Changes "

    ''' <summary>
    ''' Clears all the fields from the collection.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnClearField_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnClearFields.Click
        Manager.ClearFields()
        UpdateFieldsListView()
        UpdateInitialValueListView()
    End Sub

    ''' <summary>
    ''' Shows the import wizard, when the import button is clicked
    ''' </summary>
    Private Sub btnImportField_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnImport.Click
        ' We don't need to clone the manager or stage objects.
        ' The manager is only updated by the form if OK is pressed; The stage is
        ' only used to load the business objects
        Dim f As New frmImportCollectionDetails(Manager, Stage)
        f.SetEnvironmentColours(Me)
        f.ShowInTaskbar = False
        If f.ShowDialog() = DialogResult.OK Then
            UpdateFieldsListView()
            lvInitialValue.Rows.Clear()
            lvInitialValue.UpdateView()
        End If
        f.Dispose()
    End Sub

    ''' <summary>
    ''' Handles the 'Remove Field' button being clicked.
    ''' </summary>
    Private Sub btnRemoveField_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveField.Click

        Dim row As clsCollectionFieldListRow =
         TryCast(lvCollectionFields.CurrentEditableRow, clsCollectionFieldListRow)
        If row Is Nothing Then
            UserMessage.Show(
             My.Resources.frmStagePropertiesCollection_PleaseFirstSelectARowToRemoveByPlacingTheMouseCursorInTheFirstColumnOfThatRow)
            Return
        End If

        'Delete the field from the definition...
        Manager.DeleteField(row.CollectionField.Name)

        'Update the list...
        lvCollectionFields.CurrentEditableRow.Remove()
        lvCollectionFields.UpdateView()

        UpdateSingleRowState()

    End Sub

#End Region

#Region " Remove Row (Initial/Current Value) "

    ''' <summary>
    ''' Handles 'Remove Row' being clicked on Initial Value tab
    ''' </summary>
    Private Sub btnRemoveInitialValueRow_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveInitialValueRow.Click
        RemoveRow(lvInitialValue, DisplayedInitialValue)
    End Sub

    ''' <summary>
    ''' Handles 'Remove Row' being clicked on Current Value tab
    ''' </summary>
    Private Sub btnRemoveCurrentValueRow_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveCurrentValueRow.Click
        RemoveRow(lvCurrentValue, DisplayedCurrentValue)
    End Sub

    ''' <summary>
    ''' Removes the row from the specified collection which corresponds to the
    ''' selected (or last) row in the supplied listview.
    ''' </summary>
    ''' <param name="lview">The listview row which conains the row to
    ''' be removed from the collection.</param>
    ''' <param name="coll">The clsCollection from which the row is
    ''' to be removed.</param>
    Private Sub RemoveRow(ByVal lview As ctlListView, ByVal coll As clsCollection)

        'Choose row to delete - the selected row or the last one
        Dim lviewRow As clsListRow = lview.CurrentEditableRow
        If lviewRow Is Nothing Then lviewRow = lview.LastRow

        If lviewRow IsNot Nothing Then

            Dim row As clsCollectionRow = TryCast(lviewRow.Tag, clsCollectionRow)
            If coll.Contains(row) Then coll.Remove(row)
            lviewRow.Remove()
            lview.UpdateView()
        End If
    End Sub

#End Region

#Region " Add Value (Initial/Current Value) "

    ''' <summary>
    ''' Handles 'Add Row' being added on initial value tab
    ''' </summary>
    Private Sub btnAddInitialValueRow_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddInitialValueRow.Click
        AddRow(lvInitialValue, DisplayedInitialValue)
    End Sub

    ''' <summary>
    ''' Handles 'Add Row' being added on current value tab
    ''' </summary>
    Private Sub btnAddCurrentValueRow_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddCurrentValueRow.Click
        AddRow(lvCurrentValue, DisplayedCurrentValue)
    End Sub

    ''' <summary>
    ''' Adds a row into the given collection, corresponding to the given listview,
    ''' and updates the listview with the new row.
    ''' </summary>
    ''' <param name="lview">The listview being added to.</param>
    ''' <param name="coll">The collection to which it should be added.</param>
    Private Sub AddRow(ByVal lview As ctlListView, ByVal coll As clsCollection)
        Dim index As Integer
        If lview.CurrentEditableRow Is Nothing Then
            index = lview.Rows.Count
        Else
            index = 1 + lview.CurrentEditableRow.Index
        End If
        Try
            coll.Insert(index)
            UpdateCollectionListView(lview, coll, index)

        Catch ede As EmptyDefinitionException
            UserMessage.Show(My.Resources.frmStagePropertiesCollection_YouMustDefineAtLeastOneFieldBeforeAddingARow)

        End Try
    End Sub

#End Region

    ''' <summary>
    ''' Handles the tab control deselecting a particular page.
    ''' This just ensures that there are no errors in the setup of the collection
    ''' fields before continuing to another tab.
    ''' </summary>
    Private Sub HandleDeselecting(ByVal sender As Object, ByVal e As TabControlCancelEventArgs) _
     Handles tabFieldsAndValues.Deselecting

        If Object.ReferenceEquals(e.TabPage, tbFields) Then
            Dim names As ICollection(Of String) = Stage.FindRepeatedNames()
            If names.Count > 0 Then
                e.Cancel = True
                Dim sb As New StringBuilder(My.Resources.frmStagePropertiesCollection_DuplicateFieldNamesHaveBeenDiscovered)
                sb.Append(vbCrLf) _
                 .Append(My.Resources.frmStagePropertiesCollection_PleaseCorrectThisBeforeContinuingTheNamesAre).Append(vbCrLf)
                For Each name As String In names
                    sb.Append("* ").Append(name).Append(vbCrLf)
                Next
                UserMessage.Show(sb.ToString())
            End If
        End If

    End Sub

    ''' <summary>
    ''' Handles the editable row changing in the current and initial value lists.
    ''' </summary>
    ''' <remarks>This ensures that the collection row that the row represents is
    ''' updated with the new values from the row</remarks>
    Private Sub HandleValueChanged(
     ByVal sender As Object, ByVal e As ListRowChangedEventArgs) _
     Handles lvCurrentValue.EditableRowChanged, lvInitialValue.EditableRowChanged

        Dim oldRow As clsListRow = e.OldRow
        If oldRow Is Nothing Then Return

        Dim row As clsCollectionRow = TryCast(oldRow.Tag, clsCollectionRow)
        Debug.Assert(row IsNot Nothing)
        If row Is Nothing Then Return

        Dim lv As ctlListView = oldRow.Owner
        For Each col As ctlListColumn In lv.Columns
            Dim fld As clsCollectionFieldInfo = TryCast(col.Tag, clsCollectionFieldInfo)

            If fld Is Nothing Then Throw New NullReferenceException(
                String.Format(My.Resources.frmStagePropertiesCollection_CollectionFieldForColumn0NotSet, col.Text))
            If row.ContainsKey(fld.Name) Then row(fld.Name) = oldRow.Items(col.Index).Value
        Next

    End Sub

    ''' <summary>
    ''' Updates the state of the controls on the page depending on the edit mode
    ''' being entered, and the list view being navigated.
    ''' Note that we cannot use the <see cref="CurrentMode"/> safely here, since it
    ''' uses the state of the GUI to define the mode, and we are here to change the
    ''' state of the GUI to match the mode being transitioned to.
    ''' </summary>
    ''' <param name="navigatingListView">The list view being navigated - null 
    ''' indicates that we are at top level, the initial value listview indicates
    ''' that we are navigating the initial value collection, the current value
    ''' listview indicates that we are navigating the current value collection.
    ''' </param>
    Private Sub UpdateControlState(ByVal navigatingListView As ctlListView)

        If mBreadcrumbs.Count = 1 Then ' We're showing the top 'root' collection
            mBreadcrumbs.Visible = False
            txtName.ReadOnly = False
            txtDescription.ReadOnly = False
            chkAlwaysInit.Enabled = True
            chkPrivate.Enabled = True

            tbInitValue.Enabled = True
            tbCurrent.Enabled = ShouldShowCurrentValue()
            lvCollectionFields.Readonly = False
            panFieldButtons.Enabled = True
            chkSingleRow.Enabled = True

        ElseIf Not mBreadcrumbs.Visible Then ' We're showing a child / grandchild / etc collection
            mBreadcrumbs.Visible = True
            txtName.ReadOnly = True
            txtDescription.ReadOnly = True
            chkAlwaysInit.Enabled = False
            chkPrivate.Enabled = False

            If navigatingListView IsNot lvCurrentValue Then
                ' Navigating fields / init values - disable the current values.
                tbCurrent.Enabled = False
            End If
            If navigatingListView IsNot lvInitialValue Then
                ' Navigating fields / current values - disable the init values.
                tbInitValue.Enabled = False
            End If
            If navigatingListView IsNot lvCollectionFields Then
                ' disable the fields for now...
                lvCollectionFields.Readonly = True
                panFieldButtons.Enabled = False
                chkSingleRow.Enabled = False
            End If
        End If

        chkSingleRow.Checked = Manager.SingleRow

    End Sub

    ''' <summary>
    ''' Resets the state of the dialog to the top level - ie. the stage
    ''' </summary>
    Private Sub ResetToTop()

        ' Get the current mode - by popping back to the top, we will be setting the
        ' current mode to stage, so we need to save it.
        Dim mode As EditMode = CurrentMode

        ' Pop the breadcrumbs to the root
        Dim crumbs As List(Of KeyValuePair(Of String, Object)) = mBreadcrumbs.Pop(mBreadcrumbs.Count - 1)

        If mode = EditMode.Stage Then StoreNestedChanges(crumbs)

        ' Hide the breadcrumb control
        UpdateControlState(Nothing)

        ' Update the fields
        UpdateFieldsListView()
        UpdateInitialValueListView()
        UpdateCurrentValueListView()
    End Sub

    ''' <summary>
    ''' Stores the changes from the given structure breadcrumbs.
    ''' This is expected to be in the format as returned by the breadcrumb trail
    ''' control - ie. the first entry in the map should be the last entry added
    ''' to the breadcrumbs.
    ''' This applies all changes from each level in the dictionary to the parent
    ''' level, up to and including the current top level in the breadcrumbs.
    ''' </summary>
    ''' <param name="crumbs">The list of labels to clsCollectionInfo objects
    ''' representing the nested field structures within the collection stage being
    ''' edited by this dialog.</param>
    Private Sub StoreNestedChanges(ByVal crumbs As List(Of KeyValuePair(Of String, Object)))

        ' Inappropriate structures make everything so hard in .net
        ' This always holds a value - the way to check if it is valid or
        ' not is to check the key - if that is null, then the pair is 'empty',
        ' since the label can never be null.
        Dim prev As New KeyValuePair(Of String, clsCollectionInfo)

        For Each crumb As KeyValuePair(Of String, Object) In crumbs
            Dim defn As clsCollectionInfo = DirectCast(crumb.Value, clsCollectionInfo)
            ' So we need to apply this definition into the parent definition
            ' all the way up to the current definition.
            If prev.Key IsNot Nothing Then
                SetInto(defn, prev)
            End If
            prev = New KeyValuePair(Of String, clsCollectionInfo)(crumb.Key, defn)
        Next
        ' Last one - we need to update the 'current' definition - ie. the one
        ' popped to with the last one popped off the breadcrumb trail
        If prev.Key IsNot Nothing Then
            SetInto(DirectCast(mBreadcrumbs.TopTag, ICollectionDefinitionManager), prev)
        End If

    End Sub

    ''' <summary>
    ''' Handles a breadcrumb link being clicked on the breadcrumb control.
    ''' </summary>
    ''' <param name="sender">The breadcrumb trail control which has had a crumb
    ''' activated on it.</param>
    ''' <param name="label">The label that was activated.</param>
    ''' <param name="tag">The tag data associated with the activated crumb.</param>
    Private Sub HandleBreadcrumbActivated(
     ByVal sender As Object, ByVal label As String, ByVal tag As Object) _
     Handles mBreadcrumbs.BreadcrumbClicked

        ' Save the mode first - by popping the breadcrumb trail, we could end up
        ' changing the mode (only 1 crumb means 'stage edit' regardless of where
        ' we were before).
        Dim mode As EditMode = CurrentMode

        ' ok - we need to pop the breadcrumbs back to the given value.
        mBreadcrumbs.PopTo(label, tag)

        ' If we're down to the last collection, hide the breadcrumbs
        UpdateControlState(Nothing)

        UpdateFieldsListView()
        UpdateInitialValueListView()
        UpdateCurrentValueListView()

    End Sub

    ''' <summary>
    ''' Sets the given child field information into the corresponding field within
    ''' the specified parent manager.
    ''' </summary>
    ''' <param name="parentMgr">The manager which is holding the field which needs
    ''' to be updated with the data from the child.</param>
    ''' <param name="childPair">A String/Collection definition pair with the name of
    ''' the child field and the collection definition for that field (ie. the value
    ''' of the field's <see cref="clsCollectionFieldInfo.Children"/> property).
    ''' If the key in this pair is null, it is treated as an empty pair - ie. this
    ''' method performs no action.
    ''' </param>
    Private Sub SetInto(
     ByVal parentMgr As ICollectionDefinitionManager,
     ByVal childPair As KeyValuePair(Of String, clsCollectionInfo))

        ' If there's no label, the pair is empty. Do nothing.
        If childPair.Key Is Nothing Then Return
        ' Get the field that the child pair represents
        Dim fld As clsCollectionFieldInfo = parentMgr.GetFieldDefinition(childPair.Key)
        ' The collection definition in the pair represents the children of
        ' that field - clear what's already there and set the new values into
        ' the parent's definition for that field.
        fld.Children.SetFrom(childPair.Value)
    End Sub

    ''' <summary>
    ''' Handles a collection value control being activated - that is, it handles
    ''' the link representing a collection being activated.
    ''' </summary>
    ''' <param name="sender">The listview on which a value was activated.</param>
    ''' <param name="name">The name of the value that was clicked - generally the
    ''' column name within the collection that was clicked. Note that for the
    ''' value listviews, this will be in the form: "{name} (collection)" and the
    ''' name may need to be extracted from that.</param>
    ''' <param name="ctl">The process value control which has been activated.
    ''' </param>
    Private Sub HandleValueControlActivated(
     ByVal sender As ctlListView, ByVal name As String, ByVal ctl As IActivatableProcessValue) _
     Handles lvCurrentValue.ValueControlActivated, lvInitialValue.ValueControlActivated

        ' Extract the name from the regular expression
        name = CollNameRegex.Match(name).Groups(1).ToString()

        Dim coll = UpdateColumnDefinitions(name, ctl)
        mBreadcrumbs.Push(name, coll)
        UpdateControlState(sender)

        UpdateFieldsListView()
        UpdateInitialValueListView()
        UpdateCurrentValueListView()

    End Sub

    ''' <summary>
    ''' Checks and updates the field definitions of all child field defintions.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="ctl"></param>
    ''' <returns></returns>
    Private Function UpdateColumnDefinitions(name As String, ctl As IActivatableProcessValue) As clsCollection
        Dim v As clsProcessValue = ctl.Value

        If v.Collection Is Nothing OrElse v.Collection.FieldCount = 0 Then
            v.Collection = New clsCollection()
        End If

        v.Collection.UpdateDefinitionsChildren(Manager.GetFieldDefinition(name)?.Children)
        Return v.Collection
    End Function

    ''' <summary>
    ''' Handles a collection field control being activated, that is, it handles the
    ''' link representing the structure of a collection field being activated.
    ''' </summary>
    ''' <param name="sender">The listview on which the field was activated.</param>
    ''' <param name="name">The column name within the listview of the activated
    ''' item.</param>
    ''' <param name="ctl">The process value control which has been activated.
    ''' </param>
    Private Sub HandleFieldControlActivated(
     ByVal sender As ctlListView, ByVal name As String, ByVal ctl As IActivatableProcessValue) _
     Handles lvCollectionFields.ValueControlActivated

        ' "name" represents the name of the column, which is nice and all, but
        ' useless to us in determining the name of the field (which is defined
        ' in the row in this listview, not the column)

        ' You can only activate the current editable row, so pick out the name from
        ' the listview which sent the event.
        ' First commit the changes (so that the collection definition's field name
        ' matches up to the listview)
        sender.CommitEditableRow()
        Dim clickedField As clsCollectionFieldInfo =
         DirectCast(sender.CurrentEditableRow, clsCollectionFieldListRow).CollectionField
        name = clickedField.Name




        mBreadcrumbs.Push(name, clickedField.Children)
        UpdateControlState(sender)

        UpdateFieldsListView()
        UpdateInitialValueListView()
        UpdateCurrentValueListView()

    End Sub


End Class
