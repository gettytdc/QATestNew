Imports Newtonsoft.Json

Public Class LicenseActivationRequest
    <JsonProperty("EnvID")>
    Public Property EnvironmentID As String

    <JsonProperty("Path")>
    Public Property ServerPath As String

    <JsonProperty("ActID")>
    Public Property ActivationRequestID As Int32

    <JsonProperty("ActRef")>
    Public Property ActivationReference As String

    <JsonProperty("Time")>
    Public Property RequestDateTime As DateTime

    <JsonProperty("User")>
    Public Property User As String

    <JsonProperty("OrderId")>
    Public Property OrderID As String

    <JsonProperty("ReqID")>
    Public Property LicenseRequestID As String

    <JsonProperty("EnvInfo")>
    Public Property Information As EnvironmentInformation
End Class
