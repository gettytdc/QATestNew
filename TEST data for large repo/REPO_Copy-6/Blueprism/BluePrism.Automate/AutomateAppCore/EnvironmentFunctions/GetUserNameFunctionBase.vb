Imports BluePrism.AutomateAppCore.EnvironmentFunctions
Imports BluePrism.AutomateProcessCore

Public MustInherit Class GetUserNameFunctionBase : Inherits EnvironmentFunction
    
    Public NotOverridable Overrides ReadOnly Property DataType() As DataType
        Get
            Return DataType.text
        End Get
    End Property

    Public NotOverridable Overrides ReadOnly Property HelpText() As String
        Get
            Return My.Resources.GetUserNameFunction_GetTheNameOfTheUserResponsibleForStartingTheCurrentSessionOrEmptyTextIfNoSessio
        End Get
    End Property
        
    Public NotOverridable Overrides ReadOnly Property Name() As String
        Get
            Return "GetUserName"
        End Get
    End Property
        
    Public Overrides ReadOnly Property ShortDesc() As String
        Get
            Return My.Resources.GetUserNameFunction_GetUserName
        End Get
    End Property

    Protected Overrides MustOverride Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue
        
End Class
