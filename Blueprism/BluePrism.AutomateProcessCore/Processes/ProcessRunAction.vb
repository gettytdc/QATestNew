Namespace Processes

    ''' <summary>
    ''' The different actions used to affect the run state of the process.
    ''' </summary>
    Public Enum ProcessRunAction

        ''' <summary>
        ''' Steps along to the next stage in the process, into a subsheet/subprocess
        ''' if necessary.
        ''' </summary>
        StepIn

        ''' <summary>
        ''' As for StepIn unless the current stage is a subsheet or a subprocess.
        ''' If a subsheet/subprocess then runs continuously to next stage after
        ''' subsheet/subprocess.
        ''' </summary>
        StepOver

        ''' <summary>
        ''' Runs the process continuously to the end of the current sheet/process.
        ''' </summary>
        StepOut

        ''' <summary>
        ''' Temporarily pauses running at the current stage. Continuation is possible
        ''' using one of Go, StepIn, StepOver, StepOut.
        ''' </summary>
        Pause

        ''' <summary>
        ''' Runs through the whole process stage by stage in sequence, until the end
        ''' of the process is reached or the Stop action is performed. The RunNextStep
        ''' action must then be repeatedly used until the RunState changes from
        ''' RunState.Running.
        ''' </summary>
        Go

        ''' <summary>
        ''' Perform the next step in running. This is only applicable after Go has been
        ''' used.
        ''' </summary>
        RunNextStep

        ''' <summary>
        ''' Resets the run state of the process back to 'Off'. This is used to perform
        ''' a 'restart', although it is actually the next debug action that causes the
        ''' process to 'restart' by going back into a true run state.
        ''' </summary>
        Reset

        ''' <summary>
        ''' Go to the top of the page specified via SetRunPage. This is similar to
        ''' resetting, but used for repeated calls to a business object without
        ''' actually resetting it.
        ''' </summary>
        GotoPage

    End Enum

End Namespace