namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Autofac;
    using Models;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using BluePrism.Server.Domain.Models;
    using BpLibAdapters;
    using BpLibAdapters.Mappers;
    using CommonTestClasses;
    using CommonTestClasses.Extensions;
    using ControllerClients;
    using Domain;
    using FluentAssertions;
    using Func;
    using Mappers;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using Server.Domain.Models.Pagination;
    using static CommonTestClasses.WorkQueuesHelper;

    using BpLibWorkQueueParameters = Server.Domain.Models.WorkQueueParameters;
    using DataValueType = Models.DataValueType;
    using PagingToken = Domain.PagingTokens.PagingToken<long>;
    using QueueStatus = Models.QueueStatus;
    using WorkQueueParameters = Models.WorkQueueParameters;
    using WorkQueueSortByProperty = Server.Domain.Models.WorkQueueSortByProperty;

    [TestFixture]
    public class WorkQueuesControllerTests : ControllerTestBase<WorkQueuesControllerClient>
    {
        [SetUp]
        public override void Setup() =>
            Setup(() =>
            {
                GetMock<IServer>()
                    .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                    .Returns(new LoginResultWithReloginToken(LoginResultCode.Success));

                GetMock<IBluePrismServerFactory>()
                    .Setup(m => m.ClientInit())
                    .Returns(() => GetMock<IServer>().Object);

                RegisterMocks(builder =>
                {
                    builder.RegisterInstance(GetMock<IBluePrismServerFactory>().Object).As<IBluePrismServerFactory>();

                    return builder;
                });
            });

        [Test]
        public async Task GetWorkQueues_ShouldReturnQueuesAndPagingToken_WhenSuccessfulAndItemsPerPageIsLessThanWorkQueues()
        {
            var workQueues = new[]
            {
                new WorkQueue {Name = "aaa", KeyField = "", GroupName = ""},
                new WorkQueue {Name = "bbb", KeyField = "", GroupName = "test group"},
                new WorkQueue {Name = "ccc", KeyField = "", GroupName = "test test group"},
            };

            var testQueues = workQueues.Select(x => x.ToModel()).ToList();

            var workQueueWithGroup = workQueues.Select(x => x.ToBluePrismWorkQueueWithGroup()).ToList();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(workQueueWithGroup);

            var result = await Subject.GetWorkQueuesUsingQueryString("itemsPerPage=2&pagingToken=")
                                    .Map(x => x.Content.ReadAsAsync<ItemsPageModel<WorkQueueModel>>());

            result.Items.Should().BeEquivalentTo(testQueues);
            result.PagingToken.Should().NotBeNull();
        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnQueuesAndNullPagingToken_WhenSuccessfulAndItemsPerPageIsGreaterThanWorkQueues()
        {
            var workQueues = new[]
            {
                new WorkQueue {Name = "aaa", KeyField = "", GroupName = ""},
                new WorkQueue {Name = "bbb", KeyField = "", GroupName = "test group"},
                new WorkQueue {Name = "ccc", KeyField = "", GroupName = "test test group"},
            };

            var testQueues = workQueues.Select(x => x.ToModel()).ToList();

            var workQueuesWithGroup = workQueues.Select(x => x.ToBluePrismWorkQueueWithGroup()).ToList();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(workQueuesWithGroup);

            var result = await Subject.GetWorkQueuesUsingQueryString($"itemsPerPage={workQueues.Length + 1}&pagingToken=")
                .Map(x => x.Content.ReadAsAsync<ItemsPageModel<WorkQueueModel>>());

            result.Items.Should().BeEquivalentTo(testQueues);
            result.PagingToken.Should().BeNull();
        }

        [Test]
        public async Task GetWorkQueuesWithSortBy_ShouldReturnQueues_WhenSuccessful()
        {
            var workQueues = new[]
            {
                new WorkQueue {Name = "aaa", KeyField = "", GroupName = ""},
                new WorkQueue {Name = "bbb", KeyField = "", GroupName = "test group"},
                new WorkQueue {Name = "ccc", KeyField = "", GroupName = "test test group"},
            };

            var testQueues = workQueues.Select(x => x.ToModel()).ToList();

            var workQueuesWithGroup = workQueues.Select(x => x.ToBluePrismWorkQueueWithGroup()).ToList();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(workQueuesWithGroup);

            var result = await Subject.GetWorkQueuesSortBy("nameasc")
                .Map(x => x.Content.ReadAsAsync<ItemsPage<WorkQueueModel>>());

            result.Items.Should().BeEquivalentTo(testQueues);
        }

        [Test]
        public async Task GetWorkQueuesWithSortBy_ShouldReturnHttpStatusOk_WhenSuccessful()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(new List<WorkQueueWithGroup>());

            var result = await Subject.GetWorkQueuesSortBy("statusdesc");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        [TestCase("MaxAttemptsAsc")]
        [TestCase("maxAttemptsAsc")]
        [TestCase("Maxattemptsasc")]
        [TestCase("MAXATTEMPTSASC")]
        public async Task GetWorkQueues_OnVariousSortByCasings_ShouldSortByExpectedField(string sortBy)
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.Is<BpLibWorkQueueParameters>(p => p.SortBy == WorkQueueSortByProperty.MaxAttemptsAsc)))
                .Returns(new List<WorkQueueWithGroup>());

            var result = await Subject.GetWorkQueuesUsingQueryString($"sortBy={sortBy}");

            result.IsSuccessStatusCode.Should().BeTrue();

            GetMock<IServer>()
                .Verify(m => m.WorkQueueGetQueues(It.Is<BpLibWorkQueueParameters>(p => p.SortBy == WorkQueueSortByProperty.MaxAttemptsAsc)), Times.Once);

        }

        [Test]
        [TestCase("")]
        [TestCase("sortBy=")]
        public async Task GetWorkQueues_WithNoSortBy_ShouldUseNameAscForSorting(string sortBy)
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(
                    It.Is<BpLibWorkQueueParameters>(p => p.SortBy == WorkQueueSortByProperty.NameAsc)))
                .Returns(new List<WorkQueueWithGroup>());

            var result = await Subject.GetWorkQueuesUsingQueryString(sortBy);
            if (!result.IsSuccessStatusCode)
            {
                Assert.Fail($"Call to API failed with message: {await result.Content.ReadAsStringAsync()}");
            }

            GetMock<IServer>()
                .Verify(m => m.WorkQueueGetQueues(It.Is<BpLibWorkQueueParameters>(p => p.SortBy == WorkQueueSortByProperty.NameAsc)), Times.Once);
        }

        [Test]
        public async Task GetWorkQueuesWithSortBy_ShouldReturnHttpStatusOk_UsingBlankSortOrder_WhenSuccessful()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(new List<WorkQueueWithGroup>());

            var result = await Subject.GetWorkQueuesSortBy("");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnHttpStatusCodeBadRequest_WhenInvalidSortBy()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(new List<WorkQueueWithGroup>());

            var result = await Subject.GetWorkQueuesSortBy("anyoldcolumn");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnHttpStatusCodeBadRequest_WhenInvalidOrderBy()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(new List<WorkQueueWithGroup>());

            var result = await Subject.GetWorkQueuesSortBy("isrunning");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnHttpStatusCodeUnauthorized_WhenBluePrismLoginFailed()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.UnableToFindUser));


            var result = await Subject.GetWorkQueuesNoParameters();

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnHttpStatusOk_WhenSuccessful()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(new List<WorkQueueWithGroup>());

            var result = await Subject.GetWorkQueuesNoParameters();

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            GetMock<IServer>()
                .Setup(x => x.WorkQueueGetAllQueues())
                .Throws(new InvalidOperationException("ServerError"));

            var result = await Subject.GetWorkQueuesNoParameters();

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnOk_WhenCorrectFilterParameterSupplied()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(new List<WorkQueueWithGroup>());

            var result = await Subject.GetWorkQueuesUsingQueryString("queueStatus.eq=Paused");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnHttpStatusBadRequestWithStatusInvalidField_WhenInvalidStatusFilterParameterSupplied()
        {
            var result = await Subject.GetWorkQueuesUsingQueryString("status.eq=invalidFilter");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            (await result.Content.ReadAsStringAsync())
                .Map(JsonConvert.DeserializeObject<IList<ValidationErrorModel>>)
                .Single()
                .InvalidField
                .Should()
                .Be("Status.Eq");
        }

        [TestCaseSource(nameof(InvalidParameters))]
        public async Task GetWorkQueues_ShouldReturnHttpStatusBadRequestWithInvalidField_WhenInvalidFilterParameterSupplied(WorkQueueParameters workQueueParameters, string expectedInvalidFieldName)
        {
            var result = await Subject.GetWorkQueueWithFiltersParameters(workQueueParameters);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            (await result.Content.ReadAsStringAsync())
                .Map(JsonConvert.DeserializeObject<IList<ValidationErrorModel>>)
                .Single()
                .InvalidField
                .Should()
                .Be(expectedInvalidFieldName);
        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            GetMock<IServer>()
                .Setup(x => x.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetWorkQueuesNoParameters();

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnHttpStatusOkWithEmptyCollectionAndNullToken_WhenNoWorkQueuesExist()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(new List<WorkQueueWithGroup>());

            var result = await Subject.GetWorkQueuesNoParameters()
                .Map(x => x.Content.ReadAsAsync<ItemsPage<WorkQueueModel>>());

            result.Items.Count().Should().Be(0);
            result.PagingToken.Should().BeNull();

        }

        [Test]
        public async Task GetWorkQueues_ShouldReturnHttpBadRequest_WhenTokenPassedInDoesNotMatchParameters()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueues(It.IsAny<BpLibWorkQueueParameters>()))
                .Returns(new List<WorkQueueWithGroup>());

            var result = await Subject.GetWorkQueuesUsingQueryString("pagingToken=invalidPagingToken");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            (await result.Content.ReadAsStringAsync())
                .Map(JsonConvert.DeserializeObject<IList<ValidationErrorModel>>)
                .Single()
                .InvalidField
                .Should()
                .Be("PagingToken");

        }

        [TestCase("test", "test", 3, 1, QueueStatus.Paused)]
        [TestCase("test", "test", 2, 1, QueueStatus.Running)]
        public async Task CreateQueue_WhenModelValid_ReturnsCreatedResponse(string name, string keyField, int maxAttempts, int encryptionKeyId, QueueStatus status)
        {
            GetMock<IServer>()
                .Setup(m => m.CreateWorkQueue(It.IsAny<clsWorkQueue>(), It.IsAny<bool>()))
                .Returns(new clsWorkQueue { Id = Guid.NewGuid() });

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetAllQueues())
                .Returns(new List<clsWorkQueue>());

            var model = new CreateWorkQueueRequestModel
            {
                Name = name,
                KeyField = keyField,
                MaxAttempts = maxAttempts,
                EncryptionKeyId = encryptionKeyId,
                Status = status,
            };

            var result = await Subject.CreateQueue(model);

            result.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [TestCase(null, "test", 3, 1, QueueStatus.Paused)]
        [TestCase("", "test", 3, 1, QueueStatus.Paused)]
        [TestCase("test", null, 3, 1, QueueStatus.Paused)]
        [TestCase("test", "test", 0, 1, QueueStatus.Paused)]
        [TestCase("test", "test", 0, 1, QueueStatus.Paused)]
        public async Task CreateQueue_WhenModelIsInvalid_ReturnsBadRequestResponse(string name, string keyField, int maxAttempts, int encryptionKeyId, QueueStatus status)
        {
            var model = new CreateWorkQueueRequestModel
            {
                Name = name,
                KeyField = keyField,
                MaxAttempts = maxAttempts,
                EncryptionKeyId = encryptionKeyId,
                Status = status,
            };

            var result = await Subject.CreateQueue(model);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateQueue_WhenQueueAlreadyExists_ReturnsConflictResponse()
        {
            GetMock<IServer>()
                .Setup(m => m.CreateWorkQueue(It.IsAny<clsWorkQueue>(), It.IsAny<bool>()))
                .Returns(new clsWorkQueue { Id = Guid.NewGuid() });

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetAllQueues())
                .Returns(new List<clsWorkQueue> { new clsWorkQueue { Name = "Test" } });

            var model = new CreateWorkQueueRequestModel
            {
                Name = "Test",
                KeyField = "Test",
                MaxAttempts = 3,
                EncryptionKeyId = 0,
                Status = QueueStatus.Paused,
            };

            var result = await Subject.CreateQueue(model);

            result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateQueue_ShouldReturnWorkQueueId_WhenSuccessful()
        {
            var id = Guid.NewGuid();

            GetMock<IServer>()
                .Setup(m => m.CreateWorkQueue(It.IsAny<clsWorkQueue>(), It.IsAny<bool>()))
                .Returns(new clsWorkQueue { Id = id });

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetAllQueues())
                .Returns(new List<clsWorkQueue>());

            var model = new CreateWorkQueueRequestModel
            {
                Name = "name",
                KeyField = "keyField",
                MaxAttempts = 1,
                EncryptionKeyId = 0,
                Status = QueueStatus.Running,
            };

            var result = await Subject.CreateQueue(model)
                .Map(x => x.Content.ReadAsAsync<CreateWorkQueueResponseModel>());

            result.Id.Should().Be(id);
        }

        [Test]
        public async Task CreateWorkQueue_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            GetMock<IServer>()
                .Setup(x => x.CreateWorkQueue(It.IsAny<clsWorkQueue>(), It.IsAny<bool>()))
                .Throws(new PermissionException("Error message"));

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetAllQueues())
                .Returns(new List<clsWorkQueue> { new clsWorkQueue { Name = "testQueue" } });

            var model = new CreateWorkQueueRequestModel
            {
                Name = "Test",
                KeyField = "Test",
                MaxAttempts = 3,
                EncryptionKeyId = 0,
                Status = QueueStatus.Paused,
            };

            var result = await Subject.CreateQueue(model);
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task DeleteQueue_WhenQueueIdValid_ReturnsNoContentResponse()
        {
            var queueId = Guid.NewGuid();

            var result = await Subject.DeleteQueue(queueId);

            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task DeleteQueue_WhenQueueIdInvalid_ReturnsNotFoundResponse()
        {
            GetMock<IServer>()
                .Setup(m => m.DeleteWorkQueue(It.IsAny<Guid>()))
                .Throws(new NoSuchQueueException(Guid.Empty));

            var result = await Subject.DeleteQueue(Guid.NewGuid());

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeleteQueue_WhenQueueNotEmpty_ReturnsConflictResponse()
        {
            GetMock<IServer>()
                .Setup(m => m.DeleteWorkQueue(It.IsAny<Guid>()))
                .Throws(new QueueNotEmptyException("Test Queue", CultureInfo.InvariantCulture));

            var result = await Subject.DeleteQueue(Guid.NewGuid());

            result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task DeleteQueue_WhenQueueHasSessions_ReturnsConflictResponse()
        {
            GetMock<IServer>()
                .Setup(m => m.DeleteWorkQueue(It.IsAny<Guid>()))
                .Throws(new ForeignKeyDependencyException("Test"));

            var result = await Subject.DeleteQueue(Guid.NewGuid());

            result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task DeleteQueue_OnUnexpectedException_ReturnsInternalServerErrorResponse()
        {
            GetMock<IServer>()
                .Setup(m => m.DeleteWorkQueue(It.IsAny<Guid>()))
                .Throws<Exception>();

            var result = await Subject.DeleteQueue(Guid.NewGuid());

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task DeleteQueue_CachesSessionBetweenCalls()
        {
            await Subject.DeleteQueue(Guid.NewGuid());
            await Subject.DeleteQueue(Guid.NewGuid());

            GetMock<IServer>()
                .Verify(
                    m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<ReloginTokenRequest>()), Times.Once);

        }

        [Test]
        public async Task DeleteWorkQueue_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            GetMock<IServer>()
                .Setup(x => x.DeleteWorkQueue(It.IsAny<Guid>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.DeleteQueue(Guid.NewGuid());

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnHttpStatus204NoContent_WhenSuccessful()
        {
            var workQueueWithGroup = new WorkQueueWithGroup { Name = "name", KeyField = "keyField", Id = Guid.NewGuid() };

            GetMock<IServer>()
                .Tee(t => t
                    .Setup(x => x.WorkQueueGetQueue(It.IsAny<Guid>()))
                    .Returns(workQueueWithGroup))
                .Tee(t => t
                    .Setup(x => x.UpdateWorkQueueWithStatus(It.IsAny<clsWorkQueue>()))
                    .Verifiable());

            var result = await Subject.UpdateWorkQueue(Guid.NewGuid(), CreatePatchOperationContent());

            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnHttpStatusBadRequestWithNoOperationsMessage_WhenNoPatchOperationsSent()
        {
            var jArray = new JArray();

            var result = await Subject.UpdateWorkQueue(Guid.NewGuid(), jArray);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            (await result.Content.ReadAsStringAsync())
                .Map(JsonConvert.DeserializeObject<PatchOperationErrorModel>)
                .Reason
                .Should().Be(PatchOperationErrorReason.MissingOperation);
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnHttpStatusBadRequestWithPatchOperationErrorMessage_WhenInvalidPatchOperationSent()
        {
            var result = await Subject.UpdateWorkQueue(Guid.NewGuid(), CreatePatchOperationContent(path: "/invalidProperty"));

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            (await result.Content.ReadAsStringAsync())
                .Map(JsonConvert.DeserializeObject<PatchOperationErrorModel>)
                .Reason
                .Should().Be(PatchOperationErrorReason.InvalidOperation);
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnHttpStatusNotFound_WhenWorkQueueIdDoesNotExist()
        {
            GetMock<IServer>()
                .Setup(x => x.WorkQueueGetQueue(It.IsAny<Guid>()))
                .Returns<clsWorkQueue>(null);

            var result = await Subject.UpdateWorkQueue(Guid.NewGuid(), CreatePatchOperationContent());

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnHttpStatusInternalServerError_WhenUpdateWorkQueueFails()
        {
            var workQueueWithGroup = new WorkQueueWithGroup { Name = "name", KeyField = "keyField", Id = Guid.NewGuid() };

            GetMock<IServer>()
                .Tee(t => t
                    .Setup(x => x.WorkQueueGetQueue(It.IsAny<Guid>()))
                    .Returns(workQueueWithGroup))
                .Tee(t => t
                    .Setup(x => x.UpdateWorkQueueWithStatus(It.IsAny<clsWorkQueue>()))
                    .Throws<Exception>());

            var result = await Subject.UpdateWorkQueue(Guid.NewGuid(), CreatePatchOperationContent());

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnHttpStatusBadRequest_WhenQueueNameIsEmpty()
        {
            var workQueueWithGroup = new WorkQueueWithGroup { Name = "name", KeyField = "keyField", Id = Guid.NewGuid() };

            GetMock<IServer>()
                .Tee(t => t
                    .Setup(x => x.WorkQueueGetQueue(It.IsAny<Guid>()))
                    .Returns(workQueueWithGroup))
                .Tee(t => t
                    .Setup(x => x.UpdateWorkQueueWithStatus(It.IsAny<clsWorkQueue>()))
                    .Throws<ArgumentNullException>());

            var result = await Subject.UpdateWorkQueue(Guid.NewGuid(), CreatePatchOperationContent());

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnHttpStatusConflict_WhenNotExactlyOneQueueUpdated()
        {
            var workQueueWithGroup = new WorkQueueWithGroup { Name = "name", KeyField = "keyField", Id = Guid.NewGuid() };

            GetMock<IServer>()
                .Tee(t => t
                    .Setup(x => x.WorkQueueGetQueue(It.IsAny<Guid>()))
                    .Returns(workQueueWithGroup))
                .Tee(t => t
                    .Setup(x => x.UpdateWorkQueueWithStatus(It.IsAny<clsWorkQueue>()))
                    .Throws<BluePrismException>());

            var result = await Subject.UpdateWorkQueue(Guid.NewGuid(), CreatePatchOperationContent());

            result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnHttpStatusConflict_WhenQueueNameAlreadyExists()
        {
            var workQueueWithGroup = new WorkQueueWithGroup { Name = "name", KeyField = "keyField", Id = Guid.NewGuid() };

            GetMock<IServer>()
                .Tee(t => t
                    .Setup(x => x.WorkQueueGetQueue(It.IsAny<Guid>()))
                    .Returns(workQueueWithGroup))
                .Tee(t => t
                    .Setup(x => x.UpdateWorkQueueWithStatus(It.IsAny<clsWorkQueue>()))
                    .Throws<NameAlreadyExistsException>());

            var result = await Subject.UpdateWorkQueue(Guid.NewGuid(), CreatePatchOperationContent());

            result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [TestCase("replace", "/name", "", Description = "Invalid when operation value for 'Name' is empty")]
        [TestCase("replace", "/maxAttempts", "-1", Description = "Invalid when operation value for 'MaxAttempts' is less than zero")]
        [TestCase("replace", "/keyField", "", Description = "Invalid when operation value for 'KeyField' is empty")]
        public async Task UpdateWorkQueue_ShouldReturnHttpStatusBadRequest_WhenValuesUsedToUpdateIsInvalid(string op, string path, string value)
        {
            var result = await Subject.UpdateWorkQueue(Guid.NewGuid(), CreatePatchOperationContent(op, path, value));

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdateWorkQueue_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            GetMock<IServer>()
                .Setup(x => x.WorkQueueGetQueue(It.IsAny<Guid>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.UpdateWorkQueue(Guid.NewGuid(), CreatePatchOperationContent());
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private static JArray CreatePatchOperationContent(string op = "replace", string path = "/name", string value = "testName") =>
            new JArray
            {
                new JObject {["op"] = op, ["path"] = path, ["value"] = value}
            };

        [Test]
        public async Task GetWorkQueueItem_OnSuccess_ReturnsOkResponse()
        {
            var itemId = Guid.NewGuid();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetItem(itemId))
                .Returns(GetTestBluePrismWorkQueueItem(itemId, DateTime.UtcNow));

            var result = await Subject.GetWorkQueueItem(itemId);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetWorkQueueItem_OnNotFound_ReturnsQueueItemNotFoundResponse()
        {
            var result = await Subject.GetWorkQueueItem(Guid.NewGuid());

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetWorkQueueItem_OnUnexpectedException_ReturnsInternalServerErrorResponse()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetItem(It.IsAny<Guid>()))
                .Throws<Exception>();

            var result = await Subject.GetWorkQueueItem(Guid.NewGuid());

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task WorkQueueGetItem_WhenNoPermission_ShouldThrowPermissionError()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetItem(It.IsAny<Guid>()))
                .Throws(new PermissionException());

            var result = await Subject.GetWorkQueueItem(Guid.NewGuid());
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task WorkQueueGetItems_ShouldReturnHttpStatusOk_WhenSuccessful()
        {
            var workQueueId = Guid.NewGuid();
            var queuesItems = new[]
            {
                GetTestBluePrismWorkQueueItem(workQueueId, DateTime.UtcNow)
            };

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(workQueueId, It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Returns(queuesItems);

            var result = await Subject.WorkQueueGetQueueItems(workQueueId, WorkQueueItemSortBy.AttemptAsc);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task WorkQueueGetItems_ShouldReturnHttpStatusOkWithEmptyCollectionAndNullToken_WhenQueueHasNoItems()
        {
            var workQueueId = Guid.NewGuid();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(workQueueId, It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Returns(Array.Empty<clsWorkQueueItem>());

            var result = await Subject.WorkQueueGetQueueItems(workQueueId, WorkQueueItemSortBy.AttemptAsc);
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var workQueueItemsPage = await result.Map(x => x.Content.ReadAsAsync<ItemsPageModel<WorkQueueItemNoDataXmlModel>>());

            workQueueItemsPage.Items.Should().BeEmpty();
            workQueueItemsPage.PagingToken.Should().BeNull();
        }

        [Test]
        public async Task WorkQueueGetItems_OnInvalidSortBy_ReturnsBadRequest()
        {
            var workQueueId = Guid.NewGuid();
            var queuesItems = new[]
            {
                GetTestBluePrismWorkQueueItem(workQueueId, DateTime.Now)
            };

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(workQueueId, It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Returns(queuesItems);

            var result = await Subject.GetWorkQueueItemsUsingQueryString(workQueueId, "workQueueItemParameters.sortBy=invalidSortByName");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestCase("-5")]
        [TestCase("0")]
        [TestCase("5.5")]
        [TestCase("9999999")]
        public async Task WorkQueueGetItems_ShouldReturnBadRequest_WhenInvalidParametersProvided(string itemsPerPage)
        {
            var result = await Subject.GetWorkQueueItemsUsingQueryString(Guid.NewGuid(), $"workQueueItemParameters.itemsPerPage={itemsPerPage}");
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task WorkQueueGetItems_OnUnexpectedException_ReturnsInternalServerErrorResponse()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(It.IsAny<Guid>(), It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Throws<Exception>();

            var result = await Subject.WorkQueueGetQueueItems(Guid.NewGuid(), WorkQueueItemSortBy.AttemptAsc);

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task WorkQueueGetItems_OnQueueNotFound_ReturnsQueueNotFoundResponse()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(It.IsAny<Guid>(), It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Throws(new NoSuchQueueException(Guid.Empty));

            var result = await Subject.WorkQueueGetQueueItems(Guid.NewGuid(), WorkQueueItemSortBy.AttemptAsc);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task WorkQueueGetItems_ShouldReturnQueueItemsWithPagingToken_WhenSuccessful()
        {
            //TODO use of the GetTestBluePrismWorkQueueItemLimitedCollection is due to an error
            //deserialising json and getting the correct data from the collection in workqueueitem.Data
            var queueItems = new[]
            {
                GetTestBluePrismWorkQueueItemLimitedCollection(Guid.NewGuid(), DateTime.UtcNow),
                GetTestBluePrismWorkQueueItemLimitedCollection(Guid.NewGuid(), DateTime.UtcNow),
                GetTestBluePrismWorkQueueItemLimitedCollection(Guid.NewGuid(), DateTime.UtcNow),
            };

            var lastItem = queueItems.OrderBy(x => x.Attempt).Last();

            var workQueueItemParameters = new Models.WorkQueueItemParameters
            {
                SortBy = WorkQueueItemSortBy.AttemptAsc,
                ItemsPerPage = 2,
                KeyValue = new StartsWithOrContainsStringFilterModel { Ctn = "abc" },
                ExceptionReason = new StartsWithOrContainsStringFilterModel { Strtw = "a" },
                LastUpdated = new RangeFilterModel<DateTimeOffset?> { Gte = DateTimeOffset.Parse("2021-01-10 12:54+00:00") }
            };

            var testPagingToken = new PagingToken
            {
                PreviousIdValue = lastItem.Ident,
                DataType = lastItem.Worktime.GetType().Name,
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(lastItem.Attempt),
                ParametersHashCode = workQueueItemParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            workQueueItemParameters.PagingToken = testPagingToken.ToString();

            var testQueueItems = queueItems.Select(x => x.ToDomainObjectNoDataXml().ToModelNoDataXml()).ToList();
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(It.IsAny<Guid>(), It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Returns(queueItems);

            var result = await Subject.GetWorkQueueItemsWithFiltersParameters(Guid.Empty, workQueueItemParameters)
                .Map(x => x.Content.ReadAsAsync<ItemsPageModel<WorkQueueItemNoDataXmlModel>>());

            result.Items.ShouldBeEquivalentTo(testQueueItems);
            result.PagingToken.Should().Be(testPagingToken.ToString());
        }

        [Test]
        public async Task WorkQueueGetItems_ShouldReturnBadRequest_WhenInvalidPagingTokenProvided()
        {
            var queueItems = new[]
            {
                GetTestBluePrismWorkQueueItemLimitedCollection(Guid.NewGuid(), DateTime.Now),
                GetTestBluePrismWorkQueueItemLimitedCollection(Guid.NewGuid(), DateTime.Now),
                GetTestBluePrismWorkQueueItemLimitedCollection(Guid.NewGuid(), DateTime.Now),
            };

            var lastItem = queueItems.OrderBy(x => x.Attempt).Last();

            var initialWorkQueueItemParameters = new Models.WorkQueueItemParameters
            {
                SortBy = WorkQueueItemSortBy.AttemptAsc,
                ItemsPerPage = 2,
                CompletedDate = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("2021-12-10 12:54") },
            };

            var testPagingToken = new PagingToken
            {
                PreviousIdValue = lastItem.Ident,
                DataType = lastItem.Worktime.GetType().Name,
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(lastItem.Attempt),
                ParametersHashCode = initialWorkQueueItemParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            var testWorkQueueItemParameters = new Models.WorkQueueItemParameters
            {
                SortBy = WorkQueueItemSortBy.AttemptAsc,
                ItemsPerPage = 2,
                CompletedDate = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("2020-05-05 11:24") },
                PagingToken = testPagingToken.ToString(),
            };

            var result = await Subject.GetWorkQueueItemsWithFiltersParameters(Guid.Empty, testWorkQueueItemParameters);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetWorkQueueItems_ShouldReturnOk_WhenCorrectWorkTimeFilterParameterSupplied()
        {
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(It.IsAny<Guid>(), It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Returns(new List<clsWorkQueueItem>());

            var result = await Subject.GetWorkQueueItemsUsingQueryString(It.IsAny<Guid>(), "totalWorkTime.eq=10");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetWorkQueueItems_ShouldReturnHttpStatusBadRequestWithWorkTimeInvalidField_WhenInvalidWorkTimeFilterParameterSupplied()
        {
            var result = await Subject.GetWorkQueueItemsUsingQueryString(It.IsAny<Guid>(), "totalWorkTime.eq=fail");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            (await result.Content.ReadAsStringAsync())
                .Map(JsonConvert.DeserializeObject<IList<ValidationErrorModel>>)
                .First()
                .InvalidField
                .Should()
                .Be("TotalWorkTime.Eq");
        }

        [TestCaseSource(nameof(InvalidWorkQueueItemsParameters))]
        public async Task GetWorkQueueItems_ShouldReturnHttpStatusBadRequestWithInvalidField_WhenInvalidFilterParameterSupplied((Models.WorkQueueItemParameters, string) invalidParameter)
        {
            var (workQueueItemParameters, expectedInvalidFieldName) = invalidParameter;

            var result = await Subject.GetWorkQueueItemsWithFiltersParameters(It.IsAny<Guid>(), workQueueItemParameters);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            (await result.Content.ReadAsStringAsync())
                .Map(JsonConvert.DeserializeObject<IList<ValidationErrorModel>>)
                .First()
                .InvalidField
                .Should()
                .Be(expectedInvalidFieldName);
        }

        [Test]
        [TestCase("AttemptAsc")]
        [TestCase("ATTEMPTAsc")]
        [TestCase("attemptasc")]
        [TestCase("ATTEMPTASC")]

        public async Task GetWorkQueueItems_OnVariousSortByCasings_ShouldSortByExpectedField(string sortBy)
        {
            var queueItems = new[]
            {
                GetTestBluePrismWorkQueueItemLimitedCollection(Guid.NewGuid(), DateTime.UtcNow),
                GetTestBluePrismWorkQueueItemLimitedCollection(Guid.NewGuid(), DateTime.UtcNow),
                GetTestBluePrismWorkQueueItemLimitedCollection(Guid.NewGuid(), DateTime.UtcNow),
            };


            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueueItems(It.IsAny<Guid>(), It.IsAny<BluePrism.Server.Domain.Models.WorkQueueItemParameters>()))
                .Returns(queueItems);

            var result = await Subject.GetWorkQueueItemsUsingQueryString(It.IsAny<Guid>(), "workQueueItemParameters.sortBy=" + sortBy);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetWorkQueue_ShouldReturnOk_WhenValidWorkQueueId()
        {
            var workQueueId = Guid.NewGuid();
            var workQueue = new WorkQueue { Id = workQueueId, Name = "aaa", KeyField = "test" };


            var workQueueWithGroup = workQueue.ToBluePrismWorkQueueWithGroup();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueue(It.IsAny<Guid>()))
                .Returns(workQueueWithGroup);

            var result = await Subject.GetWorkQueue(workQueueId);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetWorkQueue_ShouldReturnWorkQueue_WhenSuccessful()
        {
            var workQueueId = Guid.NewGuid();
            var workQueues = new WorkQueue { Id = workQueueId, Name = "aaa", KeyField = "test", GroupName = string.Empty };

            var expectedWorkQueueModel = workQueues.ToModel();
            var workQueueWithGroup = workQueues.ToBluePrismWorkQueueWithGroup();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueue(It.IsAny<Guid>()))
                .Returns(workQueueWithGroup);

            var result = await Subject.GetWorkQueue(workQueueId)
                .Map(x => x.Content.ReadAsAsync<WorkQueueModel>());

            result.Should().Be(expectedWorkQueueModel);
        }

        [Test]
        public async Task GetWorkQueue_OnQueueNotFound_ReturnsQueueNotFoundResponse()
        {
            var workQueueId = Guid.NewGuid();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueue(It.IsAny<Guid>()))
                .Returns((WorkQueueWithGroup)null);

            var result = await Subject.GetWorkQueue(workQueueId);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task CreateWorkQueueItems_ShouldReturnCreatedWorkQueueIds_WhenSuccessful()
        {
            var workQueueId = Guid.NewGuid();
            var dictionary = new List<IReadOnlyDictionary<string, DataValueModel>>
            {
                new Dictionary<string, DataValueModel>()
                {
                    {"Key1", new DataValueModel() {Value = DateTimeOffset.Now, ValueType = DataValueType.DateTime}},
                    {"Key2", new DataValueModel() {Value = DateTimeOffset.Now, ValueType = DataValueType.Date}},
                    {"Key3", new DataValueModel() {Value = DateTimeOffset.Now, ValueType = DataValueType.Time}},
                    {"Key4", new DataValueModel() {Value = "Some string", ValueType = DataValueType.Text}},
                    {"Key5", new DataValueModel() {Value = (decimal)100, ValueType = DataValueType.Number}},
                    {"Key6", new DataValueModel() {Value = true, ValueType = DataValueType.Flag}},
                    {"Key7", new DataValueModel() {Value = DataHelper.ToSecureString("Password").ToString(), ValueType = DataValueType.Password}},
                    {"Key8", new DataValueModel() {Value = new TimeSpan(1,0,0), ValueType = DataValueType.TimeSpan}},
                    {"Key9", new DataValueModel() {Value = ToBase64String("Some string"), ValueType = DataValueType.Binary}},
                    {"Key10", new DataValueModel() {Value = ImageToBase64String(DrawFilledRectangle(100,100)), ValueType = DataValueType.Image}},
                    {"Key11", new DataValueModel() {Value = GetInputCollection(), ValueType = DataValueType.Collection}},
                }
            };

            var requestModel = new List<CreateWorkQueueItemModel>
            {
                new CreateWorkQueueItemModel()
                {
                    Priority = 1,
                    Status = "Open",
                    Data = new DataCollectionModel()
                    {
                        Rows = dictionary
                    },
                    Tags = new List<string>() {"tag1", "tag2"},
                    DeferredDate = null
                }
            };

            var workQueueWithGroup = new WorkQueueWithGroup { Id = Guid.NewGuid(), Name = "Test" };


            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueue(workQueueId))
                .Returns(workQueueWithGroup);

            var expectedCreatedIds = new[] { Guid.NewGuid() };
            GetMock<IServer>()
                .Setup(m => m.WorkQueueAddItemsAPI(It.Is<string>(x => x == workQueueWithGroup.Name), It.IsAny<IEnumerable<CreateWorkQueueItemRequest>>()))
                .Returns(expectedCreatedIds);

            var result = await Subject.CreateWorkQueueItems(workQueueId, requestModel)
                                    .Map(x => x.Content.ReadAsAsync<CreateWorkQueueItemResponseModel>());

            result.Ids.Should().BeEquivalentTo(expectedCreatedIds);
        }

        [Test]
        public async Task CreateWorkQueueItems_ShouldReturnCorrectStatusCode_WhenSuccessful()
        {
            var workQueueId = Guid.NewGuid();
            var dictionary = new List<IReadOnlyDictionary<string, DataValueModel>>
            {
                new Dictionary<string, DataValueModel>()
                {
                    {"Key1", new DataValueModel() {Value = DateTime.Now, ValueType = DataValueType.DateTime}},
                    {"Key2", new DataValueModel() {Value = DateTime.Now.Date, ValueType = DataValueType.Date}},
                }
            };

            var requestModel = new List<CreateWorkQueueItemModel>
            {
                new CreateWorkQueueItemModel()
                {
                    Priority = 1,
                    Status = "Open",
                    Data = new DataCollectionModel()
                    {
                       Rows = dictionary
                    },
                    Tags = new List<string>() {"tag1", "tag2"},
                    DeferredDate = null
                }
            };

            var workQueueWithGroup = new WorkQueueWithGroup { Id = Guid.NewGuid(), Name = "Test" };


            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueue(workQueueId))
                .Returns(workQueueWithGroup);

            GetMock<IServer>()
                .Setup(m => m.WorkQueueAddItemsAPI(It.IsAny<string>(), It.IsAny<IEnumerable<CreateWorkQueueItemRequest>>()))
                .Returns(new[] { Guid.NewGuid() });

            var result = await Subject.CreateWorkQueueItems(workQueueId, requestModel);

            result.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [TestCaseSource(nameof(InvalidDataValue))]
        public async Task CreateWorkQueueItems_ShouldReturnBadRequest_WhenSentInvalidDataTypes(DataValueType dataValueType, object dataValue)
        {
            var workQueueId = Guid.NewGuid();
            var dictionary = new List<IReadOnlyDictionary<string, DataValueModel>>
            {
                new Dictionary<string, DataValueModel>()
                {
                    {"Key1", new DataValueModel() {Value = dataValue, ValueType = dataValueType}},
                }
            };


            var requestModel = new List<CreateWorkQueueItemModel>
            {
                new CreateWorkQueueItemModel()
                {
                    Priority = 1,
                    Status = "Open",
                    Data = new DataCollectionModel()
                    {
                        Rows = dictionary
                    },
                    Tags = new List<string>() {"tag1", "tag2"},
                    DeferredDate = null
                }
            };


            var result = await Subject.CreateWorkQueueItems(workQueueId, requestModel);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateWorkQueueItems_ShouldReturnNotFound_WhenQueueNotFound()
        {
            var workQueueId = Guid.NewGuid();
            var requestModel = new List<CreateWorkQueueItemModel>();

            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueue(workQueueId))
                .Returns((WorkQueueWithGroup)null);


            var result = await Subject.CreateWorkQueueItems(workQueueId, requestModel);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        [TestCase]
        public async Task CreateWorkQueueItems_ShouldReturnBadRequest_WhenInvalidArgumentsError()
        {
            var workQueueId = Guid.NewGuid();
            var requestModel = new List<CreateWorkQueueItemModel>();

            var workQueueWithGroup = new WorkQueueWithGroup { Id = Guid.NewGuid(), Name = "Test" };
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueue(workQueueId))
                .Returns(workQueueWithGroup);

            GetMock<IServer>()
                .Setup(m => m.WorkQueueAddItemsAPI(It.IsAny<string>(), It.IsAny<IEnumerable<CreateWorkQueueItemRequest>>()))
                .Throws(new InvalidOperationException());

            var result = await Subject.CreateWorkQueueItems(workQueueId, requestModel);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestCase]
        public async Task CreateWorkQueueItems_ShouldReturnBadRequest_WhenArgumentNullException()
        {
            var workQueueId = Guid.NewGuid();
            var requestModel = new List<CreateWorkQueueItemModel>();

            var workQueueWithGroup = new WorkQueueWithGroup { Id = Guid.NewGuid(), Name = "Test" };
            GetMock<IServer>()
                .Setup(m => m.WorkQueueGetQueue(workQueueId))
                .Returns(workQueueWithGroup);

            GetMock<IServer>()
                .Setup(m => m.WorkQueueAddItemsAPI(It.IsAny<string>(), It.IsAny<IEnumerable<CreateWorkQueueItemRequest>>()))
                .Throws(new ArgumentNullException());

            var result = await Subject.CreateWorkQueueItems(workQueueId, requestModel);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private static (Models.WorkQueueItemParameters, string)[] InvalidWorkQueueItemsParameters() =>
            new[]
            {
                (new Models.WorkQueueItemParameters {Status = GetInvalidModel()}, "Status"),
                (new Models.WorkQueueItemParameters {KeyValue = GetInvalidModel()}, "KeyValue"),
                (new Models.WorkQueueItemParameters {LoadedDate = GetInvalidModelDateTimeOffsetFilterModel()}, "LoadedDate"),
                (new Models.WorkQueueItemParameters {DeferredDate = GetInvalidModelDateTimeOffsetFilterModel()}, "DeferredDate"),
                (new Models.WorkQueueItemParameters {LockedDate = GetInvalidModelDateTimeOffsetFilterModel()}, "LockedDate"),
                (new Models.WorkQueueItemParameters {CompletedDate = GetInvalidModelDateTimeOffsetFilterModel()}, "CompletedDate"),
                (new Models.WorkQueueItemParameters {LastUpdated = GetInvalidModelDateTimeOffsetFilterModel()}, "LastUpdated"),
                (new Models.WorkQueueItemParameters {ExceptionedDate = GetInvalidModelDateTimeOffsetFilterModel()}, "ExceptionedDate"),
            };

        private static IEnumerable<TestCaseData> InvalidParameters() =>
            new[]
            {
                (new WorkQueueParameters { Name = GetInvalidModel()}, "Name"),
                (new WorkQueueParameters { KeyField = GetInvalidModel()}, "KeyField"),
            }.ToTestCaseData();

        private static StartsWithOrContainsStringFilterModel GetInvalidModel() =>
            new StartsWithOrContainsStringFilterModel
            {
                Eq = "a",
                Gte = "a",
                Lte = "a",
            };

        private static RangeFilterModel<DateTimeOffset?> GetInvalidModelDateTimeOffsetFilterModel() =>
            new RangeFilterModel<DateTimeOffset?>
            {
                Eq = DateTimeOffset.MinValue,
            };

        private static string ToBase64String(string text)
        {
            if (text == null)
            {
                return null;
            }

            var textAsBytes = Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(textAsBytes);
        }

        public string ImageToBase64String(Bitmap image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        private Bitmap DrawFilledRectangle(int x, int y)
        {
            var bmp = new Bitmap(x, y);
            using (var graph = Graphics.FromImage(bmp))
            {
                var imageSize = new Rectangle(0, 0, x, y);
                graph.FillRectangle(Brushes.White, imageSize);
            }
            return bmp;
        }

        private static DataCollectionModel GetInputCollection()
        {
            var nestedCollectionRows = new List<IReadOnlyDictionary<string, DataValueModel>>();
            var nestedDictionary = new Dictionary<string, DataValueModel>
            {
                {"Nested Key 1", new DataValueModel() {Value = 1, ValueType = DataValueType.Number}}
            };
            nestedCollectionRows.Add(nestedDictionary);
            var nestedCollectionModel = new DataCollectionModel()
            {
                Rows = nestedCollectionRows

            };

            var dictionary = new List<IReadOnlyDictionary<string, DataValueModel>>
            {
                new Dictionary<string, DataValueModel>()
                {
                    {"Key1", new DataValueModel() {Value = nestedCollectionModel, ValueType = DataValueType.Collection}},
                }
            };

            var dataCollectionModel = new DataCollectionModel { Rows = dictionary };
            return dataCollectionModel;
        }

        private static IEnumerable<TestCaseData> InvalidDataValue() =>
            new[]
            {
                (DataValueType.Date, "not a date"),
                (DataValueType.DateTime, "not a DateTime"),
                (DataValueType.Time, "not a Time"),
                (DataValueType.Number, "not a Number"),
                (DataValueType.Flag, "not a Flag"),
                (DataValueType.Collection, "not a Collection"),
                (DataValueType.Image, "not an Image"),
                (DataValueType.Binary, "not a Binary"),
                (DataValueType.TimeSpan, "not a TimeSpan"),
                (DataValueType.Password, null),
            }.ToTestCaseData();
    }
}
