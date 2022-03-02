Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsBusinessObject
''' 
''' <summary>
''' A class representing a Business Object, with an encapsulated instance of it.
''' </summary>
Public Class clsVBO
    Inherits clsBusinessObject

    ''' <summary>
    ''' The ID this object has been given when registered with
    ''' the database.
    ''' </summary>
    Public ReadOnly Property ProcessID() As Guid
        Get
            Return mgProcessID
        End Get
    End Property
    Private mgProcessID As Guid

    ''' <summary>
    ''' The underlying process.
    ''' </summary>
    Public ReadOnly Property Process() As clsProcess
        Get
            Me.Load()
            Return mProcess
        End Get
    End Property
    Private mProcess As clsProcess

    ''' <summary>
    ''' The logging engines associated with this VBO or, more accurately, with the
    ''' parent process.
    ''' </summary>
    Friend ReadOnly Property Logger() As CompoundLoggingEngine
        Get
            If mProcess IsNot Nothing Then Return mProcess.Logger
            ' If we don't have a process, just return an empty logger; it's unused, but
            ' it is not an error condition so allow the loggers to be swallowed
            Return New CompoundLoggingEngine()
        End Get
    End Property

    ''' <summary>
    ''' True if init has been done.
    ''' </summary>
    Private mbInitialised As Boolean


    ''' <summary>
    ''' True if we're waiting for a child process.
    ''' </summary>
    ''' <remarks></remarks>
    Private mbChildWait As Boolean

    ''' <summary>
    ''' The process this Business Object belongs to.
    ''' </summary>
    Public Property ParentProcess() As clsProcess
        Get
            Return mParentProcess
        End Get
        Set(ByVal value As clsProcess)
            mParentProcess = value
            If mProcess IsNot Nothing Then
                mProcess.ParentProcess = value
            End If
        End Set
    End Property
    Private mParentProcess As clsProcess

    ''' <summary>
    ''' The set of external objects available to this object.
    ''' </summary>
    Private mExternalObjectsInfo As IGroupObjectDetails

    ''' <summary>
    ''' Flag used to indicate whether the object has been loaded.
    ''' </summary>
    Private mbLoaded As Boolean

    ''' <summary>
    ''' Stack of actions called in this object instance. Used to prevent recursion in
    ''' shared objects.
    ''' </summary>
    Public Property ActionStack() As New Stack(Of String)

    ''' <summary>
    ''' Constructor for clsVBO
    ''' </summary>
    Public Sub New(objVBOInfo As clsVBODetails, objExternalObjectsInfo As IGroupObjectDetails, objParentProcess As clsProcess)
        MyBase.New()

        mbInitialised = False
        mgProcessID = objVBOInfo.ID
        mFriendlyName = objVBOInfo.FriendlyName
        'The name is the same thing...
        Me.Name = Me.FriendlyName

        mbChildWait = False
        mParentProcess = objParentProcess
        mExternalObjectsInfo = objExternalObjectsInfo

        'VBO's are currently not configurable
        mConfigurable = False

        'Visual Business Objects always have lifecycle management, because they always
        'have an init and cleanup page.
        mLifecycle = True

        'Set not loaded by default
        mbLoaded = False

        'Assume valid until we really know whether its valid
        'or not when we call Load()
        mValid = True
    End Sub

    ''' <summary>
    ''' Override get action to make sure object is loaded.
    ''' </summary>
    ''' <param name="sName"></param>
    ''' <returns></returns>
    Public Overrides Function GetAction(ByVal sName As String) As clsBusinessObjectAction
        Load()
        Return MyBase.GetAction(sName)
    End Function

    ''' <summary>
    ''' Overrides get actions to make sure object is loaded.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function GetActions() As IList(Of clsBusinessObjectAction)
        Load()
        Return MyBase.GetActions()
    End Function

    ''' <summary>
    ''' Load the business object data, from xml
    ''' </summary>
    Private Sub Load()
        If Not mbLoaded Then

            Dim sErr As String = Nothing
            Try
                'We can no longer assume that the object is valid
                mValid = False

                'Abort if we can't load the process...
                If clsAPC.ProcessLoader Is Nothing Then
                    Me.ErrorMessage = My.Resources.Resources.clsVBO_NoProcessLoaderAvailable
                    Return
                End If

                'Load the process...
                Dim sXml As String = Nothing
                Dim lastmod As DateTime
                If Not clsAPC.ProcessLoader.GetProcessXML(mgProcessID, sXml, lastmod, sErr) Then
                    Me.ErrorMessage = sErr
                    Return
                End If

                mProcess = clsProcess.FromXML(mExternalObjectsInfo, sXml, False, sErr)
                If mProcess Is Nothing Then
                    Me.ErrorMessage = sErr
                    Return
                End If
                mProcess.ModifiedDate = lastmod
                mProcess.ParentProcess = mParentProcess

                'Get the narrative for the whole object...
                Narrative = mProcess.Description

                'Find all published actions and add them...
                Dim l As List(Of clsProcessSubSheet) = mProcess.SubSheets
                For Each s As clsProcessSubSheet In l
                    If s.Published AndAlso s.SheetType <> SubsheetType.CleanUp AndAlso s.SheetType <> SubsheetType.MainPage Then
                        Dim a As New clsVBOAction()
                        a.SetName(s.Name)
                        Dim st As clsProcessStage

                        'Set the narrative...
                        Dim info As Stages.clsSubsheetInfoStage
                        info = DirectCast(mProcess.GetStageByTypeAndSubSheet(StageTypes.SubSheetInfo, s.ID), Stages.clsSubsheetInfoStage)
                        If info IsNot Nothing Then a.SetNarrative(info.GetNarrative())

                        'Add input parameters...
                        st = mProcess.GetStageByTypeAndSubSheet(StageTypes.Start, s.ID)
                        If Not st Is Nothing Then
                            For Each p As clsProcessParameter In st.GetParameters()
                                p.CollectionInfo = GetCollectionDefinition(p, st)
                                a.AddParameter(p)
                            Next
                            For Each pre As String In st.Preconditions
                                a.SetPrecondition(pre)
                            Next
                            For Each post As String In st.PostConditions
                                a.SetEndpoint(post)
                            Next
                        End If

                        'Add output parameters...
                        st = mProcess.GetStageByTypeAndSubSheet(StageTypes.End, s.ID)
                        If Not st Is Nothing Then
                            For Each p As clsProcessParameter In st.GetParameters()
                                Dim cinfo As clsCollectionInfo = GetCollectionDefinition(p, st)
                                If cinfo IsNot Nothing Then cinfo = CType(cinfo.Clone(), clsCollectionInfo)
                                p.CollectionInfo = cinfo
                                a.AddParameter(p)
                            Next
                        End If


                        AddAction(a)
                    End If
                Next

                mRunMode = mProcess.ObjectRunMode

                If mParentProcess IsNot Nothing AndAlso mProcess.ParentObject IsNot Nothing Then

                    Dim obj As clsVBO = TryCast(mProcess.GetBusinessObjectRef(mProcess.ParentObject), clsVBO)
                    If obj Is Nothing Then
                        Me.ErrorMessage = My.Resources.Resources.clsVBO_CannotFindParentObject & mProcess.ParentObject
                        Return
                    End If
                    obj.Load()
                    If Not obj.Process.IsShared Then
                        Me.ErrorMessage = String.Format(My.Resources.Resources.clsVBO_ParentObject0MustBeDefinedAsShared,
                                                        mProcess.ParentObject)
                        Return
                    End If
                    mProcess.ParentObjRef = obj

                Else

                    Dim err As clsAMIMessage = Nothing
                    If Not mProcess.AMI.SetTargetApplication(mProcess.ApplicationDefinition.ApplicationInfo, err) Then
                        Me.ErrorMessage = String.Format(My.Resources.Resources.clsVBO_FailedToSetTargetApplicationInfo0, err.Message)
                        Return
                    End If

                End If

                mValid = True

            Catch e As Exception
                Me.ErrorMessage = e.Message
                mValid = False
            Finally
                'We will say that the object is loaded
                'even if it is invalid because if at some
                'point it becomes valid the user will have
                'to press reset anyway, at which point all
                'VBO's are thrown away. Do this in the finally
                'block to make sure its set even after the 
                'Exit Sub 's above
                mbLoaded = True
            End Try
        End If
    End Sub

    Friend Shared Function GetCollectionDefinition(ByVal p As clsProcessParameter, ByVal stg As clsProcessStage) As clsCollectionInfo
        Dim proc As clsProcess = stg.Process
        If proc Is Nothing Then Return Nothing

        Dim coll As clsCollectionStage =
         TryCast(proc.GetDataStage(p.GetMap(), stg, False), clsCollectionStage)
        If coll IsNot Nothing Then Return coll.Definition

        Return Nothing
    End Function

    Public Overrides Property RunMode() As BusinessObjectRunMode
        Get
            Me.Load()
            Return MyBase.RunMode
        End Get
        Set(ByVal value As BusinessObjectRunMode)
            MyBase.RunMode = value
        End Set
    End Property

    ''' <summary>
    ''' Handles anything that must be done to dispose the object.
    ''' </summary>
    Public Overrides Sub DisposeTasks()

        If Not mProcess Is Nothing Then
            Dim bUseDebugHook As Boolean
            bUseDebugHook = clsAPC.ProcessDebugHook IsNot Nothing AndAlso clsAPC.ProcessDebugHook.UseDebugHook AndAlso Not mParentProcess Is Nothing
            If bUseDebugHook Then clsAPC.ProcessDebugHook.ProcessDebugReturn(mProcess, mParentProcess)
            mProcess.Dispose()
            mProcess = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Run the active page
    ''' </summary>
    ''' <param name="gPageID">The ID of the page to run</param>
    ''' <param name="inputs">The input parameters</param>
    ''' <returns>A clsProcessStage.Result</returns>
    Private Function RunPage(ByVal gPageID As Guid, ByVal inputs As clsArgumentList) As StageResult
        If mValid Then
            Dim res As StageResult
            mProcess.RunPageId = gPageID
            mProcess.SetInputParams(inputs)
            res = mProcess.RunAction(ProcessRunAction.Go)
            If Not res.Success Then Return res
            Do While mProcess.RunState = ProcessRunState.Running
                res = mProcess.RunAction(ProcessRunAction.RunNextStep)
                If Not res.Success Then Return res
            Loop
            If mProcess.RunState <> ProcessRunState.Completed Then
                Return New StageResult(False, "Internal", My.Resources.Resources.clsVBO_ActionDidNotCompleteRunning)
            End If
            Return New StageResult(True)
        Else
            Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsVBO_CanNotRunRequestedActionBecauseTheObjectIsNotValid0, Me.ErrorMessage))
        End If
    End Function

    ''' <summary>
    ''' Initialise the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Overrides Function DoInit() As StageResult
        Load()
        If Not mValid Then
            Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsVBO_InitialisationFailedVisualBusinessObjectNotValid0, Me.ErrorMessage))
        End If

        'Initialise the parent object if necessary
        Dim parentObjectRef = mProcess.ParentObject
        If parentObjectRef IsNot Nothing Then
            Dim parentObject = mProcess.GetBusinessObjectRef(parentObjectRef)
            If parentObject IsNot Nothing AndAlso Not parentObject.mInited Then
                Dim result = parentObject.Init
                If Not result.Success Then Return result
            End If
        End If

        mbInitialised = True
        Return RunPage(mProcess.GetMainPage().ID, New clsArgumentList)
    End Function

    ''' <summary>
    ''' Clean up the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Overrides Function DoCleanUp() As StageResult
        Load()
        If Not mValid Then
            Return New StageResult(False, String.Format("Internal", My.Resources.Resources.clsVBO_CleanupFailedVisualBusinessObjectNotValid0, Me.ErrorMessage))
        End If

        If Not mbInitialised Then Return New StageResult(True)
        If mProcess.IsDisposed Then
            Return New StageResult(False, "Internal", My.Resources.Resources.clsVBO_VisualBusinessObjectIsAlreadyDisposed)
        End If

        'Set the runstage, runmode, etc. This isn't necessary for the init
        'page since when the runmode is 'off' a load of other initialisation (such as this)
        'takes place anyway. Pages other than init and cleanup have their own running mechanism
        'making this a special case requiring its own setup here.
        mProcess.RunState = ProcessRunState.Paused
        Dim CleanupStartStage As clsProcessStage = mProcess.GetStageByTypeAndSubSheet(StageTypes.Start, mProcess.GetCleanupPage.ID)
        If CleanupStartStage Is Nothing Then
            Return New StageResult(False, "Internal", My.Resources.Resources.clsVBO_CanNotFindStartStageForCleanupPage)
        End If

        Dim sErr As String = Nothing
        If Not mProcess.SetRunStage(CleanupStartStage.GetStageID, sErr) Then
            Return New StageResult(False, "Internal", sErr)
        End If
        Return RunPage(mProcess.GetCleanupPage.ID, New clsArgumentList)
    End Function


    ''' <summary>
    ''' Ask the Business Object to perform an action.
    ''' </summary>
    ''' <param name="sActionName">The name of the action to perform</param>
    ''' <param name="scopeStage">The stage used to resolve scope within the business
    ''' object action. Not relevant for visual business objects. May be null.</param>
    ''' <param name="inputs">The inputs</param>
    ''' <param name="outputs">On return, contains the outputs</param>
    ''' <returns>A clsProcessStage.Result</returns>
    Protected Overrides Function DoDoAction(ByVal sActionName As String, ByVal scopeStage As clsProcessStage, ByVal inputs As clsArgumentList, ByRef outputs As clsArgumentList) As StageResult
        Dim res As StageResult
        Load()
        If Not mValid Then
            Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsVBO_CanNotRunPage0BecauseTheVisualBusinessObjectIsNotValid1, Me.ErrorMessage))
        End If

        Dim gPageID As Guid
        gPageID = mProcess.GetSubSheetID(sActionName)
        If gPageID.Equals(Guid.Empty) Then
            Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsVBO_Action0NotFound, sActionName))
        End If

        'Ensure action is actually published
        Dim SheetToRun As clsProcessSubSheet = mProcess.GetSubSheetByID(gPageID)
        If Not SheetToRun Is Nothing Then
            If Not SheetToRun.Published Then
                Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsVBO_TheRequestedAction0CanNotBeRunBecauseItHasNotBeenPublished, sActionName))
            End If
        Else
            Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsVBO_Action0NotFound, sActionName))
        End If

        'Decide whether or not to use the debug hook, which is if it's available
        'AND if we want to use it...
        Dim bUseDebugHook As Boolean = (
         clsAPC.ProcessDebugHook IsNot Nothing AndAlso
         clsAPC.ProcessDebugHook.UseDebugHook AndAlso
         scopeStage?.Process IsNot Nothing AndAlso
         scopeStage.Process.RunState = ProcessRunState.Stepping
        )


        If Not mbChildWait Then
            mProcess.RunPageId = gPageID
            'Set as active page as well, so we see it straight away if we go to the
            'debugger
            mProcess.SetActiveSubSheet(gPageID)
            mProcess.SetInputParams(inputs)
            res = mProcess.RunAction(ProcessRunAction.GotoPage)
            If Not res.Success Then
                Return res
            End If

            'If possible, hand the process over to the debug hook which will take
            'responsibility for running it (it won't block while it does so though)
            If bUseDebugHook Then
                Dim sErr As String = Nothing
                If Not clsAPC.ProcessDebugHook.ProcessDebugHook(mProcess, mgProcessID, mParentProcess, mProcess.ModifiedDate, sErr) Then
                    Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsVBO_DebugHookFailed0, sErr))
                End If
            End If

            mbChildWait = True
        End If

        If mbChildWait Then
            'If we didn't hand the process off elsewhere (e.g. the Automate debugger)
            'then we need to make it run...

            If Not bUseDebugHook Then
                'We may be using an object that has already failed or completed, if we
                'are debugging and stepping over, in which case we need to reset it first.
                '(See bug #2942)
                If mProcess.RunState = ProcessRunState.Failed OrElse mProcess.RunState = ProcessRunState.Completed OrElse mProcess.RunState = ProcessRunState.Aborted Then
                    res = mProcess.RunAction(ProcessRunAction.Reset)
                    If Not res.Success Then Return res
                End If

                'Run (or continue running) the external process...
                '(we are only going to run one step's worth every time it comes
                'in here, to keep the client responsive to requests to stop etc)
                If mProcess.RunState = ProcessRunState.Off Or mProcess.RunState = ProcessRunState.Paused Then
                    res = mProcess.RunAction(ProcessRunAction.Go)
                    If Not res.Success Then
                        Return res
                    End If
                End If
                If mProcess.RunState <> ProcessRunState.Running Then
                    Return New StageResult(False, "Internal", My.Resources.Resources.clsVBO_VisualBusinessObjectDidNotStart)
                End If
                res = mProcess.RunAction(ProcessRunAction.RunNextStep)
                If Not res.Success Then
                    mbChildWait = False
                    Return res
                End If
            End If

            Select Case mProcess.RunState
                Case ProcessRunState.Aborted
                    mbChildWait = False
                    If bUseDebugHook Then clsAPC.ProcessDebugHook.ProcessDebugReturn(mProcess, mParentProcess)
                    Return New StageResult(False, "Internal", My.Resources.Resources.clsVBO_VisualBusinessObjectAborted)

                Case ProcessRunState.Failed
                    mbChildWait = False
                    If bUseDebugHook Then clsAPC.ProcessDebugHook.ProcessDebugReturn(mProcess, mParentProcess)
                    Return New StageResult(False, "Internal", My.Resources.Resources.clsVBO_VisualBusinessObjectFailed)

                Case ProcessRunState.Completed

                    'Deal with the outputs...
                    outputs = mProcess.GetOutputs()
                    mbChildWait = False
                    If bUseDebugHook Then clsAPC.ProcessDebugHook.ProcessDebugReturn(mProcess, mParentProcess)

                Case Else
                    outputs = New clsArgumentList
                    outputs.IsRunning = True
            End Select

        End If

        Return New StageResult(True)
    End Function

    ''' <summary>
    ''' Set configuration on the Business Object - a wrapper for the
    ''' Business Object function itself
    ''' </summary>
    ''' <param name="sConfigXML">The config</param>
    ''' <param name="sErr">On failure, an error description</param>
    ''' <returns>True if successful, False otherwise</returns>
    Public Function SetConfig(ByVal sConfigXML As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.Resources.clsVBO_NotConfigurable
        Return False
    End Function

    ''' <summary>
    ''' Get configuration on the Business Object - a wrapper for the
    ''' Business Object function itself
    ''' </summary>
    ''' <param name="sErr">On failure, an error description, otherwise an
    ''' empty string</param>
    ''' <returns>The ConfigXML of the Businessobject</returns>
    Public Overrides Function GetConfig(ByRef sErr As String) As String
        sErr = My.Resources.Resources.clsVBO_NotConfigurable
        Return ""
    End Function

    ''' <summary>
    ''' Show Config UI on the Business Object - a wrapper for the
    ''' Business Object function itself
    ''' </summary>
    ''' <param name="sErr">On failure, an error description</param>
    ''' <returns>True if successful, False otherwise</returns>
    Public Overrides Function ShowConfigUI(ByRef sErr As String) As Boolean
        sErr = My.Resources.Resources.clsVBO_NotConfigurable
        Return False
    End Function


    Protected Overrides Sub GetHTMLPreamble(ByVal xr As System.Xml.XmlTextWriter)
        xr.WriteElementString("h1", My.Resources.Resources.clsVBO_BusinessObjectDefinition)
        xr.WriteElementString("div", My.Resources.Resources.clsVBO_TheInformationContainedInThisDocumentIsTheProprietaryInformationOfBluePrismLimi)
        xr.WriteElementString("div", My.Resources.Resources.clsVBO_CBluePrismLimited)
        xr.WriteElementString("h2", My.Resources.Resources.clsVBO_AboutThisDocument)
        xr.WriteElementString("div", My.Resources.Resources.clsVBO_TheBusinessObjectDefinitionDescribesTheAPIsAvailableWithinASingleBusinessObject)
        xr.WriteElementString("h2", My.Resources.Resources.clsVBO_AboutBusinessObjects)
        xr.WriteElementString("div", My.Resources.Resources.clsVBO_BusinessObjectsWithinTheEnvironmentIEObjectsWhichMayBeDrawnOntoAProcessToCaptur)
    End Sub


End Class

