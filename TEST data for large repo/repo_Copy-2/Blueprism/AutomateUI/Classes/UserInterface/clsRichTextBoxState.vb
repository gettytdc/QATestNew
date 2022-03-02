''' Project  : Automate
''' Class    : clsRichTextBoxState
''' 
''' <summary>
''' A helper class for clsRichTextBox.
''' </summary>
Public Class clsRichTextBoxState

    ''' <summary>
    ''' Private member to store public property RTF
    ''' </summary>
    Private msRTF As String
    ''' <summary>
    ''' The text in the textbox.
    ''' </summary>
    ''' <value></value>
    Public Property RTF() As String
        Get
            Return msRTF
        End Get
        Set(ByVal value As String)
            msRTF = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property SelectionStart()
    ''' </summary>
    Private miSelectionStart As Integer
    ''' <summary>
    ''' The selectionstart of the textbox.
    ''' </summary>
    ''' <value></value>
    Public Property SelectionStart() As Integer
        Get
            Return miSelectionStart
        End Get
        Set(ByVal value As Integer)
            miSelectionStart = value
        End Set
    End Property


    ''' <summary>
    ''' Private member to store public property SelectionLength()
    ''' </summary>
    Private miSelectionLength As Integer
    ''' <summary>
    ''' The selectionlength of the textbox.
    ''' </summary>
    ''' <value></value>
    Public Property SelectionLength() As Integer
        Get
            Return miSelectionLength
        End Get
        Set(ByVal value As Integer)
            miSelectionLength = value
        End Set
    End Property

    ''' <summary>
    ''' Constructor. Populates class with supplied values.
    ''' </summary>
    ''' <param name="Text">The text property of the class.</param>
    ''' <param name="SelectionStart">The selectionstart property of the class.</param>
    ''' <param name="SelectionLength">The selectionlength property of the class.</param>
    Public Sub New(ByVal Text As String, ByVal SelectionStart As Integer, ByVal SelectionLength As Integer)
        msRTF = Text
        miSelectionStart = SelectionStart
        miSelectionLength = SelectionLength
    End Sub

End Class
