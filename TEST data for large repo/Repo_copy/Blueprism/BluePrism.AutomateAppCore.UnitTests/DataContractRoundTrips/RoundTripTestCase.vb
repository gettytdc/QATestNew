#If UNITTESTS Then
Imports FluentAssertions
Imports FluentAssertions.Equivalency

Namespace DataContractRoundTrips

    ''' <summary>
    ''' Contains an example request or response message used for testing serialization / deserialization
    ''' together with options for controlling the state that is compared, the way in which state is 
    ''' compared and the serializers that should be tested
    ''' </summary>
    ''' <remarks></remarks>
    Public Class RoundTripTestCase(Of TMessage, TState)
        Implements IRoundTripTestCase

        Private ReadOnly mSerializerType As TestCaseSerializerType
        Private ReadOnly mGetState As Func(Of TMessage, TState)
        Private ReadOnly mOptions As Func(Of EquivalencyAssertionOptions(Of TState), EquivalencyAssertionOptions(Of TState))
        Private ReadOnly mAfter As Action(Of TMessage)
        Private ReadOnly mMessage As TMessage
        Private ReadOnly mDescription As String

        ''' <summary>
        ''' Creates an instance of <see cref="RoundTripTestCase(Of TMessage, TState)" />
        ''' </summary>
        ''' <param name="description">A simple description of the message - displayed in the test report and test runner</param>
        ''' <param name="message">The message instance</param>
        ''' <param name="serializerType">The serializers to test against</param>
        Public Sub New(description As String, message As TMessage,
                       after As Action(Of TMessage),
                       getState As Func(Of TMessage, TState),
                       options As Func(Of EquivalencyAssertionOptions(Of TState), EquivalencyAssertionOptions(Of TState)),
                       Optional serializerType As TestCaseSerializerType = TestCaseSerializerType.Any)

            mDescription = description
            mMessage = message
            mAfter = If(after, Sub(message2)
                               End Sub)
            mOptions = If(options, Function(options2) options2)
            mGetState = getState
            mSerializerType = serializerType
        End Sub

        Public ReadOnly Property SerializerType As TestCaseSerializerType _
            Implements IRoundTripTestCase.SerializerType
            Get
                Return mSerializerType
            End Get
        End Property

        Public Sub Execute(roundtrip As Func(Of Object, Object)) _
            Implements IRoundTripTestCase.Execute

            Dim roundTripped = DirectCast(roundtrip(mMessage), TMessage)
            mAfter(roundTripped)

            Dim originalState = mGetState(mMessage)
            Dim roundTrippedState = mGetState(roundTripped)
            roundTrippedState.ShouldBeEquivalentTo(originalState, Function(options) ApplyOptions(options))

        End Sub

        Public Function ApplyOptions(options As EquivalencyAssertionOptions(Of TState)) _
            As EquivalencyAssertionOptions(Of TState)
            Return mOptions(options)
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("{0} - {1}", mMessage.GetType().Name, mDescription)
        End Function

    End Class

End Namespace
#End If
