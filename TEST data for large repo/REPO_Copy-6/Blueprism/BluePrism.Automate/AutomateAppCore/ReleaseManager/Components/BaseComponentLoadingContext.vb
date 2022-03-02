
''' <summary>
''' Base context for loading components - this maintains a dictionary for the context
''' data and delegates the retrieval of components to subclasses
''' </summary>
Public MustInherit Class BaseComponentLoadingContext : Implements IComponentLoadingContext

    ' Dictionary to hold the context data
    Private mContextData As IDictionary(Of String, Object)

    ''' <summary>
    ''' Creates a new base component loading context
    ''' </summary>
    Protected Sub New()
        mContextData = New Dictionary(Of String, Object)
    End Sub

    ''' <summary>
    ''' Gets the component of the specified type and ID.
    ''' </summary>
    ''' <param name="type">The type of component to filter on</param>
    ''' <param name="id">The ID required</param>
    ''' <returns>The component of the specified type and ID, or null if no such
    ''' component was found in this context.</returns>
    Public Overridable Function GetComponent( _
     ByVal type As PackageComponentType, ByVal id As Object) _
     As PackageComponent Implements IComponentLoadingContext.GetComponent
        For Each comp As PackageComponent In GetAllComponents(type)
            If Object.Equals(id, comp.Id) Then Return comp
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the item with the given name from the context.
    ''' </summary>
    ''' <param name="name">The name of the item required.</param>
    ''' <returns>The item corresponding to the given name or null if no such item
    ''' was found.</returns>
    Default Public Property Item(ByVal name As String) As Object _
     Implements IComponentLoadingContext.Item
        Get
            Dim obj As Object = Nothing
            mContextData.TryGetValue(name, obj)
            Return obj
        End Get
        Set(ByVal value As Object)
            mContextData(name) = value
        End Set
    End Property

    ''' <summary>
    ''' Gets all components from this context of the given type.
    ''' </summary>
    ''' <param name="type">The type of component required.</param>
    ''' <returns>A non-null collection of all components which are of the required
    ''' type.</returns>
    Public MustOverride Function GetAllComponents(ByVal type As PackageComponentType) _
     As ICollection(Of PackageComponent) Implements IComponentLoadingContext.GetAllComponents

    ''' <summary>
    ''' Gets the component of the specified type and name, or null if no such
    ''' component exists within this context.
    ''' </summary>
    ''' <param name="type">The type of component required.</param>
    ''' <param name="name">The name of the component required.</param>
    ''' <returns>The specified component from in this context, or null if no
    ''' component of the given type and name existed within this context.</returns>
    Public Overridable Function GetComponent( _
     ByVal type As PackageComponentType, ByVal name As String) _
     As PackageComponent Implements IComponentLoadingContext.GetComponent
        For Each comp As PackageComponent In GetAllComponents(type)
            If comp.Name = name Then Return comp
        Next
        Return Nothing
    End Function
End Class
