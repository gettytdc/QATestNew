Imports System.Data.SqlClient
Imports System.Globalization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Caching
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models

Partial Public Class clsServer

#Region "MI"

    Private Const mIsMIReportingEnabledCacheKey As String = "IsMIReportingEnabled"
    Private Const MIReportingEnvironmentLockExpiryKey As String = "EnvironmentLockTimeExpiry.MIReporting.InSeconds"
    Private Const MIReportingCommandTimeoutKey As String = "MIReporting.CommandTimeout.InSeconds"

    Private ReadOnly mIsMIReportingEnabledCache As Lazy(Of IRefreshCache(Of String, Boolean)) =
                         New Lazy(Of IRefreshCache(Of String, Boolean))(
                             CreateDatabaseCache(Of Boolean)(
                                 mIsMIReportingEnabledCacheKey,
                                 AddressOf RefreshIsMIReportingEnabled))

    ''' <summary>
    ''' Get a list of Process MI Template names
    ''' </summary>
    ''' <param name="Names">A list of names will be returned by this parameter, supplied parameter maybe null</param>
    <SecuredMethod(True)>
    Public Sub ProcessMITemplateNames(ByRef Names As Generic.List(Of String), ByVal gProcessID As Guid) Implements IServer.ProcessMITemplateNames
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT templatename FROM BPAProcessMITemplate WHERE processid=@processid")
            With cmd.Parameters
                .AddWithValue("@processid", gProcessID.ToString)
            End With

            Names = New List(Of String)
            Dim dr = CType(con.ExecuteReturnDataReader(cmd), SqlDataReader)
            If dr IsNot Nothing AndAlso dr.HasRows Then
                While dr.Read
                    Names.Add(CStr(dr.Item("templatename")))
                End While
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Updates the template xml of a template given a template name, and the process id to which the template relates
    ''' </summary>
    ''' <param name="sName">The name of the template</param>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    ''' <param name="sTemplateXML">The new template xml</param>
    <SecuredMethod(True)>
    Public Sub ProcessMIUpdateTemplate(ByVal sName As String, ByVal gProcessID As Guid, ByVal sTemplateXML As String) Implements IServer.ProcessMIUpdateTemplate
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("UPDATE BPAProcessMITemplate SET templatexml=@templatexml WHERE templatename=@templatename AND processid=@processid")
            With cmd.Parameters
                .AddWithValue("@templatename", sName)
                .AddWithValue("@templatexml", sTemplateXML)
                .AddWithValue("@processid", gProcessID.ToString)
            End With

            Dim Affected As Integer = CInt(con.ExecuteReturnRecordsAffected(cmd))
            If Affected <= 0 Then _
                Throw New MissingDataException(My.Resources.clsServer_CommandCompletedSuccesfullyButNoItemsWereUpdated)
        End Using
    End Sub

    ''' <summary>
    ''' Creates a new template.
    ''' </summary>
    ''' <param name="sName">The name of the template</param>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    ''' <param name="sTemplateXML">The new template xml</param>
    <SecuredMethod(True)>
    Public Sub ProcessMICreateTemplate(ByVal sName As String, ByVal gProcessID As Guid, ByVal sTemplateXML As String) Implements IServer.ProcessMICreateTemplate
        CheckPermissions()
        Using con = GetConnection()

            Dim cmd As New SqlCommand("INSERT INTO BPAProcessMITemplate (templatename, processid, defaulttemplate, templatexml) VALUES (@templatename, @processid, 0, @templatexml)")
            With cmd.Parameters
                .AddWithValue("@templatename", sName)
                .AddWithValue("@processid", gProcessID.ToString)
                .AddWithValue("@templatexml", sTemplateXML)
            End With

            con.Execute(cmd)
        End Using
    End Sub

    ''' <summary>
    ''' Deletes a process mi template
    ''' </summary>
    ''' <param name="sName">The name of the template</param>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    <SecuredMethod(True)>
    Public Sub ProcessMIDeleteTemplate(ByVal sName As String, ByVal gProcessID As Guid) Implements IServer.ProcessMIDeleteTemplate
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("DELETE FROM BPAProcessMITemplate WHERE templatename=@templatename AND processid=@processid")
            With cmd.Parameters
                .AddWithValue("@templatename", sName)
                .AddWithValue("@processid", gProcessID.ToString)
            End With

            Dim Affected As Integer = CInt(con.ExecuteReturnRecordsAffected(cmd))
            If Affected <= 0 Then _
                Throw New MissingDataException(My.Resources.clsServer_CommandCompletedSuccesfullyButNoItemsWereUpdated)
        End Using
    End Sub

    ''' <summary>
    ''' Gets a process mi template
    ''' </summary>
    ''' <param name="sName">The name of the template</param>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    ''' <param name="sTemplateXML">The template xml will be returned in this parameter</param>
    <SecuredMethod(True)>
    Public Sub ProcessMIGetTemplate(ByVal sName As String, ByVal gProcessID As Guid, ByRef sTemplateXML As String) Implements IServer.ProcessMIGetTemplate
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT templatexml FROM BPAProcessMITemplate WHERE templatename=@templatename AND processid=@processid")
            With cmd.Parameters
                .AddWithValue("@templatename", sName)
                .AddWithValue("@processid", gProcessID.ToString)
            End With

            Dim Reader = con.ExecuteReturnDataReader(cmd)
            If Reader.Read() Then
                sTemplateXML = CStr(Reader("templatexml"))
            Else
                Throw New MissingDataException(My.Resources.clsServer_NoMatchingProcessMiTemplateFound)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Gets the name of the default process mi template
    ''' </summary>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    ''' <param name="sName">The name of the template will be returned in this parameter</param>
    <SecuredMethod(True)>
    Public Sub ProcessMIGetDefaultTemplate(ByVal gProcessID As Guid, ByRef sName As String) Implements IServer.ProcessMIGetDefaultTemplate
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT templatename FROM BPAProcessMITemplate WHERE processid=@processid AND defaulttemplate=1")
            With cmd.Parameters
                .AddWithValue("@processid", gProcessID.ToString)
            End With

            Dim Reader = con.ExecuteReturnDataReader(cmd)
            If Reader.Read() Then
                sName = CStr(Reader("templatename"))
            Else
                Throw New MissingDataException(My.Resources.clsServer_NoMatchingQueueFound)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Sets the default process mi template
    ''' </summary>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    ''' <param name="sName">The name of the template</param>
    <SecuredMethod(True)>
    Public Sub ProcessMISetDefaultTemplate(ByVal gProcessID As Guid, ByRef sName As String) Implements IServer.ProcessMISetDefaultTemplate
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()

            Dim cmd As New SqlCommand("UPDATE BPAProcessMITemplate SET defaulttemplate=0 WHERE processid=@processid AND defaulttemplate=1")
            With cmd.Parameters
                .AddWithValue("@processid", gProcessID.ToString)
            End With
            con.Execute(cmd)

            cmd = New SqlCommand("UPDATE BPAProcessMITemplate SET defaulttemplate=1 WHERE processid=@processid AND templatename=@templatename")
            With cmd.Parameters
                .AddWithValue("@templatename", sName)
                .AddWithValue("@processid", gProcessID.ToString)
            End With
            con.Execute(cmd)

            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Gather some basic statistics about the database. This is used by the reporting
    ''' tool. All parameters are populated on return, unless an error occurs.
    ''' </summary>
    ''' <param name="sessioncount">The number of sessions</param>
    ''' <param name="sessionlogcount">The number of session log entries</param>
    ''' <param name="resourcecount">The number of resources</param>
    ''' <param name="queueitemcount">The number of queue items</param>
    <SecuredMethod(True)>
    Public Sub GetDBStats(ByRef sessioncount As Integer, ByRef sessionlogcount As Integer, ByRef resourcecount As Integer, ByRef queueitemcount As Integer) Implements IServer.GetDBStats
        CheckPermissions()
        Using con = GetConnection()
            Dim newid As Guid = Guid.NewGuid
            Using cmd As New SqlCommand("SELECT COUNT(*) FROM BPASession")
                sessioncount = CInt(con.ExecuteReturnScalar(cmd))
            End Using

            sessionlogcount = 0
            For Each slogtable As String In [Enum].GetNames(GetType(SessionLogTables))
                Dim sql = String.Format("SELECT COUNT(*) FROM {0} ",
                                        ValidateTableName(con, slogtable))
                Using cmd As New SqlCommand(sql)
                    sessionlogcount += CInt(con.ExecuteReturnScalar(cmd))
                End Using
            Next

            Using cmd As New SqlCommand("SELECT COUNT(*) FROM BPAResource")
                resourcecount = CInt(con.ExecuteReturnScalar(cmd))
            End Using

            Using cmd As New SqlCommand("SELECT COUNT(*) FROM BPAWorkQueueItem")
                queueitemcount = CInt(con.ExecuteReturnScalar(cmd))
            End Using
        End Using
    End Sub

    Private Sub RefreshIsMIReportingEnabled(cache As Object, e As EventArgs)
        Using connection = GetConnection()
            Dim isMIEnabled = mCacheDataProvider.IsMIReportingEnabled(connection)
            mIsMIReportingEnabledCache.Value.SetValue(mIsMIReportingEnabledCacheKey, isMIEnabled)
        End Using
    End Sub

#End Region

#Region "Process MI"

    ''' <summary>
    ''' Get the portion of a WHERE clause required to select the given sessions.
    ''' </summary>
    ''' <param name="sessions">An array of session numbers.</param>
    ''' <returns>The SQL fragment</returns>
    Private Function MIGetSessionClause(ByVal sessions() As Integer) As String
        Dim s As String = "SessionNumber IN ("
        For Each i As Integer In sessions
            s &= i.ToString() & ","
        Next
        s = s.Substring(0, s.Length - 1) & ")"
        Return s
    End Function

    ''' <summary>
    ''' Get the portion of a WHERE clause required to select the given stages.
    ''' </summary>
    ''' <param name="stages">An array of stages IDs.</param>
    ''' <returns>The SQL fragment</returns>
    Private Function MIGetStageClause(ByVal stages() As Guid) As String
        Dim s As String = "StageID IN ("
        For Each g As Guid In stages
            s &= "'" & g.ToString() & "',"
        Next
        s = s.Substring(0, s.Length - 1) & ")"
        Return s
    End Function

    ''' <summary>
    ''' Get MI data relating to decision stages
    ''' </summary>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <returns>A DataTable containing the requested data</returns>
    <SecuredMethod(True)>
    Public Function MIGetDecisionData(ByVal sessions() As Integer, ByVal stages() As Guid) As DataTable Implements IServer.MIGetDecisionData
        CheckPermissions()

        Dim res As New DataTable()
        res.TableName = "MIGetDecisionDataTable"
        res.Locale = CultureInfo.InvariantCulture
        Using con = GetConnection()
            For Each slogtable As String In [Enum].GetNames(GetType(SessionLogTables))
                Dim sbSQL As New System.Text.StringBuilder
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine(" CAST(stageid AS VARCHAR(36)) AS StageID,")
                sbSQL.AppendLine(" SUM(CASE WHEN SUBSTRING(Result, 1, DATALENGTH(Result)) = 'True' THEN 1 ELSE 0 END) AS TrueCount,")
                sbSQL.AppendLine(" SUM(CASE WHEN SUBSTRING(Result, 1, DATALENGTH(Result)) = 'False' THEN 1 ELSE 0 END) AS FalseCount")
                sbSQL.AppendFormat(" FROM {0} ", ValidateTableName(con, slogtable))
                sbSQL.AppendLine(" WHERE StageType = " & CStr(CInt(StageTypes.Decision)))

                sbSQL.AppendLine(" AND " & MIGetSessionClause(sessions))
                If stages IsNot Nothing Then sbSQL.AppendLine(" AND " & MIGetStageClause(stages))
                sbSQL.AppendLine(" GROUP BY CAST(stageid AS VARCHAR(36))")

                Using cmd As New SqlCommand(sbSQL.ToString)
                    con.LoadDataTable(cmd, res)
                End Using
            Next
        End Using

        Return res

    End Function

    ''' <summary>
    ''' Get MI data relating to calculation stages
    ''' </summary>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <returns>A DataTable containing the requested data</returns>
    <SecuredMethod(True)>
    Public Function MIGetCalculationData(ByVal sessions() As Integer, ByVal stages() As Guid) As DataTable Implements IServer.MIGetCalculationData
        CheckPermissions()

        Dim res As New DataTable()
        res.TableName = "MIGetCalculationDataTable"
        res.Locale = CultureInfo.InvariantCulture
        Using con = GetConnection()
            For Each slogtable As String In [Enum].GetNames(GetType(SessionLogTables))
                Dim sbSQL As New System.Text.StringBuilder

                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine(" CAST(StageID AS VARCHAR(36)) AS StageID,")
                sbSQL.AppendLine(" MAX(ResultType) AS DataType,")
                sbSQL.AppendLine(" COUNT(1) AS [Count],")

                sbSQL.AppendLine(" MAX(CASE ResultType WHEN " & CStr(CInt(DataType.number)) & " THEN CAST(SUBSTRING(Result, 1, DATALENGTH(Result)) AS INT) ELSE 0 END) AS NumberMaximum,")
                sbSQL.AppendLine(" MIN(CASE ResultType WHEN " & CStr(CInt(DataType.number)) & " THEN CAST(SUBSTRING(Result, 1, DATALENGTH(Result)) AS INT) ELSE 0 END)  AS NumberMinimum,")
                sbSQL.AppendLine(" SUM(CASE ResultType WHEN " & CStr(CInt(DataType.number)) & " THEN CAST(SUBSTRING(Result, 1, DATALENGTH(Result)) AS INT) ELSE 0 END)  AS NumberSum,")

                sbSQL.AppendLine(" SUM(CASE ResultType WHEN " & CStr(CInt(DataType.flag)) & " THEN CASE SUBSTRING(Result, 1, DATALENGTH(Result)) WHEN 'True' THEN 1 ELSE 0 END ELSE 0 END) AS TrueCount,")
                sbSQL.AppendLine(" SUM(CASE ResultType WHEN " & CStr(CInt(DataType.flag)) & " THEN CASE SUBSTRING(Result, 1, DATALENGTH(Result)) WHEN 'False' THEN 1 ELSE 0 END ELSE 0 END) AS FalseCount,")

                'Assuming date results like this: 01/06/2007 16:32:55 [UTC:2007-06-01 15:32:55Z]
                sbSQL.AppendLine(" MAX(CASE ResultType WHEN " & CStr(CInt(DataType.datetime)) & " THEN CAST(SUBSTRING(Result, PATINDEX('%UTC:%',Result) + 4, 19) AS DATETIME) ELSE 0 END) AS DateTimeMaximum,")
                sbSQL.AppendLine(" MIN(CASE ResultType WHEN " & CStr(CInt(DataType.datetime)) & " THEN CAST(SUBSTRING(Result, PATINDEX('%UTC:%',Result) + 4, 19) AS DATETIME) ELSE 0 END)  AS DateTimeMinimum,")

                sbSQL.AppendLine(" MAX(CASE ResultType WHEN " & CStr(CInt(DataType.date)) & " THEN CAST(SUBSTRING(Result, PATINDEX('%UTC:%',Result) + 4, 19) AS DATETIME) ELSE '' END) AS DateMaximum,")
                sbSQL.AppendLine(" MIN(CASE ResultType WHEN " & CStr(CInt(DataType.date)) & " THEN CAST(SUBSTRING(Result, PATINDEX('%UTC:%',Result) + 4, 19) AS DATETIME) ELSE '' END)  AS DateMinimum,")

                sbSQL.AppendLine(" MAX(CASE ResultType WHEN " & CStr(CInt(DataType.time)) & " THEN SUBSTRING(Result, 1, DATALENGTH(Result)) ELSE '' END) AS TimeMaximum,")
                sbSQL.AppendLine(" MIN(CASE ResultType WHEN " & CStr(CInt(DataType.time)) & " THEN SUBSTRING(Result, 1, DATALENGTH(Result)) ELSE '' END)  AS TimeMinimum")

                sbSQL.AppendFormat(" FROM {0} ", ValidateTableName(con, slogtable))
                sbSQL.AppendLine(" WHERE StageType = " & CStr(CInt(StageTypes.Calculation)))

                sbSQL.AppendLine(" AND " & MIGetSessionClause(sessions))
                If stages IsNot Nothing Then sbSQL.AppendLine(" AND " & MIGetStageClause(stages))
                sbSQL.AppendLine(" GROUP BY CAST(StageID AS VARCHAR(36))")

                Using cmd As New SqlCommand(sbSQL.ToString)
                    con.LoadDataTable(cmd, res)
                End Using
            Next
        End Using
        Return res

    End Function

    ''' <summary>
    ''' Get MI data relating to stages that have a start and end date.
    ''' </summary>
    ''' <param name="stageType">The stage type</param>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <returns>A DataTable containing the requested data</returns>
    <SecuredMethod(True)>
    Public Function MIGetReturnStageData(ByVal stageType As StageTypes, ByVal sessions() As Integer, ByVal stages() As Guid) As DataTable Implements IServer.MIGetReturnStageData
        CheckPermissions()

        Dim res As New DataTable()
        res.TableName = "MIGetReturnStageDataTable"
        res.Locale = CultureInfo.InvariantCulture
        Using con = GetConnection()
            For Each slogtable As String In [Enum].GetNames(GetType(SessionLogTables))
                Dim sbSQL As New System.Text.StringBuilder

                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine(" CAST(stageid AS VARCHAR(36)) AS StageID,")
                sbSQL.AppendLine(" COUNT(1) AS [Count],")
                sbSQL.AppendLine(" SUM(DATEDIFF(ss, StartDateTime, ISNULL(EndDateTime, StartDateTime))) AS [Total],")
                sbSQL.AppendLine(" AVG(DATEDIFF(ss, StartDateTime, ISNULL(EndDateTime, StartDateTime))) AS [Average]")
                sbSQL.AppendFormat(" FROM {0} ", ValidateTableName(con, slogtable))
                sbSQL.AppendLine(" WHERE StageType = " & CStr(CInt(stageType)))

                sbSQL.AppendLine(" AND " & MIGetSessionClause(sessions))
                If stages IsNot Nothing Then sbSQL.AppendLine(" AND " & MIGetStageClause(stages))
                sbSQL.AppendLine(" GROUP BY CAST(StageID AS VARCHAR(36))")

                Using cmd As New SqlCommand(sbSQL.ToString)
                    con.LoadDataTable(cmd, res)
                End Using
            Next
        End Using
        Return res

    End Function

    ''' <summary>
    ''' Get MI data relating to choice start stages
    ''' </summary>
    ''' <param name="stageType">The stage type - can be either ChoiceStart or
    ''' WaitStart</param>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <returns>A DataTable containing the requested data</returns>
    <SecuredMethod(True)>
    Public Function MIGetChoiceStartData(ByVal stageType As StageTypes, ByVal sessions() As Integer, ByVal stages() As Guid) As DataTable Implements IServer.MIGetChoiceStartData
        CheckPermissions()

        Dim res As New DataTable()
        res.TableName = "MIGetChoiceStartDataTable"
        res.Locale = CultureInfo.InvariantCulture
        Using con = GetConnection()
            For Each slogtable As String In [Enum].GetNames(GetType(SessionLogTables))
                Dim sbSQL As New System.Text.StringBuilder

                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine(" CAST(stageid AS VARCHAR(36)) AS StageID,")
                sbSQL.AppendLine(" SUBSTRING(Result, 2, PATINDEX('%) %',Result)-2) AS [Number],")
                sbSQL.AppendLine(" COUNT(1) AS [TrueCount]")
                sbSQL.AppendFormat(" FROM {0} ", ValidateTableName(con, slogtable))
                sbSQL.AppendLine(" WHERE StageType = " & CStr(CInt(stageType)))
                sbSQL.AppendLine(" AND " & MIGetSessionClause(sessions))
                If stages IsNot Nothing Then sbSQL.AppendLine(" AND " & MIGetStageClause(stages))

                sbSQL.AppendLine(" GROUP BY CAST(stageid AS VARCHAR(36)),")
                sbSQL.AppendLine(" SUBSTRING(Result, 2, PATINDEX('%) %',Result)-2)")

                Using cmd As New SqlCommand(sbSQL.ToString)
                    con.LoadDataTable(cmd, res)
                End Using
            Next
        End Using
        Return res

    End Function

    ''' <summary>
    ''' Get MI data relating to a generic stage type.
    ''' </summary>
    ''' <param name="stageType">The stage type</param>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <returns>A DataTable containing the requested data</returns>
    <SecuredMethod(True)>
    Public Function MIGetStageData(ByVal stageType As StageTypes, ByVal sessions() As Integer, ByVal stages() As Guid) As DataTable Implements IServer.MIGetStageData
        CheckPermissions()

        Dim res As New DataTable()
        res.TableName = "MIGetStageDataTable"
        res.Locale = CultureInfo.InvariantCulture
        Using con = GetConnection()
            For Each slogtable As String In [Enum].GetNames(GetType(SessionLogTables))
                Dim sbSQL As New System.Text.StringBuilder
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine(" CAST(stageid AS VARCHAR(36)) AS StageID,")
                sbSQL.AppendLine(" COUNT(1) AS [Count]")
                sbSQL.AppendFormat(" FROM {0} ", ValidateTableName(con, slogtable))
                sbSQL.AppendLine(" WHERE StageType = " & CStr(CInt(stageType)))
                sbSQL.AppendLine(" AND " & MIGetSessionClause(sessions))
                If stages IsNot Nothing Then sbSQL.AppendLine(" AND " & MIGetStageClause(stages))
                sbSQL.AppendLine(" GROUP BY CAST(stageid AS VARCHAR(36))")

                Using cmd As New SqlCommand(sbSQL.ToString)
                    con.LoadDataTable(cmd, res)
                End Using
            Next
        End Using
        Return res

    End Function

    ''' <summary>
    ''' Queries the DB for sessions.
    ''' </summary>
    ''' <param name="dStart">The start date</param>
    ''' <param name="dEnd">The end date</param>
    ''' <param name="resIDs">A list of Resource IDs to restrict to, or Nothing for all</param>
    ''' <param name="debugSessions">Whether to use debug sessions or normal sessions</param>
    ''' <param name="objectStudio">True for Object Studio mode, False for Process
    ''' Studio</param>
    ''' <returns>A DataTable of sessions</returns>
    <SecuredMethod(True)>
    Public Function MIReadSessions(ByVal procid As Guid, ByVal dStart As Date, ByVal dEnd As Date, ByVal resIDs As ICollection(Of Guid), ByVal debugSessions As Boolean, ByVal objectStudio As Boolean) As DataTable Implements IServer.MIReadSessions
        CheckPermissions()

        Dim sql As String

        Using cmd As New SqlCommand()
            Using con = GetConnection()

                If objectStudio Then

                    'Get the name of this business object.
                    sql = "DECLARE @ObjectName VARCHAR(128)" _
                     & " SELECT @ObjectName=Name FROM BPAProcess WHERE ProcessID='" & procid.ToString() & "'"

                    'Get the sessions where this business object is referenced in the process xml.
                    sql &= " SELECT" _
                    & " SE.SessionNumber, SE.SessionID, PR.Name AS Process, RE.Name AS Resource, SE.StatusID AS StatusID, SE.StartDateTime, SE.EndDateTime" _
                    & " FROM BPVSession SE" _
                    & " INNER JOIN BPAResource RE ON SE.StarterResourceID = RE.ResourceID" _
                    & " INNER JOIN BPAProcess PR ON SE.ProcessID = PR.ProcessID" _
                    & " INNER JOIN BPAProcessNameDependency DP ON PR.ProcessID = DP.ProcessID" _
                    & " WHERE " _
                    & " SE.StartDateTime >= @StartDateTime "
                    cmd.Parameters.AddWithValue("@StartDateTime", clsDBConnection.UtilDateToSqlDate(dStart))
                    If Not debugSessions Then
                        sql &= " AND SE.EndDateTime <= @EndDateTime "
                        cmd.Parameters.AddWithValue("@EndDateTime", clsDBConnection.UtilDateToSqlDate(dEnd))
                    End If

                    sql &= " AND DP.refProcessName=@ObjectName"

                Else

                    'Get the sessions using this process or where this process is referenced as a subprocess.
                    sql = "SELECT " _
                    & " SE.SessionNumber, SE.SessionID, PR.Name AS Process, RE.Name AS Resource, SE.StatusID AS StatusID, SE.StartDateTime, SE.EndDateTime" _
                    & " FROM BPVSession SE" _
                    & " INNER JOIN BPAResource RE ON SE.StarterResourceID = RE.ResourceID" _
                    & " INNER JOIN BPAProcess PR ON SE.ProcessID = PR.ProcessID" _
                    & " LEFT OUTER JOIN BPAProcessIDDependency DP ON PR.ProcessID = DP.ProcessID" _
                    & " WHERE " _
                    & " SE.StartDateTime >= @StartDateTime "
                    cmd.Parameters.AddWithValue("@StartDateTime", clsDBConnection.UtilDateToSqlDate(dStart))
                    If Not debugSessions Then
                        sql &= " AND SE.EndDateTime <= @EndDateTime "
                        cmd.Parameters.AddWithValue("@EndDateTime", clsDBConnection.UtilDateToSqlDate(dEnd))
                    End If

                    sql &= " AND (SE.ProcessID = '" & procid.ToString() & "'"
                    sql &= " OR DP.refProcessID = '" & procid.ToString() & "')"
                End If

                If resIDs IsNot Nothing Then
                    sql &= " AND SE.StarterResourceID IN ("
                    sql &= String.Join(",", resIDs.Select(Function(x) "'" & x.ToString() & "'").ToList())
                    sql &= ")"
                End If

                If debugSessions Then
                    sql &= " AND SE.StatusID = 5"
                Else
                    sql &= " AND SE.StatusID IN (2,3,4)"
                End If

                Try
                    cmd.CommandText = sql
                    Return con.ExecuteReturnDataTable(cmd)
                Catch ex As Exception
                    Throw New InvalidOperationException(String.Format(My.Resources.clsServer_FailedToReadSessionsFromDatabase0, ex.Message))
                End Try
            End Using
        End Using
    End Function

