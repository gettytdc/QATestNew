Imports BluePrism.Server.Domain.Models

''' <summary>
''' Stage which performs some arbitrary work.
''' FIXME: Currently, this doesn't provide much of a provision for doing any work,
''' just a progress bar so that the wizard can do the work... not really ideal
''' </summary>
Public Class BackgroundWorkerStage : Inherits WizardStage

#Region " Class scope declarations "

    ''' <summary>
    ''' Event fired in order to do the work required of the background worker which
    ''' is handling the work for this stage.
    ''' </summary>
    Public Event DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs)

    ''' <summary>
    ''' Event fired when the background worker in this stage has completed
    ''' </summary>
    Public Event RunWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)

    ''' <summary>
    ''' Event fired when cancel is triggered.
    ''' </summary>
    Public Event CancelWork(ByVal sender As Object, ByVal e As EventArgs)

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Const DefaultStageId As String = "worker-stage"

#End Region

#Region " Member variables "

    Private mId As String

    ' The argument for the background worker when it is run
    Private mArg As Object

    ' The background worker object doing the work
    Private WithEvents mWorker As BackgroundWorker

    ' The control which provides the view of the background work
    Private WithEvents mWorkerCtl As ctlWorker

    ' The main label for the worker control
    Private mMainLabel As String

    ' The state label for the worker control
    Private mStateLabel As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new work stage with the default ID.
    ''' </summary>
    Public Sub New()
        Me.New(DefaultStageId, My.Resources.BackgroundWorkerStage_Working, My.Resources.BackgroundWorkerStage_PleaseWait)
    End Sub

    ''' <summary>
    ''' Creates a new work stage with the specified ID
    ''' </summary>
    Public Sub New(ByVal id As String)
        Me.New(id, My.Resources.BackgroundWorkerStage_Working, My.Resources.BackgroundWorkerStage_PleaseWait)
    End Sub

    ''' <summary>
    ''' Creates a new worker stage with the given title and subtitle and the default
    ''' ID.
    ''' </summary>
    ''' <param name="title">The title to use for the stage</param>
    ''' <param name="subtitle">The subtitle for use in the stage</param>
    Public Sub New(ByVal title As String, ByVal subtitle As String)
        Me.New(DefaultStageId, title, subtitle)
    End Sub

    ''' <summary>
    ''' Creates a new worker stage with the given id, title and subtitle
    ''' </summary>
    ''' <param name="id">The unique ID for this stage</param>
    ''' <param name="title">The title to use for the stage</param>
    ''' <param name="subtitle">The subtitle for use in the stage</param>
    Public Sub New(ByVal id As String, ByVal title As String, ByVal subtitle As String)
        MyBase.New(title, subtitle)
        mId = id
        ConstructWorker()
    End Sub



#End Region

#Region " Properties "

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Overrides ReadOnly Property Id() As String
        Get
            Return mId
        End Get
    End Property

    ''' <summary>
    ''' Handles the stage control being removed from this stage, by ensuring that
    ''' the equivalent worker control reference is also removed.
    ''' </summary>
    ''' <param name="ctl">The control which is about to be removed</param>
    Protected Overrides Sub OnStageControlRemoving(ByVal ctl As ctlWizardStageControl)
        If mWorkerCtl Is ctl Then mWorkerCtl = Nothing
        SignalCancel()
    End Sub

    Public Sub SignalCancel()
        mWorker.CancelAsync()
        RaiseEvent CancelWork(Me, New EventArgs)
    End Sub

    ''' <summary>
    ''' The argument to pass to the background worker when it is invoked
    ''' </summary>
    Public Property Argument() As Object
        Get
            Return mArg
        End Get
        Set(ByVal value As Object)
            mArg = value
        End Set
    End Property

    ''' <summary>
    ''' The progress bar used to display the work done thus far
    ''' </summary>
    Public ReadOnly Property Progress() As ProgressBar
        Get
            If mWorkerCtl Is Nothing Then Return Nothing
            Return mWorkerCtl.Progress
        End Get
    End Property

    ''' <summary>
    ''' The main label to use in the worker control
    ''' </summary>
    Public Property MainLabel() As String
        Get
            Return mMainLabel
        End Get
        Set(ByVal value As String)
            mMainLabel = value
            If mWorkerCtl IsNot Nothing Then mWorkerCtl.Text = value
        End Set
    End Property

    ''' <summary>
    ''' The state label held in the worker control
    ''' </summary>
    Public Property StateLabel() As String
        Get
            ' We can't rely on the memvar if the control exists, since it can be
            ' changed elsewhere.
            If mWorkerCtl IsNot Nothing Then Return mWorkerCtl.StateLabel
            Return mStateLabel
        End Get
        Set(ByVal value As String)
            mStateLabel = value
            If mWorkerCtl IsNot Nothing Then mWorkerCtl.StateLabel = value
        End Set
    End Property

    ''' <summary>
    ''' Override indicating that this stage should not be saved to the history, and
    ''' should not be reachable through any navigational tool other than
    ''' programmatically. It makes no sense to go 'back' to a background worker
    ''' stage, which would just kick off the work again anyway.
    ''' </summary>
    Public Overrides ReadOnly Property CanSaveToHistory() As Boolean
        Get
            Return False
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Construct the background work thread and set the required parameters.
    ''' Need so we can cancel the thread.
    ''' </summary>
    Private Sub ConstructWorker()
        mWorker = New BackgroundWorker()
        mWorker.WorkerReportsProgress = True
        mWorker.WorkerSupportsCancellation = True
    End Sub

    ''' <summary>
    ''' Updates the progress on this stage to the given value.
    ''' </summary>
    ''' <param name="value">The value of the progress to report</param>
    Public Sub ReportProgress(ByVal value As Integer)
        ReportProgress(value, Nothing)
    End Sub

    ''' <summary>
    ''' Updates the progress on this stage to the given value.
    ''' </summary>
    ''' <param name="value">The value of the progress to report</param>
    ''' <param name="state">The state to set in the state text as part of the
    ''' progress update.</param>
    Public Sub ReportProgress(ByVal value As Integer, ByVal state As Object)
        mWorker.ReportProgress(value, state)
    End Sub

    ''' <summary>
    ''' Creates the control used to display the work
    ''' </summary>
    ''' <returns>The control for displaying the work done on this stage.</returns>
    Protected Overrides Function CreateControl() As ctlWizardStageControl
        mWorkerCtl = New ctlWorker()
        If mMainLabel IsNot Nothing Then mWorkerCtl.Text = mMainLabel
        If mStateLabel IsNot Nothing Then mWorkerCtl.StateLabel = mStateLabel
        Return mWorkerCtl
    End Function

    ''' <summary>
    ''' Initialises this stage
    ''' </summary>
    ''' <param name="wiz">The wizard that this stage is being initialised on.</param>
    Friend Overrides Sub Init(ByVal wiz As frmStagedWizard)
        MyBase.Init(wiz)

        'Need to reconstruct, as cancelled thread can't be reused.
        If mWorker.CancellationPending Then
            ConstructWorker()
        End If

        mWorker.RunWorkerAsync()
    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles the background worker's DoWork event. This just passes on the event
    ''' to listeners of this stage's corresponding DoWork event.
    ''' </summary>
    Private Sub HandleWorkerDoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles mWorker.DoWork

        Try
            RaiseEvent DoWork(Me, e)
        Catch ex As OperationCancelledException
            RaiseEvent RunWorkerCompleted(Me, New RunWorkerCompletedEventArgs(Nothing, Nothing, True))
        End Try
    End Sub

    ''' <summary>
    ''' Handles progress being changed in the background worker. This changes the
    ''' state label if the given progress is a string.
    ''' </summary>
    Private Sub HandleWorkerProgressChanged(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) Handles mWorker.ProgressChanged
        If Not mWorker.CancellationPending Then
            mWorkerCtl.Value = e.ProgressPercentage
            Dim txt As String = TryCast(e.UserState, String)
            If txt IsNot Nothing Then mWorkerCtl.StateLabel = txt
        End If
    End Sub

    ''' <summary>
    ''' Handles the worker being completed.
    ''' This updates the state label with either an error message, or the output
    ''' message from the work thread, if one is given in the result.
    ''' It them passes on the event to listeners of this stage's RunWorkerCompleted
    ''' event (which may change the status text itself)
    ''' </summary>
    Private Sub HandleWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) Handles mWorker.RunWorkerCompleted
        If e.Error IsNot Nothing Then
            mWorkerCtl.StateLabel = My.Resources.BackgroundWorkerStage_AnErrorOccurred & e.Error.Message
        Else
            Dim txt As String = TryCast(e.Result, String)
            If txt IsNot Nothing Then mWorkerCtl.StateLabel = txt
            mWorkerCtl.Text = My.Resources.BackgroundWorkerStage_Complete
        End If
        RaiseEvent RunWorkerCompleted(Me, e)
    End Sub
#End Region

End Class
