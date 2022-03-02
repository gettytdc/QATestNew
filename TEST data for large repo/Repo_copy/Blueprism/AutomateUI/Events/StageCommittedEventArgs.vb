
''' <summary>
''' Event arguments detailing the stage committed event.
''' </summary>
Public Class StageCommittedEventArgs : Inherits EventArgs

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