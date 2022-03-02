
Imports BluePrism.BPCoreLib

Imports BluePrism.AutomateAppCore
Imports LocaleTools
Imports BluePrism.Images

''' <summary>
''' Control to represent a set of conflicts on an imported release.
''' </summary>
Public Class ctlConflictSet : Inherits ctlWizardStageControl

#Region " Private Members "

    ' The conflict set set into this control
    Private mConflicts As ConflictSet

    ' The log of errors with the currently set conflict resolutions.
    Private mErrors As clsErrorLog

#End Region

#Region " Properties "

    ''' <summary>
    ''' The conflicts represented in this conflict set.
    ''' </summary>
    Public Property Conflicts() As ConflictSet
        Get
            Return mConflicts
        End Get
        Set(ByVal value As ConflictSet)
            mConflicts = value

            If value Is Nothing OrElse value.IsEmpty Then
                Return
            End If

            mContents.AutoGenerateColumns = False

            ' Choose the first option available by default if the existing option is not set
            Me.AllConflicts().ToList().ForEach(
                Sub(obj)
                    If obj.Resolution Is Nothing Then
                        Dim defaultOption = obj.Definition.Options.FirstOrDefault()
                        Dim handler = obj.ChooseOption(defaultOption)
                        obj.Resolution = New ConflictResolution(obj, defaultOption, handler)
                    End If
                End Sub)
            mContents.DataSource = New BindingSource(Me.AllConflicts(), Nothing)
        End Set
    End Property

    Public ReadOnly Property AllConflicts() As ICollection(Of Conflict)
        Get
            Return mConflicts.Conflicts.SelectMany(Function(conflicts) conflicts.Value).ToList()
        End Get
    End Property

    ''' <summary>
    ''' The resolutions to the conflicts in the set represented by this control
    ''' </summary>
    Public ReadOnly Property Resolutions() As ICollection(Of ConflictResolution)
        Get
            Return Me.AllConflicts().Select(Function(conflict) conflict.Resolution).ToList()
        End Get
    End Property

    ''' <summary>
    ''' The log of errors indicating that there is something wrong with the
    ''' conflict resolutions as currently set by the user.
    ''' </summary>
    Public Property ErrorLog() As clsErrorLog
        Get
            Return mErrors
        End Get
        Set(ByVal value As clsErrorLog)
            If value Is Nothing OrElse value.IsEmpty Then
                If mErrors IsNot Nothing Then
                    Me.pnlErrors.Controls.Clear()
                End If
            Else
                ' set the errors at the top of the page.
                Dim errorGridView As DataGridView = Nothing

                ' If there are already some errors displaying, we might as well
                ' reuse the panel that they are displaying in
                If mErrors IsNot Nothing Then
                    For Each gridView As DataGridView In Me.pnlErrors.Controls
                        If TypeOf gridView.Tag Is clsErrorLog Then errorGridView = gridView : Exit For
                    Next
                End If
                Dim headingLabel As New Label()
                If errorGridView Is Nothing Then
                    ' Create a new group, populate it with a new flow layout panel.
                    errorGridView = New DataGridView()
                    errorGridView.AutoSize = True
                    errorGridView.Dock = DockStyle.Top
                    errorGridView.MinimumSize = New Size(0, 105)
                    errorGridView.Text = My.Resources.Errors

                    headingLabel = New Label()
                    headingLabel.Name = "ctlConflictSet_errheading"
                    headingLabel.Dock = DockStyle.Top
                    headingLabel.AutoSize = True
                    headingLabel.ForeColor = Color.Red

                    Me.pnlErrors.Controls.Add(errorGridView)
                    Me.pnlErrors.Controls.SetChildIndex(errorGridView, 0)

                    Me.pnlErrors.Controls.Add(headingLabel)
                    Me.pnlErrors.Controls.SetChildIndex(headingLabel, 1)
                End If
                headingLabel.Text = LTools.Format(My.Resources.ctlConflictSet_plural_FollowingErrorsMustBeResolved, "COUNT", value.Count)

                Dim errorList = New List(Of clsError)
                ' Go through each of the errors and add it to the error box
                For Each err As clsError In value
                    errorList.Add(err)
                Next

                errorGridView.DataSource = errorList

                Me.pnlErrors.PerformLayout()
                errorGridView.Tag = value

                errorGridView.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                errorGridView.RowHeadersVisible = False
                errorGridView.Columns(1).Visible = False
                errorGridView.Columns(0).HeaderText = My.Resources.ctlConflictSet_ErrorHeaderText
                errorGridView.Columns(0).Width = errorGridView.Width
                errorGridView.Columns(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
                errorGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells

                '' Now ensure that the errors are visible :-
                Me.pnlErrors.ScrollControlIntoView(errorGridView)
                Me.pnlErrors.ScrollControlIntoView(headingLabel)

                ' And set focus on the first component again.
                Me.pnlErrors.SelectNextControl(Nothing, True, True, True, True)

            End If

            mErrors = value

        End Set
    End Property

#End Region

#Region " Methods "

    Private Sub mContents_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles mContents.CellFormatting
        Dim data = CType(mContents.Rows(e.RowIndex).DataBoundItem, Conflict)
        mContents.Rows(e.RowIndex).Cells("ComponentColumn").Value = data.Component.Name

        ' Join the definition and hint. Remove a trailing period character and separate the definition and hint with a period
        mContents.Rows(e.RowIndex).Cells("IssueColumn").Value =
            $"{data.Definition.Text.TrimEnd(My.Resources.Punctuation_Period.ToCharArray().FirstOrDefault())}{My.Resources.Punctuation_Period} {data.Definition.Hint}"

        Dim cell = DirectCast(mContents.Rows(e.RowIndex).Cells("ResolutionColumn"), DataGridViewComboBoxCell)
        If cell.DataSource Is Nothing Then
            cell.DisplayMember = "Text"
            cell.ValueMember = "Choice"
            cell.DataSource = data.Definition.Options
            cell.ToolTipText = data.Definition.Hint
            If data.ChosenOption IsNot Nothing Then
                cell.Value = data.ChosenOption.Choice
            End If
            Dim handler = data.ChooseOption(data.ChosenOption)
            If handler IsNot Nothing Then
                mContents.Rows(e.RowIndex).Cells("ArgumentsColumn").Style.BackColor = SystemColors.HighlightText
                mContents.Rows(e.RowIndex).Cells("ArgumentsColumn").Value = "..."
            End If
        End If
    End Sub

    Private Sub SetArgumentsCellStatus(ByRef row As DataGridViewRow, enable As Boolean)
        If enable = True Then
            row.Cells("ArgumentsColumn").Style.BackColor = SystemColors.HighlightText
            row.Cells("ArgumentsColumn").Value = "..."
        Else
            row.Cells("ArgumentsColumn").Value = String.Empty
            row.Cells("ArgumentsColumn").Style.BackColor = SystemColors.InactiveCaption
        End If
    End Sub

    Private Sub mContents_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles mContents.DataBindingComplete
        ' Images are updated on DataBindingComplete because when they are updated on CellFormatting, they cause the image column to keep formatting itself
        For Each row As DataGridViewRow In mContents.Rows
            Dim data = CType(row.DataBoundItem, Conflict)
            row.Cells("ImageColumn").Value = ImageLists.Components_16x16().Images(data.Component.Type.Key)
            row.Cells("ImageColumn").ToolTipText = data.Component.Type.Plural
        Next
    End Sub

    Private Sub mContents_EditingControlShowing(sender As Object, e As DataGridViewEditingControlShowingEventArgs) Handles mContents.EditingControlShowing
        If (TypeOf e.Control Is DataGridViewComboBoxEditingControl) Then
            Dim cbo = TryCast(e.Control, ComboBox)
            If (cbo IsNot Nothing) Then
                cbo.DrawMode = DrawMode.OwnerDrawFixed
                RemoveHandler cbo.SelectionChangeCommitted, AddressOf choiceComboBox_SelectedIndexChanged
                AddHandler cbo.SelectionChangeCommitted, AddressOf choiceComboBox_SelectedIndexChanged
                RemoveHandler cbo.DrawItem, AddressOf choiceComboBox_DrawItem
                AddHandler cbo.DrawItem, AddressOf choiceComboBox_DrawItem
            End If
        End If
    End Sub

    Private Sub choiceComboBox_DrawItem(sender As Object, e As DrawItemEventArgs)
        Dim cbo As ComboBox = TryCast(sender, ComboBox)
        Dim item = cbo.Items(e.Index)
        Dim display As String = cbo.GetItemText(item)

        e.DrawBackground()

        Using br As New SolidBrush(e.ForeColor)
            e.Graphics.DrawString(display, e.Font, br, e.Bounds)
        End Using

        If (e.State And DrawItemState.Focus) = DrawItemState.Focus AndAlso cbo.DroppedDown Then
            Me.ResolutionToolTip.Show(display, cbo, e.Bounds.Right, e.Bounds.Bottom, 2000)
        End If

        e.DrawFocusRectangle()
    End Sub

    Private Sub choiceComboBox_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim cbo = TryCast(sender, ComboBox)
        Dim conflict As Conflict = TryCast(Me.mContents.SelectedCells(0).OwningRow.DataBoundItem, Conflict)
        If conflict IsNot Nothing Then
            Dim selectedOption = TryCast(cbo.SelectedItem, ConflictOption)
            Dim handler = conflict.ChooseOption(selectedOption)
            If handler IsNot Nothing Then
                Me.SetArgumentsCellStatus(Me.mContents.SelectedCells(0).OwningRow, True)
                Dim handlerDialog As New frmConflictDataHandler(conflict, selectedOption, handler)
                handlerDialog.ShowInTaskbar = False
                handlerDialog.ShowDialog()
            Else
                Me.SetArgumentsCellStatus(Me.mContents.SelectedCells(0).OwningRow, False)
            End If
            conflict.Resolution = New ConflictResolution(conflict, selectedOption, handler)
        End If
    End Sub

    Private Sub mContents_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles mContents.CellDoubleClick
        Me.HandleCellClicks(sender, e)
    End Sub

    Private Sub mContents_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles mContents.CellClick
        Me.HandleCellClicks(sender, e)
    End Sub

    Private Sub HandleCellClicks(sender As Object, e As DataGridViewCellEventArgs)
        If e.ColumnIndex >= 0 Then
            If Me.mContents.Columns(e.ColumnIndex).Name = "ArgumentsColumn" And Me.mContents.SelectedCells(0).Style.BackColor = SystemColors.HighlightText Then
                Dim conflict As Conflict = TryCast(Me.mContents.SelectedCells(0).OwningRow.DataBoundItem, Conflict)
                Dim handler = conflict.ChooseOption(conflict.ChosenOption)
                Dim handlerDialog As New frmConflictDataHandler(conflict, conflict.ChosenOption, handler)
                handlerDialog.ShowInTaskbar = False
                handlerDialog.ShowDialog()
            End If
        End If
    End Sub

#End Region

End Class
