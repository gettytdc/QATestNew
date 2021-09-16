using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrism.Core.Analytics
{
    public interface IMessageEventLogger
    {
        void RecordCallEvent(string action, int size, double time, DateTime executionTime);
        void Analyse();
        IEnumerable<string> CreateReport(bool includeLegend);
        void Clear();
    }
}
