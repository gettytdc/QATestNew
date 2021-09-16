Imports BluePrism.AutomateProcessCore.Processes
Imports System.Runtime.Serialization

''' <summary>
''' Represents information about a Process (or Object)
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class ProcessInfo

    ''' <summary>
    ''' Indicates whether this is an Object or Process
    ''' </summary>
    <DataMember>
    Public Property Type As DiagramType

    ''' <summary>
    ''' The ID of the Object or Process
    ''' </summary>
    <DataMember>
    Public Property Id As Guid

    ''' <summary>
    ''' The Object or Process name
    ''' </summary>
    <DataMember>
    Public Property Name As String

    ''' <summary>
    ''' The Object or Process description
    ''' </summary>
    <DataMember>
    Public Property Description As String

    ''' <summary>
    ''' Indicates whether or not the current user can view the definition
    ''' of the Object or Process
    ''' </summary>
    <DataMember>
    Public Property CanViewDefinition As Boolean

End Class
