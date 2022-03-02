Imports System.Xml
Imports BluePrism.Core.Expressions
Imports System.Runtime.Serialization

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsAlertStage
    ''' 
    ''' <summary>
    ''' The alert stage allows alerts to be raised.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsAlertStage
        Inherits clsLinkableStage : Implements IExpressionHolder

        <DataMember>
        Private mExpression As BPExpression = BPExpression.Empty

        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsAlertStage(mParent)
        End Function

        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsAlertStage = CType(MyBase.Clone(), clsAlertStage)
            copy.mExpression = mExpression
            Return copy
        End Function

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            AlertPrologue(logger)

            Dim sErr As String = Nothing

            'Evaluate the decision, and move to the
            'next appropriate stage...
            Dim val As clsProcessValue = Nothing
            'Evaluate the expression...
            If Not clsExpression.EvaluateExpression(mExpression, val, Me, False, Nothing, sErr) Then _
             Return StageResult.InternalError(sErr)

            'The expression should have returned the correct data
            'type, and consequently the value should be either True
            'or False, so trap the error now if it isn't...
            If val.DataType <> DataType.text Then _
             Return StageResult.InternalError("Alert did not result in a text message")

            AlertEpilogue(logger, CStr(val))

            'Move to the next stage...
            If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then _
             Return StageResult.InternalError(sErr)

            mParent.RaiseStageAlert(Me, CStr(val))

            Return StageResult.OK

        End Function

        Private Sub AlertPrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.AlertPrologue(info, Me)
        End Sub

        Private Sub AlertEpilogue(logger As CompoundLoggingEngine, ByVal sResult As String)
            Dim info = GetLogInfo()
            logger.AlertEpiLogue(info, Me, sResult)
        End Sub

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Alert
            End Get
        End Property

        ''' <summary>
        ''' Returns items referred to by this stage, curently only internally defined
        ''' things are relevant.
        ''' </summary>
        ''' <param name="inclInternal">Indicates internal references required</param>
        ''' <returns>List of dependencies</returns>
        Public Overrides Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
            Dim deps As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            If inclInternal Then
                For Each dataItem As String In Expression.GetDataItems()
                    Dim outOfScope As Boolean
                    Dim stage = mParent.GetDataStage(dataItem, Me, outOfScope)
                    If Not outOfScope AndAlso stage IsNot Nothing Then _
                        deps.Add(New clsProcessDataItemDependency(stage))
                Next
            End If

            Return deps
        End Function

        ''' <summary>
        ''' Gets or sets the expression associated with this alert stage.
        ''' </summary>
        Public Property Expression() As BPExpression _
         Implements IExpressionHolder.Expression
            Get
                Return mExpression
            End Get
            Set(ByVal value As BPExpression)
                If value Is Nothing Then value = BPExpression.Empty
                mExpression = value
            End Set
        End Property

        ''' <summary>
        ''' Initialises this alert stage from the given XML element
        ''' </summary>
        ''' <param name="element">The element representing the alert stage from which
        ''' this object's data should be drawn.</param>
        Public Overrides Sub FromXML(ByVal element As XmlElement)
            MyBase.FromXML(element)
            For Each child As XmlElement In element.ChildNodes
                If child.Name <> "alert" Then Continue For

                Me.Expression =
                 BPExpression.FromNormalised(child.GetAttribute("expression"))
                Exit For
            Next
        End Sub

        ''' <summary>
        ''' Serializes this alert stage data into XML.
        ''' </summary>
        ''' <param name="doc">The XML document to which this stage should be written.
        ''' </param>
        ''' <param name="stgEl">The element representing the stage which should be
        ''' populated by this method.</param>
        ''' <param name="bSelectionOnly">True to indicate that only selected stages
        ''' are being written. This may determine that a link is not written to the
        ''' XML if the stage it is linked to is not currently selected.</param>
        Public Overrides Sub ToXml(ByVal doc As XmlDocument,
         ByVal stgEl As XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(doc, stgEl, bSelectionOnly)
            Dim elem As XmlElement = doc.CreateElement("alert")
            elem.SetAttribute("expression", Expression.NormalForm)
            stgEl.AppendChild(elem)
        End Sub

    End Class

End Namespace
