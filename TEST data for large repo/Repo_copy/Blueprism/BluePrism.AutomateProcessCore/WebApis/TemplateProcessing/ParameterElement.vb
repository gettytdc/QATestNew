Namespace WebApis.TemplateProcessing

    ''' <summary>
    ''' A content element based on a parameter placeholder found within the original text
    ''' </summary>
    Friend Class ParameterElement
        Inherits ContentElement

        ''' <summary>
        ''' Creates a new ParameterElement
        ''' </summary>
        ''' <param name="raw">The raw text from the input on which this element
        ''' is based</param>
        ''' <param name="name">The name of the parameter specified within
        ''' the placeholder</param>
        Sub New(raw As String, name As String)
            MyBase.New(raw)
            Me.Name = name
        End Sub

        ''' <summary>
        ''' The name of the parameter specified within
        ''' the placeholder
        ''' </summary>
        Public ReadOnly Property Name As String

    End Class
End NameSpace