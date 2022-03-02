Imports System.Runtime.Serialization
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI

<Serializable, DataContract([Namespace]:="bp")>
Public Class AmiAction

    <DataMember>
    Private mActionId As String
    <NonSerialized>
    Private mAction As clsActionTypeInfo

    Public Sub New(actionId As String)
        If actionId = "" Then Throw New ArgumentNullException(NameOf(actionId))
        mActionId = actionId
    End Sub

    Public Sub New(actionInfo As clsActionTypeInfo)
        mAction = actionInfo
        mActionId = actionInfo.CommandID
    End Sub

    Public ReadOnly Property Action As clsActionTypeInfo
        Get
            If mAction Is Nothing Then
                mAction = clsAMI.GetActionTypeInfo(mActionId)
            End If
            Return mAction
        End Get
    End Property
End Class
