Imports System.Xml
Imports BluePrism.AMI
Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore.Stages
Imports System.Runtime.Serialization

<Serializable, DataContract([Namespace]:="bp")>
Public Class clsWriteStep
    Inherits clsStep : Implements IActionStep, IExpressionHolder

    ' The expression that is used as the 'write value' for this step
    <DataMember>
    Private mExpression As BPExpression = BPExpression.Empty

    ''' <summary>
    ''' Creates a new empty write step
    ''' </summary>
    Public Sub New(ByVal stg As clsWriteStage)
        Me.New(stg, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new write step initialised from the given XML element.
    ''' </summary>
    ''' <param name="el">The element from which to draw the write step data. If this
    ''' is null, an empty write step is created.</param>
    Public Sub New(ByVal stg As clsWriteStage, ByVal el As XmlElement)
        MyBase.New(stg)
        If el IsNot Nothing Then FromXML(el)
    End Sub

    ''' <summary>
    ''' Gets the action ID for this step - for write steps, there is only one
    ''' action ID - namely <see cref="clsAMI.WriteActionID"/>, ie. "Write"
    ''' </summary>
    Public Overrides ReadOnly Property ActionId() As String
        Get
            Return clsAMI.WriteActionID
        End Get
    End Property

    ''' <summary>
    ''' The expression associated with this step. This can never be null
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
    ''' Populates this write step from the given xml
    ''' </summary>
    ''' <param name="e">The element from which this step's data should be drawn.
    ''' </param>
    Public Overrides Sub FromXML(ByVal e As XmlElement)

        If e.Name <> "step" Then Return

        Dim elId As Guid
        Dim Params As New List(Of clsApplicationElementParameter)

        For Each child As XmlElement In e.ChildNodes
            If child.Name = "element" Then
                elId = New Guid(child.GetAttribute("id"))
                For Each grandchild As XmlElement In child.ChildNodes
                    Select Case grandchild.Name
                        Case "elementparameter"
                            Params.Add(clsApplicationElementParameter.FromXML(grandchild))
                        Case "input"
                            'Backwards compatibility for when we used to use process
                            'parameters instead of element parameters. The change occured
                            'because we added wildcard matching. Once legacy processes have
                            'been opened and saved again, this bit of code can be removed
                            Dim p As clsProcessParameter = clsProcessParameter.FromXML(grandchild)
                            Params.Add(New clsApplicationElementParameter(p.Name, p.GetDataType, p.Expression, AMI.clsAMI.ComparisonTypes.Equal))
                    End Select
                Next
            ElseIf child.Name = "action" Then

                For Each grandchild As XmlElement In child.ChildNodes
                    Select Case grandchild.Name
                        Case "id"
                            ' Get the ID of the read action for this step
                            Dim id As String = grandchild.FirstChild.Value

                            Action = clsAMI.GetActionTypeInfo(id)
                        Case "arguments"
                            clsActionStep.AddArgumentsFromXML(Me, grandchild)
                    End Select
                Next
            End If
        Next

        Me.ElementId = elId
        Me.Expression = BPExpression.FromNormalised(e.GetAttribute("expr"))
        Me.Parameters.AddRange(Params)

    End Sub

    Public Overrides Sub ToXML(ByVal doc As XmlDocument, ByVal elem As XmlElement)
        Dim child As XmlElement = Nothing

        'Add target element
        If ElementId <> Guid.Empty Then
            child = doc.CreateElement("element")
            child.SetAttribute("id", mElementId.ToString())
            'Add parameters
            For Each p As clsApplicationElementParameter In Me.Parameters
                child.AppendChild(p.ToXML(doc))
            Next
            elem.AppendChild(child)
        End If

        'Add Expression
        If Not Expression.IsEmpty Then _
         elem.SetAttribute("expr", Expression.NormalForm)

        'Add read action
        If Action IsNot Nothing Then
            child = doc.CreateElement("action")

            'Add(ID)
            Dim child2 As XmlElement = doc.CreateElement("id")
            child2.InnerText = Action.ID
            child.AppendChild(child2)

            'Add arguments
            clsActionStep.ArgumentsToXML(Me, doc, child)
            elem.AppendChild(child)
        End If
    End Sub

End Class
