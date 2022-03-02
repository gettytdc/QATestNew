Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Resources
Imports BluePrism.Data

Namespace clsServerPartialClasses.Dashboards

    ''' <summary>
    ''' Custom data source used for the BPDS_I_LicenseInformation data source. Loads license 
    ''' limits from the application and usage from the database
    ''' </summary>
    Friend Class LicenseInformationDataSource
        Implements ITileDataSource

        Private ReadOnly mServer As clsServer

        ''' <summary>
        ''' Creates a new data source
        ''' </summary>
        ''' <param name="server">The server object used to load license data</param>
        Sub New(server As clsServer)
            mServer = server
        End Sub

        ''' <summary>
        ''' Loads the chart data
        ''' </summary>
        ''' <param name="con">The database connection</param>
        ''' <param name="params">Parameters to include in the query</param>
        ''' <returns>A DataTable containing the license data</returns>
        Public Function GetChartData(con As IDatabaseConnection, params As Dictionary(Of String, String)) As DataTable Implements ITileDataSource.GetChartData

            Dim table As New DataTable("LicenseInformation")
            table.Columns.Add(New DataColumn("Constraint", Type.GetType("System.String")))
            table.Columns.Add(New DataColumn("Limit", Type.GetType("System.Int32")))
            table.Columns.Add(New DataColumn("Used", Type.GetType("System.Int32")))
            If Not Licensing.License.AllowsUnlimitedSessions Then
                Dim dr = table.NewRow()
                dr("Constraint") = "Concurrent Sessions"
                dr("Limit") = Licensing.License.NumConcurrentSessions
                dr("Used") = mServer.CountConcurrentSessions()
                table.Rows.Add(dr)
            End If
            If Not Licensing.License.AllowsUnlimitedPublishedProcesses Then
                Dim dr = table.NewRow()
                dr("Constraint") = "Published Processes"
                dr("Limit") = Licensing.License.NumPublishedProcesses
                dr("Used") = mServer.GetPublishedProcessCount(con)
                table.Rows.Add(dr)
            End If
            If Not Licensing.License.AllowsUnlimitedResourcePCs Then
                Dim dr = table.NewRow()
                dr("Constraint") = "Runtime Resources"
                dr("Limit") = Licensing.License.NumResourcePCs
                Dim reqdAttrs = ResourceAttribute.None
                Dim deniedAttrs = ResourceAttribute.Retired Or
                    ResourceAttribute.Local Or
                    ResourceAttribute.Debug
                dr("Used") = mServer.GetResources(con, reqdAttrs, deniedAttrs).Rows.Count()
                table.Rows.Add(dr)
            End If
            Return table

        End Function
    End Class
End Namespace
