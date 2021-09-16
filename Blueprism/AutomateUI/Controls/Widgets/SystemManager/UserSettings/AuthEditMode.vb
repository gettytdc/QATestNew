
''' <summary>
''' The edit modes available for the Auth management control.
''' </summary>
Public Enum AuthEditMode As Integer

    ''' <summary>
    ''' No edit mode set
    ''' </summary>
    Unset = 0

    ''' <summary>
    ''' Manage the roles defined in this environment
    ''' </summary>
    ManageRoles = 1

    ''' <summary>
    ''' Assign the roles to the specified user
    ''' </summary>
    ManageUser = 2

End Enum

