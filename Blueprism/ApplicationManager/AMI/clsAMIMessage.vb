Imports BluePrism.Server.Domain.Models
Imports My.Resources

Namespace BluePrism.ApplicationManager.AMI
    Public Class clsAMIMessage

        ''' <summary>
        ''' The different types of message.
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum MessageTypes
            ''' <summary>
            ''' Default message type. Indicates a failure.
            ''' </summary>
            [Error] = 0
            ''' <summary>
            ''' Indicates that the message is for information
            ''' only.
            ''' </summary>
            Info = 1
            ''' <summary>
            ''' Indicates that the message is a warning, (as 
            ''' opposed to an error for example).
            ''' </summary>
            Warning = 2
        End Enum

        ''' <summary>
        ''' The different prefixes used to identify
        ''' parts of a message.
        ''' </summary>
        Private Enum MessageIdentifiers
            [ERROR]
            HELPTOPIC
            WARNING
        End Enum

        ''' <summary>
        ''' Some common messages.
        ''' </summary>
        ''' <remarks>Enumerated here to save retyping
        ''' the help text and help topic.</remarks>
        Public Enum CommonMessages
            ''' <summary>
            ''' Message returned to users after attempt to perform
            ''' an action that requires the target application to be
            ''' connected but no connection is established.
            ''' </summary>
            NotConnected
            ''' <summary>
            ''' Message returned when attempt is made to interact with a target
            ''' application (eg launch or terminate it) when no target application
            ''' information has been set.
            ''' </summary>
            NoTargetAppInformation
        End Enum


        Public Sub New(ByVal Message As String)
            Me.MessageType = MessageTypes.Error
            Me.Message = Message
        End Sub


        Public Sub New(ByVal Message As String, ByVal HelpTopic As Integer)
            Me.New(Message)
            Me.HelpTopic = HelpTopic
        End Sub

        Public Sub New(ByVal MessageType As CommonMessages)
            Select Case MessageType
                Case CommonMessages.NotConnected
                    Me.Message = NotConnected_2
                    Me.HelpTopic = 32798
                Case CommonMessages.NoTargetAppInformation
                    Me.Message = NoTargetApplicationInformationHasBeenSet
                    Me.HelpTopic = 32818
                Case Else
                    Me.Message = String.Format(InternalAMIMisconfigurationUnknownMessageType0, MessageType.ToString)
            End Select

        End Sub

        ''' <summary>
        ''' The different prefixes used to identify
        ''' parts of a message as shown to the user.
        ''' </summary>
        Public Function FriendlyMessageType() As String
            Select Case MessageType
                Case MessageTypes.Warning
                    Return Warning
                Case MessageTypes.Info
                    Return Info
                Case Else
                    Return xError

            End Select

        End Function
        ''' <summary>
        ''' Parses a message by removing the prefix(es) and splitting
        ''' the compound string into its components.
        ''' </summary>
        ''' <param name="CompoundMessage">The complicated message string
        ''' to be broken up into components (eg message text and help
        ''' topic).</param>
        ''' <returns>Returns a new AMI Message object through which
        ''' the components of the message can be accessed individually.
        ''' </returns>
        ''' <remarks>Use the constructor instead if your message if 
        ''' parsing is not required.</remarks>
        Public Shared Function Parse(ByVal CompoundMessage As String) As clsAMIMessage
            Dim RetVal As New clsAMIMessage("", -1)
            Dim Components As String() = Split(CompoundMessage, BluePrism.AMI.clsAMI.MessageDelimiter)

            For Each s As String In Components
                Select Case True

                    Case s.StartsWith(MessageIdentifiers.HELPTOPIC.ToString)

                        Dim LabelLength As Integer = MessageIdentifiers.HELPTOPIC.ToString.Length
                        s = s.Substring(LabelLength, s.Length - LabelLength).Trim
                        If s.StartsWith(":") Then s = s.Substring(1, s.Length - 1)
                        Try
                            RetVal.HelpTopic = Integer.Parse(s)
                        Catch
                            'do nothing
                        End Try


                    Case s.StartsWith(MessageIdentifiers.ERROR.ToString)
                        RetVal.MessageType = MessageTypes.Error
                        Dim LabelLength As Integer = MessageIdentifiers.ERROR.ToString.Length
                        s = s.Substring(LabelLength, s.Length - LabelLength).Trim
                        If s.StartsWith(":") Then s = s.Substring(1, s.Length - 1)
                        RetVal.Message = s

                    Case s.StartsWith(MessageIdentifiers.WARNING.ToString)
                        RetVal.MessageType = MessageTypes.Warning
                        Dim LabelLength As Integer = MessageIdentifiers.WARNING.ToString.Length
                        s = s.Substring(LabelLength, s.Length - LabelLength).Trim
                        If s.StartsWith(":") Then s = s.Substring(1, s.Length - 1)
                        RetVal.Message = s

                    Case Else
                        RetVal.Message = s

                End Select
            Next

            Return RetVal
        End Function
        ''' <summary>
        ''' The message to be shown to the user.
        ''' </summary>
        ''' <remarks></remarks>
        Public Message As String

        ''' <summary>
        ''' The type of message.
        ''' </summary>
        Public MessageType As MessageTypes

        ''' <summary>
        ''' The number of a helptopic. Defaults to -1.
        ''' </summary>
        Public HelpTopic As Integer = -1

    End Class

    ''' <summary>
    ''' Exception wrapping an AMI Message
    ''' </summary>
    <Serializable>
    Public Class AMIException : Inherits BluePrismException

        ' The message which forms the exception
        Private mMessage As clsAMIMessage

        ''' <summary>
        ''' Creates a new AMIException consisting of the given message.
        ''' </summary>
        ''' <param name="msg">The AMI message which initiated this exception</param>
        Public Sub New(ByVal msg As clsAMIMessage)
            MyBase.New(msg.Message)
            mMessage = msg
        End Sub

        ''' <summary>
        ''' Creates a new AMI Exception consisting of the AMI message represented by the
        ''' given string
        ''' </summary>
        ''' <param name="rawMsg">The string, returned from an AMI query, which can be
        ''' parsed into an AMI Message</param>
        Friend Sub New(ByVal rawMsg As String)
            Me.New(clsAMIMessage.Parse(rawMsg))
        End Sub

        ''' <summary>
        ''' The AMI Message which caused this exception to be thrown
        ''' </summary>
        Public ReadOnly Property AMIMessage() As clsAMIMessage
            Get
                Return mMessage
            End Get
        End Property

    End Class
End Namespace
