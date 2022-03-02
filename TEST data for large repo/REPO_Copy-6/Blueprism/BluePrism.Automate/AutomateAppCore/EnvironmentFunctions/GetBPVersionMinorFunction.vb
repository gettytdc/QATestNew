Imports BluePrism.AutomateProcessCore

Namespace EnvironmentFunctions

    Public Class GetBPVersionMinorFunction : Inherits EnvironmentFunction

        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property

        Protected Overrides Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue
            If parameters.Count <> 0 Then
                Throw New clsFunctionException(My.Resources.GetBPVersionMinorFunction_BPVersionMinorFunctionShouldNotHaveAnyParameters)
            End If
            Return New clsProcessValue(DataType.number, Me.GetType.Assembly.GetName.Version.Minor.ToString())
        End Function

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.GetBPVersionMinorFunction_GetTheMinorVersionNumberOfTheRunningBluePrismSoftwareEG5ForVersion35
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "BPVersionMinor"
            End Get
        End Property

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.GetBPVersionMinorFunction_GetMinorVersion
            End Get
        End Property
    End Class
End NameSpace