Namespace WebApis.TemplateProcessing

    ''' <summary>
    ''' An element within a string that contains content with delimited tokens
    ''' for parameter replacement
    ''' </summary>
    Friend MustInherit Class ContentElement

        ''' <summary>
        ''' Creates a new ContentElement
        ''' </summary>
        ''' <param name="raw">The raw text from the input on which this element
        ''' is based</param>
        Sub New(raw As String)
            Me.Raw = raw
        End Sub

        ''' <summary>
        ''' The raw text from the input on which this element
        ''' is based
        ''' </summary>
        Public ReadOnly Property Raw As String
        
    End Class
End NameSpace