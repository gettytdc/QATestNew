''' <summary>
''' Event arguments detailing the stage committing event.
''' </summary>
Public Class StageCommittingEventArgs : Inherits CancelEventArgs

    ' The stage affected
    Private mStage As WizardStage

    ''' <summary>
    ''' Creates a new stage committed event args object for the given stage
    ''' </summary>
    ''' <param name="stg">The stage affected by the event</param>
    Public Sub New(ByVal stg As WizardStage)
        mStage = stg
    End Sub

End Class
