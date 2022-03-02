Imports BluePrism.AutomateAppCore.EnvironmentFunctions
Imports BluePrism.AutomateProcessCore

Public MustInherit Class IsStopRequestedFunctionBase : Inherits EnvironmentFunction
    
    Public NotOverridable Overrides ReadOnly Property DataType As DataType
        Get
            Return DataType.flag
        End Get
    End Property

    Public NotOverridable Overrides ReadOnly Property HelpText As String
        Get
            Return My.Resources.IsStopRequestedFunction_ChecksIfASafeStopHasBeenRequestedInTheCurrentSession
        End Get
    End Property

    Public NotOverridable Overrides ReadOnly Property Name As String
        Get
            Return "IsStopRequested"
        End Get
    End Property

    Public NotOverridable Overrides ReadOnly Property ShortDesc As String
        Get
            Return My.Resources.IsStopRequestedFunction_ChecksIfStopRequestedForThisSession
        End Get
    End Property

    Protected Overrides MustOverride Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue
        
End Class
