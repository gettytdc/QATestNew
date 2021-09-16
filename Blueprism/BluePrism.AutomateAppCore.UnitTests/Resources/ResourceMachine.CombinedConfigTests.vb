#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateProcessCore
Imports NUnit.Framework

Namespace Resources

    ''' <summary>
    ''' A suite of tests that check the logic in the clsResourceMachine class.
    ''' </summary>
    <TestFixture>
    Public Class ResourceMachineCombinedConfigTests
        Private mSut As ResourceMachine.CombinedConfig

        <SetUp>
        Public Sub SetUp()
            mSut = New ResourceMachine.CombinedConfig()
        End Sub

        <Test>
        Public Sub Constructor_SetAllLoggingToIndeterminateState()
            Dim config = New ResourceMachine.CombinedConfig()
            Assert.AreEqual(ResourceMachine.CombinedConfig.CombinedState.Indeterminate, config.LoggingDefault)
            Assert.AreEqual(ResourceMachine.CombinedConfig.CombinedState.Indeterminate, config.LoggingAllOverride)
            Assert.AreEqual(ResourceMachine.CombinedConfig.CombinedState.Indeterminate, config.LoggingKeyOverride)
            Assert.AreEqual(ResourceMachine.CombinedConfig.CombinedState.Indeterminate, config.LoggingErrorsOnlyOverride)
            Assert.AreEqual(ResourceMachine.CombinedConfig.CombinedState.Indeterminate, config.LoggingMemory)
            Assert.AreEqual(ResourceMachine.CombinedConfig.CombinedState.Indeterminate, config.LoggingForceGC)
            Assert.AreEqual(ResourceMachine.CombinedConfig.CombinedState.Indeterminate, config.LoggingToEventLog)
            Assert.AreEqual(ResourceMachine.CombinedConfig.CombinedState.Indeterminate, config.LoggingWebServices)
        End Sub

        <Test>
        <TestCase(ResourceMachine.CombinedConfig.CombinedState.Indeterminate, True, ResourceMachine.CombinedConfig.CombinedState.Indeterminate)>
        <TestCase(ResourceMachine.CombinedConfig.CombinedState.Indeterminate, False, ResourceMachine.CombinedConfig.CombinedState.Indeterminate)>
        <TestCase(ResourceMachine.CombinedConfig.CombinedState.Enabled, True, ResourceMachine.CombinedConfig.CombinedState.Enabled)>
        <TestCase(ResourceMachine.CombinedConfig.CombinedState.Enabled, False, ResourceMachine.CombinedConfig.CombinedState.Indeterminate)>
        <TestCase(ResourceMachine.CombinedConfig.CombinedState.Disabled, False, ResourceMachine.CombinedConfig.CombinedState.Disabled)>
        <TestCase(ResourceMachine.CombinedConfig.CombinedState.Disabled, True, ResourceMachine.CombinedConfig.CombinedState.Indeterminate)>
        Public Sub CompoundStateAndFlag_ReturnsCorrectState(state As ResourceMachine.CombinedConfig.CombinedState,
                                                            flag As Boolean,
                                                            expectedState As ResourceMachine.CombinedConfig.CombinedState)
            Assert.AreEqual(expectedState, mSut.CompoundState(state, flag))
        End Sub

        <Test>
        Public Sub SetLoggingFlags_DefaultTrue_SetsCorrectly()

            mSut.SetLoggingStates(clsAPC.Diags.DefaultDiags, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled)
        End Sub

        <Test>
        Public Sub SetLoggingFlags_OverrideAllTrue_SetsCorrectly()

            mSut.SetLoggingStates(clsAPC.Diags.LogOverrideAll, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled)
        End Sub

        <Test>
        Public Sub SetLoggingFlags_OverrideKeyTrue_SetsCorrectly()

            mSut.SetLoggingStates(clsAPC.Diags.LogOverrideKey, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled)
        End Sub

        <Test>
        Public Sub SetLoggingFlags_OverrideErrorsOnlyTrue_SetsCorrectly()

            mSut.SetLoggingStates(clsAPC.Diags.LogOverrideErrorsOnly, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled)
        End Sub

        <Test>
        Public Sub SetLoggingFlags_MemoryTrue_SetsCorrectly()

            mSut.SetLoggingStates(clsAPC.Diags.LogMemory, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled)
        End Sub

        <Test>
        Public Sub SetLoggingFlags_ForceGCTrue_SetsCorrectly()

            mSut.SetLoggingStates(clsAPC.Diags.ForceGC, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled)
        End Sub

        <Test>
        Public Sub SetLoggingFlags_WebServicesTrue_SetsCorrectly()

            mSut.SetLoggingStates(clsAPC.Diags.LogWebServices, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled)
        End Sub

        <Test>
        Public Sub SetLoggingFlags_EventLogTrue_SetsCorrectly()

            mSut.SetLoggingStates(clsAPC.Diags.DefaultDiags, True)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Disabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled)
        End Sub

        <Test>
        Public Sub Compound_LogDefault_AllLoggingEnabled_OnlyLogDefault()
            mSut.EnableAllLogging()

            mSut.CompoundLoggingStates(clsAPC.Diags.DefaultDiags, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate)
        End Sub

        <Test>
        Public Sub Compound_LogOverrideAll_AllLoggingEnabled_OnlyLogOverrideAll()
            mSut.EnableAllLogging()

            mSut.CompoundLoggingStates(clsAPC.Diags.LogOverrideAll, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate)
        End Sub

        <Test>
        Public Sub Compound_LogOverrideKey_AllLoggingEnabled_OnlyLogOverrideKey()
            mSut.EnableAllLogging()

            mSut.CompoundLoggingStates(clsAPC.Diags.LogOverrideKey, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate)
        End Sub

        <Test>
        Public Sub Compound_LogOverrideErrorsOnly_AllLoggingEnabled_OnlyLogErrorsOnly()
            mSut.EnableAllLogging()

            mSut.CompoundLoggingStates(clsAPC.Diags.LogOverrideErrorsOnly, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate)
        End Sub

        <Test>
        Public Sub Compound_LogMemory_AllLoggingEnabled_OnlyLogMemory()
            mSut.EnableAllLogging()

            mSut.CompoundLoggingStates(clsAPC.Diags.LogMemory, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate)
        End Sub

        <Test>
        Public Sub Compound_LogForceGC_AllLoggingEnabled_OnlyLogForceGC()
            mSut.EnableAllLogging()

            mSut.CompoundLoggingStates(clsAPC.Diags.ForceGC, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate)
        End Sub

        <Test>
        Public Sub Compound_LogWebServices_AllLoggingEnabled_OnlyLogWebServices()
            mSut.EnableAllLogging()

            mSut.CompoundLoggingStates(clsAPC.Diags.LogWebServices, False)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate)
        End Sub

        <Test>
        Public Sub Compound_LogEventLog_AllLoggingEnabled_OnlyLogEventLog()

            mSut.EnableAllLogging()

            mSut.CompoundLoggingStates(clsAPC.Diags.DefaultDiags, True)

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Indeterminate,
                ResourceMachine.CombinedConfig.CombinedState.Enabled)
        End Sub

        <Test>
        Public Sub EnableAllLogging_DoesEnableAll()
            mSut.EnableAllLogging()

            CheckLoggingStates(
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled,
                ResourceMachine.CombinedConfig.CombinedState.Enabled)
        End Sub

        Private Sub CheckLoggingStates(defaultState As ResourceMachine.CombinedConfig.CombinedState,
                                       allOverride As ResourceMachine.CombinedConfig.CombinedState,
                                       keyOverride As ResourceMachine.CombinedConfig.CombinedState,
                                       errorsOnlyOverride As ResourceMachine.CombinedConfig.CombinedState,
                                       memory As ResourceMachine.CombinedConfig.CombinedState,
                                       forceGC As ResourceMachine.CombinedConfig.CombinedState,
                                       webServices As ResourceMachine.CombinedConfig.CombinedState,
                                       EventLog As ResourceMachine.CombinedConfig.CombinedState)

            Assert.AreEqual(defaultState, mSut.LoggingDefault)
            Assert.AreEqual(allOverride, mSut.LoggingAllOverride)
            Assert.AreEqual(keyOverride, mSut.LoggingKeyOverride)
            Assert.AreEqual(errorsOnlyOverride, mSut.LoggingErrorsOnlyOverride)
            Assert.AreEqual(memory, mSut.LoggingMemory)
            Assert.AreEqual(forceGC, mSut.LoggingForceGC)
            Assert.AreEqual(webServices, mSut.LoggingWebServices)
            Assert.AreEqual(EventLog, mSut.LoggingToEventLog)
        End Sub

    End Class
End Namespace
#End If
