Imports System.IO
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Report that will show me the number and percentage of stages that are set to high logging in my processes and objects
''' </summary>
Public Class clsHighLoggingReport
    Inherits clsReporter

#Region " Private Members "
    ' The report name displayed in the UI
    Private mName As String

    ' The report description displayed in the UI
    Private mDescription As String
#End Region

    Public Sub New()
        mName = My.Resources.clsLoggingReport_Name
        mDescription = My.Resources.clsHighLoggingReport_Description
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

        Dim sErr As String = Nothing
        Dim processIds As List(Of Guid) = gSv.GetAllProcessIDs()

        If processIds.Count > 0 Then
            sw.WriteLine(My.Resources.clsHighLoggingReport_csv_header)

            For Each processId As Guid In processIds

                Dim xml As String
                Try
                    xml = gSv.GetProcessXML(processId)
                Catch pEx As PermissionException
                    'if the user does not have permission over a given process, then ignore.
                    Continue For
                Catch ex As Exception
                    sw.WriteLine(String.Format(My.Resources.clsSystemReporter_FailedToGetXMLForProcess01, processId.ToString(), sErr))
                    Continue For
                End Try
                Dim process As clsProcess
                process = clsProcess.FromXML(clsGroupObjectDetails.Empty, xml, False, sErr)
                If process IsNot Nothing Then
                    Dim excludedTypes As New List(Of StageTypes)({StageTypes.Data,
                                                                   StageTypes.Collection,
                                                                   StageTypes.ProcessInfo,
                                                                   StageTypes.SubSheetInfo,
                                                                   StageTypes.Anchor,
                                                                   StageTypes.Block})

                    Dim processName = gSv.GetProcessNameByID(processId)
                    Dim processType As String = CStr(IIf(process.ProcessType = DiagramType.Process,
                                                          My.Resources.clsSystemReporter_Process,
                                                          My.Resources.clsSystemReporter_BusinessObject))
                    Dim stagesCount = process.GetStages(Nothing, excludedTypes).Count
                    Dim logCount = process.GetNumStagesByLogInhibitModes(Nothing, excludedTypes, LogInfo.InhibitModes.Never)
                    Dim percentage = Math.Round(logCount / stagesCount * 100, 0, MidpointRounding.ToEven)

                    sw.WriteLine(String.Join(",", processType, processName, stagesCount, logCount, percentage))
                End If
            Next
            sw.WriteLine("")
        Else
            sw.WriteLine(My.Resources.clsReporter_NoReportContentAvailable)
        End If
    End Sub
End Class
