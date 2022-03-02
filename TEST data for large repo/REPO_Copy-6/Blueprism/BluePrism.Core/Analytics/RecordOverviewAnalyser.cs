using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrism.Core.Analytics
{
    /// <summary>
    /// Record event to store information about a given action.  This is an average over the full run.
    /// </summary>
    public class RecordEvent
    {
        private class Agregate
        {
            public double Mean { get; set; }
            public double M2 { get; set; }
            public double Count { get; set; }

            public double Average() => Mean;
            public double StandardDeviation() => Math.Sqrt(Variance());
            public double Variance() => (M2 / Count);
        }

        public DateTime LastDateTime { get; private set; }

        public string Action { get; }
        //now many times has the operation been recorded.
        public int Count { get; private set; }

        //props for execution time
        public double ExecuteTime { get; private set; }
        public double MinExecutionTime { get; private set; }
        public double MaxExecutionTime { get; private set; }
        
        //props for size.
        public int MinSize { get; private set; }
        public int MaxSize { get; private set; }


        private Agregate _timeAgregate = new Agregate();
        private Agregate _sizeAgregate = new Agregate();

        public bool HotFunction { get; set; }

        //store the total amount of bytes used within the system
        private long _runningSize;

        //need to know how long ago since last execution
        private double _timesinceLastExecution;
        
        //variable to store the current stats of the system
        private double _averageImpact;
        private double _averageTime;
        
        
        public RecordEvent(string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentException("action is null or empty", nameof(action));
            }
            Action = action;
            MinExecutionTime = double.MaxValue;
            MinSize = int.MaxValue;
        }

        public void AddRecord(int size,double executeTiime,DateTime dateTime)
        {
            Count++;
            _runningSize += size;
            ExecuteTime += executeTiime;

            //calculate time since last call.
            var timespan = dateTime -LastDateTime;

            if (!LastDateTime.Equals(DateTime.MinValue))
            {
                _timesinceLastExecution += timespan.TotalMilliseconds;
            }
            LastDateTime = dateTime;

            //record the min/max values.
            MinSize = Math.Min(size, MinSize);
            MaxSize = Math.Max(size, MaxSize);
            MinExecutionTime = Math.Min(executeTiime, MinExecutionTime);
            MaxExecutionTime = Math.Max(executeTiime, MaxExecutionTime);

            //finally calculate the agregate            
            UpdateAgregate(executeTiime, _timeAgregate);
            UpdateAgregate(size, _sizeAgregate);
        }

        /// <summary>
        /// Welford's algoritm to calculate the running stats for mean/standard div etc.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="agregate"></param>

        private void UpdateAgregate(double x,Agregate agregate)
        {
            agregate.Count++;
            double newMean = agregate.Mean + (x - agregate.Mean) / agregate.Count;
            agregate.M2 += (x - agregate.Mean) * (x - newMean);
            agregate.Mean = newMean;
        }

        /// <summary>
        /// average size of the call
        /// </summary>
        /// <returns></returns>
        public double AverageSize() => _sizeAgregate.Average(); 
        public double SizeStd() => _sizeAgregate.StandardDeviation();
        
        /// <summary>
        /// Average time 
        /// </summary>
        /// <returns></returns>
        public double AverageTime() => _timeAgregate.Average();
        public double TimeStd() => _timeAgregate.StandardDeviation();

        public double AverageTimeSinceLastExecution() => (Count == 0) ? 0 : _timesinceLastExecution / Count;

        public override string ToString()
        {
            return $"Action {Action} Count {Count} : Average Size {AverageSize()} : Average Time {AverageTime()} : Impact {Impact}, {ExecutionImpact}";
        }

        //need better names for this
        public double Impact { get { return _averageImpact; } }
        public double ExecutionImpact { get { return _averageTime; } }

        public void CalculateStatisics(int overallCount,double overallExecTime)
        {
            //don't know if this is correct
            const double MagicThreshold = 0.85;
            _averageImpact = ((double)Count / overallCount)*100d;
            _averageTime = (AverageTime() / overallExecTime)*100d;

            var calc = ((double)Count / overallCount) * AverageTime();
            HotFunction = calc > MagicThreshold;           
        }
    }

    /// <summary>
    /// communication data container class 
    /// </summary>
    public class RecordOverviewAnalyser : RecordAnalyserBase , IMessageEventLogger
    {
        private readonly IDictionary<int, RecordEvent> _callCount = new Dictionary<int, RecordEvent>();
        
        /// <summary>
        /// Simple method to store call count.
        /// </summary>
        /// <param name="action"></param>
        public void RecordCallEvent(string action, int size, double time, DateTime executionTime)
        {
            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentException($"action is null or empty", nameof(action));
            }
            if (executionTime == DateTime.MinValue)
            {
                throw new ArgumentException($"execution time has not been set", nameof(executionTime));
            }

            int key = GetKey(action);
            if (!_callCount.ContainsKey(key))
            {
                _callCount.Add(key, new RecordEvent(action));
            }
            _callCount[key].AddRecord(size, time, executionTime);
        }

        /// <summary>
        /// Calculate the percentage impact.
        /// </summary>
        public void Analyse()
        {
            //the total number of calls
            int totalCount = _callCount.Sum(s => s.Value.Count);
            double totalTime = _callCount.Sum(s => s.Value.ExecuteTime);

            foreach (var record in _callCount.Values)
            {
                record.CalculateStatisics(totalCount, totalTime);
            }
        }

        /// <summary>
        /// Create a report of all items contained within the table
        /// </summary>
        public IEnumerable<string> CreateReport(bool includeLegend)
        {
            var lines = new List<string>();
            var reports = _callCount.Values;

            if (includeLegend)
            {
                lines.Add($"Action,Count,MinSize,MaxSize,AvgSize,StdivTime [ms],MinExecTime [ms],MaxExecTime [ms],AvgExecTime [ms],StdivExecTime [ms],TotalExecTime [ms],AvgTimeSinceLastExec [ms],Call %,Time %,HotFunction,UTC now: {DateTime.UtcNow}");
            }

            //how todo the ordering, 
            var records = reports.OrderByDescending(s => s.Count);

            foreach (var record in records)
            {
                lines.Add($"{record.Action},{record.Count}," +
                          $"{record.MinSize},{record.MaxSize},{record.AverageSize()},{record.SizeStd()}," +
                          $"{record.MinExecutionTime},{record.MaxExecutionTime},{record.AverageTime()},{record.TimeStd()},{record.ExecuteTime}," +
                          $"{record.AverageTimeSinceLastExecution()},{record.Impact},{record.ExecutionImpact},{record.HotFunction}");
            }
            return lines;
        }

        /// <summary>
        /// Reset the container for the next run.
        /// </summary>
        public void Clear()
        {
            _callCount.Clear();
        }
    }
}
