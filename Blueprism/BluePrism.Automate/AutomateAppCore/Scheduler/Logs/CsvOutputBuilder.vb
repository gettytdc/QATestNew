Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

''' <summary>
''' CSV output builder
''' </summary>
Friend Class CsvOutputBuilder
    Implements ILogOutputBuilder

    Private ReadOnly mOutput As IO.TextWriter
    Private mType As String
    Private mShowInstanceTime As Boolean
    Private mDisposedValue As Boolean

    Public Sub New(output As IO.TextWriter)
        mOutput = output
    End Sub

    Public Sub WriteHeader(showInstanceTime As Boolean) Implements ILogOutputBuilder.WriteHeader
        mShowInstanceTime = showInstanceTime
        If mShowInstanceTime Then
            mOutput.WriteLine(My.Resources.CsvOutputBuilder_TypeStatusNameInstanceStartEndServerTerminationReason)
        Else
            mOutput.WriteLine(My.Resources.CsvOutputBuilder_TypeStatusNameStartEndServerTerminationReason)
        End If
    End Sub

    Public Sub WriteNoEntries(startDate As Date, endDate As Date) Implements ILogOutputBuilder.WriteNoEntries
        WriteHeader(True)
    End Sub

    Public Sub WriteGroupHeader(scheduleDate As Date) Implements ILogOutputBuilder.WriteGroupHeader
        'Do Nothing
    End Sub

    Public Sub WriteGroupSeperator() Implements ILogOutputBuilder.WriteGroupSeperator
        'Do Nothing
    End Sub

    Public Sub SetFinishedSchedule() Implements ILogOutputBuilder.SetFinishedSchedule
        mType = My.Resources.CsvOutputBuilder_Schedule
    End Sub

    Private Function Normalise(entry As String) As String
        Return If(String.IsNullOrEmpty(entry),
                   String.Empty,
                   """" & entry.Replace("""", """""") & """")
    End Function

    Private Function Normalise(dt As Date) As String
        Return If(dt = Nothing OrElse dt = Date.MaxValue,
                   String.Empty,
                   dt.ToLocalTime().ToString("u"))
    End Function

    Public Sub WriteEntry(
     status As ItemStatus, name As String,
     instdate As Date, startDate As Date, endDate As Date,
     server As String, terminationReason As String) Implements ILogOutputBuilder.WriteEntry

        If mShowInstanceTime Then
            mOutput.WriteLine($"{mType},{clsEnum.GetLocalizedFriendlyName(status)},{Normalise(name)},{Normalise(instdate)},{Normalise(startDate)},{Normalise(endDate)},{Normalise(server)},{Normalise(terminationReason)}"
            )
        Else
            mOutput.WriteLine($"{mType},{clsEnum.GetLocalizedFriendlyName(status)},{Normalise(name)},{Normalise(startDate)},{Normalise(endDate)},{Normalise(server)},{Normalise(terminationReason)}"
            )
        End If
    End Sub

    Public Sub SetExecutingSchedule() Implements ILogOutputBuilder.SetExecutingSchedule
        mType = My.Resources.CsvOutputBuilder_Schedule
    End Sub

    Public Sub SetExecutingTask() Implements ILogOutputBuilder.SetExecutingTask
        mType = My.Resources.CsvOutputBuilder_Task
    End Sub

    Public Sub SetFinishedTask() Implements ILogOutputBuilder.SetFinishedTask
        mType = My.Resources.CsvOutputBuilder_Task
    End Sub

    Public Sub SetExecutingSession() Implements ILogOutputBuilder.SetExecutingSession
        mType = My.Resources.CsvOutputBuilder_Session
    End Sub

    Public Sub SetFinishedSession() Implements ILogOutputBuilder.SetFinishedSession
        mType = My.Resources.CsvOutputBuilder_Session
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

