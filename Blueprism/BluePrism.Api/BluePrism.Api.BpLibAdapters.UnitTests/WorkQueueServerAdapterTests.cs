namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using BluePrism.Server.Domain.Models;
    using BluePrism.Api.BpLibAdapters.Mappers;
    using BluePrism.Api.BpLibAdapters.Mappers.FilterMappers;
    using Domain;
    using Domain.Errors;
    using Domain.Filters;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Server.Domain.Models.Pagination;
    using Utilities.Testing;

    using static CommonTestClasses.WorkQueuesHelper;

    using BpLibWorkQueueParameters = Server.Domain.Models.WorkQueueParameters;
    using PagingToken = Domain.PagingTokens.PagingToken<long>;
    using WorkQueueParameters = Domain.WorkQueueParameters;
    using WorkQueueItemParameters = Domain.WorkQueueItemParameters;
    using WorkQueueSortByProperty = Domain.WorkQueueSortByProperty;
    using WorkQueueItemSortByProperty = Domain.WorkQueueItemSortByProperty;

    [TestFixture]
    public class WorkQueueServerAdapterTests : UnitTestBase<WorkQueueServerAdapter>
    {
        public override void OneTimeSetup()
        {
            base.OneTimeSetup();

            FilterMapper.SetFilterMappers(new IFilterMapper[] { new NullFilterMapper(), });
        }

        [Test]
        public async Task WorkQueueGetQueues_OnSuccess_ReturnsQueues()
        {
            var expectedResult = new ItemsPage<WorkQueue>()
            {
                Items = new[]
                {
                    GetQueueWithName("def"),
                    GetQueueWithName("abc"),
                }
            };


            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(expectedResult.Items.Select(x => x.ToBluePrismWorkQueueWithGroup()).ToList());

            var workQueueParameters = new WorkQueueParameters
            {
                SortBy = WorkQueueSortByProperty.NameAsc,
                ItemsPerPage = 10,
                NameFilter = new NullFilter<string>(),
                AverageWorkTimeFilter = new NullFilter<int>(),
                MaxAttemptsFilter = new NullFilter<int>(),
                CompletedItemCountFilter = new NullFilter<int>(),
                ExceptionedItemCountFilter = new NullFilter<int>(),
                KeyFieldFilter = new NullFilter<string>(),
                LockedItemCountFilter = new NullFilter<int>(),
                PendingItemCountFilter = new NullFilter<int>(),
                QueueStatusFilter = new NullFilter<Domain.QueueStatus>(),
                TotalCaseDurationFilter = new NullFilter<int>(),
                TotalItemCountFilter = new NullFilter<int>()
            };

            var result = await ClassUnderTest.WorkQueueGetQueues(workQueueParameters);

            // ReSharper disable once CoVariantArrayConversion
            ((Success<ItemsPage<WorkQueue>>)result).Value.Items.Should().BeEquivalentTo(expectedResult.Items);
            ((Success<ItemsPage<WorkQueue>>)result).Value.PagingToken.Should().NotBeNull();
        }

        [Test]
        public async Task WorkQueueGetQueues_WhenNoPermission_ShouldThrowPermissionError()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Throws(new PermissionException());

            var workQueueParameters = new WorkQueueParameters
            {
                SortBy = WorkQueueSortByProperty.NameAsc,
                ItemsPerPage = 10,
                NameFilter = new NullFilter<string>(),
                AverageWorkTimeFilter = new NullFilter<int>(),
                MaxAttemptsFilter = new NullFilter<int>(),
                CompletedItemCountFilter = new NullFilter<int>(),
                ExceptionedItemCountFilter = new NullFilter<int>(),
                KeyFieldFilter = new NullFilter<string>(),
                LockedItemCountFilter = new NullFilter<int>(),
                PendingItemCountFilter = new NullFilter<int>(),
                QueueStatusFilter = new NullFilter<Domain.QueueStatus>(),
                TotalCaseDurationFilter = new NullFilter<int>(),
                TotalItemCountFilter = new NullFilter<int>()
            };

            var result = await ClassUnderTest.WorkQueueGetQueues(workQueueParameters);
            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task WorkQueueGetQueueById_WhenNoPermission_ShouldThrowPermissionError()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueue(It.IsAny<Guid>()))
                .Throws(new PermissionException());

            var result = await ClassUnderTest.WorkQueueGetQueueById(Guid.NewGuid());

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task WorkQueueGetQueueNames_OnSuccess_ReturnsQueueNames()
        {
            var testQueues = new[]
            {
                GetQueueWithName("def"),
                GetQueueWithName("abc"),
            };

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetAllQueues())
                .Returns(testQueues.Select(x => x.ToBluePrismObject()).ToList());

            var result = await ClassUnderTest.WorkQueueGetQueueNames();

            // ReSharper disable once CoVariantArrayConversion
            ((Success<IEnumerable<string>>)result).Value.Should().BeEquivalentTo(testQueues.Select(x => x.Name));
        }

        [Test]
        public async Task WorkQueueGetQueueNames_WhenNoPermission_ShouldThrowPermissionError()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetAllQueues())
                .Throws(new PermissionException());

            var result = await ClassUnderTest.WorkQueueGetQueueNames();

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task CreateWorkQueue_OnSuccess_ReturnsSuccess()
        {
            GetMock<IServer>()
                .Setup(m => m.CreateWorkQueue(It.IsAny<clsWorkQueue>(), It.IsAny<bool>()))
                .Returns((clsWorkQueue x, bool y) => x);

            var result = await ClassUnderTest.CreateWorkQueue(new WorkQueue());

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task CreateWorkQueue_WhenNoPermission_ShouldThrowPermissionError()
        {
            GetMock<IServer>()
                .Setup(m => m.CreateWorkQueue(It.IsAny<clsWorkQueue>(), It.IsAny<bool>()))
                .Throws(new PermissionException());

            var result = await ClassUnderTest.CreateWorkQueue(new WorkQueue());

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task DeleteWorkQueue_OnSuccess_ReturnsSuccess()
        {
            var result = await ClassUnderTest.DeleteWorkQueue(Guid.NewGuid());

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task DeleteWorkQueue_OnInvalidId_ReturnsQueueNotFoundError()
        {
            GetMock<IServer>()
                .Setup(m => m.DeleteWorkQueue(It.IsAny<Guid>()))
                .Throws(new NoSuchQueueException(Guid.NewGuid()));

            var result = await ClassUnderTest.DeleteWorkQueue(Guid.NewGuid());

            result.Should().BeAssignableTo<Failure<QueueNotFoundError>>();
        }

        [Test]
        public async Task DeleteWorkQueue_OnExistingSessions_ReturnsQueueStillContainsSessionsError()
        {
            GetMock<IServer>()
                .Setup(m => m.DeleteWorkQueue(It.IsAny<Guid>()))
                .Throws(new ForeignKeyDependencyException(""));

            var result = await ClassUnderTest.DeleteWorkQueue(Guid.NewGuid());

            result.Should().BeAssignableTo<Failure<QueueStillContainsSessionsError>>();
        }

        [Test]
        public async Task DeleteWorkQueue_OnQueueNotEmpty_ReturnsQueueNotEmptyError()
        {
            GetMock<IServer>()
                .Setup(m => m.DeleteWorkQueue(It.IsAny<Guid>()))
                .Throws(new QueueNotEmptyException("", CultureInfo.InvariantCulture));

            var result = await ClassUnderTest.DeleteWorkQueue(Guid.NewGuid());

            result.Should().BeAssignableTo<Failure<QueueNotEmptyError>>();
        }

        [Test]
        public async Task DeleteWorkQueue_WhenNoPermission_ShouldThrowPermissionError()
        {
            GetMock<IServer>()
                .Setup(m => m.DeleteWorkQueue(It.IsAny<Guid>()))
                .Throws(new PermissionException());

            var result = await ClassUnderTest.DeleteWorkQueue(new Guid());

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task WorkQueueGetItem_OnSuccess_ReturnsSuccess()
        {
            var testItem = GetTestBluePrismWorkQueueItem(Guid.NewGuid(), DateTime.UtcNow);

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetItem(testItem.ID))
                .Returns(testItem);

            var result = await ClassUnderTest.WorkQueueGetItem(testItem.ID);

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task WorkQueueGetItem_OnSuccess_ReturnsValue()
        {
            var itemId = Guid.NewGuid();
            var baseDate = DateTime.UtcNow;
            var testItem = GetTestBluePrismWorkQueueItem(itemId, baseDate);

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetItem(testItem.ID))
                .Returns(testItem);

            var result = await ClassUnderTest.WorkQueueGetItem(testItem.ID);

            var resultValue = ((Success<WorkQueueItem>)result).Value;
            ValidateModelsAreEqual(testItem, resultValue);
        }

        // This tests functionality written against incorrect behaviour within Blue Prism. It should be modified or removed if that functionality is fixed
        [Test]
        public async Task WorkQueueGetItem_OnInvalidCastException_ReturnsQueueNotFoundError()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetItem(It.IsAny<Guid>()))
                .Throws<InvalidCastException>();

            var result = await ClassUnderTest.WorkQueueGetItem(Guid.NewGuid());

            result.Should().BeAssignableTo<Failure<QueueItemNotFoundError>>();
        }

        [Test]
        public async Task WorkQueueGetItem_OnNoItem_ReturnsQueueItemNotFoundError()
        {
            var result = await ClassUnderTest.WorkQueueGetItem(Guid.NewGuid());

            result.Should().BeAssignableTo<Failure<QueueItemNotFoundError>>();
        }

        [Test]
        public async Task WorkQueueGetItem_WhenNoPermission_ShouldThrowPermissionError()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetItem(It.IsAny<Guid>()))
                .Throws(new PermissionException());

            var result = await ClassUnderTest.WorkQueueGetItem(Guid.NewGuid());
            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnSuccess_WhenSuccessful()
        {
            GetMock<IServer>()
                .Setup(x => x.WorkQueueGetQueue(It.IsAny<Guid>()))
                .Returns(new WorkQueueWithGroup());

            var result = await ClassUnderTest.UpdateWorkQueue(new WorkQueue());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnFailedResultWithInvalidArgumentsError_WhenWorkQueueNameIsEmpty()
        {
            GetMock<IServer>()
                .Setup(y => y.WorkQueueGetQueue(It.IsAny<Guid>()))
                .Returns(new WorkQueueWithGroup());

            GetMock<IServer>()
                .Setup(y => y.UpdateWorkQueueWithStatus(It.IsAny<clsWorkQueue>()))
                .Throws<ArgumentNullException>();

            var result = await ClassUnderTest.UpdateWorkQueue(new WorkQueue());

            (result is Failure<InvalidArgumentsError>).Should().BeTrue();
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnFailedResultWithNotOnlyOneQueueUpdatedError_WhenNotExactlyOneQueueUpdated()
        {
            GetMock<IServer>()
                .Setup(y => y.WorkQueueGetQueue(It.IsAny<Guid>()))
                .Returns(new WorkQueueWithGroup());

            GetMock<IServer>()
                .Setup(y => y.UpdateWorkQueueWithStatus(It.IsAny<clsWorkQueue>()))
                .Throws<BluePrismException>();

            var result = await ClassUnderTest.UpdateWorkQueue(new WorkQueue());

            (result is Failure<NotOnlyOneQueueUpdatedError>).Should().BeTrue();
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnFailedResultWithQueueAlreadyExistsError_WhenQueueAlreadyExists()
        {
            GetMock<IServer>()
                .Setup(y => y.WorkQueueGetQueue(It.IsAny<Guid>()))
                .Returns(new WorkQueueWithGroup());

            GetMock<IServer>()
                .Setup(y => y.UpdateWorkQueueWithStatus(It.IsAny<clsWorkQueue>()))
                .Throws<NameAlreadyExistsException>();

            var result = await ClassUnderTest.UpdateWorkQueue(new WorkQueue());

            (result is Failure<QueueAlreadyExistsError>).Should().BeTrue();
        }

        [Test]
        public async Task UpdateWorkQueue_WhenNoPermission_ShouldThrowPermissionError()
        {
            GetMock<IServer>()
                .Setup(m => m.UpdateWorkQueueWithStatus(It.IsAny<clsWorkQueue>()))
                .Throws(new PermissionException());

            var result = await ClassUnderTest.UpdateWorkQueue(new WorkQueue());
            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task WorkQueueGetItems_OnSuccess_ReturnsSuccess()
        {
            var workQueueId = Guid.NewGuid();
            var baseDate = DateTime.UtcNow;
            var testQueueItems = new[]
            {
                GetTestBluePrismWorkQueueItem(Guid.NewGuid(), baseDate),
                GetTestBluePrismWorkQueueItem(Guid.NewGuid(), baseDate),
            };

            var workQueueItemParameters = GetTestWorkQueueItemParameters();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(workQueueId, It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Returns(testQueueItems);

            var result = await ClassUnderTest.WorkQueueGetQueueItems(workQueueId, workQueueItemParameters);

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task WorkQueueGetItems_OnSuccessWithEmptyArray_ReturnsSuccess()
        {
            var workQueueId = Guid.NewGuid();
            var testQueueItems = Array.Empty<clsWorkQueueItem>();

            var workQueueItemParameters = GetTestWorkQueueItemParameters();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(workQueueId, It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Returns(testQueueItems);

            var result = await ClassUnderTest.WorkQueueGetQueueItems(workQueueId, workQueueItemParameters);

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task WorkQueueGetItems_OnSuccess_ReturnsExpectedItems()
        {
            var workQueueId = Guid.NewGuid();
            var baseDate = DateTime.UtcNow;

            var testQueueItems = new[]
            {
                GetTestBluePrismWorkQueueItem(Guid.NewGuid(), baseDate, 1, 2),
                GetTestBluePrismWorkQueueItem(Guid.NewGuid(), baseDate, 3, 4),
            };

            var workQueueItemParameters = GetTestWorkQueueItemParameters();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(workQueueId, It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Returns(testQueueItems);

            var result = await ClassUnderTest.WorkQueueGetQueueItems(workQueueId, workQueueItemParameters);
            var resultValue = ((Success<ItemsPage<WorkQueueItemNoDataXml>>)result).Value;

            ValidateModelsAreEqual(testQueueItems, resultValue.Items.ToArray());
        }

        [Test]
        public async Task WorkQueueGetItems_OnSuccess_ReturnsExpectedPagingToken()
        {
            var workQueueId = Guid.NewGuid();
            var baseDate = DateTime.UtcNow;

            var testQueueItems = Enumerable
                .Repeat(GetTestBluePrismWorkQueueItem(Guid.NewGuid(), baseDate, 1, 2), 11)
                .ToArray();

            var workQueueItemParameters = GetTestWorkQueueItemParameters();
            workQueueItemParameters.SortBy = WorkQueueItemSortByProperty.WorkTimeAsc;

            var testPagingToken = new PagingToken
            {
                PreviousIdValue = testQueueItems.OrderBy(x => x.Worktime).Last().Ident,
                DataType = testQueueItems.OrderBy(x => x.Worktime).Last().Worktime.GetType().Name,
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(testQueueItems.OrderBy(x => x.Worktime).Last().Worktime),
                ParametersHashCode = workQueueItemParameters.GetHashCodeForValidation(),
            }.ToString();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(workQueueId, It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Returns(testQueueItems);

            var result = await ClassUnderTest.WorkQueueGetQueueItems(workQueueId, workQueueItemParameters);
            var resultValue = ((Success<ItemsPage<WorkQueueItemNoDataXml>>)result).Value.PagingToken;

            ((Some<string>)resultValue).Value.Should().Be(testPagingToken);
        }

        [Test]
        public async Task WorkQueueGetItems_OnSuccessWhenNoMoreItemsLeftToReturn_ReturnsNonePagingToken()
        {
            var workQueueId = Guid.NewGuid();
            var baseDate = DateTime.UtcNow;

            var testQueueItems = new[]
            {
                GetTestBluePrismWorkQueueItem(Guid.NewGuid(), baseDate, 1, 2),
            };

            var workQueueItemParameters = GetTestWorkQueueItemParameters();
            workQueueItemParameters.SortBy = WorkQueueItemSortByProperty.WorkTimeAsc;
            workQueueItemParameters.ItemsPerPage = 10;

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(workQueueId, It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Returns(testQueueItems);

            var result = await ClassUnderTest.WorkQueueGetQueueItems(workQueueId, workQueueItemParameters);
            var resultValue = ((Success<ItemsPage<WorkQueueItemNoDataXml>>)result).Value.PagingToken;

            resultValue.Should().BeAssignableTo<None<string>>();
        }

        [Test]
        public async Task WorkQueueGetItems_OnInvalidQueueId_ReturnsQueueNotFoundError()
        {
            var workQueueItemParameters = GetTestWorkQueueItemParameters();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(It.IsAny<Guid>(), It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Throws(new NoSuchQueueException(Guid.NewGuid()));

            var result = await ClassUnderTest.WorkQueueGetQueueItems(Guid.NewGuid(), workQueueItemParameters);

            result.Should().BeAssignableTo<Failure<QueueNotFoundError>>();
        }

        [Test]
        public async Task WorkQueueGetQueueItems_WhenNoPermission_ShouldThrowPermissionError()
        {
            var workQueueItemsParameters = GetTestWorkQueueItemParameters();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(It.IsAny<Guid>(), It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Throws(new PermissionException());

            var result = await ClassUnderTest.WorkQueueGetQueueItems(Guid.NewGuid(), workQueueItemsParameters);
            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task CreateWorkQueueItems_OnSuccess_ReturnsSuccess()
        {
            var workQueueName = "queue1";
            var createdIds = new[]
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
            };
            var createWorkQueueItemsRequest = CreateWorkQueueItemsRequest();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueAddItemsAPI(workQueueName, It.Is<IEnumerable<CreateWorkQueueItemRequest>>(x => x.Count() == createWorkQueueItemsRequest.Count)))
                .Returns(createdIds);

            var result = await ClassUnderTest.WorkQueueAddItemsAPI(workQueueName, createWorkQueueItemsRequest);

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task CreateWorkQueueItems_OnSuccess_ReturnsCreatedIds()
        {
            var workQueueName = "queue1";
            var createdIds = new[]
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
            };
            var createWorkQueueItemsRequest = CreateWorkQueueItemsRequest();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueAddItemsAPI(workQueueName, It.Is<IEnumerable<CreateWorkQueueItemRequest>>(x => x.Count() == createWorkQueueItemsRequest.Count)))
                .Returns(createdIds);

            var result = await ClassUnderTest.WorkQueueAddItemsAPI(workQueueName, createWorkQueueItemsRequest);

            result.OnSuccess(x => x.Should().BeEquivalentTo(createdIds));
        }

        [Test]
        public async Task CreateWorkQueueItems_OnArgumentNullException_ReturnsInvalidArgumentsError()
        {
            var workQueueName = "queue1";
            var createWorkQueueItemsRequest = CreateWorkQueueItemsRequest();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueAddItemsAPI(It.IsAny<string>(), It.IsAny<IEnumerable<CreateWorkQueueItemRequest>>()))
                .Throws(new ArgumentNullException());

            var result = await ClassUnderTest.WorkQueueAddItemsAPI(workQueueName, createWorkQueueItemsRequest);

            result.Should().BeAssignableTo<Failure<InvalidArgumentsError>>();
        }

        [Test]
        public async Task CreateWorkQueueItems_OnInvalidOperationException_ReturnsInvalidArgumentsError()
        {
            var workQueueName = "queue1";
            var createWorkQueueItemsRequest = CreateWorkQueueItemsRequest();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueAddItemsAPI(It.IsAny<string>(), It.IsAny<IEnumerable<CreateWorkQueueItemRequest>>()))
                .Throws(new InvalidOperationException());

            var result = await ClassUnderTest.WorkQueueAddItemsAPI(workQueueName, createWorkQueueItemsRequest);

            result.Should().BeAssignableTo<Failure<InvalidArgumentsError>>();
        }

        [Test]
        public async Task CreateWorkQueueItems_OnNoSuchQueueException_ReturnsQueueNotFoundError()
        {
            var workQueueName = "queue1";
            var createWorkQueueItemsRequest = CreateWorkQueueItemsRequest();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueAddItemsAPI(It.IsAny<string>(), It.IsAny<IEnumerable<CreateWorkQueueItemRequest>>()))
                .Throws(new NoSuchQueueException(workQueueName, CultureInfo.CurrentCulture));

            var result = await ClassUnderTest.WorkQueueAddItemsAPI(workQueueName, createWorkQueueItemsRequest);

            result.Should().BeAssignableTo<Failure<QueueNotFoundError>>();
        }

        [Test]
        public async Task CreateWorkQueueItems_OnPermissionsException_ReturnsPermissionError()
        {
            var workQueueName = "queue1";
            var createWorkQueueItemsRequest = CreateWorkQueueItemsRequest();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueAddItemsAPI(It.IsAny<string>(), It.IsAny<IEnumerable<CreateWorkQueueItemRequest>>()))
                .Throws(new PermissionException());

            var result = await ClassUnderTest.WorkQueueAddItemsAPI(workQueueName, createWorkQueueItemsRequest);

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        private static List<CreateWorkQueueItem> CreateWorkQueueItemsRequest()
        {
            var createWorkQueueItemsRequest = new List<CreateWorkQueueItem>()
            {
                new CreateWorkQueueItem()
                {
                    Data = new DataCollection()
                    {
                        Rows = new List<IReadOnlyDictionary<string, DataValue>>()
                    }

                },
                new CreateWorkQueueItem()
                {
                    Data = new DataCollection()
                    {
                        Rows = new List<IReadOnlyDictionary<string, DataValue>>()
                    }
                }
            };
            return createWorkQueueItemsRequest;
        }

        private static WorkQueueItemParameters GetTestWorkQueueItemParameters() =>
            new WorkQueueItemParameters
            {
                ItemsPerPage = 10,
                SortBy = WorkQueueItemSortByProperty.LastUpdatedDesc,
                Status = new NullFilter<string>(),
                ExceptionReason = new NullFilter<string>(),
                KeyValue = new NullFilter<string>(),
                Attempt = new NullFilter<int>(),
                Priority = new NullFilter<int>(),
                WorkTime = new NullFilter<int>(),
                LoadedDate = new NullFilter<DateTimeOffset>(),
                DeferredDate = new NullFilter<DateTimeOffset>(),
                LockedDate = new NullFilter<DateTimeOffset>(),
                CompletedDate = new NullFilter<DateTimeOffset>(),
                LastUpdated = new NullFilter<DateTimeOffset>(),
                ExceptionedDate = new NullFilter<DateTimeOffset>(),
            };
    }
}
