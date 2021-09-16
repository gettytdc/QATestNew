using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BluePrism.Core.Resources
{
    public class ResourceConnectionStatistics
    {
        private readonly IDictionary<Guid, ResourceConnectionStatistic> _internalDictionary
            = new ConcurrentDictionary<Guid, ResourceConnectionStatistic>();

        public IDictionary<Guid, ResourceConnectionStatistic> GetAll() => _internalDictionary;
        
        public void Update(Guid id, ResourceConnectionStatistic newItem) => _internalDictionary[id] = newItem;

        public void Remove(Guid id) => _internalDictionary.Remove(id);
    }
}
