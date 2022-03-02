using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.Processes;
using BluePrism.AutomateProcessCore.Stages;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting.TestSupport;
using Moq;
using NUnit.Framework;

namespace AutomateUI.UnitTests
{
    [TestFixture]
    [Ignore("These seem to break the entire test run")]
    public class ProcessViewerTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // UserMessage.Show
            MethodReplacer.ReplaceMethod(() => UserMessage.Show(Parameter.Of<string>()), () => MockMethods.UserMessageShow(Parameter.Of<string>()));
            MethodReplacer.ReplaceMethod(() => UserMessage.ShowFloating(
                Parameter.Of<Control>(), 
                Parameter.Of<ToolTipIcon>(), 
                Parameter.Of<string>(), 
                Parameter.Of<string>(), 
                Parameter.Of<Point>(), 
                Parameter.Of<int>(), 
                Parameter.Of<bool>()), 
                () => MockMethods.UserMessageShowFloating(Parameter.Of<Control>(), 
                Parameter.Of<ToolTipIcon>(), 
                Parameter.Of<string>(), 
                Parameter.Of<string>(), 
                Parameter.Of<Point>(),
                Parameter.Of<int>(), 
                Parameter.Of<bool>()));

            // clsProcess.RunAction
            var x = Parameter.Of<clsProcessBreakpointInfo>();
            MethodReplacer<clsProcess, MockMethods>.ReplaceMethod(dst => dst.RunAction(Parameter.Of<ProcessRunAction>(), ref x, Parameter.Of<bool>()), src => src.ProcessRunAction(Parameter.Of<ProcessRunAction>(), ref x, Parameter.Of<bool>()));

            // clsProcess.GetCameraLocation
            MethodReplacer<clsProcess, MockMethods>.ReplaceMethod(dst => dst.GetCameraLocation(), src => src.ProcessGetCameraLocation());

            // clsProcess.Zoom
            MethodReplacer<clsProcess, MockMethods>.ReplaceMethod("get_Zoom", "ProcessZoom", new Type[] {});

            // clsProcess.GetExtent
            var p = Parameter.Of<Rectangle>();
            var g = Parameter.Of<Guid>();
            MethodReplacer<clsProcess, MockMethods>.ReplaceMethod(dst => dst.GetExtent(ref p, ref g), src => src.ProcessGetExtent(ref p, ref g));

            // clsProcess.ChildWaiting
            MethodReplacer<clsProcess, MockMethods>.ReplaceMethod("get_ChildWaiting", "ProcessChildWaiting", new Type[] {});

            // clsProcess.GetStage
            MethodReplacer<clsProcess, MockMethods>.ReplaceMethod(dst => dst.GetStage(Parameter.Of<Guid>()), src => src.ProcessGetStage(Parameter.Of<Guid>()));

            // clsProcess.RunStage
            MethodReplacer<clsProcess, MockMethods>.ReplaceMethod("get_RunStage", "get_ProcessStage", new Type[] {});

            // clsProcess.FocusCameraOnStage
            MethodReplacer<clsProcess, MockMethods>.ReplaceMethod(dst => dst.FocusCameraOnStage(Parameter.Of<float>(), Parameter.Of<float>(), Parameter.Of<clsProcessStage>()), src => src.ProcessFocusCameraOnStage(Parameter.Of<float>(), Parameter.Of<float>(), Parameter.Of<clsProcessStage>()));

            // clsProcess.GetBusinessObjectRef
            MethodReplacer<clsProcess, MockMethods>.ReplaceMethod(dst => dst.GetBusinessObjectRef(Parameter.Of<string>()), src => src.ProcessGetBusinessObjectRef(Parameter.Of<string>()));

            // clsProcessStage.IsVisible
            MethodReplacer<clsProcessStage, MockMethods>.ReplaceMethod(dst => dst.IsVisible(Parameter.Of<Size>()), src => src.ProcessStageIsVisible(Parameter.Of<Size>()));

            // gSv
            MethodReplacer.ReplaceMethod(typeof(app).GetMethod("get_gSv"), typeof(MockMethods).GetMethod("get_AppServer"));

