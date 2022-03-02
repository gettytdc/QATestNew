Imports System.Runtime.Serialization
''' <summary>
''' Indicates the usage of this process. Eg Retired processes cannot be run or
''' modified. These values can be combined with a logical OR operation
''' </summary>
<Flags, DataContract([Namespace]:="bp")>
Public Enum ProcessAttributes
    ''' <summary>
    ''' No attributes set on the process
    ''' </summary>
    <EnumMember(Value:="n")> None = 0

    ''' <summary>
    ''' Indicates that the process is retired which means it can't be run or modified 
    ''' </summary>
    <EnumMember(Value:="r")> Retired = 1

    ''' <summary>
    ''' Indicates that the process is published, only published processes can be run
    ''' in control room.
    ''' </summary>
    ''' <remarks>Note that this is not the opposite of Retired, a process can
    ''' be both Published and Retired.</remarks>
    <EnumMember(Value:="p")> Published = 2

    ''' <summary>
    ''' Indicates that the process is published as a Web Service.
    ''' </summary>
    <EnumMember(Value:="w")> PublishedWS = 4

End Enum

