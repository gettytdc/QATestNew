#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports FluentAssertions
Imports NUnit.Framework

Namespace ReleaseManager.Conflicts

    <TestFixture()>
    Public Class ConflictArgumentTests

        <Test()>
        Public Sub CustomTitleOrDefault_WithoutCustomTitle_ReturnsName()

            Dim sut = New ConflictArgument("name", New clsProcessValue())

            sut.CustomTitleOrDefault.Should.Be("name")

        End Sub

        <Test()>
        Public Sub CustomTitleOrDefault_WithCustomTitle_ReturnsCustomTitle()

            Dim sut = New ConflictArgument("name", New clsProcessValue(), "title")

            sut.CustomTitleOrDefault.Should.Be("title")

        End Sub
    
    End Class

End NameSpace

#End If
