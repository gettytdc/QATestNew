Imports System.Runtime.Serialization

Namespace Dashboard

    <Serializable>
    <DataContract([Namespace]:="bp")>
    Public Class WorkQueueComposition
        <IgnoreDataMember>
        Public Id As Guid
        <IgnoreDataMember>
        Public Name As String
        <IgnoreDataMember>
        Public Completed As Integer
        <IgnoreDataMember>
        Public Pending As Integer
        <IgnoreDataMember>
        Public Deferred As Integer
        <IgnoreDataMember>
        Public Locked As Integer
        <IgnoreDataMember>
        Public Exceptioned As Integer
    End Class
End NameSpace
