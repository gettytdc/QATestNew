Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.DependencyInjection

Namespace EnvironmentFunctions

    ''' <summary>
    ''' Function to check if a stop has been requested on the current session
    ''' </summary>
    ''' <remarks>This is usually the result of a "Safe Stop" request by a control rool user</remarks>
    Public Class IsStopRequestedFunction : Inherits IsStopRequestedFunctionBase

        Private ReadOnly mServerFactory As Func(Of IServer)

        Public Sub New(serverFactory As Func(Of IServer))
            mServerFactory = serverFactory
        End Sub

        Protected Overrides Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue

            If IsNothing(process.Session) OrElse process.Session.Identifier.SessionIdentifierType <> SessionIdentifierType.RuntimeResource Then
                Return False
            End If

            Dim sessionId = CType(process.Session.Identifier, RuntimeResourceSessionIdentifier)

            Return mServerFactory().IsStopRequested(sessionId.SessionNumber)
        End Function
    End Class
End Namespace