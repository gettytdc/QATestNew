Imports System.Linq
Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace WebApis

    ''' <summary>
    ''' Helper class for constructing WebApiConfiguration objects. WebApiConfiguration is a 
    ''' fairly complex immutable object (not totally immutable at the moment actually - it might
    ''' be made fully immutable or fully mutable - will depend on what happens as we build the 
    ''' configuration edit forms). The builder class makes it easier to build up the values used 
    ''' to initialise a new object and insulates code using the object against signature changes
    ''' and the addition of new properties. This might be moved to the test namespace in the
    ''' future.
    ''' </summary>
    Public Class WebApiConfigurationBuilder

        Private mBaseUrl As String = "https://mybiz.org/"
        Private mCommonHeaders As New List(Of HttpHeader)
        Private mCommonParameters As New List(Of ActionParameter)
        Private mCommonAuthentication As IAuthentication = New EmptyAuthentication()
        Private mActions As New List(Of WebApiAction)
        Private mCommonCode As CodeProperties = New CodeProperties("", CodeLanguage.VisualBasic, {}, {})
        Private mConfigurationSettings As WebApiConfigurationSettings = New WebApiConfigurationSettings()

        ''' <summary>
        ''' Sets the authentication will be assigned to the created object.
        ''' </summary>
        ''' <returns>The current WebApiConfigurationBuilder instance - this is designed
        ''' to enable chained method calls when setting up objects</returns>
        Public Function WithCommonAuthentication(authentication As IAuthentication) As WebApiConfigurationBuilder
            mCommonAuthentication = authentication
            Return Me
        End Function

        ''' <summary>
        ''' Sets the BaseUrl that will be used to create the object
        ''' </summary>
        ''' <returns>The current WebApiConfigurationBuilder instance - this is designed
        ''' to enable chained method calls when setting up objects</returns>
        Public Function WithBaseUrl(url As String) As WebApiConfigurationBuilder
            mBaseUrl = url
            Return Me
        End Function


        ''' <summary>
        ''' Adds an action parameter that will be used to initialise the object's 
        ''' common parameters
        ''' </summary>
        ''' <returns>The current WebApiConfigurationBuilder instance - this is designed
        ''' to enable chained method calls when setting up objects</returns>
        Public Function WithParameter(name As String, dataType As DataType, exposeToProcess As Boolean) As WebApiConfigurationBuilder
            Dim initialValue = New clsProcessValue(dataType, "")
            Dim parameter As New ActionParameter(name, "", dataType, exposeToProcess, initialValue)
            Return WithParameters({parameter})
        End Function

        ''' <summary>
        ''' Sets the list of action parameters that will be used to initialise the object's
        ''' common parameters
        ''' </summary>
        ''' <returns>The current WebApiConfigurationBuilder instance - this is designed
        ''' to enable chained method calls when setting up objects</returns>
        Public Function WithParameters(parameters As IEnumerable(Of ActionParameter)) As WebApiConfigurationBuilder
            mCommonParameters.AddRange(parameters)
            Return Me
        End Function

        ''' <summary>
        ''' Sets the HttpHeaderCollection that will be used to initialise the object's
        ''' common headers
        ''' </summary>
        ''' <returns>The current WebApiConfigurationBuilder instance - this is designed
        ''' to enable chained method calls when setting up objects</returns>
        Public Function WithHeaders(headers As IEnumerable(Of HttpHeader)) As WebApiConfigurationBuilder
            mCommonHeaders.AddRange(headers)
            Return Me
        End Function

        ''' <summary>
        ''' Adds a header that will be used to initialise the object's common headers
        ''' </summary>
        ''' <returns>The current WebApiConfigurationBuilder instance - this is designed
        ''' to enable chained method calls when setting up objects</returns>
        Public Function WithHeader(name As String, value As String, Optional id As Integer = 0) As WebApiConfigurationBuilder
            mCommonHeaders.Add(New HttpHeader(id, name, value))
            Return Me
        End Function

        ''' <summary>
        ''' Adds the common code properties that will be used to initialise the object
        ''' </summary>
        ''' <returns>The current WebApiConfigurationBuilder instance - this is designed
        ''' to enable chained method calls when setting up objects</returns>
        Public Function WithCommonCode(commonCode As CodeProperties) As WebApiConfigurationBuilder
            mCommonCode = commonCode
            Return Me
        End Function

        ''' <summary>
        ''' Adds the configuration settings that will be used to initialise the object
        ''' </summary>
        ''' <returns>The current WebApiConfigurationBuilder instance - this is designed
        ''' to enable chained method calls when setting up objects</returns>
        Public Function WithConfigurationSettings(settings As WebApiConfigurationSettings) As WebApiConfigurationBuilder
            mConfigurationSettings = settings
            Return Me
        End Function

        ''' <summary>
        ''' Adds an action that will be used when creating the object
        ''' </summary>
        ''' <returns>The current WebApiConfigurationBuilder instance - this is designed
        ''' to enable chained method calls when setting up objects</returns>
        Public Function WithAction(name As String,
                                   httpMethod As HttpMethod,
                                   urlPath As String,
                                   Optional description As String = "",
                                   Optional enabled As Boolean = True,
                                   Optional enableRequestOutputParameter As Boolean = False,
                                   Optional disableSendingOfRequest As Boolean = False,
                                   Optional bodyContent As IBodyContent = Nothing,
                                   Optional headers As IEnumerable(Of HttpHeader) = Nothing,
                                   Optional parameters As IEnumerable(Of ActionParameter) = Nothing,
                                   Optional outputParameterConfiguration As OutputParameterConfiguration = Nothing,
                                   Optional id As Integer = 0
                                  ) As WebApiConfigurationBuilder



            headers = If(headers, New HttpHeader() {})
            bodyContent = If(bodyContent, New TemplateBodyContent("Test Template"))
            Dim request As New WebApiRequest(httpMethod, urlPath, headers, bodyContent)

            parameters = If(parameters, Enumerable.Empty(Of ActionParameter))
            outputParameterConfiguration = If(outputParameterConfiguration, New OutputParameterConfiguration(Enumerable.Empty(Of ResponseOutputParameter), ""))
            Dim action = New WebApiAction(id,
                                          name,
                                          description,
                                          enabled,
                                          enableRequestOutputParameter,
                                          disableSendingOfRequest,
                                          request,
                                          parameters,
                                          outputParameterConfiguration)
            mActions.Add(action)

            Return Me

        End Function

        ''' <summary>
        ''' Executes the specified action with the current <see cref="WebApiConfigurationBuilder"/>
        ''' instance.
        ''' </summary>
        ''' <param name="customise">The action to execute. A null value will be ignored
        ''' and will not result in an errors</param>
        ''' <returns>The current WebApiConfigurationBuilder instance - this is designed
        ''' to enable chained method calls when setting up objects</returns>
        Public Function Configure(customise As Action(Of WebApiConfigurationBuilder)) As WebApiConfigurationBuilder
            If customise IsNot Nothing Then
                customise(Me)
            End If
            Return Me
        End Function

        ''' <summary>
        ''' Creates a new WebApiConfiguration object with the values that
        ''' have been set up
        ''' </summary>
        ''' <returns>The new WebApiConfiguration object</returns>
        Public Function Build() As WebApiConfiguration

            Return New WebApiConfiguration(mBaseUrl, mCommonHeaders, mCommonParameters, mCommonCode,
                                           mActions.AsReadOnly, mCommonAuthentication, mConfigurationSettings)

        End Function

        ''' <summary>
        ''' Clears existing data
        ''' </summary>
        ''' <returns>The current WebApiConfigurationBuilder instance - this is designed
        ''' to enable chained method calls when setting up objects</returns>
        Public Function Reset() As WebApiConfigurationBuilder

            mActions = New List(Of WebApiAction)
            Return Me

        End Function


    End Class

End Namespace