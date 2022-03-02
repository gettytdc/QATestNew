Namespace WebApis.TemplateProcessing
    ''' <summary>
    ''' A content element containing raw static text, exactly as it occurred
    ''' in the input
    ''' </summary>
    Friend Class LiteralElement
        Inherits StaticElement

        ''' <summary>
        ''' Creates a new LiteralElement
        ''' </summary>
        ''' <param name="raw">The raw text from the input on which this element
        ''' is based</param>
        Sub New(raw As String)
            MyBase.New(raw, raw)
        End Sub
        
    End Class
End NameSpace