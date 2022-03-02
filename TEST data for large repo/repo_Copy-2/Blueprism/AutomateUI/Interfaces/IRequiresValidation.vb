
''' <summary>
''' Interface which for validating. In the case of invalid state, inform the user.
''' </summary>
Public Interface IRequiresValidation
    ''' <summary>
    ''' Performs validation. If invalid, throw exception with revelant information.
    ''' </summary>
    Sub Validate()
End Interface
