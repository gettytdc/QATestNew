Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.BPCoreLib
Imports LocaleTools

Public Class ctlChartTile : Implements ITileDefinitionControl

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

    'Reference to the parent tile
    Private mBaseTile As Tile
    'Item representing a missing data source
    Private mMissingItem As ComboBoxItem

    Public Sub New()

        'This call is required by the designer.
        InitializeComponent()

        'Initialise chart type dropdown list
        Dim cTypes As New List(Of ValRep)
        For Each i As ChartTypes In [Enum].GetValues(GetType(ChartTypes))
            If i.ToString.Equals("StackedBar100") OrElse i.ToString.Equals("StackedColumn100") Then Continue For
            Dim HeaderText As String = My.Resources.ResourceManager.GetString("clsChartTile_" & i.ToString, My.Resources.Culture)
            If (HeaderText Is Nothing) Then
                HeaderText = i.GetFriendlyName()
            End If
            cTypes.Add(New ValRep(i, HeaderText))
        Next
        cmbChartType.DataSource = cTypes

        'Initialise data source dropdown list
        For Each ds As KeyValuePair(Of String, String) In GetChartDataSources()
            cmbProcedure.Items.Add(New ComboBoxItem(ds.Key, ds.Value))
        Next
    End Sub

#Region "ITileDefinitionControl implementation"

    'Unpack chart tile properties
    Public Sub UnpackChart(tile As Tile) Implements ITileDefinitionControl.LoadProperties
        mBaseTile = tile

        If mBaseTile.ID = Guid.Empty Then
            cmbChartType.SelectedItem = cmbChartType.Items(0)
            cmbProcedure.SelectedItem = cmbProcedure.Items(0)
        Else
            Dim chart As New ChartTile(mBaseTile)
            Dim selectedChart = MapUnSupportedChartTypes(chart.ChartType)
            Dim HeaderText As String = My.Resources.ResourceManager.GetString("clsChartTile_" & selectedChart.ToString, My.Resources.Culture)
            If (HeaderText Is Nothing) Then
                HeaderText = chart.ChartType.GetFriendlyName()
            End If
            cmbChartType.SelectedIndex = cmbChartType.FindStringExact(HeaderText)
            Dim i As Integer = cmbProcedure.FindStringExact(chart.Procedure)
            If i < 0 Then
                'If data source not found add missing item
                mMissingItem = New ComboBoxItem(String.Format(My.Resources.ctlChartTile_MISSING0, chart.Procedure), String.Empty)
                i = cmbProcedure.Items.Add(mMissingItem)
            End If
            cmbProcedure.SelectedIndex = i
        End If

        DataSourceChanged()
    End Sub

    'Pack entered chart details
    Public Sub Pack() Implements ITileDefinitionControl.UnloadProperties
        Dim chart As New ChartTile(mBaseTile)

        chart.ChartType = CType(CType(cmbChartType.SelectedItem, ValRep).value, ChartTypes)
        'Don't update data source or params if it is missing
        If cmbProcedure.SelectedItem IsNot mMissingItem Then
            chart.Procedure = CType(cmbProcedure.SelectedItem, ComboBoxItem).Text
            chart.Parameters.Clear()
            For Each row As DataGridViewRow In dgvParameters.Rows
                If row.Cells(1).Value IsNot Nothing Then
                    chart.Parameters.Add(CStr(row.Cells(2).Value), CStr(row.Cells(1).Value))
                End If
            Next
        End If

        chart.SaveProperties()
    End Sub

    'Build chart preview
    Public Function BuildPreview(name As String, desc As String) As System.Windows.UIElement _
     Implements ITileDefinitionControl.BuildPreview
        mBaseTile.Name = name
        mBaseTile.Description = desc
        Pack()
        Dim chart As New ChartTile(mBaseTile)
        Return chart.Build(New Size(1, 1))
    End Function

    'Return formatted XML properties
    Public Function FormatXMLProperties() As String Implements ITileDefinitionControl.FormatXMLProperties
        Dim sb As New StringBuilder()
        sb.Append($"{lblChartType.Text}={cmbChartType.SelectedItem}")
        Dim ds As String = CType(cmbProcedure.SelectedItem, ComboBoxItem).Text
        If cmbProcedure.SelectedItem Is mMissingItem Then ds = ds.Substring(9)
        sb.Append($", {lblProcedure.Text}={ds}")
        If dgvParameters.RowCount > 0 Then
            sb.Append($", {lblParameters.Text}(")
            Dim i As Integer = 0
            For Each row As DataGridViewRow In dgvParameters.Rows
                If row.Cells(1).Value IsNot Nothing Then
                    sb.Append(String.Format("{0}{1}={2}", IIf(i > 0, ", ", ""),
                                row.Cells(2).Value, row.Cells(1).Value))
                    i += 1
                End If
            Next
            sb.Append(")")
        End If
        Return sb.ToString()
    End Function
