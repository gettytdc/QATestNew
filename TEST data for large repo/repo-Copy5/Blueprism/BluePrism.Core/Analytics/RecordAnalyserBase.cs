using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrism.Core.Analytics
{
    public class RecordAnalyserBase
    {
        private readonly IList<string> _keys = new List<string>();


        /// <summary>
        /// return the action name based on the key.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected string KeyAction(int index)
        {
            return _keys[index];
        }

        /// <summary>
        /// If the system is going to store lots of records, then it would make sence not to keep storing the action
        /// over and over.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected int GetKey(string action)
        {
            if (!_keys.Contains(action))
            {
                _keys.Add(action);
            }
            return _keys.IndexOf(action);
        }
    }
}
