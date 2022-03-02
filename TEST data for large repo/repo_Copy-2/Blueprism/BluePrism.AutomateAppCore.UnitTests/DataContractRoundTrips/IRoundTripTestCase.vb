#If UNITTESTS Then
Namespace DataContractRoundTrips

    ''' <summary>
    ''' Contains an example request or response message used for testing serialization / deserialization
    ''' together with options for controlling the state that is compared, the way in which state is 
    ''' compared and the serializers that should be tested
    ''' </summary>
    ''' <remarks>Main purpose of interface is to make it easier to work with implementations,
    ''' which use generics</remarks>
    Public Interface IRoundTripTestCase
        ''' <summary>
        ''' The type of serializer to test against
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property SerializerType() As TestCaseSerializerType

        ''' <summary>
        ''' Executes the roundtrip test using the specified roundtrip function
        ''' </summary>
        ''' <param name="roundtrip"></param>
        ''' <remarks></remarks>
        Sub Execute(roundtrip As Func(Of Object, Object))
    End Interface

End Namespace
#End If
