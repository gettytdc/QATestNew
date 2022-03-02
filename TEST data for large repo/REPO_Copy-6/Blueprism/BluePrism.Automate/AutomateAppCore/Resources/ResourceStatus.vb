Imports System.Runtime.Serialization
''' <summary>
''' The different display states possible for a resource connection.
''' </summary>
<DataContract([Namespace]:="bp", Name:="r")>
Public Enum ResourceStatus
    ''' <summary>
    ''' State Representing that the resource is busy with sessions
    ''' </summary>
    <EnumMember(Value:="B")> Working

    ''' <summary>
    ''' State Representing that the resource has no work to do 
    ''' </summary>
    <EnumMember(Value:="I")> Idle

    ''' <summary>
    ''' State Representing that the resource is in a warning state
    ''' </summary>
    <EnumMember(Value:="W")> Warning

    ''' <summary>
    ''' State Representing that the resource is offline
    ''' </summary>
    <EnumMember(Value:="O")> Offline

    ''' <summary>
    ''' State Representing that the resource has recently lost communication
    ''' </summary>
    <EnumMember(Value:="M")> Missing

    ''' <summary>
    ''' State Representing that the resource is a login agent and is logged out
    ''' </summary>
    <EnumMember(Value:="A")> LoggedOut

    ''' <summary>
    ''' State Representing that the resource is a member of a pool
    ''' </summary>
    <EnumMember(Value:="P")> Pool

    ''' <summary>
    ''' State Representing that the resource is private and for a specific user's use.
    ''' </summary>
    <EnumMember(Value:="X")> [Private]


End Enum

