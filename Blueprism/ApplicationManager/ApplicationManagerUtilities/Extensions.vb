Imports System.Runtime.CompilerServices

Imports ComparisonTypes =
 BluePrism.ApplicationManager.ApplicationManagerUtilities.clsMatchTarget.ComparisonTypes

''' <summary>
''' Extension methods for types within AppManUtilities
''' </summary>
Public Module Extensions

    ''' <summary>
    ''' Checks if the given <see cref="ComparisonTypes"/> value is an equality check
    ''' or not (ie. Equal or NotEqual).
    ''' </summary>
    ''' <param name="this">The type value to check</param>
    ''' <returns>True if the given value represents an equal / not-equal comparison;
    ''' false if it represents any other kind of comparison.</returns>
    <Extension>
    Public Function IsEqualityCheck(this As ComparisonTypes) As Boolean
        Return (
            this = ComparisonTypes.Equal OrElse
            this = ComparisonTypes.NotEqual
        )
    End Function

End Module
