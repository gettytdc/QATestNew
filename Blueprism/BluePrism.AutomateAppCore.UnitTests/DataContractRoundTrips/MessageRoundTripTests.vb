#If UNITTESTS Then
Imports System.Reflection
Imports BluePrism.UnitTesting.TestSupport
Imports FluentAssertions
Imports NUnit.Framework

Namespace DataContractRoundTrips

    Public Class MessageRoundTripTests

        <SetUp>
        Public Sub SetUp()
            EquivalencyStepWrapper.WrapCollectionSteps(AssertionOptions.EquivalencySteps)
        End Sub

        <TearDown()>
        Public Sub TearDown()
            AssertionOptions.EquivalencySteps.Reset()
        End Sub

        <Test, TestCaseSource("GetDataContractSerializerTestCases")>
        Public Sub RoundTripUsingDataContractSerializer(ByRef testCase As IRoundTripTestCase)

            testCase.Execute(Function(m) ServiceUtil.DoDataContractRoundTrip(m))

        End Sub

        Public Shared Function GetDataContractSerializerTestCases() As IEnumerable(Of IRoundTripTestCase)

            Return GetTestCases() _
                .Where(Function(tc) tc.SerializerType = TestCaseSerializerType.Any _
                                    Or tc.SerializerType = TestCaseSerializerType.DataContractSerializer)

        End Function

        <Test, TestCaseSource("GetNetDataContractSerializerTestCases")>
        Public Sub RoundTripUsingNetDataContractSerializer(ByRef testCase As IRoundTripTestCase)

            testCase.Execute(Function(m) ServiceUtil.DoNetDataContractRoundTrip(m))

        End Sub

        Protected Shared Function GetNetDataContractSerializerTestCases() As IEnumerable(Of IRoundTripTestCase)

            Return GetTestCases() _
                .Where(Function(tc) tc.SerializerType = TestCaseSerializerType.Any _
                                    Or tc.SerializerType = TestCaseSerializerType.NetDataContractSerializer)

        End Function

        <Test, TestCaseSource("GetBinarySerializerTestCases")>
        Public Sub RoundTripUsingBinarySerializer(ByRef testCase As IRoundTripTestCase)

            testCase.Execute(Function(m) ServiceUtil.DoBinarySerializationRoundTrip(m))

        End Sub

        Protected Shared Function GetBinarySerializerTestCases() As IEnumerable(Of IRoundTripTestCase)

            Return GetTestCases() _
                .Where(Function(tc) tc.SerializerType = TestCaseSerializerType.Any _
                                    Or tc.SerializerType = TestCaseSerializerType.BinarySerializer)

        End Function

        Public Shared Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            ' Optional filter used during development to filter by generator type or 
            ' description shown in test runner
            Dim generatorType As String = Nothing
            Dim testCaseFilter As String = Nothing
            Dim generators = GetGenerators().
                    Where(Function(g) generatorType Is Nothing OrElse g.GetType.Name.StartsWith(generatorType))

            ' Handle errors that occur during test case creation and rethrow with 
            ' additional information - the information displayed by NUnit when
            ' an exception occurs in the test case method does not make it easy
            ' to determine the cause of the exception
            For Each generator In generators
                Try
                    For Each testCase In generator.GetTestCases()
                        If testCaseFilter Is Nothing OrElse testCase.ToString().StartsWith(testCaseFilter) Then
                            Yield testCase
                        End If
                    Next
                Catch exception As Exception
                    Throw New InvalidOperationException(String.Format("An error occurred within {0} while generating test cases: {1}", generator.GetType().Name, exception.ToString()), exception)
                End Try
            Next

        End Function

        ''' <summary>
        ''' Gets sequence of TestCaseGenerator instances by instantiating each class
        ''' that implements TestCaseGenerator within the current assembly
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function GetGenerators() As IEnumerable(Of TestCaseGenerator)

            Dim types = Assembly.GetExecutingAssembly().GetExportedTypes().Where(Function(t) IsGenerator(t))
            Return types.Select(Function(t) Activator.CreateInstance(t)).OfType(Of TestCaseGenerator)()

        End Function

        Private Shared Function IsGenerator(ByRef type As Type) As Boolean
            Dim generatorType As Type = GetType(TestCaseGenerator)
            Return type.IsClass And Not type.IsAbstract And generatorType.IsAssignableFrom(type)
        End Function

    End Class

End Namespace
#End If
