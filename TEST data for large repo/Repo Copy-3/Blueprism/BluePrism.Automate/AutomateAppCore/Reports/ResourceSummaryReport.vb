Imports System.IO
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Extensions

Public Class ResourceSummaryReport
    Inherits clsReporter

#Region " Private Members "
    ' The report name displayed in the UI
    Private ReadOnly mName As String

    ' The report description displayed in the UI
    Private ReadOnly mDescription As String
#End Region

    Public Sub New()
        mName = My.Resources.RuntimeResourceSummary_Name
        mDescription = My.Resources.RuntimeResourceSummary_Description
    End Sub

    Public Overrides ReadOnly Property Name As String
        Get
            Return mName
        End Get
    End Property

    Public Overrides ReadOnly Property Description As String
        Get
            Return mDescription
        End Get
    End Property

    Public Overrides ReadOnly Property OutputFormat As OutputFormats
        Get
            Return OutputFormats.CSV
        End Get
    End Property

    Public Overrides Function HasPermission() As Boolean
        Return User.Current.HasPermission("System Manager")
    End Function

    Public Overrides Function GetArguments() As List(Of ArgumentInfo)
        Return New List(Of ArgumentInfo)
    End Function

    Protected Overrides Sub GenerateReport(args As List(Of Object), sw As StreamWriter)
        MyBase.GenerateReport(args, sw)
        Dim resourceReportData = gSv.GetResourceReport()
        Dim resourcesDatatable As New DataTable()
        resourcesDatatable.Columns.Add(New DataColumn(My.Resources.ResourceReportingColumn_ResourceName))
        resourcesDatatable.Columns.Add(New DataColumn(My.Resources.ResourceReportingColumn_FQDN))
        resourcesDatatable.Columns.Add(New DataColumn(My.Resources.ResourceReportingColumn_ResourceID))
        resourcesDatatable.Columns.Add(New DataColumn(My.Resources.ResourceReportingColumn_Attribute))
        resourcesDatatable.Columns.Add(New DataColumn(My.Resources.ResourceReportingColumn_Retired))
        resourcesDatatable.Columns.Add(New DataColumn(My.Resources.ResourceReportingColumn_Pool))
        resourcesDatatable.Columns.Add(New DataColumn(My.Resources.ResourceReportingColumn_PoolController))
        resourcesDatatable.Columns.Add(New DataColumn(My.Resources.ResourceReportingColumn_UserID))
        resourcesDatatable.Columns.Add(New DataColumn(My.Resources.ResourceReportingColumn_LoggingLevel))
        resourcesDatatable.Columns.Add(New DataColumn(My.Resources.ResourceReportingColumn_Schedules))

        For Each resource In resourceReportData
            resourcesDatatable.Rows.Add(resource.Name,
                                        resource.FullyQualifiedDomainName,
                                        resource.ResourceId,
                                        resource.AttributeId.GetLocalizedName,
                                        If(resource.AttributeId.IsRetired, My.Resources.AutomateAppCore_True, My.Resources.AutomateAppCore_False),
                                        resource.Pool,
                                        If(resource.IsController, My.Resources.AutomateAppCore_True, My.Resources.AutomateAppCore_False),
                                        resource.UserId,
                                        GetLoggingLocalizedName(resource.LoggingLevel),
                                        resource.Schedules)
        Next

        sw.Write(resourcesDatatable.DataTableToCsv(True))

    End Sub

    Private Function GetLoggingLocalizedName(logging As clsAPC.Diags) As String
        Dim loggingLevels As String() = logging.ToString().Split(","c)
        Dim res As String = String.Empty

        For Each loggingLevel As String In loggingLevels
            res += $", {My.Resources.ResourceManager.GetString($"clsAPCDiags_{loggingLevel.Trim()}")}"
        Next

        Return CStr(IIf(String.IsNullOrWhiteSpace(res), logging.ToString(), res.Trim(","c, " "c)))
    End Function

End Class
