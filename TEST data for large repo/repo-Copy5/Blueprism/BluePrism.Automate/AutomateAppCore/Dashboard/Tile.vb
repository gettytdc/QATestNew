Imports System.Runtime.Serialization
<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class Tile
    'Unique tile ID
    <DataMember>
    Public Property ID() As Guid

    'Tile type
    <DataMember>
    Public Property Type() As TileTypes

    'Tile name
    <DataMember>
    Public Property Name() As String

    'Tile description
    <DataMember>
    Public Property Description() As String

    'Automatic refresh interval
    <DataMember>
    Public Property RefreshInterval() As TileRefreshIntervals

    'Tile type specific properties
    <DataMember>
    Public Property XMLProperties() As String
    
    ''' <summary>
    ''' Default constructor
    ''' </summary>
    Public sub New()
    End sub

    ''' <summary>
    ''' Copy constructor for the tile
    ''' </summary>
    ''' <param name="tile"></param>
    Public Sub New(tile as Tile)
        ID = tile.ID
        Type = tile.Type
        Name = tile.Name
        Description = tile.Description
        RefreshInterval = tile.RefreshInterval
        XMLProperties = tile.XMLProperties
    End Sub

End Class
