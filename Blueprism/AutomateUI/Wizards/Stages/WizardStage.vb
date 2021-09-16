''' <summary>
''' Class representing a stage in a wizard.
''' </summary>
Public MustInherit Class WizardStage : Implements IDisposable

#Region " Event Declarations "

    ''' <summary>
    ''' Event fired when a stage is in the process of being committed.
    ''' </summary>
    ''' <param name="sender">The stage being committed</param>
    ''' <param name="e">The args detailing the commit. Setting the 
    ''' <see cref="StageCommittingEventArgs.Cancel"/> property to True will cause
    ''' the commit to be cancelled.</param> 
    Public Event Committing(ByVal sender As WizardStage, ByVal e As StageCommittingEventArgs)

    ''' <summary>
    ''' Event fired when a stage has been committed.
    ''' </summary>
    ''' <param name="sender">The stage which has been committed</param>
    ''' <param name="e">The args detailing the commit</param>
    Public Event Committed(ByVal sender As WizardStage, ByVal e As StageCommittedEventArgs)

    ''' <summary>
    ''' Event fired when a stage has been 'activated'. Usually this implies the the
    ''' enter key has been pressed in a text box, or a list entry has been double-
    ''' clicked. Its precise meaning will depend on the context, ie the stage which
    ''' has been activated.
    ''' </summary>
    ''' <param name="sender">The stage which has been activated</param>
    ''' <param name="e">The args detailing the event.</param>
    Public Event Activated(ByVal sender As WizardStage, ByVal e As ActivationEventArgs)

    ''' <summary>
    ''' Event fired when the title or subtitle of a stage have been changed.
    ''' </summary>
    Public Event TitlesChanged(ByVal sender As WizardStage, ByVal e As EventArgs)

#End Region

#Region " Member variables "

    ' To detect redundant calls to Dispose()
    Private mDisposed As Boolean = False

    ' The title of the stage
    Private mTitle As String

    ' The subtitle of the stage
    Private mSubtitle As String

    ' The control used by this stage.
    Protected WithEvents mControl As ctlWizardStageControl

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new stage with the given title and subtitle
    ''' </summary>
    ''' <param name="title">The title for this stage.</param>
    ''' <param name="subtitle">The subtitle for this stage.</param>
    Public Sub New(ByVal title As String, ByVal subtitle As String)
        mTitle = title
        mSubtitle = subtitle
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The title of this stage.
    ''' </summary>
    Public Property Title() As String
        Get
            Return mTitle
        End Get
        Set(ByVal value As String)
            If mTitle <> value Then
                mTitle = value
                RaiseEvent TitlesChanged(Me, EventArgs.Empty)
            End If
        End Set
    End Property

    ''' <summary>
    ''' The title of this stage.
    ''' </summary>
    Public Property SubTitle() As String
        Get
            Return mSubtitle
        End Get
        Set(ByVal value As String)
            If mSubtitle <> value Then
                mSubtitle = value
                RaiseEvent TitlesChanged(Me, EventArgs.Empty)
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets the control for this stage, or sets it to null, thus disposing of it.
    ''' </summary>
    ''' <exception cref="ArgumentException">If an attempt is made to set this control
    ''' to something other than null</exception>
    Public Property Control() As ctlWizardStageControl
        Get
            If mControl Is Nothing Then mControl = CreateControl()
            Return mControl
        End Get
        Set(ByVal value As ctlWizardStageControl)
            If value IsNot Nothing Then Throw New ArgumentException(My.Resources.WizardStage_CanOnlySetControlToNull)
            If mControl IsNot Nothing Then
                OnStageControlRemoving(mControl)
                mControl.Dispose()
                mControl = Nothing
            End If
        End Set
    End Property

    ''' <summary>
    ''' Flag indicating that this stage can be safely saved to the history in a
    ''' wizard, such that it can be reshown by clicking on the <c>Back</c> button
    ''' or through some other navigational tool
    ''' </summary>
    Public Overridable ReadOnly Property CanSaveToHistory() As Boolean
        Get
            Return True
        End Get
    End Property

#End Region

#Region " Abstract methods / properties "

    ''' <summary>
    ''' The unique ID for this stage to identify it with later.
    ''' </summary>
    Public MustOverride ReadOnly Property Id() As String

    ''' <summary>
    ''' Creates the control used by this stage.
    ''' </summary>
    ''' <returns>The control used to represent this stage.</returns>
    Protected MustOverride Function CreateControl() As ctlWizardStageControl

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles the control being 'activated'. What this means depends largely on
    ''' the control - it could be pressing enter in a text field, or double clicking
    ''' a listview item.
    ''' Whichever, this fires the <see cref="Activated"/> event for this stage.
    ''' </summary>
    Private Sub HandleControlActivated(ByVal sender As Object, ByVal e As ActivationEventArgs) Handles mControl.Activated
        OnActivated(e)
    End Sub

#End Region

#Region " Other methods "

    ''' <summary>
    ''' Initialises this stage
    ''' </summary>
    ''' <param name="wiz">The wizard that this stage is being initialised on.</param>
    Friend Overridable Sub Init(ByVal wiz As frmStagedWizard)
    End Sub

    ''' <summary>
    ''' Fires the Activated event for this stage.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Sub OnActivated(ByVal e As ActivationEventArgs)
        RaiseEvent Activated(Me, e)
    End Sub

    ''' <summary>
    ''' Fires the Committing event for this stage.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overridable Sub OnCommitting(ByVal e As StageCommittingEventArgs)
        RaiseEvent Committing(Me, e)
    End Sub

    ''' <summary>
    ''' Fires the Committed event for this stage.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overridable Sub OnCommitted(ByVal e As StageCommittedEventArgs)
        RaiseEvent Committed(Me, e)
    End Sub

    ''' <summary>
    ''' Handler called immediately before the wizard stage control for this stage
    ''' is removed.
    ''' </summary>
    ''' <param name="ctl">The control which is about to be removed.</param>
    ''' <remarks>There is no equivalent public event for this - subclasses should
    ''' override this method in order to handle the stage control being removed.
    ''' Typically, this might be used for a stage which needs particular events, and
    ''' requires a WithEvents reference to the control itself.
    ''' </remarks>
    Protected Overridable Sub OnStageControlRemoving(ByVal ctl As ctlWizardStageControl)
    End Sub

    ''' <summary>
    ''' Commits this stage from the given wizard
    ''' </summary>
    ''' <returns>True if the commit was successful, False if there were validation
    ''' errors or other problems which meant that the commit couldn't complete.
    ''' </returns>
    Friend Overridable Function TryCommit() As Boolean
        Dim ce As New StageCommittingEventArgs(Me)
        OnCommitting(ce)
        If ce.Cancel Then Return False
        OnCommitted(New StageCommittedEventArgs(Me))
        Return True
    End Function

#End Region

#Region " Finalize / Dispose "

    ''' <summary>
    ''' Disposes of this stage
    ''' </summary>
    ''' <param name="disposing">True if this is being disposed explicitly, False if
    ''' it is being disposed of on object finalization.</param>
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not mDisposed Then
            If disposing Then
                Me.Control = Nothing
            End If
        End If
        mDisposed = True
    End Sub

    ''' <summary>
    ''' Explicitly disposes of this stage.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class
