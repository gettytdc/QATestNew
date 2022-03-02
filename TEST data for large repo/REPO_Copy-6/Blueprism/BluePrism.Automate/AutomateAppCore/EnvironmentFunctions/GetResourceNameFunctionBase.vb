Imports BluePrism.AutomateAppCore.EnvironmentFunctions
Imports BluePrism.AutomateProcessCore

Public MustInherit Class GetResourceNameFunctionBase : Inherits EnvironmentFunction
    
    Public NotOverridable Overrides ReadOnly Property DataType() As DataType
        Get
            Return DataType.text
        End Get
    End Property

    Public NotOverridable Overrides ReadOnly Property HelpText() As String
        Get
            Return My.Resources.GetResourceNameFunction_GetTheNameOfTheResourceRunningTheCurrentProcess
        End Get
    End Property
        
    Public NotOverridable Overrides ReadOnly Property Name() As String
        Get
            Return "GetResourceName"
        End Get
    End Property
        
    Public NotOverridable Overrides ReadOnly Property ShortDesc() As String
        Get
            Return My.Resources.GetResourceNameFunction_GetResourceName
        End Get
    End Property

    Protected Overrides MustOverride Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue

    Public Overridable ReadOnly Property GetResourceNameFunctionShouldNotHaveAnyParamsException() As clsFunctionException
        Get
            Return New clsFunctionException(My.Resources.GetResourceNameFunction_GetResourceNameFunctionShouldNotHaveAnyParameters)
        End Get
    End Property
        
End Class
