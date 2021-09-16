Imports BluePrism.AutomateProcessCore.clsProcessBreakpoint

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcessBreakpointInfo
''' 
''' <summary>
''' Gives information about a breakpoint which has fired.
''' </summary>
Public Class clsProcessBreakpointInfo

    ''' <summary>
    ''' The breakpoint we are giving info about.
    ''' </summary>
    Private mBreakpoint As clsProcessBreakpoint


    ''' <summary>
    ''' If an error was encountered while processing the breakpoint (for example, the
    ''' condition could not be evaluated), then this is an error message describing
    ''' it. Otherwise it is Nothing, signifying no error.
    ''' </summary>
    Private mErrorMessage As String


    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="breakpoint">The breakpoint to give info about.</param>
    ''' <param name="breakpointType">The breakpoint type.</param>
    ''' <param name="errMsg">If an error was encountered while processing the
    ''' breakpoint (for example, the condition could not be evaluated), then this is
    ''' an error message describing it. Otherwise it is Nothing, signifying no
    ''' error.</param>
    Public Sub New(ByVal breakpoint As clsProcessBreakpoint, ByVal breakpointType As BreakEvents, ByVal errMsg As String)
        mBreakpoint = breakpoint
        mErrorMessage = errMsg
        mBreakpointType = breakpointType
    End Sub


    ''' <summary>
    ''' A message summarising why the breakpoint fired.
    ''' </summary>
    Public ReadOnly Property Message() As String
        Get
            Select Case mBreakpointType

                Case BreakEvents.WhenConditionMet
                    If mErrorMessage IsNot Nothing Then Return String.Format(My.Resources.Resources.clsProcessBreakpointInfo_AnErrorOccurredWhileProcessingTheBreakpoint0, mErrorMessage)
                    If Not mBreakpoint.Condition IsNot Nothing AndAlso mBreakpoint.Condition.Length > 0 Then
                        Return My.Resources.Resources.clsProcessBreakpointInfo_TheBreakpointConditionOnThisStageWasMet
                    Else
                        Return My.Resources.Resources.clsProcessBreakpointInfo_ABreakpointWasReached
                    End If

                Case BreakEvents.WhenDataValueChanged
                    Return String.Format(My.Resources.Resources.clsProcessBreakpointInfo_TheDataItem0WasModifiedByTheLastDebugStage, mBreakpoint.OwnerStage.GetName())

                Case BreakEvents.WhenDataValueRead
                    Return String.Format(My.Resources.Resources.clsProcessBreakpointInfo_TheDataItem0WasAccessedByTheLastDebugStage, mBreakpoint.OwnerStage.GetName())

                Case BreakEvents.HandledException
                    Return My.Resources.Resources.clsProcessBreakpointInfo_AnExceptionOccurredWhichWillBeHandledIfYouContinue

                Case Else
                    Return My.Resources.Resources.clsProcessBreakpointInfo_InvalidBreakpointType
            End Select
        End Get
    End Property


    ''' <summary>
    ''' The type of breakpoint raised. This is necessary because we cannot 
    ''' get this information from the breakpoint alone - a data item breakpoint
    ''' may be raised because its value was changed or because its value was
    ''' accessed etc etc. This clarifies the reason why the breakpoint was
    ''' raised.
    ''' </summary>
    Public ReadOnly Property BreakPointType() As BreakEvents
        Get
            Return Me.mBreakpointType
        End Get
    End Property
    Private mBreakpointType As BreakEvents

    ''' <summary>
    ''' Gets whether this breakpoint info object represents a transient break event,
    ''' ie. a 'Run To' break within the process.
    ''' </summary>
    Public ReadOnly Property IsTransient() As Boolean
        Get
            Return ((BreakPointType And BreakEvents.Transient) <> 0)
        End Get
    End Property

    ''' <summary>
    ''' The condition of the breakpoint. If no condition exists returns the string
    ''' "&lt;No condition set&gt;"
    ''' </summary>
    ''' <value>.</value>
    Public ReadOnly Property Condition() As String
        Get
            If Not (Me.mBreakpoint.Condition Is Nothing OrElse Me.mBreakpoint.Condition = "") Then
                Return Me.mBreakpoint.Condition
            Else
                Return My.Resources.Resources.clsProcessBreakpointInfo_NoConditionSet
            End If
        End Get
    End Property


    ''' <summary>
    ''' The stage to which this breakpoint info relates.
    ''' </summary>
    ''' <value>The stage.</value>
    Public ReadOnly Property BreakpointStage() As clsProcessStage
        Get
            Return mBreakpoint.OwnerStage
        End Get
    End Property

End Class
