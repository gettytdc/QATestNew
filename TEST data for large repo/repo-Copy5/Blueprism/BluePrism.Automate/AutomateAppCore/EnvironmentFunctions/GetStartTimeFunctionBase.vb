Imports BluePrism.AutomateAppCore.EnvironmentFunctions
Imports BluePrism.AutomateProcessCore

Public MustInherit Class GetStartTimeFunctionBase : Inherits EnvironmentFunction
    
    Public NotOverridable Overrides ReadOnly Property DataType() As DataType
        Get
            Return DataType.datetime
        End Get
    End Property
    
    Public NotOverridable Overrides ReadOnly Property HelpText() As String
        Get
            Return My.Resources.GetStartTimeFunction_GetsTheStartTimeOfThisProcessInstance
        End Get
    End Property
        
    Public NotOverridable Overrides ReadOnly Property Name() As String
        Get
            Return "GetStartTime"
        End Get
    End Property
        
    Public NotOverridable Overrides ReadOnly Property ShortDesc() As String
        Get
            Return My.Resources.GetStartTimeFunction_GetStartTime
        End Get
    End Property

    Protected Overrides MustOverride Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue

    Public Overridable ReadOnly Property MissingSessionException() As clsFunctionException
        Get 
            return New clsFunctionException(My.Resources.GetStartTimeFunction_MissingSessionAtGetStartTime)
        End Get
    End Property

    Public Overridable ReadOnly Property GetStartTimeFunctionSessionNotRunningException() As clsFunctionException
        Get 
            return New clsFunctionException(My.Resources.GetStartTimeFunction_NotRunning)
        End Get
    End Property

    Public Overridable ReadOnly Property GetStartTimeFunctionShouldHaveNoParametersException() As clsFunctionException
        Get 
            Return New clsFunctionException(My.Resources.GetStartTimeFunction_GetStartTimeFunctionShouldNotHaveAnyParameters)
        End Get
    End Property
        
End Class
