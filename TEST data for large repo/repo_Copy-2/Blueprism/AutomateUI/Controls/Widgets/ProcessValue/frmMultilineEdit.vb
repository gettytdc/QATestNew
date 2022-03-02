Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports LocaleTools

''' <summary>
''' Form used to capture text data which spans multiple lines.
''' </summary>
Public Class frmMultilineEdit

#Region " Class-scope Declarations "

    ''' <summary>
    ''' The languages supported in this form.
    ''' </summary>
    Private Shared SupportedLanguages As ICollection(Of CodeLanguage) = _
     GetReadOnly.ICollectionFrom( _
      CodeLanguage.PlainText, _
      CodeLanguage.Javascript, _
      CodeLanguage.Xml, _
      CodeLanguage.Sql)

    ''' <summary>
    ''' A wrapper class for the code language element, to use within combo boxes
    ''' which are showing languages.
    ''' </summary>
    Private Class CodeLanguageWrapper

        ' The language represented by this object
        Private mLang As CodeLanguage

        ' The display name of the language, cached from the FriendlyName attribute
        ' value associated with the CodeLanguage value.
        Private mName As String

        ''' <summary>
        ''' Creates a new wrapper around a CodeLanguage value.
        ''' </summary>
        ''' <param name="lang">The language to represent in this object</param>
        Public Sub New(ByVal lang As CodeLanguage)
            mLang = lang
        End Sub

        ''' <summary>
        ''' The CodeLanguage value associated with this object.
        ''' </summary>
        Public ReadOnly Property Value() As CodeLanguage
            Get
                Return mLang
            End Get
        End Property

        ''' <summary>
        ''' Gets a string representation of this code language wrapper.
        ''' </summary>
        ''' <returns>The friendly name for the language wrapped in this object.
        ''' </returns>
        Public Overrides Function ToString() As String
            If mName Is Nothing Then mName = LTools.GetC(mLang.GetFriendlyName(), "misc", "code_language")
            Return mName
        End Function

        ''' <summary>
        ''' Checks the given object for equality with this object.
        ''' </summary>
        ''' <param name="obj">The object to test for equality with this object.
        ''' </param>
        ''' <returns>True if the given object is a non-null CodeLanguageWrapper with
        ''' the same underlying CodeLanguage value as this object.</returns>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim clw As CodeLanguageWrapper = TryCast(obj, CodeLanguageWrapper)
            Return (clw IsNot Nothing AndAlso clw.mLang = mLang)
        End Function

        ''' <summary>
        ''' Gets an integer hash of this object.
        ''' </summary>
        ''' <returns>An integer hash of this object, actually just a function of the
        ''' CodeLanguage value it wraps</returns>
        Public Overrides Function GetHashCode() As Integer
            Return CInt(mLang)
        End Function
    End Class

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates an empty, editable multiline edit form
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new multiline edit form using data from the given control
    ''' </summary>
    ''' <param name="txtControl">The process text control from which to draw the
    ''' data for this form</param>
    Public Sub New(ByVal txtControl As ctlProcessText)
        InitializeComponent()

        ' Initialise the combo box with the supported languages
        For Each lang As CodeLanguage In SupportedLanguages
            cmbLang.Items.Add(New CodeLanguageWrapper(lang))
        Next

        ' Default to plain text
        cmbLang.SelectedIndex = 0

        ' Set the text and readonly values from the given control, if it's there
        If txtControl IsNot Nothing Then
            EditText = txtControl.Text
            [ReadOnly] = txtControl.ReadOnly
        End If
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the edit text for this form
    ''' </summary>
    <Browsable(True), DefaultValue(""), Description( _
     "The text value of the component in this form")> _
    Public Property EditText() As String
        Get
            If IsShowingCodeControl _
             Then Return ctlCode.Code _
             Else Return txtTextEntry.Text
        End Get
        Set(ByVal value As String)
            ' Set in both, just to be sure
            txtTextEntry.Text = value
            ctlCode.Code = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the readonly state of this form
    ''' </summary>
    <Browsable(True), DefaultValue(False), Description( _
     "Whether this form is readonly or not")> _
    Public Property [ReadOnly]() As Boolean
        Get
            Return txtTextEntry.ReadOnly
        End Get
        Set(ByVal value As Boolean)
            txtTextEntry.ReadOnly = value
            ctlCode.ReadOnly = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the currently selected language in this form
    ''' </summary>
    <Browsable(True), DefaultValue(CodeLanguage.PlainText), Description( _
     "The code language to use in this form")> _
    Public Property Language() As CodeLanguage
        Get
            Dim clw As CodeLanguageWrapper = _
             DirectCast(cmbLang.SelectedItem, CodeLanguageWrapper)
            If clw Is Nothing Then Return CodeLanguage.PlainText
            Return clw.Value
        End Get
        Set(ByVal value As CodeLanguage)
            If SupportedLanguages.Contains(value) Then _
             cmbLang.SelectedItem = New CodeLanguageWrapper(value)
        End Set
    End Property

    ''' <summary>
    ''' Gets whether the code control (ie. scintilla) is the 'active' control in this
    ''' form.
    ''' </summary>
    <Browsable(False), _
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Private ReadOnly Property IsShowingCodeControl() As Boolean
        Get
            ' If the current language is a scintilla-handled language, get the text
            ' value from the code editor, otherwise use the textbox.
            Return (ScintillaNameAttribute.GetNameFor(Language) IsNot Nothing)
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Handles a language being chosen via the combo box
    ''' </summary>
    Private Sub HandleLanguageSelected( _
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles cmbLang.SelectedIndexChanged

        ' Get the text from the current control, so we can set it in the new control
        Dim currText As String = EditText

        ' Get the scintilla language name for the currently selected language
        Dim scintillaLang As String = ScintillaNameAttribute.GetNameFor(Language)

        ' If we have a scintilla name, we're showing the code control, otherwise
        ' we're just displaying it as plaintext
        If scintillaLang IsNot Nothing Then
            ctlCode.Populate(currText, scintillaLang)
            txtTextEntry.Visible = False
            ctlCode.Visible = True

        Else
            txtTextEntry.Text = currText
            txtTextEntry.Visible = True
            ctlCode.Visible = False

        End If
    End Sub

#End Region

End Class
