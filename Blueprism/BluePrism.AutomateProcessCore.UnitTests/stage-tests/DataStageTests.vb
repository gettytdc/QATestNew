#If UNITTESTS Then

Imports System.Linq
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.ProcessLoading
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.CharMatching
Imports Moq
Imports NUnit.Framework

Public Class DataStageTests

    Dim mMock As IGroupObjectDetails = (New Mock(Of IGroupObjectDetails)).Object
    Dim mProcess As clsProcess
    Dim mDataStage As clsDataStage

    <SetUp>
    Public Sub SetUp()
        clsAPC.ProcessLoader = New PrcoessLoaderMock()
        mProcess = New clsProcess(mMock, DiagramType.Process, False)
        mDataStage = DirectCast(mProcess.AddDataStage("Test Data Stage", DataType.text), clsDataStage)

        mDataStage.Exposure = StageExposureType.Environment
    End Sub

    <Test(Description:="Check that, if the data type of an environment level data stage does not match, the corresponding environment variable an error is raised")>
    <TestCase(DataType.number, 123, ExpectedResult:=True, Description:="Expect to FAIL as data stage data type is text and this test case is number")>
    <TestCase(DataType.text, 1234, ExpectedResult:=False, Description:="Expect to PASS as data stage data type is text as is this test case")>
    Public Function Can_Compare_Data_Stage_Environment_Variable_Type(dataType As DataType, val As Integer) As Boolean

        Dim clsValue As clsProcessValue

        Select Case dataType
            Case DataType.text
                clsValue = New clsProcessValue(val.ToString) With {.DataType = dataType}
            Case Else
                clsValue = New clsProcessValue(val) With {.DataType = dataType}
        End Select

        Dim envVar = New clsArgument("Test Data Stage", clsValue)

        mProcess.EnvVars.Add("Test Data Stage", envVar)

        Dim errors = mDataStage.CheckForErrors(False, False)

        ' Return "are there any errors?"
        Return errors.Any(Function(e) e.CheckID.Equals(ValidationCheckType.DataStageDataTypeMissMatchEnvVariableDataType))
    End Function

    <Test(Description:="Check that, if the data type of an environment level data stage does not match, this can be fixed")>
    Public Sub Can_Fix_Data_Stage_Environment_Variable_Type_Missmatches()

        Dim envVar As clsArgument = New clsArgument("Test Data Stage", New clsProcessValue(123) With {.DataType = DataType.number})

        ' The envar type does not match that of mDataStage so should generate an error
        mProcess.EnvVars.Add("Test Data Stage", envVar)

        ' Check for this error and try to resolve
        Dim errors = mDataStage.CheckForErrors(True, False)

        ' Assert that there are no errors of the data type missmatch check
        Assert.IsFalse(errors.Any(Function(e) e.CheckID.Equals(ValidationCheckType.DataStageDataTypeMissMatchEnvVariableDataType)))
    End Sub

End Class

#End If