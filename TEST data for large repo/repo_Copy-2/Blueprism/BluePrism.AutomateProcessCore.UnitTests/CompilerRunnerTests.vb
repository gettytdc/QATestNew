#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.UnitTesting.TestSupport
Imports FluentAssertions
Imports Moq
Imports NUnit.Framework

Namespace UnitTests
    Public Class CompilerRunnerTests

        Private Function CreateProcess() As clsProcess
            Return New clsProcess(Mock.Of(Of IGroupObjectDetails), DiagramType.Object, True)
        End Function

        Private Sub ExecuteMethod(stageName As String,
                                  inputs() As clsArgument,
                                  outputs() As clsArgument,
                                  target As Object)

            Dim process = CreateProcess()
            Dim stage As New clsCodeStage(process) With {.Name = stageName}
            Dim runner As New clsCompilerRunner(process)

            Dim errorMessage As String = Nothing

            ReflectionHelper.SetPrivateField("mInstance", runner, target)
            runner.Execute(stage, 
                           New clsArgumentList(inputs), 
                           new clsArgumentList(outputs), 
                           errorMessage)
        End Sub

        <Test>
        Public Sub Execute_ShouldSetParametersFromInputs()
            Dim value1 = New clsProcessValue("value 1")
            Dim value2 = New clsProcessValue("value 2")
            Dim inputs = { New clsArgument("input1", value1),
                           New clsArgument("input1", value2) }
            Dim outputs = { New clsArgument("output1", New clsProcessValue("")),
                            New clsArgument("output2", New clsProcessValue("")) }

            Dim target As New TestClass()
            ExecuteMethod("Sub 1", inputs, outputs, target)

            target.LastInput1.Should.Be(value1.EncodedValue)
            target.LastInput2.Should.Be(value2.EncodedValue)

        End Sub

        <Test>
        Public Sub Execute_ShouldUpdateOutputs()
            Dim value1 = New clsProcessValue("value 1")
            Dim value2 = New clsProcessValue("value 2")
            Dim inputs = { New clsArgument("input1", value1),
                           New clsArgument("input1", value2) }
            Dim outputs = { New clsArgument("output1", New clsProcessValue("")),
                            New clsArgument("output2", New clsProcessValue("")) }

            Dim target As New TestClass()
            ExecuteMethod("Sub 1", inputs, outputs, target)

            outputs(0).Value.Should.Be(value1)
            outputs(1).Value.Should.Be(value2)
        End Sub

        Protected Class TestClass
            Public Property LastInput1 As String

            Public Property LastInput2 As String

            Public Sub Sub_1(input1 As String, input2 As String, ByRef output1 As String, ByRef output2 As String)
                LastInput1 = input1
                LastInput2 = input2
                output1 = input1
                output2 = input2
            End Sub
        End Class
    End Class
End Namespace

#End If