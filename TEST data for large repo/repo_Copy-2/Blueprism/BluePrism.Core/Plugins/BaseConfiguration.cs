using System;
using System.Collections.Generic;

namespace BluePrism.Core.Plugins
{
    public class BaseConfiguration : IConfiguration, ICollection<IConfigElement>
    {
        private ICollection<IConfigElement> _elements;

        public BaseConfiguration()
        {
            _elements = new List<IConfigElement>();
        }

        public void Validate()
        {
            throw new NotImplementedException();
        }

        IEnumerable<IConfigElement> IConfiguration.Elements
        {
            get { return this; }
        }

        public void Add(IConfigElement item)
        {
            _elements.Add(item);
        }

        public void Clear()
        {
            _elements.Clear();
        }

        public bool Contains(IConfigElement item)
        {
            return _elements.Contains(item);
        }

        public void CopyTo(IConfigElement[] array, int arrayIndex)
        {
            _elements.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _elements.Count; }
        }

        public bool IsReadOnly
        {
            get { return _elements.IsReadOnly; }
        }

        public bool Remove(IConfigElement item)
        {
            return _elements.Remove(item);
        }

        public IEnumerator<IConfigElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
