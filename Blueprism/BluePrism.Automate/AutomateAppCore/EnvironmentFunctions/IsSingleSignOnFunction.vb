Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore

Namespace EnvironmentFunctions

    Public Class IsSingleSignOnFunction : Inherits EnvironmentFunction
        
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
                Throw New clsFunctionException(My.Resources.IsSingleSignOnFunction_SingleSignonFunctionShouldNotHaveAnyParameters)
            End If

            Dim isSingleSignOn = mServerFactory().DatabaseType() = DatabaseType.SingleSignOn
            Return New clsProcessValue(DataType.flag, isSingleSignOn.ToString())
        End Function

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.IsSingleSignOnFunction_DetermineIfSingleSignonIsBeingUsedRatherThanBluePrismAuthenticationTrueIfSo
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "SingleSignon"
            End Get
        End Property

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.IsSingleSignOnFunction_UsingSingleSignon
            End Get
        End Property
    End Class
End NameSpace