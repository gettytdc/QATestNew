
Imports System.Runtime.Serialization
Imports BluePrism.Images
Imports BluePrism.BPCoreLib.Data

Namespace Groups

    ''' <summary>
    ''' Represents tile nodes within the tree structure
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp")>
    <KnownType(GetType(TileTypes))>
    <KnownType(GetType(List(Of ResourceGroupMember)))>
    Public Class TileGroupMember : Inherits GroupMember

#Region " Class-scope Declarations "

        ''' <summary>
        ''' Inner class to hold the data names for the properties in this class
        ''' </summary>
        Private Class DataNames
            Public Const TileType As String = "TileType"
            Public Const Description As String = "Description"
        End Class

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new tile group member using data from a provider.
        ''' </summary>
        ''' <param name="prov">The provider of the data to initialise this group
        ''' member with - this expects: <list>
        ''' <item>id: Guid: The ID of the tile</item>
        ''' <item>name: String: The name of the tile</item>
        ''' <item>description: String: The text describing the tile</item>
        ''' <item>tiletype: <see cref="TileTypes"/>: The type of tile
        ''' that this member represents.</item>
        ''' </list></param>
        Public Sub New(prov As IDataProvider)
            MyBase.New(prov)
            Description = prov.GetString("description")
            TileType = prov.GetValue("tiletype", TileTypes.Chart)
        End Sub

        ''' <summary>
        ''' Creates a new tile group member using the data from the given tile.
        ''' </summary>
        ''' <param name="tile">The tile from which to create this group member.
        ''' </param>
        Public Sub New(tile As Tile)
            Me.New(NullDataProvider.Instance)
            If tile IsNot Nothing Then
                Id = tile.ID
                Name = tile.Name
                Description = tile.Description
                TileType = tile.Type
            End If
        End Sub

        ''' <summary>
        ''' Creates a new, empty process or object node
        ''' </summary>
        Public Sub New()
            Me.New(NullDataProvider.Instance)
        End Sub

#End Region

#Region " Associated Data Properties "

        ''' <summary>
        ''' The type of tile represented by this group member
        ''' </summary>
        <DataMember>
        Public Property TileType() As TileTypes
            Get
                Return GetData(DataNames.TileType, CType(0, TileTypes))
            End Get
            Set(value As TileTypes)
                SetData(DataNames.TileType, value)
            End Set
        End Property

        ''' <summary>
        ''' The description of the tile
        ''' </summary>
        <DataMember>
        Public Property Description As String
            Get
                Return GetData(DataNames.Description, "")
            End Get
            Set(value As String)
                SetData(DataNames.Description, value)
            End Set
        End Property

#End Region

#Region " Other Properties "

        ''' <summary>
        ''' The linking table between nodes of this type and groups. In this case,
        ''' the table is <c>BPAGroupTile</c>.
        ''' </summary>
        Public Overrides ReadOnly Property LinkTableName As String
            Get
                Return "BPAGroupTile"
            End Get
        End Property

        ''' <summary>
        ''' The column which holds the id of the entity that this node represents, in
        ''' this case "tileid".
        ''' </summary>
        Friend Overrides ReadOnly Property MemberIdColumnName As String
            Get
                Return "tileid"
            End Get
        End Property

        ''' <summary>
        ''' Gets the image key for a tile
        ''' </summary>
        Public Overrides ReadOnly Property ImageKey As String
            Get
                ' Right now, this is a constant - in future, when there are different
                ' types of tile, this may lean a bit more on the tile type
                Return ImageLists.Keys.Component.Tile
            End Get
        End Property

        ''' <summary>
        ''' The type of group member that this object represents.
        ''' </summary>
        Public Overrides ReadOnly Property MemberType As GroupMemberType
            Get
                Return GroupMemberType.Tile
            End Get
        End Property

        ''' <summary>
        ''' Extracts a localised name, only if the type is correct.  Otherwise the name is simply returned
        ''' </summary>
        ''' <param name="localiser">localisation function</param>
        ''' <returns>Display string</returns>
        Public Overrides Function GetLocalisedName(localiser As Func(Of IGroupMember, String)) As String
            Return localiser(Me)
        End Function

#End Region

    End Class

End Namespace
