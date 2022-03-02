#If UNITTESTS Then
Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models
Imports BluePrism.UnitTesting
Imports BluePrism.UnitTesting.TestSupport
Imports Moq
Imports NUnit.Framework

Namespace License

    <TestFixture>
    Public Class LicenseTests

        <SetUp>
        Public Sub Setup()
            LegacyUnitTestHelper.SetupDependencyResolver()
        End Sub

        <Test>
        Public Sub GetLicenseActivationRequest_NullArgumentException()
            Dim server = CreateServer(True)
            Assert.Throws(Of ArgumentNullException)(Sub() server.GetLicenseActivationRequest(Nothing, Nothing))
        End Sub

        Private Function GetEffectiveLicenses(keys As ICollection(Of KeyInfo)) As ICollection(Of KeyInfo)
            Dim effectiveLicenses = keys.Where(Function(x) x.Effective)
            If effectiveLicenses.Any() Then
                Return effectiveLicenses.ToList
            Else
                Return {KeyInfo.DefaultLicense}.ToList
            End If
        End Function

        <Test>
        Public Sub GetLicenseActivationRequest_NoDatabaseId()
            Dim server = CreateServer(True)
            Dim database = CreateDatabase(False, False, True, True, False, False)
            Assert.Throws(Of BluePrismException)(Sub() server.GetLicenseActivationRequest(database.Object, New KeyInfo(CType(Nothing, String))))
        End Sub

        <Test>
        Public Sub GetLicenseActivationRequest_DatabaseIdErrorWithNoEnvironment()
            Dim server = CreateServer(True)
            Dim database = CreateDatabase(True, False, True, True, False, False)
            Assert.Throws(Of BluePrismException)(Sub() server.GetLicenseActivationRequest(database.Object, New KeyInfo(CType(Nothing, String))))
        End Sub

        <Test>
        Public Sub GetLicenseActivationRequest_ReadError()
            Dim server = CreateServer(True)
            Dim database = Me.CreateDatabase(False, False, False, False, True, False)
            Assert.Throws(Of BluePrismException)(Sub() server.GetLicenseActivationRequest(database.Object, New KeyInfo(TestXml)))
        End Sub

        <Test>
        Public Sub GetLicenseActivationRequest_ServerPathNull()
            Dim server = Me.CreateServer(True)
            Dim result = server.GetPathForLicenseActivationRequest(Nothing, "")
            Assert.AreEqual(result, "\")
        End Sub

        <Test>
        Public Sub GetLicenseActivationRequest_ServerPathOKForSize()
            Dim server = Me.CreateServer(True)
            Dim serverName As String = "servername"
            Dim databaseName As String = "databasename"
            Dim result = server.GetPathForLicenseActivationRequest(serverName, databaseName)
            Assert.AreEqual(result, serverName + "\" + databaseName)
        End Sub

        <Test>
        Public Sub GetLicenseActivationRequest_ServerPathExceedsSize()
            Dim server = Me.CreateServer(True)

            Dim serverName As String = Nothing
            Dim databaseName As String = Nothing
            Dim servName As New System.Text.StringBuilder
            Dim dbName As New System.Text.StringBuilder
            For i As Integer = 0 To 999
                servName.Append("a")
                dbName.Append("b")
            Next
            serverName = servName.ToString()
            databaseName = dbName.ToString()

            Dim whatResultShouldBe As String = Nothing
            Dim tmp As New System.Text.StringBuilder
            For i As Integer = 0 To 99
                tmp.Append("a")
            Next
            whatResultShouldBe = tmp.ToString()
            tmp = New System.Text.StringBuilder
            For i As Integer = 0 To 49
                tmp.Append("b")
            Next
            whatResultShouldBe = whatResultShouldBe + "\" + tmp.ToString()


            Dim result = server.GetPathForLicenseActivationRequest(serverName, databaseName)

            Assert.IsTrue(result.Length = 151)
            Assert.AreEqual(result, whatResultShouldBe)
        End Sub

        <Test>
        Public Sub GetLicenseActivationRequest()
            Dim server = CreateServer(True)
            Dim database = Me.CreateDatabase(True, False, False, True, False, False)
            Dim result = server.GetLicenseActivationRequest(database.Object, New KeyInfo(Licence2017))
            Assert.IsNotNull(result)
            Assert.AreEqual(":"c, result(1))
            database.Verify(Sub(rl) rl.BeginTransaction(), Times.Once)
            database.Verify(Function(rl) rl.ExecuteReturnDataReader(Moq.It.IsAny(Of SqlCommand)), Times.Exactly(3))
            database.Verify(Sub(rl) rl.Execute(Moq.It.IsAny(Of SqlCommand)), Times.Once)
            database.Verify(Sub(rl) rl.CommitTransaction(), Times.Once)
        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_SingleLicenseNoActivationIsInDate_ReturnsLicense()

            Dim key As KeyInfo = New Key With {
                    .Starts = New Date(2016, 11, 14),
                    .Expires = New Date(2121, 12, 21),
                    .KeyId = 20,
                    .RequiresActivation = False
                    }
            Dim keys = {key}

            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({key}, result)

        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_SingleLicense_NoActivation_NotStarted_ReturnsDefaultLicense()

            Dim key As KeyInfo = New Key With {
                    .Starts = New Date(2120, 11, 14),
                    .Expires = New Date(2121, 12, 21),
                    .KeyId = 8,
                    .RequiresActivation = False
                    }
            Dim keys = {key}

            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({KeyInfo.DefaultLicense}, result)

        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_SingleLicenseNoActivation_HasExpired_ReturnsDefaultLicense()

            Dim key As KeyInfo = New Key With {
                    .Starts = New Date(2018, 11, 14),
                    .Expires = New Date(2018, 12, 21),
                    .KeyId = 7,
                    .RequiresActivation = False
                    }
            Dim keys = {key}


            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({KeyInfo.DefaultLicense}, result)
        End Sub

        'Activated
        <Test>
        Public Sub GetEffectiveLicenses_SingleLicenseActivated_IsInDate_NotInGrace_ReturnsLicense()

            Dim startDate = New Date(2018, 11, 14)
            Dim key As KeyInfo = New Key With {
                    .Starts = startDate,
                    .Expires = New Date(2100, 12, 21),
                    .KeyId = 21,
                    .GracePeriod = GetGracePeriod(startDate, False),
                    .RequiresActivation = True,
                    .Activated = True
                    }
            Dim keys = {key}

            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({key}, result)

        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_SingleLicenseActivated_NotStarted_NotInGrace_ReturnsDefaultLicense()

            Dim startDate = New Date(2121, 11, 14)
            Dim key As KeyInfo = New Key With {
                    .Starts = startDate,
                    .Expires = New Date(2122, 12, 21),
                    .KeyId = 10,
                    .GracePeriod = GetGracePeriod(startDate, False),
                    .RequiresActivation = True
                    }
            Dim keys = {key}


            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({KeyInfo.DefaultLicense}, result)

        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_SingleLicenseActivated_HasExpired_ReturnsDefaultLicense()

            Dim startDate = New Date(2018, 11, 14)
            Dim key As KeyInfo = New Key With {
                    .Starts = startDate,
                    .Expires = New Date(2018, 12, 21),
                    .KeyId = 15,
                    .RequiresActivation = True,
                    .GracePeriod = GetGracePeriod(startDate, False)
                    }
            Dim keys = {key}

            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({KeyInfo.DefaultLicense}, result)
        End Sub

        'NotActivated
        <Test>
        Public Sub GetEffectiveLicenses_SingleLicense_NotActivated_IsInDate_IsInGracePeriod_ReturnsLicense()

            Dim startDate = New Date(2018, 11, 14)
            Dim key As KeyInfo = New Key With {
                    .Starts = startDate,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate, True),
                    .KeyId = 22,
                    .RequiresActivation = True
                    }
            Dim keys = {key}


            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({key}, result)
        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_SingleLicense_NotActivated_IsInDate_ButNotInGracePeriod_ReturnsDefaultLicense()

            Dim startDate = New Date(2018, 11, 14)
            Dim key As KeyInfo = New Key With {
                    .Starts = startDate,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate, False),
                    .KeyId = 22,
                    .RequiresActivation = True
                    }
            Dim keys = {key}

            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({KeyInfo.DefaultLicense}, result)

        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_SingleLicense_NotActivated_NotStarted_IsWithinGracePeriod_ReturnsDefaultLicense()

            Dim startDate = New Date(2100, 11, 14)
            Dim key As KeyInfo = New Key With {
                    .Starts = startDate,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate, True),
                    .KeyId = 22,
                    .RequiresActivation = True
                    }
            Dim keys = {key}


            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({KeyInfo.DefaultLicense}, result)

        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_SingleLicense_NotActivated_HasExpired_IsWithinGracePeriod_ReturnsDefaultLicense()

            Dim startDate = New Date(2018, 11, 14)
            Dim key As KeyInfo = New Key With {
                    .Starts = startDate,
                    .Expires = New Date(2018, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate, True),
                    .KeyId = 22,
                    .RequiresActivation = True
                    }
            Dim keys = {key}

            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({KeyInfo.DefaultLicense}, result)
        End Sub

        'AwaitingResponse
        <Test>
        Public Sub GetEffectiveLicenses_SingleLicense_AwaitingResponse_IsInDate_IsInGracePeriod_ReturnsLicense()

            Dim startDate = New Date(2019, 1, 1)
            Dim key As KeyInfo = New Key With {
                    .Starts = startDate,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate, True),
                    .KeyId = 22,
                    .RequiresActivation = True
                    }
            Dim keys = {key}


            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({key}, result)
        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_SingleLicense_AwaitingResponse_IsInDate_ButNotInGracePeriod_ReturnsDefaultLicense()

            Dim startDate = New Date(2018, 11, 14)
            Dim key As KeyInfo = New Key With {
                    .Starts = startDate,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate, False),
                    .KeyId = 22,
                    .RequiresActivation = True
                    }
            Dim keys = {key}

            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({KeyInfo.DefaultLicense}, result)
        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_SingleLicense_AwaitingResponse_NotStarted_IsInGrace_ReturnsDefaultLicense()

            Dim startDate = New Date(2100, 11, 14)
            Dim key As KeyInfo = New Key With {
                    .Starts = startDate,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate, True),
                    .KeyId = 22,
                    .RequiresActivation = True
                    }
            Dim keys = {key}

            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({KeyInfo.DefaultLicense}, result)

        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_SingleLicense_AwaitingResponse_WithinGracePeriod_HasExpired_ReturnsDefaultLicense()

            Dim startDate = New Date(2018, 11, 14)
            Dim key As KeyInfo = New Key With {
                    .Starts = startDate,
                    .Expires = New Date(2018, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate, True),
                    .KeyId = 22,
                    .RequiresActivation = True
                    }
            Dim keys = {key}

            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({KeyInfo.DefaultLicense}, result)
        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_MultipleLicenses_ReturnsOnlyValidLicenses()



            ' one which has expired
            Dim startDate1 = New Date(2018, 11, 14)
            Dim key1 As KeyInfo = New Key With {
                    .Starts = startDate1,
                    .Expires = New Date(2018, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate1, True),
                    .KeyId = 21,
                    .RequiresActivation = True
                    }

            ' one not in grace period and not activated yet
            Dim startDate2 = New Date(2018, 11, 14)
            Dim key2 As KeyInfo = New Key With {
                    .Starts = startDate2,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate2, False),
                    .KeyId = 22,
                    .RequiresActivation = True
                    }

            'some valid ones
            Dim startDate3 = New Date(2018, 11, 14)
            Dim key3 As KeyInfo = New Key With {
                    .Starts = startDate3,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate3, False),
                    .KeyId = 23,
                    .RequiresActivation = True,
                    .Activated = True
                    }

            Dim startDate4 = New Date(2018, 11, 14)
            Dim key4 As KeyInfo = New Key With {
                    .Starts = startDate4,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate4, False),
                    .KeyId = 23,
                    .RequiresActivation = True,
                    .Activated = True
                    }

            Dim startDate5 = New Date(2018, 11, 14)
            Dim key5 As KeyInfo = New Key With {
                    .Starts = startDate5,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate5, False),
                    .KeyId = 23,
                    .RequiresActivation = False,
                    .Activated = True
                    }

            Dim keys = {key1, key2, key3, key4, key5}

            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({key3, key4, key5}, result)
        End Sub

        <Test>
        Public Sub GetEffectiveLicenses_MultipleLicenses_NoneAreValid_ReturnsDefault()

            ' one which has expired
            Dim startDate1 = New Date(2018, 11, 14)
            Dim key1 As KeyInfo = New Key With {
                    .Starts = startDate1,
                    .Expires = New Date(2018, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate1, True),
                    .KeyId = 21,
                    .RequiresActivation = True
                    }

            ' one not in grace period and not activated yet
            Dim startDate2 = New Date(2018, 11, 14)
            Dim key2 As KeyInfo = New Key With {
                    .Starts = startDate2,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate2, False),
                    .KeyId = 22,
                    .RequiresActivation = True
                    }

            'one activated but not started yet
            Dim startDate3 = New Date(2100, 11, 14)
            Dim key3 As KeyInfo = New Key With {
                    .Starts = startDate3,
                    .Expires = New Date(2100, 12, 21),
                    .GracePeriod = GetGracePeriod(startDate3, False),
                    .KeyId = 23,
                    .RequiresActivation = True
                    }

            Dim keys = {key1, key2, key3}

            Dim result = GetEffectiveLicenses(keys)
            Assert.AreEqual({KeyInfo.DefaultLicense}, result)
        End Sub

        <Test>
        Public Sub CheckKeyActivationInfo()
            Dim key As KeyInfo = New Key With {
                    .Starts = New Date(2018, 11, 14),
                    .Expires = New Date(2018, 12, 21),
                    .GracePeriod = 100,
                    .KeyId = 22,
                    .RequiresActivation = True
                    }
            Assert.AreEqual(100, key.GracePeriodDays)
            Assert.AreEqual(True, key.RequiresActivation)
        End Sub

        <Test>
        Public Sub CheckKeyActivationInfo_DefaultValues()
            Dim key As KeyInfo = New Key With {
                    .Starts = New Date(2018, 11, 14),
                    .Expires = New Date(2018, 12, 21),
                    .KeyId = 22
                    }
            Assert.AreEqual(0, key.GracePeriodDays)
            Assert.AreEqual(False, key.RequiresActivation)
        End Sub

        ''' <summary>
        ''' Helper function to return a suitable grace period based on the license
        ''' start date and whether the license needs to be in grace for the test
        ''' </summary>
        Private Function GetGracePeriod(start As DateTime, isInGrace As Boolean) As Integer
            If isInGrace Then
                Return CType((DateDiff(DateInterval.Day, start, DateTime.UtcNow,) + 10), Integer)
            Else
                Return Math.Max(
                    0,
                    CType((DateDiff(DateInterval.Day, DateTime.UtcNow, start) - 10), Integer))
            End If
        End Function

        Private Function CreateServer(loggedIn As Boolean) As clsServer

            Dim mockServer = New Mock(Of clsServer)(MockBehavior.Strict)

            Dim server = mockServer.Object
            If loggedIn Then
                Dim userMock = New Mock(Of IUser)()
                ReflectionHelper.SetPrivateField("mLoggedInUser", server, userMock.Object)
            End If
            Return server

        End Function

        Private Function CreateDatabase(returnsDatabaseId As Boolean,
                                        returnsEnvironmentId As Boolean,
                                        databaseIdErrors As Boolean,
                                        returnsRequest As Boolean,
                                        readError As Boolean,
                                        returnsDataVersion As Boolean) As Mock(Of IDatabaseConnection)

            Dim database = New Mock(Of IDatabaseConnection)(MockBehavior.Strict)

            Dim databaseIdReader = New Mock(Of IDataReader)
            If Not returnsDatabaseId OrElse readError Then
                databaseIdReader.Setup(Function(r1) r1.Read()).Returns(False)
            Else
                If databaseIdErrors Then
                    databaseIdReader.Setup(Function(rl) rl.Read()).Throws(New BluePrismException())
                Else
                    databaseIdReader.Setup(Function(r1) r1.Read()).Returns(True)
                End If

                databaseIdReader.Setup(Function(rl) rl(0)).Returns(New Guid(EnvironmentId))
            End If

            Dim environmentIdReader = New Mock(Of IDataReader)
            If readError Then
                environmentIdReader.Setup(Function(rl) rl.Read()).Returns(False)
            Else
                environmentIdReader.Setup(Function(r1) r1.Read()).Returns(True)
            End If

            If returnsEnvironmentId Then
                environmentIdReader.Setup(Function(rl) rl(0)).Returns(New Guid(EnvironmentId))
            Else
                environmentIdReader.Setup(Function(rl) rl(0)).Returns(Guid.Empty)
            End If

            Dim serverNameReader = New Mock(Of IDataReader)
            If readError Then
                serverNameReader.Setup(Function(r1) r1.Read()).Returns(False)
            Else
                serverNameReader.Setup(Function(r1) r1.Read()).Returns(True)
                serverNameReader.Setup(Function(rl) rl(0)).Returns("Server")
            End If


            Dim requestReader = New Mock(Of IDataReader)
            If readError Then
                requestReader.Setup(Function(rl) rl.Read()).Returns(False)
            Else
                requestReader.Setup(Function(rl) rl.Read()).Returns(True)
                requestReader.Setup(Function(rl) rl(0)).Returns(1)
                requestReader.Setup(Function(rl) rl(1)).Returns(Guid.NewGuid())
            End If

            Dim responseReader = New Mock(Of IDataReader)

            If readError Then
                responseReader.Setup(Function(rl) rl.Read()).Returns(False)
            Else
                responseReader.SetupSequence(Function(rl) rl.Read()).Returns(True).Returns(False)
                responseReader.Setup(Function(r1) r1.FieldCount).Returns(2)
                responseReader.Setup(Function(rl) rl.GetName(0)).Returns("id")
                responseReader.Setup(Function(rl) rl.GetName(1)).Returns("licensekey")

                responseReader.Setup(Function(rl) rl(0)).Returns(2)
                responseReader.Setup(Function(rl) rl(1)).Returns(Licence2017)
            End If

            database.Setup(Function(rl) rl.GetDatabaseName()).Returns("Name")
            database.Setup(Sub(rl) rl.BeginTransaction())
            Dim sequence = database.SetupSequence(Function(rl) rl.ExecuteReturnDataReader(Moq.It.IsAny(Of SqlCommand))).
                    Returns(databaseIdReader.Object)

            If databaseIdErrors Then
                sequence = sequence.Returns(environmentIdReader.Object)
            End If

            If returnsRequest Then
                sequence = sequence.Returns(serverNameReader.Object).
                    Returns(requestReader.Object)
            Else
                sequence = sequence.Returns(responseReader.Object)
            End If

            If returnsDataVersion Then
                database.Setup(Function(r1) r1.ExecuteReturnScalar(Moq.It.IsAny(Of SqlCommand))).Returns(Function() 0)
            End If


            database.Setup(Sub(rl) rl.Execute(Moq.It.IsAny(Of SqlCommand)))
            database.Setup(Sub(rl) rl.CommitTransaction())
            database.Setup(Sub(rl) rl.RollbackTransaction())
            Return database
        End Function


        <Test>
        Public Sub ValidateLicenseActivationResponse_NullArgumentException()
            Dim server = CreateServer(True)
            Assert.Throws(Of ArgumentNullException)(Sub() server.ValidateLicenseActivationResponse(Nothing, Nothing, Nothing))
        End Sub

        <Test>
        Public Sub ValidateLicenseActivationResponse_NoDatabaseId()
            Dim server = CreateServer(True)
            Dim database = CreateDatabase(False, False, True, False, False, False)
            Assert.Throws(Of BluePrismException)(Sub() server.ValidateLicenseActivationResponse(database.Object, New KeyInfo(CType(Nothing, String)), GoodBase64Response))
        End Sub

        <Test>
        Public Sub ValidateLicenseActivationResponse_DatabaseIdErrorWithNoEnvironment()
            Dim server = CreateServer(True)
            Dim database = CreateDatabase(True, False, True, False, False, False)
            Assert.Throws(Of BluePrismException)(Sub() server.ValidateLicenseActivationResponse(database.Object, New KeyInfo(CType(Nothing, String)), GoodBase64Response))
        End Sub

        <Test>
        Public Sub ValidateLicenseActivationResponse_GetLicenseActivationJSONContent_NullArgumentException1()
            Assert.Throws(Of ArgumentNullException)(Sub() Licensing.GetLicenseActivationJSONContent(""))
        End Sub

        <Test>
        Public Sub ValidateLicenseActivationResponse_Good_GetLicenseActivationJSONContent()
            Dim ixisContent As LicenseActivationJSONContent = Licensing.GetLicenseActivationJSONContent(GoodBase64Response)
            Assert.True(ixisContent.User = RequestUser, "Invalid user")
        End Sub

        <Test>
        Public Sub ValidateLicenseActivationResponse_Bad_GetLicenseActivationJSONContent0()
            Dim ixisContent As LicenseActivationJSONContent = Licensing.GetLicenseActivationJSONContent(BadBase64Response)
            Assert.True(ixisContent Is Nothing, "Invalid reference")
        End Sub

        <Test>
        Public Sub ValidateLicenseActivationResponse_LicenseMismatch()
            Dim server = CreateServer(True)
            Dim database = Me.CreateDatabase(True, False, False, False, False, False)
            Dim key As KeyInfo = New Key With {
                    .Id = 1000
                    }
            Dim result = server.ValidateLicenseActivationResponse(database.Object, key, GoodBase64Response)
            Assert.IsFalse(result, "Validation succeeded")
            database.Verify(Sub(rl) rl.BeginTransaction(), Times.Once)
            database.Verify(Function(rl) rl.ExecuteReturnDataReader(Moq.It.IsAny(Of SqlCommand)), Times.Exactly(2))
            database.Verify(Sub(rl) rl.RollbackTransaction(), Times.Once)
        End Sub

        <Test>
        Public Sub ValidateLicenseActivationResponse_SUCCESS()
            Dim server = CreateServer(True)
            Dim database = CreateDatabase(True, False, False, False, False, True)
            Dim key As KeyInfo = New Key With {
                    .Id = 2
                    }
            Dim result = server.ValidateLicenseActivationResponse(database.Object, key, GoodBase64Response)
            Assert.IsTrue(result, "Validation failed")
            database.Verify(Sub(rl) rl.BeginTransaction(), Times.Once)
            database.Verify(Function(rl) rl.ExecuteReturnDataReader(Moq.It.IsAny(Of SqlCommand)), Times.Exactly(2))
            database.Verify(Sub(rl) rl.Execute(Moq.It.IsAny(Of SqlCommand)), Times.Once)
            database.Verify(Sub(rl) rl.CommitTransaction(), Times.Once)
        End Sub

        <Test>
        Public Sub ValidateLicenseActivationResponse_FAIL()
            Dim server = CreateServer(True)
            Dim database = CreateDatabase(True, False, False, False, False, False)
            Dim result = server.ValidateLicenseActivationResponse(database.Object, New KeyInfo(CType(Nothing, String)), GoodBase64Response)
            Assert.IsFalse(result, "Validation succeeded")
            database.Verify(Sub(rl) rl.BeginTransaction(), Times.Once)
            database.Verify(Function(rl) rl.ExecuteReturnDataReader(Moq.It.IsAny(Of SqlCommand)), Times.Exactly(2))
            database.Verify(Sub(rl) rl.RollbackTransaction(), Times.Once)
        End Sub


    End Class

End Namespace
#End If
