Imports AutomateControls

''' <summary>
''' A code editing control.
''' </summary>
Public Class ctlCodeEditor

    ''' <summary>
    ''' Creates a new CodeEditor control
    ''' </summary>
    Public Sub New()

        ' Wrapped into a try..catch because sometimes (in the designer), the loading
        ' of the scintilla unmanaged DLL fails
        ' See http://scintillanet.codeplex.com/workitem/23101 for a full explanation
        Try
            ' This call is required by the Windows Form Designer.
            InitializeComponent()

        Catch ex As Win32Exception
            ' Only output the error if we're loading in the designer - we have better
            ' ways of handling this in the product proper, and it's almost certainly
            ' only a problem in a development environment
            If UIUtil.IsInVisualStudio Then _
             MessageBox.Show(String.Format(
              My.Resources.ctlCodeEditor_AnErrorOccurredInitializingTheCodeEditor101EnsureThatTheScintillaDLLsAreInTheSy,
              vbCrLf, ex), My.Resources.ctlCodeEditor_Error, MessageBoxButtons.OK, MessageBoxIcon.Error)

            ' We still want to error, we just want a better explanation of the
            ' problem, especially when designing
            Throw

        End Try

    End Sub

    ''' <summary>
    ''' Gets or sets the code currently in the editor.
    ''' </summary>
    Public Property Code() As String
        Get
            Return mEditor.Text
        End Get
        Set(ByVal value As String)
            ' "ReadOnly" in the Scintilla control actually makes programmatic access
            ' fail too - ie. it's readonly programatically as well as in the UI,
            ' so we must briefly allow access in order to work the same way that
            ' ReadOnly does for textboxes (path of least surprise)
            Dim ro As Boolean = [ReadOnly]
            If ro Then [ReadOnly] = False
            Try
                mEditor.Text = value
            Finally
                If ro Then [ReadOnly] = True
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the readonly state of this control
    ''' </summary>
    Public Property [ReadOnly]() As Boolean
        Get
            Return mEditor.IsReadOnly
        End Get
        Set(ByVal value As Boolean)
            mEditor.IsReadOnly = value
        End Set
    End Property

    Public Property BackgroundColour() As Color
        Get
            Return mEditor.BackColor
        End Get
        Set(ByVal value As Color)
            mEditor.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Populates the code editor with given code.
    ''' </summary>
    ''' <param name="code">The code to edit.</param>
    ''' <param name="language">The language of the code. Allowed values are "csharp",
    ''' "visualbasic" and "javascript".</param>
    Public Sub Populate(ByVal code As String, ByVal language As String)

        Me.Code = code

        'Set the syntax highlighter language, if we recognise the language and the
        'highligher has support for it.
        'List of supported languages:
        '  http://scintillanet.codeplex.com/wikipage?title=HowToSyntax
        Dim syntaxlang As String = Nothing
        Select Case language
            Case "csharp"
                syntaxlang = "cs"
            Case "visualbasic"
                syntaxlang = "vbscript"
            Case "javascript"
                syntaxlang = "js"
            Case Else
                ' Default to whatever we've been given
                ' If it's not recognised, it just defaults to a 'plaintext' mode
                syntaxlang = language
        End Select
        If syntaxlang IsNot Nothing Then
            ' xml doesn't use keywords, but when you change the language, it doesn't
            ' remove the keywords from whatever was selected last... which is
            ' rubbish. Ergo, we remove them manually.
            mEditor.Lexing.SetKeywords(0, "")
            mEditor.ConfigurationManager.Language = syntaxlang
            mEditor.ConfigurationManager.Configure()
        End If

        mEditor.Indentation.IndentWidth = 4
        mEditor.Indentation.TabIndents = True
        mEditor.Indentation.TabWidth = 4
        mEditor.Margins(0).Width = 40
    End Sub


End Class
