Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.Server.Domain.Models

Namespace CommandHandling

    ''' <summary>
    ''' Handles creation of ICommandHandler instances
    ''' </summary>
    Friend Interface IHandlerFactory

        ''' <summary>
        ''' Creates an ICommandHandler instance for the specified command
        ''' </summary>
        ''' <param name="application">The application within which the command is
        ''' running</param>
        ''' <param name="query">The query being handled</param>
        ''' <returns>The handler instance</returns>
        ''' <exception cref="MissingConstructorException">
        ''' If no public constructor was available in the handler type to
        ''' create the handler with
        ''' </exception>
        Function CreateHandler(application As clsLocalTargetApp, _
                               query As clsQuery) As ICommandHandler
    End Interface

End Namespace
