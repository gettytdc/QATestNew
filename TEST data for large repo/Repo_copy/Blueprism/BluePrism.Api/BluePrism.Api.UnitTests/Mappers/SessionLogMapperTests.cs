namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Mappers;
    using CommonTestClasses.Extensions;
    using Domain;
    using FluentAssertions;
    using Func;
    using Models;
    using NUnit.Framework;
    using DataValueType = Domain.DataValueType;

    [TestFixture]
    public class SessionLogMapperTests
    {
        [Test]
        public void SessionLogItem_ToModel_ReturnsExpectedValue()
        {
            var item = new Domain.SessionLogItem
            {
                LogId = 1234,
                StageName = Guid.NewGuid().ToString(),
                StageType = Domain.StageTypes.Calculation,
                Result = Guid.NewGuid().ToString(),
                ResourceStartTime = OptionHelper.Some(DateTimeOffset.Now),
                HasParameters = true
            };

            var expectedResult = new Models.SessionLogItemModel
            {
                LogId = 1234,
                StageName = item.StageName,
                StageType = Models.StageTypes.Calculation,
                Result = item.Result,
                ResourceStartTime = item.ResourceStartTime is Some<DateTimeOffset> startTime ? startTime.Value : (DateTimeOffset?)null,
                HasParameters = item.HasParameters
            };

            item.ToModel().Should().Be(expectedResult);
        }

        [Test]
        [TestCaseSource(nameof(ExpectedStageTypeMappings))]
        public void StageTypes_ToModel_ReturnsExpectedValue(Domain.StageTypes domainStageType, Models.StageTypes modelStageType)
            => domainStageType.ToModel().Should().Be(modelStageType);

        [Test]
        [TestCaseSource(nameof(GetDomainStageTypeValues))]
        public void StageTypes_ToModel_CoversAllPossibleValues(Domain.StageTypes stageType)
        {
            Action test = () => stageType.ToModel();

            test.ShouldNotThrow();
        }

        [Test]
        public void StageTypes_ToModel_OnUnexpectedValue_ThrowsArgumentException()
        {
            Action test = () => ((Domain.StageTypes)(-1)).ToModel();

            test.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void ToModel_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var domainModel = new Domain.SessionLogItemParameters
            {
                Inputs = new Dictionary<string, DataValue>
                {
                    ["Field1"] = new DataValue { ValueType = DataValueType.Text, Value = "\"1\"" },
                    ["Field2"] = new DataValue { ValueType = DataValueType.Number, Value = 2M },
                },
                Outputs = new Dictionary<string, DataValue>
                {
                    ["Field1"] = new DataValue { ValueType = DataValueType.Text, Value = "\"2\"" },
                    ["Field2"] = new DataValue { ValueType = DataValueType.Number, Value = 4M },
                }
            };

            var responseModel = new Models.SessionLogParametersModel
            {
                Inputs = new Dictionary<string, DataValueModel>
                {
                    ["Field1"] = new DataValueModel { ValueType = Models.DataValueType.Text, Value = "\"1\"" },
                    ["Field2"] = new DataValueModel { ValueType = Models.DataValueType.Number, Value = 2M },
                },
                Outputs = new Dictionary<string, DataValueModel>
                {
                    ["Field1"] = new DataValueModel { ValueType = Models.DataValueType.Text, Value = "\"2\"" },
                    ["Field2"] = new DataValueModel { ValueType = Models.DataValueType.Number, Value = 4M },
                }
            };

            var result = domainModel.ToModel();
            result.ShouldBeEquivalentTo(responseModel);
        }

        private static IEnumerable<TestCaseData> ExpectedStageTypeMappings => new[]
        {
            (Domain.StageTypes.Undefined, Models.StageTypes.Undefined),
            (Domain.StageTypes.Action, Models.StageTypes.Action),
            (Domain.StageTypes.Decision, Models.StageTypes.Decision),
            (Domain.StageTypes.Calculation, Models.StageTypes.Calculation),
            (Domain.StageTypes.Data, Models.StageTypes.Data),
            (Domain.StageTypes.Collection, Models.StageTypes.Collection),
            (Domain.StageTypes.Process, Models.StageTypes.Process),
            (Domain.StageTypes.SubSheet, Models.StageTypes.SubSheet),
            (Domain.StageTypes.ProcessInfo, Models.StageTypes.ProcessInfo),
            (Domain.StageTypes.SubSheetInfo, Models.StageTypes.SubSheetInfo),
            (Domain.StageTypes.Start, Models.StageTypes.Start),
            (Domain.StageTypes.End, Models.StageTypes.End),
            (Domain.StageTypes.Anchor, Models.StageTypes.Anchor),
            (Domain.StageTypes.Note, Models.StageTypes.Note),
            (Domain.StageTypes.LoopStart, Models.StageTypes.LoopStart),
            (Domain.StageTypes.LoopEnd, Models.StageTypes.LoopEnd),
            (Domain.StageTypes.Read, Models.StageTypes.Read),
            (Domain.StageTypes.Write, Models.StageTypes.Write),
            (Domain.StageTypes.Navigate, Models.StageTypes.Navigate),
            (Domain.StageTypes.Code, Models.StageTypes.Code),
            (Domain.StageTypes.ChoiceStart, Models.StageTypes.ChoiceStart),
            (Domain.StageTypes.ChoiceEnd, Models.StageTypes.ChoiceEnd),
            (Domain.StageTypes.WaitStart, Models.StageTypes.WaitStart),
            (Domain.StageTypes.WaitEnd, Models.StageTypes.WaitEnd),
            (Domain.StageTypes.Alert, Models.StageTypes.Alert),
            (Domain.StageTypes.Exception, Models.StageTypes.Exception),
            (Domain.StageTypes.Recover, Models.StageTypes.Recover),
            (Domain.StageTypes.Resume, Models.StageTypes.Resume),
            (Domain.StageTypes.Block, Models.StageTypes.Block),
            (Domain.StageTypes.MultipleCalculation, Models.StageTypes.MultipleCalculation),
            (Domain.StageTypes.Skill, Models.StageTypes.Skill),
        }.ToTestCaseData();

        private static IEnumerable<TestCaseData> GetDomainStageTypeValues() =>
            Enum.GetValues(typeof(Domain.StageTypes))
                .Cast<Domain.StageTypes>()
                .ToTestCaseData();
    }
}
