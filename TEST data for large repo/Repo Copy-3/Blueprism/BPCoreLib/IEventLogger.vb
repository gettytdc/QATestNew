Imports NLog

Public Interface IEventLogger

    Sub [Error](msg As String, logger As Logger)
    Sub Warn(msg As String, logger As Logger)
    Sub Info(msg As String, logger As Logger)
    Sub Debug(msg As String, logger As Logger)

End Interface
