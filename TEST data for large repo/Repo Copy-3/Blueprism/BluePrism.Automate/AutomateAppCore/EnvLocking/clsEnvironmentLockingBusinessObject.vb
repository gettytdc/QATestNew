Imports System.Threading
Imports BluePrism.Server.Domain.Models
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.My.Resources
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.Data.DataModels.WorkQueueAnalysis

Public Class clsEnvironmentLockingBusinessObject : Inherits clsInternalBusinessObject

    ''' <summary>
    ''' The new constructor just creates the Internal Business Object Actions
    ''' </summary>
    ''' <param name="process">A reference to the process calling the object</param>
    ''' <param name="session">The session the object is running under</param>
    Public Sub New(ByVal process As clsProcess, ByVal session As clsSession)
        MyBase.New(process, session,
          "BluePrism.AutomateAppCore.clsEnvironmentLockingBusinessObject",
          IboResources.clsEnvironmentLockingBusinessObject_EnvironmentLocking)

        Narrative = IboResources.clsEnvironmentLockingBusinessObject_ProvidesABasicEnvironmentWideLockHandlingFacilityToAllowHandlingOfCriticalSecti

        AddAction(New AcquireLockAction(Me))
        AddAction(New ReleaseLockAction(Me))
        AddAction(New QueryLockAction(Me))

    End Sub

    Public Overrides Function DoCleanUp(
     ByVal sessionIdentifier As SessionIdentifier) As StageResult

        ' We want to release any locks which have been acquired by this session.
        Try
            gSv.ReleaseEnvLocksForSession(sessionIdentifier)
            Return StageResult.OK

        Catch ex As Exception
            Return New StageResult(
             False, IboResources.clsEnvironmentLockingBusinessObject_ReleaseLocksException, ex.Message)

        End Try

    End Function

    Public Overrides Function CheckLicense() As Boolean
        Return True
    End Function

    ''' <summary>
    ''' Base class for actions on the environment locking business object.
    ''' </summary>
    Private MustInherit Class EnvLockingAction : Inherits clsInternalBusinessObjectAction


        ''' <summary>
        ''' Class to hold the parameter names as constants.
        ''' </summary>
        Protected Class Params

            Public Shared Name As String = NameOf(IboResources.clsEnvironmentLockingBusinessObject_Params_Name)
            Public Shared Timeout As String = NameOf(IboResources.clsEnvironmentLockingBusinessObject_Params_Timeout)
            Public Shared PreferredToken As String = NameOf(IboResources.clsEnvironmentLockingBusinessObject_Params_PreferredToken)
            Public Shared KeepLock As String = NameOf(IboResources.clsEnvironmentLockingBusinessObject_Params_KeepLock)
            Public Shared Comment As String = NameOf(IboResources.clsEnvironmentLockingBusinessObject_Params_Comment)
            Public Shared Token As String = NameOf(IboResources.clsEnvironmentLockingBusinessObject_Params_Token)
            Public Shared AllSessions As String = NameOf(IboResources.clsEnvironmentLockingBusinessObject_Params_AllSessions)
            Public Shared LockHeld As String = NameOf(IboResources.clsEnvironmentLockingBusinessObject_Params_LockHeld)

        End Class
        Public Function _T(ByVal param As String) As String
            Return IboResources.ResourceManager.GetString(param, New Globalization.CultureInfo("en"))
        End Function

        ''' <summary>
        ''' The maximum number of characters allowed in a lock name.
        ''' </summary>
        Protected Const MaxLengthName As Integer = 255

        ''' <summary>
        ''' The maximum number of characters allowed in a lock token.
        ''' </summary>
        Protected Const MaxLengthToken As Integer = 255

        ''' <summary>
        ''' The maximum number of characters allowed in a lock comment.
        ''' </summary>
        Protected Const MaxLengthComment As Integer = 1024

        ''' <summary>
        ''' Creates a new env locking action based on the given business object.
        ''' </summary>
        ''' <param name="obj">The business object which acts as the owner of this
        ''' action.</param>
        Public Sub New(ByVal obj As clsEnvironmentLockingBusinessObject)
            MyBase.New(obj)
        End Sub

        ''' <summary>
        ''' Performs the associated action.
        ''' </summary>
        ''' <param name="process"></param>
        ''' <param name="session"></param>
        ''' <param name="scopeStage"></param>
        ''' <param name="sErr"></param>
        ''' <returns></returns>
        Public MustOverride Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage,
         ByRef sErr As String) As Boolean

        ''' <summary>
        ''' Gets the endpoint of this action.
        ''' </summary>
        ''' <returns>A description of the endpoint after this action has been performed.
        ''' </returns>
        Public MustOverride Overrides Function GetEndpoint() As String

        ''' <summary>
        ''' Gets the preconditions for this action.
        ''' </summary>
        ''' <returns>A collection of preconditions which must be met before this action
        ''' can safely be called.</returns>
        Public Overrides Function GetPreConditions() As Collection
            Return EmptyPreCondition()
        End Function

        ''' <summary>
        ''' Validates the parameters, ensuring that they do not exceed the maximum
        ''' respective lengths.
        ''' </summary>
        ''' <param name="name">The name of the lock</param>
        ''' <param name="token">The token to use for the lock</param>
        ''' <param name="comment">The comment to ascribe to the lock</param>
        ''' <remarks>Any null values are skipped - ie. they will not throw any errors
        ''' when encountered</remarks>
        ''' <exception cref="InvalidFormatException">If any of the arguments are
        ''' larger than their configured maximum</exception>
        ''' <seealso cref="MaxLengthName"/>
        ''' <seealso cref="MaxLengthToken"/>
        ''' <seealso cref="MaxLengthComment"/>
        Protected Sub ValidateParams(
         name As String, token As String,
         comment As String, stageName As String)
            If name IsNot Nothing AndAlso name.Length > MaxLengthName Then _
             Throw New InvalidFormatException(
              IboResources.clsEnvironmentLockingBusinessObject_EnvLockingAction_NameIsTooLargeMaximumOf0CharactersIsAllowedGivenNameWas1Characters,
              MaxLengthName, name.Length)

            If String.Equals(name, IboResources.clsEnvironmentLockingBusinessObject_EnvLockingAction_SendPublishedDashboards, StringComparison.CurrentCultureIgnoreCase) Then _
                Throw New InvalidFormatException(
              IboResources.clsEnvironmentLockingBusinessObject_EnvLockingAction_ThisNameIsReservedForLocksThatProtectSendingPublishedDashboardDataToDataGateways,
              name, stageName)

            If String.Equals(name, WorkQueueAnalysisConstants.SnapshotLockName, StringComparison.CurrentCultureIgnoreCase) Then
                Throw New InvalidFormatException(IboResources.clsEnvironmentLockingBusinessObject_EnvLockingAction_ThisNameIsReservedForWorkQueueSnapshots, name, stageName)
            End If

            If String.Equals(name, clsServer.RefreshMIDataEnvLockName, StringComparison.CurrentCultureIgnoreCase) Then
                Throw New InvalidFormatException(IboResources.clsEnvironmentLockingBusinessObject_EnvLockingAction_ThisNameIsReservedForMIReporting, name, stageName)
            End If

            If token IsNot Nothing AndAlso token.Length > MaxLengthToken Then _
             Throw New InvalidFormatException(
              IboResources.clsEnvironmentLockingBusinessObject_EnvLockingAction_PreferredTokenIsTooLargeMaximumOf0CharactersIsAllowedGivenTokenWas1Characters,
              MaxLengthToken, token.Length)

            If comment IsNot Nothing AndAlso comment.Length > MaxLengthComment Then _
             Throw New InvalidFormatException(
              IboResources.clsEnvironmentLockingBusinessObject_EnvLockingAction_CommentIsTooLargeMaximumOf0CharactersIsAllowedGivenCommentWas1Characters,
              MaxLengthComment, comment.Length)

        End Sub

    End Class

    ''' <summary>
    ''' Action to acquire a named lock
    ''' </summary>
    Private Class AcquireLockAction : Inherits EnvLockingAction

        ''' <summary>
        ''' Creates a new action on the given business object.
        ''' </summary>
        ''' <param name="bo">The object acting as the owner of this action.</param>
        Public Sub New(ByVal bo As clsEnvironmentLockingBusinessObject)
            MyBase.New(bo)

            SetName(NameOf(IboResources.clsEnvironmentLockingBusinessObject_Action_AcquireLock))
            SetNarrative(IboResources.clsEnvironmentLockingBusinessObject_AcquireLockAction_AcquiresTheLockAssociatedWithTheGivenNameIfThisSessionAlreadyHoldsTheRequiredLo)

            AddParameter(Params.Name, DataType.text, ParamDirection.In,
             IboResources.clsEnvironmentLockingBusinessObject_AcquireLockAction_TheNameOfTheRequiredLockMandatoryParameter)
            AddParameter(Params.Timeout, DataType.timespan, ParamDirection.In,
             IboResources.clsEnvironmentLockingBusinessObject_AcquireLockAction_TheMaximumAmountOfTimeToAwaitTheLockDefaultIsToWaitForever)
            AddParameter(Params.PreferredToken, DataType.text, ParamDirection.In,
             IboResources.clsEnvironmentLockingBusinessObject_AcquireLockAction_ThePreferredTokenToUseForTheLockDefaultBehaviourIsToGenerateANewUniqueToken)
            AddParameter(Params.Comment, DataType.text, ParamDirection.In,
             IboResources.clsEnvironmentLockingBusinessObject_AcquireLockAction_TheCommentsToSetOnTheLock)

            AddParameter(Params.Token, DataType.text, ParamDirection.Out,
             IboResources.clsEnvironmentLockingBusinessObject_AcquireLockAction_TheTokenRegisteredAgainstTheLockThisWillBeEmptyIfTheLockCouldNotBeAcquired)

        End Sub

        ''' <summary>
        ''' Performs the 'acquire lock' action.
        ''' </summary>
        ''' <param name="process"></param>
        ''' <param name="session"></param>
        ''' <param name="scopeStage"></param>
        ''' <param name="sErr"></param>
        ''' <returns></returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Dim name As String = Nothing
            Dim timeout As Integer = -1
            Dim token As String = ""
            Dim comment As String = Nothing
            For Each arg As clsArgument In Inputs
                If arg.Value.IsNull Then Continue For
                Select Case arg.Name
                    Case "Name" : name = CStr(arg.Value).Trim()
                    Case "Timeout" : timeout = CInt(CType(arg.Value, TimeSpan).TotalSeconds)
                    Case "Preferred Token" : token = CStr(arg.Value).Trim()
                    Case "Comment" : comment = CStr(arg.Value).Trim()
                End Select
            Next
            If name = "" Then Return SendError(sErr,
             IboResources.clsEnvironmentLockingBusinessObject_AcquireLockAction_ANameMustBeProvidedToAcquireALock)

            ' If there's no preference for the token, let the server generate one.
            Try
                ValidateParams(name, token, comment, scopeStage.Name)

                ' Set an (approx) end time...
                ' If it's before the while loop is entered, (eg. timeout = 0), then the
                ' loop is performed at least once to check for the lock.
                Dim endTime As Date = Date.MaxValue

                ' If not waiting forever, set the end time.
                If timeout >= 0 Then endTime =
                 Date.Now + New TimeSpan(timeout * TimeSpan.TicksPerSecond)

                ' So we don't overwrite the preferred token on failing lock acquisition.
                Dim acquiredToken As String

                Do
                    ' Try and acquire the lock
                    acquiredToken = gSv.AcquireEnvLock(name, token, session.Identifier, comment, 0)

                    ' If we succeeded, return success along with the token
                    If acquiredToken <> "" Then AddOutput(_T(Params.Token), acquiredToken) : Return True

                    ' If we failed, wait a beat and try again
                    Thread.Sleep(250)

                Loop While Date.Now < endTime

                AddOutput(_T(Params.Token), "")
                Return True

            Catch ex As Exception
                sErr = ex.Message
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Gets the endpoint after the action has completed.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsEnvironmentLockingBusinessObject_AcquireLockAction_TheTokenProvidedGuaranteesTheLockIfItIsEmptyThenTheLockWasNotAcquired
        End Function
    End Class

    ''' <summary>
    ''' Action to release a lock or a number of locks based on the name and
    ''' associated tokens.
    ''' </summary>
    Private Class ReleaseLockAction : Inherits EnvLockingAction

        ''' <summary>
        ''' Creates a new action for releasing locks.
        ''' </summary>
        ''' <param name="bo">The business object on which this action is registered.
        ''' </param>
        Public Sub New(ByVal bo As clsEnvironmentLockingBusinessObject)
            MyBase.New(bo)

            SetName(NameOf(IboResources.clsEnvironmentLockingBusinessObject_Action_ReleaseLock))
            SetNarrative(IboResources.clsEnvironmentLockingBusinessObject_ReleaseLockAction_ReleasesTheLockSAssociatedWithTheGivenNameAndToken)

            AddParameter(Params.Name, DataType.text, ParamDirection.In,
             IboResources.clsEnvironmentLockingBusinessObject_ReleaseLockAction_TheNameOfTheSpecificLockToBeReleasedDefaultBehaviourIsToReleaseAllLocksWithTheG)
            AddParameter(Params.Token, DataType.text, ParamDirection.In,
             IboResources.clsEnvironmentLockingBusinessObject_ReleaseLockAction_TheTokenAssociatedWithTheLockSToBeReleasedMandatoryParameter)
            AddParameter(Params.Comment, DataType.text, ParamDirection.In,
             IboResources.clsEnvironmentLockingBusinessObject_ReleaseLockAction_TheCommentsToSetOnTheLockS)
            AddParameter(Params.AllSessions, DataType.flag, ParamDirection.In,
             IboResources.clsEnvironmentLockingBusinessObject_ReleaseLockAction_TrueToReleaseLocksMatchingTheSpecifiedConstraintsAcrossAllSessionsFalseToOnlyRe)
            AddParameter(Params.KeepLock, DataType.flag, ParamDirection.In,
             IboResources.clsEnvironmentLockingBusinessObject_ReleaseLockAction_KeepEnviromentLockOnRelease)

        End Sub


        ''' <summary>
        ''' Performs the releasing of the lock(s)
        ''' </summary>
        ''' <param name="process"></param>
        ''' <param name="session"></param>
        ''' <param name="scopeStage"></param>
        ''' <param name="sErr"></param>
        ''' <returns></returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Try
                Dim name As String = Nothing
                Dim token As String = Nothing
                Dim comment As String = Nothing
                Dim allSessions As Boolean = False
                Dim keepLock As Boolean = False
                For Each arg As clsArgument In Inputs
                    If arg.Value.IsNull Then Continue For
                    Select Case arg.Name
                        Case "Name" : name = CStr(arg.Value).Trim()
                        Case "Token" : token = CStr(arg.Value).Trim()
                        Case "Comment" : comment = CStr(arg.Value).Trim()
                        Case "All Sessions" : allSessions = CBool(arg.Value)
                        Case "Keep Environment Lock" : keepLock = CBool(arg.Value)
                    End Select
                Next

                If token = "" Then Return SendError(sErr,
                 IboResources.clsEnvironmentLockingBusinessObject_ReleaseLockAction_ATokenMustBeProvidedToReleaseALock)

                ValidateParams(name, token, comment, scopeStage.Name)

                Dim sessionToRelease As SessionIdentifier = If(allSessions, Nothing, session.Identifier)

                If name = "" Then ' If no name was given, release all locks with the given token.
                    gSv.ReleaseAllEnvLocks(token, comment, sessionToRelease, keepLock)
                Else ' Otherwise, release the specifically named one.
                    gSv.ReleaseEnvLock(name, token, comment, sessionToRelease, keepLock)
                End If
                Return True

            Catch ex As Exception
                sErr = ex.Message
                Return False

            End Try

        End Function

        ''' <summary>
        ''' Gets the endpoint after the release lock action has been performed.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsEnvironmentLockingBusinessObject_ReleaseLockAction_TheSpecifiedLockSWillHaveBeenReleased
        End Function
    End Class


    ''' <summary>
    ''' Action to check the status and current comment on a named lock.
    ''' </summary>
    Private Class QueryLockAction : Inherits EnvLockingAction

        ''' <summary>
        ''' Creates a new action for querying a lock.
        ''' </summary>
        ''' <param name="bo">The business object on which this action is registered.
        ''' </param>
        Public Sub New(ByVal bo As clsEnvironmentLockingBusinessObject)
            MyBase.New(bo)

            SetName(NameOf(IboResources.clsEnvironmentLockingBusinessObject_Action_QueryLock))
            SetNarrative(IboResources.clsEnvironmentLockingBusinessObject_QueryLockAction_QueriesTheCurrentStateOfTheLock)

            AddParameter(Params.Name, DataType.text, ParamDirection.In,
             IboResources.clsEnvironmentLockingBusinessObject_QueryLockAction_TheNameOfTheRequiredLockMandatoryParameter)
            AddParameter(Params.Token, DataType.text, ParamDirection.In,
             IboResources.clsEnvironmentLockingBusinessObject_QueryLockAction_TheTokenToCheckAgainstTheLockIfProvidedChecksIfTheLockIsHeldByThisTokenOtherwis)

            AddParameter(Params.LockHeld, DataType.flag, ParamDirection.Out,
             IboResources.clsEnvironmentLockingBusinessObject_QueryLockAction_FlagIndicatingIfTheLockIsHeldByTheSpecifiedTokenIfOneWasGivenOrAtAllOtherwise)
            AddParameter(Params.Comment, DataType.text, ParamDirection.Out,
             IboResources.clsEnvironmentLockingBusinessObject_QueryLockAction_TheCommentCurrentlySetOnTheLockRegardlessOfWhetherTheLockIsCurrentlyHeld)

        End Sub

        ''' <summary>
        ''' Performs the querying of the lock(s)
        ''' </summary>
        ''' <param name="process"></param>
        ''' <param name="session"></param>
        ''' <param name="scopeStage"></param>
        ''' <param name="sErr"></param>
        ''' <returns></returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Try

                Dim name As String = Inputs.GetString(_T(Params.Name))
                Dim token As String = Inputs.GetString(_T(Params.Token))

                If String.IsNullOrEmpty(name) Then Return SendError(sErr,
                 IboResources.clsEnvironmentLockingBusinessObject_QueryLockAction_ANameMustBeProvidedToQueryALock)

                ValidateParams(name, token, Nothing, scopeStage.Name)

                Dim comment As String = Nothing
                Dim isLocked As Boolean = gSv.IsEnvLockHeld(name, token, comment)

                AddOutput(_T(Params.LockHeld), isLocked)
                AddOutput(_T(Params.Comment), comment)

                Return True

            Catch ex As Exception
                sErr = ex.Message
                Return False

            End Try

        End Function

        ''' <summary>
        ''' Gets the endpoint after the query lock action has been performed.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsEnvironmentLockingBusinessObject_QueryLockAction_NoChangesAreMadeByThisAction
        End Function
    End Class


End Class
