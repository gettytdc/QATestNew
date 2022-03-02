Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandling
Imports NLog

Namespace CommandHandlers.Shared

    ''' <summary>
    ''' Convenience base class for command handlers that operate on an instance
    ''' of ILocalTargetApp
    ''' </summary>
    Friend MustInherit Class CommandHandlerBase : Implements ICommandHandler
        ''' <summary>
        ''' Creates a new instance of the handler
        ''' </summary>
        ''' <param name="application">The application against which the handler is 
        ''' running</param>
        Protected Sub New(application As ILocalTargetApp)
            Me.Application = application
        End Sub

        ''' <summary>
        ''' The application against which the handler is running 
        ''' </summary>
        Protected ReadOnly Property Application As ILocalTargetApp
        Protected ReadOnly Property Logger As ILogger = LogManager.GetCurrentClassLogger()

        ''' <summary>
        ''' Implements the command handler functionality
        ''' </summary>
        ''' <param name="context">Details about the application and query to execute</param>
        ''' <returns>A Reply indicating the outcome of the command</returns>
        Public MustOverride Function Execute(context As CommandContext) As Reply _
            Implements ICommandHandler.Execute
    End Class
End Namespace
