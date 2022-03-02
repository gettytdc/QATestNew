Imports System.Runtime.Serialization

Namespace Dashboard
    <Serializable>
    <DataContract([Namespace]:="bp")>
    Public Class ResourceUtilization
        <IgnoreDataMember>
        Public ResourceId As Guid
        <IgnoreDataMember>
        Public DigitalWorkerName As String
        <IgnoreDataMember>
        Public UtilizationDate As DateTimeOffset
        <IgnoreDataMember>
        Public Usages As IEnumerable(Of Integer)
    End Class
End NameSpace
