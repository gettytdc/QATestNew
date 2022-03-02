Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.clsServerPartialClasses
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Images

Namespace Groups

    ''' <summary>
    ''' Represents queues within a group
    ''' </summary>
    <Serializable>
    <DataContract([Namespace]:="bp")>
    Public Class QueueGroupMember : Inherits GroupMember

        ''' <summary>
        ''' Inner class to hold the data names for the properties in this class
        ''' </summary>
        Private Class DataNames
            Public Const QueueGuid As String = "QueueGuid"
            Public Const Running As String = "Running"
            Public Const EncryptKeyID As String = "EncryptID"
            Public Const IsActive As String = "IsActive"
            Public Const RequiredFeature As String = "RequiredFeature"
        End Class

        ''' <summary>
        ''' A filter which can be applied to a group tree which allows all active
        ''' and controllable queue members to pass, but no others.
        ''' </summary>
        Public Shared Function ActiveAndControllableFilter(controllableActiveQueues As ICollection(Of Integer)) As Predicate(Of IGroupMember)
            Return _
                Function(groupMember)
                    Dim queueGroupMember = TryCast(groupMember, QueueGroupMember)
                    If queueGroupMember Is Nothing Then Return False

                    Return queueGroupMember.IsActive AndAlso controllableActiveQueues.Contains(queueGroupMember.IdAsInteger())

                End Function
        End Function

        ''' <summary>
        ''' A filter which can be applied to a group tree which allows all passive
        ''' (ie. not active) queue members to pass, but no others.
        ''' </summary>
        Public Shared ReadOnly Property PassiveFilter As Predicate(Of IGroupMember)
            Get
                Return _
                 Function(m)
                     Dim qm = TryCast(m, QueueGroupMember)
                     Return (qm IsNot Nothing AndAlso Not qm.IsActive)
                 End Function
            End Get
        End Property

        ''' <summary>
        ''' Creates a new queue group member using data from a provider.
        ''' </summary>
        ''' <param name="prov">The provider of the data to initialise this group
        ''' member with - this expects: <list>
        ''' <item>id: Integer: The ID (ie. ident) of the queue</item>
        ''' <item>name: String: The name of the queue</item>
        ''' <item>guid: Guid: The GUID-based ID of the queue - the int is the PK, the
        ''' GUID is the old PK, kept for reasons.</item>
        ''' <item>running: Boolean: Whether the queue is running or not.</item>
        ''' <item>encryptname: String: The name of the encryption scheme used to
        ''' encrypt this queue; null if it is not encrypted.</item>
        ''' <item>processid: Guid: The ID of the process which is configured to work
        ''' the (active) queue; null/Guid.Empty if the queue is not an active queue.
        '''  </item>
        ''' <item>resourcegroupid: Guid: The ID of the resource group which is
        ''' configured to run the sessions on behalf of the (active) queue;
        ''' null/Guid.Empty if the queue is not an active queue</item>
        ''' <item>isactive: Boolean: True to indicate an active queue; False to
        ''' indicate a 'passive' queue.</item>
        ''' </list></param>
        Public Sub New(prov As IDataProvider)
            MyBase.New(prov)
            QueueGuid = prov.GetGuid("guid")
            Running = prov.GetValue("running", False)
            EncryptKeyID = prov.GetValue("encryptid", 0)
            IsActive = prov.GetValue("isactive", False)
            Dim parsedFeature As Feature
            RequiredFeature = If([Enum].TryParse(prov.GetValue("requiredFeature", ""), parsedFeature), parsedFeature, Feature.None)
        End Sub

        ''' <summary>
        ''' Creates a new, empty queue grouwp member
        ''' </summary>
        Public Sub New()
            Me.New(NullDataProvider.Instance)
        End Sub

        ''' <summary>
        ''' Creates a new queue group member based on data in the given queue
        ''' </summary>
        ''' <param name="wq">The work queue which contains the data from which this
        ''' group member should draw its values</param>
        Public Sub New(wq As clsWorkQueue)
            Me.New(NullDataProvider.Instance)
            Id = wq.Ident
            Name = wq.Name
            QueueGuid = wq.Id
            Running = wq.IsRunning
            EncryptKeyID = wq.EncryptionKeyID
            IsActive = wq.IsActive
        End Sub

        ''' <summary>
        ''' The old-style queue ID for this queue
        ''' </summary>
        <DataMember>
        Public Property QueueGuid As Guid
            Get
                Return GetData(DataNames.QueueGuid, Guid.Empty)
            End Get
            Set
                SetData(DataNames.QueueGuid, Value)
            End Set
        End Property

        ''' <summary>
        ''' Flag indicating if the queue represented by this member is running or not
        ''' </summary>
        <DataMember>
        Public Property Running As Boolean
            Get
                Return GetData(DataNames.Running, False)
            End Get
            Set
                SetData(DataNames.Running, Value)
            End Set
        End Property

        ''' <summary>
        ''' The name of the encryption used to encrypt this queue; null if the queue
        ''' is not encrypted.
        ''' </summary>
        <DataMember>
        Public Property EncryptKeyID As Integer
            Get
                Return GetData(DataNames.EncryptKeyID, 0)
            End Get
            Set
                SetData(DataNames.EncryptKeyID, Value)
            End Set
        End Property

        ''' <summary>
        ''' Flag indicating if this queue group member represents an active queue
        ''' </summary>
        <DataMember>
        Public Property IsActive As Boolean
            Get
                Return GetData(DataNames.IsActive, False)
            End Get
            Set
                SetData(DataNames.IsActive, Value)
            End Set
        End Property

        <DataMember>
        Public Property RequiredFeature As Feature
            Get
                Return GetData(DataNames.RequiredFeature, Feature.None)
            End Get
            Set
                SetData(DataNames.RequiredFeature, Value)
            End Set
        End Property


        ''' <summary>
        ''' The linking table between nodes of this type and groups. In this case,
        ''' the table is <c>BPAGroupQueue</c>.
        ''' </summary>
        Public Overrides ReadOnly Property LinkTableName As String
            Get
                Return "BPAGroupQueue"
            End Get
        End Property

        ''' <summary>
        ''' The image key to use for this object.
        ''' </summary>
        Public Overrides ReadOnly Property ImageKey As String
            Get
                Return If(IsActive,
                          ImageLists.Keys.Component.ActiveQueue,
                          ImageLists.Keys.Component.Queue)
            End Get
        End Property

        ''' <summary>
        ''' The group member type represented by this class
        ''' </summary>
        Public Overrides ReadOnly Property MemberType As GroupMemberType
            Get
                Return GroupMemberType.Queue
            End Get
        End Property

        ''' <summary>
        ''' Gets a dependency with which references to this group member can be
        ''' searched for, or null if this group member cannot be searched for
        ''' dependencies.
        ''' </summary>
        Public Overrides ReadOnly Property Dependency As clsProcessDependency
            Get
                If Name = "" Then Return Nothing
                Return New clsProcessQueueDependency(Name)
            End Get
        End Property


    End Class

End Namespace

