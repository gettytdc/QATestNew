using System;
using BluePrism.AutomateProcessCore;
using BluePrism.DigitalWorker.Sessions;

namespace BluePrism.DigitalWorker.UnitTests.Sessions
{
    public class SessionContextBuilder
    {
        private Guid _processId = BackgroundProcessId;
        private BusinessObjectRunMode _runMode;

        private static readonly Guid BackgroundProcessId = Guid.Parse("b83871bc-45e1-45af-b47c-0bacac221845");
        private static readonly Guid ForegroundProcessId = Guid.Parse("f83871bc-45e1-45af-b47c-0bacac221845");
        private static readonly Guid ExclusiveProcessId = Guid.Parse("e83871bc-45e1-45af-b47c-0bacac221845");
        private static readonly string TestProcessXml = "<process />";
        private const string SessionStartedByUsername = "Tester";

        public SessionContextBuilder Background()
        {
            _runMode = BusinessObjectRunMode.Background;
            _processId = BackgroundProcessId;
            return this;
        }

        public SessionContextBuilder Foreground()
        {
            _runMode = BusinessObjectRunMode.Foreground;
            _processId = ForegroundProcessId;
            return this;
        }

        public SessionContextBuilder Exclusive()
        {
            _runMode = BusinessObjectRunMode.Exclusive;
            _processId = ExclusiveProcessId;
            return this;
        }

        public SessionContext Build()
        {
            return new SessionContext(Guid.NewGuid(),
                new ProcessInfo(_processId, _runMode, TestProcessXml), SessionStartedByUsername);
        }
    }
}