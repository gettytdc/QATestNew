Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.Server.Domain.Models

Namespace CommandHandling

    ''' <summary>
    ''' Instantiates instances of command handlers
    ''' </summary>
    Friend Class HandlerFactory : Implements IHandlerFactory

        Private ReadOnly mDependencyResolver As IHandlerDependencyResolver

        ''' <summary>
        ''' Map of command ids to handler types - a regular dictionary should be 
        ''' safe for reads but this type has no significant extra cost for reads
        ''' </summary>
        Private ReadOnly mHandlers As ConcurrentDictionary(Of String, HandlerDescriptor)

        ''' <summary>
        ''' Creates a new HandlerFactory
        ''' </summary>
        ''' <param name="handlers">A collection of handlers</param>
        ''' <param name="dependencyResolver">The IHandlerDependencyResolver used
        ''' to get constructor parameters used when instantiating ICommandHandler
        ''' instances</param>
        Public Sub New(handlers As IEnumerable(Of HandlerDescriptor), _
                       dependencyResolver As IHandlerDependencyResolver)
            Dim handlerList = handlers.ToList()
            Dim duplicates = handlerList.GroupBy(Function(handler) handler.CommandId).
                Where(Function(grouping) grouping.Count > 1).
                SelectMany(Function(grouping) grouping).
                OrderBy(Function(handler) handler.CommandId).
                ToList()
            If duplicates.Any
                Dim list = String.Join(Environment.NewLine, duplicates.Select(Function(duplicate) $"{duplicate.CommandId}: {duplicate.HandlerType.FullName}"))
                Throw New DuplicateException(String.Format(My.Resources.DuplicateCommandIdsWereFoundTheFollowingHandlersHaveDuplicateIds0, list))

            End If

            mHandlers = New ConcurrentDictionary(Of String, HandlerDescriptor) _
                (handlerList.ToDictionary(Function(descriptor) descriptor.CommandId))
            mDependencyResolver = dependencyResolver

        End Sub

        ''' <inheritdoc />
        Public Function CreateHandler(application As clsLocalTargetApp, query As clsQuery) As ICommandHandler _
            Implements IHandlerFactory.CreateHandler

            Dim handler As HandlerDescriptor = Nothing
            If mHandlers.TryGetValue(query.Command, handler) Then
                Return CreateHandler(application, handler)
            Else
                Return Nothing
            End If

        End Function

        Private Function CreateHandler(application As clsLocalTargetApp, descriptor As HandlerDescriptor) As ICommandHandler

            Dim handlerType = descriptor.HandlerType
            Dim constructors = handlerType.GetConstructors()
            If constructors.Length <> 1 Then
                Throw New MissingConstructorException(My.Resources.UnableToCreateAnInstanceOf0ThisTypeShouldHaveASinglePublicConstructor, handlerType)
            End If

            Dim constructor = constructors(0)
            Dim args = constructor.GetParameters() _
                .Select(Function(info) GetDependency(handlerType, application, info.ParameterType)) _
                .ToArray()
            Return CType(Activator.CreateInstance(handlerType, args), ICommandHandler)

        End Function

        Private Function GetDependency(handlerType As Type,
                                       application As clsLocalTargetApp,
                                       dependencyType As Type) As Object
            Dim dependency As Object = Nothing
            If Not mDependencyResolver.TryGetDependency(dependencyType, application, dependency) Then
                Throw New MissingDependencyException(My.Resources.ADependencyOfType0CouldNotBeFoundWhenCreatingAnInstanceOf1, dependencyType, handlerType)
            End If
            Return dependency
        End Function
    End Class
End NameSpace
