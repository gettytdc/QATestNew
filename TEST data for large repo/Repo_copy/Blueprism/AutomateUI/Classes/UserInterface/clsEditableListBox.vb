''' <summary>
''' A listbox which pops up a text box to edit the list entry when selected with the
''' mouse.
''' </summary>
Public Class clsEditableListBox : Inherits ListBox

    ''' <summary>
    ''' Event fired when the <see cref="[ReadOnly]"/> property in this list box is
    ''' changed.
    ''' </summary>
    Public Event ReadOnlyChanged(ByVal sender As Object, ByVal e As EventArgs)

    ' A textbox used to accept the user input for a selection
    Private WithEvents txtBox As New AutomateControls.Textboxes.StyledTextBox

    ' The item index saved in the MouseDown event for use in the MouseUp event
    Private mItemIndex As Integer

    ' Flag indicating whether this listbox is currently set as readonly
    Private mReadOnly As Boolean

    ''' <summary>
    ''' Creates a new editable list box.
    ''' </summary>
    Public Sub New()
        ForeColor = Color.Green
        txtBox.BorderStyle = BorderStyle.None
        txtBox.Visible = False
        Controls.Add(txtBox)
    End Sub

    ''' <summary>
    ''' Gets or sets the readonly state of this listbox.
    ''' </summary>
    Public Property [ReadOnly]() As Boolean
        Get
            Return mReadOnly
        End Get
        Set(ByVal value As Boolean)
            If mReadOnly <> value Then
                mReadOnly = value
                OnReadOnlyChanged(EventArgs.Empty)
            End If
        End Set
    End Property

    ''' <summary>
    ''' Raises the <see cref="ReadOnlyChanged"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnReadOnlyChanged(ByVal e As EventArgs)
        RaiseEvent ReadOnlyChanged(Me, e)
    End Sub

    ''' <summary>
    ''' Handles a mousedown event in this box
    ''' </summary>
    ''' <param name="e">The args detailing the mousedown event</param>
    Protected Overrides Sub OnMouseDown(ByVal e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        mItemIndex = SelectedIndex
    End Sub

    ''' <summary>
    ''' Handles a mouseup event in this box
    ''' </summary>
    ''' <param name="e">The args detailing the mouseup event</param>
    Protected Overrides Sub OnMouseUp(ByVal e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If [ReadOnly] Then Return ' We don't do anything if we're currently readonly

        If mItemIndex = -1 Then mItemIndex = Me.Items.Add("")

        Const offset As Integer = 2

        With txtBox
            Dim r As Rectangle = GetItemRectangle(mItemIndex)
            .Location = New Point(r.X + offset, r.Y)
            .Size = New Size(r.Width - offset, r.Height)
            .Text = CStr(Items(mItemIndex))
            .Show()
            .SelectAll()
            .Focus()
        End With

    End Sub

    ''' <summary>
    ''' Handles the raised textbox being validated, ensuring that the data is
    ''' committed
    ''' </summary>
    Private Sub HandleLostFocus(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtBox.LostFocus

        Dim txt As String = txtBox.Text
        Items(mItemIndex) = txt

        If txt = "" Then
            Items.Remove(mItemIndex)
            txtBox.Hide()
            Exit Sub
        End If

        For i As Integer = 0 To Items.Count - 1
            SetSelected(i, False)
        Next

        If mItemIndex >= 3 Then mItemIndex = Items.Add("")

    End Sub

    ''' <summary>
    ''' Handles keypresses in the textbox to handle enter/escape chars.
    ''' </summary>
    Private Sub KeyPressHandler(ByVal sender As Object, ByVal e As KeyPressEventArgs) _
     Handles txtBox.KeyPress
        If e.KeyChar = ChrW(13) Or e.KeyChar = ChrW(27) Then txtBox.Hide()
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub
End Class
