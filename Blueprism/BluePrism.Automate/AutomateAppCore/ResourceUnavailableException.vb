
Imports ResourceDBStatus = BluePrism.AutomateAppCore.clsResourceMachine.ResourceDBStatus

''' ---------------------------------------------------------------------------------
''' Project  : AutomateAppCore
''' Class    : ResourceUnavailableException
''' ---------------------------------------------------------------------------------
''' <summary>
''' Exception thrown if a resource is unavailable for executing a process.
''' </summary>
''' ---------------------------------------------------------------------------------
Public Class ResourceUnavailableException
    Inherits ApplicationException

    ' The name of the resource
    Private mName As String

    ' The current status of the resource
    Private mStatus As ResourceDBStatus

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The name of the resource which is unavilable
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Public ReadOnly Property ResourceName() As String
        Get
            Return mName
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The status of the resource which means that it is unavailable.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Public ReadOnly Property Status() As clsResourceMachine.ResourceDBStatus
        Get
            Return mStatus
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Creates a new exception indicating that the resource with the specified
    ''' name is unavailable, and providing its current status.
    ''' </summary>
    ''' <param name="resourceName">The name of the unavailable resource.</param>
    ''' <param name="status">The status of the resource, which means it is 
    ''' unavailable for executing a process on it.</param>
    ''' -----------------------------------------------------------------------------
    Public Sub New(ByVal resourceName As String, ByVal status As ResourceDBStatus)

        MyBase.New("Resource: '" & resourceName & "' is unavailable - Status is: " & status)

        mName = resourceName
        mStatus = status

    End Sub

End Class
