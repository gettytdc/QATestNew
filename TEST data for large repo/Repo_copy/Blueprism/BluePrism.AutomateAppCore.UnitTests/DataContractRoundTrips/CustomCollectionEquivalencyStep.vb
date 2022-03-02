#If UNITTESTS Then
Imports FluentAssertions.Equivalency

Namespace DataContractRoundTrips

    ''' <summary>
    ''' Extends an equivalency step with additional structural equality comparison applied by StructuralEqualityEquivalencyStep.
    ''' Used to compare instances of custom collections within the application that inherit from a collection type.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CustomCollectionEquivalencyStep(Of TInner As {IEquivalencyStep, New})
        Implements IEquivalencyStep

        Private ReadOnly mInner As New TInner
        Private ReadOnly mStructuralStep As New StructuralEqualityEquivalencyStep

        Public Function CanHandle(context As IEquivalencyValidationContext, config As IEquivalencyAssertionOptions) As Boolean Implements IEquivalencyStep.CanHandle
            Return mInner.CanHandle(context, config)
        End Function

        Public Function Handle(context As IEquivalencyValidationContext, parent As IEquivalencyValidator, config As IEquivalencyAssertionOptions) As Boolean Implements IEquivalencyStep.Handle
            Dim handled = mInner.Handle(context, parent, config)
            If handled Then
                RunStructuralStep(context, parent, config)
            End If
            Return handled
        End Function

        Private Sub RunStructuralStep(context As IEquivalencyValidationContext, parent As IEquivalencyValidator, config As IEquivalencyAssertionOptions)

            If (context.Subject Is Nothing) Then
                Return
            End If

            Dim ns = context.Subject.GetType().Namespace
            If ns IsNot Nothing AndAlso ns.StartsWith("BluePrism") Then
                mStructuralStep.Handle(context, parent, config)
            End If

        End Sub

    End Class

End Namespace
#End If
