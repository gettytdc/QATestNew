Imports System.ServiceModel
Imports System.ServiceModel.Channels
Imports System.ServiceModel.Dispatcher
Imports NLog

Friend Class WCFMessageInspector
    Implements IClientMessageInspector

    Private _logger As ILogger = LogManager.GetCurrentClassLogger()

    Public Sub New()
    End Sub

    Public Function BeforeSendRequest(ByRef request As Message, channel As IClientChannel) As Object Implements IClientMessageInspector.BeforeSendRequest
        Return DateTime.Now.Ticks
    End Function

    Public Sub AfterReceiveReply(ByRef reply As Message, correlationState As Object) Implements IClientMessageInspector.AfterReceiveReply

        If Not (_logger.IsDebugEnabled OrElse _logger.IsTraceEnabled) Then Return

        Dim starttime As Long
        If correlationState IsNot Nothing Then
            starttime = DirectCast(correlationState, Long)
        End If
        Dim timetaken = New TimeSpan(DateTime.Now.Ticks - starttime)

        Dim buffer As MessageBuffer = reply.CreateBufferedCopy(Int32.MaxValue)
        reply = buffer.CreateMessage()
        Dim message = buffer.CreateMessage().ToString()

        Dim name As String = ""
        Dim nameStart = message.IndexOf("http://tempuri.org/IServer/", StringComparison.InvariantCultureIgnoreCase)
        If (nameStart > 0) Then
            nameStart += 27
            Dim nameEnd = message.IndexOf("Response", nameStart, StringComparison.InvariantCultureIgnoreCase)
            name = message.Substring(nameStart, nameEnd - nameStart)
        End If

        If (name <> "Nop" AndAlso Not String.IsNullOrEmpty(name)) Then
            Dim logMessage = $"Function: {name},        Reply Size: {message.Length},    Time: {String.Format("{0:0.00}", timetaken.TotalMilliseconds)}ms"
            If _logger.IsDebugEnabled Then _logger.Debug(logMessage)
            If _logger.IsTraceEnabled Then _logger.Trace($"Response: {Environment.NewLine}{message}")
        End If

    End Sub

End Class
