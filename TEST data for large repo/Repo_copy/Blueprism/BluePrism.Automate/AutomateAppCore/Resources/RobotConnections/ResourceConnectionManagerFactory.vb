Imports Autofac
Imports BluePrism.BPCoreLib.DependencyInjection

Namespace Resources
    Public Module ResourceConnectionManagerFactory
        Private mResourceConnectionManager As IUserAuthResourceConnectionManager = Nothing

        Public Function GetResourceConnectionManager(useASCR As Boolean) As IUserAuthResourceConnectionManager
            If mResourceConnectionManager Is Nothing OrElse mResourceConnectionManager.IsDisposed Then

                If useASCR Then
                    mResourceConnectionManager = DependencyResolver.Resolve(Of IUserAuthResourceConnectionManager)
                Else
                    mResourceConnectionManager = CType(DependencyResolver.Resolve(Of IResourceConnectionManager)(New TypedParameter(GetType(ConnectionType), ConnectionType.Direct), New TypedParameter(GetType(Boolean), False)), IUserAuthResourceConnectionManager)
                End If
            End If

            Return mResourceConnectionManager
        End Function
    End Module
End Namespace
