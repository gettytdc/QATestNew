#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore

Namespace DataContractRoundTrips.Generators

    Public Class ProcessDependencyListTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Yield Create("Default", New clsProcessDependencyList())

            Dim pdlShared As New clsProcessDependencyList() With {
                    .IsShared = True}

            Yield Create("Shared", pdlShared)

            Dim pdlPopulated As New clsProcessDependencyList()
            pdlPopulated.Add(New clsProcessNameDependency("Test Process"))
            pdlPopulated.Add(New clsProcessNameDependency("Another test Process"))

            Yield Create("Populated", pdlPopulated)

            Dim pdlRunMode As New clsProcessDependencyList() With {
                    .RunMode = BusinessObjectRunMode.Foreground}

            Yield Create("Run Mode", pdlRunMode)


        End Function

    End Class

End Namespace
#End If
