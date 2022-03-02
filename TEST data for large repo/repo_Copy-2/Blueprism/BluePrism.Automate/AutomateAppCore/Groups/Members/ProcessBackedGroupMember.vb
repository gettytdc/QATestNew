Imports System.Runtime.Serialization
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Server.Domain.Models

Namespace Groups

    ''' <summary>
    ''' Represents object/process nodes within the tree structure
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp", Name:="pgm")>
    Public MustInherit Class ProcessBackedGroupMember : Inherits GroupMember : Implements IMemberMetaData

#Region " Class-scope Declarations "

        ''' <summary>
        ''' Inner class to hold the data names for the properties in this class
        ''' </summary>
        Private Class DataNames
            Public Const Description As String = "Description"
            Public Const CreatedBy As String = "CreatedBy"
            Public Const ModifiedBy As String = "ModifiedBy"
            Public Const CreatedAt As String = "CreatedAt"
            Public Const ModifiedAt As String = "ModifiedAt"
            Public Const Attributes As String = "Attributes"
            Public Const LockUserId As String = "LockUserId"
            Public Const LockMachineName As String = "LockMachineName"
            Public Const LockTime As String = "LockTime"
            Public Const IsProcessLocked As String = "IsProcessLocked"
            Public Const WebServiceName As String = "WebServiceName"
            Public Const WebServiceDocLit As String = "WebServiceDocLit"
            Public Const WebServiceLegacyNamespace As String = "WebServiceLegacyNamespace"
            Public Const IsShareable As String = "SharedObject"
        End Class

#End Region

#Region " Filter Predicates "

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all non-retired,
        ''' non-exposed group members to pass, but no others
        ''' </summary>
        Public Shared ReadOnly Property NotRetiredAndNotExposed As Predicate(Of IGroupMember)
            Get
                Return Function(gm)
                           ' If it's a process, allow any published processes through
                           Dim pgm = TryCast(gm, ProcessBackedGroupMember)
                           If pgm Is Nothing Then Return False
                           Return Not pgm.HasAnyAttribute(ProcessAttributes.Retired Or ProcessAttributes.PublishedWS)
                       End Function
            End Get
        End Property

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all non-retired,
        ''' exposed group members to pass, but no others
        ''' </summary>
        Public Shared ReadOnly Property NotRetiredAndExposed As Predicate(Of IGroupMember)
            Get
                Return Function(gm)
                           ' If it's a process, allow any published processes through
                           Dim pgm = TryCast(gm, ProcessBackedGroupMember)
                           If pgm Is Nothing Then Return False
                           Return (Not pgm.HasAttribute(ProcessAttributes.Retired) AndAlso pgm.HasAttribute(ProcessAttributes.PublishedWS))
                       End Function
            End Get
        End Property

#End Region

#Region " Associated Data Properties "

        ''' <summary>
        ''' The description of the process that this member represents
        ''' </summary>
        <DataMember(Name:="de", EmitDefaultValue:=False)>
        Public Property Description As String
            Get
                Return GetData(DataNames.Description, "")
            End Get
            Set(value As String)
                SetData(DataNames.Description, value)
            End Set
        End Property

        ''' <summary>
        ''' The name of the user who created the process that this member represents
        ''' </summary>
        <DataMember(Name:="cb", EmitDefaultValue:=False)>
        Public Property CreatedBy() As String
            Get
                Return GetData(DataNames.CreatedBy, "")
            End Get
            Set(value As String)
                SetData(DataNames.CreatedBy, value)
            End Set
        End Property

        ''' <summary>
        ''' The name of the user who last modified the process that this member
        ''' represents
        ''' </summary>
        <DataMember(Name:="mb", EmitDefaultValue:=False)>
        Public Property ModifiedBy() As String
            Get
                Return GetData(DataNames.ModifiedBy, "")
            End Get
            Set(value As String)
                SetData(DataNames.ModifiedBy, value)
            End Set
        End Property

        ''' <summary>
        ''' The date/time that the process that this member represents was created
        ''' </summary>
        <DataMember(Name:="ca", EmitDefaultValue:=False)>
        Public Property CreatedAt() As Date
            Get
                Return GetData(DataNames.CreatedAt, Date.MinValue)
            End Get
            Set(value As Date)
                SetData(DataNames.CreatedAt, value)
            End Set
        End Property

        ''' <summary>
        ''' The date/time that the process that this member represents was last
        ''' modified
        ''' </summary>
        <DataMember(Name:="ma", EmitDefaultValue:=False)>
        Public Property ModifiedAt() As Date
            Get
                Return GetData(DataNames.ModifiedAt, Date.MinValue)
            End Get
            Set(value As Date)
                SetData(DataNames.ModifiedAt, value)
            End Set
        End Property

        ''' <summary>
        ''' The attributes for this process
        ''' </summary>
        <DataMember(Name:="at")>
        Public Property Attributes As ProcessAttributes
            Get
                Return GetData(DataNames.Attributes, ProcessAttributes.None)
            End Get
            Set(value As ProcessAttributes)
                SetData(DataNames.Attributes, value)
            End Set
        End Property

        ''' <summary>
        ''' The ID of the user who has this process-backed entity locked
        ''' </summary>
        <DataMember(EmitDefaultValue:=False, IsRequired:=False, Name:="luid")>
        Public Property LockUserId As Guid
            Get
                Return GetData(DataNames.LockUserId, Guid.Empty)
            End Get
            Set(value As Guid)
                SetData(DataNames.LockUserId, value)
            End Set
        End Property

        ''' <summary>
        ''' The ID of the resource on which this process-backed entity is locked
        ''' </summary>
        <DataMember(IsRequired:=False, EmitDefaultValue:=False, Name:="lmn")>
        Public Property LockMachineName As String
            Get
                Return GetData(DataNames.LockMachineName, "")
            End Get
            Set(value As String)
                SetData(DataNames.LockMachineName, value)
            End Set
        End Property

        ''' <summary>
        ''' The date/time at which the lock on this process/object was acquired
        ''' </summary>
        <DataMember(IsRequired:=False, EmitDefaultValue:=False, Name:="lt")>
        Public Property LockTime As Date
            Get
                Return GetData(DataNames.LockTime, Date.MinValue)
            End Get
            Set(value As Date)
                SetData(DataNames.LockTime, value)
            End Set
        End Property

        ''' <summary>
        ''' A flag which states if the member is locked
        ''' </summary>
        <DataMember(IsRequired:=False, EmitDefaultValue:=False, Name:="lck")>
        Public Property IsProcessLocked As Boolean
            Get
                Return GetData(Of Boolean)(DataNames.IsProcessLocked, False)
            End Get
            Set(value As Boolean)
                SetData(DataNames.IsProcessLocked, value)
            End Set
        End Property

        ''' <summary>
        ''' The web service name that the object/process is exposed as (only set if
        ''' actually exposed as a web service).
        ''' </summary>
        <DataMember(IsRequired:=False, EmitDefaultValue:=False, Name:="wsn")>
        Public Property WebServiceName As String
            Get
                Return GetData(DataNames.WebServiceName, "")
            End Get
            Set(value As String)
                SetData(DataNames.WebServiceName, value)
            End Set
        End Property

        ''' <summary>
        ''' A flag which states if the web service should use a default or
        ''' Document/literal encoding type
        ''' </summary>
        <DataMember(IsRequired:=False, EmitDefaultValue:=False, Name:="wsdl")>
        Public Property WebServiceDocLit As Boolean
            Get
                Return GetData(Of Boolean)(DataNames.WebServiceDocLit, False)
            End Get
            Set(value As Boolean)
                SetData(Of Boolean)(DataNames.WebServiceDocLit, value)
            End Set
        End Property

        ''' <summary>
        ''' Returns the internalisation text value
        ''' </summary>
        Public ReadOnly Property WebServiceDocLitFormatText() As String
            Get
                If (GetData(Of Boolean)(DataNames.WebServiceDocLit, False)) Then
                    Return My.Resources.WebServiceDetails_Resource.WebServiceEncodingFormat_DocumentLiteral
                Else
                    Return My.Resources.WebServiceDetails_Resource.WebServiceEncodingFormat_Rpc
                End If
            End Get
        End Property

        ''' <summary>
        ''' A flag which states if the Web Service should use the Legacy
        ''' RPC XML Webspace Format
        ''' </summary>
        <DataMember(IsRequired:=False, EmitDefaultValue:=False, Name:="wsln")>
        Public Property WebServiceLegacyNamespace As Boolean
            Get
                Return GetData(Of Boolean)(DataNames.WebServiceLegacyNamespace, False)
            End Get
            Set(value As Boolean)
                SetData(Of Boolean)(DataNames.WebServiceLegacyNamespace, value)
            End Set
        End Property

        ''' <summary>
        ''' Returns the internalisation text value of WebServiceLegacyNamespace
        ''' </summary>
        Public ReadOnly Property WebServiceLegacyNamespaceFormatText() As String
            Get
                If (GetData(Of Boolean)(DataNames.WebServiceDocLit, False)) And GetData(Of Boolean)(DataNames.WebServiceLegacyNamespace, False) = False Then
                    Return My.Resources.WebServiceDetails_Resource.WebServiceRPCXML_NA
                ElseIf (GetData(Of Boolean)(DataNames.WebServiceLegacyNamespace, False)) Then
                    Return My.Resources.WebServiceDetails_Resource.WebServiceRPCXML_True
                Else
                    Return My.Resources.WebServiceDetails_Resource.WebServiceRPCXML_False
                End If
            End Get
        End Property

        ''' <summary>
        ''' A flag that indicates whether or not the Object is shareable
        ''' </summary>
        <DataMember(IsRequired:=False, EmitDefaultValue:=False, Name:="is")>
        Public Property IsShareable As Boolean
            Get
                Return GetData(Of Boolean)(DataNames.IsShareable, False)
            End Get
            Set(value As Boolean)
                SetData(Of Boolean)(DataNames.IsShareable, value)
            End Set
        End Property

