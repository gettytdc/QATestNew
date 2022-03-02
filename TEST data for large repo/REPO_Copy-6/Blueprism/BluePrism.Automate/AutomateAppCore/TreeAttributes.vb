Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Core.Resources

<DataContract([Namespace]:="bp")>
<Serializable()>
Public Class TreeAttributes
    <DataMember(Name:="ra", EmitDefaultValue:=False)>
    Public Property RequiredProcessAttributes As ProcessAttributes
    <DataMember(Name:="ua", EmitDefaultValue:=False)>
    Public Property UnacceptableProcessAttributes As ProcessAttributes

    <DataMember(Name:="rr", EmitDefaultValue:=False)>
    Public Property RequiredResourceAttributes As ResourceAttribute
    <DataMember(Name:="ur", EmitDefaultValue:=False)>
    Public Property UnacceptableResourceAttributes As ResourceAttribute

    Public Overrides Function Equals(obj As Object) As Boolean

        Dim other = TryCast(obj, TreeAttributes)
        If other Is Nothing Then Return False

        Return RequiredProcessAttributes = other.RequiredProcessAttributes AndAlso
            UnacceptableProcessAttributes = other.UnacceptableProcessAttributes AndAlso
            RequiredResourceAttributes = other.RequiredResourceAttributes AndAlso
            UnacceptableResourceAttributes = other.UnacceptableResourceAttributes

    End Function

    Public Overrides Function GetHashCode() As Integer

        Dim hash As Integer = 17
        hash = hash * 31 + CInt(RequiredProcessAttributes)
        hash = hash * 31 + CInt(UnacceptableProcessAttributes)
        hash = hash * 31 + CInt(RequiredResourceAttributes)
        hash = hash * 31 + CInt(UnacceptableResourceAttributes)

        Return hash

    End Function

End Class
