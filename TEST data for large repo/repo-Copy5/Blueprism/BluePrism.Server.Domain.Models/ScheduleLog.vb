Imports Func
Imports System.Runtime.Serialization

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class ScheduleLog
    <DataMember(Name:="li", EmitDefaultValue:=False)>
    Public Property ScheduleLogId As Integer

    <DataMember(Name:="s", EmitDefaultValue:=False)>
    Public Property Status As ItemStatus

    <DataMember(Name:="t", EmitDefaultValue:=False)>
    Public Property StartTime As [Option](Of DateTime)

    <DataMember(Name:="e", EmitDefaultValue:=False)>
    Public Property EndTime As [Option](Of DateTime)

    <DataMember(Name:="sn", EmitDefaultValue:=False)>
    Public Property ServerName As String

    <DataMember(Name:="i", EmitDefaultValue:=False)>
    Public Property ScheduleId As Integer

    <DataMember(Name:="n", EmitDefaultValue:=False)>
    Public Property ScheduleName As String
End Class
