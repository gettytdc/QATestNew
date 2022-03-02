
Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Pagination
Imports Func

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class WorkQueueParameters
    <DataMember(Name:="s", EmitDefaultValue:=False)>
    Public Property SortBy As WorkQueueSortByProperty
    <DataMember(Name:="o", EmitDefaultValue:=False)>
    Public Property ItemsPerPage As Integer
    <DataMember(Name:="p", EmitDefaultValue:=False)>
    Public Property PageNumber As Integer
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property NameFilter As DataFilter(Of String)
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property QueueStatusFilter As DataFilter(Of QueueStatus)
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property KeyFieldFilter As DataFilter(Of String)
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property MaxAttemptsFilter As DataFilter(Of Integer)
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
   Public Property PendingItemsCountFilter As DataFilter(Of Integer)
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
   Public Property LockedItemsCountFilter As DataFilter(Of Integer)
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
   Public Property CompletedItemsCountFilter As DataFilter(Of Integer)
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
   Public Property  ExceptionedItemsCountFilter As DataFilter(Of Integer)
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property TotalItemCountFilter As DataFilter(Of Integer)
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
   Public Property  AverageItemWorkTimeFilter As DataFilter(Of Integer)
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
   Public Property  TotalCaseDurationFilter As DataFilter(Of Integer)
   <IgnoreDataMember>
   Public Property PagingToken As [Option](Of WorkQueuePagingToken)    
End Class
