#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Resources

Namespace DataContractRoundTrips.Generators


    Public Class CombinedConfigTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim combinedConfig = New ResourceMachine.CombinedConfig()
            combinedConfig.EnableAllLogging()
            Yield Create("Simple", combinedConfig)


        End Function
    End Class

End Namespace
#End If
