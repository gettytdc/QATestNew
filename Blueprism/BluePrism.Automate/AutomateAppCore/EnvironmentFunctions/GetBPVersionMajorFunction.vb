Imports BluePrism.AutomateProcessCore

Namespace EnvironmentFunctions

    Public Class GetBPVersionMajorFunction : Inherits EnvironmentFunction

        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property

        Protected Overrides Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue
            If parameters.Count <> 0 Then
                Throw New clsFunctionException(My.Resources.GetBPVersionMajorFunction_BPVersionMajorFunctionShouldNotHaveAnyParameters)
            End If
            Return New clsProcessValue(DataType.number, Me.GetType.Assembly.GetName.Version.Major.ToString())
        End Function

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.GetBPVersionMajorFunction_GetTheMajorVersionNumberOfTheRunningBluePrismSoftwareEG3ForVersion35
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "BPVersionMajor"
            End Get
        End Property

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.GetBPVersionMajorFunction_GetMajorVersion
            End Get
        End Property
    End Class
End NameSpace