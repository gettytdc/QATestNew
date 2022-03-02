#If UNITTESTS Then
Imports FluentAssertions
Imports FluentAssertions.Equivalency

Namespace DataContractRoundTrips

    ''' <summary>
    ''' Utility used to wrap IEquivalencySteps instances within an EquivalencyStepCollection with
    ''' CustomCollectionEquivalencySteps
    ''' </summary>
    ''' <remarks></remarks>
    Public Class EquivalencyStepWrapper

        ''' <summary>
        ''' Replaces existing collection IEquivalencySteps in the EquivalencyStepCollection with instances of 
        ''' CustomCollectionEquivalancyStep that extend the collection comparison with structural comparison.
        ''' </summary>
        ''' <param name="steps">The collection of IEquivalencySteps</param>
        Public Shared Sub WrapCollectionSteps(steps As EquivalencyStepCollection)
            Wrap(Of GenericEnumerableEquivalencyStep)(steps)
        End Sub

        ''' <summary>
        ''' Replaces existing IEquivalencyStep in the collection with an instance of CustomCollectionEquivalancyStep
        ''' that wraps the existing step
        ''' </summary>
        ''' <typeparam name="TInnerStep"></typeparam>
        ''' <param name="steps"></param>
        ''' <remarks></remarks>
        Private Shared Sub Wrap(Of TInnerStep As {IEquivalencyStep, New})(steps As EquivalencyStepCollection)

            If (Not steps.Any(Function(s) s.GetType() = GetType(TInnerStep))) Then
                Throw New ArgumentException("A step of type " & GetType(TInnerStep).ToString() & " does not exist in the collection")
            End If

            steps.AddAfter(Of TInnerStep, CustomCollectionEquivalencyStep(Of TInnerStep))()
            steps.Remove(Of TInnerStep)()

        End Sub

    End Class

End Namespace
#End If
