#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Common.Security

Namespace DataContractRoundTrips.Generators

    Public Class PackageComponentTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim credential = New clsCredential()
            credential.ID = Guid.NewGuid()
            credential.Name = "Test credentials"
            credential.Username = "Frank"
            credential.Password = New SafeString("password123")
            credential.Description = "Test credentials"
            credential.ExpiryDate = New DateTime(2025, 1, 1)
            Dim credentialComponent = New CredentialComponent(Nothing, credential)


            credentialComponent.Description = "A Description"
            credentialComponent.Modifications.Add(PackageComponent.ModificationType.IncomingId, Guid.NewGuid())

            Yield Create("Standard", credentialComponent,
                         Function(options) options.Excluding(Function(c) c.Conflicts) _
                            .Excluding(Function(c) c.AssociatedData) _
                            .Excluding(Function(c) c.ResolutionApplier) _
                            .Excluding(Function(c) c.Members) _
                            .Excluding(Function(c) c.AssociatedCredential))

            Dim dashboardComponent = New DashboardComponent(Nothing, Guid.NewGuid, "Test dashboard")

            Yield Create("Standard", dashboardComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(c) c.Conflicts) _
                            .Excluding(Function(c) c.AssociatedData) _
                            .Excluding(Function(c) c.AssociatedDashboard) _
                            .Excluding(Function(c) c.ImportPermission))

            TestCaseGenerator.SetField(dashboardComponent, "mExistingDashboardID", Guid.NewGuid())
            Yield CreateWithCustomState("Private members", dashboardComponent, Function(c) TestCaseGenerator.GetField(c, "mExistingDashboardID"))

            Dim dataSourceComponent = New DataSourceComponent(Nothing, "Test datasource")
            Yield Create("Standard", dataSourceComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(c) c.Conflicts) _
                            .Excluding(Function(c) c.SQL) _
                            .Excluding(Function(c) c.AssociatedData))

            Dim variable = New clsEnvironmentVariable("test1", New clsProcessValue(DataType.number, "123"), "Test description")
            Dim environmentVariableComponent = New EnvironmentVariableComponent(Nothing, variable)
            Yield Create("Standard", environmentVariableComponent,
                         Function(options) options.Excluding(Function(s) s.Conflicts) _
                            .Excluding(Function(c) c.AssociatedData))

            Dim fontComponent = New FontComponent(Nothing, "Comic Sans", "v1")
            Yield Create("Standard", fontComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) s.Conflicts) _
                            .Excluding(Function(c) c.AssociatedFontData) _
                            .Excluding(Function(c) c.AssociatedData))

            Dim dataProvider = New DictionaryDataProvider(New Hashtable() From {
                                                             {"id", "1"},
                                                             {"name", "Calendar 1"}
                                                             })

            Dim calendarComponent = New CalendarComponent(Nothing, dataProvider)
            Yield Create("Standard", calendarComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) s.Conflicts) _
                            .Excluding(Function(s) s.AssociatedCalendar) _
                            .Excluding(Function(c) c.AssociatedData))


            Dim groupComponent = New GroupComponent(Nothing, 1, "Group 1", PackageComponentType.Process, True)
            Yield Create("Standard", groupComponent,
                         Function(options) options.IgnoringCyclicReferences())

            Dim processComponent = New ProcessComponent(Nothing, Guid.NewGuid(), "Process 1")
            processComponent.Published = True

            Yield Create("Standard", processComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) s.Conflicts) _
                            .Excluding(Function(c) c.AssociatedProcess) _
                            .Excluding(Function(c) c.AssociatedData))

            TestCaseGenerator.SetField(processComponent, "mExistingProcessWithSameNameId", Guid.NewGuid())
            Yield CreateWithCustomState("Private Members", processComponent, Function(c) TestCaseGenerator.GetField(c, "mExistingProcessWithSameNameId"))


            Dim scheduleComponent = New ScheduleComponent(Nothing, 1, "Schedule 1")
            Yield Create("Standard", scheduleComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) s.Conflicts) _
                            .Excluding(Function(s) s.AssociatedSchedule) _
                            .Excluding(Function(c) c.AssociatedData))


            Dim scheduleListComponent = New ScheduleListComponent(Nothing, 1, "Schedule List 1")
            Yield Create("Standard", scheduleListComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(c) c.AssociatedData))

            Dim tileComponent = New TileComponent(Nothing, Guid.NewGuid(), "Tile 1")
            Yield Create("Standard", tileComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) s.Conflicts) _
                            .Excluding(Function(s) s.AssociatedTile) _
                            .Excluding(Function(c) c.AssociatedData))

            TestCaseGenerator.SetField(tileComponent, "mExistingTileID", Guid.NewGuid())
            Yield CreateWithCustomState("Private members", tileComponent, Function(c) TestCaseGenerator.GetField(c, "mExistingTileID"))

            Dim vboComponent = New VBOComponent(Nothing, Guid.NewGuid(), "VBO 1")
            Yield Create("Standard", vboComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) s.Conflicts) _
                            .Excluding(Function(s) s.AssociatedProcess) _
                            .Excluding(Function(c) c.AssociatedData))

            Dim webServiceComponent = New WebServiceComponent(Nothing, Guid.NewGuid(), "Web Service 1")
            Yield Create("Standard", webServiceComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) s.Conflicts) _
                            .Excluding(Function(s) s.AssociatedWebService) _
                            .Excluding(Function(c) c.AssociatedData))

            Dim workQueueComponent = New WorkQueueComponent(Nothing, 1, "Queue 1")
            Yield Create("Standard", workQueueComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) s.Conflicts) _
                            .Excluding(Function(s) s.AssociatedQueue) _
                            .Excluding(Function(c) c.AssociatedData))

            Dim webApiComponent = New WebApiComponent(Nothing, Guid.NewGuid(), "Web API 1")
            Yield Create("Standard", webApiComponent,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) s.Conflicts) _
                            .Excluding(Function(s) s.AssociatedWebApi) _
                            .Excluding(Function(c) c.AssociatedData))


        End Function

    End Class

End Namespace
#End If
