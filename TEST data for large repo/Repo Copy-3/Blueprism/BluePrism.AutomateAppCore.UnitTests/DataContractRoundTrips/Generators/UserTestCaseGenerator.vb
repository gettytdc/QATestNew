#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Server.Domain.Models

Namespace DataContractRoundTrips.Generators

    Public Class UserTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            ' Shared data - note that passwordexpiry is set within the
            ' warning interval so that PasswordExpiresSoon property can 
            ' be used to implicitly test that the warning interval is 
            ' roundtripped successfully

            Dim mockDataProviderNativeUser = New DictionaryDataProvider(New Hashtable() From {
                                                                           {"authtype", AuthMode.Native},
                                                                           {"userid", Guid.NewGuid()},
                                                                           {"username", "Donald"},
                                                                           {"created", DateTime.UtcNow},
                                                                           {"expiry", DateTime.UtcNow + TimeSpan.FromDays(7)},
                                                                           {"passwordexpiry", DateTime.UtcNow + TimeSpan.FromDays(1)},
                                                                           {"alerteventtypes", AlertEventType.ProcessRunning},
                                                                           {"alertnotificationtypes", AlertNotificationType.Sound},
                                                                           {"lastsignedin", DateTime.UtcNow - TimeSpan.FromMinutes(35)},
                                                                           {"isdeleted", True},
                                                                           {"passwordexpirywarninginterval", 3},
                                                                           {"locked", True},
                                                                           {"passworddurationweeks", 4}
                                                                           })

            Dim mockDataProviderSystemUser = New DictionaryDataProvider(New Hashtable() From {
                                                                           {"authtype", AuthMode.System},
                                                                           {"userid", Guid.NewGuid()},
                                                                           {"username", "Donald"},
                                                                           {"created", DateTime.UtcNow},
                                                                           {"expiry", DateTime.UtcNow + TimeSpan.FromDays(7)},
                                                                           {"passwordexpiry", DateTime.UtcNow + TimeSpan.FromDays(1)},
                                                                           {"alerteventtypes", AlertEventType.ProcessRunning},
                                                                           {"alertnotificationtypes", AlertNotificationType.Sound},
                                                                           {"lastsignedin", DateTime.UtcNow - TimeSpan.FromMinutes(35)},
                                                                           {"isdeleted", True},
                                                                           {"passwordexpirywarninginterval", 3},
                                                                           {"locked", True},
                                                                           {"passworddurationweeks", 4}
                                                                           })

            Dim user1 = New User(mockDataProviderNativeUser)
            user1.SignedInAt = DateTime.UtcNow
            user1.SubscribedAlerts = AlertEventType.ProcessComplete Or AlertEventType.ProcessFailed Or AlertEventType.ProcessStopped
            user1.AlertNotifications = AlertNotificationType.MessageBox Or AlertNotificationType.PopUp
            Dim role1 = New Role(Role.DefaultNames.SystemAdministrators)
            role1.Add(Permission.CreatePermission(1, Permission.SystemManager.Audit.Alerts))
            role1.Add(Permission.CreatePermission(2, Permission.SystemManager.Audit.AuditLogs))
            Dim role2 = New Role(Role.DefaultNames.ProcessAdministrators)
            role2.Add(Permission.CreatePermission(3, Permission.SystemManager.Audit.BusinessObjectsLogs))
            role2.Add(Permission.CreatePermission(4, Permission.Resources.ControlResource))
            user1.Roles.Add(role1)
            user1.Roles.Add(role2)

            Yield Create("Unlocked user with roles", user1)

            Dim user2 = New User(mockDataProviderNativeUser)
            user2.SignedInAt = DateTime.UtcNow
            Yield Create("No roles", user2)

            Dim user3 = New User(mockDataProviderNativeUser)
            user3.AlertNotifications = AlertNotificationType.Taskbar
            user3.SubscribedAlerts = AlertEventType.ProcessComplete
            Yield Create("AlertNotifications and SubscribedAlerts", user3)

            Dim user4 = New User(mockDataProviderSystemUser)
            Yield Create("System user", user4)

            Dim externalUser = New User(Guid.NewGuid(), "externalDave",
                                        "external.dave@salesforce.com",
                                        "Salesforce", "SAML", New RoleSet())
            Yield Create("External User", externalUser)

            Dim authenticationServerUser = New User(Guid.NewGuid(), "test.testerson", Guid.NewGuid(), "test.testerson")
            Yield Create("Authentication Server User", authenticationServerUser)


        End Function




    End Class

End Namespace
#End If
