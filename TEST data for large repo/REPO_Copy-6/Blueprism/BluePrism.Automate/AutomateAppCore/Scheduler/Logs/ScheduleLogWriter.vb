Imports BluePrism.Scheduling
Imports System.IO
Imports BluePrism.Server.Domain.Models

''' <summary>
''' This class takes care of writing schedule logs to the Screen or CSV file
''' </summary>
Public Class ScheduleLogWriter
    Implements IDisposable

    ''' <summary>
    ''' The builder instance
    ''' </summary>
    Private ReadOnly mBuilder As ILogOutputBuilder
    Private mDisposedValue As Boolean

    ''' <summary>
    ''' Creates a new Writer
    ''' </summary>
    ''' <param name="output">The output textwriter to use</param>
    ''' <param name="type">The type of output to generate</param>
    Public Sub New(ByVal output As TextWriter, ByVal type As ScheduleLogOutputType)
        Select Case type
            Case ScheduleLogOutputType.CSV
                mBuilder = New CsvOutputBuilder(output)
            Case ScheduleLogOutputType.Readable
                mBuilder = New ReadableOutputBuilder(output)
        End Select
    End Sub

    ''' <summary>
    ''' Event fired as work in the exporting progresses.
    ''' </summary>
    ''' <param name="percent">The percentage through the whole export task that
    ''' is done so far. This is obviously approximate.</param>
    Public Event ProgressUpdate(ByVal percent As Double)

    ''' <summary>
    ''' Outputs the given list using the currently set builder
    ''' </summary>
    ''' <param name="list">The list to output the contents of</param>
    Public Sub OutputScheduleList(list As ScheduleList)

        Dim entries As ICollection(Of IScheduleInstance) =
            list.Store.GetListEntries(list)

        If entries.Count = 0 Then
            mBuilder.WriteNoEntries(list.GetStartDate(), list.GetEndDate())
            RaiseEvent ProgressUpdate(100)
            Return
        End If

        Dim currDate As Date = Date.MinValue
        mBuilder.WriteHeader(True)

        Dim curr As Double = 0.0
        For Each inst As IScheduleInstance In entries
            Dim scheduleDate As Date = inst.InstanceTime.Date
            If scheduleDate <> currDate Then
                mBuilder.WriteGroupHeader(scheduleDate)
                currDate = scheduleDate

            ElseIf list.ListType = ScheduleListType.Report Then ' separate verbose report entries
                mBuilder.WriteGroupSeperator()

            End If

            ' Set the format if 'unstarted' is not appropriate.
            ' if entry has an end date, use 'finished', if a start date but no end
            ' date, use 'executing'.
            If inst.EndTime <> Date.MaxValue Then
                mBuilder.SetFinishedSchedule()

            ElseIf inst.StartTime <> Date.MinValue Then
                mBuilder.SetExecutingSchedule()

            End If

            Dim log = TryCast(inst, IScheduleLog)
            Dim schedulerName = If(log IsNot Nothing, log.SchedulerName, Nothing)

            ' Write the entry to the console.
            mBuilder.WriteEntry(
             inst.Status, inst.Schedule.Name,
             inst.InstanceTime, inst.StartTime,
             inst.EndTime, schedulerName, "")

            ' Output the log data (if the entry is a log - OutputScheduleLog() ignores it if
            ' it isn't)
            OutputScheduleLogWithoutHeader(TryCast(inst, HistoricalScheduleLog))
            curr += 1
            RaiseEvent ProgressUpdate(99 * curr / entries.Count)
        Next
        RaiseEvent ProgressUpdate(100)

    End Sub

    ''' <summary>
    ''' Outputs the schedule list specified by the given parameters.
    ''' </summary>
    ''' <param name="name">The name of the schedule list to output, null if
    ''' specifying a dynamic schedule list.</param>
    ''' <param name="days">The number of days from the given absolute date
    ''' to bound the list. Ignored if a name is present.</param>
    ''' <param name="dt">The absolute date from which the list should search
    ''' </param>
    ''' <param name="store">The store which acts as a gateway to the schedule and
    ''' report information</param>
    ''' <param name="type">The type of list to output</param>
    Public Sub OutputScheduleList(ByVal name As String, ByVal days As Integer, ByVal dt As Date,
     ByVal store As DatabaseBackedScheduleStore, ByVal type As ScheduleListType)

        Dim list As ScheduleList

        ' Do we have a name? 
        If name IsNot Nothing Then
            list = store.GetScheduleList(name, type)
            If list Is Nothing Then
                Throw New ScheduleWriterException("The {0} '{1}' does not exist", type, name)
            End If
        Else
            ' We have a number and a date
            list = New ScheduleList(store)
            list.ListType = type
            list.Name = "<TemporaryList>"
            list.AbsoluteDate = dt
            list.DaysDistance = days
            list.AllSchedules = True
        End If

        OutputScheduleList(list)

    End Sub

    ''' <summary>
    ''' Attempts to deduce the status of the item that a log entry represents, given
    ''' the data held on the entry.
    ''' </summary>
    ''' <param name="entry">The log entry from which the status is assumed.</param>
    ''' <returns>An item status representing the deduced status from the log entry.
    ''' Briefly, if it has no end date, it is assumed to be
    ''' <see cref="ItemStatus.Running"/>; if it has no termination reason set it is
    ''' assumed to be <see cref="ItemStatus.Completed"/>; otherwise it is assumed to
    ''' be <see cref="ItemStatus.Terminated"/></returns>
    ''' <remarks></remarks>
    Private Function DeduceStatus(ByVal entry As CompoundLogEntry) As ItemStatus
        If entry.EndDate = Nothing Then Return ItemStatus.Running
        If entry.TerminationReason = "" Then Return ItemStatus.Completed
        Return ItemStatus.Terminated
    End Function

    ''' <summary>
    ''' Outputs the given schedule log to the output textwriter
    ''' </summary>
    ''' <param name="log">The log to write out to the console</param>
    Public Sub OutputScheduleLog(ByVal log As HistoricalScheduleLog)
        mBuilder.WriteHeader(False)
        OutputScheduleLogWithoutHeader(log)
    End Sub

    ''' <summary>
    ''' Outputs the given schedule log, callers of this function should have already
    ''' written a header.
    ''' </summary>
    ''' <param name="log">The log to write out, If the given log is null, it is 
    ''' ignored.</param>
    Private Sub OutputScheduleLogWithoutHeader(ByVal log As HistoricalScheduleLog)
        If log Is Nothing Then Return

        For Each taskEntry As TaskCompoundLogEntry In log.CompoundEntries
            If taskEntry.EndDate = Nothing Then
                mBuilder.SetExecutingTask()
            Else
                mBuilder.SetFinishedTask()
            End If

            mBuilder.WriteEntry(DeduceStatus(taskEntry), taskEntry.Name,
             Nothing, taskEntry.StartDate, taskEntry.EndDate, log.SchedulerName, taskEntry.TerminationReason)

            For Each sessEntry As SessionCompoundLogEntry In taskEntry.SortedSessions
                If sessEntry.EndDate <> Nothing Then
                    mBuilder.SetFinishedSession()
                Else
                    mBuilder.SetExecutingSession()
                End If

                mBuilder.WriteEntry(DeduceStatus(sessEntry),
                 sessEntry.SessionID.ToString(), Nothing,
                 sessEntry.StartDate, sessEntry.EndDate, log.SchedulerName,
                 sessEntry.TerminationReason)

            Next sessEntry

        Next taskEntry

    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not mDisposedValue Then
            If disposing Then
                mBuilder.Dispose()
            End If

            mDisposedValue = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
