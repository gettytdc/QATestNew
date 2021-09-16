#If UNITTESTS Then
Namespace DataContractRoundTrips

    ''' <summary>
    ''' The type of serializer used to test a roundtrip test case
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum TestCaseSerializerType
        ''' <summary>
        ''' Tested with all serializers
        ''' </summary>
        Any = 0
        ''' <summary>
        ''' Tested with DataContractSerializer only
        ''' </summary>
        DataContractSerializer = 1
        ''' <summary>
        ''' Tested with NetDataContractSerializer only
        ''' </summary>
        NetDataContractSerializer = 2
        ''' <summary>
        ''' Tested with Binary Serializer only
        ''' </summary>
        BinarySerializer = 3
    End Enum

End Namespace
#End If
