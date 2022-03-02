Imports Newtonsoft.Json

Public Class EnvironmentInformation

    <JsonProperty("Res")>
    Public Property NumberOfResources As Int32

    <JsonProperty("Apps")>
    Public Property NumberOfAppServers As Int32

    <JsonProperty("Auth")>
    Public Property AuthenticationMethod As Int32
End Class
