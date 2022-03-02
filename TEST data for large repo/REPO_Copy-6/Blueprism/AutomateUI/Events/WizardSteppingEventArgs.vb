
''' <summary>
''' Event args detailing a step transitioning from one stage to another in a
''' staged wizard. This would usually be due to the Next button being pressed
''' or such like.
''' The transition can be 'skipped' by setting the Skipped property to true
''' in the appropriate event.
''' </summary>
Friend Class WizardSteppingEventArgs : Inherits EventArgs

    ' The wizard that this event occurred on.
    Private mWizard As frmStagedWizard

    ' The step that is being stepped *to*
    Private mStep As Integer

    ' Whether to skip this step or not.
    Private mSkip As Boolean

    ''' <summary>
    ''' Creates a new event args object for the given wizard and step number
    ''' </summary>
    ''' <param name="wiz">The wizard that this event is occurring - ie. the one in
    ''' which the actual stage object is held.</param>
    ''' <param name="stepNo">The step number for the stage which is being stepped to
    ''' </param>
    Public Sub New(ByVal wiz As frmStagedWizard, ByVal stepNo As Integer)
        mWizard = wiz
        mStep = stepNo
    End Sub

    ''' <summary>
    ''' The step number for the stage which is being transitioned to.
    ''' </summary>
    Public ReadOnly Property StepNumber() As Integer
        Get
            Return mStep
        End Get
    End Property

    ''' <summary>
    ''' The stage that is being transitioned to.
    ''' </summary>
    Public ReadOnly Property Stage() As WizardStage
        Get
            Return mWizard.GetStage(mStep)
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if the stage represented in these args should be skipped or
    ''' not. Set by the event listeners to skip over a redundant stage.
    ''' </summary>
    Public Property Skip() As Boolean
        Get
            Return mSkip
        End Get
        Set(ByVal value As Boolean)
            mSkip = value
        End Set
    End Property
End Class
