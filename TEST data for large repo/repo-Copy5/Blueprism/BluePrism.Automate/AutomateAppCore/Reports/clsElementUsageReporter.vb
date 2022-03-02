
Imports BluePrism.AutomateAppCore.Auth
Imports System.IO

''' <summary>
''' Generates element usage reports.
''' </summary>
Public Class clsElementUsageReporter
    Inherits clsReporter

#Region " Private Members "
    ' The report name displayed in the UI
    Private mName As String

    ' The report description displayed in the UI
    Private mDescription As String
#End Region

    Public Sub New()
        mName = My.Resources.clsElementUsageReporter_ElementUsage
        mDescription = My.Resources.clsElementUsageReporter_UsageOfAllApplicationModellerElementsInABusinessObject
    End Sub

    Public Overrides ReadOnly Property Name() As String
        Get
            Return mName
        End Get
    End Property

    Public Overrides ReadOnly Property Description() As String
        Get
            Return mDescription
        End Get
    End Property

    Public Overrides ReadOnly Property OutputFormat() As OutputFormats
        Get
            Return OutputFormats.CSV
        End Get
    End Property

    Public Overrides Function HasPermission() As Boolean
        Return User.Current.HasPermission("System Manager")
    End Function

    Public Overrides Function GetArguments() As List(Of ArgumentInfo)
        Dim args As New List(Of ArgumentInfo)
        args.Add(New ArgumentInfo(My.Resources.clsElementUsageReporter_Process, My.Resources.clsElementUsageReporter_TheNameOfTheProcessBusinessObject, True, GetType(String)))
        Return args
    End Function

    Protected Overrides Sub GenerateReport(ByVal args As List(Of Object), ByVal sw As StreamWriter)
        MyBase.GenerateReport(args, sw)

        Dim sErr As String = Nothing

        Dim procname As String = CType(args(0), String)

        Dim lines = gSv.GetProcessElementUsageDetails(procname)

        If lines.Count > 0 Then
            'Write the results to the CSV file...
            sw.WriteLine(My.Resources.clsElementUsageReporter_ObjectPageStageElementNarrativeDescription)
            For Each line As String In lines.Values
                sw.WriteLine(line)
            Next
            sw.WriteLine("")
        Else
            sw.WriteLine(My.Resources.clsReporter_NoReportContentAvailable)
        End If


    End Sub

End Class
