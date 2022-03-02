Imports System.Runtime.Serialization

<Serializable, DataContract([Namespace]:="bp"), KnownType(NameOf(clsProcessBreakpoint.GetAllKnownTypes))>
Public Class clsProcessBreakpoint
    Implements ICloneable

    Protected Friend Shared Function GetAllKnownTypes() As IEnumerable(Of Type)
        Return clsProcessStage.GetAllKnownTypes()
    End Function

    ''' <summary>
    ''' Create a new breakpoint.
    ''' </summary>
    ''' <param name="parentStage">The parent stage of the breakpoint. Sometimes a
    ''' breakpoint is owned by a stage, i.e. permanently attached to it. Other
    ''' times, such as when the breakpoint is caused by a handled exception, the
    ''' breakpoint is created on the fly, but is still related to a specific
    ''' stage.</param>
    Public Sub New(ByVal parentStage As clsProcessStage)
        mParentStage = parentStage
    End Sub

    <DataMember>
    Private mParentStage As clsProcessStage

    <DataMember>
    Private mCondition As String = ""

    <DataMember>
    Private mBreakPointType As BreakEvents
    ''' <summary>
    ''' The different events in response to which a process can break. When used on
    ''' a breakpoint itself, these values can be combined as a bitmask, although
    ''' only some combinations are valid. When used to signify the type of breakpoint
    ''' that has actually occurred, only one will ever be present.
    ''' </summary>
    Public Enum BreakEvents
        ''' <summary>
        ''' Empty place holder.
        ''' </summary>
        None = 0
        ''' <summary>
        ''' Breaks when a certain condition is met (ie expression evaluates to True).
        ''' </summary>
        WhenConditionMet = 1
        ''' <summary>
        ''' Breaks every time a data item value is accessed.
        ''' </summary>
        WhenDataValueRead = 2
        ''' <summary>
        ''' Breaks every time a data item value is changed.
        ''' </summary>
        WhenDataValueChanged = 4
        ''' <summary>
        ''' A breakpoint automatically created and raised when a handled exception
        ''' occurs.
        ''' </summary>
        HandledException = 8
        ''' <summary>
        ''' A breakpoint automatically removed as soon as it is encountered. Used for
        ''' "Run to this stage" option
        ''' </summary>
        Transient = 16
    End Enum

    ''' <summary>
    ''' Returns True if this is a transient breakpoint (i.e. created for the "Run to
    ''' this stage" debug option).
    ''' </summary>
    Public ReadOnly Property IsTransient() As Boolean
        Get
            Return ((BreakPointType And BreakEvents.Transient) <> 0)
        End Get
    End Property

    ''' <summary>
    ''' The condition under which the breakpoint will fire. If this is an empty
    ''' string, no condition is set, i.e. the breakpoint will fire every time.
    ''' </summary>
    Public Property Condition() As String
        Get
            Return mCondition
        End Get
        Set(ByVal value As String)
            mCondition = value
        End Set
    End Property


    ''' <summary>
    ''' Determines if the condition on this breakpoint is met. If no condition is
    ''' set then will return true. This way, a single call can be made to determine
    ''' if the breakpoint should fire, rather than checking for a null condition
    ''' separately.
    ''' </summary>
    ''' <param name="Process">The process object to use to evaluate expression.</param>
    ''' <returns>Returns True if expression is blank or evaluates to True;
    ''' false otherwise.</returns>
    Public Function IsConditionMet(ByVal process As clsProcess) As Boolean

        If mCondition = "" Then Return True
        Dim objValue As clsProcessValue = Nothing
        Dim sErr As String = Nothing
        If Not clsExpression.EvaluateExpression(mCondition, objValue, OwnerStage, False, Nothing, sErr) Then
            Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsProcessBreakpoint_FailedToEvaluateBreakpointCondition0, sErr))
        End If
        If objValue.DataType <> DataType.flag Then
            Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsProcessBreakpoint_ConditionInsideBreakpointExpressionEvaluatesToA0DataItemOnly1ResultsAreValid, clsProcessDataTypes.GetFriendlyName(objValue.DataType), clsProcessDataTypes.GetFriendlyName(DataType.flag)))
        End If
        Return objValue.Equals(New clsProcessValue(DataType.flag, "True", False))

    End Function


    ''' <summary>
    ''' The type of breakpoint. Can be multiple values.
    ''' </summary>
    Public Property BreakPointType() As BreakEvents
        Get
            Return mBreakPointType
        End Get
        Set(ByVal value As BreakEvents)
            mBreakPointType = value
        End Set
    End Property

    ''' <summary>
    ''' The stage owning this breakpoint.
    ''' </summary>
    Public ReadOnly Property OwnerStage() As clsProcessStage
        Get
            Return mParentStage
        End Get
    End Property

    ''' <summary>
    ''' Produces a deep copy of this breakpoint and returns the result as a
    ''' clsProcessBreakpoint object.
    ''' </summary>
    ''' <returns>See summary.</returns>
    Public Function Clone() As Object Implements System.ICloneable.Clone
        Dim b As New clsProcessBreakpoint(OwnerStage)
        b.Condition = mCondition
        b.BreakPointType = mBreakPointType
        Return b
    End Function


    ''' <summary>
    ''' Serialises this object as XML, returning an xmlelement. It is up to the
    ''' caller to append the returned element to the document, if desired.
    ''' </summary>
    ''' <param name="doc">The document to which this xml element will belong.</param>
    ''' <returns>An XmlElement representing this breakpoint (which, unlike every
    ''' other implementation of ToXml throughout the product, is not yet appended to
    ''' the document, or null if this breakpoint is
    ''' <see cref="BreakEvents.Transient">transient</see> and thus should not be
    ''' serialised to XML.</returns>
    ''' <remarks>Transient breakpoints are <em>not</em> serialized to XML by this
    ''' method.</remarks>
    Public Function ToXML(ByVal doc As Xml.XmlDocument) As Xml.XmlElement
        If Me.IsTransient Then Return Nothing

        Dim e As Xml.XmlElement = doc.CreateElement("breakpoint")
        e.SetAttribute("type", CStr(CInt(mBreakPointType)))
        Dim condition As Xml.XmlElement = doc.CreateElement("condition")
        condition.AppendChild(doc.CreateTextNode(mCondition))
        e.AppendChild(condition)
        Return e
    End Function


    ''' <summary>
    ''' Deserialises an xml element and returns the breakpoint object represented
    ''' by that xml. The xml element is expected to have name "breakpoint".
    ''' </summary>
    ''' <param name="breakpointElement">The element that represents the breakpoint</param>
    ''' <param name="parentStage">The stage which is to own this breakpoint.
    ''' Must not be null.</param>
    ''' <returns>Breakpoint object, unless bad parameters are passed.</returns>
    Public Shared Function FromXML(ByVal breakpointElement As Xml.XmlElement, ByVal parentStage As clsProcessStage) As clsProcessBreakpoint
        If Not (breakpointElement Is Nothing OrElse parentStage Is Nothing) Then
            Select Case breakpointElement.Name
                Case "Breakpoint", "breakpoint"
                    Dim processBreakpoint As New clsProcessBreakpoint(parentStage)

                    'Get breakpoint type attribute
                    Dim typeAttribute As String = Nothing
                    If breakpointElement.HasAttribute("type") Then
                        typeAttribute = breakpointElement.GetAttribute("type")
                    ElseIf breakpointElement.HasAttribute("Type") Then
                        typeAttribute = breakpointElement.GetAttribute("Type")
                    End If

                    If typeAttribute IsNot Nothing Then
                        processBreakpoint.BreakPointType = CType(typeAttribute, clsProcessBreakpoint.BreakEvents)
                    End If

                    'get condition
                    For Each conditionElement As Xml.XmlElement In breakpointElement.ChildNodes
                        Select Case conditionElement.Name
                            Case "Condition", "condition"
                                processBreakpoint.Condition = conditionElement.InnerText
                        End Select
                    Next

                    Return processBreakpoint
            End Select
        End If

        Throw New InvalidOperationException("Bad parameters passed to clsProcessBreakpoint.FromXML")
    End Function

End Class