#End Region

#Region "Reporting"

    ''' <summary>
    ''' Returns MI reporting configuration settings
    ''' </summary>
    ''' <param name="enabled">MI Enabled flag</param>
    ''' <param name="autoRefresh">Refresh statistics automatically via scheduler</param>
    ''' <param name="refreshAt">Daily refresh time</param>
    ''' <param name="lastRefreshed">Date/time of last refresh</param>
    ''' <param name="keepDaily">Daily statistics retention period (days)</param>
    ''' <param name="keepMonthly">Monthly statistics retention period (months)</param>
    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Sub GetMIConfig(ByRef enabled As Boolean, ByRef autoRefresh As Boolean,
                           ByRef refreshAt As Date, ByRef lastRefreshed As Date,
                           ByRef keepDaily As Integer, ByRef keepMonthly As Integer) Implements IServer.GetMIConfig
        CheckPermissions()
        Using con = GetConnection()
            GetMIConfig(con, enabled, autoRefresh, refreshAt, lastRefreshed, keepDaily, keepMonthly)
        End Using
    End Sub

    ''' <summary>
    ''' Returns MI reporting configuration settings
    ''' </summary>
    ''' <param name="enabled">MI Enabled flag</param>
    ''' <param name="autoRefresh">Refresh statistics automatically via scheduler</param>
    ''' <param name="refreshAt">Daily refresh time</param>
    ''' <param name="lastRefreshed">Date/time of last refresh</param>
    ''' <param name="keepDaily">Daily statistics retention period (days)</param>
    ''' <param name="keepMonthly">Monthly statistics retention period (months)</param>
    ''' <param name="connection">The database connection to use</param>
    Private Sub GetMIConfig(connection As IDatabaseConnection,
                        ByRef enabled As Boolean, ByRef autoRefresh As Boolean,
                           ByRef refreshAt As Date, ByRef lastRefreshed As Date,
                           ByRef keepDaily As Integer, ByRef keepMonthly As Integer)
        Using command As New SqlCommand("select mienabled, autorefresh, refreshat, lastrefresh, dailyfor, monthlyfor from BPAMIControl where id=1")
            Using reader = connection.ExecuteReturnDataReader(command)
                Dim provider As New ReaderDataProvider(reader)
                reader.Read()

                enabled = provider.GetValue("mienabled", False)
                If Licensing.License.CanUse(LicenseUse.BPServer) Then
                    autoRefresh = provider.GetValue("autorefresh", False)
                Else
                    autoRefresh = False
                End If
                refreshAt = provider.GetValue("refreshat", Date.MinValue)
                lastRefreshed = provider.GetValue("lastrefresh", Date.MinValue)
                keepDaily = provider.GetValue("dailyfor", 0)
                keepMonthly = provider.GetValue("monthlyfor", 0)
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Handles request from BPServer to refresh the MI data.
    ''' </summary>
    ''' <returns>True if refresh took place</returns>
    <SecuredMethod(True)>
    Public Function MICheckAutoRefresh() As Boolean Implements IServer.MICheckAutoRefresh
        CheckPermissions()
        Dim enabled As Boolean, autoRefresh As Boolean
        Dim refreshAt As DateTime, nextRefresh As DateTime
        Using connection = GetConnection()

            'Nothing to do if auto refresh not required
            GetMIConfig(connection, enabled, autoRefresh, refreshAt, nextRefresh, Nothing, Nothing)
            If Not enabled OrElse Not autoRefresh Then Return False

            'Work out next refresh time
            nextRefresh = DateAdd(DateInterval.Day, 2, nextRefresh)
            nextRefresh = nextRefresh.Add(New TimeSpan(refreshAt.Hour, refreshAt.Minute, refreshAt.Second))

            'Initiate the refresh if we've reached the next refresh time
            If nextRefresh > Date.Now Then Return False

            Dim expiryTimeInSeconds As Integer = GetIntPref(MIReportingEnvironmentLockExpiryKey)

            Dim token = GetEnvironmentLockWithForceReleaseOfExpired(RefreshMIDataEnvLockName, expiryTimeInSeconds)
            If Not String.IsNullOrEmpty(token) Then

                Dim commandTimeoutTimeInSeconds As Integer = GetIntPref(MIReportingCommandTimeoutKey)

                If commandTimeoutTimeInSeconds > expiryTimeInSeconds Then
                    commandTimeoutTimeInSeconds = expiryTimeInSeconds
                    Log.Warn("The {0} setting value is greater than the value of the {1} setting. 
                                    This is not allowed as it may cause unexpected behaviour. The CommandTimeout has been set to {2} seconds",
                             MIReportingCommandTimeoutKey, MIReportingEnvironmentLockExpiryKey, expiryTimeInSeconds)
                End If

                Try
                    Dim cmd As New SqlCommand("usp_RefreshMI")
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandTimeout = commandTimeoutTimeInSeconds
                    connection.Execute(cmd)

                    Return True
                Finally
                    ReleaseEnvLock(RefreshMIDataEnvLockName, token, Environment.MachineName, Nothing, True)
                End Try
            End If
        End Using

        Return False
    End Function

    ''' <summary>
    ''' Get the amount of time the MI refresh has been locked for
    ''' </summary>
    ''' <returns>Returns Timespan.Zero if not locked. Timespan.MinValue if unable to get 'lastrefresh' out of the sql provider.</returns>
    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Function MIGetTimeLocked() As TimeSpan Implements IServer.MIGetTimeLocked
        CheckPermissions()
        Using con = GetConnection()
            Using cmd As New SqlCommand(
                "select lastrefresh from BPAMIControl where id = 1 and refreshinprogress = 1")

                Using reader = con.ExecuteReturnDataReader(cmd)
                    Dim provider As New ReaderDataProvider(reader)

                    If Not reader.Read() Then Return TimeSpan.Zero

                    Dim lastrefresh = provider.GetValue("lastrefresh", Date.MinValue)
                    Return If(lastrefresh = Date.MinValue, TimeSpan.MinValue, Date.UtcNow - lastrefresh)
                End Using
            End Using
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function IsMIReportingEnabled() As Boolean Implements IServer.IsMIReportingEnabled
        CheckPermissions()
        Using connection = GetConnection()
            Return IsMIReportingEnabled(connection)
        End Using
    End Function

    Private Function IsMIReportingEnabled(connection As IDatabaseConnection) As Boolean
        Return mIsMIReportingEnabledCache.Value.GetValue(mIsMIReportingEnabledCacheKey, Function() mCacheDataProvider.IsMIReportingEnabled(connection))
    End Function

    ''' <summary>
    ''' Saves the MI reporting configuration settings
    ''' </summary>
    ''' <param name="enabled">MI Enabled flag</param>
    ''' <param name="autoRefresh">Refresh statistics automatically via scheduler</param>
    ''' <param name="refreshAt">Daily refresh time</param>
    ''' <param name="keepDaily">Daily statistics retention period (days)</param>
    ''' <param name="keepMonthly">Monthly statistics retention period (months)</param>
    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Sub SetMIConfig(enabled As Boolean, autoRefresh As Boolean, refreshAt As Date,
                           keepDaily As Integer, keepMonthly As Integer) Implements IServer.SetMIConfig
        CheckPermissions()
        Dim sb As New StringBuilder("update BPAMIControl set mienabled=@enabled")
        If enabled Then
            sb.Append(", autorefresh=@auto, refreshat=@at, dailyfor=@daily, monthlyfor=@monthly")
        End If
        sb.Append(" where id=1")

        Using con = GetConnection()
            Dim cmd As New SqlCommand(sb.ToString())
            With cmd.Parameters
                .AddWithValue("@enabled", enabled)
                If enabled Then
                    .AddWithValue("@auto", autoRefresh)
                    .AddWithValue("@at", refreshAt)
                    .AddWithValue("@daily", keepDaily)
                    .AddWithValue("@monthly", keepMonthly)
                End If
            End With
            con.BeginTransaction()
            con.Execute(cmd)

            'Audit changes
            sb.Clear()
            sb.Append(My.Resources.clsServer_SetMIConfig_MIEnabled & enabled.ToString())
            If enabled Then
                sb.Append(My.Resources.clsServer_SetMIConfig_UseBPServer & autoRefresh.ToString())
                If autoRefresh Then sb.Append(My.Resources.clsServer_SetMIConfig_RefreshAt & refreshAt.ToLongTimeString())
                sb.Append(My.Resources.clsServer_SetMIConfig_KeepDailyStatsFor & keepDaily)
                sb.Append(My.Resources.clsServer_SetMIConfig_KeepMonthlyStatsFor & keepMonthly)
            End If
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyReporting, sb.ToString())
            con.CommitTransaction()
            InvalidateCaches()
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.System.Reporting)>
    Public Sub ResetMIRefreshLock() Implements IServer.ResetMIRefreshLock
        CheckPermissions()

        Using con = GetConnection()
            Using cmd As New SqlCommand(
                $"update BPAMIControl 
                set refreshinprogress = 0
                where id = 1")

                con.BeginTransaction()
                con.Execute(cmd)
            End Using

            AuditRecordSysConfigEvent(con, SysConfEventCode.ReleaseMIRefreshLock,
                                      String.Format(My.Resources.EventCodeAttribute_TheUser0ManuallyReleasedTheMIRefreshLock, User.CurrentName))

            con.CommitTransaction()
            InvalidateCaches()
        End Using
    End Sub

    ''' <summary>
    ''' Logs session started event (for MI purposes).
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="sessID">The ID of the session that started</param>
    Private Sub LogMISessionStart(con As IDatabaseConnection, sessID As Guid)
        If IsMIReportingEnabled(con) Then
            Try
                Const sql As String = "
                if not exists (select 1 from BPMIUtilisationShadow where sessionid = @sessID) 
                insert into BPMIUtilisationShadow (
                    sessionid,
                    resourceid,
                    processid,
                    startdatetime)
                select
                    ses.sessionid,
                    ses.starterresourceid,
                    ses.processid,
                    ses.startdatetime
                from BPASession ses
                where ses.sessionid=@sessID"

                Dim cmd As New SqlCommand(sql)
                cmd.Parameters.AddWithValue("@sessID", sessID)
                con.Execute(cmd)
            Catch ex As Exception
                Log.Error(ex, "Error logging session started event")
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Logs session finished event (for MI purposes).
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="sessID">The ID of the session that finished</param>
    Private Sub LogMISessionEnd(con As IDatabaseConnection, sessID As Guid)
        If IsMIReportingEnabled(con) Then
            Try
                Const sql As String = "
                update sdw set
                    enddatetime=ses.enddatetime
                from BPMIUtilisationShadow sdw
                    inner join BPASession ses on ses.sessionid=sdw.sessionid
                where sdw.sessionid=@sessID"

                Dim cmd As New SqlCommand(sql)
                cmd.Parameters.AddWithValue("@sessID", sessID)
                con.Execute(cmd)
            Catch ex As Exception
                Log.Error(ex, "Error logging session finished event")
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Logs work item event (for MI purposes).
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="idents">Work item identifiers</param>
    ''' <param name="op">The type of event</param>
    Private Sub LogMIQueueItemEvents(con As IDatabaseConnection,
     idents As ICollection(Of Long), op As WorkQueueOperation)
        If IsMIReportingEnabled(con) Then
            Try
                If op = WorkQueueOperation.ItemLocked _
                    OrElse op = WorkQueueOperation.ItemDeleted Then Exit Sub

                Dim cmd As New SqlCommand()
                cmd.Parameters.AddWithValue("@op", op)

                Const sql As String = "
                insert into BPMIProductivityShadow (
                    queueident,
                    itemid,
                    eventid,
                    eventdatetime,
                    worktime,
                    elapsedtime,
                    attempt)
                select
                    i.queueident,
                    i.id,
                    @op,
                    GETUTCDATE(),
                    i.worktime,
                    case when i.finished is not null then DATEDIFF(SECOND, i.loaded, i.finished) else 0 end,
                    i.attempt
                from BPAWorkQueueItem i
                where i.ident in ("

                UpdateMultipleIds(con, cmd, idents, "id", sql)
            Catch ex As Exception
                Log.Error(ex, "Error logging MI queue item event.")
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Logs work item event (for MI purposes).
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="ids">Work items IDs</param>
    ''' <param name="op">The type of event</param>
    Private Sub LogMIQueueItemEvents(con As IDatabaseConnection,
     ids As ICollection(Of Guid), op As WorkQueueOperation)
        If IsMIReportingEnabled(con) Then
            Try
                If op = WorkQueueOperation.ItemLocked _
                    OrElse op = WorkQueueOperation.ItemDeleted Then Exit Sub

                Dim cmd As New SqlCommand()
                cmd.Parameters.AddWithValue("@op", op)

                Const sql As String = "
                insert into BPMIProductivityShadow (
                    queueident,
                    itemid,
                    eventid,
                    eventdatetime,
                    worktime,
                    elapsedtime,
                    attempt)
                select
                    i.queueident,
                    i.id,
                    @op,
                    GETUTCDATE(),
                    i.worktime,
                    case when i.finished Is Not null then DATEDIFF(SECOND, i.loaded, i.finished) Else 0 End,
                    i.attempt
                from BPAWorkQueueItem i
                    left join BPAWorkQueueItem inext on i.id=inext.id and inext.attempt=i.attempt+1
                where inext.id is null and i.id in ("

                UpdateMultipleIds(con, cmd, ids, "id", sql)
            Catch ex As Exception
                Log.Error(ex, "Error logging MI queue item event.")
            End Try
        End If
    End Sub

    Private Sub LogMIQueueItemDeletion(connection As IDatabaseConnection, idsToDelete As ICollection(Of Guid))
        If IsMIReportingEnabled(connection) Then
            Const sql As String = "
                insert into BPMIProductivityShadow
                    (queueident, itemid, eventid, eventdatetime, worktime, elapsedtime, attempt, statewhendeleted)
                select
                    i.queueident, i.id, 7, GETUTCDATE(), 0, 0, 0,
                    case
                        when i.exception is not null then 5
                        when i.completed is not null then 4
                    else 1 end
                from BPAWorkQueueItem i
                where i.id in ("

            Try
                Using command = New SqlCommand()
                    UpdateMultipleIds(connection, command, idsToDelete, "id", sql)
                End Using
            Catch
                ' Ignore any exceptions
            End Try
        End If
    End Sub

#End Region

    ''' <summary>
    ''' Generates data for the elementusage report.
    ''' </summary>
    ''' <param name="processname">The name of the process to generate report data for</param>
    ''' <returns></returns>
    <SecuredMethod(True)>
    Public Function GetProcessElementUsageDetails(processname As String) As SortedDictionary(Of String, String) Implements IServer.GetProcessElementUsageDetails
        CheckPermissions()
        Using con = GetConnection()
            Return GetProcessElementUsageDetails(con, processname)
        End Using
    End Function

    Private Function GetProcessElementUsageDetails(con As IDatabaseConnection, processName As String) As SortedDictionary(Of String, String)

        Dim procid As Guid = GetProcessIDByName(con, processName, True)
        If procid = Guid.Empty Then
            Throw New InvalidOperationException(String.Format(My.Resources.clsServer_Process0NotFound, processName))
        End If

        Dim xml As String = GetProcessXML(con, procid)
        Dim proc As clsProcess

        Dim sErr = String.Empty

        proc = clsProcess.FromXML(clsGroupObjectDetails.Empty, xml, False, sErr)
        If proc Is Nothing Then
            Throw New InvalidOperationException(String.Format(My.Resources.clsServer_FailedToParseXMLForProcess0, sErr))
        End If
        If proc.ProcessType <> DiagramType.Object Then
            Throw New InvalidOperationException(My.Resources.clsServer_CanOnlyAnalyseElementUsageForABusinessObject)
        End If
        Dim data As List(Of ElementUsageInstance)
        data = proc.GetElementUsageDetails()

        'We now have the data - format it in CSV style and sort it...
        Dim lines As New SortedDictionary(Of String, String)
        For Each row As ElementUsageInstance In data
            Dim el As clsApplicationElement = proc.ApplicationDefinition.FindElement(row.ElementID)
            Dim element As String = el.FullPath
            Dim narrative As String = el.Narrative
            Dim desc As String = el.Description
            Dim page As String = proc.GetSubSheetName(proc.GetStage(row.StageID).GetSubSheetID())
            Dim stage As String = proc.GetStage(row.StageID).GetName()
            Dim ename As String = element
            element = """" & element.Replace("""", """""") & """"
            narrative = """" & narrative.Replace("""", """""") & """"
            desc = """" & desc.Replace("""", """""") & """"
            page = """" & page.Replace("""", """""") & """"
            stage = """" & stage.Replace("""", """""") & """"
            lines(ename) = processName & "," & page & "," & stage & "," & element & "," & narrative & "," & desc
        Next
        proc.Dispose()
        Return lines
    End Function

End Class
