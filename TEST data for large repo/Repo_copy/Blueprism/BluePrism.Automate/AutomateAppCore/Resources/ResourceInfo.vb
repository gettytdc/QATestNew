Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.Resources.ResourceMachine
Imports BluePrism.Core.Resources

Namespace Resources
    ''' <summary>
    ''' Represents information about a resource
    ''' </summary>
    <Serializable>
    <DataContract(Name:="r", [Namespace]:="bp")>
    Public Class ResourceInfo

        ''' <summary>
        ''' The resources Identity
        ''' </summary>
        <DataMember(Name:="ID")>
        Public Property ID As Guid

        ''' <summary>
        ''' The Pool to which the resource belongs
        ''' </summary>
        <DataMember(Name:="P", IsRequired:=False, EmitDefaultValue:=False)>
        Public Property Pool As Guid

        ''' <summary>
        ''' The reported status of the resource
        ''' </summary>
        <DataMember(Name:="S")>
        Public Property Status As ResourceDBStatus

        ''' <summary>
        ''' The attributes of the resource
        ''' </summary>
        <DataMember(Name:="A")>
        Public Property Attributes As ResourceAttribute

        ''' <summary>
        ''' The Name of the resource
        ''' </summary>
        <DataMember(Name:="N")>
        Public Property Name As String

        ''' <summary>
        ''' The time when the resource information was last updated
        ''' </summary>
        <DataMember(Name:="L")>
        Public Property LastUpdated As Date

        <DataMember(Name:="LCS")>
        Public Property LastConnectionStatistics As ResourceConnectionStatistic

        <DataMember(Name:="LCT")>
        Public Property LastConnectionStatus As String

        ''' <summary>
        ''' The number of active sessions on the resource
        ''' </summary>
        <DataMember(Name:="AS")>
        Public Property ActiveSessions As Integer

        ''' <summary>
        ''' The number of warning sessions on the resource
        ''' </summary>
        <DataMember(Name:="WS")>
        Public Property WarningSessions As Integer

        ''' <summary>
        ''' The number of pending sessions on the resource
        ''' </summary>
        <DataMember(Name:="PS")>
        Public Property PendingSessions As Integer

        ''' <summary>
        ''' The calculated state of the resource
        ''' </summary>
        <DataMember(Name:="DS")>
        Public Property DisplayStatus As ResourceStatus

        ''' <summary>
        ''' An informational string about the resource
        ''' </summary>
        <DataMember(Name:="I", EmitDefaultValue:=False)>
        Public Property Information As String

        ''' <summary>
        ''' The colour that represents the status of the resource.
        ''' </summary>
        <DataMember(Name:="IC")>
        Public Property InfoColour As Integer

        ''' <summary>
        ''' Flag designating this resource as pool controller
        ''' </summary>
        <DataMember(Name:="C", EmitDefaultValue:=False)>
        Public Property Controller As Boolean

        ''' <summary>
        ''' The user that owns this resource
        ''' </summary>
        ''' <remarks>Will be guid.empty if it is a public resource</remarks>
        <DataMember(Name:="U", IsRequired:=False, EmitDefaultValue:=False)>
        Public Property UserID As Guid

        ''' <summary>
        ''' A name of a Pool to which the resource belongs
        ''' </summary>
        <DataMember(Name:="PN", IsRequired:=False, EmitDefaultValue:=False)>
        Public Property PoolName As String

        ''' <summary>
        ''' The Pool to which the resource belongs
        ''' </summary>
        <DataMember(Name:="GI", IsRequired:=False, EmitDefaultValue:=False)>
        Public Property GroupID As Guid

        ''' <summary>
        ''' A folder name to which the resource belongs
        ''' </summary>
        <DataMember(Name:="GN", IsRequired:=False, EmitDefaultValue:=False)>
        Public Property GroupName As String

        ''' <summary>
        ''' A utility function to convert a status to a text representation
        ''' </summary>
        Public Shared Function GetResourceStatusFriendlyName(status As ResourceStatus) As String
            Select Case status
                Case ResourceStatus.Working
                    Return My.Resources.ResourceInfo_Working
                Case ResourceStatus.Idle
                    Return My.Resources.ResourceInfo_Idle
                Case ResourceStatus.Warning
                    Return My.Resources.ResourceInfo_Warning
                Case ResourceStatus.Offline
                    Return My.Resources.ResourceInfo_Offline
                Case ResourceStatus.Missing
                    Return My.Resources.ResourceInfo_Missing
                Case ResourceStatus.LoggedOut
                    Return My.Resources.ResourceInfo_LoggedOut
                Case ResourceStatus.Pool
                    Return ""
                Case ResourceStatus.Private
                    Return My.Resources.ResourceInfo_OnlinePrivate
                Case Else
                    Return My.Resources.ResourceInfo_UnknownInvalidState
            End Select
        End Function

    End Class

End Namespace
