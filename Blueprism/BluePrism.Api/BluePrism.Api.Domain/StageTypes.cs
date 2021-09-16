namespace BluePrism.Api.Domain
{
    using System;

    [Flags]
    public enum StageTypes
    {
        Undefined = 1 << 0,
        Action = 1 << 1,
        Decision = 1 << 2,
        Calculation = 1 << 3,
        Data = 1 << 4,
        Collection = 1 << 5,
        Process = 1 << 6,
        SubSheet = 1 << 7,
        ProcessInfo = 1 << 8,
        SubSheetInfo = 1 << 9,
        Start = 1 << 10,
        End = 1 << 11,
        Anchor = 1 << 12,
        Note = 1 << 13,
        LoopStart = 1 << 14,
        LoopEnd = 1 << 15,
        Read = 1 << 16,
        Write = 1 << 17,
        Navigate = 1 << 18,
        Code = 1 << 19,
        ChoiceStart = 1 << 20,
        ChoiceEnd = 1 << 21,
        WaitStart = 1 << 22,
        WaitEnd = 1 << 23,
        Alert = 1 << 24,
        Exception = 1 << 25,
        Recover = 1 << 26,
        Resume = 1 << 27,
        Block = 1 << 28,
        MultipleCalculation = 1 << 29,
        Skill = 1 << 30,
    }
}
