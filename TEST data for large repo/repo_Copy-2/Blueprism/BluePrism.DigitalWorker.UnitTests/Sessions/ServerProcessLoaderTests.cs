using System;
using System.Collections.Generic;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.ProcessLoading;
using BluePrism.ApplicationManager.AMI;
using BluePrism.CharMatching;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.DigitalWorker.UnitTests.Sessions
{
    public class ServerProcessLoaderTests : UnitTestBase<ServerProcessInfoLoader>
    {
        private static readonly Guid TestProcessId = Guid.Parse("d83871bc-45e1-45af-b47c-0bacac221845");
        private static readonly string TestProcessXml = "<process />";
        private static readonly BusinessObjectRunMode TestRunMode = BusinessObjectRunMode.Exclusive;

        public override void OneTimeSetup()
        {
            base.OneTimeSetup();
            clsAPC.ProcessLoader = new TestProcessLoader();
        }

        public override void OneTimeTearDown()
        {
            clsAPC.ProcessLoader = null;
        }

        [Test]
        public void GetProcess_ValidProcess_ShouldLoadFromServer()
        {
            var process = ClassUnderTest.GetProcess(TestProcessId);

            process.ProcessId.Should().Be(TestProcessId);
            process.ProcessXml.Should().Be(TestProcessXml);
            process.EffectiveRunMode.Should().Be(TestRunMode);
        }

        private class TestProcessLoader : IProcessLoader
        {
            public bool GetProcessXML(Guid gProcessID, ref string sXML, ref DateTime lastmod, ref string sErr)
            {
                if (gProcessID == TestProcessId)
                {
                    sXML = TestProcessXml;
                    return true;
                }
                else
                {
                    sErr = "failed to get Process XML";
                    return false;
                }
            }

            public BusinessObjectRunMode GetEffectiveRunMode(Guid processId)
            {
                if (processId == TestProcessId)
                {
                    return TestRunMode;
                }
                else
                {
                    // No explicit check for invalid id in server code
                    throw new NullReferenceException();
                }
            }

            #region Not implemented 

            public BPFont GetFont(string name)
            {
                throw new NotImplementedException();
            }
            public string GetFontOcrPlus(string name)
            {
                throw new NotImplementedException();
            }

            public bool DeleteFont(string name)
            {
                throw new NotImplementedException();
            }

            public ICollection<string> AvailableFontNames { get; set; }

            public void SaveFont(BPFont font)
            {
                throw new NotImplementedException();
            }
            public void SaveFontOcrPlus(string name, string data)
            {
                throw new NotImplementedException();
            }

            public bool GetProcessAtrributes(Guid gProcessID, ref ProcessAttributes Attributes, ref string serr)
            {
                throw new NotImplementedException();
            }

            public Dictionary<string, clsArgument> GetEnvVars()
            {
                throw new NotImplementedException();
            }
            public Dictionary<string, clsArgument> GetEnvVars(bool refreshFromServer)
            {
                throw new NotImplementedException();
            }

            public clsArgument GetEnvVarSingle(string name, bool updateCache)
            {
                throw new NotImplementedException();
            }

            public clsGlobalInfo GetAMIInfo()
            {
                throw new NotImplementedException();
            }

            public CacheRefreshBehaviour CacheBehaviour { get; set; }

            #endregion

        }

    }


}
