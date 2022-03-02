
''' <summary>
''' Possible Run state for a process.
''' </summary>
''' <remarks>
''' Full documentation is available on the wiki on the Automate page:
''' https://portal.blueprism.com/wiki/index.php?title=Automate#Process_Run_State
''' </remarks>
Public Enum ProcessRunState

    ''' <summary>
    ''' No running is underway
    ''' </summary>
    Off

    ''' <summary>
    ''' Running is underway but has been paused at the current stage.
    ''' </summary>
    Paused

    ''' <summary>
    ''' Running has been terminated due to an error.
    ''' </summary>
    Failed

    ''' <summary>
    ''' The process is executing a single step.
    ''' </summary>
    Stepping

    ''' <summary>
    ''' The process is executing a single step in step over mode.
    ''' </summary>
    SteppingOver

    ''' <summary>
    ''' The process is running
    ''' </summary>
    Running

    ''' <summary>
    ''' The process has completed
    ''' </summary>
    Completed

    ''' <summary>
    ''' Used by a child process/object during debug to indicate
    ''' that user closed window before successful completion.
    ''' </summary>
    ''' <remarks>The parent process/object should respond by entering a 'failed'
    ''' state</remarks>
    Aborted

End Enum
