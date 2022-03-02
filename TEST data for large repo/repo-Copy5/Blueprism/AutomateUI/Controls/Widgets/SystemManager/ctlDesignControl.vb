
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Control to capture design control changes.
''' </summary>
Public Class ctlDesignControl

#Region " Class-scope Definitions "

    ''' <summary>
    ''' A wrapper for a control collection which can be used when removing certain
    ''' controls from an existing control collection.
    ''' </summary>
    Private Class BetterControlCollection : Inherits List(Of Control)
        Public Sub New(ByVal coll As ControlCollection)
            For Each ctl As Control In coll
                Add(ctl)
            Next
        End Sub
    End Class

    ''' <summary>
    ''' A structure combining the category and severity into a single unit
    ''' </summary>
    Private Structure CategoryType
        Public Category As Integer
        Public Type As Integer
    End Structure

#End Region

#Region " Member Variables "

    ' The controls which should not be removed from the table panel when this control
    ' is being populated.
    Private mConstantControls As ICollection(Of Control)

    ' The prefs which govern autovalidation settings mapped against the checkboxes
    ' which alter those settings.
    Private mAutoValidatePrefs As IDictionary(Of CheckBox, String)

    ' Status flag indicating whether the validate checkboxes are currently being checked
    Private mCheckingValidateCheckboxes As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new design control panel.
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        ' Set up the controls which should not be removed when the main panel is repopulated
        mConstantControls = New clsSet(Of Control)
        For Each ctl As Control In panMain.Controls
            mConstantControls.Add(ctl)
        Next

        ' Add the prefs mapped to the checkboxes which represent them
        mAutoValidatePrefs = New Dictionary(Of CheckBox, String)
        mAutoValidatePrefs(cbValidateOnOpen) = PreferenceNames.AutoValidation.AutoValidateOnOpen
        mAutoValidatePrefs(cbValidateOnReset) = PreferenceNames.AutoValidation.AutoValidateOnReset
        mAutoValidatePrefs(cbValidateOnSave) = PreferenceNames.AutoValidation.AutoValidateOnSave

        ' Add padding to the table panel to ensure no horizontal scrollbar
        panMain.Padding = New Padding(0, 0, SystemInformation.VerticalScrollBarWidth, 0)

    End Sub

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Populates this panel from the database.
    ''' </summary>
    Public Sub Populate()
        panMain.SuspendLayout()
        Try
            ' Clear the existing stuff in the panel
            For Each c As Control In New BetterControlCollection(panMain.Controls)
                If Not mConstantControls.Contains(c) Then panMain.Controls.Remove(c)
            Next
            panMain.RowCount = 1

            ' Get the data
            Dim allowModify As Boolean =
               User.Current.HasPermission(Permission.SystemManager.Audit.ConfigureDesignControls)
            EnableFormElementsBasedOnPermissions(allowModify)

            Dim categories As IDictionary(Of Integer, String) = gSv.GetValidationCategories()
            Dim types As IDictionary(Of Integer, String) = gSv.GetValidationTypes()
            Dim actions As IDictionary(Of Integer, String) = gSv.GetValidationActions()
            Dim actionSettings As IDictionary(Of Integer, IDictionary(Of Integer, Integer)) =
             gSv.GetValidationAllActionSettings()

            ' Go through the retrieved data - 
            ' for each category add a label, an Advanced button and label/combo pairs
            ' for each severity in there, replacing the combo with a read-only textbox
            ' if the user doesn't have modify rights.
            Dim ct As CategoryType
            For Each ct.Category In categories.Keys

                ' Separate from the previous category / headers
                AddSeparator(NewRow())

                ' Add the rows required for this category
                Dim rows As New List(Of Integer)
                Dim sevCount As Integer = types.Count
                While sevCount > 0
                    rows.Add(NewRow())
                    sevCount -= 1
                End While

                ' Top left label
                AddLabel(categories(ct.Category), FontStyle.Bold, 2, 1, rows(0), 0)

                ' Bottom left button - tag it with the category ID
                AddAdvancedButton(My.Resources.Advanced, CollectionUtil.Last(rows), 0).Tag = ct.Category

                ' For Each type add a label and a combo box in columns 1 & 2 respectively
                ' The label represents the type, the combo the restricted actions
                Dim i As Integer = 0
                For Each ct.Type In types.Keys
                    AddLabel(types(ct.Type), rows(i), 1)
                    Dim selectedAction As String = actions(actionSettings(ct.Category)(ct.Type))
                    If allowModify Then
                        AddCombo(actions.Values, selectedAction, rows(i), 2).Tag = ct
                    Else
                        AddReadOnlyText(selectedAction, rows(i), 2)
                    End If
                    ' Move onto the next row
                    i += 1
                Next
            Next

            ' Now deal with the checkboxes
            For Each cb As CheckBox In mAutoValidatePrefs.Keys
                cb.Checked = gSv.GetPref(mAutoValidatePrefs(cb), True)
            Next

        Finally
            panMain.ResumeLayout()
        End Try
    End Sub

    Private Sub EnableFormElementsBasedOnPermissions(allowModify As Boolean)
        llExport.Enabled = allowModify
        llImport.Enabled = allowModify
        cbValidateOnOpen.Enabled = allowModify
        cbValidateOnReset.Enabled = allowModify
        cbValidateOnSave.Enabled = allowModify
    End Sub

#End Region

#Region " Table Panel manipulators "

    ''' <summary>
    ''' Creates a new row in the table layout panel.
    ''' </summary>
    ''' <returns>The index of the new row</returns>
    Private Function NewRow() As Integer
        Dim row As Integer = panMain.RowCount
        panMain.RowCount = row + 1
        Return row
    End Function

    ''' <summary>
    ''' Adds a horizontal rule on the given row of the table panel, which spans all
    ''' columns in the panel
    ''' </summary>
    ''' <param name="row">The row index on which the separator should be added.
    ''' </param>
    ''' <returns>The separator control after being added to the panel.</returns>
    Private Function AddSeparator(ByVal row As Integer) As Control
        Dim hr As New AutomateControls.Line3D()
        hr.Dock = DockStyle.Top
        panMain.Controls.Add(hr, 0, row)
        panMain.SetColumnSpan(hr, panMain.ColumnCount)
        Return hr
    End Function

    ''' <summary>
    ''' Adds a label to the table panel
    ''' </summary>
    ''' <param name="text">The text to display on the control</param>
    ''' <param name="style">The font style to use on the control</param>
    ''' <param name="rowspan">The number of rows the control should span.</param>
    ''' <param name="colspan">The number of columns the control should span.</param>
    ''' <param name="row">The row index at which the control should be added.</param>
    ''' <param name="col">The column index at which the control should be added.
    ''' </param>
    ''' <returns>The control after it is added to the panel</returns>
    Private Function AddLabel(
     ByVal text As String, ByVal style As FontStyle,
     ByVal rowspan As Integer, ByVal colspan As Integer,
     ByVal row As Integer, ByVal col As Integer) As Label
        Dim lbl As New Label()
        lbl.Text = text
        If style <> lbl.Font.Style Then lbl.Font = New Font(lbl.Font, style)
        lbl.Padding = lblCategory.Padding
        lbl.Dock = DockStyle.Fill
        panMain.Controls.Add(lbl, col, row)
        panMain.SetRowSpan(lbl, rowspan)
        panMain.SetColumnSpan(lbl, colspan)
        Return lbl
    End Function

    ''' <summary>
    ''' Adds a label to the table panel, using the default font style, and occupying
    ''' a single cell in the table.
    ''' </summary>
    ''' <param name="text">The text to display on the control</param>
    ''' <param name="row">The row index at which the control should be added.</param>
    ''' <param name="col">The column index at which the control should be added.
    ''' </param>
    ''' <returns>The control after it is added to the panel</returns>
    Private Function AddLabel(ByVal text As String, ByVal row As Integer, ByVal col As Integer) _
      As Label
        Return AddLabel(text, FontStyle.Regular, 1, 1, row, col)
    End Function

    ''' <summary>
    ''' Adds a button representing 'Advanced' settings to the table panel
    ''' </summary>
    ''' <param name="text">The text to display on the control</param>
    ''' <param name="row">The row index at which the control should be added.</param>
    ''' <param name="col">The column index at which the control should be added.
    ''' </param>
    ''' <returns>The control after it is added to the panel</returns>
    Private Function AddAdvancedButton(
     ByVal text As String, ByVal row As Integer, ByVal col As Integer) As Button
        Dim btn As New AutomateControls.Buttons.StandardStyledButton()
        btn.Text = text
        btn.Margin = lblCategory.Padding
        btn.AutoSize = True

        panMain.Controls.Add(btn, col, row)
        AddHandler btn.Click, AddressOf HandleAdvancedButtonPressed

        Return btn
    End Function

    ''' <summary>
    ''' Adds a combo box to the panel
    ''' </summary>
    ''' <param name="entries">The entries which should be added to the combo box.
    ''' </param>
    ''' <param name="selectedEntry">The entry which should be selected on the combo
    ''' box initially.</param>
    ''' <param name="row">The row index at which the control should be added.</param>
    ''' <param name="col">The column index at which the control should be added.
    ''' </param>
    ''' <returns>The control after it is added to the panel</returns>
    Private Function AddCombo(
     ByVal entries As ICollection(Of String), ByVal selectedEntry As String,
     ByVal row As Integer, ByVal col As Integer) As ComboBox
        Dim combo As New ComboBox()
        combo.DropDownStyle = ComboBoxStyle.DropDownList
        combo.Dock = DockStyle.Top
        For Each entry As String In entries
            combo.Items.Add(entry)
        Next
        combo.Text = selectedEntry
        combo.Margin = lblCategory.Padding
        panMain.Controls.Add(combo, col, row)
        AddHandler combo.SelectedIndexChanged, AddressOf HandleActionChanged
        Return combo
    End Function

    ''' <summary>
    ''' Adds a read only text box to the table panel
    ''' </summary>
    ''' <param name="text">The text to display on the control</param>
    ''' <param name="row">The row index at which the control should be added.</param>
    ''' <param name="col">The column index at which the control should be added.
    ''' </param>
    ''' <returns>The control after it is added to the panel</returns>
    Private Function AddReadOnlyText(
     ByVal text As String, ByVal row As Integer, ByVal col As Integer) As TextBox
        Dim txt As New AutomateControls.Textboxes.StyledTextBox
        txt.ReadOnly = True
        txt.Dock = DockStyle.Top
        txt.Margin = lblCategory.Padding
        txt.Text = text
        panMain.Controls.Add(txt, col, row)
        Return txt
    End Function

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles the validation action setting changing
    ''' </summary>
    Private Sub HandleActionChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim combo As ComboBox = DirectCast(sender, ComboBox)
        Dim ct As CategoryType = DirectCast(combo.Tag, CategoryType)
        gSv.SetValidationActionSetting(ct.Category, ct.Type, combo.SelectedIndex)
    End Sub

    ''' <summary>
    ''' Handles the click of the Design Control advanced buttons
    ''' </summary>
    Private Sub HandleAdvancedButtonPressed(ByVal sender As Object, ByVal e As EventArgs)
        Dim c As Control = TryCast(sender, Control)
        Using f As New frmAdvancedDesignControl()
            f.Populate(DirectCast(c.Tag, Integer))
            f.SetEnvironmentColoursFromAncestor(Me)
            f.ShowInTaskbar = False
            f.ShowDialog()
        End Using
    End Sub

    ''' <summary>
    ''' Handles the import link being clicked.
    ''' </summary>
    Private Sub llImport_LinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llImport.LinkClicked
        Dim openDialog As New OpenFileDialog
        openDialog.Filter = My.Resources.XMLFilesXmlXml
        If openDialog.ShowDialog() = DialogResult.OK Then
            Dim imp As New clsValidationInfoImportExport()
            Try
                imp.Import(openDialog.FileName)
            Catch ex As Exception
                UserMessage.ShowExceptionMessage(ex)
            End Try
            Populate()
        End If
    End Sub

    ''' <summary>
    ''' Handles the export link being clicked.
    ''' </summary>
    Private Sub llExport_LinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llExport.LinkClicked
        Dim dia As New SaveFileDialog
        dia.Filter = My.Resources.XMLFilesXmlXml
        If dia.ShowDialog = DialogResult.OK Then
            Dim exp As New clsValidationInfoImportExport()
            exp.Export(dia.FileName)
        End If
    End Sub

    ''' <summary>
    ''' Handles an auto-validate checkbox being set.
    ''' </summary>
    Private Sub HandleAutoValidateChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles cbValidateOnOpen.CheckedChanged, cbValidateOnReset.CheckedChanged, cbValidateOnSave.CheckedChanged
        If mCheckingValidateCheckboxes Then Return
        mCheckingValidateCheckboxes = True
        Try
            Dim cb As CheckBox = DirectCast(sender, CheckBox)
            If cb Is cbValidateOnSave AndAlso Not cb.Checked Then
                Dim res As DialogResult = MessageBox.Show(
                 My.Resources.DisablingValidationOnSaveWillMeanThatProcessesCanBeSavedWithErrorsRegardlessOfO,
                 My.Resources.AreYouSure, MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                If res <> DialogResult.OK Then
                    ' Set it back to enabled and geddahelloudahere
                    cb.Checked = True
                    Return
                End If
            End If
            gSv.SetSystemPref(mAutoValidatePrefs(cb), cb.Checked)
        Finally
            mCheckingValidateCheckboxes = False
        End Try
    End Sub

#End Region

End Class
