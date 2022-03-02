using System;
using System.Threading;

namespace LoginAgentServiceTest
{
    internal class MockLoginAgentService : LoginAgentService.LoginAgentService
    {
        protected override bool IsUserLoggedOn
        {
            get { return false; }
        }

        private static void Main(string[] args)
        {
            var s = new MockLoginAgentService();
            s.OnStart(args);
            Thread.Sleep(TimeSpan.FromSeconds(10));
            s.OnStop();
        }
    }
}
