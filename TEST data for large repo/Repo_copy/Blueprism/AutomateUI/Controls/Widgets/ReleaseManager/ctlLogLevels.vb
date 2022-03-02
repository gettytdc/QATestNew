Imports AutomateControls
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateAppCore
Imports BluePrism.Images
Imports BluePrism.AutomateProcessCore

Public Class ctlLogLevels : Inherits ctlWizardStageControl

#Region " Private Members "

    Private _processComponents As IEnumerable(Of ProcessComponent)

#End Region

#Region " Properties "

    Public Property ProcessComponents() As IEnumerable(Of ProcessComponent)
        Get
            Return _processComponents
        End Get
        Set(ByVal value As IEnumerable(Of ProcessComponent))
            _processComponents = value

            If value Is Nothing Then
                Return
            End If

            Dim excludedTypes As New List(Of StageTypes)({StageTypes.Data, StageTypes.Collection, StageTypes.ProcessInfo, StageTypes.SubSheetInfo, StageTypes.Anchor, StageTypes.Block})
            Dim sortableList As New SortableBindingList(Of DataGridData)
            sortableList.AddAll(
                _processComponents.Select(
                Function(p)
                    Dim numberOfStages = p.AssociatedProcess.GetNumStages(Nothing, excludedTypes)
                    Dim numberOfStagesLogging = p.AssociatedProcess.GetNumStagesByLogInhibitModes(Nothing, excludedTypes, LogInfo.InhibitModes.Never)
                    Return New DataGridData() With
                    {
                        .Name = p.Name,
                        .NumberOfStages = numberOfStages,
                        .NumberOfStagesLogging = numberOfStagesLogging,
                        .PackageComponentType = p.Type,
                        .LoggingPercentage = Convert.ToInt32((numberOfStagesLogging / numberOfStages) * 100)
                    }
                End Function) _
                .OrderByDescending(Function(d) d.LoggingPercentage) _
                .ThenByDescending(Function(d) d.NumberOfStagesLogging) _
                .ThenBy(Function(d) d.Name))
            ContentsDataGridView.AutoGenerateColumns = False
            ContentsDataGridView.DataSource = sortableList
        End Set
    End Property

#End Region

#Region " Methods "

    Private Sub ContentsDataGridView_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles ContentsDataGridView.DataBindingComplete
        For Each row As DataGridViewRow In ContentsDataGridView.Rows
            Dim data = CType(row.DataBoundItem, DataGridData)
            row.Cells("ImageColumn").Value = ImageLists.Components_16x16().Images(data.PackageComponentType.Key)
            row.Cells("ImageColumn").ToolTipText = data.PackageComponentType.Plural
        Next
    End Sub

#End Region

#Region "Private Classes"
    Protected Class DataGridData
        Public Property Name() As String
        Public Property NumberOfStages() As Int32
        Public Property NumberOfStagesLogging() As Int32
        Public Property PackageComponentType() As PackageComponentType
        Public Property LoggingPercentage() As Int32
    End Class
#End Region

End Class
