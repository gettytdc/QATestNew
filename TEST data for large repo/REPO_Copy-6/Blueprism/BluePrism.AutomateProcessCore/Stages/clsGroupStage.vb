Imports System.Runtime.Serialization

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsGroupStage
    ''' 
    ''' <summary>
    ''' Abstract class representing a stage which belongs to a group.
    ''' This includes loop start/end, choice start/end, wait start/end, etc.
    ''' </summary>
    ''' <remarks>The group ID allows two or more stages can be associated with each
    ''' other and deleted, copied, pasted etc at the same time.</remarks>
    <Serializable, DataContract([Namespace]:="bp")>
    Public MustInherit Class clsGroupStage
        Inherits clsLinkableStage


        Protected Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub


        ''' <summary>
        ''' Gets the group ID for this stage. All stages in the group
        ''' carry the same ID.
        ''' </summary>
        Public Function GetGroupID() As Guid
            Return mgGroupID
        End Function
        <DataMember>
        Protected mgGroupID As Guid = Guid.Empty

        ''' <summary>
        ''' Sets the group ID for this stage. All stages in the group must carry the
        ''' same ID.
        ''' </summary>
        ''' <param name="gID">The ID of the group.</param>
        Public Sub SetGroupID(ByVal gID As Guid)
            mgGroupID = gID
        End Sub

        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsGroupStage = CType(MyBase.Clone, clsGroupStage)
            copy.mgGroupID = Me.mgGroupID
            Return copy
        End Function

        Public Overrides Sub FromXML(ByVal e2 As System.Xml.XmlElement)
            MyBase.FromXML(e2)
            For Each e3 As Xml.XmlElement In e2.ChildNodes
                Select Case e3.Name
                    Case "groupid"
                        Me.SetGroupID(New Guid(e3.InnerText))
                End Select
            Next
        End Sub

        Public Overrides Sub ToXml(ByVal ParentDocument As System.Xml.XmlDocument, ByVal StageElement As System.Xml.XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(ParentDocument, StageElement, bSelectionOnly)

            If mgGroupID.ToString() <> Guid.Empty.ToString() Then
                Dim e2 As Xml.XmlElement = ParentDocument.CreateElement("groupid")
                e2.AppendChild(ParentDocument.CreateTextNode(mgGroupID.ToString()))
                StageElement.AppendChild(e2)
            End If

        End Sub

    End Class

End Namespace