Imports System.Runtime.Serialization

<Serializable>
<DataContract(Namespace:="bp")>
Public Class LicenseActivationEvent

    Public Sub New(success As Boolean, eventDateTime As DateTime, eventType As LicenseEventTypes, user As String)
        Me.Success = success
        Me.EventDateTime = eventDateTime
        Me.EventType = eventType
        Me.EventUser = user
    End Sub

    <DataMember>
    Public Property Success As Boolean

    <DataMember>
    Public Property EventDateTime As DateTime

    <DataMember>
    Public Property EventType As LicenseEventTypes

    <DataMember>
    Public Property EventUser As String

End Class
