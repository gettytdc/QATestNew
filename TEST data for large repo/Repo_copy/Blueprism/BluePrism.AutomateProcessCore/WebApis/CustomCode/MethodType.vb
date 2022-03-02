Namespace WebApis.CustomCode

    ''' <summary>
    ''' Enum describing the different types of custom code method that may be called
    ''' when a Web API Action is executed
    ''' </summary>
    Public Enum MethodType
        ''' <summary>
        ''' Generates an HTTP Request Body from the Web API Action's input parameters
        ''' </summary>
        RequestContent = 0

        ''' <summary>
        ''' Generates a Web API Action's output parameters from an HTTP Response body
        ''' </summary>
        OutputParameter = 1
    End Enum
End Namespace
