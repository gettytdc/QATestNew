using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.DataMonitor;
using BluePrism.Data.DataModels.WorkQueueAnalysis;
using BluePrism.WorkQueueAnalysis.Enums;
using NLog;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;

namespace BluePrism.WorkQueueAnalysis.Classes
{
    public class SnapshotManager : IDisposable
    {
        #region Member Variables

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IServer _server;

        private IClock _clock;
        private ICollection<QueueSnapshot> _snapshotsToProcess;
        private ICollection<WorkQueueSnapshotInformation> _queuesWithSnapshotMetadata;
        private IDataMonitor _monitor;
        private DateTime _dateSnapshotsLastCleared;

        #endregion

        #region Constructor

        public SnapshotManager(IServer server, IClock clock = null, IDataMonitor monitor = null)
        {
            _server = server;
            _monitor = monitor;
            _clock = clock;
            _dateSnapshotsLastCleared = DateTime.UtcNow;

            InitialiseMissingFields();
            SetupEvents();
        }

        #endregion

        #region Events

        private void SetupEvents()
        {
            _monitor.MonitoredDataUpdated += _monitor_MonitoredDataUpdated;
        }

        private void _monitor_MonitoredDataUpdated(object sender, MonitoredDataUpdateEventArgs e)
        {
            if (e.Name != DataNames.ConfiguredSnapshots) return;
            Log.Debug("Data monitor detected change in '{0}', updating cache with new values...", DataNames.ConfiguredSnapshots);

            try
            {
                var cache = MemoryCache.Default;
                var cacheContainsConfiguredSnapshots = cache.Contains(WorkQueueAnalysisConstants.ConfiguredSnapshotsCacheKey);
                var newConfiguredSnapshots = _server.GetQueueSnapshots();

                if (cacheContainsConfiguredSnapshots)
                {
                    var oldConfiguredSnapshots = GetObjectFromCache(WorkQueueAnalysisConstants.ConfiguredSnapshotsCacheKey,
                        WorkQueueAnalysisConstants.DefaultConfiguredSnapshotsCacheExpiryInMinutes,
                        _server.GetQueueSnapshots);

                    UpdateObjectInCache(WorkQueueAnalysisConstants.ConfiguredSnapshotsCacheKey,
                        WorkQueueAnalysisConstants.DefaultConfiguredSnapshotsCacheExpiryInMinutes,
                        newConfiguredSnapshots);

                    Log.Debug("Removed {0} configured snapshot(s) from cache and replaced with {1} configured snapshots from the server.",
                        oldConfiguredSnapshots.Count, newConfiguredSnapshots.Count);
                }
                else
                {
                    UpdateObjectInCache(WorkQueueAnalysisConstants.ConfiguredSnapshotsCacheKey,
                        WorkQueueAnalysisConstants.DefaultConfiguredSnapshotsCacheExpiryInMinutes,
                        newConfiguredSnapshots);
                    Log.Debug("Updated cache with {0} configured snapshot(s).", newConfiguredSnapshots.Count);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Any fields that haven't been provided 'Mock' instances for unit testing we can initialise them here instead.
        /// </summary>
        private void InitialiseMissingFields()
        {
            if (_clock == null)
                _clock = SystemClock.Instance;

            if (_monitor == null)
                _monitor = new TimerDataMonitor(new DatabaseMonitoredDataStore())
                {
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(WorkQueueAnalysisConstants.DefaultConfiguredSnapshotsPollIntervalInSeconds)
                };
        }

        private List<WorkQueueSnapshotInformation> GetQueuesWithMetadataThatRequireSnapshotting()
        {
            var result = new List<WorkQueueSnapshotInformation>();

            foreach (var queueWithSnapshotMetadata in _queuesWithSnapshotMetadata)
            {
                Log.Debug("Checking if queue '{0}' requires snapshotting...", queueWithSnapshotMetadata.QueueIdentifier);

                var snapshotsForQueue = _snapshotsToProcess
                    .Where(x => x.QueueIdentifier == queueWithSnapshotMetadata.QueueIdentifier)
                    .ToList();
                if (!snapshotsForQueue.Any())
                {
                    Log.Debug("Unable to find configured snapshots for queue {0}, skipping.", queueWithSnapshotMetadata.QueueIdentifier);
                    continue;
                }

                var typeOfRefreshRequired =
                    GetQueueSnapshotRequirements(snapshotsForQueue, queueWithSnapshotMetadata);

                switch (typeOfRefreshRequired)
                {
                    case QueueSnapshotRequirements.Unknown:
                        Log.Debug("Unable to determine state of queue {0}.",
                            queueWithSnapshotMetadata.QueueIdentifier);
                        continue;
                    case QueueSnapshotRequirements.InitialSnapshot:
                        queueWithSnapshotMetadata.SnapshotIdsToProcess =
                            GetInitialConfiguredSnapshotForQueue(queueWithSnapshotMetadata, snapshotsForQueue);
                        break;
                    case QueueSnapshotRequirements.FromLastSnapshot:
                        queueWithSnapshotMetadata.SnapshotIdsToProcess =
                            GetOutstandingSnapshotsForQueue(queueWithSnapshotMetadata, snapshotsForQueue);
                        break;
                    case QueueSnapshotRequirements.NoRefresh:
                        Log.Debug("Queue '{0}' is up to date.", queueWithSnapshotMetadata.QueueIdentifier);
                        continue;
                    case QueueSnapshotRequirements.CycleRestart:
                        queueWithSnapshotMetadata.SnapshotIdsToProcess =
                            GetFirstSnapshotOfWeekForQueue(queueWithSnapshotMetadata, snapshotsForQueue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(typeOfRefreshRequired));
                }

                if (queueWithSnapshotMetadata.SnapshotIdsToProcess != null && queueWithSnapshotMetadata.SnapshotIdsToProcess.Any())
                {
                    Log.Debug(
                        "Flagging queue {0} for snapshotting.", queueWithSnapshotMetadata.QueueIdentifier);
                    result.Add(queueWithSnapshotMetadata);
                }
                else
                {
                    Log.Debug("No outstanding configured snapshots found for queue '{0}', skipping.", 
                        queueWithSnapshotMetadata.QueueIdentifier);
                }
            }

            return result;
        }

        private List<WorkQueueSnapshotInformation.SnapshotTriggerInformation> GetFirstSnapshotOfWeekForQueue(
            WorkQueueSnapshotInformation queueWithSnapshotMetadata, IEnumerable<QueueSnapshot> allSnapshotsForQueue)
        {
            var firstSnapshotOfWeek = OrderSnapshots(allSnapshotsForQueue).FirstOrDefault();
            return GetTriggerIfNotAlreadyCreatedForSnapshot(queueWithSnapshotMetadata, firstSnapshotOfWeek);
        }

        private bool SnapshotCycleHasCompleted(WorkQueueSnapshotInformation queueWithSnapshotMetadata, 
            List<QueueSnapshot> allSnapshotsForQueue)
        {
            var finalSnapshotOfCycle = OrderSnapshots(allSnapshotsForQueue).LastOrDefault();
            var lastSnapshotProcessed = allSnapshotsForQueue
                .FirstOrDefault(x => x.SnapshotId == queueWithSnapshotMetadata.LastSnapshotId);

            return finalSnapshotOfCycle == lastSnapshotProcessed;
        }

        private static List<QueueSnapshot> OrderSnapshots(IEnumerable<QueueSnapshot> snapshots)
        {
            return snapshots
                .OrderBy(x => x.DayOfWeek)
                .ThenBy(y => y.TimeOfDay)
                .ToList();
        }

        private List<WorkQueueSnapshotInformation.SnapshotTriggerInformation> GetTriggerIfNotAlreadyCreatedForSnapshot(
            WorkQueueSnapshotInformation queueWithSnapshotMetadata, QueueSnapshot nextSnapshot)
        {
            var result = new List<WorkQueueSnapshotInformation.SnapshotTriggerInformation>();

            Log.Debug("Checking if trigger already exists for next snapshot...");
            if (!_server.TriggerExistsInDatabase(nextSnapshot.SnapshotId,
                nextSnapshot.QueueIdentifier))
            {
                Log.Debug("Trigger does not exist, flagging for creation.");
                var snapshotTriggerInformation = new WorkQueueSnapshotInformation.SnapshotTriggerInformation
                {
                    SnapshotId = nextSnapshot.SnapshotId,
                    SnapshotTimeOffset = GetOffsetDateTimeForSnapshot(queueWithSnapshotMetadata.Timezone,
                            nextSnapshot)
                        .ToDateTimeOffset(),
                    EventType = nextSnapshot.EventType
                };

                result.Add(snapshotTriggerInformation);
            }
            else
            {
                Log.Debug("Trigger already exists in database.");
            }

            return result;
        }

        private ICollection<QueueSnapshot> RetrieveAndValidateInformationForSnapshotting()
        {
            var emptySnapshotCollection = new Collection<QueueSnapshot>();

            var snapshotsToProcess = GetObjectFromCache(WorkQueueAnalysisConstants.ConfiguredSnapshotsCacheKey,
                WorkQueueAnalysisConstants.DefaultConfiguredSnapshotsCacheExpiryInMinutes, _server.GetQueueSnapshots);
            if (snapshotsToProcess == null || !snapshotsToProcess.Any())
            {
                Log.Debug("No configured snapshots found.");
                return emptySnapshotCollection;
            }
            Log.Debug("{0} configured snapshot(s) retrieved from the cache.", snapshotsToProcess.Count);

            _queuesWithSnapshotMetadata = _server.GetQueuesWithTimezoneAndSnapshotInformation();
            if (_queuesWithSnapshotMetadata == null || !_queuesWithSnapshotMetadata.Any())
            {
                Log.Debug("No work queues configured for snapshotting found.");
                return emptySnapshotCollection;
            }
            Log.Debug("{0} queue(s) with timezone and snapshot information retrieved from the server.",
                _queuesWithSnapshotMetadata.Count);

            return snapshotsToProcess;
        }

        private static void ValidateParameters(WorkQueueSnapshotInformation queueWithSnapshotMetadata,
            IEnumerable<QueueSnapshot> configuredSnapshotsForQueue)
        {
            if (queueWithSnapshotMetadata == null)
                throw new ArgumentNullException(nameof(queueWithSnapshotMetadata));
            if (configuredSnapshotsForQueue == null)
                throw new ArgumentNullException(nameof(configuredSnapshotsForQueue));
        }

        private List<WorkQueueSnapshotInformation.SnapshotTriggerInformation> ConvertToSnapshotTriggerInformation(
            TimeZoneInfo queueTimezone, IEnumerable<QueueSnapshot> outstandingSnapshots)
        {
            return outstandingSnapshots.Select(snapshot => new WorkQueueSnapshotInformation.SnapshotTriggerInformation
                {
                    SnapshotId = snapshot.SnapshotId,
                    SnapshotTimeOffset = GetOffsetDateTimeForSnapshot(queueTimezone, snapshot)
                        .ToDateTimeOffset(),
                    EventType = snapshot.EventType
                })
                .ToList();
        }

        private bool CycleRestartRequired(WorkQueueSnapshotInformation queueWithSnapshotMetadata,
            IEnumerable<QueueSnapshot> allSnapshotsForQueue)
        {
            var dayOfWeekInTimezone = GetCurrentDayOfWeekInTimezone(queueWithSnapshotMetadata.Timezone);
            var currentTimeInTimezone = GetCurrentTimeInTimezone(queueWithSnapshotMetadata.Timezone);
            var orderedSnapshots = OrderSnapshots(allSnapshotsForQueue);
            var finalSnapshot = orderedSnapshots.LastOrDefault();
            if (finalSnapshot == null) return false;

            return dayOfWeekInTimezone < finalSnapshot.DayOfWeek ||
                   dayOfWeekInTimezone == finalSnapshot.DayOfWeek
                   && currentTimeInTimezone.CompareTo(finalSnapshot.TimeOfDay) <= 0;
        }

        private static T GetObjectFromCache<T>(string cacheItemName, int cacheTimeInMinutes, Func<T> objectSettingFunction)
        {
            var cache = MemoryCache.Default;
            var cachedObject = (T)cache[cacheItemName];

            if (cachedObject == null)
            {
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheTimeInMinutes)
                };

                cachedObject = objectSettingFunction();
                cache.Set(cacheItemName, cachedObject, policy);
            }

            return cachedObject;
        }

        private static void UpdateObjectInCache<T>(string key, int cacheTimeInMinutes, T objectToUpdate)
        {
            ClearCacheEntry(key);

            var cache = MemoryCache.Default;
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheTimeInMinutes)
            };

