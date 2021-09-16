#If UNITTESTS Then
Namespace DataContractRoundTrips.Generators


    Public Class DashboardTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim dashboard1 = New Dashboard(DashboardTypes.Personal, Guid.NewGuid(), "Dashboard1")

            Yield Create("Empty tile collection", dashboard1)

            Dim dashboard2 = New Dashboard(DashboardTypes.Global, Guid.NewGuid(), "Dashboard2")
            Dim tile = New DashboardTile()
            tile.Size = New Drawing.Size(5, 3)
            tile.LastRefreshed = DateTime.UtcNow

            tile.Tile = New Tile()
            tile.Tile.ID = Guid.NewGuid()
            tile.Tile.Name = "Tile1"
            tile.Tile.Description = "An interesting tile"
            tile.Tile.RefreshInterval = TileRefreshIntervals.EveryMinute
            tile.Tile.Type = TileTypes.Chart
            tile.Tile.XMLProperties = "<a><b></b></a>"

            dashboard2.Tiles.Add(tile)

            Yield Create("Populated tile collection", dashboard2)

        End Function
    End Class

End Namespace
#End If
