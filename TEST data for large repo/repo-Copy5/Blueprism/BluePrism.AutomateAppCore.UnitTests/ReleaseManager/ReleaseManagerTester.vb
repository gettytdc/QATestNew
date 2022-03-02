#If UNITTESTS Then
Imports BluePrism.UnitTesting.TestSupport
Imports NUnit.Framework

Namespace ReleaseManager

    <TestFixture>
    Public Class ReleaseManagerTester


        Private Sub CheckEqual(ByVal orig As PackageComponent, ByVal other As PackageComponent)
            If orig Is other Then Return
            If orig Is Nothing OrElse other Is Nothing Then Assert.Fail("{0} <> {1}", orig, other)
            Assert.That(orig.GetType(), [Is].EqualTo(other.GetType()))
            Assert.That(orig.Name, [Is].EqualTo(other.Name))
            Assert.That(orig.Description, [Is].EqualTo(other.Description))
            Assert.That(orig.Owner, [Is].EqualTo(other.Owner))
        End Sub

        Private Sub CheckEqual(ByVal orig As ComponentGroup, ByVal other As ComponentGroup)
            If orig Is other Then Return
            CheckEqual(DirectCast(orig, PackageComponent), other) ' Test their PackageComponent parts
            Assert.That(orig.ShowMembersInComponentTree, [Is].EqualTo(other.ShowMembersInComponentTree))
            Assert.That(orig.Members, [Is].EquivalentTo(other.Members))
        End Sub

        Private Sub CheckEqual(ByVal orig As clsPackage, ByVal other As clsPackage)
            CheckEqual(DirectCast(orig, ComponentGroup), other)
            Assert.ByVal(orig.Releases, [Is].EquivalentTo(other.Releases))
        End Sub

        <Test>
        Public Sub TestSerializing()

            ' An empty package
            Dim pkg As New clsPackage("Package 1", Date.UtcNow, "user")
            CheckEqual(pkg, ServiceUtil.DoBinarySerializationRoundTrip(pkg))

        End Sub

    End Class

End Namespace
#End If
