Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Pagination
Imports Func

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class ScheduleLogParameters
    <IgnoreDataMember>
    Public Property ScheduleId As DataFilter(Of Integer)
    <IgnoreDataMember>
    Public Property ItemsPerPage As Integer
    <IgnoreDataMember>
    Public Property StartTime As DataFilter(Of DateTimeOffset)
    <IgnoreDataMember>
    Public Property EndTime As DataFilter(Of DateTimeOffset)
    <IgnoreDataMember>
    Public Property ScheduleLogStatus As DataFilter(Of ItemStatus)
    <IgnoreDataMember>
    Public Property PagingToken As [Option](Of ScheduleLogsPagingToken)

End Class
