namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AutoFixture;
    using BpLibAdapters.Mappers;
    using CommonTestClasses;
    using Domain;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Server.Domain.Models.Pagination;
    using static Func.OptionHelper;

    using Some = Func.Some;

    [TestFixture(Category = "Unit Test")]
    public class WorkQueueItemsPageMapperTests
    {
        [Test]
        public void ToWorkQueueItemsPage_WithEmptyTestItems_ReturnsNonePaginationToken()
        {
            var items = new List<WorkQueueItemNoDataXml>();
            var parameters = new WorkQueueItemParameters { ItemsPerPage = 10 };

            var workQueueItemsPage = items.ToWorkQueueItemsPage(parameters);
            workQueueItemsPage.PagingToken.Should().BeAssignableTo<None<string>>();
        }

        [Test]
        [TestCaseSource(nameof(WorkQueueItemsPageTestCases))]
        public void ToWorkQueueItemsPage_TestItems_ReturnsCorrectPaginationToken(WorkQueueItemsPageTestCase testCase)
        {
            var (selectPropertyFunc, orderByFunc, workQueueItemSortByProperty) = testCase;

            var items = orderByFunc(GetWorkQueueItems()).ToList();

            var parameters = new WorkQueueItemParameters { ItemsPerPage = 10, SortBy = workQueueItemSortByProperty };

            var lastItem = orderByFunc(items).Last();
            var lastItemProperty = selectPropertyFunc(lastItem);

            var pagingToken = new PagingToken<long>
            {
                DataType = lastItemProperty.GetTypeName(),
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(lastItemProperty),
                PreviousIdValue = lastItem.Ident,
                ParametersHashCode = parameters.GetHashCodeForValidation(),
            };

            var workQueueItemsPage = items.ToWorkQueueItemsPage(parameters);

            ((Some<string>)workQueueItemsPage.PagingToken).Value.Should().Be(pagingToken.ToString());
        }

        [Test]
        public void ToWorkQueueItemsPage_ReturnsDifferentPaginationToken_WhenSortHasChanged()
        {
            var items = GetWorkQueueItems();
            var parameters = new WorkQueueItemParameters { ItemsPerPage = 10, SortBy = WorkQueueItemSortByProperty.DeferredAsc };

            var lastItem = items.OrderBy(x => x.DeferredDate).Last();

            var pagingToken = new PagingToken<long>
            {
                DataType = lastItem.DeferredDate.GetTypeName(),
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(lastItem.DeferredDate),
                PreviousIdValue = lastItem.Ident,
                ParametersHashCode = parameters.GetHashCodeForValidation(),
            };

            parameters.SortBy = WorkQueueItemSortByProperty.AttemptWorkTimeDesc;

            var workQueueItemsPage = items.ToWorkQueueItemsPage(parameters);

            ((Some<string>)workQueueItemsPage.PagingToken).Value.Should().NotBe(pagingToken.ToString());
        }

        private static PagingToken<string> DeserializePagingToken(Option<string> pagingToken) =>
            pagingToken is Some<string> p
                ? p.Value.Map(Convert.FromBase64String).Map(Encoding.UTF8.GetString).Map(JsonConvert.DeserializeObject<PagingToken<string>>)
                : throw new ArgumentException($"Parameter {nameof(pagingToken)} must have a value", nameof(pagingToken));

        [TestCase(WorkQueueSortByProperty.NameAsc)]
        [TestCase(WorkQueueSortByProperty.NameDesc)]
        public void Indexer_ShouldGetName_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = "Test Name";
            var model = new WorkQueue
            {
                Name = expectedResult,
            };

            var result = new[] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(expectedResult);
        }

        [TestCase(WorkQueueSortByProperty.RunningAsc)]
        [TestCase(WorkQueueSortByProperty.RunningDesc)]
        public void Indexer_ShouldGetRunning_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = QueueStatus.Running;
            var model = new WorkQueue
            {
                Status = expectedResult,
            };

            var result = new [] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(Convert.ToInt32(expectedResult).ToString());
        }

        [TestCase(WorkQueueSortByProperty.KeyFieldAsc)]
        [TestCase(WorkQueueSortByProperty.KeyFieldDesc)]
        public void Indexer_ShouldGetKeyField_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = "Key Field1";
            var model = new WorkQueue
            {
                KeyField = expectedResult,
            };

            var result = new [] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(expectedResult);
        }

        [TestCase(WorkQueueSortByProperty.MaxAttemptsAsc)]
        [TestCase(WorkQueueSortByProperty.MaxAttemptsDesc)]
        public void Indexer_ShouldGetMaxAttempt_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = 1;
            var model = new WorkQueue
            {
                MaxAttempts = expectedResult,
            };

            var result = new [] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(expectedResult.ToString());
        }

        [TestCase(WorkQueueSortByProperty.EncryptIdAsc)]
        [TestCase(WorkQueueSortByProperty.EncryptIdDesc)]
        public void Indexer_ShouldGetEncryptId_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = 1;
            var model = new WorkQueue
            {
                EncryptionKeyId = expectedResult,
            };

            var result = new [] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(expectedResult.ToString());
        }

        [TestCase(WorkQueueSortByProperty.TotalAsc)]
        [TestCase(WorkQueueSortByProperty.TotalDesc)]
        public void Indexer_ShouldGetTotal_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = 1;
            var model = new WorkQueue
            {
                TotalItemCount = expectedResult,
            };

            var result = new [] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(expectedResult.ToString());
        }

        [TestCase(WorkQueueSortByProperty.CompletedAsc)]
        [TestCase(WorkQueueSortByProperty.CompletedDesc)]
        public void Indexer_ShouldGetCompleted_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = 1;
            var model = new WorkQueue
            {
                CompletedItemCount = expectedResult,
            };

            var result = new [] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(expectedResult.ToString());
        }

        [TestCase(WorkQueueSortByProperty.PendingAsc)]
        [TestCase(WorkQueueSortByProperty.PendingDesc)]
        public void Indexer_ShouldGetPending_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = 1;
            var model = new WorkQueue
            {
                PendingItemCount = expectedResult,
            };

            var result = new [] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(expectedResult.ToString());
        }

        [TestCase(WorkQueueSortByProperty.ExceptionedAsc)]
        [TestCase(WorkQueueSortByProperty.ExceptionedDesc)]
        public void Indexer_ShouldGetExceptioned_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = 1;
            var model = new WorkQueue
            {
                ExceptionedItemCount = expectedResult,
            };

            var result = new [] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(expectedResult.ToString());
        }

        [TestCase(WorkQueueSortByProperty.TotalWorkTimeAsc)]
        [TestCase(WorkQueueSortByProperty.TotalWorkTimeDesc)]
        public void Indexer_ShouldGetTotalWorkTime_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = TimeSpan.FromHours(1);
            var model = new WorkQueue
            {
                TotalCaseDuration = expectedResult,
            };

            var result = new [] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(expectedResult.ToString());
        }

        [TestCase(WorkQueueSortByProperty.AverageWorkedTimeAsc)]
        [TestCase(WorkQueueSortByProperty.AverageWorkedTimeDesc)]
        public void Indexer_ShouldGetAverageTime_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = TimeSpan.FromHours(1);
            var model = new WorkQueue
            {
                AverageWorkTime = expectedResult,
            };

            var result = new [] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(expectedResult.ToString());
        }

        [TestCase(WorkQueueSortByProperty.LockedAsc)]
        [TestCase(WorkQueueSortByProperty.LockedDesc)]
        public void Indexer_ShouldGetLocked_WhenCalledWithSortBy(WorkQueueSortByProperty sortByProperty)
        {
            var expectedResult = 1;
            var model = new WorkQueue
            {
                LockedItemCount = expectedResult,
            };

            var result = new [] {model}.ToItemsPage(new WorkQueueParameters {SortBy = sortByProperty});

            DeserializePagingToken(result.PagingToken).PreviousSortColumnValue.Should().Be(expectedResult.ToString());
        }

        private static IEnumerable<WorkQueueItemsPageTestCase> WorkQueueItemsPageTestCases
        {
            get
            {
                Func<WorkQueueItemNoDataXml, Option<DateTimeOffset>> selectExceptionDateFunc = x => x.ExceptionedDate;
                yield return new WorkQueueItemsPageTestCase
                {
                    SelectPropertyFunc = selectExceptionDateFunc,
                    WorkQueueItemSortByProperty = WorkQueueItemSortByProperty.ExceptionAsc,
                    OrderByFunc = args => args.OrderBy(x => ((Some<DateTimeOffset>)selectExceptionDateFunc(x)).Value),
                };
                yield return new WorkQueueItemsPageTestCase
                {
                    SelectPropertyFunc = selectExceptionDateFunc,
                    WorkQueueItemSortByProperty = WorkQueueItemSortByProperty.ExceptionDesc,
                    OrderByFunc = args => args.OrderByDescending(x => ((Some<DateTimeOffset>)selectExceptionDateFunc(x)).Value),
                };

                Func<WorkQueueItemNoDataXml, object> selectAttemptWorkTimeInSecondsFunc = x => x.AttemptWorkTimeInSeconds;
                yield return new WorkQueueItemsPageTestCase
                {
                    SelectPropertyFunc = selectAttemptWorkTimeInSecondsFunc,
                    WorkQueueItemSortByProperty = WorkQueueItemSortByProperty.AttemptWorkTimeAsc,
                    OrderByFunc = args => args.OrderBy(x => selectAttemptWorkTimeInSecondsFunc),
                };
                yield return new WorkQueueItemsPageTestCase
                {
                    SelectPropertyFunc = selectAttemptWorkTimeInSecondsFunc,
                    WorkQueueItemSortByProperty = WorkQueueItemSortByProperty.AttemptWorkTimeDesc,
                    OrderByFunc = args => args.OrderByDescending(x => selectAttemptWorkTimeInSecondsFunc),
                };
                Func<WorkQueueItemNoDataXml, string> selectExceptionReasonFunc = x => x.ExceptionReason;
                yield return new WorkQueueItemsPageTestCase
                {
                    SelectPropertyFunc = selectExceptionReasonFunc,
                    WorkQueueItemSortByProperty = WorkQueueItemSortByProperty.ExceptionReasonAsc,
                    OrderByFunc = args => args.OrderBy(x => selectExceptionReasonFunc),
                };
                yield return new WorkQueueItemsPageTestCase
                {
                    SelectPropertyFunc = selectExceptionReasonFunc,
                    WorkQueueItemSortByProperty = WorkQueueItemSortByProperty.ExceptionReasonDesc,
                    OrderByFunc = args => args.OrderByDescending(x => selectExceptionReasonFunc),
                };
            }
        }

        private static IReadOnlyList<WorkQueueItemNoDataXml> GetWorkQueueItems() =>
            new Fixture()
                .Build<WorkQueueItemNoDataXml>()
                .With(x => x.LoadedDate, Some(new DateTimeOffset(new Fixture().Create<DateTime>())))
                .With(x => x.DeferredDate, Some(new DateTimeOffset(new Fixture().Create<DateTime>())))
                .With(x => x.LockedDate, Some(new DateTimeOffset(new Fixture().Create<DateTime>())))
                .With(x => x.CompletedDate, Some(new DateTimeOffset(new Fixture().Create<DateTime>())))
                .With(x => x.ExceptionedDate, Some(new DateTimeOffset(new Fixture().Create<DateTime>())))
                .With(x => x.LastUpdated, Some(new DateTimeOffset(new Fixture().Create<DateTime>())))
                .CreateMany(10)
                .ToList();
    }
}
