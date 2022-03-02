Imports System.Runtime.Serialization

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class ResourcesSummaryUtilizationParameters

    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property StartDate As DateTime
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property EndDate As DateTime
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property ResourceIds As IEnumerable(Of Guid)
   
End Class
