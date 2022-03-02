using System;
using BenchmarkDotNet.Attributes;
using BluePrism.AutomateProcessCore;

namespace BluePrism.AutomateAppCore.Benchmarks
{
    [MemoryDiagnoser]
    public class CalandarInternalBusinessObjectBenchmark
    {
        private readonly clsCalendarsBusinessObject _target;
        private readonly clsArgumentList _workingDaysInputs;
        private readonly clsArgumentList _isWorkingDayInputs;

        public CalandarInternalBusinessObjectBenchmark()
        {
            _target = new clsCalendarsBusinessObject(null, null);

            _workingDaysInputs = new clsArgumentList
            {
                new clsArgument("Calendar Name", new clsProcessValue("Working Week / No Holidays")),
                new clsArgument("First Date", new clsProcessValue(DataType.date, DateTime.Today.AddDays(-1))),
                new clsArgument("Last Date", new clsProcessValue(DataType.date, DateTime.Today))
            };

            _isWorkingDayInputs = new clsArgumentList
            {
                new clsArgument("Calendar Name", new clsProcessValue("Working Week / No Holidays")),
                new clsArgument("Single Date", new clsProcessValue(DataType.date, DateTime.Today)),
            };
        }

        [Benchmark]
        public void InitBenchmark()
        {
            _target.DoInit();
        }

        [Benchmark]
        public void GetWorkingDaysInRangeBenchmark()
        {
            var outputs = new clsArgumentList();
            _target.DoAction("Get Working Days In Range", null, _workingDaysInputs, ref outputs);
        }

        [Benchmark]
        public void CountWorkingDaysInRangeBenchmark()
        {
            var outputs = new clsArgumentList();
            _target.DoAction("Count Working Days In Range", null, _workingDaysInputs, ref outputs);
        }

        [Benchmark]
        public void IsWorkingDayBenchmark()
        {
            var outputs = new clsArgumentList();
            _target.DoAction("Is Working Day", null, _isWorkingDayInputs, ref outputs);
        }
    }
}
