Namespace Resources
    Public Interface IRobotAddressStore
        ReadOnly Property RequireSecureResourceConnection As Boolean
        ReadOnly Property ResourceRegistrationMode As ResourceRegistrationMode
        Function GetRobotAddress(robotName As String) As RobotAddress
        Property Server As IServer
    End Interface
End Namespace
