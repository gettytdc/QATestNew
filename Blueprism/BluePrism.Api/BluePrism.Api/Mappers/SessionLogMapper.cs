namespace BluePrism.Api.Mappers
{
    using System;
    using System.Runtime.CompilerServices;
    using Func;
    using Models;

    public static class SessionLogMapper
    {
        public static Models.SessionLogItemModel ToModel(this Domain.SessionLogItem sessionLogItem) =>
            new Models.SessionLogItemModel
            {
                LogId = sessionLogItem.LogId,
                StageName = sessionLogItem.StageName,
                StageType = sessionLogItem.StageType.ToModel(),
                ResourceStartTime = sessionLogItem.ResourceStartTime is Some<DateTimeOffset> startTime ? startTime.Value : (DateTimeOffset?)null,
                Result = sessionLogItem.Result,
                HasParameters = sessionLogItem.HasParameters,
            };

        public static Models.StageTypes ToModel(this Domain.StageTypes stageType)
        {
            switch (stageType)
            {
                case Domain.StageTypes.Undefined:
                    return Models.StageTypes.Undefined;

                case Domain.StageTypes.Action:
                    return Models.StageTypes.Action;

                case Domain.StageTypes.Decision:
                    return Models.StageTypes.Decision;

                case Domain.StageTypes.Calculation:
                    return Models.StageTypes.Calculation;

                case Domain.StageTypes.Data:
                    return Models.StageTypes.Data;

                case Domain.StageTypes.Collection:
                    return Models.StageTypes.Collection;

                case Domain.StageTypes.Process:
                    return Models.StageTypes.Process;

                case Domain.StageTypes.SubSheet:
                    return Models.StageTypes.SubSheet;

                case Domain.StageTypes.ProcessInfo:
                    return Models.StageTypes.ProcessInfo;

                case Domain.StageTypes.SubSheetInfo:
                    return Models.StageTypes.SubSheetInfo;

                case Domain.StageTypes.Start:
                    return Models.StageTypes.Start;

                case Domain.StageTypes.End:
                    return Models.StageTypes.End;

                case Domain.StageTypes.Anchor:
                    return Models.StageTypes.Anchor;

                case Domain.StageTypes.Note:
                    return Models.StageTypes.Note;

                case Domain.StageTypes.LoopStart:
                    return Models.StageTypes.LoopStart;

                case Domain.StageTypes.LoopEnd:
                    return Models.StageTypes.LoopEnd;

                case Domain.StageTypes.Read:
                    return Models.StageTypes.Read;

                case Domain.StageTypes.Write:
                    return Models.StageTypes.Write;

                case Domain.StageTypes.Navigate:
                    return Models.StageTypes.Navigate;

                case Domain.StageTypes.Code:
                    return Models.StageTypes.Code;

                case Domain.StageTypes.ChoiceStart:
                    return Models.StageTypes.ChoiceStart;

                case Domain.StageTypes.ChoiceEnd:
                    return Models.StageTypes.ChoiceEnd;

                case Domain.StageTypes.WaitStart:
                    return Models.StageTypes.WaitStart;

                case Domain.StageTypes.WaitEnd:
                    return Models.StageTypes.WaitEnd;

                case Domain.StageTypes.Alert:
                    return Models.StageTypes.Alert;

                case Domain.StageTypes.Exception:
                    return Models.StageTypes.Exception;

                case Domain.StageTypes.Recover:
                    return Models.StageTypes.Recover;

                case Domain.StageTypes.Resume:
                    return Models.StageTypes.Resume;

                case Domain.StageTypes.Block:
                    return Models.StageTypes.Block;

                case Domain.StageTypes.MultipleCalculation:
                    return Models.StageTypes.MultipleCalculation;

                case Domain.StageTypes.Skill:
                    return Models.StageTypes.Skill;

                default:
                    throw new ArgumentException($"Unexpected stage type value: {stageType}", nameof(stageType));
            }
        }

        public static Models.SessionLogParametersModel ToModel(this Domain.SessionLogItemParameters @this) =>
            new SessionLogParametersModel
            {
                Inputs = @this.Inputs.ToModel(),
                Outputs = @this.Outputs.ToModel()
            };
    }
}
