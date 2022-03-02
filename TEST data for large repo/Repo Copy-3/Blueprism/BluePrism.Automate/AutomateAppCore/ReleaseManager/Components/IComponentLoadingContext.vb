''' <summary>
''' Interface describing the context used to load components - this provides access
''' to other previously loaded components such that all referenced components can use
''' the same object references.
''' </summary>
Public Interface IComponentLoadingContext

    ''' <summary>
    ''' Finds the component of the specified type with the given ID in this context.
    ''' </summary>
    ''' <param name="type">The component type to search for.</param>
    ''' <param name="id">The ID to search for</param>
    ''' <returns>The package component from within this group of the required type,
    ''' and which has an ID of the same type and value as the specified ID. Null if
    ''' no such component could be found in this group.</returns>
    Function GetComponent( _
     ByVal type As PackageComponentType, ByVal id As Object) As PackageComponent

    ''' <summary>
    ''' Finds the component of the specified type with the given ID in this context.
    ''' </summary>
    ''' <param name="type">The component type to search for.</param>
    ''' <param name="name">The name to search for</param>
    ''' <returns>The package component from within this group of the required type,
    ''' and which has an ID of the same type and value as the specified ID. Null if
    ''' no such component could be found in this group.</returns>
    Function GetComponent( _
     ByVal type As PackageComponentType, ByVal name As String) As PackageComponent

    ''' <summary>
    ''' Gets all components of the specified type
    ''' </summary>
    ''' <param name="type">The type of components required.</param>
    ''' <returns>A non-null collection of package components within this context
    ''' which have the specified type.</returns>
    Function GetAllComponents(ByVal type As PackageComponentType) _
     As ICollection(Of PackageComponent)

    ''' <summary>
    ''' Gets the item with the given name from the context.
    ''' </summary>
    ''' <param name="name">The name of the item required.</param>
    ''' <returns>The item corresponding to the given name or null if no such item was
    ''' found.</returns>
    Default Property Item(ByVal name As String) As Object

End Interface