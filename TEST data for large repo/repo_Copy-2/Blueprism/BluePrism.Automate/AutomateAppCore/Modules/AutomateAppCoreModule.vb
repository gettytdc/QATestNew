Imports Autofac

Imports BluePrism.ClientServerResources.Core.Interfaces

Public Class AutomateAppCoreModule
    Inherits Autofac.Module

    Protected Overrides Sub Load(builder As Autofac.ContainerBuilder)
        builder.RegisterType(Of TokenValidator).As(Of ITokenValidator)()
        builder.RegisterType(Of TokenRegistration).As(Of ITokenRegistration)()
    End Sub
End Class
