#If UNITTESTS Then
Imports System.Reflection
Imports FluentAssertions.Equivalency

Namespace DataContractRoundTrips

    Public MustInherit Class TestCaseGenerator
        Public MustOverride Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

        ''' <summary>
        ''' Creates an instance of <see cref="RoundTripTestCase(Of TMessage, TState)" />
        ''' </summary>
        ''' <param name="description">A simple description of the message - this is displayed when running the unit test and provides useful information</param>
        ''' <param name="message">The message instance</param>
        ''' <param name="options">Optional configuration of the options used when comparing the original with the roundtripped message</param>
        ''' <param name="after">Action that is executed on the roundtripped object (can be used to initialise it before the comparison is run)</param>
        ''' <param name="serializerType">The serializers to test against</param>
        Public Shared Function Create(Of TMessage)(description As String, message As TMessage,
                                                   Optional options As Func(Of EquivalencyAssertionOptions(Of TMessage), EquivalencyAssertionOptions(Of TMessage)) = Nothing,
                                                   Optional after As Action(Of TMessage) = Nothing,
                                                   Optional serializerType As TestCaseSerializerType = TestCaseSerializerType.Any) _
            As IRoundTripTestCase
            ' RoundTripTestCase supports comparing custom statem but by default just compare the entire message
            Dim getState = Function(m As TMessage) m
            Return New RoundTripTestCase(Of TMessage, TMessage)(description, message, after, getState, options, serializerType)

        End Function

        ''' <summary>
        ''' Creates an instance of <see cref="RoundTripTestCase(Of TMessage, TState)" /> for scenario where
        ''' custom state is obtained from the original and roundtripped objects and compared
        ''' </summary>
        ''' <param name="description">A simple description of the message - this is displayed when running the unit test and provides useful information</param>
        ''' <param name="message">The message instance</param>
        ''' <param name="getState">Gets the state to compare between the original and roundtripped messages. The root objects are compared by default, but this function can be used to provide specific data to compare.</param>
        ''' <param name="options">Optional configuration of the options used when comparing the original with the roundtripped message</param>
        ''' <param name="serializerType">The serializers to test against</param>
        Public Shared Function CreateWithCustomState(Of TMessage, TState)(description As String, message As TMessage,
                                                                          getState As Func(Of TMessage, TState),
                                                                          Optional options As Func(Of EquivalencyAssertionOptions(Of TState), EquivalencyAssertionOptions(Of TState)) = Nothing,
                                                                          Optional serializerType As TestCaseSerializerType = TestCaseSerializerType.Any) _
            As IRoundTripTestCase

            Return New RoundTripTestCase(Of TMessage, TState)(description, message, Nothing, getState, options, serializerType)

        End Function

        ''' <summary>
        ''' Gets the value of a non-public field for a specified object.
        ''' </summary>
        ''' <param name="instance">The object containing the field</param>
        ''' <param name="name">The name of the non-public field</param>
        ''' <returns>The value of the non-public field</returns>
        Public Shared Function GetField(instance As Object, name As String) As Object
            Dim fieldInfo = instance.GetType().GetField(name, BindingFlags.Instance _
                                                              Or BindingFlags.NonPublic)

            If fieldInfo Is Nothing Then Throw New InvalidOperationException("Field not available")

            Return fieldInfo.GetValue(instance)

        End Function

        ''' <summary>
        ''' Sets the value of a non-public field for a specified object.
        ''' </summary>
        ''' <param name="instance">The object containing the field</param>
        ''' <param name="name">The name of the non-public field</param>
        Public Shared Sub SetField(instance As Object, name As String, value As Object)
            Dim fieldInfo = instance.GetType().GetField(name, BindingFlags.Instance _
                                                              Or BindingFlags.NonPublic)

            If fieldInfo Is Nothing Then Throw New InvalidOperationException("Field not available")

            fieldInfo.SetValue(instance, value)
        End Sub

    End Class

End Namespace
#End If
