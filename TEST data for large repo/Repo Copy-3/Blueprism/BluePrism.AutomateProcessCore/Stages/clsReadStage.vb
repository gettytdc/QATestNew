Imports System.Xml
Imports System.Text
Imports System.Runtime.Serialization
Imports BluePrism.ApplicationManager.AMI

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsReadStage
    ''' 
    ''' <summary>
    ''' The read stage reads data from application elements and stores the data in 
    ''' data items. 
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsReadStage
        Inherits clsAppStage

        ''' <summary>
        ''' Creates a new instance of the clsReadStage class and sets its parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of a Read stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsReadStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Read
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
            Dim deps As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            If inclInternal Then
                For Each stp As clsReadStep In Steps
                    If stp.Stage <> String.Empty Then
                        Dim outOfScope As Boolean
                        Dim stage = mParent.GetDataStage(stp.Stage, Me, outOfScope)
                        If Not outOfScope AndAlso stage IsNot Nothing Then _
                            deps.Add(New clsProcessDataItemDependency(stage))
                    End If
                Next
            End If

            Return deps
        End Function

        Public Overrides Function Execute(ByRef stageId As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing

            'A list of arguments we will use to fake some 'outputs' information to
            'go in the logs.
            Dim outputs As New clsArgumentList

            Try
                ReadPrologue(logger)

                'Execute each step...
                For Each readStep As clsReadStep In Steps
                    Dim payload = GetPayload(readStep)
                    Dim args = GetArgs(readStep)
                    Dim sResult = RunQuery(logger, readStep, payload, args)
                    Dim dt = If(readStep.ActionDataType = DataType.unknown, DataType.text, readStep.ActionDataType)

                    'Store the result...
                    Dim val As clsProcessValue
                    Select Case dt
                        Case DataType.collection, DataType.image, DataType.text
                            val = New clsProcessValue(dt, sResult)
                        Case Else
                            val = clsProcessValue.FromUIText(dt, sResult)
                    End Select

                    If Not mParent.StoreValueInDataItem(readStep.Stage, val, Me, sErr) Then _
                     Return StageResult.InternalError(sErr)

                    outputs.Add(New clsArgument(payload.TargetElement.Name, val))

                Next

                If Not mParent.UpdateNextStage(stageId, LinkType.OnSuccess, sErr) Then _
                 Return StageResult.InternalError(sErr)

                ReadEpilogue(logger, outputs)

                Return StageResult.OK

            Catch ex As Exception
                Return StageResult.InternalError(ex)

            End Try

        End Function

        Private Sub ReadPrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.ReadPrologue(info, Me)
        End Sub

        Private Sub ReadEpilogue(logger As CompoundLoggingEngine, ByVal outputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.ReadEpiLogue(info, Me, outputs)
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
            Return New clsReadStep(Me, el)
        End Function

        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList
            Dim errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            Dim rowNo As Integer = 1
            For Each rs As clsReadStep In Me.Steps
                errors.AddRange(rs.CheckForErrors(Me, 1, bAttemptRepair))
                rowNo += 1
            Next

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
            For Each n As clsReadStep In Steps
                total += 1
                If n.Action.RequiresFocus Then
                    globalcount += 1
                    stepdetails.AppendLine(String.Format(My.Resources.Resources.clsReadStage_Step0Action12, stepnum.ToString(), n.ActionName, n.ActionId))
                End If
                stepnum += 1
            Next
            If stepdetails.Length > 0 Then
                globaldetails.AppendLine("====" & GetName() & "====")
                globaldetails.AppendLine(GetNarrative())
                globaldetails.AppendLine(My.Resources.Resources.clsReadStage_StageID + GetStageID().ToString())
                globaldetails.Append(stepdetails.ToString())
            End If
        End Sub

    End Class
End Namespace
