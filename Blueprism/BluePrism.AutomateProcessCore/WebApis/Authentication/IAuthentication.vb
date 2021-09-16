Namespace WebApis.Authentication

    ''' <summary>
    ''' Contains configuration for a specific type of authentication within the configuration of 
    ''' a Web API
    ''' </summary>
    Public Interface IAuthentication : Inherits IXElement

        ''' <summary>
        ''' The Authentication Type that this object represents
        ''' </summary>
        ''' <returns>A Web API Authentication type</returns>
        ReadOnly Property Type() As AuthenticationType

        ''' <summary>
        ''' Get a collection of action parameters that are automatically generated
        ''' for this authentication type at process level
        ''' </summary>
        Function GetInputParameters() As IEnumerable(Of ActionParameter)

    End Interface
End Namespace
