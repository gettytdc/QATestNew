
Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Reflection
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.BPUtil
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

'Note - This is imported only because there are some clipboard
'       operations in here. This project really should not have
'       any dependencies on this - we aim to get rid of it by moving
'       the actual clipboard access to the application side.
Imports System.Windows.Forms

'Note - This is only imported for some view-related calculations
'       which should really be renderer-specific and not in here.
'       We aim to get rid of it by moving view-related calculations
'       to the renderer.
Imports System.Drawing
Imports System.Linq
Imports BluePrism.Common.Security
Imports BluePrism.Core.Xml

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcess
''' 
''' <summary>
''' The main class representing a process, along with all the editing operations that
''' can be performed on it, and the ability to run it.
''' 
''' The Dispose() method must ALWAYS be used when the object is no longer required.
''' </summary>
Public Class clsProcess : Inherits clsDataMonitor : Implements IDisposable

#Region " Class-scope definitions "

    Public ReadOnly Property Fonts As FontCache = New FontCache()

    ''' <summary>
    ''' In version 5 and above we introduce a scale factor to the default zoom so that
    ''' the diagrams are easier to see.
    ''' </summary>
    Public Const ScaleFactor As Single = 1.25!


    ''' <summary>
    ''' The minimum percentage allowed for a zoom value - currently 1%
    ''' </summary>
    Const MinimumZoomPercent As Integer = 1

    ''' <summary>
    ''' The maximum percentage allowed for a zoom value - currently there is no max
    ''' </summary>
    Const MaximumZoomPercent As Integer = Integer.MaxValue

    ''' <summary>
    ''' Event raised when an error occurs that cannot be notified by normal means.
    ''' This is generally only relevant when debugging. Process/Object Studio should
    ''' handle this and display the error. At runtime, it should not be necessary to
    ''' handle it at all.
    ''' </summary>
    ''' <param name="res">A non-successful clsProcessStage.Result</param>
    Public Shared Event AsyncError(ByVal res As StageResult)

    ''' <summary>
    ''' Event raised by an Alert stage.
    ''' </summary>
    ''' <param name="stg">The alert stage</param>
    ''' <param name="msg">The alert text</param>
    Public Shared Event StageAlert(ByVal stg As clsAlertStage, ByVal msg As String)

    ''' <summary>
    ''' The width, in world coordinates of a grid square.
    ''' </summary>
    Public Const GridUnitSize As Integer = 15

    ''' <summary>
    ''' The default color for a block
    ''' </summary>
    Public Shared ReadOnly DefaultBlockColour As Color =
     Color.FromArgb(255, 127, 178, 229)

    ''' <summary>
    ''' The name given to the init page when process type is business object.
    ''' </summary>
    Public Const InitPageName As String = "Initialise"

    ''' <summary>
    ''' The name given to the main page when the process type is process.
    ''' </summary>
    Public Const MainPageName As String = "Main Page"

    ''' <summary>
    ''' Delegate definition for stage filter to be used in combination
    ''' with GetStages()
    ''' </summary>
    Public Delegate Function IncludeStage(ByVal Stage As clsProcessStage) As Boolean

    ''' <summary>
    ''' Garbles a password, for inclusion as an initial value in the
    ''' process XML.
    ''' </summary>
    ''' <param name="pwd">The real password</param>
    ''' <returns>The garbled password</returns>
    Public Shared Function GarblePassword(ByVal pwd As SafeString) As String
        If pwd Is Nothing OrElse pwd.Length = 0 Then Return ""

        Using pinned = pwd.Pin()

            Dim chars As Char() = pinned.Chars
            Chipher(chars)
            Dim sb As New StringBuilder()
            For i As Integer = 0 To chars.Length - 1
                If sb.Length > 0 Then sb.Append(","c)
                sb.Append(Asc(chars(i)))
            Next
            Return sb.ToString()

        End Using

    End Function

    ''' <summary>
    ''' Ungarbles a password that has been previously garbled using
    ''' the GarblePassword function.
    ''' </summary>
    ''' <param name="pwd">The garbled password</param>
    ''' <returns>The real password</returns>
    Public Shared Function UngarblePassword(ByVal pwd As String) As SafeString
        If pwd = "" Then Return New SafeString()

        Dim charVals() As String = Split(pwd, ",")
        Dim ss As New SafeString()

        Dim output As String = ""
        For Each s As String In charVals
            ss.AppendChar(Chr(CInt(s)))
        Next

        Using pinned = ss.Pin()
            Chipher(pinned.Chars)
            ' We now need a new secure string with these values
            Dim newSs = New SafeString()
            For Each c As Char In pinned.Chars
                newSs.AppendChar(c)
            Next
            Return newSs
        End Using

    End Function

    '
    ' The following three functions are copied from the originals
    ' in clsOptions, for now. They could use some tidying up, and
    ' will probably need to be made available to the application.
    '
    Private Shared Sub Chipher(ByVal secret As Char())
        Dim key As String = "Aut0m4te"
        Dim L As Integer = Len(key)
        Dim X As Integer
        Dim sChar As Integer
        For X = 0 To Len(secret) - 1
            sChar = Asc(key.Mid(1 + (X Mod L), 1))
            secret(X) = Chr(Asc(secret(X)) Xor sChar)
        Next
    End Sub

    ''' <summary>
    ''' Creates a new process/object of the given type, ensuring that all initial
    ''' stages and notes are created as needed.
    ''' </summary>
    ''' <param name="tp">The type of process to create</param>
    ''' <param name="extObjs">The external objects to use for the new process</param>
    ''' <param name="name">The name of the process to create</param>
    ''' <param name="desc">The description of the process</param>
    ''' <param name="username">The username of the creator of the process</param>
    ''' <returns>The newly created and initialised process/VBO</returns>
    Public Shared Function CreateProcess(
     tp As DiagramType,
     extObjs As IGroupObjectDetails,
     name As String,
     desc As String,
     username As String
    ) As clsProcess

        Dim proc As New clsProcess(extObjs, tp, True) With {
            .Id = Guid.NewGuid(),
            .Name = name,
            .Description = desc,
            .CreatedBy = username,
            .CreatedDate = Date.Now,
            .ModifiedBy = username,
            .ModifiedDate = Date.Now
        }
        ' Text of the hint note box added to a Clean Up page in a newly created VBO.
        ' Used only in <see cref="CreateProcess"/> (at time of going to press)
        Dim CleanUpHint As String =
        My.Resources.Resources.clsProcess_CleanUpPageThisIsAnOptionalPageWhereYouMightChooseToPerformSomeFinalisationOrCl & vbCrLf & vbCrLf & _
 _
        My.Resources.Resources.clsProcess_TheCleanupActionWillBeCalledAutomaticallyImmediatelyAfterClosingYourBusinessObj & vbCrLf & vbCrLf & _
 _
        My.Resources.Resources.clsProcess_CleanUpYouWillNotBeAbleToCallThisActionFromABusinessProcessNorWillItBeCalledAtAnyOther

        ' Text of the hint note box added to a Initialise page in a newly created VBO.
        ' Used only in <see cref="CreateProcess"/> (at time of going to press)
        Dim InitialiseHint As String =
        My.Resources.Resources.clsProcess_InitialisePage & vbCrLf & vbCrLf & _
 _
        My.Resources.Resources.clsProcess_ThisIsAnOptionalPageWhereYouMightChooseToPerformSomeInitialisationTasksAfterYou & vbCrLf & vbCrLf & _
 _
        My.Resources.Resources.clsProcess_TheInitialiseActionWillBeCalledAutomaticallyImmediatelyAfterLoadingYourBusiness & vbCrLf & vbCrLf & _
 _
        My.Resources.Resources.clsProcess_InitialiseYouWillNotBeAbleToCallThisActionFromABusinessProcessNorWillItBeCalledAtAnyOther
        proc.CreateMissingSystemSheets()

        'make into a business object if need be
        If tp = DiagramType.Object Then
            ' Add a 'first action' for the user
            proc.ActiveSubSheet = proc.AddSubSheet(My.Resources.Resources.clsProcess_Action1)

            'Create cleanup note stage with handy hint
            Dim cleanupPage As clsProcessSubSheet = proc.GetCleanupPage
            proc.AddStage(New clsNoteStage(proc) With {
                .Location = New PointF(-180, 60),
                .Size = New SizeF(180, 230),
                .Narrative = CleanUpHint,
                .SubSheet = cleanupPage
            })

            'Create cleanup note stage with handy hint
            Dim initPage As clsProcessSubSheet = proc.GetMainPage
            proc.AddStage(New clsNoteStage(proc) With {
                .Location = New PointF(-180, 60),
                .Size = New SizeF(180, 230),
                .Narrative = InitialiseHint,
                .SubSheet = initPage
            })
            initPage.StartStage.OnSuccess = initPage.EndStage.Id

        End If
        Return proc

    End Function

    Private Shared ReadOnly GroupBusinessObjectFactory _
        As Func(Of IGroupObjectDetails, clsProcess, clsSession, Boolean, List(Of String), clsGroupBusinessObject) =
        Function(externalObjectsInfo, process, session, includeParent, objects) _
        New clsGroupBusinessObject(externalObjectsInfo, process, session, includeParent, objects)

#End Region

#Region " Instance Events "

    ''' <summary>
    ''' Event raised when the RunState of this process instance changes.
    ''' </summary>
    ''' <param name="state">The new mode</param>
    Public Event RunStateChanged(ByVal state As ProcessRunState)

    ''' <summary>
    ''' Event raised to signify the addition of a new stage to the process.
    ''' </summary>
    ''' <param name="stage">The stage newly added</param>
    Public Event StageAdded(ByVal stage As clsProcessStage)

    ''' <summary>
    ''' Event raised to signify the deletion of an existing stage in the
    ''' process.
    ''' </summary>
    ''' <param name="stage">The stage deleted.</param>
    Public Event StageDeleted(ByVal stage As clsProcessStage)

    ''' <summary>
    ''' Event raised to signify that the process XML has been reloaded.
    ''' This will happen when an undo/redo occurs, amongst other occasions.
    ''' </summary>
    Public Event ProcessXMLReloaded()

    ''' <summary>
    ''' Event raised when a stage is modified.
    ''' </summary>
    ''' <param name="newStg">The new version of the stage.</param>
    ''' <param name="oldStg">The old version of the stage.</param>
    Public Event StageModified(
     ByVal newStg As clsProcessStage, ByVal oldStg As clsProcessStage)

    ''' <summary>
    ''' Notifies clients of changes to selection.
    ''' </summary>
    Public Event SelectionChanged()

    ''' <summary>
    ''' Event raised any time the undo buffer is changed, for example when
    ''' a new state is added or if we have navigated back/forward along
    ''' the chain of states.
    ''' 
    ''' Essentially this is designed to allow the user interface to update
    ''' the undo/redo buttons as the user navigates backwards/forwards
    ''' along the undo buffer, or as states are added/removed.
    ''' </summary>
    ''' <param name="NewState">The new state of the process' undo/redo
    ''' manager</param>
    Public Event UndoRedoStatusChanged(
     ByVal NewState As clsUndoRedoManager.ManagerStates)

    ''' <summary>
    ''' An event describing a saved undo position.
    ''' </summary>
    ''' <param name="state">Details of the undo position</param>
    Public Event UndoPositionSaved(ByVal state As clsUndo)

#End Region

#Region " Member Variables "
    Private mSchema As ProcessSchema
    Private mStateManager As ProcessStateManager = New ProcessStateManager()

    Public Property Name As String
        Get
            Return mSchema.Name
        End Get
        Set(value As String)
            mSchema.Name = value
        End Set
    End Property

    Public Property Description As String
        Get
            Return mSchema.Description
        End Get
        Set(value As String)
            mSchema.Description = value
        End Set
    End Property

    'Current input parameters for the process
    Friend mInputParameters As clsArgumentList

    'Output parameters from the process - only populated when the end
    'stage is executed.
    Friend mOutputParameters As clsArgumentList

    ' Flag indicating if stop has been requested on this process since the current
    ' step started.
    Private mImmediateStopRequested As Boolean

    ''' <summary>
    ''' The ID of the active subsheet.
    ''' </summary>
    ''' <remarks> When editing/viewing, this indicates the subsheet being viewed. Note
    ''' that the convention that the main page has an ID of Guid.Empty still exists.
    ''' </remarks>
    Friend mActiveSubsheetId As Guid



    'Selection offset, valid after calls to SelectAtPosition
    Private mSelectionOffsetStage As clsProcessStage
    Private mSelectionOffsetX As Single
    Private mSelectionOffsetY As Single
    Private mSelectionCorner As Integer

    Private mState As ProcessRunState

    ''' <summary>
    ''' True when in recovery mode. Not relevant if the Run Mode is Off.
    ''' </summary>
    Friend mRecoveryMode As Boolean

    ''' <summary>
    ''' When in recovery mode, this is the Recovery stage where the current recovery
    ''' phase started.
    ''' </summary>
    Private mRecoverStart As clsProcessStage

    ''' <summary>
    ''' When in recovery mode, these contain the type and detail of the exception
    ''' being recovered from, and the stage ID of the stage where the exception
    ''' originated.
    ''' </summary>
    Friend mRecoveryType As String
    Friend mRecoveryDetail As String
    Friend mRecoverySource As Guid

    Private mSession As clsSession

    'If the current stage is set to the start stage of a subsheet,
    'the following contains the input parameters in XML format
    Friend mRunInputs As clsArgumentList

    Public Property RunPageId As Guid
        Get
            Return mStateManager.RunPageId
        End Get
        Set(value As Guid)
            mStateManager.RunPageId = value
        End Set
    End Property

    ''' <summary>
    ''' Call stack for subsheets during run. Each entry is a return pointer to the
    ''' calling stage itself.
    ''' </summary>
    Friend mSubSheetStack As Stack(Of Guid)

    ''' <summary>
    ''' When running, if this is not Nothing, we are in a 'ChildWait' status, i.e.
    ''' waiting for a child process to complete. The current stage will be the one
    ''' that put us in that state, and the one to move us out of it.
    ''' </summary>
    Friend WithEvents mChildWaitProcess As clsProcess

    ''' <summary>
    ''' When mChildWaitProcess is not Nothing, this is True if the child process is
    ''' owned by the current stage (meaning it should be disposed if that stage is
    ''' not going to be allowed to complete). Otherwise, it is False, for example
    ''' if it is just a reference to an existing Business Object.
    ''' </summary>
    Friend mChildWaitProcessOwned As Boolean

    ''' <summary>
    ''' Holds a collection of past states of this process in order that changes
    ''' can be undone.
    ''' 
    ''' Why this is a member of clsProcess is a mystery - it does not form part
    ''' of the natural encapsulation of a process. I believe that undo/redo operations
    ''' should be handled in the ctlProcessViewer class. PJW 26-04-2006.
    ''' </summary>
    Private WithEvents mUndoRedoManager As New clsUndoRedoManager()

    ''' <summary>
    ''' Set of Business Objects represent the underlying actual instances used when
    ''' the process is running.
    ''' 
    ''' In the case where the root clsProcess is an object (e.g. when an object is
    ''' being tested in Object Studio, or an object called as a web service) AND
    ''' the object shares the application model of a parent, the parent object
    ''' reference is not included in this list; it is loaded immediately and held
    ''' in mParentObjRef.
    ''' 
    ''' This can be Nothing, at times when the process has not been initialised.
    ''' </summary>
    Private mObjRefs As clsGroupBusinessObject = Nothing

    ''' <summary>
    ''' Set of Business Objects available for selection when editing a process.
    ''' 
    ''' This can be Nothing, at times when the process has not been queried for
    ''' object information.
    ''' </summary>
    Private mAllObjRefs As clsGroupBusinessObject

    ''' <summary>
    ''' A compound logging engine. Always valid, but may contain no entries if the
    ''' client has not attached any logging engines.
    ''' </summary>
    Private mLogger As CompoundLoggingEngine

    ''' <summary>
    ''' Private member to store public property DefaultStageFontColour()
    ''' </summary>
    Private mDefaultStageFontColour As Color

    ''' <summary>
    ''' Private member to store public property Attributes()
    ''' </summary>
    Private mAttributes As ProcessAttributes

    ''' <summary>
    ''' A reference to an AMI object, if were're using one. Otherwise Nothing.
    ''' </summary>
    Private mAMI As clsAMI

    'Cleanup code...
    Private mDisposed As Boolean = False

    Private mEnvVars As Dictionary(Of String, clsArgument)

    Private mSkills As List(Of SkillDetails)

    ''' <summary>
    ''' The parent process when this process is being run as a subprocess, or as a
    ''' Business Object owned by a process.
    ''' </summary>
    Public Property ParentProcess As clsProcess
    Public Property ExternalObjectsInfo As IGroupObjectDetails
    Public Property CreatedBy As String
        Get
            Return mSchema.CreatedBy
        End Get
        Set(value As String)
            mSchema.CreatedBy = value
        End Set
    End Property

    Public Property CreatedDate As DateTime
        Get
            Return mSchema.CreatedDate
        End Get
        Set(value As DateTime)
            mSchema.CreatedDate = value
        End Set
    End Property

    Public Property ModifiedBy As String
        Get
            Return mSchema.ModifiedBy
        End Get
        Set(value As String)
            mSchema.ModifiedBy = value
        End Set
    End Property

    Public Property ModifiedDate As DateTime
        Get
            Return mSchema.ModifiedDate
        End Get
        Set(value As DateTime)
            mSchema.ModifiedDate = value
        End Set
    End Property

    ''' <summary>
    ''' When a data stage raises a breakpoint as a result of its value being
    ''' read, changed etc.
    ''' </summary>
    Private mRaisedBreakPointInfo As clsProcessBreakpointInfo

    ''' <summary>
    ''' Indicates whether events that would normally be raised in response to a
    ''' selectionchange should be suppressed.
    ''' </summary>
    Private mSuppressSelectionEvents As Boolean

    Private mCompilerRunner As clsCompilerRunner

    ''' <summary>
    ''' The default period between retries if a logging operation fails
    ''' </summary>
    Private Const DefaultRetrySeconds As Integer = 5

    ''' <summary>
    ''' The default number of attempts to log an entry
    ''' </summary>
    Private Const DefaultAttempts As Integer = 5

    Private Const DefaultVersion As String = "1.0"
#End Region

#Region " Constructors "

    ''' <summary>
    ''' Constructor for the clsProcess class.
    ''' </summary>
    ''' <param name="objExternalObjectsInfo">Details of the set of
    ''' external objects available to the process. The caller should
    ''' create an instance of this class and add the relevant
    ''' information.</param>
    ''' <param name="newProcessType">The type of the process, which must be one of
    ''' clsProcess.Type, i.e. a 'real' process or a business object.</param>
    ''' <param name="bEditable">Specify if the process should be editable. This
    ''' is currently used for the purposes of optimisation - specifically, if we
    ''' know the process won't changed, we only need to load the business objects
    ''' it currently references, and not those it might use. However, usage of this
    ''' setting may be expanded and properly enforced later, so set it properly!</param>
    Public Sub New(objExternalObjectsInfo As IGroupObjectDetails, newProcessType As DiagramType, bEditable As Boolean)
        Me.New(objExternalObjectsInfo, newProcessType, bEditable, DefaultVersion)
    End Sub

    Private Sub New(objExternalObjectsInfo As IGroupObjectDetails, newProcessType As DiagramType, bEditable As Boolean, version As String)
        mSchema = New ProcessSchema(bEditable, version, newProcessType)

        'Store details of available external objects for use later...
        ExternalObjectsInfo = objExternalObjectsInfo
        BreakOnHandledException = False
        Functions = New clsFunctions(Me)
        SelectNone()

        'Initialise the run mode to off - a rare occurence of setting the private
        'member directly - and set defaults for other run-state-related stuff...
        mState = ProcessRunState.Off
        mRunInputs = Nothing

        Preconditions = New List(Of String)

        'Add the main page - all processes have this subsheet
        'with empty subsheet ID by convention.
        '
        'Adding this page automatically adds start stage and subsheet info stage, 
        Dim MainPage As clsProcessSubSheet = AddSubSheet(MainPageName, Guid.Empty, False)
        MainPage.SheetType = SubsheetType.MainPage

        'replace the subsheetinfo stage with a procssinfo stage
        Dim SubsheetInfoStage As clsSubsheetInfoStage = CType(GetStageByTypeAndSubSheet(StageTypes.SubSheetInfo, MainPage.ID), clsSubsheetInfoStage)
        Debug.Assert(Not SubsheetInfoStage Is Nothing)
        RemoveStage(SubsheetInfoStage)
        Dim ProcInfoStage As clsProcessInfoStage = CType(AddStage(StageTypes.ProcessInfo, Name), clsProcessInfoStage)
        ProcInfoStage.SetSubSheetID(MainPage.ID)
        ProcInfoStage.SetDisplayX(-195)
        ProcInfoStage.SetDisplayY(-105)
        ProcInfoStage.SetDisplayWidth(150)
        ProcInfoStage.SetDisplayHeight(90)
        If ProcessType = DiagramType.Object Then
            ProcInfoStage.NamespaceImports.Add("System")
            ProcInfoStage.NamespaceImports.Add("System.Drawing")
            ProcInfoStage.NamespaceImports.Add("System.Data")
            ProcInfoStage.AssemblyReferences.Add("System.dll")
            ProcInfoStage.AssemblyReferences.Add("System.Data.dll")
            ProcInfoStage.AssemblyReferences.Add("System.Xml.dll")
            ProcInfoStage.AssemblyReferences.Add("System.Drawing.dll")
        End If

        'set the default view sheet (recall main page is init page)
        mActiveSubsheetId = GetMainPage().ID

        GetEnvironmentVars(False)

        If ProcessType = DiagramType.Object Then
            GetMainPage().Name = InitPageName
            mApplicationDefinition = New clsApplicationDefinition()
            mCompilerRunner = New clsCompilerRunner(Me)

            Dim info As clsGlobalInfo = If(
                clsAPC.ProcessLoader IsNot Nothing,
                clsAPC.ProcessLoader.GetAMIInfo,
                New clsGlobalInfo)

            mAMI = New clsAMI(info)
        Else
            mCompilerRunner = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Attempt to update the env var cache for each process. Passing 
    ''' </summary>
    ''' <param name="refreshFromServer"></param>
    Public Sub GetEnvironmentVars(refreshFromServer As Boolean)

        'We always get these, although if we have a parent process we will
        'not actually use them. The problem is we don't know until later,
        'but they might be needed before then - e.g. if editing a process
        'in Process Studio, before running it.
        ' If the process loader is not initialised (eg. when just parsing a
        ' process), then we definitely won't need them, so skip them.
        If clsAPC.ProcessLoader IsNot Nothing Then _
            mEnvVars = clsAPC.ProcessLoader.GetEnvVars(refreshFromServer)
    End Sub

#End Region

#Region " Everything Else "

    ''' <summary>
    ''' Reads a Single from a Process XML element, taking into account backward
    ''' compatibility requirements (i.e. pre-5.0 they were incorrectly written using
    ''' locale-based formatting).
    ''' </summary>
    Friend Shared Function ReadXmlSingle(el As XmlElement) As Single
        Dim txt As String = el.InnerText
        If txt.Contains(",") Then
            txt = txt.Replace(",", ".")
        End If
        Return XmlConvert.ToSingle(txt)
    End Function

    ''' <summary>
    ''' Gets or sets whether a session running this process (as the master process of
    ''' the session - ie. not as a subprocess) should abort if logging fails, after
    ''' a defined number of attempts.
    ''' </summary>
    Public Property AbortOnLogError() As Boolean

    ''' <summary>
    ''' Gets or sets whether nested collections are passed by reference (as before
    ''' bg-1321 was fixed) or by value. This affects all calc stages, multicalc
    ''' stages and parameters initialised by this process and operates on a
    ''' per-process basis - ie. it does not affect subprocesses or VBOs called by
    ''' this process. Each has its own setting which is honoured within that
    ''' subprocess/VBO.
    ''' </summary>
    Public Property PassNestedCollectionsByReference As Boolean = True

    ''' <summary>
    ''' Gets or sets the number of attempts to make writing a log for a session which
    ''' is running this process (as the master process of the session - ie. not as a
    ''' subprocess)
    ''' </summary>
    Public Property LoggingAttempts As Integer = DefaultAttempts

    ''' <summary>
    ''' Gets or sets the number of seconds to wait between attempts to write to the
    ''' log in a session running this process (as the master process of the session -
    ''' ie. not as a subprocess)
    ''' </summary>
    Public Property LoggingRetryPeriod As Integer = DefaultRetrySeconds

    ''' <summary>
    ''' The business object run mode for this process.
    ''' </summary>
    ''' <remarks>Relevant only when ProcessMode property is set to Object. Not to be
    ''' confused with the RunMode, which is a totally different concept.</remarks>
    Public Property ObjectRunMode As BusinessObjectRunMode

    ''' <summary>
    ''' Gets or sets the current running state of this process. If the state of this
    ''' process is set to anything other than <see cref="ProcessRunState.Running"/>,
    ''' the <see cref="RunToStageId"/> is cleared.
    ''' </summary>
    Public Property RunState() As ProcessRunState
        Get
            Return mState
        End Get
        Set(ByVal value As ProcessRunState)
            If value = mState Then Return
            mState = value
            ' If runstate changes to anything other than running, nix the runto stage
            If value <> ProcessRunState.Running Then RunToStageId = Guid.Empty
            RaiseEvent RunStateChanged(mState)
        End Set
    End Property

    ''' <summary>
    ''' Get the display name for the current process <see cref="RunState"/>
    ''' </summary>
    Public ReadOnly Property RunStateDisplay() As String
        Get
            'This is the only one which differs from its enumeration name
            If mState = ProcessRunState.SteppingOver Then Return My.Resources.Resources.clsProcess_SteppingOver

            'The rest can just go as they are
            Return mState.ToString()
        End Get
    End Property

    ''' <summary>
    ''' Raises a stage alert event.
    ''' </summary>
    ''' <param name="Stage">The alert stage</param>
    ''' <param name="Message">The alert text</param>
    Friend Sub RaiseStageAlert(ByVal Stage As clsAlertStage, ByVal Message As String)
        RaiseEvent StageAlert(Stage, Message)
    End Sub

    ''' <summary>
    ''' Checks if this process is in a running state or not. According to 
    ''' https://portal.blueprism.com/wiki/index.php?title=Automate#Process_Run_State,
    ''' having a run state implies that it has a set of initialised Business Objects,
    ''' data items have 'current' values, etc.
    ''' </summary>
    ''' <returns>True if this process is currently in a running state, False
    ''' otherwise.</returns>
    Public Function IsRunning() As Boolean
        ' A process is in a run state if its run mode is not 'off'.
        ' Even if failed / completed - a 'next stage' can be set and stepped over
        ' so the run state remains.
        Return (mState <> ProcessRunState.Off)
    End Function

    ''' <summary>
    ''' The ID of the run stage, i.e. the next stage that will be executed. Not
    ''' relevant when the Run Mode is Off.
    ''' </summary>
    Public Property RunStageID() As Guid
        Get
            Return mStateManager.RunStageId
        End Get
        Private Set(value As Guid)
            mStateManager.RunStageId = value
        End Set
    End Property

    ''' <summary>
    ''' The current run stage - ie. the next stage to be executed, if one exists.
    ''' This will be null if the <see cref="RunState"/> is
    ''' <see cref="ProcessRunState.Off"/> or no current run stage is set.
    ''' </summary>
    Public ReadOnly Property RunStage() As clsProcessStage
        Get
            If RunStageID = Nothing OrElse mState = ProcessRunState.Off Then Return Nothing
            Return GetStage(RunStageID)
        End Get
    End Property

    ''' <summary>
    ''' Gets the start stage associated with this process (ie. with the initial page
    ''' of the process), or null if there is no start stage defined in this process's
    ''' initial page.
    ''' </summary>
    Public ReadOnly Property StartStage() As clsStartStage
        Get
            Return TryCast(GetStage(GetStartStage()), clsStartStage)
        End Get
    End Property

    ''' <summary>
    ''' Gets the process info stage for this process, or null if it doesn't have one
    ''' </summary>
    Public ReadOnly Property InfoStage() As clsProcessInfoStage
        Get
            Return DirectCast(
             GetStageByTypeAndSubSheet(StageTypes.ProcessInfo, Nothing),
             clsProcessInfoStage)
        End Get
    End Property

    ''' <summary>
    ''' The ID of the stage that was the source of the current exception status, or
    ''' Guid.Empty if we are not currently engaged in recovering from an exception.
    ''' </summary>
    Public ReadOnly Property ExceptionSource() As Guid
        Get
            If mRecoveryMode Then Return mRecoverySource
            Return Guid.Empty
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this process is currently engaged in a full validation cycle, ie
    ''' that a non-stage-specific validation is currently in progress within this
    ''' process.
    ''' </summary>
    Public ReadOnly Property IsFullValidateInProgress() As Boolean
        Get
            Return (mBacklinksLookup IsNot Nothing)
        End Get
    End Property

    ''' <summary>
    ''' The session, when we're running, or Nothing if we're not. Code that can only
    ''' be executed when the process is actually running can safely assume this is
    ''' not Nothing!
    ''' 
    ''' Only the root process of a session actually has the private member mSession
    ''' set to the session in question. Accessing the property though, will get you
    ''' the session from a child process.
    ''' </summary>
    Public Property Session() As clsSession
        Get
            ' If we have no session registered, but we are running as a child process,
            ' check back through the ancestor processes until we find a session, or we
            ' reach the 'alpha' process.
            If mSession Is Nothing AndAlso ParentProcess IsNot Nothing Then
                Return ParentProcess.Session
            End If
            Return mSession
        End Get
        Set(ByVal value As clsSession)
            mSession = value
        End Set
    End Property

    ''' <summary>
    ''' Set of Functions available to this process.
    ''' </summary>
    Public ReadOnly Property Functions As clsFunctions

    ''' <summary>
    ''' The font that will be applied to new stages by default. Returns black if not
    ''' already set.
    ''' </summary>
    ''' <value></value>
    Public Property DefaultStageFontColour() As Color
        Get
            If Not mDefaultStageFontColour.Equals(Color.Empty) Then
                Return mDefaultStageFontColour
            Else
                Return Color.Black
            End If
        End Get
        Set(ByVal value As Color)
            mDefaultStageFontColour = value
        End Set
    End Property

    ''' <summary>
    ''' Attributes of this process.
    ''' </summary>
    ''' <value></value>
    Public Property Attributes As ProcessAttributes

    ''' <summary>
    ''' Gets the index of the first allowed normal subsheet in this process.
    ''' Basically, if it's a process, it's 1 (0 is the 'Main Page'); if it's an
    ''' object, it's 2 (0 is 'Initialisation', 1 is 'Clean Up').
    ''' </summary>
    Public ReadOnly Property FirstNormalSubSheetIndex() As Integer
        Get
            Return CInt(IIf(ProcessType = DiagramType.Object, 2, 1))
        End Get
    End Property

    Public ReadOnly Property ProcessType() As DiagramType
        Get
            Return mSchema.ProcessType
        End Get
    End Property

    Public ReadOnly Property Editable As Boolean
        Get
            Return If(mSchema?.IsEditable, False)
        End Get
    End Property

    Public ReadOnly Property IsDisposed() As Boolean
        Get
            Return mDisposed
        End Get
    End Property
    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
    Public Overloads Sub Dispose(ByVal disposing As Boolean)
        If mDisposed Then Return
        mDisposed = True
        If disposing Then
            Try
                DisposeObjects()

                'Release 'Child Wait Process' if it is owned by the stage that created
                'it...
                If mChildWaitProcess IsNot Nothing _
                 AndAlso mChildWaitProcessOwned Then mChildWaitProcess.Dispose()

                If mCompilerRunner IsNot Nothing Then mCompilerRunner.Dispose()

                mSchema.Stages.Clear()

                If mAMI IsNot Nothing Then mAMI.Dispose()

                Fonts?.Dispose()

            Catch ' Ignore exceptions
            End Try
        End If
    End Sub
    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    ''' <summary>
    ''' Return a reference to the AMI instance currently in use.
    ''' </summary>
    ''' <returns>A clsAMI object, or Nothing if there isn't one available.</returns>
    Public ReadOnly Property AMI() As clsAMI
        Get
            If ProcessType <> DiagramType.Object Then
                Throw New InvalidOperationException(My.Resources.Resources.clsProcess_TheAMIPropertyOnlyValidForProcessesOfTypeObject)
            End If

            If ParentObjRef IsNot Nothing Then
                If CType(ParentObjRef, clsVBO).Process IsNot Nothing Then _
                 Return CType(ParentObjRef, clsVBO).Process.AMI
            End If
            Return mAMI
        End Get
    End Property

    ''' <summary>
    ''' The application definition associated with this process - only relevant if
    ''' it is a business object.
    ''' </summary>
    Public Property ApplicationDefinition() As clsApplicationDefinition
        Get
            If ParentObjRef IsNot Nothing Then
                If CType(ParentObjRef, clsVBO).Process IsNot Nothing Then _
                 Return CType(ParentObjRef, clsVBO).Process.ApplicationDefinition
            End If
            Return mApplicationDefinition
        End Get
        Set(ByVal value As clsApplicationDefinition)
            Debug.Assert(ProcessType = DiagramType.Object,
             My.Resources.Resources.clsProcess_AttemptToSetApplicationDefinitionPropertyOfProcessWhenProcessTypeIsNotBusinessO)
            mApplicationDefinition = value
            Mark() ' Indicate that this process's data has changed
        End Set
    End Property
    Private mApplicationDefinition As clsApplicationDefinition

    ''' <summary>
    ''' The name of the parent object, for the purposes of model sharing - only
    ''' relevant if it is a business object. Nothing when there isn't one.
    ''' </summary>
    Public Property ParentObject() As String
        Get
            Return mParentObject
        End Get
        Set(ByVal value As String)
            mParentObject = value
            Mark()
        End Set
    End Property
    Private mParentObject As String = Nothing

    ''' <summary>
    ''' When loaded, a reference to the parent object, for the purposes of model
    ''' sharing - only relevant if it is a business object. Nothing when there isn't
    ''' one.
    ''' </summary>
    Public Property ParentObjRef() As clsBusinessObject

    ''' <summary>
    ''' Applies to Objects only.
    ''' Indicates that this object is defined as having global scope at runtime, and
    ''' is able to share it's application model with other 'child' objects. This is
    ''' deliberately not applicable to 'child' objects so that we can distinguish
    ''' between parent/child objects easily on the database. However, 'child' objects
    ''' inherit scope from their parents at runtime (see HasProcessLevelScope below).
    ''' </summary>
    Public Property IsShared() As Boolean
        Get
            If ProcessType = DiagramType.Process OrElse ParentObject IsNot Nothing Then _
             Return False
            Return mShared
        End Get
        Set(value As Boolean)
            mShared = value
        End Set
    End Property
    Private mShared As Boolean = False

    ''' <summary>
    ''' Applies to Objects only.
    ''' Returns true if the same instance of this object should be used.
    ''' </summary>
    Public ReadOnly Property HasProcessLevelScope() As Boolean
        Get
            If ParentObject IsNot Nothing Then Return True
            Return IsShared
        End Get
    End Property

    Public ReadOnly Property SkillsInUse As List(Of SkillDetails)
        Get
            Return mSkills
        End Get
    End Property

    Public ReadOnly Property FileExtension As String
        Get
            Return If(ProcessType = DiagramType.Process, ProcessFileExtension, ObjectFileExtension)
        End Get
    End Property

    Public Const ProcessFileExtension As String = "bpprocess"

    Public Const ObjectFileExtension As String = "bpobject"

    ''' <summary>
    ''' Load parent object (containing the shared model). This is only required where
    ''' the child object is the root. Other parent objects will be loaded on demand
    ''' via clsVBO.
    ''' Note that the root process's parent object is not added to mObjRefs since it
    ''' doesn't want to be disposed with the rest if resetting in object studio.
    ''' </summary>
    ''' <param name="id">Parent object ID</param>
    Public Sub LoadParent(ByVal id As Guid)
        If ParentObject Is Nothing Then Exit Sub

        'Remove any previous parent reference
        UnloadParent()

        'Create new VBO
        Dim parent As New clsVBODetails()
        parent.ID = id
        parent.FriendlyName = ParentObject
        ParentObjRef = New clsVBO(parent, ExternalObjectsInfo, Me)

        'Ensure it is loaded
        ParentObjRef.GetActions()
    End Sub

    ''' <summary>
    ''' Unload parent (if no longer sharing a model)
    ''' </summary>
    Public Sub UnloadParent()
        'Dispose of any old object reference
        If ParentObjRef IsNot Nothing Then
            ParentObjRef.Dispose()
            ParentObjRef = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Update the stored database value for a statistic.
    ''' </summary>
    ''' <param name="sName">The statistic name</param>
    ''' <param name="objValue">The value</param>
    ''' <param name="sErr">If an error occurs, contains an error description.</param>
    ''' <returns>True if the statistic was successfully updated.</returns>
    Public Function UpdateStatistic(ByVal sName As String, ByVal objValue As clsProcessValue, ByRef sErr As String) As Boolean

        'Do nothing unless running...
        If mState = ProcessRunState.Off Then Exit Function

        If Logger.Count = 0 Then
            sErr = My.Resources.Resources.clsProcess_NoLoggingEngineAvailableToUpdateStatistic
            Return False
        End If

        Try
            Logger.LogStatistic(Session.ID, sName, objValue)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Delete the specified stage from the process.
    ''' </summary>
    ''' <param name="stage">The stage to delete.</param>
    ''' <param name="sErr">In the event of failure, this contains an error message.
    ''' </param>
    ''' <returns>True if successful, False otherwise.</returns>
    Public Function DeleteStage(ByVal stage As clsProcessStage, ByRef sErr As String) As Boolean

        Try
            'Get the stage ID of the stage we are deleting...
            Dim targetStageID As Guid = stage.GetStageID()

            'Clear the selection, in case we have the deleted stage
            'selected...
            SelectNone()

            'If we have deleted the current run stage, reset that...
            If targetStageID = RunStageID Then RunStageID = Guid.Empty

            'Remove any links to this stage...
            For Each stg As clsProcessStage In mSchema.Stages
                Select Case stg.StageType
                    Case StageTypes.Decision
                        Dim decn As clsDecisionStage = CType(stg, clsDecisionStage)
                        If decn.OnTrue = targetStageID Then decn.OnTrue = Guid.Empty
                        If decn.OnFalse = targetStageID Then decn.OnFalse = Guid.Empty

                    Case StageTypes.ChoiceStart, StageTypes.WaitStart
                        Dim chc As clsChoiceStartStage = CType(stg, clsChoiceStartStage)
                        For Each node As clsChoice In chc.Choices
                            If node.LinkTo = targetStageID Then node.LinkTo = Guid.Empty
                        Next
                    Case Else
                        If TypeOf stg Is clsLinkableStage Then
                            Dim linkStg As clsLinkableStage = CType(stg, clsLinkableStage)
                            If linkStg.OnSuccess = targetStageID Then linkStg.OnSuccess = Guid.Empty
                        End If
                End Select
            Next

            RaiseEvent StageDeleted(stage)

            'Finally, remove the stage itself...
            RemoveStage(stage)

            Return True
        Catch ex As Exception
            sErr = String.Format(My.Resources.Resources.clsProcess_UnableToDeleteStage0, ex.Message)
        End Try
    End Function

    Private Sub DeleteSkill(skillId As Guid)
        Dim skillIdString = skillId.ToString()
        mSkills.Remove(mSkills.FirstOrDefault(Function(x) x.SkillId.Equals(skillIdString)))
    End Sub

    ''' <summary>
    ''' Delete the specified stage from the process. Note that this only removes the
    ''' stage from the internal list of stages - none of the other actions that
    ''' should be taken when removing a stage (e.g. removing references to it) are
    ''' done.
    ''' </summary>
    ''' <param name="objStage">A reference to the stage to delete.</param>
    ''' <remarks>This should only be used internally, and for testing.</remarks>
    Public Sub RemoveStage(ByVal objStage As clsProcessStage)
        mSchema.Stages.Remove(objStage)

        If objStage.StageType = StageTypes.Skill Then
            Dim skillStage = CType(objStage, clsSkillStage)
            DeleteSkill(skillStage.SkillId)
        End If
    End Sub

    Public Function GetUniqueStageID(ByVal stype As StageTypes) As String
        Return mSchema.GetUniqueStageID(stype)
    End Function

    ''' <summary>
    ''' Gets the default world coordinates of a start stage on a new sheet. Useful
    ''' if you wish to snap these coordinates to a grid before adding the stage.
    ''' </summary>
    ''' <param name="Xcoord">The x coordinate of the stage.</param>
    ''' <param name="Ycoord">The y coordinate of the stage.</param>
    Public Sub GetDefaultStartStageLocationForANewSubSheet(ByRef Xcoord As Single, ByRef Ycoord As Single)
        Xcoord = 15
        Ycoord = -105
    End Sub

    ''' <summary>
    ''' Gets the default world coordinates of an end stage on a new sheet. Useful
    ''' if you wish to snap these coordinates to a grid before adding the stage.
    ''' </summary>
    ''' <param name="Xcoord">The x coordinate of the stage.</param>
    ''' <param name="Ycoord">The y coordinate of the stage.</param>
    Public Sub GetDefaultEndStageLocationForANewSubSheet(ByRef Xcoord As Single, ByRef Ycoord As Single)
        Xcoord = 15
        Ycoord = 90
    End Sub

    ''' <summary>
    ''' Adds a new sheet using the specified name. Use the overloaded method
    ''' if you want to choose the sheet's ID
    ''' 
    ''' Default locations for start/end stages etc are used.
    ''' </summary>
    ''' <param name="SheetName">The sheet name.</param>
    ''' <returns>Returns the ID of the new subsheet.</returns>
    Public Function AddSubSheet(ByVal SheetName As String) As clsProcessSubSheet
        Return AddSubSheet(SheetName, False)
    End Function

    ''' <summary>
    ''' Adds a new sheet using the specified name. Use the overloaded method
    ''' if you want to choose the sheet's ID
    ''' 
    ''' Default locations for start/end stages etc are used.
    ''' </summary>
    ''' <param name="SheetName">The sheet name.</param>
    ''' <param name="SuppressAutoStageGeneration">If set to true then
    ''' the automatic generation of start, end, process info
    ''' stages.</param>
    ''' <returns>Returns the ID of the new subsheet.</returns>
    Private Function AddSubSheet(ByVal SheetName As String, ByVal SuppressAutoStageGeneration As Boolean) As clsProcessSubSheet
        Return AddSubSheet(SheetName, Guid.NewGuid, SuppressAutoStageGeneration)
    End Function

    ''' <summary>
    ''' Overloads AddSubsheet. Adds a new sheet using the default location for the
    ''' start and end stages.
    ''' </summary>
    ''' <param name="SuppressAutoStageGeneration">If set to true then
    ''' the automatic generation of start, end, process info
    ''' stages.</param>
    ''' <param name="sName">Name of the sheet to be added.</param>
    ''' <returns>Returns the new subsheet.</returns>
    Private Function AddSubSheet(ByVal sName As String, ByVal gSheetID As Guid, ByVal SuppressAutoStageGeneration As Boolean) As clsProcessSubSheet
        Dim StartStageXCoord, StartStageYCoord As Single
        Dim EndStageXCoord, EndStageYCoord As Single

        GetDefaultStartStageLocationForANewSubSheet(StartStageXCoord, StartStageYCoord)
        GetDefaultEndStageLocationForANewSubSheet(EndStageXCoord, EndStageYCoord)

        Return AddSubSheet(sName, StartStageXCoord, StartStageYCoord, EndStageXCoord, EndStageYCoord, gSheetID, SuppressAutoStageGeneration)
    End Function

    Public Function AddSubSheet(ByVal sName As String,
     ByVal sngStartStageXPosition As Single,
     ByVal sngStartStageYPosition As Single,
     ByVal sngEndStageXPosition As Single,
     ByVal sngEndStageYPosition As Single,
     ByVal gSheetID As Guid,
     ByVal SuppressAutoStageGeneration As Boolean) As clsProcessSubSheet
        Return AddSubSheet(sName, sngStartStageXPosition, sngStartStageYPosition,
         sngEndStageXPosition, sngEndStageYPosition, gSheetID,
         SuppressAutoStageGeneration, SubsheetType.Normal, -1)
    End Function

    ''' <summary>
    ''' Adds the main (sub) sheet for this process, based on its type and returns a
    ''' reference to the object which represents that sheet.
    ''' </summary>
    ''' <returns>The subsheet object representing the main sheet for this process /
    ''' object - either the 'Main Page' or the 'Initialise' page respectively.
    ''' </returns>
    ''' <remarks>This uses <see cref="ProcessType"/> to determine the type of main
    ''' sheet to create and how to initialise it, so it's essential that it has been
    ''' initialised itself before this method is called.</remarks>
    Private Function AddMainSheet() As clsProcessSubSheet
        Dim nm As String
        If ProcessType = DiagramType.Object Then nm = InitPageName Else nm = MainPageName
        ' The main sheet has an empty ID by convention
        Dim sheet As clsProcessSubSheet = AddSubSheet(nm, Guid.Empty, True)
        sheet.SheetType = SubsheetType.MainPage
        Return sheet
    End Function

    ''' <summary>
    ''' Add a new subsheet to the process.
    ''' </summary>
    ''' <param name="sName">The name for the new subsheet</param>
    ''' <param name="sngStartStageXPosition">The x coordinate of the start stage
    ''' of the new sheet. Use the overloaded method if you do not wish to specify
    ''' this.</param>
    ''' <param name="sngStartStageYPosition">The y coordinate of the start stage
    ''' of the new sheet. Use the overloaded method if you do not wish to specify
    ''' this.</param>
    ''' <param name="sngEndStageXPosition">The x coordinate of the end stage
    ''' of the new sheet. Use the overloaded method if you do not wish to specify
    ''' this.</param>
    ''' <param name="sngEndStageYPosition">The y coordinate of the end stage
    ''' of the new sheet. Use the overloaded method if you do not wish to specify
    ''' this.</param>
    ''' <param name="SuppressAutoStageGeneration">If set to true then
    ''' the automatic generation of start, end, process info
    ''' stages.</param>
    ''' <returns>The object representing the new subsheet</returns>
    Public Function AddSubSheet(ByVal sName As String,
     ByVal sngStartStageXPosition As Single,
     ByVal sngStartStageYPosition As Single,
     ByVal sngEndStageXPosition As Single,
     ByVal sngEndStageYPosition As Single,
     ByVal gSheetID As Guid,
     ByVal SuppressAutoStageGeneration As Boolean,
     ByVal sheetType As SubsheetType,
     ByVal iPosition As Integer) As clsProcessSubSheet

        Dim newsheet As New clsProcessSubSheet(Me) With {
            .SheetType = sheetType,
            .ID = gSheetID,
            .Name = sName,
            .ZoomPercent = 100,
            .CameraX = 0,
            .CameraY = 0
        }

        If iPosition >= 0 And iPosition < SubSheets.Count And newsheet.IsNormal Then
            ' Make sure we don't insert it in between the special sheets by using
            ' the first 'normal' index if iPosition is too low
            SubSheets.Insert(
             Math.Max(iPosition + 1, FirstNormalSubSheetIndex), newsheet)
        Else
            SubSheets.Add(newsheet)
        End If

        If Not SuppressAutoStageGeneration Then
            'Add the info stage, all subsheets have this when created,
            'and it can't be removed...
            With AddStage(StageTypes.SubSheetInfo, sName)
                .SetSubSheetID(gSheetID)
                .SetDisplayX(-195)
                .SetDisplayY(-105)
                .SetDisplayWidth(150)
                .SetDisplayHeight(90)
            End With

            'Add the start stage...
            With AddStage(StageTypes.Start, My.Resources.Resources.clsProcess_GetUniqueStageId_Start)
                .SetSubSheetID(gSheetID)
                .SetDisplayX(sngStartStageXPosition)
                .SetDisplayY(sngStartStageYPosition)
                .SetDisplayWidth(60)
                .SetDisplayHeight(30)
                .LogInhibit = LogInfo.InhibitModes.Never
                If ProcessType = DiagramType.Object Then .LogInhibit = LogInfo.InhibitModes.Always
            End With

            'Add the end stage...
            With AddStage(StageTypes.End, My.Resources.Resources.clsProcess_GetUniqueStageId_End)
                .SetSubSheetID(gSheetID)
                .SetDisplayX(sngEndStageXPosition)
                .SetDisplayY(sngEndStageYPosition)
                .SetDisplayWidth(60)
                .SetDisplayHeight(30)
                .LogInhibit = LogInfo.InhibitModes.Never
                If ProcessType = DiagramType.Object Then .LogInhibit = LogInfo.InhibitModes.Always
            End With
        End If

        Return newsheet
    End Function

    Public Function CreateStage(ByVal stagetype As StageTypes) As clsProcessStage
        Select Case stagetype
            Case StageTypes.Action
                Return New clsActionStage(Me)
            Case StageTypes.Anchor
                Return New clsAnchorStage(Me)
            Case StageTypes.Calculation
                Return New clsCalculationStage(Me)
            Case StageTypes.Collection
                Return New clsCollectionStage(Me)
            Case StageTypes.Data
                Return New clsDataStage(Me)
            Case StageTypes.Decision
                Return New clsDecisionStage(Me)
            Case StageTypes.End
                Return New clsEndStage(Me)
            Case StageTypes.LoopEnd
                Return New clsLoopEndStage(Me)
            Case StageTypes.LoopStart
                Return New clsLoopStartStage(Me)
            Case StageTypes.Note
                Return New clsNoteStage(Me)
            Case StageTypes.Process
                Return New clsSubProcessRefStage(Me)
            Case StageTypes.ProcessInfo
                Return New clsProcessInfoStage(Me)
            Case StageTypes.Skill
                Return New clsSkillStage(Me)
            Case StageTypes.Start
                Return New clsStartStage(Me)
            Case StageTypes.SubSheet
                Return New clsSubSheetRefStage(Me)
            Case StageTypes.SubSheetInfo
                Return New clsSubsheetInfoStage(Me)
            Case StageTypes.Read
                Return New clsReadStage(Me)
            Case StageTypes.Write
                Return New clsWriteStage(Me)
            Case StageTypes.Navigate
                Return New clsNavigateStage(Me)
            Case StageTypes.Code
                Return New clsCodeStage(Me)
            Case StageTypes.ChoiceStart
                Return New clsChoiceStartStage(Me)
            Case StageTypes.ChoiceEnd
                Return New clsChoiceEndStage(Me)
            Case StageTypes.WaitStart
                Return New clsWaitStartStage(Me)
            Case StageTypes.WaitEnd
                Return New clsWaitEndStage(Me)
            Case StageTypes.Alert
                Return New clsAlertStage(Me)
            Case StageTypes.Exception
                Return New clsExceptionStage(Me)
            Case StageTypes.Recover
                Return New clsRecoverStage(Me)
            Case StageTypes.Resume
                Return New clsResumeStage(Me)
            Case StageTypes.Block
                Return New clsBlockStage(Me)
            Case StageTypes.MultipleCalculation
                Return New clsMultipleCalculationStage(Me)
            Case Else
                Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' Adds a data stage to this process (using exactly the same mechanism as the
    ''' "clsProcess.AddStage" method).
    ''' </summary>
    ''' <param name="name">The name of the data stage to add</param>
    ''' <param name="tp">The type of the data stage to add. If the given type is
    ''' <see cref="DataType.collection"/>, a <see cref="clsCollectionStage"/> is
    ''' created; otherwise a base <see cref="clsDataStage"/> is created.</param>
    ''' <returns>The stage created and added to this process</returns>
    Public Function AddDataStage(ByVal name As String, ByVal tp As DataType) As clsDataStage
        Dim stgType As StageTypes = StageTypes.Data
        If tp = DataType.collection Then stgType = StageTypes.Collection
        Dim stg As clsDataStage = CType(AddStage(stgType, name), clsDataStage)
        stg.DataType = tp
        Return stg
    End Function

    ''' <summary>
    ''' Add a new stage to the process. Note that this overload also replaces the
    ''' current selection with the newly added stage.
    ''' </summary>
    ''' <param name="stagetype">The type of stage to create</param>
    ''' <param name="initialName">The initial name for the new stage.</param>
    ''' <returns>A reference to the new clsProcessStage object.</returns>
    Public Function AddStage(stagetype As StageTypes, initialName As String) As clsProcessStage
        Dim stg As clsProcessStage = CreateStage(stagetype)
        If initialName Is Nothing Then
            stg.InitialName = GetUniqueStageID(stagetype)
        Else
            stg.InitialName = initialName
        End If
        If stg.StageType = StageTypes.Block Then
            stg.Color = DefaultBlockColour
        Else
            stg.Color = DefaultStageFontColour
        End If
        'Add the stage and select it...
        SelectStage(AddStage(stg), False)
        Return stg
    End Function

    ''' <summary>
    ''' Add a stage, given a clsProcessStage object. The parent of the
    ''' stage is changed by this action. Note that this overload DOES NOT affect
    ''' the select, unlike the other one!
    ''' </summary>
    ''' <param name="objStage">The clsProcessStage to add.</param>
    ''' <returns>The index of the added stage</returns>
    Public Function AddStage(ByVal objStage As clsProcessStage) As Integer
        mSchema.Stages.Add(objStage)
        objStage.Process = Me
        RaiseEvent StageAdded(objStage)
        Return mSchema.Stages.Count - 1
    End Function

    ''' <summary>
    ''' Create a link between two stages. The two indexes are
    ''' the source stage and destination stage. If it is not
    ''' possible to create the link, False is returned and sErr
    ''' contains an error message to display to the user.
    ''' </summary>
    ''' <param name="SourceObject">The object from which the link should start.</param>
    ''' <param name="DestinationStage">The stage from to which the link should flow.</param>
    ''' <param name="sErr">A string to carry an error message.</param>
    ''' <returns>True if the link is successfully created,
    ''' false otherwise.</returns>
    Public Function CreateLink(ByVal SourceObject As ILinkable, ByVal DestinationStage As clsProcessStage, ByRef sErr As String) As Boolean

        If Not CanCreateLinkTo(DestinationStage, sErr) Then
            Return False
        End If

        Dim DestinationID As Guid = DestinationStage.GetStageID
        If TypeOf SourceObject Is clsProcessStage Then
            If CType(SourceObject, clsProcessStage).GetSubSheetID().CompareTo(DestinationStage.GetSubSheetID()) <> 0 Then
                sErr = My.Resources.Resources.clsProcess_CanTCreateALinkBetweenStagesOnDifferentSheets
                Return False
            End If
        End If

        Return SourceObject.SetNextLink(DestinationID, sErr)
    End Function

    ''' <summary>
    ''' Returns true if we can create a link to this type of stage.
    ''' </summary>
    ''' <param name="objStage">The stage for which we want to see if we can create a link to</param>
    ''' <param name="sErr">The message to give to the user explaining what cannot be linked to.</param>
    ''' <returns>Returns true if a link can be created to the specified
    ''' stage; false otherwise.</returns>
    ''' <remarks>When false, sErr carries back an error message.</remarks>
    Public Function CanCreateLinkTo(ByVal objStage As clsProcessStage, ByRef sErr As String) As Boolean
        Select Case objStage.StageType
            Case StageTypes.Data, StageTypes.Start, StageTypes.Collection, StageTypes.ProcessInfo, StageTypes.SubSheetInfo, StageTypes.ChoiceEnd, StageTypes.WaitEnd, StageTypes.Recover, StageTypes.Block
                sErr = String.Format(My.Resources.Resources.clsProcess_CanTCreateALinkToA0Stage, clsStageTypeName.GetLocalizedFriendlyName(objStage.StageType.ToString()))
                Return False
            Case Else
                Return True
        End Select
    End Function

    ''' <summary>
    ''' Returns true if we can create a link from this type of stage.
    ''' </summary>
    ''' <param name="objStage">The stage for which we want to see if we can create a link from</param>
    ''' <returns>Returns true if a link can be created from the specified
    ''' stage; false otherwise.</returns>
    Public Function CanCreateLinkFrom(objStage As clsProcessStage) As Boolean
        Select Case objStage.StageType
            Case StageTypes.ChoiceStart, StageTypes.WaitStart
                Return False
            Case Else
                Return True
        End Select
    End Function

    ''' <summary>
    ''' Check if this process has a start stage
    ''' </summary>
    ''' <returns>True if it has, False if it hasn't.</returns>
    Public Function HasStartStage() As Boolean
        For Each st As clsProcessStage In mSchema.Stages
            If st.StageType = StageTypes.Start Then
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Get the number of stages in the process
    ''' </summary>
    ''' <returns>The number of stages in the process!</returns>
    Public Function GetNumStages() As Integer
        Return GetNumStages(Nothing, Nothing)
    End Function

    Public Function GetNumStages(includedTypes As IEnumerable(Of StageTypes), excludedTypes As IEnumerable(Of StageTypes)) As Integer
        Return GetStages(includedTypes, excludedTypes).Count
    End Function

    Public Function GetNumStagesByLogInhibitModes(inhibitMode As LogInfo.InhibitModes) As Integer
        Return GetStages.Where(Function(l) l.LogInhibit = inhibitMode).Count
    End Function

    Public Function GetNumStagesByLogInhibitModes(includedTypes As IEnumerable(Of StageTypes), excludedTypes As IEnumerable(Of StageTypes), inhibitMode As LogInfo.InhibitModes) As Integer
        Return GetStages(includedTypes, excludedTypes).Where(Function(l) l.LogInhibit = inhibitMode).Count
    End Function

    ''' <summary>
    ''' Replace a stage with a modified copy
    ''' </summary>
    ''' <param name="gID">The Stage ID of the stage being replaced</param>
    ''' <param name="objStage">The replacement clsProcessStage</param>
    ''' <remarks>
    ''' Use with extreme caution. Better still, do not use.
    ''' </remarks>
    Public Sub SetStage(ByVal gID As Guid, ByVal objStage As clsProcessStage)
        Dim iStage As Integer
        iStage = GetStageIndex(gID)
        If iStage <> -1 Then
            Dim OldStage As clsProcessStage = mSchema.Stages(iStage)
            mSchema.Stages(iStage) = objStage
            RaiseEvent StageModified(objStage, OldStage)
        End If
    End Sub

    Public Function GetStages(includedTypes As IEnumerable(Of StageTypes), excludedTypes As IEnumerable(Of StageTypes)) As List(Of clsProcessStage)
        Return New List(Of clsProcessStage)(mSchema.Stages.Where(
            Function(s)
                Return (includedTypes Is Nothing OrElse includedTypes.Contains(s.StageType)) AndAlso (excludedTypes Is Nothing OrElse Not excludedTypes.Contains(s.StageType))
            End Function))
    End Function

    Public Function GetStages() As List(Of clsProcessStage)
        Return New List(Of clsProcessStage)(mSchema.Stages)
    End Function

    Public Function GetConflictingDataStages(stage As clsDataStage) As clsDataStage
        Return mSchema.GetConflictingDataStages(stage)
    End Function

    Public Function GetStagesWithBreakpoint() As List(Of clsProcessStage)
        Return mSchema.Stages.Where(Function(s) s.HasBreakPoint).ToList()
    End Function

    Public Function GetStages(ByVal type As StageTypes) As List(Of clsProcessStage)
        Return mSchema.GetStages(type)
    End Function

    Public Function GetStages(Of StageType As clsProcessStage)() As List(Of StageType)
        Return mSchema.GetStages(Of StageType)().ToList()
    End Function

    Public Function GetStages(Of StageType As clsProcessStage)(ByVal subsheetId As Guid) As IEnumerable(Of StageType)
        Return mSchema.GetStages(Of StageType)(subsheetId)
    End Function

    ''' <summary>
    ''' Overloaded function to get the array of stages with a given name.
    ''' </summary>
    ''' <param name="stageName">The name of the stages</param>
    ''' <returns>An array of clsProcessStage objects, or Nothing if none were
    ''' found.</returns>
    Public Function GetStages(ByVal stageName As String) As clsProcessStage()
        Dim i As Integer
        Dim temp As New List(Of clsProcessStage)
        Dim stages As clsProcessStage()

        For i = 0 To mSchema.Stages.Count - 1
            If stageName = mSchema.Stages(i).GetName() Then
                temp.Add(mSchema.Stages(i))
            End If
        Next

        If temp.Count > 0 Then
            stages = temp.ToArray()
            Return stages
        End If

        Return Nothing

    End Function

    ''' <summary>
    ''' Overloaded function to get the array of stages with a given name and 
    ''' of a particular type or types enumerated by clsProcessStage.StageTypes
    ''' </summary>
    ''' <param name="name">The name of the stages</param>
    ''' <param name="stgTypes">The type of stages to include (this can
    ''' be more than one, e.g. "StageTypes.Data Or StageTypes.Collection"),
    ''' or StageTypes.Undefined to return all stages.
    ''' </param>
    ''' <returns>A collection of clsProcessStage objects</returns>
    Public Function GetStages(ByVal name As String, ByVal ParamArray stgTypes() As StageTypes) As ICollection(Of clsProcessStage)
        Dim types As New clsSet(Of StageTypes)(stgTypes)

        Dim stageList As New List(Of clsProcessStage)
        For i As Integer = 0 To mSchema.Stages.Count - 1
            Dim s As clsProcessStage = mSchema.Stages(i)
            If name = s.GetName() AndAlso (types.Count = 0 OrElse types.Contains(s.StageType)) Then
                stageList.Add(s)
            End If
        Next
        Return stageList
    End Function

    Public Function GetStagesBySubsheetAndType(ByVal subsheetid As Guid, ByVal type As StageTypes) As List(Of clsProcessStage)
        If type = StageTypes.Undefined Then Return mSchema.Stages
        Return mSchema.GetStagesByTypeAndSubSheet(type, subsheetid).ToList()
    End Function

    ''' <summary>
    ''' Gets a list of stages located on the specified page.
    ''' </summary>
    ''' <param name="PageID">The ID of the page of interest</param>
    ''' <returns>Returns a List of stages located on the page of interest.</returns>
    Public Function GetStages(ByVal PageID As Guid) As List(Of clsProcessStage)
        Dim lst As New List(Of clsProcessStage)
        For Each st As clsProcessStage In mSchema.Stages
            If st.GetSubSheetID.Equals(PageID) Then lst.Add(st)
        Next
        Return lst
    End Function

    Public Function GetStage(ByVal index As Integer) As clsProcessStage
        Return mSchema.GetStageByIndex(index)
    End Function

    Public Overridable Function GetStage(ByVal gID As Guid) As clsProcessStage
        Return mSchema.GetStageById(gID)
    End Function

    Public Function GetStage(ByVal sName As String) As clsProcessStage
        Return mSchema.GetStageByName(sName)
    End Function

    Friend Function GetDataAndCollectionStagesByName(name As String) As clsDataStage
        Return mSchema.GetDataAndCollectionStageByName(name)
    End Function

    Public Function GetCollectionStage(ByVal name As String, ByVal scopeStg As clsProcessStage, ByRef outOfScope As Boolean) As clsCollectionStage
        Return mSchema.GetCollectionStage(name, scopeStg, outOfScope)
    End Function

    Public Function GetDataStage(ByVal sName As String, ByVal objScopeStage As clsProcessStage, ByRef bOutOfScope As Boolean) As clsDataStage
        Return mSchema.GetDataStage(sName, objScopeStage, bOutOfScope)
    End Function

    Private Function GetStageID(ByVal iStageIndex As Integer) As Guid
        Return If(mSchema.GetStageByIndex(iStageIndex)?.GetStageID(), Guid.Empty)
    End Function

    Public Function GetStageName(ByVal gID As Guid) As String
        Return If(mSchema.GetStageById(gID)?.Name, "")
    End Function

    Public Function GetStageIndex(ByVal gID As Guid) As Integer
        Return mSchema.GetStageIndex(gID)
    End Function

    Public Function GetGroupStages(ByVal GroupID As Guid) As List(Of clsGroupStage)
        Return mSchema.GetGroupStages(GroupID)
    End Function

    ''' <summary>
    ''' Delete the subsheet with the given ID. Since the sheet has been
    ''' deleted a new sheet must be supplied so that the active sheet is
    ''' not invalid.
    ''' </summary>
    ''' <param name="gID">The ID of the subsheet</param>
    ''' <param name="gNewSheetID">The ID of the sheet to be made active after it has been deleted. 
    ''' If guid.empty is provided then the main page / init page will be used</param>
    Public Sub DeleteSubSheet(ByVal gID As Guid, ByVal gNewSheetID As Guid)

        'Don't allow the main page to be deleted.
        If ProcessType = DiagramType.Process And gID.Equals(GetMainPage.ID) Then Return

        'Remove all stages associated with the subsheet...
        Dim bIncomplete = True
        While bIncomplete
            bIncomplete = False
            For Each objStage In mSchema.Stages
                If objStage.GetSubSheetID().Equals(gID) Then
                    RemoveStage(objStage)
                    bIncomplete = True
                    Exit For
                ElseIf objStage.StageType() = StageTypes.SubSheet Then
                    Dim objSheetRef As clsSubSheetRefStage = CType(objStage, clsSubSheetRefStage)
                    If objSheetRef.ReferenceId.Equals(gID) Then
                        objSheetRef.ReferenceId = Guid.Empty
                    End If
                End If
            Next
        End While

        'Remove the subsheet record itself...
        For Each objSub As clsProcessSubSheet In SubSheets
            If objSub.ID.Equals(gID) Then
                SubSheets.Remove(objSub)
                Exit For
            End If
        Next

        'Update the Active Sub sheet to be the new sheet
        'If no new sheet is provided use the main page / init page
        If gNewSheetID.Equals(Guid.Empty) Then
            mActiveSubsheetId = GetMainPage.ID
        Else
            mActiveSubsheetId = gNewSheetID
        End If

    End Sub

    Public Function GetSubSheetByID(ByVal SubsheetID As Guid) As clsProcessSubSheet
        Return mSchema.GetSubSheetByID(SubsheetID)
    End Function

    ''' <summary>
    ''' Gets the cleanup page for this process, if one exists.
    ''' </summary>
    ''' <remarks>A cleanup page will only exist if the process type is Object. You
    ''' should not call this method unless the process type is object.</remarks>
    Public Function GetCleanupPage() As clsProcessSubSheet
        Return mSchema.GetCleanUpPage()
    End Function

    Public Function GetNormalSheets() As IEnumerable(Of clsProcessSubSheet)
        Return mSchema.GetNormalSheets()
    End Function

    Public Function HasExtraSheets() As Boolean
        Return mSchema.HasExtraSheets()
    End Function

    Public ReadOnly Property SubSheets As List(Of clsProcessSubSheet)
        Get
            Return mSchema.SubSheets
        End Get
    End Property

    Public Function GetSubSheetID(ByVal sName As String) As Guid
        Return mSchema.GetSubSheetID(sName)
    End Function

    Public Function GetSubSheetIDSafeName(ByVal sName As String) As Guid
        Return mSchema.GetSubSheetIDWithSafeName(sName)
    End Function

    Public Function GetMainPage() As clsProcessSubSheet
        Return mSchema.GetMainPage()
    End Function

    Public Function RenameSubSheet(ByVal gID As Guid, ByVal sNewName As String, ByRef sErr As String) As Boolean
        Return mSchema.RenameSubSheet(gID, sNewName, sErr)
    End Function

    Public Sub SetSubSheetOrder(ByVal subSheetIDs As ICollection(Of Guid))
        mSchema.SetSubSheetOrder(subSheetIDs)
    End Sub

    Public Function GetSubSheetIndex(ByVal gSheetID As Guid) As Integer
        Return mSchema.GetSubSheetIndex(gSheetID)
    End Function

    Public Function GetStartStage() As Guid
        'Determine the appropriate page to look at - if it's a Business Object, we use the
        'currently viewed page, otherwise it's the main page...
        Dim pageid = If(ProcessType = DiagramType.Object, mActiveSubsheetId, GetMainPage.ID)
        Return mSchema.GetSubSheetStartStage(pageid)
    End Function

    Public Function GetSubSheetStartStage(ByVal gSubID As Guid) As Guid
        Return mSchema.GetSubSheetStartStage(gSubID)
    End Function

    Public Function GetStageByTypeAndSubSheet(ByVal stagetype As StageTypes, ByVal gSubSheetID As Guid) As clsProcessStage
        Return mSchema.GetStageByTypeAndSubSheet(stagetype, gSubSheetID)
    End Function

    Public Function GetStageByIdAndSubSheet(ByVal id As Guid, ByVal gSubSheetID As Guid) As clsProcessStage
        Return mSchema.GetStagesByIdAndSubSheet(id, gSubSheetID)
    End Function

    Public Function GetEndStage() As Guid
        Return mSchema.GetSubSheetEndStage(Guid.Empty)
    End Function

    Public Function GetSubSheetEndStage(ByVal gSubID As Guid) As Guid
        Return mSchema.GetSubSheetEndStage(gSubID)
    End Function

    Friend Function UpdateNextStage(ByRef runStageID As Guid, ByVal linkTp As LinkType, ByRef sErr As String) As Boolean
        Return mSchema.UpdateNextStage(runStageID, linkTp, sErr)
    End Function

    Friend Function GetNextStage(ByVal gCurrentStage As Guid, ByVal linkTp As LinkType) As Guid
        Return mSchema.GetNextStage(gCurrentStage, linkTp)
    End Function

    ''' <summary>
    ''' Returns true if something copyable (to the clipboard!) is
    ''' selected.
    ''' </summary>
    ''' <returns></returns>
    Public Function CanCopy(ByRef sErr As String) As Boolean
        If SelectionContainer.Count = 0 Then
            'No selection, can't copy...
            Return False
        Else
            'Ok then, we can only copy if we have a stage somewhere
            'in the selection. Copying a bunch of links would be
            'meaningless...
            Dim ps As clsProcessSelection
            Dim bStageFound As Boolean
            For Each ps In SelectionContainer
                If ps.mtType = clsProcessSelection.SelectionType.Stage Then
                    bStageFound = True
                End If
            Next

            If bStageFound Then
                'gather a list of stages in the selection which are part of a group
                Dim GroupsToSelect As New List(Of Guid)
                Dim GroupID As Guid
                For Each Selection As clsProcessSelection In SelectionContainer
                    Dim StageToTest As clsProcessStage = GetStage(Selection.mgStageID)
                    If TypeOf StageToTest Is clsGroupStage Then
                        GroupID = CType(StageToTest, clsGroupStage).GetGroupID
                        If Not GroupID.Equals(Guid.Empty) Then
                            If Not GroupsToSelect.Contains(GroupID) Then GroupsToSelect.Add(GroupID)
                        End If
                    End If
                Next
                'check that all members of all such groups are contained
                'in the selection
                Dim StagesMissingFromSelection As New Collection
                Dim bSelectionContainsStage As Boolean
                For Each grpID As Guid In GroupsToSelect
                    For Each st As clsProcessStage In mSchema.Stages
                        If TypeOf st Is clsGroupStage Then
                            If CType(st, clsGroupStage).GetGroupID.Equals(grpID) Then
                                bSelectionContainsStage = False
                                For Each selection As clsProcessSelection In SelectionContainer
                                    If selection.mgStageID.Equals(st.GetStageID) Then
                                        bSelectionContainsStage = True
                                        Exit For
                                    End If
                                Next
                                If Not bSelectionContainsStage Then StagesMissingFromSelection.Add(st)
                            End If
                        End If
                    Next
                Next

                If StagesMissingFromSelection.Count > 0 Then
                    sErr =
                    My.Resources.Resources.clsProcess_YouHaveSelectedOneOrMoreStagesWhichAreGroupedToAnotherStageForExampleALoopStart
                    Return False
                End If

                Return True
            End If

            sErr = My.Resources.Resources.clsProcess_CanNotCopyOrCutStageDataBecauseNoStagesAreSelected
            Return False
        End If
    End Function

    ''' <summary>
    ''' Returns true if something cutable (to the clipboard!) is
    ''' selected.
    ''' </summary>
    ''' <returns></returns>
    Public Function CanCut(ByRef sErr As String) As Boolean
        'For now the only requirements for CanCut are identical to CanCopy
        Return CanCopy(sErr)
    End Function

    ''' <summary>
    ''' Returns true if something can be pasted from the clipboard.
    ''' </summary>
    ''' <param name="bAsSheet">Indicates pasting subsheets into a process rather than
    ''' pasting a selection of stages onto the canvass</param>
    ''' <returns>True if the clipboard contents can be pasted</returns>
    Public Function CanPaste(Optional ByVal bAsSheet As Boolean = False) As Boolean
        Try
            'Get the data from the clipboard and see if it is something
            'we might be able to paste into a process...
            Dim data As IDataObject = Clipboard.GetDataObject()
            If data Is Nothing Then Return False

            Dim xml As String = CStr(data.GetData(DataFormats.StringFormat, True))
            If xml Is Nothing Then Return False

            Return CanPasteXML(xml, bAsSheet)
        Catch
            'If we fail to get the data object to paste
            'then surely we cannot paste...
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Returns true if the given XML can be pasted. Pasting can either
    ''' be onto the canvass (in which case the clipboard contents must contain a stage
    ''' selection only), or as one or more sheets onto the process (inwhich case the 
    ''' clipboard contents must contain at least one subsheet).
    ''' </summary>
    ''' <param name="xml">The XML representing the 'clipboard fragment'. This
    ''' should have been previously generated by GetSelectionXML().</param>
    ''' <param name="bAsSheet">Indicates subsheets are expected</param>
    ''' <returns>True if the XML is can be pasted</returns>
    Public Function CanPasteXML(ByVal xml As String, Optional ByVal bAsSheet As Boolean = False) As Boolean
        ' A quick check first - if it's not XML, then it can't be pasted;
        ' If the root element is not <process> then it can't be pasted.
        ' This is here just to try and discard obviously non-process-xml data without
        ' attempting to parse a full document or even throw an exception
        xml = xml.Trim()
        If Not xml.StartsWith("<process") Then Return False

        'We need to give this the set of external objects even though it is only
        'parsing the proc. Otherwise mExternalObjectsInfo will be set to nothing
        'causing null reference exceptions.
        Try
            Using proc As clsProcess = clsProcess.FromXml(ExternalObjectsInfo, xml, False, False)
                If proc Is Nothing Then
                    ' Cannot paste if process could not be determined from XML
                    Return False
                ElseIf Not proc.Name.StartsWith("__selection__") Then
                    ' Cannot paste if XML wasn't copied from a process
                    Return False
                ElseIf bAsSheet AndAlso proc.ProcessType <> ProcessType Then
                    ' Cannot paste object sheet into process and vice-versa
                    Return False
                ElseIf Not bAsSheet AndAlso proc.HasExtraSheets Then
                    ' Cannot paste subsheet selection onto canvas
                    Return False
                ElseIf bAsSheet AndAlso Not proc.HasExtraSheets Then
                    ' Cannot paste stage selection as new subsheet
                    Return False
                End If
                Return True
            End Using
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Clear current selection
    ''' </summary>
    Public Sub SelectNone()
        mSelectionOffsetStage = Nothing
        If SelectionContainer Is Nothing Then
            SelectionContainer = New clsProcessSelectionContainer()
        End If
        SelectionContainer.ClearAllSelections()
        mSelectionCorner = 0
    End Sub

    ''' <summary>
    ''' Select a particular stage
    ''' </summary>
    ''' <param name="iStageIndex">The index of the stage to select</param>
    ''' <param name="bExtend">True to extent the current selection to include
    ''' this stage. Otherwise the stage will become the new selection.</param>
    Public Sub SelectStage(ByVal iStageIndex As Integer, ByVal bExtend As Boolean)
        If bExtend Then
            If IsStageSelected(GetStageID(iStageIndex)) Then Return
        Else
            SelectNone()
        End If
        Dim ps As New clsProcessSelection
        ps.mtType = clsProcessSelection.SelectionType.Stage
        ps.mgStageID = GetStageID(iStageIndex)
        SelectionContainer.Add(ps)
    End Sub

    ''' <summary>
    ''' Deselect a particular stage...
    ''' </summary>
    ''' <param name="gStageID">The ID of the stage to be removed from
    ''' the selection.</param>
    Public Sub DeselectStage(ByVal gStageID As Guid)
        Dim ps As clsProcessSelection
        For i As Integer = 0 To SelectionContainer.Count - 1
            ps = SelectionContainer.Item(i)
            If ps.mgStageID.Equals(gStageID) Then
                SelectionContainer.Remove(i)
                Exit For
            End If
        Next
    End Sub

    ''' <summary>
    ''' Deselect a particular link...
    ''' </summary>
    ''' <param name="gStageID"></param>
    ''' <param name="sLinkType"></param>
    Public Sub DeselectLink(ByVal gStageID As Guid, ByVal sLinkType As String)
        Dim i As Integer
        Dim ps As New clsProcessSelection
        For i = 0 To SelectionContainer.Count - 1
            ps = CType(SelectionContainer.PrimarySelection, clsProcessSelection)
            If ps.mgStageID.Equals(gStageID) And ps.msLinkType = sLinkType Then
                SelectionContainer.Remove(i)
                Exit Sub
            End If
        Next
    End Sub

    ''' <summary>
    ''' Determines if a particular stage is selected.
    ''' </summary>
    ''' <param name="gStageID">The ID of the stage for which
    ''' selection is to be tested.</param>
    ''' <returns>Returns true if the specified stage is selected.</returns>
    Public Function IsStageSelected(ByVal gStageID As Guid) As Boolean
        Dim ps As clsProcessSelection
        For Each ps In SelectionContainer
            If ps.mtType = clsProcessSelection.SelectionType.Stage And ps.mgStageID.Equals(gStageID) Then
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Returns true if the given link is selected.
    ''' </summary>
    ''' <param name="gStageID"></param>
    ''' <param name="sLinkType"></param>
    ''' <returns></returns>
    Public Function IsLinkSelected(ByVal gStageID As Guid, ByVal sLinkType As String) As Boolean
        Dim ps As clsProcessSelection
        For Each ps In SelectionContainer
            If ps.mtType = clsProcessSelection.SelectionType.Link And ps.mgStageID.Equals(gStageID) And ps.msLinkType = sLinkType Then
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Used to determine whether the choice node at the given index is selected.
    ''' </summary>
    ''' <param name="gStageID">The stage id of the choice start stage.</param>
    ''' <param name="iChoiceindex">The index of the choice</param>
    ''' <returns>True if the node is selected</returns>
    Public Function IsChoiceNodeSelected(ByVal gStageID As Guid, ByVal iChoiceindex As Integer) As Boolean
        Dim ps As clsProcessSelection
        For Each ps In SelectionContainer
            If ps.mtType = clsProcessSelection.SelectionType.ChoiceNode AndAlso ps.mgStageID.Equals(gStageID) AndAlso ps.miChoiceIndex = iChoiceindex Then
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Used to determine whether the choice node link at the given index is selected
    ''' </summary>
    ''' <param name="gStageID">The stage id of the choice start stage.</param>
    ''' <param name="iChoiceindex">The index of the choice</param>
    ''' <returns>True if the link is selected</returns>
    Public Function IsChoiceNodeLinkSelected(ByVal gStageID As Guid, ByVal iChoiceindex As Integer) As Boolean
        Dim ps As clsProcessSelection
        For Each ps In SelectionContainer
            If ps.mtType = clsProcessSelection.SelectionType.ChoiceLink AndAlso ps.mgStageID.Equals(gStageID) AndAlso ps.miChoiceIndex = iChoiceindex Then
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Attempt selection of a resize handle at the given
    ''' position. If there is a resize handle, the current
    ''' selection information is updated to note that a corner
    ''' is being manipulated.
    ''' </summary>
    ''' <param name="sngX">The world X position to check</param>
    ''' <param name="sngY">The world Y position to check</param>
    ''' <param name="sngRadius">The radius around the handle
    ''' for detection purposes</param>
    ''' <returns>True if a resize handle was present</returns>
    Public Function SelectResizeHandles(ByVal sngX As Single, ByVal sngY As Single, ByVal sngRadius As Single) As Boolean

        'Resize handles only exist when a stage is selected,
        'so don't bother looking any further if that is not
        'the case...
        If GetSelectionType() <> clsProcessSelection.SelectionType.Stage Then Exit Function

        Dim objStage As clsProcessStage
        objStage = GetStage(CType(SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID)

        If objStage Is Nothing Then Return False

        'Get information we need about the stage...
        Dim sngWidth As Single, sngHeight As Single
        Dim sngSX As Single, sngSY As Single
        Select Case objStage.StageType
            'Do not allow anchors to be resized
            Case StageTypes.Anchor
                Return False
            Case StageTypes.Block
                sngWidth = objStage.GetDisplayWidth / 2
                sngHeight = objStage.GetDisplayHeight / 2
                sngSX = objStage.GetDisplayX() + sngWidth
                sngSY = objStage.GetDisplayY() + sngHeight
            Case Else
                sngWidth = -objStage.GetDisplayWidth() / 2
                sngHeight = -objStage.GetDisplayHeight() / 2
                sngSX = objStage.GetDisplayX()
                sngSY = objStage.GetDisplayY()
        End Select

        'Check each corner in turn, starting from the top left...
        Dim sngCX As Single, sngCY As Single
        Dim iCorner As Integer
        For iCorner = 1 To 4
            'Calculate the position of the corner...
            sngCX = sngSX + sngWidth
            sngCY = sngSY + sngHeight

            'See if the test position is within range of
            'the corner...
            If Math.Abs(sngCX - sngX) < sngRadius And Math.Abs(sngCY - sngY) < sngRadius Then

                'Mark this corner (resize handle) as the one we
                'are using, keeping track of the offset we clicked
                'at so we 'hold' the handle by that point when
                'dragging...
                mSelectionOffsetStage = objStage
                mSelectionOffsetX = sngX - sngCX
                mSelectionOffsetY = sngY - sngCY
                mSelectionCorner = iCorner
                Return True

            End If

            'Adjust offsets for next corner, moving round
            'clockwise...
            Select Case iCorner
                Case 1, 3
                    sngWidth = -sngWidth
                Case 2
                    sngHeight = -sngHeight
            End Select

        Next

        Return False
    End Function

    ''' <summary>
    ''' Get the stage at the given position, on the specified subsheet.
    ''' </summary>
    ''' <param name="sngX">The X position</param>
    ''' <param name="sngY">The Y position</param>
    ''' <param name="SubsheetID">The subsheet on which to search for stages.</param>
    ''' <returns>A reference to the clsProcessStage representing the
    ''' stage at the given position, or Nothing if there isn't a stage
    ''' there.</returns>
    Public Function GetStageAtPosition(ByVal sngX As Single, ByVal sngY As Single, ByVal SubsheetID As Guid) As clsProcessStage
        For Each Stage As clsProcessStage In mSchema.Stages
            If Stage.IsAtPosition(sngX, sngY, SubsheetID) Then
                Return Stage
            End If
        Next
        Return Nothing
    End Function

    Private Function GetStageAtPosition(ByVal Location As PointF, ByVal SubsheetID As Guid) As clsProcessStage
        Return GetStageAtPosition(Location.X, Location.Y, SubsheetID)
    End Function

    ''' <summary>
    ''' Get the stage at the given position, on the currently active subsheet.
    ''' </summary>
    ''' <param name="Location">The location of the stage sought.</param>
    ''' <returns>A reference to the clsProcessStage representing the
    ''' stage at the given position, or Nothing if there isn't a stage
    ''' there.</returns>
    Public Function GetStageAtPosition(ByVal Location As PointF) As clsProcessStage
        Return GetStageAtPosition(Location, mActiveSubsheetId)
    End Function

    ''' <summary>
    ''' Gets the linkable object at the specified location, on the specified
    ''' subsheet.
    ''' </summary>
    ''' <param name="Location">The location of interest, in world
    ''' coordinates.</param>
    ''' <param name="SubsheetID">The subsheet on which to search for 
    ''' linkable objects.</param>
    ''' <returns>Returns the linkable object at the specified location,
    ''' if any exists. If none exists then returns null reference.</returns>
    ''' <remarks>The linkable object may be one of many things, including
    ''' a stage object or a choice node.</remarks>
    Public Function GetLinkableObjectAtPosition(ByVal Location As PointF, ByVal SubsheetID As Guid) As ILinkable
        Dim stg = GetStageAtPosition(Location, SubsheetID)
        Dim LinkableStage As ILinkable = TryCast(stg, ILinkable)
        If LinkableStage IsNot Nothing Then
            If CanCreateLinkFrom(stg) Then Return LinkableStage
        Else
            'We are only looking at choice nodes and wait nodes.
            'We can use fact that waitstart inherits from choicestart
            For Each ChoiceStart As clsChoiceStartStage In GetStages(Of clsChoiceStartStage)()
                If ChoiceStart.GetSubSheetID = SubsheetID Then
                    For Each Node As clsChoice In ChoiceStart.Choices
                        If Node.DisplayBounds.Contains(Location) Then Return Node
                    Next
                End If
            Next
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the linkable object at the specified location, on the currently
    ''' active subsheet.
    ''' </summary>
    ''' <param name="Location">The location of interest, in world
    ''' coordinates.</param>
    ''' <returns>Returns the linkable object at the specified location,
    ''' if any exists. If none exists then returns null reference.</returns>
    ''' <remarks>The linkable object may be one of many things, including
    ''' a stage object or a choice node.</remarks>
    Public Function GetLinkableObjectAtPosition(ByVal Location As PointF) As ILinkable
        Return GetLinkableObjectAtPosition(Location, mActiveSubsheetId)
    End Function

    ''' <summary>
    ''' Get the block that contains the given stage.
    ''' </summary>
    ''' <param name="st">The stage to check.</param>
    ''' <returns>A reference to the containing block, or Nothing if there isn't one.
    ''' </returns>
    Public Function GetBlockContainingStage(ByVal st As clsProcessStage) As clsBlockStage
        Dim blocks As List(Of clsProcessStage)
        blocks = GetStagesBySubsheetAndType(st.GetSubSheetID(), StageTypes.Block)
        For Each block As clsProcessStage In blocks
            Dim bounds As RectangleF = block.GetDisplayBounds()
            If st.IsInRegion(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom, st.GetSubSheetID()) Then
                Return CType(block, clsBlockStage)
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Select everything within the given rectangular region.
    ''' </summary>
    ''' <param name="sngSX">Start X position</param>
    ''' <param name="sngSY">Start Y position</param>
    ''' <param name="sngFX">Finish X position</param>
    ''' <param name="sngFY">Finish Y position</param>
    ''' <param name="gSubSheetID">The subsheet ID</param>
    ''' <param name="bExtendSelection">True to extend the selection,
    ''' False to replace it</param>
    Public Sub SelectRegion(ByVal sngSX As Single, ByVal sngSY As Single, ByVal sngFX As Single, ByVal sngFY As Single, ByVal gSubSheetID As Guid, ByVal bExtendSelection As Boolean)
        Dim ChangeMade As Boolean = False
        Try
            mSuppressSelectionEvents = True

            If Not bExtendSelection Then SelectNone()
            Dim i As Integer
            For i = 0 To mSchema.Stages.Count - 1
                Dim s As clsProcessStage = mSchema.Stages(i)

                Dim bEntirely As Boolean = (s.StageType = StageTypes.Block)

                If s.IsInRegion(sngSX, sngSY, sngFX, sngFY, gSubSheetID, bEntirely) Then
                    SelectStage(i, True)
                    ChangeMade = True
                End If

            Next
        Finally
            mSuppressSelectionEvents = False
        End Try

        If ChangeMade Then
            RaiseEvent SelectionChanged()
        End If
    End Sub

    ''' <summary>
    ''' Select all stages on a subsheet.
    ''' </summary>
    ''' <param name="gSubSheetID">The subsheet id</param>
    Public Sub SelectAll(ByVal gSubSheetID As Guid)
        Try
            'We suppress selection events here because otherwise an event is raised
            'for each and every stage - we prefer to raise a single event instead
            '(see below).
            mSuppressSelectionEvents = True

            SelectNone()

            For i As Integer = 0 To mSchema.Stages.Count - 1
                If mSchema.Stages(i).GetSubSheetID.Equals(gSubSheetID) Then
                    SelectStage(i, True)
                End If
            Next
        Finally
            mSuppressSelectionEvents = False
        End Try

        RaiseEvent SelectionChanged()
    End Sub

    ''' <summary>
    ''' Select the object at the given position. It is assumed that this position is
    ''' referring to the currently viewed sheet. Optionally, the object can be added
    ''' to the selection rather than replacing it - in this case, set bExtendSelection
    ''' to True.
    ''' </summary>
    ''' <param name="Position">The position, in world coordinates.</param>
    ''' <param name="bExtendSelection">True to extend the current selection, False to
    ''' replace it.</param>
    ''' <param name="bKeepIfAlreadySelected">True to keep the current selection if
    ''' the item at the position is already selected.</param>
    Public Sub SelectAtPosition(ByVal Position As PointF, ByVal bExtendSelection As Boolean, ByVal bKeepIfAlreadySelected As Boolean)

        Dim ps As clsProcessSelection = Nothing

        'See if there is a stage at this position...
        For Each st As clsProcessStage In mSchema.Stages
            If st.IsAtPosition(Position.X, Position.Y, mActiveSubsheetId) Then
                ps = New clsProcessSelection
                ps.mtType = clsProcessSelection.SelectionType.Stage
                ps.mgStageID = st.GetStageID()
                If Not bExtendSelection Then
                    If bKeepIfAlreadySelected AndAlso IsStageSelected(ps.mgStageID) Then
                        ps = Nothing
                    Else
                        SelectNone()
                    End If
                Else
                    If IsStageSelected(ps.mgStageID) Then
                        If Not bKeepIfAlreadySelected Then
                            DeselectStage(ps.mgStageID)
                        End If
                        ps = Nothing
                    End If
                End If
                If Not ps Is Nothing Then
                    'We want this stage to become the new primary
                    'selection, so make sure it goes in at the start
                    'of the collection...
                    If SelectionContainer.Count <> 0 Then
                        SelectionContainer.AddPrimarySelection(ps)
                    Else
                        SelectionContainer.Add(ps)
                    End If
                End If

                'Calculate the offset of the position from
                'the centre of the object, so that later
                'calls to GetSelectionOffset can work.
                mSelectionOffsetStage = st
                mSelectionOffsetX = Position.X - st.GetDisplayX()
                mSelectionOffsetY = Position.Y - st.GetDisplayY()
                mSelectionCorner = 0

                Exit Sub
            End If
        Next

        'See if there is a link...
        Dim linkType As String = Nothing
        For Each objStage As clsProcessStage In mSchema.Stages
            If objStage.IsLinkAtPosition(mActiveSubsheetId, Position.X, Position.Y, linkType) Then
                ps = New clsProcessSelection
                ps.mtType = clsProcessSelection.SelectionType.Link
                ps.mgStageID = objStage.GetStageID()
                ps.msLinkType = linkType
                If Not bExtendSelection Then
                    If bKeepIfAlreadySelected AndAlso IsLinkSelected(ps.mgStageID, ps.msLinkType) Then
                        ps = Nothing
                    Else
                        SelectNone()
                    End If
                    mSelectionOffsetStage = Nothing
                    mSelectionOffsetX = 0
                    mSelectionOffsetY = 0
                    mSelectionCorner = 0
                Else
                    If IsLinkSelected(ps.mgStageID, ps.msLinkType) Then
                        DeselectLink(ps.mgStageID, ps.msLinkType)
                        ps = Nothing
                    End If
                End If
                If Not ps Is Nothing Then SelectionContainer.Add(ps)
                Exit Sub
            End If
        Next

        'See if there is a choice node
        For Each cst As clsChoiceStartStage In GetStages(Of clsChoiceStartStage)(mActiveSubsheetId)
            For nodeIndex As Integer = 0 To cst.Choices.Count - 1
                Dim node As clsChoice = cst.Choices(nodeIndex)
                If node.DisplayBounds.Contains(Position) Then
                    ps = New clsProcessSelection
                    ps.mtType = clsProcessSelection.SelectionType.ChoiceNode
                    ps.miChoiceIndex = nodeIndex
                    ps.mgStageID = cst.GetStageID
                    If Not bExtendSelection Then
                        If bKeepIfAlreadySelected AndAlso IsChoiceNodeSelected(ps.mgStageID, nodeIndex) Then
                            ps = Nothing
                        Else
                            SelectNone()
                        End If
                    Else
                        If IsChoiceNodeSelected(ps.mgStageID, nodeIndex) Then
                            If Not bKeepIfAlreadySelected Then
                                DeselectStage(ps.mgStageID)
                            End If
                            ps = Nothing
                        End If
                    End If
                    If Not ps Is Nothing Then
                        'We want this stage to become the new primary
                        'selection, so make sure it goes in at the start
                        'of the collection...
                        If SelectionContainer.Count <> 0 Then
                            SelectionContainer.AddPrimarySelection(ps)
                        Else
                            SelectionContainer.Add(ps)
                        End If
                    End If
                    mSelectionOffsetStage = cst
                    Exit Sub
                Else
                    If node.LinkTo <> Guid.Empty Then
                        Dim onTrueStage As clsProcessStage = GetStage(node.LinkTo)
                        If cst.IsLinkAtPosition(Position, node.DisplayBounds, onTrueStage.GetDisplayBounds) Then
                            ps = New clsProcessSelection
                            ps.mtType = clsProcessSelection.SelectionType.ChoiceLink
                            ps.mgStageID = cst.GetStageID
                            ps.msLinkType = linkType
                            ps.miChoiceIndex = nodeIndex
                            If Not bExtendSelection Then
                                If bKeepIfAlreadySelected AndAlso IsChoiceNodeSelected(ps.mgStageID, nodeIndex) Then
                                    ps = Nothing
                                Else
                                    SelectNone()
                                End If
                            Else
                                If IsChoiceNodeSelected(ps.mgStageID, nodeIndex) Then
                                    If Not bKeepIfAlreadySelected Then
                                        DeselectStage(ps.mgStageID)
                                    End If
                                    ps = Nothing
                                End If
                            End If
                            If Not ps Is Nothing Then
                                'We want this stage to become the new primary
                                'selection, so make sure it goes in at the start
                                'of the collection...
                                If SelectionContainer.Count <> 0 Then
                                    SelectionContainer.AddPrimarySelection(ps)
                                Else
                                    SelectionContainer.Add(ps)
                                End If
                            End If
                            Exit Sub
                        End If
                    End If
                End If
            Next
        Next

        If Not bExtendSelection Then SelectNone()
        mSelectionOffsetStage = Nothing
    End Sub

    Public Sub ResetChoiceNodePositions(ByVal objChoiceStart As clsChoiceStartStage, ByVal objChoiceEnd As clsChoiceEndStage)

        Dim dx As Single = objChoiceStart.GetDisplayX - objChoiceEnd.GetDisplayX
        Dim dy As Single = objChoiceStart.GetDisplayY - objChoiceEnd.GetDisplayY
        Dim count As Integer = objChoiceStart.Choices.Count
        Dim length As Single = CSng(Math.Sqrt(dx * dx + dy * dy))
        Dim offset As Single = length / (count + 1)
        Dim i As Single = offset
        For Each objChoice As clsChoice In objChoiceStart.Choices
            objChoice.Distance = i
            i += offset
        Next
    End Sub

    ''' <summary>
    ''' This returns, based on the last SelectAtPosition, the
    ''' offset from the centre of the object of the actual
    ''' position given.
    ''' </summary>
    ''' <param name="objStage">Receives the stage this relates to</param>
    ''' <param name="sngX">Receives the X position</param>
    ''' <param name="sngY">Receives the Y position</param>
    ''' <param name="iCorner">Receives either 0 if the object
    ''' itself is being dragged, or 1-4 to denote a corner
    ''' being dragged, numbered clockwise from the top left.
    ''' </param>
    Public Sub GetSelectionOffset(ByRef objStage As clsProcessStage, ByRef sngX As Single, ByRef sngY As Single, ByRef iCorner As Integer)
        objStage = mSelectionOffsetStage
        sngX = mSelectionOffsetX
        sngY = mSelectionOffsetY
        iCorner = mSelectionCorner
    End Sub

    ''' <summary>
    ''' Get the current selection type.
    ''' </summary>
    ''' <returns>One of Stage, Link, None or Multiple. Stage
    ''' and Link are returned only when a single item of the
    ''' respective type is selected. Multiple is returned when more
    ''' than one item is selected, regardless of type, and None is
    ''' returned when nothing at all is selected.
    ''' </returns>
    Public Function GetSelectionType() As clsProcessSelection.SelectionType
        If SelectionContainer.Count = 0 Then Return clsProcessSelection.SelectionType.None
        If SelectionContainer.Count > 1 Then Return clsProcessSelection.SelectionType.Multiple
        Return CType(SelectionContainer.PrimarySelection, clsProcessSelection).mtType
    End Function

    ''' <summary>
    ''' The current selection. The first item in the collection is always the
    ''' 'primary' selection. NEVER SET THIS DIRECTLY.
    ''' </summary>
    Private WithEvents mSelectionContainer As clsProcessSelectionContainer

    Public Property SelectionContainer As clsProcessSelectionContainer
        Get
            Return mSelectionContainer
        End Get
        Private Set(value As clsProcessSelectionContainer)
            If mSelectionContainer IsNot Nothing Then _
                RemoveHandler mSelectionContainer.SelectionChanged, AddressOf HandleSelectionChanged

            mSelectionContainer = value

            If mSelectionContainer IsNot Nothing Then _
                AddHandler mSelectionContainer.SelectionChanged, AddressOf HandleSelectionChanged
        End Set
    End Property

    ''' <summary>
    ''' Handles SelectionChanged event from selection container.
    ''' </summary>
    Private Sub HandleSelectionChanged()
        If Not mSuppressSelectionEvents Then RaiseEvent SelectionChanged()
    End Sub

    Public Function GetSubSheetName(ByVal sheetId As Guid) As String
        Return mSchema.GetSubsheetName(sheetId)
    End Function

    ''' <summary>
    ''' Interprets the supplied string as a process, and adapts it where
    ''' necessary so that it can be pasted into the current process.
    ''' </summary>
    ''' <param name="xml">The xml of the process to be pasted.</param>
    ''' <param name="report">Carries back report information, such as
    ''' details of stages excluded from the clipboard process, which
    ''' are unsuitable for pasting.</param>
    ''' <param name="sErr">Carries back an error message in the event
    ''' of an error, typically when the return value is null.</param>
    ''' <returns>Returns the process suitable for pasting, or nothing
    ''' in the event of an error.</returns>
    ''' <remarks>By adapting the process, we mean dealing with conflicting
    ''' stage ids between this process and the clipboard process, for example.
    ''' </remarks>
    Public Function GetPastableProcess(ByVal xml As String, ByRef report As String,
     ByRef sErr As String) As clsProcess
        ' Get process object from clipboard xml
        Try
            Dim proc As clsProcess =
             FromXml(ExternalObjectsInfo, xml, False, False)
            Return GetPastableProcess(proc, report, sErr)
        Catch
            sErr = My.Resources.Resources.clsProcess_UnableToInterpretClipboardTextAsProcessXml
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Interprets the supplied string as a process, and adapts it where
    ''' necessary so that it can be pasted into the current process.
    ''' </summary>
    ''' <param name="proc">The process object to be pasted.</param>
    ''' <param name="report">Carries back report information, such as
    ''' details of stages excluded from the clipboard process, which
    ''' are unsuitable for pasting.</param>
    ''' <param name="sErr">Carries back an error message in the event
    ''' of an error, typically when the return value is null.</param>
    ''' <returns>Returns the process suitable for pasting, or nothing
    ''' in the event of an error.</returns>
    ''' <remarks>By adapting the process, we mean dealing with conflicting
    ''' stage ids between this process and the clipboard process, for example.
    ''' </remarks>
    Public Function GetPastableProcess(
     ByVal proc As clsProcess, ByRef report As String, ByRef sErr As String) _
     As clsProcess

        'Reject process if it contains no stages
        If proc.GetNumStages() = 0 Then
            sErr = "Nothing to paste - clipboard process has no stages"
            Return Nothing
        End If

        Dim objSrcStage As clsProcessStage
        Dim gNewID As Guid
        Dim htOldGroupIDsToNewGroupIDs As New Hashtable

        'Determines whether new IDs should be given to stages automatically, or whether
        'they should retain IDs (as far as possible, only changing in event of a clash).
        Dim GenerateNewIDs As Boolean = (proc.Name <> "__selection__" & Name)

        'Determines if the clipboard process contains a selection of sheets:
        'If it contains sheets it can only be pasted onto tab header.
        'If it doesn't contain sheets it can only be pasted onto the canvass
        Dim SheetSelection As Boolean = proc.HasExtraSheets

        'Go through the clipboard process and change any stage IDs that
        'already exist in the process we are pasting into. At the same
        'time, any links to those stages must be remapped.
        For i As Integer = 0 To proc.GetNumStages() - 1
            'Get the stage from the clipboard 'process'...
            objSrcStage = proc.GetStage(i)
            'See if we already have a stage with the same ID...
            Dim gOldID As Guid = objSrcStage.GetStageID
            If GenerateNewIDs OrElse (GetStage(gOldID) IsNot Nothing) Then
                'If so, the stage we are pasting will have to have a
                'new ID...
                gNewID = Guid.NewGuid
                objSrcStage.SetStageID(gNewID)
                'new IDs are also needed for the group IDs
                If TypeOf objSrcStage Is clsGroupStage Then
                    Dim objGroupStage As clsGroupStage = CType(objSrcStage, clsGroupStage)
                    If objGroupStage.GetGroupID <> Guid.Empty Then
                        Dim oldGroupID As Guid = objGroupStage.GetGroupID
                        Dim newGroupID As Guid = CType(htOldGroupIDsToNewGroupIDs(oldGroupID), Guid)
                        If newGroupID.Equals(Guid.Empty) Then
                            newGroupID = Guid.NewGuid
                            htOldGroupIDsToNewGroupIDs.Add(oldGroupID, newGroupID)
                        End If
                        objGroupStage.SetGroupID(newGroupID)
                    End If
                End If
                'And any links to the stage must be remapped...
                For Each stg As clsProcessStage In proc.GetStages()
                    Select Case stg.StageType
                        Case StageTypes.Decision
                            Dim decn As clsDecisionStage = CType(stg, clsDecisionStage)
                            If decn.OnTrue = gOldID Then decn.OnTrue = gNewID
                            If decn.OnFalse = gOldID Then decn.OnFalse = gNewID

                        Case StageTypes.ChoiceStart, StageTypes.WaitStart
                            Dim chc As clsChoiceStartStage = CType(stg, clsChoiceStartStage)
                            For Each node As clsChoice In chc.Choices
                                If node.LinkTo = gOldID Then node.LinkTo = gNewID
                            Next
                        Case Else
                            If TypeOf stg Is clsLinkableStage Then
                                Dim linkStg As clsLinkableStage = CType(stg, clsLinkableStage)
                                If linkStg.OnSuccess = gOldID Then linkStg.OnSuccess = gNewID
                            End If
                    End Select
                Next
            End If
        Next

        'If clipboard process is a subsheet selection then generate new ids
        'where required. Also no validation of the pasted stage types is required
        If SheetSelection Then
            Dim objSrcSheet As clsProcessSubSheet
            For Each sheet As clsProcessSubSheet In proc.GetNormalSheets
                Dim gOldID As Guid = sheet.ID
                If GenerateNewIDs OrElse (GetSubSheetByID(sheet.ID) IsNot Nothing) Then
                    'Get the subsheet and allocate a new ID
                    objSrcSheet = proc.GetSubSheetByID(gOldID)
                    gNewID = Guid.NewGuid()
                    objSrcSheet.ID = gNewID

                    'Remap any stages to the new sheet ID (including other sheets
                    'referencing it in the selection)
                    For i As Integer = 0 To proc.GetNumStages() - 1
                        objSrcStage = proc.GetStage(i)
                        If objSrcStage.GetSubSheetID().Equals(gOldID) Then
                            objSrcStage.SetSubSheetID(gNewID)
                        End If
                        Select Case objSrcStage.StageType
                            Case StageTypes.SubSheet
                                Dim objSubSheet As clsSubSheetRefStage = CType(objSrcStage, clsSubSheetRefStage)
                                If objSubSheet.ReferenceId.Equals(gOldID) Then
                                    objSubSheet.ReferenceId = gNewID
                                End If
                        End Select
                    Next
                End If
            Next
            Return proc
        End If

        Dim sThing As String =
         CStr(IIf(ProcessType = DiagramType.Process, My.Resources.Resources.clsProcess_GetPastableProcess_Process, My.Resources.Resources.clsProcess_GetPastableProcess_BusinessObject))

        'Get each stage from the clipboard 'process' and check that it is legal
        For i As Integer = proc.GetNumStages() - 1 To 0 Step -1
            'Get the stage from the clipboard 'process'...
            objSrcStage = proc.GetStage(i)

            'See if this stage should not be pasted for some reason...
            Dim bDeleteStage As Boolean = False
            Select Case objSrcStage.StageType()

                Case StageTypes.ProcessInfo
                    'Disallow pasting of process info stages
                    If report = "" Then
                        report = String.Format(My.Resources.Resources.clsProcess_The0InformationBoxFromTheClipboardWasIgnored, sThing)
                    Else
                        report &= vbCrLf & String.Format(My.Resources.Resources.clsProcess_AlsoThe0InformationBoxFromTheClipboardWasIgnored, sThing)
                    End If
                    bDeleteStage = True

                Case StageTypes.SubSheetInfo
                    If Not objSrcStage.GetSubSheetID.Equals(proc.GetMainPage.ID) Then
                        If report = "" Then
                            report = String.Format(My.Resources.Resources.clsProcess_The0InformationBoxFromTheClipboardWasNotPastedAsA1InformationBoxCanOnlyGoOnTheF, sThing, sThing) & vbCrLf
                        Else
                            report &= vbCrLf & String.Format(My.Resources.Resources.clsProcess_AlsoThe0InformationBoxFromTheClipboardWasNotPastedAsA1InformationBoxCanOnlyGoOn, sThing, sThing) & vbCrLf
                        End If
                        bDeleteStage = True
                    End If

                Case StageTypes.Code, StageTypes.Read, StageTypes.Write, StageTypes.Navigate, StageTypes.WaitEnd, StageTypes.WaitStart
                    If ProcessType <> DiagramType.Object Then
                        If report = "" Then
                            report = String.Format(My.Resources.Resources.clsProcess_TheStage0WillNotBePastedBecauseItIsAnObjectStudioStage, objSrcStage.GetName)
                        Else
                            report &= vbCrLf & String.Format(My.Resources.Resources.clsProcess_AlsoTheStage0WillNotBePastedBecauseItIsAnObjectStudioStage, objSrcStage.GetName)
                        End If
                        bDeleteStage = True
                    End If

            End Select

            If bDeleteStage Then
                proc.RemoveStage(objSrcStage)
            End If
        Next

        Return proc
    End Function

    ''' <summary>
    ''' Perform a paste operation as a new subsheet for the current process. A new
    ''' subsheet will be added to the process with the supplied preferred name (if
    ''' required this name will be adjusted to ensure uniqueness).
    ''' </summary>
    ''' <param name="sName">Preferred sheet name</param>
    ''' <param name="clipboardXML">The XML to be pasted</param>
    ''' <param name="iPosition">Tab index to add the new sheet</param>
    ''' <returns>The newly created sheet ID</returns>
    Public Function PasteSubSheet(ByVal sName As String, ByVal clipboardXML As String, ByVal iPosition As Integer) As Guid
        Dim newID As Guid = Guid.Empty

        Using proc As clsProcess = GetPastableProcess(clipboardXML, Nothing, Nothing)
            If proc IsNot Nothing Then
                Dim sheet = proc.GetNormalSheets.FirstOrDefault
                If sheet IsNot Nothing Then
                    ' Add copy of passed sheet
                    Dim oldName = sheet.Name
                    newID = AddSheet(sName, sheet, iPosition)
                    ' Add in the stages for this sheet too
                    For Each stage As clsProcessStage In proc.GetStages()
                        stage.Name = If(stage.Name = oldName, sName, stage.Name)
                        If stage.GetSubSheetID().Equals(sheet.ID) Then
                            AddStage(stage)
                        End If
                    Next
                End If
            End If
        End Using

        Return newID
    End Function

    ''' <summary>
    ''' Perform a paste operation as a set of new subsheets for the current process.
    ''' New subsheet will be added to the process with their original names and IDs
    ''' where possible - however if clashes occur with existing subsheets then these
    ''' will be adjusted to ensure uniqueness.
    ''' </summary>
    ''' <param name="clipboardXML">The XML to be pasted</param>
    ''' <param name="iPosition">Tab index to add the new sheets</param>
    ''' <returns>A list of the newly created sheet IDs</returns>
    Public Function PasteSubSheets(ByVal clipboardXML As String, ByVal iPosition As Integer) As IList(Of Guid)
        Dim sheetIDs As New List(Of Guid)

        Using proc As clsProcess = GetPastableProcess(clipboardXML, Nothing, Nothing)
            If proc IsNot Nothing Then
                ' Determine starting position for pasting
                iPosition = Math.Max(iPosition + 1, FirstNormalSubSheetIndex)

                For Each sheet As clsProcessSubSheet In proc.GetNormalSheets
                    ' Add to collection at required position
                    Dim newID As Guid = AddSheet(sheet.Name, sheet, iPosition)
                    ' if we've changed the sheet name, update any stage references to it
                    If GetSubSheetByID(newID).Name <> sheet.Name Then
                        For Each refStg As clsSubSheetRefStage In proc.GetStages(Of clsSubSheetRefStage)()
                            If refStg.ReferenceId.Equals(newID) Then
                                'Rename any 'SubSheet' stages (references) linked to the subsheet (page).
                                refStg.Name = refStg.GetName.Replace(sheet.Name, GetSubSheetByID(newID).Name)
                            End If
                        Next
                    End If

                    sheetIDs.Add(newID)
                    iPosition += 1
                Next
                ' Add in the stagess too
                For Each stage As clsProcessStage In proc.GetStages()
                    AddStage(stage)
                Next
            End If
        End Using

        Return sheetIDs
    End Function

    ''' <summary>
    ''' Add subsheet to current process at specified location.
    ''' </summary>
    ''' <param name="sheet">The sheet to add</param>
    ''' <param name="iPosition">The location to add it</param>
    ''' <returns>The new subsheet ID</returns>
    Private Function AddSheet(ByVal sName As String, ByVal sheet As clsProcessSubSheet, ByVal iPosition As Integer) As Guid
        ' Copy subsheet to current process
        Dim newSheet As New clsProcessSubSheet(Me)
        newSheet.ID = sheet.ID
        newSheet.Name = GetUniquePasteName(sName)
        newSheet.SheetType = sheet.SheetType
        newSheet.CameraX = sheet.CameraX
        newSheet.CameraY = sheet.CameraY
        newSheet.Zoom = sheet.Zoom
        newSheet.Published = False

        If iPosition >= 0 And iPosition < SubSheets.Count Then
            ' Make sure we don't insert it in between the special sheets by using
            ' the(first) 'normal' index if iPosition is too low
            SubSheets.Insert(Math.Max(iPosition + 1, FirstNormalSubSheetIndex), newSheet)
        Else
            SubSheets.Add(newSheet)
        End If

        Return newSheet.ID
    End Function

    ''' <summary>
    ''' Create a suitable subsheet copy name for this process, e.g.
    ''' "Sheet 1 - Copy" or "Sheet 1 - Copy (1)" etc.
    ''' </summary>
    ''' <param name="sName">Preferred sheet name</param>
    ''' <returns>New sheet name</returns>
    Private Function GetUniquePasteName(ByVal sName As String) As String
        Dim newName As String = sName
        Dim i As Integer = 0
        While Not GetSubSheetID(newName).Equals(Guid.Empty)
            If i > 0 Then
                newName = String.Format(My.Resources.Resources.clsProcess_0Copy1, sName, i)
            Else
                newName = String.Format(My.Resources.Resources.clsProcess_0Copy, sName)
            End If
            i += 1
        End While
        Return newName
    End Function

    ''' <summary>
    ''' Perform a paste operation. The data is pasted onto the page currently being
    ''' viewed.
    ''' </summary>
    ''' <param name="Position">The position, in world coordinates,
    ''' at which the first stage in the clipboard process will be
    ''' pasted; other stages will be positioned relative to this one.
    ''' </param>
    ''' <param name="sReport">Carries back a report about stages omitted,
    ''' or replaced during the paste operation.</param>
    ''' <param name="sErr">Carries an error message in the event of an error.</param>
    ''' <returns>Returns true on success.</returns>
    Public Function PasteProcess(ByVal objClipProc As clsProcess, ByVal Position As PointF, ByRef sReport As String, ByRef sErr As String) As Boolean

        If objClipProc Is Nothing Then
            sErr = My.Resources.Resources.clsProcess_BadProcessPassedToPasteProcessMethod
            Return False
        End If

        Try
            'Clear the current selection, as the pasted objects will become
            'the new selection...
            SelectNone()

            Dim ReplacedStages As New List(Of String)
            Dim IgnoredClipboardStages As New List(Of String)

            Dim stages As List(Of clsProcessStage) = objClipProc.GetStages()
            If stages.Count > 0 Then
                Dim FirstStageLocation As PointF = stages(0).Location

                'Replace certain stages using the new stages in the
                'clipboard process, where appropriate
                mSuppressSelectionEvents = True
                For Each stg As clsProcessStage In stages

                    Dim replaceStage As clsProcessStage = Nothing

                    Select Case stg.StageType

                        Case StageTypes.Start
                            replaceStage = GetStageByTypeAndSubSheet(stg.StageType(), mActiveSubsheetId)
                            ReplacedStages.Add(String.Format(My.Resources.Resources.clsProcess_StartStage0, stg.Name))

                        Case StageTypes.Code
                            'Rename code stages if existing stages exist with that name
                            For Each st As clsProcessStage In GetStages()
                                If stg.Name = st.Name Then
                                    stg.Name = GetUniqueStageID(stg.StageType)
                                End If
                            Next

                        Case StageTypes.SubSheetInfo
                            Dim SourceSheet As clsProcessSubSheet = objClipProc.GetSubSheetByID(stg.GetSubSheetID)
                            If SourceSheet.SheetType = SubsheetType.MainPage Then
                                'this is a process info stage
                                replaceStage = GetStageByTypeAndSubSheet(stg.StageType(), mActiveSubsheetId)
                                ReplacedStages.Add(String.Format(My.Resources.Resources.clsProcess_InformationBox0, stg.Name))
                            Else
                                'this is an ordinary page info stage
                                If mActiveSubsheetId.Equals(GetMainPage.ID) Then
                                    If sErr = "" Then
                                        sErr = My.Resources.Resources.clsProcess_ThePageInformationBoxFromTheClipboardWillNotBePastedAsAPageInformationBoxCanOnl & vbCrLf
                                    End If
                                    IgnoredClipboardStages.Add(String.Format(My.Resources.Resources.clsProcess_PageInformationStage0, stg.Name))
                                    GoTo DontAdd
                                End If
                                replaceStage = GetStageByTypeAndSubSheet(stg.StageType, mActiveSubsheetId)
                                ReplacedStages.Add(String.Format(My.Resources.Resources.clsProcess_PageInformationStage0b, stg.Name))
                            End If

                        Case StageTypes.Collection
                            If mState <> ProcessRunState.Off Then
                                ' Ensure that the value is set to empty
                                DirectCast(stg, clsCollectionStage).Value =
                                 New clsProcessValue(New clsCollection())
                            End If

                            ' Not a replacement, but some actions (most obviously the
                            ' "Launch" nav action) are dynamic, based on the app info
                            ' of the current VBO. We need to ensure that we are using
                            ' the correct step action flavour for this process before
                            ' we paste it in. A round trip to GetActionTypeInfo() can
                            ' ensure that we have it right.
                        Case StageTypes.Navigate, StageTypes.Read
                            For Each stp As clsStep In CType(stg, clsAppStage).Steps
                                If stp.Action IsNot Nothing Then _
                                 stp.Action = clsAMI.GetActionTypeInfo(
                                  stp.ActionId, ApplicationDefinition.ApplicationInfo)
                            Next

                    End Select

                    If replaceStage IsNot Nothing Then
                        RemoveStage(replaceStage)
                    End If

                    'The offset between this stage, and the first
                    'stage in the clipboard process
                    Dim Offset As New SizeF(stg.GetDisplayX - FirstStageLocation.X, stg.GetDisplayY - FirstStageLocation.Y)

                    'Add the newly added stage to the process, and select it
                    AddStage(stg)
                    stg.Location = PointF.Add(Position, Offset)
                    stg.SetSubSheetID(GetActiveSubSheet())
                    Dim iIndex As Integer
                    iIndex = GetStageIndex(stg.GetStageID())
                    SelectStage(iIndex, True)

