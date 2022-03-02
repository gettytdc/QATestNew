
Imports System.Data.SqlClient
Imports System.Drawing

Imports BluePrism.AutomateAppCore.Groups
Imports TileRefresh = BluePrism.AutomateAppCore.TileRefreshIntervals

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports System.Xml.Linq
Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.Dashboards
Imports BluePrism.Data
Imports BluePrism.DataPipeline
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Server.Domain.Models.Dashboard
Imports LocaleTools

Partial Public Class clsServer

    Private Const SendPublishedDashboardsEnvLockName As String = "SEND_PUBLISHED_DASHBOARDS"
    Public Const RefreshMIDataEnvLockName As String = "__RefreshMIData__"
    Public Const DataGatewaysEnvironmentLockExpiryKey As String = "EnvironmentLockTimeExpiry.DataGateways.InSeconds"

#Region "Chart stored procedures"

    ''' <summary>
    ''' Returns a list of stored procedures suitable for use with dashboard tiles.
    ''' If a tile type is not passed then only custom data sources are returned.
    ''' </summary>
    ''' <returns>The list of stored procedures</returns>
    <SecuredMethod(True)>
    Public Function ListStoredProcedures(Optional type As TileTypes = Nothing) _
     As Dictionary(Of String, String) Implements IServer.ListStoredProcedures
        CheckPermissions()
        Dim spList As New Dictionary(Of String, String)()

        Using con = GetConnection()
            'Build list of published data sources suitable for the tile type
            Dim cmd As New SqlCommand()
            If type <> Nothing Then
                cmd.CommandText = "select spname, helppage from BPATileDataSources where (tiletype & @type) = @type"
                cmd.Parameters.AddWithValue("@type", type)
                Using reader = con.ExecuteReturnDataReader(cmd)
                    Dim prov As New ReaderDataProvider(reader)
                    While reader.Read()
                        spList.Add(prov.GetString("spname"), prov.GetString("helppage"))
                    End While
                End Using
            End If

            'Append any custom data sources
            cmd.CommandText = "select name from sys.objects where schema_id=schema_id() and type='P' and name like 'DS_%'"
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    spList.Add(prov.GetString("name"), String.Empty)
                End While
            End Using
        End Using

        Return spList
    End Function

    ''' <summary>
    ''' Returns the list of parameters associated with the passed stored procedure.
    ''' </summary>
    ''' <param name="name">Stored procedure name</param>
    ''' <returns>The list of parameter names</returns>
    <SecuredMethod(True)>
    Public Function ListStoredProcedureParameters(name As String) As List(Of String) Implements IServer.ListStoredProcedureParameters
        CheckPermissions()
        Dim paramList As New List(Of String)()

        Dim sb As New StringBuilder("select parameter_name")
        sb.Append(" from information_schema.parameters")
        sb.Append(" where specific_name=@name and parameter_mode='IN'")
        sb.Append(" order by ordinal_position")

        Dim cmd As New SqlCommand(sb.ToString())
        cmd.Parameters.AddWithValue("@name", name)

        Using con = GetConnection()
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    paramList.Add(prov.GetString("parameter_name"))
                End While
            End Using
        End Using

        Return paramList
    End Function

    ''' <summary>
    ''' Gets a DataTable containing the data from the specified data source
    ''' </summary>
    ''' <param name="dataSourceName">The name of the data source (the name of a stored 
    ''' procedure or an internal data source)</param>
    ''' <param name="params">List of parameters</param>
    ''' <returns>A results data table</returns>
    <SecuredMethod(True)>
    Public Function GetChartData(dataSourceName As String, params As Dictionary(Of String, String)) As DataTable Implements IServer.GetChartData
        CheckPermissions()
        Using con = GetConnection()
            Dim chartData As DataTable = GetChartData(con, dataSourceName, params)

            chartData = LocalizeDataBySource(dataSourceName, chartData)

            Return chartData
        End Using
    End Function

    Private Function LocalizeDataBySource(dataSourceName As String, chartData As DataTable) As DataTable

        Select Case dataSourceName
            Case "BPDS_WorkforceAvailability"
                If chartData.Columns.Contains("Status") Then
                    For Each row As DataRow In chartData.Rows
                        row("Status") = LTools.Get(row("Status"), "tile", mLoggedInUserLocale)
                    Next
                End If
            Case "BPDS_TotalAutomations"
                If chartData.Columns.Contains("Type") Then
                    For Each row As DataRow In chartData.Rows
                        row("Type") = LTools.Get(row("Type"), "tile", mLoggedInUserLocale)
                    Next
                End If
            Case "BPDS_ResourceUtilisationByHour"
                If chartData.Columns.Count > 0 Then
                    For Each row As DataRow In chartData.Rows
                        row(0) = LTools.Get(row(0).ToString(), "tile", mLoggedInUserLocale)
                    Next
                End If
            Case "BPDS_DailyUtilisation", "BPDS_ProcessUtilisationByHour", "BPDS_Exceptions", "BPDS_DailyProductivity"
                If chartData.Columns.Contains("ValueLabel") Then
                    For Each row As DataRow In chartData.Rows
                        row("ValueLabel") = LTools.Get(row("ValueLabel").ToString(), "tile", mLoggedInUserLocale)
                    Next
                End If
            Case "BPDS_I_LicenseInformation"
                If chartData.Columns.Contains("Constraint") Then
                    For Each row As DataRow In chartData.Rows
                        row("Constraint") = LTools.Get(row("Constraint").ToString(), "tile", mLoggedInUserLocale)
                    Next
                End If
            Case "BPDS_FTEProductivityComparison"
                If chartData.Columns.Contains("TheDate") Then
                    For Each row As DataRow In chartData.Rows
                        Dim dateString = row("TheDate").ToString()
                        Dim convertedDate As Date
                        If Date.TryParseExact(dateString, "MMMM yyyy", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, convertedDate) Then
                            row("TheDate") = convertedDate.ToString("MMMM yyyy", Globalization.CultureInfo.CurrentCulture)
                        End If
                    Next
                End If

        End Select

        For Each localizedLabel As DataColumn In chartData.Columns
            localizedLabel.ColumnName = LTools.Get(localizedLabel.ColumnName, "tile", mLoggedInUserLocale)
        Next

        Return chartData

    End Function

    ''' <summary>
    ''' Gets a DataTable containing the data from the specified data source
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="dataSourceName">The name of the data source (the name of a stored 
    ''' procedure or an internal data source)</param>
    ''' <param name="params">List of parameters</param>
    ''' <returns>A results data table</returns>
    Private Function GetChartData(con As IDatabaseConnection, dataSourceName As String, params As Dictionary(Of String, String)) As DataTable

        Dim dataSource = GetChartDataSource(dataSourceName)
        Return dataSource.GetChartData(con, params)

    End Function

    Private Function GetChartData(con As IDatabaseConnection, dataSourceName As String, params As Dictionary(Of String, Object)) As DataTable

        Dim dataSource = New StoredProcedureObjectValueDataSource(dataSourceName)
        Return dataSource.GetChartData(con, params)

    End Function

    ''' <summary>
    ''' Gets the <see cref="ITileDataSource"/> for the specified data source
    ''' </summary>
    ''' <param name="dataSourceName">The name of the datasource definition used by the tile</param>
    ''' <returns>The <see cref="ITileDataSource"/> object</returns>
    ''' <remarks>Tile data sources were originally implemented to use a stored procedure
    ''' based on the data source name. Internal data sources (that use internal 
    ''' application code rather than stored procedures) have now been introduced.
    ''' Recognised internal data sources use a custom class to provide the data</remarks>
    Private Function GetChartDataSource(dataSourceName As String) As ITileDataSource

        ' Keeping source creation nice and simple while we only have a few (one) 
        ' custom data sources
        Select Case dataSourceName
            Case "BPDS_I_LicenseInformation"
                Return New LicenseInformationDataSource(Me)
            Case Else
                Return New StoredProcedureDataSource(dataSourceName)
        End Select

    End Function

    ''' <summary>
    ''' Holds a metrics package, which contains the source and
    ''' instances of the metrics.
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp")>
    Public Class MetricsPackage
        <DataMember>
        Property Source As String
        <DataMember>
        Property Instances As New Dictionary(Of String, DataTable)
    End Class

    ''' <summary>
    ''' Sends PublishedDashboards data to data gateways
    ''' </summary>
    <SecuredMethod(True)>
    Public Sub SendPublishedDashboardsToDataGateways() _
        Implements IServer.SendPublishedDashboardsToDataGateways
        CheckPermissions()

        Dim expiryTimeInSeconds As Integer = GetIntPref(DataGatewaysEnvironmentLockExpiryKey)

        If IsEnvLockHeld(SendPublishedDashboardsEnvLockName, Nothing, Nothing) Then
            If HasEnvLockExpired(SendPublishedDashboardsEnvLockName,
                                  Nothing,
                                 expiryTimeInSeconds) Then
                ReleaseEnvLock(SendPublishedDashboardsEnvLockName, Nothing, "Lock Expired.", Nothing, True)
            Else
                Throw New InvalidOperationException(My.Resources.clsServer_ALockIsInPlacePreventingPublishedDashboardDataFromBeingSent)
            End If
        End If

        Dim token = AcquireEnvLock(SendPublishedDashboardsEnvLockName, Nothing, Nothing, Environment.MachineName, expiryTimeInSeconds)

        If Not String.IsNullOrEmpty(token) Then

            Try
                Using con = GetConnection()
                    Dim dashboards As List(Of PublishedDashboardSettings) = GetPublishedDashboards(con)

                    Dim dataPipelineEvents As New List(Of DataPipelineEvent)
                    For Each dash In dashboards
                        Dim mp As New MetricsPackage()
                        mp.Source = dash.DashboardName

                        Dim tiles = GetDashboardTiles(con, dash.DashboardId)
                        Dim refreshInterval = dash.PublishToDataGatewayInterval

                        'Ignore dashboard if it has been updated within the time interval
                        If dash.LastSent.AddSeconds(refreshInterval) > DateTime.UtcNow Then Continue For

                        For Each t In tiles
                            If t.Tile.XMLProperties Is Nothing Then Continue For

                            'Unpack XML properties
                            Dim doc = XDocument.Parse(t.Tile.XMLProperties)
                            Dim chartElement = doc.Element("Chart")
                            Dim procElement = chartElement.Element("Procedure")
                            Dim proc = procElement.Attribute("name").Value
                            Dim params As New Dictionary(Of String, String)()
                            For Each paramElement In procElement.Elements("Param")
                                params.Add(paramElement.Attribute("name").Value, paramElement.Value)
                            Next

                            Dim data = GetChartData(con, proc, params)
                            mp.Instances(t.Tile.Name) = data

                            dataPipelineEvents.Add(New DataPipelineEvent(EventType.Dashboard) With {
                                          .EventData = New Dictionary(Of String, Object)() From {
                                            {"Source", mp.Source},
                                            {"Subject", t.Tile.Name},
                                            {"Values", data}}})
                        Next

                        Using cmd As New SqlCommand("update BPADashboard set lastsent = @lastsent where id = @id")
                            With cmd.Parameters
                                .AddWithValue("@lastsent", DateTime.UtcNow)
                                .AddWithValue("@id", dash.DashboardId)
                            End With
                            con.Execute(cmd)
                        End Using

                    Next

                    ' Push data events to the pipeline
                    If dataPipelineEvents.Any Then
                        con.BeginTransaction()
                        mDataPipelinePublisher.PublishToDataPipeline(con, dataPipelineEvents)
                        con.CommitTransaction()
                    End If

                End Using

            Finally
                ReleaseEnvLock(SendPublishedDashboardsEnvLockName, token, Environment.MachineName, Nothing, True)
            End Try

        End If
    End Sub

    Private Shared Function GetPublishedDashboards(con As IDatabaseConnection) As List(Of PublishedDashboardSettings)
        Dim dashboards As New List(Of PublishedDashboardSettings)

        Using cmd As New SqlCommand(" select id, name, sendeveryseconds, lastsent from BPADashboard " &
                            " where dashtype=@dashboardtype")
            With cmd.Parameters
                .AddWithValue("@dashboardtype", DashboardTypes.Published)
            End With

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    dashboards.Add(New PublishedDashboardSettings(prov.GetGuid("id"), prov.GetValue("name", "?"), prov.GetInt("sendeveryseconds", 3600), prov.GetValue("lastsent", DateTime.MinValue)))
                End While
            End Using
        End Using

        Return dashboards
    End Function

    ''' <summary>
    ''' Returns the definition of the passed stored procedure.
    ''' </summary>
    ''' <param name="name">The stored procedure name</param>
    ''' <returns>The SQL definition</returns>
    <SecuredMethod(True)>
    Public Function GetDataSourceDefinition(name As String) As String Implements IServer.GetDataSourceDefinition
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("select object_definition(object_id(@name)) as source")
            cmd.Parameters.AddWithValue("@name", name)
            Return IfNull(con.ExecuteReturnScalar(cmd), String.Empty)
        End Using
    End Function

    ''' <summary>
    ''' Returns true if data sources (stored procedures) can be created using the
    ''' current user's connection, otherwise false.
    ''' </summary>
    <SecuredMethod(True)>
    Public Function CanCreateDataSource() As Boolean Implements IServer.CanCreateDataSource
        CheckPermissions()
        Using con = GetConnection()
            'Need create procedure permission (note this is at database level)
            Dim cmd As New SqlCommand(
                "select 1 from sys.fn_my_permissions(null, 'DATABASE')" &
                " where permission_name='CREATE PROCEDURE'")
            If IfNull(con.ExecuteReturnScalar(cmd), 0) = 0 Then Return False
            'Also need Alter on the default schema
            cmd.CommandText =
                "select 1 from sys.fn_my_permissions(SCHEMA_NAME(), 'SCHEMA')" &
                " where permission_name='ALTER'"
            Return (IfNull(con.ExecuteReturnScalar(cmd), 0) = 1)
        End Using
    End Function

    ''' <summary>
    ''' Returns true if the user can grant execute permission to the custom
    ''' datasource BP role (bpa_ExecuteSP_DataSource_custom)
    ''' </summary>
    <SecuredMethod(True)>
    Public Function CanGrantExecuteOnDataSource() As Boolean Implements IServer.CanGrantExecuteOnDataSource
        CheckPermissions()
        Using con = GetConnection()
            'Need Control on the default schema
            Dim cmd As New SqlCommand(
                "select 1 from sys.fn_my_permissions(SCHEMA_NAME(), 'SCHEMA')" &
                " where permission_name='CONTROL'")
            Return (IfNull(con.ExecuteReturnScalar(cmd), 0) = 1)
        End Using
    End Function

    ''' <summary>
    ''' Returns true if the specified data source (stored procedure) can be altered
    ''' using the current user's connection, otherwise false.
    ''' </summary>
    ''' <param name="name">The data source name</param>
    <SecuredMethod(True)>
    Public Function CanAlterDataSource(name As String) As Boolean Implements IServer.CanAlterDataSource
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
                "select 1 from sys.fn_my_permissions(@name, 'OBJECT')" &
                " where permission_name='ALTER'")
            cmd.Parameters.AddWithValue("@name", name)
            Return (IfNull(con.ExecuteReturnScalar(cmd), 0) = 1)
        End Using
    End Function

#End Region

#Region "Tiles"

    ''' <summary>
    ''' Returns the definition of the passed tile ID
    ''' </summary>
    ''' <param name="id">Tile ID</param>
    ''' <returns>A tile object</returns>
    <SecuredMethod(True)>
    Public Function GetTileDefinition(id As Guid) As Tile Implements IServer.GetTileDefinition
        CheckPermissions()
        Using con = GetConnection()
            Return GetTileDefinition(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Returns the definitions of the passed tile IDs
    ''' </summary>
    ''' <param name="idList">List of tile IDs</param>
    ''' <returns>A list of tile objects</returns>
    <SecuredMethod(True)>
    Public Function GetTileDefinitions(idList As List(Of Guid)) As List(Of Tile) Implements IServer.GetTileDefinitions
        CheckPermissions()
        Dim tileList As New List(Of Tile)
        Using con = GetConnection()
            For Each id As Guid In idList
                tileList.Add(GetTileDefinition(con, id))
            Next
        End Using
        Return tileList
    End Function

    ''' <summary>
    ''' Returns the definition of the passed tile ID
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="id">Tile ID</param>
    ''' <returns>A tile object</returns>
    Private Function GetTileDefinition(con As IDatabaseConnection, id As Guid) As Tile
        Dim tile As Tile

        Dim sb As New StringBuilder("select name, tiletype, description, autorefresh, xmlproperties")
        sb.Append(" from BPATile where id=@id")

        Dim cmd As New SqlCommand(sb.ToString())
        cmd.Parameters.AddWithValue("@id", id)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then
                Throw New MissingItemException(My.Resources.clsServer_TileWithID0NotFound, id)
            End If

            tile = LoadTile(id, New ReaderDataProvider(reader))
        End Using

        Return tile
    End Function

    ''' <summary>
    ''' Determines if the passed tile is referenced by 1 or more dashboards
    ''' </summary>
    ''' <param name="id">Tile ID</param>
    ''' <returns>True if tile is in use</returns>
    <SecuredMethod(True)>
    Public Function IsTileInUse(id As Guid) As Boolean Implements IServer.IsTileInUse
        CheckPermissions()
        Dim cmd As New SqlCommand("select top 1 dashid from BPADashboardTile where tileid=@id")
        Using con = GetConnection()
            cmd.Parameters.AddWithValue("@id", id)
            Dim obj As Object = con.ExecuteReturnScalar(cmd)
            If obj Is Nothing Then Return False
            Return True
        End Using
    End Function

    ''' <summary>
    ''' Returns the ID of the tile with the passed name
    ''' </summary>
    ''' <param name="name">Tile name</param>
    ''' <returns>Tile ID (or guid.empty if no tile found)</returns>
    <SecuredMethod(True)>
    Public Function GetTileIDByName(name As String) As Guid Implements IServer.GetTileIDByName
        CheckPermissions()
        Using con = GetConnection()
            Return GetTileIDByName(con, name)
        End Using
    End Function

    ''' <summary>
    ''' Returns the ID of the tile with the passed name
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="name">Tile name</param>
    ''' <returns>Tile ID (or guid.empty if no tile found)</returns>
    Private Function GetTileIDByName(con As IDatabaseConnection, name As String) As Guid
        Dim cmd As New SqlCommand("select id from BPATile where name=@name")
        cmd.Parameters.AddWithValue("@name", name)
        Return IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)
    End Function

    ''' <summary>
    ''' Returns the name of the tile with the passed id
    ''' </summary>
    ''' <param name="id">Tile id</param>
    ''' <returns>Tile name (or string.empty if no tile found)</returns>
    <SecuredMethod(True)>
    Public Function GetTileNameByID(id As Guid) As String Implements IServer.GetTileNameByID
        CheckPermissions()
        Using con = GetConnection()
            Return GetTileNameByID(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Returns the name of the tile with the passed id
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="id">Tile id</param>
    ''' <returns>Tile name (or string.empty if no tile found)</returns>
    Private Function GetTileNameByID(con As IDatabaseConnection, id As Guid) As String
        Dim cmd As New SqlCommand("select name from BPATile where id=@id")
        cmd.Parameters.AddWithValue("@id", id)
        Return IfNull(LTools.Get(con.ExecuteReturnScalar(cmd), "tile", mLoggedInUserLocale), String.Empty)
    End Function

    ''' <summary>
    ''' Instantiates a tile object using the passed data provider.
    ''' </summary>
    ''' <param name="id">Tile ID</param>
    ''' <param name="prov">Data provider</param>
    ''' <returns>A tile object</returns>
    Private Function LoadTile(id As Guid, prov As ReaderDataProvider) As Tile
        Dim tile As New Tile()
        tile.ID = id
        tile.Type = prov.GetValue("tiletype", TileTypes.Chart)
        tile.Name = LTools.Get(prov.GetString("name"), "tile", mLoggedInUserLocale)
        tile.Description = LTools.Get(prov.GetString("description"), "tile", mLoggedInUserLocale)
        tile.RefreshInterval = prov.GetValue("autorefresh", TileRefresh.Never)
        tile.XMLProperties = prov.GetString("xmlproperties")
        Return tile
    End Function

    ''' <summary>
    ''' Creates a new tile record in the database, optionally within a group.
    ''' </summary>
    ''' <param name="groupID">Group ID (or Guid.Empty if not in a group)</param>
    ''' <param name="tile">A tile object</param>
    ''' <returns>The newly created tile object with its new ID set within</returns>
    ''' <exception cref="NameAlreadyExistsException">If a tile with the same name as
    ''' the given tile already exists on the database.</exception>
    <SecuredMethod(True)>
    Public Function CreateTile(groupID As Guid, tile As Tile,
     formattedProperties As String) As Tile Implements IServer.CreateTile
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            If TileExists(con, tile.Name) Then
                Throw New NameAlreadyExistsException(
                         My.Resources.clsServer_ATileWithName0AlreadyExists, tile.Name)
            End If


            tile.ID = Guid.NewGuid()
            CreateTile(con, groupID, tile)
            AuditRecordDashboardTileEvent(con, DashboardEventCode.CreateTile, tile, formattedProperties)
            con.CommitTransaction()
            Return tile
        End Using
    End Function

    ''' <summary>
    ''' Checks if a tile with a specified name exists in the system
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="name">The name to check for</param>
    ''' <returns>True if a tile exists with the specified name; false otherwise.
    ''' </returns>
    Private Function TileExists(con As IDatabaseConnection, name As String) As Boolean
        Using cmd As New SqlCommand("select 1 from BPATile where name = @name")
            cmd.Parameters.AddWithValue("@name", name)
            Return (IfNull(con.ExecuteReturnScalar(cmd), 0) = 1)
        End Using
    End Function

    ''' <summary>
    ''' Copies the given tile.
    ''' </summary>
    ''' <param name="tileId">The ID of the tile to copy</param>
    ''' <returns>The newly created tile</returns>
    <SecuredMethod(True)>
    Public Function CopyTile(tileId As Guid) As Tile Implements IServer.CopyTile
        CheckPermissions()
        Using con = GetConnection()
            Return CopyTile(con, tileId)
        End Using
    End Function

    ''' <summary>
    ''' Copies the given tile without assigning it a group
    ''' </summary>
    ''' <param name="con">The connection to the database</param>
    ''' <param name="tileId">The ID of the tile to copy</param>
    ''' <returns>The tile object created as a result of this operation</returns>
    Private Function CopyTile(con As IDatabaseConnection, tileId As Guid) As Tile
        Dim tile As Tile = GetTileDefinition(con, tileId)
        tile.Name = BPUtil.FindUnique(
            "{1} ({0})", Function(nm) TileExists(con, nm), Left(tile.Name, 45))
        tile.ID = Guid.NewGuid()
        CreateTile(con, Nothing, tile)
        Return tile
    End Function

    ''' <summary>
    ''' Creates a new tile record in the database, optionally within a group.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="groupID">Group ID (or Guid.Empty if not in a group)</param>
    ''' <param name="tile">A tile object</param>
    ''' <remarks>Note that the tile's ID is overwritten when this method is called.
    ''' </remarks>
    Private Sub CreateTile(con As IDatabaseConnection, groupID As Guid, tile As Tile)
        If tile.ID = Guid.Empty Then tile.ID = Guid.NewGuid()

        Dim sb As New StringBuilder("insert into BPATile")
        sb.Append(" (id, name, tiletype, description, autorefresh, xmlproperties)")
        sb.Append(" values(@id, @name, @tiletype, @description, @autorefresh, @xmlproperties)")

        Dim cmd As New SqlCommand(sb.ToString())
        With cmd.Parameters
            .AddWithValue("@id", tile.ID)
            .AddWithValue("@name", tile.Name)
            .AddWithValue("@tiletype", tile.Type)
            .AddWithValue("@description", tile.Description)
            .AddWithValue("@autorefresh", tile.RefreshInterval)
            .AddWithValue("@xmlproperties", tile.XMLProperties)
        End With
        con.Execute(cmd)

        'If required, add tile to the passed group
        If groupID <> Guid.Empty Then
            AddToGroup(GroupTreeType.Tiles, groupID,
             New TileGroupMember() With {.Id = tile.ID, .Name = tile.Name})
        End If
    End Sub

    ''' <summary>
    ''' Updates the attributes of the passed tile.
    ''' </summary>
    ''' <param name="tile">A tile object</param>
    <SecuredMethod(True)>
    Public Sub UpdateTile(tile As Tile, oldName As String, formattedProperties As String) Implements IServer.UpdateTile
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()

            If tile.Name <> oldName AndAlso TileExists(con, tile.Name) Then _
                Throw New NameAlreadyExistsException(My.Resources.clsServer_ATileWithName0AlreadyExists, tile.Name)

            UpdateTile(con, tile)
            AuditRecordDashboardTileEvent(con, DashboardEventCode.ModifyTile, tile, formattedProperties)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Updates the attributes of the passed tile.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="tile">A tile object</param>
    Private Sub UpdateTile(con As IDatabaseConnection, tile As Tile)
        Dim sb As New StringBuilder("update BPATile")
        sb.Append(" set name=@name, tiletype=@tiletype, description=@description,")
        sb.Append(" autorefresh=@autorefresh, xmlproperties=@xmlproperties")
        sb.Append(" where id=@id")

        Dim cmd As New SqlCommand(sb.ToString())
        With cmd.Parameters
            .AddWithValue("@id", tile.ID)
            .AddWithValue("@name", tile.Name)
            .AddWithValue("@tiletype", tile.Type)
            .AddWithValue("@description", tile.Description)
            .AddWithValue("@autorefresh", tile.RefreshInterval)
            .AddWithValue("@xmlproperties", tile.XMLProperties)
        End With
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Deletes the passed tile.
    ''' </summary>
    ''' <param name="tileId">The ID of the tile to be deleted</param>
    <SecuredMethod(True)>
    Public Sub DeleteTile(tileId As Guid) Implements IServer.DeleteTile
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            DeleteTile(con, tileId)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Deletes the tile with the passed ID from the database.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="id">Tile ID</param>
    Private Sub DeleteTile(con As IDatabaseConnection, id As Guid)
        Dim tile As Tile = GetTileDefinition(con, id)
        If tile Is Nothing Then
            Throw New NoSuchElementException(My.Resources.clsServer_NoTileFoundWithID0, id)
        End If

        Dim cmd As New SqlCommand("delete from BPATile where id=@tileid")
        cmd.Parameters.AddWithValue("@tileid", id)
        con.Execute(cmd)

        AuditRecordDashboardTileEvent(con, DashboardEventCode.DeleteTile, tile, Nothing)

    End Sub

#End Region

#Region "Dashboards"

    ''' <summary>
    ''' Returns the list of dashboard views available to the currently logged in
    ''' user.
    ''' </summary>
    ''' <returns>List of dashboard views</returns>
    <SecuredMethod(True)>
    Public Function GetDashboardList() As List(Of Dashboard) Implements IServer.GetDashboardList
        CheckPermissions()
        Using con = GetConnection()
            Return GetDashboardList(con)
        End Using
    End Function

    ''' <summary>
    ''' Returns the list of dashboard views available to the currently logged in
    ''' user.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <returns>List of dashboard views</returns>
    Private Function GetDashboardList(con As IDatabaseConnection) As List(Of Dashboard)

        Using cmd As New SqlCommand(
            " select dashtype,id,name from BPADashboard " &
            " where dashtype=@global " &
            " or (dashtype=@personal and userid=@userid) " &
            " or dashtype=@published")

            With cmd.Parameters
                .AddWithValue("@userid", GetLoggedInUserId())
                .AddWithValue("@global", DashboardTypes.Global)
                .AddWithValue("@personal", DashboardTypes.Personal)
                .AddWithValue("@published", DashboardTypes.Published)
            End With

            Dim viewList As New List(Of Dashboard)
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    Dim view As New Dashboard(
                        prov.GetValue("dashtype", DashboardTypes.Personal),
                        prov.GetValue("id", Guid.Empty),
                        LTools.Get(prov.GetString("name"), "tile", mLoggedInUserLocale, "dash"))

                    viewList.Add(view)
                End While
            End Using

            Return viewList
        End Using

    End Function

    ''' <summary>
    ''' Returns a Dashboard by ID
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    <SecuredMethod(True)>
    Public Function GetDashboardById(id As Guid) As Dashboard Implements IServer.GetDashboardById
        CheckPermissions()
        Using con = GetConnection()
            Return GetDashboardById(con, id)
        End Using

    End Function

    ''' <summary>
    ''' Returns a Dashboard by ID
    ''' </summary>
    ''' <param name="con"></param>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Private Function GetDashboardById(con As IDatabaseConnection, id As Guid) As Dashboard
        Using cmd As New SqlCommand(
            " select dashtype,id,name from BPADashboard where id=@id")

            With cmd.Parameters
                .AddWithValue("@id", id)
            End With

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                If reader.Read() Then
                    Dim view As New Dashboard(
                        prov.GetValue("dashtype", DashboardTypes.Personal),
                        prov.GetValue("id", Guid.Empty),
                        LTools.Get(prov.GetString("name"), "tile", mLoggedInUserLocale, "dash"))
                    Return view
                End If
            End Using

            Return Nothing
        End Using
    End Function

    ''' <summary>
    ''' Returns the tiles associated with the passed dashboard ID.
    ''' </summary>
    ''' <param name="id">Dashboard ID</param>
    ''' <returns>A list of dashboard tiles</returns>
    <SecuredMethod(True)>
    Public Function GetDashboardTiles(id As Guid) As List(Of DashboardTile) Implements IServer.GetDashboardTiles
        CheckPermissions()
        Using con = GetConnection()
            Return GetDashboardTiles(con, id)
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetWorkQueueCompositions(workQueueIds As IEnumerable(Of Guid)) As IEnumerable(Of WorkQueueComposition) Implements IServer.GetWorkQueueCompositions
        CheckPermissions()
        Using con = GetConnection()

            Dim storedProcedureParameter = New DataTable()
            storedProcedureParameter.Columns.Add("id", GetType(Guid))

            For Each item As Guid In workQueueIds
                storedProcedureParameter.Rows.Add(item)
            Next

            Dim dict = New Dictionary(Of String, Object) From {
                {"QueueIds", storedProcedureParameter}
            }

            Dim workQueueCompositionDataSoruce = "BPDS_Get_Work_Queue_Compositions"

            Dim data = GetChartData(con, workQueueCompositionDataSoruce, dict)

            Dim workQueueCompositions = New List(Of WorkQueueComposition)

            For Each row As DataRow In data.Rows
                Dim workQueueCompositionItem = New WorkQueueComposition With {
                    .Id = CType(row("id"), Guid),
                    .Name = CType(row("name"), String),
                    .Completed = CType(row("completed"), Integer),
                    .Deferred = CType(row("deferred"), Integer),
                    .Locked = CType(row("locked"), Integer),
                    .Pending = CType(row("pending"), Integer),
                    .Exceptioned = CType(row("exceptioned"), Integer)
                }

                workQueueCompositions.Add(workQueueCompositionItem)
            Next
            Return workQueueCompositions
        End Using
    End Function
    
    <SecuredMethod(True)>
    Public Function GetResourceUtilization(resourceUtilizationParameters As ResourceUtilizationParameters) As IEnumerable(Of ResourceUtilization)Implements IServer.GetResourceUtilization
        CheckPermissions()
        Using con = GetConnection()

            Dim resourceIdsDataTableParameter = New DataTable()
            resourceIdsDataTableParameter.Columns.Add("id", GetType(Guid))
            For Each resourceId As Guid In resourceUtilizationParameters.ResourceIds
                resourceIdsDataTableParameter.Rows.Add(resourceId)
            Next


            Dim storedProcedureParameters = New Dictionary(Of String, Object) From
            {
                    {"startdate", resourceUtilizationParameters.StartDate},
                    {"resourceId", resourceIdsDataTableParameter},
                    {"pageNumber", resourceUtilizationParameters.PageNumber},
                    {"pageSize", resourceUtilizationParameters.PageSize}
            }

            Dim storedProcedureName = "BPDS_Get_Process_History_Per_Worker_Parameterised_Query"
            Dim data = GetChartData(con, storedProcedureName, storedProcedureParameters)

            Dim resourceUtilizationDatabaseResponse = New List(Of ResourceUtilizationDatabaseResponse)

            For Each row As DataRow In data.Rows
                Dim item = New ResourceUtilizationDatabaseResponse With {
                        .ResourceId = CType(row("resourceid"), Guid),
                        .DigitalWorkerName = CType(row("digitalworkername"), String),
                        .UtilizationDate = CType(row("utilizationdate"), DateTime),
                        .Usage = CType(row("usage"), Integer)
                        }

                resourceUtilizationDatabaseResponse.Add(item)
            Next

            Dim result = resourceUtilizationDatabaseResponse.GroupBy(Function(resource) resource.ResourceId,
                                                                     Function(id, resources) New ResourceUtilization With {
                                                           .Usages = resources.Select(Function(x) x.Usage),
                                                           .UtilizationDate = resources.First().UtilizationDate,
                                                           .DigitalWorkerName = resources.First().DigitalWorkerName,
                                                           .ResourceId = id})


            Return result
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetResourcesSummaryUtilization(resourcesSummaryUtilizationParameters As ResourcesSummaryUtilizationParameters) As IEnumerable(Of ResourcesSummaryUtilization)Implements IServer.GetResourcesSummaryUtilization
        CheckPermissions()
        Using con = GetConnection()

            Dim resourceIdsDataTableParameter = New DataTable()
            resourceIdsDataTableParameter.Columns.Add("id", GetType(Guid))
            For Each resourceId As Guid In resourcesSummaryUtilizationParameters.ResourceIds
                resourceIdsDataTableParameter.Rows.Add(resourceId)
            Next


            Dim storedProcedureParameters = New Dictionary(Of String, Object) From
            {
                    {"startdate", resourcesSummaryUtilizationParameters.StartDate},
                    {"enddate", resourcesSummaryUtilizationParameters.EndDate},
                    {"resourceId", resourceIdsDataTableParameter}
            }

            Dim storedProcedureName = "BPDS_Get_Process_History_By_Date_Range_Query"
            Dim data = GetChartData(con, storedProcedureName, storedProcedureParameters)

            Dim result = New List(Of ResourcesSummaryUtilization)

            For Each row As DataRow In data.Rows
                Dim utilizationHeatMap = New ResourcesSummaryUtilization With {
                        .Dates = CType(row("dates"), DateTime),
                        .Usage = CType(row("usage"), Integer)
                        }

                result.Add(utilizationHeatMap)
            Next
            Return result
        End Using
    End Function

    ''' <summary>
    ''' Returns the tiles associated with the passed dashboard ID.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="id">Dashboard ID</param>
    ''' <returns>A list of dashboard tiles</returns>
    Private Function GetDashboardTiles(con As IDatabaseConnection, id As Guid) As List(Of DashboardTile)
        Dim tileList As New List(Of DashboardTile)

        'Check dashboard exists
        Dim cmd As New SqlCommand("select id from BPADashboard where id=@dashid")
        cmd.Parameters.AddWithValue("@dashid", id)
        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Throw New NoSuchDashboardException(id)
        End Using

        'Retrieve list of tiles associated with the dashboard view
        Dim sb As New StringBuilder("select a.tileid, a.width, a.height, b.name, b.tiletype,")
        sb.Append(" b.description, b.autorefresh, b.xmlproperties")
        sb.Append(" from BPADashboardTile a inner join BPATile b on b.id=a.tileid")
        sb.Append(" where a.dashid=@dashid order by a.displayorder")
        cmd.CommandText = sb.ToString()

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                Dim dashTile As New DashboardTile()
                dashTile.Tile = LoadTile(prov.GetValue("tileid", Guid.Empty), prov)
                dashTile.Size = New Size(prov.GetValue("width", 1), prov.GetValue("height", 1))
                tileList.Add(dashTile)
            End While
        End Using

        Return tileList
    End Function

    ''' <summary>
    ''' Creates a new dashboard with the passed details.
    ''' </summary>
    ''' <param name="dash">A dashboard object</param>
    <SecuredMethod(True)>
    Public Sub CreateDashboard(dash As Dashboard) Implements IServer.CreateDashboard
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            SaveDashboard(con, dash, True)
            AuditRecordDashboardEvent(con, DashboardEventCode.CreateDashboard, dash)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Updates the passed dashboard.
    ''' </summary>
    ''' <param name="dash">A dashboard object</param>
    <SecuredMethod(True)>
    Public Sub UpdateDashboard(dash As Dashboard) Implements IServer.UpdateDashboard
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            SaveDashboard(con, dash, False)
            AuditRecordDashboardEvent(con, DashboardEventCode.ModifyDashboard, dash)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Updates the passed dashboard.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="dash">A dashboard view object</param>
    Private Sub SaveDashboard(con As IDatabaseConnection, dash As Dashboard, create As Boolean)
        Dim sb As New StringBuilder()
        If create Then
            sb.Append("insert into BPADashboard (id, name, dashtype, userid)")
            sb.Append(" values (@dashid, @name, @dashtype, @userid)")
        Else
            sb.Append("update BPADashboard")
            sb.Append(" set name=@name, dashtype=@dashtype, userid=@userid")
            sb.Append(" where id=@dashid;")
            sb.Append(" delete from BPADashboardTile where dashid=@dashid;")
        End If

        Dim cmd As New SqlCommand(sb.ToString())
        With cmd.Parameters
            .AddWithValue("@dashid", dash.ID)
            .AddWithValue("@name", dash.Name)
            .AddWithValue("@dashtype", dash.Type)
            If dash.Type = DashboardTypes.Personal Then
                .AddWithValue("@userid", GetLoggedInUserId())
            Else
                .AddWithValue("@userid", DBNull.Value)
            End If
        End With
        con.Execute(cmd)

        sb.Clear()
        sb.Append("insert into BPADashboardTile (dashid, tileid, displayorder, width, height)")
        sb.Append(" values (@dashid, @tileid, @order, @width, @height)")

        cmd.CommandText = sb.ToString()
        Dim posnParam As SqlParameter = cmd.Parameters.Add("@order", SqlDbType.Int)
        Dim tileParam As SqlParameter = cmd.Parameters.Add("@tileid", SqlDbType.UniqueIdentifier)
        Dim widthParam As SqlParameter = cmd.Parameters.Add("@width", SqlDbType.Int)
        Dim heightParam As SqlParameter = cmd.Parameters.Add("@height", SqlDbType.Int)
        For i As Integer = 0 To dash.Tiles.Count - 1
            tileParam.Value = dash.Tiles(i).Tile.ID
            posnParam.Value = i + 1
            widthParam.Value = dash.Tiles(i).Size.Width
            heightParam.Value = dash.Tiles(i).Size.Height
            con.Execute(cmd)
        Next

    End Sub

    ''' <summary>
    ''' Deletes the passed dashboard.
    ''' </summary>
    ''' <param name="dash">Dashboard</param>
    <SecuredMethod(True)>
    Public Sub DeleteDashboard(dash As Dashboard) Implements IServer.DeleteDashboard
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            DeleteDashboard(con, dash.ID)
            AuditRecordDashboardEvent(con, DashboardEventCode.DeleteDashboard, dash)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Deletes the dashboard with the passed ID.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="id">Dashboard ID</param>
    Private Sub DeleteDashboard(con As IDatabaseConnection, id As Guid)
        Dim cmd As New SqlCommand("delete from BPADashboard where id=@dashid")
        cmd.Parameters.AddWithValue("@dashid", id)
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Sets the passed dashboard as the user's welcome page
    ''' </summary>
    ''' <param name="dash">The dashboard</param>
    <SecuredMethod(True)>
    Public Sub SetHomePage(dash As Dashboard) Implements IServer.SetHomePage
        CheckPermissions()
        'ToDo:  Ideally this would all be done in the same transaction, but currently
        '       there is no means of passing a connection when setting preferences
        SetUserPref(PreferenceNames.UI.DefaultDashboard, dash.ID)
        Using con = GetConnection()
            AuditRecordDashboardEvent(con, DashboardEventCode.SetHomePage, dash)
        End Using
    End Sub

    ''' <summary>
    ''' Returns the ID of the global dashboard with the passed name
    ''' </summary>
    ''' <param name="name">Global dashboard name</param>
    ''' <returns>Dashboard ID (or guid.empty if not found)</returns>
    <SecuredMethod(True)>
    Public Function GetDashboardIDByName(name As String) As Guid Implements IServer.GetDashboardIDByName
        CheckPermissions()
        Using con = GetConnection()
            Return GetDashboardIDByName(con, name)
        End Using
    End Function

    ''' <summary>
    ''' Returns the ID of the global dashboard with the passed name
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="name">Global dashboard name</param>
    ''' <returns>Dashboard ID (or guid.empty if not found)</returns>
    Private Function GetDashboardIDByName(con As IDatabaseConnection, name As String) As Guid
        Dim cmd As New SqlCommand("select id from BPADashboard where dashtype=0 and name=@name")
        cmd.Parameters.AddWithValue("@name", name)
        Return IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)
    End Function

    ''' <summary>
    ''' Returns the name of the dashboard with the passed id
    ''' </summary>
    ''' <param name="id">Dashboard id</param>
    ''' <returns>Dashboard name (or string.empty if not found)</returns>
    <SecuredMethod(True)>
    Public Function GetDashboardNameByID(id As Guid) As String Implements IServer.GetDashboardNameByID
        CheckPermissions()
        Using con = GetConnection()
            Return GetDashboardNameByID(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Returns the name of the dashboard with the passed id
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="id">Dashboard id</param>
    ''' <returns>Dashboard name (or string.empty if not found)</returns>
    Private Function GetDashboardNameByID(con As IDatabaseConnection, id As Guid) As String
        Dim cmd As New SqlCommand("select name from BPADashboard where id=@id")
        cmd.Parameters.AddWithValue("@id", id)
        Return IfNull(con.ExecuteReturnScalar(cmd), String.Empty)
    End Function

#End Region

End Class
