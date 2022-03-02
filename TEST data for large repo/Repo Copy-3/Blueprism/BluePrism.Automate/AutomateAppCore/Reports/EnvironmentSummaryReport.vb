Imports System.IO
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Logging
Imports BluePrism.BPCoreLib

Public Class EnvironmentSummaryReport
    Inherits clsReporter

#Region " Private Members "
    ' The report name displayed in the UI
    Private ReadOnly mName As String

    ' The report description displayed in the UI
    Private ReadOnly mDescription As String
#End Region

    Public Sub New()
        mName = My.Resources.EnvironmentSummaryReport_Name
        mDescription = My.Resources.EnvironmentSummaryReport_Description
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

        'Go To DB and Return DataTable (use con.ExecuteReturnDataTable(cmd))
        Dim environmentData = gSv.GetEnvironmentData()
        Dim environmentDataTable As New DataTable()
        environmentDataTable.Columns.Add(New DataColumn(My.Resources.EnvironmentReportingColumnFqdn))
        environmentDataTable.Columns.Add(New DataColumn(My.Resources.EnvironmentReportingColumnPort))
        environmentDataTable.Columns.Add(New DataColumn(My.Resources.EnvironmentReportingColumnBPVersion))
        environmentDataTable.Columns.Add(New DataColumn(My.Resources.EnvironmentReportingColumnEnvironmentType))
        environmentDataTable.Columns.Add(New DataColumn(My.Resources.EnvironmentReportingColumnFirstConnected))
        environmentDataTable.Columns.Add(New DataColumn(My.Resources.EnvironmentReportingColumnLastUpdated))
        environmentDataTable.Columns.Add(New DataColumn(My.Resources.EnvironmentReportingCustomEncryption))
        environmentDataTable.Columns.Add(New DataColumn(My.Resources.EnvironmentReportingCustomCertificateExpirationDate))
        environmentDataTable.Columns.Add(New DataColumn(My.Resources.EnvironmentReportingColumnApplicationServer))


        For Each data As EnvironmentData in environmentData
            environmentDataTable.Rows.Add(data.FullyQualifiedDomainName,
                                          If(data.PortNumber >= 0, data.PortNumber.ToString, ""),
                                          data.VersionNumber,
                                          My.Resources.ResourceManager.GetString($"EnvironmentReportingType{data.EnvironmentType.ToString()}"),
                                          data.CreatedDateTime,
                                          data.UpdatedDateTime,
                                          If(data.CertificateExpTime.HasValue, My.Resources.EnvironmentReportingCustomEncryptionEnabled, My.Resources.EnvironmentReportingCustomEncryptionDisabled),
                                          If(data.CertificateExpTime.HasValue, data.CertificateExpTime.ToString(), String.Empty),
                                          data.ApplicationServer)

        Next

        sw.Write(environmentDataTable.DataTableToCsv(True))

    End Sub
End Class
