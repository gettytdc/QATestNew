Namespace DependencyInjection
    Public Interface IDependencyResolver : Inherits IDisposable
        Function Resolve(Of T)() As T
    End Interface
End NameSpace