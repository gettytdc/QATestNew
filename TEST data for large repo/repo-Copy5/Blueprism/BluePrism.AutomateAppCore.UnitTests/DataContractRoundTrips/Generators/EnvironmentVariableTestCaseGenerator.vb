#If UNITTESTS
Imports BluePrism.AutomateProcessCore

Namespace DataContractRoundTrips.Generators

    Public Class EnvironmentVariableTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim var As New clsEnvironmentVariable("Path",
                                                  New clsProcessValue("C:\Program Files\Blue Prism Limited"), "The Path")

            ' No need to test any other data types since these will be covered by the
            ' clsProcessValue test cases
            Yield Create("Text Variable", var)

        End Function

    End Class

End Namespace
#End If
