''' <summary>
''' Provides a way to map keycodes to the appropriate emulator control
''' codes or menomic sequences. The names of the enum are used to
''' parse text and so should not be modified. Custom attributes are added
''' to provide mappings.
''' </summary>
Public Enum KeyCodeMappings

    <KeyCodeInfo("Upper shift")>
    <HLLAPITerminal.KeyCode("@S")>
    UpperShift

    <KeyCodeInfo("Alt key.")>
    <HLLAPITerminal.KeyCode("@A")>
    Alt

    <KeyCodeInfo("Ctrl key.")>
    <HLLAPITerminal.KeyCode("@r")>
    Ctrl

    <KeyCodeInfo("Tab Backward key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.BackTab)>
    <HLLAPITerminal.KeyCode("@B")>
    LeftTab

    <KeyCodeInfo("Clear key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Clear)>
    <HLLAPITerminal.KeyCode("@C")>
    Clear

    <KeyCodeInfo("Delete key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Delete)>
    <HLLAPITerminal.KeyCode("@D")>
    Delete

    <KeyCodeInfo("Send data to the host (Enter) key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Transmit)>
    <HLLAPITerminal.KeyCode("@E")>
    Enter

    <KeyCodeInfo("Erase to end of field.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Erase_Eof)>
    <HLLAPITerminal.KeyCode("@F")>
    EraseEOF

    <KeyCodeInfo("Display the host help screen.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Help)>
    <HLLAPITerminal.KeyCode("@H")>
    Help

    <KeyCodeInfo("Insert key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Insert)>
    <HLLAPITerminal.KeyCode("@I")>
    Insert

    <KeyCodeInfo("Partition Jump key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.PartitionJump)>
    <HLLAPITerminal.KeyCode("@J")>
    Jump

    <KeyCodeInfo("Left Arrow (Cursor) key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Left)>
    <HLLAPITerminal.KeyCode("@L")>
    CursorLeft

    <KeyCodeInfo("New Line key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.NewLine)>
    <HLLAPITerminal.KeyCode("@N")>
    NewLine

    <KeyCodeInfo("Space key.")>
    <HLLAPITerminal.KeyCode("@O")>
    Space

    <KeyCodeInfo("Print the host screen.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Print)>
    <HLLAPITerminal.KeyCode("@P")>
    Print

    <KeyCodeInfo("Reset key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Reset)>
    <HLLAPITerminal.KeyCode("@R")>
    Reset

    <KeyCodeInfo("Tab Forward key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Tab)>
    <HLLAPITerminal.KeyCode("@T")>
    RightTab

    <KeyCodeInfo("Up Arrow (Cursor) key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Up)>
    <HLLAPITerminal.KeyCode("@U")>
    CursorUp

    <KeyCodeInfo("Down Arrow (Cursor) key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Down)>
    <HLLAPITerminal.KeyCode("@V")>
    CursorDown

    <KeyCodeInfo("Double byte character set")>
    <HLLAPITerminal.KeyCode("@X")>
    DBCS

    <KeyCodeInfo("Right Arrow (Cursor) key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Right)>
    <HLLAPITerminal.KeyCode("@Z")>
    CursorRight

    <KeyCodeInfo("Home key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Home)>
    <HLLAPITerminal.KeyCode("@0")>
    Home

    <KeyCodeInfo("F1/PF1 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F1)>
    <HLLAPITerminal.KeyCode("@1")>
    PF1

    <KeyCodeInfo("F2/PF2 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F2)>
    <HLLAPITerminal.KeyCode("@2")>
    PF2

    <KeyCodeInfo("F3/PF3 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F3)>
    <HLLAPITerminal.KeyCode("@3")>
    PF3

    <KeyCodeInfo("F4/PF4 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F4)>
    <HLLAPITerminal.KeyCode("@4")>
    PF4

    <KeyCodeInfo("F5/PF5 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F5)>
    <HLLAPITerminal.KeyCode("@5")>
    PF5

    <KeyCodeInfo("F6/PF6 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F6)>
    <HLLAPITerminal.KeyCode("@6")>
    PF6

    <KeyCodeInfo("F7/PF7 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F7)>
    <HLLAPITerminal.KeyCode("@7")>
    PF7

    <KeyCodeInfo("F8/PF8 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F8)>
    <HLLAPITerminal.KeyCode("@8")>
    PF8

    <KeyCodeInfo("F9/PF9 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F9)>
    <HLLAPITerminal.KeyCode("@9")>
    PF9

    <KeyCodeInfo("F10/PF10 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F10)>
    <HLLAPITerminal.KeyCode("@a")>
    PF10

    <KeyCodeInfo("F11/PF11 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F11)>
    <HLLAPITerminal.KeyCode("@b")>
    PF11

    <KeyCodeInfo("F12/PF12 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F12)>
    <HLLAPITerminal.KeyCode("@c")>
    PF12

    <KeyCodeInfo("F13/PF13 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F13)>
    <HLLAPITerminal.KeyCode("@d")>
    PF13

    <KeyCodeInfo("F14/PF14 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F14)>
    <HLLAPITerminal.KeyCode("@e")>
    PF14

    <KeyCodeInfo("F15/PF15 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F15)>
    <HLLAPITerminal.KeyCode("@f")>
    PF15

    <KeyCodeInfo("F16/PF16 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F16)>
    <HLLAPITerminal.KeyCode("@g")>
    PF16

    <KeyCodeInfo("F17/PF17 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F17)>
    <HLLAPITerminal.KeyCode("@h")>
    PF17

    <KeyCodeInfo("F18/PF18 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F18)>
    <HLLAPITerminal.KeyCode("@i")>
    PF18

    <KeyCodeInfo("F19/PF19 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F19)>
    <HLLAPITerminal.KeyCode("@j")>
    PF19

    <KeyCodeInfo("F20/PF20 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F20)>
    <HLLAPITerminal.KeyCode("@k")>
    PF20

    <KeyCodeInfo("F21/PF21 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F21)>
    <HLLAPITerminal.KeyCode("@l")>
    PF21

    <KeyCodeInfo("F22/PF22 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F22)>
    <HLLAPITerminal.KeyCode("@m")>
    PF22

    <KeyCodeInfo("F23/PF23 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F23)>
    <HLLAPITerminal.KeyCode("@n")>
    PF23

    <KeyCodeInfo("F24/PF24 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.F24)>
    <HLLAPITerminal.KeyCode("@o")>
    PF24

    <KeyCodeInfo("End of Field key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.EndOfField)>
    <HLLAPITerminal.KeyCode("@q")>
    [End]

    <KeyCodeInfo("Page Up key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.PageUp)>
    <HLLAPITerminal.KeyCode("@u")>
    PageUp

    <KeyCodeInfo("Page Down key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.PageDown)>
    <HLLAPITerminal.KeyCode("@v")>
    PageDown

    <KeyCodeInfo("PA1 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.PA1)>
    <HLLAPITerminal.KeyCode("@x")>
    PA1

    <KeyCodeInfo("PA2 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.PA2)>
    <HLLAPITerminal.KeyCode("@y")>
    PA2

    <KeyCodeInfo("PA3 function key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.PA3)>
    <HLLAPITerminal.KeyCode("@z")>
    PA3

    <KeyCodeInfo("@ (at) symbol key.")>
    <HLLAPITerminal.KeyCode("@@")>
    At

    <KeyCodeInfo("Alternate Cursor key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.AltCursor)>
    <HLLAPITerminal.KeyCode("@$")>
    AlternateCursor

    <KeyCodeInfo("Backspace key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.DestructiveBackSpace)>
    <HLLAPITerminal.KeyCode("@<")>
    Backspace

    <KeyCodeInfo("Activate test program (AS/400 legacy only).")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Test)>
    <HLLAPITerminal.KeyCode("@A@C")>
    Test

    <KeyCodeInfo("Delete Word key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.DeleteWord)>
    <HLLAPITerminal.KeyCode("@A@D")>
    DeleteWord

    <KeyCodeInfo("Field Exit key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.FieldExit)>
    <HLLAPITerminal.KeyCode("@A@E")>
    FieldExit

    <KeyCodeInfo("Erase Input key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.EraseInput)>
    <HLLAPITerminal.KeyCode("@A@F")>
    EraseInput

    <KeyCodeInfo("Send system request (SysRq) key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.SystemRequest)>
    <HLLAPITerminal.KeyCode("@A@H")>
    SystemRequest

    <KeyCodeInfo("Insert toggle key.")>
    <HLLAPITerminal.KeyCode("@A@I")>
    InsertToggle

    <KeyCodeInfo("Cursor Select key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.CursorSelect)>
    <HLLAPITerminal.KeyCode("@A@J")>
    CursorSelect

    <KeyCodeInfo("Fast Left Arrow (Cursor) key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.LeftDouble)>
    <HLLAPITerminal.KeyCode("@A@L")>
    CursorLeftFast

    <KeyCodeInfo("Attention key to activate attention program (AS/400 only).")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.Attention)>
    <HLLAPITerminal.KeyCode("@A@Q")>
    Attention

    <KeyCodeInfo("Device cancel key.")>
    <HLLAPITerminal.KeyCode("@A@R")>
    DeviceCancel

    <KeyCodeInfo("Print presentation space.")>
    <HLLAPITerminal.KeyCode("@A@T")>
    PrintPS

    <KeyCodeInfo("Fast Up Arrow (Cursor) key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.UpDouble)>
    <HLLAPITerminal.KeyCode("@A@U")>
    CursorUpFast

    <KeyCodeInfo("Fast Down Arror (Cursor) key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.DownDouble)>
    <HLLAPITerminal.KeyCode("@A@V")>
    CursorDownFast

    <KeyCodeInfo("Fast Right Arrow (Cursor) key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.RightDouble)>
    <HLLAPITerminal.KeyCode("@A@Z")>
    CursorRightFast

    <KeyCodeInfo("Reverse Field key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.ReverseField)>
    <HLLAPITerminal.KeyCode("@A@9")>
    ReverseVideo

    <KeyCodeInfo("Underscore key (PC/3270).")>
    <HLLAPITerminal.KeyCode("@A@b")>
    Underscore

    <KeyCodeInfo("Reset Reverse Video key (PC/3270).")>
    <HLLAPITerminal.KeyCode("@A@c")>
    ResetReverseVideo

    <KeyCodeInfo("Red (PC/3270).")>
    <HLLAPITerminal.KeyCode("@A@d")>
    Red

    <KeyCodeInfo("Pink (PC/3270.")>
    <HLLAPITerminal.KeyCode("@A@e")>
    Pink

    <KeyCodeInfo("Green (PC/3270).")>
    <HLLAPITerminal.KeyCode("@A@f")>
    Green

    <KeyCodeInfo("Yellow (PC/3270).")>
    <HLLAPITerminal.KeyCode("@A@g")>
    Yellow

    <KeyCodeInfo("Blue (PC/3270).")>
    <HLLAPITerminal.KeyCode("@A@h")>
    Blue

    <KeyCodeInfo("Turquoise (PC/3270).")>
    <HLLAPITerminal.KeyCode("@A@i")>
    Turquoise

    <KeyCodeInfo("White (PC/3270).")>
    <HLLAPITerminal.KeyCode("@A@j")>
    White

    <KeyCodeInfo("Reset Host Color (PC/3270).")>
    <HLLAPITerminal.KeyCode("@A@l")>
    ResetHostColor

    <KeyCodeInfo("Print (PC).")>
    <HLLAPITerminal.KeyCode("@A@t")>
    PrintScrn

    <KeyCodeInfo("Rollup (PC400).")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.RollUp)>
    <HLLAPITerminal.KeyCode("@A@u")>
    Rollup

    <KeyCodeInfo("Rolldown (PC400).")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.RollDown)>
    <HLLAPITerminal.KeyCode("@A@v")>
    Rolldown

    <KeyCodeInfo("Forward Word Tab.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.NextWord)>
    <HLLAPITerminal.KeyCode("@A@y")>
    ForwardWordTab

    <KeyCodeInfo("Backward Word Tab.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.PreviousWord)>
    <HLLAPITerminal.KeyCode("@A@z")>
    BackwardWordTab

    <KeyCodeInfo("Field Minus key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.FieldMinus)>
    <HLLAPITerminal.KeyCode("@A@-")>
    FieldMinus

    <KeyCodeInfo("Field Plus key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.FieldPlus)>
    <HLLAPITerminal.KeyCode("@A@+")>
    FieldPlus

    <KeyCodeInfo("Record Backspace (PC400)")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.BackSpace)>
    <HLLAPITerminal.KeyCode("@A@<")>
    RecordBackspace

    <KeyCodeInfo("Print Presentation Space on Host (PC400)")>
    <HLLAPITerminal.KeyCode("@S@E")>
    PrintPSOH

    <KeyCodeInfo("Dup key.")>
    <HLLAPITerminal.KeyCode("@S@x")>
    Dup

    <KeyCodeInfo("Field Mark key.")>
    <ARNetTerminal.KeyCode(ARNetTerminal.ControlKeyCode.FieldMark)>
    <HLLAPITerminal.KeyCode("@S@y")>
    FieldMark
End Enum