#End Region

    'Queries the database for suitable chart data sources
    Private Function GetChartDataSources() As Dictionary(Of String, String)
        Dim dataSources As New Dictionary(Of String, String)
        Try
            dataSources = gSv.ListStoredProcedures(TileTypes.Chart)
        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlChartTile_FailedToRetrieveChartDataSources0 & ex.Message, ex)
        End Try

        Return dataSources
    End Function

    'Queries database for stored procedure parameters
    Private Function GetDataSourceParams(dsName As String) As List(Of String)
        Dim paramList As New List(Of String)
        Try
            paramList = gSv.ListStoredProcedureParameters(dsName)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlChartTile_FailedToRetrieveDataSourceParameters0, ex.Message), ex)
        End Try

        Return paramList
    End Function

    'Handle change in data source
    Private Sub DataSourceChanged()
        'Remove missing data source item if another selected
        If mMissingItem IsNot Nothing AndAlso cmbProcedure.SelectedItem IsNot mMissingItem Then
            cmbProcedure.Items.Remove(mMissingItem)
            mMissingItem = Nothing
        End If

        'Load parameters for this data source
        Dim chart As New ChartTile(mBaseTile)
        dgvParameters.Rows.Clear()
        For Each param As String In GetDataSourceParams(CType(cmbProcedure.SelectedItem, ComboBoxItem).Text)
            Dim value As String = String.Empty
            Dim friendly As String = LTools.Get(param, "tile", Options.Instance.CurrentLocale, "param")
            If chart.Parameters.TryGetValue(param, value) Then
                ' Friendly name stored in column 0, param real name in column 2
                dgvParameters.Rows.Add(friendly, value, param)
            Else
                dgvParameters.Rows.Add(friendly, Nothing, param)
            End If
        Next

        'Enable help button for system data sources
        If CType(cmbProcedure.SelectedItem, ComboBoxItem).Text.StartsWith("BPDS_") Then
            btnDetails.Enabled = True
        Else
            btnDetails.Enabled = False
        End If
    End Sub

    Private Sub cmbProcedure_SelectionChangeCommitted(sender As Object, e As EventArgs) _
     Handles cmbProcedure.SelectionChangeCommitted
        DataSourceChanged()
    End Sub

    Private Sub btnDetails_Click(sender As Object, e As EventArgs) Handles btnDetails.Click
        Dim page As String = CType(cmbProcedure.SelectedItem, ComboBoxItem).Tag.ToString()
        Try
            OpenHelpFile(Me, "DataSources/" & page)
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Private Function MapUnSupportedChartTypes(chartType As ChartTypes) As ChartTypes
        Dim selectedChart = chartType
        Select Case chartType
            Case ChartTypes.StackedBar100
                selectedChart = ChartTypes.Bar
            Case ChartTypes.StackedColumn100
                selectedChart = ChartTypes.Column
        End Select
        Return selectedChart
    End Function

End Class
