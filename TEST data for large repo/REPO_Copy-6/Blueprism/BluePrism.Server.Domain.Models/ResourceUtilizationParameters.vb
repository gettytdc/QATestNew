Imports System.Runtime.Serialization

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class ResourceUtilizationParameters
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property StartDate As DateTime
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property ResourceIds As IEnumerable(Of Guid)
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property PageNumber As Integer?
    <IgnoreDataMember>
    <CLSCompliantAttribute(False)>
    Public Property PageSize As Integer?
End Class
