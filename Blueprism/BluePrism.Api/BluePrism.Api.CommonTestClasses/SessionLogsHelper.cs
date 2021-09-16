namespace BluePrism.Api.CommonTestClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using AutomateAppCore;
    using BPCoreLib.Data;
    using BpLibAdapters.Extensions;
    using BpLibAdapters.Mappers;
    using Domain;
    using FluentAssertions;
    using Func;
    using StageTypes = AutomateProcessCore.StageTypes;

    public static class SessionLogsHelper
    {
        public static ICollection<clsSessionLogEntry> GetTestBluePrismLogEntries(int count, DateTimeOffset? startTime = null)
        {
            var testLogEntries = new List<clsSessionLogEntry>();

            using (var reader = GetTestDataTable(count, startTime).CreateDataReader())
            {
                var provider = new ReaderDataProvider(reader);
                while (reader.Read())
                {
                    testLogEntries.Add(new clsSessionLogEntry(provider));
                }
            }

            return testLogEntries;
        }

        public static ICollection<SessionLogItem> GetTestDomainSessionLogItems(int count, DateTimeOffset? starTime = null) =>
            GetTestBluePrismLogEntries(count, starTime).Select(x => x.ToDomainObject()).ToArray();


        public static ICollection<clsSessionLogEntry> GetEmptyLogEntries() =>
            new[] { new clsSessionLogEntry(new EmptyDataProvider()) };

        private static DataTable GetTestDataTable(int rowCount, DateTimeOffset? startTime)
        {
            var table = new DataTable("Table")
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn("startdatetime", typeof(string)),
                    new DataColumn("starttimezoneoffset", typeof(int)),
                    new DataColumn("stagename", typeof(string)),
                    new DataColumn("stagetype", typeof(int)),
                    new DataColumn("result", typeof(string)),
                    new DataColumn("resulttype", typeof(int)),
                    new DataColumn("attributexml", typeof(string)),
                    new DataColumn("lognumber", typeof(int)),
                    new DataColumn("logid", typeof(long))
                },
            };

            for (var i = 0; i < rowCount; ++i)
            {
                var row = table.NewRow();
                row["startdatetime"] = startTime ?? DateTime.UtcNow.AddDays(1).AddHours(i).ToDateTimeOffset();
                row["stagetype"] = AutomateProcessCore.StageTypes.Data;
                row["stagename"] = "TestStage";
                row["result"] = "TestResult";
                row["resulttype"] = AutomateProcessCore.DataType.unknown;
                row["attributexml"] = "<?xml ?>";
                row["lognumber"] = i;
                row["logid"] = i;

                table.Rows.Add(row);
            }

            return table;
        }

        public static void ValidateModelsAreEqual(clsSessionLogEntry clsSessionLogEntry, SessionLogItem domainSessionLogItem)
        {
            clsSessionLogEntry.LogId.Should().Be(domainSessionLogItem.LogId);
            clsSessionLogEntry.StageName.Should().Be(domainSessionLogItem.StageName);
            clsSessionLogEntry.Result.Should().Be(domainSessionLogItem.Result);
            clsSessionLogEntry.StartDate.Should().Be(domainSessionLogItem.ResourceStartTime is Some<DateTimeOffset> dt ? dt.Value : DateTimeOffset.MinValue);
            ValidateStageTypesAreEqual(clsSessionLogEntry.StageType, domainSessionLogItem.StageType).Should().BeTrue();
            (!string.IsNullOrEmpty(clsSessionLogEntry.AttributeXml) == domainSessionLogItem.HasParameters)
                .Should()
                .BeTrue();
        }

        private static bool ValidateStageTypesAreEqual(StageTypes bluePrismStageTypes, Domain.StageTypes domainStageTypes) =>
            (bluePrismStageTypes == StageTypes.Action && domainStageTypes == Domain.StageTypes.Action) ||
            (bluePrismStageTypes == StageTypes.Alert && domainStageTypes == Domain.StageTypes.Alert) ||
            (bluePrismStageTypes == StageTypes.Anchor && domainStageTypes == Domain.StageTypes.Anchor) ||
            (bluePrismStageTypes == StageTypes.Block && domainStageTypes == Domain.StageTypes.Block) ||
            (bluePrismStageTypes == StageTypes.Calculation && domainStageTypes == Domain.StageTypes.Calculation) ||
            (bluePrismStageTypes == StageTypes.Code && domainStageTypes == Domain.StageTypes.Code) ||
            (bluePrismStageTypes == StageTypes.Collection && domainStageTypes == Domain.StageTypes.Collection) ||
            (bluePrismStageTypes == StageTypes.Data && domainStageTypes == Domain.StageTypes.Data) ||
            (bluePrismStageTypes == StageTypes.Decision && domainStageTypes == Domain.StageTypes.Decision) ||
            (bluePrismStageTypes == StageTypes.End && domainStageTypes == Domain.StageTypes.End) ||
            (bluePrismStageTypes == StageTypes.Exception && domainStageTypes == Domain.StageTypes.Exception) ||
            (bluePrismStageTypes == StageTypes.Navigate && domainStageTypes == Domain.StageTypes.Navigate) ||
            (bluePrismStageTypes == StageTypes.Note && domainStageTypes == Domain.StageTypes.Note) ||
            (bluePrismStageTypes == StageTypes.Process && domainStageTypes == Domain.StageTypes.Process) ||
            (bluePrismStageTypes == StageTypes.Read && domainStageTypes == Domain.StageTypes.Read) ||
            (bluePrismStageTypes == StageTypes.Recover && domainStageTypes == Domain.StageTypes.Recover) ||
            (bluePrismStageTypes == StageTypes.Resume && domainStageTypes == Domain.StageTypes.Resume) ||
            (bluePrismStageTypes == StageTypes.Skill && domainStageTypes == Domain.StageTypes.Skill) ||
            (bluePrismStageTypes == StageTypes.Start && domainStageTypes == Domain.StageTypes.Start) ||
            (bluePrismStageTypes == StageTypes.Undefined && domainStageTypes == Domain.StageTypes.Undefined) ||
            (bluePrismStageTypes == StageTypes.Write && domainStageTypes == Domain.StageTypes.Write) ||
            (bluePrismStageTypes == StageTypes.ChoiceEnd && domainStageTypes == Domain.StageTypes.ChoiceEnd) ||
            (bluePrismStageTypes == StageTypes.ChoiceStart && domainStageTypes == Domain.StageTypes.ChoiceStart) ||
            (bluePrismStageTypes == StageTypes.LoopEnd && domainStageTypes == Domain.StageTypes.LoopEnd) ||
            (bluePrismStageTypes == StageTypes.LoopStart && domainStageTypes == Domain.StageTypes.LoopStart) ||
            (bluePrismStageTypes == StageTypes.MultipleCalculation && domainStageTypes == Domain.StageTypes.MultipleCalculation) ||
            (bluePrismStageTypes == StageTypes.ProcessInfo && domainStageTypes == Domain.StageTypes.ProcessInfo) ||
            (bluePrismStageTypes == StageTypes.SubSheet && domainStageTypes == Domain.StageTypes.SubSheet) ||
            (bluePrismStageTypes == StageTypes.WaitEnd && domainStageTypes == Domain.StageTypes.WaitEnd) ||
            (bluePrismStageTypes == StageTypes.WaitStart && domainStageTypes == Domain.StageTypes.WaitStart) ||
            (bluePrismStageTypes == StageTypes.SubSheetInfo && domainStageTypes == Domain.StageTypes.SubSheetInfo);

    }
}
