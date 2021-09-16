Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation.Shared
    ''' <summary>
    ''' Shared utility functions for working with UIA toggle elements
    ''' </summary>
    Public Module ToggleHelper
    
        ''' <summary>
        ''' Sets the state using an ITogglePattern instance
        ''' </summary>
        ''' <param name="pattern"></param>
        ''' <param name="state"></param>
        Public Sub SetState(pattern As ITogglePattern, state As ToggleState)

            Dim loopLimiter = 0
            While pattern.CurrentToggleState <> state AndAlso loopLimiter < 5
                pattern.Toggle()
                loopLimiter = loopLimiter + 1
            End While
        End Sub

    End Module
End NameSpace