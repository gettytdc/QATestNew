''' <summary>
''' Enumeration of the different types of stage
''' </summary>
Public Enum StageTypes As Integer
    Undefined = 1
    Action = 2
    Decision = 4
    Calculation = 8
    Data = 16
    Collection = 32
    Process = 64
    SubSheet = 128
    ProcessInfo = 256
    SubSheetInfo = 512
    Start = 1024
    [End] = 2048
    Anchor = 4096
    Note = 8192
    LoopStart = 16384
    LoopEnd = 32768
    Read = 65536
    Write = 131072
    Navigate = 262144
    Code = 524288
    ChoiceStart = 1048576
    ChoiceEnd = 2097152
    WaitStart = 4194304
    WaitEnd = 8388608
    Alert = 16777216
    Exception = 33554432
    Recover = 67108864
    [Resume] = 134217728
    Block = 268435456
    MultipleCalculation = 536870912
    Skill = 1073741824
End Enum
