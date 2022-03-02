Imports System.Drawing
Imports System.Runtime.Serialization
Imports System.Xml
Imports BluePrism.Core.Expressions

Namespace Stages

    ''' <summary>
    ''' This class of objects represent calculations. Calculations hold an expression
    ''' and the name of a stage to wich a result is stored.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsCalculationStage
        Inherits clsLinkableStage : Implements IExpressionHolder

        ''' <summary>
        ''' This holds the name of the data item in which the calculation result
        ''' should be stored in.
        ''' </summary>
        <DataMember>
        Private mStoreIn As String

        ''' <summary>
        ''' This holds the map type of the storein value (which is now always stage)
        ''' </summary>
        <DataMember>
        Private sMapping As MapType

        <DataMember>
        Private mExpression As BPExpression = BPExpression.Empty

        ''' <summary>
        ''' The name of the stage into which the result of this calculation is stored
        ''' </summary>
        Public Property StoreIn() As String
            Get
                Return mStoreIn
            End Get
            Set(ByVal value As String)
                mStoreIn = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the data stage that this calc stage is configured to store its
        ''' result in, or null if either the 'store in' field is not set, this stage
        ''' has no <see cref="Process">process</see> that it is assigned to or no
        ''' data/collection stage with the given name exists in the parent process.
        ''' </summary>
        Public ReadOnly Property StoreInStage() As clsDataStage
            Get
                If mStoreIn = "" Then Return Nothing
                Return mParent?.GetDataAndCollectionStagesByName(mStoreIn)
            End Get
        End Property

        ''' <summary>
        ''' The mapping that this stage uses. For a calc stage, this is always
        ''' <see cref="MapType.Stage"/>
        ''' </summary>
        Public ReadOnly Property Mapping() As MapType
            Get
                Return MapType.Stage
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the expression which is worked out by this calc stage
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
        ''' Creates a new instance of the clsCalculationStage class and sets its
        ''' parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of a calculation stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsCalculationStage(mParent)
        End Function

        ''' <summary>
        ''' Creates a deep copy of the calculation stage.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsCalculationStage = _
             DirectCast(MyBase.Clone(), clsCalculationStage)
            copy.mExpression = mExpression
            copy.mStoreIn = mStoreIn

            Return copy
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Calculation
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
                For Each dataItem As String In Expression.GetDataItems()
                    Dim outOfScope As Boolean
                    Dim stage = mParent.GetDataStage(dataItem, Me, outOfScope)
                    If Not outOfScope AndAlso stage IsNot Nothing Then _
                        deps.Add(New clsProcessDataItemDependency(stage))
                Next
                If StoreIn <> String.Empty Then
                    Dim outOfScope As Boolean
                    Dim stage = mParent.GetDataStage(StoreIn, Me, outOfScope)
                    If Not outOfScope AndAlso stage IsNot Nothing Then _
                        deps.Add(New clsProcessDataItemDependency(stage))
                End If
            End If

            Return deps
        End Function

        ''' <summary>
        ''' Executes this stage
        ''' </summary>
        ''' <param name="runStgId">The current run stage ID - this is set by the
        ''' stage</param>
        ''' <returns></returns>
        Public Overrides Function Execute(ByRef runStgId As Guid, logger As CompoundLoggingEngine) As StageResult

            CalculationPrologue(logger)

            Dim sErr As String = Nothing

            'Evaluate the expression...
            Dim res As clsProcessValue = Nothing

            If Not clsExpression.EvaluateExpression(mExpression, res, Me, False, Nothing, sErr) Then
                Return StageResult.InternalError(
                 My.Resources.Resources.clsCalculationStage_FailedToEvaluateExpression01, mExpression.LocalForm, sErr)
            End If

            CalculationEpilogue(logger, res)

            If mStoreIn Is Nothing OrElse Not mParent.StoreValueInDataItem(mStoreIn, res, Me, sErr) Then
                Return StageResult.InternalError(String.Format(My.Resources.Resources.clsCalculationStage_CouldNotStoreCalculationResult0, sErr))
            End If

            'Move to the next stage...
            If Not mParent.UpdateNextStage(runStgId, LinkType.OnSuccess, sErr) Then
                Return StageResult.InternalError(sErr)
            End If

            Return StageResult.OK

        End Function

        Private Sub CalculationPrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.CalculationPrologue(info, Me)
        End Sub

        Private Sub CalculationEpilogue(logger As CompoundLoggingEngine, ByVal objCalcRes As clsProcessValue)
            Dim info = GetLogInfo()
            logger.CalculationEpiLogue(info, Me, objCalcRes)
        End Sub

        ''' <summary>
        ''' Loads the calculation within this stage from the given XML element
        ''' </summary>
        ''' <param name="el">The XML element representing the stage that this calc
        ''' data is stored within</param>
        Public Overrides Sub FromXML(ByVal el As XmlElement)
            MyBase.FromXML(el)
            For Each child As XmlElement In el
                If child.Name <> "calculation" Then Continue For

                mStoreIn = child.GetAttribute("stage")
                mExpression = _
                 BPExpression.FromNormalised(child.GetAttribute("expression"))
            Next
        End Sub

        ''' <summary>
        ''' Writes the data encompassing this calculation stage to the given XML
        ''' document.
        ''' </summary>
        ''' <param name="doc">The document to write this calculation to</param>
        ''' <param name="stgEl">The stage element under which the XML representing
        ''' this calculation should be written</param>
        ''' <param name="justSelection">True to limit the XML writing to selected
        ''' stages only - this determines whether the link is written to the
        ''' resultant XML or not.</param>
        Public Overrides Sub ToXml(ByVal doc As XmlDocument, _
         ByVal stgEl As XmlElement, ByVal justSelection As Boolean)
            MyBase.ToXml(doc, stgEl, justSelection)

            Dim calcEl As XmlElement = doc.CreateElement("calculation")
            calcEl.SetAttribute("expression", mExpression.NormalForm)
            calcEl.SetAttribute("stage", mStoreIn)
            stgEl.AppendChild(calcEl)
        End Sub

        ''' <summary>
        ''' Validates the calculation stage's expression, store-in location,
        ''' scope, datatype compatibility, etc.
        ''' </summary>
        ''' <returns>Returns a list containing any errors found.</returns>
        ''' <remarks>Useful alternative to CheckForErrors because this method
        ''' only returns errors about the calculation aspect, and not peripheral
        ''' problems such as missing links or bad stage names, etc.</remarks>
        Public Function CheckCalculation() As ValidationErrorList
            Dim errors As New ValidationErrorList()

            ValidateExprAndStoreIn(mExpression, mStoreIn, Nothing, mParent, Me, errors)

            Return errors
        End Function

        ''' <summary>
        ''' Validate the combination of an expression and a 'storein' - broken out
        ''' separately so it can also be used by the multiple calculation stage.
        ''' </summary>
        ''' <param name="expression">The expression</param>
        ''' <param name="storein">The destination to store in</param>
        ''' <param name="loc">Extra text to describe the location of the error (use
        ''' this to identify multi-calc stage steps) or Nothing if it's not required.
        ''' </param>
        ''' <param name="parent">The parent process</param>
        ''' <param name="scopeStage">The stage to use for scope checking.</param>
        ''' <param name="errors">A List(Of clsProcess.ValidateResult) that will be
        ''' added to accordingly.</param>
        Friend Shared Sub ValidateExprAndStoreIn(ByVal expression As BPExpression, ByVal storein As String, ByVal loc As String, ByVal parent As clsProcess, ByVal scopeStage As clsProcessStage, ByVal errors As ValidationErrorList)

            Dim sErr As String = Nothing
            If loc Is Nothing Then
                loc = String.Empty
            Else
                loc = String.Format(My.Resources.Resources.clsCalculationStage_On0, loc)
            End If

            'Validate expression
            Dim res As clsProcessValue = Nothing
            Dim expressionInfo As clsExpressionInfo = Nothing
            errors.AddRange(clsExpression.CheckExpressionForErrors(expression, _
             scopeStage, DataType.unknown, "", expressionInfo, res))

            'Validate storage locations for calculation results...
            Dim dt As DataType = DataType.unknown
            If res IsNot Nothing Then dt = res.DataType
            errors.AddRange(parent.CheckStoreInForErrors(storein, dt, scopeStage, loc))

        End Sub

        ''' <summary>
        ''' Checks this stage for errors, optionally attempting to repair them
        ''' </summary>
        ''' <param name="attemptRepair">True to attempt to repair the errors found,
        ''' false to report all the errors without repairing.</param>
        ''' <param name="skipObj">Not used by this implementation of the method
        ''' </param>
        ''' <returns>A list of validation errors discovered within this stage
        ''' </returns>
        Public Overrides Function CheckForErrors(
         ByVal attemptRepair As Boolean, ByVal skipObj As Boolean) _
         As ValidationErrorList
            Dim errors As ValidationErrorList =
             MyBase.CheckForErrors(attemptRepair, skipObj)
            errors.AddRange(CheckCalculation())
            Return errors
        End Function


        Friend Shared Sub DrawHexagon(ByVal r As IRender, ByVal b As RectangleF)
            Dim p(5) As PointF
            With b
                p(0).X = .Left
                p(0).Y = .Top + (.Height / 2)

                p(1).X = .Left + (.Width / 8)
                p(1).Y = .Bottom

                p(2).X = .Right - (.Width / 8)
                p(2).Y = .Bottom

                p(3).X = .Right
                p(3).Y = .Top + (.Height / 2)

                p(4).X = .Right - (.Width / 8)
                p(4).Y = .Top

                p(5).X = .Left + (.Width / 8)
                p(5).Y = .Top
            End With

            r.FillPolygon(p)
            r.DrawPolygon(p)
        End Sub

    End Class
End Namespace
