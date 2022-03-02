Imports BluePrism.BPCoreLib

''' <summary>
''' Attribute to hold the scintilla name for a language
''' </summary>
Public Class ScintillaNameAttribute : Inherits Attribute

    ' The scintilla name set in this attribute
    Private mName As String

    ''' <summary>
    ''' Creates a new scintilla name attribute
    ''' </summary>
    ''' <param name="name">The name of a language as recognised by scintilla</param>
    Public Sub New(ByVal name As String)
        mName = name
    End Sub

    ''' <summary>
    ''' The scintilla name associated with a language
    ''' </summary>
    Public ReadOnly Property Name() As String
        Get
            Return mName
        End Get
    End Property

    ''' <summary>
    ''' Gets the name held in a scintilla name attribute associated with the given
    ''' language.
    ''' </summary>
    ''' <param name="lang">The language value for which the name attribute's value is
    ''' required.</param>
    ''' <returns>The name value associated with the given language, or null if there
    ''' is no name attribute associated with it.</returns>
    Public Shared Function GetNameFor(ByVal lang As CodeLanguage) As String
        Dim attr As ScintillaNameAttribute = _
         BPUtil.GetAttributeValue(Of ScintillaNameAttribute)(lang)
        If attr Is Nothing Then Return Nothing
        Return attr.Name
    End Function

End Class
