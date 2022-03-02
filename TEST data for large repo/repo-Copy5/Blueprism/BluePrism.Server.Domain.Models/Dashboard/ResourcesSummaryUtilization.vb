Imports System.Runtime.Serialization

Namespace Dashboard
    <Serializable>
    <DataContract([Namespace]:="bp")>
    Public Class ResourcesSummaryUtilization
        <IgnoreDataMember>
        Public Dates As DateTimeOffset
        <IgnoreDataMember>
        Public Usage As Integer
    End Class
End NameSpace
