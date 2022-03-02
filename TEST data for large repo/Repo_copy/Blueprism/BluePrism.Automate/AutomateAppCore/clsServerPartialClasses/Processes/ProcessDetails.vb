

Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore

Namespace clsServerPartialClasses.Processes

    ''' <summary>
    ''' Contains information about a process
    ''' </summary>
    <DataContract(Name:="pd", [Namespace]:="bp")>
    <Serializable>
    Public Class ProcessDetails
        <DataMember(Name:="z")>
        Public Property Zipped As Boolean
        <DataMember(Name:="x")>
        Public Property Xml As Byte()
        <DataMember(Name:="l")>
        Public Property LastModified As Date
        <DataMember(Name:="a")>
        Public Property Attributes As ProcessAttributes

    End Class

End Namespace