            cache.Set(key, objectToUpdate, policy);
        }

        private static void ClearCacheEntry(string key)
        {
            var cache = MemoryCache.Default;

            if (cache.Contains(key))
            {
                cache.Remove(key, CacheEntryRemovedReason.Removed);
            }
        }

        #endregion

        #region Public Methods

        public int SnapshotWorkQueuesIfRequired()
        {
            Log.Debug("Checking if any work queues require snapshotting...");
            _snapshotsToProcess = RetrieveAndValidateInformationForSnapshotting();

            var configuredSnapshotsAreAvailable = _snapshotsToProcess.Any();
            var queuesWithSnapshotMetadataToProcess = new List<WorkQueueSnapshotInformation>();

            if (configuredSnapshotsAreAvailable)
                queuesWithSnapshotMetadataToProcess = GetQueuesWithMetadataThatRequireSnapshotting();

            var actionsRequired = GetSnapshotActionsRequired(queuesWithSnapshotMetadataToProcess.Count);
            if (actionsRequired.AnyActionsRequired)
            {
                ExecuteSnapshottingProcedure(queuesWithSnapshotMetadataToProcess, actionsRequired);
                return queuesWithSnapshotMetadataToProcess.Count;
            }

            Log.Debug("No actions required, end of processing.");
            return 0;
        }

