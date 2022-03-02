#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

Namespace DataContractRoundTrips.Generators

    Public Class ProcessDependencyTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim vals = New Object(1) {3, Guid.NewGuid()}
            Dim processDependency = clsProcessDependency.Create("ProcessIDDependency", vals)
            Yield Create("With ID", processDependency)

            Dim elementDependency = New clsProcessElementDependency("Process element dependency 1", Guid.NewGuid())
            elementDependency.AddStage(Guid.NewGuid())

            Yield Create("Simple", elementDependency)

            Dim actionDependency = New clsProcessActionDependency("Process action dependency", "Action name 1")
            Yield Create("Simple", actionDependency)

            Dim parentDependency = New clsProcessParentDependency("Parent 1")
            Yield Create("Simple", parentDependency)

            Dim nameDependency = New clsProcessNameDependency("Process 1")
            Yield Create("Simple", nameDependency)

            Dim idDependency = New clsProcessIDDependency(Guid.NewGuid(), "Process 1")
            Yield Create("Simple", idDependency)

            Dim webServiceDependency = New clsProcessWebServiceDependency("Service 1")
            Yield Create("Simple", webServiceDependency)

            Dim webApiDependency = New clsProcessWebApiDependency("Web Api Service 1")
            Yield Create("Simple", webApiDependency)

            Dim queueDependency = New clsProcessQueueDependency("Queue 1")
            Yield Create("Simple", queueDependency)

            Dim credentialDependency = New clsProcessCredentialsDependency("Credential 1")
            Yield Create("Simple", credentialDependency)

            Dim environmentVarDependency = New clsProcessEnvironmentVarDependency("Env Var 1")
            Yield Create("Simple", environmentVarDependency)

            Dim calendarDependency = New clsProcessCalendarDependency("Calendar 1")
            Yield Create("Simple", calendarDependency)

            Dim fontDependency = New clsProcessFontDependency("Font 1")
            Yield Create("Simple", fontDependency)

            Dim pageDependency = New clsProcessPageDependency(Guid.NewGuid())
            Yield Create("Simple", pageDependency)

            Dim dataItemDependency = New clsProcessDataItemDependency("Data item 1")
            Yield Create("Simple", dataItemDependency)

        End Function

    End Class

End Namespace
#End If
