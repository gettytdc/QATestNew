Imports System.Xml
Imports System.Drawing

Imports BluePrism.Core.Expressions
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateProcessCore.Stages
Imports System.Runtime.Serialization

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsChoice
''' 
''' <summary>
''' This class represents a choice within a choice start stage.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class clsChoice : Implements ILinkable, IExpressionHolder

#Region " Class-scope declarations "

    ''' <summary>
    ''' The width of the node, in world coordinates: ie the width
    ''' of the square region containing the node image.
    ''' </summary>
    Public Const DisplayWidth As Integer = 10

#End Region

#Region " Member Variables "

    ' The ID of the stage to goto when this choice is matched
    <DataMember>
    Private mLinkStage As Guid

    <DataMember>
    Private mName As String

    ' The distance from the owning choice stage that this node is placed
    <DataMember>
    Private mDistance As Single

    ' The expression that will be evaluated to check if this choice is true
    ' This can never be null
    <DataMember>
    Private mExpr As BPExpression = BPExpression.Empty

    <DataMember>
    Private mOwningStage As clsChoiceStartStage

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new instance of the clsChoice class and set the process to the
    ''' parent of this choices parent which is a stage.
    ''' </summary>
    ''' <param name="owner">The owning choice start stage of this choice</param>
    Public Sub New(ByVal owner As clsChoiceStartStage)
        OwningStage = owner
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The parent choice start stage, owning this choice node, if any.
    ''' </summary>
    Public Property OwningStage() As clsChoiceStartStage
        Get
            Return mOwningStage
        End Get
        Set(ByVal value As clsChoiceStartStage)
            mOwningStage = value
        End Set
    End Property

    ''' <summary>
    ''' The process to which the stage to which this choice belongs to, or null if it
    ''' has no owning stage or if that stage does not belong to a process.
    ''' </summary>
    Protected ReadOnly Property Process As clsProcess
        Get
            Return OwningStage?.Process
        End Get
    End Property

    ''' <summary>
    ''' Provides access to the Name of the choice
    ''' </summary>
    ''' <value></value>
    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal Value As String)
            mName = Value
        End Set
    End Property

    ''' <summary>
    ''' provides access to the choice expression.
    ''' </summary>
    ''' <value></value>
    Public Property Expression() As BPExpression _
     Implements IExpressionHolder.Expression
        Get
            Return mExpr
        End Get
        Set(ByVal value As BPExpression)
            If value Is Nothing Then value = BPExpression.Empty
            mExpr = value
        End Set
    End Property

    ''' <summary>
    ''' The offset (in world coordinates) of this choice node from
    ''' the centre of its <see cref="OwningStage">owning choice stage</see>.
    ''' </summary>
    Public Property Distance() As Single
        Get
            Return mDistance
        End Get
        Set(ByVal Value As Single)
            Dim NewValue As Single = Value
            If OwningStage IsNot Nothing Then
                NewValue = Math.Max(OwningStage.GetMinimumNodeDistance, NewValue)
                NewValue = Math.Min(OwningStage.GetMaximumNodeDistance, NewValue)
            End If
            mDistance = NewValue
        End Set
    End Property

    ''' <summary>
    ''' Gets the centre of the choice node in world coordinates.
    ''' </summary>
    Public ReadOnly Property Location() As PointF
        Get
            ' Get the centre of the choice node in world coordinates.
            Dim stg As clsChoiceStartStage = OwningStage

            Dim endStg As clsChoiceEndStage = stg?.Process?.GetChoiceEnd(stg)
            If endStg Is Nothing Then Return PointF.Empty
            Dim dx As Single = endStg.GetDisplayX() - stg.GetDisplayX()
            Dim dy As Single = endStg.GetDisplayY() - stg.GetDisplayY()
            Dim i As PointF = stg.Location

            Dim veclen As Single = CSng(Math.Sqrt(dx * dx + dy * dy))
            If veclen <> 0 Then
                Dim Cosine As Single = dx / veclen
                Dim Sine As Single = dy / veclen

                i.X += Cosine * Me.Distance
                i.Y += Sine * Me.Distance
            End If

            Return i
        End Get
    End Property

    ''' <summary>
    ''' Gets the bounds of the area occupied by this node, in world coordinates.
    ''' </summary>
    ''' <remarks>Relies on the ParentStage property being set for accurate results.
    ''' </remarks>
    Public ReadOnly Property DisplayBounds() As RectangleF
        Get
            Dim b As New RectangleF(Location, New SizeF(DisplayWidth, DisplayWidth))
            b.Offset(-DisplayWidth \ 2, -DisplayWidth \ 2)
            Return b
        End Get
    End Property

    ''' <summary>
    ''' The ID of the stage that this choice links to. This will be
    ''' <see cref="Guid.Empty"/> if it is not linked to any stage.
    ''' </summary>
    Public Property LinkTo() As Guid Implements ILinkable.OnSuccess
        Get
            Return mLinkStage
        End Get
        Set(ByVal value As Guid)
            mLinkStage = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Sets the id of the choice to goto when the choices expression evaluates to
    ''' true
    ''' </summary>
    ''' <param name="g">The ID of the stage to which this choice should link</param>
    Private Function SetNextLink(ByVal g As Guid, ByRef sErr As String) As Boolean _
     Implements ILinkable.SetNextLink
        LinkTo = g
        Return True
    End Function

    ''' <summary>
    ''' Clones the choice.
    ''' </summary>
    ''' <returns>Returns a deep clone of this choice</returns>
    ''' <remarks>The clone will contain the same values as this choice, except for
    ''' the owning process (for which the clone refers to the same object as this
    ''' choice by reference) and the parent choice start stage (which is set to null
    ''' in the cloned choice)
    ''' </remarks>
    Public Overridable Function Clone() As clsChoice
        ' FIXME: parent choice start stage is set to null? Er, no. Should it be?
        Return DirectCast(MemberwiseClone(), clsChoice)
    End Function

    ''' <summary>
    ''' Writes this choice to XML in the given document
    ''' </summary>
    ''' <param name="doc">The XML document to which the choice XML is to be written.
    ''' </param>
    ''' <param name="el">The element to which the XML should be written</param>
    ''' <param name="bSelectionOnly">True to only write XML for the current selection
    ''' within the process</param>
    Friend Overridable Sub ToXML( _
     ByVal doc As XmlDocument, ByVal el As XmlElement, _
     ByVal bSelectionOnly As Boolean)
        el.SetAttribute("expression", Me.Expression.NormalForm)
        BPUtil.AppendTextElement(el, "name", Name)
        BPUtil.AppendTextElement(el, "distance", XmlConvert.ToString(Distance))

        ' If there's a link, and it links to a selected stage or this isn't just
        ' a selectionOnly XML write the link to the XML
        If LinkTo <> Guid.Empty AndAlso _
         (Not bSelectionOnly OrElse Process.IsStageSelected(LinkTo)) Then _
         BPUtil.AppendTextElement(el, "ontrue", LinkTo.ToString())

    End Sub

    ''' <summary>
    ''' Parses this choice from the given XML element
    ''' </summary>
    ''' <param name="choiceElem">The element which contains the choice data.</param>
    Friend Overridable Sub FromXML(ByVal choiceElem As XmlElement)
        Expression = _
         BPExpression.FromNormalised(choiceElem.GetAttribute("expression"))
        For Each elem As XmlElement In choiceElem.ChildNodes
            Select Case elem.Name
                Case "name" : mName = elem.InnerText
                Case "distance" : mDistance = clsProcess.ReadXmlSingle(elem)
                Case "ontrue" : LinkTo = New Guid(elem.InnerText)
            End Select
        Next
    End Sub

#End Region

End Class