        private SnapshotActionsRequired GetSnapshotActionsRequired(int numberOfQueuesFlaggedForSnapshotting)
        {
            var actionsRequired = new SnapshotActionsRequired();

            if (numberOfQueuesFlaggedForSnapshotting > 0)
            {
                Log.Debug("{0} queue(s) flagged for snapshotting.", numberOfQueuesFlaggedForSnapshotting);
                actionsRequired.CreateTriggersInDatabase = true;
                actionsRequired.ExecuteStoredProcedure = true;
            }
            else
            {
                if (_server.TriggersDueToBeProcessed())
                {
                    Log.Debug("Triggers exist in database that are due for processing.");
                    actionsRequired.ExecuteStoredProcedure = true;
                }
            }
            
            if (_dateSnapshotsLastCleared.DayOfYear != DateTime.UtcNow.DayOfYear)
            {
                Log.Debug("Orphaned snapshot rows are due to be cleared.");
                actionsRequired.ClearOrphanedSnapshots = true;
            }

            return actionsRequired;
        }

        public List<WorkQueueSnapshotInformation.SnapshotTriggerInformation> GetOutstandingSnapshotsForQueue(
            WorkQueueSnapshotInformation queueWithSnapshotMetadata, List<QueueSnapshot> allSnapshotsForQueue)
        {
            ValidateParameters(queueWithSnapshotMetadata, allSnapshotsForQueue);

            var snapshotsDueByNowInOrder = GetSnapshotsDueByNowInDayAndTimeOrder(
                queueWithSnapshotMetadata, allSnapshotsForQueue);
            var numberSnapshotsDueByNow = snapshotsDueByNowInOrder.Count;
            Log.Debug(
                "Found {0} configured snapshot(s) for queue {1} that are before the current date/time in queue timezone.",
                numberSnapshotsDueByNow, queueWithSnapshotMetadata.QueueIdentifier);

            var mostRecentScheduledSnapshot = snapshotsDueByNowInOrder.LastOrDefault();
            var indexOfMostRecentScheduledSnapshot = snapshotsDueByNowInOrder
                .FindIndex(x => x.SnapshotId == mostRecentScheduledSnapshot?.SnapshotId);
            var indexOfLastSnapshotProcessed = snapshotsDueByNowInOrder
                .FindIndex(x => x.SnapshotId == queueWithSnapshotMetadata.LastSnapshotId);

            if (indexOfLastSnapshotProcessed == -1 || indexOfMostRecentScheduledSnapshot == -1) return null;

            var numberOfSnapshotsOutstanding = indexOfMostRecentScheduledSnapshot - indexOfLastSnapshotProcessed;
            var outstandingSnapshots = snapshotsDueByNowInOrder
                .GetRange(indexOfLastSnapshotProcessed + 1, numberOfSnapshotsOutstanding)
                .ToList();

            LogTraceInformationForOutstandingSnapshots(queueWithSnapshotMetadata, mostRecentScheduledSnapshot, 
                indexOfLastSnapshotProcessed, indexOfMostRecentScheduledSnapshot, outstandingSnapshots);

            Log.Debug(
                "Determined {0} out of {1} snapshots are yet to be processed for queue {2}.",
                numberOfSnapshotsOutstanding,
                numberSnapshotsDueByNow,
                queueWithSnapshotMetadata.QueueIdentifier);

            return ConvertToSnapshotTriggerInformation(queueWithSnapshotMetadata.Timezone, outstandingSnapshots);
        }

