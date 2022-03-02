Imports System.Runtime.Serialization
Imports BluePrism.BPCoreLib

''' <summary>
''' Enum to show restricted status of the group
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp", Name:="ps")>
Public Enum PermissionState
    ''' <summary>
    ''' No restriction exists
    ''' </summary>
    <EnumMember(Value:="U")>
    <FriendlyName("Unrestricted")>
    UnRestricted

    ''' <summary>
    ''' The group has restricted permissions applied directly 
    ''' to it
    ''' </summary>
    <EnumMember(Value:="R")>
    <FriendlyName("Restricted")>
    Restricted

    ''' <summary>
    ''' The group has restricted permissions inherited from 
    ''' a group further up the tree.
    ''' </summary>
    <EnumMember(Value:="I")>
    <FriendlyName("Restricted by inheritance")>
    RestrictedByInheritance

    ''' <summary>
    ''' Flag to signify that the status of the group is unknown.
    ''' </summary>
    <EnumMember(Value:="X")>
    <FriendlyName("Unknown")>
    Unknown
End Enum
