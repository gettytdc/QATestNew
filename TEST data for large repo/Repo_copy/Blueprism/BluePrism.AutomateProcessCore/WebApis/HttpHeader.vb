Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models

Namespace WebApis

    ''' <summary>
    ''' Defines an individual HTTP header for Web API configuration
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class HttpHeader

        <DataMember>
        Private mId As Integer

        <DataMember>
        Private mName As String

        <DataMember>
        Private mValue As String

        ''' <summary>
        ''' Creates a new HttpHeader without a unique ID
        ''' </summary>
        ''' <param name="name">The name used for the header field name in the HTTP 
        ''' request</param>
        ''' <param name="value">The value for the header</param>
        ''' <exception cref="ArgumentNullException">If either <paramref name="name"/>
        ''' or <paramref name="value"/> is null.</exception>
        Public Sub New(name As String, value As String)
            Me.New(0, name, value)
        End Sub

        ''' <summary>
        ''' Creates a new HttpHeader
        ''' </summary>
        ''' <param name="id">The id of the header</param>
        ''' <param name="name">The name used for the header field name in the HTTP 
        ''' request</param>
        ''' <param name="value">The value for the header</param>
        ''' <exception cref="ArgumentNullException">If either <paramref name="name"/>
        ''' or <paramref name="value"/> is null.</exception>
        Public Sub New(id As Integer, name As String, value As String)
            If name Is Nothing Then Throw New ArgumentNullException(NameOf(name))
            If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))
            mId = id
            mName = name
            mValue = value
        End Sub

        ''' <summary>
        ''' The id of the header
        ''' </summary>
        Public ReadOnly Property Id As Integer
            Get
                Return mId
            End Get
        End Property

        ''' <summary>
        ''' The name used for the header field name in the HTTP request
        ''' </summary>
        Public ReadOnly Property Name As String
            Get
                Return mName
            End Get
        End Property

        ''' <summary>
        ''' The value for the header
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Value As String
            Get
                Return mValue
            End Get
        End Property

        ''' <summary>
        ''' Generates an XML element representation of this instance of the
        ''' <see cref="HttpHeader"/> object.
        ''' </summary>
        ''' <returns>
        ''' An XML Element representing this object
        ''' </returns>
        Public Function ToXElement() As XElement
            Return <httpheader name=<%= Name %> value=<%= Value %>/>
        End Function

        ''' <summary>
        ''' Create a new instance of <see cref="HttpHeader"/> from an XML Element
        ''' that represents that object.
        ''' </summary>
        ''' <returns>
        ''' A new instance of <see cref="HttpHeader"/> from an XML Element
        ''' that represents that object.
        ''' </returns>
        Public Shared Function FromXElement(element As XElement) As HttpHeader

            If Not element.Name.LocalName.Equals("httpheader") Then _
             Throw New MissingXmlObjectException("httpheader")

            Dim name = element.Attribute("name")?.Value
            If name Is Nothing Then Throw New MissingXmlObjectException("name")

            Dim value = element.Attribute("value")?.Value
            If value Is Nothing Then Throw New MissingXmlObjectException("value")

            Return New HttpHeader(name, value)

        End Function

    End Class

End Namespace
