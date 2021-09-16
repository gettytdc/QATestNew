Imports System.Xml
Imports BluePrism.Core.Expressions
Imports System.Runtime.Serialization

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsMultipleCalculationStage
    ''' 
    ''' <summary>
    ''' This class represents multiple calculations. Multiple Calculations 
    ''' have a list string variables in which an expressions are held, as well as a
    ''' variable which stores a reference to a data item (by name) that specifies 
    ''' where to store the result of each expression.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsMultipleCalculationStage
        Inherits clsLinkableStage

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="Parent">The parent process</param>
        Public Sub New(ByVal Parent As clsProcess)
            MyBase.New(Parent)
        End Sub

        ''' <summary>
        ''' Creates a clsMultipleCalculationStage for the purpose of cloning.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsMultipleCalculationStage(mParent)
        End Function

        ''' <summary>
        ''' Executes the stage.
        ''' </summary>
        ''' <param name="gRunStageID"></param>
        ''' <returns>A Result object</returns>
        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            MultipleCalculationPrologue(logger)

            Dim sErr As String = Nothing
            Dim results As New clsArgumentList()
            Dim i As Integer = 1
            For Each tp As clsCalcStep In Steps
                Dim res As clsProcessValue = Nothing
                If Not clsExpression.EvaluateExpression(tp.Expression, res, Me, False, Nothing, sErr) Then
                    Return StageResult.InternalError(
                     My.Resources.Resources.clsMultipleCalculationStage_FailedToEvaluateExpression01, tp.Expression.LocalForm, sErr)
                End If

                'Store the result...
                If Not mParent.StoreValueInDataItem(tp.StoreIn, res, Me, sErr) Then
                    Return StageResult.InternalError(My.Resources.Resources.clsMultipleCalculationStage_CouldNotStoreCalculationResult0, sErr)
                End If
                results.Add(New clsArgument("step" & i, res))
                i += 1
            Next

            MultipleCalculationEpilogue(logger, results)

            'Move to the next stage...
            If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                Return StageResult.InternalError(sErr)
            End If

            Return StageResult.OK
        End Function

        Private Sub MultipleCalculationPrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.MultipleCalculationPrologue(info, Me)
        End Sub

        Private Sub MultipleCalculationEpilogue(logger As CompoundLoggingEngine, ByVal objResults As clsArgumentList)
            Dim info = GetLogInfo()
            logger.MultipleCalculationEpiLogue(info, Me, objResults)
        End Sub

        ''' <summary>
        ''' Returns the stagetype of the stage
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.MultipleCalculation
            End Get
        End Property

        ''' <summary>
        ''' Returns items referred to by this stage, currently only things defined
        ''' within the process (e.g. data items).
        ''' </summary>
        ''' <param name="inclInternal">Indicates internal references required</param>
        ''' <returns>List of dependencies</returns>
        Public Overrides Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
            Dim deps As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            If inclInternal Then
                For Each stp As clsCalcStep In mSteps
                    For Each dataItem As String In stp.Expression.GetDataItems()
                        Dim outOfScope As Boolean
                        Dim stage = mParent.GetDataStage(dataItem, Me, outOfScope)
                        If Not outOfScope AndAlso stage IsNot Nothing Then _
                            deps.Add(New clsProcessDataItemDependency(stage))
                    Next
                    If stp.StoreIn <> String.Empty Then
                        Dim outOfScope As Boolean
                        Dim stage = mParent.GetDataStage(stp.StoreIn, Me, outOfScope)
                        If Not outOfScope AndAlso stage IsNot Nothing Then _
                            deps.Add(New clsProcessDataItemDependency(stage))
                    End If
                Next
            End If

            Return deps
        End Function

        ''' <summary>
        ''' The calculation steps to execute
        ''' </summary>
        Public ReadOnly Property Steps() As List(Of clsCalcStep)
            Get
                Return mSteps
            End Get
        End Property
        <DataMember>
        Private mSteps As List(Of clsCalcStep) = New List(Of clsCalcStep)

        ''' <summary>
        ''' Deep-clones the stage, ensuring that all child elements are cloned too.
        ''' </summary>
        ''' <returns>A deep clone of this stage</returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsMultipleCalculationStage = CType(MyBase.Clone, clsMultipleCalculationStage)
            For Each st As clsCalcStep In Me.Steps
                copy.Steps.Add(New clsCalcStep(copy, st))
            Next
            Return copy
        End Function

        ''' <summary>
        ''' Writes the stage data to xml
        ''' </summary>
        ''' <param name="doc"></param>
        ''' <param name="stgEl"></param>
        ''' <param name="selectedOnly"></param>
        Public Overrides Sub ToXml(ByVal doc As XmlDocument,
         ByVal stgEl As XmlElement, ByVal selectedOnly As Boolean)
            MyBase.ToXml(doc, stgEl, selectedOnly)

            Dim stepsEl As XmlElement = doc.CreateElement("steps")
            For Each s As clsCalcStep In Steps
                Dim calcEl As XmlElement = doc.CreateElement("calculation")
                calcEl.SetAttribute("expression", s.Expression.NormalForm)
                calcEl.SetAttribute("stage", s.StoreIn)
                stepsEl.AppendChild(calcEl)
            Next
            stgEl.AppendChild(stepsEl)
        End Sub

        ''' <summary>
        ''' Reads the stage data from an xml element.
        ''' </summary>
        ''' <param name="stgEl">The XML element representing the stage</param>
        Public Overrides Sub FromXML(ByVal stgEl As System.Xml.XmlElement)
            MyBase.FromXML(stgEl)

            Steps.Clear()

            For Each stepsEl As Xml.XmlElement In stgEl
                If stepsEl.Name <> "steps" Then Continue For

                For Each stepEl As Xml.XmlElement In stepsEl
                    If stepEl.Name <> "calculation" Then Continue For

                    Dim st As New clsCalcStep(Me)
                    st.Expression = BPExpression.FromNormalised(
                     stepEl.GetAttribute("expression"))
                    st.StoreIn = stepEl.GetAttribute("stage")

                    Steps.Add(st)
                Next
            Next

        End Sub

        ''' <summary>
        ''' Validates the multiple calculation stage's expressions, store-in locations,
        ''' scope, datatype compatibility, etc.
        ''' </summary>
        ''' <param name="bAttemptRepair">Boolean indicating whether to attempt repairing 
        ''' the errors</param>
        ''' <param name="SkipObjects">If True, checks for installed Business Objects
        ''' and similar are skipped.</param>
        ''' <returns>Returns a list containing any errors found.</returns>
        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList

            Dim errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)
            Dim index As Integer = 1
            For Each calcStep As clsCalcStep In Me.Steps
                Dim desc As String = String.Format(
                 My.Resources.Resources.clsMultipleCalculationStage_Step0OfStage1OnPage2, index, Me.Name, SubSheet.Name)

                'Get calculation details
                clsCalculationStage.ValidateExprAndStoreIn(
                 calcStep.Expression, calcStep.StoreIn, desc, mParent, Me, errors)

                index += 1
            Next
            Return errors
        End Function

    End Class
End Namespace
