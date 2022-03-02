Imports AutomateUI.Controls.Widgets.SystemManager.WebApi
Imports AutomateUI.Controls.Widgets.SystemManager.WebApi.Request
Imports BluePrism.BPCoreLib.Collections

Public Class WebApiCommonCodePanel : Implements IGuidanceProvider

    Private mCommonCode As CodePropertiesDetails

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        cmbLanguage.DataSource = GetLanguages().ToList()

    End Sub

    Private Iterator Function GetLanguages() As IEnumerable(Of BluePrism.AutomateProcessCore.Compilation.CodeLanguage)
        Yield BluePrism.AutomateProcessCore.Compilation.CodeLanguage.VisualBasic
        Yield BluePrism.AutomateProcessCore.Compilation.CodeLanguage.CSharp
    End Function

    Friend WriteOnly Property CommonCode As CodePropertiesDetails
        Set(value As CodePropertiesDetails)
            mCommonCode = value
            PopulatePanel()
        End Set
    End Property

    Public ReadOnly Property GuidanceText As String Implements IGuidanceProvider.GuidanceText
        Get
            Return WebApi_Resources.GuidanceCommonCodePanel
        End Get
    End Property

    ''' <summary>
    ''' Populates the user control panel with the data from the CodePropertyDetails instance.
    ''' </summary>
    Private Sub PopulatePanel()
        If mCommonCode Is Nothing Then Throw New NullReferenceException(NameOf(mCommonCode))

        'Set up headers
        lstDlls.Columns.Add(WebApi_Resources.ExternalReferences)
        lstDlls.LastColumnAutoSize = True

        lstNamespaces.Columns.Add(WebApi_Resources.NamespaceImports)
        lstNamespaces.LastColumnAutoSize = True

        'For each reference populate listview control with entry
        For Each s As String In mCommonCode.References
            AddSingleLineListRow(lstDlls, s)
        Next
        'If no references exist, add a row by default
        If lstDlls.Rows.Count = 0 Then AddSingleLineListRow(lstDlls, "")

        'For each import populate listview control with entry
        For Each s As String In mCommonCode.Namespaces
            AddSingleLineListRow(lstNamespaces, s)
        Next
        'If no namespaces exist, add a row by default
        If lstNamespaces.Rows.Count = 0 Then AddSingleLineListRow(lstNamespaces, "")

        cmbLanguage.SelectedItem = mCommonCode.Language

        ctlEditor.Populate(mCommonCode.Code, mCommonCode.Language.Name)

        lstDlls.UpdateView()
        lstNamespaces.UpdateView()
        UpdateButtons()

    End Sub

    ''' <summary>
    ''' Adds a single line text value into a listrow, which it then adds into the
    ''' given listview.
    ''' </summary>
    ''' <param name="lv">The ListView to add the row into</param>
    ''' <param name="val">The value to set in the item within the row</param>
    ''' <returns>The listrow created and added into the listview</returns>
    Private Function AddSingleLineListRow(
     ByVal lv As ctlListView, ByVal val As String) As clsListRow
        Dim row As New clsListRow(lv)
        row.AddItem(val, False)
        lv.Rows.Add(row)
        Return row
    End Function

    ''' <summary>
    ''' Makes buttons enabled/disabled according to the status
    ''' of the application.
    ''' </summary>
    Private Sub UpdateButtons()
        btnRemoveRef.Enabled = (lstDlls.CurrentEditableRow IsNot Nothing)
        btnRemoveNamespace.Enabled = (lstNamespaces.CurrentEditableRow IsNot Nothing)
        btnBrowse.Enabled = btnRemoveRef.Enabled
    End Sub

    Private Sub btnCheckCode_Click(sender As Object, e As EventArgs) _
        Handles btnCheckCode.Click

        CodeValidationHelper.Validate(mCommonCode, {},
                                      Sub(m) UserMessage.Show(m), Sub(m) UserMessage.Err(m))

    End Sub

    ''' <summary>
    ''' Handles the Browse button for DLL references being pressed
    ''' </summary>
    Private Sub HandleBrowseDll(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnBrowse.Click

        ' Might as well check that we have somewhere to put a DLL ref before we start
        Dim currRow As clsListRow = lstDlls.CurrentEditableRow
        If currRow Is Nothing Then '
            UserMessage.Show(WebApi_Resources.CommonCode_Unable_To_Populate_Field)
            Return
        End If

        ' Capture the DLL that we want to add a reference to
        Using fo As New OpenFileDialog()
            With fo
                .AddExtension = True
                .CheckFileExists = True
                .CheckPathExists = True
                .DereferenceLinks = True
                .Filter = "Dynamic Link Libraries (*.dll)|*.dll"
                .Multiselect = False
                .InitialDirectory = Environment.SystemDirectory &
                 "\..\Microsoft.NET\Framework\v" & Environment.Version.ToString(3)
            End With

            If fo.ShowDialog() <> DialogResult.OK Then Return

            Dim ref As String = fo.FileName
            If ref = "" Then Return

            lstDlls.EditRowControl().Items(0).NestedControl.Text = ref

        End Using
    End Sub

    ''' <summary>
    ''' Event fired when the user wants to add a row to the Dll's
    ''' </summary>
    Private Sub btnAddNamespace_Click(sender As Object, e As EventArgs) _
        Handles btnAddNamespace.Click

        AddNewRow(lstNamespaces)
        UpdateDependancies()
    End Sub

    ''' <summary>
    ''' Event fired when the user wants to remove a row from the names spaces.
    ''' </summary>
    Private Sub btnRemoveNamespace_Click(sender As Object, e As EventArgs) _
        Handles btnRemoveNamespace.Click

        RemoveCurrentRow(lstNamespaces)
        UpdateDependancies()
    End Sub

    ''' <summary>
    ''' Event fired when the user wants to add a row to the Dll's
    ''' </summary>
    Private Sub btnAddReference_Click(sender As Object, e As EventArgs) _
        Handles btnAddReference.Click

        AddNewRow(lstDlls)
        UpdateDependancies()
    End Sub

    ''' <summary>
    ''' Event fired when the user wants to remove a row from the Dll's
    ''' </summary>
    Private Sub btnRemoveRef_Click(sender As Object, e As EventArgs) _
        Handles btnRemoveRef.Click

        RemoveCurrentRow(lstDlls)
        UpdateDependancies()
    End Sub

    ''' <summary>
    ''' Event fired when the user leaves the Dll List control.
    ''' </summary>
    Private Sub lstDlls_Validated(sender As Object, e As EventArgs) _
        Handles lstDlls.Validated

        UpdateDependancies()
    End Sub

    ''' <summary>
    ''' Event fired when the user leaves the Namespaces List control.
    ''' </summary>
    Private Sub lstNamespaces_Validated(sender As Object, e As EventArgs) _
        Handles lstNamespaces.Validated

        UpdateDependancies()
    End Sub

    ''' <summary>
    ''' Event fired when the user changes the chosen language.
    ''' </summary>
    Private Sub cmbLanguage_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbLanguage.SelectedIndexChanged

        Dim language = DirectCast(cmbLanguage.SelectedValue, BluePrism.AutomateProcessCore.Compilation.CodeLanguage)
        If mCommonCode IsNot Nothing Then
            mCommonCode.Language = language
            ctlEditor.Populate(mCommonCode.Code, language.Name)
        End If
    End Sub

    ''' <summary>
    ''' Event fired when the user either leaves the code editing control.
    ''' </summary>
    Private Sub mEditor_Validating(sender As Object, e As CancelEventArgs) _
        Handles ctlEditor.Validating

        mCommonCode.Code = ctlEditor.Code
    End Sub

    ''' <summary>
    ''' Event that fires when the user leaves the User Control, required to
    ''' update the Import and Dll lists.
    ''' </summary>
    Private Sub WebApiCommonCodePanel_Validating(sender As Object, e As CancelEventArgs) _
        Handles MyBase.Validating

        UpdateDependancies()
    End Sub

    ''' <summary>
    ''' Handles the selected row being changed on one of the listviews.
    ''' </summary>
    Private Sub HandleSelectedRowChanged( _
     ByVal sender As Object, ByVal e As ListRowChangedEventArgs) _
     Handles lstDlls.EditableRowChanged, lstNamespaces.EditableRowChanged

        UpdateButtons()
    End Sub

    ''' <summary>
    ''' Adds a new empty row to the given listview and focuses on it
    ''' </summary>
    ''' <param name="lv">The listview to which the new row should be added</param>
    Private Sub AddNewRow(ByVal lv As ctlListView)
        lv.CurrentEditableRow = AddSingleLineListRow(lv, "")
        lv.FocusEditColumn(0)
        lv.UpdateView()
        UpdateButtons()
    End Sub

    ''' <summary>
    ''' Removes the current editable row from the given listview.
    ''' </summary>
    ''' <param name="lv">The listview whose current editable row should be removed
    ''' </param>
    Private Sub RemoveCurrentRow(ByVal lv As ctlListView)
        lv.Rows.Remove(lv.CurrentEditableRow)
        lv.UpdateView()
        UpdateButtons()
    End Sub

    ''' <summary>
    ''' Updates the CodePropertyDetails references and namespaces.
    ''' </summary>
    Private Sub UpdateDependancies()
        mCommonCode.References.Clear()
        mCommonCode.Namespaces.Clear()

        mCommonCode.References.AddAll(GetCodeDependancy(lstDlls))
        mCommonCode.Namespaces.AddAll(GetCodeDependancy(lstNamespaces))
    End Sub

    ''' <summary>
    ''' Commits any changes that are currently occuring in the list, this is necessary
    ''' as any current editing will not be committed, then retrieves a list of items
    ''' from the chosen ctlListView.
    ''' </summary>
    Private Function GetCodeDependancy(list As ctlListView) As IReadOnlyCollection(Of String)

        ' Commit any changes that currently being made on the control.
        list.EndEditing()

        Return list.Rows.Select(Function(r) CStr(r.Items(0).Value).Trim()).
            Where(Function(s) s <> "").ToList()

    End Function

End Class
