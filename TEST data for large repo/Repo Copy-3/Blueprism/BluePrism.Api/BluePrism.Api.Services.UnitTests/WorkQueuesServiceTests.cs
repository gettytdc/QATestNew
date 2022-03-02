namespace BluePrism.Api.Services.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Autofac;
    using BpLibAdapters;
    using Domain;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;

    using static Func.ResultHelper;
    using static Func.OptionHelper;
    using static CommonTestClasses.WorkQueuesHelper;

    [TestFixture]
    public class WorkQueuesServiceTests : UnitTestBase<WorkQueuesService>
    {
        public override void Setup()
        {
            base.Setup(builder =>
            {
                builder.RegisterGeneric(typeof(MockAdapterAuthenticatedMethodRunner<>)).As(typeof(IAdapterAuthenticatedMethodRunner<>));
            });
        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnPredefinedValues_WhenSuccessful()
        {
            var workQueue = GetQueueWithName("test1");
            var workQueue2 = GetQueueWithName("test2");
            var workQueues = new List<WorkQueue> { workQueue, workQueue2 };
            var expectedPagingToken = "12345";

            var workQueueItemPage = new ItemsPage<WorkQueue>()
            {
                Items = workQueues,
                PagingToken = Some(expectedPagingToken)
            };

            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.WorkQueueGetQueues(It.IsAny<WorkQueueParameters>()))
                .ReturnsAsync(Succeed(workQueueItemPage));

            var result = await ClassUnderTest.GetWorkQueues(new WorkQueueParameters());

            result.Should().BeAssignableTo<Success>();
            result.OnSuccess(x => x.Items.Should().BeEquivalentTo(workQueues));
            result.OnSuccess(x => ((Some<string>) x.PagingToken).Value.Should().Be(expectedPagingToken));
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldCallUpdateWorkQueue_OnWorkQueueServerAdapter()
        {
            var id = Guid.NewGuid();

            GetMock<IWorkQueueServerAdapter>()
                .Tee(x => x
                    .Setup(m => m.WorkQueueGetQueues(It.IsAny<WorkQueueParameters>()))
                    .ReturnsAsync(ResultHelper<ItemsPage<WorkQueue>>.Fail(new QueueNotFoundError(""))))
                .Tee(x => x
                    .Setup(m => m.WorkQueueGetQueueById(id))
                    .ReturnsAsync(Succeed(new WorkQueue { Id = id })));

            await ClassUnderTest.UpdateWorkQueue(id, Succeed);

            GetMock<IWorkQueueServerAdapter>()
                .Verify(m => m.UpdateWorkQueue(It.Is<WorkQueue>(x => x.Id == id)), Times.Once);
        }

        [Test]
        public async Task GetWorkQueueNames_ShouldReturnFailedResult_WhenFailed()
        {
            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.WorkQueueGetQueues(It.IsAny<WorkQueueParameters>()))
                .ReturnsAsync(ResultHelper<ItemsPage<WorkQueue>>.Fail(new QueueNotFoundError("")));

            var result = await ClassUnderTest.GetWorkQueues(new WorkQueueParameters());

            result.Should().BeAssignableTo<Failure>();
        }

        [Test]
        public async Task CreateWorkQueue_ShouldReturnGuid_WhenSuccessful()
        {
            var workQueueId = Guid.NewGuid();

            var workQueue = new WorkQueue { Name = "test1", Id = workQueueId };

            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.CreateWorkQueue(It.IsAny<WorkQueue>()))
                .ReturnsAsync(Succeed(workQueue));

            GetMock<IWorkQueueServerAdapter>()
                .Setup(m => m.WorkQueueGetQueueNames())
                .ReturnsAsync(Succeed<IEnumerable<string>>(new string[0]));

            var result = await ClassUnderTest.CreateWorkQueue(new WorkQueue
            {
                Name = "testName",
                KeyField = "keyField",
                MaxAttempts = 3,
                Status = QueueStatus.Running,
                EncryptionKeyId = 0
            });

            result.Should().BeAssignableTo<Success>();
            ((Success<Guid>)result).Value.Should().Be(workQueueId);
        }

        [Test]
        public async Task CreateWorkQueue_ShouldReturnFailedResult_WhenFailed()
        {
            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.CreateWorkQueue(It.IsAny<WorkQueue>()))
                .ReturnsAsync(ResultHelper<WorkQueue>.Fail<TestError>());

            GetMock<IWorkQueueServerAdapter>()
                .Setup(m => m.WorkQueueGetQueueNames())
                .ReturnsAsync(Succeed<IEnumerable<string>>(new string[0]));

            var result = await ClassUnderTest.CreateWorkQueue(new WorkQueue
            {
                Name = "testName",
                KeyField = "keyField",
                MaxAttempts = 3,
                Status = QueueStatus.Running,
                EncryptionKeyId = 0
            });
            result.Should().BeAssignableTo<Failure>();
        }

        [Test]
        public async Task CreateWorkQueue_WhenQueueAlreadyExists_Fails()
        {
            GetMock<IWorkQueueServerAdapter>()
                .Setup(m => m.WorkQueueGetQueueNames())
                .ReturnsAsync(Succeed<IEnumerable<string>>(new[] { "Test" }));

            var result = await ClassUnderTest.CreateWorkQueue(new WorkQueue
            {
                Name = "Test",
                KeyField = "Test",
                MaxAttempts = 3,
                Status = QueueStatus.Paused,
                EncryptionKeyId = 0,
            });

            (result is Failure<QueueAlreadyExistsError>).Should().BeTrue();
        }

        [Test]
        public async Task DeleteWorkQueue_CallsDeleteWorkQueueOnAdapter()
        {
            var id = Guid.NewGuid();

            await ClassUnderTest.DeleteWorkQueue(id);

            GetMock<IWorkQueueServerAdapter>()
                .Verify(m => m.DeleteWorkQueue(id), Times.Once);
        }

        [Test]
        public async Task GetWorkQueueItem_OnSuccess_ReturnsItem()
        {
            var itemId = Guid.NewGuid();

            GetMock<IWorkQueueServerAdapter>()
                .Setup(m => m.WorkQueueGetItem(itemId))
                .ReturnsAsync(Succeed(new WorkQueueItem { Id = itemId }));

            var result = await ClassUnderTest.GetWorkQueueItem(itemId);

            ((Success<WorkQueueItem>)result).Value.Id.Should().Be(itemId);
        }

        [Test]
        public async Task GetWorkQueueItem_OnFailure_ReturnsFailure()
        {
            var itemId = Guid.NewGuid();

            GetMock<IWorkQueueServerAdapter>()
                .Setup(m => m.WorkQueueGetItem(itemId))
                .ReturnsAsync(ResultHelper<WorkQueueItem>.Fail<QueueItemNotFoundError>());

            var result = await ClassUnderTest.GetWorkQueueItem(itemId);

            result.Should().BeAssignableTo<Failure<QueueItemNotFoundError>>();
        }

        [Test]
        public async Task GetWorkQueueItems_ShouldReturnPredefinedValues_WhenSuccessful()
        {
            var workQueueId = Guid.NewGuid();
            var workQueueItem1 = new WorkQueueItemNoDataXml { CompletedDate = Some(new DateTimeOffset(DateTime.UtcNow)) };
            var workQueueItem2 = new WorkQueueItemNoDataXml { ExceptionedDate = Some(new DateTimeOffset(DateTime.UtcNow)) };
            var workQueueItem3 = new WorkQueueItemNoDataXml { LoadedDate = Some(new DateTimeOffset(DateTime.UtcNow)) };

            var workQueueItems = new List<WorkQueueItemNoDataXml> { workQueueItem1, workQueueItem2, workQueueItem3 };
            var pagingToken = Some("testPagingToken");

            var workQueueItemsPage = new ItemsPage<WorkQueueItemNoDataXml> { Items = workQueueItems, PagingToken = pagingToken };
            var workQueueItemParameters = new WorkQueueItemParameters();

            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.WorkQueueGetQueueItems(workQueueId, workQueueItemParameters))
                .ReturnsAsync(Succeed(workQueueItemsPage));

            var result = await ClassUnderTest.WorkQueueGetQueueItems(workQueueId, workQueueItemParameters);

            result.Should().BeAssignableTo<Success>();
            result.OnSuccess(x => x.Should().Be(workQueueItemsPage));
        }

        [Test]
        public async Task GetWorkQueueItems_OnFailure_WhenQueueIdInvalid()
        {

            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.WorkQueueGetQueueItems(It.IsAny<Guid>(), It.IsAny<WorkQueueItemParameters>()))
                .ReturnsAsync(ResultHelper<ItemsPage<WorkQueueItemNoDataXml>>.Fail(new QueueNotFoundError("")));

            var workQueueItemParameters = new WorkQueueItemParameters();

            var result = await ClassUnderTest.WorkQueueGetQueueItems(Guid.NewGuid(), workQueueItemParameters);

            result.Should().BeAssignableTo<Failure<QueueNotFoundError>>();
        }

        [Test]
        public async Task GetWorkQueue_OnSuccess_ShouldReturnItem()
        {
            var workQueue = new WorkQueue();
            var workQueueId = Guid.NewGuid();

            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.WorkQueueGetQueueById(It.Is<Guid>(y => y == workQueueId)))
                .ReturnsAsync(Succeed(workQueue));

            var result = await ClassUnderTest.GetWorkQueue(workQueueId);

            result.Should().BeAssignableTo<Success>();
            result.OnSuccess(x => x.Should().BeSameAs(workQueue));
        }

        [Test]
        public async Task GetWorkQueue_OnFailure_WhenQueueIdInvalid()
        {
            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.WorkQueueGetQueueById(It.IsAny<Guid>()))
                .ReturnsAsync(ResultHelper<WorkQueue>.Fail(new QueueNotFoundError("")));
    
            var result = await ClassUnderTest.GetWorkQueue(Guid.NewGuid());

            result.Should().BeAssignableTo<Failure<QueueNotFoundError>>();
        }

        [Test]
        public async Task CreateWorkQueueItems_OnSuccess_ShouldReturnCreatedIds()
        {
            var workQueueId = Guid.NewGuid();
            var workQueueName = "Queue1";

            var workQueue = new WorkQueue {Name = workQueueName};

            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.WorkQueueGetQueueById(workQueueId))
                .ReturnsAsync(Succeed(workQueue));

            var createWorkQueueItemsRequests = new List<CreateWorkQueueItem>()
            {
                new CreateWorkQueueItem(),
                new CreateWorkQueueItem()
            };

            IEnumerable<Guid> createdIds = new[]{Guid.NewGuid(), Guid.NewGuid() };
            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.WorkQueueAddItemsAPI(workQueueName, createWorkQueueItemsRequests))
                .ReturnsAsync(Succeed(createdIds));

                
            var result = await ClassUnderTest.CreateWorkQueueItems(workQueueId, createWorkQueueItemsRequests);

            result.Should().BeAssignableTo<Success>();
            result.OnSuccess(x => x.Should().BeEquivalentTo(createdIds));
        }

        [Test]
        public async Task CreateWorkQueueItems_WithInvalidQueueId_ShouldThrowException()
        {
            var workQueueId = Guid.NewGuid();

            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.WorkQueueGetQueueById(workQueueId))
                .ReturnsAsync(ResultHelper<WorkQueue>.Fail(new QueueNotFoundError("")));

            var createWorkQueueItemsRequests = new List<CreateWorkQueueItem>()
            {
                new CreateWorkQueueItem(),
                new CreateWorkQueueItem()
            };
            
            var result = await ClassUnderTest.CreateWorkQueueItems(workQueueId, createWorkQueueItemsRequests);

            result.Should().BeAssignableTo<Failure<QueueNotFoundError>>();
        }

        [Test]
        public async Task CreateWorkQueueItems_WithInvalidArguments_ShouldThrowException()
        {
            var workQueueId = Guid.NewGuid();
            var workQueueName = "Queue1";

            var workQueue = new WorkQueue { Name = workQueueName };

            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.WorkQueueGetQueueById(workQueueId))
                .ReturnsAsync(Succeed(workQueue));

            var createWorkQueueItemsRequests = new List<CreateWorkQueueItem>()
            {
                new CreateWorkQueueItem(),
                new CreateWorkQueueItem()
            };

            GetMock<IWorkQueueServerAdapter>()
                .Setup(x => x.WorkQueueAddItemsAPI(workQueueName, createWorkQueueItemsRequests))
                .ReturnsAsync(ResultHelper<IEnumerable<Guid>>.Fail(new InvalidArgumentsError("")));

            var result = await ClassUnderTest.CreateWorkQueueItems(workQueueId, createWorkQueueItemsRequests);

            result.Should().BeAssignableTo<Failure<InvalidArgumentsError>>();
        }

        private class TestError : ResultError { }
    }
}