        private static void LogTraceInformationForOutstandingSnapshots(WorkQueueSnapshotInformation queueWithSnapshotMetadata,
            QueueSnapshot mostRecentScheduledSnapshot, int indexOfLastSnapshotProcessed, int indexOfMostRecentScheduledSnapshot,
            IReadOnlyCollection<QueueSnapshot> outstandingSnapshots)
        {
            Log.Trace("Last snapshot id for queue {0} is: {1}", queueWithSnapshotMetadata.QueueIdentifier,
                queueWithSnapshotMetadata.LastSnapshotId);
            Log.Trace("Most Recent Snapshot for queue {0} is: {1}", queueWithSnapshotMetadata.QueueIdentifier,
                mostRecentScheduledSnapshot?.SnapshotId);
            Log.Trace("Index of last snapshot processed for queue {0} is: {1}",
                queueWithSnapshotMetadata.QueueIdentifier, indexOfLastSnapshotProcessed);
            Log.Trace("Index of most recent snapshot for queue {0} is: {1}", queueWithSnapshotMetadata.QueueIdentifier,
                indexOfMostRecentScheduledSnapshot);
            Log.Trace(
                "Number of outstanding configured snapshots retrieved from snapshots that are before the current date/time: {0}",
                outstandingSnapshots.Count);
        }

