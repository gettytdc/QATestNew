''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsInternalBusinessObjectAction
''' 
''' <summary>
''' This is the base class for all internal business object actions
''' </summary>
Public MustInherit Class clsInternalBusinessObjectAction
    Inherits clsBusinessObjectAction

#Region " Member Variables "

    ' The clsInternalBusinessObject that this action belongs to.
    Protected mParent As clsInternalBusinessObject

    ' A collection of clsArguments representing the action inputs.
    Private mInputs As clsArgumentList

    ' A collection of clsArgument representing the action outputs.
    Private mOutputs As clsArgumentList

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new internal business object action with no parent.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the clsInternalBusinessObjectAction class. 
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        mParent = parent
        mInputs = New clsArgumentList()
        mOutputs = New clsArgumentList()
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The clsInternalBusinessObject that this action belongs to.
    ''' </summary>
    Public Property Parent() As clsInternalBusinessObject
        Get
            Return mParent
        End Get
        Friend Set(ByVal value As clsInternalBusinessObject)
            mParent = value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to a collection of clsArguments representing the inputs.
    ''' </summary>
    ''' <value></value>
    Public Property Inputs() As clsArgumentList
        Get
            Return mInputs
        End Get
        Set(ByVal Value As clsArgumentList)
            mInputs = Value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to a collection of clsArgument representing the action outputs.
    ''' </summary>
    ''' <value></value>
    Public Property Outputs() As clsArgumentList
        Get
            Return mOutputs
        End Get
        Set(ByVal Value As clsArgumentList)
            mOutputs = Value
        End Set
    End Property

#End Region

#Region " Do/Execute Methods "

    ''' <summary>
    ''' Subclasses must override the do function to provide an implementation for the
    ''' internal business object action
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made,
    ''' or Nothing if unknown.</param>
    ''' <param name="ScopeStage">The stage to be used in scope calculations, usually
    ''' the action stage calling the business object.</param>
    ''' <param name="sErr">An Error if the function fails</param>
    ''' <returns>True if successful.</returns>
    ''' <remarks>TODO: I'm going to make this protected so that it's not called by
    ''' mistake, and the outputs collection is thus not cleared.</remarks>
    Public MustOverride Function [Do](ByVal process As clsProcess, ByVal session As clsSession, _
     ByVal ScopeStage As clsProcessStage, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Subclasses must override the do function to provide an implementation for the
    ''' internal business object action
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made,
    ''' or Nothing if unknown.</param>
    ''' <param name="ScopeStage">The stage to be used in scope calculations, usually
    ''' the action stage calling the business object.</param>
    ''' <param name="sErr">An Error if the function fails</param>
    ''' <returns>True if successful.</returns>
    Public Function Execute(ByVal process As clsProcess, ByVal session As clsSession, _
     ByVal ScopeStage As clsProcessStage, ByRef sErr As String) As Boolean
        ' Ensure that the outputs are cleared, ready for the action
        Outputs.Clear()
        Return Me.Do(process, session, ScopeStage, sErr)
    End Function

#End Region

#Region " SendError Methods "

    ''' <summary>
    ''' Returns an error flag, first setting the given recipient to the specified
    ''' message. This just means that an error message can be returned in a
    ''' single line rather than having to do it over two lines. A la:
    ''' <code>
    '''  ' Instead of :
    '''  sErr = "An error message"
    '''  Return False
    '''  ' you just write :
    '''  Return SendError(sErr, "An error message")
    ''' </code>
    ''' </summary>
    ''' <param name="recipient">The output parameter into which the error message
    ''' should be set.</param>
    ''' <param name="msg">The formatted message to set.</param>
    Protected Function SendError(ByRef recipient As String, ByVal msg As String) As Boolean
        recipient = msg
        Return False
    End Function

    ''' <summary>
    ''' Returns an error flag, first setting the given recipient to the specified
    ''' message. This just means that an error message can be returned in a
    ''' single line rather than having to do it over two lines. A la:
    ''' <code>
    '''  ' Instead of :
    '''  sErr = "An error message"
    '''  Return False
    '''  ' you just write :
    '''  Return SendError(sErr, "An error message")
    '''  ' Or using format specifiers
    '''  Return SendError(sErr, "Error on {0:dd/MM/yyy}: {1}", Date.Now, "Another message")
    ''' </code>
    ''' </summary>
    ''' <param name="recipient">The output parameter into which the error message
    ''' should be set.</param>
    ''' <param name="msg">The formatted message to set.</param>
    ''' <param name="args">The args detailing the error message.</param>
    ''' <returns>False</returns>
    Protected Function SendError(ByRef recipient As String, _
     ByVal msg As String, ByVal ParamArray args() As Object) As Boolean
        recipient = String.Format(msg, args)
        Return False
    End Function

#End Region

#Region " AddOutput Methods "

    ''' <summary>
    ''' Helper method to add an arbitrarily typed value to the outputs.
    ''' </summary>
    ''' <param name="name">The name of the argument to add</param>
    ''' <param name="tp">The type of the argument</param>
    ''' <param name="value">The encoded value of the argument to add</param>
    Protected Sub AddOutput(ByVal name As String, ByVal tp As DataType, ByVal value As String)
        AddOutput(name, New clsProcessValue(tp, value))
    End Sub

    ''' <summary>
    ''' Helper method to add a process value to the outputs argument list
    ''' </summary>
    ''' <param name="name">The name of the argument to add</param>
    ''' <param name="value">The value of the argument to add</param>
    ''' <remarks>If null is passed as the value to add, nothing is added to the
    ''' output arguments.</remarks>
    Protected Sub AddOutput(ByVal name As String, ByVal value As clsProcessValue)
        If value Is Nothing Then Return
        Outputs.Add(New clsArgument(name, value))
    End Sub

    ''' <summary>
    ''' Helper method to add a <em>date</em> value to the outputs - note that this
    ''' will disregard any time element of the given date value.
    ''' </summary>
    ''' <param name="name">The name of the output argument to add</param>
    ''' <param name="value">The value of the output argument to add.</param>
    Protected Sub AddOutput(ByVal name As String, ByVal value As Date)
        AddOutput(name, DataType.date, value)
    End Sub

    ''' <summary>
    ''' Helper method to add a date to the outputs.
    ''' This will add an empty date to the outputs if the given date is either
    ''' <see cref="Date.MinValue"/> or <see cref="Date.MaxValue"/>.
    ''' </summary>
    ''' <param name="name">The name of the output to add</param>
    ''' <param name="tp">The type of the output to add.</param>
    ''' <param name="dt">The value of the argument to add.</param>
    Protected Sub AddOutput(ByVal name As String, ByVal tp As DataType, ByVal dt As Date)
        If dt = Date.MinValue OrElse dt = Date.MaxValue _
         Then AddOutput(name, New clsProcessValue(tp)) _
         Else AddOutput(name, New clsProcessValue(tp, dt))
    End Sub

#End Region

End Class