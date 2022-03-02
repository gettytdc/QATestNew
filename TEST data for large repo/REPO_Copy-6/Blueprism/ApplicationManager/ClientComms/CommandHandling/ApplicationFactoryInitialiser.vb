Imports System.Collections.Generic
Imports System.Reflection
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.Operations
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.UIAutomation

Namespace CommandHandling

    ''' <summary>
    ''' Manages initialisation of a shared IHandlerFactory instance used by all 
    ''' applications to create command handlers
    ''' </summary>
    ''' <remarks>
    ''' While we're trying to separate out functionality from clsLocalTargetApp, 
    ''' this class helps to tie things together.
    ''' </remarks>
    Friend Class ApplicationFactoryInitialiser

        ''' <summary>
        ''' Creates a shared HandlerFactory instance used by 
        ''' all applications
        ''' to create command handlers for specific commands
        ''' </summary>
        Public Shared Function Initialise() As HandlerFactory

            Dim handlers As IEnumerable(Of HandlerDescriptor) = GetHandlers()
            Dim resolver = CreateDependencyResolver()
            Return New HandlerFactory(handlers, resolver)

        End Function

        ''' <summary>
        ''' Gets definitions of all concrete handlers currently used to process
        ''' commands
        ''' </summary>
        ''' <returns>Sequence of HandlerDescriptors</returns>
        Public Shared Function GetHandlers() As IEnumerable(Of HandlerDescriptor)

            Dim scanner As New HandlerScanner(New DefaultCommandIdConvention)
            Dim handlerNamespace = GetHandlerNamespace()
            Dim handlerAssembly = Assembly.GetExecutingAssembly()
            Return scanner.FindHandlers(handlerAssembly, handlerNamespace)

        End Function

        ''' <summary>
        ''' Gets the parent namespace within which to scan handlers
        ''' </summary>
        ''' <returns></returns>
        Private Shared Function GetHandlerNamespace() As String

            Dim current = GetType(ApplicationFactoryInitialiser).Namespace
            Dim root = current.Substring(0, current.LastIndexOf("."))
            Return root & ".CommandHandlers"

        End Function

        ''' <summary>
        ''' Creates an IHandlerDependencyResolver. This allows particular types
        ''' to be defined that will be used as contructor parameters when 
        ''' creating ICommandHandler instances. 
        ''' </summary>
        ''' <remarks>
        ''' Dependency injection has been deliberately kept simple with a hard-coded
        ''' map of types. We don't deal with anything like lifecyle management, 
        ''' scoping, nested components etc. An IOC container should really be introduced 
        ''' here and in other areas of the application if more sophisticated and 
        ''' efficient dependency injection is needed. 
        ''' </remarks>
        Private Shared Function CreateDependencyResolver() _
            As IHandlerDependencyResolver

            Return New HandlerDependencyResolver(GetDependencies())

        End Function

        ''' <summary>
        ''' Returns a hardcoded collection of dependencies used to initialise the
        ''' HandlerDependencyResolver. This collection of dependency types needs to 
        ''' be amended as new dependencies are added.
        ''' </summary>
        ''' <returns>A collection of dependencies</returns>
        Private Shared Function GetDependencies() As Dictionary(Of Type, Func(Of clsLocalTargetApp, Object))

            Return New Dictionary(Of Type, Func(Of clsLocalTargetApp, Object)) From
                {
                {GetType(clsLocalTargetApp), Function(application) application},
                {GetType(ILocalTargetApp), Function(application) application},
                {GetType(IAutomationFactory), Function(application) AutomationTypeProvider.GetType(Of IAutomationFactory)},
                {GetType(IUIAutomationIdentifierHelper), Function(application) application.UIAutomationIdentifierHelper},
                {GetType(IMouseOperationsProvider), Function(application) New MouseOperationsProvider()},
                {GetType(IWindowOperationsProvider), Function(application) New WindowOperationsProvider()},
                {GetType(IKeyboardOperationsProvider), Function(application) New KeyboardOperationsProvider()},
                {GetType(IAutomationHelper), Function(application) AutomationTypeProvider.GetType(Of IAutomationHelper)},
                {GetType(IBrowserAutomationIdentifierHelper), Function(application) DependencyResolver.Resolve(Of IBrowserAutomationIdentifierHelper)}
                }
        End Function
    End Class
End Namespace