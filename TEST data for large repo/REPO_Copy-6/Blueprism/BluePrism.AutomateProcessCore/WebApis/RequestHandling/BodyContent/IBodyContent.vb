Namespace WebApis.RequestHandling.BodyContent

    ''' <summary>
    ''' Contains configuration for a specific type of body content within the configuration of 
    ''' a Web API
    ''' </summary>
    Public Interface IBodyContent : Inherits IXElement

        ''' <summary>
        ''' The Body Type that this object represents
        ''' </summary>
        ''' <returns>A Web API request body type</returns>
        ReadOnly Property Type As WebApiRequestBodyType

        ''' <summary>
        ''' Get a collection of action parameters that are automatically generated
        ''' for this body type at process level
        ''' </summary>
        Function GetInputParameters() As IEnumerable(Of ActionParameter)

    End Interface

End Namespace
