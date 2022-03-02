Namespace Resources
    Public Interface IResourceRunner
        Inherits IDisposable

        Sub Init(Optional startedCallback As Action = Nothing)

        Function IsRunning() As Boolean

        Function SessionsRunning() As Boolean
        
        Sub Shutdown()
        
    End Interface
End Namespace