        public List<WorkQueueSnapshotInformation.SnapshotTriggerInformation> GetInitialConfiguredSnapshotForQueue(
            WorkQueueSnapshotInformation queueWithSnapshotMetadata, IEnumerable<QueueSnapshot> allSnapshotsForQueue)
        {
            var initialConfiguredSnapshotRequired =
                FindInitialConfiguredSnapshot(queueWithSnapshotMetadata, allSnapshotsForQueue);
            if (initialConfiguredSnapshotRequired == null) return null;
            
            Log.Debug("Determined the first configured snapshot due for queue {0} is '{1}' (Day:={2} : Time:={3})",
                queueWithSnapshotMetadata.QueueIdentifier,
                initialConfiguredSnapshotRequired.SnapshotId,
                initialConfiguredSnapshotRequired.DayOfWeek.ToString(),
                initialConfiguredSnapshotRequired.TimeOfDay.ToString("HH:mm", CultureInfo.InvariantCulture));

            return GetTriggerIfNotAlreadyCreatedForSnapshot(queueWithSnapshotMetadata, initialConfiguredSnapshotRequired);
        }

        public QueueSnapshot FindInitialConfiguredSnapshot(WorkQueueSnapshotInformation queueWithSnapshotMetadata,
            IEnumerable<QueueSnapshot> configuredSnapshotsForQueue)
        {
            ValidateParameters(queueWithSnapshotMetadata, configuredSnapshotsForQueue);

            var possibleOptions = new List<QueueSnapshot>();
            var dayOfWeekInTimezone = GetCurrentDayOfWeekInTimezone(queueWithSnapshotMetadata.Timezone);
            var currentTimeInTimezone = GetCurrentTimeInTimezone(queueWithSnapshotMetadata.Timezone);

            foreach (var configuredSnapshot in configuredSnapshotsForQueue)
            {
                if (configuredSnapshot.EventType != SnapshotTriggerEventType.Snapshot)
                    continue;

                if (configuredSnapshot.DayOfWeek > dayOfWeekInTimezone)
                {
                    possibleOptions.Add(configuredSnapshot);
                }
                else
                {
                    if (configuredSnapshot.DayOfWeek == dayOfWeekInTimezone &&
                        configuredSnapshot.TimeOfDay.CompareTo(currentTimeInTimezone) >= 0)
                    {
                        possibleOptions.Add(configuredSnapshot);
                    }
                }
            }
            return OrderSnapshots(possibleOptions).FirstOrDefault();
        }

