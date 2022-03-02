
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports System.IO

''' <summary>
''' Generates the general 'System' reports.
''' </summary>
Public Class clsSystemReporter
    Inherits clsReporter

#Region " Private Members "
    ' The report name displayed in the UI
    Private mName As String

    ' The report description displayed in the UI
    Private mDescription As String
#End Region

    Public Sub New()
        mName = My.Resources.clsSystemReporter_System
        mDescription = My.Resources.clsSystemReporter_GeneralStatusOfTheSystemProcessesBusinessObjects
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
            Return OutputFormats.WikiText
        End Get
    End Property

    Public Overrides Function HasPermission() As Boolean
        Return User.Current.HasPermission("System Manager")
    End Function

    Public Overrides Function GetArguments() As List(Of ArgumentInfo)
        Return New List(Of ArgumentInfo)
    End Function

    Protected Overrides Sub GenerateReport(ByVal args As List(Of Object), ByVal sw As StreamWriter)
        MyBase.GenerateReport(args, sw)

        Dim sErr As String = Nothing

        sw.WriteLine(My.Resources.clsSystemReporter_Overview)
        sw.WriteLine(My.Resources.clsSystemReporter_BluePrismSystemReport)
        sw.WriteLine("")
        sw.WriteLine(My.Resources.clsSystemReporter_Date & Date.Now().ToUniversalTime().ToString("u"))
        sw.WriteLine(My.Resources.clsSystemReporter_VersionBluePrism & GetType(clsSystemReporter).Assembly.GetName.Version.ToString)
        Dim db As clsDBConnectionSetting = Options.Instance.DbConnectionSetting()
        If db.ConnectionType = ConnectionType.BPServer Then
            sw.WriteLine(My.Resources.clsSystemReporter_BluePrismServer & db.DBServer)
        ElseIf db.ConnectionType = ConnectionType.Availability Then
            sw.WriteLine(My.Resources.clsSystemReporter_AvailabilityGroup & db.DBServer)
        Else
            sw.WriteLine(String.Format(My.Resources.clsSystemReporter_Database0On1, db.DatabaseName, db.DBServer))
            If db.WindowsAuth Then
                sw.WriteLine(My.Resources.clsSystemReporter_DatabaseConnectionWindowsAuthentication)
            Else
                sw.WriteLine(String.Format(My.Resources.clsSystemReporter_DatabaseConnectionSQLServerAuthenticationAs0, db.DBUserName))
            End If
        End If
        Dim procs As List(Of Guid) = gSv.GetAllProcessIDs()
        sw.WriteLine(My.Resources.clsSystemReporter_Processes & procs.Count.ToString())
        sw.WriteLine("")
        sw.WriteLine(My.Resources.clsSystemReporter_DatabaseStatistics)
        Dim sessioncount As Integer, sessionlogcount As Integer, resourcecount As Integer, queueitemcount As Integer
        Try
            gSv.GetDBStats(sessioncount, sessionlogcount, resourcecount, queueitemcount)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.clsSystemReporter_FailedToGetStats0, ex.Message))
        End Try
        sw.WriteLine(My.Resources.clsSystemReporter_SessionCount & sessioncount.ToString())
        sw.WriteLine(My.Resources.clsSystemReporter_SessionLogCount & sessionlogcount.ToString())
        sw.WriteLine(My.Resources.clsSystemReporter_ResourceCount & resourcecount.ToString())
        sw.WriteLine(My.Resources.clsSystemReporter_QueueItemCount & queueitemcount.ToString())
        sw.WriteLine("")
        sw.WriteLine(My.Resources.clsSystemReporter_ProcessDetails)
        For Each procid As Guid In procs

            Dim xml As String
            Try
                xml = gSv.GetProcessXML(procid)
            Catch ex As Exception
                sw.WriteLine(String.Format(My.Resources.clsSystemReporter_FailedToGetXMLForProcess01, procid.ToString(), sErr))
                Continue For
            End Try
            Dim proc As clsProcess
            proc = clsProcess.FromXML(clsGroupObjectDetails.Empty, xml, False, sErr)
            If proc Is Nothing Then
                sw.WriteLine(String.Format(My.Resources.clsSystemReporter_FailedToParseXMLForProcess01, procid.ToString(), sErr))
            Else
                Dim heading As String = CStr(IIf(proc.ProcessType = DiagramType.Process, My.Resources.clsSystemReporter_Process, My.Resources.clsSystemReporter_BusinessObject))
                sw.WriteLine(String.Format("=={0} ({1})==", gSv.GetProcessNameByID(procid), heading))
                sw.WriteLine(My.Resources.clsSystemReporter_ID & procid.ToString())
                Dim createdby As String = Nothing, createdate As Date
                Dim modifiedby As String = Nothing, modifieddate As Date
                If Not gSv.GetProcessInfo(procid, createdby, createdate, modifiedby, modifieddate) Then
                    sw.WriteLine(My.Resources.clsSystemReporter_FailedToGetProcessInfo)
                Else
                    sw.WriteLine(String.Format(My.Resources.clsSystemReporter_CreatedBy0On1, createdby, createdate.ToString("u")))
                    sw.WriteLine(String.Format(My.Resources.clsSystemReporter_ModifiedBy0On1, modifiedby, modifieddate.ToString("u")))
                End If
                sw.WriteLine(My.Resources.clsSystemReporter_Type & proc.ProcessType.ToString())
                If proc.ProcessType = DiagramType.Object Then
                    sw.WriteLine(My.Resources.clsSystemReporter_RunMode & proc.ObjectRunMode.ToString())
                End If
                sw.WriteLine(My.Resources.clsSystemReporter_Pages & proc.SubSheets.Count.ToString())
                sw.WriteLine(My.Resources.clsSystemReporter_Stages & proc.GetStages().Count.ToString())
                sw.WriteLine("")

                Dim rules = gSv.GetValidationInfo()
                Dim validationInfo = rules.ToDictionary(Of Integer, clsValidationInfo)(Function(y) y.CheckID, Function(z) z)
                Dim exTypes As New HashSet(Of String)(StringComparer.CurrentCultureIgnoreCase)

                ' if we are performing exception type validation we need to pass in the
                ' list of existing exception types
                Try
                    If validationInfo(142).Enabled Then
                        Dim exceptionTypes = gSv.GetExceptionTypes()
                        exTypes.UnionWith(exceptionTypes)
                    End If
                Catch ex As Exception
                    sw.WriteLine(String.Format(My.Resources.clsServer_DatabaseError0, ex.Message))
                End Try


                'Filtered (according to System Manager settings) list of validation issues...
                Dim vrl As ICollection(Of ValidateProcessResult) =
                   clsValidationInfo.FilteredValidateProcess(proc, validationInfo, False, True, exTypes)

                If vrl.Count > 0 Then
                    sw.WriteLine(My.Resources.clsSystemReporter_ValidationIssues)
                    sw.WriteLine(My.Resources.clsSystemReporter_TotalCount & vrl.Count.ToString())
                    'Gather up all the identical issues so we just have a
                    'count of each instead of a long list...
                    Dim vcount As New Dictionary(Of Integer, Integer)
                    For Each vr As ValidateProcessResult In vrl
                        If vcount.ContainsKey(vr.CheckID) Then
                            vcount(vr.CheckID) += 1
                        Else
                            vcount.Add(vr.CheckID, 1)
                        End If
                    Next
                    For Each checkid As Integer In vcount.Keys
                        sw.WriteLine("*" & validationInfo(checkid).Message & My.Resources.clsSystemReporter_X & vcount(checkid))
                    Next
                    sw.WriteLine("")
                End If

                'Analyse actions if it's an object...
                If proc.ProcessType = DiagramType.Object Then

                    Dim total As Integer, globalcount As Integer
                    Dim globaldetails As String = Nothing
                    proc.AnalyseAMIActions(total, globalcount, globaldetails)
                    sw.WriteLine(My.Resources.clsSystemReporter_AMIActionUsage)
                    sw.WriteLine(My.Resources.clsSystemReporter_Total & total.ToString())
                    sw.WriteLine(My.Resources.clsSystemReporter_Global & globalcount.ToString())
                    If Not globaldetails Is Nothing Then
                        sw.WriteLine(My.Resources.clsSystemReporter_GlobalAMIActionDetails)
                        sw.WriteLine(globaldetails)
                    End If

                    'Element usage info as well...
                    Dim el_total As Integer, el_used As Integer
                    proc.GetElementUsageInfo(el_total, el_used)
                    sw.WriteLine(My.Resources.clsSystemReporter_ElementUsage)
                    sw.WriteLine(My.Resources.clsSystemReporter_Total & el_total.ToString())
                    sw.WriteLine(My.Resources.clsSystemReporter_Used & el_used.ToString())
                End If

                proc.Dispose()
            End If
            sw.WriteLine("")
        Next

    End Sub

End Class