#End Region

#Region " Other Properties "

        ''' <summary>
        ''' The process type represented by this group member
        ''' </summary>
        Public MustOverride ReadOnly Property ProcessType As DiagramType

        ''' <summary>
        ''' Gets the <see cref="GroupMember.RawMember">raw member</see> for this
        ''' group member as process-backed group member.
        ''' </summary>
        Protected Shadows ReadOnly Property RawMember As ProcessBackedGroupMember
            Get
                Return DirectCast(MyBase.RawMember, ProcessBackedGroupMember)
            End Get
        End Property

        ''' <summary>
        ''' Flag indicating if this process-backed entity is locked or not.
        ''' </summary>
        Public Overrides ReadOnly Property IsLocked As Boolean
            Get
                Return IsProcessLocked
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this process/object is retired or not.
        ''' </summary>
        Public Overrides ReadOnly Property IsRetired As Boolean
            Get
                Return Attributes.HasFlag(ProcessAttributes.Retired)
            End Get
        End Property

        ''' <summary>
        ''' The linking table between nodes of this type and groups. In this case,
        ''' the table is <c>BPAGroupProcess</c>.
        ''' </summary>
        Public Overrides ReadOnly Property LinkTableName As String
            Get
                Return "BPAGroupProcess"
            End Get
        End Property

        ''' <summary>
        ''' The column name of the member ID column in the link table for this type
        ''' </summary>
        Friend Overrides ReadOnly Property MemberIdColumnName As String
            Get
                Return "processid"
            End Get
        End Property

        ''' <summary>
        ''' Changes the item subtitle for process-backed group members to use the
        ''' description of the process/object.
        ''' </summary>
        Protected Overrides ReadOnly Property ItemSubTitle As String
            Get
                Return Description
            End Get
        End Property

        Public ReadOnly Property HasMetaData As Boolean Implements IMemberMetaData.HasMetaData
            Get
                Return Not (
                    String.IsNullOrWhiteSpace(CreatedBy) AndAlso
                    String.IsNullOrWhiteSpace(ModifiedBy) AndAlso
                    CreatedAt = Date.MinValue AndAlso
                    ModifiedAt = Date.MinValue
                    )
            End Get
        End Property

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new process-backed group member using data from a provider.
        ''' </summary>
        ''' <param name="prov">The provider of the data to initialise this group
        ''' member with - this expects: <list>
        ''' <item>lockuser: Guid: The ID of the user that has the process/VBO locked
        ''' </item>
        ''' <item>lockresource: Guid: The ID of the resource which has the lock on
        ''' the process/VBO</item>
        ''' <item>lockdatetime: DateTime: The date/time of the lock</item>
        ''' <item>webservicename: String: The web service name if exposed as one
        ''' </item>
        ''' <item>webserviceflag: WebServiceEncodingFormat: A flag stating which SOAP 
        ''' format to use
        ''' </item>
        ''' <item>webservicelegacynamespace: Flag: A flag stating which XML Namespace
        ''' format to use
        ''' </item>
        ''' <item>sharedObject: A flag indicating whether the Object is shareable</item>
        ''' </list></param>
        Public Sub New(prov As IDataProvider)
            MyBase.New(prov)
            Description = prov.GetString("description")
            Attributes = prov.GetValue("attributes", ProcessAttributes.None)
            WebServiceName = prov.GetString("webservicename")
            WebServiceDocLit = prov.GetBool("forcedocumentliteral")
            WebServiceLegacyNamespace = prov.GetBool("useLegacyNamespace")
            IsShareable = prov.GetBool("sharedObject")
            IsProcessLocked = (prov.GetGuid("lockuser") <> Guid.Empty AndAlso
                prov.GetString("lockmachinename") <> "" AndAlso
                prov.GetValue("lockdatetime", Date.MinValue) <> Date.MinValue)
        End Sub

        ''' <summary>
        ''' Creates a new, empty process or object node
        ''' </summary>
        Public Sub New()
            Me.New(NullDataProvider.Instance)
        End Sub

#End Region

#Region " Methods "

        Public Sub UpdateLockStatus()
            gSv.GetProcessLockInfo(IdAsGuid, LockUserId, LockMachineName, LockTime)
            RefreshLockStatus()
        End Sub

        ''' <summary>
        ''' Resets the locked state of this group member. Does nothing if this member
        ''' has no 'locked' concept or if it is not currently locked.
        ''' </summary>
        Public Overrides Sub ResetLock()
            If Not IsLocked Then Return
            LockUserId = Nothing
            LockMachineName = ""
            LockTime = Date.MinValue
        End Sub

        ''' <summary>
        ''' Gets whether this member has any of the attributes OR'd into the given
        ''' enum value.
        ''' </summary>
        ''' <param name="attr">The attributes to check this member for</param>
        ''' <returns>True if this member has any of the attributes encoded into
        ''' <paramref name="attr"/> or if <paramref name="attr"/> is
        ''' <see cref="ProcessAttributes.None"/>; False otherwise.</returns>
        Public Function HasAnyAttribute(attr As ProcessAttributes) As Boolean
            Return Attributes.HasAnyFlag(attr)
        End Function

        ''' <summary>
        ''' Gets whether this member has the attribute given (or all the attributes
        ''' given if the value contains more than one OR'd attribute).
        ''' </summary>
        ''' <param name="attr">The attribute(s) to check this member for</param>
        ''' <returns>True if this member has the attribute(s) provided or if
        ''' <paramref name="attr"/> is <see cref="ProcessAttributes.None"/>;
        ''' False otherwise.</returns>
        Public Function HasAttribute(attr As ProcessAttributes) As Boolean
            Return Attributes.HasAnyFlag(attr)
        End Function

        ''' <summary>
        ''' Adds a process attribute to the attributes in this member.
        ''' </summary>
        ''' <param name="attr">The attribute to set on in this group member.</param>
        Public Sub AddAttribute(attr As ProcessAttributes)
            Attributes = Attributes Or attr
        End Sub

        ''' <summary>
        ''' Removes a process attribute from the attributes in this member.
        ''' </summary>
        ''' <param name="attr">The attribute to set off in this group member.</param>
        Public Sub RemoveAttribute(attr As ProcessAttributes)
            Attributes = Attributes And Not attr
        End Sub

        Public Sub Retire()

            If IsInGroup() Then
                Dim permLogic = New GroupPermissionLogic()
                If Not permLogic.ValidateMoveMember(Me, Owner, RootGroup, False, Function() True, User.Current) Then
                    Throw New PermissionException("You do not have permission to move this out of that group.")
                End If
            End If

            gSv.RetireProcessOrObject(IdAsGuid)
            AddAttribute(ProcessAttributes.Retired)
            If IsInGroup Then Remove()
        End Sub


        Public Sub UnRetire(moveToGroup As IGroup)

            Dim permLogic = New GroupPermissionLogic()
            If Not permLogic.ValidateMoveMember(Me, Owner, moveToGroup, False, Function() True, User.Current) Then
                Throw New PermissionException("You do not have permission to move this process into that group.")
            End If

            gSv.UnretireProcessOrObject(IdAsGuid, moveToGroup.IdAsGuid())
            RemoveAttribute(ProcessAttributes.Retired)
            MoveTo(moveToGroup)
        End Sub

        Public Sub UpdateMetaInfo(updatedMetaInfo As ProcessMetaInfo) Implements IMemberMetaData.UpdateMetaInfo
            If updatedMetaInfo Is Nothing Then Throw New InvalidValueException($"The expected {GetType(ProcessMetaInfo)} returned was null.")
            With Me
                .CreatedAt = updatedMetaInfo.CreatedAt
                .ModifiedAt = updatedMetaInfo.ModifiedAt
                .Description = updatedMetaInfo.Description
                .CreatedBy = updatedMetaInfo.CreatedBy
                .ModifiedBy = updatedMetaInfo.ModifiedBy
                .LockMachineName = updatedMetaInfo.LockMachineName
                .LockUserId = updatedMetaInfo.LockUserId
                .LockTime = updatedMetaInfo.LockTime
            End With
        End Sub

        ''' <summary>
        ''' When the lock status has been updated, the class neeeds to refesh it's internal lock status
        ''' </summary>
        Private Sub RefreshLockStatus()
            IsProcessLocked = (LockUserId <> Guid.Empty AndAlso
                               LockMachineName <> String.Empty AndAlso
                               LockTime <> Date.MinValue)
        End Sub
#End Region
    End Class

    ''' <summary>
    ''' Container class to hold meta about a process used on the Studio tab front end.
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp", Name:="pmi")>
    Public Class ProcessMetaInfo
        <DataMember(Name:="p", EmitDefaultValue:=False)>
        Public Property ProcessId As Guid

        <DataMember(Name:="de", EmitDefaultValue:=False)>
        Public Property Description As String

        <DataMember(Name:="cb", EmitDefaultValue:=False)>
        Public Property CreatedBy As String

        <DataMember(Name:="mb", EmitDefaultValue:=False)>
        Public Property ModifiedBy As String

        <DataMember(Name:="lmn", EmitDefaultValue:=False)>
        Public Property LockMachineName As String

        <DataMember(Name:="ca", EmitDefaultValue:=False)>
        Public Property CreatedAt As Date

        <DataMember(Name:="ma", EmitDefaultValue:=False)>
        Public Property ModifiedAt As Date

        <DataMember(Name:="lt", EmitDefaultValue:=False)>
        Public Property LockTime As Date

        <DataMember(Name:="luid", EmitDefaultValue:=False)>
        Public Property LockUserId As Guid
    End Class

End Namespace
