Imports BluePrism.AutomateProcessCore

Namespace EnvironmentFunctions

    Public Class GetResourceNameFunction : Inherits GetResourceNameFunctionBase
        
        Private ReadOnly mServerFactory As Func(Of IServer)

        Public Sub New(serverFactory As Func(Of IServer))
            mServerFactory = serverFactory
        End Sub

        Protected Overrides Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue
            If parameters.Any() Then
                Throw GetResourceNameFunctionShouldNotHaveAnyParamsException()
            End If

            If process.Session Is Nothing Then
                Throw New clsFunctionException(My.Resources.GetResourceNameFunction_NotRunning)
            End If

            Dim resourceName = mServerFactory().GetResourceNameFromSessionId(process.Session.ID)

            Return New clsProcessValue(DataType.text, resourceName, True)
        End Function
    End Class
End NameSpace