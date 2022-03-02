Namespace CommandHandling
    
    ''' <summary>
    ''' Formulates command id values based on command handler types. This is
    ''' used when scanning assembly for handler types to map command ids to
    ''' handlers.
    ''' </summary>
    Friend Interface ICommandIdConvention
    
        ''' <summary>
        ''' Gets the id to identify a handler
        ''' </summary>
        ''' <param name="handlerType">The type of handler</param>
        ''' <returns>The command id</returns>
        Function GetId(handlerType As Type) As String

    End Interface
End NameSpace