        public List<QueueSnapshot> GetSnapshotsDueByNowInDayAndTimeOrder(WorkQueueSnapshotInformation queueWithSnapshotMetadata,
            IEnumerable<QueueSnapshot> allSnapshotsForQueue)
        {
            ValidateParameters(queueWithSnapshotMetadata, allSnapshotsForQueue);

            var dayOfWeekInTimezone = GetCurrentDayOfWeekInTimezone(queueWithSnapshotMetadata.Timezone);
            var currentTimeInTimezone = GetCurrentTimeInTimezone(queueWithSnapshotMetadata.Timezone);
            var result = new List<QueueSnapshot>();

            foreach (var snapshot in allSnapshotsForQueue)
            {
                if (snapshot.DayOfWeek < dayOfWeekInTimezone)
                {
                    result.Add(snapshot);
                }
                else
                {
                    if (snapshot.DayOfWeek == dayOfWeekInTimezone &&
                        snapshot.TimeOfDay.CompareTo(currentTimeInTimezone) <= 0)
                    {
                        result.Add(snapshot);
                    }
                }
            }

            return OrderSnapshots(result);
        }

        public void ExecuteSnapshottingProcedure(List<WorkQueueSnapshotInformation> queuesToSnapshot,
            SnapshotActionsRequired actionsRequired)
        {
            var lockToken = AcquireSnapshotLock();
            if (string.IsNullOrWhiteSpace(lockToken))
            {
                Log.Debug("Unable to obtain environment lock.");
                return;
            }

            Log.Debug("Lock token '{0}' acquired for '{1}", lockToken, WorkQueueAnalysisConstants.SnapshotLockName);

            try
            {
                ExecuteActions(queuesToSnapshot, actionsRequired);
            }
            catch (SqlException sqlException) when (sqlException.Number == 547) //Foreign Key violation.
            {
                Log.Debug(
                    "Foreign Key violation when attempting to insert snapshot triggers, it is likely that " +
                    "the cache was unable to update in time after a configuration was modified.");
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                throw;
            }
            finally
            {
                Log.Debug("Attempting to release lock '{0}'...", lockToken);
                _server.ReleaseEnvLock(WorkQueueAnalysisConstants.SnapshotLockName, lockToken, "Processing Complete.",
                    null, keepLock: true);
                Log.Debug("Lock released successfully.");
            }
        }

        private void ExecuteActions(List<WorkQueueSnapshotInformation> queuesToSnapshot, SnapshotActionsRequired actionsRequired)
        {
            if (actionsRequired.CreateTriggersInDatabase)
            {
                Log.Debug("Requesting the server to create triggers to snapshot queues.");
                var numberOfTriggers = _server.SetQueuesToBeSnapshotted(queuesToSnapshot);
                Log.Debug("Server has created {0} triggers in the database.", numberOfTriggers);
            }

            if (actionsRequired.ExecuteStoredProcedure)
            {
                Log.Debug("Executing usp_TriggerQueueSnapshot.");
                _server.StartQueueSnapshottingProcess();
            }

            if (actionsRequired.ClearOrphanedSnapshots)
            {
                Log.Debug("Clearing orphaned snapshot data.");
                _server.ClearOrphanedSnapshotData();
                _dateSnapshotsLastCleared = DateTime.UtcNow;
            }
        }

        public string AcquireSnapshotLock()
        {
            Log.Debug("Attempting to acquire '{0}' environment lock...", WorkQueueAnalysisConstants.SnapshotLockName);

            //This is required because C# won't allow us to pass 'NULL' to a 'ref' parameter, so this is a dummy replacement.
            string nullParameter = null;
            var expiryTimeInSeconds =
                _server.GetIntPref(WorkQueueAnalysisConstants.WorkQueueAnalysisEnvironmentLockExpiryKey);

            if (_server.IsEnvLockHeld(WorkQueueAnalysisConstants.SnapshotLockName, null, ref nullParameter))
            {
                Log.Debug("Lock is held by another Blue Prism server, checking if expired....");
                if (_server.HasEnvLockExpired(WorkQueueAnalysisConstants.SnapshotLockName, null, expiryTimeInSeconds))
                {
                    Log.Debug("Lock held by another Blue Prism server has expired, releasing lock...");
                    _server.ReleaseEnvLock(WorkQueueAnalysisConstants.SnapshotLockName, null, "Lock Expired.",
                        null, keepLock: true);
                    Log.Debug("Lock released.");
                }
                else
                {
                    Log.Debug("Lock has not expired, unable to obtain lock.", WorkQueueAnalysisConstants.SnapshotLockName);
                    return null;
                }
            }

            return _server.AcquireEnvLock(WorkQueueAnalysisConstants.SnapshotLockName, null,
                null, Environment.MachineName, expiryTimeInSeconds);
        }

