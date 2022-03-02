Imports BluePrism.ApplicationManager.ApplicationManagerUtilities

Namespace CommandHandling

    ''' <summary>
    ''' Handles an individual query against a target application and
    ''' returns the result
    ''' </summary>
    Friend Interface ICommandHandler

        ''' <summary>
        ''' Executes the query specified and returns a result
        ''' </summary>
        ''' <param name="context">Contains information about the current query and 
        ''' application</param>
        ''' <returns>The result of the query</returns>
        Function Execute(context As CommandContext) As Reply

    End Interface

End NameSpace