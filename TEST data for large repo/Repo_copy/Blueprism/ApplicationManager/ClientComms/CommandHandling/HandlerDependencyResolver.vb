Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports BluePrism.ApplicationManager.CommandHandling

''' <summary>
''' Simple IHandlerDependencyResolver that creates dependencies from a 
''' fixed collection of dependencies. 
''' </summary>
''' <remarks>
''' This has been deliberately kept simple with a hard-coded list of dependencies
''' An IOC container should really be introduced here and in other areas of the 
''' application if more sophisticated and efficient dependency injection is needed. 
''' </remarks>
Public Class HandlerDependencyResolver
    : Implements IHandlerDependencyResolver

    Private ReadOnly mDependencies As ReadOnlyDictionary _
        (Of Type,Func(Of clsLocalTargetApp, Object))

    ''' <summary>
    ''' Creates a new HandlerDependencyResolver
    ''' </summary>
    ''' <param name="dependencies">A dictionary of dependencies. Each item's key
    ''' defines the type to create and the value contains a function used to 
    ''' create the object</param>
    Public Sub New(dependencies As IDictionary(Of Type, Func(Of clsLocalTargetApp, Object)))
        mDependencies = New ReadOnlyDictionary _
            (Of Type, Func(Of clsLocalTargetApp, Object))(dependencies)
    End Sub

    ''' <summary>
    ''' Attempts to gets a dependency and indicates whether it was found
    ''' </summary>
    ''' <param name="type">The type fo the dependency</param>
    ''' <param name="application">The application against which the command is
    ''' executing</param>
    ''' <param name="dependency">The parameter that will be initialised with
    ''' the dependency if it is defined</param>
    ''' <returns>Indicates whether the dependency is defined</returns>
    Public Function TryGetDependency(type As Type, application As clsLocalTargetApp, _
                                     ByRef dependency As Object) As Boolean _
        Implements IHandlerDependencyResolver.TryGetDependency
        
        Dim func As Func(Of clsLocalTargetApp, Object) = Nothing
        If Not mDependencies.TryGetValue(type, func)
            dependency = Nothing
            Return False
        End If
        dependency = func(application)
        Return True

    End Function

End Class