DontAdd:
                Next

                mSuppressSelectionEvents = False
                RaiseEvent SelectionChanged()
            End If

            'Report on the replaced stages:
            Dim sb As New StringBuilder()
            If ReplacedStages.Count > 0 Then
                sb.Append(vbCrLf).Append(vbCrLf).Append(
                 My.Resources.Resources.clsProcess_TheFollowingStagesWereReplacedByTheClipboardStages)
                For Each replacement As String In ReplacedStages
                    sb.Append(vbCrLf).Append(replacement)
                Next
            End If

            'Report on ignored stages:
            If IgnoredClipboardStages.Count > 0 Then
                sb.Append(vbCrLf).Append(vbCrLf).Append(
                 My.Resources.Resources.clsProcess_TheFollowingClipboardStagesWereIgnored)
                For Each instance As String In IgnoredClipboardStages
                    sb.Append(vbCrLf).Append(instance)
                Next
            End If
            If sb.Length > 0 Then sReport &= sb.ToString()

            Return True
        Catch e As Exception
            sErr = String.Format(My.Resources.Resources.clsProcess_ExceptionOccurred0, e.Message)
        Finally
            mSuppressSelectionEvents = False
        End Try
    End Function

    ''' <summary>
    ''' Gets an XML representation of the process
    ''' </summary>
    ''' <param name="bFormatted">Set to true if XML indentation is required</param>
    ''' <returns>The requested XML</returns>
    Public Function GenerateXML(Optional ByVal bFormatted As Boolean = True) As String
        Return GenerateXML(Guid.Empty, bFormatted)
    End Function

    ''' <summary>
    ''' Gets an XML representation of the process, or if a sheet ID is passed an XML
    ''' representation of just that page as a process.
    ''' </summary>
    ''' <param name="gSheetID">The sheet id (optional)</param>
    ''' <param name="bFormatted">Set to true if XML indentation is required</param>
    ''' <returns>The requested XML</returns>
    Public Function GenerateXML(ByVal gSheetID As Guid, Optional ByVal bFormatted As Boolean = True) As String
        If IsDisposed Then Throw New ObjectDisposedException(Name)

        Dim x As New XmlDocument()
        Dim e As XmlElement, e2 As XmlElement
        Dim bPageAsProcess As Boolean = (gSheetID <> Guid.Empty)

        Dim root As XmlElement = x.CreateElement("process")
        If bPageAsProcess Then
            root.SetAttribute("name", GetSubSheetName(gSheetID))
        Else
            root.SetAttribute("name", Name)
        End If
        root.SetAttribute("version", Version)
        root.SetAttribute("bpversion", Assembly.GetExecutingAssembly().GetName().Version.ToString())
        root.SetAttribute("narrative", Description)
        root.SetAttribute("byrefcollection",
         XmlConvert.ToString(PassNestedCollectionsByReference))
        If ProcessType = DiagramType.Object Then
            root.SetAttribute("type", "object")
            root.SetAttribute("runmode", ObjectRunMode.ToString())
            If IsShared Then root.SetAttribute("shared", IsShared.ToString())
        End If
        x.AppendChild(root)

        ' If we have any non-default logging options set, record them
        If AbortOnLogError OrElse
         LoggingAttempts <> DefaultAttempts OrElse
         LoggingRetryPeriod <> DefaultRetrySeconds Then
            Dim el As XmlElement = x.CreateElement("logging")
            el.SetAttribute("abort-on-error", XmlConvert.ToString(AbortOnLogError))
            el.SetAttribute("attempts", XmlConvert.ToString(LoggingAttempts))
            el.SetAttribute("retry-period", XmlConvert.ToString(LoggingRetryPeriod))
            root.AppendChild(el)
        End If

        ' Add application definition if relevant...
        If ProcessType = DiagramType.Object Then
            If ParentObject IsNot Nothing Then
                'If using a shared model add an empty application definition
                root.AppendChild(New clsApplicationDefinition().ToXML(x))

                'And parent object if there is one...
                Dim el As XmlElement = x.CreateElement("parentobject")
                el.AppendChild(x.CreateTextNode(ParentObject.ToString))
                root.AppendChild(el)
            ElseIf Not ApplicationDefinition Is Nothing Then
                root.AppendChild(ApplicationDefinition.ToXML(x))
            End If
        End If

        ' Add view information...
        Dim pg As clsProcessSubSheet = GetMainPage()
        Dim viewEl As XmlElement = x.CreateElement("view")
        AppendTextElement(viewEl, "camerax", XmlConvert.ToString(pg.CameraX))
        AppendTextElement(viewEl, "cameray", XmlConvert.ToString(pg.CameraY))
        Dim zoomEl = AppendTextElement(viewEl, "zoom", XmlConvert.ToString(pg.Zoom))
        zoomEl.SetAttribute("version", "2")
        root.AppendChild(viewEl)

        ' Add Preconditions 
        e = x.CreateElement("preconditions")
        If Not Preconditions Is Nothing Then
            Dim sCond As String
            For Each sCond In Preconditions
                e2 = x.CreateElement("condition")
                e2.SetAttribute("narrative", sCond)
                e.AppendChild(e2)
            Next
        End If
        root.AppendChild(e)

        ' And Endpoint
        e = x.CreateElement("endpoint")
        e.SetAttribute("narrative", Endpoint)
        root.AppendChild(e)

        ' Create subsheet elements (except for main page)
        If Not bPageAsProcess Then
            For Each sheet As clsProcessSubSheet In SubSheets
                If Not sheet.SheetType = SubsheetType.MainPage Then
                    root.AppendChild(GetSubSheetXML(x, sheet))
                End If
            Next
        End If

        ' Create stage elements
        For Each stage As clsProcessStage In mSchema.Stages
            Dim bIncludeStage As Boolean = True
            If bPageAsProcess AndAlso Not stage.GetSubSheetID().Equals(gSheetID) Then
                bIncludeStage = False
            End If

            If bIncludeStage Then
                Dim stgEl As XmlElement = x.CreateElement("stage")
                'If this stage has a subsheeet ID, add the subsheetid element...
                'unless we are exporting the subsheet as a new process
                If Not bPageAsProcess Then
                    If Not stage.GetSubSheetID().Equals(Guid.Empty) Then
                        e2 = x.CreateElement("subsheetid")
                        e2.AppendChild(x.CreateTextNode(stage.GetSubSheetID().ToString()))
                        stgEl.AppendChild(e2)
                    End If
                End If

                stage.ToXml(x, stgEl, False)
                root.AppendChild(stgEl)
            End If
        Next

        Dim s As New StringWriter()
        Dim xs As New XmlWriterSettings()
        xs.NewLineHandling = NewLineHandling.Replace
        xs.Indent = bFormatted
        xs.OmitXmlDeclaration = True
        xs.Encoding = Encoding.UTF8
        xs.CheckCharacters = False
        Using xw As XmlWriter = XmlWriter.Create(s, xs)
            x.WriteContentTo(xw)
            xw.Flush()
        End Using

        Return s.ToString()
    End Function

    ''' <summary>
    ''' Gets an XML representation of the currently selected page.
    ''' </summary>
    ''' <param name="sheetID">The selected sheet ID</param>
    ''' <param name="bFormatted">Set to true if XML indentation is required</param>
    ''' <returns>The requested XML</returns>
    Public Function GeneratePageSelectionXML(ByVal sheetID As Guid, Optional ByVal bFormatted As Boolean = True) As String
        Dim sheetIDs As New List(Of Guid)
        sheetIDs.Add(sheetID)
        Return GeneratePageSelectionXML(sheetIDs, bFormatted)
    End Function

    ''' <summary>
    ''' Gets an XML representation of the currently selected pages.
    ''' </summary>
    ''' <param name="sheetIDs">List of selected sheet ids</param>
    ''' <param name="bFormatted">Set to true if XML indentation is required</param>
    ''' <returns>The requested XML</returns>
    Public Function GeneratePageSelectionXML(ByVal sheetIDs As IList(Of Guid), Optional ByVal bFormatted As Boolean = True) As String
        Dim x As New XmlDocument()
        Dim root As XmlElement, e As XmlElement, e2 As XmlElement

        ' Create root for subsheet selection
        root = x.CreateElement("process")
        root.SetAttribute("name", "__selection__" & Name)
        If ProcessType = DiagramType.Object Then
            root.SetAttribute("type", "object")
        End If
        x.AppendChild(root)

        ' Create subsheet elements for selection
        For Each sheet As clsProcessSubSheet In SubSheets
            If sheetIDs.Contains(sheet.ID) Then
                root.AppendChild(GetSubSheetXML(x, sheet))
            End If
        Next

        ' Create stage elements for selection
        For Each stage As clsProcessStage In mSchema.Stages
            If sheetIDs.Contains(stage.GetSubSheetID()) Then
                e = x.CreateElement("stage")
                'If this stage has a subsheeet ID, add the subsheetid element...
                If Not stage.GetSubSheetID().Equals(Guid.Empty) Then
                    e2 = x.CreateElement("subsheetid")
                    e2.AppendChild(x.CreateTextNode(stage.GetSubSheetID().ToString()))
                    e.AppendChild(e2)
                End If

                stage.ToXml(x, e, False)
                root.AppendChild(e)
            End If
        Next

        Dim s As New StringWriter()
        Dim xs As New XmlWriterSettings()
        xs.NewLineHandling = NewLineHandling.Replace
        xs.Indent = bFormatted
        xs.OmitXmlDeclaration = True
        xs.Encoding = Encoding.UTF8
        xs.CheckCharacters = False
        Using xw As XmlWriter = XmlWriter.Create(s, xs)
            x.WriteContentTo(xw)
            xw.Flush()
        End Using

        Return s.ToString()
    End Function

    ''' <summary>
    ''' Gets an XML representation of the currently selected stages
    ''' </summary>
    ''' <param name="bFormatted">Set to true if XML indentation is required</param>
    ''' <returns>The requested XML</returns>
    Public Function GenerateSelectionXML(Optional ByVal bFormatted As Boolean = True) As String
        Dim x As New XmlDocument()
        Dim root As XmlElement, e As XmlElement, e2 As XmlElement

        ' Create root for stage selection
        root = x.CreateElement("process")
        root.SetAttribute("name", "__selection__" & Name)
        If ProcessType = DiagramType.Object Then
            root.SetAttribute("type", "object")
            root.SetAttribute("runmode", ObjectRunMode.ToString())
        End If
        x.AppendChild(root)

        ' Create stage elements for selection
        For Each stage As clsProcessStage In mSchema.Stages
            If IsStageSelected(stage.GetStageID()) Then
                e = x.CreateElement("stage")
                'If this stage has a subsheeet ID, add the subsheetid element...
                If Not stage.GetSubSheetID().Equals(Guid.Empty) Then
                    e2 = x.CreateElement("subsheetid")
                    e2.AppendChild(x.CreateTextNode(stage.GetSubSheetID().ToString()))
                    e.AppendChild(e2)
                End If

                stage.ToXml(x, e, True)
                root.AppendChild(e)
            End If
        Next

        Dim s As New StringWriter()
        Dim xs As New XmlWriterSettings()
        xs.NewLineHandling = NewLineHandling.Replace
        xs.OmitXmlDeclaration = True
        xs.Encoding = Encoding.UTF8
        xs.CheckCharacters = False
        Using xw As XmlWriter = XmlWriter.Create(s, xs)
            x.WriteContentTo(xw)
            xw.Flush()
        End Using

        Return s.ToString()
    End Function

    ''' <summary>
    ''' Gets an XML representation of the passed sheet
    ''' </summary>
    ''' <param name="doc">XML document</param>
    ''' <param name="sheet">The subsheet to get the XML for</param>
    ''' <returns>XML representation of the sheet</returns>
    Private Function GetSubSheetXML(
     ByVal doc As XmlDocument, ByVal sheet As clsProcessSubSheet) As XmlElement

        Dim ssheet = doc.CreateElement("subsheet")
        ssheet.SetAttribute("subsheetid", sheet.ID.ToString())
        ssheet.SetAttribute("type", sheet.SheetType.ToString())
        ssheet.SetAttribute("published", sheet.Published.ToString())

        BPUtil.AppendTextElement(ssheet, "name", sheet.Name)

        Dim view = doc.CreateElement("view")
        BPUtil.AppendTextElement(view, "camerax", XmlConvert.ToString(sheet.CameraX))
        BPUtil.AppendTextElement(view, "cameray", XmlConvert.ToString(sheet.CameraY))

        Dim zoom =
            BPUtil.AppendTextElement(view, "zoom", XmlConvert.ToString(sheet.Zoom))
        zoom.SetAttribute("version", "2")

        ssheet.AppendChild(view)

        Return ssheet
    End Function

    ''' <summary>
    ''' Extracts the process name from the XML provided by the given reader.
    ''' It is expected to be the XML representation of a process. If it does not
    ''' represent a process or the reader is null, this will return null.
    ''' </summary>
    ''' <param name="reader">The stream reader over the XML from which the process
    ''' name should be extracted.</param>
    ''' <returns>The name of the process, as defined in the given XML.</returns>
    Public Shared Function ExtractProcessName(ByVal reader As TextReader) As String
        If reader Is Nothing Then Return Nothing
        ' Use a reader so we don't have to read the whole thing into a DOM
        Using xr As New XmlTextReader(reader)
            While xr.Read()
                If xr.NodeType = XmlNodeType.Element AndAlso xr.Name = "process" Then
                    Return xr("name")
                End If
            End While
            Return Nothing
        End Using
    End Function

    ''' <summary>
    ''' Extracts the process name from the given XML, expected to be the XML
    ''' representation of a process. If it does not represent a process, this
    ''' will return null.
    ''' </summary>
    ''' <param name="xml">The XML from which the process name should be extracted
    ''' </param>
    ''' <returns>The name of the process, as defined in the given XML.</returns>
    Public Shared Function ExtractProcessName(ByVal xml As String) As String
        Return ExtractProcessName(New StringReader(xml))
    End Function

    ''' <summary>
    ''' Create a new process based on an XML definition
    ''' If an error occurs, nothing is returned and sErr will 
    ''' contain an error description.
    ''' </summary>
    ''' <param name="externalObjs">A reference to the manager of external objects
    ''' available to the created process</param>
    ''' <param name="xml">An XML string representing the process</param>
    ''' <param name="editable">Determines whether the new process will be editable.
    ''' See the more detailed notes for the same parameter on the clsProcess
    ''' constructor.</param>
    ''' <param name="sErr">An error message if the process failed to load</param>
    ''' <returns>A new process or null if the process could not be parsed from the
    ''' given XML</returns>
    Public Shared Function FromXML(externalObjs As IGroupObjectDetails, xml As String,
                                   editable As Boolean, ByRef sErr As String) As clsProcess
        Try
            Return FromXml(externalObjs, xml, editable)
        Catch ex As Exception
            sErr = String.Format(My.Resources.Resources.clsProcess_ExceptionWhileLoadingProcess0, ex.Message)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Create a new full process based on an XML definition.
    ''' </summary>
    ''' <param name="externalObjs">A reference to the manager of external objects
    ''' available to the created process</param>
    ''' <param name="xml">An XML string representing the process</param>
    ''' <param name="editable">Determines whether the new process will be editable.
    ''' See the more detailed notes for the same parameter on the clsProcess
    ''' constructor.</param>
    ''' <returns>The process generated from the given XML.</returns>
    ''' <exception cref="ArgumentNullException">If the given XML was null or empty.
    ''' </exception>
    ''' <exception cref="XmlException">If the given string was not valid XML and so
    ''' could not be parsed into a process.</exception>
    ''' <exception cref="InvalidFormatException">If the given XML did not represent
    ''' a process - ie. its root node was something other than "process"</exception>
    Public Shared Function FromXml(externalObjs As IGroupObjectDetails, xml As String,
                                   editable As Boolean) As clsProcess
        Return FromXml(externalObjs, xml, editable, True)
    End Function

    ''' <summary>
    ''' Create a new process based on an XML definition.
    ''' If an error occurs, nothing is returned and sErr will 
    ''' contain an error description.
    ''' </summary>
    ''' <param name="externalObjs">A reference to the manager of external objects
    ''' available to the created process</param>
    ''' <param name="xml">An XML string representing the process</param>
    ''' <param name="editable">Determines whether the new process will be editable.
    ''' See the more detailed notes for the same parameter on the clsProcess
    ''' constructor.</param>
    ''' <returns>The process generated from the given XML.</returns>
    ''' <exception cref="ArgumentNullException">If the given XML was null or empty.
    ''' </exception>
    ''' <exception cref="XmlException">If the given string was not valid XML and so
    ''' could not be parsed into a process.</exception>
    ''' <exception cref="InvalidFormatException">If the given XML did not represent
    ''' a process - ie. its root node was something other than "process"</exception>
    Public Shared Function FromXml(externalObjs As IGroupObjectDetails, xml As String,
                                   editable As Boolean, fullProc As Boolean) As clsProcess

        ' We need the process the type (ie. process/object) before we can create the
        ' clsProcess object, which we must do before we can (fully) parse.
        Dim doc As New ProcessXmlDocument(xml)
        Dim proc As New clsProcess(externalObjs, doc.ProcessType, editable, doc.Version)
        proc.ParseXMLInternal(doc, fullProc)

        Return proc

    End Function

    ''' <summary>
    ''' Reads the zoom value for the given XML element, scaling it up using the
    ''' <see cref="ScaleFactor"/> if necessary (ie. if the zoom is from a pre-5.0
    ''' version of BP and is currently unchanged from the default of 100%).
    ''' </summary>
    ''' <param name="el">The element from which the zoom value should be read.
    ''' </param>
    ''' <returns>The zoom factor from the given element, scaled up as necessary.
    ''' </returns>
    Private Function ReadZoom(el As XmlElement) As Single

        Dim zm = ReadXmlSingle(el)

        ' If there's no version attribute in the zoom element, it means that it's an
        ' old value - we need to scale the zoom if it is currently 100%
        If el.GetAttribute("version") = "" AndAlso zm = 1.0! Then zm *= ScaleFactor

        Return zm
    End Function



    ''' <summary>
    ''' Navigates the given XML Document, populating this process from its data.
    ''' </summary>
    ''' <param name="doc">The fully loaded XML document from where the process data
    ''' should be drawn</param>
    ''' <param name="fullProcess">True to treat the given XML as a fully formed
    ''' process - ie. with all necessary system pages - eg. when pasting from the
    ''' clipboard or from the undo/redo buffer, the pasted stages should not be
    ''' treated as a full process.</param>
    ''' <exception cref="InvalidFormatException">If the root element of the given
    ''' document was not 'process'</exception>
    Private Sub ParseXMLInternal(
     ByVal doc As ProcessXmlDocument, ByVal fullProcess As Boolean)

        mSchema.Stages.Clear()
        Preconditions.Clear()

        SubSheets.Clear()

        Dim stg As clsProcessStage
        Dim root As XmlElement = doc.DocumentElement


        Name = root.GetAttribute("name")
        Description = root.GetAttribute("narrative")
        Dim byRefAttr = root.GetAttribute("byrefcollection")
        PassNestedCollectionsByReference =
            (byRefAttr = "" OrElse XmlConvert.ToBoolean(byRefAttr))

        Guid.TryParse(root.GetAttribute("preferredid"), Id)

        ' If we are a VBO, get our object runmode from the attribute
        ' (assume default - ie. exclusive if not there or invalid)
        If ProcessType = DiagramType.Object Then
            clsEnum(Of BusinessObjectRunMode).TryParse(
             root.GetAttribute("runmode"), True, ObjectRunMode)
            If root.HasAttribute("shared") Then _
             IsShared = XmlConvert.ToBoolean(root.GetAttribute("shared").ToLower())
        End If

        'We need to add a main / init page
        Dim main As clsProcessSubSheet = AddMainSheet()

        For Each elem As XmlElement In root.ChildNodes
            Select Case elem.Name

                Case "logging"
                    AbortOnLogError = XmlConvert.ToBoolean(
                     elem.GetAttribute("abort-on-error"))
                    LoggingAttempts = XmlConvert.ToInt32(
                     elem.GetAttribute("attempts"))
                    LoggingRetryPeriod = XmlConvert.ToInt32(
                     elem.GetAttribute("retry-period"))

                Case "appdef"
                    mApplicationDefinition = clsApplicationDefinition.FromXML(elem)
                    Dim Err As clsAMIMessage = Nothing
                    mAMI.SetTargetApplication(mApplicationDefinition.ApplicationInfo, Err)

                Case "parentobject"
                    mParentObject = elem.ChildNodes(0).InnerText

                Case "preconditions"
                    For Each preconEl As XmlElement In elem.ChildNodes
                        If preconEl.Name = "condition" Then _
                         Preconditions.Add(preconEl.GetAttribute("narrative"))
                    Next

                Case "endpoint"
                    Endpoint = elem.GetAttribute("narrative")

                Case "view"
                    'this view info gets stored in subsheet info of main page
                    For Each viewEl As XmlElement In elem.ChildNodes
                        Select Case viewEl.Name
                            Case "camerax" : main.CameraX = ReadXmlSingle(viewEl)
                            Case "cameray" : main.CameraY = ReadXmlSingle(viewEl)
                            Case "zoom" : main.Zoom = ReadZoom(viewEl)
                        End Select
                    Next

                Case "subsheet"
                    Dim sheet As New clsProcessSubSheet(Me)
                    sheet.ID = New Guid(elem.GetAttribute("subsheetid"))

                    'set published attribute
                    If elem.HasAttribute("published") Then
                        sheet.Published =
                         XmlConvert.ToBoolean(elem.GetAttribute("published").ToLower())
                    Else
                        sheet.Published = False
                    End If

                    'set subsheet type (eg main page, init page etc)
                    Dim tp As SubsheetType
                    clsEnum(Of SubsheetType).TryParse(
                     elem.GetAttribute("type"), True, tp)
                    sheet.SheetType = tp

                    For Each subsheetEl As XmlElement In elem.ChildNodes
                        Select Case subsheetEl.Name
                            Case "name"
                                sheet.Name = subsheetEl.InnerText
                            Case "view"
                                For Each viewEl As XmlElement In subsheetEl.ChildNodes
                                    Select Case viewEl.Name
                                        Case "camerax" : sheet.CameraX = ReadXmlSingle(viewEl)
                                        Case "cameray" : sheet.CameraY = ReadXmlSingle(viewEl)
                                        Case "zoom" : sheet.Zoom = ReadZoom(viewEl)
                                    End Select
                                Next
                        End Select
                    Next

                    SubSheets.Add(sheet)

                Case "stage"
                    ' Make sure we are not trying to load an old style 
                    ' process without stage id's - these are not supported.
                    If elem.GetAttribute("stageid") = "" Then _
                     Throw New InvalidFormatException(
                      My.Resources.Resources.clsProcess_ThisIsAVeryOldProcessWhichCannotBeLoadedIntoThisVersionOfAutomatePleaseContactB)

                    stg = CreateStage(
                     clsEnum(Of StageTypes).Parse(elem.GetAttribute("type")))

                    stg.FromXML(elem)

                    AddStage(stg)
            End Select
        Next

        ' Check for subsheets that don't have a valid clsProcessSubSheet
        ' record. This is for backward compatibility only - all newly
        ' saved processes will have these!
        If fullProcess Then CreateMissingSubSheets()

        ' Check for old-style loop stages that don't have group ID's
        ' and create them...
        If CreateMissingGroupIDs() Then _
         Debug.WriteLine("Old loop stages found and fixed")

        ' Check for missing 'system' sheets - eg. Initialise / Clean Up / Main Page
        If fullProcess Then CreateMissingSystemSheets()

        RaiseEvent ProcessXMLReloaded()

    End Sub

    ''' <summary>
    ''' Convert a process (or action) name to a 'safe' name that can be used, for
    ''' example, as an XML element name or web service operation/parameter name.
    ''' </summary>
    ''' <param name="sName">The real name, which can contain spaces and
    ''' punctuation.</param>
    ''' <returns>The safe name.</returns>
    Public Shared Function GetSafeName(ByVal sName As String) As String
        Dim safename As New StringBuilder()
        For Each c As Char In sName
            If Char.IsLetterOrDigit(c) Then safename.Append(c)
        Next
        Dim safestr As String = safename.ToString()
        If safestr.Length > 0 Then
            If Char.IsDigit(safestr(0)) Then safestr = "_" & safestr
        End If
        Return safestr
    End Function
    Public Enum IsValidForType
        Valid
        InValid
        InvalidXML
        InvalidFileType
    End Enum
    Public Shared Function CheckValidExtensionForType(path As String) As IsValidForType
        Dim xml = File.ReadAllText(path)
        Dim expected As DiagramType
        Select Case New FileInfo(path).Extension.ToLowerInvariant().Substring(1)
            Case ObjectFileExtension
                expected = DiagramType.Object
            Case ProcessFileExtension
                expected = DiagramType.Process
            Case "xml"
                expected = DiagramType.Unset
            Case Else
                Return IsValidForType.InvalidFileType
        End Select

        Try
            Using reader As New StringReader(xml)
                Using xr As New XmlTextReader(reader)
                    While xr.Read()
                        If xr.NodeType = XmlNodeType.Element AndAlso xr.Name = "process" Then
                            Dim type = If(xr("type") = "object", DiagramType.Object, DiagramType.Process)
                            If expected = DiagramType.Unset OrElse type = expected Then
                                Return IsValidForType.Valid
                            Else
                                Return IsValidForType.InValid
                            End If
                        End If
                    End While

                    Return IsValidForType.InvalidXML
                End Using
            End Using
        Catch e As Exception
            'ignore catch as the file was probably not any type of xml
        End Try
        Return IsValidForType.InvalidXML
    End Function
    Public Property Endpoint As String
        Get
            Return mSchema.EndPoint
        End Get
        Set(value As String)
            mSchema.EndPoint = value
        End Set
    End Property
    Public Property Preconditions As ICollection(Of String)

    ''' <summary>
    ''' Get the current version of the process.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Version As String
        Get
            Return mSchema.Version
        End Get
    End Property

    ''' <summary>
    ''' Get a list of the states available to be undone in the current buffer.
    ''' </summary>
    ''' <returns>A list of states which can be undone, with the next first.</returns>
    Public Function GetUndoStates() As List(Of clsUndo)
        Return mUndoRedoManager.GetUndoStates()
    End Function

    ''' <summary>
    ''' Get a list of the states available to be redone in the current buffer.
    ''' </summary>
    ''' <returns>A list of states which can be redone, with the next first.</returns>
    Public Function GetRedoStates() As List(Of clsUndo)
        Return mUndoRedoManager.GetRedoStates()
    End Function

    ''' <summary>
    ''' Perform a number of undo operations.
    ''' </summary>
    ''' <param name="n">The number of operations to perform</param>
    ''' <param name="sErr">An error message in the event of failure</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Public Function Undo(ByVal n As Integer, ByRef sErr As String) As Boolean

        For i As Integer = 1 To n

            If mUndoRedoManager.CanUndo Then

                'save current data values if running/debugging - as not stored in xml
                Dim datastages As List(Of clsProcessStage)
                If mState <> ProcessRunState.Off Then
                    datastages = GetStages(StageTypes.Data Or StageTypes.Collection)
                Else
                    datastages = New List(Of clsProcessStage)
                End If

                Dim undoAction As New clsUndo()
                If Not mUndoRedoManager.Undo(undoAction) Then            'this automatically stores the forward redo point
                    sErr = My.Resources.Resources.clsProcess_UndoFailed
                    Return False
                End If
                Try
                    ParseXMLInternal(New ProcessXmlDocument(undoAction.State), False)
                Catch ex As Exception
                    sErr = String.Format(My.Resources.Resources.clsProcess_UndoFailed0, ex.Message)
                    Return False
                End Try

                'reset the current values if running/debuging - not stored in xml
                If mState <> ProcessRunState.Off Then
                    ResetDataStages(datastages)
                End If

            Else
                sErr = My.Resources.Resources.clsProcess_CouldNotUndo
                Return False
            End If

        Next

        'Need to do the following until undo is saving the
        'selection state...
        SelectNone()
        Return True

    End Function

    ''' <summary>
    ''' Performs a number of redo operations.
    ''' </summary>
    ''' <param name="n">The number to perform, 1-n</param>
    ''' <param name="sErr">On failure, contains an error message.</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Public Function Redo(ByVal n As Integer, ByRef sErr As String) As Boolean

        For i As Integer = 1 To n
            If mUndoRedoManager.CanRedo Then

                'save current data values if running/debugging - as not stored in xml
                Dim datastages As List(Of clsProcessStage) = Nothing
                If mState <> ProcessRunState.Off Then
                    datastages = GetStages(StageTypes.Data Or StageTypes.Collection)
                End If

                Dim redoAction As New clsUndo()

                If Not mUndoRedoManager.Redo(redoAction) Then
                    Return True
                End If
                Try
                    ParseXMLInternal(New ProcessXmlDocument(redoAction.State), False)
                Catch ex As Exception
                    sErr = String.Format(My.Resources.Resources.clsProcess_RedoFailed0, ex.Message)
                    Return False
                End Try

                'reset the current values if running/debuging - not stored in xml
                If mState <> ProcessRunState.Off Then
                    ResetDataStages(datastages)
                End If

            Else
                sErr = My.Resources.Resources.clsProcess_RedoFailed
                Return False
            End If

        Next

        'Need to do the following until redo is saving the
        'selection state...
        SelectNone()
        Return True

    End Function

    ''' <summary>
    ''' Current values of data/collection stages are not held in the XML. So we need
    ''' to attempt to reset their values from copies taken prior to the undo/redo.
    ''' This might not always be possible e.g. if data types have changed or fields
    ''' removed from collections.
    ''' </summary>
    ''' <param name="datastages"></param>
    Private Sub ResetDataStages(ByVal datastages As List(Of clsProcessStage))
        ' Try to restore curent values for any data/collection stages
        For Each stage As clsProcessStage In datastages
            If TypeOf (stage) Is clsCollectionStage Then
                Dim copy As clsCollectionStage = CType(GetStage(stage.GetStageID()), clsCollectionStage)
                If copy IsNot Nothing AndAlso copy.Exposure <> StageExposureType.Environment Then
                    ' Try to transfer current values for each row (if the fields still exist)
                    Dim col As New clsCollection()
                    For Each oldRow As clsCollectionRow In CType(stage, clsCollectionStage).Value.Collection.Rows
                        Dim row As New clsCollectionRow()
                        For Each field As clsCollectionFieldInfo In copy.FieldDefinitions
                            ' If the field existed before with the same data type get it's value
                            ' otherwise create a null process value
                            If CType(stage, clsCollectionStage).ContainsField(field.Name) AndAlso
                             CType(stage, clsCollectionStage).GetFieldDefinition(field.Name).DataType = field.DataType Then
                                row.Add(field.Name, oldRow.Item(field.Name))
                            Else
                                row.Add(field.Name, New clsProcessValue(field.DataType))
                            End If
                        Next
                        If row.Count > 0 Then
                            col.Add(row)
                        End If
                    Next
                    If col.Count > 0 Then
                        ' Reset selected row
                        col.SetCurrentRow(CType(stage, clsCollectionStage).Value.Collection.CurrentRowIndex)
                    End If
                    copy.Value.Collection = col
                End If
            Else
                Dim copy As clsDataStage = CType(GetStage(stage.GetStageID()), clsDataStage)
                If copy IsNot Nothing AndAlso copy.Exposure <> StageExposureType.Environment Then
                    Try
                        copy.SetValue(CType(stage, clsDataStage).GetValue())
                    Catch ex As ApplicationException
                        'likely a bad cast due to the data type of the stage
                        'having changed - ignore error
                    End Try
                End If
            End If
        Next

        'may have added back data stage - if so need to set current value
        Dim newdatastages As List(Of clsProcessStage) = GetStages(StageTypes.Data Or StageTypes.Collection)
        If newdatastages.Count > datastages.Count Then
            For Each stg As clsDataStage In newdatastages
                Try
                    If stg.GetValue().IsNull Then _
                     stg.SetValue(stg.GetInitialValue())
                Catch ex As ApplicationException
                    'Likely a bad cast due to the data type of the stage
                    'having changed - ignore error
                End Try
            Next
        End If
    End Sub

    ''' <summary>
    ''' The maximum number of undo levels allowed for this process
    ''' </summary>
    Public Property MaxUndoLevels() As Integer
        Get
            Return mUndoRedoManager.MaxUndoLevels
        End Get
        Set(ByVal value As Integer)
            mUndoRedoManager.SetMaxUndoLevels(value)
        End Set
    End Property

    ''' <summary>
    ''' Saves the given undo state to the undo/redo manager
    ''' </summary>
    ''' <param name="undoState">The state to save to the manager</param>
    Private Sub SaveUndoPosition(ByVal undoState As clsUndo)
        If mUndoRedoManager.Enabled Then
            mUndoRedoManager.AddState(undoState)
            RaiseEvent UndoPositionSaved(undoState)
        End If
    End Sub

    ''' <summary>
    ''' Save the current position to the undo buffer. This should
    ''' be called at UI level at any point which the user would
    ''' expect an undo to return to.
    ''' </summary>
    Public Sub SaveUndoPosition()
        Mark()
        If mUndoRedoManager.Enabled Then SaveUndoPosition(New clsUndo(GenerateXML()))
    End Sub

    ''' <summary>
    ''' Saves the current position to the undo buffer.
    ''' </summary>
    ''' <param name="iActionType">The action performed</param>
    Public Sub SaveUndoPosition(ByVal iActionType As clsUndo.ActionType)
        SaveUndoPosition(iActionType, "", "")
    End Sub

    ''' <summary>
    ''' Saves the current position to the undo buffer.
    ''' </summary>
    ''' <param name="iActionType">The action performed</param>
    ''' <param name="sSummary">A summary of the action</param>
    ''' <param name="sDescription">A description of the action</param>
    Public Sub SaveUndoPosition(ByVal iActionType As clsUndo.ActionType, ByVal sSummary As String, ByVal sDescription As String)
        Mark()
        If mUndoRedoManager.Enabled Then
            SaveUndoPosition(New clsUndo(GenerateXML(), iActionType, sSummary, sDescription))
        End If
    End Sub

    ''' <summary>
    ''' Saves the current position to the undo buffer.
    ''' </summary>
    ''' <param name="iActionType">The action performed</param>
    ''' <param name="oStage">The relevant stage</param>
    Public Sub SaveUndoPosition(ByVal iActionType As clsUndo.ActionType, ByVal oStage As clsProcessStage)
        Mark()
        If mUndoRedoManager.Enabled Then
            SaveUndoPosition(New clsUndo(GenerateXML(), iActionType, oStage))
        End If
    End Sub

    ''' <summary>
    ''' Saves the current position to the undo buffer.
    ''' </summary>
    ''' <param name="iActionType">The action performed</param>
    ''' <param name="aStages">The relevant stages</param>
    Public Sub SaveUndoPosition(ByVal iActionType As clsUndo.ActionType, ByVal aStages As clsProcessStage())
        Mark()
        If mUndoRedoManager.Enabled Then
            SaveUndoPosition(New clsUndo(GenerateXML(), iActionType, aStages))
        End If
    End Sub

    ''' <summary>
    ''' Saves the current position to the undo buffer.
    ''' </summary>
    ''' <param name="iActionType">The action performed</param>
    ''' <param name="oStage">The relevant subsheet</param>
    Public Sub SaveUndoPosition(ByVal iActionType As clsUndo.ActionType, ByVal oStage As clsProcessSubSheet)
        Mark()
        If mUndoRedoManager.Enabled Then
            SaveUndoPosition(New clsUndo(GenerateXML(), iActionType, oStage))
        End If
    End Sub

    ''' <summary>
    ''' Clears the undo and redo buffers. Should be called when the process has been
    ''' successfully saved.
    ''' </summary>
    Public Sub ClearUndoBuffer()
        'Need to clear properly here rather than simply discard
        'and create new manager, because this way an event is 
        'raised and observers learn of the change of state.
        mUndoRedoManager.ClearStates()

        'There should always be the latest state in the undo buffer
        'so we add it again straight away.
        SaveUndoPosition()

    End Sub

    ''' <summary>
    ''' Resets the changed data flag within this object. Immediately after
    ''' calling this method, <see cref="HasChanged"/> will return False.
    ''' </summary>
    Public Overrides Sub ResetChanged()
        ClearUndoBuffer()
        MyBase.ResetChanged()
    End Sub

    ''' <summary>
    ''' Clear recovery mode. Used only by clsResumeStage.
    ''' </summary>
    ''' <returns>True if successful. False if the process was not in recovery mode,
    ''' which indicates a serious error in the process.</returns>
    Friend Function ClearRecovery() As Boolean
        If Not mRecoveryMode Then Return False
        mRecoveryMode = False
        Return True
    End Function

    ''' <summary>
    ''' Find a recovery stage, relative to the given stage, searching up the
    ''' hierarchy as required. If a recovery stage is successfully found at a
    ''' higher level up the SubSheet stack, the stack will be adjusted accordingly.
    ''' If no recovery stage is found or if the parameter is false, the SubSheet
    ''' stack is left unchanged.
    ''' </summary>
    ''' <param name="relstage">The stage relative to which to find the Recovery
    ''' stage, i.e. the stage where the error has occurred.</param>
    ''' <param name="recstage">If we are already recovering, this is the Recovery
    ''' stage we started recovering at. Otherwise, Nothing.</param>
    ''' <returns>The recovery stage found, or Nothing if there isn't one.</returns>
    Private Function FindRecoveryStage(ByVal relstage As clsProcessStage,
     ByVal recstage As clsProcessStage) As clsRecoverStage
        Return FindRecoveryStage(relstage, recstage, True)
    End Function

    ''' <summary>
    ''' Find a recovery stage, relative to the given stage, searching up the
    ''' hierarchy as required. If a recovery stage is successfully found at a
    ''' higher level up the SubSheet stack and <paramref name="updateProcessStack"/>
    ''' is set, the stack will be adjusted accordingly. If no recovery stage is
    ''' found or if the parameter is false, the SubSheet stack is left unchanged.
    ''' </summary>
    ''' <param name="relstage">The stage relative to which to find the Recovery
    ''' stage, i.e. the stage where the error has occurred.</param>
    ''' <param name="recstage">If we are already recovering, this is the Recovery
    ''' stage we started recovering at. Otherwise, Nothing.</param>
    ''' <param name="updateProcessStack">True to update the subsheet stack held by
    ''' this process when finding the recovery stage; False to effectively run a
    ''' read-only query to determine the appropriate recovery stage for a stage.
    ''' </param>
    ''' <returns>The recovery stage found, or Nothing if there isn't one.</returns>
    Private Function FindRecoveryStage(ByVal relstage As clsProcessStage,
     ByVal recstage As clsProcessStage, ByVal updateProcessStack As Boolean) _
     As clsRecoverStage

        ' If we're updating the process directly, just use that stack
        ' otherwise 'clone' it and use a local copy of the stack for our search
        Dim subsheetStack As Stack(Of Guid)
        If updateProcessStack Then
            subsheetStack = mSubSheetStack
        Else
            ' This delightful double constructor call effectively 'clones'
            ' the stack reversing it twice.
            subsheetStack = New Stack(Of Guid)(New Stack(Of Guid)(mSubSheetStack))
        End If

        'If we're already recovering and the error has occurred on a different page to
        'the start of our current recovery, we need to backtrack up the subsheet stack
        'that far before we start.
        Dim stackskip As Integer = 0
        If recstage IsNot Nothing AndAlso Not recstage.GetSubSheetID().Equals(relstage.GetSubSheetID()) Then
            Dim found As Boolean = False
            For Each st As Guid In subsheetStack
                stackskip += 1
                If GetStage(st).GetSubSheetID() = recstage.GetSubSheetID() Then
                    found = True
                    Exit For
                End If
            Next
            If Not found Then
                'The circumstances in which this occur will be similar to those reported under
                'bug #4729.
                Throw New InvalidOperationException(My.Resources.Resources.clsProcess_AStackImbalanceHasOccurredCheckYourProcessForErrors)
            End If
        End If

        'Determine our starting point and whether we want to find the recovery stage
        'at the same level in the hierarchy as that, or one level higher.
        Dim srcstage As clsProcessStage
        Dim higher As Boolean
        If recstage Is Nothing Then
            srcstage = relstage
            higher = False
        Else
            srcstage = recstage
            higher = True
        End If

        'Get recovery stages on the same page...
        Dim ra As List(Of clsProcessStage)
        ra = GetStagesBySubsheetAndType(relstage.GetSubSheetID(), StageTypes.Recover)

        'If we're in a block, look for a recovery stage there...
        Dim thisblock As clsBlockStage = GetBlockContainingStage(relstage)
        If thisblock IsNot Nothing Then
            For Each r As clsRecoverStage In ra
                If GetBlockContainingStage(r) Is thisblock Then
                    If higher Then
                        higher = False
                        Exit For
                    Else
                        While stackskip > 0
                            subsheetStack.Pop()
                            stackskip -= 1
                        End While
                        Return r
                    End If
                End If
            Next
        End If

        'Look for a recovery stage on the same page...
        If Not higher Then
            For Each r As clsRecoverStage In ra
                If GetBlockContainingStage(r) Is Nothing Then
                    While stackskip > 0
                        subsheetStack.Pop()
                        stackskip -= 1
                    End While
                    Return r
                End If
            Next
        End If

        'Look up the subsheet stack until we find one...
        Dim levelsup As Integer = 1
        For Each id As Guid In subsheetStack
            If levelsup > stackskip Then
                Dim deststage As clsProcessStage = GetStage(id)
                'Get all recovery stages on this page...
                ra = GetStagesBySubsheetAndType(deststage.GetSubSheetID(), StageTypes.Recover)
                Dim foundstage As clsProcessStage = Nothing
                'See if the stage that made the call to the lower level page was inside
                'a block, and if so, search for a recovery stage inside that same block...
                Dim destblock As clsBlockStage = GetBlockContainingStage(deststage)
                If destblock IsNot Nothing Then
                    For Each r As clsRecoverStage In ra
                        If GetBlockContainingStage(r) Is destblock Then
                            foundstage = r
                            Exit For
                        End If
                    Next
                End If
                'If the calling stage wasn't in a block, or if it was but there was no
                'recovery stage in that block, look for a recovery stage at page level (i.e.
                'not in a block)...
                If foundstage Is Nothing Then
                    For Each r As clsRecoverStage In ra
                        If GetBlockContainingStage(r) Is Nothing Then
                            foundstage = r
                            Exit For
                        End If
                    Next
                End If

                'If we found an appropriate recovery stage this time around, adjust the
                'subsheet stack accordingly and return the found stage...
                If foundstage IsNot Nothing Then
                    While levelsup > 0
                        subsheetStack.Pop()
                        levelsup -= 1
                    End While
                    Return CType(foundstage, clsRecoverStage)
                End If

            End If
            'Keep track of how many levels up we have gone...
            levelsup += 1
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' Gets or sets a stop request requested on this process or, more accurately, on
    ''' the entry process in this session. The stop request flag is reset on each
    ''' step within the process, so this indicates the state of the stop request
    ''' since the current step started. It has no meaning outside a step being
    ''' executed.
    ''' </summary>
    Public Property ImmediateStopRequested() As Boolean
        Get
            If ParentProcess IsNot Nothing _
             Then Return ParentProcess.ImmediateStopRequested
            Return mImmediateStopRequested
        End Get
        Set(ByVal value As Boolean)
            If ParentProcess IsNot Nothing Then
                ParentProcess.ImmediateStopRequested = value
            Else
                mImmediateStopRequested = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Runs through the next step (stage) of the process. On entry, we expect the
    ''' Run Mode to be either Stepping, SteppingOver or Running. On exit, it could
    ''' be: Running, Completed, Failed or Paused.
    ''' </summary>
    ''' <param name="breakpointInfo">On return, contains either Nothing or
    ''' information about a breakpoint triggered during the execution of this step.
    ''' </param>
    ''' <param name="bPauseWhenBreakpointRaised">A flag specifying whether to pause
    ''' when a breakpoint is raised.</param>
    ''' <returns>A clsProcessStage.Result</returns>
    ''' <remarks>This method is not used directly, but indirectly invoked by a client
    ''' of APC via the RunAction method.</remarks>
    Private Function RunStep(ByRef breakpointInfo As clsProcessBreakpointInfo, ByVal bPauseWhenBreakpointRaised As Boolean) As StageResult

        ' If stop was requested before, by running another step, the implication is
        ' that it is no longer requested, so reset it.
        ImmediateStopRequested = False

        Dim res As StageResult = New StageResult(False, "Internal", My.Resources.Resources.clsProcess_NoResultWasGeneratedThisShouldNeverHappen)

        'Store current stage before we do anything, so we can detect if it has
        'changed afterwards.
        Dim gPrevStageID As Guid = RunStageID
        Dim loggingError As Boolean = False

        Try
            Select Case mState
                Case ProcessRunState.Stepping, ProcessRunState.SteppingOver, ProcessRunState.Running

                    'It should not happen that there is no current stage at this point...
                    If RunStageID.Equals(Guid.Empty) Then
                        RunState = ProcessRunState.Failed
                        res = New StageResult(False, "Internal", My.Resources.Resources.clsProcess_ThereIsNoCurrentStage)
                        GoTo exiterror
                    End If

                    'Get a reference to the current stage, making sure it is actually still
                    'valid in case the user has deleted it. (Although really I would hope we
                    'would deal with that at the point where the stage is deleted!)
                    Dim objStage As clsProcessStage
                    objStage = GetStage(RunStageID)
                    objStage.DateTimeStarted = DateTimeOffset.Now
                    If objStage Is Nothing Then
                        RunState = ProcessRunState.Failed
                        RunStageID = Guid.Empty
                        res = New StageResult(False, "Internal", My.Resources.Resources.clsProcess_TheCurrentStageHasBeenDeleted)
                        GoTo exiterror
                    End If

                    'Data items etc have the opportunity to raise a breakpoint
                    'during this stage, if they are read, modified etc
                    'A change in the member mRaisedBreakpoint indicates
                    'that this has happened.
                    mRaisedBreakPointInfo = Nothing

                    'Execute the actual logic for the stage...
                    Try
                        res = objStage.Execute(RunStageID, Logger)
                    Catch lfe As LogFailedException
                        ' We don't want to try and recover from a log failure; we
                        ' want to terminate the session so pass to the outer handler
                        Throw
                    Catch ex As PermissionException
                        res = New StageResult(False, "Internal", "Current user is not permitted to perform this action")
                    Catch ex As Exception
                        res = New StageResult(False, "Internal", My.Resources.Resources.clsProcess_UnexpectedError & ex.Message)
                    End Try

                    If Not res.Success Then

                        'An exception occurred while running the stage. See if we can
                        'handle it locally before we let it actually happen. Note that
                        'by passing mRecoveryMode in as the 'higher' parameter to
                        'FindRecoveryStage, we cause the error to propagate up another
                        'level if we are already in recovery mode.
                        Dim recstage As clsRecoverStage
                        If mRecoveryMode Then
                            recstage = FindRecoveryStage(objStage, mRecoverStart)
                        Else
                            recstage = FindRecoveryStage(objStage, Nothing)
                        End If
                        If recstage IsNot Nothing AndAlso Not recstage.AttemptsExceeded Then

                            objStage.LogError(res.GetText(), Logger)
                            'Set recovery type and detail...
                            mRecoveryType = res.ExceptionType
                            mRecoveryDetail = res.ExceptionDetail
                            'Set the rest of the recovery mode information...
                            mRecoveryMode = True
                            mRecoverStart = recstage
                            mRecoverySource = objStage.GetStageID()
                            RunStageID = recstage.GetStageID()

                            'Break on the exception if that is enabled...
                            If bPauseWhenBreakpointRaised AndAlso BreakOnHandledException Then
                                Dim b As New clsProcessBreakpoint(objStage)
                                mRaisedBreakPointInfo = New clsProcessBreakpointInfo(b, clsProcessBreakpoint.BreakEvents.HandledException, Nothing)
                            End If

                            'Because we handled the exception, we have now run the stage
                            'successfully - we don't want to return an error!
                            res = New StageResult(True)
                            GoTo exitok
                        End If

                        'We couldn't handle the exception, so let the error propagate...
                        GoTo exiterror
                    End If

                Case Else
                    Debug.Assert(False, My.Resources.Resources.clsProcess_RunStepCalledWhenInAnUnexpectedState)

            End Select
        Catch lfe As LogFailedException
            loggingError = True
            res = StageResult.InternalError(My.Resources.Resources.clsProcess_LoggingFailed & lfe.Message)
            GoTo exiterror

        Catch e As Exception
            res = New StageResult(False, "Internal", My.Resources.Resources.clsProcess_RunStep_Exception & e.Message)
            GoTo exiterror
        End Try

exitok:
        'Breakpoint info should be null on return unless breakpoint encountered
        breakpointInfo = Nothing

        Try

            'Stop if a break was raised externally (eg by data item),
            'and return info about that breakpoint
            If mState = ProcessRunState.Running OrElse mState = ProcessRunState.Stepping OrElse mState = ProcessRunState.SteppingOver Then
                If bPauseWhenBreakpointRaised Then
                    If Not mRaisedBreakPointInfo Is Nothing Then
                        breakpointInfo = mRaisedBreakPointInfo
                        RunState = ProcessRunState.Paused
                        mRaisedBreakPointInfo = Nothing
                    End If
                End If
            End If

            'Stop if we have reached a breakpoint, and return info about that breakpoint
            If mState = ProcessRunState.Running OrElse mState = ProcessRunState.Stepping OrElse mState = ProcessRunState.SteppingOver Then
                If bPauseWhenBreakpointRaised And RunStageID <> gPrevStageID Then
                    Dim nextStg As clsProcessStage = GetStage(RunStageID)
                    If nextStg.Id = RunToStageId Then
                        breakpointInfo = New clsProcessBreakpointInfo(
                         New clsProcessBreakpoint(nextStg), clsProcessBreakpoint.BreakEvents.Transient, Nothing)
                        RunState = ProcessRunState.Paused
                    End If
                    If nextStg.HasBreakPoint() Then
                        Try
                            If nextStg.BreakPoint.IsConditionMet(Me) Then
                                breakpointInfo = New clsProcessBreakpointInfo(nextStg.BreakPoint, clsProcessBreakpoint.BreakEvents.WhenConditionMet, Nothing)
                                RunState = ProcessRunState.Paused
                            End If
                        Catch ex As Exception
                            'Error checking breakpoint, so raise the breakpoint to bring attention
                            'to breakpoint error. Probably a problem with the user's expression
                            breakpointInfo = New clsProcessBreakpointInfo(nextStg.BreakPoint, clsProcessBreakpoint.BreakEvents.WhenConditionMet, ex.Message)
                            RunState = ProcessRunState.Paused
                        End Try
                    End If
                End If
            End If

            If mState = ProcessRunState.Stepping OrElse mState = ProcessRunState.SteppingOver Then
                RunState = ProcessRunState.Paused
            End If

        Catch ex As Exception
            res = New StageResult(False, "Internal", My.Resources.Resources.clsProcess_RunStep_Exception & ex.Message)
            GoTo exiterror
        End Try

        Return res

exiterror:
        Try
            ' No point in attempting to log it if we are terminating the session
            ' due to logging not working... the error should be stored into the
            ' windows event log anyway, pointless trying to duplicate it.
            If Not loggingError Then GetStage(RunStageID).LogError(res.GetText(), Logger)

        Catch ex As Exception
            'We're already reporting an error, so there's little point reporting another
            'at this stage and masking the original.

        End Try

        'If we are failing, clear the subsheet stack. If this is a business object or subprocess, the
        'caller could catch the exception then make another call here. See bug #4418 for example.
        'Only do this if we're actually running though, since if we're stepping we will remain where
        'we are and need to keep the context.
        If (mState = ProcessRunState.Running) Then mSubSheetStack.Clear()

        RunState = ProcessRunState.Failed

        Return res

    End Function

    ''' <summary>
    ''' Set the value of a field in the current row of a collection stage.
    ''' </summary>
    ''' <param name="map">The name of the collection stage and field, e.g.
    ''' "MyCol.Field2"</param>
    ''' <param name="value">The value to set. The instance can be amended, for
    ''' example, by casting the data to another type</param>
    ''' <param name="scopeStage">The stage that specifies the scope for this 
    ''' operation</param>
    ''' <exception cref="NoSuchStageException">If no collection stage with the name
    ''' provided in <paramref name="map"/> could be found</exception>
    ''' <exception cref="OutOfScopeException">If the referenced collection stage was
    ''' not in scope of the given <paramref name="scopeStage"/></exception>
    ''' -----------------------------------------------------------------------------
    Private Sub SetCollectionField(map As String, value As clsProcessValue,
                                       scopeStage As clsProcessStage)

        Dim coll As (Stage As String, Fields As String) = clsCollection.SplitPath(map)

        Dim outOfScope As Boolean
        Dim stg = GetCollectionStage(coll.Stage, scopeStage, outOfScope)

        If outOfScope Then Throw New OutOfScopeException(
         My.Resources.Resources.clsProcess_CanTAccessCollection0OutOfScope, coll.Stage)

        If stg Is Nothing Then Throw New NoSuchStageException(
         My.Resources.Resources.clsProcess_CouldNotFindReferencedCollection0, coll.Stage)

        stg.Value.Collection.SetField(coll.Fields, value, True)
    End Sub

    Friend Function ReadDataItemsFromParameters(ByVal parameters As List(Of clsProcessParameter), ByVal stage As clsProcessStage,
                                                ByRef arguments As clsArgumentList, ByRef sErr As String, ByVal allowScopeChange As Boolean,
                                                ByVal isOutput As Boolean) As Boolean
        Return mSchema.ReadDataItemsFromParameters(parameters, stage, arguments, sErr, allowScopeChange, isOutput)
    End Function

    ''' <summary>
    ''' Store a value in a 'data item'.
    ''' </summary>
    ''' <param name="itemName">The name of the data item, e.g. "MyData 1". Could
    ''' also be a collection field, e.g. MyCollection.MyField". An error is thrown if
    ''' this target does not exist.</param>
    ''' <param name="val">The value to be stored, as a clsProcessValue. This
    ''' instance may be amended - e.g. cast to a different type</param>
    ''' <param name="scopeStage">The stage that defines the scope for this
    ''' operation.</param>
    ''' <param name="sErr">On failure, contains an error description</param>
    ''' <returns>True if successful, False otherwise</returns>
    Friend Function StoreValueInDataItem(ByVal itemName As String, ByVal val As clsProcessValue, ByVal scopeStage As clsProcessStage, ByRef sErr As String) As Boolean

        ' If we're storing in a collection field, that's a whole different way of storing the value
        If itemName.IndexOf("."c) > 0 Then
            Try
                SetCollectionField(itemName, val, scopeStage)
                Return True
            Catch ex As Exception
                sErr = ex.Message
                Return False
            End Try
        End If

        ' Get the stage
        Dim outOfScope As Boolean
        Dim stg As clsDataStage = GetDataStage(itemName, scopeStage, outOfScope)

        ' If it couldn't be found, error
        If stg Is Nothing Then
            sErr = String.Format(My.Resources.Resources.clsProcess_Stage0DoesNotExist, itemName)
            Return False

        ElseIf outOfScope Then ' If the stage is out of scope - error out.
            sErr = String.Format(My.Resources.Resources.clsProcess_CannotStoreOutputIn0BecauseItIsOnAnotherPageAndHasBeenHidden, itemName)
            Return False

        ElseIf stg.StageType = StageTypes.Collection Then
            ' Otherwise, it exists and is in scope
            ' If it's a collection (i.e. we're setting a collection in a stage, not
            ' setting a value in a collection) check the definition
            Dim colstage As clsCollectionStage = CType(stg, clsCollectionStage)
            Dim def As clsCollectionInfo = colstage.Definition
            'check the collection's value matches its definition.
            If def IsNot Nothing Then
                val = CoerceCollection(def, val, sErr)
                If val Is Nothing Then
                    sErr = String.Format(My.Resources.Resources.clsProcess_CollectionFieldsAreDefinedButCannotBeMatchedToTheIncomingCollection0, sErr)
                    Return False
                End If
            End If

        End If

        'Cast value if necessary, fail if the cast is attempted and fails.
        Try
            val = val.CastInto(stg.DataType)
        Catch ex As Exception
            sErr = String.Format(My.Resources.Resources.clsProcess_FailedToTranslateData0, ex.Message)
            Return False
        End Try

        ' Disable attempts to set env var values.
        If stg.Exposure = StageExposureType.Environment Then
            sErr = My.Resources.Resources.clsProcess_CannotSetTheValueOnAnEnvironmentVariable
            Return False
        End If

        'Null values should not reach this far, but a strange bug fix to #1092 seems
        'to have thought they should, so they are dealt with as follows for now:-
        'This is here (rather than beginning of method) in order to ensure that other
        'errors (eg setting an env var, out of scope 'store in' variables etc) are
        'caught and reported even when a null value is being 'set'.
        If val.IsNull Then Return True

        'Update the value...
        stg.SetValue(val)

        If stg.Exposure = StageExposureType.Statistic Then
            If Not UpdateStatistic(stg.GetName, stg.GetValue, sErr) Then
                sErr = String.Format(My.Resources.Resources.clsProcess_ErrorUpdatingStatistic0, stg.GetName) & sErr
                Return False
            End If
        End If

        Return True

    End Function

    ''' <summary>
    ''' Populate data items based on the supplied inputs/outputs xml.
    ''' </summary>
    ''' <param name="scopeStage">The stage that defines the scope for this
    ''' operation.</param>
    ''' <param name="Arguments">The input/output arguments</param>
    ''' <param name="sErr">On failure, this contains an error code.</param>
    ''' <param name="bOutput">True if the XML data is in 'outputs' format, otherwise
    ''' False for 'inputs' format.</param>
    ''' <remarks>Used, for example, when an action returns and the values need to be
    ''' stored in data items, or when inputs supplied to a process and data items
    ''' need to be populated.</remarks>
    ''' <returns>True if successful, False otherwise</returns>
    Friend Function StoreParametersInDataItems(ByVal scopeStage As clsProcessStage, ByVal Arguments As clsArgumentList, ByRef sErr As String, Optional ByVal bOutput As Boolean = True) As Boolean

        If Arguments Is Nothing Then
            sErr = My.Resources.Resources.clsProcess_OutputsFromExternalProcessWereEmpty
            Return False
        End If

        For Each arg As clsArgument In Arguments

            For Each param As clsProcessParameter In scopeStage.GetParameters
                If arg.Name = param.Name Then
                    If param.Direction = ParamDirection.In AndAlso bOutput Then
                        Continue For
                    End If

                    If param.Direction = ParamDirection.Out AndAlso Not bOutput Then
                        Continue For
                    End If

                    If param.Direction = ParamDirection.None Then
                        sErr = My.Resources.Resources.clsProcess_ParameterIsNotAnInOrOutParameter
                        Return False
                    End If


                    If Not param.GetMapType = MapType.None Then

                        If Not param.GetMapType = MapType.Stage Then
                            sErr = My.Resources.Resources.clsProcess_OutputParametersCanOnlyMapToStages
                            Return False
                        End If

                        Dim objValue As clsProcessValue = arg.Value

                        'Now set the value...
                        If Not String.IsNullOrEmpty(param.GetMap) Then
                            If Not StoreValueInDataItem(param.GetMap(), objValue, scopeStage, sErr) Then Return False
                        End If
                    End If
                End If
            Next
        Next

        Return True

    End Function

    ''' <summary>
    ''' Prepares a collection to be started into a destination. The definition of the
    ''' collection must match that of the destination (in which case the passed
    ''' collection will be returned verbatim), OR it must at least have fields that
    ''' can be cast to the destination's (in that case, a new collection is returned).
    ''' </summary>
    ''' <param name="destColInfo">The definition of the destination collection</param>
    ''' <param name="col">The value of the collection</param>
    ''' <param name="sErr">On return, if the collection can't be stored (i.e. when
    ''' Nothing is returned), this contains the reason why, for presentation to the
    ''' user.</param>
    ''' <returns>The coerced collection (either new, or the same as the input), or
    ''' Nothing if the collection can't be stored in the destination.</returns>
    Private Function CoerceCollection(ByVal destColInfo As clsCollectionInfo, ByVal col As clsProcessValue, ByRef sErr As String) As clsProcessValue

        'We can store a null collection anywhere...
        If col.Collection Is Nothing Then Return col

        col = col.Clone()
        For Each colRow As clsCollectionRow In col.Collection.Rows
            'Even though we are only modifying values, doing so will block
            'further iteration of the keys (field names) so we need to loop
            'on a copy of them.
            Dim fn As New List(Of String)(colRow.FieldNames)
            For Each field As String In fn

                If Not destColInfo.Contains(field) Then
                    sErr = String.Format(My.Resources.Resources.clsProcess_TheCollectionDefinitionDoesNotContainTheField0, field)
                    Return Nothing
                End If

                Dim fieldVal As clsProcessValue = colRow(field)
                Dim fieldInfo As clsCollectionFieldInfo = destColInfo.GetField(field)
                If fieldVal.DataType <> fieldInfo.DataType Then
                    If clsProcessDataTypes.CanCast(fieldVal.DataType, fieldInfo.DataType) Then
                        'Note that all the rows should get re-cast on the first
                        'time around the loop, leaving nothing to be done on
                        'subsquent loops.
                        col.Collection.ChangeFieldDataType(field, fieldInfo.DataType)
                    Else
                        sErr = String.Format(My.Resources.Resources.clsProcess_TheCollectionDefinitionOfField0IsOfDatatype1WhichDoesNotMatchTheIncomingFieldOf, field, clsProcessDataTypes.GetFriendlyName(fieldVal.DataType), clsProcessDataTypes.GetFriendlyName(fieldInfo.DataType))
                        Return Nothing
                    End If
                End If

                If fieldInfo.DataType = DataType.collection Then
                    If fieldInfo.HasChildren Then 'Only if the nested collection is defined
                        Dim fcol As clsProcessValue = CoerceCollection(fieldInfo.Children, fieldVal, sErr)
                        If fcol Is Nothing Then Return Nothing
                        colRow(field) = fcol
                    End If
                End If

            Next
        Next

        Return col

    End Function

    ''' <summary>
    ''' Sets the run stage, i.e. the next stage that will be executed in debug mode.
    ''' </summary>
    ''' <param name="gStageID">The ID of the stage to set as the next run stage.</param>
    ''' <param name="sErr">In the event of failure, this contains an error message.</param>
    ''' <returns>True if successful, False otherwise.</returns>
    ''' <remarks>If the process is not currently running, it is moved into run mode first.
    ''' The running state will be set to stepping after any call.</remarks>
    Public Function SetRunStage(ByVal gStageID As Guid, ByRef sErr As String) As Boolean

        'Get the stage requested...
        Dim objStage As clsProcessStage = GetStage(gStageID)
        If objStage Is Nothing Then
            sErr = My.Resources.Resources.clsProcess_MissingStageForSetRunStage
            Return False
        End If

        'Set for runnable stages, reject otherwise...
        Select Case objStage.StageType
            Case StageTypes.Data, StageTypes.Collection, StageTypes.ProcessInfo, StageTypes.SubSheetInfo, StageTypes.Undefined
                sErr = My.Resources.Resources.clsProcess_CannotSetNextStageThatKindOfStage
                Return False
            Case Else
                If RunState = ProcessRunState.Off Then
                    Dim res As StageResult = RunAction(ProcessRunAction.StepIn)
                    If Not res.Success Then
                        sErr = res.GetText()
                        Return False
                    End If
                End If
                RunStageID = objStage.GetStageID
                mState = ProcessRunState.Paused
                Return True
        End Select

    End Function

    ''' <summary>
    ''' Check a 'store in' value for errors.
    ''' </summary>
    ''' <param name="storein">The 'store in' value - i.e. the name of the destination
    ''' to be stored in - usually a Data Stage name.</param>
    ''' <param name="valuetype">The DataType of that value being stored. If this is
    ''' DataType.unknown, type checking is skipped.</param>
    ''' <param name="scopeStage">The stage to be used as a scope stage, if any.</param>
    ''' <param name="loc">The location of the error. This can either be an empty
    ''' string, or a description in the form, for example, " in row 1". Note the
    ''' leading space!</param>
    ''' <param name="colInfo">Optionally, details of the parameter's defined
    ''' collection fields. If this is given, instead of Nothing, then a) the target
    ''' must be a collection, and b) it must have fields matching these!</param>
    ''' <param name="reading">If this is True, we are actually reading, not storing,
    ''' so some things may be allowed that would not be for writing.</param>
    ''' <param name="isParam">If True, it's legal to have no stage specified
    ''' at all here. Example - an output parameter from an action - you don't *have*
    ''' to store it somewhere.</param>
    ''' <returns>Returns a list of errors, which may be empty, but not null.</returns>
    Public Function CheckStoreInForErrors(ByVal storein As String, ByVal valuetype As DataType,
     ByVal scopeStage As clsProcessStage, ByVal loc As String,
     Optional ByVal colInfo As clsCollectionInfo = Nothing,
     Optional ByVal reading As Boolean = False, Optional ByVal isParam As Boolean = False) _
      As ValidationErrorList

        Dim errors As New ValidationErrorList

        ' Check if there's any stage referenced at all
        If storein = "" Then
            If isParam Then ' Valid for output params, but raise a warning just in case
                errors.Add(New ValidateProcessResult(scopeStage, 135, loc))

            Else ' Not at all valid for calc stages - or non-parameters
                errors.Add(New ValidateProcessResult(scopeStage, 13, loc))

            End If
        Else
            Dim stagename As String = storein
            Dim fieldname As String = Nothing

            Dim offset As Integer = stagename.IndexOf("."c)
            If offset <> -1 Then
                fieldname = stagename.Substring(offset + 1)
                stagename = stagename.Substring(0, offset)
            End If
            Dim outOfScope As Boolean
            Dim dest As clsDataStage = GetDataStage(stagename, scopeStage, outOfScope)

            If dest Is Nothing Then
                ' This is an error, regardless of whether it's a param or not - it's
                ' not that the "store in" ("read from" if reading) value is missing,
                ' it's that it's there, but the stage it refers to doesn't exist
                If reading _
                 Then errors.Add(New ValidateProcessResult(scopeStage, 14, loc)) _
                 Else errors.Add(New ValidateProcessResult(scopeStage, 13, loc))

            Else
                If dest.StageType <> StageTypes.Data AndAlso dest.StageType <> StageTypes.Collection Then
                    errors.Add(New ValidateProcessResult(scopeStage, 12, loc))
                Else
                    If outOfScope Then
                        errors.Add(New ValidateProcessResult(scopeStage, 11, loc))
                    Else
                        Dim destType As DataType = EstablishStoreInDataType(scopeStage, loc, errors, fieldname, dest)

                        If colInfo Is Nothing Then

                            'Validate the datatype if possible...
                            If valuetype <> DataType.unknown Then
                                If destType <> DataType.unknown Then
                                    Dim val As New clsProcessValue(valuetype)
                                    Dim sErr As String = Nothing
                                    If Not val.CanCastInto(destType) Then _
                                     errors.Add(scopeStage, 9, loc)
                                End If
                            End If

                        Else
                            Dim reason As String = Nothing
                            If Not colInfo.CanMapInto(CType(dest, clsCollectionStage).Definition, reason, My.Resources.Resources.clsProcess_TheOutputParameterSCollection, My.Resources.Resources.clsProcess_TheTargetStoreInCollection) Then
                                errors.Add(New ValidateProcessResult(scopeStage, 41, loc, reason))
                            End If
                        End If

                        'Make sure we're not trying to write to an environment variable, but
                        'that's allowed in we're reading, not writing...
                        If Not reading AndAlso dest.Exposure = StageExposureType.Environment Then
                            errors.Add(New ValidateProcessResult(scopeStage, 7, loc))
                        End If

                    End If
                End If
            End If
        End If

        Return errors

    End Function

    Private Function EstablishStoreInDataType(ByVal scopeStage As clsProcessStage, ByVal locationOfError As String,
                                              ByVal errors As ValidationErrorList, ByVal fieldNamePath As String,
                                              ByVal destinationStage As clsDataStage) As DataType
        Dim destinationType As DataType = destinationStage.GetDataType()
        If destinationType = DataType.collection AndAlso destinationStage.StageType = StageTypes.Collection AndAlso fieldNamePath IsNot Nothing Then
            If CType(destinationStage, clsCollectionStage).Definition Is Nothing Then
                'Dynamic collection, so we can't verify the field name
                destinationType = DataType.unknown
            Else
                Dim fields = Split(fieldNamePath, "."c)
                Dim currentField = CType(destinationStage, clsCollectionStage).Definition

                For Each field As String In fields
                    Dim destinationField As clsCollectionFieldInfo = currentField.GetField(field)
                    If destinationField Is Nothing Then
                        errors.Add(New ValidateProcessResult(scopeStage, 10, field, locationOfError))
                        'We already know it's wrong, so we
                        'won't do the next bit of verification
                        destinationType = DataType.unknown
                    Else
                        destinationType = destinationField.DataType()
                        currentField = destinationField.Children
                    End If
                Next
            End If
        End If

        Return destinationType
    End Function

    ''' <summary>
    ''' Get the current stage for test/debug mode
    ''' </summary>
    ''' <returns>The stage ID, or an empty Guid if there isn't one</returns>
    Public Function GetTestCurStageID() As Guid
        If mState = ProcessRunState.Off Then
            GetTestCurStageID = Guid.Empty
        Else
            GetTestCurStageID = RunStageID
        End If
    End Function


    ''' <summary>
    ''' Get the extent of the process.
    ''' </summary>
    ''' <param name="rectExtent"></param>
    ''' <param name="gSubSheetID"></param>
    ''' <remarks>
    ''' This currently calculates on every call, where really it could
    ''' cache and re-calculate only when the process has changed. Be
    ''' aware of the performance implications when making use of this
    ''' method.
    ''' </remarks>
    Public Sub GetExtent(ByRef rectExtent As Rectangle, ByRef gSubSheetID As Guid)

        'Calculate extent. Should really only do this when the
        'process changes, since it could take a while for a large
        'process, and this function is being used frequently.
        Dim xmin As Single, xmax As Single, ymin As Single, ymax As Single
        Dim x As Single, y As Single, w As Single, h As Single
        Dim i As Integer
        Dim objStage As clsProcessStage
        xmin = Single.MaxValue
        xmax = Single.MinValue
        ymin = Single.MaxValue
        ymax = Single.MinValue
        For i = 0 To mSchema.Stages.Count - 1
            objStage = mSchema.Stages(i)
            If objStage.GetSubSheetID().Equals(gSubSheetID) Then
                x = objStage.GetDisplayX()
                y = objStage.GetDisplayY()
                w = objStage.GetDisplayWidth() / 2
                h = objStage.GetDisplayHeight() / 2
                If x - w < xmin Then xmin = x - w
                If x + w > xmax Then xmax = x + w
                If y - h < ymin Then ymin = y - h
                If y + h > ymax Then ymax = y + h
            End If
        Next

        'Push the extent beyond what it actually is, which will allow
        'some room for adding new items around the edge. Once these
        'are added, the view range will expand still further.
        xmin = xmin - 200
        xmax = xmax + 200
        ymin = ymin - 200
        ymax = ymax + 200

        'Apply a minimum size to the 'page'...
        If xmin > -1000 Then xmin = -1000
        If xmax < 1000 Then xmax = 1000
        If ymin > -1000 Then ymin = -1000
        If ymax < 1000 Then ymax = 1000

        rectExtent.X = CInt(xmin)
        rectExtent.Y = CInt(ymin)
        rectExtent.Width = CInt(xmax - xmin + 1)
        rectExtent.Height = CInt(ymax - ymin + 1)

    End Sub

    ''' <summary>
    ''' Reset the current value of all data items to the initial value.
    ''' This is called during InitialiseData, as part of the Start stage, and
    ''' also when the restart button is pressed.
    ''' </summary>
    ''' <param name="subsheetId">If specified, resetting is restricted to data items
    ''' on this subsheet which want resetting when the page executes. Normally though
    ''' Guid.Empty is specified, meaning reset everything.</param>
    Friend Sub ResetDataItemCurrentValues(ByVal subsheetId As Guid)
        For Each st As clsProcessStage In mSchema.Stages
            If subsheetId.Equals(Guid.Empty) OrElse st.GetSubSheetID().Equals(subsheetId) Then
                Dim dataStg As clsDataStage = TryCast(st, clsDataStage)
                If dataStg IsNot Nothing AndAlso (subsheetId.Equals(Guid.Empty) OrElse dataStg.AlwaysInit) Then
                    dataStg.ResetValue()
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Prepares the process ready for running.
    ''' </summary>
    ''' <remarks>Eg performs jobs such as creating new
    ''' instances of all business objects.</remarks>
    Private Sub InitialiseForRunning()
        mSubSheetStack = New Stack(Of Guid)
        mOutputParameters = Nothing
        mChildWaitProcess = Nothing

        DisposeObjects()

        'On start of a new scope, get references to used objects (not loaded yet)
        If ProcessType = DiagramType.Process OrElse ParentProcess Is Nothing Then _
            GetBusinessObjects(True)
    End Sub

    Private Sub DisposeObjects()
        mObjRefs?.Dispose()
        mObjRefs = Nothing
        mAllObjRefs?.Dispose()
        mAllObjRefs = Nothing
        mSkills = Nothing
    End Sub

    ''' <summary>
    ''' Get the current camera X position.
    ''' </summary>
    ''' <returns></returns>
    Public Function GetCameraX() As Single
        'The main page now has its own sheet
        'and corresponding ID so this will work
        Return GetActiveSubSheetRef().CameraX
    End Function

    ''' <summary>
    ''' Get the current camera Y position.
    ''' </summary>
    ''' <returns></returns>
    Public Function GetCameraY() As Single
        'The main page now has its own sheet
        'and corresponding ID so this will work
        Return GetActiveSubSheetRef().CameraY
    End Function

    ''' <summary>
    ''' Gets or sets the current camera zoom.
    ''' </summary>
    ''' <returns></returns>
    Public Property Zoom() As Single
        Get
            'The main page now has its own sheet
            'and corresponding ID so this will work
            Return ActiveSubSheet.Zoom
        End Get
        Set(value As Single)
            'The main page now has its own sheet
            'and corresponding ID so this will work sngCameraX = Location.X
            ActiveSubSheet.Zoom = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the current camera zoom as a percentage
    ''' </summary>
    Public Property ZoomPercent() As Integer
        Get
            Return ActiveSubSheet.ZoomPercent
        End Get
        Set(value As Integer)
            ActiveSubSheet.ZoomPercent = value
        End Set
    End Property

    ''' <summary>
    ''' Increments the zoom value of the active subsheet, returning the resultant
    ''' percentage. Note that this increases the zoom in 25% steps (eg. 100% ->
    ''' 125% -> 150% -> 175%; not increasing cumulatively by 25%).
    ''' </summary>
    ''' <returns>The new zoom percentage value after being incremented by 1 step
    ''' </returns>
    Public Function IncrementZoom() As Integer
        ' Increment up to the next 25% step
        ' Note - 25% is chosen because the scale factor equivalent of 0.25 is exactly
        ' representable as a floating point value - ie. no rounding errors
        Dim newPC As Integer = GetNextNumberDivisibleBy(ZoomPercent + 1, 25)
        If newPC > MaximumZoomPercent Then newPC = MaximumZoomPercent
        ZoomPercent = newPC
        Return newPC
    End Function

    ''' <summary>
    ''' Decrements the zoom value of the active subsheet, returning the resultant
    ''' percentage. Note that this decreases the zoom in 25% steps (eg. 150% ->
    ''' 125% -> 100%; not decreasing cumulatively by 25%), down to a minimum value of
    ''' 1% (see <see cref="MinimumZoomPercent"/>).
    ''' </summary>
    ''' <returns>The new zoom percentage value after being decremented by 1 step
    ''' </returns>
    Public Function DecrementZoom() As Integer
        ' Drop by 26 and get the next value divisible by 25 - basically drop to the
        ' next number *down* which is divisible by 25
        Dim newPC As Integer = GetNextNumberDivisibleBy(ZoomPercent - 26, 25)
        If newPC < MinimumZoomPercent Then newPC = MinimumZoomPercent
        ZoomPercent = newPC
        Return newPC
    End Function

    ''' <summary>
    ''' Gets the next number after value divisible by the given number.
    ''' </summary>
    ''' <param name="value">The value for which the next value is required.</param>
    ''' <param name="divisibleBy">The number that the returned value should be
    ''' divisible by.</param>
    ''' <returns>The first number following <paramref name="value"/> which is
    ''' divisible by <paramref name="divisibleBy"/></returns>
    Public Function GetNextNumberDivisibleBy(
     value As Integer, divisibleBy As Integer) As Integer
        Return ((value \ divisibleBy) + 1) * divisibleBy
    End Function

    ''' <summary>
    ''' Set the camera X position, in world coordinates
    ''' </summary>
    ''' <param name="v">Value.</param>
    Public Sub SetCameraX(ByVal v As Single)
        'The main page now has its own sheet
        'and corresponding ID so this will work
        GetActiveSubSheetRef().CameraX = v
    End Sub

    ''' <summary>
    ''' Set the camera Y position, in world coordinates.
    ''' </summary>
    ''' <param name="v">Value</param>
    Public Sub SetCameraY(ByVal v As Single)
        'The main page now has its own sheet
        'and corresponding ID so this will work
        GetActiveSubSheetRef().CameraY = v
    End Sub

    ''' <summary>
    ''' Sets the position of the camera, in world coordinates, for the current
    ''' page, as specified by GetActiveSubsheet()
    ''' </summary>
    ''' <param name="Location">The location.</param>
    Public Sub SetCameraLocation(ByVal Location As PointF)
        'The main page now has its own sheet
        'and corresponding ID so this will work sngCameraX = Location.X
        GetActiveSubSheetRef().CameraY = Location.Y
        GetActiveSubSheetRef().CameraX = Location.X
    End Sub

    ''' <summary>
    ''' Gets the position of the camera in world coordinates, for the current
    ''' page, as specified by GetActiveSubsheet()
    ''' </summary>
    ''' <returns>As summary.</returns>
    Public Function GetCameraLocation() As PointF
        Return GetActiveSubSheetRef().Camera
    End Function

    ''' <summary>
    ''' Gets the bounds of the viewport on the world.
    ''' </summary>
    ''' <param name="ViewPortSize">The size of the viewport in
    ''' non-native coordinates.</param>
    ''' <returns>Returns a rectangle whose location is the
    ''' top-left corner of the currently visible section
    ''' of the world. Coordinates of returned rectangle
    ''' are world coords.</returns>
    Public Function GetWorldViewPort(ByVal ViewPortSize As Size, Optional ByVal CustomZoomFactor As Single = -1) As RectangleF
        If CustomZoomFactor <= 0 Then
            CustomZoomFactor = Zoom
        End If

        Dim NativeViewPortSize As SizeF = New SizeF(ViewPortSize.Width / CustomZoomFactor, ViewPortSize.Height / CustomZoomFactor)
        Dim NativeViewPortLocation As PointF = New PointF(CSng(GetCameraX() - 0.5 * NativeViewPortSize.Width), CSng(GetCameraY() - 0.5 * NativeViewPortSize.Height))

        Return New RectangleF(NativeViewPortLocation, NativeViewPortSize)
    End Function


    ''' <summary>
    ''' Gets or sets the active subsheet in this process
    ''' </summary>
    Public Property ActiveSubSheet As clsProcessSubSheet
        Get
            Return GetSubSheetByID(mActiveSubsheetId)
        End Get
        Set(value As clsProcessSubSheet)
            mActiveSubsheetId = If(value Is Nothing, Guid.Empty, value.ID)
        End Set
    End Property

    ''' <summary>
    ''' Set the active subsheet (the subsheet currently being viewed, or to be
    ''' executed)
    ''' </summary>
    ''' <param name="gID">The ID of the subsheet</param>
    ''' <remarks>
    ''' </remarks>
    Public Sub SetActiveSubSheet(ByVal gID As Guid)
        mActiveSubsheetId = gID
    End Sub

    ''' <summary>
    ''' Get the active subsheet
    ''' </summary>
    ''' <returns>The ID of the subsheet being viewed/executed</returns>
    Public Function GetActiveSubSheet() As Guid
        Return mActiveSubsheetId
    End Function

    ''' <summary>
    ''' Get the active subsheet
    ''' </summary>
    ''' <returns>A reference to the subsheet being viewed/executed</returns>
    Public Function GetActiveSubSheetRef() As clsProcessSubSheet
        For Each objSub As clsProcessSubSheet In SubSheets
            If objSub.ID.Equals(mActiveSubsheetId) Then
                Return objSub
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Set the input parameters for this process. These are read internally whenever
    ''' the process moves from <see cref="ProcessRunState.Off"/> to anything else, so
    ''' this needs to be called before that happens.
    ''' </summary>
    ''' <param name="InputParams">The input parameters</param>
    Public Sub SetInputParams(ByVal InputParams As clsArgumentList)
        If ProcessType = DiagramType.Process Then
            mInputParameters = InputParams
        Else
            mRunInputs = InputParams
        End If
    End Sub

    ''' <summary>
    ''' Get the input parameters for this process.
    ''' </summary>
    ''' <returns>The input parameters</returns>
    Public Function GetInputParams() As clsArgumentList
        Return mInputParameters
    End Function

    ''' <summary>
    ''' Get the output parameters from the process. The "End" stage
    ''' must have executed to get any outputs!
    ''' </summary>
    Public Function GetOutputs() As clsArgumentList
        Return mOutputParameters
    End Function

    ''' <summary>
    ''' The environment variables for the current session this process is running.
    ''' </summary>
    Public ReadOnly Property EnvVars() As Dictionary(Of String, clsArgument)
        Get
            If ParentProcess IsNot Nothing Then Return ParentProcess.EnvVars
            Return mEnvVars
        End Get
    End Property

    Public Property Id As Guid
        Get
            Return mSchema.Id
        End Get
        Set(value As Guid)
            mSchema.Id = value
        End Set
    End Property

    ''' <summary>
    ''' The compound logging engine associated with this process.
    ''' </summary>
    Friend ReadOnly Property Logger() As CompoundLoggingEngine
        Get
            mLogger = If(mLogger, New CompoundLoggingEngine())
            Return mLogger
        End Get
    End Property

    Public Function GetLoopStart(ByVal objEnd As clsLoopEndStage) As clsLoopStartStage
        Return mSchema.GetGroupStage(Of clsLoopStartStage)(objEnd)
    End Function

    Public Function GetLoopEnd(ByVal objStart As clsLoopStartStage) As clsLoopEndStage
        Return mSchema.GetGroupStage(Of clsLoopEndStage)(objStart)
    End Function

    Public Function GetChoiceEnd(ByVal objStart As clsChoiceStartStage) As clsChoiceEndStage
        Return mSchema.GetGroupStage(Of clsChoiceEndStage)(objStart)
    End Function

    Public Function GetChoiceStart(ByVal objEnd As clsChoiceEndStage) As clsChoiceStartStage
        Return mSchema.GetGroupStage(Of clsChoiceStartStage)(objEnd)
    End Function

    ''' <summary>
    ''' Check for subsheets that don't have a valid clsProcessSubSheet
    ''' record. This is for backward compatibility only - all newly
    ''' saved processes will have these!
    ''' 
    ''' Note that this method performs the important job of adding 
    ''' a subsheet to represent the main page - something which was
    ''' simply inferred in older documents via a blank guid subsheet
    ''' reference.
    ''' </summary>
    Private Sub CreateMissingSubSheets()

        ' Go through all subsheet infos and make sure there is a corresponding
        ' subsheet object.
        For Each ssInfo As clsSubsheetInfoStage In GetStages(StageTypes.SubSheetInfo)
            Dim ssId As Guid = ssInfo.GetSubSheetID()
            If GetSubSheetByID(ssId) Is Nothing Then ' No sheet object found
                ' Create it
                Dim ss As New clsProcessSubSheet(Me)
                ' If no page id is set, it must be the main page of a legacy process
                If ssId = Nothing Then
                    ss.ID = Guid.Empty
                    ss.Name = MainPageName
                    ss.SheetType = SubsheetType.MainPage
                Else ' otherwise, inherit details from the info stage
                    ss.ID = ssId
                    ss.Name = ssInfo.Name
                End If
                ' Add the sheet and move on.
                SubSheets.Add(ss)
            End If
        Next

    End Sub

    ''' <summary>
    ''' Used after loading to create any missing group IDs on loop
    ''' stages - this will only happen for processes that were created
    ''' before these were introduced, so this code can be phased out
    ''' eventually.
    ''' 
    ''' On the other hand, it may be useful for correcting badly
    ''' formed xml.
    ''' </summary>
    ''' <returns>True if any old stuff was found</returns>
    Public Function CreateMissingGroupIDs() As Boolean
        Dim iStage As Integer
        Dim objStage As clsProcessStage, objCurStage As clsProcessStage
        Dim gCurStage As Guid
        Dim gGroupID As Guid
        Dim iNestLevel As Integer
        Dim bFoundAny As Boolean = False
        For iStage = 0 To mSchema.Stages.Count - 1
            objStage = mSchema.Stages(iStage)
            If objStage.StageType = StageTypes.LoopStart AndAlso CType(objStage, clsGroupStage).GetGroupID().CompareTo(Guid.Empty) = 0 Then
                'Found a loop start with no Group ID, so now find the
                'matching end using the same flawed method as we used
                'to!
                gGroupID = Guid.NewGuid()
                CType(objStage, clsGroupStage).SetGroupID(gGroupID)
                iNestLevel = 0
                gCurStage = GetNextStage(objStage.GetStageID(), LinkType.OnSuccess)
                Do
                    objCurStage = GetStage(gCurStage)
                    'PJW: The current stage could be empty so we need to check here!
                    If Not objCurStage Is Nothing Then
                        Select Case objCurStage.StageType
                            Case StageTypes.LoopStart
                                iNestLevel += 1
                                gCurStage = GetNextStage(gCurStage, LinkType.OnSuccess)
                            Case StageTypes.LoopEnd
                                iNestLevel -= 1
                                If iNestLevel < 0 Then
                                    CType(objCurStage, clsGroupStage).SetGroupID(gGroupID)
                                    bFoundAny = True
                                    Exit Do
                                End If
                                gCurStage = GetNextStage(gCurStage, LinkType.OnSuccess)
                            Case StageTypes.Decision
                                gCurStage = GetNextStage(gCurStage, LinkType.OnFalse)
                            Case Else
                                gCurStage = GetNextStage(gCurStage, LinkType.OnSuccess)
                        End Select
                        'Stop looking if we come to a dead end!
                        If gCurStage.CompareTo(Guid.Empty) = 0 Then
                            Exit Do
                        End If
                    Else
                        Exit Do
                    End If
                Loop
            End If
        Next
        Return bFoundAny
    End Function

    ''' <summary>
    ''' Determine if there are any system sheets missing and insert them if they are.
    ''' </summary>
    Private Sub CreateMissingSystemSheets()

        ' Main sheet - valid for both process and object
        If GetMainPage() Is Nothing Then AddMainSheet()

        ' Objects require a cleanup page
        If ProcessType = DiagramType.Object AndAlso GetCleanupPage() Is Nothing Then

            ' If it's not there, add it
            Dim ss As clsProcessSubSheet = AddSubSheet("Clean Up",
             15, -105, 15, 90, Guid.NewGuid(), False, SubsheetType.CleanUp, 1)

            ss.Published = True

            ' Link start to end.
            ss.StartStage.OnSuccess = ss.EndStage.Id

        End If

    End Sub

    ''' <summary>
    ''' Translate a position from 'world' to screen coordinates.
    ''' </summary>
    ''' <param name="sngX">The X position, updated on return</param>
    ''' <param name="sngY">The Y position, updated on return</param>
    Public Sub TranslateWorldToScreen(ByRef sngX As Single, ByRef sngY As Single, ByVal sngViewWidth As Single, ByVal sngViewHeight As Single, Optional ByVal CustomZoomFactor As Single = -1)
        'Move relative to camera position...
        Dim sngCamXS As Single, sngCamYS As Single
        Dim sngZoom As Single = CustomZoomFactor
        If sngZoom <= 0 Then
            sngZoom = Zoom
        End If

        sngCamXS = GetCameraX() * sngZoom
        sngCamYS = GetCameraY() * sngZoom
        sngX = sngX * sngZoom - sngCamXS + sngViewWidth / 2
        sngY = sngY * sngZoom - sngCamYS + sngViewHeight / 2
    End Sub

    ''' <summary>
    ''' Translates the supplied point from world coordinates
    ''' to screen coordinates, based on the supplied view size.
    ''' </summary>
    ''' <param name="Location">The point to be translated.</param>
    ''' <param name="ViewSize">The size of the 'screen' area.</param>
    ''' <param name="CustomZoomFactor">A custome zoom factor, if desired.
    ''' When zero or less, the process' own zoom factor is used.</param>
    Public Sub TranslateWorldToScreen(ByRef Location As PointF, ByVal ViewSize As Size, Optional ByVal CustomZoomFactor As Single = -1)
        TranslateWorldToScreen(Location.X, Location.Y, ViewSize.Width, ViewSize.Height, CustomZoomFactor)
    End Sub

    ''' <summary>
    ''' Focus the camera on a particular stage.
    ''' </summary>
    ''' <param name="sngViewWidth">The width of the view area</param>
    ''' <param name="sngViewHeight">The height of the view area</param>
    ''' <param name="objstage">The clsProcessStage to focus on.
    ''' If a null value is supplied then no action is taken.</param>
    Public Sub FocusCameraOnStage(ByVal sngViewWidth As Single, ByVal sngViewHeight As Single, ByVal objstage As clsProcessStage)

        If objstage Is Nothing Then Exit Sub

        If Not objstage.GetSubSheetID().Equals(mActiveSubsheetId) Then
            mActiveSubsheetId = objstage.GetSubSheetID()
        End If

        Dim sngX As Single, sngY As Single
        sngX = objstage.GetDisplayX
        sngY = objstage.GetDisplayY
        TranslateWorldToScreen(sngX, sngY, sngViewWidth, sngViewHeight)

        If sngX > sngViewWidth OrElse sngX < 0 Then
            SetCameraX(objstage.GetDisplayX)
        End If

        If sngY > sngViewHeight OrElse sngY < 0 Then
            SetCameraY(objstage.GetDisplayY)
        End If

    End Sub

    ''' <summary>
    ''' Ensures that the specified point is visible, by
    ''' adjusting the camara location (if necessary).
    ''' </summary>
    ''' <param name="WorldPoint">The point, in world 
    ''' coordinates, which must be made visible.</param>
    ''' <param name="DisplayWidth">The width, in pixels of the
    ''' display. Used to calculate the width of the visible
    ''' world area, based on the current zoom level.</param>
    Public Sub FocusCameraOnPoint(ByVal WorldPoint As PointF, ByVal DisplayWidth As Size)
        Dim CurrentViewPort As RectangleF = GetWorldViewPort(DisplayWidth)
        Dim ViewPortCentre As PointF = New PointF(CurrentViewPort.X + CurrentViewPort.Width / 2, CurrentViewPort.Y + CurrentViewPort.Height / 2)

        If Not CurrentViewPort.Contains(WorldPoint) Then
            Dim ViewportHalfSize As SizeF = New SizeF(CurrentViewPort.Width / 2, CurrentViewPort.Height / 2)

            'Adjust horizontally, if needs be
            Dim HorizontalDifference As Single = WorldPoint.X - ViewPortCentre.X
            If Math.Abs(HorizontalDifference) > ViewportHalfSize.Width Then
                If HorizontalDifference > 0 Then
                    ViewPortCentre.X = WorldPoint.X - ViewportHalfSize.Width
                Else
                    ViewPortCentre.X = WorldPoint.X + ViewportHalfSize.Width
                End If
            End If

            'Adjust vertically, if needs be
            Dim VerticalDifference As Single = WorldPoint.Y - ViewPortCentre.Y
            If Math.Abs(VerticalDifference) > ViewportHalfSize.Height Then
                If VerticalDifference > 0 Then
                    ViewPortCentre.Y = WorldPoint.Y - ViewportHalfSize.Height
                Else
                    ViewPortCentre.Y = WorldPoint.Y + ViewportHalfSize.Height
                End If
            End If

            SetCameraLocation(ViewPortCentre)
        End If
    End Sub

    ''' <summary>
    ''' Ensures that the supplied rectangle is visible, by moving the
    ''' camera when necessary.
    ''' </summary>
    ''' <param name="R">The rectangle to be made visible</param>
    ''' <param name="ViewPortSize">The size in pixels of the viewing area</param>
    ''' <param name="CurrentMouseMovement">The current (or recent) mouse movements
    ''' of the user. If the user is active then the camera will only be moved
    ''' if the movements would appear to assist the user in their current activity.</param>
    ''' <remarks>If the supplied rectangle is too large to fit
    ''' into the viewport entirely then the camera will only
    ''' be moved if none of the rectangle is visible at all.</remarks>
    Public Sub FocusCameraOnRectangle(ByVal R As RectangleF, ByVal CurrentMouseMovement As clsGeometry.Vector, ByVal ViewPortSize As Size)
        Dim CurrentViewPort As RectangleF = GetWorldViewPort(ViewPortSize)
        If Not CurrentViewPort.Contains(R) Then
            Dim ViewportHalfSize As SizeF = New SizeF(CurrentViewPort.Width / 2, CurrentViewPort.Height / 2)

            If (R.Left < CurrentViewPort.Left) AndAlso (R.Right < CurrentViewPort.Right) Then
                'Move left, as long as mouse is either still, or is also moving left
                If CurrentMouseMovement.X <= 0 Then
                    SetCameraX(Math.Max(R.Right - CurrentViewPort.Width, R.Left) + ViewportHalfSize.Width)
                End If
            End If
            If (R.Right > CurrentViewPort.Right) AndAlso (R.Left > CurrentViewPort.Left) Then
                'Move right, as long as mouse is either still, or is also moving right
                If CurrentMouseMovement.X >= 0 Then
                    SetCameraX(Math.Min(R.Right - CurrentViewPort.Width, R.Left) + ViewportHalfSize.Width)
                End If
            End If
            If (R.Top < CurrentViewPort.Top) AndAlso (R.Bottom < CurrentViewPort.Bottom) Then
                'Move up, as long as mouse is either still, or is also moving up
                If CurrentMouseMovement.Y <= 0 Then
                    SetCameraY(Math.Max(R.Bottom - CurrentViewPort.Height, R.Top) + ViewportHalfSize.Height)
                End If
            End If
            If (R.Bottom > CurrentViewPort.Bottom) AndAlso (R.Top > CurrentViewPort.Top) Then
                'Move down, as long as mouse is either still, or is also moving down
                If CurrentMouseMovement.Y >= 0 Then
                    SetCameraY(Math.Min(R.Bottom - CurrentViewPort.Height, R.Top) + ViewportHalfSize.Height)
                End If
            End If
        End If
    End Sub



    ''' <summary>
    ''' Gets the names of all the VBOs used by the process. This one filters out 
    ''' everything except VBOs, i.e. internal business objects and web services.
    ''' </summary>
    ''' <returns>A List of strings, each one being the name of a business object
    ''' used by the process.</returns>
    ''' -----------------------------------------------------------------------------
    Public Function GetVisualBusinessObjectsUsedNames() As IEnumerable(Of String)

        Return GetStages(Of clsActionStage)() _
                .Select(Function(x) New With {
                            .Name = x.ObjectName, .Object = GetBusinessObjectRef(.Name)}) _
                .Where(Function(y) _
                           TypeOf y.Object Is clsVBO OrElse
                           (y.Object Is Nothing AndAlso Not String.IsNullOrEmpty(y.Name))) _
                .Select(Function(x) x.Name) _
                .Distinct()

    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the names of all the business objects used by the process.
    ''' </summary>
    ''' <returns>A List of strings, each one being the name of a business object
    ''' used by the process.</returns>
    Private Function GetBusinessObjectsUsedNames() As IEnumerable(Of String)

        Return _
            GetStages(Of clsActionStage)().
                Select(Function(x) x.ObjectName).
                Distinct()

    End Function


    ''' <summary>
    ''' Gets the IDs of all the business objects used by the process.
    ''' </summary>
    ''' <param name="includeParent">Whether to include the parent business object</param>
    ''' <returns>A collection of GUIDs</returns>
    Public Function GetBusinessObjectsUsedIds(includeParent As Boolean) As IEnumerable(Of Guid)

        Return _
            GetBusinessObjectsUsed(includeParent).
                Select(Function(x) x.ProcessID).
                Distinct()

    End Function

    ''' <summary>
    ''' Gets all the business objects used by the process.
    ''' </summary>
    ''' <param name="includeParent">Whether to include the parent business object</param>
    ''' <returns>A collection of VBOs</returns>
    Public Function GetBusinessObjectsUsed(includeParent As Boolean) As IEnumerable(Of clsVBO)

        Dim shouldProcessBypassCache = {
            ProcessRunState.Off,
            ProcessRunState.Aborted,
            ProcessRunState.Failed,
            ProcessRunState.Completed
        }.Contains(mState)

        Return FlattenGroupBusinessObjectTree(
            GetBusinessObjects(True, shouldProcessBypassCache, includeParent))

    End Function


    ''' <summary>
    ''' Get all the processes used (i.e. called as a subprocess) by the process.
    ''' </summary>
    ''' <returns>A List of process IDs.</returns>
    Public Function GetProcessesUsed() As IEnumerable(Of Guid)

        Return _
            GetStages(Of clsSubProcessRefStage)().
                Select(Function(x) x.ReferenceId).
                Distinct()

    End Function

    ''' <summary>
    ''' Returns a flattened enumerable of the group business object tree
    ''' </summary>
    ''' <param name="businessObject">The root of the group business object tree</param>
    ''' <returns>enumerable of the group business object tree</returns>
    Private Shared Function FlattenGroupBusinessObjectTree(businessObject As clsBusinessObject) As IEnumerable(Of clsVBO)

        Select Case businessObject.GetType()
            Case GetType(clsGroupBusinessObject)
                Return _
                    DirectCast(businessObject, clsGroupBusinessObject).
                        Children.
                        SelectMany(AddressOf FlattenGroupBusinessObjectTree)

            Case GetType(clsVBO)
                Return {DirectCast(businessObject, clsVBO)}

            Case Else
                Return {}

        End Select

    End Function

    ''' <summary>
    ''' A lookup table used to improve performance during process validation.
    ''' </summary>
    Private mBacklinksLookup As IDictionary(Of clsProcessStage, clsSet(Of clsProcessStage))

    ''' <summary>
    ''' Perform validation on the process. This is not an exhaustive check - there
    ''' can still be errors. The idea is that this function will be extended over
    ''' time to cover more potential errors in the process.
    ''' </summary>
    ''' <param name="bAttemptRepair">If True, attempts will be made to repair errors
    ''' where possible.</param>
    ''' <param name="bNoObjects">If True, checks for installed Business Objects
    ''' and similar are skipped.</param>
    ''' <param name="exceptionTypes">A list of existing exception types, or an empty 
    ''' list if no exception validation is required.</param>
    ''' <returns>A List of ValidateProcessResult objects, each describing an
    ''' error in the process. If the list is empty, the process is valid.
    ''' </returns>
    Public Function ValidateProcess(bAttemptRepair As Boolean, bNoObjects As Boolean,
                                    exceptionTypes As ICollection(Of String),
                                    dependencyPermissionChecker As IDependencyPermissionChecker,
                                    Optional elementDeps As clsProcessDependencyList = Nothing
                                    ) _
                                    As ValidationErrorList

        'Data items get reset to their initial values when we reset, although
        'this should not be visisble to the user, since now we are no longer
        'in run mode, the 'current value' has no meaning.
        if mState = ProcessRunState.Off Then ResetDataItemCurrentValues(Guid.Empty)

        Dim results As New ValidationErrorList

        'Generate lookup table...
        mBacklinksLookup = BuildStageLinksLookupTable()

        'Generate dependency information
        Dim deps As clsProcessDependencyList = Me.GetDependencies(True)
        If elementDeps IsNot Nothing Then deps.Merge(elementDeps)

        'Most checks we will do on each stage, in the following loop....
        For i As Integer = 0 To mSchema.Stages.Count - 1
            Dim objStage As clsProcessStage = mSchema.Stages(i)
            If objStage.StageType = StageTypes.Exception Then
                results.AddRange(objStage.CheckForErrors(bAttemptRepair, bNoObjects, exceptionTypes))
            Else
                results.AddRange(objStage.CheckForErrors(bAttemptRepair, bNoObjects))
            End If
            'Check for unreferenced data items/collections
            If (objStage.StageType = StageTypes.Data OrElse
             objStage.StageType = StageTypes.Collection) AndAlso
             Not deps.Has(New clsProcessDataItemDependency(objStage)) Then
                If objStage.StageType = StageTypes.Data Then
                    results.Add(objStage, 138, objStage.Name)
                Else
                    results.Add(objStage, 139, objStage.Name)
                End If
            End If

            'Check for valid write expressions
            If (objStage.StageType = StageTypes.Write) Then
                Dim writeStage As clsWriteStage = CType(objStage, clsWriteStage)
                For Each stp As clsWriteStep In writeStage.Steps
                    Dim val As clsProcessValue = Nothing
                    Dim errmsg As String = Nothing
                    If Not clsExpression.EvaluateExpression(stp.Expression,
                                                            val,
                                                            objStage,
                                                            False,
                                                            Nothing,
                                                            errmsg) Then
                        results.Add(objStage, 23, String.Empty, errmsg)
                    End If
                Next
            End If
        Next

        'Check for blocks that overlap with each other...
        Dim blocks As List(Of clsBlockStage) = GetStages(Of clsBlockStage)()
        For Each blk As clsBlockStage In blocks
            For Each blk2 As clsProcessStage In blocks
                If blk Is blk2 Then Continue For
                If blk.Intersects(blk2) Then results.Add(blk, 25)
            Next
        Next
        'And also for stages that are partially inside and outside a block...
        For Each stg As clsProcessStage In mSchema.Stages
            If stg.StageType = StageTypes.Block Then Continue For
            For Each blk As clsBlockStage In blocks
                If stg.IsPartlyWithin(blk) Then results.Add(stg, 26)
            Next
        Next

        'Check for subsheets which never get called
        If ProcessType = DiagramType.Process Then
            For Each sht As clsProcessSubSheet In SubSheets
                Dim HasReferences As Boolean = False

                If sht.SheetType = SubsheetType.MainPage Then
                    HasReferences = True
                Else
                    For Each refStg As clsSubSheetRefStage _
                     In GetStages(Of clsSubSheetRefStage)()
                        If refStg.ReferenceId = sht.ID Then
                            HasReferences = True
                            Exit For
                        End If
                    Next
                End If

                If Not HasReferences Then results.Add(Nothing, 27, sht.Name)
            Next
        End If

        'Check for resume stages that are not in a recovery section...
        For Each res As clsResumeStage In GetStages(Of clsResumeStage)()
            If Not res.IsInRecoverySection() Then results.Add(res, 28)
        Next

        'Check for recovery stages that link to the end without resuming...
        For Each st As clsEndStage In GetStages(Of clsEndStage)()
            Dim badrec As clsProcessStage = StageLinksBackToRecovery(st)
            If badrec IsNot Nothing Then results.Add(badrec, 29)
        Next

        'Check for exception stages that say to use the current exception information
        'but aren't in a recovery section...
        For Each ex As clsExceptionStage In GetStages(Of clsExceptionStage)()
            If ex.UseCurrentException AndAlso Not ex.IsInRecoverySection() Then
                results.Add(ex, 30)
            End If
        Next

        If mCompilerRunner IsNot Nothing Then mCompilerRunner.ValidateCode(results)

        'Check for problems with the application definition if there is one...
        If mApplicationDefinition IsNot Nothing Then

            ' A stack containing the groups which need to be checked.
            Dim groups As New Stack(Of clsApplicationMember)

            ' Kick it off with the root element
            groups.Push(mApplicationDefinition.RootApplicationElement)

            While groups.Count > 0
                Dim current As clsApplicationMember = groups.Pop()
                For Each child As clsApplicationMember In current.ChildMembers
                    Dim el As clsApplicationElement = TryCast(child, clsApplicationElement)
                    If el IsNot Nothing _
                     AndAlso el.HasDynamicAttributes AndAlso el.Narrative.Length = 0 Then
                        results.Add(Nothing, 130, child.Name)
                    End If
                    'Check for unreferenced application elements
                    If el IsNot Nothing _
                     AndAlso Not deps.Has(New clsProcessElementDependency(Me.Name, el.ID)) Then
                        results.Add(Nothing, 140, child.Name)
                    End If
                    ' If there are any nested elements, add this child 
                    ' to the elements whose children we need to check
                    If child.HasChildren Then groups.Push(child)
                Next
            End While
        End If

        Dim inaccessibleRefs = dependencyPermissionChecker.GetInaccessibleDependenciesInProcessDependencyTree(Me)

        For Each ref In inaccessibleRefs
            results.Add(Me.GetStage(ref.Key), 143, String.Join(", ", ref.Value))
        Next

        'No longer required...
        mBacklinksLookup = Nothing

        Return results
    End Function


    ''' <summary>
    ''' Determine if the given stage links back to a recovery stage without passing
    ''' through a resume stage.
    ''' </summary>
    ''' <param name="st">The stage to check</param>
    ''' <param name="visited">For use in recursive internal calls only, holds a
    ''' list of stages already visited during the search.</param>
    ''' <returns>The first recovery stage linked back to, or Nothing if one is not
    ''' found.</returns>
    ''' <remarks>This can only be used during the process validation 'process',
    ''' since it requires mBacklinksLookup to have been generated.</remarks>
    Private Function StageLinksBackToRecovery(ByVal st As clsProcessStage,
     Optional ByVal visited As clsSet(Of clsProcessStage) = Nothing) As clsProcessStage

        If mBacklinksLookup Is Nothing Then Throw New InvalidOperationException(
         My.Resources.Resources.clsProcess_StageLinksBackToRecoveryCalledOutOfContext)

        If visited Is Nothing Then visited = New clsSet(Of clsProcessStage)

        ' If nothing links to this stage, then it can't link back to a recovery stage
        If Not mBacklinksLookup.ContainsKey(st) Then Return Nothing

        ' Go through all the stages that link to the stage
        For Each st2 As clsProcessStage In mBacklinksLookup(st)

            ' If we've already examined it, move onto the next one
            If visited.Contains(st2) Then Continue For

            ' We don't want to process any stages beyond the resume stage
            If TypeOf st2 Is clsResumeStage Then Continue For

            ' We've found our recovery stage, return that to the caller
            If TypeOf st2 Is clsRecoverStage Then Return st2

            ' Otherwise, log that the stage has been visited and recurse into it
            ' to continue looking further back; if a result is returned, pass it back
            ' out of this method
            visited.Add(st2)
            Dim st3 As clsProcessStage = StageLinksBackToRecovery(st2, visited)
            If st3 IsNot Nothing Then Return st3

        Next

        ' No recovery stage was found, nothing to return
        Return Nothing

    End Function

    ''' <summary>
    ''' Determine if the given stage is within a recovery section by searching all
    ''' links backwards.
    ''' </summary>
    ''' <param name="st">The stage to check</param>
    ''' <param name="visited">For use in recursive internal calls only, holds a
    ''' list of stages already visited during the search.</param>
    ''' <returns>True if the stage is definitely in a recovery section, or False if
    ''' either it isn't, or if a definitive answer is not possible.</returns>
    Friend Function IsStageInRecoverySection(ByVal st As clsProcessStage,
     Optional ByVal visited As IBPSet(Of clsProcessStage) = Nothing) As Boolean

        ' If the stage is a recover stage, then of course it's in a recovery section
        If st.StageType = StageTypes.Recover Then Return True

        If mBacklinksLookup Is Nothing Then Throw New InvalidOperationException(
         My.Resources.Resources.clsProcess_IsStageInRecoverySectionCalledOutOfContext)

        ' Ensure that we don't get stuck in a loop by only searching back from
        ' unvisited stages. Set up a cache to take care of that.
        If visited Is Nothing Then visited = New clsSet(Of clsProcessStage)

        ' If nothing links to this stage, it can't possibly be in a recovery section
        If Not mBacklinksLookup.ContainsKey(st) Then Return False

        For Each priorStg As clsProcessStage In mBacklinksLookup(st)

            ' If we reach a Resume before reaching a Recover, we're definitely not in
            ' a recovery section in one branch of the processing, ergo we're not in a
            ' recovery section.
            If priorStg.StageType = StageTypes.Resume Then Return False

            ' If this stage hasn't already been visited, check to see if that stage
            ' is itself in a recovery stage - if it isn't, then neither is this stage
            If visited.Add(priorStg) AndAlso
             Not IsStageInRecoverySection(priorStg, visited) Then Return False

        Next

        ' If we reach here, then we've got through the entire process without
        ' proving that the stage is *not* in a recovery section - the only way
        ' this could have happened if a Recover stage was found (without a
        ' corresponding Resume stage) for each branch leading to this stage.
        Return True

    End Function


    ''' <summary>
    ''' Builds a Dictionary used to quickly lookup lists of stages that link back to 
    ''' each stage.
    ''' </summary>
    ''' <returns>A Dictionary keyed by stage, with the value being a List of stages
    ''' that are backlinked to the keyed stage.</returns>
    Private Function BuildStageLinksLookupTable() _
     As IDictionary(Of clsProcessStage, clsSet(Of clsProcessStage))
        Dim backLinks As _
         New clsGeneratorDictionary(Of clsProcessStage, clsSet(Of clsProcessStage))

        For Each stg As clsProcessStage In mSchema.Stages
            For Each linkee As clsProcessStage In stg.GetLinks()
                backLinks(linkee).Add(stg)
            Next
        Next

        ' We've built it up using a generator, but we don't want to be generating
        ' a new set every time we reference a stage, so use the inner dictionary
        ' (which has no auto-generator component to it)
        Return backLinks.InnerDictionary
    End Function

    ''' <summary>
    ''' Get full details about the usage of all AMI elements within the process, for
    ''' reporting purposes.
    ''' </summary>
    ''' <returns>A list of ElementUsageInstance objects, each detailing an instance
    ''' of an element being used.</returns>
    Public Function GetElementUsageDetails() As List(Of ElementUsageInstance)

        Dim result As New List(Of ElementUsageInstance)

        For Each st As clsProcessStage In mSchema.Stages
            Dim appst As clsAppStage = TryCast(st, clsAppStage)
            If appst IsNot Nothing Then
                For Each stp As clsStep In appst.Steps
                    Dim info As New ElementUsageInstance()
                    info.ElementID = stp.ElementId
                    info.StageID = st.GetStageID()
                    result.Add(info)
                Next
            ElseIf st.StageType = StageTypes.WaitStart Then
                Dim els As List(Of Guid) = CType(st, clsWaitStartStage).GetElementsUsed()
                For Each g As Guid In els
                    Dim info As New ElementUsageInstance()
                    info.ElementID = g
                    info.StageID = st.GetStageID()
                    result.Add(info)
                Next
            End If
        Next
        Return result

    End Function


    ''' <summary>
    ''' Get information about the usage of AMI elements within the process, for
    ''' reporting purposes.
    ''' </summary>
    ''' <param name="total">The total number of elements defined.</param>
    ''' <param name="used">The number actually used.</param>
    Public Sub GetElementUsageInfo(ByRef total As Integer, ByRef used As Integer)

        'Create a flat list of all the elements in the tree...
        Dim l As New List(Of clsApplicationMember)
        l.Add(mApplicationDefinition.RootApplicationElement)
        Dim index As Integer = 0
        While index < l.Count
            For Each c As clsApplicationMember In l(index).ChildMembers
                l.Add(c)
            Next
            index += 1
        End While
        total = l.Count

        'Remove used items from the list so we can see what's left...
        For Each st As clsProcessStage In mSchema.Stages
            Dim appst As clsAppStage = TryCast(st, clsAppStage)
            If appst IsNot Nothing Then
                For Each stp As clsStep In appst.Steps
                    For i As Integer = l.Count - 1 To 0 Step -1
                        If l(i).ID.Equals(stp.ElementId) Then l.RemoveAt(i)
                    Next
                Next
            ElseIf st.StageType = StageTypes.WaitStart Then
                Dim els As List(Of Guid) = CType(st, clsWaitStartStage).GetElementsUsed()
                For Each g As Guid In els
                    For i As Integer = l.Count - 1 To 0 Step -1
                        If l(i).ID.Equals(g) Then l.RemoveAt(i)
                    Next
                Next
            End If

        Next
        used = total - l.Count

    End Sub


    ''' <summary>
    ''' Analyse the usage of actions in a process. In the counts that are returned,
    ''' it is the AMI-based actions that are counted, so for example, a Navigate
    ''' stage with three rows will count as three.
    ''' </summary>
    ''' <param name="total">On return, contains the total number of actions.</param>
    ''' <param name="globalcount">On return, contains the number of actions that
    ''' use a 'global' method, i.e. a global mouse click or keypress.</param>
    ''' <param name="globaldetails">On return, contains details of global action
    ''' usage, or Nothing if there are no details available.</param>
    Public Sub AnalyseAMIActions(ByRef total As Integer, ByRef globalcount As Integer, ByRef globaldetails As String)

        globalcount = 0
        total = 0
        Dim gd As New StringBuilder()
        For Each objStage As clsProcessStage In mSchema.Stages
            objStage.AnalyseAMIActions(total, globalcount, gd)
        Next
        If gd.Length = 0 Then
            globaldetails = Nothing
        Else
            globaldetails = gd.ToString()
        End If

    End Sub

    ''' <summary>
    ''' Get the Business Objects in use by this process. For semi-internal use only.
    ''' The set of objects used can change, so this reference should only be
    ''' retrieved and used immediately - never stored.
    ''' </summary>
    ''' <param name="running">Optional - default is False. If set to True then
    ''' only objects referenced by this process are added regardless of whether it is
    ''' editable or not (e.g. if debugging in Studio)</param>
    ''' <param name="shouldBypassCache">
    ''' If set to <c>true</c> the cache will not be used and will be updated with
    ''' the latest value.
    ''' </param>
    ''' <returns>
    ''' A clsBusinessObjects class
    ''' </returns>
    Public Function GetBusinessObjects(
        Optional ByVal running As Boolean = False,
        Optional shouldBypassCache As Boolean = False,
        Optional includeParent As Boolean = False) _
     As clsGroupBusinessObject

        If Not Editable OrElse running Then
            'If this process is not editable, or we're running a process
            'we'll pass a list of business objects we currently reference, and they'll
            'be the only ones available.
            If shouldBypassCache OrElse mObjRefs Is Nothing Then
                Dim usedObjects = GetBusinessObjectsUsedNames()
                mObjRefs = GroupBusinessObjectFactory(ExternalObjectsInfo, Me, Session, includeParent, usedObjects.ToList)
            End If
            Return mObjRefs
        Else
            'Otherwise we'll pass Nothing for the list, meaning all available objects
            'are going to be loaded which is obviously slower but means it will be
            'possible to add them to the process!
            If shouldBypassCache OrElse mAllObjRefs Is Nothing Then
                mAllObjRefs = GroupBusinessObjectFactory(ExternalObjectsInfo, Nothing, Nothing, includeParent, Nothing)
            End If
            Return mAllObjRefs
        End If

    End Function

    ''' <summary>
    ''' Get the Business Objects in use by this process. For semi-internal use only.
    ''' The set of objects used can change, so this reference should only be
    ''' retrieved and used immediately - never stored.
    ''' </summary>
    ''' <returns>
    ''' A clsBusinessObjects class
    ''' </returns>
    Public Function GetAllBusinessObjects() As clsGroupBusinessObject

        If mAllObjRefs Is Nothing Then
            mAllObjRefs = GroupBusinessObjectFactory(ExternalObjectsInfo, Nothing, Nothing, True, Nothing)
        End If
        Return mAllObjRefs

    End Function
    ''' <summary>
    ''' Returns a reference to the passed object within the scope of this owning
    ''' process. If there is no object reference (i.e. the object was not directly
    ''' referenced by the process) then it is created and added.
    ''' </summary>
    ''' <param name="name">The requested object name</param>
    ''' <returns>A reference to the requested object</returns>
    Public Overridable Function GetBusinessObjectRef(ByVal name As String) As clsBusinessObject
        Dim objectReference As clsBusinessObject = Nothing

        'First get the object ref from current process/object list
        'If the object is a VBO (or not found) then check owning process scope
        objectReference = GetBusinessObjects(True)?.FindObjectReference(name)
        If objectReference IsNot Nothing Then
            If Not TypeOf objectReference Is clsVBO Then Return objectReference
            Dim process = CType(objectReference, clsVBO).Process
            If process Is Nothing Then Return Nothing
            If Not process.HasProcessLevelScope Then Return objectReference
        End If

        'Find the owning process (or root process/object)
        Dim owningProcess = Me
        While Not owningProcess.ProcessType = DiagramType.Process
            If owningProcess.ParentProcess Is Nothing Then Exit While
            owningProcess = owningProcess.ParentProcess
        End While

        'Check if requested object is the owning process's parent (special case)
        'otherwise attempt to find object in owning process's object ref list
        If name = owningProcess.ParentObject Then
            objectReference = owningProcess.ParentObjRef
        Else
            objectReference = owningProcess.GetBusinessObjects(True)?.FindObjectReference(name)
            If objectReference Is Nothing Then
                'If it's not there add it, and retry
                owningProcess.AddBusinessObject(Me, name)
                objectReference = owningProcess.GetBusinessObjects(True)?.FindObjectReference(name)
            End If
        End If

        'Set (or reset) the parent - object instances could
        'be reused in other parts of the same overall process scope
        If objectReference IsNot Nothing Then
            Dim vbo = DirectCast(objectReference, clsVBO)
            If vbo.Process IsNot Nothing AndAlso vbo.Process.RunState = ProcessRunState.Completed Then _
                vbo.ParentProcess = Me
        End If

        Return objectReference
    End Function

    Friend Function GetSkill(skillId As Guid) As SkillDetails
        If mSkills Is Nothing Then mSkills = New List(Of SkillDetails)()
        Dim skill = mSkills.FirstOrDefault(Function(s) s.SkillId = skillId.ToString())

        If skill IsNot Nothing Then Return skill

        If Not TryAddSkill(skillId) Then Return Nothing

        Return GetSkill(skillId)
    End Function

    ''' <summary>
    ''' Add the passed object to the object reference list for this process
    ''' </summary>
    ''' <param name="parent">The parent object/process</param>
    ''' <param name="name">The object name</param>
    Public Sub AddBusinessObject(ByVal parent As clsProcess, ByVal name As String)
        Dim refs As New clsGroupBusinessObject(ExternalObjectsInfo, parent, Session, False,
                                              New List(Of String) From {name})
        Dim obr = refs.FindObjectReference(name)
        If obr IsNot Nothing Then mObjRefs.Children.Add(obr)
        refs.Dispose()
    End Sub

    Private Function TryAddSkill(id As Guid) As Boolean
        Dim skill = ExternalObjectsInfo.Children.OfType(Of SkillDetails)().FirstOrDefault(Function(o) o.SkillId = id.ToString())
        If skill IsNot Nothing Then
            mSkills.Add(skill)
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Get the count of undo items in the undo buffer collection
    ''' This is required to enable/disable the undo menu option correctly.
    ''' </summary>
    ''' <returns>Integer containing count of undo items</returns>
    Public Function CanUndo() As Boolean
        Return mUndoRedoManager.CanUndo
    End Function

    ''' <summary>
    ''' Get the count of redo items in the redo buffer collection
    ''' This is required to enable/disable the redo menu option correctly.
    ''' </summary>
    ''' <returns>Integer containing count of redo items</returns>
    Public Function CanRedo() As Boolean
        Return mUndoRedoManager.CanRedo
    End Function

    ''' <summary>
    ''' Determines whether or not the process automatically raises a breakpoint
    ''' when a handled exception occurs during debugging.
    ''' </summary>
    Public Property BreakOnHandledException() As Boolean

    ''' <summary>
    ''' Causes process flow to halt at the current stage during debug.  Information
    ''' about the supplied breakpoint will be made available for display to the user.
    ''' </summary>
    ''' <param name="objBreakPointInfo">Info object about the breakpoint causing
    ''' debugging to halt. Typically this is a data item breakpoint raised because
    ''' a data item value has changed.</param>
    Public Sub RaiseBreakPoint(ByVal objBreakPointInfo As clsProcessBreakpointInfo)
        mRaisedBreakPointInfo = objBreakPointInfo
    End Sub



    ''' <summary>
    ''' Handles the StatusChanged event of the undo/redo manager.
    ''' </summary>
    Private Sub HandleUndoRedoManagerStatusChanged(ByVal NewStatus As clsUndoRedoManager.ManagerStates) Handles mUndoRedoManager.StatusChanged
        RaiseEvent UndoRedoStatusChanged(NewStatus)
    End Sub

    ''' <summary>
    ''' Gets the clsCompilerRunner object in use.
    ''' </summary>
    ''' <returns>A reference to the clsCompilerRunner, or Nothing if there isn't
    ''' one.</returns>
    Public ReadOnly Property CompilerRunner() As clsCompilerRunner
        Get
            Return mCompilerRunner
        End Get
    End Property

#Region "Running"

    ''' <summary>
    ''' Perform a run action - i.e. tell the process to change run state in some
    ''' way.
    ''' </summary>
    ''' <param name="Action">The action requested</param>
    ''' <returns>A clsProcessStage.Result</returns>
    ''' <remarks>This overload is provided for callers that don't care about breakpoints,
    ''' and routes through to the real function with default parameters.</remarks>
    Public Function RunAction(ByVal Action As ProcessRunAction) As StageResult
        Return RunAction(Action, Nothing, False)
    End Function

    ''' <summary>
    ''' Perform a run action - i.e. tell the process to change run state in some
    ''' way.
    ''' </summary>
    ''' <param name="Action">The action requested</param>
    ''' <param name="BreakPointInfo">Info about any breakpoint raised.
    ''' If this is not Nothing on return then a breakpoint was encountered.</param>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Function RunAction(ByVal Action As ProcessRunAction, ByRef BreakPointInfo As clsProcessBreakpointInfo, ByVal bPauseWhenBreakPointRaised As Boolean) As StageResult

        Dim res As StageResult
        Dim sErr As String = Nothing

        BreakPointInfo = Nothing

        Select Case Action

            Case ProcessRunAction.Reset

                'Reset actually means return to 'Off' state,
                'which we'll only do if we're not there already. It also means something
                'else currently - refresh business objects - but see further on for that.
                If mState <> ProcessRunState.Off Then
                    RunStageID = Guid.Empty
                    mRecoveryMode = False
                    mRunInputs = Nothing
                    RunState = ProcessRunState.Off
                    'Clean up existing business objects if we've used them...
                    If mObjRefs IsNot Nothing Then
                        res = mObjRefs.DoCleanup()
                        If Not res.Success Then Return res
                    End If
                End If

                'Get a new set of Business Objects, but only because we want changes to
                'show up in the UI, otherwise we could keep using the existing ones until
                'we run again...
                DisposeObjects()

                'Clear all session variables. Note that we only do this if it's
                'our own session, not a parent's, which we would get if we accessed
                'the Session property.
                If mSession IsNot Nothing Then mSession.ClearVars()

                ' Get a new set of environment variables too
                Try
                    mEnvVars = clsAPC.ProcessLoader.GetEnvVars()

                Catch ex As Exception
                    Return StageResult.InternalError(ex)

                End Try

            Case ProcessRunAction.Pause
                RunState = ProcessRunState.Paused

            Case ProcessRunAction.StepOver
                If Not InitIfNeeded(sErr) Then Return New StageResult(False, "Internal", sErr)

                If RunState = ProcessRunState.SteppingOver Then Exit Select

                Dim gNextStage As Guid

                Dim CurrentStage As clsProcessStage = GetStage(RunStageID)
                If CurrentStage Is Nothing Then
                    If RunStageID = Guid.Empty Then
                        sErr = My.Resources.Resources.clsProcess_ThereIsNoCurrentRunStage
                    Else
                        sErr = String.Format(My.Resources.Resources.clsProcess_InternalErrorCouldNotFindCurrentRunStageWithID0, RunStageID.ToString)
                    End If
                    Return New StageResult(False, "Internal", sErr)
                End If

                If TypeOf CurrentStage Is clsLinkableStage Then
                    gNextStage = CType(CurrentStage, clsLinkableStage).OnSuccess
                End If

                Dim starttype As StageTypes = CurrentStage.StageType

                Do
                    RunState = ProcessRunState.SteppingOver

                    res = RunStep(BreakPointInfo, bPauseWhenBreakPointRaised)
                    If Not res.Success Then
                        Return res
                    End If

                    'break execution loop if ...
                    'not over subsheet/subprocess or ...
                    'reached intended stopping point or ...
                    'a breakpoint raised
                    If (starttype <> StageTypes.SubSheet AndAlso
                     starttype <> StageTypes.Action AndAlso
                     starttype <> StageTypes.Skill AndAlso
                     starttype <> StageTypes.Process) OrElse
                     RunStageID.Equals(gNextStage) OrElse
                     (Not BreakPointInfo Is Nothing) Then
                        Exit Do
                    End If

                    ' Also break out of the loop if we've ended up on a recovery
                    ' stage at a higher execution level than the stage we're
                    ' stepping over.
                    Dim curstage As clsProcessStage = GetStage(RunStageID)
                    If curstage.StageType = StageTypes.Recover Then
                        ' Find the recovery stage (without altering the process)
                        ' for the stage we're stepping over. If it's the same as the
                        ' recovery stage that we're currently on, we break out of
                        ' the loop. Otherwise, an exception is being handled below
                        ' the level of the stage we're stepping over, so we carry
                        ' on - 'breaking' on exceptions is handled elsewhere.
                        Dim higherRecoveryStage As clsProcessStage =
                         FindRecoveryStage(CurrentStage, Nothing, False)
                        If curstage Is higherRecoveryStage Then Exit Do
                    End If
                Loop

            Case ProcessRunAction.StepOut
                If Not InitIfNeeded(sErr) Then Return New StageResult(False, "Internal", sErr)

                Dim CurrentStage As clsProcessStage = GetStage(RunStageID)
                If CurrentStage Is Nothing Then
                    If RunStageID = Guid.Empty Then
                        sErr = My.Resources.Resources.clsProcess_ThereIsNoCurrentRunStage
                    Else
                        sErr = String.Format(My.Resources.Resources.clsProcess_InternalErrorCouldNotFindCurrentRunStageWithID0, RunStageID.ToString)
                    End If
                    Return New StageResult(False, "Internal", sErr)
                End If

                Dim gSubsheetOfStageStartedAt As Guid = CurrentStage.GetSubSheetID()

                Do
                    RunState = ProcessRunState.SteppingOver
                    Dim currentSubSheetID = GetStage(RunStageID).GetSubSheetID
                    res = RunStep(BreakPointInfo, True)
                    If Not res.Success Then
                        Return res
                    End If

                    'Break execution loop if breakpoint reached
                    If Not BreakPointInfo Is Nothing Then
                        Exit Do
                    End If

                    'If the stage caused an exception, but the exception is being recovered outside of this subsheet then this will show as a success,
                    'but it's finished on this page
                    Dim nextStage = GetStage(RunStageID)
                    If nextStage.StageType = StageTypes.Recover AndAlso nextStage.GetSubSheetID <> gSubsheetOfStageStartedAt AndAlso currentSubSheetID <> nextStage.GetSubSheetID Then
                        Exit Do
                    End If

                    'Break execution loop if reached next end stage on the same page we started on ...
                    'Note that this will not work if a subpage calls itself.
                    Dim isFinalStageOnSubPage = IsTerminatingStage(nextStage)
                    If isFinalStageOnSubPage AndAlso nextStage.GetSubSheetID().Equals(gSubsheetOfStageStartedAt) Then
                        'Do one more stage if we can, in order to take us out of subpage
                        RunState = ProcessRunState.SteppingOver
                        res = RunStep(BreakPointInfo, bPauseWhenBreakPointRaised)
                        If Not res.Success Then
                            Return res
                        Else
                            Exit Do
                        End If
                    'Special case, we are stepping out of the final stage.
                    Else
                        Dim isCurrentStageTerminatingStage = IsTerminatingStage(CurrentStage)
                        If isCurrentStageTerminatingStage AndAlso CurrentStage.GetSubSheetID().Equals(gSubsheetOfStageStartedAt) Then
                            RunState = ProcessRunState.SteppingOver
                            Exit Do
                        End If 
                    End If
                Loop


            Case ProcessRunAction.StepIn
                If Not InitIfNeeded(sErr) Then Return New StageResult(False, "Internal", sErr)

                RunState = ProcessRunState.Stepping

                res = RunStep(BreakPointInfo, bPauseWhenBreakPointRaised)
                If Not res.Success Then
                    Return res
                End If

            Case ProcessRunAction.Go
                If Not InitIfNeeded(sErr) Then Return New StageResult(False, "Internal", sErr)
                RunState = ProcessRunState.Running

            Case ProcessRunAction.RunNextStep

                If mState <> ProcessRunState.Running Then
                    sErr = My.Resources.Resources.clsProcess_CanTRunNextStepWhenNotRunningInTheFirstPlace
                    Return New StageResult(False, "Internal", sErr)
                End If

                res = RunStep(BreakPointInfo, bPauseWhenBreakPointRaised)
                If Not res.Success Then
                    Return res
                End If

            Case ProcessRunAction.GotoPage
                If mState = ProcessRunState.Off Then
                    If Not InitIfNeeded(sErr) Then Return New StageResult(False, "Internal", sErr)
                Else
                    RunState = ProcessRunState.Paused
                    Dim gStartStage As Guid
                    Dim gRunPage As Guid
                    If RunPageId.Equals(Guid.Empty) Then
                        gRunPage = GetActiveSubSheet()
                    Else
                        gRunPage = RunPageId
                    End If
                    gStartStage = GetStageByTypeAndSubSheet(StageTypes.Start, gRunPage).GetStageID()
                    RunStageID = gStartStage
                    mRecoveryMode = False
                End If

        End Select

        'Allow the application to respond whilst we are 
        'stepping over an action (This was causing bug# 4266)
        'but also leads to general unresponsiveness of 
        'automate until the step over action has completed.
        Application.DoEvents()

        Return New StageResult(True)
    End Function

    Private Function IsTerminatingStage(currentStage As clsProcessStage) As Boolean
        If currentStage.StageType = StageTypes.End Then _
            Return True

        If currentStage.StageType = StageTypes.Exception Then
            Dim recoveryStagesOnPage = GetStagesBySubsheetAndType(currentStage.GetSubSheetID(), StageTypes.Recover).ToList()

            If recoveryStagesOnPage.Any(Function(s) GetBlockContainingStage(s) Is Nothing) Then _
                Return False

            'If we're in a block, look for a recovery stage there...
            Dim containingblock As clsBlockStage = GetBlockContainingStage(currentStage)
            If containingblock Is Nothing Then _
                Return True

            Return Not recoveryStagesOnPage.Any(Function(s) GetBlockContainingStage(s) Is containingblock)
        End If

        Return False
    End Function

    ''' <summary>
    ''' Initialise ready for running if necessary - i.e. if running is not already
    ''' in progress.
    ''' </summary>
    ''' <param name="sErr">In the event of failure, this contains an error
    ''' message</param>
    ''' <returns>True if successful, False otherwise</returns>
    Private Function InitIfNeeded(ByRef sErr As String) As Boolean
        Try
            If mState = ProcessRunState.Off Then

                'Determine the correct start stage, which depends on the process type
                'and where relevant, what page was requested.
                Dim gStartStage As Guid
                If ProcessType = DiagramType.Process Then
                    gStartStage = GetStage(GetStartStage()).GetStageID()
                Else
                    Dim gRunPage As Guid
                    If RunPageId.Equals(Guid.Empty) Then
                        gRunPage = GetActiveSubSheet()
                    Else
                        gRunPage = RunPageId
                    End If
                    gStartStage = GetStageByTypeAndSubSheet(StageTypes.Start, gRunPage).GetStageID()
                End If
                RunStageID = gStartStage
                mRecoveryMode = False

                ResetRecoveryAttempts(Guid.Empty)

                'Compile code if necessary...
                If Not CompilerRunner Is Nothing Then
                    If Not CompilerRunner.Compile(sErr) Then Return False
                End If

                InitialiseForRunning()

                'Clear all session variables. Note that we only do this if it's
                'our own session, not a parent's, which we would get if we accessed
                'the Session property.
                If mSession IsNot Nothing Then mSession.ClearVars()
                ResetDataItemCurrentValues(Guid.Empty)
            End If
            Return True
        Catch e As Exception
            sErr = String.Format(My.Resources.Resources.clsProcess_InitFailed0, e.Message)
        End Try
    End Function

    Friend Sub ResetRecoveryAttempts(subsheetId As Guid)
        For Each st As clsProcessStage In mSchema.Stages
            If subsheetId.Equals(Guid.Empty) OrElse st.GetSubSheetID().Equals(subsheetId) Then
                Dim recoverStage = TryCast(st, clsRecoverStage)
                recoverStage?.ResetAttempts()
            End If
        Next
    End Sub
#End Region


    ''' <summary>
    ''' Event handler to track change of state in a child process we are waiting for.
    ''' Both subprocess and action stages can trigger this waiting behaviour, within
    ''' their own execution handlers, but the status changes are detected here.
    ''' </summary>
    ''' <param name="r">The new Run Mode</param>
    Private Sub ChildWaitProcessRunningStateChanged(ByVal r As ProcessRunState) _
     Handles mChildWaitProcess.RunStateChanged
        Select Case r
            Case ProcessRunState.Completed, ProcessRunState.Aborted
                'If we're paused, then we were stepping originally. We need one more
                'step to get this process to pick up the new status and move on to
                'the next stage...
                If mState = ProcessRunState.Paused Then
                    Dim res As StageResult = RunAction(ProcessRunAction.StepIn)
                    If Not res.Success Then
                        'Something went wrong during the finalising of the return
                        'from the call to the child process. Most likely something
                        'like being unable to store the output parameters. Because
                        'we're deep within event handlers now, we need to do
                        'something to propogate this error back up to the debugger.
                        '(Only interactive mode is really relevant - at runtime, it
                        'would be logged anyway)
                        RaiseEvent AsyncError(res)
                    End If
                End If
        End Select
    End Sub


    ''' <summary>
    ''' True if the process is executing, and currently waiting for a child process
    ''' (e.g. subprocess or business object) to complete before it continues
    ''' </summary>
    Public ReadOnly Property ChildWaiting() As Boolean
        Get
            Return Not mChildWaitProcess Is Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the root process for this process - ie. the process which is at the top
    ''' of the calling stack within which this process is a subprocess. If this
    ''' process has no parent process, it is the root process and as such, this will
    ''' return itself.
    ''' </summary>
    Public ReadOnly Property RootProcess() As clsProcess
        Get
            Dim parent As clsProcess = ParentProcess
            If parent IsNot Nothing Then Return parent.RootProcess
            Return Me
        End Get
    End Property

    ''' <summary>
    ''' Gets the "Run To" stage ID for this process. Guid.Empty indicates that no
    ''' "Run To" stage is set for this process.
    ''' This is reset when the <see cref="RunState"/> changes to anything other than
    ''' <see cref="ProcessRunState.Running"/>.
    ''' </summary>
    Public Property RunToStageId() As Guid

    ''' <summary>
    ''' Write the given process XML to a file, nicely formatted, and with the
    ''' preferred ID attribute inserted along the way
    ''' </summary>
    ''' <param name="processXML">The process XML definition to write.</param>
    ''' <param name="processId">The ID of the process.</param>
    ''' <param name="processAttributes">The attributes for the process.</param>
    ''' <param name="outputFile">The output file to write to.</param>
    ''' <param name="sErr">On failure, contains an error message.</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Public Shared Function ExportXMLToFile(processXML As String, processId As Guid, processAttributes As ProcessAttributes, outputFile As String, ByRef sErr As String) As Boolean

        'Insert additional XML attributes into the xml tree
        Dim xmlDocument As New ReadableXmlDocument(processXML)

        AddAttributeToProcessXMLNode(xmlDocument, "preferredid", processId.ToString())
        If processAttributes.HasFlag(ProcessAttributes.Published) Then AddAttributeToProcessXMLNode(xmlDocument, "published", "true")

        Try
            'Take the XML with the added attributes and write it out to the chosen file
            Using xmlWriter As New XmlTextWriter(outputFile, Encoding.UTF8)
                xmlWriter.Formatting = Formatting.Indented
                xmlDocument.WriteContentTo(xmlWriter)
                xmlWriter.Flush()
            End Using

        Catch ex As Exception
            sErr = String.Format(My.Resources.Resources.clsProcess_ThereWasAnErrorWritingToTheSpecifiedLocation0, ex.Message)
            Return False

        End Try

        Return True

    End Function

    Private Shared Sub AddAttributeToProcessXMLNode(xmlDocument As ReadableXmlDocument, attributeName As String, attributeValue As String)
        Dim xmlAttribute As XmlAttribute = DirectCast(xmlDocument.SelectSingleNode($"process/@{attributeName}"), XmlAttribute)

        If xmlAttribute Is Nothing Then
            xmlAttribute = xmlDocument.CreateAttribute(attributeName)
            xmlDocument.SelectSingleNode("process").Attributes.Append(xmlAttribute)
        End If
        xmlAttribute.Value = attributeValue
    End Sub

    ''' <summary>
    ''' Get information about all the dependencies present in this Process.
    ''' </summary>
    ''' <param name="inclInternal">Include internal references</param>
    ''' <returns>An clsProcessDependencyList.</returns>
    Public Function GetDependencies(inclInternal As Boolean) As clsProcessDependencyList
        Dim deps As New clsProcessDependencyList()

        'Set the run mode and parent object.
        deps.RunMode = BusinessObjectRunMode.Background
        If ProcessType = DiagramType.Object Then
            deps.RunMode = ObjectRunMode
            If ParentObject IsNot Nothing Then
                deps.Add(New clsProcessParentDependency(ParentObject))
            End If
            deps.IsShared = IsShared
        End If

        'Examine stages for any dependencies
        For Each st As clsProcessStage In mSchema.Stages
            For Each dep As clsProcessDependency In st.GetDependencies(inclInternal)
                deps.Add(dep, st.Id)
            Next
        Next

        'For objects examine application model for dependencies
        If ProcessType = DiagramType.Object AndAlso ApplicationDefinition IsNot Nothing Then
            For Each dep As clsProcessDependency In ApplicationDefinition.GetDependencies(inclInternal)
                deps.Add(dep)
            Next
        End If

        Return deps
    End Function

#End Region

End Class
