Option Strict On

Imports System.ServiceModel.Channels
Imports System.ServiceModel.Description
Imports System.ServiceModel.Dispatcher
Imports System.Xml
Imports System.Runtime.Serialization
Imports System.ServiceModel

Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

''' <summary>
''' This class inserts the behaviour into wcf which allows us to intercept response messages.
''' </summary>
''' <remarks></remarks>
Friend Class ClientExceptionHandlingBehaviour
    Implements IEndpointBehavior

    Public Sub AddBindingParameters(serviceEndpoint As ServiceEndpoint, bindingParameters As BindingParameterCollection) Implements IEndpointBehavior.AddBindingParameters
        ' Stub
    End Sub

    Public Sub ApplyClientBehavior(serviceEndpoint As ServiceEndpoint, clientRuntime As ClientRuntime) Implements IEndpointBehavior.ApplyClientBehavior
        ' Inject our logic into the WCF stack.
        clientRuntime.MessageInspectors.Insert(0, New MyExceptionHandlingMessageInspector())
    End Sub

    Public Sub ApplyDispatchBehavior(serviceEndpoint As ServiceEndpoint, endpointDispatcher As EndpointDispatcher) Implements IEndpointBehavior.ApplyDispatchBehavior
        ' Stub
    End Sub

    Public Sub Validate(serviceEndpoint As ServiceEndpoint) Implements IEndpointBehavior.Validate
        ' Stub
    End Sub


End Class

''' <summary>
''' This class intercepts wcf responses and allows us to re-throw the inner exceptions
''' </summary>
''' <remarks></remarks>
Friend Class MyExceptionHandlingMessageInspector : Implements IClientMessageInspector

    ''' <summary>
    ''' This function is called when the response is received from the server 
    ''' </summary>
    ''' <param name="reply">The server's reply to the wcf call</param>
    ''' <param name="correlationState"></param>
    ''' <remarks></remarks>
    Public Sub AfterReceiveReply(ByRef reply As Message, correlationState As Object) _
     Implements IClientMessageInspector.AfterReceiveReply

        ' We're only interested in handling faults
        If Not reply.IsFault Then Return

        ' Create a copy of the original reply to allow default WCF processing
        Dim buffer As MessageBuffer = reply.CreateBufferedCopy(Int32.MaxValue)

        ' Restore the original message
        reply = buffer.CreateMessage()

        ' Create a message fault object and try and get a BPServerFault out of it
        Dim fault As BPServerFault = Nothing
        Dim msgFault = MessageFault.CreateFault(buffer.CreateMessage(), Integer.MaxValue)

        Try
            fault = msgFault.GetDetail(Of BPServerFault)()
        Catch
            ' So the fault doesn't contain a BPServerFault;
            ' We check for an exception instead using the old-fashioned method
        End Try

        If fault IsNot Nothing Then fault.Rethrow()

        ' Get the inner exception out of the message - if there is one.
        Dim e As Exception = ExtractInnerException(buffer)

        ' rethrow, keeping the stacktrace.
        If e IsNot Nothing Then Throw e.RethrowWithStackTrace()

    End Sub

    ''' <summary>
    ''' This is stubbed as we don't want to tinker with the outgoing message
    ''' </summary>
    ''' <param name="request"></param>
    ''' <param name="channel"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function BeforeSendRequest(ByRef request As Message, channel As IClientChannel) As Object Implements IClientMessageInspector.BeforeSendRequest
        Return Nothing
    End Function


    Private Function ExtractInnerException(message As MessageBuffer) As Exception

        ' Get the classname attribute
        Dim classname As String = GetExceptionClassName(message.CreateMessage)

        ' no point in continuing if we don't have the exception class name
        If String.IsNullOrEmpty(classname) Then Return Nothing

        ' Get the inner exception from the message
        Return GetInnerException(classname, message.CreateMessage)

    End Function

    ''' <summary>
    ''' Extract the inner exception from the message 
    ''' </summary>
    ''' <param name="typeName">The type name of the exception, not including the
    ''' assembly reference that the type resides in</param>
    ''' <param name="message">The message in which the exception data resides.
    ''' </param>
    ''' <returns>The exception of the specified type populated with the details
    ''' found in the given message.</returns>
    ''' <exception cref="CommunicationException">If the type of exception could
    ''' not be found in the current App Domain, or if there was some problem which
    ''' occurred while trying to deserialize the incoming exception.</exception>
    Private Function GetInnerException(typeName As String, message As Message) As Exception

        Const detailElementName As String = "Detail"

        Using reader As XmlDictionaryReader = message.GetReaderAtBodyContents()

            ' Find <soap:Detail>
            While reader.Read()
                If reader.NodeType = XmlNodeType.Element AndAlso detailElementName.Equals(reader.LocalName) Then
                    Exit While
                End If
            End While

            ' Move to the contents of <soap:Detail>
            If Not reader.EOF AndAlso reader.Read() Then

                ' Create the serialization engine setting it to be the correct type.
                Dim tp As Type = BPUtil.FindType(typeName)
                If tp Is Nothing Then Return Nothing
                Dim serializer As New DataContractSerializer(tp)

                Try
                    ' Deserialize the fault using the type we think it is 
                    Dim result As Object = serializer.ReadObject(reader)
                    Return TryCast(result, Exception)

                Catch ex As SerializationException
                    Return New CommunicationException("SerializationException", ex)
                End Try
            End If

        End Using
        Return Nothing
    End Function


    ''' <summary>
    ''' Finds an element named className in the given message and returns the value.
    ''' </summary>
    ''' <param name="message"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function GetExceptionClassName(message As Message) As String
        Const ClassNameElementKey As String = "ClassName"

        Using reader As XmlDictionaryReader = message.GetReaderAtBodyContents()

            While reader.Read()
                If reader.NodeType = XmlNodeType.Element AndAlso
                    ClassNameElementKey.Equals(reader.LocalName) Then
                    Return reader.ReadElementString
                End If
            End While
        End Using

        Return String.Empty
    End Function

End Class




