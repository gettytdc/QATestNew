Imports BluePrism.AutomateProcessCore

Namespace EnvironmentFunctions
    Public Class IsBPServerFunction : Inherits EnvironmentFunction

        Private ReadOnly mServerFactory As Func(Of IServer)

        Public Sub New(serverFactory As Func(Of IServer))
            mServerFactory = serverFactory
        End Sub

        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.flag
            End Get
        End Property

        Protected Overrides Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue
            If parameters.Count <> 0 Then
                Throw New clsFunctionException(My.Resources.IsBPServerFunction_BPServerFunctionShouldNotHaveAnyParameters)
            End If
            Return New clsProcessValue(DataType.flag, mServerFactory().IsServer.ToString())
        End Function

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.IsBPServerFunction_DetermineIfABluePrismServerIsBeingUsedRatherThanADirectDatabaseConnectionTrueIf
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "BPServer"
            End Get
        End Property

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.IsBPServerFunction_UsingBluePrismServer
            End Get
        End Property
    End Class
End NameSpace