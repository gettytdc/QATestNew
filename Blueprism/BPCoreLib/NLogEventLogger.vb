Imports NLog

Public Class NLogEventLogger : Implements IEventLogger

    Public Sub [Error](msg As String, logger As Logger) Implements IEventLogger.[Error]
        logger.Error(msg)
    End Sub

    Public Sub Warn(msg As String, logger As Logger) Implements IEventLogger.Warn
        logger.Warn(msg)
    End Sub

    Public Sub Info(msg As String, logger As Logger) Implements IEventLogger.Info
        logger.Info(msg)
    End Sub

    Public Sub Debug(msg As String, logger As Logger) Implements IEventLogger.Debug
        logger.Debug(msg)
    End Sub
End Class
