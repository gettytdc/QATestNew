Imports System.Runtime.Serialization

Namespace Resources

    <Serializable>
    <DataContract(Name:="ra", [Namespace]:="bp")>
    Public Class RobotAddress

        Public Sub New(id As Guid, name As String, host As String, portNo As Integer, useCertificate As Boolean, fqdName As String)
            ResourceId = id
            ResourceName = name
            HostName = host
            Port = portNo
            UseSsl = useCertificate
            FQDN = fqdName

        End Sub

        <DataMember(Name:="i", EmitDefaultValue:=False)>
        Public ReadOnly Property ResourceId As Guid

        <DataMember(Name:="n")>
        Public ReadOnly Property ResourceName As String

        <DataMember(Name:="h")>
        Public ReadOnly Property HostName As String

        <DataMember(Name:="p")>
        Public ReadOnly Property Port As Integer

        <DataMember(Name:="s")>
        Public ReadOnly Property UseSsl As Boolean

        <DataMember(Name:="f")>
        Public ReadOnly Property FQDN As String

    End Class

End Namespace
