Imports System.Threading
Imports System.Threading.Tasks
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.BackgroundJobs
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models

Partial Public Class clsServer

    Private mAuthenticationServerHttpRequester As IAuthenticationServerHttpRequester

    Private ReadOnly mGetCredential As Func(Of Guid, clsCredential) =
        Function(id) GetCredential(id, True, True, False)

    Private ReadOnly mConvertToAuthenticationServerUser As Action(Of IDatabaseConnection, IUser, Guid, String) =
        Sub(con, user, authenticationServerUserId, userName) ConvertToAuthenticationServerUser(con, user, authenticationServerUserId, userName)

    Private ReadOnly mCreateNewUser As Action(Of IDatabaseConnection, User) =
        Sub(con, user) CreateNewUser(con, user, Nothing)

    Private ReadOnly mCreateNewAuthenticationServerUserWithUniqueName As Action(Of IDatabaseConnection, String, Guid) =
        Sub(con, name, id) CreateNewAuthenticationServerUserWithUniqueName(con, name, id)

    Private ReadOnly mGetUserByName As Func(Of IDatabaseConnection, String, User) =
        Function(con, username) GetUser(con, Nothing, username)


    Private ReadOnly Property AuthenticationServerHttpRequester As IAuthenticationServerHttpRequester
        Get
            If mAuthenticationServerHttpRequester Is Nothing Then
                mAuthenticationServerHttpRequester = mDependencyResolver.Resolve(Of IAuthenticationServerHttpRequester)
            End If

            Return mAuthenticationServerHttpRequester
        End Get
    End Property

    <SecuredMethod(Permission.SystemManager.AuthenticationServer.MapAuthenticationServerUsers)>
    Public Function MapAuthenticationServerUsers(usersToMap As List(Of UserMappingRecord), notifier As BackgroundJobNotifier) As BackgroundJob _
        Implements IServer.MapAuthenticationServerUsers
        CheckPermissions()
        CleanUpExpiredBackgroundJobs()
        Dim jobId = GetNewBackgroundJobId()
        Dim job = New BackgroundJob(jobId)
        UpdateBackgroundJob(jobId, notifier, 0, BackgroundJobStatus.Running)
        ThreadPool.QueueUserWorkItem(Sub() MapUsers(usersToMap, jobId, notifier))
        Return job
    End Function
    <SecuredMethod(Permission.SystemManager.AuthenticationServer.MapAuthenticationServerUsers)>
    Public Function GetAllNativeBluePrismUserNames() As List(Of String) _
        Implements IServer.GetAllNativeBluePrismUserNames
        CheckPermissions()
        
        Try
            Return GetAllUsers(False).Where(Function(x) x.AuthType = AuthMode.Native AndAlso _
                                                        Not String.IsNullOrWhiteSpace(x.Name) AndAlso _
                                                        Not x.AuthenticationServerUserId.HasValue _
                                           ).Select(Function(u) u.Name).ToList()
        Catch ex As Exception
            Log.Error(ex, "Error reading native users")
            Throw
        End Try
        Return Nothing
    End Function
    Private Async Sub MapUsers(usersToMap As List(Of UserMappingRecord), jobId As Guid, notifier As BackgroundJobNotifier)

        Dim logonOptions As LogonOptions
        Dim clientCredential As clsCredential
        Dim mappingResults As List(Of UserMappingResult) = Nothing

        Try
            Using con = GetConnection()
                AuditRecordAuthenticationServerUserMappingStartedEvent(con, usersToMap.Count)

                logonOptions = GetLogonOptions(con)

                If logonOptions.AuthenticationServerAuthenticationEnabled Then
                    Dim result = MapUsersResult.Failed(MapUsersErrorCode.MappingNotAvailableWhenAuthenticationServerEnabled)
                    AuditRecordAuthenticationServerUserMappingFinishedEvent(con, result)
                    UpdateBackgroundJob(jobId, notifier, 0, BackgroundJobStatus.Failure,
                                        resultData:=result)
                    Return
                End If

                If logonOptions.SingleSignon Then
                    Dim result = MapUsersResult.Failed(MapUsersErrorCode.InvalidActionInSsoEnvironment)
                    AuditRecordAuthenticationServerUserMappingFinishedEvent(con, result)
                    UpdateBackgroundJob(jobId, notifier, 0, BackgroundJobStatus.Failure,
                                        resultData:=result)
                    Return
                End If

                If String.IsNullOrWhiteSpace(logonOptions.AuthenticationServerUrl) Then
                    Dim result = MapUsersResult.Failed(MapUsersErrorCode.AuthenticationServerUrlNotSet)
                    AuditRecordAuthenticationServerUserMappingFinishedEvent(con, result)
                    UpdateBackgroundJob(jobId, notifier, 0, BackgroundJobStatus.Failure,
                                        resultData:=result)
                    Return
                End If

                Dim credentialId = logonOptions.AuthenticationServerApiCredentialId

                If Not credentialId.HasValue OrElse credentialId = Guid.Empty Then
                    Dim result = MapUsersResult.Failed(MapUsersErrorCode.AuthenticationServerCredentialIdNotSet)
                    AuditRecordAuthenticationServerUserMappingFinishedEvent(con, result)
                    UpdateBackgroundJob(jobId, notifier, 0, BackgroundJobStatus.Failure,
                                        resultData:=result)
                    Return
                End If

                Try
                    clientCredential = mGetCredential(credentialId.Value)
                Catch noSuchCredentialException As NoSuchCredentialException
                    Dim result = MapUsersResult.Failed(MapUsersErrorCode.AuthenticationServerCredentialIdNotSet)
                    AuditRecordAuthenticationServerUserMappingFinishedEvent(con, result)
                    UpdateBackgroundJob(jobId, notifier, 0, BackgroundJobStatus.Failure, resultData:=result)
                    Return
                End Try

                If clientCredential.Type Is Nothing OrElse clientCredential.Type.Name <> CredentialType.OAuth2ClientCredentials.Name Then
                    Dim result = MapUsersResult.Failed(MapUsersErrorCode.AuthenticationServerCredentialIdNotSet)
                    AuditRecordAuthenticationServerUserMappingFinishedEvent(con, result)
                    UpdateBackgroundJob(jobId, notifier, 0, BackgroundJobStatus.Failure, resultData:=result)
                    Return
                End If

                Dim usersMapped = 0

                Dim mappingTasks = usersToMap.Select(Async Function(record) As Task(Of UserMappingResult)
                                                         Try
                                                             Dim result = Await MapUser(record, logonOptions.AuthenticationServerUrl, clientCredential)
                                                             Dim percentProgress = CalculateJobPercentageProgress(usersToMap.Count, Interlocked.Increment(usersMapped))
                                                             UpdateBackgroundJob(jobId, notifier, percentProgress, BackgroundJobStatus.Running)
                                                             Return result
                                                         Catch ex As Exception
                                                             Return UserMappingResult.Failed(record, UserMappingResultCode.UnexpectedError)
                                                         End Try
                                                     End Function).ToList()

                Await Task.WhenAll(mappingTasks)

                mappingResults = mappingTasks.
                                    Where(Function(task) Not task.IsFaulted).
                                    Select(Function(task) task.Result).
                                    ToList()

                Dim results = MapUsersResult.Completed(mappingResults)
                AuditRecordAuthenticationServerUserMappingFinishedEvent(con, results)
                UpdateBackgroundJob(jobId, notifier, 100, BackgroundJobStatus.Success, resultData:=results)

            End Using
        Catch ex As Exception
            Log.Error(ex, "Error mapping Authentication Server users")
            If mappingResults IsNot Nothing Then
                UpdateBackgroundJob(jobId, notifier, 100, BackgroundJobStatus.Success, resultData:=MapUsersResult.Completed(mappingResults))
            Else
                UpdateBackgroundJob(jobId, notifier, 0, BackgroundJobStatus.Failure, resultData:=MapUsersResult.Failed(MapUsersErrorCode.UnexpectedError))
            End If
        End Try
    End Sub

    Private Shared Function CalculateJobPercentageProgress(total As Integer, progress As Integer) As Integer
        Return If(total <= 0, 0, CInt(progress / total * 100))
    End Function

    Private Async Function MapUser(record As UserMappingRecord, authenticationServerUrl As String, clientCredential As clsCredential) As Task(Of UserMappingResult)
        Try
            Dim mappingExistingBluePrismUser = Not String.IsNullOrWhiteSpace(record.BluePrismUsername)
            Dim mappingExistingAuthenticationServerUser = record.AuthenticationServerUserId.HasValue

            Using connection = GetConnection()

                If mappingExistingBluePrismUser AndAlso mappingExistingAuthenticationServerUser Then _
                    Return Await MapExistingUsers(connection, record, authenticationServerUrl, clientCredential)

                If mappingExistingBluePrismUser Then _
                    Return Await CreateAuthenticationServerUserAndMapToExistingBluePrismUser(connection, record, authenticationServerUrl, clientCredential)

                If mappingExistingAuthenticationServerUser Then _
                    Return Await CreateBluePrismUserAndMapToExistingAuthenticationServerUser(connection, record, authenticationServerUrl, clientCredential)

                Return UserMappingResult.Failed(record, UserMappingResultCode.MissingMappingRecordValues)
            End Using
        Catch ex As Exception
            Log.Error(ex)
            Return UserMappingResult.Failed(record, UserMappingResultCode.UnexpectedError)
        End Try
    End Function

    Private Async Function CreateAuthenticationServerUserAndMapToExistingBluePrismUser(connection As IDatabaseConnection, record As UserMappingRecord,
                                                                                       authenticationServerUrl As String, clientCredential As clsCredential) _
        As Task(Of UserMappingResult)

        Dim getUserResult = GetBluePrismUserForMapping(connection, record.BluePrismUsername)
        If getUserResult.User Is Nothing Then
            Return UserMappingResult.Failed(record, getUserResult.CannotMapUserReason)
        End If

        If String.IsNullOrEmpty(record.Email) OrElse String.IsNullOrEmpty(record.FirstName) OrElse String.IsNullOrEmpty(record.LastName) Then
            Return UserMappingResult.Failed(record, UserMappingResultCode.MissingMappingRecordValues)
        End If

        Dim authenticationServerUser = Await AuthenticationServerHttpRequester.PostUser(record, authenticationServerUrl, clientCredential)

        If authenticationServerUser Is Nothing Then
            Return UserMappingResult.Failed(record, UserMappingResultCode.ErrorCreatingAuthenticationServerUserRecord)
        End If

        Return UpdateBluePrismUser(connection, record, getUserResult.User, authenticationServerUser)
    End Function

    Private Async Function CreateBluePrismUserAndMapToExistingAuthenticationServerUser(connection As IDatabaseConnection, record As UserMappingRecord,
                                                                                        authenticationServerUrl As String, clientCredential As clsCredential) _
        As Task(Of UserMappingResult)

        Dim authenticationServerUser = Await AuthenticationServerHttpRequester.GetUser(record.AuthenticationServerUserId, authenticationServerUrl, clientCredential)
        If authenticationServerUser Is Nothing Then
            Return UserMappingResult.Failed(record, UserMappingResultCode.AuthenticationServerUserNotLoaded)
        End If

        Return CreateBluePrismUser(connection, record, authenticationServerUser)
    End Function

    Private Async Function MapExistingUsers(connection As IDatabaseConnection, record As UserMappingRecord, authenticationServerUrl As String, clientCredential As clsCredential) _
        As Task(Of UserMappingResult)

        Dim getUserResult = GetBluePrismUserForMapping(connection, record.BluePrismUsername)
        If getUserResult.User Is Nothing Then
            Return UserMappingResult.Failed(record, getUserResult.CannotMapUserReason)
        End If

        Dim authenticationServerUser = Await AuthenticationServerHttpRequester.GetUser(record.AuthenticationServerUserId, authenticationServerUrl, clientCredential)
        If authenticationServerUser Is Nothing Then
            Return UserMappingResult.Failed(record, UserMappingResultCode.AuthenticationServerUserNotLoaded)
        End If

        Return UpdateBluePrismUser(connection, record, getUserResult.User, authenticationServerUser)
    End Function

    Private Function GetBluePrismUserForMapping(connection As IDatabaseConnection, bluePrismUserName As String) As (User As User, CannotMapUserReason As UserMappingResultCode)
        Dim user As User = Nothing
        Try
            user = mGetUserByName(connection, bluePrismUserName)
        Catch noElementException As NoSuchElementException
            Return (Nothing, UserMappingResultCode.BluePrismUserNotFound)
        End Try

        If user.Deleted Then _
            Return (Nothing, UserMappingResultCode.BluePrismUserDeleted)

        If {AuthMode.System, AuthMode.Anonymous}.Contains(user.AuthType) Then _
            Return (Nothing, UserMappingResultCode.CannotMapSystemUser)

        If user.AuthType = AuthMode.AuthenticationServer Then _
            Return (Nothing, UserMappingResultCode.BluePrismUserHasAlreadyBeenMapped)

        Dim validAuthenticationTypes = {AuthMode.Native}

        If Not validAuthenticationTypes.Contains(user.AuthType) Then _
            Return (Nothing, UserMappingResultCode.BluePrismUsersAuthTypeDoesNotSupportMapping)

        Return (user, UserMappingResultCode.None)
    End Function

    Private Function CreateBluePrismUser(connection As IDatabaseConnection, record As UserMappingRecord, authenticationServerUser As AuthenticationServerUser) _
        As UserMappingResult

        Try
            mCreateNewAuthenticationServerUserWithUniqueName(connection, authenticationServerUser.Username, authenticationServerUser.Id.Value)
            Return UserMappingResult.Success(record)
        Catch ex As Exception
            Log.Error(ex)
            Return UserMappingResult.Failed(record, UserMappingResultCode.UnexpectedError)
        End Try
    End Function

    Private Function UpdateBluePrismUser(connection As IDatabaseConnection, record As UserMappingRecord, bluePrismUser As IUser, authenticationServerUser As AuthenticationServerUser) As UserMappingResult

        Try
            mConvertToAuthenticationServerUser(connection, bluePrismUser, authenticationServerUser.Id.Value, authenticationServerUser.Username)
        Catch alreadyMappedException As AuthenticationServerUserIdAlreadyInUseException
            Return UserMappingResult.Failed(record, UserMappingResultCode.AuthenticationServerUserAlreadyMappedToAnotherUser)
        Catch ex As Exception
            Log.Error(ex)
            Return UserMappingResult.Failed(record, UserMappingResultCode.UnexpectedError)
        End Try

        Return UserMappingResult.Success(record)
    End Function
End Class
