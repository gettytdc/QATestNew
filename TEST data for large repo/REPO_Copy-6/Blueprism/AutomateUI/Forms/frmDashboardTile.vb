Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.Server.Domain.Models

Friend Class frmDashboardTile : Implements IEnvironmentColourManager

#Region "Member variables"

    'Read only indicator
    Private mReadOnly As Boolean

    'The tile being created/edited
    Private mTile As Tile

    'The original tile name
    Private mOrigTileName As String

    'The group to create the tile in
    Private mGroupID As Guid

    'The tile definition control
    Private mControl As Control

    'Utility class to support dropdown lists
    Private Class ValRep
        Public value As Object
        Public Representation As String

        Sub New(val As Object, rep As String)
            Me.value = val
            Me.Representation = rep
        End Sub

        Public Overrides Function ToString() As String
            Return Me.Representation
        End Function
    End Class

    'Localized Refresh strings
    Private TileRefreshIntervalsLoc As Dictionary(Of String, String)

#End Region

#Region "Constructor"

    ''' <summary>
    ''' Creates a new dashboard tile form for a new tile in no specified group.
    ''' </summary>
    Public Sub New()
        Me.New(False, Nothing, New Tile())
    End Sub

    Public Sub New(display As Boolean, groupID As Guid, tile As Tile)

        'This call is required by the Windows Form Designer.
        InitializeComponent()
        TileRefreshIntervalsLoc = New Dictionary(Of String, String) From
    {{"Never", My.Resources.clsDashboard_Never}, {"Every minute", My.Resources.clsDashboard_EveryMinute}, {"Every 5 minutes", My.Resources.clsDashboard_Every5Minutes},
    {"Every 10 minutes", My.Resources.clsDashboard_Every10Minutes}, {"Every 30 minutes", My.Resources.clsDashboard_Every30Minutes}}

        mGroupID = groupID
        mTile = tile
        mOrigTileName = tile.Name
        mReadOnly = (display = True)

        'Initialise dropdown lists
        Dim intervalList As New List(Of ValRep)
        For Each i As TileRefreshIntervals In [Enum].GetValues(GetType(TileRefreshIntervals))
            intervalList.Add(New ValRep(i, TileRefreshIntervalsLoc.Item(i.GetFriendlyName())))
        Next
        cmbTileRefresh.DataSource = intervalList
        Dim tileTypeList As New List(Of ValRep)
        tileTypeList.Add(New ValRep(TileTypes.Chart, My.Resources.clsDashboard_Chart))
        cmbTileType.DataSource = tileTypeList

        'Setup tile definition
        UnpackTile()
        If mReadOnly Then
            txtTileName.Enabled = False
            txtDescription.Enabled = False
            cmbTileType.Enabled = False
            cmbTileRefresh.Enabled = False
            mControl.Enabled = False
        Else
            btnOK.Enabled = (txtTileName.Text <> String.Empty)
        End If
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The value of the tile held by this control
    ''' </summary>
    ''' <remarks>This ensures that the tile has the latest value before retrieving,
    ''' and that the UI is updated to the new value of the tile when setting.
    ''' </remarks>
    Public Property TileValue As Tile
        Get
            PackTile()
            Return mTile
        End Get
        Set(value As Tile)
            mTile = value
            UnpackTile()
        End Set
    End Property

#End Region

#Region "Packing/Unpacking"

    'Unpack the generic tile definition
    Private Sub UnpackTile()
        If mTile.ID = Guid.Empty Then
            cmbTileType.SelectedItem = cmbTileType.Items(0)
            cmbTileRefresh.SelectedItem = cmbTileRefresh.Items(0)
        Else
            txtTileName.Text = mTile.Name
            txtDescription.Text = mTile.Description
            cmbTileType.SelectedItem = mTile.Type
            cmbTileRefresh.SelectedIndex = cmbTileRefresh.FindStringExact(TileRefreshIntervalsLoc.Item(mTile.RefreshInterval.GetFriendlyName()))
        End If

        ChangeTileType()
    End Sub

    'Pack entered tile details
    Private Sub PackTile()
        mTile.Name = txtTileName.Text
        mTile.Description = txtDescription.Text
        mTile.Type = CType(CType(cmbTileType.SelectedItem, ValRep).value, TileTypes)
        mTile.RefreshInterval = CType(CType(cmbTileRefresh.SelectedItem, ValRep).value, TileRefreshIntervals)

        CType(mControl, ITileDefinitionControl).UnloadProperties()
    End Sub

#End Region

#Region "Saving/Changing type"

    'Save tile definition to database
    Private Function SaveTile() As Boolean
        'Check name/description lengths
        If txtTileName.Text.Length > 50 Then
            UserMessage.Show(My.Resources.TileNameCannotBeGreaterThan50Characters)
            Return False
        ElseIf txtDescription.Text.Length > 80 Then
            UserMessage.Show(My.Resources.TileDescriptionCannotBeGreaterThan80Characters)
            Return False
        End If

        PackTile()
        Dim properties As String = CType(mControl, ITileDefinitionControl).FormatXMLProperties()
        Try
            If mTile.ID = Guid.Empty Then
                mTile = gSv.CreateTile(mGroupID, mTile, properties)
            Else
                gSv.UpdateTile(mTile, mOrigTileName, properties)
            End If
            Return True
        Catch duplicate As NameAlreadyExistsException
            UserMessage.Show(duplicate.Message)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.FailedToSaveTile0, ex.Message), ex)
        End Try
        Return False
    End Function

    'Handle change of tile type
    Private Sub ChangeTileType()
        If mControl IsNot Nothing Then
            mControl.Dispose()
            panTile.Controls.Remove(mControl)
        End If

        Dim tileType As TileTypes = CType(CType(cmbTileType.SelectedItem, ValRep).value, TileTypes)
        Select Case tileType
            Case TileTypes.Chart
                mControl = CType(Activator.CreateInstance(GetType(ctlChartTile)), Control)
        End Select

        panTile.SuspendLayout()
        CType(mControl, ITileDefinitionControl).LoadProperties(mTile)
        mControl.Dock = DockStyle.Fill
        panTile.Controls.Add(mControl)
        panTile.ResumeLayout(True)
    End Sub

#End Region

#Region "Event handlers"

    Private Sub txtTileName_TextChanged(sender As Object, e As EventArgs) Handles txtTileName.TextChanged
        If txtTileName.Text <> String.Empty Then
            btnOK.Enabled = True
        Else
            btnOK.Enabled = False
        End If
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        'Save tile to database and exit
        If mReadOnly Then
            DialogResult = DialogResult.Cancel
            Close()
        Else
            If Not SaveTile() Then Exit Sub

            DialogResult = DialogResult.OK
            Close()
        End If
    End Sub

    Private Sub tcTileDetails_Selected(sender As Object, e As EventArgs) _
     Handles tcTileDetails.Selected
        'Generate preview
        If tcTileDetails.SelectedTab Is tpPreview Then
            tileView.InitForPreview(CType(mControl, ITileDefinitionControl).BuildPreview(txtTileName.Text, txtDescription.Text))
        End If
    End Sub

    Private Sub cmbTiletype_SelectionChangeCommitted(sender As Object, e As EventArgs) _
     Handles cmbTileType.SelectionChangeCommitted
        'Tile type changed
        ChangeTileType()
    End Sub

    Public Overrides Function GetHelpFile() As String
        Return "dashboards-tiles.htm"
    End Function

    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

#End Region

#Region "IEnvironmentColourManager implementation"

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return objTitleBar.BackColor
        End Get
        Set(value As Color)
            objTitleBar.BackColor = value
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return objTitleBar.TitleColor
        End Get
        Set(value As Color)
            objTitleBar.TitleColor = value
            objTitleBar.SubtitleColor = value
        End Set
    End Property

#End Region

End Class
