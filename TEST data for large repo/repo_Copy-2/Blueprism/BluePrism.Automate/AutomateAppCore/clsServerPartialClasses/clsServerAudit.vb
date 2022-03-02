Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib
Imports BluePrism.Data
Imports BluePrism.Scheduling.Calendar
Imports LocaleTools
Imports BluePrism.DataPipeline
Imports BluePrism.DataPipeline.DataPipelineOutput
Imports Newtonsoft.Json
Imports BluePrism.Data.DataModels.WorkQueueAnalysis
Imports System.Globalization
Imports BluePrism.Server.Domain.Models
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping

Partial Public Class clsServer

    Public Const SingleSignOnEventCode = "S004"

#Region " EventCodeAttribute and supporting methods "


    ''' <summary>
    ''' Gets the non-null event code attribute associated with the given event code
    ''' enumeration value.
    ''' </summary>
    ''' <param name="eventCode">The event code instance to retrieve the event code
    ''' attribute for.</param>
    ''' <returns>The event code attribute instance corresponding to the given enum
    ''' value or an empty attribute instance if one is not associated with it.
    ''' </returns>
    Private Shared Function GetEventCodeAttribute(ByVal eventCode As [Enum]) As EventCodeAttribute
        Dim type As System.Type = eventCode.GetType
        Dim info As Reflection.FieldInfo = type.GetField(eventCode.ToString)
        Dim attribs() As EventCodeAttribute = TryCast(
         info.GetCustomAttributes(GetType(EventCodeAttribute), False), EventCodeAttribute())
        If attribs.Length > 0 Then Return attribs(0) Else Return EventCodeAttribute.Empty
    End Function

    ''' <summary>
    ''' Function to get an eventcode string from an eventcode enum
    ''' </summary>
    ''' <param name="eventCode">The enum to get the event code string for</param>
    ''' <returns>The event code string</returns>
    Private Shared Function GetEventCode(ByVal eventCode As [Enum]) As String
        Return GetEventCodeAttribute(eventCode).Code
    End Function

    ''' <summary>
    ''' Formats the narrative from the given event code using the specified
    ''' arguments. 
    ''' </summary>
    ''' <param name="eventCode">The event code from which the narrative can be drawn
    ''' </param>
    ''' <param name="args">The arguments for the narrative - the requirements for
    ''' these will be dictated by the event code and its associated narrative.
    ''' </param>
    ''' <returns>The narrative string for the given audit event populated with the
    ''' given arguments.</returns>
    Private Function GetNarrative(
     eventCode As [Enum], ParamArray args() As Object) As String
        Return GetEventCodeAttribute(eventCode).FormatNarrative(AuditLocale, args)
    End Function

    Public ReadOnly Property AuditLocale As CultureInfo
        Get
            Return If(Not String.IsNullOrEmpty(mLoggedInUserLocale), CultureInfo.CreateSpecificCulture(mLoggedInUserLocale), Nothing)
            'If mAuditLocale Is Nothing Then
            '    Using con = GetConnection()
            '        Dim locale As String = Nothing
            '        If TryGetPref(con, "audit.locale", Nothing, locale) Then
            '            mAuditLocale = CultureInfo.CreateSpecificCulture(locale)
            '        End If
            '    End Using
            'End If
            'Return mAuditLocale
        End Get
    End Property
    Private mAuditLocale As CultureInfo

    <SecuredMethod(Permission.SystemManager.Audit.AuditLogs)>
    Public Sub SetAuditLocale(locale As String) Implements IServer.SetAuditLocale
        CheckPermissions()

        Using connection = GetConnection()
            SetSystemPref(connection, "audit.locale", locale)
            mAuditLocale = Nothing
        End Using
    End Sub

#End Region

    ' Used event letters :
    ' L : Login
    ' U : User
    ' P : Process
    ' B : Business Object
    ' S : SysConf
    ' R : Relman
    ' O : "Object" ?
    ' R : Resource
    ' C : Calendar
    ' T : Schedule (I think 'T' for task, since 'S' was taken)
    ' Q : Queue
    ' F : Fonts
    ' K : Key Store (encryption)
    ' A : Credentials (Account details)
    ' D : Dashboards and tiles
    ' G : Groups
    ' H : Environment Lock
    ' EV: Environment Variables
    ' WS: Web Connection Settings
    ' SK: Skills
    ' AR: Archive/Restore
    ' DP: Data Pipelines
    ' WQ: Work Queue Analysis

#Region " Login / Logout"

    Private Sub RecordLoginAuditEvent(con As IDatabaseConnection, userName As String, machine As String, authenticationMode As AuthMode)

        If Not gAuditingEnabled Then Return

        Dim narrative As String = String.Format(
                My.Resources.clsServer_User0LoggedIntoResource1Using2,
                userName,
                machine,
                GetLocalizedAuthModeDescription(authenticationMode))

        WriteLoginAuditEvent(con, narrative, String.Empty)

    End Sub

    Private Sub RecordLoginWithReloginTokenAuditEvent(con As IDatabaseConnection, userName As String, machine As String, authenticationMode As AuthMode)

        If Not gAuditingEnabled Then Return

        Dim narrative As String = String.Format(
                My.Resources.clsServer_User0LoggedIntoResource1Using2,
                userName,
                machine,
                GetLocalizedAuthModeDescription(authenticationMode))

        Dim comments = My.Resources.ReloginTokenUsedToLogIn

        WriteLoginAuditEvent(con, narrative, comments)
    End Sub

    Private Sub RecordLoginViaAuthenticationServerAuditEvent(con As IDatabaseConnection, userName As String, machine As String, issuer As String, authenticationTime As DateTimeOffset?)

        If Not gAuditingEnabled Then Return

        Dim comments = String.Format(
            My.Resources.clsServer_TheAccessTokenIssuerIs0TheAccessTokenAuthenticationTimeIs1,
            issuer,
            If(authenticationTime.HasValue, authenticationTime.Value.ToUniversalTime().ToString(), String.Empty))

        Dim narrative As String = String.Format(
                My.Resources.clsServer_User0LoggedIntoResource1Using2,
                userName,
                machine,
                GetLocalizedAuthModeDescription(AuthMode.AuthenticationServer))

        WriteLoginAuditEvent(con, narrative, comments)

    End Sub

    Private Sub WriteLoginAuditEvent(con As IDatabaseConnection, narrative As String, comments As String)
        Try
            Dim cmd As New SqlCommand("INSERT INTO BPAAuditEvents (Eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) SELECT GETUTCDATE(), @EventCode, @Narrative, @SrcUserID, ResourceID, @Comments FROM BPAResource WHERE [Name] = @MachineName")
            With cmd.Parameters
                .AddWithValue("@EventCode", GetEventCode(LoginEventCode.Login))
                .AddWithValue("@Narrative", narrative)
                .AddWithValue("@SrcUserID", GetLoggedInUserId)
                .AddWithValue("@MachineName", mLoggedInMachine)
                .AddWithValue("@Comments", If(Not String.IsNullOrEmpty(comments), CObj(comments), DBNull.Value))
            End With
            con.ExecuteReturnRecordsAffected(cmd)
        Catch
        End Try
    End Sub

    Private Function GetLocalizedAuthModeDescription(authenticationMode As AuthMode) As String
        Select Case authenticationMode
            Case AuthMode.Native
                Return My.Resources.clsServer_NativeAuthenticationLower
            Case AuthMode.External
                Return My.Resources.clsServer_AnExternalProviderLower
            Case AuthMode.MappedActiveDirectory, AuthMode.ActiveDirectory
                Return My.Resources.clsServer_ActiveDirectory
            Case AuthMode.AuthenticationServer
                Return My.Resources.clsServer_TheAuthenticationServerLower
            Case Else
                Throw New NotImplementedException($"No localized description defined for {authenticationMode} AuthMode")
        End Select
    End Function


    Private Sub RecordLogoutAuditEvent(con As IDatabaseConnection, userName As String, machine As String)

        If Not gAuditingEnabled Then Return

        Dim narrative = String.Format(My.Resources.clsServer_User0LoggedOutOfResource1, userName, machine)

        Try
            Dim cmd As New SqlCommand("INSERT INTO BPAAuditEvents (Eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID) SELECT GETUTCDATE(), @EventCode, @Narrative, @SrcUserID, ResourceID FROM BPAResource WHERE [Name] = @MachineName")
            With cmd.Parameters
                .AddWithValue("@EventCode", GetEventCode(LoginEventCode.Logout))
                .AddWithValue("@Narrative", narrative)
                .AddWithValue("@SrcUserID", GetLoggedInUserId)
                .AddWithValue("@MachineName", mLoggedInMachine)
            End With
            con.ExecuteReturnRecordsAffected(cmd)
        Catch
        End Try

    End Sub

#End Region

#Region " Release Manager "

    ''' <summary>
    ''' Records a release manager audit event for the given event code and
    ''' package.
    ''' </summary>
    ''' <param name="con">The connection over which the event should recorded.
    ''' </param>
    ''' <param name="evtCode">The event code indicating which audit event is
    ''' being recorded.</param>
    ''' <param name="pkg">The package referred to by this audit</param>
    ''' <exception cref="ArgumentNullException">If the given package was null.
    ''' </exception>
    Private Sub RecordReleaseManagerAuditEvent(ByVal con As IDatabaseConnection,
     ByVal evtCode As RelmanEventCode, ByVal pkg As clsPackage)
        RecordReleaseManagerAuditEvent(con, evtCode, pkg, Nothing, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Records a release manager audit event for the given event code and
    ''' package.
    ''' </summary>
    ''' <param name="con">The connection over which the event should recorded.
    ''' </param>
    ''' <param name="evtCode">The event code indicating which audit event is
    ''' being recorded.</param>
    ''' <param name="rel">The release referred to by this audit</param>
    ''' <exception cref="ArgumentNullException">If the given release was null, or if
    ''' its package was null.</exception>
    Private Sub RecordReleaseManagerAuditEvent(ByVal con As IDatabaseConnection,
     ByVal evtCode As RelmanEventCode, ByVal rel As clsRelease)
        If rel Is Nothing Then Throw New ArgumentNullException(NameOf(rel))
        RecordReleaseManagerAuditEvent(con, evtCode, rel.Package, rel, Nothing, Nothing)
    End Sub


    ''' <summary>
    ''' Records a release manager audit event for the given event code and
    ''' package.
    ''' </summary>
    ''' <param name="con">The connection over which the event should recorded.
    ''' </param>
    ''' <param name="evtCode">The event code indicating which audit event is
    ''' being recorded.</param>
    ''' <param name="pkg">The package that this event refers to.</param>
    ''' <param name="rel">The release that this event refers to, if it relates to a
    ''' release. Null otherwise.</param>
    ''' <param name="comp">The package component that this event relates to, or null
    ''' if it does not reference a package component.</param>
    ''' <param name="comments">The comments to record - treated as a format string if
    ''' any <paramref name="args"/> are given, otherwise it is treated as a straight
    ''' comment string. Null indicates no comment.</param>
    ''' <param name="args">The arguments for the comments format string - if empty,
    ''' the <paramref name="comments"/> parameter is treated as a simple string, ie.
    ''' not a format string.</param>
    Private Sub RecordReleaseManagerAuditEvent(ByVal con As IDatabaseConnection,
     ByVal evtCode As RelmanEventCode, ByVal pkg As clsPackage, ByVal rel As clsRelease,
     ByVal comp As PackageComponent, ByVal comments As String, ByVal ParamArray args() As Object)

        If Not gAuditingEnabled Then Return

        ' Set up the narrative args
        ' ... for the package
        Dim pkgName As String = "<No Package>"
        If pkg IsNot Nothing Then pkgName = pkg.Name

        ' ... for the release
        Dim relName As String = Nothing
        Dim relFile As String = Nothing
        If rel IsNot Nothing Then
            relName = rel.Name
            relFile = rel.FileName
        End If

        ' ... for the component
        Dim compType As String = Nothing
        Dim compName As String = Nothing
        If comp IsNot Nothing Then
            compType = PackageComponentType.GetLocalizedFriendlyName(comp.Type)
            compName = comp.Name
        End If

        ' Get the event code attribute for this event code (provides code and narrative)
        Dim attr As EventCodeAttribute = GetEventCodeAttribute(evtCode)

        Using cmd As New SqlCommand(
         " insert into BPAAuditEvents" &
         " (eventdatetime, scode, snarrative, gsrcuserid, comments)" &
         " values (getutcdate(), @code, @narr, @userid, @comment)")

            With cmd.Parameters
                .AddWithValue("@code", attr.Code)
                .AddWithValue("@narr", attr.FormatNarrative(AuditLocale,
                 GetLoggedInUserName(), pkgName, relName, relFile, compType, compName))
                .AddWithValue("@userid", GetLoggedInUserId())
                If comments Is Nothing Then
                    .AddWithValue("@comment", DBNull.Value)

                ElseIf args Is Nothing OrElse args.Length = 0 Then
                    .AddWithValue("@comment", comments)

                Else
                    .AddWithValue("@comment", String.Format(comments, args))

                End If
            End With
            con.Execute(cmd)

        End Using

    End Sub

#End Region

#Region " User "

    ''' <summary>
    ''' Record a user-related audit event.
    ''' </summary>
    ''' <param name="eventCode">The event code</param>
    ''' <param name="userId">The target user id</param>
    ''' <param name="comments">The event comments</param>
    ''' <param name="oldUserName">The old username, if relevant</param>
    <SecuredMethod(True)>
    Public Sub AuditRecordUserEvent(
     ByVal eventCode As UserEventCode,
     ByVal userId As Guid,
     ByVal comments As String,
     Optional ByVal oldUserName As String = "",
     Optional ByVal externalIdMapping As ExternalIdentityMapping = Nothing,
     Optional ByVal oldExternalIdMapping As ExternalIdentityMapping = Nothing
    ) Implements IServer.AuditRecordUserEvent
        CheckPermissions()
        Using con = GetConnection()
            AuditRecordUserEvent(con, eventCode, userId, comments, oldUserName,
                                  externalIdMapping, oldExternalIdMapping)
        End Using
    End Sub

    ''' <summary>
    ''' Record a user-related audit event.
    ''' </summary>
    ''' <param name="con">The connection to use to write the audit event</param>
    ''' <param name="eventCode">The event code</param>
    ''' <param name="userId">The target user id</param>
    ''' <param name="comments">The event comments</param>
    ''' <param name="oldUserName">The old username</param>
    ''' <exception cref="Exception">If any errors occur while attempting to write
    ''' the user auditable event</exception>
    Private Sub AuditRecordUserEvent(
     ByVal con As IDatabaseConnection,
     ByVal eventCode As UserEventCode,
     ByVal userId As Guid,
     ByVal comments As String,
     ByVal oldUserName As String,
     ByVal externalIdMapping As ExternalIdentityMapping,
     ByVal oldExternalIdMapping As ExternalIdentityMapping,
     Optional newAuthType As AuthMode = Nothing)

        ' Not really a success - but it's not really an error either
        If Not gAuditingEnabled Then Return

        ' Get the target user name - somewhat more readable than the ID
        Dim userName As String = GetUserName(con, userId)

        Dim cmd As New SqlCommand(
         " insert into BPAAuditEvents (" &
         "   eventdatetime, sCode, sNarrative, gSrcUserID, gTgtUserID, comments) " &
         "   values (getutcdate(), @code, @narr, @srcUser, @targUser, @comments)")

        With cmd.Parameters
            .AddWithValue("@code", GetEventCode(eventCode))
            .AddWithValue("@narr",
             GetNarrative(eventCode,
                          GetLoggedInUserNameForAuthenticationServerAuditEvents(con),
                          userName, oldUserName,
                          externalIdMapping?.ExternalId, oldExternalIdMapping?.ExternalId,
                          externalIdMapping?.IdentityProviderName, oldExternalIdMapping?.IdentityProviderName,
                          If(newAuthType <> AuthMode.Unspecified, newAuthType.ToLocalizedDisplayName(), "")))
            .AddWithValue("@srcUser", GetLoggedInUserId())
            .AddWithValue("@targUser", userId)
            .AddWithValue("@comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
        End With

        con.ExecuteReturnRecordsAffected(cmd)

    End Sub

    Private Sub AuditRecordUserHasScopeFlagChangedEvent(
     con As IDatabaseConnection,
     userId As Guid,
     newValue As Boolean, oldValue As Boolean)

        Dim eventCode = UserEventCode.UserHasBluePrismApiScopeChanged

        ' Not really a success - but it's not really an error either
        If Not gAuditingEnabled Then Return

        ' Get the target user name - somewhat more readable than the ID
        Dim userName As String = GetUserName(con, userId)

        Dim cmd As New SqlCommand(
         " insert into BPAAuditEvents (" &
         "   eventdatetime, sCode, sNarrative, gSrcUserID, gTgtUserID) " &
         "   values (getutcdate(), @code, @narr, @srcUser, @targUser)")

        With cmd.Parameters
            .AddWithValue("@code", GetEventCode(eventCode))
            .AddWithValue("@narr",
             GetNarrative(eventCode,
                          GetLoggedInUserNameForAuthenticationServerAuditEvents(con),
                          userName, newValue, oldValue))
            .AddWithValue("@srcUser", GetLoggedInUserId())
            .AddWithValue("@targUser", userId)
        End With

        con.ExecuteReturnRecordsAffected(cmd)

    End Sub

    Private Sub AuditRecordActiveDirectoryUserEvent(eventCode As UserEventCode, con As IDatabaseConnection, userId As Guid, sid As String, userName As String, preSaveSid As String)

        If Not gAuditingEnabled Then Return

        Dim cmd As New SqlCommand(
            " insert into BPAAuditEvents (" &
            "   eventdatetime, sCode, sNarrative, gSrcUserID, gTgtUserID, comments) " &
            "   values (getutcdate(), @code, @narr, @srcUser, @targUser, null)")

        Dim arguments As Object()
        Select Case eventCode
            Case UserEventCode.UserMappedToActiveDirectoryUser, UserEventCode.DeleteMappedActiveDirectoryUser,
                UserEventCode.UserConvertedToDeletedMultiAuthAd, UserEventCode.UserConvertedToMultiAuthAd
                arguments = {GetLoggedInUserNameForAuthenticationServerAuditEvents(con), userName, sid}
            Case UserEventCode.ChangeActiveDirectoryIdentity
                arguments = {GetLoggedInUserNameForAuthenticationServerAuditEvents(con), userName, Nothing, sid, preSaveSid}
            Case Else
                Throw New NotImplementedException($"Method not implemented for event code {eventCode}")
        End Select

        With cmd.Parameters
            .AddWithValue("@code", GetEventCode(eventCode))
            .AddWithValue("@narr", GetNarrative(eventCode, arguments))
            .AddWithValue("@srcUser", GetLoggedInUserId())
            .AddWithValue("@targUser", userId)
        End With
        con.ExecuteReturnRecordsAffected(cmd)
    End Sub

    Private Sub AuditRecordAuthenticationServerUserMappingStartedEvent(con As IDatabaseConnection, numberOfMappingRecords As Integer)

        ' Not really a success - but it's not really an error either
        If Not gAuditingEnabled Then Return


        Dim cmd As New SqlCommand(
         " insert into BPAAuditEvents (" &
         "   eventdatetime, sCode, sNarrative, gSrcUserID) " &
         "   values (getutcdate(), @code, @narr, @srcUser)")

        With cmd.Parameters
            .AddWithValue("@code", GetEventCode(UserEventCode.AuthenticationServerUserMappingStarted))
            .AddWithValue("@narr", GetNarrative(UserEventCode.AuthenticationServerUserMappingStarted, GetLoggedInUserNameForAuthenticationServerAuditEvents(con), numberOfMappingRecords))
            .AddWithValue("@srcUser", GetLoggedInUserId())
        End With

        con.ExecuteReturnRecordsAffected(cmd)

    End Sub

    Private Sub AuditRecordAuthenticationServerUserMappingFinishedEvent(con As IDatabaseConnection, result As MapUsersResult)

        ' Not really a success - but it's not really an error either
        If Not gAuditingEnabled Then Return

        Dim cmd As New SqlCommand(
         " insert into BPAAuditEvents (" &
         "   eventdatetime, sCode, sNarrative, gSrcUserID, comments) " &
         "   values (getutcdate(), @code, @narr, @srcUser, @comments)")

        Dim comments = String.Empty
        Dim status = String.Empty
        Select Case result.Status
            Case MapUsersStatus.Completed
                status = My.Resources.MapUsersStatus_Completed
                comments = GetCompletedMappingAuditComment(status, result)
            Case MapUsersStatus.CompletedWithErrors
                status = My.Resources.MapUsersStatus_CompletedWithErrors
                comments = GetCompletedMappingAuditComment(status, result)
            Case MapUsersStatus.Failed
                status = My.Resources.MapUsersStatus_Failed
                Select Case result.ErrorCode
                    Case MapUsersErrorCode.InvalidActionInSsoEnvironment
                        comments = $"{status}: { My.Resources.MapAuthenticationServerUsers_CannotPerformMappingInSingleSignOnEnvironment }"
                    Case MapUsersErrorCode.AuthenticationServerCredentialIdNotSet
                        comments = $"{status}: { My.Resources.MapAuthenticationServerUsers_AuthenticationServerAPICredentialNotConfigured }"
                    Case MapUsersErrorCode.AuthenticationServerUrlNotSet
                        comments = $"{status}: { My.Resources.MapAuthenticationServerUsers_AuthenticationServerUrlNotConfigured }"
                    Case MapUsersErrorCode.MappingNotAvailableWhenAuthenticationServerEnabled
                        comments = $"{status}: { My.Resources.MapAuthenticationServerUsers_UserMappingIsNotAvailableWhenAuthenticationServerIsEnabled }"
                    Case MapUsersErrorCode.UnexpectedError
                        comments = $"{status}: { My.Resources.MapAuthenticationServerUsers_AnUnexpectedErrorOccurredWhenMappingAuthenticationServerUsers }"
                End Select
        End Select

        With cmd.Parameters
            .AddWithValue("@code", GetEventCode(UserEventCode.AuthenticationServerUserMappingFinished))
            .AddWithValue("@narr", GetNarrative(UserEventCode.AuthenticationServerUserMappingFinished, GetLoggedInUserNameForAuthenticationServerAuditEvents(con)))
            .AddWithValue("@srcUser", GetLoggedInUserId())
            .AddWithValue("@comments", comments)
        End With

        con.ExecuteReturnRecordsAffected(cmd)

    End Sub

    Private Function GetCompletedMappingAuditComment(status As String, result As MapUsersResult) As String
        Return String.Format(My.Resources.AuthenticationServerUserMappingFinishedEvent_Comments, status, result.SuccessfullyMappedRecordsCount, result.RecordsThatFailedToMap.Count)
    End Function

    Private Sub AuditRecordUserMappedToAuthenticationServerUserEvent(con As IDatabaseConnection, userId As Guid, authenticationServerUserId As Guid)

        If Not gAuditingEnabled Then Return

        Dim userName As String = GetUserName(con, userId)

        Dim cmd As New SqlCommand(
         " insert into BPAAuditEvents (" &
         "   eventdatetime, sCode, sNarrative, gSrcUserID, gTgtUserID, comments) " &
         "   values (getutcdate(), @code, @narr, @srcUser, @targUser, @comments)")
        With cmd.Parameters
            .AddWithValue("@code", GetEventCode(UserEventCode.UserMappedToAuthenticationServerUserId))
            .AddWithValue("@narr", GetNarrative(UserEventCode.UserMappedToAuthenticationServerUserId, GetLoggedInUserNameForAuthenticationServerAuditEvents(con), userName, authenticationServerUserId))
            .AddWithValue("@srcUser", GetLoggedInUserId())
            .AddWithValue("@targUser", userId)
            .AddWithValue("@comments", DBNull.Value)
        End With

        con.ExecuteReturnRecordsAffected(cmd)

    End Sub

    Private Sub AuditRecordUserMappedToAuthenticationServerServiceAccountEvent(con As IDatabaseConnection, userId As Guid, authenticationServerClientId As String)

        If Not gAuditingEnabled Then Return

        Dim userName As String = GetUserName(con, userId)

        Dim cmd As New SqlCommand(
         " insert into BPAAuditEvents (" &
         "   eventdatetime, sCode, sNarrative, gSrcUserID, gTgtUserID, comments) " &
         "   values (getutcdate(), @code, @narr, @srcUser, @targUser, @comments)")
        With cmd.Parameters
            .AddWithValue("@code", GetEventCode(UserEventCode.UserMappedToAuthenticationServerServiceAccount))
            .AddWithValue("@narr", GetNarrative(UserEventCode.UserMappedToAuthenticationServerServiceAccount, GetLoggedInUserNameForAuthenticationServerAuditEvents(con), userName, authenticationServerClientId))
            .AddWithValue("@srcUser", GetLoggedInUserId())
            .AddWithValue("@targUser", userId)
            .AddWithValue("@comments", DBNull.Value)
        End With

        con.ExecuteReturnRecordsAffected(cmd)

    End Sub

    Private Function GetLoggedInUserNameForAuthenticationServerAuditEvents(connection As IDatabaseConnection) As String

        Dim loggedInUserName = GetLoggedInUserName()
        If String.IsNullOrEmpty(loggedInUserName) Then

            Const applicationServerUserName = "Application Server"
            Const schedulerUserName = "Scheduler"

            If GetSystemUserId(connection, schedulerUserName) = GetLoggedInUserId() Then
                loggedInUserName = applicationServerUserName
            End If
        End If
        Return loggedInUserName
    End Function
#End Region

#Region " Process / Business Object "

    ''' <summary>
    ''' Record a process-related audit event.
    ''' </summary>
    ''' <param name="eventCode">A string classifying the type of event.</param>
    ''' <param name="targetProcessID">The guid of the process on which the event acted.
    ''' </param>
    ''' <param name="comments">Comments to supplement the sNarrative parameter
    ''' </param>
    ''' <param name="newXML">The latest version of the process xml.</param>
    ''' <param name="summary">A comment summarising the changes made to the process
    ''' </param>
    <SecuredMethod(True)>
    Public Sub AuditRecordProcessEvent(eventCode As ProcessEventCode,
                                       targetProcessID As Guid,
                                       comments As String,
                                       newXML As String,
                                       summary As String) Implements IServer.AuditRecordProcessEvent
        CheckPermissions()
        AuditRecordProcessOrVboEvent(DirectCast(eventCode, ProcessOrVboEventCode), False, targetProcessID, comments, newXML, summary)
    End Sub

    ''' <summary>
    ''' Record a business object-related event.
    ''' </summary>
    ''' <param name="eventCode">A string classifying the type of event.</param>
    ''' <param name="targetProcessID">The guid of the process on which the event
    ''' acted.</param>
    ''' <param name="comments">Comments to supplement the summary</param>
    ''' <param name="newXML">The latest version of the xml.</param>
    ''' <param name="summary">A comment summarising the changes made to the bo.
    ''' </param>
    <SecuredMethod(True)>
    Public Sub AuditRecordBusinessObjectEvent(eventCode As BusinessObjectEventCode,
                                              targetProcessID As Guid,
                                              comments As String,
                                              newXML As String,
                                              summary As String) Implements IServer.AuditRecordBusinessObjectEvent
        CheckPermissions()
        AuditRecordProcessOrVboEvent(DirectCast(eventCode, ProcessOrVboEventCode), True, targetProcessID, comments, newXML, summary)
    End Sub

    ''' <summary>
    ''' Records an audit event for a process or VBO, defined by the given properties.
    ''' This will ensure that the appropriate audit event code from
    ''' <see cref="ProcessEventCode"/> or <see cref="BusinessObjectEventCode"/> is
    ''' used in the audit record, along with the appropriate narrative describing
    ''' the audited event.
    ''' </summary>
    ''' <param name="eventCode">The event code detailing the operation which has
    ''' occurred on the process / vbo</param>
    ''' <param name="isVBO">True to indicate that a VBO is being audited here;
    ''' False to indicate a process.</param>
    ''' <param name="processId">The ID of the process / VBO.</param>
    ''' <param name="comments">Comments regarding the auditable event.</param>
    ''' <param name="newXML">The new XML from the process / VBO</param>
    ''' <param name="summary">A summary of the auditable event.</param>
    <SecuredMethod(True)>
    Public Sub AuditRecordProcessOrVboEvent(eventCode As ProcessOrVboEventCode,
                                            isVBO As Boolean,
                                            processId As Guid,
                                            comments As String,
                                            newXML As String,
                                            summary As String) Implements IServer.AuditRecordProcessOrVboEvent
        CheckPermissions()
        Using con = GetConnection()
            AuditRecordProcessOrVboEvent(con, CType(eventCode, ProcessOrVboEventCode),
             isVBO, processId, comments, newXML, summary)
        End Using
    End Sub


    ''' <summary>
    ''' Records an audit event for a process or VBO, defined by the given properties.
    ''' This will ensure that the appropriate audit event code from
    ''' <see cref="ProcessEventCode"/> or <see cref="BusinessObjectEventCode"/> is
    ''' used in the audit record, along with the appropriate narrative describing
    ''' the audited event.
    ''' </summary>
    ''' <param name="cOn">The connection over which the audit record should be
    ''' written.</param>
    ''' <param name="eventCode">The event code detailing the operation which has
    ''' occurred on the process / vbo</param>
    ''' <param name="isVBO">True to indicate that a VBO is being audited here;
    ''' False to indicate a process.</param>
    ''' <param name="processId">The ID of the process / VBO.</param>
    ''' <param name="comments">Comments regarding the auditable event.</param>
    ''' <param name="newXML">The new XML from the process / VBO</param>
    ''' <param name="summary">A summary of the auditable event.</param>
    ''' <exception cref="AuditOperationFailedException">If any SQL errors occur
    ''' while attempting to write the audit record</exception>
    Private Sub AuditRecordProcessOrVboEvent(con As IDatabaseConnection,
                                             eventCode As ProcessOrVboEventCode,
                                             isVBO As Boolean,
                                             processId As Guid,
                                             comments As String,
                                             newXML As String,
                                             summary As String)

        Try
            If Not gAuditingEnabled Then Return

            ' ProcessOrVboEventCode's code has a question mark placeholder where the
            ' "B" or "P" for business objects or processes respectively, need to go
            Dim eventCodePrefix As Char = If(isVBO, "B"c, "P"c)

            ' Get the name of the process being audited.
            Dim processName As String = GetProcessNameById(con, processId)

            ' Get the narrative, adding the placeholder text as appropriate.
            Dim narrative As String = GetNarrative(eventCode,
             GetLoggedInUserName(), processName, If(isVBO, My.Resources.ResourceManager.GetString("clsServer_AuditRecordProcessOrVboEvent_BusinessObject", AuditLocale), My.Resources.ResourceManager.GetString("clsServer_AuditRecordProcessOrVboEvent_Process", AuditLocale)))

            Dim cmd As New SqlCommand(
             "INSERT INTO BPAAuditEvents" &
             " (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtProcID, " &
             "   comments, newXML, EditSummary) VALUES " &
             " (GETUTCDATE(), @EventCode, @Narrative, @SrcUserID, @TgtProcessID, " &
             "   @Comments, @NewXML, @Summary)")
            With cmd.Parameters
                .AddWithValue("@EventCode", GetEventCode(eventCode).Replace("?"c, eventCodePrefix))
                .AddWithValue("@Narrative", narrative)
                .AddWithValue("@SrcUserID", GetLoggedInUserId())
                .AddWithValue("@TgtProcessID", processId)
                .AddWithValue("@Comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
                .AddWithValue("@NewXML", IIf(newXML IsNot Nothing, newXML, DBNull.Value))
                .AddWithValue("@Summary", IIf(summary IsNot Nothing, summary, DBNull.Value))
            End With
            con.Execute(cmd)

        Catch sqle As SqlException
            Throw New AuditOperationFailedException(sqle)

        End Try

    End Sub

    ''' <summary>
    ''' Record an object-related event
    ''' </summary>
    ''' <param name="cOn">The connection to use</param>
    ''' <param name="eventCode">The code of the event.</param>
    ''' <param name="businessObjectName">The name of the business object affected.</param>
    ''' <param name="comments">Any further comments about the event. Leave blank
    ''' if desired.</param>
    ''' <returns>True if successful, otherwise false</returns>
    Private Function AuditRecordObjectEvent(con As IDatabaseConnection,
                                            ByVal eventCode As ObjectEventCode,
                                            ByVal businessObjectName As String, ByVal comments As String) As Boolean

        If Not gAuditingEnabled Then Return True


        Dim narrative As String

        Select Case eventCode
            Case ObjectEventCode.AddObject
                narrative = String.Format(My.Resources.ResourceManager.GetString("clsServer_TheUser0AddedTheNewBusinessObjectWebService1", AuditLocale), GetLoggedInUserName(), businessObjectName)
            Case ObjectEventCode.DeleteObject
                narrative = String.Format(My.Resources.ResourceManager.GetString("clsServer_TheUser0DeletedTheBusinessObjectWebService1", AuditLocale), GetLoggedInUserName(), businessObjectName)
            Case ObjectEventCode.UpdateObject
                narrative = String.Format(My.Resources.ResourceManager.GetString("clsServer_TheUser0UpdatedTheBusinessObjectWebService1", AuditLocale), GetLoggedInUserName(), businessObjectName)
            Case ObjectEventCode.ConfigureObject
                narrative = String.Format(My.Resources.ResourceManager.GetString("clsServer_TheUser0ChangedTheConfigurationOfTheBusinessObjectWebService1", AuditLocale), GetLoggedInUserName(), businessObjectName)
            Case Else
                narrative = ""
        End Select

        'take current date and time and dump with everything else to new table row
        Try
            Dim cmd As New SqlCommand("INSERT INTO BPAAuditEvents (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) Select GETUTCDATE(), @EventCode, @Narrative, @SrcUserID, ResourceID, @Comments FROM BPAResource WHERE [Name] = @MachineName")
            With cmd.Parameters
                .AddWithValue("@EventCode", GetEventCode(eventCode))
                .AddWithValue("@Narrative", narrative)
                .AddWithValue("@SrcUserID", GetLoggedInUserId())
                .AddWithValue("@Comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
                .AddWithValue("@MachineName", mLoggedInMachine)
            End With
            con.ExecuteReturnRecordsAffected(cmd)
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

#End Region

#Region " SysConfig "

    ''' <summary>
    ''' Record a sysconfig-related event.
    ''' </summary>
    ''' <param name="eventCode">The event code</param>
    ''' <param name="comments">The event comments</param>
    ''' <param name="args">Even specific arguments</param>
    ''' <returns>True if successful, otherwise false</returns>
    <SecuredMethod(True)>
    Public Function AuditRecordSysConfigEvent(ByVal eventCode As SysConfEventCode, ByVal comments As String, ParamArray args() As String) As Boolean Implements IServer.AuditRecordSysConfigEvent
        CheckPermissions()
        Try
            Using con = GetConnection()
                AuditRecordSysConfigEvent(con, eventCode, comments, args)
            End Using
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Record a sysconfig-related event.
    ''' </summary>
    ''' <param name="con">The connection to the db</param>
    ''' <param name="eventCode">The event code</param>
    ''' <param name="args">list of event specific paramters</param>
    ''' <param name="comments">The event comments</param>
    Private Sub AuditRecordSysConfigEvent(ByVal con As IDatabaseConnection, ByVal eventCode As SysConfEventCode, ByVal comments As String, ParamArray args() As String)
        If Not gAuditingEnabled Then Return

        ' Prepend the username to the arguments list
        Dim eventArgs = New List(Of String) From {GetLoggedInUserName()}
        If args IsNot Nothing Then eventArgs.AddRange(args)

        ' If the event is ModifyArchice Args(0) is possibly the resource name
        ' we need to put the ID of the resource on the database record
        Dim targetResourceID As Guid = Guid.Empty
        If eventCode = SysConfEventCode.ModifyArchive _
            AndAlso args IsNot Nothing _
            AndAlso args.Length > 0 _
            AndAlso Not String.IsNullOrWhiteSpace(args(0)) Then

            Try
                targetResourceID = GetResourceId(con, args(0))
            Catch
                ' Don't set it, use the default.
            End Try
        End If

        'Form a narrative for the event based on supplied arguments
        Dim narrative As String = GetNarrative(eventCode, eventArgs.ToArray())

        'take current date and time and dump with everything else to new table row
        Using cmd As New SqlCommand("INSERT INTO BPAAuditEvents (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) VALUES (GETUTCDATE(), @EventCode, @Narrative, @SrcUserID, @TgtResourceID, @Comments)")
            With cmd.Parameters
                .AddWithValue("@EventCode", GetEventCode(eventCode))
                .AddWithValue("@Narrative", narrative)
                .AddWithValue("@SrcUserID", GetLoggedInUserId)
                .AddWithValue("@TgtResourceID", targetResourceID)
                .AddWithValue("@Comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
            End With
            con.ExecuteReturnRecordsAffected(cmd)
        End Using
    End Sub

    Private Function AuditRecordWebSettingsEvent(
                            ByVal con As IDatabaseConnection,
                            ByVal eventCode As WebSettingsEventCode,
                            ByVal comments As String) As Boolean
        If Not gAuditingEnabled Then Return True

        Try
            Using cmd As New SqlCommand("INSERT INTO BPAAuditEvents (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) VALUES (GETUTCDATE(), @EventCode, @Narrative, @SrcUserID, @TgtResourceID, @Comments)")
                With cmd.Parameters
                    .AddWithValue("@EventCode", GetEventCode(eventCode))
                    .AddWithValue("@Narrative", GetNarrative(eventCode, {GetLoggedInUserName()}))
                    .AddWithValue("@SrcUserID", GetLoggedInUserId)
                    .AddWithValue("@TgtResourceID", Guid.Empty)
                    .AddWithValue("@Comments", If(comments, ""))
                End With
                con.ExecuteReturnRecordsAffected(cmd)
            End Using

            Return True
        Catch
            Return False
        End Try
    End Function

#End Region

#Region " Resource "

    ''' <summary>
    ''' Record a resource-related event.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="eventCode">The code of the event.</param>
    ''' <param name="targetResourceName">The name of the resource affected.</param>
    ''' <param name="targetPoolName">The name of the pool affected.</param>
    ''' <param name="comments">Any further comments about the event. Leave blank
    ''' if desired.</param>
    ''' <returns>True if successful, otherwise false</returns>
    Private Function AuditRecordResourceEvent(con As IDatabaseConnection, eventCode As ResourceEventCode, targetResourceName As String, targetPoolName As String, comments As String) As Boolean

        If Not gAuditingEnabled Then Return True

        Dim narrative As String
        Dim targetResourceID As Guid

        'get target resource id from Target resource name
        Try
            targetResourceID = GetResourceId(targetResourceName)
        Catch
            targetResourceID = Guid.Empty
        End Try

        Select Case eventCode
            Case ResourceEventCode.ChangedAttributes
                narrative = String.Format(My.Resources.ResourceManager.GetString("clsServer_TheUser0ChangedTheAttributesOfTheResource1", AuditLocale), GetLoggedInUserName(), targetResourceName)
            Case ResourceEventCode.AddResourceToPool
                narrative = String.Format(My.Resources.ResourceManager.GetString("clsServer_TheUser0AddedTheResource1ToThePool2", AuditLocale), GetLoggedInUserName(), targetResourceName, targetPoolName)
            Case ResourceEventCode.RemoveResourceFromPool
                narrative = String.Format(My.Resources.ResourceManager.GetString("clsServer_TheUser0RemovedTheResource1FromThePool2", AuditLocale), GetLoggedInUserName(), targetResourceName, targetPoolName)
            Case ResourceEventCode.CreatePool
                narrative = String.Format(My.Resources.ResourceManager.GetString("clsServer_TheUser0CreatedAPoolCalled1", AuditLocale), GetLoggedInUserName(), targetPoolName)
            Case ResourceEventCode.DeletePool
                narrative = String.Format(My.Resources.ResourceManager.GetString("clsServer_TheUser0DeletedAPoolCalled1", AuditLocale), GetLoggedInUserName(), targetPoolName)
            Case Else
                narrative = ""
        End Select

        'take current date and time and dump with everything else to new table row
        Try
            Dim cmd As New SqlCommand("INSERT INTO BPAAuditEvents (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) VALUES (GETUTCDATE(), @EventCode, @Narrative, @SrcUserID, @TgtResourceID, @Comments)")
            With cmd.Parameters
                .AddWithValue("@EventCode", GetEventCode(eventCode))
                .AddWithValue("@Narrative", narrative)
                .AddWithValue("@SrcUserID", GetLoggedInUserId())
                .AddWithValue("@TgtResourceID", targetResourceID.ToString)
                .AddWithValue("@Comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
            End With
            con.ExecuteReturnRecordsAffected(cmd)
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

#End Region

#Region " Schedule / Calendar "

    ''' <summary>
    ''' Records an audit event on a calendar.
    ''' </summary>
    ''' <param name="con">The connection to use to write the audit record.</param>
    ''' <param name="evt">The event code for the audit event being recorded</param>
    ''' <param name="cal">The calendar that the event refers to</param>
    Private Sub RecordCalendarEvent(ByVal con As IDatabaseConnection,
     ByVal evt As CalendarEventCode, ByVal cal As ScheduleCalendar)

        If Not gAuditingEnabled Then Return

        Dim narr As String = GetNarrative(evt, GetLoggedInUserName(), cal.ToString)

        Dim cmd As New SqlCommand(
         "insert into BPAAuditEvents " &
         " (eventdatetime, sCode, sNarrative, gSrcUserID, comments) " &
         " values (getutcdate(), @evt, @narr, @userid, @comments)")

        With cmd.Parameters
            .AddWithValue("@evt", GetEventCode(evt))
            .AddWithValue("@narr", narr)
            .AddWithValue("@userid", GetLoggedInUserId())
            Dim comments As String = Nothing
            Select Case evt
                Case CalendarEventCode.Created, CalendarEventCode.Modified
                    comments = My.Resources.ResourceManager.GetString("clsServerAudit_Configuration", AuditLocale) & cal.Configuration
            End Select
            .AddWithValue("@comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
        End With

        con.Execute(cmd)

    End Sub

    ''' <summary>
    ''' Records a schedule audit event using the values in the given event object.
    ''' </summary>
    ''' <param name="con">The connection over which the audit event should be logged
    ''' </param>
    ''' <param name="evt">The event to log.</param>
    Private Sub AuditRecordScheduleEvent(
     ByVal con As IDatabaseConnection, ByVal evt As ScheduleAuditEvent)

        If Not gAuditingEnabled Then Return

        ' We have guids and numbers - we need to translate them into names
        Dim userName, scheduleName, taskName, resourceName, processName As String

        Dim cmd As New SqlCommand(
         "select isnull(u.username, '['+u.systemusername+']') as username " &
         " from BPAUser u where u.userid=@id")
        cmd.Parameters.AddWithValue("@id", evt.UserId)
        userName = CStr(con.ExecuteReturnScalar(cmd))

        cmd = New SqlCommand("select name from BPASchedule where id=@id")
        cmd.Parameters.AddWithValue("@id", evt.ScheduleId)
        scheduleName = CStr(con.ExecuteReturnScalar(cmd))

        cmd = New SqlCommand("select name from BPATask where id=@id")
        cmd.Parameters.AddWithValue("@id", evt.TaskId)
        taskName = CStr(con.ExecuteReturnScalar(cmd))

        cmd = New SqlCommand("select r.name from BPAResource r where r.resourceid=@id")
        cmd.Parameters.AddWithValue("@id", evt.ResourceId)
        resourceName = CStr(con.ExecuteReturnScalar(cmd))

        cmd = New SqlCommand("select p.name from BPAProcess p where p.processid=@id")
        cmd.Parameters.AddWithValue("@id", evt.ProcessId)
        processName = CStr(con.ExecuteReturnScalar(cmd))

        ' The narrative for each event type is stored in an attribute in the event
        ' code attribute associated with the event code enum.
        ' The GetNarrative() call for schedule events expects the following arguments:
        ' <list>
        ' <item>0 : Username</item>
        ' <item>1 : Schedule name</item>
        ' <item>2 : Task name</item>
        ' <item>3 : Process name</item>
        ' <item>4 : Resource name</item>
        ' </list>

        Dim narrative As String = GetNarrative(evt.Code,
         userName, scheduleName, taskName, processName, resourceName)
        Dim comments As String = evt.Comment

        cmd = New SqlCommand(
         "insert into BPAAuditEvents " &
         " (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtProcId, gTgtResourceID, comments) " &
         " values (getutcdate(), @evt, @narr, @userid, @processid, @resourceid, @comments)")
        With cmd.Parameters
            .AddWithValue("@evt", GetEventCode(evt.Code))
            .AddWithValue("@narr", narrative)
            .AddWithValue("@userid", evt.UserId)
            .AddWithValue("@processid", evt.ProcessId)
            .AddWithValue("@resourceid", evt.ResourceId)
            .AddWithValue("@comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
        End With
        con.Execute(cmd)

    End Sub

#End Region

#Region " Work Queue Analysis "

    Friend Sub AuditRecordSnapshotConfigurationChangesEvent(con As IDatabaseConnection,
        eventCode As WorkQueueAnalysisEventCode, oldConfig As SnapshotConfiguration, config As SnapshotConfiguration)

        If Not gAuditingEnabled Then Return

        Dim snapshotIDInfo = String.Format(My.Resources.ResourceManager.GetString("AuditWQA_SnapshotConfigID0", AuditLocale), config.Id)
        Dim narrative = GetNarrative(eventCode, GetLoggedInUserName(), config.Name)
        Dim helper = New WorkQueueAnalysisAuditHelper()
        Dim resourceManager = WorkQueueAnalysis_Resources.ResourceManager
        Dim comment As New StringBuilder()
        If oldConfig.Name <> config.Name Then _
            comment.Append(String.Format(My.Resources.ResourceManager.GetString("AuditWQA_SnapshotConfigChangesOldNewValues", AuditLocale), My.Resources.ResourceManager.GetString("AuditWQA_Name", AuditLocale), oldConfig.Name, config.Name) + " ")
        If oldConfig.Interval <> config.Interval Then _
            comment.Append(String.Format(My.Resources.ResourceManager.GetString("AuditWQA_SnapshotConfigChangesOldNewValues", AuditLocale), My.Resources.ResourceManager.GetString("AuditWQA_Interval", AuditLocale), oldConfig.Interval.ToLocalizedString(resourceManager), config.Interval.ToLocalizedString(resourceManager)) + " ")
        If oldConfig.Timezone.DisplayName IsNot config.Timezone.DisplayName Then _
            comment.Append(String.Format(My.Resources.ResourceManager.GetString("AuditWQA_SnapshotConfigChangesOldNewValues", AuditLocale), My.Resources.ResourceManager.GetString("AuditWQA_Timezone", AuditLocale), oldConfig.Timezone.DisplayName, config.Timezone.DisplayName) + " ")
        If oldConfig.StartTime.TickOfDay <> config.StartTime.TickOfDay Then _
            comment.Append(String.Format(My.Resources.ResourceManager.GetString("AuditWQA_SnapshotConfigChangesOldNewValues", AuditLocale), My.Resources.ResourceManager.GetString("AuditWQA_StartTime", AuditLocale), oldConfig.StartTime, config.StartTime) + " ")
        If oldConfig.EndTime.TickOfDay <> config.EndTime.TickOfDay Then _
            comment.Append(String.Format(My.Resources.ResourceManager.GetString("AuditWQA_SnapshotConfigChangesOldNewValues", AuditLocale), My.Resources.ResourceManager.GetString("AuditWQA_EndTime", AuditLocale), oldConfig.EndTime, config.EndTime) + " ")
        If oldConfig.Enabled <> config.Enabled Then _
            comment.Append(String.Format(My.Resources.ResourceManager.GetString("AuditWQA_SnapshotConfigChangesOldNewValues", AuditLocale), My.Resources.ResourceManager.GetString("AuditWQA_Enabled", AuditLocale), oldConfig.Enabled, config.Enabled) + " ")

        If Not oldConfig.DaysOfTheWeek.IsEqualTo(config.DaysOfTheWeek) Then
            Dim daysAdded = helper.GetDaysAdded(config.DaysOfTheWeek, oldConfig.DaysOfTheWeek)
            Dim daysRemoved = helper.GetDaysRemoved(config.DaysOfTheWeek, oldConfig.DaysOfTheWeek)
            comment.Append(String.Format(My.Resources.ResourceManager.GetString("AuditWQA_SnapshotDaysOfTheWeekAdded0Removed1", AuditLocale), daysAdded, daysRemoved))
        End If

        If comment.Length > 0 Then
            Dim auditInformation = $"{snapshotIDInfo} {comment.ToString()}"
            AuditRecordSnapshotConfigurationEvent(con, eventCode, narrative, auditInformation)
        End If

    End Sub

    Private Sub AuditRecordSnapshotConfigurationEvent(con As IDatabaseConnection,
                                             eventCode As WorkQueueAnalysisEventCode, narrative As String, comments As String)

        Using cmd As New SqlCommand("
                insert into BPAAuditEvents (eventdatetime, sCode, sNarrative, gSrcUserID, comments)
                values (GETUTCDATE(), @EventCode, @Narrative, @SrcUserID, @Comments)")

            With cmd.Parameters
                .AddWithValue("@EventCode", GetEventCode(eventCode))
                .AddWithValue("@Narrative", narrative)
                .AddWithValue("@SrcUserID", GetLoggedInUserId)
                .AddWithValue("@Comments", comments)
            End With
            con.ExecuteReturnRecordsAffected(cmd)
        End Using

    End Sub

    Friend Sub AuditRecordWorkQueueAnalysisEvent(con As IDatabaseConnection, eventCode As WorkQueueAnalysisEventCode,
                                                 snapshotName As String, comments As String)

        Dim narrative = GetNarrative(eventCode, GetLoggedInUserName(), snapshotName)

        AuditRecordSnapshotConfigurationEvent(con, eventCode, narrative, comments)
    End Sub

#End Region

#Region " Work Queue "

    ''' <summary>
    ''' Record a workqueue-related event.
    ''' </summary>
    ''' <param name="eventCode">The code of the event.</param>
    ''' <param name="queueName">The name of the queue, for events where the name
    ''' cannot be looked up based on its ID (eg rename, delete queue).</param>
    ''' <param name="queueID">The ID of the queue affected.</param>
    ''' <param name="comments">Any further comments about the event. Leave blank
    ''' if desired.</param>
    ''' <returns>True if successful, otherwise false</returns>
    <SecuredMethod(True)>
    Public Function AuditRecordWorkQueueEvent(
     ByVal eventCode As WorkQueueEventCode,
     ByVal queueID As Guid,
     ByVal queueName As String,
     ByVal comments As String) As Boolean Implements IServer.AuditRecordWorkQueueEvent
        CheckPermissions()
        Try
            Using con = GetConnection()
                AuditRecordWorkQueueEvent(con, eventCode, queueID, queueName, comments)
                Return True
            End Using
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Record a workqueue-related event.
    ''' </summary>
    ''' <param name="con">The connection over which the audit should be recorded.
    ''' </param>
    ''' <param name="eventCode">The code of the event.</param>
    ''' <param name="queueName">The name of the queue, for events where the name
    ''' cannot be looked up based on its ID (eg rename, delete queue). If this
    ''' argument is given (ie. not null or empty), it will be used in place of the
    ''' queue's name as currently held on the database under the given ID.</param>
    ''' <param name="queueID">The ID of the queue affected.</param>
    ''' <param name="comments">Format string containing any further (optional)
    ''' comments about the event.</param>
    ''' <exception cref="Exception">If any errors occur while recording the work
    ''' queue event.</exception>
    Private Sub AuditRecordWorkQueueEvent(
     ByVal con As IDatabaseConnection,
     ByVal eventCode As WorkQueueEventCode,
     ByVal queueID As Guid,
     ByVal queueName As String,
     ByVal comments As String)

        If Not gAuditingEnabled Then Return

        ' If we don't have a queue name given to us, derive it from the queue record
        ' which corresponds to the given queue ID.
        If queueName = "" Then
            Dim queue As WorkQueueWithGroup = GetQueueById(con, queueID)
            If queue IsNot Nothing Then queueName = queue.Name
        End If

        Dim cmd As New SqlCommand(
         " insert into BPAAuditEvents " &
         " (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) " &
         " values (getutcdate(), @eventcode, @narrative, @userid, @resourceid, @comments)")

        With cmd.Parameters
            .AddWithValue("@eventcode", GetEventCode(eventCode))
            .AddWithValue("@narrative", GetNarrative(eventCode, GetLoggedInUserName(), queueName))
            .AddWithValue("@userid", GetLoggedInUserId())
            .AddWithValue("@resourceid", Guid.Empty)
            .AddWithValue("@comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
        End With

        con.ExecuteReturnRecordsAffected(cmd)

    End Sub

#End Region

#Region " Font "

    ''' <summary>
    ''' Records an audit event for a font
    ''' </summary>
    ''' <param name="con">The connection on which to record the audit event</param>
    ''' <param name="eventCode">The event code indicating the type of audit event
    ''' being recorded</param>
    ''' <param name="fontName">The (current) name of the font</param>
    Private Sub AuditRecordFontEvent(
     ByVal con As IDatabaseConnection,
     ByVal eventCode As FontEventCode,
     ByVal fontName As String)
        AuditRecordFontEvent(con, eventCode, fontName, fontName)
    End Sub

    ''' <summary>
    ''' Records an audit event for a font
    ''' </summary>
    ''' <param name="con">The connection on which to record the audit event</param>
    ''' <param name="eventCode">The event code indicating the type of audit event
    ''' being recorded</param>
    ''' <param name="fontName">The (current) name of the font</param>
    ''' <param name="prevFontName">The previous name of the font, if appropriate.
    ''' This is only relevant for modify audit events and in that case it should
    ''' always contain the previous font name, even if it is identical to
    ''' <paramref name="fontName"/>
    ''' </param>
    Private Sub AuditRecordFontEvent(
     ByVal con As IDatabaseConnection,
     ByVal eventCode As FontEventCode,
     ByVal fontName As String,
     ByVal prevFontName As String)

        If Not gAuditingEnabled Then Return

        Dim cmd As New SqlCommand(
         " insert into BPAAuditEvents " &
         " (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) " &
         " values (getutcdate(), @evtcode, @narr, @userid, @resourceid, @comments)")

        ' If this is a modify event, we check the prevFontName - if it is different
        ' to the font name, we substitute a ModifyWithRename event code in.
        If eventCode = FontEventCode.Modify AndAlso fontName <> prevFontName Then _
         eventCode = FontEventCode.ModifyWithRename

        With cmd.Parameters
            .AddWithValue("@evtcode", GetEventCode(eventCode))
            .AddWithValue("@narr",
              GetNarrative(eventCode, GetLoggedInUserName(), fontName, prevFontName))
            .AddWithValue("@userid", GetLoggedInUserId())
            .AddWithValue("@resourceid", Guid.Empty)
            .AddWithValue("@comments", String.Format(My.Resources.ResourceManager.GetString("clsServer_LoggedIntoResource0", AuditLocale), mLoggedInMachine))
        End With

        con.ExecuteReturnRecordsAffected(cmd)

    End Sub


#End Region

#Region " Credentials "

    ''' <summary>
    ''' Record an audit event for a credential
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="eventCode">The audit event code</param>
    Private Sub AuditRecordCredentialsEvent(
     ByVal con As IDatabaseConnection,
     ByVal eventCode As CredentialsEventCode,
     ByVal cred As clsCredential,
     passwordChanged As Boolean)

        AuditRecordCredentialsEvent(con, eventCode, cred.Name, cred, passwordChanged)
    End Sub

    ''' <summary>
    ''' Record an audit event for a credential
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="eventCode">The audit event code</param>
    ''' <param name="prevName">The previous name of the credentials (if appropriate)
    ''' This should be the same as the <see cref="clsCredential.Name">credential's
    ''' Name</see> unless it is a Modify event and the name has been changed.
    ''' </param>
    Private Sub AuditRecordCredentialsEvent(
     ByVal con As IDatabaseConnection,
     ByVal eventCode As CredentialsEventCode,
     ByVal prevName As String,
     ByVal cred As clsCredential,
     passwordChanged As Boolean)

        If Not gAuditingEnabled Then Return

        Dim cmd As New SqlCommand(
          " insert into BPAAuditEvents " &
          " (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) " &
          " values (getutcdate(), @evtcode, @narr, @userid, @resourceid, @comments)")

        ' If this is a Modify event and the name has changed then upgrade
        ' to a ModifyWithRename event.
        If eventCode = CredentialsEventCode.Modify AndAlso cred.Name <> prevName Then _
         eventCode = CredentialsEventCode.ModifyWithRename

        ' Generate credential summary, but not for delete events
        Dim comments As String
        If eventCode = CredentialsEventCode.Delete Then
            comments = String.Empty
        Else
            comments = CredentialSummary(con, cred, passwordChanged)
        End If

        With cmd.Parameters
            .AddWithValue("@evtcode", GetEventCode(eventCode))
            .AddWithValue("@narr", GetNarrative(eventCode, GetLoggedInUserName(), cred.Name, prevName))
            .AddWithValue("@userid", GetLoggedInUserId())
            .AddWithValue("@resourceid", Guid.Empty)
            .AddWithValue("@comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
        End With

        con.ExecuteReturnRecordsAffected(cmd)

    End Sub

    ''' <summary>
    ''' Returns a textual summary of the credential
    ''' </summary>
    Private Function CredentialSummary(con As IDatabaseConnection,
     cred As clsCredential,
     passwordChanged As Boolean) As String

        Dim sb As New StringBuilder()
        Dim list As New List(Of String)

        sb.Append(My.Resources.ResourceManager.GetString("clsServer_Username", AuditLocale) & cred.Username)
        sb.Append(My.Resources.ResourceManager.GetString("clsServer_Password_Changed", AuditLocale) & If(passwordChanged, My.Resources.ResourceManager.GetString("clsServer_Yes"), My.Resources.ResourceManager.GetString("clsServer_No")))
        sb.Append(My.Resources.ResourceManager.GetString("clsServer_CredentialSummary_Description", AuditLocale) & cred.Description)

        sb.Append(My.Resources.ResourceManager.GetString("clsServer_CredentialSummary_AuthType", AuditLocale) & cred.Type.LocalisedTitle())

        If cred.ExpiryDate <> Date.MinValue Then
            sb.Append(My.Resources.ResourceManager.GetString("clsServer_Expires", AuditLocale) & cred.ExpiryDate.ToShortDateString())
        Else
            sb.Append(My.Resources.ResourceManager.GetString("clsServer_ExpiresNever", AuditLocale))
        End If
        sb.Append(My.Resources.ResourceManager.GetString("clsServer_Invalid", AuditLocale) & cred.IsInvalid.ToString())

        sb.Append(My.Resources.ResourceManager.GetString("clsServer_Properties", AuditLocale))
        If cred.Properties.Count = 0 Then
            sb.Append(My.Resources.ResourceManager.GetString("clsServer_None", AuditLocale))
        Else
            sb.Append("(")
            CollectionUtil.JoinInto(cred.Properties.Keys, ",", sb)
            sb.Append(")")
        End If

        sb.Append(My.Resources.ResourceManager.GetString("clsServer_Access", AuditLocale))
        If cred.IsForAllProcesses Then
            sb.Append(My.Resources.ResourceManager.GetString("clsServer_AllProcesses", AuditLocale))
        Else
            list.Clear()
            For Each proc As Guid In cred.ProcessIDs
                list.Add(GetProcessNameById(con, proc))
            Next
            Dim sbList As New StringBuilder()
            sb.Append(String.Format(My.Resources.ResourceManager.GetString("clsServer_Processes0", AuditLocale), CollectionUtil.JoinInto(list, ",", sbList)))
        End If

        If cred.IsForAllResources Then
            sb.Append(My.Resources.ResourceManager.GetString("clsServer_AllResources", AuditLocale))
        Else
            list.Clear()
            For Each res As Guid In cred.ResourceIDs
                list.Add(Me.GetResourceName(con, res))
            Next
            Dim sbList As New StringBuilder()
            sb.Append(String.Format(My.Resources.ResourceManager.GetString("clsServer_Resources0", AuditLocale), CollectionUtil.JoinInto(list, ",", sbList)))
        End If

        If cred.IsForAllRoles Then
            sb.Append(My.Resources.ResourceManager.GetString("clsServer_AnyRole", AuditLocale))
        Else
            list.Clear()
            For Each role As Role In cred.Roles
                list.Add(LTools.GetC(role.Name, "roleperms", "role"))
            Next
            Dim sbList As New StringBuilder()
            sb.Append(String.Format(My.Resources.ResourceManager.GetString("clsServer_Roles0", AuditLocale), CollectionUtil.JoinInto(list, ",", sbList)))
        End If

        Return sb.ToString()
    End Function

#End Region

#Region " Dashboards and tiles "

    ''' <summary>
    ''' Record an audit event for a dashboard.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="eventCode">Event code</param>
    ''' <param name="dashboard">The dashboard</param>
    Private Sub AuditRecordDashboardEvent(con As IDatabaseConnection, eventCode As DashboardEventCode, dashboard As Dashboard)
        If Not gAuditingEnabled Then Return

        Dim cmd As New SqlCommand(
          " insert into BPAAuditEvents " &
          " (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) " &
          " values (getutcdate(), @evtcode, @narr, @userid, @resourceid, @comments)")

        ' Generate dashboard summary (max 512 chars)
        Dim comments As String = String.Empty
        If eventCode = DashboardEventCode.SetHomePage OrElse
         eventCode = DashboardEventCode.DeleteDashboard Then
            comments = My.Resources.ResourceManager.GetString("clsServer_DashboardID", AuditLocale) & dashboard.ID.ToString()
        Else
            comments = DashboardSummary(dashboard)
        End If

        With cmd.Parameters
            .AddWithValue("@evtcode", GetEventCode(eventCode))
            .AddWithValue("@narr", GetNarrative(eventCode, GetLoggedInUserName(), dashboard.Name))
            .AddWithValue("@userid", GetLoggedInUserId())
            .AddWithValue("@resourceid", Guid.Empty)
            .AddWithValue("@comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
        End With

        con.ExecuteReturnRecordsAffected(cmd)
    End Sub

    ''' <summary>
    ''' Returns a textual summary of the passed dashboard.
    ''' </summary>
    ''' <param name="dashboard">The dashboard</param>
    ''' <returns>Textual summary</returns>
    Private Function DashboardSummary(dashboard As Dashboard) As String
        Dim sb As New StringBuilder()

        sb.Append(My.Resources.ResourceManager.GetString("clsServer_DashboardSummary_ID", AuditLocale) & dashboard.ID.ToString())
        sb.Append(My.Resources.ResourceManager.GetString("clsServer_Name", AuditLocale) & dashboard.Name)
        sb.Append(My.Resources.ResourceManager.GetString("clsServer_Type", AuditLocale) & Ltools.Get(dashboard.Type.ToString(),"tile",AuditLocale.Name,"Type"))

        Dim tileList As New List(Of String)
        For Each tile As DashboardTile In dashboard.Tiles
            tileList.Add(String.Format("{0} ({1}x{2})", tile.Tile.Name, tile.Size.Width, tile.Size.Height))
        Next

        If tileList.Count > 0 Then
            Dim sbList As New StringBuilder()
            sb.Append(String.Format(My.Resources.ResourceManager.GetString("clsServer_Tiles0", AuditLocale), CollectionUtil.JoinInto(tileList, ",", sbList)))
        End If

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Record an audit event for a tile.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="eventCode">Event code</param>
    ''' <param name="tile">The tile</param>
    Private Sub AuditRecordDashboardTileEvent(con As IDatabaseConnection, eventCode As DashboardEventCode,
     tile As Tile, formattedProperties As String)
        If Not gAuditingEnabled Then Return

        Dim cmd As New SqlCommand(
          " insert into BPAAuditEvents " &
          " (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) " &
          " values (getutcdate(), @evtcode, @narr, @userid, @resourceid, @comments)")

        ' Generate tile summary (max 512 chars)
        Dim comments As String = String.Empty
        If eventCode = DashboardEventCode.DeleteTile Then
            comments = My.Resources.ResourceManager.GetString("clsServer_TileID", AuditLocale) & tile.ID.ToString()
        Else
            comments = TileSummary(tile, formattedProperties)
        End If

        With cmd.Parameters
            .AddWithValue("@evtcode", GetEventCode(eventCode))
            .AddWithValue("@narr", GetNarrative(eventCode, GetLoggedInUserName(), tile.Name))
            .AddWithValue("@userid", GetLoggedInUserId())
            .AddWithValue("@resourceid", Guid.Empty)
            .AddWithValue("@comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
        End With

        con.ExecuteReturnRecordsAffected(cmd)
    End Sub

    ''' <summary>
    ''' Returns a textual summary of the passed tile.
    ''' </summary>
    ''' <param name="tile">The tile</param>
    ''' <returns>Textual summary</returns>
    Private Function TileSummary(tile As Tile, formattedProperties As String) As String
        Dim sb As New StringBuilder()
        Dim chartTypeLocalised = My.Resources.ResourceManager.GetString("clsServer_ChartType_" & tile.Type.ToString(), AuditLocale)
        Dim refreshIntervalLocalised = My.Resources.ResourceManager.GetString("clsServer_RefreshInterval_" & tile.RefreshInterval.ToString(), AuditLocale)

        sb.Append(My.Resources.ResourceManager.GetString("clsServer_TileSummary_ID", AuditLocale) & tile.ID.ToString())
        sb.Append(My.Resources.ResourceManager.GetString("clsServer_Name", AuditLocale) & tile.Name)
        sb.Append(My.Resources.ResourceManager.GetString("clsServer_Type", AuditLocale) &
                  DirectCast(IIf(String.IsNullOrEmpty(chartTypeLocalised), tile.Type.ToString(), chartTypeLocalised), String))
        If tile.RefreshInterval >= 0 Then _
         sb.Append(My.Resources.ResourceManager.GetString("clsServer_Refresh", AuditLocale) &
                  DirectCast(IIf(String.IsNullOrEmpty(refreshIntervalLocalised), tile.RefreshInterval.GetFriendlyName(), refreshIntervalLocalised), String))
        If tile.Description <> String.Empty Then _
         sb.Append(My.Resources.ResourceManager.GetString("clsServer_Description", AuditLocale) & tile.Description)
        If tile.XMLProperties <> String.Empty Then _
         sb.Append(String.Format(My.Resources.ResourceManager.GetString("clsServer_Properties0", AuditLocale), formattedProperties))

        Return sb.ToString()
    End Function

#End Region

#Region " Key Store "

    ''' <summary>
    ''' Records an audit event for an encryption scheme
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="eventCode">The event code</param>
    ''' <param name="scheme">The encryption scheme</param>
    Private Sub AuditRecordKeyStoreEvent(
     ByVal con As IDatabaseConnection,
     ByVal eventCode As KeyStoreEventCode,
     ByVal scheme As clsEncryptionScheme)
        AuditRecordKeyStoreEvent(con, eventCode, scheme, scheme.Name)
    End Sub

    ''' <summary>
    ''' Records an audit event for an encryption scheme
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="eventCode">The event code</param>
    ''' <param name="scheme">The encryption scheme</param>
    ''' <param name="prevSchemeName">The previous name of the encryption scheme, if
    ''' appropriate.</param>
    Private Sub AuditRecordKeyStoreEvent(
     ByVal con As IDatabaseConnection,
     ByVal eventCode As KeyStoreEventCode,
     ByVal scheme As clsEncryptionScheme,
     ByVal prevSchemeName As String)

        If Not gAuditingEnabled Then Return

        Dim cmd As New SqlCommand(
         " insert into BPAAuditEvents " &
         " (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) " &
         " values (getutcdate(), @evtcode, @narr, @userid, @resourceid, @comments)")

        ' If this is a modify event, we check the prevSchemeName - if it is different
        ' to the scheme name, we substitute a ModifyWithRename event code in.
        If eventCode = KeyStoreEventCode.Modify AndAlso scheme.Name <> prevSchemeName Then _
         eventCode = KeyStoreEventCode.ModifyWithRename

        Dim sb As New StringBuilder()
        If eventCode <> KeyStoreEventCode.Create Then
            sb.AppendFormat(My.Resources.ResourceManager.GetString("clsServer_ID0"), scheme.ID)
        End If
        sb.AppendFormat(My.Resources.ResourceManager.GetString("clsServer_Available0"), scheme.IsAvailable.ToString())
        sb.AppendFormat(My.Resources.ResourceManager.GetString("clsServer_KeyLocation0"), scheme.KeyLocation.ToString())
        If scheme.KeyLocation = EncryptionKeyLocation.Database Then
            sb.AppendFormat(My.Resources.ResourceManager.GetString("clsServer_Method0"), scheme.AlgorithmName)
        End If

        With cmd.Parameters
            .AddWithValue("@evtcode", GetEventCode(eventCode))
            .AddWithValue("@narr",
              GetNarrative(eventCode, GetLoggedInUserName(), scheme.Name, prevSchemeName))
            .AddWithValue("@userid", GetLoggedInUserId())
            .AddWithValue("@resourceid", Guid.Empty)
            .AddWithValue("@comments", IIf(sb.Length <= 0, sb.ToString(), DBNull.Value))
        End With

        con.ExecuteReturnRecordsAffected(cmd)

    End Sub

#End Region

#Region " Groups "

    ''' <summary>
    ''' Record a group-related audit event.
    ''' </summary>
    ''' <param name="con">The connection to use to write the audit event</param>
    ''' <param name="eventCode">The event code</param>
    ''' <param name="member">The affected group member</param>
    ''' <param name="comments">The event comments</param>
    ''' <param name="oldGroupName">The old username</param>
    Private Sub AuditRecordGroupEvent(
      con As IDatabaseConnection,
      eventCode As GroupTreeEventCode,
      member As IGroupMember,
      comments As String,
      oldGroupName As String)

        If Not gAuditingEnabled Then Return

        Dim type = If(member.IsGroup,
            TreeDefinitionAttribute.GetLocalizedFriendlyName(member.Tree.TreeType.GetTreeDefinition().GroupName),
            member.MemberType.GetLocalizedFriendlyName())

        Dim cmd As New SqlCommand(
         " insert into BPAAuditEvents (" &
         "   eventdatetime, sCode, sNarrative, gSrcUserID, comments) " &
         "   values (getutcdate(), @code, @narr, @srcUser, @comments)")

        With cmd.Parameters
            .AddWithValue("@code", GetEventCode(eventCode))
            .AddWithValue("@narr",
             GetNarrative(eventCode, GetLoggedInUserName(), type, member.Name, oldGroupName))
            .AddWithValue("@srcUser", GetLoggedInUserId())
            .AddWithValue("@comments", IIf(comments IsNot Nothing, comments, DBNull.Value))
        End With
        con.ExecuteReturnRecordsAffected(cmd)

    End Sub

#End Region

#Region "Environment Locks"

    Private Sub AuditRecordEnvironmentLockManualUnlock(
        connection As IDatabaseConnection,
        eventCode As EnvironmentLockEventCode,
        environmentLockName As String,
        comments As String)

        If Not gAuditingEnabled Then Exit Sub

        Dim command As New SqlCommand(
                "INSERT INTO BPAAuditEvents (eventdatetime, sCode, sNarrative, gSrcUserID, comments)" &
                "VALUES (getutcdate(), @eventCode, @narrative, @sourceUser, @comments)")
        With command.Parameters
            .AddWithValue("@eventCode", GetEventCode(eventCode))
            .AddWithValue("@narrative", GetNarrative(eventCode, GetLoggedInUserName(), environmentLockName))
            .AddWithValue("@sourceUser", GetLoggedInUserId())
            .AddWithValue("@comments", IIf(comments = Nothing, DBNull.Value, comments))
        End With
        connection.ExecuteReturnRecordsAffected(command)
    End Sub

#End Region

#Region "Environment Variables"
    Private Sub AuditRecordEnvironmentVariablesChanges(
     con As IDatabaseConnection,
     auditEvent As EnvironmentVariablesAuditEvent)

        If Not gAuditingEnabled Then Return

        Dim cmd As New SqlCommand(
         "select isnull(u.username, '[' + u.systemusername + ']') as username " &
         " from BPAUser u where u.userid=@id")
        cmd.Parameters.AddWithValue("@id", auditEvent.UserId)
        Dim userName = CStr(con.ExecuteReturnScalar(cmd))
        Dim environmentVariableName = auditEvent.EnvironmentVariableName

        Dim narrative = GetNarrative(auditEvent.Code,
         userName, environmentVariableName)

        Dim comment = If(String.IsNullOrEmpty(auditEvent.Comment), "", auditEvent.Comment)

        Dim command As New SqlCommand(
                "INSERT INTO BPAAuditEvents (eventdatetime, sCode, sNarrative, gSrcUserID, comments)" &
                "VALUES (getutcdate(), @eventCode, @narrative, @sourceUser, @comments)")
        With command.Parameters
            .AddWithValue("@eventCode", GetEventCode(auditEvent.Code))
            .AddWithValue("@narrative", narrative)
            .AddWithValue("@sourceUser", auditEvent.UserId)
            .AddWithValue("@comments", comment)
        End With
        Try
            con.ExecuteReturnRecordsAffected(command)
        Catch ex As Exception
            Throw
        End Try
    End Sub
#End Region

#Region "Skill Settings"

    Private Function AuditRecordSkillSettingsEvent(
                            ByVal con As IDatabaseConnection,
                            ByVal eventCode As SkillSettingsEventCode,
                            ByVal comments As String) As Boolean
        If Not gAuditingEnabled Then Return True

        Try
            Using cmd As New SqlCommand("INSERT INTO BPAAuditEvents (eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) VALUES (GETUTCDATE(), @EventCode, @Narrative, @SrcUserID, @TgtResourceID, @Comments)")
                With cmd.Parameters
                    .AddWithValue("@EventCode", GetEventCode(eventCode))
                    .AddWithValue("@Narrative", GetNarrative(eventCode, {GetLoggedInUserName()}))
                    .AddWithValue("@SrcUserID", GetLoggedInUserId)
                    .AddWithValue("@TgtResourceID", Guid.Empty)
                    .AddWithValue("@Comments", If(comments, ""))
                End With
                con.ExecuteReturnRecordsAffected(cmd)
            End Using

            Return True
        Catch
            Return False
        End Try
    End Function

#End Region

#Region "Archive"
    <SecuredMethod(True)>
    Public Sub AuditRecordArchiveEvent(
        eventCode As ArchiveOperationEventCode,
        narrative As String, comments As String) Implements IServer.AuditRecordArchiveEvent
        CheckPermissions()
        Using connection = GetConnection()
            AuditRecordArchiveEvent(connection, eventCode, narrative, comments)
        End Using
    End Sub

    Private Sub AuditRecordArchiveEvent(connection As IDatabaseConnection,
                                        eventCode As ArchiveOperationEventCode,
                                        narrative As String,
                                        comments As String)
        If Not gAuditingEnabled Then Exit Sub
        Using command As New SqlCommand("
                insert into BPAAuditEvents (eventdatetime, sCode, sNarrative, gSrcUserID, comments)
                values (GETUTCDATE(), @EventCode, @Narrative, @SrcUserID, @Comments)")
            With command.Parameters
                .AddWithValue("@EventCode", GetEventCode(eventCode))
                .AddWithValue("@Narrative", GetNarrative(eventCode, GetLoggedInUserName(), narrative))
                .AddWithValue("@SrcUserID", GetLoggedInUserId)
                .AddWithValue("@Comments", If(comments, ""))
            End With
            connection.ExecuteReturnRecordsAffected(command)
        End Using
    End Sub

#End Region

#Region "Data Pipelines"

#Region "Data Pipelines Configs"
    Friend Sub AuditRecordDataPipelineConfigEvent(con As IDatabaseConnection,
        eventCode As DataPipelineEventCode, configuration As DataPipelineProcessConfig)

        If Not gAuditingEnabled Then Exit Sub

        Dim configName = configuration.Name
        If configName = "Default" Then
            configName = My.Resources.AuditDG_configurationFileIsCustomConfigurationDefault
        End If

        Dim narrative = GetNarrative(eventCode, GetLoggedInUserName(), configName)

        Dim configFile = If(configuration.LogstashConfigFile, "")

        Dim comments = String.Format(My.Resources.AuditDG_configurationFileIsCustomConfiguration, configFile, configuration.IsCustom)

        AuditRecordDataPipelineEvent(con, eventCode, narrative, comments)

    End Sub

    Friend Sub AuditRecordDataPipelineSettingsEvent(con As IDatabaseConnection,
        eventCode As DataPipelineEventCode, settings As DataPipelineSettings, oldSettings As DataPipelineSettings)

        If Not gAuditingEnabled Then Return

        Dim narrative = GetNarrative(eventCode, GetLoggedInUserName())
        Dim comments As New StringBuilder()
        If oldSettings.WriteSessionLogsToDatabase <> settings.WriteSessionLogsToDatabase Then _
            comments.Append($"{NameOf(DataPipelineSettings.WriteSessionLogsToDatabase)}:{settings.WriteSessionLogsToDatabase}; ")
        If oldSettings.SendSessionLogsToDataGateways <> settings.SendSessionLogsToDataGateways Then _
            comments.Append($"{NameOf(DataPipelineSettings.SendSessionLogsToDataGateways)}: {settings.SendSessionLogsToDataGateways}; ")
        If oldSettings.MonitoringFrequency <> settings.MonitoringFrequency Then _
            comments.Append($"{NameOf(DataPipelineSettings.MonitoringFrequency)}: {settings.MonitoringFrequency}; ")
        If oldSettings.SendPublishedDashboardsToDataGateways <> settings.SendPublishedDashboardsToDataGateways Then _
            comments.Append($"{NameOf(DataPipelineSettings.SendPublishedDashboardsToDataGateways)}: {settings.SendPublishedDashboardsToDataGateways};")
        If oldSettings.ServerPort <> settings.ServerPort Then _
            comments.Append($"{NameOf(DataPipelineSettings.ServerPort)}: {settings.ServerPort};")
        If oldSettings.UseIntegratedSecurity <> settings.UseIntegratedSecurity Then _
            comments.Append($"{NameOf(DataPipelineSettings.UseIntegratedSecurity)}: {settings.UseIntegratedSecurity};")
        If oldSettings.DatabaseUserCredentialName <> settings.DatabaseUserCredentialName Then _
            comments.Append($"{NameOf(DataPipelineSettings.DatabaseUserCredentialName)}: {settings.DatabaseUserCredentialName};")

        If Not comments.Length.Equals(0) Then _
            AuditRecordDataPipelineEvent(con, eventCode, narrative, comments.ToString())

        Dim oldSettingsDictionary = oldSettings.PublishedDashboardSettings.ToDictionary(Function(x) x.DashboardId, Function(x) x.PublishToDataGatewayInterval)
        For Each dashboardSetting In settings.PublishedDashboardSettings
            Dim dashboardId = dashboardSetting.DashboardId
            If oldSettingsDictionary.ContainsKey(dashboardId) Then
                Dim oldSendInterval As New TimeSpan(0, 0, oldSettingsDictionary.Item(dashboardId))
                Dim currentSendInterval As New TimeSpan(0, 0, dashboardSetting.PublishToDataGatewayInterval)
                Dim modifyDashboardEventCode = DataPipelineEventCode.ModifyDashboardSendInterval
                Dim dashboardUpdateNarrative = GetNarrative(modifyDashboardEventCode, GetLoggedInUserName(), dashboardSetting.DashboardName)
                Dim dashboardUpdateComment = String.Format(My.Resources.ResourceManager.GetString("clsServerAudit_OldSendIntervalNewSendInterval", AuditLocale), oldSendInterval.TotalMinutes, currentSendInterval.TotalMinutes)
                AuditRecordDataPipelineEvent(con, modifyDashboardEventCode, dashboardUpdateNarrative, dashboardUpdateComment)
            End If
        Next

    End Sub

    Private Sub AuditRecordDataPipelineEvent(con As IDatabaseConnection,
        eventCode As DataPipelineEventCode, narrative As String, comments As String)

        Using cmd As New SqlCommand("
                insert into BPAAuditEvents (eventdatetime, sCode, sNarrative, gSrcUserID, comments)
                values (GETUTCDATE(), @EventCode, @Narrative, @SrcUserID, @Comments)")

            With cmd.Parameters
                .AddWithValue("@EventCode", GetEventCode(eventCode))
                .AddWithValue("@Narrative", narrative)
                .AddWithValue("@SrcUserID", GetLoggedInUserId)
                .AddWithValue("@Comments", comments)
            End With
            con.ExecuteReturnRecordsAffected(cmd)
        End Using

    End Sub

#End Region
#Region "Data Pipelines Output Configs"
    Friend Sub AuditRecordDataPipelineOutputConfigEvent(con As IDatabaseConnection,
                                                  eventCode As DataPipelineEventCode, configuration As DataPipelineOutputConfig)

        If Not gAuditingEnabled Then Exit Sub

        Dim narrative = GetNarrative(eventCode, GetLoggedInUserName(), configuration.Name)
        Dim comments = JsonConvert.SerializeObject(configuration)
        AuditRecordDataPipelineEvent(con, eventCode, narrative, comments)

    End Sub

    Friend Sub AuditRecordDataPipelineOutputConfigEvent(con As IDatabaseConnection,
                                                        eventCode As DataPipelineEventCode, configuration As DataPipelineOutputConfig, oldConfiguration As DataPipelineOutputConfig)

        If Not gAuditingEnabled Then Exit Sub

        Dim narrative = GetNarrative(eventCode, GetLoggedInUserName(), configuration.Name)
        Dim comments = String.Format(My.Resources.AuditDG_ChangesOldNewValues, JsonConvert.SerializeObject(configuration), JsonConvert.SerializeObject(oldConfiguration))
        AuditRecordDataPipelineEvent(con, eventCode, narrative, comments)

    End Sub
#End Region

#End Region

End Class
