#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis

Namespace DataContractRoundTrips.Generators


    Public Class CodePropertiesTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim codeProperties = New CodeProperties("global code",
                                                    CodeLanguage.CSharp,
                                                    {"Namespace1", "Namespace2"},
                                                    {"System.dll", "System.Data.dll"})
            Yield Create("Full", codeProperties)
        End Function

    End Class

End Namespace
#End If
