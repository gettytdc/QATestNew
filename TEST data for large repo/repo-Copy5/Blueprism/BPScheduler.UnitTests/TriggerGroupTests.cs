#if UNITTESTS

using BluePrism.Scheduling;
using BluePrism.Scheduling.Triggers;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BPScheduler.UnitTests
{
    [TestFixture]
    public class TriggerGroupTests
    {
        private TriggerGroup _triggerGroup;

        [SetUp]
        public void SetUpTriggerGroup()
        {
            _triggerGroup = new TriggerGroup();
        }

        #region BaseTrigger overrides
        [Test]
        public void TestPriorityGet()
        {
            _triggerGroup.Add(EveryNDaysTestFactory.Create(priority: 3));
            _triggerGroup.Add(EveryNDaysTestFactory.Create(priority: 4));
            _triggerGroup.Add(EveryNDaysTestFactory.Create(priority: 2));

            _triggerGroup.Priority.Should().Be(4);
        }

        [Test]
        public void TestPrioritySet()
        {
            Action action = () => _triggerGroup.Priority = 5;

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("The priority of a trigger group cannot be modified in this manner");
        }

        [Test]
        public void TestModeGet_Fire()
        {
            _triggerGroup.Add(EveryNDaysTestFactory.Create(triggerMode: TriggerMode.Fire));
            _triggerGroup.Add(EveryNDaysTestFactory.Create(triggerMode: TriggerMode.Fire));
            _triggerGroup.Add(EveryNDaysTestFactory.Create(triggerMode: TriggerMode.Fire));

            var mode = _triggerGroup.Mode;

            mode.Should().Be(TriggerMode.Fire);
        }

        [Test]
        public void TestModeGet_Suppress()
        {
            _triggerGroup.Add(EveryNDaysTestFactory.Create(triggerMode: TriggerMode.Suppress));
            _triggerGroup.Add(EveryNDaysTestFactory.Create(triggerMode: TriggerMode.Suppress));
            _triggerGroup.Add(EveryNDaysTestFactory.Create(triggerMode: TriggerMode.Suppress));

            var mode = _triggerGroup.Mode;

            mode.Should().Be(TriggerMode.Suppress);
        }

        [TestCase(TriggerMode.Fire, TriggerMode.Suppress)]
        [TestCase(TriggerMode.Fire, TriggerMode.Indeterminate)]
        [TestCase(TriggerMode.Suppress, TriggerMode.Indeterminate)]
        [TestCase(TriggerMode.Indeterminate, TriggerMode.Indeterminate)]
        [TestCase(TriggerMode.Indeterminate)]
        [TestCase()]
        public void TestModeGet_Indeterminate(params TriggerMode[] triggerModesToAdd)
        {
            foreach (var triggerMode in triggerModesToAdd)
                _triggerGroup.Add(EveryNDaysTestFactory.Create(triggerMode: triggerMode));

            var mode = _triggerGroup.Mode;

            mode.Should().Be(TriggerMode.Indeterminate);
        }

        [Test]
        public void TestModeSet()
        {
            Action action = () => _triggerGroup.Mode = TriggerMode.Fire;

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("The mode of a trigger group cannot be modified in this manner");
        }

        [Test]
        public void TestStartGet_WithTriggers()
        {
            var secondTrigger = EveryNDaysTestFactory.Create(addDays: 0);

            _triggerGroup.Add(EveryNDaysTestFactory.Create(addDays: 2));
            _triggerGroup.Add(secondTrigger);
            _triggerGroup.Add(EveryNDaysTestFactory.Create(addDays: 1));

            _triggerGroup.Start.Should().Be(secondTrigger.Start);
        }

        [Test]
        public void TestStartGet_NoTriggers()
        {
            var startTime = _triggerGroup.Start;

            startTime.Should().Be(DateTime.MaxValue);
        }

        [Test]
        public void TestStartSet()
        {
            Action action = () => _triggerGroup.Start = DateTime.Now;

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("The start date of a trigger group cannot be modified in this manner");
        }

        [Test]
        public void TestEndGet_WithTriggers()
        {
            var secondTrigger = EveryNDaysTestFactory.Create(addDays: 2);

            _triggerGroup.Add(EveryNDaysTestFactory.Create(addDays: 0));
            _triggerGroup.Add(secondTrigger);
            _triggerGroup.Add(EveryNDaysTestFactory.Create(addDays: 1));

            _triggerGroup.End.Should().Be(secondTrigger.End);
        }

        [Test]
        public void TestEndGet_NoTriggers()
        {
            var endTime = _triggerGroup.End;

            endTime.Should().Be(DateTime.MinValue);
        }

        [Test]
        public void TestEndSet()
        {
            Action action = () => _triggerGroup.End = DateTime.Now;

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("The end date of a trigger group cannot be modified in this manner");
        }

        [Test]
        public void TestScheduleGet()
        {
            _triggerGroup.Schedule.Should().BeNull();

            var schedule = new Mock<ISchedule>();
            _triggerGroup.Schedule = schedule.Object;

            _triggerGroup.Schedule.Should().Be(schedule.Object);
        }

        [Test]
        public void TestScheduleSet()
        {
            var firstTrigger = AddTriggerWithNoSchedule();
            var secondTrigger = AddTriggerWithNoSchedule();
            
            var schedule = new Mock<ISchedule>();
            _triggerGroup.Schedule = schedule.Object;

            firstTrigger.Schedule.Should().Be(schedule.Object);
            secondTrigger.Schedule.Should().Be(schedule.Object);
        }

        private EveryNDays AddTriggerWithNoSchedule()
        {
            var trigger = EveryNDaysTestFactory.Create();
            _triggerGroup.Add(trigger);

            trigger.Schedule.Should().BeNull();

            return trigger;
        }

        [Test]
        public void TestMetaDataGet()
        {
            _triggerGroup.MetaData.Should().HaveCount(0);

            var firstTrigger = EveryNDaysTestFactory.Create(addDays: 1);
            var secondTrigger = EveryNDaysTestFactory.Create(addDays: 2);

            _triggerGroup.Add(firstTrigger);
            _triggerGroup.Add(secondTrigger);

            _triggerGroup.MetaData.Should().HaveCount(2);
            var firstMatch = _triggerGroup.MetaData.SingleOrDefault(x => x.Equals(firstTrigger.PrimaryMetaData));
            firstMatch.Should().NotBeNull();
            var secondMatch = _triggerGroup.MetaData.SingleOrDefault(x => x.Equals(secondTrigger.PrimaryMetaData));
            secondMatch.Should().NotBeNull();
        }

        [Test]
        public void TestPrimaryMetaDataGet()
        {
            var userTrigger = EveryNDaysTestFactory.Create(userTrigger: true);

            _triggerGroup.UserTrigger = userTrigger;

            _triggerGroup.PrimaryMetaData.Should().Be(userTrigger.PrimaryMetaData);
        }

        [Test]
        public void TestPrimaryMetaDataGet_NullUserTrigger()
        {
            _triggerGroup.PrimaryMetaData.Should().BeNull();

            _triggerGroup.Add(EveryNDaysTestFactory.Create(userTrigger: false));

            _triggerGroup.PrimaryMetaData.Should().BeNull();
        }

        [Test]
        public void TestCopy()
        {
            var trigger = EveryNDaysTestFactory.Create();
            _triggerGroup.Add(trigger);

            var copiedTriggerGroup = _triggerGroup.Copy() as TriggerGroup;

            copiedTriggerGroup.PrimaryMetaData.Should().Be(_triggerGroup.PrimaryMetaData);
            copiedTriggerGroup.Members.Should().ContainSingle();
            copiedTriggerGroup.Should().NotContain(trigger);
            AssertEqual(copiedTriggerGroup.Members.Single(), _triggerGroup.Members.Single());
        }

        private void AssertEqual(object first, object second)
        {
            var firstJson = JsonConvert.SerializeObject(first);
            var secondJson = JsonConvert.SerializeObject(second);

            firstJson.Should().Be(secondJson);
        }

        [Test]
        public void TestCopy_StripsProperties()
        {
            _triggerGroup.Add(EveryNDaysTestFactory.Create());
            _triggerGroup.Schedule = new Mock<ISchedule>().Object;
            _triggerGroup.Group = new TriggerGroup();

            var copiedTriggerGroup = _triggerGroup.Copy();

            copiedTriggerGroup.Schedule.Should().BeNull();
            copiedTriggerGroup.Group.Should().BeNull();
        }


        #endregion

        #region ITrigger / ITriggerGroup implementations
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        public void TestUserTriggerGet_SingleUserTrigger(int systemTriggerCount)
        {
            var userTrigger = CreateAndAddUserTrigger();

            for (int j = 0; j < systemTriggerCount; j++)
                _triggerGroup.Add(EveryNDaysTestFactory.Create());

            _triggerGroup.UserTrigger.Should().Be(userTrigger);
        }


        private EveryNDays CreateAndAddUserTrigger()
        {
            var userTrigger = EveryNDaysTestFactory.Create();
            userTrigger.IsUserTrigger = true;

            _triggerGroup.Add(userTrigger);

            return userTrigger;
        }

        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(2, 0)]
        [TestCase(2, 1)]
        [TestCase(2, 10)]
        public void TestUserTriggerGet_VaryingUserTriggers(int userTriggerCount, int systemTriggerCount)
        {
            for (int i = 0; i < userTriggerCount; i++)
                CreateAndAddUserTrigger();

            for (int j = 0; j < systemTriggerCount; j++)
                _triggerGroup.Add(EveryNDaysTestFactory.Create());

            _triggerGroup.UserTrigger.Should().BeNull();
        }

        [Test]
        public void TestUserTriggerSet()
        {
            _triggerGroup.UserTrigger.Should().BeNull();

            var userTrigger = EveryNDaysTestFactory.Create(userTrigger: true);
            _triggerGroup.UserTrigger = userTrigger;

            _triggerGroup.UserTrigger.Should().Be(userTrigger);
        }

        [Test]
        public void TestUserTriggerSet_DuplicateTrigger()
        {
            _triggerGroup.Count.Should().Be(0);

            var firstUserTrigger = EveryNDaysTestFactory.Create(userTrigger: true);
            _triggerGroup.UserTrigger = firstUserTrigger;
            _triggerGroup.Should().HaveCount(1).And.Contain(firstUserTrigger);

            var secondUserTrigger = EveryNDaysTestFactory.Create(userTrigger: true);
            _triggerGroup.UserTrigger = secondUserTrigger;
            _triggerGroup.Should().HaveCount(1).And.Contain(secondUserTrigger);
        }

        [Test]
        public void TestUserTriggerSet_WithSystemTrigger()
        {
            var systemTrigger = EveryNDaysTestFactory.Create();

            Action action = () => _triggerGroup.UserTrigger = systemTrigger;

            action.ShouldThrow<ArgumentException>()
                .WithMessage($"Trigger {systemTrigger} is not a user trigger");
        }

        [Test]
        public void TestAddMetaData()
        {
            _triggerGroup.Count.Should().Be(0);

            var metaData = new TriggerMetaData { Priority = 83 };
            var trigger = _triggerGroup.Add(metaData);

            trigger.Should().NotBeNull();
            trigger.Priority.Should().Be(83);
            _triggerGroup.Should().HaveCount(1).And.Contain(trigger);
        }

        [Test]
        public void TestAddMetaData_Null()
        {
            var metaData = new TriggerMetaData();
            var trigger = _triggerGroup.Add(metaData);

            trigger.Should().NotBeNull();

            var trigger2 = _triggerGroup.Add(metaData);
            trigger2.Should().NotBeNull(
                because: "this method news up a trigger every time - null branch unreachable");
        }

        [Test]
        public void TestAdd()
        {
            var trigger = EveryNDaysTestFactory.Create();
            
            var result = _triggerGroup.Add(trigger);

            result.Should().BeTrue();
            _triggerGroup.Should().HaveCount(1).And.Contain(trigger);
        }

        [Test]
        public void TestAdd_ParentAsChild()
        {
            Action action = () => _triggerGroup.Add(_triggerGroup);

            action.ShouldThrow<CircularReferenceException>()
                .WithMessage($"{_triggerGroup} is an ancestor of this trigger. A trigger cannot be owned / parented by itself.");
        }

        [Test]
        public void TestAdd_FromAnotherGroup()
        {
            var trigger = EveryNDaysTestFactory.Create();
            new TriggerGroup().Add(trigger);

            Action action = () => _triggerGroup.Add(trigger);

            action.ShouldThrow<AlreadyAssignedException>()
                .WithMessage($"The provided trigger is already assigned to a different group.");
        }

        [Test]
        public void TestAdd_WithAnotherSchedule()
        {
            _triggerGroup.Schedule = new Mock<ISchedule>().Object;
            var trigger = EveryNDaysTestFactory.Create(mockSchedule: true);

            Action action = () => _triggerGroup.Add(trigger);

            action.ShouldThrow<AlreadyAssignedException>()
                .WithMessage("The provided trigger is already associated with a different schedule.");
        }

        [Test]
        public void TestRemove()
        {
            var trigger = new EveryNDays(1);
            _triggerGroup.Add(trigger);

            var result = _triggerGroup.Remove(trigger);

            _triggerGroup.Count.Should().Be(0);
            result.Should().BeTrue();
        }

        [Test]
        public void TestRemove_ElementNotPresent()
        {
            _triggerGroup.Count.Should().Be(0);

            var result = _triggerGroup.Remove(new EveryNSeconds(1));

            result.Should().BeFalse();
        }

        [Test]
        public void TestGetNextInstance_ByDate()
        {
            var expectedWinner = EveryNDaysTestFactory.Create(addDays: 1);

            var mockedSchedule = new Mock<ISchedule>();
            mockedSchedule.Setup(schedule => schedule.Owner).Returns(new Mock<IScheduler>().Object);
            mockedSchedule.Setup(schedule => schedule.Owner.Store).Returns(new Mock<IScheduleStore>().Object);
            mockedSchedule.Setup(schedule => schedule.Owner.Store.GetServerTimeZone()).Returns(TimeZoneInfo.Local);

            _triggerGroup.Schedule = mockedSchedule.Object;

            _triggerGroup.Add(EveryNDaysTestFactory.Create(addDays: 3));
            _triggerGroup.Add(expectedWinner);
            _triggerGroup.Add(EveryNDaysTestFactory.Create(addDays: 4));

            var result = _triggerGroup.GetNextInstance(EveryNDaysTestFactory.SnapshotTestTime);

            result.Trigger.Should().Be(expectedWinner);
        }

        [Test]
        public void TestGetNextInstance_ByPriority()
        {
            var expectedWinner = EveryNDaysTestFactory.Create(priority: 0);

            var mockedSchedule = new Mock<ISchedule>();
            mockedSchedule.Setup(schedule => schedule.Owner).Returns(new Mock<IScheduler>().Object);
            mockedSchedule.Setup(schedule => schedule.Owner.Store).Returns(new Mock<IScheduleStore>().Object);
            mockedSchedule.Setup(schedule => schedule.Owner.Store.GetServerTimeZone()).Returns(TimeZoneInfo.Local);

            _triggerGroup.Schedule = mockedSchedule.Object;

            _triggerGroup.Add(EveryNDaysTestFactory.Create(priority: 1));
            _triggerGroup.Add(expectedWinner);
            _triggerGroup.Add(EveryNDaysTestFactory.Create(priority: 5));

            var result = _triggerGroup.GetNextInstance(EveryNDaysTestFactory.SnapshotTestTime);

            result.Trigger.Should().Be(expectedWinner);
        }

        // Note ordering quirks with fire/indeterminate
        [TestCase(TriggerMode.Fire, TriggerMode.Indeterminate, TriggerMode.Indeterminate)]
        [TestCase(TriggerMode.Indeterminate, TriggerMode.Fire, TriggerMode.Fire)]
        [TestCase(TriggerMode.Fire, TriggerMode.Suppress, TriggerMode.Fire)]
        [TestCase(TriggerMode.Suppress, TriggerMode.Fire, TriggerMode.Fire)]
        [TestCase(TriggerMode.Suppress, TriggerMode.Indeterminate, TriggerMode.Indeterminate)]
        [TestCase(TriggerMode.Indeterminate, TriggerMode.Suppress, TriggerMode.Indeterminate)]
        public void TestGetNextInstance_ByTriggerMode(TriggerMode firstTrigger, TriggerMode secondTrigger, TriggerMode expectedWinner)
        {
            var mockedSchedule = new Mock<ISchedule>();
            mockedSchedule.Setup(schedule => schedule.Owner).Returns(new Mock<IScheduler>().Object);
            mockedSchedule.Setup(schedule => schedule.Owner.Store).Returns(new Mock<IScheduleStore>().Object);
            mockedSchedule.Setup(schedule => schedule.Owner.Store.GetServerTimeZone()).Returns(TimeZoneInfo.Local);

            _triggerGroup.Schedule = mockedSchedule.Object;

            _triggerGroup.Add(EveryNDaysTestFactory.Create(triggerMode: firstTrigger));
            _triggerGroup.Add(EveryNDaysTestFactory.Create(triggerMode: secondTrigger));

            var result = _triggerGroup.GetNextInstance(EveryNDaysTestFactory.SnapshotTestTime);

            result.Trigger.Mode.Should().Be(expectedWinner);
        }

        [Test]
        public void TestGetNextInstance_Null()
        {
            var now = DateTime.Now;

            var result = _triggerGroup.GetNextInstance(now);

            result.Should().BeNull();
        }

        [Test]
        public void TestClear()
        {
            var trigger = EveryNDaysTestFactory.Create();
            _triggerGroup.Add(trigger);
            trigger.Schedule = new Mock<ISchedule>().Object;

            _triggerGroup.Clear();

            _triggerGroup.Should().BeEmpty();
            trigger.Group.Should().BeNull();
            trigger.Schedule.Should().BeNull();
        }

        [Test]
        public void TestMembersGet()
        {
            var trigger = EveryNDaysTestFactory.Create();
            _triggerGroup.Add(trigger);

            var members = _triggerGroup.Members;

            members.Should().HaveCount(1).And.Contain(trigger);
        }

        [Test]
        public void TestGetInstances()
        {
            var triggerGroup = _triggerGroup;
            var trigger = EveryNDaysTestFactory.Create(addDays: 1, userTrigger: true);
            _triggerGroup.UserTrigger = trigger;
            triggerGroup.Add(trigger);

            var instances = triggerGroup.GetInstances(trigger.Start.AddSeconds(-1), trigger.Start.AddSeconds(+1));

            instances.Should().HaveCount(1);
            instances.First().Should().NotBeNull();
            instances.First().Mode.Should().Be(TriggerMode.Fire);
            instances.First().When.Should().Be(trigger.Start);
            instances.First().Trigger.Should().Be(trigger);
        }

        #endregion

        #region ICollection implementations
        [Test]
        public void TestContains_ElementPresent()
        {
            var trigger = EveryNDaysTestFactory.Create();
            _triggerGroup.Add(trigger);

            var result = _triggerGroup.Contains(trigger);

            result.Should().BeTrue();
        }

        [Test]
        public void TestContains_ElementMissing()
        {
            var trigger = EveryNDaysTestFactory.Create();

            var result = _triggerGroup.Contains(trigger);

            result.Should().BeFalse();
        }

        [TestCase(1, 0)]
        [TestCase(1, 1)]
        [TestCase(2, 0)]
        [TestCase(5, 5)]
        [TestCase(10, 3)]
        public void TestCopyTo(int triggerCount, int copyToIndex)
        {
            var array = AddTriggersAndInitialiseArray(triggerCount, copyToIndex);

            _triggerGroup.CopyTo(array, copyToIndex);

            array.Should().ContainInOrder(_triggerGroup);
            AssertElementsAtExpectedIndexes(array, copyToIndex);
        }

        private ITrigger[] AddTriggersAndInitialiseArray(int triggerCount, int copyToIndex)
        {
            for (int i = 0; i < triggerCount; i++)
                _triggerGroup.Add(EveryNDaysTestFactory.Create());

            var array = new ITrigger[triggerCount + copyToIndex];

            return array;
        }

        private static void AssertElementsAtExpectedIndexes(ITrigger[] array, int copyToIndex)
        {
            for (int j = 0; j < array.Length; j++)
            {
                if (j >= copyToIndex)
                    array[j].Should().NotBeNull();
                else
                    array[j].Should().BeNull();
            }
        }

        [Test]
        public void TestCopyTo_ArrayTooSmall()
        {
            _triggerGroup.Add(EveryNDaysTestFactory.Create());

            Action action = () => _triggerGroup.CopyTo(Array.Empty<ITrigger>(), 0);

            action.ShouldThrow<ArgumentException>()
                .WithMessage("Specified argument was out of the range of valid values.\r\n"
                            +"Parameter name: arrayIndex:0; array.Length:0; members.Count:1");
        }

        [Test]
        public void TestCopyTo_NullArray()
        {
            Action action = () => _triggerGroup.CopyTo(null, 0);

            action.ShouldThrow<ArgumentException>()
                .WithMessage("Value cannot be null.\r\nParameter name: array");
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(17)]
        public void TestCount(int numberOfTriggers)
        {
            for (int i = 0; i < numberOfTriggers; i++)
                _triggerGroup.Add(EveryNDaysTestFactory.Create());

            _triggerGroup.Count.Should().Be(numberOfTriggers);
        }

        [Test]
        public void TestIsReadOnly()
        {
            _triggerGroup.IsReadOnly.Should().BeFalse();
        }
        #endregion

        #region IEnumerable implementations
        [Test]
        public void TestGetEnumerator()
        {
            var trigger = EveryNDaysTestFactory.Create();
            _triggerGroup.Add(trigger);

            var enumerator = _triggerGroup.GetEnumerator();

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Should().Be(trigger);
            enumerator.MoveNext().Should().BeFalse();
        }
        #endregion
    }
}

#endif
