''' <summary>
''' Interface that must be implemented to
''' allow status information and the like to be passed back.
''' </summary>
Public Interface ISessionNotifier
    Sub RaiseError(ByVal message As String)
    Sub RaiseError(ByVal msg As String, ByVal ParamArray args() As Object)
    Sub RaiseWarn(ByVal message As String)
    Sub RaiseWarn(ByVal msg As String, ByVal ParamArray args() As Object)
    Sub RaiseInfo(ByVal message As String)
    Sub RaiseInfo(ByVal msg As String, ByVal ParamArray args() As Object)
    Sub AddNotification(ByVal msg As String)
    Sub AddNotification(formattedMsg As String, ParamArray args() As Object)
    Sub NotifyStatus()
    Sub HandleSessionStatusFailure(ByVal rr As RunnerRecord, ByVal errmsg As String)
    Sub VarChanged(ByVal var As clsSessionVariable)
End Interface
