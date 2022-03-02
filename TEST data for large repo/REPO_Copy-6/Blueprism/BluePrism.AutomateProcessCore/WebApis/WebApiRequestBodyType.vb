Imports System.ComponentModel

Namespace WebApis

    ''' <summary>
    ''' Enumeration of the types of request body content supported in Web API actions
    ''' </summary>
    Public Enum WebApiRequestBodyType

        ''' <summary>
        ''' Default - no body type selected
        ''' </summary>
        None = 0

        ''' <summary>
        ''' Text Template body content
        ''' </summary>
        Template = 1

        ''' <summary>
        ''' Single File body content
        ''' </summary>
        SingleFile = 2

        ''' <summary>
        ''' Multi-part file body content
        ''' </summary>
        MultiFile = 3

        ''' <summary>
        ''' Content generated using custom code
        ''' </summary>
        <Description("Custom Code")>
        CustomCode = 4

    End Enum

End Namespace
