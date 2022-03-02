Imports BluePrism.BPCoreLib

Imports NUnit.Framework

#If UNITTESTS Then
Namespace Utilities
    <TestFixture()>
    Public Class ExtensionTests
        <Test>
        Public Sub TestCapitalizeWord()
            'Arrange
            Dim word = "word"
            Dim expected = "Word"
            'Act
            Dim result = word.Capitalize
            'Assert
            Assert.AreEqual(expected, result)
        End Sub
        <Test>
        Public Sub TestCapitalizeSingleLetterWord()
            'Arrange
            Dim word = "w"
            Dim expected = "W"
            'Act
            Dim result = word.Capitalize
            'Assert
            Assert.AreEqual(expected, result)
        End Sub

        <Test>
        Public Sub TestCapitalizeEmptyString()
            'Arrange
            Dim word = String.Empty
            Dim expected = String.Empty
            'Act
            Dim result = word.Capitalize
            'Assert
            Assert.AreEqual(expected, result)
        End Sub
    End Class
End Namespace
#End If
