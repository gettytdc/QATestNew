Imports BluePrism.AutomateProcessCore

''' <summary>
''' Process Value control to handle text data
''' </summary>
Public Class ctlProcessText : Inherits UserControl : Implements IProcessValue

#Region " Published Events "

    ''' <summary>
    ''' Event fired when the value embedded in this control is changed
    ''' </summary>
    Public Event Changed As EventHandler Implements IProcessValue.Changed

#End Region

#Region " Member Variables "

    ' The initial value of this control
    Private mLastText As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new process text editor control with multiline support
    ''' </summary>
    Public Sub New()
        Me.New(True)
    End Sub

    ''' <summary>
    ''' Creates a new process text editor control, with or without multiline support
    ''' as specified.
    ''' </summary>
    ''' <param name="withMultiline">True to provide a text control with multiline
    ''' support; False to provide a single line text control.</param>
    Public Sub New(ByVal withMultiline As Boolean)
        InitializeComponent()
        Multiline = withMultiline
        ImeMode = ImeMode.On
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the multiline setting of the control.
    ''' </summary>
    Public Property Multiline() As Boolean
        Get
            Return txtValue.Multiline
        End Get
        Set(ByVal value As Boolean)
            txtValue.Multiline = value
            btnEditButton.Visible = value
        End Set
    End Property

    ''' <summary>
    ''' Checks if this control contains multiline data or not. Note that a textbox
    ''' which is <em>not</em> set to support <see cref="Multiline"/> data will always
    ''' indicate that it does not contain multiline data, regardless of whether the
    ''' data in the control contains newline characters.
    ''' </summary>
    ''' <returns>True if this control is set to support <see cref="Multiline"/> data
    ''' and the current <see cref="Text"/> contains any newline characters;
    ''' False otherwise.</returns>
    Public ReadOnly Property HasMultilineData() As Boolean
        Get
            Return (Multiline AndAlso Text.IndexOfAny(vbCrLf.ToCharArray()) <> -1)
        End Get
    End Property

    ''' <summary>
    ''' The property that allows access to the underlying clsProcessValue we actually
    ''' store the value in me.text
    ''' </summary>
    Public Property Value() As clsProcessValue Implements IProcessValue.Value
        Get
            Return txtValue.Text
        End Get
        Set(ByVal value As clsProcessValue)
            If value Is Nothing Then value = ""
            mLastText = CStr(value)
            txtValue.Text = value.FormattedValue
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the maximum length of the value in this text control
    ''' </summary>
    Public Property MaxLength() As Integer
        Get
            Return txtValue.MaxLength
        End Get
        Set(ByVal value As Integer)
            txtValue.MaxLength = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the text associated with this control
    ''' </summary>
    Public Overrides Property Text() As String
        Get
            Return txtValue.Text
        End Get
        Set(ByVal value As String)
            txtValue.Text = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the readonly state of this control
    ''' </summary>
    Public Property [ReadOnly]() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return txtValue.ReadOnly
        End Get
        Set(ByVal value As Boolean)
            txtValue.ReadOnly = value
        End Set
    End Property

#End Region

#Region " Event Handlers / Raisers "

    ''' <summary>
    ''' Handles the multiline edit button being clicked
    ''' </summary>
    Private Sub HandleEditButtonClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnEditButton.Click
        Using f As New frmMultilineEdit(Me)
            f.ShowInTaskbar = False
            f.ShowDialog(Me)
            Text = f.EditText
        End Using
    End Sub

    ''' <summary>
    ''' Handles the textbox being validated
    ''' </summary>
    Private Sub HandleValidated(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtValue.Validated
        Commit(True)
    End Sub

    ''' <summary>
    ''' Handles the textbox text being changed.
    ''' </summary>
    Private Sub HandleTextChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtValue.TextChanged
        Commit(False)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="Changed"/> event for this control
    ''' </summary>
    ''' <param name="e">The args detailing the changed event</param>
    Protected Overridable Sub OnChanged(ByVal e As EventArgs)
        RaiseEvent Changed(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the keydown event for the textbox, ensuring that cursor key events
    ''' are propogated to listeners to this control. Note that they are not
    ''' propogated for controls which contain multiline data - it will scroll through
    ''' the lines within the textbox in that case
    ''' </summary>
    Private Sub HandleKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) _
     Handles txtValue.KeyDown
        If HasMultilineData Then Return
        If e.KeyCode = Keys.Up OrElse e.KeyCode = Keys.Down Then OnKeyDown(e)
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Selects this control, ensuring that the text in the texbox is all selected.
    ''' </summary>
    Public Sub SelectControl() Implements IProcessValue.SelectControl
        txtValue.Select()
    End Sub

    ''' <summary>
    ''' Commits the changes made in this control
    ''' </summary>
    Public Sub Commit() Implements IProcessValue.Commit
        Commit(False)
    End Sub

    ''' <summary>
    ''' Commits the changes made in this control, optionally resetting the selection
    ''' in the embedded textbox
    ''' </summary>
    ''' <param name="resetSelection">True to reset the caret to the beginning of the
    ''' text after committing the value; False to leave it as it is</param>
    ''' <remarks>The resetting of the selection ensures that the beginning of the
    ''' text is visible after the control has been navigated from - if you move to
    ''' a different cell within a <see cref="ctlListView"/>, it may otherwise look
    ''' like the data has been lost.</remarks>
    Public Sub Commit(ByVal resetSelection As Boolean)
        If Text <> mLastText Then mLastText = Text : OnChanged(EventArgs.Empty)
        If resetSelection Then txtValue.SelectionStart = 0
    End Sub

#End Region

End Class
