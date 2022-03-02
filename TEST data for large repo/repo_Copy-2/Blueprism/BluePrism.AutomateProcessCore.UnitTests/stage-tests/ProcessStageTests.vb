#If UNITTESTS Then

Imports System.Linq
Imports BluePrism.AutomateProcessCore.Stages
Imports NUnit.Framework

Public Class ProcessStageTests
    Private Class TestStage
        Inherits clsProcessStage
        Private mStage As StageTypes

        Sub New(stage As StageTypes)
            MyBase.New(Nothing)
            mStage = stage
            Name = "new"
        End Sub

        Public Overrides ReadOnly Property StageType As StageTypes
            Get
                Return mStage
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult
            Throw New NotImplementedException()
        End Function

        Public Overrides Function CloneCreate() As clsProcessStage
            Throw New NotImplementedException()
        End Function

        Friend Overloads Function GetLogInfo(hasFailed As Boolean) As LogInfo
            Return MyBase.GetLogInfo(hasFailed)
        End Function

    End Class

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsAlways_DefaultDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.DefaultDiags
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(True)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsOnSuccess_DefaultDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.DefaultDiags
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsNever_DefaultDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.DefaultDiags
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsAlways_ForceGCDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.ForceGC
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(True)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsOnSuccess_ForceGCDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.ForceGC
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsNever_ForceGCDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.ForceGC
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsAlways_LogWebServicesDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogWebServices
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(True)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsOnSuccess_LogWebServicesDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogWebServices
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsNever_LogWebServicesDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogWebServices
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsAlways_LogAllDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideAll
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsOnSuccess_LogAllDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideAll
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsNever_LogAllDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideAll
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsAlways_LogErrorsOnlyDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideErrorsOnly
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsOnSuccess_LogErrorsOnlyDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideErrorsOnly
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsNever_LogErrorsOnlyDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideErrorsOnly
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(KeyStages))>
    Public Sub LogInfo_KeyStage_StageInhibitsAlways_LogKeyDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(KeyStages))>
    Public Sub LogInfo_KeyStage_StageInhibitsOnSuccess_LogKeyDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(KeyStages))>
    Public Sub LogInfo_KeyStage_StageInhibitsNever_LogKeyDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(True)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(NonKeyStages))>
    Public Sub LogInfo_NonKeyStage_StageInhibitsAlways_LogKeyDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(True)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(NonKeyStages))>
    Public Sub LogInfo_NonKeyStage_StageInhibitsOnSuccess_LogKeyDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(True)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(NonKeyStages))>
    Public Sub LogInfo_NonKeyStage_StageInhibitsNever_LogKeyDiags_HasFailed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(True)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsAlways_DefaultDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.DefaultDiags
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsOnSuccess_DefaultDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.DefaultDiags
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsNever_DefaultDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.DefaultDiags
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(False)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsAlways_ForceGCDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.ForceGC
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsOnSuccess_ForceGCDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.ForceGC
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsNever_ForceGCDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.ForceGC
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(False)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsAlways_LogWebServicesDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogWebServices
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsOnSuccess_LogWebServicesDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogWebServices
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsNever_LogWebServicesDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogWebServices
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(False)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsAlways_LogAllDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideAll
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(False)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsOnSuccess_LogAllDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideAll
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(False)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsNever_LogAllDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideAll
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(False)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsAlways_LogErrorsOnlyDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideErrorsOnly
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsOnSuccess_LogErrorsOnlyDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideErrorsOnly
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(AllStages))>
    Public Sub LogInfo_StageInhibitsNever_LogErrorsOnlyDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideErrorsOnly
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(KeyStages))>
    Public Sub LogInfo_KeyStage_StageInhibitsAlways_LogKeyDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(False)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(KeyStages))>
    Public Sub LogInfo_KeyStage_StageInhibitsOnSuccess_LogKeyDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(False)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(KeyStages))>
    Public Sub LogInfo_KeyStage_StageInhibitsNever_LogKeyDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(False)

        Assert.False(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(NonKeyStages))>
    Public Sub LogInfo_NonKeyStage_StageInhibitsAlways_LogKeyDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Always

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(NonKeyStages))>
    Public Sub LogInfo_NonKeyStage_StageInhibitsOnSuccess_LogKeyDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.OnSuccess

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    <TestCaseSource(NameOf(NonKeyStages))>
    Public Sub LogInfo_NonKeyStage_StageInhibitsNever_LogKeyDiags_HasPassed(stageType As StageTypes)
        clsAPC.Diagnostics = clsAPC.Diags.LogOverrideKey
        Dim stage = New TestStage(stageType)
        stage.LogInhibit = LogInfo.InhibitModes.Never

        Dim info = stage.GetLogInfo(False)

        Assert.True(info.Inhibit)
    End Sub

    Protected Shared Function KeyStages() As IEnumerable(Of StageTypes)
        Return clsProcessStage.KeyStages
    End Function

    Protected Shared Function AllStages() As IEnumerable(Of StageTypes)
        Return CType([Enum].GetValues(GetType(StageTypes)), StageTypes())
    End Function

    Protected Shared Function NonKeyStages() As IEnumerable(Of StageTypes)
        Return AllStages().Except(KeyStages())
    End Function

End Class

#End If