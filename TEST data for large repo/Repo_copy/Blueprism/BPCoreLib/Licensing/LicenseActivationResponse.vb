Imports Newtonsoft.Json
Imports System.Runtime.Serialization

<Serializable> <DataContract(Namespace:="bp")>
Public Class LicenseActivationResponse
    <JsonProperty("data")>
    Public Property data As String
    
    <JsonProperty("sig64")>
    Public Property sig64 As String 
End Class

<Serializable> <DataContract(Namespace:="bp")>
Public Class LicenseActivationJSONContent
    <DataMember>
    <JsonProperty("ActID")>
    Public Property ActivationRequestID As Integer

    <DataMember>
    <JsonProperty("ActRef")>
    Public Property ActivationReference As Guid

    <DataMember>
    <JsonProperty("EnvID")>
    Public Property EnvironmentID As String

    <DataMember>
    <JsonProperty("Time")>
    Public Property DateTime As DateTime

    <DataMember>
    <JsonProperty("User")>
    Public Property User As String
End Class







