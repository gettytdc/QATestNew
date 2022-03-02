Imports BluePrism.Server.Domain.Models

''' <summary>
''' Interface that defines the functions common to building Readable output 
''' vs CSV output.
''' </summary>
Friend Interface ILogOutputBuilder
    Inherits IDisposable

    Sub WriteNoEntries(ByVal startDate As Date, ByVal endDate As Date)
    Sub WriteHeader(ByVal showInstanceTime As Boolean)
    Sub WriteGroupHeader(ByVal scheduleDate As Date)
    Sub WriteGroupSeperator()
    Sub WriteEntry(
         status As ItemStatus, name As String,
         instdate As Date, startDate As Date, endDate As Date,
         server As String, terminationReason As String)
    Sub SetFinishedSchedule()
    Sub SetExecutingSchedule()
    Sub SetExecutingTask()
    Sub SetFinishedTask()
    Sub SetFinishedSession()
    Sub SetExecutingSession()

End Interface
