
Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Pagination
Imports Func

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class ScheduleParameters
    <IgnoreDataMember>
    Public Name As DataFilter(Of String)
    <IgnoreDataMember>
    Public RetirementStatus As DataFilter(Of RetirementStatus)
    <IgnoreDataMember>
    Public ItemsPerPage As Integer
    <IgnoreDataMember>
    Public PagingToken As [Option](Of SchedulePagingToken)
End Class
