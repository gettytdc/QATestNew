Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation.Shared

    ''' <summary>
    ''' Shared utility functions for scrolling within UIAutomation elements
    ''' </summary>
    Public Module ScrollHelper
    
        ''' <summary>
        ''' Helper method to determine the scroll direction and amount from the query
        ''' passed into the original Execution method.
        ''' </summary>
        ''' <param name="query"></param>
        ''' <returns> A scroll amount for either vertical or horizontal.</returns>
        Public Function GetScrollAmountFromQuery(query As clsQuery) As ScrollAmount

            Dim scrollUp = query.GetBoolParam(clsQuery.ParameterNames.ScrollUp)
            Dim bigStep = query.GetBoolParam(clsQuery.ParameterNames.BigStep)

            Return If(scrollUp, If(bigStep, ScrollAmount.LargeDecrement, ScrollAmount.SmallDecrement),
                      If(bigStep, ScrollAmount.LargeIncrement, ScrollAmount.SmallIncrement))
        End Function

    End Module
End NameSpace