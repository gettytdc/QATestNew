''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsWebServiceAction
''' 
''' <summary>
''' This class represents an operation of a webservice, this makes web services
''' operations compatible with business object actions.
''' </summary>
Public Class clsWebServiceAction
    Inherits clsBusinessObjectAction

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()
        MyBase.New()
        mInputMessage = Nothing
        mOutputMessage = Nothing
    End Sub

    ''' <summary>
    ''' holds the namespace of the web service operation.
    ''' This is the namespace of the message we will send to the service.
    ''' </summary>
    Public Property InputMessageNamespace As String

    ''' <summary>
    ''' holds the namespace of the web service operation.
    ''' This is the namespace of the message we will receive from the service.
    ''' </summary>
    Public Property OutputMessageNamespace As String

    ''' <summary>
    ''' Provides access to the inputs of the operation
    ''' </summary>
    Public Property Inputs() As clsArgumentList
        Get
            Return mInputs
        End Get
        Set(ByVal Value As clsArgumentList)
            mInputs = Value
        End Set
    End Property
    Private mInputs As clsArgumentList

    ''' <summary>
    ''' The outputs of the operation - a List of clsArgument instances.
    ''' </summary>
    Public ReadOnly Property Outputs() As clsArgumentList
        Get
            Return mOutputs
        End Get
    End Property
    Private mOutputs As clsArgumentList


    ''' <summary>
    ''' The protocol object used for sending messages.
    ''' </summary>
    ''' <remarks>
    ''' This should be hot swapable with other implementation classes, but currently
    ''' can only be used with clsSoap11 or clsSoap12
    ''' </remarks>
    Public Property Protocol() As clsWebServiceProtocol
        Get
            Return mProtocol
        End Get
        Set(ByVal Value As clsWebServiceProtocol)
            mProtocol = Value
        End Set
    End Property
    Private mProtocol As clsWebServiceProtocol


    ''' <summary>
    ''' Get information about a particular parameter for this action.
    ''' </summary>
    ''' <param name="name">The parameter name.</param>
    ''' <param name="dir">The direction of the parameter.</param>
    ''' <returns>The clsProcessParameter describing the parameter requested, or
    ''' Nothing if there is no such parameter.</returns>
    Public Overrides Function GetParameter(ByVal name As String, ByVal dir As ParamDirection) As clsProcessParameter

        For Each p As clsWebParameter In mWebParameters
            If p.Name = name AndAlso p.Direction = dir Then Return p
        Next
        Return Nothing

    End Function

    ''' <summary>
    ''' Get information about the parameters for this action
    ''' </summary>
    ''' <returns>An array of clsProcessParameter objects describing the parameters.
    ''' </returns>
    Public Overrides Function GetParameters() As IList(Of clsProcessParameter)
        Return mWebParameters
    End Function

    ''' <summary>
    ''' Holds a list of clsWebParameters
    ''' </summary>
    Private mWebParameters As New List(Of clsProcessParameter)()

    ''' <summary>
    ''' Adds a parameter to the operation
    ''' </summary>
    ''' <param name="param"></param>
    Public Overloads Sub AddParameter(ByVal param As clsWebParameter)
        mWebParameters.Add(param)
    End Sub

    ''' <summary>
    ''' Performs the webservice operation by invoking the soap class
    ''' </summary>
    ''' <param name="sServiceLocation"></param>
    Friend Sub DoAction(ByVal sServiceLocation As String)

        mProtocol.Location = sServiceLocation

        ' Do we have an input message, use the matching namespace.
        If Not String.IsNullOrEmpty(Me.InputMessage) Then
            mProtocol.Namespace = InputMessageNamespace
        Else
            mProtocol.Namespace = OutputMessageNamespace
        End If

        mProtocol.Action = Me
        mProtocol.DoAction(mWebParameters, mInputs, mOutputs)

    End Sub


    ''' <summary>
    ''' Get a description of the endpoint for the action.
    ''' </summary>
    ''' <returns>A string containing the endpoint description</returns>
    Public Overrides Function GetEndpoint() As String
        'We don't support endpoints for a web service, so this is
        'blank.
        Return ""
    End Function

    ''' <summary>
    ''' Get a description of the preconditions for the action.
    ''' </summary>
    ''' <returns>A Collection of strings, each describing an precondition
    ''' </returns>
    Public Overrides Function GetPreConditions() As Collection
        'We don't support preconditions for a web service, so this is
        'an empty collection.
        Return New Collection
    End Function


    ''' <summary>
    ''' The name of the input message for this action, as used in the SOAP message,
    ''' or Nothing if one is not defined. This is used for document/literal messages.
    ''' </summary>
    Public Property InputMessage() As String
        Get
            Return mInputMessage
        End Get
        Set(ByVal value As String)
            mInputMessage = value
        End Set
    End Property
    Private mInputMessage As String

    ''' <summary>
    ''' The name of the output message for this action, as used in the SOAP message,
    ''' or Nothing if one is not defined. This is used for document/literal messages.
    ''' </summary>
    Public Property OutputMessage() As String
        Get
            Return mOutputMessage
        End Get
        Set(ByVal value As String)
            mOutputMessage = value
        End Set
    End Property
    Private mOutputMessage As String

End Class
