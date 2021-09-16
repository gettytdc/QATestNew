Imports System.Data.SqlClient
Imports System.Globalization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Core.Xml
Imports BluePrism.Data
Imports BluePrism.DataPipeline
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Utilities.Functional
Imports Newtonsoft.Json

Partial Public Class clsServer

    ' This value is the maximum legnth that the json serializer can handle.
    Private Const MaxJsonLegnth As Integer = 2097152

    ''' <summary>
    ''' Returns whether session logging should write to the Unicode log table or not.
    ''' </summary>
    ''' <returns>True if unicode logging is enabled, otherwise false</returns>
    <SecuredMethod()>
    Public Function UnicodeLoggingEnabled() As Boolean Implements IServer.UnicodeLoggingEnabled
        CheckPermissions()
        Using con = GetConnection()
            Return UnicodeLoggingEnabled(con)
        End Using
    End Function

    ''' <summary>
    ''' Returns whether session logging should write to the Unicode log table or not.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <returns>True if unicode logging is enabled, otherwise false</returns>
    Private Function UnicodeLoggingEnabled(con As IDatabaseConnection) As Boolean
        Dim cmd As New SqlCommand("select unicodeLogging from BPASysConfig")
        Return IfNull(con.ExecuteReturnScalar(cmd), False)
    End Function

    ''' <summary>
    ''' Enables/disables unicode support for session logs
    ''' </summary>
    ''' <param name="enable">True to enable unicode, or False to disable it</param>
    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Sub UpdateUnicodeLogging(enable As Boolean) Implements IServer.UpdateUnicodeLogging
        CheckPermissions()
        Using con = GetConnection()
            'Check there are no pending or active sessions
            If CountConcurrentSessions(con, Nothing) <> 0 Then Throw New InvalidStateException(
                If(enable, My.Resources.clsServer_UnicodeLoggingCannotBeEnabledWhileThereArePendingOrRunningSessions,
                My.Resources.clsServer_UnicodeLoggingCannotBeDisabledWhileThereArePendingOrRunningSessions))

            'If disabling, check there are no rows in the Unicode table
            If Not enable AndAlso UnicodeLogsExist(con) Then Throw New InvalidStateException(
                My.Resources.clsServer_UnicodeLoggingCannotBeDisabledWhileThereAreUnicodeSessionsLogsPresentInTheTable)

            'Otherwise toggle the unicode logging flag
            con.BeginTransaction()
            Dim cmd As New SqlCommand("update BPASysConfig set unicodeLogging=@enable")
            cmd.Parameters.AddWithValue("@enable", enable)
            con.Execute(cmd)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyLogging,
                If(enable, My.Resources.clsServer_UnicodeLoggingEnabled, My.Resources.clsServer_UnicodeLoggingDisabled))
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Indicates whether or not any records exist in the Unicode session log table.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <returns>True if records exist, otherwise False</returns>
    Private Function UnicodeLogsExist(con As IDatabaseConnection) As Boolean
        Dim cmd As New SqlCommand("select top 1 1 from BPASessionLog_Unicode")
        Return IfNull(con.ExecuteReturnScalar(cmd), False)
    End Function

    ''' <summary>
    ''' Returns the number of rows in a given session
    ''' </summary>
    ''' <param name="sessNo">The session number</param>
    ''' <returns>The number of rows</returns>
    <SecuredMethod(Permission.SystemManager.Audit.ProcessLogs, Permission.SystemManager.Audit.BusinessObjectsLogs)>
    Public Function GetLogsCount(ByVal sessNo As Integer) As Integer Implements IServer.GetLogsCount
        CheckPermissions()
        Using con = GetConnection()
            Try
                Dim count As Integer = 0
                For Each slogtable As String In [Enum].GetNames(GetType(SessionLogTables))
                    Dim sql = String.Format("SELECT COUNT(SessionNumber) FROM {0} WHERE SessionNumber = @SessionNumber",
                                            ValidateTableName(con, slogtable))
                    Using cmd As New SqlCommand(sql)
                        cmd.Parameters.AddWithValue("@SessionNumber", sessNo)
                        count += CInt(con.ExecuteReturnScalar(cmd))
                    End Using
                Next
                Return count
            Catch
                Return 0
            End Try
        End Using

    End Function

    ''' <summary>
    ''' Gets a set of session log entries as text - actually a subsection of the
    ''' session log rather than the entire thing.
    ''' </summary>
    ''' <param name="sessNo">The session number of the session log required.</param>
    ''' <param name="startNo">The start of the session log to retrieve</param>
    ''' <param name="rowCount">The maximum number of rows to return</param>
    ''' <returns>A data table containing the required section of the specified
    ''' session log</returns>
    ''' <remarks>Apparnetly, log information is returned in a "minimal text format".
    ''' </remarks>
    <SecuredMethod(True)>
    Public Function GetLogsAsText(
     ByVal sessNo As Integer, ByVal startNo As Integer, ByVal rowCount As Integer) _
     As DataTable Implements IServer.GetLogsAsText
        CheckPermissions()
        ' Prepare the output data table
        Dim tab As New DataTable("Table") With {
            .Locale = CultureInfo.InvariantCulture
        }
        With tab.Columns
            .Add("Start", GetType(String))
            .Add("StartOffset", GetType(Integer))
            .Add("ObjectName", GetType(String))
            .Add("StageType", GetType(String))
            .Add("Text", GetType(String))
            .Add("Result", GetType(String))
            .Add("ResultType", GetType(String))
            .Add("ParameterXml", GetType(String))
            .Add("LogNumber", GetType(Integer))
        End With

        Using con = GetConnection()

            For Each slogtable As String In [Enum].GetNames(GetType(SessionLogTables))

                Dim sql = String.Format("SELECT * FROM (" &
                                        " SELECT " &
                                        "   logid," &
                                        "   startdatetime," &
                                        "   starttimezoneoffset," &
                                        "   stagetype," &
                                        "   objectname," &
                                        "   actionname," &
                                        "   processname," &
                                        "   pagename," &
                                        "   stagename," &
                                        "   result," &
                                        "   resulttype," &
                                        "   attributexml," &
                                        "   ROW_NUMBER() OVER (PARTITION BY sessionnumber ORDER BY logid) AS LogNumber" &
                                        " FROM {0} " &
                                        " WHERE sessionnumber = @sessno ) AS x" &
                                        " WHERE x.LogNumber BETWEEN @start AND @end ",
                                        ValidateTableName(con, slogtable))

                Using cmd As New SqlCommand(sql)
                    With cmd.Parameters
                        .AddWithValue("@sessno", sessNo)
                        .AddWithValue("@start", startNo)
                        .AddWithValue("@end", startNo + rowCount - 1)
                    End With

                    Using reader = con.ExecuteReturnDataReader(cmd)
                        Dim prov As New ReaderDataProvider(reader)
                        While reader.Read()
                            Dim row As DataRow = tab.NewRow()
                            With New clsSessionLogEntry(prov)
                                row("Start") = .StartDate
                                row("StageType") = .StageType.ToString().ToUpper()
                                row("ObjectName") = .ObjectName
                                row("Text") = .Text
                                If .Result <> "" Then row("Result") = .Result
                                row("ResultType") = .ResultTypeDisplay
                                row("ParameterXml") = .AttributeXml
                                row("LogNumber") = .LogNumber
                            End With
                            tab.Rows.Add(row)
                        End While

                    End Using
                End Using
            Next

        End Using

        clsDBConnection.SetUnspecifiedDateTime(tab)
        Return tab

    End Function

    <SecuredMethod(True)>
    Public Function GetSessionAttributeXml(sessionNumber As Integer, logId As Long) As String Implements IServer.GetSessionAttributeXml
        CheckPermissions()

        Using connection = GetConnection()
            Dim command = mDatabaseCommandFactory(
            "SELECT 
	            [attributexml] 
            FROM [BPASessionLog_NonUnicode] 
            WHERE [sessionnumber] = @sessionnumber and [logid] = @logid
            UNION
            SELECT
	            [attributexml]
            FROM [BPASessionLog_Unicode] 
            WHERE [sessionnumber] = @sessionnumber and [logid] = @logid
            ORDER BY [attributexml] DESC"
            )

            command.AddParameter("@sessionnumber", sessionNumber)
            command.AddParameter("@logid", logId)

            Using reader = connection.ExecuteReturnDataReader(command)
                If reader.Read() Then
                    Return If(reader.IsDBNull(0), String.Empty, reader.GetString(0))
                End If

                Return Nothing
            End Using
        End Using
    End Function
    ''' <summary>
    ''' Gets a set of session log entries as a subsection of the
    ''' session log with paging.
    ''' </summary>
    ''' <param name="sessNo">The session number from which the log entries should be drawn.</param>
    ''' <param name="sessionLogsParameters">The parameters defining pagination</param>
    ''' <returns>A collection of log entries</returns>
    <SecuredMethod(Permission.SystemManager.Audit.ProcessLogs, Permission.SystemManager.Audit.BusinessObjectsLogs)>
    Public Function GetLogs(ByVal sessNo As Integer, sessionLogsParameters As SessionLogsParameters) _
     As ICollection(Of clsSessionLogEntry) Implements IServer.GetLogs
        CheckPermissions()

        Dim logEntriesList = New List(Of clsSessionLogEntry)()
        Using con = GetConnection()

            For Each slogtable As String In [Enum].GetNames(GetType(SessionLogTables))

                Using sqlCommand = New SqlCommand()
                    Dim whereClauses = sessionLogsParameters.GetSqlWhereClauses(sqlCommand)
                    Dim itemsPerPage = sessionLogsParameters.ItemsPerPage
                    Dim tableName = ValidateTableName(con, slogtable)
                    Dim whereSql = String.Join("", whereClauses.Select(Function(x) $" and {x.SqlText}").ToArray())

                    Dim selectQuery = $"select TOP {itemsPerPage}
                                           l.logid,
                                           l.startdatetime,
                                           l.starttimezoneoffset,
                                           l.stagetype,
                                           l.objectname,
                                           l.actionname,
                                           l.processname,
                                           l.pagename,
                                           l.stagename,
                                           l.result,
                                           l.resulttype,
                                           l.attributexml,
                                           ROW_NUMBER() OVER (PARTITION BY l.sessionnumber ORDER BY l.logid) AS LogNumber
                                           from {tableName} as l
                                           join BPASession as s
                                               on l.sessionnumber = s.sessionnumber
                                           where l.sessionnumber = @sessno {whereSql} {MteSqlGenerator.MteToken}
                                           order by l.logid"

                    Const sessionTableAlias As String = "s"
                    Dim mteQueryBuilder = New MteSqlGenerator(selectQuery, sessionTableAlias, True)
                    Dim sqlWithMte = mteQueryBuilder.GetQueryAndSetParameters(mLoggedInUser, sqlCommand)

                    sqlCommand.CommandText = sqlWithMte
                    sqlCommand.Parameters.AddWithValue("@sessno", sessNo)
                    whereClauses.ForEach(Sub(x) x.Parameters.ForEach(Sub(param) sqlCommand.Parameters.Add(param)).Evaluate()).Evaluate()

                    Using reader = con.ExecuteReturnDataReader(sqlCommand)
                        Dim prov As New ReaderDataProvider(reader)
                        While reader.Read()
                            logEntriesList.Add(New clsSessionLogEntry(prov))
                        End While
                    End Using
                End Using
            Next
        End Using
        Return logEntriesList

    End Function

    ''' <summary>
    ''' Gets a subset of log entries in full verbose form
    ''' </summary>
    ''' <param name="sessNo">The session number from which the log entries should be
    ''' drawn.</param>
    ''' <param name="startNo">The start number of the session log entry to start
    ''' retrieving from.</param>
    ''' <param name="rowCount">The maximum number of rows to return from the session
    ''' log.</param>
    ''' <returns>A DataTable containing the full data from the required session log,
    ''' capturing all of the specified log entries.</returns>
    <SecuredMethod(Permission.SystemManager.Audit.ProcessLogs, Permission.SystemManager.Audit.BusinessObjectsLogs)>
    Public Function GetLogs(sessNo As Integer,
     startNo As Integer, rowCount As Integer) As DataTable Implements IServer.GetLogs
        CheckPermissions()
        Dim result As New DataTable()
        result.TableName = "session_logs"
        result.Locale = CultureInfo.InvariantCulture

        Using con = GetConnection()

            For Each slogtable As String In [Enum].GetNames(GetType(SessionLogTables))
                Dim sb As New StringBuilder(1024)
                sb.Append(
            " SELECT * FROM (" &
                 " SELECT " &
                 "     ROW_NUMBER() OVER (PARTITION BY sessionnumber ORDER BY logid) AS LogNumber," &
                 "     stageid as [StageID]," &
                 "     stagename as [StageName]," &
                 "     stagetype as [StageTypeValue]," &
                 "     case stagetype")
                For Each st As StageTypes In [Enum].GetValues(GetType(StageTypes))
                    sb.AppendFormat(" when {0} then '{1}'", CInt(st), st)
                Next
                sb.Append(
                 "     else '' end as [StageType]," &
                 "     processname as [Process]," &
                 "     pagename as [Page]," &
                 "     objectname as [Object]," &
                 "     actionname as [Action]," &
                 "     result as [Result]," &
                 "     resulttype as [ResultTypeValue]," &
                 "     case resulttype")
                For Each dt As DataType In [Enum].GetValues(GetType(DataType))
                    sb.AppendFormat(" when {0} then '{1}'", CInt(dt), dt)
                Next
                sb.Append(
                 "     else '' end as [ResultType]," &
                 "     startdatetime as [Resource Start]," &
                 "     starttimezoneoffset as [StartOffset]," &
                 "     enddatetime as [Resource End]," &
                 "     endtimezoneoffset as [EndOffset]," &
                 "     attributexml as [ParameterXml]," &
                 "     automateworkingset AS [Blue Prism Memory]," &
                 "     targetappname as [Target App ID]," &
                 "     targetappworkingset as [Target Memory]")
                sb.AppendFormat(" from {0} ", ValidateTableName(con, slogtable))
                sb.Append(
                    " WHERE sessionnumber = @sessno) AS x " &
                    " WHERE x.LogNumber between @start And @end "
                )

                Using cmd As New SqlCommand(sb.ToString())
                    With cmd.Parameters
                        .AddWithValue("@sessno", sessNo)
                        .AddWithValue("@start", startNo)
                        .AddWithValue("@end", startNo + rowCount - 1)
                    End With
                    con.LoadDataTable(cmd, result)
                End Using
            Next
        End Using

        Return result

    End Function


    ''' <summary>
    ''' Updates the end date and attribute xml or a log record.
    ''' </summary>
    ''' <param name="sessionNo">The session number</param>
    ''' <param name="logId">The log entry identifier</param>
    ''' <param name="attrXml">The XML describing the attributes for this log update;
    ''' empty implies no attributes to set.</param>
    ''' <param name="endDate">The datetime/offset that the session entry ended</param>
    ''' <param name="unicode">Whether to log in unicode table.</param>
    ''' <param name="resourceId">The resource id</param>
    ''' <param name="resourceName">The resource name</param>
    ''' <param name="processName">The process or subprocess name</param>
    ''' <param name="stageId">The stage ID</param>
    ''' <param name="stageName">The stage name</param>
    ''' <param name="stageType">The stage type</param>
    ''' <param name="sheetName">The sheet name</param>
    ''' <param name="objectName">The object name</param>
    ''' <param name="actionName">The action type</param>
    ''' <param name="startDate">The datetime/offset the stage started</param>
    <SecuredMethod(True)>
    Public Sub UpdateLog(sessionNo As Integer, logId As Long, attrXml As String,
                         endDate As DateTimeOffset, unicode As Boolean, resourceId As Guid, resourceName As String, processName As String,
                         stageId As Guid, stageName As String, stageType As StageTypes, sheetName As String, objectName As String,
                         actionName As String, startDate As DateTimeOffset) Implements IServer.UpdateLog
        CheckPermissions()
        Using con = GetConnection()
            Using cmd As New SqlCommand(
                String.Format("update {0} set enddatetime=@enddate, endtimezoneoffset=@endtimezoneoffset",
                                        If(unicode, "BPASessionLog_Unicode", "BPASessionLog_NonUnicode")))
                If attrXml <> "" Then
                    cmd.CommandText &= ", attributexml=@attrxml"
                    cmd.Parameters.AddWithValue("@attrxml", attrXml)
                    'Just update the end date.
                End If
                cmd.CommandText &= " where sessionnumber=@sessno AND logid=@logid"

                With cmd.Parameters
                    .AddWithValue("@sessno", sessionNo)
                    .AddWithValue("@logid", logId)
                    .AddWithValue("@enddate", endDate.DateTime)
                    .AddWithValue("@endtimezoneoffset", endDate.Offset.TotalSeconds)
                End With

                con.Execute(cmd)

                SendSessionLogsToDataGateways(sessionNo, resourceId, resourceName, stageId, stageName, stageType, "",
                                      DataType.unknown, startDate, attrXml, processName, sheetName, objectName,
                                      actionName, con, endDate)

            End Using
        End Using
    End Sub


    ''' <summary>
    ''' Creates a log record.
    ''' </summary>
    ''' <param name="iSessionNumber">The session number</param>
    ''' <param name="gResourceId">The resource id</param>
    ''' <param name="sResourceName">The resource name</param>
    ''' <param name="gStageId">The stage ID</param>
    ''' <param name="sStageName">The stage name</param>
    ''' <param name="iStageType">The stage type</param>
    ''' <param name="sResult">The result/message</param>
    ''' <param name="iResultType">The result data type</param>
    ''' <param name="sAttributeXML">The inputs or outputs</param>
    ''' <param name="sProcessName">The process or subprocess name</param>
    ''' <param name="sPageName">The page name</param>
    ''' <param name="sObjectName">The object name</param>
    ''' <param name="sActionName">The action name</param>
    ''' <param name="loginfo">Logging information</param>
    ''' <param name="startDate">The datetime/offset the stage started</param>
    ''' <returns>The sequence number for the log entry that can be used (together
    ''' with the session number) to identify the record created</returns>
    ''' <exception cref="Exception">If any other errors occur while attempting to
    ''' write the log entry to the database</exception>
    <SecuredMethod(True)>
    Public Function LogToDB(
                iSessionNumber As Integer,
                gResourceId As Guid,
                sResourceName As String,
                gStageId As Guid, sStageName As String,
                iStageType As StageTypes, sResult As String,
                iResultType As DataType, loginfo As LogInfo,
                startDate As DateTimeOffset,
                Optional ByVal sAttributeXML As String = "",
                Optional ByVal sProcessName As String = "",
                Optional ByVal sPageName As String = "",
                Optional ByVal sObjectName As String = "",
                Optional ByVal sActionName As String = "",
                Optional unicode As Boolean = False) As Long Implements IServer.LogToDB
        CheckPermissions()
        Using con = GetConnection()

            con.BeginTransaction()

            'These fields only serve to enhance log viewer
            'readability and can be safely truncated if necessary.
            If sStageName.Length > 128 Then _
             sStageName = sStageName.Substring(0, 125) & "..."

            If sProcessName.Length > 128 Then _
             sProcessName = sProcessName.Substring(0, 125) & "..."

            If sPageName.Length > 128 Then _
             sPageName = sPageName.Substring(0, 125) & "..."

            If sObjectName.Length > 128 Then _
             sObjectName = sObjectName.Substring(0, 125) & "..."

            If sActionName.Length > 128 Then _
             sActionName = sActionName.Substring(0, 125) & "..."

            Dim settings As DataPipelineSettings = GetDataAccesss(con).GetDataPipelineSettings()

            Dim result = 0L
            If settings.WriteSessionLogsToDatabase Then
                result = CLng(LogToDB(con,
                                 iSessionNumber,
                                 gStageId,
                                 sStageName,
                                 iStageType,
                                 sResult,
                                 iResultType,
                                 loginfo,
                                 startDate,
                                 sAttributeXML,
                                 sProcessName,
                                 sPageName,
                                 sObjectName,
                                 sActionName,
                                 unicode))
            End If

            'Do not want to send these StageTypes to data gateways here because these will be handled by the UpdateLog method which sends the end time for this stage type
            If iStageType = StageTypes.Action OrElse iStageType = StageTypes.SubSheet OrElse iStageType = StageTypes.Process Then
                con.CommitTransaction()
                Return result
            Else
                SendSessionLogsToDataGateways(iSessionNumber, gResourceId, sResourceName, gStageId, sStageName, iStageType, sResult,
                                          iResultType, startDate, sAttributeXML, sProcessName, sPageName, sObjectName,
                                          sActionName, con)

                con.CommitTransaction()
                Return result
            End If
        End Using
    End Function

    Private Sub SendSessionLogsToDataGateways(iSessionNumber As Integer, gResourceId As Guid, sResourceName As String, gStageId As Guid, sStageName As String, iStageType As StageTypes, sResult As String,
                                              iResultType As DataType, startDate As DateTimeOffset, sAttributeXML As String, sProcessName As String, sPageName As String, sObjectName As String,
                                              sActionName As String, con As IDatabaseConnection, Optional endDate? As DateTimeOffset = Nothing)
        If GetDataAccesss(con).GetDataPipelineSettings().SendSessionLogsToDataGateways Then

            Dim attributeJson As String = String.Empty

            If Not String.IsNullOrWhiteSpace(sAttributeXML) Then
                Dim doc = New ReadableXmlDocument()
                doc.LoadXml(sAttributeXML)
                attributeJson = JsonConvert.SerializeXmlNode(doc)
            End If

            Dim dataPipelineEvent = New DataPipelineEvent(EventType.SessionLog) With {
                .EventData = New Dictionary(Of String, Object) From {
                                {"SessionNumber", iSessionNumber},
                                {"ResourceId", gResourceId},
                                {"ResourceName", sResourceName},
                                {"StageId", gStageId},
                                {"StageName", sStageName},
                                {"StageType", iStageType.ToString()},
                                {"Result", sResult},
                                {"ResultType", iResultType.ToString()},
                                {"StartDate", startDate},
                                {"Attributes", attributeJson},
                                {"ProcessName", sProcessName},
                                {"PageName", sPageName},
                                {"ObjectName", sObjectName},
                                {"ActionName", sActionName}
                                }}

            If endDate IsNot Nothing Then
                dataPipelineEvent.EventData.Add("EndDate", endDate)
            End If

            mDataPipelinePublisher.PublishToDataPipeline(con, {dataPipelineEvent})
        End If
    End Sub

    Private Function LogToDB(
                con As IDatabaseConnection,
                iSessionNumber As Integer,
                gStageId As Guid, sStageName As String,
                iStageType As StageTypes, sResult As String,
                iResultType As DataType, loginfo As LogInfo,
                startDate As DateTimeOffset,
                Optional ByVal sAttributeXML As String = "",
                Optional ByVal sProcessName As String = "",
                Optional ByVal sPageName As String = "",
                Optional ByVal sObjectName As String = "",
                Optional ByVal sActionName As String = "",
                Optional unicode As Boolean = False) As Object

        Dim template = "
insert into {0}
(
    sessionnumber,
    startdatetime,
    starttimezoneoffset,
    stageid,
    stagetype,
    stagename,
    processname,
    pagename,
    objectname,
    actionname,
    resulttype,
    result,
    attributexml,
    automateworkingset,
    targetappname,
    targetappworkingset
)
output inserted.logid
values
(
    @sessionnumber,
    @startdatetime,
    @starttimezoneoffset,
    @stageid,
    @stagetype,
    @stagename,
    @processname,
    @pagename,
    @objectname,
    @actionname,
    @resulttype,
    @result,
    @attributexml,
    @automateworkingset,
    @targetappname,
    @targetappworkingset
)"

        Dim tableName = If(unicode, "BPASessionLog_Unicode", "BPASessionLog_NonUnicode")
        Dim sql = String.Format(template, tableName)
        Dim cmd As New SqlCommand(sql)

        With cmd.Parameters
            .AddWithValue("@sessionnumber", iSessionNumber)
            .AddWithValue("@startdatetime", IIf(startDate <> DateTimeOffset.MinValue, startDate.DateTime, DateTimeOffset.Now))
            .AddWithValue("@starttimezoneoffset", startDate.Offset.TotalSeconds)
            .AddWithValue("@stageid", gStageId)
            .AddWithValue("@stagetype", iStageType)
            .AddWithValue("@stagename", IIf(sStageName <> "", sStageName, DBNull.Value))
            .AddWithValue("@processname", IIf(sProcessName <> "", sProcessName, DBNull.Value))
            .AddWithValue("@pagename", IIf(sPageName <> "", sPageName, DBNull.Value))
            .AddWithValue("@objectname", IIf(sObjectName <> "", sObjectName, DBNull.Value))
            .AddWithValue("@actionname", IIf(sActionName <> "", sActionName, DBNull.Value))

            If iResultType <> DataType.unknown Then
                .AddWithValue("@resulttype", iResultType)
                .AddWithValue("@result", IIf(sResult <> "", sResult, DBNull.Value))
            Else
                .AddWithValue("@resulttype", DBNull.Value)
                .AddWithValue("@result", DBNull.Value)
            End If
            .AddWithValue("@attributexml", IIf(sAttributeXML <> "", sAttributeXML, DBNull.Value))

            If loginfo.AutomateWorkingSet <> 0 Then
                .AddWithValue("@automateworkingset", loginfo.AutomateWorkingSet)
            Else
                .AddWithValue("@automateworkingset", 0)
            End If

            If loginfo.TargetAppWorkingSet <> 0 Then
                Dim taname As String = loginfo.TargetAppName
                If taname IsNot Nothing AndAlso taname.Length > 32 Then taname = taname.Substring(0, 32)
                .AddWithValue("@targetappname", taname)
                .AddWithValue("@targetappworkingset", loginfo.TargetAppWorkingSet)
            Else
                .AddWithValue("@targetappname", DBNull.Value)
                .AddWithValue("@targetappworkingset", 0)
            End If
        End With

        Return con.ExecuteReturnScalar(cmd)
    End Function

    ''' <summary>
    ''' Search a single session log for the first occurrence of a value in any field
    ''' </summary>
    ''' <param name="sessNo">The session to search</param>
    ''' <param name="term">The value to seach for</param>
    ''' <returns>The logId of the first log row that
    ''' matches the search term, or -1 if no result is found.</returns>
    ''' <exception cref="Exception">If any errors occur</exception>
    <SecuredMethod(True)>
    Public Function SearchSession(ByVal sessNo As Integer, ByVal term As String) _
     As Long Implements IServer.SearchSession
        CheckPermissions()
        Return SearchSession(sessNo, New String() {
         "stagename", "processname", "pagename", "objectname",
         "actionname", "result", "attributexml"},
         term)
    End Function

    ''' <summary>
    ''' Search a single session log for the first occurrence of a value.
    ''' </summary>
    ''' <param name="sessNo">The session to search</param>
    ''' <param name="fields">The names of the fields to search</param>
    ''' <param name="term">The value to seach for</param>
    ''' <returns>The logId of the first log row that
    ''' matches the search term, or -1 if no result is found.</returns>
    ''' <exception cref="ArgumentNullException">If the given <paramref name="term"/>
    ''' is null or empty, or the <paramref name="fields"/> collection is null.
    ''' </exception>
    ''' <exception cref="EmptyCollectionException">If the <paramref name="fields"/>
    ''' collection was empty.</exception>
    ''' <exception cref="Exception">If any other errors occur</exception>
    <SecuredMethod(True)>
    Public Function SearchSession(ByVal sessNo As Integer,
     ByVal fields As ICollection(Of String), ByVal term As String) As Long Implements IServer.SearchSession
        CheckPermissions()
        If fields Is Nothing Then Throw New ArgumentNullException(NameOf(fields))
        If term = "" Then Throw New ArgumentNullException(NameOf(term))

        If fields.Count = 0 Then Throw New EmptyCollectionException(
         My.Resources.clsServer_YouMustProvideFieldsToSearchWithinTheSessionLog)

        term = term.Trim()

        Using con = GetConnection()

            For Each slogtable As String In [Enum].GetNames(GetType(SessionLogTables))

                Using cmd As New SqlCommand()
                    cmd.Parameters.AddWithValue("@sessno", sessNo)
                    AddVarChar(cmd.Parameters, "@term", "%" & term & "%")

                    Dim sb As New StringBuilder()
                    sb.Append(" select top 1 logid")
                    sb.AppendFormat(" from {0} ", ValidateTableName(con, slogtable))
                    sb.Append(" where sessionnumber = @sessno AND (")

                    Dim first As Boolean = True
                    Dim altCount As Integer = 0
                    For Each fld As String In fields
                        If first Then first = False Else sb.Append(" or ")
                        sb.Append(fld).Append(" like @term")

                        ' If it's a locale-sensitive field, check if the given value can
                        ' be parsed as a date/time or a number, and use the clsProcessValue's
                        ' internal format for it if it can.
                        Dim validFields As String() = {"result", "attributexml"}
                        If validFields.Contains(fld.ToLower()) Then
                            Dim dt As Date
                            Dim dec As Decimal
                            If Date.TryParse(
                             term, Nothing, DateTimeStyles.NoCurrentDateDefault, dt) Then
                                ' We have a potential date - go through the possible encodes
                                ' and add them to the search
                                For Each enc As String In clsProcessValue.GetDateEncodes(dt)
                                    ' If we're already searching for it, there's no need
                                    ' to add it again. Let's not make work for ourselves
                                    If enc.Contains(term) Then Continue For

                                    ' Otherwise, add this variation to search for too
                                    altCount += 1
                                    sb.AppendFormat(" or {0} like @alt{1}",
                                                    ValidateFieldName(con, slogtable, fld),
                                                    altCount)
                                    AddVarChar(
                                     cmd.Parameters, "@alt" & altCount, "%" & enc & "%")
                                Next

                            ElseIf Decimal.TryParse(term, dec) Then
                                ' We have a potential number - add the possible internal
                                ' encoding of the number
                                Dim enc As String = clsProcessValue.GetNumberEncode(dec)

                                ' If this is what we're already searching for, no need to
                                ' add it again.
                                If enc <> term Then
                                    altCount += 1
                                    sb.AppendFormat(" or {0} like @alt{1}",
                                                    ValidateFieldName(con, slogtable, fld),
                                                    altCount)
                                    AddVarChar(
                                     cmd.Parameters, "@alt" & altCount, "%" & enc & "%")

                                End If

                            End If

                        End If

                    Next
                    sb.Append(")")

                    cmd.CommandText = sb.ToString()

                    ' Set a higher command timeout than the default 30 seconds on this, since
                    ' it could take a long time on a large session. See bug #4227. In the
                    ' event that this is not sufficient, see my comment on that bug!
                    cmd.CommandTimeout = Options.Instance.SqlCommandTimeoutLong

                    Dim res = IfNull(con.ExecuteReturnScalar(cmd), -1L)
                    If res <> -1 Then Return res
                End Using
            Next
            Return -1

        End Using

    End Function

    <SecuredMethod(True)>
    Public Function LogIdToLogNumber(sessionNumber As Integer, logId As Long) As Integer Implements IServer.LogIdToLogNumber
        CheckPermissions()
        Using connection = GetConnection()
            Using cmd As New SqlCommand(
            " SELECT x.LogNumber FROM " &
            "    (SELECT logid,ROW_NUMBER() OVER (PARTITION BY sessionnumber ORDER BY logid) AS LogNumber " &
            "     FROM " & GetSessionLogTableName(connection, sessionNumber) &
            "     WHERE sessionnumber = @sessionNo) AS x " &
            " WHERE x.logid = @logId")
                With cmd.Parameters
                    .AddWithValue("@sessionNo", sessionNumber)
                    .AddWithValue("@logId", logId)
                End With
                Dim result = connection.ExecuteReturnScalar(cmd)
                Return CInt(result)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Reads a period of audit records from the database into a datatable.
    ''' The datatable has the following columns:
    ''' Time            (datetime)
    ''' Narrative       (string)
    ''' Comments        (string)
    ''' </summary>
    ''' <param name="startDateTime">The start date</param>
    ''' <param name="endDateTime">The end date</param>
    ''' <returns>The table of data containing the required audit logs.</returns>
    ''' <exception cref="Exception">If any errors occur while attempting to read
    ''' the audit log data</exception>
    <SecuredMethod(Permission.SystemManager.Audit.AuditLogs)>
    Public Function GetAuditLogsByDateRange(
     ByVal startDateTime As Date, ByVal endDateTime As Date) As DataTable Implements IServer.GetAuditLogsByDateRange
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             " select a.eventdatetime as Time," &
             "   a.sNarrative as Narrative," &
             "   a.comments as Comments" &
             " from BPAAuditEvents a" &
             " where a.eventdatetime between @startdate and @enddate" &
             " order by a.eventid"
            )

            With cmd.Parameters
                .AddWithValue("@startdate", startDateTime)
                .AddWithValue("@enddate", endDateTime)
            End With

            Return con.ExecuteReturnDataTable(cmd)
        End Using
    End Function

    ''' <summary>
    ''' Reads a period of audit records from the database into a datatable.
    ''' The datatable has the following columns:
    ''' Time            (datetime)
    ''' Narrative       (string)
    ''' Comments        (string)
    ''' </summary>
    ''' <param name="startDateTime">The start date</param>
    ''' <param name="endDateTime">The end date</param>
    ''' <param name="dtlogs">The datatable. May be null on return, particularly
    ''' if function returns false.</param>
    <SecuredMethod(Permission.SystemManager.Audit.AuditLogs)>
    Public Sub GetAuditLogData(startDateTime As Date, endDateTime As Date, ByRef dtlogs As DataTable) _
        Implements IServer.GetAuditLogData
        CheckPermissions()
        Try
            dtlogs = GetAuditLogsByDateRange(startDateTime, endDateTime)
        Catch ex As Exception
            Throw New BluePrismException(ex, ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Gets the xml of a past version of a process (or business object) from the
    ''' audit table.
    ''' </summary>
    ''' <param name="eventId">The eventID at which to retrieve the xml. The
    ''' data will be taken from the 'newxml' column recorded against this id.</param>
    ''' <param name="processId">The process id of the process (or busines object).
    ''' This is required for data integrity checks.</param>
    ''' <returns>Returns the xml of the requested process or an empty string if no
    ''' such event id was found in the audit event table.</returns>
    <SecuredMethod()>
    Public Function GetProcessHistoryXML(eventId As Integer, processId As Guid) As String _
    Implements IServer.GetProcessHistoryXML
        CheckPermissions({Permission.SystemManager.Processes.History, Permission.SystemManager.BusinessObjects.History}.Concat(
                         Permission.ProcessStudio.ImpliedViewProcess).Concat(
                         Permission.ObjectStudio.ImpliedViewBusinessObject).ToArray())
        Using con = GetConnection()
            CheckProcessOrObjectPermissions(con, processId, "view",
                                            Permission.ProcessStudio.ImpliedViewProcess,
                                            Permission.ObjectStudio.ImpliedViewBusinessObject)
            Return GetProcessHistoryXML(con, eventId, processId)
        End Using
    End Function

    ''' <summary>
    ''' Gets the xml of a past version of a process (or business object) from the
    ''' audit table.
    ''' </summary>
    ''' <param name="connection">The connection to the database to use</param>
    ''' <param name="eventId">The eventID at which to retrieve the xml. The
    ''' data will be taken from the 'newxml' column recorded against this id.</param>
    ''' <param name="processId">The process id of the process (or busines object).
    ''' This is required for data integrity checks.</param>
    ''' <returns>Returns the xml of the requested process or an empty string if no
    ''' such event id was found in the audit event table.</returns>
    Private Function GetProcessHistoryXML(
     connection As IDatabaseConnection, eventId As Integer, processId As Guid) As String
        Using command As New SqlCommand(
            " select newxml from BPAAuditEvents" &
            " where gTgtProcID = @processID AND eventid = @eventId")

            With command.Parameters
                .AddWithValue("@eventid", eventId)
                .AddWithValue("@processid", processId)
            End With
            Return IfNull(connection.ExecuteReturnScalar(command), "")
        End Using
    End Function

    ''' <summary>
    ''' Populates a datatable with a list of versions of a specific Automate Process.
    ''' Called by ctlLogs.PopulateHistoryListView to get datetimes at which a specific
    ''' process was modified
    ''' </summary>
    ''' <param name="procId">The ID of the process of interest</param>
    ''' <returns>The datatable to be populated. Columns will be EventDateTime,
    ''' EventID, sCode, username, EditSummary, ordered by eventdatetime descending.
    ''' </returns>
    <SecuredMethod(Permission.SystemManager.Processes.History, Permission.SystemManager.BusinessObjects.History, Permission.ProcessStudio.GroupName, Permission.ObjectStudio.GroupName)>
    Public Function GetProcessHistoryLog(ByVal procId As Guid) As DataTable Implements IServer.GetProcessHistoryLog
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             " select " &
             "   e.eventdatetime," &
             "   e.eventid," &
             "   e.scode," &
             "   case u.isdeleted" &
             "     when 1 then u.username + ' (deleted)'" &
             "     else u.username" &
             "   end as username," &
             "   e.editsummary," &
             "   case " &
             "     when e.newxml is null then 0" &
             "     else 1" &
             "   end as xmlavailable" &
             " from BPAAuditEvents e" &
             "   left join BPAUser u on e.gsrcuserid = u.userid" &
             " where gtgtprocid = @procid" &
             " order by eventdatetime desc")
            cmd.Parameters.AddWithValue("@procid", procId)
            Return con.ExecuteReturnDataTable(cmd)
        End Using

    End Function

    ''' <summary>
    ''' Gets the user preference for the hidden columns in the Log Viewer.
    ''' </summary>
    <SecuredMethod(Permission.SystemManager.Audit.ProcessLogs, Permission.SystemManager.Audit.BusinessObjectsLogs)>
    Public Function GetLogViewerHiddenColumns() As Integer Implements IServer.GetLogViewerHiddenColumns
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT ISNULL(LogViewerHiddenColumns, -1) FROM BPAUser WHERE UserID = @UserID")
            With cmd.Parameters
                .AddWithValue("@UserID", GetLoggedInUserId())
            End With
            Return CInt(con.ExecuteReturnScalar(cmd))
        End Using
    End Function

    ''' <summary>
    ''' Sets the user preference for the hidden columns in the Log Viewer.
    ''' </summary>
    <SecuredMethod(True)>
    Public Sub SetLogViewerHiddenColumns(iColumns As Integer) Implements IServer.SetLogViewerHiddenColumns
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("UPDATE BPAUser SET LogViewerHiddenColumns = @HiddenColumns WHERE UserID = @UserID")
            With cmd.Parameters
                .AddWithValue("@HiddenColumns", iColumns)
                .AddWithValue("@UserID", GetLoggedInUserId())
            End With
            con.Execute(cmd)
        End Using
    End Sub

    ''' <summary>
    ''' Updates an exception screenshot for a given resource.
    ''' </summary>
    ''' <param name="details">The details of the screenshot</param>
    <SecuredMethod(True)>
    Public Sub UpdateExceptionScreenshot(details As clsScreenshotDetails) Implements IServer.UpdateExceptionScreenshot
        CheckPermissions()
        Using con = GetConnection()
            ' Get encryption scheme
            Dim encryptid = GetDefaultEncrypter(con)

            con.BeginTransaction()
            Using cmd As New SqlCommand(
                 " update BPAScreenshot with (serializable)" &
                 " set screenshot = @screenshot, encryptid = @encryptid, stageid = @stageid, processname = @processname, lastupdated = @lastupdated, timezoneoffset = @timezoneoffset where resourceid = @resourceid" &
                 " if @@rowcount = 0" &
                 " begin" &
                 "   insert BPAScreenshot (screenshot, encryptid, stageid, processname, lastupdated, timezoneoffset, resourceid) values (@screenshot, @encryptid, @stageid, @processname, @lastupdated, @timezoneoffset, @resourceid)" &
                 " end")

                Dim timestamp = details.Timestamp
                Dim lastupdated = timestamp.DateTime
                Dim timezoneoffset = timestamp.Offset.TotalMinutes

                With cmd.Parameters
                    .AddWithValue("@screenshot", Encrypt(encryptid, details.Screenshot))
                    .AddWithValue("@encryptid", encryptid)
                    .AddWithValue("@stageid", details.StageId)
                    .AddWithValue("@processname", details.ProcessName)
                    .AddWithValue("@lastupdated", lastupdated)
                    .AddWithValue("@timezoneoffset", timezoneoffset)
                    .AddWithValue("@resourceid", details.ResourceId)
                End With
                con.Execute(cmd)
            End Using
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Checks whether a screenshot is available for the given resource
    ''' </summary>
    ''' <param name="resourceId">The id of the resource</param>
    <SecuredMethod(Permission.Resources.ViewResourceScreenCaptures)>
    Function CheckExceptionScreenshotAvailable(resourceId As Guid) As Boolean Implements IServer.CheckExceptionScreenshotAvailable
        CheckPermissions()
        Using con = GetConnection()
            Return CheckExceptionScreenshotAvailable(con,
                                                     resourceId,
                                                     New MemberPermissions(GetEffectiveGroupPermissionsForResource(con, resourceId)))
        End Using
    End Function

    Protected Function CheckExceptionScreenshotAvailable(con As IDatabaseConnection, resourceId As Guid, resourcePermissions As MemberPermissions) As Boolean

        If Not resourcePermissions.HasPermission(mLoggedInUser, Permission.Resources.ViewResourceScreenCaptures) Then
            Return False
        End If

        Using cmd As New SqlCommand(
         " select count(screenshot) from BPAScreenshot where resourceid = @resourceid")
            With cmd.Parameters
                .AddWithValue("@resourceid", resourceId)
            End With
            Return CInt(con.ExecuteReturnScalar(cmd)) > 0
        End Using
    End Function

    ''' <summary>
    ''' Gets the latest exception screenshot for a given resource.
    ''' </summary>
    ''' <param name="resourceId">The id of the resource</param>
    <SecuredMethod(Permission.Resources.ViewResourceScreenCaptures)>
    Public Function GetExceptionScreenshot(resourceId As Guid) As clsScreenshotDetails Implements IServer.GetExceptionScreenshot
        CheckPermissions()
        Using con = GetConnection()
            Return GetExceptionScreenshot(con,
                                   resourceId,
                                   New MemberPermissions(GetEffectiveGroupPermissionsForResource(con, resourceId)))

        End Using


    End Function

    Protected Function GetExceptionScreenshot(con As IDatabaseConnection, resourceId As Guid, resourcePermissions As MemberPermissions) As clsScreenshotDetails

        If Not resourcePermissions.HasPermission(mLoggedInUser, Permission.Resources.ViewResourceScreenCaptures) Then
            Throw New BluePrismException(String.Format(My.Resources.clsServer_UserDoesNotHaveTheCorrectPermissionToViewTheScreenCaptureForResource0, resourceId))
        End If

        Using cmd As New SqlCommand(
         " select screenshot, encryptid, stageid, processname, lastupdated, timezoneoffset from BPAScreenshot where resourceid = @resourceid")
            With cmd.Parameters
                .AddWithValue("@resourceid", resourceId)
            End With
            Using reader = con.ExecuteReturnDataReader(cmd)
                If reader.Read() Then
                    Dim prov = New ReaderDataProvider(reader)

                    Dim timestamp = New DateTimeOffset(
                        prov.GetValue("lastupdated", DateTime.MinValue),
                        TimeSpan.FromMinutes(prov.GetInt("timezoneoffset")))

                    Dim details As New clsScreenshotDetails With {
                        .Screenshot = Decrypt(prov.GetInt("encryptid"), prov.GetString("screenshot")),
                        .StageId = prov.GetValue("stageid", Guid.Empty),
                        .ProcessName = prov.GetString("processname"),
                        .Timestamp = timestamp,
                        .ResourceId = resourceId}

                    Return details
                End If
            End Using
        End Using
        Throw New BluePrismException(My.Resources.clsServer_ThereIsNoScreenshotForThisResource)
    End Function

    ''' <summary>
    ''' Re-encrypts the latest exception screenshot for each resource, using the
    ''' currently selected default encryption scheme
    ''' </summary>
    ''' <returns>The number of records updated</returns>
    <SecuredMethod(Permission.SystemManager.Security.ManageEncryptionSchemes)>
    Public Function ReEncryptExceptionScreenshot() As Integer Implements IServer.ReEncryptExceptionScreenshots
        CheckPermissions()
        Using con = GetConnection()
            Dim newEncryptID = GetDefaultEncrypter(con)
            Dim ids As New List(Of Guid)
            Dim screenshot As String

            'Build a list of resource IDs that have screen captures that
            'aren't encrypted with the currently configured scheme
            Using cmd = New SqlCommand("select resourceid from BPAScreenshot where encryptid <> @encryptid")
                cmd.Parameters.AddWithValue("@encryptid", newEncryptID)
                Using reader = con.ExecuteReturnDataReader(cmd)
                    Dim prov As New ReaderDataProvider(reader)
                    While reader.Read()
                        ids.Add(prov.GetValue("resourceid", Guid.Empty))
                    End While
                End Using
            End Using
            If ids.Count = 0 Then Return -1

            con.BeginTransaction()
            For Each id In ids
                ' Retrieve the screenshots (decrypting with the original scheme)
                Using cmd As New SqlCommand("select screenshot, encryptid from BPAScreenshot where resourceid = @resourceid")
                    cmd.Parameters.AddWithValue("@resourceid", id)
                    Using reader = con.ExecuteReturnDataReader(cmd)
                        reader.Read()
                        Dim prov = New ReaderDataProvider(reader)
                        screenshot = Decrypt(prov.GetInt("encryptid"), prov.GetString("screenshot"))
                    End Using
                End Using

                ' Update the screenshot (encrypting with the new scheme)
                Using cmd As New SqlCommand("update BPAScreenshot set screenshot = @screenshot, encryptid = @encryptid where resourceid = @resourceid")
                    With cmd.Parameters
                        .AddWithValue("@resourceid", id)
                        .AddWithValue("@screenshot", Encrypt(newEncryptID, screenshot))
                        .AddWithValue("@encryptid", newEncryptID)
                    End With
                    con.Execute(cmd)
                End Using
            Next
            con.CommitTransaction()

            Return ids.Count
        End Using
    End Function

End Class
