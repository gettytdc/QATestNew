#If UNITTESTS Then

Imports System.Linq
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports NUnit.Framework

Namespace WebApis.RequestHandling.BodyContent

    Public Class FileCollectionBodyContentTests

        <Test>
        Public Sub GetInputParameters_ShouldReturnSingleParameter()

            Dim bodyContent = New FileCollectionBodyContent()

            Dim parameterCount = bodyContent.GetInputParameters().Count()

            Assert.That(parameterCount, Iz.EqualTo(1))
        End Sub

        <Test>
        Public Sub GetInputParameters_ShouldReturnParameterWithCorrectSchema()

            Dim bodyContent = New FileCollectionBodyContent()

            Dim parameterSchema = bodyContent.
                                    GetInputParameters().
                                    OfType(Of ActionParameterWithCollection).
                                    First().
                                    CollectionInfo

            Assert.That(parameterSchema, [Is].EqualTo(bodyContent.InputSchema))
        End Sub

        <Test>
        Public Sub GetInputParameters_ShouldReturnParameterWithCorrectName()

            Dim bodyContent = New FileCollectionBodyContent()

            Dim parameterName = bodyContent.
                                    GetInputParameters().
                                    First().
                                    Name

            Assert.That(parameterName, [Is].EqualTo(bodyContent.FileInputParameterName))
        End Sub
    End Class

End Namespace

#End If
