''' Project  : Automate
''' Class    : frmInputBox
''' 
''' <summary>
''' Simple replacement for the VB 'InputBox(...)' function. In VB6 and below you
''' could test for Cancel being pressed by checking StrPtr(returnValue) against 0.
''' There's no such function in VB.net, so you can't tell the difference between
''' an empty string being entered and Cancel (or Close) being pressed.
''' I've tried to emulate the InputBox() parameters in frmInputBox.GetText() - I
''' skipped the X/Y params since I can't recall ever seeing code which used them.
''' </summary>
Friend Class frmInputBox
    Inherits frmForm

    ''' <summary>
    ''' Creates a dialog box with the prompt "Please enter some text", The title 
    ''' "Blue Prism" and no initial value.
    ''' </summary>
    Private Sub New()

        Me.New(My.Resources.PleaseEnterSomeText, My.Resources.BluePrism, "")

    End Sub

    ''' <summary>
    ''' Creates an input box with the specified prompt, title and initial value
    ''' </summary>
    ''' <param name="prompt">The text which the prompt label should display</param>
    ''' <param name="title">The text which should go into the title bar</param>
    ''' <param name="initialValue">The initial value in the text field</param>
    Private Sub New(ByVal prompt As String, ByVal title As String, ByVal initialValue As String)
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        Me.Title = title
        Me.Value = initialValue
        Me.Prompt = prompt

    End Sub

    ''' <summary>
    ''' The value of the input box - basically the text that is currently set
    ''' in the text field.
    ''' </summary>
    Public Property Value() As String
        Get
            Return txtBox.Text
        End Get
        Set(ByVal value As String)
            txtBox.Text = value
        End Set
    End Property

    ''' <summary>
    ''' The title of the input box - the text which displays in the title bar of
    ''' the dialog box.
    ''' </summary>
    Public Property Title() As String
        Get
            Return Me.Text
        End Get
        Set(ByVal value As String)
            Me.Text = value
        End Set
    End Property

    ''' <summary>
    ''' The text which prompts the user to enter some text.
    ''' This is shown by a label on the dialog box.
    ''' </summary>
    Public Property Prompt() As String
        Get
            Return lblPrompt.Text
        End Get
        Set(ByVal value As String)
            lblPrompt.Text = value
        End Set
    End Property

    ''' <summary>
    ''' The maximum number of characters allowed in the input box.
    ''' </summary>
    Public Property MaxLength() As Integer
        Get
            Return txtBox.MaxLength
        End Get
        Set(ByVal value As Integer)
            txtBox.MaxLength = value
        End Set
    End Property

    ''' <summary>
    ''' Prompts for and returns text written by the user in a single-line
    ''' text field. This is only really appropriate for quick entry of
    ''' short strings and supports only basic text-field editing.
    ''' </summary>
    ''' <param name="prompt">The text for the label which prompts the user
    ''' for their text</param>
    ''' <param name="title">The text which should be displayed in the title bar
    ''' of the input box dialog</param>
    ''' <param name="initValue">The initial value to use in the text box.</param>
    ''' <returns>The text that was entered by the user, or Nothing if the user
    ''' cancelled or closed the dialog box.</returns>
    Public Shared Function GetText(
     ByVal prompt As String,
     Optional ByVal title As String = "",
     Optional ByVal initValue As String = "",
     Optional ByVal maxChars As Integer = 32767) As String

        Dim box As New frmInputBox(prompt, title, initValue)
        box.MaxLength = maxChars
        box.ShowInTaskbar = False
        Dim response As DialogResult = box.ShowDialog()
        If response = DialogResult.OK Then
            Return box.Value
        Else ' Cancel | Esc | Alt-F4 etc.
            Return Nothing
        End If

    End Function

    ''' <summary>
    ''' Prompts the user to enter text, and returns a boolean indicating whether
    ''' they did so, or whether they cancelled.
    ''' The value entered is returned in the output parameter : 
    ''' <paramref name="textValue"/>.
    ''' </summary>
    ''' <param name="prompt">The prompt to display to the user.</param>
    ''' <param name="textValue">On entering the method, the initial text to display
    ''' to the user, on exiting the method the text that the user entered.
    ''' </param>
    ''' <returns>True if the user clicked OK on the text entry dialog; False if
    ''' the user clicked Cancel.</returns>
    Public Shared Function HasEnteredText(
     ByVal prompt As String,
     ByRef textValue As String) As Boolean

        Return HasEnteredText(prompt, "", textValue, 32767)

    End Function

    ''' <summary>
    ''' Prompts the user to enter text, and returns a boolean indicating whether
    ''' they did so, or whether they cancelled.
    ''' The value entered is returned in the output parameter : 
    ''' <paramref name="textValue"/>.
    ''' </summary>
    ''' <param name="prompt">The prompt to display to the user.</param>
    ''' <param name="title">The title to display in the dialog box in which the
    ''' user is prompted for text.</param>
    ''' <param name="textValue">On entering the method, the initial text to display
    ''' to the user, on exiting the method the text that the user entered.
    ''' </param>
    ''' <returns>True if the user clicked OK on the text entry dialog; False if
    ''' the user clicked Cancel.</returns>
    Public Shared Function HasEnteredText(
     ByVal prompt As String,
     ByVal title As String,
     ByRef textValue As String) As Boolean

        Return HasEnteredText(prompt, title, textValue, 32767)

    End Function


    ''' <summary>
    ''' Prompts the user to enter text, and returns a boolean indicating whether
    ''' they did so, or whether they cancelled.
    ''' The value entered is returned in the output parameter : 
    ''' <paramref name="textValue"/>.
    ''' </summary>
    ''' <param name="prompt">The prompt to display to the user.</param>
    ''' <param name="title">The title to display in the dialog box in which the
    ''' user is prompted for text.</param>
    ''' <param name="textValue">On entering the method, the initial text to display
    ''' to the user, on exiting the method the text that the user entered.
    ''' </param>
    ''' <param name="maxChars">The maximum number of characters allowed in the
    ''' generated input box - by default, this is 32767.</param>
    ''' <returns>True if the user clicked OK on the text entry dialog; False if
    ''' the user clicked Cancel.</returns>
    Public Shared Function HasEnteredText(
     ByVal prompt As String,
     ByVal title As String,
     ByRef textValue As String,
     ByVal maxChars As Integer) As Boolean

        If title Is Nothing Then title = ""

        Dim box As New frmInputBox(prompt,
         CStr(IIf(title Is Nothing, "", title)),
         CStr(IIf(textValue Is Nothing, "", textValue)))

        box.MaxLength = maxChars

        Dim response As DialogResult = box.ShowDialog()
        If response = DialogResult.OK Then
            textValue = box.Value
            Return True
        Else ' Cancel | Esc | Alt-F4 etc.
            Return False
        End If

    End Function


End Class