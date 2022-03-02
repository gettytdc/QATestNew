Imports BluePrism.AutomateProcessCore

Namespace EnvironmentFunctions
    ''' <summary>
    ''' Class to encapsulate a basic environment function. There's not a huge amount
    ''' of overlap between all of the environment functions, but they are all in the
    ''' "Environment" group, and they take no parameters.
    ''' </summary>
    Public MustInherit Class EnvironmentFunction : Inherits clsFunction

        ''' <summary>
        ''' The group name for this function. The Environment functions are all
        ''' within the 'Environment' group.
        ''' </summary>
        ''' <remarks>
        ''' N.B. This should be translated by it's consumers e.g. AutomateUI.clsExpressionTreeView.GetLocalizedFriendlyName()/>
        ''' </remarks>
        Public NotOverridable Overrides ReadOnly Property GroupName() As String
            Get
                Return "Environment"
            End Get
        End Property

        ''' <summary>
        ''' The default signature - ie. the list of parameters that this function
        ''' expects. Environment functions typically expect no parameters.
        ''' </summary>
        Public NotOverridable Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return clsFunctionParm.EmptySignature
            End Get
        End Property

    End Class
End NameSpace