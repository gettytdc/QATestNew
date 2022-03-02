''' <summary>
''' Different modes in which this viewer operates - readonly, compare, edit etc.
''' Warning: Any new Object Studio modes should be added to the ObjectStudioModes
''' collection defined after this enum
''' </summary>
<Flags>
Public Enum ProcessViewMode

    ''' <summary>
    ''' No mode at all
    ''' </summary>
    None = 0

    ''' <summary>
    ''' Typeless mode for editing, needs a type to be meaningful
    ''' </summary>
    Edit = 1

    ''' <summary>
    ''' Typeless mode for cloning, needs a type to be meaningful
    ''' </summary>
    Clone = 2

    ''' <summary>
    ''' Typeless mode for previewing, needs a type to be meaningful
    ''' </summary>
    Preview = 4

    ''' <summary>
    ''' Typeless mode for debugging, needs a type to be meaningful
    ''' </summary>
    Debug = 8

    ''' <summary>
    ''' Typeless mode for adhoc testing, needs a type to be meaningful
    ''' </summary>
    AdhocTest = 16

    ''' <summary>
    ''' Typeless mode for comparing, needs a type to be meaningful
    ''' </summary>
    Compare = 32

    ''' <summary>
    ''' All modes (without types) available in this process view mode enum
    ''' </summary>
    AllModes = Edit Or Clone Or Preview Or Debug Or AdhocTest Or Compare

    ''' <summary>
    ''' Modeless type for a business process, needs a mode to be meaningful
    ''' </summary>
    Process = 1024

    ''' <summary>
    ''' Modeless type for a business object, needs a mode to be meaningful
    ''' </summary>
    [Object] = 2048

    ''' <summary>
    ''' All types (without modes) available in this process view mode enum
    ''' </summary>
    ''' <remarks></remarks>
    AllTypes = Process Or [Object]

    ''' <summary>
    ''' Allows full editing of the process.
    ''' </summary>
    EditProcess = Edit Or Process

    ''' <summary>
    ''' Allows full editing of the process, whilst taking special care when
    ''' creating new locks etc.
    ''' </summary>
    CloneProcess = Clone Or Process

    ''' <summary>
    ''' Readonly mode. Every piece of information about the process is visible
    ''' in this mode, but nothing may be modified or saved.
    ''' </summary>
    PreviewProcess = Preview Or Process

    ''' <summary>
    ''' Debugging mode for processes
    ''' </summary>
    DebugProcess = Debug Or Process

    ''' <summary>
    ''' Allows processes to be debugged in readonly mode, and provides buttons
    ''' for feedback to test lab.
    ''' </summary>
    AdHocTestProcess = AdhocTest Or Process

    ''' <summary>
    ''' Used in Audit comparison of processes. Displays special tooltips about
    ''' changes made to stages and applies special colours to stages.
    ''' </summary>
    CompareProcesses = Compare Or Process

    ''' <summary>
    ''' Corresponds to mode EditProcess, but for business objects
    ''' </summary>
    EditObject = Edit Or [Object]

    ''' <summary>
    ''' Corresponds to mode CloneProcess, but for business objects
    ''' </summary>
    CloneObject = Clone Or [Object]

    ''' <summary>
    ''' Corresponds to mode PreviewProcess, but for business objects
    ''' </summary>
    PreviewObject = Preview Or [Object]

    ''' <summary>
    ''' Corresponds to mode DebugProcess, but for business objects
    ''' </summary>
    DebugObject = Debug Or [Object]

    ''' <summary>
    ''' Corresponds to mode AdHocTestProcess, but for business objects
    ''' </summary>
    AdHocTestObject = AdhocTest Or [Object]

    ''' <summary>
    ''' Corresponds to mode CompareProcesses, but for business objects
    ''' </summary>
    CompareObjects = Compare Or [Object]

End Enum
