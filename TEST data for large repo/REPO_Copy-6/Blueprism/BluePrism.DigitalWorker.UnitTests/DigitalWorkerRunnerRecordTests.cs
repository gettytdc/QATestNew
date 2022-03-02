using BluePrism.DigitalWorker.Sessions;
using BluePrism.UnitTesting;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrism.DigitalWorker.UnitTests
{
    [TestFixture]
    class DigitalWorkerRunnerRecordTests : UnitTestBase<DigitalWorkerRunnerRecord>
    {
        [Test]
        public void Create_InvalidDataAccess_ThrowException()
        {
            Action create = () => new DigitalWorkerRunnerRecord(
                null,
                "",
                Guid.NewGuid(),
                Guid.NewGuid(),
                3,
                "dave",
                Guid.NewGuid(),
                GetMock<IRunningSessionMonitor>().Object);

            create.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Create_InvalidWorkerName_ThrowException()
        {
            Action create = () => new DigitalWorkerRunnerRecord(
                GetMock<IDigitalWorkerDataAccess>().Object,
                "",
                Guid.NewGuid(),
                Guid.NewGuid(),
                3,
                "",
                Guid.NewGuid(),
                GetMock<IRunningSessionMonitor>().Object);

            create.ShouldThrow<ArgumentNullException>();
        }
    }
}
