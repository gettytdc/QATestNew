Imports BluePrism.AutomateAppCore.Auth

''' <summary>
''' Helper class to check a request has the required permissions 
''' </summary>
Public Interface IPermissionValidator
    ''' <summary>
    ''' Method checks if permissions are valid, elsewise throws a Permission Exception
    ''' </summary>
    ''' <param name="context">The setup context of the request</param>
    Sub EnsurePermissions(context As ServerPermissionsContext)
End Interface