        public QueueSnapshotRequirements GetQueueSnapshotRequirements(List<QueueSnapshot> allSnapshotsForQueue,
            WorkQueueSnapshotInformation queueWithSnapshotMetadata)
        {
            ValidateParameters(queueWithSnapshotMetadata, allSnapshotsForQueue);

            if (queueWithSnapshotMetadata.LastSnapshotId == -1)
            {
                Log.Debug("Queue {0} requires initial snapshot as no previous snapshot Id found.",
                    queueWithSnapshotMetadata.QueueIdentifier);
                return QueueSnapshotRequirements.InitialSnapshot;
            }

            if (SnapshotCycleHasCompleted(queueWithSnapshotMetadata, allSnapshotsForQueue))
            {
                Log.Debug("End of snapshot cycle detected for Queue {0}. Checking if restart required..",
                    queueWithSnapshotMetadata.QueueIdentifier);

                if (CycleRestartRequired(queueWithSnapshotMetadata, allSnapshotsForQueue))
                {
                    Log.Debug("Restart of cycle is required.");
                    return QueueSnapshotRequirements.CycleRestart;
                }

                Log.Debug("Restart of cycle is not required.");
                return QueueSnapshotRequirements.NoRefresh;
            }

            var snapshotsThatShouldHaveBeenDone = GetSnapshotsDueByNowInDayAndTimeOrder(queueWithSnapshotMetadata,
                allSnapshotsForQueue);
            if (!snapshotsThatShouldHaveBeenDone.Any())
                return QueueSnapshotRequirements.NoRefresh;

            var lastSnapshot = allSnapshotsForQueue
                .FirstOrDefault(x => x.SnapshotId == queueWithSnapshotMetadata.LastSnapshotId);
            return snapshotsThatShouldHaveBeenDone
                       .LastOrDefault()?.SnapshotId == lastSnapshot?.SnapshotId ?
                QueueSnapshotRequirements.NoRefresh :
                QueueSnapshotRequirements.FromLastSnapshot;
        }

        public LocalTime GetCurrentTimeInTimezone(TimeZoneInfo queueTimezone)
        {
            var timeZone = BclDateTimeZone.FromTimeZoneInfo(queueTimezone);
            var clock = _clock.InZone(timeZone);

            return clock.GetCurrentTimeOfDay();
        }

        public IsoDayOfWeek GetCurrentDayOfWeekInTimezone(TimeZoneInfo queueTimezone)
        {
            var timeZone = BclDateTimeZone.FromTimeZoneInfo(queueTimezone);
            var clock = _clock.InZone(timeZone);

            return clock.GetCurrentDate().DayOfWeek;
        }

        public OffsetDateTime GetOffsetDateTimeForSnapshot(TimeZoneInfo queueTimezone, QueueSnapshot snapshot)
        {
            var timeZone = BclDateTimeZone.FromTimeZoneInfo(queueTimezone);
            var offsetDateTime = _clock.InZone(timeZone).GetCurrentOffsetDateTime();

            var dayDifference = snapshot.DayOfWeek - offsetDateTime.DayOfWeek;
            var (year, month, day) = offsetDateTime.Date.PlusDays(dayDifference);
            var snapshotDateTime = new LocalDateTime(year, month, day,
                snapshot.TimeOfDay.Hour, snapshot.TimeOfDay.Minute);

            return new OffsetDateTime(snapshotDateTime, offsetDateTime.Offset);
        }

        #endregion

        #region Interface Implementations


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _monitor?.Dispose();
                    ClearCacheEntry(WorkQueueAnalysisConstants.ConfiguredSnapshotsCacheKey);
                }
            }
            _disposed = true;
        }

        ~SnapshotManager()
        {
            Dispose(false);
        }


        #endregion
    }
}