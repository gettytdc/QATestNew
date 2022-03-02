Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models.Pagination
Imports Func

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class SessionLogsParameters
    <IgnoreDataMember>
    Public Property ItemsPerPage As Integer
    <IgnoreDataMember>
    Public Property PagingToken As [Option](Of SessionLogsPagingToken)
End Class
