Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

Namespace WebApis
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class WebApiConfigurationSettings
        Private Const HTTP_REQUEST_CONNECTION_TIMEOUT_DEFAULT = 10
        Private Const AUTH_SERVER_REQUEST_CONNECTION_TIMEOUT_DEFAULT = 10

        <DataMember>
        Private mHttpRequestConnectionTimeout As Integer
        <DataMember>
        Private mAuthServerRequestConnectionTimeout As Integer

        Public ReadOnly Property HttpRequestConnectionTimeout As Integer
            Get
                Return mHttpRequestConnectionTimeout
            End Get
        End Property

        Public ReadOnly Property AuthServerRequestConnectionTimeout As Integer
            Get
                Return mAuthServerRequestConnectionTimeout
            End Get
        End Property

        Public Sub New()
            ApplyDefaults()
        End Sub

        Public Sub New(
                  httpRequestTimeout As Integer,
                  authServerRequestTimeout As Integer)

            mHttpRequestConnectionTimeout = httpRequestTimeout
            mAuthServerRequestConnectionTimeout = authServerRequestTimeout
        End Sub

        Private Sub ApplyDefaults()
            mHttpRequestConnectionTimeout = HTTP_REQUEST_CONNECTION_TIMEOUT_DEFAULT
            mAuthServerRequestConnectionTimeout = AUTH_SERVER_REQUEST_CONNECTION_TIMEOUT_DEFAULT
        End Sub

        Public Function ToXElement() As XElement
            Return _
                <configurationsettings
                    requesttimeout=<%= HttpRequestConnectionTimeout %>
                    authserverrequesttimeout=<%= AuthServerRequestConnectionTimeout %>/>
        End Function

        Public Shared Function FromXElement(element As XElement) As WebApiConfigurationSettings

            If Not element.Name.LocalName.Equals("configurationsettings") Then _
                Throw New MissingXmlObjectException("configurationsettings")

            Dim requestTimeout = element.Attribute("requesttimeout")?.Value(Of Integer)()

            If requestTimeout Is Nothing Then _
                Throw New MissingXmlObjectException("requesttimeout")

            Dim authServerRequestTimeout = element.Attribute("authserverrequesttimeout")?.Value(Of Integer)()

            If authServerRequestTimeout Is Nothing Then _
                Throw New MissingXmlObjectException("authserverrequesttimeout")

            Return New WebApiConfigurationSettings(
                requestTimeout.Value,
                authServerRequestTimeout.Value)

        End Function
    End Class
End Namespace
