Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources.ResourceMachine
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Core.Resources
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models


Partial Public Class clsServer

    Private Const MaxLicenseActivationRequestServerName = 100
    Private Const MaxLicenseActivationRequestDatabaseName = 50

    ''' <summary>
    ''' Gets the license info from the database.
    ''' </summary>
    ''' <returns>The current keys.</returns>
    <SecuredMethod(True)>
    Public Function GetLicenseInfo() As List(Of KeyInfo) Implements IServer.GetLicenseInfo
        CheckPermissions()
        Using con = GetConnection()
            Return GetLicenseInfo(con)
        End Using
    End Function

    Private Function GetLicenseInfo(connection As IDatabaseConnection) As List(Of KeyInfo)
        Using command As New SqlCommand(
            " SELECT id, licensekey, installedon, installedby, licenseactivationresponse " &
            " FROM BPALicense ")
            Dim environmentId = GetLicenseActivationEnvironmentId(connection)
            Dim allRequests = GetActivationRequests(connection)
            Dim keys As New List(Of KeyInfo)
            Dim invalidKeys As New List(Of Integer)
            Using reader = connection.ExecuteReturnDataReader(command)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read
                    Dim id = prov.GetInt("id")
                    Dim activationRequest As ICollection(Of ActivationInfo) = Nothing
                    allRequests.TryGetValue(id, activationRequest)

                    Try
                        keys.Add(New KeyInfo(id,
                             prov.GetString("licensekey"),
                             prov.GetValue("installedon", Date.MinValue),
                             prov.GetGuid("installedby"),
                             prov.GetString("licenseactivationresponse"),
                             activationRequest,
                             environmentId))
                    Catch ex As InvalidFormatException
                        invalidKeys.Add(id)
                    End Try
                End While
            End Using

            'Cleanup any legacy license keys which will be invalid
            For Each iid In invalidKeys
                Using deleteCommand As New SqlCommand("DELETE FROM BPALicense WHERE id=@id")
                    deleteCommand.Parameters.AddWithValue("@id", iid)
                    connection.Execute(deleteCommand)
                End Using
                AuditRecordSysConfigEvent(connection, SysConfEventCode.ModifySystemLicenceKey,
                String.Format(My.Resources.clsServer_AutomaticLicenseProcessingRemovedInvalidLicenseKeyWithIdOf0, iid))
            Next

            Return keys
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function AddLicenseKey(key As KeyInfo, fileName As String) As List(Of KeyInfo) Implements IServer.AddLicenseKey
        CheckPermissions()

        Using connection = GetConnection()
            connection.BeginTransaction()

            Dim keys = GetLicenseInfo(connection)
            'Check license is valid for single sign on
            Dim auth = LicenseInfo.FromLicenseKeys(keys)
            If auth.IsLicensed AndAlso IsSingleSignOn(connection) AndAlso
             Not auth.CanUse(LicenseUse.ActiveDirectory) Then Throw New InvalidModeException(
                My.Resources.clsServer_AnNHSEditionLicenseKeyCannotBeUsedWithAnActiveDirectoryDatabase)

            'Check license can be added.
            If Not Licensing.CanAddLicense(key, keys) Then Return keys

            'Insert new license
            Using insertCommand As New SqlCommand(
                " INSERT INTO BPALicense (licensekey, installedon, installedby)" &
                " VALUES(@key, @at, @by)")

                With insertCommand.Parameters
                    .AddWithValue("@key", key.Key)
                    .AddWithValue("@at", IIf(key.SetAt = Date.MinValue, DBNull.Value, key.SetAt))
                    .AddWithValue("@by", IIf(key.SetBy = Guid.Empty, DBNull.Value, key.SetBy))
                End With
                connection.Execute(insertCommand)
            End Using

            AuditRecordSysConfigEvent(connection, SysConfEventCode.ModifySystemLicenceKey,
                    String.Format(My.Resources.clsServer_LicenseKeyAddedFrom01, fileName, key.ToString()))

            IncrementDataVersion(connection, DataNames.Licensing)

            connection.CommitTransaction()

            'Set the key as well - this will ensure it's updated on a BP Server instance.
            Dim licenseKeys = GetLicenseInfo(connection)
            Licensing.SetLicenseKeys(licenseKeys)

            'If not required for this licence, truncate the work queue logs
            If Not Licensing.License.TransactionModel Then
                WorkQueueLogTruncate(connection)
            End If

            Return licenseKeys
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function RemoveLicenseKey(key As KeyInfo) As List(Of KeyInfo) Implements IServer.RemoveLicenseKey
        CheckPermissions()

        Using connection = GetConnection()
            connection.BeginTransaction()

            Dim keys = GetLicenseInfo(connection)

            'Id might not be passed in. Therefore we can use the Key which is also unique
            If key.Id = 0 Then
                key = keys?.Where(Function(k) k.Key = key.Key)?.FirstOrDefault()
            End If

            If key Is Nothing Then
                Return keys
            End If

            'Check license is valid for single sign on
            Dim auth = LicenseInfo.FromLicenseKeys(keys)
            If auth.IsLicensed AndAlso IsSingleSignOn(connection) AndAlso
             Not auth.CanUse(LicenseUse.ActiveDirectory) Then Throw New InvalidModeException(
                My.Resources.clsServer_AnNHSEditionLicenseKeyCannotBeUsedWithAnActiveDirectoryDatabase)

            Dim deleteLicenseActivationrequestsCommand As New SqlCommand("DELETE FROM BPALicenseActivationRequest WHERE LicenseId=@id")
            With deleteLicenseActivationrequestsCommand.Parameters
                .AddWithValue("@id", key.Id)
            End With
            connection.Execute(deleteLicenseActivationrequestsCommand)

            Dim deleteLicenseCommand As New SqlCommand("DELETE FROM BPALicense WHERE Id=@id")
            With deleteLicenseCommand.Parameters
                .AddWithValue("@id", key.Id)
            End With
            connection.Execute(deleteLicenseCommand)

            AuditRecordSysConfigEvent(connection, SysConfEventCode.ModifySystemLicenceKey,
                String.Format(My.Resources.clsServer_LicenseKeyRemoved0, key.ToString()))

            IncrementDataVersion(connection, DataNames.Licensing)

            connection.CommitTransaction()

            'Set the keys as well - this will ensure it's updated on a BP Server instance.
            Dim licenseKeys = GetLicenseInfo(connection)
            Licensing.SetLicenseKeys(licenseKeys)

            'If not required for this licence, truncate the work queue logs
            If Not Licensing.License.TransactionModel Then
                WorkQueueLogTruncate(connection)
            End If

            Return licenseKeys
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetLicenseActivationRequest(keyInfo As KeyInfo) As String Implements IServer.GetLicenseActivationRequest
        CheckPermissions()
        Using con = GetConnection()
            Return GetLicenseActivationRequest(con, keyInfo)
        End Using
    End Function

    Friend Function GetLicenseActivationRequest(connection As IDatabaseConnection, keyInfo As KeyInfo) As String

        If keyInfo Is Nothing Then Throw New ArgumentNullException(NameOf(keyInfo))

        Dim environmentId As String = GetLicenseActivationEnvironmentId(connection)

        Dim serverName As String = Me.GetServerName(connection)
        Dim databaseName As String = connection.GetDatabaseName()
        Dim serverPath As String = GetPathForLicenseActivationRequest(serverName, databaseName)

        Dim request = New LicenseActivationRequest() With
            {
                .EnvironmentID = environmentId,
                .Information = New EnvironmentInformation() With
                {
                    .NumberOfAppServers = 0,
                    .AuthenticationMethod = 1,
                    .NumberOfResources = keyInfo.NumResourcePCs
                },
                .OrderID = keyInfo.SalesOrderId,
                .ServerPath = serverPath,
                .LicenseRequestID = keyInfo.LicenseRequestID,
                .RequestDateTime = DateTime.UtcNow,
                .User = mLoggedInUser.Name
            }

        connection.BeginTransaction()

        ' An empty BPALicenseActivationRequest is created initially so that it returns a new datbase id and reference
        Using cmd As New SqlCommand("INSERT INTO BPALicenseActivationRequest (LicenseId, Request, UserId) VALUES (@licenseId, @request, @userId);
                SELECT SCOPE_IDENTITY(), (SELECT [Reference] FROM BPALicenseActivationRequest WHERE [RequestId] = SCOPE_IDENTITY());")
            With cmd.Parameters
                .AddWithValue("@licenseId", keyInfo.Id)
                .AddWithValue("@request", String.Empty)
                .AddWithValue("@userId", mLoggedInUser.Id)
            End With
            Using dataReader = connection.ExecuteReturnDataReader(cmd)
                If dataReader.Read Then
                    request.ActivationRequestID = CType(dataReader(0), Integer)
                    request.ActivationReference = CType(dataReader(1), Guid).ToString()
                Else
                    Throw New BluePrismException(My.Resources.clsServer_DatabaseIdCouldNotBeRetrieved)
                End If
            End Using
        End Using

        Dim encryptedByteArray = Licensing.EncryptLicenseActivationRequest(request, My.Resources.clsServer_DataTooLongForLicenseActivationKeysize)
        Dim encryptedRequest = $"{Me.GetType().Assembly.GetName().Version.Major}:{Convert.ToBase64String(encryptedByteArray)}"

        Using cmd As New SqlCommand("UPDATE BPALicenseActivationRequest SET [Request] = @encryptedRequest WHERE RequestId = @requestId")
            With cmd.Parameters
                .AddWithValue("@encryptedRequest", encryptedRequest)
                .AddWithValue("@requestId", request.ActivationRequestID)
            End With

            connection.Execute(cmd)
        End Using

        AuditRecordSysConfigEvent(connection, SysConfEventCode.ModifySystemLicenceKey,
            String.Format(My.Resources.clsServer_LicenseActivationRequestedFor0, keyInfo.ToString))

        connection.CommitTransaction()
        Return encryptedRequest
    End Function

    Friend Function GetPathForLicenseActivationRequest(serverName As String, databaseName As String) As String

        If serverName IsNot Nothing AndAlso serverName.Length > MaxLicenseActivationRequestServerName Then
            serverName = serverName.Substring(0, MaxLicenseActivationRequestServerName)
        End If

        If databaseName IsNot Nothing AndAlso databaseName.Length > MaxLicenseActivationRequestDatabaseName Then
            databaseName = databaseName.Substring(0, MaxLicenseActivationRequestDatabaseName)
        End If

        Return $"{serverName}\{databaseName}"
    End Function

    Friend Function GetLicenseActivationEnvironmentId(connection As IDatabaseConnection) As String
        Dim environmentId As String = String.Empty
        Try
            environmentId = Me.GetDatabaseId(connection)
        Catch ex As BluePrismException
            environmentId = Me.GetEnvironmentId(connection)
        End Try

        If String.IsNullOrWhiteSpace(environmentId) Then
            Throw New BluePrismException(My.Resources.clsServer_EnvironmentIdCouldNotBeRetrieved)
        End If

        Return environmentId
    End Function



    <SecuredMethod(True)>
    Public Function ValidateLicenseActivationResponseForLicense(licenseToActivate As KeyInfo, ByVal base64String As String) As Boolean Implements IServer.ValidateLicenseActivationResponseForLicense
        CheckPermissions()
        Using con = GetConnection()
            Return ValidateLicenseActivationResponse(con, licenseToActivate, base64String)
        End Using
    End Function

    Friend Function ValidateLicenseActivationResponse(connection As IDatabaseConnection, licenseToActivate As KeyInfo, base64String As String) As Boolean

        Dim isValid As Boolean = False

        If String.IsNullOrWhiteSpace(base64String) Then
            Throw New ArgumentNullException("License Activation Response must be provided")
        End If

        If licenseToActivate Is Nothing Then
            Throw New ArgumentNullException("License to activate must be provided")
        End If

        Dim ixisContent = Licensing.GetLicenseActivationJSONContent(base64String)
        If ixisContent IsNot Nothing Then
            If ixisContent.EnvironmentID.Equals(GetLicenseActivationEnvironmentId(connection), StringComparison.CurrentCultureIgnoreCase) Then
                connection.BeginTransaction()
                Dim license = Me.GetLicenseByActivationRequest(connection, ixisContent.ActivationRequestID, ixisContent.ActivationReference)
                If license IsNot Nothing AndAlso licenseToActivate.Id = license.Id Then
                    Using cmd = New SqlCommand("UPDATE BPALicense SET [licenseactivationresponse] = @response, [activatedby] = @userId, activationdate =  GETUTCDATE()  WHERE id = @licenseid")
                        With cmd.Parameters
                            .AddWithValue("@response", base64String)
                            .AddWithValue("@licenseid", license.Id)
                            .AddWithValue("@userId", mLoggedInUser.Id)
                        End With
                        connection.Execute(cmd)
                    End Using

                    AuditRecordSysConfigEvent(connection,
                                              SysConfEventCode.ModifySystemLicenceKey,
                                              String.Format(My.Resources.clsServer_LicenseVerificationAcceptedFor0, license.ToString))

                    IncrementDataVersion(connection, DataNames.Licensing)

                    connection.CommitTransaction()
                    isValid = True
                Else
                    connection.RollbackTransaction()
                End If
            End If
        End If

        Return isValid
    End Function

    Private Function GetLicenseByActivationRequest(connection As IDatabaseConnection, requestId As Integer, reference As Guid) As KeyInfo
        Using command As New SqlCommand("SELECT lic.* FROM BPALicense lic 
INNER JOIN BPALicenseActivationRequest req ON lic.id = req.LicenseId
WHERE req.RequestId = @requestId AND req.Reference = @reference")
            With command.Parameters
                .AddWithValue("@requestId", requestId)
                .AddWithValue("@reference", reference)
            End With

            Using reader = connection.ExecuteReturnDataReader(command)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read
                    Return New KeyInfo(prov.GetInt("id"),
                             prov.GetString("licensekey"),
                             prov.GetValue("installedon", Date.MinValue),
                             prov.GetGuid("installedby"),
                             prov.GetString("licenseactivationresponse"),
                             Nothing,
                             Nothing)
                End While
            End Using
        End Using

        Return Nothing
    End Function

    Private Function GetActivationRequests(connection As IDatabaseConnection) As IDictionary(Of Integer, ICollection(Of ActivationInfo))
        Dim allRequests As New Dictionary(Of Integer, ICollection(Of ActivationInfo))
        Using command As New SqlCommand(
                " SELECT LicenseId, RequestId, Reference FROM BPALicenseActivationRequest ")

            Using reader = connection.ExecuteReturnDataReader(command)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read
                    Dim id = prov.GetInt("LicenseId")
                    Dim activationInfo = New ActivationInfo(prov.GetInt("RequestId"),
                                                            prov.GetGuid("Reference"))
                    Dim activationRequests As ICollection(Of ActivationInfo) = Nothing
                    If allRequests.TryGetValue(id, activationRequests) Then
                        activationRequests.Add(activationInfo)
                    Else
                        activationRequests = New List(Of ActivationInfo)
                        activationRequests.Add(activationInfo)
                        allRequests.Add(id, activationRequests)
                    End If
                End While
            End Using
        End Using
        Return allRequests
    End Function

    ''' <summary>
    ''' Determines if the current license permits the publishing
    ''' of another process (or the number of processes specified).
    ''' </summary>
    ''' <param name="con">The connection to use.</param>
    ''' <param name="num">The number of processes that it
    ''' is proposed to publish.</param>
    ''' <returns>Returns true if the specified number of processes
    ''' can be published, or false otherwise.</returns>
    Private Function CanPublishProcesses(
     con As IDatabaseConnection, ByVal num As Integer) As Boolean
        Dim lic = Licensing.License
        Return (lic.AllowsUnlimitedPublishedProcesses OrElse
            GetPublishedProcessCount(con) + num <= lic.NumPublishedProcesses)
    End Function


    ''' <summary>
    ''' Determines if the current license permits the creation of a session (taking
    ''' into account the number of existing pending/running sessions).
    ''' </summary>
    ''' <param name="sessionId">The ID of the session to be created, if known</param>
    ''' <returns>Returns true if the sessions can be created or false if the
    ''' specified sessions would exceed the concurrent session limit in the currently
    ''' installed licence.</returns>
    <SecuredMethod(True)>
    Public Function CanCreateSession(ByVal sessionId As Guid) As Boolean Implements IServer.CanCreateSession
        CheckPermissions()
        Return Licensing.License.AllowsUnlimitedSessions _
         OrElse CanCreateSessions(GetSingleton.ICollection(sessionId))
    End Function

    ''' <summary>
    ''' Determines if the current license permits the creation of a session (taking
    ''' into account the number of existing pending/running sessions).
    ''' </summary>
    ''' <param name="num">The number of sessions which are wanted to be created
    ''' </param>
    ''' <returns>Returns true if the sessions can be created or false if the
    ''' specified sessions would exceed the concurrent session limit in the currently
    ''' installed licence.</returns>
    <SecuredMethod(True)>
    Public Function CanCreateSessions(ByVal num As Integer) As Boolean Implements IServer.CanCreateSessions
        CheckPermissions()

        Dim license = Licensing.License

        If license.AllowsUnlimitedSessions Then Return True
        Return (CountConcurrentSessions() + num <=
         license.NumConcurrentSessions)
    End Function

    ''' <summary>
    ''' Determines if the current license permits the creation of the specified
    ''' sessions (taking into account the number of existing sessions).
    ''' </summary>
    ''' <param name="sessions">The IDs of the sessions which are to be created, if
    ''' known (eg. if recreating sessions found on the database with a known ID).
    ''' These can be <see cref="Guid.Empty"/> if the IDs are not known - the number
    ''' of session IDs determines how many sessions are expected to be required.
    ''' </param>
    ''' <returns>Returns true if the sessions can be created or false if the
    ''' specified sessions would exceed the concurrent session limit in the currently
    ''' installed licence.</returns>
    ''' <exception cref="Exception">If any errors occur while checking the current
    ''' sessions.</exception>
    Private Function CanCreateSessions(
     ByVal sessions As ICollection(Of Guid)) As Boolean

        Dim license = Licensing.License

        If license.AllowsUnlimitedSessions Then Return True
        Return (CountConcurrentSessions(sessions) + sessions.Count <=
         license.NumConcurrentSessions)
    End Function

    ''' <summary>
    ''' Checks if a resource with the given attributes can be created or if it
    ''' hits a product / licence limit.
    ''' </summary>
    ''' <param name="con">The connection over which to check the resource
    ''' availability</param>
    ''' <param name="resourceId">The ID of the resource to be activated</param>
    ''' <param name="attributes"></param>
    ''' <param name="denyMessage"></param>
    ''' <returns></returns>
    Private Function CanActivateResource(con As IDatabaseConnection, resourceId As Guid,
     attributes As ResourceAttribute, ByRef denyMessage As String) As Boolean

        'Zero corresponds to unlimited, but only for "proper" resource PCs
        'A zero for desktop means zero
        If (attributes And ResourceAttribute.Local) = 0 _
         AndAlso Licensing.License.NumResourcePCs = 0 Then Return True

        'No restrictions for debugging
        If (attributes And ResourceAttribute.Debug) <> 0 Then Return True

        Dim requiredAttributes As ResourceAttribute
        Dim deniedAttributes As ResourceAttribute = ResourceAttribute.Retired
        If (attributes And ResourceAttribute.Local) <> 0 Then
            requiredAttributes = ResourceAttribute.Local
        Else
            requiredAttributes = ResourceAttribute.None
            deniedAttributes = deniedAttributes Or ResourceAttribute.Local Or ResourceAttribute.Debug
        End If

        'Count all the rows, except if the resource name matches the one being
        'registered, since it may already be there but marked as offline, etc.
        Dim ids As New clsSet(Of Guid)
        For Each row As DataRow In GetResources(con, requiredAttributes, deniedAttributes).Rows
            ids.Add(New Guid(row("resourceid").ToString()))
        Next
        ids.Remove(resourceId)

        If (attributes And ResourceAttribute.Local) <> 0 Then
            'Desktop robots - no longer restricted
        Else
            If ids.Count >= Licensing.License.NumResourcePCs Then
                denyMessage = Licensing.MaxResourcesLimitReachedMessage
                Return False
            End If
        End If

        Return True

    End Function

    ''' <summary>
    ''' Determines if the resource can be retired.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="resourceId">The ID of the resource to be retired</param>
    ''' <returns>True if the specified resource, otherwise False</returns>
    Private Function CanRetireResource(con As IDatabaseConnection, resourceId As Guid) As Boolean
        Dim cmd As New SqlCommand("SELECT name,statusid,lastupdated FROM BPAResource WHERE resourceid=@resid")
        With cmd.Parameters
            .AddWithValue("@resid", resourceId)
        End With

        Using dataReader = con.ExecuteReturnDataReader(cmd)
            If dataReader.Read Then
                Dim status = CType(dataReader("statusid"), ResourceDBStatus)

                If status = ResourceDBStatus.Offline OrElse IsDBNull(dataReader("lastupdated")) Then
                    Return True
                Else
                    Dim lastupdated As Date = CType(dataReader("lastupdated"), Date).ToLocalTime()
                    If lastupdated < Now.AddMinutes(-5) Then
                        Return True
                    End If
                End If
            End If
        End Using

        Return False
    End Function

    <SecuredMethod(True)>
    Public Function GetAuditLogDataForLicense(licenseID As Integer) As IEnumerable(Of LicenseActivationEvent) Implements IServer.GetAuditLogDataForLicense
        CheckPermissions()

        Dim result = New List(Of LicenseActivationEvent)
        Dim lastLine As LicenseActivationEvent = Nothing
        Using con = GetConnection()
            Using cmd = New SqlCommand With {.CommandText = "SELECT [l].[licensekey], 
                    [l].[installedon], 
                    [IU].[username] AS InstalledBy, 
                    [la].[RequestDateTime] AS RequestedOn, 
                    COALESCE([ru].[username],'') AS RequestedBy, 
                    [l].[activationdate] AS ActivatedOn, 
                    COALESCE([au].[username],'') AS ActivatedBy
            FROM BPALicense AS l
                    LEFT JOIN [BPALicenseActivationRequest] AS la ON l.id = la.LicenseId
                    LEFT JOIN [BPAUser] AS iu ON [l].[installedby] = [iu].[userid]
                    LEFT JOIN [BPAUser] AS ru ON [la].[UserId] = [ru].[userid]
                    LEFT JOIN [BPAUser] AS au ON [l].[activatedby] = [au].[userid]
            WHERE l.id = @id;"}


                cmd.Parameters.AddWithValue("@id", licenseID)

                Using reader = con.ExecuteReturnDataReader(cmd)
                    Dim prov As New ReaderDataProvider(reader)
                    Dim pass = 1
                    While reader.Read
                        If pass = 1 Then
                            'Add the first row
                            result.Add(New LicenseActivationEvent(True, prov.GetValue("installedOn", DateTime.MinValue), LicenseEventTypes.LicenseImported, prov.GetString("installedBy")))
                            If prov.GetValue("activatedOn", DateTime.MinValue) <> DateTime.MinValue Then
                                lastLine = New LicenseActivationEvent(True, prov.GetValue("activatedOn", DateTime.MinValue), LicenseEventTypes.LicenseActivated, prov.GetString("activatedBy"))
                            End If
                        End If

                        If prov.GetValue("RequestedOn", DateTime.MinValue) <> DateTime.MinValue Then
                            result.Add(New LicenseActivationEvent(True, prov.GetValue("RequestedOn", DateTime.MinValue), LicenseEventTypes.LicenseActivationRequestGenerated, prov.GetString("requestedBy")))
                        End If
                        pass += 1
                    End While

                    If lastLine IsNot Nothing Then
                        result.Add(lastLine)
                    End If
                End Using
            End Using
        End Using
        Return result
    End Function

    <SecuredMethod(True)>
    Public Function GetActivationRequestsForLicense(licenseID As Integer) As IEnumerable(Of String) Implements IServer.GetActivationRequestsForLicense
        CheckPermissions()

        Dim result = New List(Of String)
        Dim lastLine As LicenseActivationEvent = Nothing
        Using con = GetConnection()
            Using cmd = New SqlCommand With {.CommandText = "select 
                    [la].[Request] AS Request 
                from BPALicense as l
                    left join [BPALicenseActivationRequest] AS la on l.id = la.LicenseId
                where l.id = @id;"}

                cmd.Parameters.AddWithValue("@id", licenseID)

                Using reader = con.ExecuteReturnDataReader(cmd)
                    While reader.Read
                        result.Add(reader.Item("Request").ToString())
                    End While
                End Using
            End Using
        End Using
        Return result
    End Function
End Class
