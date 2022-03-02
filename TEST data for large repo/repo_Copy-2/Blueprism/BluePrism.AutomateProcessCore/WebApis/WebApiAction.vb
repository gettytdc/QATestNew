Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility
Imports BluePrism.Utilities.Functional

Namespace WebApis

    ''' <summary>
    ''' Configuration that applies to a single Web API action 
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    <KnownType(GetType(ReadOnlyCollection(Of ActionParameter)))>
    <KnownType(GetType(ReadOnlyCollection(Of HttpHeader)))>
    Public Class WebApiAction

        <DataMember>
        Private mId As Integer

        <DataMember>
        Private mName As String

        <DataMember>
        Private mDescription As String

        <DataMember>
        Private mRequest As WebApiRequest

        <DataMember>
        Private mEnabled As Boolean

        <DataMember>
        Private mEnableRequestDataOutputParameter As Boolean

        <DataMember>
        Private mDisableSendingOfRequest As Boolean

        <DataMember>
        Private mParameters As IReadOnlyCollection(Of ActionParameter)

        <DataMember>
        Private mOutputParameterConfiguration As OutputParameterConfiguration

        ''' <summary>
        ''' Creates a new Web API action instance without a unique ID.
        ''' </summary>
        ''' <param name="name">The name of the action.</param>
        ''' <param name="description">The description of the action.</param>
        ''' <param name="enabled">True to enable this action; False to create it
        ''' disabled.</param>
        ''' <param name="enableRequestDataOutputParameter">True to enable to 
        ''' additional output parameter containing the request.</param>
        ''' <param name="disableSendingOfRequest">True to disable the actual sending 
        ''' of the request. False to allow the request to be sent.</param>
        ''' <param name="request">The request details for this action.</param>
        ''' <param name="parameters">The parameters for this action.</param>
        ''' <param name="outputParameterConfiguration">The custom output parameters configuration for this action.</param>
        ''' <exception cref="ArgumentNullException">If any of <paramref name="name"/>,
        ''' <paramref name="description"/>, <paramref name="request"/>, <paramref name="parameters"/> or
        ''' <paramref name="outputParameterConfiguration"/> is null.</exception>
        Public Sub New(
         name As String,
         description As String,
         enabled As Boolean,
         enableRequestDataOutputParameter As Boolean,
         disableSendingOfRequest As Boolean,
         request As WebApiRequest,
         parameters As IEnumerable(Of ActionParameter),
         outputParameterConfiguration As OutputParameterConfiguration)
            Me.New(
                0,
                name,
                description,
                enabled,
                enableRequestDataOutputParameter,
                disableSendingOfRequest,
                request,
                parameters,
                outputParameterConfiguration
                )
        End Sub

        ''' <summary>
        ''' Creates a new Web API action
        ''' </summary>
        ''' <param name="id">The unique ID for this action or 0 if it has no ID as
        ''' yet.</param>
        ''' <param name="name">The name of the action.</param>
        ''' <param name="description">The description of the action.</param>
        ''' <param name="enabled">True to enable this action; False to create it
        ''' disabled.</param>
        ''' <param name="enableRequestDataOutputParameter">True to enable to 
        ''' additional output parameter containing the request.</param>
        ''' <param name="disableSendingOfRequest">True to disable the actual sending 
        ''' of the request. False to allow the request to be sent.</param>
        ''' <param name="request">The request details for this action.</param>
        ''' <param name="parameters">The parameters for this action.</param>
        ''' <param name="outputParameterConfiguration">The custom output parameters configuration for this action.</param>
        ''' <exception cref="ArgumentNullException">If any of <paramref name="name"/>,
        ''' <paramref name="description"/>, <paramref name="request"/>, <paramref name="parameters"/> 
        ''' or <paramref name="outputParameterConfiguration"/> is null.</exception>
        Public Sub New(
         id As Integer,
         name As String,
         description As String,
         enabled As Boolean,
         enableRequestDataOutputParameter As Boolean,
         disableSendingOfRequest As Boolean,
         request As WebApiRequest,
         parameters As IEnumerable(Of ActionParameter),
         outputParameterConfiguration As OutputParameterConfiguration)

            If name Is Nothing Then Throw New ArgumentNullException(NameOf(name))
            If description Is Nothing Then _
             Throw New ArgumentNullException(NameOf(description))
            If request Is Nothing Then _
             Throw New ArgumentNullException(NameOf(request))
            If parameters Is Nothing Then _
             Throw New ArgumentNullException(NameOf(parameters))
            If outputParameterConfiguration Is Nothing Then _
             Throw New ArgumentNullException(NameOf(OutputParameters))

            mId = id
            mName = name
            mDescription = description
            mEnabled = enabled
            mEnableRequestDataOutputParameter = enableRequestDataOutputParameter
            mDisableSendingOfRequest = disableSendingOfRequest
            mRequest = request
            mParameters = parameters.ToList().AsReadOnly
            mOutputParameterConfiguration = outputParameterConfiguration

        End Sub

        ''' <summary>
        ''' The id of the action
        ''' </summary>
        Public ReadOnly Property Id As Integer
            Get
                Return mId
            End Get
        End Property

        ''' <summary>
        ''' The name of the action
        ''' </summary>
        Public ReadOnly Property Name As String
            Get
                Return mName
            End Get
        End Property

        ''' <summary>
        ''' Description of the action
        ''' </summary>
        Public ReadOnly Property Description As String
            Get
                Return mDescription
            End Get
        End Property

        ''' <summary>
        ''' Determines whether the action is enabled
        ''' </summary>
        Public ReadOnly Property Enabled As Boolean
            Get
                Return mEnabled
            End Get
        End Property

        ''' <summary>
        ''' Determines whether the additional output parameter 
        ''' containing information about the request should be generated
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EnableRequestDataOutputParameter As Boolean
            Get
                Return mEnableRequestDataOutputParameter
            End Get
        End Property

        Public ReadOnly Property DisableSendingOfRequest As Boolean
            Get
                Return mDisableSendingOfRequest
            End Get
        End Property

        ''' <summary>
        ''' Details about the HTTP request that is sent to execute the action
        ''' </summary>
        Public ReadOnly Property Request As WebApiRequest
            Get
                Return mRequest
            End Get
        End Property

        ''' <summary>
        ''' Parameters available
        ''' </summary>
        Public ReadOnly Property Parameters As IReadOnlyCollection(Of ActionParameter)
            Get
                Return mParameters
            End Get
        End Property


        Public ReadOnly Property OutputParameterConfiguration As OutputParameterConfiguration
            Get
                Return mOutputParameterConfiguration
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{NameOf(Name)}: {Name}, {NameOf(Request)}: {Request}"

        End Function

        ''' <summary>
        ''' Generates an XML element representation of this instance of the
        ''' <see cref="WebApiAction"/> object.
        ''' </summary>
        ''' <returns>
        ''' An XML Element representing this object
        ''' </returns>
        Public Function ToXElement() As XElement
            Return _
                <action
                    id=<%= Id %>
                    name=<%= Name %>
                    enabled=<%= Enabled %>
                    enableRequestOutputParameter=<%= EnableRequestDataOutputParameter %>
                    disableSendingOfRequest=<%= DisableSendingOfRequest %>>
                    <description>
                        <%= From d In New XCData(Description).ToEscapedEnumerable()
                            Select d
                        %>
                    </description>
                    <%= Request.ToXElement() %>
                    <parameters>
                        <%= From p In Parameters Select x = p.ToXElement() %>
                    </parameters>
                    <%= OutputParameterConfiguration.ToXElement() %>
                </action>
        End Function

        ''' <summary>
        ''' Create a new instance of <see cref="WebApiAction"/> from an XML Element
        ''' that represents that object.
        ''' </summary>
        ''' <returns>
        ''' A new instance of <see cref="WebApiAction"/> from an XML Element
        ''' that represents that object.
        ''' </returns>
        Public Shared Function FromXElement(element As XElement) As WebApiAction
            If Not element.Name.LocalName.Equals("action") Then _
                Throw New MissingXmlObjectException("action")

            Dim name = element.Attribute("name")?.Value
            If name Is Nothing Then Throw New MissingXmlObjectException("name")

            Dim enabled = element.Attribute("enabled")?.Value(Of Boolean)()
            If enabled Is Nothing Then Throw New MissingXmlObjectException("enabled")

            Dim enableRequestOutputParameter = element.Attribute("enableRequestOutputParameter")?.Value(Of Boolean)()
            If enableRequestOutputParameter Is Nothing Then Throw New MissingXmlObjectException("enableRequestOutputParameter")

            Dim disableSendingOfRequest = element.Attribute("disableSendingOfRequest")?.Value(Of Boolean)()
            If disableSendingOfRequest Is Nothing Then Throw New MissingXmlObjectException("disableSendingOfRequest")

            Dim description = element.
                                Elements.
                                FirstOrDefault(Function(x) x.Name = "description")?.
                                Nodes.
                                OfType(Of XCData).
                                GetConcatenatedValue()
            If description Is Nothing Then Throw New MissingXmlObjectException("description")


            Dim request = element.
                            Elements.
                            FirstOrDefault(Function(x) x.Name = "request")?.
                            Map(Function(x) WebApiRequest.FromXElement(x))
            If request Is Nothing Then Throw New MissingXmlObjectException("request")


            Dim params = element.
                            Elements.
                            FirstOrDefault(Function(x) x.Name = "parameters")?.
                            Elements.
                            Where(Function(x) x.Name = "actionparameter").
                            Select(Function(x) ActionParameter.FromXElement(x))
            If params Is Nothing Then Throw New MissingXmlObjectException("parameters")


            Dim outputParameters = element.
                                Elements.
                                FirstOrDefault(Function(x) x.Name = "outputparameters")?.
                                Map(Function(x) OutputParameterConfiguration.FromXElement(x))

            If outputParameters Is Nothing Then Throw New MissingXmlObjectException("outputparameters")

            Return New WebApiAction(name,
                                    description,
                                    enabled.GetValueOrDefault(),
                                    enableRequestOutputParameter.GetValueOrDefault(),
                                    disableSendingOfRequest.GetValueOrDefault(),
                                    request,
                                    params,
                                    outputParameters)

        End Function


    End Class
End Namespace
