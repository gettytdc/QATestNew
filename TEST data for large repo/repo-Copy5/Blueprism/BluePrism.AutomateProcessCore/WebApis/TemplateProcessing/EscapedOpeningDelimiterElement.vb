Namespace WebApis.TemplateProcessing
    ''' <summary>
    ''' A content element containing static text from an escaped opening parameter
    ''' delimiter
    ''' </summary>
    Friend Class EscapedOpeningDelimiterElement
        Inherits StaticElement

        ''' <summary>
        ''' Creates a new LiteralElement
        ''' </summary>
        ''' <param name="raw">The raw text from the input on which this element
        ''' is based</param>
        Sub New(raw As String, text As String)
            MyBase.New(raw, text)
        End Sub
        
    End Class
End NameSpace