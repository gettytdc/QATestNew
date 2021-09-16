Imports BluePrism.Core.Resources

Namespace Resources
    ''' <summary>
    ''' Resource Machine that inherits <see cref="ResourceMachine"/> with some specific implementation
    ''' for AppServer Controlled Robots
    ''' </summary>
    Public Class ServerConnectedResourceMachine
        Inherits ResourceMachine

        Public Sub New(ByVal constate As ResourceConnectionState, ByVal name As String, ByVal ID As Guid, ByVal attribs As Core.Resources.ResourceAttribute)
            MyBase.New(constate, name, ID, attribs)
        End Sub

        Public Overrides ReadOnly Property IsConnected As Boolean
            Get
                Return DisplayStatus <> ResourceStatus.Missing AndAlso DisplayStatus <> ResourceStatus.Offline
            End Get
        End Property

        Public Overrides Function CheckResourcePCStatus(ByVal resourceName As String, ByRef errorMessage As String) As Boolean
            If Me IsNot Nothing Then
                If ProvideConnectionState() = ResourceConnectionState.Disconnected Then
                    errorMessage = String.Format(My.Resources.clsResourceMachine_AppServerCannotCommunicate, resourceName)
                ElseIf ConnectionState = ResourceConnectionState.Sleep OrElse (DisplayStatus <> ResourceStatus.Missing AndAlso
                        Not (Attributes = ResourceAttribute.Pool AndAlso DisplayStatus = ResourceStatus.Offline)) Then
                    If DBStatus = ResourceMachine.ResourceDBStatus.Ready Then Return True
                Else
                    errorMessage = String.Format(My.Resources.clsResourceMachine_0IsCurrently1AndIsNotAvailable, resourceName, ResourceInfo.GetResourceStatusFriendlyName(DisplayStatus))
                End If
            Else
                errorMessage = String.Format(My.Resources.clsResourceMachine_FailedToGetStatusOf0ResourceNotFound, resourceName)
            End If
            
            errorMessage = FormatDefaultResourcePcConnectionErrorMessage(resourceName, errorMessage)

            Return False
        End Function
    End Class
End Namespace
