#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Resources
Imports FluentAssertions
Imports NUnit.Framework

Namespace Resources

    <TestFixture>
    Public Class DigitalWorkerNameTests

        <TestCase("digitalworker")>
        <TestCase("digitalworker1")>
        <TestCase("digital_worker")>
        <TestCase("digital-worker")>
        Public Sub IsValid_ValidValue_ShouldBeTrue(fullName As String)
            Dim result = DigitalWorkerName.IsValid(fullName)
            result.Should().Be(True)
        End Sub

        <TestCase("digital worker")>
        <TestCase("digitalworker1*")>
        <TestCase("digital.worker")>
        <TestCase("digital:worker")>
        <TestCase(" digital:worker")>
        <TestCase("digital:worker ")>
        <TestCase("")>
        <TestCase(Nothing)>
        Public Sub IsValid_InValidValue_ShouldBeFalse(fullName As String)
            Dim result = DigitalWorkerName.IsValid(fullName)
            result.Should().Be(False)
        End Sub

        <TestCase("digitalworker")>
        <TestCase("digitalworker1")>
        <TestCase("digital_worker")>
        <TestCase("digital-worker")>
        Public Sub Contructor_ValidValue_ShouldInitialiseState(fullName As String)
            Dim result = New DigitalWorkerName(fullName)
            result.FullName.Should().Be(fullName)
        End Sub

        <TestCase("digital worker")>
        <TestCase("digitalworker1*")>
        <TestCase("digital.worker")>
        <TestCase("digital:worker")>
        <TestCase(" digital:worker")>
        <TestCase("digital:worker ")>
        <TestCase("")>
        <TestCase(Nothing)>
        Public Sub Contructor_InvalidValue_ShouldThrow(fullName As String)
            Dim action As Action = Function() New DigitalWorkerName(fullName)
            action.ShouldThrow(Of ArgumentException)()
        End Sub
    End Class

End Namespace
#End If
