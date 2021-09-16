Imports System.Xml
Imports BluePrism.Server.Domain.Models
Imports BluePrism.AMI
Imports ComparisonTypes = BluePrism.AMI.clsAMI.ComparisonTypes
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.BPCoreLib

Public Class clsWaitChoice
    Inherits clsChoice
    Implements IActionStep

#Region " Member Variables "

    ' The action type associated with this wait choice
    Private mAction As clsActionTypeInfo

    ' The list of parameters associated with this wait choice
    Private mParams As List(Of clsApplicationElementParameter)

    ' The argument values set within this choice
    Private mArgumentValues As Dictionary(Of String, String)

    ' The comparison type for this choice
    Private mComparisonType As ComparisonTypes = ComparisonTypes.Equal

    ' The element ID for this choice
    Private mElementID As Guid

    ' The expected reply for this choice
    Private mExpectedReply As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new wait choice owned by the given wait start stage
    ''' </summary>
    Public Sub New(ByVal owner As clsWaitStartStage)
        MyBase.New(owner)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' See interface for documentation.
    ''' </summary>
    ''' <exception cref="InvalidTypeException">When setting, if trying to set an
    ''' action which is not a <see cref="clsConditionTypeInfo"/> instance</exception>
    Private Property Action() As clsActionTypeInfo Implements IActionStep.Action
        Get
            Return mAction
        End Get
        Set(ByVal value As clsActionTypeInfo)
            If TryCast(value, clsConditionTypeInfo) Is Nothing Then Throw New _
             InvalidTypeException(My.Resources.Resources.clsWaitChoice_AttemptToSetANonConditionActionOnAWaitChoice)
            mAction = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the action ID for this step or null if no action has been set in this
    ''' step.
    ''' </summary>
    Private ReadOnly Property ActionId() As String Implements IActionStep.ActionId
        Get
            If mAction Is Nothing Then Return Nothing
            Return mAction.ID
        End Get
    End Property

    ''' <summary>
    ''' The condition to check. This is really just a typesafe wrapper around
    ''' the Action property.
    ''' </summary>
    Public Property Condition() As clsConditionTypeInfo
        Get
            Return CType(mAction, clsConditionTypeInfo)
        End Get
        Set(ByVal value As clsConditionTypeInfo)
            mAction = value
        End Set
    End Property

    ''' <summary>
    ''' See interface for documentation.
    ''' </summary>
    Public ReadOnly Property ArgumentValues() As IDictionary(Of String, String) _
     Implements IActionStep.ArgumentValues
        Get
            If mArgumentValues Is Nothing Then _
             mArgumentValues = New Dictionary(Of String, String)
            Return mArgumentValues
        End Get
    End Property

    ''' <summary>
    ''' Provides access to the waitchoice's expected reply.
    ''' </summary>
    Public Property ExpectedReply() As String
        Get
            Return mExpectedReply
        End Get
        Set(ByVal value As String)
            mExpectedReply = value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the elementID of the waitchoice
    ''' </summary>
    Public Property ElementID() As Guid Implements IActionStep.ElementId
        Get
            Return mElementID
        End Get
        Set(ByVal value As Guid)
            mElementID = value
        End Set
    End Property

    ''' <summary>
    ''' A List of clsProcessParameter objects that go with the element ID. This will
    ''' be an empty list unless the Application Definition specifies that the element
    ''' is 'parameterised' in some way.
    ''' </summary>
    Public ReadOnly Property Parameters() As List(Of clsApplicationElementParameter) _
     Implements IActionStep.Parameters
        Get
            If mParams Is Nothing Then _
             mParams = New List(Of clsApplicationElementParameter)
            Return mParams
        End Get
    End Property

    ''' <summary>
    ''' The type of comparison to be applied to this
    ''' wait choice.
    ''' </summary>
    ''' <remarks>Defaults to Equal.</remarks>
    Public Property ComparisonType() As clsAMI.ComparisonTypes
        Get
            Return mComparisonType
        End Get
        Set(ByVal value As ComparisonTypes)
            mComparisonType = value
        End Set
    End Property

    Public ReadOnly Property OutputValues As IDictionary(Of String, String) Implements IActionStep.OutputValues
        Get
            Return New Dictionary(Of String, String)
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Determines which comparison types are available for the specified application
    ''' manager condition.
    ''' </summary>
    ''' <param name="cond">The condition of interest.</param>
    ''' <returns>Returns an array of the comparison types available.</returns>
    Public Shared Function GetAllowedComparisonTypes(
     ByVal cond As clsConditionTypeInfo) As ICollection(Of ComparisonTypes)
        'Get the data type of the chosen condition.
        Dim dt As DataType
        If Not clsProcessDataTypes.TryParse(cond.DataType, dt) Then dt = DataType.text
        Return clsAMI.GetAllowedComparisonTypes(dt.ToString())
    End Function

#End Region

    ''' <summary>
    ''' Converts the WaitChoice into XML
    ''' </summary>
    ''' <param name="doc">The XML document to which this choice is to be written.
    ''' </param>
    ''' <param name="el">The element upon which this data should be added</param>
    ''' <param name="bSelectionOnly">True to only write to XML any selected objects
    ''' in the process; False to write the whole choice regardless of selection.
    ''' </param>
    Friend Overrides Sub ToXML(ByVal doc As XmlDocument, ByVal el As XmlElement,
     ByVal bSelectionOnly As Boolean)
        Dim child As XmlElement = Nothing

        el.SetAttribute("reply", Me.ExpectedReply)

        BPUtil.AppendTextElement(el, "name", Name)
        BPUtil.AppendTextElement(el, "distance", XmlConvert.ToString(Distance))

        Dim proc As clsProcess = Me.Process
        ' If there's a link, and it links to a selected stage or this isn't just
        ' a selectionOnly XML write the link to the XML
        If LinkTo <> Guid.Empty AndAlso proc IsNot Nothing AndAlso
         (Not bSelectionOnly OrElse proc.IsStageSelected(LinkTo)) Then _
         BPUtil.AppendTextElement(el, "ontrue", LinkTo.ToString())

        'Add target element
        If ElementID <> Guid.Empty Then
            child = doc.CreateElement("element")
            child.SetAttribute("id", ElementID.ToString())
            'Add parameters
            For Each p As clsApplicationElementParameter In Me.Parameters
                child.AppendChild(p.ToXML(doc))
            Next
            el.AppendChild(child)
        End If

        'Add Condition
        If Condition IsNot Nothing Then
            Dim condel As XmlElement = doc.CreateElement("condition")
            BPUtil.AppendTextElement(condel, "id", Condition.ID)
            el.AppendChild(condel)

            'Add arguments
            clsActionStep.ArgumentsToXML(Me, doc, condel)

        End If

        'Add comparison type
        BPUtil.AppendTextElement(el, "comparetype", ComparisonType.ToString())

    End Sub

    ''' <summary>
    ''' Clones this wait choice, pointing to the same process (by reference), though
    ''' it's disassociated from the parent wait start stage.
    ''' </summary>
    ''' <returns>A deep clone of this wait choice</returns>
    Public Overrides Function Clone() As clsChoice
        Dim copy As clsWaitChoice = DirectCast(MyBase.Clone(), clsWaitChoice)

        ' Reset to null to force the copy to create a new parameters list
        copy.mParams = Nothing
        For Each p As clsApplicationElementParameter In Parameters
            copy.Parameters.Add(p.Clone())
        Next

        ' Again, reset to null to force a new collection to be generated
        copy.mArgumentValues = Nothing
        For Each a As String In ArgumentValues.Keys
            copy.ArgumentValues(a) = ArgumentValues(a)
        Next

        ' The condition type info need not be cloned even though it's a ref type;
        ' they are statically defined and immutable
        Return copy

    End Function

    ''' <summary>
    ''' Converts XML into a wait choice
    ''' </summary>
    ''' <param name="ChoiceElement"></param>
    Friend Overrides Sub FromXML(ByVal ChoiceElement As System.Xml.XmlElement)

        Me.ExpectedReply = ChoiceElement.GetAttribute("reply")

        For Each eChoiceData As Xml.XmlElement In ChoiceElement.ChildNodes
            Select Case eChoiceData.Name
                Case "name"
                    Me.Name = eChoiceData.InnerText
                Case "distance"
                    Me.Distance = clsProcess.ReadXmlSingle(eChoiceData)
                Case "element"
                    ElementID = New Guid(eChoiceData.GetAttribute("id"))
                    For Each grandchild As XmlElement In eChoiceData.ChildNodes
                        Select Case grandchild.Name
                            Case "elementparameter"
                                Me.Parameters.Add(clsApplicationElementParameter.FromXML(grandchild))
                            Case "input"
                                'Backwards compatibility for when we used to use process
                                'parameters instead of element parameters. The change occured
                                'because we added wildcard matching. Once legacy processes have
                                'been opened and saved again, this bit of code can be removed
                                Dim p As clsProcessParameter = clsProcessParameter.FromXML(grandchild)
                                Me.Parameters.Add(New clsApplicationElementParameter(p.Name, p.GetDataType, p.Expression, clsAMI.ComparisonTypes.Equal))
                        End Select
                    Next
                Case "ontrue"
                    LinkTo = New Guid(eChoiceData.InnerText)
                Case "condition"
                    For Each grandchild As XmlElement In eChoiceData.ChildNodes
                        Select Case grandchild.Name
                            Case "id"
                                Me.Condition = clsAMI.GetConditionTypeInfo(grandchild.FirstChild.Value)
                                If Me.Condition Is Nothing Then
                                    Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWaitChoice_AMIDidNotRecogniseTheConditionType0, grandchild.FirstChild.Value))
                                End If
                            Case "arguments"
                                clsActionStep.AddArgumentsFromXML(Me, grandchild)

                        End Select
                    Next
                Case "comparetype"
                    ComparisonType = clsEnum(Of ComparisonTypes).Parse(eChoiceData.InnerText)
            End Select
        Next
    End Sub

End Class