            // MemberPermissions.HasPermission
            MethodReplacer<MemberPermissions, MockMethods>.ReplaceMethod(dst => dst.HasPermission(Parameter.Of<IUser>(), Parameter.Of<string[]>()), src => src.MemberPermissionsHasPermission(Parameter.Of<IUser>(), Parameter.Of<string[]>()));
        }

        private WebConnectionSettings _webConnectionSettings;

        [SetUp]
        public void SetUp()
        {
            MockMethods.Reset();
            _webConnectionSettings = new WebConnectionSettings(5, 10, 10, new List<UriWebConnectionSettings>());
        }

        [Test]
        public void DebugActionShowsMessageAndExitsWhenProcessHasNotBeenSaved()
        {
            var classUnderTest = new ctlProcessViewer();
            ReflectionHelper.SetPrivateField("mThisProcessHasNeverEverBeenSaved", classUnderTest, true);
            bool result;
            result = classUnderTest.DebugAction(ProcessRunAction.StepOver);
            Assert.AreEqual(1, MockMethods.GetCallCount("UserMessageShow"));
            Assert.IsFalse(result);
        }

        [Test]
        public void DebugActionReturnsIfNoProcess()
        {
            var classUnderTest = new ctlProcessViewer();
            ReflectionHelper.SetPrivateField("_mProcess", classUnderTest, null);
            var result = classUnderTest.DebugAction(ProcessRunAction.StepOver);
            Assert.IsFalse(result);
        }

        [Test]
        public void DebugActionRunsActionAndUpdates()
        {
            var classUnderTest = new ctlProcessViewer();
            var process = new clsProcess(null, DiagramType.Process, false)
            {
                Session = new clsSession(Guid.NewGuid(), 0, _webConnectionSettings),
                RunState = ProcessRunState.Running
            };
            ReflectionHelper.SetPrivateField("_mProcess", classUnderTest, process);
            var renderer = new clsRenderer(classUnderTest);
            ReflectionHelper.SetPrivateField("mRenderer", classUnderTest, renderer);
            ReflectionHelper.SetPrivateField("mobjParentForm", classUnderTest, new frmApplication());
            MockMethods.ProcessStage = new clsStartStage(process);
            var result = classUnderTest.DebugAction(ProcessRunAction.StepOver);
            Assert.IsTrue(result);
            Assert.AreEqual(1, MockMethods.GetCallCount("ProcessRunAction"));
            Assert.AreEqual(2, MockMethods.GetCallCount("ProcessFocusCameraOnStage"));
        }

        [Test]
        public void DebugActionReturnsFalseOnError()
        {
            var classUnderTest = new ctlProcessViewer();
            var process = new clsProcess(null, DiagramType.Process, false)
            {
                Session = new clsSession(Guid.NewGuid(), 0, _webConnectionSettings)
            };
            ReflectionHelper.SetPrivateField("_mProcess", classUnderTest, process);
            var renderer = new clsRenderer(classUnderTest);
            ReflectionHelper.SetPrivateField("mRenderer", classUnderTest, renderer);
            ReflectionHelper.SetPrivateField("mobjParentForm", classUnderTest, new frmApplication());
            MockMethods.ProcessRunActionResult = false;
            var result = classUnderTest.DebugAction(ProcessRunAction.StepOver);
            Assert.IsFalse(result);
            Assert.AreEqual(1, MockMethods.GetCallCount("ProcessRunAction"));
            Assert.AreEqual("Internal : Test error", MockMethods.UserMessageShowMessage);
        }

        [Test]
        public void DebugActionStartsDebugSession()
        {
            var classUnderTest = new ctlProcessViewer();
            var process = new clsProcess(null, DiagramType.Process, false)
            {
                Session = null
            };
            ReflectionHelper.SetPrivateField("_mProcess", classUnderTest, process);
            var serverMock = new Mock<IServer>();
            serverMock.Setup(m => m.GetResourceId(It.IsAny<string>())).Returns(Guid.NewGuid());
            MockMethods.AppServer = serverMock.Object;
            ReflectionHelper.InvokePrivateMethod<User>("set_Current", null, new User(AuthMode.Unspecified, Guid.NewGuid(), "UnitTestUser"));
            var renderer = new clsRenderer(classUnderTest);
            ReflectionHelper.SetPrivateField("mRenderer", classUnderTest, renderer);
            ReflectionHelper.SetPrivateField("mobjParentForm", classUnderTest, new frmApplication());
            classUnderTest.DebugAction(ProcessRunAction.StepOver);
            var y = Parameter.Of<int>();
            serverMock.Verify(m => m.CreateDebugSession(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<Guid>(), ref y), Times.Once);
        }

        [Test]
        public void DebugActionCanRunMultipleStages()
        {
            var classUnderTest = new ctlProcessViewer();
            var process = new clsProcess(null, DiagramType.Process, false)
            {
                Session = new clsSession(Guid.NewGuid(), 0, _webConnectionSettings)
            };
            ReflectionHelper.SetPrivateField("_mProcess", classUnderTest, process);
            var renderer = new clsRenderer(classUnderTest);
            ReflectionHelper.SetPrivateField("mRenderer", classUnderTest, renderer);
            ReflectionHelper.SetPrivateField("mobjParentForm", classUnderTest, new frmApplication());
            var result = classUnderTest.DebugAction(ProcessRunAction.RunNextStep);
            Assert.IsTrue(result);
            Assert.AreEqual(3, MockMethods.GetCallCount("ProcessRunAction"));
        }

        [Test]
        public void DebugActionStopsOnBreakPoint()
        {
            var classUnderTest = new ctlProcessViewer();
            var process = new clsProcess(null, DiagramType.Process, false)
            {
                Session = new clsSession(Guid.NewGuid(), 0, _webConnectionSettings)
            };
            ReflectionHelper.SetPrivateField("_mProcess", classUnderTest, process);
            var renderer = new clsRenderer(classUnderTest);
            ReflectionHelper.SetPrivateField("mRenderer", classUnderTest, renderer);
            ReflectionHelper.SetPrivateField("mobjParentForm", classUnderTest, new frmApplication());
            MockMethods.ProcessRunActionBreakpointInfo = new clsProcessBreakpointInfo(null, clsProcessBreakpoint.BreakEvents.Transient, null);
            var result = classUnderTest.DebugAction(ProcessRunAction.RunNextStep);
            Assert.IsTrue(result);
            Assert.AreEqual(1, MockMethods.GetCallCount("ProcessRunAction"));
        }

        [Test]
        [TestCase(false, ProcessRunAction.StepOver)]
        [TestCase(true, ProcessRunAction.StepIn)]
        public void DebugActionChecksPermissionsOnObjectOnStepIn(bool hasPermission, ProcessRunAction expectedAction)
        {
            var classUnderTest = new ctlProcessViewer();
            var process = new clsProcess(null, DiagramType.Process, false)
            {
                Session = new clsSession(Guid.NewGuid(), 0, _webConnectionSettings)
            };
            ReflectionHelper.SetPrivateField("_mProcess", classUnderTest, process);
            var renderer = new clsRenderer(classUnderTest);
            ReflectionHelper.SetPrivateField("mRenderer", classUnderTest, renderer);
            ReflectionHelper.SetPrivateField("mobjParentForm", classUnderTest, new frmApplication());
            MockMethods.ProcessRunActionBreakpointInfo = new clsProcessBreakpointInfo(null, clsProcessBreakpoint.BreakEvents.Transient, null);
            MockMethods.ProcessStage = new clsActionStage(process);
            var serverMock = new Mock<IServer>();
            serverMock.Setup(m => m.GetEffectiveMemberPermissionsForProcess(It.IsAny<Guid>())).Returns(new MemberPermissions(null));
            MockMethods.AppServer = serverMock.Object;
            bool hasCalledHasPermission = false;
            MockMethods.MemberPermissionsHasPermissionResult = () =>
            {
                bool hasPermissionResult = hasCalledHasPermission || hasPermission;
                hasCalledHasPermission = true;
                return hasPermissionResult;
            };
            var result = classUnderTest.DebugAction(ProcessRunAction.StepIn);
            Assert.IsTrue(result);
            Assert.AreEqual(expectedAction, MockMethods.ProcessRunActionAction);
            Assert.Contains("View Business Object Definition", MockMethods.RequestedPermissions);
        }

        [Test]
        [TestCase(false, ProcessRunAction.StepOver)]
        [TestCase(true, ProcessRunAction.StepIn)]
        public void DebugActionChecksPermissionsOnProcessOnStepIn(bool hasPermission, ProcessRunAction expectedAction)
        {
            var classUnderTest = new ctlProcessViewer();
            var process = new clsProcess(null, DiagramType.Process, false)
            {
                Session = new clsSession(Guid.NewGuid(), 0, _webConnectionSettings)
            };
            ReflectionHelper.SetPrivateField("_mProcess", classUnderTest, process);
            var renderer = new clsRenderer(classUnderTest);
            ReflectionHelper.SetPrivateField("mRenderer", classUnderTest, renderer);
            ReflectionHelper.SetPrivateField("mobjParentForm", classUnderTest, new frmApplication());
            MockMethods.ProcessRunActionBreakpointInfo = new clsProcessBreakpointInfo(null, clsProcessBreakpoint.BreakEvents.Transient, null);
            MockMethods.ProcessStage = new clsSubProcessRefStage(process);
            var serverMock = new Mock<IServer>();
            serverMock.Setup(m => m.GetEffectiveMemberPermissionsForProcess(It.IsAny<Guid>())).Returns(new MemberPermissions(null));
            MockMethods.AppServer = serverMock.Object;
            var hasCalledHasPermission = false;
            MockMethods.MemberPermissionsHasPermissionResult = () =>
            {
                var hasPermissionResult = hasCalledHasPermission || hasPermission;
                hasCalledHasPermission = true;
                return hasPermissionResult;
            };
            var result = classUnderTest.DebugAction(ProcessRunAction.StepIn);
            Assert.IsTrue(result);
            Assert.AreEqual(expectedAction, MockMethods.ProcessRunActionAction);
            Assert.Contains("View Process Definition", MockMethods.RequestedPermissions);
        }

        [Test]
        [TestCase(false, ProcessRunAction.Go, true, false)]
        [TestCase(false, ProcessRunAction.GotoPage, false, true)]
        [TestCase(false, ProcessRunAction.Pause, false, true)]
        [TestCase(false, ProcessRunAction.Reset, false, true)]
        [TestCase(false, ProcessRunAction.RunNextStep, true, false)]
        [TestCase(false, ProcessRunAction.StepIn, true, false)]
        [TestCase(false, ProcessRunAction.StepOut, true, false)]
        [TestCase(false, ProcessRunAction.StepOver, true, false)]
        [TestCase(true, ProcessRunAction.Go, true, true)]
        [TestCase(true, ProcessRunAction.GotoPage, false, true)]
        [TestCase(true, ProcessRunAction.Pause, false, true)]
        [TestCase(true, ProcessRunAction.Reset, false, true)]
        [TestCase(true, ProcessRunAction.RunNextStep, true, true)]
        [TestCase(true, ProcessRunAction.StepIn, true, true)]
        [TestCase(true, ProcessRunAction.StepOut, true, true)]
        [TestCase(true, ProcessRunAction.StepOver, true, true)]
        public void DebugActionChecksPermissionsOnProcessOnStepOver(bool hasPermission, ProcessRunAction action, bool shouldCheckPermission, bool shouldSucceed)
        {
            var classUnderTest = new ctlProcessViewer();
            var process = new clsProcess(null, DiagramType.Process, false)
            {
                Session = new clsSession(Guid.NewGuid(), 0, _webConnectionSettings)
            };
            ReflectionHelper.SetPrivateField("_mProcess", classUnderTest, process);
            var renderer = new clsRenderer(classUnderTest);
            ReflectionHelper.SetPrivateField("mRenderer", classUnderTest, renderer);
            ReflectionHelper.SetPrivateField("mobjParentForm", classUnderTest, new frmApplication());
            MockMethods.ProcessRunActionBreakpointInfo = new clsProcessBreakpointInfo(null, clsProcessBreakpoint.BreakEvents.Transient, null);
            MockMethods.ProcessStage = new clsSubProcessRefStage(process);
            var serverMock = new Mock<IServer>();
            serverMock.Setup(m => m.GetEffectiveMemberPermissionsForProcess(It.IsAny<Guid>())).Returns(new MemberPermissions(null));
            MockMethods.AppServer = serverMock.Object;
            MockMethods.MemberPermissionsHasPermissionResult = () => hasPermission;
            var result = classUnderTest.DebugAction(action);
            Assert.AreEqual(shouldSucceed, result);
            if (shouldCheckPermission)
            {
                Assert.Contains("Execute Process", MockMethods.RequestedPermissions);
            }
        }

        [Test]
        [TestCase(false, ProcessRunAction.Go, true, false)]
        [TestCase(false, ProcessRunAction.GotoPage, false, true)]
        [TestCase(false, ProcessRunAction.Pause, false, true)]
        [TestCase(false, ProcessRunAction.Reset, false, true)]
        [TestCase(false, ProcessRunAction.RunNextStep, true, false)]
        [TestCase(false, ProcessRunAction.StepIn, true, false)]
        [TestCase(false, ProcessRunAction.StepOut, true, false)]
        [TestCase(false, ProcessRunAction.StepOver, true, false)]
        [TestCase(true, ProcessRunAction.Go, true, true)]
        [TestCase(true, ProcessRunAction.GotoPage, false, true)]
        [TestCase(true, ProcessRunAction.Pause, false, true)]
        [TestCase(true, ProcessRunAction.Reset, false, true)]
        [TestCase(true, ProcessRunAction.RunNextStep, true, true)]
        [TestCase(true, ProcessRunAction.StepIn, true, true)]
        [TestCase(true, ProcessRunAction.StepOut, true, true)]
        [TestCase(true, ProcessRunAction.StepOver, true, true)]
        public void DebugActionChecksPermissionsOnObjectOnStepOver(bool hasPermission, ProcessRunAction action, bool shouldCheckPermission, bool shouldSucceed)
        {
            var classUnderTest = new ctlProcessViewer();
            var process = new clsProcess(null, DiagramType.Process, false)
            {
                Session = new clsSession(Guid.NewGuid(), 0, _webConnectionSettings)
            };
            ReflectionHelper.SetPrivateField("_mProcess", classUnderTest, process);
            var renderer = new clsRenderer(classUnderTest);
            ReflectionHelper.SetPrivateField("mRenderer", classUnderTest, renderer);
            ReflectionHelper.SetPrivateField("mobjParentForm", classUnderTest, new frmApplication());
            MockMethods.ProcessRunActionBreakpointInfo = new clsProcessBreakpointInfo(null, clsProcessBreakpoint.BreakEvents.Transient, null);
            MockMethods.ProcessStage = new clsActionStage(process);
            var serverMock = new Mock<IServer>();
            serverMock.Setup(m => m.GetEffectiveMemberPermissionsForProcess(It.IsAny<Guid>())).Returns(new MemberPermissions(null));
            MockMethods.AppServer = serverMock.Object;
            MockMethods.MemberPermissionsHasPermissionResult = () => hasPermission;
            var result = classUnderTest.DebugAction(action);
            Assert.AreEqual(shouldSucceed, result);
            if (shouldCheckPermission)
            {
                Assert.Contains("Execute Business Object", MockMethods.RequestedPermissions);
            }
        }

        [Test]
        [TestCase(true, ToolTipIcon.Info)]
        [TestCase(false, ToolTipIcon.Warning)]
        public void DebugActionShowsMessageWhenResettingWithParent(bool isParentLoadSuccessful, ToolTipIcon expectedIcon)
        {
            var classUnderTest = new ctlProcessViewer();
            var process = new clsProcess(null, DiagramType.Process, false)
            {
                Session = new clsSession(Guid.NewGuid(), 0, _webConnectionSettings),
                RunState = ProcessRunState.Running,
                ParentObject = "Test"
            };
            ReflectionHelper.SetPrivateField("_mProcess", classUnderTest, process);
            var renderer = new clsRenderer(classUnderTest);
            ReflectionHelper.SetPrivateField("mRenderer", classUnderTest, renderer);
            ReflectionHelper.SetPrivateField("mobjParentForm", classUnderTest, new frmApplication());
            MockMethods.ProcessStage = new clsStartStage(process);
            var serverMock = new Mock<IServer>();
            serverMock.Setup(m => m.GetProcessIDByName(It.IsAny<string>(), It.IsAny<bool>())).Returns(isParentLoadSuccessful ? Guid.NewGuid() : Guid.Empty);
            MockMethods.AppServer = serverMock.Object;
            var result = classUnderTest.DebugAction(ProcessRunAction.Reset);
            Assert.IsTrue(result);
            Assert.AreEqual(expectedIcon, MockMethods.UserMessageShowFloatingIcon);
        }

        private partial class MockMethods
        {
            private static readonly Dictionary<string, int> CalledMethods = new Dictionary<string, int>();

            public static bool ProcessRunActionResult { get; set; } = true;
            public static clsProcessBreakpointInfo ProcessRunActionBreakpointInfo { get; set; } = null;
            public static clsProcessStage ProcessStage { get; set; } = null;
            public static Func<bool> MemberPermissionsHasPermissionResult = () => false;
            public static string UserMessageShowMessage { get; set; } = string.Empty;
            public static ToolTipIcon UserMessageShowFloatingIcon { get; set; }
            public static ProcessRunAction ProcessRunActionAction { get; set; } = 0;
            public static string[] RequestedPermissions { get; set; } = new string[] { };
            public static IServer AppServer { get; set; }

            public static void UserMessageShow(string message)
            {
                IncrementMethodCallCount(nameof(UserMessageShow));
                UserMessageShowMessage = message;
            }

            public static void UserMessageShowFloating(Control parent, ToolTipIcon icon, string title, string prompt, Point location, int duration, bool isBalloon)
            {
                IncrementMethodCallCount(nameof(UserMessageShowFloating));
                UserMessageShowFloatingIcon = icon;
            }

            public StageResult ProcessRunAction(ProcessRunAction action, ref clsProcessBreakpointInfo breakpointInfo, bool pauseWhenBreakPointRaised)
            {
                IncrementMethodCallCount(nameof(ProcessRunAction));
                breakpointInfo = ProcessRunActionBreakpointInfo;
                ProcessRunActionAction = action;
                return ProcessRunActionResult ? StageResult.OK : StageResult.InternalError("Test error");
            }

            public PointF ProcessGetCameraLocation()
            {
                IncrementMethodCallCount(nameof(ProcessGetCameraLocation));
                return new PointF(0, 0);
            }

            public float ProcessZoom()
            {
                IncrementMethodCallCount(nameof(ProcessZoom));
                return 1;
            }

            public void ProcessGetExtent(ref Rectangle rectExtent, ref Guid gSubSheetID)
            {
                IncrementMethodCallCount(nameof(ProcessGetExtent));
            }

            public bool ProcessChildWaiting()
            {
                IncrementMethodCallCount(nameof(ProcessChildWaiting));
                return CalledMethods[nameof(ProcessChildWaiting)] < 3;
            }

            public clsProcessStage ProcessGetStage(Guid id)
            {
                IncrementMethodCallCount(nameof(ProcessGetStage));
                return ProcessStage;
            }

            public void ProcessFocusCameraOnStage(float width, float height, clsProcessStage stage)
            {
                IncrementMethodCallCount(nameof(ProcessFocusCameraOnStage));
            }

            public bool ProcessStageIsVisible(Size size)
            {
                IncrementMethodCallCount(nameof(ProcessStageIsVisible));
                return false;
            }

            public clsBusinessObject ProcessGetBusinessObjectRef(string name)
            {
                IncrementMethodCallCount(nameof(ProcessGetBusinessObjectRef));
                return new clsVBO(new clsVBODetails() { ID = Guid.NewGuid(), FriendlyName = "Test Object" }, null, null);
            }

            public bool MemberPermissionsHasPermission(IUser user, params string[] permissions)
            {
                IncrementMethodCallCount(nameof(MemberPermissionsHasPermission));
                RequestedPermissions = permissions.Concat(RequestedPermissions).Distinct().ToArray();
                return MemberPermissionsHasPermissionResult.Invoke();
            }

            private static void IncrementMethodCallCount(string methodName)
            {
                CalledMethods[methodName] = GetCallCount(methodName) + 1;
            }

            public static int GetCallCount(string methodName)
            {
                return CalledMethods.ContainsKey(methodName) ? CalledMethods[methodName] : 0;
            }

            public static void Reset()
            {
                CalledMethods.Clear();
                ProcessRunActionResult = true;
                ProcessRunActionBreakpointInfo = null;
                ProcessStage = null;
                MemberPermissionsHasPermissionResult = () => false;
                UserMessageShowMessage = string.Empty;
                //UserMessageShowFloatingIcon = null;
                ProcessRunActionAction = 0;
                RequestedPermissions = new string[] { };
            }
        }
    }
}
