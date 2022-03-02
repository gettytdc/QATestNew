Imports Autofac

Namespace DependencyInjection

    ''' <summary>
    ''' Provides access to types registered in the application's shared dependency
    ''' injection container. Types are registered in the container at application 
    ''' startup and made available via DependencyResolver. Note that direct
    ''' resolution of dependencies from the container should be kept to a minimum.
    ''' This static resolver is designed for use at the boundary between types 
    ''' stored in the container and areas of the application where we do not manage 
    ''' object creation, such as within UI code. It is also likely to be used from 
    ''' legacy code to bridge across to DI-friendly classes.
    ''' </summary>
    Public Class DependencyResolver

        Private Shared mContainer As IContainer = Nothing

        ''' <summary>
        ''' Initialises DependencyResolver with the container instance that
        ''' This is called during application startup.
        ''' </summary>
        ''' <param name="container">The container used to resolve dependencies</param>
        Public Shared Sub Initialise(container As IContainer)

            If (mContainer IsNot Nothing) Then
                Throw New InvalidOperationException("DependencyResolver has already been initialised")
            End If
            If container Is Nothing Then
                Throw New ArgumentNullException(NameOf(container))
            End If

            mContainer = container

        End Sub

        ''' <summary>
        ''' Resolves an object from the application's IOC container based on the 
        ''' specified service type
        ''' </summary>
        ''' <typeparam name="T">The type of the service to resolve</typeparam>
        ''' <returns>The object resolved from the IOC container</returns>
        Public Shared Function Resolve(Of T)(ParamArray parameters As Core.Parameter()) As T

            If mContainer Is Nothing Then
                Throw New InvalidOperationException("DependencyResolver has not been initialised")
            End If

            Return mContainer.Resolve(Of T)(parameters)
        End Function


        Public Shared Function ResolveKeyed(Of T)(key As Object, ParamArray parameters As Core.Parameter()) As T

            If mContainer Is Nothing Then
                Throw New InvalidOperationException("DependencyResolver has not been initialised")
            End If

            Return mContainer.ResolveKeyed(Of T)(key, parameters)
        End Function


        Public Shared Function GetScopedResolver() As IDependencyResolver
            Return New ScopedDependencyResolver(mContainer.BeginLifetimeScope())
        End Function

        ''' <summary>
        ''' Starts a lifetime scope within the application's IOC container, resolves 
        ''' a service and executes the specified action. The scope is ended after 
        ''' executing the action.
        ''' </summary>
        Public Shared Sub InScope(Of T)(action As Action(Of T))
            Using scope = mContainer.BeginLifetimeScope()
                Dim service = scope.Resolve(Of T)()
                action(service)
            End Using
        End Sub

        ''' <summary>
        ''' Starts a lifetime scope within the application's IOC container, adds 
        ''' custom registrations to the scope using the configure action, resolves
        ''' a service and executes the specified action. The scope is ended after 
        ''' executing the action.
        ''' </summary>
        Public Shared Sub InScope(Of T)(configure As Action(Of ContainerBuilder), action As Action(Of T))
            Using scope = mContainer.BeginLifetimeScope(configure)
                Dim service = scope.Resolve(Of T)()
                action(service)
            End Using
        End Sub

        ''' <summary>
        ''' Starts a lifetime scope within the application's IOC container, resolves 
        ''' a service and returns the result of the specified function. The scope is 
        ''' ended after executing the function.
        ''' </summary>
        Public Shared Function FromScope(Of TService, TResult)(func As Func(Of TService, TResult)) As TResult
            Using scope = mContainer.BeginLifetimeScope()
                Dim service = scope.Resolve(Of TService)()
                Return func(service)
            End Using
        End Function

        ''' <summary>
        ''' Starts a lifetime scope within the application's IOC container, adds 
        ''' custom registrations to the scope using the configure action, resolves
        ''' a service and executes the specified function. The scope is ended after 
        ''' executing the function.
        ''' </summary>
        Public Shared Function FromScope(Of TService, TResult)(configure As Action(Of ContainerBuilder),
                                                        func As Func(Of TService, TResult)) As TResult
            Using scope = mContainer.BeginLifetimeScope(configure)
                Dim service = scope.Resolve(Of TService)()
                Return func(service)
            End Using
        End Function

        ''' <summary>
        ''' Starts a lifetime scope within the application's IOC container, adds 
        ''' custom registrations to the scope using the configure action and executes 
        ''' the specified function with the given scope. The scope is ended after 
        ''' executing the function.
        ''' </summary>
        Public Shared Function FromScope(Of TResult)(configure As Action(Of ContainerBuilder),
                                                        func As Func(Of ILifetimeScope, TResult)) As TResult
            Using scope = mContainer.BeginLifetimeScope(configure)
                Return func(scope)
            End Using
        End Function

        ''' <summary>
        ''' Resets this DependencyResolver, clearing the current container object
        ''' (exposed for testing)
        ''' </summary>
        Public Shared Sub Reset()
            mContainer = Nothing
        End Sub

        ''' <summary>
        ''' Sets the container.
        ''' </summary>
        ''' <param name="container">The container.</param>
        ''' <remarks>
        ''' This should only EVER be called as part of unit testing
        ''' </remarks>
        Public Shared Sub SetContainer(container As IContainer)
            mContainer = container
        End Sub

    End Class
End Namespace
