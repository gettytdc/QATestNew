Imports System.Runtime.Serialization
Imports System.Xml
Imports System.Text
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Common.Security
Imports BluePrism.ApplicationManager.AMI

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsWriteStage
    ''' 
    ''' <summary>
    ''' The write stage evaluates an expression and stores the data in 
    ''' and application element. 
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsWriteStage
        Inherits clsAppStage

        ''' <summary>
        ''' Creates a new instance of the clsWriteStage class and sets its parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of a Write stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsWriteStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Write
            End Get
        End Property

        ''' <summary>
        ''' Returns items referred to by this stage, currently only things defined
        ''' within the process (e.g. data items).
        ''' Note model element references are handled by the base class (clsAppStage)
        ''' </summary>
        ''' <param name="inclInternal">Indicates internal references required</param>
        ''' <returns>List of dependencies</returns>
        Public Overrides Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
            Dim dependencies As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            If inclInternal Then
                For Each stp As clsWriteStep In Steps
                    For Each dataItem As String In stp.Expression.GetDataItems()
                        Dim outOfScope As Boolean
                        Dim stage = mParent.GetDataStage(dataItem, Me, outOfScope)
                        If Not outOfScope AndAlso stage IsNot Nothing Then _
                            dependencies.Add(New clsProcessDataItemDependency(stage))
                    Next
                Next
            End If

            Return dependencies
        End Function

        ''' <summary>
        ''' Gets the arguments for the given write step.
        ''' </summary>
        ''' <param name="stp">The step for which arguments are required. If no action
        ''' is assigned to the step (eg. write stages are not associated with an
        ''' action) then an empty dictionary is returned, otherwise the registered
        ''' arguments for the step are populated into the returned dictionary</param>
        ''' <returns>A dictionary containing the arguments for the given step - the
        ''' argument values keyed mapped their corresponding name.</returns>
        ''' <exception cref="InvalidCastException">If the given step was not an
        ''' instance of <see cref="clsWriteStep"/></exception>
        ''' <exception cref="BluePrismException">If the expression in the given step
        ''' could not be evaluated or cast into the target element's data type.
        ''' </exception>
        Protected Overrides Function GetArgs(ByVal stp As IActionStep) _
         As Dictionary(Of String, String)
            Dim args As New Dictionary(Of String, String)
            Dim targetEl As clsApplicationElement =
             mParent.ApplicationDefinition.FindElement(stp.ElementId)

            Dim val As clsProcessValue = Nothing
            Dim errmsg As String = Nothing
            If Not clsExpression.EvaluateExpression(
             DirectCast(stp, clsWriteStep).Expression, val, Me, False, Nothing, errmsg) Then
                Throw New BluePrismException(
                 My.Resources.Resources.FailedToEvaluateExpressionInRow0Of12,
                 Steps.IndexOf(DirectCast(stp, clsStep)) + 1, DisplayIdentifer, errmsg)
            End If

            If targetEl.DataType <> val.DataType Then
                Try
                    val = val.CastInto(targetEl.DataType)
                Catch bce As BadCastException
                    Throw New BluePrismException(
                     My.Resources.Resources.FailedToCastResultOfExpressionInRow0Of1ToTheRequiredDataType2,
                     Steps.IndexOf(DirectCast(stp, clsStep)) + 1, DisplayIdentifer, bce.Message)
                End Try
            End If

            ' Most data types and convertible through their encoded value if they are
            ' convertible at all - password -> text is not and so has to be handled
            ' separately. AMI is expecting 'text' but the user has overridden the
            ' data type to accept a password, so we must get the plaintext from the
            ' password in order to ensure AMI understands what it is receiving.
            ' See bg-312 and its dependencies for more information and the reasoning
            ' for doing things this way.
            If targetEl.DefaultDataType = DataType.text AndAlso
             targetEl.DataType = DataType.password Then
                val = New clsProcessValue(CStr(val))
            End If
            args.Add("NewText", val.EncodedValue)
            For Each a In clsActionStep.GetArguments(stp, Me)
                args.Add(a.Key, a.Value)
            Next
            
            Return args

        End Function

        ''' <summary>
        ''' Executes this stage and updates the ref parameter with the ID of the
        ''' next stage if sucessful
        ''' </summary>
        ''' <param name="stageId">The ID of the stage - on successful execution, this
        ''' will be updated to contain the ID of the next stage to be executed.
        ''' </param>
        ''' <returns>A result indicating success or the nature of the failure.
        ''' </returns>
        Public Overrides Function Execute(ByRef stageId As Guid, logger As CompoundLoggingEngine) As StageResult
            Try
                WritePrologue(logger)

                'A list of arguments we will use to fake some 'inputs' information to
                'go in the logs.
                Dim inputs As New clsArgumentList

                'Execute each step...
                Dim rowNo As Integer = 0
                For Each n As clsWriteStep In Steps
                    rowNo += 1

                    ' Create the payload for the step - the target element and the
                    ' list of identifiers covering the element and its dynamic parameters
                    Dim p As Payload = GetPayload(n)

                    ' Create the arguments - for a write stage, there is a single
                    ' argument - a 'NewText' parameter with the evaluated expression
                    Dim args As Dictionary(Of String, String) = GetArgs(n)

                    'Run the query
                    RunQuery(logger, n, p, args)

                    ' Add the argument to the inputs for later logging
                    Dim el As clsApplicationElement = p.TargetElement
                    If el.DataType = DataType.password Then
                        inputs.Add(New clsArgument(el.Name,
                         New SafeString(args("NewText"))))
                    Else
                        inputs.Add(New clsArgument(el.Name,
                         New clsProcessValue(el.DataType, args("NewText"))))
                    End If

                Next

                Dim sErr As String = Nothing
                If Not mParent.UpdateNextStage(stageId, LinkType.OnSuccess, sErr) Then _
                 Return StageResult.InternalError(sErr)

                WriteEpilogue(logger, inputs)
                Return StageResult.OK

            Catch ex As Exception
                Return StageResult.InternalError(ex)

            End Try

        End Function

        Private Sub WritePrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.WritePrologue(info, Me)
        End Sub

        Private Sub WriteEpilogue(logger As CompoundLoggingEngine, ByVal inputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.WriteEpiLogue(info, Me, inputs)
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
            Return New clsWriteStep(Me, el)
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
            For Each n As clsWriteStep In Steps
                total += 1
            Next
        End Sub

    End Class

End Namespace
