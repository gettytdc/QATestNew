Namespace Auth

    ''' <summary>
    ''' This interface defines the Permission function that should be implemented
    ''' on all controls that have a permission associated with them
    ''' </summary>
    Public Interface IPermission

        ''' <summary>
        ''' The required permissions for this implementation. This will never return a
        ''' null collection, but may return an empty collection which indicates that no
        ''' specific permissions are required (ie. that anyone has access to this
        ''' implementing object).
        ''' </summary>
        ''' <value>The collection of required permissions for this component.</value>
        ReadOnly Property RequiredPermissions() As ICollection(Of Permission)

    End Interface

End Namespace