#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore

Namespace DataContractRoundTrips.Generators

    Public Class ArgumentTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            For Each p In TestHelper.CreateProcessValueDictionary()
                Yield Create(String.Format("{0} process value", p.Key), New clsArgument("arg", p.Value))
            Next

        End Function

    End Class
End Namespace
#End If
