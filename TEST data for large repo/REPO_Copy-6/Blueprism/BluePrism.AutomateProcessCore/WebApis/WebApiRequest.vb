Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Net.Http
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Utilities.Functional
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace WebApis

    ''' <summary>
    ''' Contains details of the HTTP request that will be made by a WebApiAction
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    <KnownType(GetType(ReadOnlyCollection(Of HttpHeader)))>
    <KnownType(GetType(TemplateBodyContent))>
    <KnownType(GetType(NoBodyContent))>
    <KnownType(GetType(FileCollectionBodyContent))>
    <KnownType(GetType(SingleFileBodyContent))>
    <KnownType(GetType(CustomCodeBodyContent))>
    Public Class WebApiRequest

        ''' <summary>
        ''' Field used to support serialization of HttpMethod property
        ''' </summary>
        <DataMember>
        Private mHttpMethodValue As String

        <NonSerialized>
        Private mHttpMethod As HttpMethod = Nothing

        <DataMember>
        Private mUrlPath As String

        <DataMember>
        Private mHeaders As IReadOnlyCollection(Of HttpHeader)

        <DataMember>
        Private mBodyContent As IBodyContent


        ''' <summary>
        ''' Creates a new WebApiRequest
        ''' </summary>
        ''' <param name="httpMethod">The HTTP method to use in the request</param>
        ''' <param name="urlPath">The path within the URL of the resource requested 
        ''' in the HTTP request made to execute the action. This is combined with 
        ''' the URI defined in the parent WebApiConfiguration's BaseUrl.</param>
        ''' <param name="headers">HTTP headers sent with the request</param>
        ''' <param name="bodyContent">The type of content stored in the body template</param>
        Sub New(httpMethod As HttpMethod, urlPath As String, headers As IEnumerable(Of HttpHeader), bodyContent As IBodyContent)

            If httpMethod Is Nothing Then Throw New ArgumentNullException(NameOf(httpMethod))
            If urlPath Is Nothing Then Throw New ArgumentNullException(NameOf(urlPath))
            If headers Is Nothing Then Throw New ArgumentNullException(NameOf(headers))
            If bodyContent Is Nothing Then Throw New ArgumentNullException(NameOf(bodyContent))

            mHttpMethod = httpMethod
            mHttpMethodValue = httpMethod.Method
            mUrlPath = urlPath
            mHeaders = headers.ToList().AsReadOnly()
            mBodyContent = bodyContent

        End Sub

        ''' <summary>
        ''' The HTTP method to use in the request
        ''' </summary>
        Public ReadOnly Property HttpMethod As HttpMethod
            Get
                If mHttpMethod Is Nothing Then
                    mHttpMethod = New HttpMethod(mHttpMethodValue)
                End If
                Return mHttpMethod
            End Get
        End Property

        ''' <summary>
        ''' The path within the URL of the resource requested in the HTTP request
        ''' made to execute the action. This is combined with the URI defined in
        ''' the parent WebApiConfiguration's BaseUrl.
        ''' </summary>
        Public ReadOnly Property UrlPath As String
            Get
                Return mUrlPath
            End Get
        End Property

        ''' <summary>
        ''' HTTP headers sent with the request
        ''' </summary>
        Public ReadOnly Property Headers As IReadOnlyCollection(Of HttpHeader)
            Get
                Return mHeaders
            End Get
        End Property

        ''' <summary>
        ''' Template for body content sent with the request
        ''' </summary>
        Public ReadOnly Property BodyContent As IBodyContent
            Get
                Return mBodyContent
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{NameOf(HttpMethod)}: {HttpMethod}, {NameOf(UrlPath)}: {UrlPath}"

        End Function

        ''' <summary>
        ''' Generates an XML element representation of this instance of the
        ''' <see cref="WebApiRequest"/> object.
        ''' </summary>
        ''' <returns>
        ''' An XML Element representing this object
        ''' </returns>
        Public Function ToXElement() As XElement
            Return _
                <request
                    httpmethod=<%= HttpMethod.Method %>
                    urlpath=<%= UrlPath %>>
                    <headers>
                        <%= Headers.Select(Function(x) x.ToXElement()) %>
                    </headers>
                    <%= BodyContent.ToXElement() %>
                </request>
        End Function

        ''' <summary>
        ''' Create a new instance of <see cref="WebApiRequest"/> from an XML Element
        ''' that represents that object.
        ''' </summary>
        ''' <returns>
        ''' A new instance of <see cref="WebApiRequest"/> from an XML Element
        ''' that represents that object.
        ''' </returns>
        Public Shared Function FromXElement(element As XElement) As WebApiRequest

            If Not element.Name.LocalName.Equals("request") Then _
                Throw New MissingXmlObjectException("request")

            Dim httpMethod = element.
                                Attribute("httpmethod")?.
                                Value.
                                Map(Function(x) New HttpMethod(x))

            If httpMethod Is Nothing Then Throw New MissingXmlObjectException("httpmethod")


            Dim urlPath = element.Attribute("urlpath")?.Value
            If urlPath Is Nothing Then _
                Throw New MissingXmlObjectException("urlpath")

            Dim headers = element.
                            Elements.
                            FirstOrDefault(Function(x) x.Name = "headers")?.
                            Elements.
                            Where(Function(x) x.Name = "httpheader").
                            Select(Function(x) HttpHeader.FromXElement(x))

            If headers Is Nothing Then _
                Throw New MissingXmlObjectException("headers")

            Dim bodyContentElement = element.
                                Elements.
                                FirstOrDefault(Function(x) x.Name = "bodycontent")

            If bodyContentElement Is Nothing Then _
                Throw New MissingXmlObjectException("bodycontent")

            Dim bodyContent = BodyContentDeserializer.Deserialize(bodyContentElement)

            Return New WebApiRequest(httpMethod, urlPath, headers, bodyContent)

        End Function

    End Class

End Namespace
