Namespace WebApis.TemplateProcessing
    ''' <summary>
    ''' A content element containing static text
    ''' </summary>
    Friend MustInherit Class StaticElement 
        Inherits ContentElement

        ''' <summary>
        ''' Creates a new StaticElement
        ''' </summary>
        ''' <param name="raw">The raw text from the input on which this element
        ''' is based</param>
        ''' <param name="text">The static text that should be added when
        ''' rendering the element</param>
        Protected Sub New(raw As String, text As String)
            MyBase.New(raw)
            Me.Text = text
        End Sub

        ''' <summary>
        ''' The static text content for this element
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Text As String

    End Class
End NameSpace