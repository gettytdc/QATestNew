using System;
using System.Runtime.Serialization;

namespace BluePrism.Core.ActiveDirectory
{
    [Serializable, DataContract(Namespace = "bp")]
    public class QueryPageOptions
    {
        [DataMember]
        private readonly int _startIndex;
        [DataMember]
        private readonly int _pageSize;

        public QueryPageOptions(int startIndex, int pageSize)
        {
            if (startIndex < 0)
                throw new ArgumentException("Start index cannot be less than zero");
            if (pageSize < 1)
                throw new ArgumentException("Page size cannot be less than one");

            _startIndex = startIndex;
            _pageSize = pageSize;
        }
                
        public int StartIndex { get => _startIndex; }
        public int PageSize { get => _pageSize; }
    }
}
