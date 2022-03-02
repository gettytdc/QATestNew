Namespace CommandHandling

    ''' <summary>
    ''' Contains information about an ICommandHandler
    ''' </summary>
    Friend Class HandlerDescriptor

        ''' <summary>
        ''' Creates a new HandlerDescriptor
        ''' </summary>
        ''' <param name="handlerType">The concrete type of the ICommandHandler</param>
        ''' <param name="commandId">A unique identifier for the command</param>
        Public Sub New(handlerType As Type, commandId As String)
            If handlerType Is Nothing Then
                Throw New ArgumentNullException(NameOf(handlerType))
            End If
            If Not GetType(ICommandHandler).IsAssignableFrom(handlerType) Then
                Throw New ArgumentException(String.Format(My.Resources.x0DoesNotImplement1, handlerType, GetType(ICommandHandler)), NameOf(Type))
            End If

            If String.IsNullOrWhiteSpace(commandId) Then
                Throw New ArgumentException(My.Resources.InvalidId, NameOf(commandId))
            End If

            Me.HandlerType = handlerType
            Me.CommandId = commandId
        End Sub

        ''' <summary>
        ''' The type of handler used to execute the command
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HandlerType as Type

        ''' <summary>
        ''' A unique identifier for the command
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CommandId as String

    End Class
End NameSpace