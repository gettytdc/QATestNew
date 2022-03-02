Imports System.Drawing
Imports System.ServiceModel
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.BackgroundJobs
Imports BluePrism.AutomateAppCore.clsServerPartialClasses
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.DataContracts
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.WorkQueues
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Scheduling.Calendar
Imports BluePrism.Scheduling
Imports BluePrism.Core.Network
Imports BluePrism.Common.Security
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.Sessions
Imports BluePrism.Data
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.Skills
Imports BluePrism.DataPipeline
Imports BluePrism.Data.DataModels.WorkQueueAnalysis
Imports BluePrism.DataPipeline.DataPipelineOutput
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.Processes
Imports BluePrism.AutomateAppCore.Logging
Imports BluePrism.Core.ActiveDirectory
Imports BluePrism.Core.ActiveDirectory.UserQuery
Imports BluePrism.Core.ActiveDirectory.DirectoryServices
Imports BluePrism.Core.Resources
Imports BluePrism.Server.Domain.Models
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping
Imports BluePrism.AutomateAppCore.Sessions
Imports BluePrism.ClientServerResources.Core.Config
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.Server.Domain.Models.Dashboard

''' Project  : AutomateAppCore
''' Class    : IServer
'''
''' <summary>
''' Definition of the interface provided to the application by Blue Prism Server. The
''' actual instance implementing it may be local or on a remote server.
'''
''' All parameters passed in and out of these methods are (when using a remote
''' server) subject to serialization via either .NET Remoting or WCF, which gives
''' us the following restrictions and requirements:
'''
''' * The interface MUST NOT contain any properties - only methods are acceptable.
'''
''' * All methods must be marked with an OperationContract attribute.
'''
''' * Overloads are allowed, but must be given unique names via the OperationContract.
'''   Before doing that, consider whether an overload is actually sensible.
'''
''' * Generic methods are not allowed. Each defined function (operation) must resolve
'''   to 'precisely' one published function.
'''
''' * Parameters (and their contents) must be concrete types (although ICollection is
'''   acceptable). All non-simple parameters need to be marked up with DataContract
'''   and DataMember attributes. Where a class contains references, it the
'''   DataContract should have IsReference enabled.
'''
''' * Where a parameter type, or member of, can resolve to multiple base classes,
'''   each of those needs to be marked up with KnownType attributes to let the WCF
'''   serializer know what types may appear. Alternatively, a NetDataContractSerialzier
'''   can be used.
'''
''' * Exceptions can be thrown. However, this should only happen in a case which is
'''   genuinuely an Exception - i.e. something compeltely unexpected happened.
'''   Throwing an exception will result in the current WCF connection faulting,
'''   requiring the client to reconnect (perhaps to a different server in the case of
'''   load balancing).
'''
''' * Reporting an error that is 'business as usual' should be done via a Fault - i.e.
'''   throwing a FaultException(Of YourFaultClass). This will be passed back cleanly
'''   as a SOAP fault which will allow the connection to remain intact. For an
'''   example of this, see QueueNotEmptyFault.
'''    TODO: need to rewrite that bit to cover the need to now use the appropriate
'''          wrapper (when Adrian pushes it)
'''
''' This interface also forms the boundary between client (Automate.exe,
''' AutomateC.exe, etc) and app server (Blue Prism Server). The client is assumed to
''' be untrusted, and all authentication and authorisation happens *inside* this
''' interface.
'''
''' General considerations:
'''
''' * No more information that necessary should be passed across this boundary. This
'''   is important in terms of security, bandwidth usage and complexity. Consider
'''   using more granular methods of accessing information, with more specific
'''   authorisation restrictions, where possible. For example, although "change your
'''   own password" and "change a user's password" are functionaly equivalent, they
'''   need different implementations with different permissions. Similarly, "get me
'''   a list of all user's names" is preferable to "get all user data and then only
'''   look at the name fields".
''' </summary>
<ServiceContract(SessionMode:=SessionMode.Required)>
<ServiceKnownType(GetType(Group))>
<ServiceKnownType(GetType(clsSortedSet(Of String)))>
<ServiceKnownType(GetType(User))>
<ServiceKnownType(GetType(List(Of clsValidationInfo)))>
<ServiceKnownType(GetType(MapUsersResult))>
Public Interface IServer
    Inherits IServerFeatures, IServerDataPipeline

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CheckSnapshotIsolationIsEnabledInDB() As Boolean


    ''' <summary>
    ''' True when this instance of clsServer is being accessed remotely. i.e. if you
    ''' are a client, and your instance of clsServer is hosted on a Blue Prism
    ''' Server, this will be True. In all other cases, it will be False.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsServer() As Boolean

    ''' <summary>
    ''' Returns the Fully Qualified Domain Name of Blue Prism Server currently running.
    ''' Server
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetServerFullyQualifiedDomainName() As String

    ''' <summary>
    ''' The database or server that we are connected to, in a label form
    ''' (ie. human-readable). When connecting to a Blue Prism Server, this will
    ''' reflect the server machine name, otherwise it will return the underlying
    ''' database.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetConnectedTo() As String

    ''' <summary>
    ''' Tests the validity of the current system database connection setting by
    ''' attempting a sample read and a sample write operation.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub Validate()

    ''' <summary>
    ''' This does nothing at all. (But calling it will keep the remote object alive
    ''' in a Blue Prism Server scenario!)
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub Nop()

    ''' <summary>
    ''' Get info on all possible Process Validation checks.
    ''' </summary>
    ''' <returns>A dictionary of ValidationInfo objects, with the key being the check
    ''' ID.</returns>
    ''' <remarks>Throws an exception if something goes wrong.</remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetValidationInfo() As IEnumerable(Of clsValidationInfo)

    ''' <summary>
    ''' Sets the validation info values for the given collection of objects.
    ''' </summary>
    ''' <param name="checks">The collection of validation info objects containing
    ''' the settings for the validation checks in this environment.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetValidationInfo(ByVal checks As ICollection(Of clsValidationInfo))

    ''' <summary>
    ''' Gets the categories defined for the validation rules
    ''' </summary>
    ''' <returns>Gets a map of validation category labels keyed against their
    ''' database IDs.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetValidationCategories() As IDictionary(Of Integer, String)

    ''' <summary>
    ''' Gets the types defined for the validation rules - effectively the severity
    ''' levels used in the validation rules.
    ''' </summary>
    ''' <returns>A map of validation severity labels keyed against their database IDs
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetValidationTypes() As IDictionary(Of Integer, String)

    ''' <summary>
    ''' Gets the supported actions for validation rules
    ''' </summary>
    ''' <returns>A map of action labels keyed against their database IDs</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetValidationActions() As IDictionary(Of Integer, String)

    ''' <summary>
    ''' Gets a map of actions (IDs) keyed against their severity levels for a given
    ''' category (again, by ID).
    ''' </summary>
    ''' <param name="category">The database ID of the category required.</param>
    ''' <returns>A map of action IDs keyed against the support severity level IDs.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetValidationActionSettings(ByVal category As Integer) As IDictionary(Of Integer, Integer)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetValidationAllActionSettings() As IDictionary(Of Integer, IDictionary(Of Integer, Integer))

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetValidationActionSetting(ByVal catid As Integer, ByVal typeid As Integer, ByVal actionid As Integer)

    ''' <summary>
    ''' Set the Resource Registration Mode.
    ''' </summary>
    ''' <param name="mode">The new mode.
    ''' </param>
    ''' <exception cref="ArgumentOutOfRangeException">If the mode is invalid.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetResourceRegistrationMode(ByVal mode As ResourceRegistrationMode)

    ''' <summary>
    ''' Get the Resource Registration Mode.
    ''' </summary>
    ''' <returns>The current mode.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceRegistrationMode() As ResourceRegistrationMode

    ''' <summary>
    ''' Get the address and port to use to connect to the given Resource.
    ''' </summary>
    ''' <param name="resourceName">The name of the Resource.</param>
    ''' <param name="hostname">On return, the hostname to connect to.</param>
    ''' <param name="portNo">On return, the port to connect to.</param>
    ''' <param name="ssl">On return, True if ssl is used.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub GetResourceAddress(ByVal resourceName As String, ByRef hostname As String,
                           ByRef portNo As Integer, ByRef ssl As Boolean, ByRef requiresSecure As Boolean)

    ''' <summary>
    ''' Gets the system preference for prevention of Resource registrations.
    ''' </summary>
    ''' <returns>The current value</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetPreventResourceRegistrationSetting() As Boolean

    ''' <summary>
    ''' Gets the system preference for requirement of secured Resource connections.
    ''' </summary>
    ''' <returns>The current value</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetRequireSecuredResourceConnections() As Boolean

    ''' <summary>
    ''' Gets the system preference in the database enforcing edit summaries
    ''' in process studio.
    ''' </summary>
    ''' <returns>True if successful</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEnforceEditSummariesSetting() As Boolean

    ''' <summary>
    ''' Gets the system preference in the database that determines whether process
    ''' XML should be compressed.
    ''' </summary>
    ''' <returns>True if successful</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCompressProcessXMLSetting() As Boolean

    ''' <summary>
    ''' Sets the system preference for prevention of Resource registrations.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetPreventResourceRegistrationSetting(value As Boolean)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetControllingUserPermissionSetting() As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetControllingUserPermissionSetting(value As Boolean)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AuditStartProcEngineSettingChange(value As Boolean)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AuditStartDatabaseConversion()
    ''' <summary>
    ''' Sets the system preference for requirement of secured Resource connections.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetRequireSecuredResourceConnections(value As Boolean)

    ''' <summary>
    ''' Gets the system preference for whether anonymous resource pc connections are
    ''' disallowed
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAllowAnonymousResources() As Boolean

    ''' <summary>
    ''' Sets the system preference for whether anonymous resource pc connections are
    ''' disallowed
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetAllowAnonymousResources(value As Boolean)


    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetTesseractEngine() As Integer


    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetTesseractEngine(value As Integer)

    ''' <summary>
    ''' Sets the system preference for whether pasting of passwords is allowed
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetAllowPasswordPasting(value As Boolean)

    ''' <summary>
    ''' Gets the system preference for whether pasting of passwords is allowed
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAllowPasswordPasting() As Boolean

    ''' <summary>
    ''' Sets the system preference in the database enforcing edit summaries
    ''' in process studio.
    ''' </summary>
    ''' <param name="value">Set to true to enforce the use of summaries;
    ''' false otherwise.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetEnforceEditSummariesSetting(ByVal value As Boolean)

    ''' <summary>
    ''' Gets all the toolstrip positions for the current user in the specified mode.
    ''' </summary>
    ''' <param name="objectStudio">True to return the toolstrip positions saved for
    ''' this user within object studio, False to return the process studio
    ''' equivalents</param>
    ''' <returns>The collection of UI Element positions stored for the logged in user
    ''' in the specified mode</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetToolStripPositions(ByVal objectStudio As Boolean) _
        As ICollection(Of clsUIElementPosition)

    ''' <summary>
    ''' Sets the toolstrip positions for the current user from the given collection.
    ''' </summary>
    ''' <param name="posns">The collection of position details to save</param>
    ''' <param name="objectStudio">True to save the toolstrip positions for object
    ''' studio mode, rather than process studio mode</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetToolStripPositions(
                                     ByVal posns As ICollection(Of clsUIElementPosition), ByVal objectStudio As Boolean)

    ''' <summary>
    ''' Function to check if versioned data has been updated or not.
    ''' This is handled by the version number held on the data tracking table,
    ''' which must be incremented by any function which updates the data managed
    ''' by the record in question.
    ''' </summary>
    ''' <param name="dataName">The name of the data registered on the version table.
    ''' </param>
    ''' <param name="currentVersion">The current version held in the system. This
    ''' will be updated as a ref parameter if it differs from the latest version
    ''' registered on the database.</param>
    ''' <returns>True if the data has been updated, ie. if the version number on the
    ''' database differs from the one specified; False otherwise.</returns>
    ''' <exception cref="ArgumentException">If the given data name was not found in
    ''' the data version table.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function HasDataUpdated(ByVal dataName As String, ByRef currentVersion As Long) As Boolean

    ''' <summary>
    ''' Gets the current data versions for all monitored data in the database.
    ''' </summary>
    ''' <returns>A mapping of version numbers against data names as currently held in
    ''' the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCurrentDataVersions() As IDictionary(Of String, Long)

    ''' <summary>
    ''' Gets the updated data versions for a given map of data names.
    ''' </summary>
    ''' <param name="currVers">The current versions held against the datanames that
    ''' they are versions of. Only these will be queried in the version query.
    ''' </param>
    ''' <returns>A map of the data names whose versions differ from those passed in,
    ''' along with the new version numbers assigned to those data names.;
    ''' Note that any which still have the same version number registered against
    ''' them on the database are not returned in this method.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetUpdatedDataVersions(
     currVers As IDictionary(Of String, Long)) As IDictionary(Of String, Long)

    ''' <summary>
    ''' Get the current archiving resource, used for automatic archiving.
    ''' </summary>
    ''' <param name="id">The new resource ID.</param>
    ''' <param name="folder">The new folder.</param>
    ''' <param name="age">The age of items to archive, e.g. "6m".</param>
    ''' <param name="delete">True to delete old items, False to archive them to
    ''' files in 'folder'.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetAutoArchivingSettings(ByVal id As Guid, ByVal folder As String, ByVal age As String, ByVal delete As Boolean, resourcecName As String)

    ''' <summary>
    ''' Get the current archiving resource, used for automatic archiving.
    ''' </summary>
    ''' <param name="resource">On return, contains the Resource ID.</param>
    ''' <param name="folder">On return, contains the folder.</param>
    ''' <param name="age">On return, contains the selected archiving age.</param>
    ''' <param name="delete">On return, contains True if deletion is required, False
    ''' if logs should be archived to 'folder'.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub GetAutoArchivingSettings(ByRef resource As Guid, ByRef folder As String, ByRef age As String, ByRef delete As Boolean)

    ''' <summary>
    ''' Set the archiving mode.
    ''' </summary>
    ''' <param name="auto">True to set the archiving mode to 'auto-archive'; False to
    ''' set it to 'manual archive'.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetAutoArchiving(ByVal auto As Boolean)

    ''' <summary>
    ''' Gets whether the current archiving mode is 'auto-archiving'.
    ''' </summary>
    ''' <returns>True if the currently configured archiving mode is auto-archiving;
    ''' False if it is set to manual archiving.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsAutoArchiving() As Boolean

    ''' <summary>
    ''' Set the archiving last complete time.
    ''' </summary>
    ''' <param name="d">The new time.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetArchivingLastComplete(ByVal d As DateTime)

    ''' <summary>
    ''' Get the last time auto-archiving was completed.
    ''' </summary>
    ''' <returns>The time of last completion.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetArchivingLastComplete() As DateTime

    ''' <summary>
    ''' Checks to see if archiving is already in progress somewhere. The field
    ''' BPASysConfig.ArchiveInProgress holds the name of the machine running an archive
    ''' process. If that machine is still 'alive', the local machine cannot do any
    ''' archiving. An Exception is thrown if there is any reason why the archiving
    ''' can't proceed. Otherwise, it can.
    ''' </summary>
    ''' <param name="aliveInterval">The 'alive interval' - if a machine is marked as
    ''' running an archive operation, but it hasn't registered with the database
    ''' within this many minutes, it will be considered 'inactive' and won't act as
    ''' a barrier to this machine proceeding.</param>
    ''' <exception cref="ArchiveAlreadyInProgressException">If any active machine has
    ''' an archive lock, and thus this machine cannot proceed with an archive
    ''' operation.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ArchiveCheckCanProceed(ByVal aliveInterval As Integer)

    ''' <summary>
    ''' Indicates whether the archive lock is currently in place (i.e. a resource is
    ''' currently performing an archive operation). If the lock is present then the
    ''' archiving resource name (and the time it last responded) are returned.
    ''' </summary>
    ''' <param name="resource">Returns the archiving resource name (if there is one)
    ''' </param>
    ''' <param name="lastUpdated">Returns the resources last response timestamp
    ''' (see BPAAliveResources)</param>
    ''' <returns>True if the archive lock is in place, otherwise False</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsArchiveLockSet(ByRef resource As String, ByRef lastUpdated As DateTime) As Boolean

    ''' <summary>
    ''' Acquires an archive lock for the current logged in machine.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AcquireArchiveLock()

    ''' <summary>
    ''' Releases the archive lock held by this resource (or optionally any resource)
    ''' </summary>
    ''' <param name="force">If set to True then the lock is released regardless of
    ''' whether it is held by this resource or not</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ReleaseArchiveLock(Optional force As Boolean = False)

    ''' <summary>
    ''' Gets all the session logs corresponding to the given session IDs.
    ''' Note that if the given collection contains the same session ID more than
    ''' once, only one session log object will be returned for that ID.
    ''' </summary>
    ''' <param name="ids">The collection of IDs for which the session logs are
    ''' required.</param>
    ''' <returns>The collection of session logs corresponding to the given IDs.
    ''' </returns>
    <OperationContract(Name:="GetSessionLogsIDs"), FaultContract(GetType(BPServerFault))>
    Function GetSessionLogs(ByVal ids As ICollection(Of Guid)) _
        As ICollection(Of clsSessionLog)

    ''' <summary>
    ''' Gets the lock info for the process with the Id specified, if locked. Otherwise will assign default values.
    ''' </summary>
    ''' <param name="processID">The Id of the process</param>
    ''' <param name="lockedBy">The Id of the user this process has been locked by</param>
    ''' <param name="machineLockedBy">The name of the machine this process was locked by</param>
    ''' <param name="lockedAt">The date and time the process was locked at</param>
    <OperationContract(Name:=NameOf(GetProcessLockInfo)), FaultContract(GetType(BPServerFault))>
    Sub GetProcessLockInfo(processID As Guid, ByRef lockedBy As Guid, ByRef machineLockedBy As String, ByRef lockedAt As Date)

    ''' <summary>
    ''' Gets session log objects found in the current database which satisfy the
    ''' given filter.
    ''' </summary>
    ''' <param name="fil">The filter to apply to the session logs. May be null or
    ''' empty to indicate that the collection of logs should not be filtered.</param>
    ''' <returns>The session logs which satisfy the given filter.</returns>
    <OperationContract(Name:="GetSessionLogsFilter"), FaultContract(GetType(BPServerFault))>
    Function GetSessionLogs(ByVal fil As clsSessionLogFilter) _
        As ICollection(Of clsSessionLog)

    ''' <summary>
    ''' Gets session log objects found in the current database which satisfy the
    ''' given filter.
    ''' </summary>
    ''' <param name="fil">The filter to apply to the session logs. May be null or
    ''' empty to indicate that the collection of logs should not be filtered.</param>
    ''' <returns>The session logs which satisfy the given filter.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionLogsTable(ByVal fil As clsSessionLogFilter) As DataTable

    ''' <summary>
    ''' Gets all the session logs which occurred between the given dates, and which
    ''' ran the given process (or any session if no process is given)
    ''' </summary>
    ''' <param name="fromDate">The first date from which session logs are required.
    ''' </param>
    ''' <param name="toDate">The last at which session logs are required.</param>
    ''' <param name="processId">The ID of the process for which the session logs
    ''' are required. If this is empty, then all sessions between the two dates
    ''' will be retrieved.</param>
    ''' <returns>A collection of session log objects representing the sessions that
    ''' occurred in between the given dates.</returns>
    <OperationContract(Name:="GetSessionLogsRange"), FaultContract(GetType(BPServerFault))>
    Function GetSessionLogs(
                                   ByVal fromDate As DateTime, ByVal toDate As DateTime, ByVal processId As Guid) _
        As ICollection(Of clsSessionLog)

    ''' <summary>
    ''' Restores the given session log (but not its entry data) into the database,
    ''' giving it a new session number.
    ''' </summary>
    ''' <param name="log">The log which should have a BPASession record restored to
    ''' represent it on the database. Note that the new session number is *not*
    ''' assigned to the log object by this method.</param>
    ''' <returns>The new session number assigned to the given session log in the
    ''' database..</returns>
    ''' <remarks>Note that no entries are restored as part of this session log
    ''' restoration, only the log and its own directly owned data.</remarks>
    ''' <exception cref="SessionAlreadyExistsException">If a session log with the
    ''' same session ID as the log provided already exists on the database.
    ''' </exception>
    ''' <exception cref="KeyNotFoundException">If a foreign key constraint failed,
    ''' meaning that a process, session or other related data didn't exist for the
    ''' given session log.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function RestoreSessionLog(ByVal log As clsSessionLog) As Integer

    ''' <summary>
    ''' Restores the session log data for the given collection log entries.
    ''' </summary>
    ''' <param name="entries">The entries which should be restored to the database.
    ''' </param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RestoreSessionLogData(ByVal entries As ICollection(Of clsSessionLogEntry))

    ''' <summary>
    ''' Gets the session log data for the given <paramref name="sessionNo">session
    ''' number</paramref> after the specified <paramref name="lastLogId">log Id
    ''' </paramref>. No more than the <paramref name="number">maximum number
    ''' </paramref> of records specified are retrieved.
    ''' </summary>
    ''' <param name="sessionNo">The session number of the session whose log entries
    ''' are required.</param>
    ''' <param name="lastLogId">The last logId - only log entries
    ''' beyond this value will be retrieved.</param>
    ''' <param name="number">The maximum number of records to retrieve.</param>
    ''' <returns>A collection of session log entries from the specified session,
    ''' starting from immediately after the last sequence number, and containing no
    ''' more elements than the maximum specified. If the returned collection is
    ''' empty, then there are no more log entries to return.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionLogData(sessionNo As Integer, lastLogId As Long, number As Integer, sessionLogMaxAttributeXmlLength As Integer) _
        As ICollection(Of clsSessionLogEntry)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionLogData_pre65(ByVal sessionNo As Integer, ByVal lastSeqNo As Integer, ByVal number As Integer) _
        As ICollection(Of clsSessionLogEntry)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionLogAttributeXml(sessionNo As Integer, logId As Long, offset As Long, number As Integer) _
        As ICollection(Of clsSessionLogEntry)

    ''' <summary>
    ''' Gets the session ID of any sessions which are marked as
    ''' <see cref="SessionStatus.Archived">archived</see>, meaning that they have
    ''' been archived and should have been deleted, but the operation was interrupted
    ''' </summary>
    ''' <returns>A collection of GUIDs representing the IDs of the orphaned sessions
    ''' in the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetOrphanedSessionIds() As ICollection(Of Guid)

    ''' <summary>
    ''' Gets the session number of any sessions which are marked as
    ''' <see cref="SessionStatus.Archived">archived</see>, meaning that they have
    ''' been archived and should have been deleted, but the operation was interrupted
    ''' </summary>
    ''' <returns>A collection of numbers representing the session numbers of the
    ''' orphaned sessions in the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetOrphanedSessionNumbers() As ICollection(Of Integer)

    ''' <summary>
    ''' Deletes the given session from the database, as well as any alerts which
    ''' refer to that session.
    ''' </summary>
    ''' <param name="sessionNo">The session number to delete</param>
    ''' <returns>The (total) number of log records deleted</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ArchiveSession(sessionNo As Integer) As Integer

    ''' <summary>
    ''' Deletes the given sessions from the database, as well as any alerts which
    ''' refer to that session.
    ''' </summary>
    ''' <param name="sessionNos">The session numbers to delete</param>
    ''' <returns>The (total) number of log records deleted</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ArchiveSessions(ByVal sessionNos As ICollection(Of Integer)) As Integer

    ''' <summary>
    ''' Copies archived data back into the database.
    ''' </summary>
    ''' <param name="dsSession">The archived data to be restored.</param>
    ''' <remarks>Throws an Exception in the event of failure.</remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ArchiveRestoreFromDataSet(ByVal dsSession As DataSet)

    ''' <summary>
    ''' Sets the configXML for a paticular resource (business object) in the database.
    ''' </summary>
    ''' <param name="sResourceName">The name of the business object,
    ''' e.g "CommonAutomation.clsWord"</param>
    ''' <param name="sConfigXML">The XML containing the config settings</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetResourceConfig(ByVal sResourceName As String, ByVal sConfigXML As String)

    ''' <summary>
    ''' Reads a resource's config details from the database.
    ''' </summary>
    ''' <param name="sResourceName">The resource name</param>
    ''' <returns>The config details</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceConfig(ByVal sResourceName As String) As String

    ''' <summary>
    ''' Reads resource details from the database.
    ''' </summary>
    ''' <param name="requiredAttributes">Attributes which returned rows must
    ''' possess. Set to none to be fully inclusive.</param>
    ''' <param name="unacceptableAttributes">Attributes which returned rows must
    ''' not possess. Set to none to be fully inclusive.</param>
    ''' <returns>A datatable with columns name, status, resourceid, attributeid and
    ''' pool if no error occurs, or Nothing if an error does occur.</returns>
    <OperationContract(Name:="GetResourcesAttr"), FaultContract(GetType(BPServerFault))>
    Function GetResources(requiredAttributes As ResourceAttribute,
                          unacceptableAttributes As ResourceAttribute,
                          robotName As String) As DataTable

    ''' <summary>
    ''' Returns resources registered on a specific machine.
    ''' </summary>
    ''' <param name="requiredAttributes">Attributes which returned rows must
    ''' possess. Set to none to be fully inclusive.</param>
    ''' <param name="unacceptableAttributes">Attributes which returned rows must
    ''' not possess. Set to none to be fully inclusive.</param>
    ''' <param name="hostName">The machine host name to search on</param>
    ''' <returns>A datatable with columns name, status, resourceid, attributeid and
    ''' pool if no error occurs, or Nothing if an error does occur.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourcesForHost(requiredAttributes As ResourceAttribute,
                            unacceptableAttributes As ResourceAttribute,
                            hostName As String) As DataTable

    ''' <summary>
    ''' Returns status information about all currently known resources
    ''' </summary>
    ''' <param name="requiredAttributes">Attributes which returned rows must
    ''' possess. Set to none to be fully inclusive.</param>
    ''' <param name="unacceptableAttributes">Attributes which returned rows must
    ''' not possess. Set to none to be fully inclusive.</param>
    ''' <param name="hostName">The machine host name to search on</param>
    ''' <returns>A collection of resourceInfo POCO objects.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceInfo(requiredAttributes As ResourceAttribute,
                             unacceptableAttributes As ResourceAttribute,
                             Optional hostName As String = Nothing) As ICollection(Of ResourceInfo)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceInfoCompressed(requiredAttributes As ResourceAttribute,
                             unacceptableAttributes As ResourceAttribute) As Byte()

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourcesData(resourceParameters As ResourceParameters) As IReadOnlyCollection(Of ResourceInfo)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceData(resourceId As Guid) As ResourceInfo

    ''' <summary>
    ''' Returns resources culture
    ''' </summary>
    ''' <returns>A collection of ResourceCulture</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourcesCulture() As ICollection(Of ResourceCulture)

    ''' <summary>
    ''' Mark the specified resource as retired
    ''' </summary>
    ''' <param name="resourceId">The ID of the resource to retire</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RetireResource(resourceId As Guid)

    ''' <summary>
    ''' Mark the specified resource as unretired
    ''' </summary>
    ''' <param name="resourceId">The ID of the resource to unretire</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UnretireResource(resourceId As Guid)

    ''' <summary>
    ''' Enables or disables the windows event logging on the specified resource.
    ''' </summary>
    ''' <param name="resourceId">The ID of the resource for which the event
    ''' logging should be enabled/disabled.</param>
    ''' <param name="enable">True to enable logging on the specified resource, false
    ''' to disable it. Note that if disabled, logging will still be written to the
    ''' textarea within the resource PCs' windows</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetResourceEventLogging(resourceId As Guid, enable As Boolean)

    ''' <summary>
    ''' Checks the event logging status of a specified resource.
    ''' </summary>
    ''' <param name="resourceId">The ID of the resource</param>
    ''' <returns>True if logging to windows event log is enabled for the specified
    ''' resource; False if it is disabled.</returns>
    ''' <exception cref="NoSuchElementException">If no resource was found with the
    ''' given resource ID.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsResourceEventLoggingEnabled(ByVal resourceId As Guid) As Boolean

    ''' <summary>
    ''' Register a resource PC with the database. Any existing registration with the
    ''' same name is overridden, unless the 'update' parameter is False.
    ''' </summary>
    ''' <param name="name">The name/address of the PC</param>
    ''' <param name="domainName">The FQDN of the PC</param>
    ''' <param name="status">The initial status code</param>
    ''' <param name="attributes">The attributes for the resource</param>
    ''' <param name="requiresSsl">True if the Resource requires ssl connections.</param>
    ''' <param name="updateResourceRecord">True to update the Resource record if a resource with
    ''' the given name already exists. Otherwise, giving the name of an existing
    ''' Resource will result in an error being returned.</param>
    ''' <param name="userId">The id of the user the resource was started as.
    ''' This will be guid.empty if it is a public resource</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RegisterResourcePC(name As String, domainName As String,
        status As ResourceMachine.ResourceDBStatus,
        requiresSsl As Boolean, attributes As ResourceAttribute,
        updateResourceRecord As Boolean, userId As Guid, currentculture As String)

    ''' <summary>
    ''' Get the refresh interval for refreshing runtime resource information
    ''' to the database.
    ''' This is used for both session state and runtime resource state.
    ''' </summary>
    ''' <returns>The required update frequency in seconds</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetRuntimeResourceRefreshFrequency() As Integer

    ''' <summary>
    ''' Get the refresh interval for refreshing runtime resource information
    ''' to the database.
    ''' This is used for both session state and runtime resource state.
    ''' </summary>
    ''' <param name="freqSeconds">The new refresh frequency in seconds</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetRuntimeResourceRefreshFrequency(freqSeconds As Integer)


    ''' <summary>
    ''' Allows status update of resource PC.
    ''' </summary>
    ''' <param name="name">Name of resource being refreshed</param>
    ''' <param name="status">Current status of resource</param>
    ''' <param name="runningSessions">Number of running sessions</param>
    ''' <param name="activeSessions">Number of running actions</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RefreshResourcePC(ByVal name As String, ByVal status As ResourceMachine.ResourceDBStatus,
        ByVal runningSessions As Integer, ByVal activeSessions As Integer)

    ''' <summary>
    ''' Refresh session info
    ''' </summary>
    ''' <param name="sessionNo">Identity of the session</param>
    ''' <param name="lastStage">Name of the last stage to run</param>
    ''' <param name="lastStageUpdated">Time at which the last stage started</param>
    ''' <param name="warningThreshold">The stage warning threshold in seconds</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RefreshSessionInfo(sessionNo As Integer, lastStage As String,
                           lastStageUpdated As DateTimeOffset, warningThreshold As Integer)

    ''' <summary>
    ''' Resource PCs call this on startup (after registration) and also periodically,
    ''' to keep track of pool information. If the Resource PC is a member of a pool
    ''' then information about the controller of the pool is returned. Additionally,
    ''' the Resource PC may take over controllership of the pool during this call.
    '''
    ''' In order to reduce unnecessary database load, information about the Resource
    ''' PC's intended diagnostics status is retrieved and returned at the same time,
    ''' as is auto-archiving status.
    ''' </summary>
    ''' <param name="resourceID">The ID of the Resource PC making the call.</param>
    ''' <param name="iscontroller">True if the Resource PC believes itself to be
    ''' the controller of the pool.</param>
    ''' <param name="poolID">On return, contains the ID of the Pool the Resource PC
    ''' is a member of, or Guid.Empty if none.</param>
    ''' <param name="controllerID">On return, contains the ID of the controller of
    ''' the pool. Will be Guid.Empty if the Resource PC is not a member of a pool,
    ''' or if the pool has no controller. The latter should not happen!</param>
    ''' <param name="iscontrollernow">On return, contains True if the Resource PC has
    ''' become the controller of its pool during the call.</param>
    ''' <param name="diags">On return, contains the diagnostics flags for the
    ''' Resource PC.</param>
    ''' <param name="isAutoArchiver">On return, contains True if the Resource is
    ''' designated as the auto-archiving machine.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub PoolUpdate(ByVal resourceID As Guid, ByVal iscontroller As Boolean, ByRef poolID As Guid, ByRef controllerID As Guid, ByRef iscontrollernow As Boolean, ByRef diags As Integer, ByRef isAutoArchiver As Boolean)


    ''' <summary>
    ''' Deregisters the resource PC with the given name from the database. This
    ''' happens when a resource PC shuts down - the record for the resource PC is not
    ''' physically removed, but the status is set to offline.
    ''' </summary>
    ''' <param name="name">The resource name</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeregisterLoginAgent(name As String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourcesPoolInfo() As IEnumerable(Of ResourcePoolInfo)


    ''' <summary>
    ''' Deregisters the resource PC with the given name from the database. This
    ''' happens when a resource PC shuts down - the record for the resource PC is not
    ''' physically removed, but the status is set to offline.
    ''' </summary>
    ''' <param name="name">The resource name</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeregisterResourcePC(name As String)

    ''' <summary>
    ''' Gets the IDs of the resource pools and their members.
    ''' </summary>
    ''' <returns>A dictionary mapping a set of resource IDs against their owning pool
    ''' IDs. The first entry in the collection representing the members will be the
    ''' currently registered controller of the pool, followed by the other members
    ''' in arbitrary order.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetPoolResourceIds() As IDictionary(Of Guid, ICollection(Of Guid))

    ''' <summary>
    ''' Get the name of the Resource that is the controller of given pool.
    ''' </summary>
    ''' <param name="poolid">The ID of the pool.</param>
    ''' <param name="name">On successful return, contains the controller name.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub GetPoolControllerName(ByVal poolid As Guid, ByRef name As String)

    ''' <summary>
    ''' Get information about a Resource's pool status.
    ''' </summary>
    ''' <param name="resid">The ID of the Resource to get information about.</param>
    ''' <param name="poolid">On return, contains the ID of the pool the Resource is
    ''' a member of, or Guid.Empty if the Resource is not a member of one.</param>
    ''' <param name="controllerid">On return, contains the ID of the controller of
    ''' that pool. This will be Guid.Empty if there is no controller, or no pool.
    ''' </param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub GetResourcePoolInfo(ByVal resid As Guid, ByRef poolid As Guid, ByRef controllerid As Guid)

    ''' <summary>
    ''' Add a Resource to a Resource Pool.
    ''' </summary>
    ''' <param name="poolName">The name of the pool to add it to.</param>
    ''' <param name="resourceName">The name of the Resource being added.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AddResourceToPool(poolName As String, resourceName As String)

    ''' <summary>
    ''' Remove a Resource from a Resource Pool.
    ''' </summary>
    ''' <param name="resourceName">The name of the Resource being removed.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RemoveResourceFromPool(resourceName As String)

    ''' <summary>
    ''' Create a Resource Pool in the database with the passed name
    ''' </summary>
    ''' <param name="poolName">The resource pool name</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub CreateResourcePool(poolName As String)

    ''' <summary>
    ''' Delete a Resource Pool from the database.
    ''' </summary>
    ''' <param name="poolName">The resource pool name</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteResourcePool(poolName As String)

    ''' <summary>
    ''' Get the ID of a Resource from the database, given its name.
    ''' </summary>
    ''' <param name="sResourceName">The Resource name</param>
    ''' <returns>The Resource ID, or Guid.Empty if the named Resource is not
    ''' registered in the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceId(ByVal sResourceName As String) As Guid

    ''' <summary>
    ''' Check if any of a set of Resources have an FQDN set.
    ''' </summary>
    ''' <param name="resourceIDs">A List of Resource IDs</param>
    ''' <returns>True if one or more of the given Resources have an FQDN set.
    ''' </returns>
    ''' <exception cref="Exception">An arbitrary exception might be thrown if the
    ''' database code fails for any reason.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ResourcesHaveFQDN(ByVal resourceIDs As ICollection(Of Guid)) As Boolean

    ''' <summary>
    ''' Reset the FQDN for the given Resource.
    ''' </summary>
    ''' <param name="resourceId">Resource ID</param>
    ''' <exception cref="Exception">An arbitrary exception might be thrown if the
    ''' database code fails for any reason.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ResetResourceFQDN(resourceId As Guid)

    ''' <summary>
    ''' Get the attributes of a resource from the database, given its name.
    ''' </summary>
    ''' <param name="resourceName">The Resource name</param>
    ''' <returns>The attributes of the resource.</returns>
    ''' <remarks>Throws an exception in the event of an error.</remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceAttributes(ByVal resourceName As String) As ResourceAttribute

    ''' <summary>
    ''' Get the diagnostics flags for the given Resource PC.
    ''' </summary>
    ''' <param name="resid">The ID of the Resource PC.</param>
    ''' <returns>The diagnostics flags.</returns>
    ''' <remarks>Throws an exception in the event of an error.</remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceDiagnostics(ByVal resid As Guid) As Integer

    ''' <summary>
    ''' Get the diagnostics flags and settings for the given Resource PCs.
    ''' </summary>
    ''' <param name="ids">The IDs of the Resource PCs</param>
    ''' <returns>The combined configuration settings for the specified resources.
    ''' </returns>
    ''' <remarks>Throws an exception in the event of an error.</remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceDiagnosticsCombined(ByVal ids As ICollection(Of Guid)) _
        As ResourceMachine.CombinedConfig

    ''' <summary>
    ''' Set the diagnostics flags for the given Resource PC.
    ''' </summary>
    ''' <param name="resourceId">The ID of the Resource PC.</param>
    ''' <param name="diags">The new diagnostics flags</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetResourceDiagnostics(resourceId As Guid, diags As Integer)

    ''' <summary>
    ''' Set the diagnostics flags for the given Resource PC. We provide checked and
    ''' unchecked parameters so that some flags can be left as is.
    ''' </summary>
    ''' <param name="resourceId">The ID of the Resource PC.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetResourceDiagnosticsCombined(resourceId As Guid, logLevel As Integer, logMemory As Integer, logForceGC As Integer, logWebServices As Integer)

    ''' <summary>
    ''' Reads a resource name from the database into a string.
    ''' </summary>
    ''' <param name="gResourceId">The resource id</param>
    ''' <returns>The name of the resource</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceName(ByVal gResourceId As Guid) As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceNameFromSessionId(sessionId As Guid) As String

    ''' <summary>
    ''' Gets all the web service definitions in this environment
    ''' </summary>
    ''' <returns>A collection of web service definitions in this environment.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetWebServiceDefinitions() As ICollection(Of clsWebServiceDetails)

    ''' <summary>
    ''' Get a collection of all web apis in this environment, containing just the
    ''' basic top level info that can be displayed on a list control
    ''' </summary>
    ''' <returns>A collection of Web APIs</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetWebApiListItems() As ICollection(Of WebApiListItem)

    ''' <summary>
    ''' Get a collection of all the web apis in this environment
    ''' </summary>
    ''' <returns>A collection of web apis</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetWebApis() As ICollection(Of WebApi)

    ''' <summary>
    ''' Gets the web api  with the given ID.
    ''' </summary>
    ''' <param name="id">The ID of the required web api</param>
    ''' <returns>The <see cref="WebApi"/> object represented by the given ID.
    ''' </returns>
    ''' <exception cref="NoSuchElementException">If no web api exists with
    ''' the specified ID.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetWebApi(ByVal id As Guid) As WebApi

    ''' <summary>
    ''' Gets the web service definition with the given ID.
    ''' </summary>
    ''' <param name="id">The ID of the required web service</param>
    ''' <returns>The web service details object represented by the given ID.
    ''' </returns>
    ''' <exception cref="NoSuchElementException">If no web service exists with
    ''' the specified ID.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetWebServiceDefinition(ByVal id As Guid) As clsWebServiceDetails

    ''' <summary>
    ''' Gets the ID of the web service with the given name.
    ''' </summary>
    ''' <param name="name">The name of the web service to retrieve.</param>
    ''' <returns>The ID of the web service with the given name or Guid.Empty, if no
    ''' such web service exists.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetWebServiceId(ByVal name As String) As Guid

    ''' <summary>
    ''' Get the id of a web api with the given name
    ''' </summary>
    ''' <param name="name">The name of the web api to return the id of</param>
    ''' <returns>The id of a web api</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetWebApiId(ByVal name As String) As Guid

    ''' <summary>
    ''' Saves the given web service to the database, overwriting the current
    ''' definition if it exists, creating a new one if it doesn't
    ''' </summary>
    ''' <param name="ws">The web service to save</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SaveWebServiceDefinition(ByVal ws As clsWebServiceDetails)

    ''' <summary>
    ''' Saves the given web api to the database, overwriting the current
    ''' web api if it exists, creating a new one if it doesn't
    ''' </summary>
    ''' <param name="webApi">The web api to save</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SaveWebApi(ByVal webApi As WebApi)

    ''' <summary>
    ''' Update the state of a particular web service, i.e. whether it is enabled or
    ''' not.
    ''' </summary>
    ''' <param name="gServiceID">The service identifier</param>
    ''' <param name="bEnabled">The new state</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateWebServiceEnabled(ByVal gServiceID As Guid, ByVal bEnabled As Boolean)
    ''' <summary>
    ''' Update the state of a particular web api, i.e. whether it is enabled or
    ''' not.
    ''' </summary>
    ''' <param name="webApiId">The id of the web api</param>
    ''' <param name="enabled">The new state</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateWebApiEnabled(ByVal webApiId As Guid, ByVal enabled As Boolean)

    ''' <summary>
    ''' Deletes a web service from the database.
    ''' </summary>
    ''' <param name="sServiceName">A name of the web service to delete</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteWebservice(ByVal sServiceName As String)

    ''' <summary>
    ''' Deletes a web api from the database.
    ''' </summary>
    ''' <param name="id">The ID of the web api to delete</param>
    ''' <param name="name">The name of the web api to delete</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteWebApi(id As Guid, name As String)

    ''' <summary>
    ''' Deletes web services from the database.
    ''' </summary>
    ''' <param name="services">A List containing the ids of the web services to
    ''' delete</param>
    ''' <remarks>This function can delete several services in one hit, which it does
    ''' by building a big SQL statement with OR statements. Not sure if this is very
    ''' efficient, but it works.</remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteWebservices(ByVal services As List(Of Guid))
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub InsertSkill(skill As Skill)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub InsertSkillVersion(version As WebSkillVersion, id As Guid)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateSkillEnabled(id As Guid, enabled As Boolean)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSkillsWithVersionLinkedToWebApi(webApiName As String, template As String) As IEnumerable(Of String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSkill(id As Guid) As Skill

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSkills() As IEnumerable(Of Skill)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDetailsForAllSkills() As IEnumerable(Of SkillDetails)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSkillVersionsWithWebApi(id As Guid) As IEnumerable(Of Guid)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteSkill(id As Guid, name As String)

    ''' <summary>
    ''' Returns whether session logging should write to the Unicode log table or not.
    ''' </summary>
    ''' <returns>True if unicode logging is enabled, otherwise false</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function UnicodeLoggingEnabled() As Boolean

    ''' <summary>
    ''' Enables/disables unicode support for session logs
    ''' </summary>
    ''' <param name="enable">True to enable unicode, or False to disable it</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateUnicodeLogging(enable As Boolean)

    ''' <summary>
    ''' Returns the number of rows in a given session
    ''' </summary>
    ''' <param name="sessNo">The session number</param>
    ''' <returns>The number of rows</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetLogsCount(ByVal sessNo As Integer) As Integer


    ''' <summary>
    ''' Gets a set of session log entries as text - actually a subsection of the
    ''' session log rather than the entire thing.
    ''' </summary>
    ''' <param name="sessNo">The session number of the session log required.</param>
    ''' <param name="startNo">The start of the session log to retrieve</param>
    ''' <param name="rowCount">The maximum number of rows to return</param>
    ''' <returns>A data table containing the required section of the specified
    ''' session log</returns>
    ''' <remarks>Apparnetly, log information is returned in a "minimal text format".
    ''' </remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetLogsAsText(ByVal sessNo As Integer, ByVal startNo As Integer, ByVal rowCount As Integer) As DataTable

    ''' <summary>
    ''' Gets a set of session log entries as a subsection of the
    ''' session log with paging.
    ''' </summary>
    ''' <param name="sessNo">The session number from which the log entries should be drawn.</param>
    ''' <param name="sessionLogsParameters">The parameters defining pagination</param>
    ''' <returns>A collection of log entries</returns>
    <OperationContract(Name:="GetPagedLogs"), FaultContract(GetType(BPServerFault))>
    Function GetLogs(ByVal sessNo As Integer, sessionLogsParameters As SessionLogsParameters) As ICollection(Of clsSessionLogEntry)

    ''' <summary>
    ''' Gets a subset of log entries in full verbose form
    ''' </summary>
    ''' <param name="sessNo">The session number from which the log entries should be
    ''' drawn.</param>
    ''' <param name="startNo">The start number of the session log entry to start
    ''' retrieving from.</param>
    ''' <param name="rowCount">The maximum number of rows to return from the session
    ''' log.</param>
    ''' <returns>A DataTable containing the full data from the required session log,
    ''' capturing all of the specified log entries.</returns>
    <OperationContract(Name:="GetLogs"), FaultContract(GetType(BPServerFault))>
    Function GetLogs(ByVal sessNo As Integer, ByVal startNo As Integer, ByVal rowCount As Integer) As DataTable

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionAttributeXml(sessionNumber As Integer, logId As Long) As String

    ''' <summary>
    ''' Updates the end date and attribute xml or a log record.
    ''' </summary>
    ''' <param name="sessionNo">The session number</param>
    ''' <param name="logId">The log entry identifier</param>
    ''' <param name="attrXml">The XML describing the attributes for this log update;
    ''' empty implies no attributes to set.</param>
    ''' <param name="endDate">The datetime/offset that the session entry ended</param>
    ''' <param name="unicode">Whether to log in unicode table.</param>
    ''' <param name="resourceId">The resource id</param>
    ''' <param name="resourceName">The resource name</param>
    ''' <param name="processName">The process or subprocess name</param>
    ''' <param name="stageId">The stage ID</param>
    ''' <param name="stageName">The stage name</param>
    ''' <param name="stageType">The stage type</param>
    ''' <param name="sheetName">The sheet name</param>
    ''' <param name="objectName">The object name</param>
    ''' <param name="actionName">The action type</param>
    ''' <param name="startDate">The datetime/offset the stage started</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateLog(sessionNo As Integer, logId As Long, attrXml As String, endDate As DateTimeOffset, unicode As Boolean,
                  resourceId As Guid, resourceName As String, processName As String, stageId As Guid, stageName As String, stageType As StageTypes, sheetName As String, objectName As String, actionName As String, startDate As DateTimeOffset)

    ''' <summary>
    ''' Creates a log record.
    ''' </summary>
    ''' <param name="iSessionNumber">The session number</param>
    ''' <param name="gResourceId">The resource id</param>
    ''' <param name="sResourceName">The resource name</param>
    ''' <param name="gStageId">The stage ID</param>
    ''' <param name="sStageName">The stage name</param>
    ''' <param name="iStageType">The stage type</param>
    ''' <param name="sResult">The result/message</param>
    ''' <param name="iResultType">The result data type</param>
    ''' <param name="sAttributeXML">The inputs or outputs</param>
    ''' <param name="sProcessName">The process or subprocess name</param>
    ''' <param name="sPageName">The page name</param>
    ''' <param name="sObjectName">The object name</param>
    ''' <param name="sActionName">The action name</param>
    ''' <param name="loginfo">Logging information</param>
    ''' <param name="startDate">The datetime/offset the stage started</param>
    ''' <returns>The sequence number for the log entry that can be used (together
    ''' with the session number) to identify the record created</returns>
    ''' <exception cref="AlreadyExistsException">If an entry already exists in the
    ''' session log table with the given session number and sequence number combo.
    ''' </exception>
    ''' <exception cref="Exception">If any other errors occur while attempting to
    ''' write the log entry to the database</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function LogToDB(
                       iSessionNumber As Integer,
                       gResourceId As Guid,
                       sResourceName As String,
                       gStageId As Guid, sStageName As String,
                       iStageType As StageTypes, sResult As String,
                       iResultType As DataType, loginfo As LogInfo,
                       startDate As DateTimeOffset,
                       Optional ByVal sAttributeXML As String = "",
                       Optional ByVal sProcessName As String = "",
                       Optional ByVal sPageName As String = "",
                       Optional ByVal sObjectName As String = "",
                       Optional ByVal sActionName As String = "",
                       Optional unicode As Boolean = False) As Long

    ''' <summary>
    ''' Search a single session log for the first occurrence of a value in any field
    ''' </summary>
    ''' <param name="sessNo">The session to search</param>
    ''' <param name="term">The value to seach for</param>
    ''' <returns>The logid of the first log row that
    ''' matches the search term, or -1 if no result is found.</returns>
    ''' <exception cref="Exception">If any errors occur</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SearchSession(ByVal sessNo As Integer, ByVal term As String) As Long


    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function LogIdToLogNumber(sessionNumber As Integer, logId As Long) As Integer

    ''' <summary>
    ''' Search a single session log for the first occurrence of a value.
    ''' </summary>
    ''' <param name="sessNo">The session to search</param>
    ''' <param name="fields">The names of the fields to search</param>
    ''' <param name="term">The value to seach for</param>
    ''' <returns>The logid of the first log row that
    ''' matches the search term, or -1 if no result is found.</returns>
    ''' <exception cref="ArgumentNullException">If the given <paramref name="term"/>
    ''' is null or empty, or the <paramref name="fields"/> collection is null.
    ''' </exception>
    ''' <exception cref="EmptyCollectionException">If the <paramref name="fields"/>
    ''' collection was empty.</exception>
    ''' <exception cref="Exception">If any other errors occur</exception>
    <OperationContract(Name:="SearchSessionFields"), FaultContract(GetType(BPServerFault))>
    Function SearchSession(ByVal sessNo As Integer, ByVal fields As ICollection(Of String), ByVal term As String) As Long

    ''' <summary>
    ''' Reads a period of audit records from the database into a datatable.
    ''' The datatable has the following columns:
    ''' Time            (datetime)
    ''' Narrative       (string)
    ''' Comments        (string)
    ''' </summary>
    ''' <param name="startDateTime">The start date</param>
    ''' <param name="endDateTime">The end date</param>
    ''' <returns>The table of data containing the required audit logs.</returns>
    ''' <exception cref="Exception">If any errors occur while attempting to read
    ''' the audit log data</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAuditLogsByDateRange(ByVal startDateTime As Date, ByVal endDateTime As Date) As DataTable

    ''' <summary>
    ''' Reads a period of audit records from the database into a datatable.
    ''' The datatable has the following columns:
    ''' Time            (datetime)
    ''' Narrative       (string)
    ''' Comments        (string)
    ''' </summary>
    ''' <param name="startDateTime">The start date</param>
    ''' <param name="endDateTime">The end date</param>
    ''' <param name="dtlogs">The datatable. May be null on return, particularly
    ''' if function returns false.</param>
    ''' <param name="sErr">The error message</param>
    ''' <returns>True if successful</returns>
    <OperationContract(Name:="GetAuditLogDataWithErr"), FaultContract(GetType(BPServerFault))>
    Sub GetAuditLogData(ByVal startDateTime As Date, ByVal endDateTime As Date, ByRef dtlogs As DataTable)

    ''' <summary>
    ''' Gets the xml of a past version of a process (or business object) from the
    ''' audit table.
    ''' </summary>
    ''' <param name="eventId">The eventID at which to retrieve the xml. The
    ''' data will be taken from the 'newxml' column recorded against this id.</param>
    ''' <param name="processId">The process id of the process (or busines object).
    ''' This is required for data integrity checks.</param>
    ''' <returns>Returns the xml of the requested process or an empty string if no
    ''' such event id was found in the audit event table.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessHistoryXML(eventId As Integer, processId As Guid) As String

    ''' <summary>
    ''' Populates a datatable with a list of versions of a specific Automate Process.
    ''' Called by ctlLogs.PopulateHistoryListView to get datetimes at which a specific
    ''' process was modified
    ''' </summary>
    ''' <param name="procId">The ID of the process of interest</param>
    ''' <returns>The datatable to be populated. Columns will be EventDateTime,
    ''' EventID, sCode, username, EditSummary, ordered by eventdatetime descending.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessHistoryLog(ByVal procId As Guid) As DataTable

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetUserUILayoutPreferences() As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetUserUILayoutPreferences(preferences As String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetLogViewerHiddenColumns() As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetLogViewerHiddenColumns(iColumns As Integer)

    ''' <summary>
    ''' Checks if a 'stop request' has been made for the session with the given
    ''' identity, indicating that the request has been acknowledged if such a request
    ''' has been made.
    ''' </summary>
    ''' <param name="sessionNo">The identity of the session to check for a stop
    ''' request.</param>
    ''' <returns>True if a stop request has been entered for this session; False if
    ''' no such stop request is found, or if the session number did not correspond to
    ''' a valid session.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsStopRequested(sessionNo As Integer) As Boolean

    ''' <summary>
    ''' Checks if a 'stop request' has been made for the session with the given
    ''' identity.
    ''' </summary>
    ''' <param name="sessionNo">The identity of the session to check for a stop
    ''' request.</param>
    ''' <param name="markAcknowledged">True to set an flag indicating that the
    ''' request has been acknowledged; False to leave the flag untouched. Note that
    ''' the ack flag is only set once - ie. if it is already set, it will not be
    ''' overwritten by this method.</param>
    ''' <returns>True if a stop request has been entered for this session; False if
    ''' no such stop request is found, or if the session number did not correspond to
    ''' a valid session.</returns>
    <OperationContract(Name:="IsStopRequestedWithAck"), FaultContract(GetType(BPServerFault))>
    Function IsStopRequested(sessionNo As Integer, markAcknowledged As Boolean) As Boolean

    ''' <summary>
    ''' Requests a 'safe stop' of a session
    ''' </summary>
    ''' <param name="sessionNo">The identity of the session for which a stop request
    ''' should be made</param>
    ''' <returns>True if the stop request was made as a result of this call; False if
    ''' the stop request was not made by this call, either because a stop request has
    ''' already been made on the session, or no running session with the given
    ''' number was found.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function RequestStopSession(sessionNo As Integer) As Boolean

    ''' <summary>
    ''' Gets the session details for the session with the given ID.
    ''' </summary>
    ''' <param name="sessionNo">The sessionnumber value for which the session data is
    ''' required.</param>
    ''' <returns>A SessionData object containing the data for the specified session,
    ''' or null if the session was not found on the database.</returns>
    <OperationContract(Name:="GetSessionDetailsByNo"), FaultContract(GetType(BPServerFault))>
    Function GetSessionDetails(ByVal sessionNo As Integer) As clsServer.SessionData

    ''' <summary>
    ''' Gets the session details for the session with the given ID.
    ''' </summary>
    ''' <param name="sessionId">The ID for which the session data is required.
    ''' </param>
    ''' <returns>A SessionData object containing the data for the specified session,
    ''' or null if the session was not found on the database.</returns>
    <OperationContract(Name:="GetSessionDetailsByID"), FaultContract(GetType(BPServerFault))>
    Function GetSessionDetails(ByVal sessionId As Guid) As clsServer.SessionData

    ''' <summary>
    ''' Gets the name of the resource which ran a session.
    ''' </summary>
    ''' <param name="sessId">The session ID for which the running resource name is
    ''' required.</param>
    ''' <returns>The name of the resource set as running the session, or an empty
    ''' string if no resource was found.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionResourceName(sessId As Guid) As String

    ''' <summary>
    ''' Gets the ID of the resource that this session is set to run on. This is the
    ''' *actual* resource, regardless of pool status - i.e. if the session ran on a
    ''' pool this will be the pool member doing the running, not the pool itself.
    ''' </summary>
    ''' <param name="sessionID">The session id</param>
    ''' <returns>The name ID the resource that this session is set to run on
    ''' or Guid.Empty if no resource can be found</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionResourceID(ByVal sessionID As Guid) As Guid

    ''' <summary>
    ''' Changes the user responsible for a session
    ''' </summary>
    ''' <param name="sessionid">The session id</param>
    ''' <param name="userid">The user id</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetSessionUserID(ByVal sessionid As Guid, ByVal userid As Guid)

    ''' <summary>
    ''' Updates a session's status to 'Running'.
    ''' </summary>
    ''' <param name="gsessionid">The session id</param>
    ''' <param name="startDateTime">The date/time (UTC) that the session started</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetPendingSessionRunning(gsessionid As Guid, startDateTime As DateTimeOffset)

    ''' <summary>
    ''' Writes a session record into the database.
    ''' </summary>
    ''' <param name="processId">The process id</param>
    ''' <param name="starterResourceId">The starting resource id</param>
    ''' <param name="startDateTime">The date/time (UTC) that the session started</param>
    ''' <param name="runningResourceId">The running resource id</param>
    ''' <param name="sessionID">The session id</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub CreateDebugSession(
        processId As Guid, starterResourceId As Guid,
        runningResourceId As Guid, startDateTime As DateTimeOffset,
        sessionID As Guid, ByRef sessNo As Integer)

    ''' <summary>
    ''' Finishes a session in the database, either by updating the end time or by
    ''' deleting it when it has not produced any log data.
    ''' </summary>
    ''' <param name="gSessionID">The session id</param>
    ''' <param name="endDateTime">The date/time that the session finished as a datetimeoffset</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub FinishDebugSession(ByVal gSessionID As Guid, endDateTime As DateTimeOffset)

    ''' <summary>
    ''' Writes a session record into the database.
    ''' </summary>
    ''' <param name="processId">The process id</param>
    ''' <param name="queueIdent">The identity of the active queue for which this is a
    ''' pending session; 0 if the session is not on behalf of an active queue.
    ''' </param>
    ''' <param name="token">An authorisation token</param>
    ''' <param name="startingResource">The starting resource id</param>
    ''' <param name="runningResource">The running resource id</param>
    ''' <param name="startDateTime">The date/time (UTC) that the session started</param>
    ''' <param name="sessionId">The session ID created</param>
    ''' <param name="sessNo">The session number created</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessXmlForCreatedSession(
        processId As Guid,
        queueIdent As Integer,
        token As String,
        startingResource As Guid,
        runningResource As Guid,
        startDateTime As DateTimeOffset,
        sessionId As Guid,
        ByRef sessNo As Integer) As String

    <OperationContract(Name:="CreatePendingSessionLegacy"), FaultContract(GetType(BPServerFault))>
    Sub CreatePendingSession(
        processId As Guid,
        queueIdent As Integer,
        startingUserId As Guid,
        startingResource As Guid,
        runningResource As Guid,
        startDateTime As DateTimeOffset,
        sessionId As Guid,
        ByRef sessNo As Integer)

    ''' <summary>
    ''' Gets the session ID corresponding to the given session number.
    ''' </summary>
    ''' <param name="sessionNumber">The number for which the session ID is
    ''' required.</param>
    ''' <returns>The Session ID corresponding to the given number.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionID(ByVal sessionNumber As Integer) As Guid

    ''' <summary>
    ''' Gets the session number
    ''' </summary>
    ''' <param name="gSessionId">Session ID</param>
    ''' <returns>Session number, or -1 if an error occurs</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionNumber(ByVal gSessionId As Guid) As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionScheduleNumber(sessid As Guid) As Integer 
    ''' <summary>
    ''' Gets the session numbers for the sessions identified by the given IDs
    ''' </summary>
    ''' <param name="ids">The IDs for which the session numbers are required.
    ''' </param>
    ''' <returns>The collection of session numbers corresponding to the given
    ''' session IDs.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionNumbers(ByVal ids As ICollection(Of Guid)) As ICollection(Of Integer)

    ''' <summary>
    ''' Writes a statistic into the database, either as a new record or by updating
    ''' an existing one.
    ''' </summary>
    ''' <param name="gSessionId">The session id</param>
    ''' <param name="sName">The statistic name</param>
    ''' <param name="sValue">The statistic value</param>
    ''' <param name="sDataType">The statistic data type</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateStatistic(ByVal gSessionId As Guid, ByVal sName As String, ByVal sValue As String, ByVal sDataType As String)

    ''' <summary>
    ''' Update alert configuration information
    ''' </summary>
    ''' <param name="user">The user object to update</param>
    ''' <param name="processes">A collection of process IDs the user is interested in
    ''' </param>
    ''' <param name="schedules">The IDs of the schedules that the user is interested
    ''' in.</param>
    ''' <exception cref="SqlClient.SqlException">If any database errors occur while attempting
    ''' to udpate the alert configuration.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateAlertConfig(
                                 ByVal user As User,
                                 ByVal processes As ICollection(Of Guid),
                                 ByVal schedules As ICollection(Of Integer))

    ''' <summary>
    ''' Get alert history information.
    ''' </summary>
    ''' <param name="user">The user of interest.</param>
    ''' <param name="historyDate">The date (local time) its converted to utc
    ''' and then results are returned between this date and 1 day ahead of
    ''' this date</param>
    ''' <returns>A DataTable containing the information.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAlertHistory(user As User, historyDate As DateTime) As DataTable

    ''' <summary>
    ''' Get process details for the alerts form.
    ''' </summary>
    ''' <param name="userid">The ID of the user in question</param>
    ''' <returns>A DataTable containing the information.</returns>
    ''' <exception cref="SqlClient.SqlException">If any database errors occur while attempting
    ''' to udpate the alert configuration.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAlertProcessDetails(ByVal userid As Guid) As DataTable

    ''' <summary>
    ''' Gets the schedule IDs for all of the schedules that the given user has an
    ''' alert subscription to.
    ''' </summary>
    ''' <param name="userid">The ID of the user for which the their schedule
    ''' subscription data is required.</param>
    ''' <returns>A collection of integers representing schedule IDs that the user is
    ''' subscribed to within the alerts system.</returns>
    ''' <exception cref="SqlClient.SqlException">If any database errors occur while attempting
    ''' to udpate the alert configuration.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSubscribedScheduleAlerts(ByVal userid As Guid) As ICollection(Of Integer)

    ''' <summary>
    ''' Update and acknowledge alerts for a particular user.
    ''' </summary>
    ''' <param name="sessionsToIgnore">A comma-separated list of session IDs to be
    ''' ignored. Yes, that's right, it really is a comma-separated list of Guids in
    ''' String form. Or an empty string.</param>
    ''' <param name="resourceid">The Resource ID</param>
    ''' <param name="user">The User</param>
    ''' <returns>A DataTable containing new alerts, or Nothing if an error
    ''' occurred.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function UpdateAndAcknowledgeAlerts(
                                               ByVal sessionsToIgnore As ICollection(Of Guid),
                                               ByVal resourceid As Guid,
                                               ByVal user As User) As DataTable

    ''' <summary>
    ''' Creates a stage alert for the given session with the specified message.
    ''' This will use the resource ID running the supplied session as the resource
    ''' for the alert.
    ''' </summary>
    ''' <param name="sessionId">The ID of the session which generated the alert.
    ''' </param>
    ''' <param name="message">The message to write to the alert.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub CreateStageAlert(sessionId As Guid, message As String)

    ''' <summary>
    ''' Creates an alert of the given type for the given session, overriding the
    ''' resource ID with the given value and using the default message for that
    ''' event type.
    ''' The message for this alert will be determined by the
    ''' <see cref="clsServer.GetAlertEventTypeMessage"/> method.
    ''' </summary>
    ''' <param name="type">The event type of the alert</param>
    ''' <param name="sessionId">The ID of the session which generated the alert.
    ''' </param>
    ''' <param name="resourceId">The ID of the resource to log in this alert. If
    ''' this is left <see cref="Guid.Empty">empty</see>, the running resource ID
    ''' from the session will be used.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub CreateProcessAlert(type As AlertEventType, sessionId As Guid,
                           ByVal resourceId As Guid)

    ''' <summary>
    ''' Creates an alert of the given type for the given session, overriding the
    ''' resource ID and process ID with the given value.
    ''' </summary>
    ''' <param name="type">The event type of the alert</param>
    ''' <param name="sessionId">The ID of the session which generated the alert.
    ''' </param>
    ''' <param name="resourceId">The ID of the resource to log in this alert. If
    ''' this is left <see cref="Guid.Empty">empty</see>, the running resource ID
    ''' from the session will be used.</param>
    ''' <param name="processId">The ID of the process to log in this alert. If
    ''' this is left <see cref="Guid.Empty">empty</see>, the process ID from the
    ''' session will be used.</param>
    ''' <param name="message">The message to write to the alert.</param>
    <OperationContract(Name:="CreateProcessAlertWithMessage"), FaultContract(GetType(BPServerFault))>
    Sub CreateProcessAlert(
                                       ByVal type As AlertEventType,
                                       ByVal sessionId As Guid,
                                       ByVal resourceId As Guid,
                                       ByVal processId As Guid,
                                       ByVal message As String)

    ''' <summary>
    ''' Creates a schedule alert with the given parameters
    ''' </summary>
    ''' <param name="type">The type of event to create</param>
    ''' <param name="scheduleId">The ID of the schedule to which this alert event
    ''' should refer. This should always be populated even if the alert event is
    ''' for a task.</param>
    ''' <param name="taskId">The ID of the task to which this alert event should
    ''' refer, zero if no task is referred to.</param>
    ''' <exception cref="ArgumentException">If the given schedule ID is zero.
    ''' </exception>
    ''' <exception cref="SqlClient.SqlException">If any errors occur on the database while
    ''' attempting to create schedule alert.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub CreateScheduleAlert(
                                   ByVal type As AlertEventType,
                                   ByVal scheduleId As Integer,
                                   ByVal taskId As Integer)

    ''' <summary>
    ''' Gets a list of the alerts machines from the BPAAlertsMachines
    ''' table.
    ''' </summary>
    ''' <returns>A list of machine names.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAlertsMachines() As List(Of String)

    ''' <summary>
    ''' Deletes process alerts machines from the BPAAlertsMachines table.
    ''' </summary>
    ''' <param name="machines">The list of machines to be deleted.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteAlertsMachines(machines As List(Of String))

    ''' <summary>
    ''' Determines if the named machine is registered for process alerts.
    ''' </summary>
    ''' <param name="MachineName">The name of the machine to be checked.</param>
    ''' <returns>Returns True if registered, False otherwise.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsAlertMachineRegistered(ByVal MachineName As String) As Boolean

    ''' <summary>
    ''' Registers the supplied machine for use in process alerts.
    ''' </summary>
    ''' <param name="MachineName">The hostanme of the machine to be registered.
    ''' </param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RegisterAlertMachine(ByVal MachineName As String)

    ''' <summary>
    ''' Clears any previously unacknowledged alerts for a particular user.
    ''' </summary>
    ''' <param name="userid">The ID of the user to delete alerts for.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ClearOldAlerts(ByVal userid As Guid)

    ''' <summary>
    ''' Updates a session's status to 'Terminated'. In the real world, this means
    ''' 'Failed' as a result of an unhandled runtime error (an exception in the
    ''' process).
    ''' </summary>
    ''' <param name="sessId">The session id</param>
    ''' <param name="endDateTime">The date/time (UTC) that the session finished</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetSessionTerminated(sessId As Guid, endDateTime As DateTimeOffset, sessionExceptionDetail As SessionExceptionDetail)

    ''' <summary>
    ''' Updates a session's status to 'Completed'.
    ''' </summary>
    ''' <param name="sessId">The session id</param>
    ''' <param name="endDateTime">The date/time (UTC) that the session finished</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetSessionCompleted(sessId As Guid, endDateTime As DateTimeOffset)

    ''' <summary>
    ''' Updates a session's status to 'Stopped'.
    ''' </summary>
    ''' <param name="sessId">The session id</param>
    ''' <param name="endDateTime">The date/time (UTC) that the session finished</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetSessionStopped(sessId As Guid, endDateTime As DateTimeOffset)

    ''' <summary>
    ''' Deletes a session from the database.
    ''' </summary>
    ''' <param name="sessId">The session id</param>
    <OperationContract(Name:="DeleteSessionByID"), FaultContract(GetType(BPServerFault))>
    Sub DeleteSession(sessId As Guid)


    ''' <summary>
    ''' Deletes a session from the database.
    ''' </summary>
    ''' <param name="token">An authorisation token</param>
    ''' <param name="sessId">The session id</param>
    <OperationContract(Name:="DeleteSessionByIDAs"), FaultContract(GetType(BPServerFault))>
    Sub DeleteSessionAs(token As String, sessId As Guid)

    ''' <summary>
    ''' Get the startup parameters (in XML format) that were used to start a given
    ''' session.
    ''' </summary>
    ''' <param name="sessionID">The ID of the session.</param>
    ''' <returns>The startup parameters used, or Nothing if there weren't any. Throws
    ''' an exception if something goes wrong.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionStartParams(ByVal sessionID As Guid) As String

    ''' <summary>
    ''' Get the status of the specified session
    ''' </summary>
    ''' <param name="sessId">The ID of a particluar session to get information about
    ''' </param>
    ''' <returns>The session status, or SessionStatus.All if unknown</returns>
    <OperationContract(Name:="GetSessionStatusByID"), FaultContract(GetType(BPServerFault))>
    Function GetSessionStatus(ByVal sessId As Guid) As SessionStatus

    ''' <summary>
    ''' Gets the statuses of all identified sessions and returns the dictionary
    ''' containing that status.
    ''' </summary>
    ''' <param name="dict">The dictionary containing the Session IDs for which the
    ''' session statuses are required</param>
    ''' <returns>The given dictionary, populated with the current session statuses.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionStatus(ByVal dict As IDictionary(Of Guid, SessionStatus)) _
        As IDictionary(Of Guid, SessionStatus)

    <OperationContract(Name:="GetSessionsForResource"), FaultContract(GetType(BPServerFault))>
    Function GetSessionsForResource(resourceName As String) As ICollection(Of clsProcessSession)

    <OperationContract(Name:="GetSessionsForAllResources"), FaultContract(GetType(BPServerFault))>
    Function GetSessionsForAllResources() As ICollection(Of clsProcessSession)

    ''' <summary>
    ''' Gets the first Session ID found matching the incomplete GUID string.
    ''' </summary>
    ''' <param name="sIncomplete">An incomplete GUID string.</param>
    ''' <returns>The first matching GUID found, or Guid.Empty if no match is found.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCompleteSessionID(ByVal sIncomplete As String) As Guid

    ''' <summary>
    ''' Gets the first Resource ID found matching the incomplete GUID string.
    ''' </summary>
    ''' <param name="sIncomplete">An incomplete GUID string.</param>
    ''' <returns>The first matching GUID found, or Guid.Empty if no match is found.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCompleteResourceID(ByVal sIncomplete As String) As Guid

    ''' <summary>
    ''' Gets the first Process ID found matching the incomplete GUID string.
    ''' </summary>
    ''' <param name="sIncomplete">An incomplete GUID string.</param>
    ''' <returns>The first matching GUID found, or Guid.Empty if no match is found.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCompleteProcessID(ByVal sIncomplete As String) As Guid

    ''' <summary>
    ''' Gets the first Process ID found matching the incomplete GUID string.
    ''' </summary>
    ''' <param name="sIncomplete">An incomplete Guid string.</param>
    ''' <returns>The first matching Guid found, or Guid.Empty if no match is found.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCompletePoolID(ByVal sIncomplete As String) As Guid

    ''' <summary>
    ''' Gets the first User ID found matching the incomplete GUID string.
    ''' </summary>
    ''' <param name="sIncomplete">An incomplete GUID string.</param>
    ''' <returns>The first matching GUID found, or Guid.Empty if no match is found.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCompleteUserID(ByVal sIncomplete As String) As Guid

    ''' <summary>
    ''' Gets the sessions created on behalf of the queue with the given identity.
    ''' </summary>
    ''' <param name="ident">The identity of the active queue for which the
    ''' current sessions are required.</param>
    ''' <param name="statuses">The statuses of the sessions to return. If none are
    ''' supplied, all sessions will be retrieved</param>
    ''' <returns>A collection of process sessions which were created on behalf of the
    ''' given queue</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionsForQueue(ident As Integer, ParamArray statuses() As SessionStatus) As ICollection(Of clsProcessSession)

    ''' <summary>
    ''' Gets session with the given ID.
    ''' </summary>
    ''' <param name="sessionId">The sessionid value for which the session data is
    ''' required.</param>
    ''' <returns>A clsProcessSession object containing the data for the specified session,
    ''' or null if the session was not found on the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetActualSessionById(sessionId As Guid) As clsProcessSession

    ''' <summary>
    ''' Gets the actual session objects from the database representing all sessions
    ''' </summary>
    ''' <returns>A collection of session objects representing all sessions in the
    ''' database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetActualSessions() As ICollection(Of clsProcessSession)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetActualSessionsFilteredAndOrdered(processSessionParameters As ProcessSessionParameters) As ICollection(Of clsProcessSession)

    ''' <summary>
    ''' Gets the actual session objects from the database from a specific resource
    ''' </summary>
    ''' <param name="resourceName">The name of the resource for which all sessions
    ''' are required or null to return all sessions for all resources.</param>
    ''' <returns>A collection of sessions from the given resource, or all sessions if
    ''' no resource name was given.</returns>
    <OperationContract(Name:="GetActualSessionsResource"), FaultContract(GetType(BPServerFault))>
    Function GetActualSessions(resourceName As String) As ICollection(Of clsProcessSession)

    ''' <summary>
    ''' Gets the actual session objects from the database from a specific set of
    ''' resources
    ''' </summary>
    ''' <param name="resourceNames">The names of the resources for which all sessions
    ''' are required or null/empty to return all sessions for all resources.</param>
    ''' <returns>A collection of sessions from the given resources, or all sessions
    ''' if no resource name was given.</returns>
    <OperationContract(Name:="GetActualSessionsResourceList"), FaultContract(GetType(BPServerFault))>
    Function GetActualSessions(resourceNames As ICollection(Of String)) As ICollection(Of clsProcessSession)

    ''' <summary>
    ''' Gets the actual session object from the database for a specific session.
    ''' </summary>
    ''' <param name="sessionId">The ID of the session for which an actual
    ''' <see cref="clsProcessSession"/> objects is required or
    ''' <see cref="Guid.Empty"/> to return all sessions.</param>
    ''' <returns>The session object corresponding to the given session ID in a
    ''' collection, -or- all sessions if <see cref="Guid.Empty"/> is given -or-
    ''' an empty collection if no session with the given ID was found, or if there
    ''' were no sessions in the database.</returns>
    <OperationContract(Name:="GetActualSessionsOneSession"), FaultContract(GetType(BPServerFault))>
    Function GetActualSessions(sessionId As Guid) As ICollection(Of clsProcessSession)

    ''' <summary>
    ''' Counts the number of concurrent sessions currently in the database, ie. those
    ''' sessions which have a state of PENDING or RUNNING (statusid of 0 or 1,
    ''' respectively).
    ''' </summary>
    ''' <returns>The number of PENDING or RUNNING sessions registered in the database
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CountConcurrentSessions() As Integer

    ''' <summary>
    ''' Counts the number of concurrent sessions currently in the database, ie. those
    ''' sessions which have a state of PENDING or RUNNING (statusid of 0 or 1,
    ''' respectively), optionally excluding sessions with specified IDs.
    ''' </summary>
    ''' <param name="excluding">The session IDs to be excluded from the count. A null
    ''' or empty collection excludes no sessions from the count.</param>
    ''' <returns>The number of PENDING or RUNNING sessions registered in the database
    ''' not including those with the IDs specified in <paramref name="excluding"/>.
    ''' </returns>
    <OperationContract(Name:="CountConcurrentSessionsExcluding"), FaultContract(GetType(BPServerFault))>
    Function CountConcurrentSessions(ByVal excluding As ICollection(Of Guid)) As Integer

    ''' <summary>
    ''' Reads pending session data from the database.
    ''' </summary>
    ''' <returns>A data table of session data</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetPendingSessions() As DataTable

    ''' <summary>
    ''' Reads running or stopping session details from the database.
    ''' </summary>
    ''' <param name="resourceID">The resourceid for which to get sessions</param>
    ''' <returns> A datatable with columns sessionid, startdatetime, enddatetime,
    ''' processid, starterresourceid, starteruserid and runningresourceid.</returns>
    ''' <remarks>Throws an exception if an error occurs.</remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetRunningOrStoppingSessions(resourceID As Guid) As DataTable


    ''' <summary>
    ''' Get sessions stopped after a certain time.
    ''' </summary>
    ''' <returns> A datatable with columns sessionid and enddatetime</returns>
    ''' <remarks>Throws an exception if an error occurs.</remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSessionsEndedAfter(endedAfter As Date) As DataTable

    ''' <summary>
    ''' Fetches all sessions from the database filtered by the
    ''' criteria specified in the arguments. Filtering is by one or more of ProcessName
    ''' ResourcName, Session Status, StartDateTime, EndDateTime. To have one of these
    ''' criteria ignored, simply supply the string "*" (or a null reference in the place
    ''' of a date).
    ''' </summary>
    ''' <param name="processNames">A string array of processes of interest.
    ''' The name "All" is valid if no filtering by process name is desired.</param>
    ''' <param name="resourceName">The hostname of a resource of interest. If specified,
    ''' only sessions run on this resource will be fetched.
    ''' The name "All" is valid if no filtering by resource name is desired.</param>
    ''' <param name="usernames">A collection of usernames to search for.</param>
    ''' <param name="sessStatus">The status of interest. If specified, only sessions with
    ''' the specified status will bo returned.</param>
    ''' <param name="startDate">A date of interest. If not null then only sessions
    ''' with a starttime after this date will be returned.</param>
    ''' <param name="endDate">A date of interest. If not null then only sessions
    ''' with a starttime after this date will be returned.</param>
    ''' <param name="updatedBefore">A date of interest. If not null then only sessions
    ''' with a lastupdated date before this date will be returned.</param>
    ''' <param name="localButRemote">If True, include sessions on machines which are
    ''' marked as Local but are actually remote to this machine. If False, they are
    ''' excluded. Machines marked as Local can only be connected to and controlled
    ''' from their local PC, so these machines should be excluded from, for example,
    ''' Control Room, by setting this parameter to False. This can be overriden
    ''' by the ExcludeAllLocal parameter.</param>
    ''' <param name="excludeAllLocal">If true, then no local sessions will be returned.
    ''' This parameter overrides the bLocalButRemote parameter - if the bLocalButRemote
    ''' parameter is true and ExcludeAllLocal is also true then no local sessions
    ''' will be included, even if they are running remotely.</param>
    ''' <param name="maxSessionCount">The maximum amount of clsProcessSession returned
    ''' from the query. </param>
    ''' <param name="sortInfo">How to sort the filtered sessions</param>
    ''' <returns>A datatable of session data</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetFilteredSessions(
                                        processNames As ICollection(Of String),
                                        resourceName As ICollection(Of String),
                                        usernames As ICollection(Of String),
                                        sessStatus As SessionStatus,
                                        startDate As Date,
                                        endDate As Date,
                                        updatedBefore As Date,
                                        localButRemote As Boolean,
                                        excludeAllLocal As Boolean,
                                        maxSessionCount As Integer,
                                        sortInfo As SessionSortInfo) As ICollection(Of clsProcessSession)

    ''' <summary>
    ''' Updates the 'startparamsxml' field of a session record.
    ''' </summary>
    ''' <param name="sessionId">The session id</param>
    ''' <param name="xml">The start paramters XML</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetProcessStartParams(sessionId As Guid, xml As String)

    ''' <summary>
    ''' Updates the 'startparamsxml' field of a session record.
    ''' </summary>
    ''' <param name="token">An authorisation token</param>
    ''' <param name="sessionId">The session id</param>
    ''' <param name="xml">The start paramters XML</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetProcessStartParamsAs(token As String, sessionId As Guid, xml As String)

    ''' <summary>
    ''' Finds any debug sessions that were not able to be stopped successfully and either deletes or ends them correctly.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub CleanupFailedDebugSessions()

    ''' <summary>
    ''' Reads the statistics table from the database.
    ''' </summary>
    ''' <returns>A datatable with fields sessionid, name, datatype, value_text,
    ''' value_number, value_date and value_flag</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetStatistics() As DataTable

    ''' <summary>
    ''' Gets the license info from the database.
    ''' </summary>
    ''' <returns>The current keys.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetLicenseInfo() As List(Of KeyInfo)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function AddLicenseKey(key As KeyInfo, fileName As String) As List(Of KeyInfo)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function RemoveLicenseKey(key As KeyInfo) As List(Of KeyInfo)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetLicenseActivationRequest(ByVal keyInfo As KeyInfo) As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ValidateLicenseActivationResponseForLicense(licenseToActivate As KeyInfo, ByVal response As String) As Boolean



    ''' <summary>
    ''' Add an exception type to the database table if not already there.
    ''' </summary>
    ''' <param name="exType">The type of the exception to add</param>
    ''' <returns>True if the exception type was added to the database table</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function AddExceptionType(ByVal exType As String) As Boolean

    ''' <summary>
    ''' Adds a collection of exception types to the database if they do not already
    ''' exist in the BPAExceptionType table
    ''' </summary>
    ''' <param name="exceptionTypesToSave">A collection of string exception types.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AddExceptionTypes(exceptionTypesToSave As IEnumerable(Of String))

    ''' <summary>
    ''' Finds all exception types in existing objects and processes and removes ones
    ''' that are no longer in use.
    ''' </summary>
    ''' <returns>True if successful</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function DeleteExceptionType(ByVal sExceptionType As String) As Boolean

    ''' <summary>
    ''' Gets a list of exception types.
    ''' </summary>
    ''' <returns>A list of exception types</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetExceptionTypes() As List(Of String)

    ''' <summary>
    ''' Reads the process name from the database.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <returns>The process name</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessNameByID(ByVal gProcessID As Guid) As String


    ''' <summary>
    ''' Checks to see if a process is locked.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <param name="userName">The name of the user locking the process</param>
    ''' <param name="machineName">The  machine on which the process is locked</param>
    ''' <returns>True if the process is locked, False if unlocked</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ProcessIsLocked(ByVal gProcessID As Guid, ByRef userName As String, ByRef machineName As String) As Boolean

    ''' <summary>
    ''' Creates a lock record in the database for a process.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub LockProcess(ByVal gProcessID As Guid)

    ''' <summary>
    ''' Unlocks the process with the given ID and, if specified, allows processes
    ''' to be unlocked by the a different machine to the currently logged in one
    ''' </summary>
    ''' <param name="procId">The ID of the process to unlock.</param>
    ''' <param name="blnAllowUnlockFromAnyMachine" >If True, can unlock the process even if
    ''' locked by another machine to the currently logged in one.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function UnlockProcess(procId As Guid, Optional blnAllowUnlockFromAnyMachine As Boolean = False) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function UnlockProcessImport(processId As Guid) As Boolean

    ''' <summary>
    ''' Reads details of locked processes from the database.
    ''' </summary>
    ''' <param name="errorMessage">The error message</param>
    ''' <returns>A hashtable of process details</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetLockedProcesses(useBusinessObjects As Boolean) As LockedProcessesResult

    ''' <summary>
    ''' Deletes the process or VBO with the given ID.
    ''' </summary>
    ''' <param name="procId">The ID of the process or VBO to delete</param>
    ''' <exception cref="AlreadyLockedException">If the process or VBO is currently
    ''' locked elsewhere</exception>
    ''' <exception cref="LockFailedException">If the lock could not be acquired for
    ''' any other reason</exception>
    ''' <exception cref="AuditOperationFailedException">If the writing of the audit
    ''' record failed</exception>
    ''' <exception cref="OperationFailedException">If the process or VBO could not
    ''' be deleted because there were some pending or completed sessions in the
    ''' database which referred to it</exception>
    ''' <exception cref="Exception">If any other error occurs while attempting to
    ''' delete the process/vbo</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteProcess(procId As Guid, deleteReason As String)

    ''' <summary>
    ''' Write process details to the database.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <param name="gCreatedBy">The creating user id</param>
    ''' <param name="dCreateDate">The creation date</param>
    ''' <param name="gModifiedBy">The modifying user id</param>
    ''' <param name="dModifiedDate">The modification date</param>
    ''' <param name="bNew">A flag to indicated that the process is new</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetProcessInfo(ByVal gProcessID As Guid, ByVal gCreatedBy As Guid, ByVal dCreateDate As Date, ByVal gModifiedBy As Guid, ByVal dModifiedDate As Date, ByVal bNew As Boolean)

    ''' <summary>
    ''' Reads process details from the database. There is a more detailed overload
    ''' if additional information is required.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <param name="sCreatedBy">The creator user name</param>
    ''' <param name="dCreateDate">The creation date</param>
    ''' <param name="sModifiedBy">The moste recent modifier user name</param>
    ''' <param name="dModifiedDate">The most recent modification date</param>
    ''' <returns>True if successful</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessInfo(ByVal gProcessID As Guid, ByRef sCreatedBy As String, ByRef dCreateDate As Date, ByRef sModifiedBy As String, ByRef dModifiedDate As Date) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessMetaInfo(procId As Guid()) As ProcessMetaInfo()

    ''' <summary>
    ''' Reads process details from the database. There is a less detailed overload
    ''' that can be used when some of this information is not required.
    ''' </summary>
    ''' <param name="procId">The process id</param>
    ''' <param name="createdBy">The creator user name</param>
    ''' <param name="createdDate">The creation date</param>
    ''' <param name="modifiedBy">The moste recent modifier user name</param>
    ''' <param name="modifiedDate">The most recent modification date</param>
    ''' <param name="type">The process type - see clsProcess.Type</param>
    ''' <returns>True if successful</returns>
    <OperationContract(Name:="GetProcessInfoWithType"), FaultContract(GetType(BPServerFault))>
    Function GetProcessInfo(ByVal procId As Guid,
                                   ByRef createdBy As String, ByRef createdDate As Date,
                                   ByRef modifiedBy As String, ByRef modifiedDate As Date,
                                   ByRef type As DiagramType) As Boolean

    ''' <summary>
    ''' Updates a process in the database.
    ''' </summary>
    ''' <param name="processId">The process id</param>
    ''' <param name="name">The process name</param>
    ''' <param name="version">The process version</param>
    ''' <param name="description">The process description</param>
    ''' <param name="newXml">The process XML definition.</param>
    ''' <param name="auditComments">The audit comments</param>
    ''' <param name="auditSummary">User's summary of changes</param>
    ''' <param name="lastModified">On return, contains the DateTime that was stored for
    ''' the last modified date and time in the database.</param>
    ''' <param name="references">External process references</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub EditProcess(processId As Guid, name As String, version As String,
                                    description As String, newXml As String, auditComments As String, auditSummary As String,
                                    ByRef lastModified As DateTime, references As clsProcessDependencyList)

    ''' <summary>
    ''' Checks if the given process name is unique amongst both processes and
    ''' business objects in the system.
    ''' </summary>
    ''' <param name="gCurrentProcessID">The ID of a process/business object to
    ''' ignore in the search. Use Guid.Empty to bypass this check.</param>
    ''' <param name="ConflictingProcessID">Carries back the ID of the first
    ''' conflicting process or object found.</param>
    ''' <param name="sName">The name to search for.</param>
    ''' <returns>True if no process or business object exists with the given name;
    ''' False if either a process or business object was found.</returns>
    <OperationContract(Name:="IsProcessNameUniqueWithConflict"), FaultContract(GetType(BPServerFault))>
    Function IsProcessNameUnique(
                                            ByVal gCurrentProcessID As Guid,
                                            ByRef ConflictingProcessID As Guid,
                                            ByVal sName As String) As Boolean

    ''' <summary>
    ''' Checks if the given name is currently in use in a process/VBO
    ''' </summary>
    ''' <param name="gCurrentProcessID">An ID to ignore - ie. an existing process/VBO
    ''' with this ID will not be counted as a collision.</param>
    ''' <param name="sName">The name to check</param>
    ''' <param name="bUseBusinessObjects">True to check VBOs; False to check
    ''' processes </param>
    ''' <returns>True if a process/VBO was found with the given name; False otherwise
    ''' </returns>
    <OperationContract(Name:="IsProcessNameUniqueInProcess"), FaultContract(GetType(BPServerFault))>
    Function IsProcessNameUnique(ByVal gCurrentProcessID As Guid, ByVal sName As String,
                                            ByVal bUseBusinessObjects As Boolean) As Boolean

    ''' <summary>
    ''' Finds out whether the process/business object name is unique amongst
    ''' existing processes/business objects in the database.
    ''' </summary>
    ''' <param name="gCurrentProcessID">The ID of a process/business object to
    ''' ignore in the search. Use Guid.Empty to bypass this check.</param>
    ''' <param name="sName">The name to check.</param>
    ''' <param name="ConflictingProcessID">Carries back the ID of the first conflicting
    ''' process found.</param>
    ''' <returns>True if the process is unique and False if not or any other error
    ''' occurred</returns>
    <OperationContract(Name:="IsProcessNameUniqueAmongstExisting"), FaultContract(GetType(BPServerFault))>
    Function IsProcessNameUnique(
                                            ByVal gCurrentProcessID As Guid, ByRef ConflictingProcessID As Guid,
                                            ByVal sName As String, ByVal isVBO As Nullable(Of Boolean),
                                            ByRef lastModifedBy As String, ByRef lastModifiedDate As Date) As Boolean


    ''' <summary>
    ''' Determines whether the specified ID is unique, amongst existing processes
    ''' (or business objects).
    ''' </summary>
    ''' <param name="gProcessID">The ID of interest.</param>
    ''' <param name="bUseBusinessObjects">If True, then only business objects will be
    ''' matched. If False then only processes will be matched.</param>
    ''' <returns>Returns True if the supplied ID is unique.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsProcessIDUnique(ByVal gProcessID As Guid, ByVal bUseBusinessObjects As Boolean) As Boolean

    ''' <summary>
    ''' Find a process by name.
    ''' </summary>
    ''' <param name="sName">The process name to search for</param>
    ''' <param name="IncludeBusinessObjects">When True, business objects will be
    ''' searched as well as processes. Optional; defaults to False.</param>
    ''' <returns>The process ID, or Guid.Empty if the process was not found.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessIDByName(ByVal sName As String, Optional ByVal IncludeBusinessObjects As Boolean = False) As Guid

    ''' <summary>
    ''' Find a process by published web service name.
    ''' </summary>
    ''' <param name="sName">The process name to search for</param>
    ''' <param name="IncludeBusinessObjects">When True, business objects will be
    ''' searched as well as processes. Optional; defaults to False.</param>
    ''' <returns>The process ID, or Guid.Empty if the process was not found.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessIDByWSName(ByVal sName As String, Optional ByVal IncludeBusinessObjects As Boolean = False) As Guid

    ''' <summary>
    ''' Clones the given process using the specified parameters over the given
    ''' database connection.
    ''' </summary>
    ''' <param name="processId">The ID to use for the new process</param>
    ''' <param name="name">The name to use for the new process</param>
    ''' <param name="version">The version of the process to save</param>
    ''' <param name="description">The description of the process.</param>
    ''' <param name="xml">The XML describing the process body.</param>
    ''' <param name="overwriteId">True to force the new process to be saved
    ''' as a new version of an existing process if a process is found with
    ''' the specified ID.</param>
    ''' <param name="isObject">True if the new process is an object; False otherwise.
    ''' </param>
    ''' <param name="references">External process references</param>
    ''' <param name="group">The group to create the process in</param>
    ''' <param name="summary">The summary text for the audit log</param>
    ''' <remarks>For best performance, assuming you have a clsProcess object when
    ''' you call this, you should also update the dependencies after you call this.
    ''' You do that using UpdateExternalDependencies().</remarks>
    <OperationContract(), FaultContract(GetType(BPServerFault))>
    Sub CloneProcess(processId As Guid, name As String, version As String, description As String,
                      xml As String, overwriteId As Boolean, isObject As Boolean,
                      references As clsProcessDependencyList, group As Guid, summary As String)

    ''' <summary>
    ''' Creates the given process using the specified parameters over the given
    ''' database connection.
    ''' </summary>
    ''' <param name="processId">The ID to use for the new process</param>
    ''' <param name="name">The name to use for the new process</param>
    ''' <param name="version">The version of the process to save</param>
    ''' <param name="description">The description of the process.</param>
    ''' <param name="xml">The XML describing the process body.</param>
    ''' <param name="overwriteId">True to force the new process to be saved
    ''' as a new version of an existing process if a process is found with
    ''' the specified ID.</param>
    ''' <param name="isObject">True if the new process is an object; False otherwise.
    ''' </param>
    ''' <param name="references">External process references</param>
    ''' <param name="group">The group to create the process in</param>
    ''' <remarks>For best performance, assuming you have a clsProcess object when
    ''' you call this, you should also update the dependencies after you call this.
    ''' You do that using UpdateExternalDependencies().</remarks>
    <OperationContract(), FaultContract(GetType(BPServerFault))>
    Sub CreateProcess(processId As Guid, name As String, version As String, description As String,
                      xml As String, overwriteId As Boolean, isObject As Boolean,
                      references As clsProcessDependencyList, group As Guid)

    ''' <summary>
    ''' Imports the given process using the specified parameters over the given
    ''' database connection.
    ''' </summary>
    ''' <param name="processId">The ID to use for the new process</param>
    ''' <param name="name">The name to use for the new process</param>
    ''' <param name="version">The version of the process to save</param>
    ''' <param name="description">The description of the process.</param>
    ''' <param name="xml">The XML describing the process body.</param>
    ''' <param name="overwriteId">True to force the new process to be saved
    ''' as a new version of an existing process if a process is found with
    ''' the specified ID.</param>
    ''' <param name="isObject">True if the new process is an object; False otherwise.
    ''' </param>
    ''' <param name="references">External process references</param>
    ''' <param name="fileName">The file from which the process was imported</param>
    ''' <remarks>For best performance, assuming you have a clsProcess object when
    ''' you call this, you should also update the dependencies after you call this.
    ''' You do that using UpdateExternalDependencies().</remarks>
    <OperationContract(), FaultContract(GetType(BPServerFault))>
    Sub ImportProcess(processId As Guid, name As String, version As String, description As String,
                      xml As String, overwriteId As Boolean, isObject As Boolean,
                      references As clsProcessDependencyList, fileName As String)

    ''' <summary>
    ''' Reads a process XML definition from the database.
    ''' </summary>
    ''' <param name="procId">The process ID</param>
    ''' <returns>The XML definition of the process.</returns>
    ''' <exception cref="NoSuchElementException">If no process with the given ID was
    ''' found on the database</exception>
    ''' <exception cref="SqlClient.SqlException">If any database errors occcur while attempting
    ''' to retrieve the XML for the process</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessXML(ByVal procId As Guid) As String

    ''' <summary>
    ''' Returns a list of process startup arguments with empty values
    ''' </summary>
    ''' <param name="procId"></param>
    ''' <returns></returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetBlankProcessArguments(procId As Guid) As List(Of clsArgument)

    ''' <summary>
    ''' Reads the last modified date of a process from the database.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <returns>The date/time the process was modified, or Nothing if not found.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessLastModified(ByVal gProcessID As Guid) As Date

    ''' <summary>
    ''' Get the user id of the user who last modified the process.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <returns>The user id of the user who last modified the process, or Nothing if not found.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessLastModifiedBy(ByVal gProcessID As Guid) As Guid

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessXMLAndAssociatedData(gProcessID As Guid) As ProcessDetails

    ''' <summary>
    ''' Get a list of the IDs of all the processes in the database. This includes
    ''' all types and statuses, without exception.
    ''' </summary>
    ''' <returns>A list of Guids, or Nothing if an error occurred.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAllProcessIDs() As List(Of Guid)

    ''' <summary>
    ''' Gets all non-retired processes into a data table of the same format as that
    ''' returned by <see cref="GetProcesses"/>
    ''' </summary>
    ''' <returns>If no error occurs, returns a DataTable containing the process id,
    ''' name, version, description, status, and a 0/1 "locked" integer column
    ''' indicating if the process is locked. If an error occurs, returns Nothing
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAvailableProcesses() As DataTable

    ''' <summary>
    ''' Gets the currently available processes matching the desired statuses.
    ''' Processes are ordered by name.
    ''' </summary>
    ''' <param name="requiredAttributes">Statuses which are required. Only processes
    ''' with this (these) status(es) will be selected. Setting this parameter to
    ''' ProcessAttributes.None will mean all processes are selected.</param>
    ''' <param name="unacceptableAttributes">Statuses which not allowed. Processes
    ''' which have this (these) statuses will not be returned even if they have the
    ''' required statuses as specified in the RequiredStatuses parameter. Setting this
    ''' to ProcessAtrributes.None will mean no processes are filtered out.</param>
    ''' <param name="useBusinessObjects">Determines whether we fetch business objects
    ''' or processes.</param>
    ''' <returns>If no error occurs, returns a DataTable containing the process id,
    ''' name, version, description, status, and a 0/1 "locked" integer column
    ''' indicating if the process is locked. If an error occurs, returns Nothing.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcesses(ByVal requiredAttributes As ProcessAttributes,
                                     ByVal unacceptableAttributes As ProcessAttributes,
                                     Optional ByVal useBusinessObjects As Boolean = False) As DataTable

    ''' <summary>
    ''' Overloaded function. Returns all processes regardless of status. To filter
    ''' on status use overloaded method.
    ''' </summary>
    ''' <returns>If no error occurs, returns a datatable containing the process' id, name, version and description, and
    ''' a 0/1 "locked" integer column indicating if the process is locked. If an
    ''' error occurs, returns nothing.</returns>
    <OperationContract(Name:="GetProcessesAll"), FaultContract(GetType(BPServerFault))>
    Function GetProcesses(Optional ByVal bUseBusinessObjects As Boolean = False) As DataTable

    ''' <summary>
    ''' Overloads GetProcesses(). The results are limited to those processes whose
    ''' guid is in the supplied list.
    ''' </summary>
    ''' <param name="GuidList">A list of process ids</param>
    ''' <returns>Returns a datatable containing same information as GetProcesses()
    ''' but only returns rows corresponding to the processes specified in the
    ''' argument 'GuidList'.</returns>
    <OperationContract(Name:="GetProcessesList"), FaultContract(GetType(BPServerFault))>
    Function GetProcesses(ByVal GuidList As List(Of Guid), Optional ByVal bUseBusinessObjects As Boolean = False) As DataTable

    ''' <summary>
    ''' Publishes the specified Processes so that it is available within Control Room
    ''' </summary>
    ''' <param name="processId">The ID of the Process to publish</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub PublishProcess(processId As Guid)

    ''' <summary>
    ''' Unpublishes the specified Processes so that it is not available within Control Room
    ''' </summary>
    ''' <param name="processId">The ID of the Process to unpublish</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UnpublishProcess(processId As Guid)

    ''' <summary>
    ''' Exposes the specified Process as a Web Service.
    ''' </summary>
    ''' <param name="processId">The ID of the Process to expose</param>
    ''' <param name="details">The Web Service parameters</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ExposeProcessAsWebService(processId As Guid, details As WebServiceDetails)

    ''' <summary>
    ''' Exposes the specified Object as a Web Service.
    ''' </summary>
    ''' <param name="objectId">The ID of the Object to expose</param>
    ''' <param name="details">The Web Service parameters</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ExposeObjectAsWebService(objectId As Guid, details As WebServiceDetails)

    ''' <summary>
    ''' Conceals the specified exposed Process
    ''' </summary>
    ''' <param name="processId">The ID of the Process to be concealed</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ConcealProcessWebService(processId As Guid)

    ''' <summary>
    ''' Conceals the specified exposed Object
    ''' </summary>
    ''' <param name="objectId">The ID of the Object to be concealed</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ConcealObjectWebService(objectId As Guid)

    ''' <summary>
    ''' Mark the specified Process / Object as retired
    ''' </summary>
    ''' <param name="processId">The ID of the Process to retire</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RetireProcessOrObject(processId As Guid)

    ''' <summary>
    ''' Mark the specified Process as unretired
    ''' </summary>
    ''' <param name="processId">The ID of the Process to unretire</param>
    ''' <param name="targetGroupId">The ID of the group to unretire the process to</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UnretireProcessOrObject(processId As Guid, targetGroupId As Guid)


    ''' <summary>
    ''' Gets the Web Service publishing name of the specified process.
    ''' </summary>
    ''' <param name="procid">The process.</param>
    ''' <returns>Returns an object of type WSDetails.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessWSDetails(ByVal procid As Guid) As WebServiceDetails

    ''' <summary>
    ''' Sets the status of the specified process.
    ''' </summary>
    ''' <param name="procId">The process to update</param>
    ''' <param name="attrs">The process attributes</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetProcessAttributes(ByVal procId As Guid, ByVal attrs As ProcessAttributes)

    ''' <summary>
    ''' Gets the attributes of the specified process.
    ''' </summary>
    ''' <param name="procid">The id of the process for which the attributes are
    ''' required.</param>
    ''' <returns>The process attributes corresponding to the given process ID.
    ''' </returns>
    ''' <exception cref="NoSuchElementException">If no process with the given ID
    ''' was found on the database.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessAttributes(ByVal procid As Guid) As ProcessAttributes

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessAttributesBulk(processIds As List(Of Guid)) As Dictionary(Of Guid, ProcessAttributes)


    ''' <summary>
    ''' Get a list of all fonts stored in the database.
    ''' </summary>
    ''' <returns>A DataTable with columns 'name' and 'version'.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetFonts() As DataTable

    ''' <summary>
    ''' Gets a collection of the font names on the database
    ''' </summary>
    ''' <returns>The names of all the fonts on the system.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetFontNames() As ICollection(Of String)

    ''' <summary>
    ''' Delete a font from the database.
    ''' </summary>
    ''' <param name="name">The name of the font.</param>
    ''' <returns>True if the font was found on the database and deleted; False if the
    ''' font was not found on the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function DeleteFont(ByVal name As String) As Boolean

    ''' <summary>
    ''' Get data for the specified font.
    ''' </summary>
    ''' <param name="name">The name of the font.</param>
    ''' <param name="version">On success, contains the font version.</param>
    ''' <returns>The XML font definition, or Nothing if a font with the given name
    ''' does not exist in the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetFont(ByVal name As String, ByRef version As String) As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetFontOcrPlus(ByVal name As String, ByRef version As String) As String

    ''' <summary>
    ''' Checks if the font data has updated from the given version number, updating
    ''' the version number reference if it has.
    ''' </summary>
    ''' <param name="versionNo">The version number to test. On exit, this contains
    ''' the most up to date version number for the fonts in the environment.</param>
    ''' <returns>True if the font data has been updated since the specified version
    ''' number; False if the given version number was the latest version of the
    ''' fonts recorded.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function HasFontDataUpdated(ByRef versionNo As Long) As Boolean

    ''' <summary>
    ''' Update (or create) font data.
    ''' </summary>
    ''' <param name="name">The name of the font. If a font doesn't already exist
    ''' with that name, a new record is created.</param>
    ''' <param name="version">The font version.</param>
    ''' <param name="data">The XML font definition.</param>
    <OperationContract(Name:="SaveAndRenameFont"), FaultContract(GetType(BPServerFault))>
    Sub SaveFont(ByVal origName As String, ByVal name As String, ByVal version As String, ByVal data As String)

    ''' <summary>
    ''' Update (or create) font data.
    ''' </summary>
    ''' <param name="name">The name of the font. If a font doesn't already exist
    ''' with that name, a new record is created.</param>
    ''' <param name="version">The font version.</param>
    ''' <param name="data">The XML font definition.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SaveFont(ByVal name As String, ByVal version As String, ByVal data As String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SaveFontOcrPlus(ByVal name As String, ByVal data As String)

    ''' <summary>
    ''' Get a list of Process MI Template names
    ''' </summary>
    ''' <param name="Names">A list of names will be returned by this parameter, supplied parameter maybe null</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ProcessMITemplateNames(ByRef Names As Generic.List(Of String), ByVal gProcessID As Guid)

    ''' <summary>
    ''' Updates the template xml of a template given a template name, and the process id to which the template relates
    ''' </summary>
    ''' <param name="sName">The name of the template</param>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    ''' <param name="sTemplateXML">The new template xml</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ProcessMIUpdateTemplate(ByVal sName As String, ByVal gProcessID As Guid, ByVal sTemplateXML As String)

    ''' <summary>
    ''' Creates a new template.
    ''' </summary>
    ''' <param name="sName">The name of the template</param>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    ''' <param name="sTemplateXML">The new template xml</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ProcessMICreateTemplate(ByVal sName As String, ByVal gProcessID As Guid, ByVal sTemplateXML As String)

    ''' <summary>
    ''' Deletes a process mi template
    ''' </summary>
    ''' <param name="sName">The name of the template</param>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ProcessMIDeleteTemplate(ByVal sName As String, ByVal gProcessID As Guid)

    ''' <summary>
    ''' Gets a process mi template
    ''' </summary>
    ''' <param name="sName">The name of the template</param>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    ''' <param name="sTemplateXML">The template xml will be returned in this parameter</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ProcessMIGetTemplate(ByVal sName As String, ByVal gProcessID As Guid, ByRef sTemplateXML As String)

    ''' <summary>
    ''' Gets the name of the default process mi template
    ''' </summary>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    ''' <param name="sName">The name of the template will be returned in this parameter</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ProcessMIGetDefaultTemplate(ByVal gProcessID As Guid, ByRef sName As String)

    ''' <summary>
    ''' Sets the default process mi template
    ''' </summary>
    ''' <param name="gProcessID">The id of the process the template belongs to.</param>
    ''' <param name="sName">The name of the template</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ProcessMISetDefaultTemplate(ByVal gProcessID As Guid, ByRef sName As String)

    ''' <summary>
    ''' Gather some basic statistics about the database. This is used by the reporting
    ''' tool. All parameters are populated on return, unless an error occurs.
    ''' </summary>
    ''' <param name="sessioncount">The number of sessions</param>
    ''' <param name="sessionlogcount">The number of session log entries</param>
    ''' <param name="resourcecount">The number of resources</param>
    ''' <param name="queueitemcount">The number of queue items</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub GetDBStats(ByRef sessioncount As Integer, ByRef sessionlogcount As Integer, ByRef resourcecount As Integer, ByRef queueitemcount As Integer)

    ''' <summary>
    ''' Gets the single environment variable with the given name.
    ''' </summary>
    ''' <param name="name">The name of the required environment variable</param>
    ''' <returns>The environment variable with the given name as found on the
    ''' database, or null if no environment variable with the given name was found.
    ''' </returns>
    ''' <exception cref="ArgumentNullException">If no name was given</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEnvironmentVariable(ByVal name As String) As clsEnvironmentVariable

    ''' <summary>
    ''' Gets all the environment variables into a dictionary mapped against their
    ''' names.
    ''' </summary>
    ''' <returns>A map of environment variables mapped against their names.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEnvironmentVariables() As ICollection(Of clsEnvironmentVariable)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEnvironmentVariablesNames() As List(Of String)

    ''' <summary>
    ''' Updates, or creates, an environment variable.
    ''' </summary>
    ''' <param name="name">The environment variable name</param>
    ''' <param name="datatype">The data type</param>
    ''' <param name="value">The value, in Automate text-encoded format.</param>
    ''' <param name="description">A description of the variable.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateEnvironmentVariable(ByVal name As String, ByVal datatype As DataType,
                                                  ByVal value As String, ByVal description As String)

    ''' <summary>
    ''' Deletes an environment variable.
    ''' </summary>
    ''' <param name="name">The environment variable name</param>
    ''' <returns>True if successful.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function DeleteEnvironmentVariable(ByVal name As String) As Boolean

    ''' <summary>
    ''' Updates environment variables based on a list of inserted, updated and
    ''' deleted items.
    ''' </summary>
    ''' <param name="inserted">The collection of env vars which are to be inserted
    ''' into the database</param>
    ''' <param name="updated">The collection of env vars which should be updated
    ''' with new data on the database.</param>
    ''' <param name="deleted">The collection of env vars which should deleted from
    ''' the database.</param>
    ''' <exception cref="ArgumentNullException">If any of the collections are null.
    ''' </exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateEnvironmentVariables(inserted As ICollection(Of clsEnvironmentVariable),
                                    updated As ICollection(Of clsEnvironmentVariable),
                                    deleted As ICollection(Of clsEnvironmentVariable))

    ''' <summary>
    ''' Updates the default back up interval
    ''' </summary>
    ''' <param name="minutes">The interval</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AutoSaveWriteInterval(ByVal minutes As Long)

    ''' <summary>
    ''' Reads the back up interval from the database.
    ''' </summary>
    ''' <returns>The interval</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function AutoSaveReadInterval() As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function AutoSaveGetBackupDateTime(processID As Guid) As DateTime

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AutoSaveGetBackupXML(ByVal ProcessID As Guid, ByRef sXML As String)

    ''' <summary>
    ''' Indicates whether a yet to be back up of a process exists
    ''' and therfore whether should be recovered.
    ''' </summary>
    ''' <param name="processId">The process Guid</param>
    ''' <returns>True if a back up exists.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function AutoSaveBackupSessionExistsForProcess(ByVal processId As Guid) As Boolean

    ''' <summary>
    ''' Indicates whether a yet to be back up of a process exists
    ''' and therfore whether should be recovered.
    ''' </summary>
    ''' <param name="processId">The process Guid</param>
    ''' <returns>True if a back up exists.</returns>
    <OperationContract(Name:="AutoSaveBackupSessionExistsForProcess2"), FaultContract(GetType(BPServerFault))>
    Function AutoSaveBackupSessionExistsForProcess(ByVal processId As String) As Boolean

    ''' <summary>
    ''' Deletes an auto-saved back up from the database
    ''' </summary>
    ''' <param name="processId">The process id</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteProcessAutoSaves(ByVal processId As Guid)

    ''' <summary>
    ''' Backs up a process object (assuming current user still holds the lock).
    ''' </summary>
    ''' <param name="procXml">The process definition in XML format</param>
    ''' <param name="procID">The process ID</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub CreateProcessAutoSave(ByVal procXml As String, ByVal procID As Guid)

    ''' <summary>
    ''' Get MI data relating to decision stages
    ''' </summary>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <returns>A DataTable containing the requested data</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function MIGetDecisionData(ByVal sessions() As Integer, ByVal stages() As Guid) As DataTable

    ''' <summary>
    ''' Get MI data relating to calculation stages
    ''' </summary>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <returns>A DataTable containing the requested data</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function MIGetCalculationData(ByVal sessions() As Integer, ByVal stages() As Guid) As DataTable

    ''' <summary>
    ''' Get MI data relating to stages that have a start and end date.
    ''' </summary>
    ''' <param name="stageType">The stage type</param>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <returns>A DataTable containing the requested data</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function MIGetReturnStageData(ByVal stageType As StageTypes, ByVal sessions() As Integer, ByVal stages() As Guid) As DataTable

    ''' <summary>
    ''' Get MI data relating to choice start stages
    ''' </summary>
    ''' <param name="stageType">The stage type - can be either ChoiceStart or
    ''' WaitStart</param>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <returns>A DataTable containing the requested data</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function MIGetChoiceStartData(ByVal stageType As StageTypes, ByVal sessions() As Integer, ByVal stages() As Guid) As DataTable

    ''' <summary>
    ''' Get MI data relating to a generic stage type.
    ''' </summary>
    ''' <param name="stageType">The stage type</param>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <returns>A DataTable containing the requested data</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function MIGetStageData(ByVal stageType As StageTypes, ByVal sessions() As Integer, ByVal stages() As Guid) As DataTable

    ''' <summary>
    ''' Queries the DB for sessions.
    ''' </summary>
    ''' <param name="dStart">The start date</param>
    ''' <param name="dEnd">The end date</param>
    ''' <param name="resIDs">A list of Resource IDs to restrict to, or Nothing for all</param>
    ''' <param name="debugSessions">Whether to use debug sessions or normal sessions</param>
    ''' <param name="objectStudio">True for Object Studio mode, False for Process
    ''' Studio</param>
    ''' <returns>A DataTable of sessions</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function MIReadSessions(ByVal procid As Guid, ByVal dStart As Date, ByVal dEnd As Date, ByVal resIDs As ICollection(Of Guid), ByVal debugSessions As Boolean, ByVal objectStudio As Boolean) As DataTable

    ''' <summary>
    ''' Determines if the current license permits the creation of a session (taking
    ''' into account the number of existing pending/running sessions).
    ''' </summary>
    ''' <param name="sessionId">The ID of the session to be created, if known</param>
    ''' <returns>Returns true if the sessions can be created or false if the
    ''' specified sessions would exceed the concurrent session limit in the currently
    ''' installed licence.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CanCreateSession(ByVal sessionId As Guid) As Boolean

    ''' <summary>
    ''' Determines if the current license permits the creation of a session (taking
    ''' into account the number of existing pending/running sessions).
    ''' </summary>
    ''' <param name="num">The number of sessions which are wanted to be created
    ''' </param>
    ''' <returns>Returns true if the sessions can be created or false if the
    ''' specified sessions would exceed the concurrent session limit in the currently
    ''' installed licence.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CanCreateSessions(ByVal num As Integer) As Boolean

    ''' <summary>
    ''' Returns MI reporting configuration settings
    ''' </summary>
    ''' <param name="enabled">MI Enabled flag</param>
    ''' <param name="autoRefresh">Refresh statistics automatically via scheduler</param>
    ''' <param name="refreshAt">Daily refresh time</param>
    ''' <param name="lastRefreshed">Date/time of last refresh</param>
    ''' <param name="keepDaily">Daily statistics retention period (days)</param>
    ''' <param name="keepMonthly">Monthly statistics retention period (months)</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub GetMIConfig(ByRef enabled As Boolean, ByRef autoRefresh As Boolean,
                               ByRef refreshAt As Date, ByRef lastRefreshed As Date,
                               ByRef keepDaily As Integer, ByRef keepMonthly As Integer)

    ''' <summary>
    ''' Handles request from BPServer to refresh the MI data.
    ''' </summary>
    ''' <returns>True if refresh took place</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function MICheckAutoRefresh() As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function MIGetTimeLocked() As TimeSpan

    ''' <summary>
    ''' Saves the MI reporting configuration settings
    ''' </summary>
    ''' <param name="enabled">MI Enabled flag</param>
    ''' <param name="autoRefresh">Refresh statistics automatically via scheduler</param>
    ''' <param name="refreshAt">Daily refresh time</param>
    ''' <param name="keepDaily">Daily statistics retention period (days)</param>
    ''' <param name="keepMonthly">Monthly statistics retention period (months)</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetMIConfig(enabled As Boolean, autoRefresh As Boolean, refreshAt As Date,
                               keepDaily As Integer, keepMonthly As Integer)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ResetMIRefreshLock()


    ''' <summary>
    ''' Generates data for the elementusage report.
    ''' </summary>
    ''' <param name="processname">The name of the process to generate report data for</param>
    ''' <returns></returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetProcessElementUsageDetails(processname As String) As SortedDictionary(Of String, String)

    ''' <summary>
    ''' Creates the given work queue over the specified connection, ensuring that
    ''' the new queue's identity is set once it has successfully completed.
    ''' If the given queue has no <see cref="clsWorkQueue.Id"/> set, this will
    ''' generate one and assign it.
    ''' </summary>
    ''' <param name="workQueue">The queue to create</param>
    ''' <param name="isCommand">Indicates whether the request to delete the work queue has come from an AutomateC command.</param>
    ''' <returns>The given work queue with its ID and identity set.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CreateWorkQueue(workQueue As clsWorkQueue, Optional isCommand As Boolean = False) As clsWorkQueue

    ''' <summary>
    ''' Deletes the work queue with the specified name over the given connection
    ''' </summary>
    ''' <param name="name">The name of the work queue to delete.</param>
    ''' <param name="queueHasSnapshotConfiguration">Indicates whether the a work queue has snapshot configured.</param>
    ''' <param name="isCommand">Indicates whether the request to delete the work queue has come from an AutomateC command.</param>
    ''' <exception cref="NoSuchQueueException">If no queue with the given name could
    ''' be found on the database.</exception>
    ''' <exception cref="ForeignKeyDependencyException">If the queue is not empty and
    ''' therefore cannot be deleted.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteWorkQueue(name As String, Optional queueHasSnapshotConfiguration As Boolean = False, Optional isCommand As Boolean = False)

    ''' <summary>
    ''' Deletes the work queue with the specified id
    ''' </summary>
    ''' <param name="id">The id of the work queue to delete.</param>
    ''' <exception cref="NoSuchQueueException">If no queue with the given id could
    ''' be found on the database.</exception>
    ''' <exception cref="ForeignKeyDependencyException">If the queue is not empty and
    ''' therefore cannot be deleted.</exception>
    <OperationContract(Name:="DeleteWorkQueueById"), FaultContract(GetType(BPServerFault))>
    Sub DeleteWorkQueue(workQueueId As Guid)

    ''' <summary>
    ''' Counts the work queue log entries which fell within the given dates
    ''' (inclusive), and which were written for the given operation types.
    ''' </summary>
    ''' <param name="startDate">The inclusive start date/time from which the log
    ''' entries should be counted.</param>
    ''' <param name="endDate">The inclusive end date/time to which the log entries
    ''' should be counted</param>
    ''' <param name="ops">The operations which should be counted and returned.
    ''' </param>
    ''' <returns>A map containing the count of log records against their respective
    ''' operation type. The transaction count held against the key
    ''' <see cref="WorkQueueOperation.None"/> represents a total of all accumulated
    ''' counts in the query.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueLogCountEntries(
                                             ByVal queueNames As ICollection(Of String),
                                             ByVal startDate As Date,
                                             ByVal endDate As Date,
                                             ByVal ops As ICollection(Of WorkQueueOperation)) _
        As IDictionary(Of WorkQueueOperation, Integer)

    ''' <summary>
    ''' Copies a work item into a work queue, optionally modifying some of its
    ''' metadata in the transition
    ''' </summary>
    ''' <param name="itemId">The identifier of the item </param>
    ''' <param name="queueName">The name of the queue to copy the item to</param>
    ''' <param name="sessId">The session ID within which this method was invoked,
    ''' or <see cref="Guid.Empty"/> if it occurred outside a session</param>
    ''' <param name="defer">The date to defer the new item to, or
    ''' <see cref="DateTime.MinValue"/> to leave the item undeferred</param>
    ''' <param name="priority">The priority of the new item, or -1 to use the
    ''' priority from the item being copied</param>
    ''' <param name="tagMask">The mask to apply to the tags copied from the original
    ''' item before creating a new item. Note that this will only affect the tags
    ''' set on the new item - it does not modify the tags applied to the original
    ''' work item.</param>
    ''' <param name="status">The status of the new item, or null/empty string to
    ''' use the status from the work item being copied.</param>
    ''' <returns>The ID of the newly created work item in the target queue.</returns>
    ''' <exception cref="NoSuchWorkItemException">If no work item could be found with
    ''' the given <paramref name="itemId"/></exception>
    ''' <exception cref="NoSuchQueueException">If no work queue could be found with
    ''' the given <paramref name="queueName"/></exception>
    ''' <exception cref="OperationFailedException">If the new ID was not available
    ''' from the adding of the new work item</exception>
    ''' <exception cref="FieldLengthException">If the key value found in the given
    ''' data was longer than that accepted by the database.</exception>
    ''' <exception cref="Exception">If any other errors occur while copying the
    ''' specified work item to a queue.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CopyWorkItem(
                                 itemId As Guid, queueName As String, sessId As Guid, defer As Date,
                                 priority As Integer, tagMask As String, status As String) As Guid

    ''' <summary>
    ''' Add one or more items to the given work queue. The operation is performed
    ''' within a transaction - in the event of failure of any kind, the database is
    ''' unchanged.
    ''' </summary>
    ''' <param name="queuename">The name of the queue to add to</param>
    ''' <param name="data">A clsCollection with a row for each item to be added to
    ''' the queue.</param>
    ''' <param name="sessionId">The ID of the session which is adding this item to
    ''' the queue.</param>
    ''' <param name="defer">A Date to defer processing until, or Date.MinValue to
    ''' make the items available immediately.</param>
    ''' <param name="tags">A semi-colon separated set of tags which should be
    ''' applied to the items being added. A "+" prefix is ignored, any tags with
    ''' "-" prefixes are also ignored (you can't remove tags from an item which
    ''' has none)</param>
    ''' <param name="priority">The priority for the items being added. Items with
    ''' lower (numerically) priority values are retrieved from the queue ahead of
    ''' higher ones.</param>
    ''' <param name="status">The initial status required for the added items.</param>
    ''' <returns>Returns The collection of IDs which were created as a result of this
    ''' method invocation.</returns>
    ''' <exception cref="FieldLengthException">If the key value found in the given
    ''' data was longer than that accepted by the database. If this is thrown, then
    ''' no data will have been added to the database.</exception>
    ''' <exception cref="NoSuchQueueException">If no queue was found in the system
    ''' with the given name.</exception>
    ''' <exception cref="ArgumentException">If there is no data in the given
    ''' collection</exception>
    ''' <exception cref="Exception">If any other errors occur while adding the
    ''' specified work queue items to the database.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueAddItems(
                                      ByVal queuename As String,
                                      ByVal data As IEnumerable(Of clsCollectionRow),
                                      ByVal sessionId As Guid,
                                      ByVal defer As Date,
                                      ByVal priority As Integer,
                                      ByVal tags As String,
                                      ByVal status As String) As ICollection(Of Guid)

     <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueAddItemsAPI(
                                  ByVal queueName As String,
                                  ByVal workQueueItems As IEnumerable(Of CreateWorkQueueItemRequest)) As IEnumerable(Of Guid)

    ''' <summary>
    ''' Sets the data in the given collection into the work item specified by the
    ''' given item ID.
    ''' </summary>
    ''' <param name="id">The ID of the item to set the data on.</param>
    ''' <param name="data">The data to set on the item. This should contain one row
    ''' exactly, and, if the queue has a key value and the item has a key value, then
    ''' the data should have the same key value in it.</param>
    ''' <exception cref="KeyValueDifferenceException">If the key value in the given
    ''' data doesn't match the key value held on the item.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueItemSetData(ByVal id As Guid, ByVal data As clsCollection)

    ''' <summary>
    ''' Sets the priority of a work queue item to a specified priority
    ''' </summary>
    ''' <param name="itemId">The ID of an item whose priority should be set.</param>
    ''' <param name="priority">The priority to set in the item</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueSetPriority(ByVal itemId As Guid, ByVal priority As Integer)

    ''' <summary>
    ''' Gets the next work queue item, using the given filters
    ''' </summary>
    ''' <param name="sessionId">The session ID to set in the item, if appropriate.
    ''' Guid.Empty if it should be set to NULL.</param>
    ''' <param name="queuename">The name of the queue for which the next item is
    ''' required.</param>
    ''' <param name="keyfilter">If present, limits the selection to one with the
    ''' specified key. Otherwise, it doesn't limit the selection by key.</param>
    ''' <param name="tagMask">If present, limits the selection to items with the
    ''' specified tag mask, in the form "+wanted tag; -unwanted; +also wanted"
    ''' where "+" denotes tags which must be present, and "-" denotes tags which
    ''' must not be present. If not present, it doesn't limit the selection by tag
    ''' </param>
    ''' <returns>The work queue item which matches the given constraints, or null if
    ''' there was no pending work queue item matching those constraints.</returns>
    ''' <exception cref="Exception">If any errors occur (database or otherwise) when
    ''' attempting to get the next item</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetNext(
                                     ByVal sessionId As Guid,
                                     ByVal queuename As String,
                                     ByVal keyfilter As String,
                                     ByVal tagMask As clsTagMask) As clsWorkQueueItem

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetById(queueName As String, sessionId As Guid, workQueueItemId As Guid) As clsWorkQueueItem


    ''' <summary>
    ''' Adds the given tag to the work queue item identified by the specified ID.
    ''' </summary>
    ''' <param name="itemId">The ID of the work queue item to add a tag to</param>
    ''' <param name="tag">The tag to add to the work queue item</param>
    ''' <param name="lockedByCurrentProcess">Is the Work Queue Item locked by current process</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueItemAddTag(
                                            ByVal itemId As Guid,
                                            ByVal tag As String,
                                            ByVal lockedByCurrentProcess As Boolean)

    ''' <summary>
    ''' Removes the specified tag from the work queue item identified by the given ID
    ''' </summary>
    ''' <param name="itemId">The ID of the work queue item to remove a tag from
    ''' </param>
    ''' <param name="tag">The tag to remove from the work queue item</param>
    ''' <param name="lockedByCurrentProcess">Is the Work Queue Item locked by current process</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueItemRemoveTag(
                                               ByVal itemId As Guid,
                                               ByVal tag As String,
                                               ByVal lockedByCurrentProcess As Boolean)


    ''' <summary>
    ''' Attempts to force a retry of the given list of work queue items.
    ''' Note that this operates on the <em>item</em>, not the retry attempt - meaning
    ''' that if an instance is passed which has a later retry attempt, the state of
    ''' the latest attempt is treated as the state of the item.
    ''' This method will fail if any of the given items are in an invalid state,
    ''' ie. they still have a pending / locked instance or if a later instance has
    ''' completed successfully.
    ''' </summary>
    ''' <param name="items">The items for which a retry attempt should be forced
    ''' through disregarding the attempt number and the queue's maxattempts value.
    ''' </param>
    ''' <returns>A collection of messages indicating which items in the given list
    ''' were not force retried and explaining why for each one.</returns>
    ''' <exception cref="InvalidWorkItemStateException">If the retry failed due
    ''' to one of the specified items being either pending or completed. The
    ''' exception's detail message will be human-readable.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueForceRetry(
                                        items As ICollection(Of clsWorkQueueItem),
                                        queueID As Guid,
                                        manualChange As Boolean) As ICollection(Of String)

    ''' <summary>
    ''' Marks the given list of work queue items as exceptions
    ''' </summary>
    ''' <param name="sessionId">The ID of the session which is marking the given
    ''' items as exceptions - this will be set into the record representing each item
    ''' as the session id which last operated on the item.</param>
    ''' <param name="items">The list of work queue items to mark as exceptions</param>
    ''' <param name="reason">The reason to enter for the exception</param>
    ''' <param name="retry">True to indicate a retry should be performed if the work
    ''' queue allows any more attempts than the item has had; False to indicate that
    ''' it shouldn't</param>
    ''' <param name="keepLocked">True to keep the clones of the specified work
    ''' queue item lockeds after a retry has been initiated.</param>
    ''' <param name="retriedItemCount">After execution, this will contain the count
    ''' of items for which a retry instance has been created.</param>
    ''' <param name="queueID"> The ID of the queue being edited.</param>
    <OperationContract(Name:="WorkQueueMarkMultipleExceptionsWithQueueID"), FaultContract(GetType(BPServerFault))>
    Sub WorkQueueMarkException(
                                      sessionId As Guid,
                                      items As IList(Of clsWorkQueueItem),
                                      reason As String,
                                      retry As Boolean,
                                      keepLocked As Boolean,
                                      ByRef retriedItemCount As Integer,
                                      queueID As Guid,
                                      manualChange As Boolean)

    ''' <summary>
    ''' Acquires the active lock on a specific active queue, returning the key which
    ''' represents the lock if the acquiring of it was successful.
    ''' </summary>
    ''' <param name="ident">The identity of the queue whose active lock should be
    ''' acquired.</param>
    ''' <returns>The GUID representing the lock acquired on the queue</returns>
    ''' <exception cref="NoSuchQueueException">If the given <paramref name="ident"/>
    ''' did not correspond to a work queue.</exception>
    ''' <exception cref="ActiveQueueLockFailedException">If the lock could not be
    ''' acquired on the required active queue.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function AcquireActiveQueueLock(ident As Integer) As Guid

    ''' <summary>
    ''' Releases the active queue lock for a specified queue and lock token.
    ''' </summary>
    ''' <param name="ident">The identity of the work queue to release the active lock
    ''' from</param>
    ''' <param name="token">The token generated when the active lock was acquired.
    ''' </param>
    ''' <exception cref="NoSuchQueueException">If the identity did not represent a
    ''' work queue in the database.</exception>
    ''' <exception cref="IncorrectLockTokenException">If the token given did not
    ''' match the lock token held on the active queue</exception>
    ''' <remarks>Note that if the work queue has no active lock set on it, this
    ''' method is effectively a no-op, ie. no error is raised, but the queue is not
    ''' locked when the method exits.</remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ReleaseActiveQueueLock(ident As Integer, token As Guid)

    ''' <summary>
    ''' Sets the target session count for a specific queue
    ''' </summary>
    ''' <param name="queueIdent">The identity of the queue</param>
    ''' <param name="target">The target sessions for the queue</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetTargetSessionCount(queueIdent As Integer, target As Integer)

    ''' <summary>
    ''' Sets the target session count for multiple queues
    ''' </summary>
    ''' <param name="activeQueueDetails">The queue details</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetTargetSessionCountForMultipleActiveQueues(activeQueueDetails As IList(Of ActiveQueueTargetSessionCount))

    ''' <summary>
    ''' Mark a Work Queue item as an exception. This should only be used if the
    ''' caller already has the item locked, but no check is made at this level.
    ''' </summary>
    ''' <param name="sessionId">The session ID that this work queue item has
    ''' been worked under, empty if not part of a session.</param>
    ''' <param name="itemid">The ID of the item to mark</param>
    ''' <param name="reason">The reason for the exception</param>
    ''' <param name="retry">True if the item should be retried, up to the maximum
    ''' number of retries specified for the queue, or False to make the exception
    ''' permanent.</param>
    ''' <param name="keepLocked">True to keep the clone of the specified work
    ''' queue item locked after a retry has been initiated.</param>
    ''' <param name="retried">True if the item had a retry generated after this
    ''' instance; False if no retry was generated, either due to
    ''' <paramref name="retry"/> being False, or because the work queue's maxattempts
    ''' had been hit for this item.</param>
    ''' <param name="queueID">The ID of the queue being edited</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueMarkException(
                                          sessionId As Guid,
                                          itemid As Guid,
                                          reason As String,
                                          retry As Boolean,
                                          keepLocked As Boolean,
                                          ByRef retried As Boolean,
                                          queueID As Guid,
                                          manualChange As Boolean)

    ''' <summary>
    ''' Gets all the related retry instances of the specified work queue item.
    ''' One of item ID or Ident must be provided. This will use the item ID over the
    ''' identity if both are present.
    ''' </summary>
    ''' <param name="itemid">The (Guid) Item ID of the work queue item for which all
    ''' instances are required.</param>
    ''' <param name="ident">The (Long) identity of the work queue item instance, for
    ''' which all related instances are required.</param>
    ''' <param name="items">The collection of work queue item instances which
    ''' represent retries of the specified item.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueGetAllRetryInstances(
                                                  ByVal itemid As Guid,
                                                  ByVal ident As Long,
                                                  ByRef items As ICollection(Of clsWorkQueueItem))

    ''' <summary>
    ''' Mark a Work Queue item as complete. This should only be used if the caller
    ''' already has the item locked, but no check is made at this level.
    ''' </summary>
    ''' <param name="sessId">The ID of the session which is marking the item as
    ''' complete - this will be stored within the item as the last session operating
    ''' on the work queue item.</param>
    ''' <param name="itemId">The ID of the item to mark</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueMarkComplete(
                                     ByVal sessId As Guid,
                                     ByVal itemId As Guid)

    ''' <summary>
    ''' Defer a Work Queue item. This should only be used if the caller
    ''' already has the item locked, but no check is made at this level.
    ''' </summary>
    ''' <param name="sessionId">The ID of the session which is deferring this work
    ''' item</param>
    ''' <param name="itemid">The ID of the item to defer</param>
    ''' <param name="until">When to defer the case until. UTC, as always.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueDefer(
                                       sessionId As Guid,
                                       itemid As Guid,
                                       until As Date,
                                       queueID As Guid)

    ''' <summary>
    ''' Defers a list of work queue items. This is used from the Control Room user
    ''' interface, and only operates on unlocked items that are pending (i.e. not
    ''' completed or exceptioned).
    ''' </summary>
    ''' <param name="items">The items to have their defer date set</param>
    ''' <param name="until">The defer date/time to use on the given items. UTC, as
    ''' always.</param>
    ''' <exception cref="InvalidWorkItemStateException">If any of the items
    ''' have later instances which are pending or completed.</exception>
    <OperationContract(Name:="WorkQueueDeferList"), FaultContract(GetType(BPServerFault))>
    Sub WorkQueueDefer(
                              items As IList(Of clsWorkQueueItem),
                              until As DateTime,
                              queueID As Guid,
                              manualChange As Boolean)

    ''' <summary>
    ''' Update the status of a Work Queue item. This should only be used if the
    ''' caller already has the item locked, but no check is made at this level.
    ''' </summary>
    ''' <param name="itemid">The ID of the item to update</param>
    ''' <param name="status">The new status to set</param>
    ''' <exception cref="ArgumentOutOfRangeException">If the given status is too
    ''' long and its data was rejected by the database.</exception>
    ''' <exception cref="SqlClient.SqlException">If any other database errors occur while
    ''' attempting to update the status.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueUpdateStatus(ByVal itemid As Guid, ByVal status As String)

    ''' <summary>
    ''' Updates the status of a list of work queue items.
    ''' </summary>
    ''' <param name="items">The list of work queue item objects to update</param>
    ''' <param name="status">The status to use for the given items.</param>
    ''' <exception cref="ArgumentOutOfRangeException">If the given status is too
    ''' long and its data was rejected by the database.</exception>
    ''' <exception cref="SqlClient.SqlException">If any other database errors occur while
    ''' attempting to update the status.</exception>
    <OperationContract(Name:="WorkQueueUpdateStatusList"), FaultContract(GetType(BPServerFault))>
    Sub WorkQueueUpdateStatus(
                                     ByVal items As IList(Of clsWorkQueueItem),
                                     ByVal status As String)

    ''' <summary>
    ''' Updates the given work queue on the database and returns it with the most
    ''' up to date details (note that statistics are not updated by this method)
    ''' </summary>
    ''' <param name="workQueue">The work queue to update on the database.</param>
    ''' <returns>The work queue after updating on the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function UpdateWorkQueue(workQueue As clsWorkQueue) As clsWorkQueue

    ''' <summary>
    ''' Updates the given work queue on the database along with the running status and returns it with the most
    ''' up to date details (note that statistics are not updated by this method)
    ''' </summary>
    ''' <param name="workQueue">The work queue to update on the database.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateWorkQueueWithStatus(workQueue As clsWorkQueue)

    ''' <summary>
    ''' Unlocks an individual Work Queue Item from a Business Object.
    ''' </summary>
    ''' <param name="id">The ID of the item to unlock</param>
    ''' <returns>True if the queue item was unlocked as a result of this call;
    ''' false if it had already been unlocked</returns>
    <OperationContract(Name:="WorkQueueUnlockItemById"), FaultContract(GetType(BPServerFault))>
    Function WorkQueueUnlockItem(id As Guid) As Boolean

    ''' <summary>
    ''' Manually unlocks an individual Work Queue Item.
    ''' </summary>
    ''' <param name="workQueueItem">The Work Queue Item to unlock</param>
    ''' <param name="queueId">The ID of the Work Queue that stores the Work Queue Item to unlock</param>
    ''' <returns>True if the queue item was unlocked as a result of this call;
    ''' false if it had already been unlocked</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueUnlockItem(workQueueItem As clsWorkQueueItem, queueId As Guid) As Boolean

    ''' <summary>
    ''' Check if an item with the given key already exists in the queue.
    ''' </summary>
    ''' <param name="queuename">The name of the queue to check</param>
    ''' <param name="key">The key to check</param>
    ''' <param name="pending">True to search any currently pending items (including
    ''' locked items and deferred items with a deferral date that has passed); False
    ''' to exclude them from the search.</param>
    ''' <param name="deferred">True to search any currently deferred items (with a
    ''' deferral date in the future); False to exclude them from the search.</param>
    ''' <param name="completed">True to search any completed items; False to exclude
    ''' them from the search.</param>
    ''' <param name="terminated">True to search any exceptioned items; False to
    ''' exclude them from the search.</param>
    ''' <returns>On return, True an item exists with the given key or False otherwise
    ''' </returns>
    ''' <exception cref="Exception">If any errors occur while checking the database.
    ''' </exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueIsItemInQueue(
                                               ByVal queuename As String,
                                               ByVal key As String,
                                               ByVal pending As Boolean,
                                               ByVal deferred As Boolean,
                                               ByVal completed As Boolean,
                                               ByVal terminated As Boolean) As ICollection(Of Guid)

    ''' <summary>
    ''' Gets the latest work queue item instance from the database which has the
    ''' specified ID, or gets nothing if no item instance with that ID was found.
    ''' </summary>
    ''' <param name="id">The item ID of the work queue item which is required.
    ''' </param>
    ''' <returns>The work queue item object representing the latest retry instance
    ''' of the queue item with the specified ID, or null if no such ID was found
    ''' on the database.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetItem(ByVal id As Guid) As clsWorkQueueItem

    ''' <summary>
    ''' Deletes an item from a queue.
    ''' </summary>
    ''' <param name="id">The ID of the work queue item to be deleted.</param>
    ''' <returns>True if successful, False otherwise</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueDeleteItem(ByVal id As Guid) As Boolean

    ''' <summary>
    ''' Gets the pending items from a queue.
    ''' </summary>
    ''' <param name="name">The name of the queue.</param>
    ''' <param name="key">The keyvalue on which to filter the items.</param>
    ''' <param name="tags">The tag mask with which toe filter the items.</param>
    ''' <param name="max">The maximum number of rows to return.</param>
    ''' <param name="skip">The number of IDs to skip before starting to return values
    ''' </param>
    ''' <returns>A collection of GUIDs representing the IDs which need to be returned
    ''' for the specified queue.</returns>
    ''' <exception cref="SqlClient.SqlException">If any database errors occur while attempting
    ''' to get the pending items from the queue.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetPending(
                                            ByVal name As String,
                                            ByVal key As String,
                                            ByVal tags As clsTagMask,
                                            ByVal max As Integer, ByVal skip As Integer) As ICollection(Of Guid)

    ''' <summary>
    ''' Gets the currently locked work queue items in the specified queue,
    ''' with tags corresponding to the given mask.
    ''' </summary>
    ''' <param name="queuename">The name of the queue for which the locked items
    ''' are required.</param>
    ''' <param name="tagMask">Only items which match the given tag mask will be
    ''' returned.</param>
    ''' <returns>A dictionary of lock times mapped against item IDs representing
    ''' the work queue items within the given queue which are currently locked.
    ''' The dictionary will be in ascending lock date order.
    ''' </returns>
    ''' <exception cref="NoSuchQueueException">If the given queue name did not
    ''' represent a queue on this system.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetLocked(
                                       ByVal queuename As String, ByVal keyFilter As String, ByVal tagMask As clsTagMask) _
        As IDictionary(Of Guid, Date)

    ''' <summary>
    ''' Gets completed items which were marked as such within the given date range.
    ''' </summary>
    ''' <param name="queuename">The name of the queue to get items from</param>
    ''' <param name="startDate">The start threshold date to use. Items marked
    ''' as completed before this date will be ignored. Pass DateTime.MinValue for
    ''' no threshold</param>
    ''' <param name="endDate">The end threshold date to use. Items marked as
    ''' complete after this date will be ignored. Pass DateTime.MaxValue for no
    ''' threshold</param>
    ''' <param name="max">Indicates the maximum number of rows to return. Pass
    ''' zero for an unlimited number of rows.</param>
    ''' <returns>The collection of Item IDs which were completed within the given
    ''' search constraints.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetCompleted(
                                          ByVal queuename As String,
                                          ByVal startDate As DateTime,
                                          ByVal endDate As DateTime,
                                          ByVal keyFilter As String,
                                          ByVal tags As clsTagMask,
                                          ByVal max As Integer) As ICollection(Of Guid)

    ''' <summary>
    ''' Gets exception items which were marked as such within the given date range.
    ''' </summary>
    ''' <param name="queuename">The name of the queue to get items from</param>
    ''' <param name="startDate">The start threshold date to use. Items marked
    ''' as an exception before this date will be ignored. Pass DateTime.MinValue for
    ''' no threshold</param>
    ''' <param name="endDate">The end threshold date to use. Items marked as
    ''' an exception after this date will be ignored. Pass DateTime.MaxValue for no
    ''' threshold</param>
    ''' <param name="maxrows">Indicates the maximum number of rows to return. Pass
    ''' zero for an unlimited number of rows.</param>
    ''' <returns>A collection of GUIDs representing the IDs of the work queue items
    ''' which were (finally, ie. without a retry) marked with an exception between
    ''' the given dates.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetExceptions(
                                           queuename As String,
                                           startDate As DateTime,
                                           endDate As DateTime,
                                           keyFilter As String,
                                           tags As clsTagMask,
                                           maxrows As Integer,
                                           resourceNames As String) As ICollection(Of Guid)


    ''' <summary>
    ''' Clears an individual Work Queue item.
    ''' </summary>
    ''' <param name="queueId">The ID of the queue being edited</param>
    ''' <param name="selectedQueueItems">The items to be deleted</param>
    ''' <returns>The number of items deleted.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueClearWorked(
                                         queueId As Guid,
                                         selectedQueueItems As IList(Of clsWorkQueueItem),
                                         extraInformation As String,
                                         manualChange As Boolean) As Integer

    ''' <summary>
    ''' Clears all worked items from a queue.
    ''' </summary>
    ''' <param name="queueID">The ID of the queue to be updated.</param>
    ''' <returns>The number of items deleted.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueClearAllWorked(ByVal queueID As Guid) As Integer

    ''' <summary>
    ''' Clears worked Work Queue items, which were processed before the supplied
    ''' date.
    ''' </summary>
    ''' <param name="queueName">The name of the queue for which the items should
    ''' be cleared - null or empty to clear from all queues.</param>
    ''' <param name="thresholdDate">The threshold date to use. Items completed or
    ''' marked as an exception before this date will be deleted.</param>
    ''' <returns>The number of items deleted.</returns>
    ''' <exception cref="NoSuchQueueException">If the specified queue does not exist
    ''' in the current environment</exception>
    ''' <exception cref="Exception">If the function fails for any other reason.
    ''' </exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueClearWorkedByDate(
                                               queueName As String,
                                               thresholdDate As DateTime,
                                               allQueues As Boolean) As Integer

    ''' <summary>
    ''' Gets the work queue with the given ID, including summary information
    ''' regarding its items as specified.
    ''' </summary>
    ''' <remarks>Used by the BluePrism.Api specific contract</remarks>
    ''' <param name="id">The ID of the queue required.</param>
    ''' <returns>The queue with the latest values from the database, or null if
    ''' no queue with the given ID was found.</returns>
    ''' <exception cref="SqlClient.SqlException">If any database errors occur while attempting
    ''' retrieve the work queues.</exception>
    <OperationContract(Name:="WorkQueueGetQueueByGuid"), FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetQueue(ByVal id As Guid) As WorkQueueWithGroup

    ''' <summary>
    ''' Gets the work queue with the given identity, including summary information
    ''' regarding its items as specified.
    ''' </summary>
    ''' <param name="ident">The identity of the queue required.</param>
    ''' <returns>The queue with the latest values from the database, or null if
    ''' no queue with the given identity was found.</returns>
    ''' <exception cref="SqlClient.SqlException">If any database errors occur while attempting
    ''' retrieve the work queues.</exception>
    <OperationContract(Name:="WorkQueueGetQueueByID"), FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetQueue(ByVal ident As Integer) As clsWorkQueue

    ''' <summary>
    ''' Gets the work queue with the given name, including summary information as
    ''' specified.
    ''' </summary>
    ''' <param name="name">The name of the required work queue.</param>
    ''' <returns>A work queue object containing the metadata and, optionally, the
    ''' statistics for the required work queue.</returns>
    <OperationContract(Name:="WorkQueueGetQueueByName"), FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetQueue(ByVal name As String) As clsWorkQueue

    ''' <summary>
    ''' Gets the list of sorted work queues currently registered within the system,
    ''' not including any summary / statistical data.
    ''' </summary>
    ''' <param name="workQueueParameters">The parameters to sort and order by.</param>
    ''' <returns>The list of work queues populated by the basic data of the
    ''' work queue without any summary of their owned items.</returns>
    ''' <exception cref="SqlClient.SqlException">If any database errors occur while attempting
    ''' retrieve the work queues.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetQueues(workQueueParameters As WorkQueueParameters) As IList(Of WorkQueueWithGroup)

    ''' <summary>
    ''' Gets the list of work queues currently registered within the system,
    ''' not including any summary / statistical data.
    ''' </summary>
    ''' <returns>The list of work queues populated by the basic data of the
    ''' work queue without any summary of their owned items.</returns>
    ''' <exception cref="SqlClient.SqlException">If any database errors occur while attempting
    ''' retrieve the work queues.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetAllQueues() As IList(Of clsWorkQueue)

    ''' <summary>
    ''' Gets a set of all the work queue names within the system.
    ''' </summary>
    ''' <returns>A set of all the names of all of the work queues in the system.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetAllQueueNames() As IBPSet(Of String)

    ''' <summary>
    ''' Gets the work queues corresponding to a set of identities.
    ''' </summary>
    ''' <param name="filter">The collection of identities of the required queues.
    ''' A null value will return all queues; an empty collection will return no
    ''' queues at all.</param>
    ''' <returns>A list of work queue objects representing the required queues.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetQueuesFiltered(filter As ICollection(Of Integer)) As IList(Of clsWorkQueue)

    ''' <summary>
    ''' Gets the statistics on the given work queues with the latest data.
    ''' </summary>
    ''' <param name="queues">The work queues to update</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetQueueStatsList(queues As ICollection(Of clsWorkQueue)) As ICollection(Of clsWorkQueue)

    ''' <summary>
    ''' Updates the active work queue data for a queue, returning the updated queue
    ''' </summary>
    ''' <param name="q">The queue to update the active queue data on</param>
    ''' <returns>The queue object, with the active queue data updated.</returns>
    ''' <remarks>If the given queue is not an active queue (within the object - ie.
    ''' there is no checking done on the database), nothing will occur.</remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function UpdateActiveQueueData(q As clsWorkQueue) As clsWorkQueue

    ''' <summary>
    ''' Gets the name of the queue with the supplied ID.
    ''' </summary>
    ''' <param name="queueID">The ID of the queue fo interest.</param>
    ''' <param name="queuename">Carries back the name of the queue, on successful
    ''' completion.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueGetQueueName(ByVal queueID As Guid, ByRef queuename As String)

    ''' <summary>
    ''' Gets the id of the queue with the supplied name.
    ''' </summary>
    ''' <param name="queuename">The name of the queue fo interest.</param>
    ''' <param name="queueID">Carries back the id of the queue, on successful
    ''' completion.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueGetQueueID(ByVal queuename As String, ByRef queueID As Guid)

    ''' <summary>
    ''' Toggles the running status of the queue with the given ID and returns the
    ''' new running status of the queue.
    ''' </summary>
    ''' <param name="queueId">The ID of the queue whose running status should be
    ''' toggled.</param>
    ''' <returns>True if the queue's running status was toggled such that it is now
    ''' in a running state; False if it was toggled and is now in a not running
    ''' state. If the queue ID was not recognised, this will do no work and return
    ''' false (a nonexistent queue is, by definition, not running)</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ToggleQueueRunningStatus(ByVal queueId As Guid) As Boolean

    ''' <summary>
    ''' Updates the running status of the queues referenced by the given IDs to the
    ''' specified status.
    ''' </summary>
    ''' <param name="ids">The IDs of the queues whose running status should be set.
    ''' </param>
    ''' <param name="running">True to enable the queues' running statuses, false to
    ''' disable them</param>
    <OperationContract(Name:="SetQueueRunningStatusList"), FaultContract(GetType(BPServerFault))>
    Sub SetQueueRunningStatus(ByVal ids As ICollection(Of Guid), ByVal running As Boolean)

    ''' <summary>
    ''' Updates the 'running' status of a queue, to either pause or resume the
    ''' queue's operation.
    ''' </summary>a
    ''' <param name="queueId">The ID of the queue of interest.</param>
    ''' <param name="running">True to resume the queue, False to pause queue.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetQueueRunningStatus(ByVal queueId As Guid, ByVal running As Boolean)

    ''' <summary>
    ''' Gets the item positions for all the item IDs in the given dictionary
    ''' This will set the value corresponding to the given item ID to the position
    ''' within the given work queue.
    ''' </summary>
    ''' <param name="queueID">The globally unique id of the work queue</param>
    ''' <param name="items">The map of items, keyed on the item guid. The position
    ''' of the item in the queue will be set in here - if the item is considered not
    ''' to be in the queue (<i>eg.</i> it is currently locked, or is marked with an
    ''' exception, or has reached the maximum number of attempts for that queue) then
    ''' the position value will be unchanged by this method.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueGetItemPositions(ByVal queueID As Guid, ByRef items As IDictionary(Of Long, clsWorkQueueItem))

    ''' <summary>
    ''' Gets a session ID (which is present in the database) for the given work
    ''' queue item. This will represent the session which last updated the given
    ''' work queue item, by either 'completing' it, marking it with an exception,
    ''' deferring it, or, failing any of those, by retrieving it and locking it.
    '''
    ''' If the session ID for the given item is not set (because the item has not
    ''' been actioned yet), or if the session it refers to no longer exists on
    ''' the database (eg. it has been archived), then Guid.Empty is returned.
    ''' </summary>
    ''' <param name="itemIdent">The identity of the work queue item for which
    ''' the session is required </param>
    ''' <param name="seqNo">The sequence number closest to the time when the
    ''' given work queue item was actioned. Note that if the exact time could
    ''' not be found, this will set the sequence number to the that of the
    ''' next time <i>before</i> the time the item was actioned.
    ''' If no action time could be found on the work queue item (it checks,
    ''' in the following order, exception time, completed time, deferred
    ''' time then locked time), this will be set to -1.
    ''' Also if the action time is longer than 30 seconds after the nearest
    ''' log entry, it will be set to -1
    ''' </param>
    ''' <returns>The GUID representing the session which actioned this work
    ''' queue item. </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetValidSessionId(
                                                   ByVal itemIdent As Long, ByRef seqNo As Integer) As Guid

    ''' <summary>
    ''' Gets the xml representation of the named filter.
    ''' </summary>
    ''' <param name="FilterName">The name of the filter of interest.</param>
    ''' <param name="FilterXML">Carries back the xml requested.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueGetFilterXML(ByVal FilterName As String, ByRef FilterXML As String)

    ''' <summary>
    ''' Creates a filter with the supplied details.
    ''' </summary>
    ''' <param name="filterName">The name of the filter to create.</param>
    ''' <param name="filterXML">The xml of the new filter.</param>
    ''' <param name="filterID">Carries back the ID of the new filter</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueCreateFilter(ByVal filterName As String, ByRef filterXML As String, ByRef filterID As Guid)

    ''' <summary>
    ''' Updates the xml of the specified filter.
    ''' </summary>
    ''' <param name="FilterName">The name of the filter to be updated.</param>
    ''' <param name="FilterXML">The new filter xml.</param>
    ''' <returns>Returns the number of records affected.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueUpdateFilter(ByVal filterName As String, ByVal filterXML As String) As Integer

    ''' <summary>
    ''' Deletes the filter with the specified name.
    ''' </summary>
    ''' <param name="FilterName">The name of the filter to delete.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueueDeleteFilter(filterName As String)

    ''' <summary>
    ''' Gets a list of the names of the filters stored in the database.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetFilterNames() As List(Of String)

    ''' <summary>
    ''' Sets the default filter on a queue.
    ''' </summary>
    ''' <param name="QueueID">The queue to be updated.</param>
    ''' <param name="FilterName">The name of the default filter to be used
    ''' on this queue.</param>
    ''' <returns>True if successful, False otherwise</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueSetDefaultFilter(ByVal queueID As Guid, ByVal filterName As String) As Boolean

    ''' <summary>
    ''' Gets the default filter associated with a queue.
    ''' </summary>
    ''' <param name="queueID">The ID of the queue of interest.</param>
    ''' <param name="filterID">Carries back the ID of the default filter configured
    ''' against this queue, or Guid.Empty if none.</param>
    ''' <param name="filterName">Carries back the name of the filter identified by
    ''' FilterID, or Nothing where appropriate.</param>
    ''' <returns>True if successful, False otherwise</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetDefaultFilter(ByVal queueID As Guid, ByRef filterID As Guid, ByRef filterName As String) As Boolean

    ''' <summary>
    ''' Gets the data collection in XML format for the queue item identified by the
    ''' given identity value.
    ''' </summary>
    ''' <param name="ident">The identity of the item for which the data is required.
    ''' </param>
    ''' <returns>The data XML corresponding to the given item, or an empty string if
    ''' the item had no data associated with it.</returns>
    <OperationContract(Name:="WorkQueueItemGetDataXmlByIdent"), FaultContract(GetType(BPServerFault))>
    Function WorkQueueItemGetDataXml(ByVal ident As Long) As String

    ''' <summary>
    ''' Gets the data collection in XML format for the (latest) queue item attempt
    ''' identified by the given ID value.
    ''' </summary>
    ''' <param name="id">The item ID for the item whose data is required. The latest
    ''' attempt of this item will be used to retrieve the data.</param>
    ''' <returns>The data XML corresponding to the given item, or an empty string if
    ''' the item had no data associated with it.</returns>
    <OperationContract(Name:="WorkQueueItemGetDataXmlByID"), FaultContract(GetType(BPServerFault))>
    Function WorkQueueItemGetDataXml(ByVal id As Guid) As String

    ''' <summary>
    ''' Retrieves the filtered contents of a specific work queue.
    ''' </summary>
    ''' <param name="queueID">The ID of the queue of interest.</param>
    ''' <param name="filter">The filtering information used to retrieve
    ''' the desired data.</param>
    ''' <param name="totalItems">The total number of items matching this query.
    ''' This is the number of rows that would be returned if the maxrows
    ''' member of the query were unlimited.</param>
    ''' <param name="results">The results of the data retrieval in the form of a
    ''' collection of <see cref="clsWorkQueueItem"/> objects.
    ''' </param>
    ''' <param name="sErr">On failure, contains an error description.</param>
    ''' <returns>True if successful, False otherwise</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub WorkQueuesGetQueueFilteredContents(
                                                       ByVal queueID As Guid,
                                                       ByVal filter As WorkQueueFilter,
                                                       ByRef totalItems As Integer,
                                                       ByRef results As ICollection(Of clsWorkQueueItem))

    ''' <summary>
    ''' Retrieves the items within the queue.
    ''' </summary>
    ''' <param name="workQueueId">The Id of the work queue we want our items from.
    ''' </param>
    ''' <returns>A collection of items within the work queue.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetQueueItems(ByVal workQueueId As Guid, workQueueItemParameters As WorkQueueItemParameters) As ICollection(Of clsWorkQueueItem)

    ''' <summary>
    ''' Gets report data using the given parameters - returns all data inside a
    ''' report data object.
    ''' </summary>
    ''' <param name="params">The parameters defining the report data to be
    ''' retrieved.</param>
    ''' <returns>A report data object with the pertinent data prepopulated.</returns>
    ''' <exception cref="ArgumentException">If any of the following are true :-
    ''' <list>
    ''' <item>No queue name was provided</item>
    ''' <item>No start date was provided (either 'added' or 'finished')</item>
    ''' <item>No state was set to be included (unworked, deferred, completed or
    ''' exceptioned)</item></list></exception>
    ''' <exception cref="SqlClient.SqlException">If any database errors occur.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function WorkQueueGetReportData(ByVal params As clsWorkQueuesBusinessObject.ReportParams) As clsWorkQueuesBusinessObject.ReportData

    ''' <summary>
    ''' Checks if the scheduler data version number has been updated from the
    ''' specified value, returning the new version number into the provided
    ''' ByRef parameter if it has changed.
    ''' </summary>
    ''' <param name="verno">The current version number held in the system, and
    ''' on exit of the method, the current version number on the database.
    ''' </param>
    ''' <returns>True if the version number on the database differed from that
    ''' provided; False otherwise.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function HasSchedulerDataUpdated(ByRef verno As Long) As Boolean

    ''' <summary>
    ''' Gets the public holiday schema from the database
    ''' </summary>
    ''' <returns>The PublicHolidaySchema representing the public holidays held
    ''' on the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function GetPublicHolidaySchema() As PublicHolidaySchema

    ''' <summary>
    ''' Gets the current calendar data from the database.
    ''' </summary>
    ''' <returns>
    ''' A Dictionary of calendar objects keyed by ID.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAllCalendars() As IDictionary(Of Integer, ScheduleCalendar)

    ''' <summary>
    ''' Creates the given calendar on the database
    ''' </summary>
    ''' <param name="cal">The calendar to create</param>
    ''' <returns>The generated integer ID for the calendar</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CreateCalendar(ByVal cal As ScheduleCalendar) As Integer

    ''' <summary>
    ''' Updates the given calendar on the database
    ''' </summary>
    ''' <param name="cal">The calendar to update</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateCalendar(ByVal cal As ScheduleCalendar)

    ''' <summary>
    ''' Deletes the given calendar from the database, along with any child
    ''' records (public holiday overrides and non-working days)
    ''' </summary>
    ''' <param name="cal">The calendar to delete, with the ID set to the ID
    ''' which needs to be deleted</param>
    ''' <exception cref="InvalidOperationException">If a record exists which
    ''' requires the specified calendar, and thus it cannot be deleted.
    ''' </exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteCalendar(ByVal cal As ScheduleCalendar)

    ''' <summary>
    ''' Checks the given schedule to see if it has been modified on the database
    ''' since it was last refreshed. If further changes have been made on the
    ''' database, the schedule is reloaded, otherwise it is left alone.
    ''' </summary>
    ''' <param name="schedule">The schedule to check to see if it has changed.
    ''' After this method returns, if the database has changed this will contain
    ''' the new version of the schedule from the database. If the schedule has
    ''' been deleted, this will contain null.</param>
    ''' <returns>True if the schedule on the database has changed since the
    ''' version passed in; False if it remains the same.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function SchedulerRefreshIfChanged(ByRef schedule As SessionRunnerSchedule) As Boolean

    ''' <summary>
    ''' Gets all active schedules currently stored on the database.
    ''' </summary>
    ''' <returns>A collection of all schedule objects which are not retired
    ''' from the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function SchedulerGetActiveSchedules(ByRef versionNo As Long) _
            As ICollection(Of SessionRunnerSchedule)

    ''' <summary>
    ''' Gets all retired schedules currently stored on the database.
    ''' </summary>
    ''' <returns>A collection of database-backed schedules which represent the
    ''' currently retired schedules.</returns>
    ''' <remarks></remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function SchedulerGetRetiredSchedules() As ICollection(Of SessionRunnerSchedule)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SchedulerGetScheduleSummaries(scheduleParameters As ScheduleParameters) As ICollection(Of ScheduleSummary)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SchedulerGetScheduleSummary(scheduleId As Integer) As ScheduleSummary

    ''' <summary>
    ''' Gets the schedule represented by the given ID.
    ''' </summary>
    ''' <param name="id">The ID of the schedule which is required.</param>
    ''' <returns>The fully populated schedule which corresponds to the given ID,
    ''' or Nothing if no such schedule exists.</returns>
    <OperationContract(Name:="SchedulerGetScheduleByID"), FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function SchedulerGetSchedule(ByVal id As Integer) As ISchedule

    ''' <summary>
    ''' Gets the schedule with the given name from the database.
    ''' Since schedule's names must be unique, there should only be at most one
    ''' schedule with the given name on the database.
    ''' </summary>
    ''' <param name="name">The name of the schedule required.</param>
    ''' <returns>The schedule with the given name or Nothing if no schedule with
    ''' the given name was found.</returns>
    <OperationContract(Name:="SchedulerGetScheduleByName"), FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function SchedulerGetSchedule(ByVal name As String) As ISchedule

    ''' <summary>
    ''' Creates the given schedule and all its dependents on the database.
    ''' </summary>
    ''' <param name="schedule">The schedule to be created</param>
    ''' <returns>The schedule after it has been created with its ID and all its
    ''' task IDs set appropriately</returns>
    ''' <exception cref="Server.Domain.Models.NameAlreadyExistsException">If either the name of the
    ''' schedule is already in use on the database, or if 2 tasks on the schedule
    ''' have the same name.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function SchedulerCreateSchedule(ByVal schedule As SessionRunnerSchedule) _
        As SessionRunnerSchedule

    ''' <summary>
    ''' Updates the given schedule on the database and returns it with any IDs
    ''' populated as appropriate
    ''' </summary>
    ''' <param name="schedule">The schedule to update</param>
    ''' <returns>The schedule after update with all IDs appropriately populated
    ''' </returns>
    ''' <exception cref="Server.Domain.Models.NameAlreadyExistsException">If either the name of the
    ''' schedule is already in use on the database, or if 2 tasks on the schedule
    ''' have the same name.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function SchedulerUpdateSchedule(ByVal schedule As SessionRunnerSchedule) As SessionRunnerSchedule

    ''' <summary>
    ''' Retires the given schedule, thereby removing it from active duty
    ''' </summary>
    ''' <param name="schedule">The schedule to be retired</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Sub SchedulerRetireSchedule(ByVal schedule As SessionRunnerSchedule)

    ''' <summary>
    ''' Unretires the given schedule, thereby restoring it to active duty
    ''' </summary>
    ''' <param name="schedule">The schedule to be unretired</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Sub SchedulerUnretireSchedule(ByVal schedule As SessionRunnerSchedule)

    ''' <summary>
    ''' Deletes the given schedule from the database, along with all its
    ''' dependents.
    ''' </summary>
    ''' <param name="schedule">The schedule to be deleted</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Sub SchedulerDeleteSchedule(ByVal schedule As SessionRunnerSchedule)

    ''' <summary>
    ''' Checks that the task can be safely deleted from its schedule.
    ''' ie. that it is not referenced on any tables which require its presence if
    ''' the schedule remains present (currently the BPAScheduleLogEntry only).
    ''' Note that this doesn't mean that the schedule which owns it cannot be
    ''' deleted, just that the task cannot be deleted from the schedule, with
    ''' the schedule left in place.
    ''' </summary>
    ''' <param name="id">The ID of the task to check for deletion availability
    ''' </param>
    ''' <returns>True if no error would occur when deleting the task from the
    ''' schedule.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SchedulerCanDeleteTask(ByVal id As Integer) As Boolean

    ''' <summary>
    ''' Gets the Task name given the task ID
    ''' </summary>
    ''' <param name="id">The task ID</param>
    ''' <returns>The task name</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SchedulerGetTaskNameFromID(ByVal id As Integer) As String

    ''' <summary>
    ''' Gets the historical schedule log for the execution of the given
    ''' schedule at the specified instant in time.
    ''' </summary>
    ''' <param name="scheduleId">The ID of the schedule for which the log
    ''' is required.</param>
    ''' <param name="instant">The instant in time for which the log is
    ''' required.</param>
    ''' <returns>The log which represents the historical log of the
    ''' execution required schedule at the required time, or Nothing if
    ''' no log exists for that point in time.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function SchedulerGetLog(ByVal scheduleId As Integer, ByVal instant As DateTime) _
        As HistoricalScheduleLog

    ''' <summary>
    ''' Gets the running logs for the given schedule ID.
    ''' Note that the schedule is not set in these logs by virtue of the fact
    ''' that the server doesn't have access to the schedule objects directly
    ''' </summary>
    ''' <param name="scheduleId">The ID for which the currently running logs
    ''' are required.</param>
    ''' <returns>A collection of schedule logs representing the currently
    ''' running instances of the specified schedule.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function SchedulerGetRunningLogs(ByVal scheduleId As Integer) _
        As ICollection(Of IScheduleLog)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function SchedulerGetCurrentAndPassedLogs(schedulerParameters As ScheduleLogParameters) _
        As ICollection(Of ScheduleLog)

    ''' <summary>
    ''' Gets the schedule logs for the given schedule between the given dates.
    ''' Note that the dates used here are exclusive.
    ''' </summary>
    ''' <param name="scheduleId">The ID of the schedule for which the logs
    ''' are required.</param>
    ''' <param name="after">The date after which the logs are required.
    ''' </param>
    ''' <param name="before">The date before which the logs are required.
    ''' </param>
    ''' <param name="reasons">A collection of activation reasons to filter the
    ''' logs on. A null or empty value indicates that all logs should be
    ''' returned regardless of their activation reason.</param>
    ''' <returns>A non-null collection of historical logs for execution
    ''' instances of the specified schedule which fell between the given
    ''' dates.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    <UseNetDataContractSerializer>
    Function SchedulerGetLogs(ByVal scheduleId As Integer,
                                         ByVal after As DateTime, ByVal before As DateTime,
                                         ByVal ParamArray reasons() As TriggerActivationReason) _
            As ICollection(Of HistoricalScheduleLog)

    ''' <summary>
    ''' Creates a new schedule log with the given ID and instant in time
    ''' on the database, returning the newly created schedule log ID.
    ''' </summary>
    ''' <param name="scheduleId">The ID of the schedule for which this
    ''' log is being created.</param>
    ''' <param name="instant">The instant in time that this log represents
    ''' the execution for.</param>
    ''' <param name="reason">The reason that the schedule was activated.</param>
    ''' <returns>The ID of the newly created schedule log</returns>
    ''' <exception cref="AlreadyActivatedException">If an entry was found on the
    ''' database for the specified schedule at the specified instance time.
    ''' </exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SchedulerCreateLog(
                                       ByVal scheduleId As Integer, ByVal instant As DateTime,
                                       ByVal reason As TriggerActivationReason, ByVal schedulerName As String) As Integer

    ''' <summary>
    ''' Pulses the schedule log with the given ID, effectively indicating that
    ''' the log is still in use - ie. the schedule is still being executed by
    ''' the scheduler.
    ''' </summary>
    ''' <param name="logId">The log ID which should be pulsed with the current
    ''' date/time</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SchedulerPulseLog(ByVal logId As Integer, ByVal schedulerName As String)

    ''' <summary>
    ''' Writes a schedule log event record to the database with the given
    ''' values.
    ''' </summary>
    ''' <param name="logId">The ID of the schedule log that this event is part of
    ''' </param>
    ''' <param name="entry">The entry to be added - note that the date on this
    ''' entry is ignored and the current date/time on the database is used
    ''' for the entry.</param>
    ''' <returns>The date/time of the entry after it has been inserted onto the
    ''' database - note that this time will be in UTC.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SchedulerAddLogEntry(
        ByVal logId As Integer,
        ByVal entry As ScheduleLogEntry) As Date

    ''' <summary>
    ''' Gets a list of clsScheduleList from the database with the given ScheduleListType
    ''' </summary>
    ''' <param name="type">The type of list to get</param>
    ''' <returns>A list of clsScheduleList but with only the name and ID populated.</returns>
    <OperationContract,
        UseNetDataContractSerializer,
        FaultContract(GetType(BPServerFault))>
    Function SchedulerGetScheduleLists(ByVal type As ScheduleListType) As ICollection(Of ScheduleList)

    ''' <summary>
    ''' Gets the schedule list corresponding to the given ID, or null if the ID
    ''' did not match any schedule lists on the database.
    ''' </summary>
    ''' <param name="id">The ID of the schedule list to retrieve.</param>
    ''' <returns>The schedule list corresponding to the given ID.</returns>
    <OperationContract(Name:="SchedulerGetScheduleListByID"),
        UseNetDataContractSerializer,
        FaultContract(GetType(BPServerFault))>
    Function SchedulerGetScheduleList(ByVal id As Integer) As ScheduleList

    ''' <summary>
    ''' Gets the complete schedule list corresponding to the given name and
    ''' type, or null if no such list exists.
    ''' </summary>
    ''' <param name="name">The name of the required list.</param>
    ''' <param name="type">The type of the required list.</param>
    ''' <returns>The schedule list corresponding to the given name and type,
    ''' or null if no such list existed on the database.</returns>
    <OperationContract(Name:="SchedulerGetScheduleListByName"),
        UseNetDataContractSerializer,
        FaultContract(GetType(BPServerFault))>
    Function SchedulerGetScheduleList(
        ByVal name As String, ByVal type As ScheduleListType) As ScheduleList

    ''' <summary>
    ''' Creates a clsScheduleList
    ''' </summary>
    ''' <param name="list">The clsScheduleList to create</param>
    <OperationContract,
        UseNetDataContractSerializer,
        FaultContract(GetType(BPServerFault))>
    Function SchedulerCreateScheduleList(ByVal list As ScheduleList) As Integer

    ''' <summary>
    ''' Updates a clsScheduleList
    ''' </summary>
    ''' <param name="list">the clsScheduleList to update</param>
    <OperationContract,
        UseNetDataContractSerializer,
        FaultContract(GetType(BPServerFault))>
    Sub SchedulerUpdateScheduleList(ByVal list As ScheduleList)

    ''' <summary>
    ''' Deletes a clsScheduleList
    ''' </summary>
    ''' <param name="listid">the id of the ScheduleList to delete</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SchedulerDeleteScheduleList(ByVal listid As Integer)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SchedulerGetSessionsWithinTask(taskId As Integer) As ICollection(Of Server.Domain.Models.ScheduledSession)

    ''' <summary>
    ''' Saves a new version of the given package, using the properties currently set
    ''' in the supplied object.
    ''' </summary>
    ''' <param name="pkg">The package to save.</param>
    ''' <remarks>
    ''' This will insert a new package instance with the same ID as it current has.
    ''' If it doesn't yet have an ID, this will insert a brand new package instance
    ''' with a newly generated ID.
    ''' On exit, the package object will have its ID and Ident properties updated to
    ''' match that on the database.
    ''' Note that there are no redundancy checks, so if the current version of the
    ''' package is the same as that which has just been passed, it will still save
    ''' a new version of the package.
    ''' </remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SavePackage(ByVal pkg As clsPackage) As clsPackage

    ''' <summary>
    ''' Initiates import of the given release into the database. The import is run
    ''' as a background job.
    ''' </summary>
    ''' <param name="release">The release to import</param>
    ''' <param name="notifier">The notifier used to signal that updates have been made
    ''' as the job progresses</param>
    ''' <returns>An identifier for the background job under which the import is
    ''' running</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ImportRelease(release As clsRelease, unlockProcesses As Boolean, notifier As BackgroundJobNotifier) _
        As BackgroundJob

    ''' <summary>
    ''' Initiates import of the dummy release into the database. The import is run
    ''' as a background job.
    ''' </summary>
    ''' <param name="release">The dummy release to import</param>
    ''' <param name="notifier">The notifier used to signal that updates have been made
    ''' as the job progresses</param>
    ''' <returns>An identifier for the background job under which the import is
    ''' running</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ImportProcessOrObjectAsRelease(release As clsRelease, unlockProcesses As Boolean, notifier As BackgroundJobNotifier) _
        As BackgroundJob

    ''' <summary>
    ''' Gets the package with the given ID
    ''' </summary>
    ''' <param name="id">The ID of the package required.</param>
    ''' <returns>The package corresponding to the given ID.</returns>
    ''' <exception cref="NoSuchElementException">If no package was found with the
    ''' given ID.</exception>
    <OperationContract(Name:="GetPackageByID"),
 FaultContract(GetType(BPServerFault))>
    Function GetPackage(ByVal id As Integer) As clsPackage

    ''' <summary>
    ''' Gets the package with the given name.
    ''' </summary>
    ''' <param name="name">The name of the required package.</param>
    ''' <returns>The package (with release data) corresponding to the given name,
    ''' or null if no such package exists.</returns>
    <OperationContract(Name:="GetPackageByName"),
 FaultContract(GetType(BPServerFault))>
    Function GetPackage(ByVal name As String) As clsPackage

    ''' <summary>
    ''' Gets all the packages in the system and their contents, but not any related
    ''' releases. These can be retrieved by calling <see cref="clsServer.PopulateReleases"/>
    ''' for the package for which releases are required.
    ''' </summary>
    ''' <returns>A collection of all packages on the system.</returns>
    <OperationContract(Name:="GetPackages"),
    FaultContract(GetType(BPServerFault))>
    Function GetPackages() As ICollection(Of clsPackage)

    ''' <summary>
    ''' Gets all the packages in the system, optionally along with their releases.
    ''' </summary>
    ''' <param name="includeReleases">True to retrieve and populate the releases into
    ''' the returned package objects.</param>
    ''' <returns>The packages on the system.</returns>
    <OperationContract(Name:="GetPackagesIncludingReleases"),
                   FaultContract(GetType(BPServerFault))>
    Function GetPackages(ByVal includeReleases As Boolean) As ICollection(Of clsPackage)

    ''' <summary>
    ''' Gets the packages (without release data) which contain the given process.
    ''' </summary>
    ''' <param name="id">The ID of the process to check packages for.</param>
    ''' <returns>The collection of packages which contain the required process.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetPackagesWithProcess(ByVal id As Guid) As ICollection(Of clsPackage)

    ''' <summary>
    ''' Deletes the given package, and any related data.
    ''' </summary>
    ''' <param name="pkg">The package to delete.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeletePackage(ByVal pkg As clsPackage)

    ''' <summary>
    ''' Creates the specified release on the database.
    ''' </summary>
    ''' <param name="rel">The release to create.</param>
    ''' <returns>The newly created release with its ID set.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CreateRelease(ByVal rel As clsRelease) As clsRelease

    ''' <summary>
    ''' Checks if the release is valid and would successfully update the database.
    ''' Currently, this only checks for a duplicate release name within the package.
    ''' </summary>
    ''' <param name="rel">The release to check</param>
    ''' <returns>True if the name on the release is valid; False if such a name
    ''' already exists on the release's package.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsValidRelease(ByVal rel As clsRelease) As Boolean

    ''' <summary>
    ''' Gets the color preferences for the environment colors
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub GetEnvironmentColors(ByRef backColor As Color, ByRef forecolor As Color)

    ''' <summary>
    ''' Gets the integer preference with the given name.
    ''' Throws an exception if the given name was not found.
    ''' This first searches the current logged in user, and then the system
    ''' preferences for the given name.
    ''' </summary>
    ''' <param name="name">The name of the required preference.</param>
    ''' <returns>The integer value assigned to the given name in the preferences.
    ''' </returns>
    ''' <exception cref="NoSuchPreferenceException">If no integer preference was
    ''' found with the specified name.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetIntPref(ByVal name As String) As Integer

    ''' <summary>
    ''' Gets the boolean preference with the given name.
    ''' Throws an exception if the given name was not found.
    ''' This first searches the current logged in user, and then the system
    ''' preferences for the given name.
    ''' </summary>
    ''' <param name="name">The name of the required preference.</param>
    ''' <returns>The boolean value assigned to the given name in the preferences.
    ''' </returns>
    ''' <exception cref="NoSuchPreferenceException">If no boolean preference was
    ''' found with the specified name.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetBoolPref(ByVal name As String) As Boolean

    ''' <summary>
    ''' Gets the guid preference with the given name.
    ''' Throws an exception if the given name was not found.
    ''' This first searches the current logged in user, and then the system
    ''' preferences for the given name.
    ''' </summary>
    ''' <param name="name">The name of the required preference.</param>
    ''' <returns>The boolean value assigned to the given name in the preferences.
    ''' </returns>
    ''' <exception cref="NoSuchPreferenceException">If no guid preference was
    ''' found with the specified name.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetGuidPref(name As String) As Guid

    ''' <summary>
    ''' Gets the string preference with the given name.
    ''' Throws an exception if the given name was not found.
    ''' </summary>
    ''' <param name="name">The name of the required preference.</param>
    ''' <returns>The string value assigned to the given name in the preferences.
    ''' </returns>
    ''' <exception cref="NoSuchPreferenceException">If no string preference was
    ''' found with the specified name.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetStringPref(ByVal name As String) As String

    ''' <summary>
    ''' Gets the preference with the given name and of the given type, or the
    ''' specified default value if the preference was not found.
    ''' This will check the user prefs for the current user (if a user is logged
    ''' in), then the system prefs for a pref of the given name and type.
    ''' </summary>
    <OperationContract(Name:="GetPrefOfString"), FaultContract(GetType(BPServerFault))>
    Function GetPref(ByVal name As String, ByVal defaultValue As String) As String

    <OperationContract(Name:="GetPrefOfInteger"), FaultContract(GetType(BPServerFault))>
    Function GetPref(ByVal name As String, ByVal defaultValue As Integer) As Integer

    <OperationContract(Name:="GetPrefOfBoolean"), FaultContract(GetType(BPServerFault))>
    Function GetPref(ByVal name As String, ByVal defaultValue As Boolean) As Boolean

    <OperationContract(Name:="GetPrefOfColor"), FaultContract(GetType(BPServerFault))>
    Function GetPref(ByVal name As String, ByVal defaultValue As Color) As Color

    <OperationContract(Name:="GetPrefOfGuid"), FaultContract(GetType(BPServerFault))>
    Function GetPref(ByVal name As String, ByVal defaultValue As Guid) As Guid

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetConnectionCheckRetrySecondsPref(ByVal defaultValue As Integer) As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SchedulerGetScheduledTasks(id As Integer) As ICollection(Of BluePrism.Server.Domain.Models.ScheduledTask)


    ''' <summary>
    ''' Sets the system preference with the given name to the given value.
    ''' </summary>
    ''' <param name="name">The name of the preference</param>
    ''' <param name="value">The value of the preference</param>
    <OperationContract(Name:="SetSystemPrefOfString"),
    FaultContract(GetType(BPServerFault))>
    Sub SetSystemPref(ByVal name As String, ByVal value As String)

    <OperationContract(Name:="SetSystemPrefOfInteger"),
 FaultContract(GetType(BPServerFault))>
    Sub SetSystemPref(ByVal name As String, ByVal value As Integer)

    <OperationContract(Name:="SetSystemPrefOfBoolean"),
 FaultContract(GetType(BPServerFault))>
    Sub SetSystemPref(ByVal name As String, ByVal value As Boolean)

    <OperationContract(Name:="SetSystemPrefOfColor"),
 FaultContract(GetType(BPServerFault))>
    Sub SetSystemPref(ByVal name As String, ByVal value As Color)

    <OperationContract(Name:="SetSystemPrefOfGuid"),
 FaultContract(GetType(BPServerFault))>
    Sub SetSystemPref(ByVal name As String, ByVal value As Guid)

    ''' <summary>
    ''' Sets the current user's preference with the given name to the given value
    ''' </summary>
    ''' <param name="name">The name of the preference</param>
    ''' <param name="value">The value of the preference</param>
    <OperationContract(Name:="SetUserPrefOfString"),
 FaultContract(GetType(BPServerFault))>
    Sub SetUserPref(ByVal name As String, ByVal value As String)

    <OperationContract(Name:="SetUserPrefOfInteger"),
 FaultContract(GetType(BPServerFault))>
    Sub SetUserPref(ByVal name As String, ByVal value As Integer)

    <OperationContract(Name:="SetUserPrefOfBoolean"),
 FaultContract(GetType(BPServerFault))>
    Sub SetUserPref(ByVal name As String, ByVal value As Boolean)

    <OperationContract(Name:="SetUserPrefOfColor"),
 FaultContract(GetType(BPServerFault))>
    Sub SetUserPref(ByVal name As String, ByVal value As Color)

    <OperationContract(Name:="SetUserPrefOfGuid"),
 FaultContract(GetType(BPServerFault))>
    Sub SetUserPref(ByVal name As String, ByVal value As Guid)


    ''' <summary>
    ''' Deletes the user pref with the given name for the currently logged in user.
    ''' </summary>
    ''' <param name="prefName">The pref name to delete for the current user.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteUserPref(prefName As String)

    ''' <summary>
    ''' Adds the node to the specified group. If the node being added is a group
    ''' and no groupID is passed then a top-level group is created (groupID must
    ''' always be passed for leaf nodes).
    ''' </summary>
    ''' <param name="treeType">The type of tree being added to</param>
    ''' <param name="groupID">The group to add to</param>
    ''' <param name="mem">The node to add</param>
    <OperationContract(Name:="AddSingleNodeToGroup"),
    FaultContract(GetType(BPServerFault))>
    Sub AddToGroup(treeType As GroupTreeType, groupID As Guid, mem As GroupMember)

    ''' <summary>
    ''' Adds the list of nodes node to the specified group. If the node being added
    ''' is a group and no groupID is passed then a top-level group is created
    ''' (groupID must always be passed for leaf nodes).
    ''' </summary>
    ''' <param name="treeType">The type of tree being added to</param>
    ''' <param name="groupID">The group to add to</param>
    ''' <param name="nodes">The nodes to add</param>
    <OperationContract(Name:="AddListOfNodesToGroup"),
    FaultContract(GetType(BPServerFault))>
    Sub AddToGroup(treeType As GroupTreeType, groupID As Guid, nodes As IEnumerable(Of GroupMember))

    ''' <summary>
    ''' Removes the node from the specified group. Note that for leaf nodes this
    ''' does not delete the underlying item (object/process etc.), it merely returns
    ''' it to the top-level of the tree. Where a group is being removed all it's
    ''' descendant subgroups and contents will be removed too.
    ''' </summary>
    ''' <param name="groupID">The group to remove from</param>
    ''' <param name="mem">The node to remove</param>
    <OperationContract(Name:="RemoveSingleNodeFromGroup"),
    FaultContract(GetType(BPServerFault))>
    Sub RemoveFromGroup(groupID As Guid, mem As GroupMember)

    ''' <summary>
    ''' Removes the list of nodes from the specified group. Note that for leaf nodes
    ''' this does not delete the underlying items (objects/processes etc.), it merely
    ''' returns them to the top-level of the tree. Where a group is being removed
    ''' all it's descendant subgroups and contents will be removed too.
    ''' </summary>
    ''' <param name="groupID">The group to remove from</param>
    ''' <param name="nodes">The nodes to remove</param>
    <OperationContract(Name:="RemoveNodesFromGroup"),
    FaultContract(GetType(BPServerFault))>
    Sub RemoveFromGroup(groupID As Guid, nodes As IEnumerable(Of GroupMember))

    ''' <summary>
    ''' Removes a group from the database.
    ''' </summary>
    ''' <param name="gp">The group to remove</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RemoveGroup(gp As Group)

    ''' <summary>
    ''' Returns the ID of the default group of this tree
    ''' </summary>
    ''' <param name="tree">The tree type for which to get the default group id</param>
    ''' <returns></returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDefaultGroupId(tree As GroupTreeType) As Guid

    ''' <summary>
    ''' Returns true if this tree type has a default group
    ''' </summary>
    ''' <param name="tree">The tree type to check if it has a default group</param>
    ''' <returns></returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function HasDefaultGroup(tree As GroupTreeType) As Boolean

    ''' <summary>
    ''' Moves the list of nodes from one group to another.
    ''' The GroupMember is returned once with updated values once it has been saved to the database.
    ''' </summary>
    ''' <param name="fromGroupID">The group to move nodes from</param>
    ''' <param name="toGroupID">The group to mode nodes to</param>
    ''' <param name="movingGroup">The list of nodes to move</param>
    ''' <param name="isCopy">Indicates that the member is a copy</param>
    ''' <returns> The moved group once it has been saved to the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function MoveGroupEntry(fromGroupID As Guid,
                       toGroupID As Guid,
                       movingGroup As GroupMember, isCopy As Boolean) As GroupMember

    ''' <summary>
    ''' Updates the given group on the database. At this point, all that means is
    ''' that it can rename the group.
    ''' </summary>
    ''' <param name="group">The group to update.</param>
    ''' <returns>The group node after the update. Note that this may change if the
    ''' group has been inserted onto the database (for direct connections, this will
    ''' be the same object; for BP Server connections, it may not be, but the
    ''' returned value will reflect the data on the database)</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function UpdateGroup(group As Group) As Group

    ''' <summary>
    ''' Gets the group IDs of any groups containing the given basenode.
    ''' </summary>
    ''' <param name="mem">The member for which all containing groups are required.
    ''' </param>
    ''' <returns>A collection of IDs representing the groups which contain the
    ''' element given.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetIdsOfGroupsContaining(
                                     mem As GroupMember) As ICollection(Of Guid)

    ''' <summary>
    ''' Gets the tree from the database with the given type.
    ''' </summary>
    ''' <param name="tp">The type detailing which tree to retrieve</param>
    ''' <returns>The tree with the given type from the database. Note that the tree
    ''' will have no <see cref="GroupTree.Store">store</see> assigned to it on return
    ''' from this method.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetTree(tp As GroupTreeType, ca As TreeAttributes) As GroupTree

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SaveTreeNodeExpandedState(id As Guid, expanded As Boolean, treeType As GroupTreeType)

    ''' <summary>
    ''' Gets a group and its subtree from the database.
    ''' </summary>
    ''' <param name="id">The ID of the required group</param>
    ''' <returns>The group, disconnected from any overarching tree, which corresponds
    ''' to the given group ID or null if no such group was found.</returns>
    ''' <exception cref="ArgumentNullException">If <paramref name="id"/> is
    ''' <see cref="Guid.Empty"/></exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetGroup(id As Guid) As Group

    ''' <summary>
    ''' Gets a group and, optionally, its subtree from the database.
    ''' </summary>
    ''' <param name="id">The ID of the required group</param>
    ''' <param name="recursive">True to descend into the group's subtree and
    ''' retrieve all group members within its subtree; False to only retrieve the
    ''' immediate members of the group</param>
    ''' <returns>The group, disconnected from any overarching tree, which corresponds
    ''' to the given group ID or null if no such group was found.</returns>
    ''' <exception cref="ArgumentNullException">If <paramref name="id"/> is
    ''' <see cref="Guid.Empty"/></exception>
    <OperationContract(Name:="GetGroupRecursive"), FaultContract(GetType(BPServerFault))>
    Function GetGroup(id As Guid, recursive As Boolean) As Group

    ''' <summary>
    ''' Returns the ID and full path (from the root of the tree) of any groups which
    ''' contain the passed member. Groups in the path are separated by a forward
    ''' slash.
    ''' </summary>
    ''' <param name="mem">The member to look for</param>
    ''' <returns>The collection of group IDs and paths</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetPathsToMember(mem As GroupMember) As IDictionary(Of Guid, String)


    ''' <summary>
    ''' Acquires the environment lock associated with the given name, setting the
    ''' session ID as appropriate.
    ''' </summary>
    ''' <param name="name">The name of the lock required.</param>
    ''' <param name="preferredToken">The token to use if a lock is free. If this is
    ''' null or empty, a token will be generated to associate with the lock.</param>
    ''' <param name="sessionIdentifier">The session identifier to set into the lock.</param>
    ''' <param name="comment">The comment to set on the lock.</param>
    ''' <param name="forceLockRelease">If specified and the requested environmental lock's status is held,
    ''' the current lock would be released if it was acquired before the value specified in this parameter (in seconds).
    ''' If a default value is used (0), then no attempts to release held locks will be made</param>
    ''' <returns>A token associated with the lock which can be used to release the
    ''' lock when it is finished with.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function AcquireEnvLock(
                                       ByVal name As String,
                                       ByVal preferredToken As String,
                                       ByVal sessionIdentifier As SessionIdentifier,
                                       ByVal comment As String,
                                       ByVal forceLockRelease As Integer) As String

    ''' <summary>
    ''' Releases the environment lock with the given name and token, leaving the
    ''' specified comment on the lock record.
    ''' </summary>
    ''' <param name="name">The name of the lock to release.</param>
    ''' <param name="token">The token to use to release the lock. If this is null or
    ''' empty, then the lock will be 'force released' - if the token is provided then
    ''' it must match the token on the database or an error will be raised.</param>
    ''' <param name="comment">The comment to leave on the lock record after it has
    ''' been successfully released.</param>
    ''' <param name="sessionIdentifier">The Identifier of the session for which the locks should be
    ''' released. A null value will not limit the releasing of locks to a
    ''' single session - ie. it will release all matching locks regardless of session
    ''' </param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ReleaseEnvLock(name As String, token As String, comment As String, sessionIdentifier As SessionIdentifier, keepLock As Boolean)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ManualReleaseEnvLock(name As String, token As String, comment As String)

    ''' <summary>
    ''' Releases all environment locks associated with the given token, setting
    ''' the specified comment on the lock records.
    ''' </summary>
    ''' <param name="token">The token for which all locks should be released.</param>
    ''' <param name="comment">The comment to set on the lock record for any
    ''' released locks.</param>
    ''' <param name="sessionIdentifier">The identifier for the session for which the locks should be
    ''' released. A null value will not limit the releasing of locks to a
    ''' single session - ie. it will release all matching locks regardless of session
    ''' </param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ReleaseAllEnvLocks(token As String, comment As String, sessionIdentifier As SessionIdentifier, keepLock As Boolean)

    ''' <summary>
    ''' Releases the environment locks associated with the given session
    ''' </summary>
    ''' <param name="sessionIdentifier">The identifier of the session whose locks should be released.
    ''' </param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ReleaseEnvLocksForSession(ByVal sessionIdentifier As SessionIdentifier)


    ''' <summary>
    ''' <para>
    ''' Checks if the given lock is held. The check is subtly different depending on
    ''' whether the token is provided or not.
    ''' </para><para>
    ''' If the token is provided, it will check if the specified lock is held by that
    ''' token. If the lock exists and is currently held against the specified token,
    ''' this will return true. Otherwise, it will return false.
    ''' </para><para>
    ''' If the given token is null, it will check if the specified lock is held
    ''' <em>at all</em>. So, if the lock exists and is currently held by anyone, this
    ''' will return true. Otherwise it will return false.
    ''' </para>
    ''' </summary>
    ''' <param name="name">The name of the lock to check.</param>
    ''' <param name="token">The token to check the lock against - if this is null
    ''' then this method just checks if the specified lock is held at all.</param>
    ''' <param name="comment">Output parameter which holds the comment held on the
    ''' lock object, regardless of whether the lock is currently held or not.
    ''' If the lock doesn't exist, or no comment exists on the lock record, this
    ''' will be set to null on exit of the method.</param>
    ''' <returns>True if the specified environment lock is held and if the token
    ''' was not provided or, if it was provided, if it matched the token associated
    ''' with the lock record. False in all other cases.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsEnvLockHeld(
                                          ByVal name As String, ByVal token As String, ByRef comment As String) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function HasEnvLockExpired(
                                          name As String, token As String, expiryTime As Integer) As Boolean

    ''' <summary>
    ''' Searches the environment locks using the given filter.
    ''' </summary>
    ''' <param name="flt">The filter to use to search the locks</param>
    ''' <returns>A collection of lock objects matching the given filter.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SearchEnvLocks(ByVal flt As clsLockFilter) As (filteredLocks As ICollection(Of clsLockInfo), totalAmountOfRows As Integer)

    ''' <summary>
    ''' Deletes the lock records with the given names.
    ''' </summary>
    ''' <param name="names">The names of the lock records which should be deleted.
    ''' </param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteLocks(ByVal names As ICollection(Of String))

    ''' <summary>
    ''' Get the effective run mode for the given process. This is the 'most hungry'
    ''' run mode of the process itself combined with all its dependencies.
    ''' </summary>
    ''' <param name="procid">The ID of the main process.</param>
    ''' <param name="sb">Optionally, can be a StringBuilder buffer to receive a
    ''' report to help explain why the run mode is what it is.</param>
    ''' <returns>The run mode</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEffectiveRunMode(procid As Guid, nonVBORunModes As Dictionary(Of String, BusinessObjectRunMode),
                                            Optional ByRef sb As StringBuilder = Nothing) As BusinessObjectRunMode

    ''' <summary>
    ''' Get the external dependencies of a given Process from the database.
    ''' </summary>
    ''' <param name="procid">The ID of the process to get dependencies for.</param>
    ''' <returns>A clsProcessDependecyList containing them all.</returns>
    ''' <remarks></remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetExternalDependencies(procid As Guid) As clsProcessDependencyList

    ''' <summary>
    ''' Updates external dependencies for all processes. Called after upgrade or via
    ''' AutomateC command option.
    ''' </summary>
    ''' <param name="force">Force rebuild regardless of stale flag</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RebuildDependencies(Optional force As Boolean = False)

    ''' <summary>
    ''' Generic function to test whether or not the passed values are referenced by
    ''' any objects/processes.
    ''' </summary>
    ''' <param name="dep">The dependency object to test for</param>
    ''' <returns>True if a reference is found, otherwise false</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsReferenced(dep As clsProcessDependency) As Boolean

    ''' <summary>
    ''' Takes a list of dependency objects to test references for, and removes those
    ''' that are not referenced by objects/processes.
    ''' </summary>
    ''' <param name="deps">List of dependency objects to test</param>
    ''' <returns>List of referenced dependencies</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function FilterUnReferenced(deps As clsProcessDependencyList) As clsProcessDependencyList

    ''' <summary>
    ''' Generic function to return object/process references for the passed value.
    ''' </summary>
    ''' <param name="dep">The dependency object to find</param>
    ''' <param name="hiddenItems">Returns True if the dependency object is
    ''' referenced by objects/process for which the current user cannot access</param>
    ''' <returns>A list containing referencing objects/processes</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetReferences(dep As clsProcessDependency, ByRef hiddenItems As Boolean) As ICollection(Of ProcessInfo)

    ''' <summary>
    ''' Returns a list of processes referenced either directly or indirectly by [processID], which contain
    ''' references to items the current user does not have permission to use.
    ''' </summary>
    ''' <param name="processID">. The Id of the process</param>
    ''' <returns>A list of process which contain references to items this user does not have permission to access.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetInaccessibleReferencesByProcessID(processID As Guid) As ICollection(Of String)

    ''' <summary>
    ''' Returns a list of processes referenced either directly or indirectly by [processName], which contain
    ''' references to items the current user does not have permission to use.
    ''' </summary>
    ''' <param name="processName">The name of the process</param>
    ''' <returns>A list of process which contain references to items this user does not have permission to access.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetInaccessibleReferencesByProcessName(processName As String) As ICollection(Of String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetInaccessibleReferencesByProcessNames(processNames As List(Of String)) As Dictionary(Of String, List(Of String))

    ''' <summary>
    ''' Returns references to a parent's shared application model elements.
    ''' </summary>
    ''' <param name="parentName">The parent object name</param>
    ''' <returns>The list of references</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSharedModelReferences(parentName As String) As clsProcessDependencyList

    ''' <summary>
    ''' Returns parent object names associated with the passed list of objects.
    ''' </summary>
    ''' <param name="objIDs">The list of object IDs to check</param>
    ''' <returns>The associated parents</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetParentReferences(objIDs As List(Of Guid)) As Dictionary(Of Guid, String)

    ''' <summary>
    ''' Returns parent object name for the passed object.
    ''' </summary>
    ''' <param name="id">The object ID to check</param>
    ''' <returns>The parent name</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetParentReference(id As Guid) As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDependenciesStatus() As clsServer.DependencyStates

    ''' <summary>
    ''' Returns a list of stored procedures suitable for use with dashboard tiles.
    ''' If a tile type is not passed then only custom data sources are returned.
    ''' </summary>
    ''' <returns>The list of stored procedures</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ListStoredProcedures(Optional type As TileTypes = Nothing) _
                As Dictionary(Of String, String)

    ''' <summary>
    ''' Returns the list of parameters associated with the passed stored procedure.
    ''' </summary>
    ''' <param name="name">Stored procedure name</param>
    ''' <returns>The list of parameter names</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ListStoredProcedureParameters(name As String) As List(Of String)

    ''' <summary>
    ''' Gets a DataTable containing the data from the specified data source
    ''' </summary>
    ''' <param name="dataSourceName">The name of the data source (the name of a stored
    ''' procedure or an internal data source)</param>
    ''' <param name="params">List of parameters</param>
    ''' <returns>A results data table</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetChartData(dataSourceName As String, params As Dictionary(Of String, String)) As DataTable

    ''' <summary>
    ''' Sends PublishedDashboards data to Data Gateways
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SendPublishedDashboardsToDataGateways()

    ''' <summary>
    ''' Returns the definition of the passed stored procedure.
    ''' </summary>
    ''' <param name="name">The stored procedure name</param>
    ''' <returns>The SQL definition</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDataSourceDefinition(name As String) As String

    ''' <summary>
    ''' Returns true if data sources (stored procedures) can be created using the
    ''' current user's connection, otherwise false.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CanCreateDataSource() As Boolean

    ''' <summary>
    ''' Returns true if the user can grant execute permission to the custom
    ''' datasource BP role (bpa_ExecuteSP_DataSource_custom)
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CanGrantExecuteOnDataSource() As Boolean

    ''' <summary>
    ''' Returns true if the specified data source (stored procedure) can be altered
    ''' using the current user's connection, otherwise false.
    ''' </summary>
    ''' <param name="name">The data source name</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CanAlterDataSource(name As String) As Boolean

    ''' <summary>
    ''' Returns the definition of the passed tile ID
    ''' </summary>
    ''' <param name="id">Tile ID</param>
    ''' <returns>A tile object</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetTileDefinition(id As Guid) As Tile

    ''' <summary>
    ''' Returns the definitions of the passed tile IDs
    ''' </summary>
    ''' <param name="idList">List of tile IDs</param>
    ''' <returns>A list of tile objects</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetTileDefinitions(idList As List(Of Guid)) As List(Of Tile)

    ''' <summary>
    ''' Determines if the passed tile is referenced by 1 or more dashboards
    ''' </summary>
    ''' <param name="id">Tile ID</param>
    ''' <returns>True if tile is in use</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsTileInUse(id As Guid) As Boolean

    ''' <summary>
    ''' Returns the ID of the tile with the passed name
    ''' </summary>
    ''' <param name="name">Tile name</param>
    ''' <returns>Tile ID (or guid.empty if no tile found)</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetTileIDByName(name As String) As Guid

    ''' <summary>
    ''' Returns the name of the tile with the passed id
    ''' </summary>
    ''' <param name="id">Tile id</param>
    ''' <returns>Tile name (or string.empty if no tile found)</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetTileNameByID(id As Guid) As String

    ''' <summary>
    ''' Creates a new tile record in the database, optionally within a group.
    ''' </summary>
    ''' <param name="groupID">Group ID (or Guid.Empty if not in a group)</param>
    ''' <param name="tile">A tile object</param>
    ''' <returns>The newly created tile object with its new ID set within</returns>
    ''' <exception cref="Server.Domain.Models.NameAlreadyExistsException">If a tile with the same name as
    ''' the given tile already exists on the database.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CreateTile(groupID As Guid, tile As Tile,
                                   formattedProperties As String) As Tile



    ''' <summary>
    ''' Copies the given tile.
    ''' </summary>
    ''' <param name="tileId">The ID of the tile to copy</param>
    ''' <returns>The newly created tile</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CopyTile(tileId As Guid) As Tile

    ''' <summary>
    ''' Updates the attributes of the passed tile.
    ''' </summary>
    ''' <param name="tile">A tile object</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateTile(tile As Tile, oldName As String, formattedProperties As String)

    ''' <summary>
    ''' Deletes the passed tile.
    ''' </summary>
    ''' <param name="tileId">The ID of the tile to be deleted</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteTile(tileId As Guid)


    ''' <summary>
    ''' Returns a dashboard by id
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDashboardById(id As Guid) As Dashboard


    ''' <summary>
    ''' Returns the list of dashboard views available to the currently logged in
    ''' user.
    ''' </summary>
    ''' <returns>List of dashboard views</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDashboardList() As List(Of Dashboard)

    ''' <summary>
    ''' Returns the tiles associated with the passed dashboard ID.
    ''' </summary>
    ''' <param name="id">Dashboard ID</param>
    ''' <returns>A list of dashboard tiles</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDashboardTiles(id As Guid) As List(Of DashboardTile)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetWorkQueueCompositions(workQueueIds As IEnumerable(Of Guid)) As IEnumerable(Of WorkQueueComposition)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceUtilization(resourceUtilizationParameters As ResourceUtilizationParameters) As IEnumerable(Of ResourceUtilization)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourcesSummaryUtilization(resourcesSummaryUtilizationParameters As ResourcesSummaryUtilizationParameters) As IEnumerable(Of ResourcesSummaryUtilization)
    
    ''' <summary>
    ''' Creates a new dashboard with the passed details.
    ''' </summary>
    ''' <param name="dash">A dashboard object</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub CreateDashboard(dash As Dashboard)

    ''' <summary>
    ''' Updates the passed dashboard.
    ''' </summary>
    ''' <param name="dash">A dashboard object</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateDashboard(dash As Dashboard)

    ''' <summary>
    ''' Deletes the passed dashboard.
    ''' </summary>
    ''' <param name="dash">Dashboard</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteDashboard(dash As Dashboard)

    ''' <summary>
    ''' Sets the passed dashboard as the user's welcome page
    ''' </summary>
    ''' <param name="dash">The dashboard</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetHomePage(dash As Dashboard)

    ''' <summary>
    ''' Returns the ID of the global dashboard with the passed name
    ''' </summary>
    ''' <param name="name">Global dashboard name</param>
    ''' <returns>Dashboard ID (or guid.empty if not found)</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDashboardIDByName(name As String) As Guid

    ''' <summary>
    ''' Returns the name of the dashboard with the passed id
    ''' </summary>
    ''' <param name="id">Dashboard id</param>
    ''' <returns>Dashboard name (or string.empty if not found)</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDashboardNameByID(id As Guid) As String

    ''' <summary>
    ''' Returns a collection of all credential objects, populated with their ID, name
    ''' description, expiry date and invalid flag.
    ''' </summary>
    ''' <returns>The collection of credential objects</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAllCredentialsInfo() As ICollection(Of clsCredential)

    ''' <summary>
    ''' Returns the credential for a given credential ID, excluding the sensitive data
    ''' logon details, property values etc. (user/process/resource restrictions are
    ''' not taken into account).
    ''' </summary>
    ''' <param name="id">The credential ID</param>
    ''' <returns>The credential object</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential was found on the
    ''' database with the given ID</exception>
    ''' <exception cref="Exception">If any other errors occur while attempting to get
    ''' the credential</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCredentialExcludingLogon(ByVal id As Guid) As clsCredential

    ''' <summary>
    ''' Returns the credential for a given credential ID, including the sensitive data,
    ''' logon details etc.(user/process/resource restrictions are not taken into account).
    ''' </summary>
    ''' <returns>The credential object</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential was found on the
    ''' database with the given ID</exception>
    ''' <exception cref="Exception">If any other errors occur while attempting to get
    ''' the credential</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCredentialIncludingLogon(ByVal id As Guid) As clsCredential

    ''' <summary>
    ''' Returns the credential for a given ID for display in the UI, including the
    ''' username and property names, but excluding the sensitive password and
    ''' property values.
    ''' </summary>
    ''' <param name="id">The credential ID</param>
    ''' <returns>The credential object</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential was found on the
    ''' database with the given ID</exception>
    ''' <exception cref="Exception">If any other errors occur while attempting to get
    ''' the credential</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCredentialForUI(ByVal id As Guid) As clsCredential

    ''' <summary>
    ''' Returns the ID of the credential for a given name (user/process/resource
    ''' restrictions are not taken into account).
    ''' </summary>
    ''' <param name="name">The credential name</param>
    ''' <returns>The credential ID, or <see cref="Guid.Empty"/> if no credential with
    ''' the given name was found</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCredentialID(ByVal name As String) As Guid

    ''' <summary>
    ''' Updates a credential, and properties.
    ''' </summary>
    ''' <param name="credential">The credential object</param>
    ''' <param name="oldName">The previous name (or the current name it has not been
    ''' changed)</param>
    ''' <param name="properties">List of properties to update. </param>
    ''' <exception cref="Server.Domain.Models.NameAlreadyExistsException">If the chosen new name is
    ''' already in use on the database by a different credential</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateCredential(
     credential As clsCredential,
     oldName As String,
     properties As ICollection(Of CredentialProperty),
     passwordChanged As Boolean)

    ''' <summary>
    ''' Creates a new credential
    ''' </summary>
    ''' <param name="credential">The credential object</param>
    ''' <returns>The new credential ID</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CreateCredential(ByVal credential As clsCredential) As Guid

    ''' <summary>
    ''' Deletes a list of credentials
    ''' </summary>
    ''' <param name="credentials">The list of credentials</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteCredentials(ByVal credentials As IEnumerable(Of clsCredential))

    ''' <summary>
    ''' Requests a single credential given the credential name and session ID, and
    ''' using the role of the current logged in user.
    ''' </summary>
    ''' <param name="sessId">The current session ID</param>
    ''' <param name="name">The name of the credential to get</param>
    ''' <returns>The credential associated with the given name</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' name is found on the database</exception>
    ''' <exception cref="PermissionException">If the current caller context does not
    ''' have access rights to the specified credential</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function RequestCredential(
                                          ByVal sessId As Guid, ByVal name As String) As clsCredential







    ''' <summary>
    ''' Requests a single data gateways credential for use in a Data Gateway process.
    ''' </summary>
    ''' <param name="name">Credential name</param>
    ''' <returns></returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function RequestCredentialForDataGatewayProcess(name As String) As clsCredential


    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDataGatewayCredentials() As List(Of String)

    ''' <summary>
    ''' Requests that the named credential is reset. The credential be accessible to
    ''' the user/process/resource.
    ''' </summary>
    ''' <param name="sessId">The current session ID</param>
    ''' <param name="name">The credential name</param>
    ''' <param name="username">The username</param>
    ''' <param name="password">The password</param>
    ''' <param name="expirydate">The expiry date</param>
    ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' <paramref name="name">name</paramref> was found in the database.</exception>
    ''' <exception cref="PermissionException">If the current calling context does not
    ''' have access rights to the specified credential</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RequestCredentialSet(ByVal sessId As Guid, ByVal name As String,
                                        ByVal username As String, ByVal password As SafeString, ByVal expirydate As Date)

    ''' <summary>
    ''' Request specified credential is invalidated. The credential be accessible to
    ''' the user/process/resource.
    ''' </summary>
    ''' <param name="sessId">The current session ID</param>
    ''' <param name="name">The credential name</param>
    ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' <paramref name="name">name</paramref> was found in the database.</exception>
    ''' <exception cref="PermissionException">If the current calling context does not
    ''' have access rights to the specified credential</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RequestCredentialInvalidated(ByVal sessId As Guid, ByVal name As String)

    ''' <summary>
    ''' Request the value of a property for a specific credential. The credential
    ''' be accessible to the user/process/resource.
    ''' </summary>
    ''' <param name="sessId">The current session ID</param>
    ''' <param name="name">The credential name</param>
    ''' <param name="propName">The property name</param>
    ''' <returns>The property value</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' <paramref name="name">name</paramref> was found in the database.</exception>
    ''' <exception cref="PermissionException">If the current calling context does not
    ''' have access rights to the specified credential</exception>
    ''' <exception cref="NoSuchElementException">If the credential did not have a
    ''' property with the name specified by <paramref name="propName"/></exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function RequestCredentialProperty(ByVal sessId As Guid,
            ByVal name As String, ByVal propName As String) As SafeString

    ''' <summary>
    ''' Request that the value of a specified credential property is set to the given
    ''' value. The credential must be accessible to the user/process/resource.
    ''' If the specified credential does not have such a property, the property will
    ''' be created before being set at the required value.
    ''' </summary>
    ''' <param name="sessId">The current session ID</param>
    ''' <param name="name">The credential name</param>
    ''' <param name="propName">The property name</param>
    ''' <param name="propValue">The property value</param>
    ''' ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' <paramref name="name">name</paramref> was found in the database.</exception>
    ''' <exception cref="PermissionException">If the current calling context does not
    ''' have access rights to the specified credential</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RequestSetCredentialProperty(
            ByVal sessId As Guid,
            ByVal name As String, ByVal propName As String,
            ByVal propValue As SafeString)

    ''' <summary>
    ''' Request that the value of a specified credential property is set to the given
    ''' value. If the specified credential does not have such a property, the property will
    ''' be created before being set at the required value.
    ''' </summary>
    ''' <param name="name">The credential name</param>
    ''' <param name="propName">The property name</param>
    ''' <param name="propValue">The property value</param>
    ''' ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' <paramref name="name">name</paramref> was found in the database.</exception>
    ''' <exception cref="PermissionException">If the logged in user doesn't have the
    ''' correct permissions to call this method</exception>
    <OperationContract(Name:="RequestSetCredentialPropertyCommandLine"), FaultContract(GetType(BPServerFault))>
    Sub RequestSetCredentialProperty(
            ByVal name As String, ByVal propName As String,
            ByVal propValue As SafeString)

    ''' <summary>
    ''' Request a list of credentials matching the passed status. Note that the
    ''' actual login details (username/password) are not returned and that user/
    ''' process/resource restrictions are not taken into account.
    ''' </summary>
    ''' <param name="sessId">The ID of the session which is requesting access to the
    ''' credential.</param>
    ''' <param name="status">The status criteria</param>
    ''' <returns>The collection of credentials matching the passed
    ''' status criteria</returns>
    ''' <remarks>This method is used by clsCredentialsBusinessObject</remarks>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function RequestCredentialsList(ByVal sessId As Guid,
                                               ByVal status As clsCredential.Status) As ICollection(Of clsCredential)

    ''' <summary>
    ''' Encrypts the given text using the encrypter with the specified name.
    ''' </summary>
    ''' <param name="encName">The name of the encrypter, as registered in this
    ''' server, to use to encrypt the text. Note that null indicates that no
    ''' encrypter should be used - ie. that the text should be returned as is.
    ''' </param>
    ''' <param name="text">The text to encrypt</param>
    ''' <returns>The text, encrypted and, if necessary, encoded into a string that
    ''' the encrypter / decrypter can recognise.</returns>
    <OperationContract(Name:="EncryptByName"),
     FaultContract(GetType(BPServerFault))>
    Function Encrypt(ByVal encName As String, ByVal text As String) As String

    ''' <summary>
    ''' Decrypts the given text using the specified decrypter.
    ''' </summary>
    ''' <param name="decName">The name of the decrypter to use. Note that a value of
    ''' null indicates that no decrypter should be used and the given text will be
    ''' returned as is.</param>
    ''' <param name="text">The encrypted text to decrypt</param>
    ''' <returns>The plain text equivalent of the given encrypted text.</returns>
    ''' <exception cref="NoSuchEncrypterException">If no decrypter with the given
    ''' name is registered in this server.</exception>
    ''' <exception cref="InvalidEncryptedDataException">If the given encrypted text
    ''' is not in a valid format for the specified decrypter</exception>
    <OperationContract(Name:="DecryptByName"),
     FaultContract(GetType(BPServerFault))>
    Function Decrypt(ByVal decName As String, ByVal text As String) As String

    ''' <summary>
    ''' Checks if this clsServer instance has a credential key available, either
    ''' from the server config or from the database.
    ''' </summary>
    ''' <returns>True if a credentials master key is available for use in this
    ''' server; False otherwise.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function HasCredentialKey() As Boolean

    ''' <summary>
    ''' Record a user-related audit event.
    ''' </summary>
    ''' <param name="eventCode">The event code</param>
    ''' <param name="userId">The target user id</param>
    ''' <param name="comments">The event comments</param>
    ''' <param name="oldUserName">The old username, if relevant</param>
    ''' <param name="externalIdMapping">The user's mapped external identity, where relevant</param>
    ''' <param name="oldExternalIdMapping">The user's old mapped external identity, where relevant</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AuditRecordUserEvent(ByVal eventCode As UserEventCode,
                                  ByVal userId As Guid,
                                  ByVal comments As String,
                                  Optional ByVal oldUserName As String = "",
                                  Optional ByVal externalIdMapping As ExternalIdentityMapping = Nothing,
                                  Optional ByVal oldExternalIdMapping As ExternalIdentityMapping = Nothing)

    ''' <summary>
    ''' Record a process-related audit event.
    ''' </summary>
    ''' <param name="eventCode">A string classifying the type of event.</param>
    ''' <param name="targetProcessID">The guid of the process on which the event acted.
    ''' </param>
    ''' <param name="comments">Comments to supplement the sNarrative parameter
    ''' </param>
    ''' <param name="newXML">The latest version of the process xml.</param>
    ''' <param name="summary">A comment summarising the changes made to the process
    ''' </param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AuditRecordProcessEvent(eventCode As ProcessEventCode,
                                targetProcessID As Guid,
                                comments As String,
                                newXML As String,
                                summary As String)

    ''' <summary>
    ''' Record a business object-related event.
    ''' </summary>
    ''' <param name="eventCode">A string classifying the type of event.</param>
    ''' <param name="targetProcessID">The guid of the process on which the event
    ''' acted.</param>
    ''' <param name="comments">Comments to supplement the summary</param>
    ''' <param name="newXML">The latest version of the xml.</param>
    ''' <param name="summary">A comment summarising the changes made to the bo.
    ''' </param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AuditRecordBusinessObjectEvent(eventCode As BusinessObjectEventCode,
                                       targetProcessID As Guid,
                                       comments As String,
                                       newXML As String,
                                       summary As String)

    ''' <summary>
    ''' Records an audit event for a process or VBO, defined by the given properties.
    ''' This will ensure that the appropriate audit event code from
    ''' <see cref="ProcessOrVboEventCode"/> is
    ''' used in the audit record, along with the appropriate narrative describing
    ''' the audited event.
    ''' </summary>
    ''' <param name="eventCode">The event code detailing the operation which has
    ''' occurred on the process / vbo</param>
    ''' <param name="isVBO">True to indicate that a VBO is being audited here;
    ''' False to indicate a process.</param>
    ''' <param name="processId">The ID of the process / VBO.</param>
    ''' <param name="comments">Comments regarding the auditable event.</param>
    ''' <param name="newXML">The new XML from the process / VBO</param>
    ''' <param name="summary">A summary of the auditable event.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AuditRecordProcessOrVboEvent(eventCode As ProcessOrVboEventCode,
                                     isVBO As Boolean,
                                     processId As Guid,
                                     comments As String,
                                     newXML As String,
                                     summary As String)

    ''' <summary>
    ''' Record a sysconfig-related event.
    ''' </summary>
    ''' <param name="eventCode">The event code</param>
    ''' <param name="comments">The event comments</param>
    ''' <param name="args">Other event specific strings</param>
    ''' <returns>True if successful, otherwise false</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function AuditRecordSysConfigEvent(ByVal eventCode As SysConfEventCode, ByVal comments As String, ParamArray args() As String) As Boolean

    ''' <summary>
    ''' Record a workqueue-related event.
    ''' </summary>
    ''' <param name="eventCode">The code of the event.</param>
    ''' <param name="queueName">The name of the queue, for events where the name
    ''' cannot be looked up based on its ID (eg rename, delete queue).</param>
    ''' <param name="queueID">The ID of the queue affected.</param>
    ''' <param name="comments">Any further comments about the event. Leave blank
    ''' if desired.</param>
    ''' <returns>True if successful, otherwise false</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function AuditRecordWorkQueueEvent(
                                                      ByVal eventCode As WorkQueueEventCode,
                                                      ByVal queueID As Guid,
                                                      ByVal queueName As String,
                                                      ByVal comments As String) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetAuditLocale(locale As String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEncryptionSchemeByName(ByVal name As String) As clsEncryptionScheme

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEncryptionSchemeExcludingKey(name As String) As clsEncryptionScheme

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function HasEncryptionScheme(name As String) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CheckSchemeForFIPSCompliance(name As String) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEncryptionSchemesExcludingKey() As ICollection(Of clsEncryptionScheme)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEncryptionSchemes() As ICollection(Of clsEncryptionScheme)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetInvalidEncryptionSchemeNames() As List(Of String)

    ''' <summary>
    ''' Checks all encryption schemes in the cached key store to see if they are FIPS compliant
    ''' when the FIPS GPO is enabled
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If there are any non FIPS compliant encryption schemes</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function DBEncryptionSchemesAreFipsCompliant() As List(Of String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetConfigEncryptMethod() As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetConfigEncryptMethod(ByVal thumbprint As String, ByVal forceConfigEncryptParam As Boolean)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCallbackConnectionConfig() As BluePrism.ClientServerResources.Core.Config.ConnectionConfig

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function StoreEncryptionScheme(ByVal scheme As clsEncryptionScheme) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteEncryptionScheme(ByVal scheme As clsEncryptionScheme)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function EncryptionSchemeInUse(ByVal id As Integer) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDefaultEncrypter() As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetDefaultEncrypter(ByVal encryptid As Integer, ByVal name As String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ReEncryptCredentials() As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ReEncryptQueueItems(batchSize As Integer) As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAlgorithmName(ByVal scheme As clsEncryptionScheme) As String

    ''' <summary>
    ''' Log in as an Anonymous Resource.
    ''' </summary>
    ''' <returns>A LoginResult describing the result</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function LoginAsAnonResource(machineName As String) As LoginResult

    ''' <summary>
    ''' Initialises the permissions in the current environment.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetPermissionData() As PermissionData

    ''' <summary>
    ''' Get the Blue Prism version in use.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetBluePrismVersion() As String

    ''' <summary>
    ''' Ensures that the connection to the database is valid
    ''' </summary>
    ''' <exception cref="UnavailableException">If there is no valid connection from
    ''' the server to the database.</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub EnsureDatabaseConnection()

    ''' <summary>
    ''' Updates the Scheduler configuration settings.
    ''' </summary>
    ''' <param name="active">Indicates whether the Scheduler is active or not</param>
    ''' <param name="checkSeconds">The number of seconds to check for missed schedules</param>
    ''' <param name="retryTimes">The number of seconds to wait between retries</param>
    ''' <param name="retryPeriod">The number of times to retry</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetSchedulerConfig(active As Boolean, checkSeconds As Integer, retryTimes As Integer, retryPeriod As Integer)

    ''' <summary>
    ''' Retrieve the audit history for the given license
    ''' </summary>
    ''' <param name="licenseID">The Id of the license</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAuditLogDataForLicense(licenseID As Integer) As IEnumerable(Of LicenseActivationEvent)

    'GetActivationRequestsForLicense
    ''' <summary>
    ''' Retrieve the requests for the given license
    ''' </summary>
    ''' <param name="licenseID">The Id of the license</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetActivationRequestsForLicense(licenseID As Integer) As IEnumerable(Of String)



    ''' <summary>
    ''' Enables/disables access to help offline.
    ''' </summary>
    ''' <param name="enable">True to enable offline help, or False to disable it</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateEnableOfflineHelp(enable As Boolean)

    ''' <summary>
    ''' Returns whether offline help has been enabled or not.
    ''' </summary>
    ''' <returns>True if offline help is enabled, otherwise false</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function OfflineHelpEnabled() As Boolean

    ''' <summary>
    ''' Updates the base URL used to host offline help documentation.
    ''' </summary>
    ''' <param name="baseUrl">The base URL that contains the offline help documentation</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateOfflineHelpBaseUrl(baseUrl As String)

    ''' <summary>
    ''' Returns the base URL used to host offline help documentation.
    ''' </summary>
    ''' <returns>The base URL for offline help documentation</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetOfflineHelpBaseUrl() As String

    ''' <summary>
    ''' Updates offline help data.
    ''' </summary>
    ''' <param name="enable">True to enable offline help, or False to disable it</param>
    ''' <param name="baseUrl">The base URL that contains the offline help documentation</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateOfflineHelpData(enable As Boolean, baseUrl As String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateHideDigitalExchangeSetting(value As Boolean)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetHideDigitalExchangeSetting(defaultValue As Boolean) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateEnableBpaEnvironmentDataSetting(value As Boolean)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEnableBpaEnvironmentDataSetting(defaultValue As Boolean) As Boolean

#Region " Secured Methods "

    ''' <summary>
    ''' Returns the sign-on related configuration settings for this environment.
    ''' </summary>
    ''' <param name="rules">Carries back the password rules</param>
    ''' <param name="options">Carries back the logon options</param>
    <OperationContract(Name:="GetSignonSettingsBPAuth"), FaultContract(GetType(BPServerFault))>
    Sub GetSignonSettings(ByRef rules As PasswordRules, ByRef options As LogonOptions)

    ''' <summary>
    ''' Updates the sign-on related configuration settings for this environment.
    ''' </summary>
    ''' <param name="rules">Password rules</param>
    ''' <param name="options">Logon options</param>
    <OperationContract(Name:="SetSignonSettingsBPAuth"), FaultContract(GetType(BPServerFault))>
    Sub SetSignonSettings(rules As PasswordRules, options As LogonOptions)

    ''' <summary>
    ''' Returns the single sign-on related configuration for this environment.
    ''' </summary>
    ''' <param name="domain">Active Directory Domain</param>
    ''' <param name="adminGroup">System Administrator Security Group</param>
    <OperationContract(Name:="GetSignonSettingsAD"), FaultContract(GetType(BPServerFault))>
    Sub GetSignonSettings(ByRef domain As String, ByRef adminGroup As String)

    ''' <summary>
    ''' Updates the single sign-on related configuration for this environment.
    ''' </summary>
    ''' <param name="domain">Active Directory Domain</param>
    ''' <param name="domainChanged">Set to True to indicate that the domain is being
    ''' changed</param>
    ''' <param name="groupSID">System Administrator Security Group SID</param>
    ''' <param name="groupName">System Administrator Security Group name</param>
    ''' <param name="groupPath">System Administrator Security Group path</param>
    <OperationContract(Name:="SetSignonSettingsAD"), FaultContract(GetType(BPServerFault))>
    Sub SetSignonSettings(domain As String, domainChanged As Boolean, groupSID As String, groupName As String, groupPath As String)

    ''' <summary>
    ''' Return the logon options for this environment.
    ''' </summary>
    ''' <param name="userList">List of users (if available)</param>
    ''' <returns>The logon options</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetLogonOptions(ByRef userList As ICollection(Of User)) As LogonOptions

    ''' <summary>
    ''' Checks that the given user password and confirmation both match and satisfy
    ''' the configured requirements.
    ''' </summary>
    ''' <param name="password">The password</param>
    ''' <param name="confirmation">Confirmation of the password</param>
    ''' <returns>True if the password satisfies the requirements, otherwise False
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsValidPassword(password As SafeString,
                             confirmation As SafeString) As Boolean

    ''' <summary>
    ''' Gets the authentication domain to be used with single sign-on
    ''' </summary>
    ''' <returns>The name of the domain stored in the database</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetActiveDirectoryDomain() As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function DatabaseType() As DatabaseType

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ValidateActiveDirectoryGroups(groups As ICollection(Of String), ByRef reason As String) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function FindActiveDirectoryUsers(searchRoot As String, filterType As UserFilterType, filter As String, queryPageOptions As QueryPageOptions, credentials As DirectorySearcherCredentials) As PaginatedUserQueryResult

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDistinguishedNameOfCurrentForest() As String

    ''' <summary>
    ''' Synchronise users from Active Directory to the Blue Prism database - i.e.
    ''' users who would be authorised to use Blue Prism according to their membership
    ''' in Domain Groups, but haven't logged in to Blue Prism yet, get records
    ''' created in the Blue Prism database.
    ''' </summary>
    ''' <returns>A summary of what's been done, to be displayed to the user, or
    ''' Nothing if no response is necessary.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function RefreshADUserList() As Auth.RefreshADUserList

    ''' <summary>
    ''' Creates a new user on the database.
    ''' </summary>
    ''' <param name="user">The populated user object with the data to create the user
    ''' record with on the database.</param>
    ''' <param name="password">The password to set on the database.</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub CreateNewUser(user As User, password As SafeString)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CreateNewAuthenticationServerUserWithUniqueName(username As String, id As Guid) As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CreateNewServiceAccount(clientName As String, clientId As String, hasBluePrismApiScope As Boolean) As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateServiceAccount(clientId As String, clientName As String, hasBluePrismApiScope As Boolean, synchronizationDate As DateTimeOffset)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CreateNewMappedActiveDirectoryUsers(users As List(Of User), batchSize As Integer) As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub CreateActiveDirectoryMapping(user As User)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSidForActiveDirectoryUser(user As User) As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub CreateNewMappedActiveDirectoryUser(user As User)

    ''' <summary>
    ''' Updates the basic data found on the given user.
    ''' </summary>
    ''' <param name="user">The user to update</param>
    ''' <param name="password">The password to set on the user. Null indicates that
    ''' the password should not be changed.</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="user"/> is null.
    ''' </exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateUser(user As User, password As SafeString)

    ''' <summary>
    ''' Updates the basic data found on the given external user.
    ''' </summary>
    ''' <param name="user">The user to update</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="user"/> is null.
    ''' </exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateExternalUser(user As User)

    ''' <summary>
    ''' Updates the basic data found on the given Authentication Server user.
    ''' </summary>
    ''' <param name="user">The user to update</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="user"/> is null.
    ''' </exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateAuthenticationServerUser(user As User)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateAdSSOUserToMappedAdUser(user As User)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ConvertDatabaseFromAdSsoToMappedAd(nativeAdminUser As NativeAdminUserModel, ByRef conversionResultMessage As String) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateMappedActiveDirectoryUser(user As User)

    ''' <summary>
    ''' Flags a user as deleted, note that this does not actually delete the user, as
    ''' then it causes problems with audit and logging.
    ''' </summary>
    ''' <param name="userId">The guid of the user to delete</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteUser(userId As Guid)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RetireAuthenticationServerUser(authServerId As Guid, synchronizationDate As DateTimeOffset)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UnretireAuthenticationServerUser(authServerId As Guid, synchronizationDate As DateTimeOffset)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteServiceAccount(clientId As String, synchronizationDate As DateTimeOffset)

    ''' <summary>
    ''' Resets the number of login attempts for the specified user
    ''' </summary>
    ''' <param name="userId">The ID of the user to reset</param>
    ''' <returns>True if the user had their login attempts reset; False if it was
    ''' already zero (or the user ID did not exist on the database)</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function UnlockUser(userId As Guid) As Boolean

    ''' <summary>
    ''' Updates the password of the given user.
    ''' </summary>
    ''' <param name="userName">The users name</param>
    ''' <param name="currentPassword">The users existing password</param>
    ''' <param name="newPassword">The users new password</param>
    ''' <param name="confirmation">A confirmation of the users new password</param>
    ''' <exception cref="InvalidPasswordException">If the given password is the same
    ''' as the previous password</exception>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdatePassword(userName As String, currentPassword As SafeString,
                       newPassword As SafeString, confirmation As SafeString)

    ''' <summary>
    ''' Updates given user's password expiry date to be 4 weeks further on than
    ''' the current expiry date or today if the user has no current expiry date.
    ''' </summary>
    ''' <param name="userName">The users name</param>
    ''' <param name="password">The users existing password</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdatePasswordExpiryDate(userName As String, password As SafeString)

    ''' <summary>
    ''' Gets the system roles applicable to the configuration of the system and adds them to
    ''' a roleset object, ignoring all roles which require features that are currently
    ''' disabled.
    ''' </summary>
    ''' <returns>A roleset containing the roles defined in the current environment.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetRoles() As RoleSet

    ''' <summary>
    ''' Updates the roles in the system to the given roleset.
    ''' After completing this method, any roles with temporary IDs in the given role
    ''' set will have their IDs set to the values assigned to them by the database;
    ''' this is also the returned value, meaning that the new IDs can be retrieved
    ''' over a BP Server connection.
    ''' </summary>
    ''' <param name="rs">The set of roles to update the system roles to.</param>
    ''' <returns>The roleset after updating, with any temporary IDs replaced by the
    ''' IDs assigned to the roles by the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function UpdateRoles(rs As RoleSet) As RoleSet

    ''' <summary>
    ''' Gets the <em>active</em> user objects representing users who are assigned to
    ''' a specified role (excluding deleted users)
    ''' </summary>
    ''' <param name="roleId">The ID of the role for which the assigned users are
    ''' required.</param>
    ''' <returns>A collection of users <em>with no role information stored</em> which
    ''' represents the active users assigned to the specified role.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetActiveUsersInRole(roleId As Integer) As ICollection(Of User)

    ''' <summary>
    ''' Counts the number of users who have a particular role assigned to them on
    ''' the database (excluding deleted users)
    ''' </summary>
    ''' <param name="r">The role to count the number of users assigned with it.
    ''' </param>
    ''' <returns>The number of users found on the database with the specified role.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CountActiveUsersWithRole(ByVal r As Role) As Integer

    ''' <summary>
    ''' Removes any keep alive timestamps associated with the current user
    ''' on the local machine.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ClearKeepAliveTimeStamp(Optional useAutomateCAliveTable As Boolean = False)

    ''' <summary>
    ''' Places a timestamp in the BPAAliveResources table together
    ''' with the guid of the currently logged in user (if there is one).
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetKeepAliveTimeStamp(Optional useAutomateCAliveTable As Boolean = False)

    ''' <summary>
    ''' Log in. This is the Windows Authentication version.
    ''' </summary>
    ''' <param name="machine">The name of the machine the login is from</param>
    ''' <param name="locale">The client locale</param>
    ''' <param name="userID">If login is successful, this contains the ID of the
    ''' logged-in user.</param>
    ''' <returns>A LoginResult describing the result</returns>
    ''' <remarks>Clients should not call this directly - they should always use the
    ''' corresponding method in clsUser, which allows some things to be cached
    ''' locally on the client.</remarks>
    <OperationContract(Name:="LoginAD"), FaultContract(GetType(BPServerFault))>
    Function Login(machine As String, locale As String, ByRef userID As Guid) As LoginResult

    <OperationContract(Name:="LoginExternal"), FaultContract(GetType(BPServerFault))>
    Function Login(machine As String, locale As String, accessToken As String, Optional reloginTokenRequest As ReloginTokenRequest = Nothing) As LoginResultWithReloginToken

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function LoginWithMappedActiveDirectoryUser(machine As String, locale As String) As LoginResult

    <OperationContract(Name:="ReloginExternal"), FaultContract(GetType(BPServerFault))>
    Function LoginWithReloginToken(locale As String, reloginTokenRequest As ReloginTokenRequest) As LoginResultWithReloginToken

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CurrentWindowsUserIsMappedToABluePrismUser() As Boolean

    ''' <summary>
    ''' Validate user credentials. Used only by the listener for validating
    ''' credentials passed across the network.
    ''' </summary>
    ''' <param name="username">The user name</param>
    ''' <param name="password">The password</param>
    ''' <returns>The validated user object, or Nothing if the credentials
    ''' were not valid.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ValidateCredentials(username As String, password As SafeString, authenticationMode As AuthMode) As IUser

    ''' <summary>
    ''' Log in. The is the Blue Prism Authentication version.
    ''' </summary>
    ''' <param name="username">The username</param>
    ''' <param name="password">The password</param>
    ''' <param name="machine">The name of the machine the login is from</param>
    ''' <param name="locale">The client locale</param>
    ''' <returns>A LoginResult describing the result</returns>
    ''' <remarks>Clients should not call this directly - they should always use the
    ''' corresponding method in clsUser, which allows some things to be cached
    ''' locally on the client.</remarks>
    <OperationContract(Name:="LoginBP"), FaultContract(GetType(BPServerFault))>
    Function Login(username As String, password As SafeString,
                   machine As String, locale As String) As LoginResult

    ''' <summary>
    ''' Updates the login date/timestamp for this user, and returns both the current
    ''' and previous login dates/times.
    ''' </summary>
    ''' <param name="currLogin">The current login date/time</param>
    ''' <param name="prevLogin">The previous login date/time</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateLoginTimestamp(ByRef currLogin As Date, ByRef prevLogin As Date)

    ''' <summary>
    ''' Log out.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub Logout()

    ''' <summary>
    ''' Reads the preference of the currently logged-in user from the BPAUser table
    ''' as to whether they like to use edit summaries.
    ''' </summary>
    ''' <param name="preference">A boolean to contain the user's preference.</param>
    ''' <param name="sErr">A message continaing any SQL error</param>
    ''' <returns>Returns True on success.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub GetUserEditSummariesPreference(ByRef preference As Boolean)

    ''' <summary>
    ''' Sets the preference of the currently logged-in user to the
    ''' BPAUser table as to whether they like to use edit summaries.
    ''' </summary>
    ''' <param name="preference">A boolean containing the user's preference.</param>
    ''' <param name="sErr">A message continaing any SQL error</param>
    ''' <returns>Returns true on success.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetUserEditSummariesPreference(ByVal preference As Boolean)

    ''' <summary>
    ''' Gets the user from the database with the given ID.
    ''' </summary>
    ''' <param name="userId">The ID of the required user.</param>
    ''' <returns>The User object corresponding to the given ID.</returns>
    ''' <exception cref="ArgumentNullException">If <paramref name="userId"/> is
    ''' empty - ie. <see cref="Guid.Empty"/>.</exception>
    ''' <exception cref="NoSuchElementException">If no user with the given user ID
    ''' was found on the database.</exception>
    <OperationContract(Name:="GetUserByID"), FaultContract(GetType(BPServerFault))>
    Function GetUser(ByVal userId As Guid) As User

    ''' <summary>
    ''' Gets the user from the database with the given username.
    ''' </summary>
    ''' <param name="username">The username of the required user.</param>
    ''' <returns>The User object corresponding to the given username.</returns>
    ''' <exception cref="ArgumentNullException">If <paramref name="username"/> is
    ''' null or empty.</exception>
    ''' <exception cref="NoSuchElementException">If no user with the given username
    ''' was found on the database.</exception>
    <OperationContract(Name:="GetUserByName"), FaultContract(GetType(BPServerFault))>
    Function GetUser(ByVal username As String) As User

    ''' <summary>
    ''' Gets all the users from the database
    ''' </summary>
    ''' <returns>A collection of all users from the database</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAllUsers(Optional populateRoles As Boolean = True) As ICollection(Of User)

    ''' <summary>
    ''' Gets all the user names currently stored in the database whether they are
    ''' login users or system users.
    ''' </summary>
    ''' <returns>Gets names of all users held on the system, with system user names
    ''' being surrounded by square brackets.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAllUserNames() As ICollection(Of String)

    ''' <summary>
    ''' Gets all the names of login users currently stored in the database.
    ''' </summary>
    ''' <returns>A collection of all login user names held on the system.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetLoginUserNames() As ICollection(Of String)

    ''' <summary>
    ''' Gets all the names of system users currently stored in the database, with
    ''' each name surrounded by square brackets.
    ''' </summary>
    ''' <returns>The system user names in the database.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSystemUserNames() As ICollection(Of String)

    ''' <summary>
    ''' Gets the username for a given user id, or an empty string if the user could
    ''' not be found or is the Scheduler user.
    ''' </summary>
    ''' <param name="userId">the user id</param>
    ''' <returns>The user name or a blank string if the user could not be found or
    ''' the function failed for any other reason</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetUserName(ByVal userId As Guid) As String

    ''' <summary>
    ''' Try to get the ID of a user, given their name.
    ''' </summary>
    ''' <param name="sUserName">The name of the user</param>
    ''' <returns>The users ID (GUID) or Guid.Empty if the user does
    ''' not exist or the operation failed for any other reason.
    ''' </returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function TryGetUserID(ByVal sUserName As String) As Guid

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function TryGetUserAttrib(ByVal userId As Guid) As AuthMode

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetLoggedInUsersAndMachines() As List(Of (id As Guid, userName As String, machineName As String, transient As Boolean))

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetRunningSessions() As ICollection(Of clsProcessSession)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function RefreshReloginToken(reloginTokenRequest As ReloginTokenRequest) As SafeString

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteReloginToken(machineName As String, processId As Integer)

    ''' <summary>
    ''' Inserts an authorisation token into the database for the passed user.
    ''' </summary>
    ''' <returns>Returns the token on success.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function RegisterAuthorisationToken() As clsAuthToken

    ''' <summary>
    ''' Inserts an authorisation token into the database which expires in a given time-fame
    ''' </summary>
    ''' <param name="validityDuration">Time in seconds to expire</param>
    ''' <returns></returns>
    <OperationContract(Name:="RegisterAuthorisationTokenWithExpiryTime"), FaultContract(GetType(BPServerFault))>
    Function RegisterAuthorisationTokenWithExpiryTime(validityDuration As Integer) As clsAuthToken

    ''' <summary>
    ''' Inserts an authorisation token into the database for the passed user.
    ''' </summary>
    ''' <returns>Returns the token on success.</returns>
    <OperationContract(Name:="RegisterAuthorisationTokenUserPass"), FaultContract(GetType(BPServerFault))>
    Function RegisterAuthorisationToken(username As String, password As SafeString, processID As Guid, authMode As AuthMode) As clsAuthToken

    <OperationContract(Name:="RegisterAuthorisationTokenForProcess"), FaultContract(GetType(BPServerFault))>
    Function RegisterAuthorisationToken(processId As Guid) As clsAuthToken

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ValidateWebServiceRequest(username As String, password As SafeString, processId As Guid) As IUser
    ''' <summary>
    ''' Validates a supplied authorisation token. On validation, the token is
    ''' immediately expired - tokens are single-use only.
    ''' </summary>
    ''' <param name="reasonInvalid">When the token is invalid (False returned),
    ''' carries back an explanation as to why the token is not valid (e.g. expired,
    ''' or belongs to different user).</param>
    ''' <returns>Returns a user object if the token is valid, or Nothing if it is not
    ''' valid and should be rejected.</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ValidateAuthorisationToken(authToken As clsAuthToken, ByRef reasonInvalid As String) As User

    ''' <summary>
    ''' Updates an exception screenshot for a given resource.
    ''' </summary>
    ''' <param name="details">The details of the screenshot</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateExceptionScreenshot(details As clsScreenshotDetails)

    ''' <summary>
    ''' Checks whether a screenshot is available for the given resource
    ''' </summary>
    ''' <param name="resourceId">The id of the resource</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CheckExceptionScreenshotAvailable(resourceId As Guid) As Boolean

    ''' <summary>
    ''' Gets the latest exception screenshot for a given resource.
    ''' </summary>
    ''' <param name="resourceId">The id of the resource</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetExceptionScreenshot(resourceId As Guid) As clsScreenshotDetails

    ''' <summary>
    ''' Sets the system preference for whether resource screenshots are allowed
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetAllowResourceScreenshot(value As Boolean)

    ''' <summary>
    ''' Gets the system preference for whether resource screenshots are allowed
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAllowResourceScreenshot() As Boolean

    ''' <summary>
    ''' Re-encrypts the resource screen captures using the currently configured
    ''' encryption scheme
    ''' </summary>
    ''' <returns>Number of records updated</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ReEncryptExceptionScreenshots() As Integer

    ''' <summary>
    ''' Sets the system preference for when stages should be deemed as overdue and
    ''' show as 'warning'
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetStageWarningThreshold(seconds As Integer)

    ''' <summary>
    ''' Gets the system preference for when stages should be deemed as overdue and
    ''' show as 'warning'
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetStageWarningThreshold() As Integer


    ''' <summary>
    ''' Gets the system preference for when window to warn when certificates expire
    ''' </summary>
    ''' <returns></returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCertificateExpThreshold() As Integer


    ''' <summary>
    ''' Indicates whether or not the current user has permission to control the
    ''' specified active queue
    ''' </summary>
    ''' <param name="ident">The Ident of the active queue to check</param>
    ''' <returns>True if the user has permission to control the queue</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsActiveQueueControllable(ident As Integer) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetControllableActiveQueueIds() As ICollection(Of Integer)

    ''' <summary>
    ''' Gets the system settings for web connections
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetWebConnectionSettings() As WebConnectionSettings

    ''' <summary>
    ''' Updates the system settings for web connections
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateWebConnectionSettings(settings As WebConnectionSettings)

#End Region

#Region "Segregated Permissions"

    ''' <summary>
    ''' Gets the group level permission restriction states for the specified groups
    ''' </summary>
    ''' <param name="groupIDs">The group ids of the groups</param>
    ''' <returns></returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetGroupPermissionStates(groupIDs As List(Of Guid)) _
        As Dictionary(Of Guid, PermissionState)

    ''' <summary>
    ''' Gets the group level permissions for the passed group, including whether
    ''' or not group level restrictions apply.
    ''' </summary>
    ''' <param name="groupID">The ID of the group</param>
    ''' <returns>>A GroupPermissions object containing the available and selected
    ''' permissions for the given group</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetActualGroupPermissions(groupID As Guid) As IGroupPermissions

    ''' <summary>
    ''' Update the group level permissions for the passed group, including whether
    ''' or not group level restrictions apply for the group.
    ''' </summary>
    ''' <param name="groupId">The Id of the group that the permissions relate to</param>
    ''' <param name="groupPermissions">The new permissions for this group</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SetActualGroupPermissions(groupId As Guid, groupPermissions As IGroupPermissions)

    ''' <summary>
    ''' Gets the group level permissions for the passed group. If the group inherits
    ''' its permissions from a group higher up the tree, those will be returned
    ''' </summary>
    ''' <param name="groupID">The ID of the group</param>
    ''' <returns>A GroupPermissions object containing the available and selected
    ''' permissions for the given group</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEffectiveGroupPermissions(groupID As Guid) As IGroupPermissions

    ''' <summary>
    ''' Returns the set of permissions that are available to members of the passed
    ''' tree type (e.g. Object, Processes, Resources etc.)
    ''' </summary>
    ''' <param name="treeType">The tree type</param>
    ''' <returns>The set of available permissions</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetGroupAvailablePermissions(treeType As GroupTreeType) As ICollection(Of GroupTreePermission)

    ''' <summary>
    ''' Returns the effective (group based) permissions for the passed group member,
    ''' considering all groups within the tree that contain it.
    ''' </summary>
    ''' <param name="member">The group member</param>
    ''' <returns>The effective permissions</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEffectiveGroupPermissionsForMember(member As IGroupMember) As ICollection(Of IGroupPermissions)

    ''' <summary>
    ''' Returns a dictionary mapping treetypes to booleans stating if the user has create permission on the default groups for those tree types.
    ''' If a tree type has no default group it will return true for that tree type.
    ''' </summary>
    ''' <param name="treeTypes"></param>
    ''' <returns></returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function HasUserGotCreatePermissionOnDefaultGroup(treeTypes As List(Of GroupTreeType)) As Dictionary(Of GroupTreeType, Boolean)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function TestMemberCanAccessProcesses(processes As List(Of Guid)) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function TestMemberCanAccessObjects(processes As List(Of Guid)) As Boolean

    ''' <summary>
    ''' Gets the combined set of permissions that a group member has in a light-weight
    ''' container.
    ''' </summary>
    ''' <param name="groupMember">Identity of member</param>
    ''' <returns></returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEffectiveMemberPermissions(groupMember As IGroupMember) As IMemberPermissions

    ''' <summary>
    ''' Gets the combined set of permissions for the current user for a given
    ''' process or object.
    ''' container.
    ''' </summary>
    ''' <param name="id">Identity of the object</param>
    ''' <returns>The current permissions for that process</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEffectiveMemberPermissionsForProcess(id As Guid) As IMemberPermissions


    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function UserHasAccessToSession(sessionId As Guid) As Boolean

    ''' <summary>
    ''' Gets the combined set of permissions for a given process or object.
    ''' container.
    ''' </summary>
    ''' <param name="id">Identity of the object</param>
    ''' <returns>The current permissions for that process</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEffectiveGroupPermissionsForProcess(id As Guid) As IGroupPermissions


    ''' <summary>
    ''' Gets the combined set of permissions for a given resource
    ''' container.
    ''' </summary>
    ''' <param name="id">Identity of the object</param>
    ''' <returns>The current permissions for that process</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEffectiveGroupPermissionsForResource(id As Guid) As IGroupPermissions

    ''' <summary>
    ''' Gets the combined set of permissions for a given resource
    ''' container.
    ''' </summary>
    ''' <param name="id">Identity of the object</param>
    ''' <returns>The current permissions for that process</returns>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEffectiveMemberPermissionsForResource(id As Guid) As IMemberPermissions

#End Region

#Region "Background Jobs"
    ''' <summary>
    ''' Gets information about progress and status of a background job
    ''' </summary>
    ''' <param name="id">Background job identifier</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetBackgroundJob(id As Guid, clearWhenComplete As Boolean) As BackgroundJobData

    ''' <summary>
    ''' Clears data held for a background job
    ''' </summary>
    ''' <param name="id">Background job identifier</param>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ClearBackgroundJob(id As Guid)

#End Region

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub AuditRecordArchiveEvent(eventCode As ArchiveOperationEventCode, narrative As String, comments As String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSnapshotConfigurations() As List(Of SnapshotConfiguration)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function DeleteSnapshotConfiguration(id As Integer, name As String) As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetWorkQueueNamesAssociatedToSnapshotConfiguration(id As Integer) As ICollection(Of String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetWorkQueueIdentifiersAssociatedToSnapshotConfiguration(id As Integer) As ICollection(Of Integer)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetSnapshotConfigurationByName(configName As String) As SnapshotConfiguration

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsMIReportingEnabled() As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetQueueSnapshots() As ICollection(Of QueueSnapshot)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetQueuesWithTimezoneAndSnapshotInformation() As ICollection(Of WorkQueueSnapshotInformation)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SetQueuesToBeSnapshotted(queuesToSnapshot As ICollection(Of WorkQueueSnapshotInformation)) As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub StartQueueSnapshottingProcess()

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ClearOrphanedSnapshotData()

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function SaveConfigurationAndApplyToQueues(configToSave As SnapshotConfiguration,
                                                originalConfigName As String,
                                                queuesToConfigure As List(Of Integer)) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ConfigurationChangesWillCauseDataDeletion(configToSave As SnapshotConfiguration,
                                                       originalConfigName As String,
                                                       queuesToConfigure As List(Of Integer)) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ConfigurationChangesWillExceedPermittedSnapshotLimit(configToSave As SnapshotConfiguration,
                                                                  queuesToConfigure As List(Of Integer)) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function TriggerExistsInDatabase(snapshotId As Long, queueIdentifier As Integer) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function TriggersDueToBeProcessed() As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAuthenticationGatewayUrl() As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsAuthenticationServerIntegrationEnabled() As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAuthenticationServerUrl() As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub BusinessObjectAdded(name As String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub BusinessObjectDeleted(name As String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function DoCreateSessionCommand(sessionData As List(Of CreateSessionData)) As Guid()

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DoStartSessionCommand(sessions As List(Of StartSessionData))

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function DoSendGetSessionVariables(resId As Guid, sessId As Guid, processId As Guid) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DoDeleteSessionCommand(sessions As List(Of DeleteSessionData))

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SendStopSession(sessions As List(Of StopSessionData))

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SendSetSessionVariable(gResourceID As Guid, sessionID As Guid, vars As String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ToggleShowSessionVariables(showSessionVariables As Boolean)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetCallbackConfigProtocol() As CallbackConnectionProtocol
End Interface

Public Interface IServerPrivate
    Sub InitCallbackConfig(config As ConnectionConfig)
    Function GetExternalDependencies(connection As IDatabaseConnection, procid As Guid, Optional processOnly As Boolean = False) As clsProcessDependencyList
    Function GetProcessIDByName(con As IDatabaseConnection, ByVal sName As String, Optional ByVal IncludeBusinessObjects As Boolean = False) As Guid
    Function GetProcessNameById(con As IDatabaseConnection, ByVal id As Guid) As String
    Function GetEffectiveMemberPermissionsForProcess(con As IDatabaseConnection, id As Guid) As IMemberPermissions
    Function IncrementDataVersion(ByVal con As IDatabaseConnection, ByVal dataName As String) As Long
    Function GetAlreadyMappedActiveDirectoryUsers(activeDirectoryUsers As IEnumerable(Of ISearchResult)) As HashSet(Of String)
End Interface

<ServiceContract(SessionMode:=SessionMode.Required)>
Public Interface IServerFeatures
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetFeatureEnabled(feature As Feature) As Boolean
End Interface


<ServiceContract(SessionMode:=SessionMode.Required)>
Public Interface IServerDataPipeline
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function RegisterDataPipelineProcess(name As String, tcpServer As String) As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function UpdateDataPipelineProcessStatus(id As Integer, dataGatewayProcessState As DataGatewayProcessState, message As String) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SaveConfig(config As DataPipelineProcessConfig)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetConfigForDataPipelineProcess(id As Integer) As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function IsCustomConfiguration(configurationName As String) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDataPipelineConfigurations() As List(Of DataPipelineProcessConfig)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetConfigurationByName(name As String) As DataPipelineProcessConfig

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ReEncryptDataPipelineConfigurationFiles() As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDataGatewayProcesses() As List(Of DataGatewayProcessStatusInformation)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SendCommandToDatapipelineProcess(id As Integer, command As DataPipelineProcessCommand)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDataPipelineSettings() As DataPipelineSettings

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ErrorOnDataGatewayProcess() As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CheckConfigExistsByID(id As Integer) As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateDataPipelineSettings(settings As DataPipelineSettings)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SendCustomDataToGateway(customData As clsCollection, sessionNumber As Integer,
                                 stageId As Guid, stageName As String, stageType As StageTypes,
                                 startDate As DateTimeOffset, processName As String,
                                 pageName As String, objectName As String,
                                 actionName As String)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SendWqaSnapshotDataToDataGateways()

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetDataPipelineOutputConfigs() As IEnumerable(Of DataPipelineOutputConfig)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SaveDataPipelineOutputConfig(config As DataPipelineOutputConfig)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub RestartLogstash()

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function ProduceLogstashConfig() As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub DeleteDataPipelineOutputConfig(dataPipelineOutputConfig As DataPipelineOutputConfig)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function DefaultEnctryptionSchemeValid() As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub SaveEnvironmentData(data As EnvironmentData, Optional applicationServerPortNumber As Integer = 0, Optional applicationServerFullyQualifiedDomainName As String = Nothing)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetEnvironmentData() As List(Of EnvironmentData)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceEnvironmentData(resourceName As String) As EnvironmentData

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function CheckResourceDetailPermission() As Boolean

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceReport() As List(Of ResourceSummaryData)

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetResourceCount() As Integer

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub UpdateActiveQueueMI()

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Sub ClearInternalAuthTokens()

    ''' <summary>
    ''' Uses the TimeZoneInfo library to return the time zone ID of the configured time zone on the Blue Prism Server.
    ''' </summary>
    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetServerTimeZoneId() As String

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function MapAuthenticationServerUsers(usersToMap As List(Of UserMappingRecord), notifier As BackgroundJobNotifier) As BackgroundJob

    <OperationContract, FaultContract(GetType(BPServerFault))>
    Function GetAllNativeBluePrismUserNames() As List(Of String)
End Interface
