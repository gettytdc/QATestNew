Imports System.Drawing
Imports System.Runtime.Serialization

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class DashboardTile
    'Tile object reference
    <DataMember>
    Public Property Tile() As Tile

    'Size of tile on this dashboard
    <DataMember>
    Public Property Size() As Size

    'Last refresh time
    <DataMember>
    Public Property LastRefreshed() As Date
End Class
