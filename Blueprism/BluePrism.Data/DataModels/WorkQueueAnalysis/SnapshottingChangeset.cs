using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace BluePrism.Data.DataModels.WorkQueueAnalysis
{
    [Serializable]
    [DataContract(Namespace = "bp")]
    public class SnapshottingChangeset
    {

        /// <summary>
        /// This figure corresponds to the number of rows inserted into the
        /// BPMIConfiguredSnapshot table when 100 queues are being snapshotted at
        /// 15 minute intervals, 24 hours a day(giving 96 snapshots), 7 days a week.
        /// The impact on the BPMIQueueSnapshot table is a multiple of 4, so any
        /// future increase of this value should be done with this in mind,
        /// the total number of rows in that table at any time will be 4 x this number
        /// </summary>
        private const int _configuredSnapshotRowLimit = (100 * 96 * 7);

        [DataMember] private List<int> _listQueuesToAdd;
        [DataMember] private List<int> _listQueuesToRemove;
        [DataMember] private bool _configPropertiesHaveChanged;
        [DataMember] private bool _configNameOrEnabledHasChanged;

        [IgnoreDataMember]
        public bool QueuesAreBeingApplied => ListQueuesToAdd.Any();
        [IgnoreDataMember]
        public bool QueuesAreBeingRemoved => ListQueuesToRemove.Any();

        public List<int> ListQueuesToAdd => _listQueuesToAdd;
        public List<int> ListQueuesToRemove => _listQueuesToRemove;

        private readonly List<int> _listQueuesToRemain;

        private readonly SnapshotConfiguration _oldConfiguration;
        private readonly SnapshotConfiguration _newConfiguration;

        private readonly QueueConfigurationsDataTable _currentQueueConfiguration;

        private readonly bool _configIsBeingEnabledByThisChangeset;
        private readonly bool _configIsBeingDisabledByThisChangeset;
        private readonly bool _configRemainsEnabledAfterThisChangeset;

        public bool ConfigPropertiesHaveChanged => _configPropertiesHaveChanged;
        public bool ConfigNameOrEnabledHasChanged => _configNameOrEnabledHasChanged;

        private int _countConfiguredSnapshotRowsToDelete;
 

        [DataMember]
        public Dictionary<int, int> QueuesAndConfigurationsToAddSnapshotRows { get; set; } = new Dictionary<int, int>();
        [DataMember]
        public List<int> QueuesToDeleteSnapshotAndTrendData { get; set; } = new List<int>();

        public SnapshottingChangeset(SnapshotConfiguration oldConfiguration, SnapshotConfiguration newConfiguration,
            List<int> oldQueueIdentifiers, List<int> newQueueIdentifiers, QueueConfigurationsDataTable currentQueueConfiguration)
        {
            if (newQueueIdentifiers == null){newQueueIdentifiers = new List<int> { };}
            if (oldQueueIdentifiers == null) {oldQueueIdentifiers = new List<int> { }; }
            
            _listQueuesToAdd = newQueueIdentifiers.FindAll(q => !oldQueueIdentifiers.Contains(q));
            _listQueuesToRemove = oldQueueIdentifiers.FindAll(q => !newQueueIdentifiers.Contains(q));
            _listQueuesToRemain = oldQueueIdentifiers.FindAll(q => newQueueIdentifiers.Contains(q));

            _oldConfiguration = oldConfiguration;
            _newConfiguration = newConfiguration;

            if(_oldConfiguration != null)
            {
                _configIsBeingEnabledByThisChangeset = !_oldConfiguration.Enabled && _newConfiguration.Enabled;
                _configIsBeingDisabledByThisChangeset = _oldConfiguration.Enabled && !_newConfiguration.Enabled;
                _configRemainsEnabledAfterThisChangeset = _oldConfiguration.Enabled && _newConfiguration.Enabled;

                _configPropertiesHaveChanged = _oldConfiguration.Interval != _newConfiguration.Interval ||
                                               _oldConfiguration.Timezone.Id != _newConfiguration.Timezone.Id ||
                                               _oldConfiguration.StartTime != _newConfiguration.StartTime ||
                                               _oldConfiguration.EndTime != _newConfiguration.EndTime ||
                                               !_oldConfiguration.DaysOfTheWeek.IsEqualTo(_newConfiguration.DaysOfTheWeek);

                _configNameOrEnabledHasChanged = _oldConfiguration.Name != _newConfiguration.Name ||
                                                 _oldConfiguration.Enabled != _newConfiguration.Enabled;
            }
            
            _currentQueueConfiguration = currentQueueConfiguration;
            GetSnapshottingChanges();
        }

        private void GetSnapshottingChanges()
        {
            ProcessQueuesBeingRemovedByChanges();
            ProcessQueuesWhichAreRemainingAfterChanges();
            ProcessQueuesBeingAddedByChanges();
        }

        private void AddToCountOfDeletedSnapshotRows(int queueIdentifier)
        {
            _countConfiguredSnapshotRowsToDelete = _countConfiguredSnapshotRowsToDelete 
                                                   + _currentQueueConfiguration.GetConfiguredSnapshotRowsForQueue(queueIdentifier);

        }

        private void ProcessQueuesBeingAddedByChanges()
        {
            foreach (int queueIdentifier in ListQueuesToAdd)
            {
                if (_currentQueueConfiguration.QueueHasExistingConfiguration(queueIdentifier))
                {
                    if (_currentQueueConfiguration.ExistingConfigIsEnabled(queueIdentifier))
                    {
                        if (_newConfiguration.Enabled)
                        {
                            AddQueueForDeletionOfSnapshotAndTrendData(queueIdentifier);

                            AddQueueForInsertionOfSnapshotRows(queueIdentifier);
                        }
                        else
                        {
                            AddQueueForDeletionOfSnapshotAndTrendData(queueIdentifier);
                        }
                    }
                    else
                    {
                        if (_newConfiguration.Enabled)
                        {
                            AddQueueForInsertionOfSnapshotRows(queueIdentifier);
                        }
                    }
                }
                else
                {
                    if (_newConfiguration.Enabled)
                    {
                        AddQueueForInsertionOfSnapshotRows(queueIdentifier);
                    }
                }
            }
        }

        private void ProcessQueuesBeingRemovedByChanges()
        {
            if (_oldConfiguration != null && _oldConfiguration.Enabled)
            {
                foreach (int queueIdentifier in ListQueuesToRemove)
                {
                    AddQueueForDeletionOfSnapshotAndTrendData(queueIdentifier);
                }
            }
        }

        private void ProcessQueuesWhichAreRemainingAfterChanges()
        {
            if (_configIsBeingEnabledByThisChangeset)
            {
                foreach (int queueIdentifier in _listQueuesToRemain)
                {
                    AddQueueForInsertionOfSnapshotRows(queueIdentifier);
                }
            }
            if (_configIsBeingDisabledByThisChangeset)
            {
                foreach (int queueIdentifier in _listQueuesToRemain)
                {
                    AddQueueForDeletionOfSnapshotAndTrendData(queueIdentifier);
                }
            }
            if (_configRemainsEnabledAfterThisChangeset && ConfigPropertiesHaveChanged)
            {
                foreach (int queueIdentifier in _listQueuesToRemain)
                {
                    AddQueueForDeletionOfSnapshotAndTrendData(queueIdentifier);
                    AddQueueForInsertionOfSnapshotRows(queueIdentifier);
                }
            }
        }
        public void AddQueueForInsertionOfSnapshotRows(int queueIdentifier)
        {
            QueuesAndConfigurationsToAddSnapshotRows.Add(queueIdentifier, _newConfiguration.Id);

        }

        public void AddQueueForDeletionOfSnapshotAndTrendData(int queueIdentifier)
        {
            QueuesToDeleteSnapshotAndTrendData.Add(queueIdentifier);
            AddToCountOfDeletedSnapshotRows(queueIdentifier);
        }


        public bool WillExceedPermittedSnapshotLimit(int countRowsInTable)
        {
         var totalRowsAfterChangeset = countRowsInTable + NetConfiguredSnapshotRowChange();
            return totalRowsAfterChangeset > _configuredSnapshotRowLimit;

        }

        public int NetConfiguredSnapshotRowChange()
        {
            var added = QueuesAndConfigurationsToAddSnapshotRows.Count * _newConfiguration.ConfiguredSnapshotRowsPerQueue();
           
            return added - _countConfiguredSnapshotRowsToDelete;
        }


    }
}
