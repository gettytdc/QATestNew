#If UNITTESTS Then
Imports FluentAssertions
Imports NUnit.Framework

Namespace DataContractRoundTrips

    <TestFixture>
    Public Class CustomCollectionEquivalencyStepTests

        <SetUp>
        Public Sub SetUp()
            EquivalencyStepWrapper.WrapCollectionSteps(AssertionOptions.EquivalencySteps)
        End Sub

        <TearDown()>
        Public Sub TearDown()
            AssertionOptions.EquivalencySteps.Reset()
        End Sub

        <Test>
        Public Sub Test()

            Dim a = New CustomCollection() With {.Name = "A"}
            a.Add("1")
            a.Add("2")
            a.Add("4")
            Dim b = New CustomCollection() With {.Name = "B"}
            b.Add("1")
            b.Add("2")
            b.Add("3")

            Try
                a.ShouldBeEquivalentTo(b)
            Catch ex As AssertionException
                ' Note that end of list of mismatches has an additional LF character
                ex.Message.Should().Contain("Expected item[2] to be ""3"", but ""4"" differs near ""4"" (index 0)." _
                                            & vbCrLf _
                                            & "Expected member Name to be ""B"", but ""A"" differs near ""A"" (index 0)." _
                                            & vbCrLf & vbLf)
                Return
            End Try

            Throw New InvalidOperationException("Expected exception not thrown")
        End Sub


        Public Class CustomCollection
            Inherits List(Of String)

            Public Property Name() As String

        End Class

        <Serializable>
        Public Class CustomDictionary
            Inherits Dictionary(Of String, String)

            Public Property Name() As String


        End Class

    End Class


End Namespace
#End If
