Namespace WebApis.RequestHandling.BodyContent

    ''' <summary>
    ''' Defines how the body content will be generated when calling a Web API Action
    ''' </summary>
    Public Interface IBodyContentGenerator

        ''' <summary>
        ''' Ensure the body content instance is compatible with this
        ''' handler
        ''' </summary>
        Function CanHandle(requestBodyContent As IBodyContent) As Boolean

        ''' <summary>
        ''' The formatted result for the body content
        ''' The formatted result for the body content
        ''' </summary>
        Function GetBodyContent(context As ActionContext) As IBodyContentResult

    End Interface

End Namespace