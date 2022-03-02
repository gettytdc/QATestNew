Imports BluePrism.Server.Domain.Models

''' <summary>
''' Readable output builder
''' </summary>
Friend Class ReadableOutputBuilder
    Implements ILogOutputBuilder

    ''' <summary>
    ''' The length of the name in the formatted entries.
    ''' </summary>
    Public Const PaddedNameLength As Integer = 40

    ''' <summary>
    ''' The format string for the header.
    ''' This format string defines padded placeholders for column titles.
    ''' </summary>
    Public Const Header As String = "- {0,-40} | {1,-8} | {2,-8} | {3,-8} | {4, -12} | {5}"

    ''' <summary>
    ''' Format string for a finished schedule - expects all 4 parameters
    ''' </summary>
    Public Const FinishedSchedule As String =
     "{0} {1,-40} | {2:HH:mm:ss} | {3:HH:mm:ss} | {4:HH:mm:ss} | {5} | {6}"

    ''' <summary>
    ''' Format String for a late finished schedule - this adds the date of
    ''' the end date to the last parameter and expects all 4 parameters.
    ''' </summary>
    Public Const LateFinishedSchedule As String =
     "{0} {1,-40} | {2:HH:mm:ss} | {3:HH:mm:ss} | {4:HH:mm:ss [yyyy-MM-dd]} | {5} | {6}"

    ''' <summary>
    ''' Format string for a currently executing schedule. This expects a
    ''' name, instance date and start date and a string for an end date.
    ''' </summary>
    Public Const ExecutingSchedule As String =
     "- {1,-40} | {2:HH:mm:ss} | {3:HH:mm:ss} | {4,-8} | {5} | {6}"

    ''' <summary>
    ''' Format string for an unstarted schedule. This expects a name and an
    ''' instance date, and strings for start and end date.
    ''' </summary>
    Public Const UnstartedSchedule As String =
     "- {1,-40} | {2:HH:mm:ss} | {3,-8} | {4,-8} | {5} | {6}"

    ''' <summary>
    ''' Format string for a finished task. This expects a name, start date
    ''' and end date and a string for the instance date.
    ''' </summary>
    Public Const FinishedTask As String =
     "{0} * {1,-38} | {2,-8} | {3:HH:mm:ss} | {4:HH:mm:ss} | {5} | {6}"

    ''' <summary>
    ''' Format string for a late finished task. This expects a name, start
    ''' date and end date and a string for the instance date. The actual
    ''' date of the end date is appended to the end time.
    ''' </summary>
    Public Const LateFinishedTask As String =
     "{0} * {1,-38} | {2,-8} | {3:HH:mm:ss} | {4:HH:mm:ss [yyyy-MM-dd]} | {5} | {6}"

    ''' <summary>
    ''' Format string for a currently executing task. This expects a name
    ''' and start date, and strings for instance date and end date.
    ''' </summary>
    Public Const ExecutingTask As String =
     "- * {1,-38} | {2,-8} | {3:HH:mm:ss} | {4,-8} | {5} | {6}"

    ''' <summary>
    ''' Format string for a finished session.This expects a name, start date
    ''' and end date and a string for the instance date.
    ''' </summary>
    Public Const FinishedSession As String =
     "{0} = {1,-38} | {2,-8} | {3:HH:mm:ss} | {4:HH:mm:ss} | {5} | {6}"

    ''' <summary>
    ''' Format string for a late finished session. This expects a name,
    ''' start date and end date and a string for the instance date. The
    ''' actual date of the end date is appended to the end time.
    ''' </summary>
    Public Const LateFinishedSession As String =
     "{0} = {1,-38} | {2,-8} | {3:HH:mm:ss} | {4:HH:mm:ss [yyyy-MM-dd]} | {5} | {6}"

    ''' <summary>
    ''' Format string for a currently executing session. This expects a name
    ''' and start date, and strings for instance date and end date.
    ''' </summary>
    Public Const ExecutingSession As String =
     "- = {1,-38} | {2,-8} | {3:HH:mm:ss} | {4,-8} | {5} | {6}"

    ''' <summary>
    ''' 
    ''' </summary>
    Private ReadOnly mOutput As IO.TextWriter

    ''' <summary>
    ''' 
    ''' </summary>
    Private mFmt As String

    ''' <summary>
    ''' The late format string to use if the end date
    ''' falls on a different date to the start date. Setting this to null will cancel
    ''' any checking in this area, and if end date is not set, this format string 
    ''' will be ignored.
    ''' </summary>
    Private mLateFmt As String

    Private mMaxNameLen As Integer
    Private mDisposedValue As Boolean

    Public Sub New(ByVal output As IO.TextWriter)
        mOutput = output
        mFmt = UnstartedSchedule
        mMaxNameLen = PaddedNameLength
    End Sub

    Public Sub WriteHeader(ByVal showInstanceTime As Boolean) Implements ILogOutputBuilder.WriteHeader
        'We ignore showInstanceTime because currently readable output always shows the instance time.

        mOutput.WriteLine()
        mOutput.WriteLine(Header,
         My.Resources.ReadableOutputBuilder_ScheduleName, My.Resources.ReadableOutputBuilder_Instance, My.Resources.ReadableOutputBuilder_Start, My.Resources.ReadableOutputBuilder_End, My.Resources.ReadableOutputBuilder_Server, My.Resources.ReadableOutputBuilder_TerminationReason)
    End Sub

    Public Sub WriteNoEntries(ByVal startDate As Date, ByVal endDate As Date) Implements ILogOutputBuilder.WriteNoEntries
        mOutput.WriteLine(My.Resources.ReadableOutputBuilder_NoSchedulesFoundBetween0DAnd1D, startDate, endDate)
    End Sub

    Public Sub WriteGroupHeader(ByVal scheduleDate As Date) Implements ILogOutputBuilder.WriteGroupHeader
        mOutput.WriteLine()
        mOutput.WriteLine(My.Resources.ReadableOutputBuilder_0DddDdMMMMYyyy, scheduleDate)
    End Sub

    Public Sub WriteGroupSeperator() Implements ILogOutputBuilder.WriteGroupSeperator
        mOutput.WriteLine("-")
    End Sub

    ''' <summary>
    ''' Char to indicate a successful schedule / task / session.
    ''' Tick char would have been nice but command prompt's handling of unicode
    ''' is tragically poor.
    ''' </summary>
    Private Const SUCCESS_CHAR As Char = "-"c

    ''' <summary>
    ''' Char to indicate a terminated (or stopped) task / session.
    ''' </summary>
    Private Const TERMINATE_CHAR As Char = "-"c

    Public Sub WriteEntry(status As ItemStatus, name As String,
     instDate As Date, startDate As Date, endDate As Date,
     server As String, terminationReason As String) Implements ILogOutputBuilder.WriteEntry

        ' If a possible late format string was set, check the end date against
        ' the start date to see if it is the same date. If it is, then use the given
        ' format string; if not, use the 'late' format string
        If mLateFmt IsNot Nothing Then
            If endDate <> Nothing AndAlso endDate <> Date.MaxValue Then
                Dim sd As Date = startDate.ToLocalTime()
                Dim ed As Date = endDate.ToLocalTime()
                If sd.Date <> ed.Date Then
                    mFmt = mLateFmt
                End If
            End If
        End If

        If name IsNot Nothing AndAlso name.Length > mMaxNameLen Then
            name = name.Substring(0, mMaxNameLen - 3) & "..."
        End If

        mOutput.WriteLine(mFmt,
         If(status = ItemStatus.Completed, SUCCESS_CHAR, TERMINATE_CHAR),
         name,
         IIf(instDate <> Nothing, instDate, "-"),
         IIf(startDate <> Nothing, startDate.ToLocalTime(), "-"),
         IIf(endDate <> Nothing AndAlso endDate <> Date.MaxValue, endDate.ToLocalTime(), "-"),
         If(server, "-"),
         If(terminationReason, "-")
        )
    End Sub

    Public Sub SetFinishedSchedule() Implements ILogOutputBuilder.SetFinishedSchedule
        mFmt = FinishedSchedule
        mLateFmt = LateFinishedSchedule
    End Sub

    Public Sub SetExecutingSchedule() Implements ILogOutputBuilder.SetExecutingSchedule
        mFmt = ExecutingSchedule
        mMaxNameLen = PaddedNameLength
    End Sub

    Public Sub SetExecutingTask() Implements ILogOutputBuilder.SetExecutingTask
        mFmt = ExecutingTask
        mMaxNameLen = PaddedNameLength - 2
    End Sub

    Public Sub SetFinishedTask() Implements ILogOutputBuilder.SetFinishedTask
        mFmt = FinishedTask
        mLateFmt = LateFinishedTask
        mMaxNameLen = PaddedNameLength - 2
    End Sub

    Public Sub SetExecutingSession() Implements ILogOutputBuilder.SetExecutingSession
        mFmt = ExecutingSession
        mMaxNameLen = PaddedNameLength - 2
    End Sub

    Public Sub SetFinishedSession() Implements ILogOutputBuilder.SetFinishedSession
        mFmt = FinishedSession
        mLateFmt = LateFinishedSession
        mMaxNameLen = PaddedNameLength - 2
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not mDisposedValue Then
            If disposing Then
                mOutput?.Dispose()
            End If

            mDisposedValue = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class

