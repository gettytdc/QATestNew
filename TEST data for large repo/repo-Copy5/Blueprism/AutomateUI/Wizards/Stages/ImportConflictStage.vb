Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore

''' <summary>
''' A stage, ie. a single part of a wizard, represented by a control which sits in
''' the wizard.
''' </summary>
Public Class ImportConflictStage : Inherits WizardStage

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Const StageId As String = "conflict.resolver"

    ' The resolutions for the conflict set managed by this stage.
    Private mResolutions As ICollection(Of ConflictResolution)

    ' The conflicts being managed by this stage
    Private mConflicts As ConflictSet

    ' The current errors being held in this stage.
    Private mErrors As clsErrorLog

    ' Flag indicating if we should be enforcing correct resolutions within the stage
    Private mAutoResolve As Boolean

    ''' <summary>
    ''' Creates a new conflict resolution stage, which disallows committing of
    ''' conflicts which remain unresolved.
    ''' </summary>
    Public Sub New()
        Me.New(True)
    End Sub

    ''' <summary>
    ''' Creates a new conflict resolution stage, which allows or disallows committing
    ''' of conflicts which remain unresolved, dependent on the given flag.
    ''' </summary>
    ''' <param name="enforceResolutions">True to have the stage disallow committing
    ''' until there are no errors in the conflict resolutions. False to have the
    ''' stage gather the information (resolutions, errors) but not enforce their
    ''' correctness.</param>
    Public Sub New(ByVal enforceResolutions As Boolean)
        MyBase.New(My.Resources.ImportConflictStage_ResolveImportConflicts, My.Resources.ImportConflictStage_CheckTheConflictsOnTheImportedRelease)
        mAutoResolve = enforceResolutions
    End Sub

    ''' <summary>
    ''' The unique ID for this stage.
    ''' </summary>
    Public Overrides ReadOnly Property Id() As String
        Get
            If StageId Is Nothing Then Throw New NotImplementedException(My.Resources.ImportConflictStage_StageIDNotSet)
            Return StageId
        End Get
    End Property

    ''' <summary>
    ''' The control which is handling the view of this stage
    ''' </summary>
    Private ReadOnly Property ConflictSetControl() As ctlConflictSet
        Get
            Return DirectCast(mControl, ctlConflictSet)
        End Get
    End Property

    ''' <summary>
    ''' The conflicts being handled by this stage.
    ''' </summary>
    Public Property Conflicts() As ConflictSet
        Get
            Return mConflicts
        End Get
        Set(ByVal value As ConflictSet)
            mConflicts = value
            If ConflictSetControl IsNot Nothing Then ConflictSetControl.Conflicts = value
            ' If we're setting a new conflict set, we need to ensure that we are
            ' starting from a clean page - ie. no resolutions or errors either
            mResolutions = Nothing
            mErrors = Nothing
        End Set
    End Property

    ''' <summary>
    ''' The resolutions set in this stage. These will only be non-empty while a stage
    ''' commit is in progress or after a successful commit.
    ''' </summary>
    Public ReadOnly Property Resolutions() As ICollection(Of ConflictResolution)
        Get
            If mResolutions Is Nothing Then Return GetEmpty.ICollection(Of ConflictResolution)()
            Return mResolutions
        End Get
    End Property

    ''' <summary>
    ''' The error log currently held in this stage
    ''' </summary>
    Public Property ErrorLog() As clsErrorLog
        Get
            Return mErrors
        End Get
        Set(ByVal value As clsErrorLog)
            mErrors = value
            ConflictSetControl.ErrorLog = value
        End Set
    End Property

    ''' <summary>
    ''' Creates the control which displays or modifies the contents of this stage
    ''' </summary>
    ''' <returns>The control used to display and modify this stage's contents.
    ''' </returns>
    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Dim cs As New ctlConflictSet()
        cs.Conflicts = mConflicts
        Return cs
    End Function

    ''' <summary>
    ''' Initialises this stage.
    ''' </summary>
    ''' <param name="wiz">The wizard that this stage is being initialised on.</param>
    Friend Overrides Sub Init(ByVal wiz As frmStagedWizard)
        MyBase.Init(wiz)
        ' reset the resolutions to ensure we don't have any holdover from previous
        ' incarnations of the stage.
        mResolutions = Nothing
    End Sub

    ''' <summary>
    ''' Flag to indicate whether this stage should be enforcing the conflict
    ''' resolutions being valid - ie. by disallowing a commit to complete if errors
    ''' are outstanding.
    ''' </summary>
    Public Property EnforceResolution() As Boolean
        Get
            Return mAutoResolve
        End Get
        Set(ByVal value As Boolean)
            mAutoResolve = value
        End Set
    End Property

    ''' <summary>
    ''' Handles this stage beginning committal. This ensures that the resolutions
    ''' are retrieved from the control before any interested listeners wish to view
    ''' the resolutions and validate them.
    ''' </summary>
    ''' <param name="e">The args detailing the committing event.</param>
    Protected Overrides Sub OnCommitting(ByVal e As StageCommittingEventArgs)

        ' Save the resolutions so they are available when testing the commit.
        mResolutions = ConflictSetControl.Resolutions
        mErrors = mConflicts.Resolve(mResolutions)
        ' If we're autoresolving and we have errors, display them and
        ' cancel the commit
        If mAutoResolve AndAlso Not mErrors.IsEmpty Then
            ConflictSetControl.ErrorLog = mErrors
            e.Cancel = True
        End If
        MyBase.OnCommitting(e)

        ' If we're cancelling the commit, clear the resolutions.
        If e.Cancel Then mResolutions = Nothing

    End Sub

End Class
