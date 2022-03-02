Imports System.IO
Imports System.Threading
Imports System.Xml
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.BPCoreLib
Imports NLog
Imports EntityType = BluePrism.AutomateProcessCore.Processes.DiagramType

''' Project  : Automate
''' Class    : clsDBLoggingEngine
''' 
''' <summary>
''' A class used to log process activity in the database.
''' </summary>
Public Class clsDBLoggingEngine : Inherits clsLoggingEngine

#Region " Class-scope declarations "

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger() 

    ''' <summary>
    ''' A class to hold details of a log record that requires
    ''' 'completing' with an end date and possibly outputs.
    ''' </summary>
    Private Class clsIncompleteLog
        Public SessionNumber As Integer
        Public logId As Long
        Public StageID As Guid
        Public Inputs As clsArgumentList
    End Class

#End Region

#Region " Member Variables "

    ''' <summary>
    ''' Logs that require end dates to be completed.
    ''' </summary>
    Private mIncompleteLogs As List(Of clsIncompleteLog)

    ''' <summary>
    ''' Indicates that this session should log to the unicode logging table
    ''' </summary>
    Private mUnicodeLog As Boolean = False

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new logging engine
    ''' </summary>
    ''' <param name="username">User starting the process</param>
    ''' <param name="resourceId">PC starting the process</param>
    ''' <param name="proc">The process that this logging engine is for</param>
    ''' <param name="debugging">Whether the process is being debugged</param>
    ''' <param name="sessId">The session ID</param>
    ''' <param name="sessNo">The session number</param>
    Public Sub New(username As String, resourceId As Guid, proc As clsProcess,
     debugging As Boolean, sessId As Guid, sessNo As Integer)
        Me.New(New LogContext(username, resourceId, proc, debugging, sessId, sessNo))
    End Sub

    ''' <summary>
    ''' Creates a new logging engine
    ''' </summary>
    ''' <param name="ctx">The context for the logging</param>
    Public Sub New(ByVal ctx As LogContext)
        MyBase.New(ctx)
        mIncompleteLogs = New List(Of clsIncompleteLog)
        mUnicodeLog = gSv.UnicodeLoggingEnabled()
    End Sub

#End Region

#Region " Methods "

    Public Overrides Sub SetEnvironmentVariable(info As LogInfo, loggableMessage As String)
        WriteLog(Nothing, loggableMessage, DataType.text, Nothing, Nothing, info)
    End Sub
    Public Overrides Sub CreateSessionLog(info As LogInfo)
        WriteLog(Nothing, My.Resources.RunnerRecord_CreatedSessionLog, DataType.text, Nothing, Nothing, info)
    End Sub
    Public Overrides Sub ImmediateStop(info As LogInfo, stopReason As String)
        WriteLog(Nothing, stopReason, DataType.text, Nothing, Nothing, info)
    End Sub
    Public Overrides Sub UnexpectedException(info As LogInfo)
        WriteLog(Nothing, My.Resources.RunnerRecord_ExceptionEncountered, DataType.text, Nothing, Nothing, info)
    End Sub


    ''' <summary>
    ''' Writes a log entry to the log, retrying as necessary
    ''' </summary>
    ''' <param name="stg">The stage which fired the log entry</param>
    ''' <param name="res">The result of the action which caused the entry</param>
    ''' <param name="resType">The type of the result</param>
    ''' <param name="inputs">The list of input arguments</param>
    ''' <param name="outputs">The list of output arguments</param>
    ''' <param name="info">An object providing information regarding the log entry
    ''' </param>
    ''' <param name="objName">The name of the business object, if appropriate</param>
    ''' <param name="actionName">The name of the action, if appropriate</param>
    ''' <returns>The sequence number for the log entry that can be used (together
    ''' with the session number) to identify the record created</returns>
    ''' <exception cref="LogFailedException">If any errors occurred</exception>
    Private Function WriteLog( _
     stg As clsProcessStage, res As String, resType As DataType,
     inputs As clsArgumentList, outputs As clsArgumentList,
     info As LogInfo,
     Optional objName As String = "",
     Optional actionName As String = "") As Long

        ' If we're inhibiting logging, return immediately
        If info.Inhibit Then Return 0

        ' Replace the params with an empty argument list if we're required to
        ' inhibit their logging.
        If info.InhibitParams Then inputs = Nothing

        Dim attempts As Integer = 0
        While True
            Try
                ' Increment the attempts and try and write the log entry
                attempts += 1
                Return DoLog(
                 stg, res, resType, inputs, outputs, info, objName, actionName)

            Catch lfe As LogFailedException
                ' Record this error on the event log, a warning if we're not (yet)
                ' failing the session; an error if we've reached our error limit
                ' and we're terminating the session
                Dim stgName As String = "<none>"
                If stg IsNot Nothing Then stgName = stg.Name

                Dim terminating As Boolean =
                 (Context.FailOnError AndAlso attempts >= Context.Attempts)

                If terminating Then 
                    Log.Error("Terminating Session - Failed to write to session log (attempt {0}): " & _
                              "Session: {1}; Message: {2}; Stage: {3}; Result: {4}", _
                              attempts, Context.SessionNo, lfe.Message, stgName, res)
                Else
                    Log.Warn("Failed to write to session log (attempt {0}): " & _
                              "Session: {1}; Message: {2}; Stage: {3}; Result: {4}", _
                              attempts, Context.SessionNo, lfe.Message, stgName, res)
                End If

                ' If we're not failing on error, there's absolutely no point in
                ' waiting at all, just return - effectively reporting success
                If Not Context.FailOnError Then Return 0

                ' If we've exhausted our attempts, throw the last exception we got
                If attempts >= Context.Attempts Then Throw

                ' Wait the appropriate number of seconds before the next retry
                Thread.Sleep(Context.RetrySeconds * 1000)

            End Try

        End While

    End Function

    ''' <summary>
    ''' Logs data to the db.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event or nothing</param>
    ''' <param name="res">The result or message value</param>
    ''' <param name="resType">The result type</param>
    ''' <param name="inputs">The inputs or nothing when there are no inputs</param>
    ''' <param name="outputs">The outputs or nothing when there are no outputs</param>
    ''' <param name="loginfo">Logging information</param>
    ''' <param name="objName">The name of the business object responsible for this
    ''' log entry, if appropriate. Empty string, otherwise</param>
    ''' <param name="actionName">The name of the action within which this log entry
    ''' is being written, if appropriate. Empty string, otherwise</param>
    ''' <returns>The sequence number for the log entry that can be used (together
    ''' with the session number) to identify the record created</returns>
    ''' <exception cref="LogFailedException">If the act of writing the log entry to
    ''' the database failed.</exception>
    Private Function DoLog(
     stg As clsProcessStage, res As String, resType As DataType,
     inputs As clsArgumentList, outputs As clsArgumentList,
     loginfo As LogInfo,
     Optional objName As String = "",
     Optional actionName As String = "") As Long

        Try

            Dim id As Guid = Guid.Empty
            Dim stgName As String = ""
            Dim stgType As StageTypes = StageTypes.Undefined
            If stg IsNot Nothing Then
                id = stg.Id
                stgName = stg.Name
                stgType = stg.StageType
            End If

            ' Replace the params with an empty argument list if we're required to
            ' inhibit their logging.
            If loginfo.InhibitParams Then inputs = Nothing : outputs = Nothing

            'According to the function documentation, we accept a null argument to the
            'stage parameter
            Dim procName As String = ""
            Dim sheetName As String = ""
            Dim dateTimeStarted As DateTimeOffset = DateTimeOffset.MinValue
            Dim proc As clsProcess = Nothing
            If stg IsNot Nothing Then
                proc = stg.Process
                If proc IsNot Nothing Then
                    procName = proc.Name
                    sheetName = stg.SubSheet.Name
                    dateTimeStarted = stg.DateTimeStarted
                End If
            End If

            If proc IsNot Nothing AndAlso proc.ProcessType = EntityType.Object Then
                objName = procName
                procName = ""
                actionName = sheetName
                sheetName = ""
            End If

            Dim paramsXml As String = GetParametersXML(inputs, outputs)

            Return gSv.LogToDB(Context.SessionNo, Context.ResourceId, Context.ResourceName, id, stgName, stgType, res,
             resType, loginfo, dateTimeStarted, paramsXml, procName, sheetName,
             objName, actionName, mUnicodeLog)

        Catch lfe As LogFailedException
            ' Just rethrow a log-failed; no need to add some static text
            Throw

        Catch ex As Exception
            ' Anything else, wrap into a log-failed error
            Throw New LogFailedException(ex,
             "Error while attempting to log to database: {0}", ex.ToString())

        End Try
    End Function

    ''' <summary>
    ''' Collects details of a log record that will be updated when an action,
    ''' subsheet or subprocess stage is completed.
    ''' </summary>
    ''' <param name="logId">The sequence number used together with the session number
    ''' to identify the log entry</param>
    ''' <param name="id">The ID of the stage this refers to</param>
    ''' <param name="inputs">The inputs that were stored in the first half of the
    ''' logging process.</param>
    Private Sub AddIncompleteLog(logId As Long, info As LogInfo, id As Guid, inputs As clsArgumentList)
        ' Make sure we don't add anything if we're not actually logging (bg-615)
        If info.Inhibit OrElse mIncompleteLogs Is Nothing Then Return

        ' Replace the params with an empty argument list if we're required to
        ' inhibit their logging.
        If info.InhibitParams Then inputs = Nothing

        Dim log As New clsIncompleteLog()
        log.SessionNumber = Context.SessionNo
        log.logId = logId
        log.StageID = id
        log.Inputs = inputs
        mIncompleteLogs.Insert(0, log)
    End Sub

    ''' <summary>
    ''' Updates the end date and outputs of an existing log record.
    ''' </summary>
    ''' <param name="stg">The stage</param>
    ''' <param name="outputs">The output XML</param>
    Private Sub UpdateLog(info As LogInfo, stg As clsProcessStage, objectName As String, actionName As String,
     outputs As clsArgumentList)


        If info.Inhibit OrElse mIncompleteLogs Is Nothing Then Return

        ' Replace the params with an empty argument list if we're required to
        ' inhibit their logging.
        If info.InhibitParams Then outputs = Nothing

        Dim stgId As Guid = Guid.Empty
        Dim stgName As String = ""
        Dim stgType As StageTypes = StageTypes.Undefined
        Dim startDateTime As DateTimeOffset = DateTimeOffset.MinValue
        If stg IsNot Nothing Then
            stgId = stg.Id
            stgName = stg.Name
            stgType = stg.StageType
            startDateTime = stg.DateTimeStarted
        End If

        Dim procName As String = ""
        Dim sheetName As String = ""
        Dim proc As clsProcess = Nothing
        If stg IsNot Nothing Then
            proc = stg.Process
            If proc IsNot Nothing Then
                procName = proc.Name
                sheetName = stg.SubSheet.Name
            End If
        End If

        'TODO: Is this piece needed for the Update As well as it is in the DoLog above - should refactor into a helper is so inc above
        If proc IsNot Nothing AndAlso proc.ProcessType = EntityType.Object Then
            objectName = procName
            procName = ""
            actionName = sheetName
            sheetName = ""
        End If


        For Each log As clsIncompleteLog In mIncompleteLogs
            If stg.Id = log.StageID AndAlso log.SessionNumber = Context.SessionNo Then
                Dim params As String = GetParametersXML(log.Inputs, outputs)
                Try
                    gSv.UpdateLog(log.SessionNumber, log.logId, params, DateTimeOffset.Now, mUnicodeLog, Context.ResourceId, Context.ResourceName,
                            procName, stgId, stgName, stgType, sheetName, objectName, actionName, startDateTime)

                Catch
                End Try
                mIncompleteLogs.Remove(log)
                Exit For
            End If

        Next

    End Sub


    ''' <summary>
    ''' Gets the params XML to store for the given argument lists.
    ''' </summary>
    ''' <param name="inputs">The list of input arguments</param>
    ''' <param name="outputs">The list of output arguments</param>
    ''' <returns>An XML string which contains the XML markup describing the argument
    ''' lists; an empty string if both lists were empty.</returns>
    Private Function GetParametersXML(ByVal inputs As clsArgumentList, ByVal outputs As clsArgumentList) As String

        If clsArgumentList.IsEmpty(inputs) AndAlso clsArgumentList.IsEmpty(outputs) _
         Then Return ""

        Using sw As New StringWriter()
            Using xr As New XmlTextWriter(sw)
                xr.WriteStartElement("parameters")
                xr.WriteStartElement("inputs")
                If Not clsArgumentList.IsEmpty(inputs) Then
                    For Each input As clsArgument In inputs
                        input.ArgumentToXML(xr, False, True)
                    Next
                End If
                xr.WriteEndElement()
                xr.WriteStartElement("outputs")
                If Not clsArgumentList.IsEmpty(outputs) Then
                    For Each output As clsArgument In outputs
                        output.ArgumentToXML(xr, True, True)
                    Next
                End If
                xr.WriteEndElement()
                xr.WriteEndElement()
                Return sw.ToString()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Logs the start of an action stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="objName"></param>
    ''' <param name="actionName"></param>
    ''' <param name="inputs"></param>
    Public Overrides Sub ActionPrologue(ByVal info As LogInfo, ByVal stg As clsActionStage, ByVal objName As String, ByVal actionName As String, ByVal inputs As clsArgumentList)
        Dim logId = WriteLog(stg, "", DataType.unknown, inputs, Nothing, info, objName, actionName)
        AddIncompleteLog(logId, info, stg.Id, inputs)
    End Sub

    ''' <summary>
    ''' Logs the completion of an action stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="outputs"></param>
    Public Overrides Sub ActionEpilogue(ByVal info As LogInfo, ByVal stg As clsActionStage, ByVal objectName As String, ByVal actionName As String, ByVal outputs As clsArgumentList)
        UpdateLog(info, stg, objectName, actionName, outputs)
    End Sub

    Public Overrides Sub AlertPrologue(info As LogInfo, stage As Stages.clsAlertStage)
    End Sub

    ''' <summary>
    ''' Logs an alert stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="sResult"></param>
    Public Overrides Sub AlertEpilogue(ByVal info As LogInfo, ByVal stg As clsAlertStage, ByVal sResult As String)
        WriteLog(stg, sResult, DataType.text, Nothing, Nothing, info)
    End Sub

    ''' <summary>
    ''' Logs a calculation stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    Public Overrides Sub CalculationPrologue(ByVal info As LogInfo, ByVal stg As clsCalculationStage)
    End Sub

    ''' <summary>
    ''' Logs a calculation stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="res">The result of the calculation</param>
    Public Overrides Sub CalculationEpilogue(ByVal info As LogInfo, ByVal stg As clsCalculationStage, ByVal res As clsProcessValue)
        WriteLog(stg, res.LoggableValue, res.DataType, Nothing, Nothing, info)
    End Sub

    Public Overrides Sub ChoicePrologue(ByVal info As LogInfo, ByVal stage As clsChoiceStartStage)
    End Sub

    ''' <summary>
    ''' Logs a choice stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="name"></param>
    ''' <param name="choiceNo"></param>
    Public Overrides Sub ChoiceEpilogue(ByVal info As LogInfo, ByVal stg As clsChoiceStartStage, ByVal name As String, ByVal choiceNo As Integer)
        WriteLog(stg, "(" & CStr(choiceNo) & ") " & name, DataType.text, Nothing, Nothing, info)
    End Sub

    Public Overrides Sub CodePrologue(ByVal info As LogInfo, ByVal stage As clsCodeStage, ByVal inputs As clsArgumentList)
    End Sub

    ''' <summary>
    ''' Logs a code stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="inputs"></param>
    ''' <param name="outputs"></param>
    Public Overrides Sub CodeEpilogue(ByVal info As LogInfo, ByVal stg As clsCodeStage, ByVal inputs As clsArgumentList, ByVal outputs As clsArgumentList)
        WriteLog(stg, "", DataType.unknown, inputs, outputs, info)
    End Sub

    Public Overrides Sub DecisionPrologue(ByVal info As LogInfo, ByVal stage As clsDecisionStage)
    End Sub

    ''' <summary>
    ''' Logs a decision stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="res"></param>
    Public Overrides Sub DecisionEpilogue(ByVal info As LogInfo, ByVal stg As clsDecisionStage, ByVal res As String)
        WriteLog(stg, res, DataType.flag, Nothing, Nothing, info)
    End Sub

    Public Overrides Sub LogDiagnostic(ByVal info As LogInfo, ByVal stg As clsProcessStage, ByVal msg As String)
        WriteLog(stg, msg, DataType.text, Nothing, Nothing, info)
    End Sub

    ''' <summary>
    ''' Logs an exception screenshot.
    ''' </summary>
    ''' <param name="stage">The exception stage</param>
    ''' <param name="processName">The running process or object name</param>
    ''' <param name="image">A clsPixRect containing the screeshot image</param>
    Public Overrides Sub LogExceptionScreenshot(info As LogInfo, stage As clsProcessStage, ByVal processName As String, image As clsPixRect, timestamp As DateTimeOffset)

        If Not Context.ScreenshotAllowed Then Return

        If Context.Debugging Then
            WriteLog(stage, My.Resources.clsDBLoggingEngine_ScreenCaptureNotSavedDueToRunningInDebugMode, DataType.text, Nothing, Nothing, info)
            Return
        End If

        Dim details As New clsScreenshotDetails With {
            .ResourceId = Context.ResourceId,
            .StageId = stage.Id,
            .ProcessName = processName,
            .Timestamp = timestamp,
            .Screenshot = image.ToString()
            }
        gSv.UpdateExceptionScreenshot(details)
    End Sub

    ''' <summary>
    ''' Logs the end stage of a VBO action.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="objName"></param>
    ''' <param name="actionName"></param>
    ''' <param name="outputs"></param>
    Public Overrides Sub ObjectEpilogue(ByVal info As LogInfo, ByVal stg As clsEndStage, ByVal objName As String, ByVal actionName As String, ByVal outputs As clsArgumentList)
        WriteLog(stg, "", DataType.unknown, Nothing, outputs, info, objName, actionName)
    End Sub

    ''' <summary>
    ''' Logs the end stage of a process.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="outputs"></param>
    Public Overrides Sub ProcessEpilogue(ByVal info As LogInfo, ByVal stg As clsEndStage, ByVal outputs As clsArgumentList)
        WriteLog(stg, "", DataType.unknown, Nothing, outputs, info)
    End Sub

    ''' <summary>
    ''' Logs the end stage of a subsheet.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="outputs"></param>
    Public Overrides Sub SubSheetEpilogue(ByVal info As LogInfo, ByVal stg As clsEndStage, ByVal outputs As clsArgumentList)
        WriteLog(stg, "", DataType.unknown, Nothing, outputs, info)
    End Sub

    ''' <summary>
    ''' Logs an error message.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="errMsg"></param>
    Public Overrides Sub LogError(ByVal info As LogInfo, ByVal stg As clsProcessStage, ByVal errMsg As String)
        WriteLog(stg, "ERROR: " & errMsg, DataType.text, Nothing, Nothing, info)
    End Sub

    Public Overrides Sub LoopEndPrologue(ByVal info As LogInfo, ByVal stage As clsLoopEndStage)
    End Sub

    ''' <summary>
    ''' Logs a loop end stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="iter"></param>
    Public Overrides Sub LoopEndEpilogue(ByVal info As LogInfo, ByVal stg As clsLoopEndStage, ByVal iter As String)
        WriteLog(stg, iter, DataType.text, Nothing, Nothing, info)
    End Sub

    Public Overrides Sub LoopStartPrologue(ByVal info As LogInfo, ByVal stage As clsLoopStartStage)
    End Sub

    ''' <summary>
    ''' Logs a loop start stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="sCount"></param>
    Public Overrides Sub LoopStartEpilogue(ByVal info As LogInfo, ByVal stg As clsLoopStartStage, ByVal sCount As String)
        WriteLog(stg, sCount, DataType.text, Nothing, Nothing, info)
    End Sub

    Public Overrides Sub MultipleCalculationPrologue(ByVal info As LogInfo, ByVal stage As clsMultipleCalculationStage)
    End Sub

    Public Overrides Sub MultipleCalculationEpilogue(ByVal info As LogInfo, ByVal stg As clsMultipleCalculationStage, ByVal objResults As clsArgumentList)
        WriteLog(stg, "", DataType.unknown, Nothing, objResults, info)
    End Sub

    Public Overrides Sub NavigatePrologue(ByVal info As LogInfo, ByVal stage As clsNavigateStage)
    End Sub

    ''' <summary>
    ''' Logs a navigate stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    Public Overrides Sub NavigateEpilogue(ByVal info As LogInfo, ByVal stg As clsNavigateStage)
        WriteLog(stg, "", DataType.unknown, Nothing, Nothing, info)
    End Sub

    ''' <summary>
    ''' Logs a note stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    Public Overrides Sub NotePrologue(ByVal info As LogInfo, ByVal stg As clsNoteStage)
        WriteLog(stg, stg.GetNarrative, DataType.text, Nothing, Nothing, info)
    End Sub

    Public Overrides Sub NoteEpilogue(ByVal info As LogInfo, ByVal stage As clsNoteStage)
    End Sub

    Public Overrides Sub ReadPrologue(ByVal info As LogInfo, ByVal stage As clsReadStage)
    End Sub

    ''' <summary>
    ''' Logs a read stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="outputs"></param>
    Public Overrides Sub ReadEpilogue(ByVal info As LogInfo, ByVal stg As clsReadStage, ByVal outputs As clsArgumentList)
        WriteLog(stg, "", DataType.unknown, Nothing, outputs, info)
    End Sub

    Public Overrides Sub ExceptionPrologue(ByVal info As LogInfo, ByVal stage As clsExceptionStage)
    End Sub

    Public Overrides Sub ExceptionEpilogue(ByVal info As LogInfo, ByVal stage As clsExceptionStage)
    End Sub

    Public Overrides Sub RecoverPrologue(ByVal info As LogInfo, ByVal stage As clsRecoverStage)
    End Sub
    ''' <summary>
    ''' Logs a recover stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    Public Overrides Sub RecoverEpilogue(ByVal info As LogInfo, ByVal stg As clsRecoverStage)
        WriteLog(stg, "", DataType.text, Nothing, Nothing, info)
    End Sub

    Public Overrides Sub ResumePrologue(ByVal info As LogInfo, ByVal stage As clsResumeStage)
    End Sub
    ''' <summary>
    ''' Logs a resume stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    Public Overrides Sub ResumeEpilogue(ByVal info As LogInfo, ByVal stg As clsResumeStage)
        WriteLog(stg, "", DataType.text, Nothing, Nothing, info)
    End Sub

    Public Overrides Sub ObjectPrologue(ByVal info As LogInfo, ByVal stg As clsStartStage, ByVal objName As String, ByVal sActionName As String, ByVal inputs As clsArgumentList)
        WriteLog(stg, "", DataType.unknown, inputs, Nothing, info)
    End Sub

    ''' <summary>
    ''' Logs the start stage of a process.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="inputs"></param>
    Public Overrides Sub ProcessPrologue(ByVal info As LogInfo, ByVal stg As clsStartStage, ByVal inputs As clsArgumentList)
        WriteLog(stg, "", DataType.unknown, inputs, Nothing, info)
    End Sub

    ''' <summary>
    ''' Logs the start stage of a subsheet.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="inputs"></param>
    Public Overrides Sub SubSheetPrologue(ByVal info As LogInfo, ByVal stg As clsStartStage, ByVal inputs As clsArgumentList)
        WriteLog(stg, "", DataType.unknown, inputs, Nothing, info)
    End Sub

    ''' <summary>
    ''' Logs a subprocess stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="inputs"></param>
    Public Overrides Sub ProcessRefPrologue(ByVal info As LogInfo, ByVal stg As clsSubProcessRefStage, ByVal inputs As clsArgumentList)
        Dim logId = WriteLog(stg, "", DataType.unknown, inputs, Nothing, info, "", "")
        AddIncompleteLog(logId, info, stg.Id, inputs)
    End Sub

    ''' <summary>
    ''' Logs the completion of a subprocess.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="outputs"></param>
    Public Overrides Sub ProcessRefEpilogue(ByVal info As LogInfo, ByVal stg As clsSubProcessRefStage, ByVal outputs As clsArgumentList)
        UpdateLog(info, stg, Nothing, Nothing, outputs)
    End Sub

    ''' <summary>
    ''' Logs a subsheet stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="inputs"></param>
    Public Overrides Sub SubSheetRefPrologue(ByVal info As LogInfo, ByVal stg As clsSubSheetRefStage, ByVal inputs As clsArgumentList)
        Dim seqNum = WriteLog(stg, "", DataType.unknown, inputs, Nothing, info, "", "")
        AddIncompleteLog(seqNum, info, stg.Id, inputs)
    End Sub

    ''' <summary>
    ''' Logs the completion of a subsheet stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="outputs"></param>
    Public Overrides Sub SubSheetRefEpilogue(ByVal info As LogInfo, ByVal stg As clsSubSheetRefStage, ByVal outputs As clsArgumentList)
        UpdateLog(info, stg, Nothing, Nothing, outputs)
    End Sub

    Public Overrides Sub WaitPrologue(ByVal info As LogInfo, ByVal stage As clsWaitStartStage)
    End Sub

    ''' <summary>
    ''' Logs a wait stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="name"></param>
    ''' <param name="choiceNo"></param>
    Public Overrides Sub WaitEpilogue(ByVal info As LogInfo, ByVal stg As clsWaitStartStage, ByVal name As String, ByVal choiceNo As Integer)
        WriteLog(stg, "(" & choiceNo & ") " & name, DataType.text, Nothing, Nothing, info)
    End Sub

    Public Overrides Sub WritePrologue(ByVal info As LogInfo, ByVal stage As clsWriteStage)
    End Sub

    ''' <summary>
    ''' Logs a write stage.
    ''' </summary>
    ''' <param name="stg">The stage generating the logging event</param>
    ''' <param name="inputs">The input parameters</param>
    Public Overrides Sub WriteEpilogue(ByVal info As LogInfo, ByVal stg As clsWriteStage, ByVal inputs As clsArgumentList)
        WriteLog(stg, "", DataType.unknown, inputs, Nothing, info)
    End Sub

    Public Overrides Sub SkillPrologue(info As LogInfo, stg As clsSkillStage, objectName As String, skillActionName As String, inputs As clsArgumentList)
        Dim logId = WriteLog(stg, "", DataType.unknown, inputs, Nothing, info, objectName, skillActionName)
        AddIncompleteLog(logId, info, stg.Id, inputs)
    End Sub

    Public Overrides Sub SkillEpilogue(info As LogInfo, stg As clsSkillStage, objectName As String, skillActionName As String, outputs As clsArgumentList)
        UpdateLog(info, stg, objectName, skillActionName, outputs)
    End Sub

    ''' <summary>
    ''' Creates or updates a statistic record in the database.
    ''' </summary>
    ''' <param name="sessId">The session id</param>
    ''' <param name="name">The statistic name</param>
    ''' <param name="val">The statistic value</param>
    Public Overrides Sub LogStatistic(ByVal sessId As Guid, ByVal name As String, ByVal val As clsProcessValue)
        gSv.UpdateStatistic(sessId, name, val.EncodedValue, val.EncodedType)
    End Sub


    ''' <summary>
    ''' Disposes of this object, ensuring that the incomplete logs are cleaned up
    ''' </summary>
    ''' <param name="disposing">True if being called from a Dispose() call, False if
    ''' being called as part of object finalization by the garbage collector.</param>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then mIncompleteLogs.Clear()
        MyBase.Dispose(disposing)
    End Sub

#End Region

End Class
