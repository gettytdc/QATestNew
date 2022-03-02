Imports BluePrism.ApplicationManager.ApplicationManagerUtilities

Namespace CommandHandling
    
    ''' <summary>
    ''' Contains information about the context of the command being
    ''' executed
    ''' </summary>
    Public Class CommandContext

        ''' <summary>
        ''' Creates a new CommandContext
        ''' </summary>
        ''' <param name="query">The query being executed by the handler</param>
        Public Sub New(query As clsQuery)
            Me.Query = query
        End Sub

        ''' <summary>
        ''' The query being executed by the handler
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Query As clsQuery
        
    End Class

End NameSpace