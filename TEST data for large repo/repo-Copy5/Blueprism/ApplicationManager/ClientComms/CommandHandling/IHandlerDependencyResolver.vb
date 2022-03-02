Namespace CommandHandling

    ''' <summary>
    ''' Gets objects to use as constructor arguments when creating handler instances.
    ''' This is used to allow a flexible range of dependencies to be injected via
    ''' each ICommandHandler's constructor.
    ''' </summary>
    Public Interface IHandlerDependencyResolver

        ''' <summary>
        ''' Gets an object of the specified type
        ''' </summary>
        ''' <param name="type">The type of object to obtain</param>
        ''' <returns>An object of the specified type</returns>
        Function TryGetDependency(type As Type, application As clsLocalTargetApp, ByRef dependency As Object) As Boolean

    End Interface
End NameSpace