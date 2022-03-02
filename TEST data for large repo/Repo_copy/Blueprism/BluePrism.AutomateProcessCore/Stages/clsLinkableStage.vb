Imports System.Xml
Imports BluePrism.BPCoreLib.Collections
Imports System.Runtime.Serialization

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsLinkableStage
    ''' 
    ''' <summary>
    ''' Abstract class representing a stage which is linkable.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public MustInherit Class clsLinkableStage
        Inherits clsProcessStage
        Implements ILinkable


        Public Sub New(ByVal Parent As clsProcess)
            MyBase.New(Parent)
        End Sub

        ''' <summary>
        ''' Sets the next link to the specified value.
        ''' </summary>
        ''' <param name="dest">The ID of the stage to which a link should be made.
        ''' </param>
        ''' <remarks>Each implementor may have their own policy about what the next link
        ''' is: ordinary process stages will change one link only; decision stages may
        ''' choose which link to change; etc.</remarks>
        Function SetNextLink(ByVal dest As Guid, ByRef sErr As String) As Boolean Implements ILinkable.SetNextLink
            mOnSuccess = dest
            Return True
        End Function


        ''' <summary>
        ''' The ID of the stage that this one links to.
        ''' </summary>
        Public Property OnSuccess() As Guid Implements ILinkable.OnSuccess
            Get
                Return mOnSuccess
            End Get
            Set(ByVal value As Guid)
                mOnSuccess = value
            End Set
        End Property

        ''' <summary>
        ''' Determine if this stage links (in a forwards direction) to another stage.
        ''' </summary>
        ''' <param name="st">The stage to check against</param>
        ''' <returns>True if there is a link, False otherwise.</returns>
        Public Overrides Function LinksTo(ByVal st As clsProcessStage) As Boolean
            Return mOnSuccess.Equals(st.GetStageID())
        End Function

        ''' <summary>
        ''' Gets a collection of stages that this stage links to. In this case it is
        ''' a List with only one entry, the onsuccess stage.
        ''' </summary>
        ''' <returns>A collection of stages that this stage links to</returns>
        Friend Overrides Function GetLinks() As ICollection(Of clsProcessStage)
            Dim linksto As clsProcessStage = mParent.GetStage(OnSuccess)
            If linksto IsNot Nothing Then Return GetSingleton.ICollection(linksto)

            Return GetEmpty.ICollection(Of clsProcessStage)()
        End Function

        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList

            Dim errors As ValidationErrorList =
             MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            'Check OnSuccess
            If mOnSuccess = Guid.Empty Then
                If Not AllowsMissingLinks Then errors.Add(Me, 57)
            Else
                Dim dest As clsProcessStage = mParent.GetStage(mOnSuccess)
                If dest Is Nothing Then
                    errors.Add(Me, 101)
                Else
                    If dest.SubSheet IsNot Me.SubSheet Then errors.Add(Me, 102)
                End If
            End If

            Return errors
        End Function

        ''' <summary>
        ''' Indicates whether this stage allows a link to be missing from this stage.
        ''' Typically not allowed, certain stages do not emit an error if such a
        ''' state is entered.
        ''' </summary>
        Protected Overridable ReadOnly Property AllowsMissingLinks() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsLinkableStage = CType(MyBase.Clone(), clsLinkableStage)
            copy.OnSuccess = mOnSuccess
            Return copy
        End Function


        Public Overrides Sub FromXML(ByVal e2 As XmlElement)
            MyBase.FromXML(e2)
            For Each child As XmlNode In e2.ChildNodes
                Select Case child.Name
                    Case "onsuccess"
                        Dim s As String = child.InnerText
                        If s.Length > 0 Then
                            mOnSuccess = New Guid(s)
                        End If
                End Select
            Next
        End Sub

    End Class

End Namespace
