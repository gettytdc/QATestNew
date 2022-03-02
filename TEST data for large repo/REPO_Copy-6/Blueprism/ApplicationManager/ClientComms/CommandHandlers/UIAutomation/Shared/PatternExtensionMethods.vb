Imports System.Runtime.CompilerServices

Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation.Shared
    ''' <summary>
    ''' Provides extension methods for interacting with UI Automation patterns
    ''' </summary>
    Friend Module PatternExtensionMethods

        ''' <summary>
        ''' Sets the toggle state.
        ''' </summary>
        ''' <param name="pattern">The toggle pattern.</param>
        ''' <param name="state">The state to set it to.</param>
        ''' <returns>The passed toggle pattern</returns>
        <Extension>
        Public Function SetToggle(pattern As ITogglePattern, state As ToggleState) As ITogglePattern
            ' Limiting the number of loops in case the desired state can never be reached
            Dim loopLimiter = 0
            While pattern.CurrentToggleState <> state AndAlso loopLimiter < 5
                pattern.Toggle()
                loopLimiter = loopLimiter + 1
            End While

            Return pattern
        End Function

    End Module
End Namespace