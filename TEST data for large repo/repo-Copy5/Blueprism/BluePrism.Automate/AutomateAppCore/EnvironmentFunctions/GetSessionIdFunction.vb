Imports BluePrism.AutomateProcessCore

Namespace EnvironmentFunctions

    ''' <summary>
    ''' Function to get the session ID of the currently running session
    ''' </summary>
    Public Class GetSessionIdFunction : Inherits EnvironmentFunction

        ''' <summary>
        ''' The return type of the function
        ''' </summary>
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property

        ''' <summary>
        ''' Evaluates this function with the given parameters.
        ''' </summary>
        ''' <param name="parameters">The list of parameters to the function.
        ''' </param>
        ''' <param name="process">The process that this function has been called
        ''' on.</param>
        ''' <returns>The value returned on evaluation of the function.
        ''' </returns>
        ''' <exception cref="clsFunctionException">If any errors occur while
        ''' attempting to evaluate the function.</exception>
        Protected Overrides Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue

            If parameters.Count <> 0 Then
                Throw New clsFunctionException(
                    My.Resources.GetSessionIdFunction_GetSessionIdFunctionShouldNotHaveAnyParameters)
            End If

            'Get the current session ID, if there is one.
            Dim idText As String = ""
            If process.Session IsNot Nothing Then
                idText = process.Session.ID.ToString()
            End If

            Return New clsProcessValue(DataType.text, idText)

        End Function

        ''' <summary>
        ''' Gets the help text associated with this function.
        ''' </summary>
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.GetSessionIdFunction_GetTheIDOfTheSessionRunningTheCurrentProcessOrEmptyTextIfNoSessionIsCurrentlyRu
            End Get
        End Property

        ''' <summary>
        ''' The name of this function.
        ''' </summary>
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "GetSessionId"
            End Get
        End Property

        ''' <summary>
        ''' A short description for this function.
        ''' </summary>
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.GetSessionIdFunction_GetSessionId
            End Get
        End Property
    End Class
End NameSpace