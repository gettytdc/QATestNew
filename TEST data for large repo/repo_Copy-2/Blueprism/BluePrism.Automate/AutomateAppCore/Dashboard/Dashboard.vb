Imports System.Runtime.Serialization

''' <summary>
''' Represents a dashboard view
''' </summary>
''' <remarks></remarks>
<Serializable, DataContract([Namespace]:="bp")>
Public Class Dashboard
    'Unique dashboard ID
    <DataMember>
    Public Property ID() As Guid

    'Dashboard name
    <DataMember>
    Public Property Name() As String

    'Dashboard type
    <DataMember>
    Public Property Type() As DashboardTypes

    'List of tiles
    <DataMember>
    Public Property Tiles() As List(Of DashboardTile)

    Public Sub New(dashType As DashboardTypes, dashID As Guid, dashName As String)
        Type = dashType
        ID = dashID
        Name = dashName
        Tiles = New List(Of DashboardTile)()
    End Sub
End Class
