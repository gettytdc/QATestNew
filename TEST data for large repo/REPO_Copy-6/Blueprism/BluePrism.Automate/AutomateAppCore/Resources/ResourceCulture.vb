Imports System.Runtime.Serialization

Namespace Resources
    ''' <summary>
    ''' Represents information about a resource
    ''' </summary>
    <Serializable>
    <DataContract(Name:="rc", [Namespace]:="bp")>
    Public Class ResourceCulture

        ''' <summary>
        ''' The resources Identity
        ''' </summary>
        <DataMember(Name:="ID")>
        Public Property ID As Guid


        ''' <summary>
        ''' The Current Culture of the runtime resource
        ''' </summary>
        <DataMember(Name:="CC")>
        Public Property CurrentCulture As String

    End Class

End Namespace
