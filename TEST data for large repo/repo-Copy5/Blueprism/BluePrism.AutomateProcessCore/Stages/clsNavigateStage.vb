Imports System.Linq
Imports System.Xml
Imports System.Text
Imports BluePrism.Server.Domain.Models
Imports System.Threading
Imports System.Runtime.Serialization
Imports BluePrism.ApplicationManager.AMI

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsNavigateStage
    ''' 
    ''' <summary>
    ''' The navigate stage performs navigation actions against a application elements
    ''' in object studio.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsNavigateStage
        Inherits clsAppStage

        ''' <summary>
        ''' Creates a new instance of the clsNavigateStage class and sets its parent
        ''' </summary>
        ''' <param name="parent">The process owning this stage.</param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of a Navigate stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsNavigateStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Navigate
            End Get
        End Property

        ''' <summary>
        ''' Executes this navigate stage
        ''' </summary>
        ''' <param name="gRunStageID">The ID of the stage - after successful
        ''' execution, this will have been updated to point to the next stage in the
        ''' process.</param>
        ''' <returns>A result object reporting success or the reason for failure of
        ''' the stage's execution.</returns>
        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult
            Try
                NavigatePrologue(logger)

                Dim interval = GetPauseAfterStep()

                ' Execute each step...
                For Each n As clsNavigateStep In Steps
                    ' It's a lot easier when you're not logging/converting anything...
                    Dim result = RunQuery(logger, n, GetPayload(n), GetArgs(n))
                    If interval > Nothing Then Thread.Sleep(interval)

                    If Not String.IsNullOrEmpty(result) Then
                        Dim outputs = GetArgumentOutputs(n)
                        If outputs.Any() Then
                            AssignOutputs(outputs, result)
                        End If
                    End If

                Next

                Dim sErr As String = Nothing
                If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then _
                 Return StageResult.InternalError(sErr)

                NavigateEpilogue(logger)
                Return StageResult.OK

            Catch ife As InvalidFormatException
                Return StageResult.InternalError(
                    My.Resources.Resources.clsNavigateStage_CouldNotEvaluatePauseAfterStep0, ife.Message)

            Catch ite As InvalidTypeException
                Return StageResult.InternalError(ite.Message)

            Catch ex As Exception
                Return StageResult.InternalError(ex)

            End Try

        End Function

        Private Sub AssignOutputs(outputs As Dictionary(Of String, String), result As String)

            Dim resultArray = result.Split(","c)

            For Each output In outputs
                Dim outputItem = resultArray.FirstOrDefault(Function(a) a.Substring(0, a.IndexOf(":"c)) = output.Key)
                If Not String.IsNullOrEmpty(outputItem) Then
                    Dim outputValue = outputItem.Substring(outputItem.IndexOf(":"c) + 1)
                    Dim val = New clsProcessValue(DataType.text, outputValue)
                    mParent.StoreValueInDataItem(output.Value, val, Me, "")
                End If
            Next

        End Sub

        Private Sub NavigatePrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.NavigatePrologue(info, Me)
        End Sub

        Private Sub NavigateEpilogue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.NavigateEpiLogue(info, Me)
        End Sub

        ''' <summary>
        ''' Creates a step from the provided XML element to reside in the given
        ''' stage.
        ''' </summary>
        ''' <param name="el">The XML element containing the information required to
        ''' populate the step</param>
        ''' <param name="appInfo">Information regarding the application in the
        ''' parent process.</param>
        ''' <returns>The step derived from the given XML.</returns>
        Protected Overrides Function CreateStep(
         el As XmlElement, appInfo As clsApplicationTypeInfo) As clsStep
            Return New clsNavigateStep(Me, el, appInfo)
        End Function

        ''' <summary>
        ''' Validates the configuration of this stage.
        ''' </summary>
        ''' <param name="repair">True to attempt to repair errors in this stage; note
        ''' that not all errors are automatically repairable.</param>
        ''' <param name="skipObjects">True to include errors about missing business
        ''' objects; False to skip such errors. Quite why this is handled differently
        ''' to all other 'Design Control' options is probably historical.</param>
        ''' <returns>A list of validation issues found in this stage.</returns>
        Public Overrides Function CheckForErrors(
         repair As Boolean, skipObjects As Boolean) As ValidationErrorList
            Dim errors As ValidationErrorList =
                MyBase.CheckForErrors(repair, skipObjects)

            For stepNo As Integer = 1 To Steps.Count
                Dim stp = DirectCast(Steps(stepNo - 1), clsNavigateStep)
                errors.AddRange(stp.CheckForErrors(Me, stepNo, repair))
            Next

            If Not PauseAfterStepExpression.IsEmpty Then

                errors.AddRange(ValidateExpression(
                    PauseAfterStepExpression,
                    My.Resources.Resources.clsNavigateStage_ForPauseAfterStep,
                    DataType.number, DataType.timespan
                ))

            End If

            Return errors

        End Function

        ''' <summary>
        ''' Analyse the usage of actions in a stage. In the counts that are returned,
        ''' it is the AMI-based actions that are counted, so for example, a Navigate
        ''' stage with three rows will count as three.
        ''' </summary>
        ''' <param name="total">The current count of the total number of actions, which
        ''' is updated on return to reflect the contents of this stage.</param>
        ''' <param name="globalcount">The current count of the number of actions that
        ''' use a 'global' method, i.e. a global mouse click or keypress. Updated on
        ''' return to reflect the contents of this stage.</param>
        ''' <param name="globaldetails">A StringBuilder to which details of any global
        ''' actions can be appended.</param>
        Public Overrides Sub AnalyseAMIActions(ByRef total As Integer, ByRef globalcount As Integer, ByVal globaldetails As StringBuilder)
            Dim stepnum As Integer = 1
            Dim stepdetails As New StringBuilder()
            For Each n As clsNavigateStep In Steps
                total += 1
                If n.Action Is Nothing Then
                    stepdetails.AppendLine(String.Format(My.Resources.Resources.clsNavigateStage_Step0NoActionSet, stepnum.ToString()))
                Else
                    If n.Action.RequiresFocus Then
                        globalcount += 1
                        stepdetails.AppendLine(String.Format(My.Resources.Resources.clsNavigateStage_Step0Action12, stepnum.ToString(), n.ActionName, n.ActionId))
                    End If
                End If
                stepnum += 1
            Next
            If stepdetails.Length > 0 Then
                globaldetails.AppendLine("====" & GetName() & "====")
                globaldetails.AppendLine(GetNarrative())
                globaldetails.AppendLine(My.Resources.Resources.clsNavigateStage_StageID + GetStageID().ToString())
                globaldetails.Append(stepdetails.ToString())
            End If
        End Sub

    End Class

End Namespace
