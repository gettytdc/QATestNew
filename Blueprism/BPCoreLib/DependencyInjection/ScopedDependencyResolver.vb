Imports Autofac

Namespace DependencyInjection
    Public Class ScopedDependencyResolver : Implements IDependencyResolver

        Private ReadOnly mScope As ILifetimeScope

        Public Sub New(scope As ILifetimeScope)
            mScope = scope
        End Sub

        Public Function Resolve (Of T)() As T Implements IDependencyResolver.Resolve
            Return mScope.Resolve(Of T)()
        End Function


        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Private mDisposed As Boolean

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not mDisposed Then
                If disposing Then
                    mScope?.Dispose()
                End If
            End If
            mDisposed = True
        End Sub

        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub

    End Class
End NameSpace