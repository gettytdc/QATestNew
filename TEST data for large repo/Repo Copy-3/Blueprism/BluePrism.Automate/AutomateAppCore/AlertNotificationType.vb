Imports System.Runtime.Serialization
''' <summary>
''' The types of events
''' </summary>
<Flags, DataContract([Namespace]:="bp")>
Public Enum AlertNotificationType
    <EnumMember> None = 0
    <EnumMember> PopUp = 1
    <EnumMember> MessageBox = 2
    <EnumMember> Taskbar = 4
    <EnumMember> Sound = 8
End Enum
