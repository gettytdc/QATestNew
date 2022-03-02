Imports System.Runtime.Serialization

Imports BluePrism.AutomateProcessCore.My.Resources

''' <summary>
''' Contains network settings that are applied to the ServicePoint for a specific URI
''' </summary>
<DataContract([Namespace]:="bp"), Serializable>
Public Class UriWebConnectionSettings : Inherits WebConnectionBaseSettings

    <DataMember>
    Private mBaseUri As Uri


    Public Sub New(uri As String)
        MyBase.New()
        ValidateUri(uri)
    End Sub

    Public Sub New(uri As String, connectionLimit As Integer, connectionTimeout As Integer?, maxIdleTime As Integer)

        MyBase.New(connectionLimit, maxIdleTime, connectionTimeout)
        ValidateUri(uri)

    End Sub

    Private Sub ValidateUri(uri As String)
        Try
            mBaseUri = New Uri(uri)
        Catch
            Throw New ArgumentException(String.Format(WebConnectionSettingsResources.Error_InvalidUri_Template, uri))
        End Try

        Dim uriString = mBaseUri.ToString()
        Dim trimmedUriString = uriString.Substring(0, uriString.Length - String.Join("", mBaseUri.Segments).Length)
        If mBaseUri <> New Uri(trimmedUriString) Then _
            Throw New ArgumentException(String.Format(WebConnectionSettingsResources.Error_InvalidUri_NotBaseOnly_Template,
                                                      uri))
    End Sub

    Public ReadOnly Property BaseUri As Uri
        Get
            Return mBaseUri
        End Get
    End Property

    Public Overrides Function Equals(obj As Object) As Boolean
        Dim uriSettings = TryCast(obj, UriWebConnectionSettings)
        If uriSettings Is Nothing Then Return False
        Dim connectionTimeoutsAreEqual = Nullable.Equals(uriSettings.ConnectionLeaseTimeout, ConnectionLeaseTimeout)

        Return uriSettings.BaseUri = BaseUri AndAlso
                     connectionTimeoutsAreEqual AndAlso
                     uriSettings.ConnectionLimit = ConnectionLimit AndAlso
                     uriSettings.MaxIdleTime = MaxIdleTime
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return If(ConnectionLeaseTimeout.HasValue, ConnectionLeaseTimeout.Value, -999) Xor
                     ConnectionLimit Xor
                     MaxIdleTime Xor
                     mBaseUri.ToString().GetHashCode()
    End Function

End Class
