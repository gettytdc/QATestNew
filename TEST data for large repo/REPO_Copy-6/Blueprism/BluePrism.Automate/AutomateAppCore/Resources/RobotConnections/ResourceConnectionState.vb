
Imports System.Runtime.Serialization

Namespace Resources
    ''' <summary>
    ''' The different states possible for a resource connection.
    ''' </summary>
    <DataContract([Namespace]:="bp", Name:="rcs")>
    Public Enum ResourceConnectionState
        ''' <summary>
        ''' We are attempting to establish a connection with this resource.
        ''' </summary>
        <EnumMember(Value:="C")> Connecting
        ''' <summary>
        ''' An active connection with this resource exists.
        ''' </summary>
        <EnumMember(Value:="E")> Connected
        ''' <summary>
        ''' An automate user has exclusive access to the resource.
        ''' </summary>
        <EnumMember(Value:="I")> InUse
        ''' <summary>
        ''' The machine can not be contacted.
        ''' </summary>
        <EnumMember(Value:="O")> Offline
        ''' <summary>
        ''' An error has occured in this connection.
        ''' </summary>
        <EnumMember(Value:="X")> [Error]
        ''' <summary>
        ''' The machine is not available. (e.g. local use only, and machine is remote)
        ''' Machines with this status are never shown in Control Room.
        ''' </summary>
        <EnumMember(Value:="U")> Unavailable
        ''' <summary>
        ''' The connection to this machine is being held by the appplication server.
        ''' </summary>
        <EnumMember(Value:="S")> Server
        ''' <summary>
        ''' Sleep state 
        ''' </summary>
        <EnumMember(Value:="Z")> Sleep
        ''' <summary>
        ''' Disconnected state 
        ''' </summary>
        <EnumMember(Value:="D")> Disconnected

        <EnumMember(Value:="H")> Hidden
    End Enum

End Namespace
