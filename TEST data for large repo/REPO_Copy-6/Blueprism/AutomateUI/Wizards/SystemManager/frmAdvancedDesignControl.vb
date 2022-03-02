Imports BluePrism.AutomateAppCore
Imports AutomateControls
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmAdvancedDesignControl
    Implements IHelp, IEnvironmentColourManager

    ''' <summary>
    ''' Provides a lookup to translate the friendly type names back to
    ''' a typeid
    ''' </summary>
    Private mTypeLookup As Dictionary(Of String, Integer)
    Private ReadOnly _permissionToEdit As Boolean = User.Current.HasPermission(Permission.SystemManager.Audit.ConfigureDesignControls)

    ''' <summary>
    ''' Populates the datatable with check descriptions and validation type combos
    ''' </summary>
    Public Sub Populate(ByVal catid As Integer)

        Dim categories As IDictionary(Of Integer, String) = gSv.GetValidationCategories()
        Me.titleBar.Title = categories(catid)

        Dim types As IDictionary(Of Integer, String) = gSv.GetValidationTypes()
        mTypeLookup = New Dictionary(Of String, Integer)
        For Each i As Integer In types.Keys
            Dim type As String = types(i)
            colSeverity.Items.Add(type)
            mTypeLookup.Add(type, i)
        Next

        Dim rules = gSv.GetValidationInfo()
        Dim validationInfo = rules.ToDictionary(Of Integer, clsValidationInfo)(Function(y) y.CheckID, Function(z) z)
        For Each info As clsValidationInfo In validationInfo.Values
            If info.CatID = catid Then
                Dim rowIndex As Integer = tblChecks.Rows.Add(
                 info.CheckID,
                 info.Enabled,
                 clsUtility.GetUnformattedString(info.Message),
                 types(info.TypeID))
                FormatRow(tblChecks.Rows(rowIndex))
            End If
        Next

        EnableFormElementsBasedOnPermissions(_permissionToEdit)
    End Sub

    Private Sub EnableFormElementsBasedOnPermissions(canModify As Boolean)
        btnCancel.Enabled = canModify
        btnOk.Enabled = canModify
    End Sub

    Private Sub FormatRow(ByVal row As DataGridViewRow)
        Dim enabled As Boolean = CBool(row.Cells(colEnabled.Name).Value) AndAlso _permissionToEdit

        ' Don't allow changes if this isn't enabled.
        row.ReadOnly = Not enabled

        ' Visually indicate the enabled state of the row
        With row.DefaultCellStyle
            If enabled Then
                ' Reset all colours to defaults
                .BackColor = Nothing
                .ForeColor = Nothing
                .SelectionBackColor = Nothing
                .SelectionForeColor = Nothing
            Else
                .BackColor = Color.FromArgb(240, 240, 240)
                .ForeColor = Color.DarkGray
                .SelectionBackColor = Color.LightBlue
                .SelectionForeColor = Color.Gray
            End If
        End With
    End Sub

    ''' <summary>
    ''' Handles a rule being enabled or disabled.
    ''' Note that although this handles the CellContentClick event on the data grid
    ''' view, it discards any events which aren't related to the 'Enabled' column.
    ''' </summary>
    Private Sub HandleRuleEnabled(ByVal sender As Object, ByVal e As DataGridViewCellEventArgs) _
     Handles tblChecks.CellContentClick

        If tblChecks.Columns(e.ColumnIndex) Is colEnabled AndAlso _permissionToEdit Then
            If e.RowIndex < 0 Then Return ' Column Click - not for us
            Dim row As DataGridViewRow = tblChecks.Rows(e.RowIndex)
            Dim cell As DataGridViewCell = row.Cells(e.ColumnIndex)

            ' Slightly unintuitively, at this point the cell's value is the opposite of what
            ' it will become after the cell content click event is completed.
            Dim enabled As Boolean = Not CBool(cell.Value)

            ' We set the value explicitly, because we want to force through a read-
            ' only row (it requires two clicks to enable a row if we leave it to
            ' winforms... I suspect due to read-only-ness of the row, but I'm not
            ' 100% sure - this works with a bit more certainty)
            cell.Value = enabled
            FormatRow(row)

        End If
    End Sub

    ''' <summary>
    ''' Updates the database with the users choices when they click OK.
    ''' </summary>
    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        Dim validationinfo As New List(Of clsValidationInfo)
        For Each row As DataGridViewRow In Me.tblChecks.Rows
            Dim v As New clsValidationInfo()
            v.CheckID = CInt(row.Cells(colCheckID.Name).Value)
            v.Enabled = CBool(row.Cells(colEnabled.Name).Value)
            Dim type As String = CStr(row.Cells(colSeverity.Name).Value)
            v.TypeID = CType(mTypeLookup(type), clsValidationInfo.Types)
            validationinfo.Add(v)
        Next
        gSv.SetValidationInfo(validationinfo)
        Me.DialogResult = DialogResult.OK
        Close()
    End Sub

    ''' <summary>
    ''' Force the combobox open when a cell is entered.
    ''' </summary>
    Private Sub tblChecks_CellEnter(ByVal sender As Object, ByVal e As DataGridViewCellEventArgs) Handles tblChecks.CellEnter
        tblChecks.BeginEdit(False)
        If tblChecks.Columns(e.ColumnIndex) Is colSeverity AndAlso _
         TypeOf tblChecks.EditingControl Is ComboBox Then
            DirectCast(tblChecks.EditingControl, ComboBox).DroppedDown = True
        End If
    End Sub

    ''' <summary>
    ''' Shows the context sensitive help
    ''' </summary>
    Private Sub btnHelp_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnHelp.Click
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    ''' <summary>
    ''' Returns the file name of the help file.
    ''' </summary>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "sysman-validation-ui.html"
    End Function

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return titleBar.BackColor
        End Get
        Set(value As Color)
            titleBar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return titleBar.TitleColor
        End Get
        Set(value As Color)
            titleBar.TitleColor = value
        End Set
    End Property
End Class
