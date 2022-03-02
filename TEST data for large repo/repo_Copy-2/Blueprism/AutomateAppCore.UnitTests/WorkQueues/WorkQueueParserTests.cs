using BluePrism.AutomateAppCore;
using NUnit.Framework;
using System.Collections.Generic;

namespace AutomateAppCore.UnitTests.WorkQueues
{
    [TestFixture]
    public class WorkQueueParserTests
    {
        [TestCaseSource(nameof(GetIconOptions))]
        public void ToContentFilter_TestAllIconKeyFilters_ShouldBuildFilterWithoutException(string testIconKey)
        {
            var testQueueUIFilter = BuildTestQueueUIFilterFromIconKey(testIconKey);
            Assert.DoesNotThrow(() => testQueueUIFilter.ToContentFilter());
        }

        protected static IEnumerable<string> GetIconOptions()
        {
            yield return "Person";
            yield return "Tick";
            yield return "Padlock";
            yield return "Ellipsis";
            yield return "";
            yield return null;
        }

        private WorkQueueUIFilter BuildTestQueueUIFilterFromIconKey(string testIconKey)
        {
            return new WorkQueueUIFilter()
            {
                IconKey = testIconKey,
                ItemKeyFilter = "",
                PriorityFilter = "",
                StatusFilter = "",
                TagsFilter = "",
                ResourceFilter = "",
                AttemptFilter = "",
                CreatedFilter = "",
                LastUpdatedFilter = "",
                NextReviewFilter = "",
                CompletedFilter = "",
                TotalWorkTimeFilter = "",
                ExceptionDateFilter = "",
                ExceptionReasonFilter = ""
            };
        }
    }
}
