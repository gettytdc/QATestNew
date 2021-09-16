

IF OBJECT_ID('desc_table') IS NOT NULL
    DROP PROC desc_table
GO

CREATE PROC desc_table @tablename nvarchar(100),@description nvarchar(1000),@object_type nvarchar(16) = N'TABLE'
AS
    DECLARE @schema nvarchar(128) = SCHEMA_NAME()
    IF (SELECT COUNT(*) FROM sys.extended_properties
        WHERE major_id=OBJECT_ID(@tablename) and name='MS_Description' and minor_id=0) >0
        EXEC sys.sp_dropextendedproperty
            @name=N'MS_Description',
            @level0type=N'SCHEMA',
            @level0name=@schema,
            @level1type=@object_type,
            @level1name=@tablename  
    EXEC sys.sp_addextendedproperty
    @name=N'MS_Description',
    @value=@description,
    @level0type=N'SCHEMA',
    @level0name=@schema,
    @level1type=@object_type,
    @level1name=@tablename
GO


IF OBJECT_ID('desc_field') IS NOT NULL
    DROP PROC desc_field
GO
CREATE PROC desc_field @tablename nvarchar(255),@fieldname nvarchar(255),@description nvarchar(2000),@object_type nvarchar(16) = N'TABLE'
AS
    DECLARE @schema nvarchar(128) = SCHEMA_NAME()
    IF (SELECT COUNT(*) FROM sys.extended_properties
        WHERE major_id=OBJECT_ID(@tablename) and name='MS_Description' and minor_id=(select column_id from sys.all_columns where object_id=OBJECT_ID(@tablename) and name=@fieldname)) >0
        EXEC sys.sp_dropextendedproperty
            @name=N'MS_Description',
            @level0type=N'SCHEMA',
            @level0name=@schema,
            @level1type=@object_type,
            @level1name=@tablename,
            @level2type=N'COLUMN',
            @level2name=@fieldname  
    EXEC sys.sp_addextendedproperty
    @name=N'MS_Description',
    @value=@description,
    @level0type=N'SCHEMA',
    @level0name=@schema,
    @level1type=@object_type,
    @level1name=@tablename,
    @level2type=N'COLUMN',
    @level2name=@fieldname  
GO


DECLARE @desc nvarchar(2000)
DECLARE @table nvarchar(255)
declare @view nvarchar(255)

SET @table=N'BPAProcessAlert'
set @desc=
    N'Keeps track of which processes are of interest to which users. Primary key '+
    N'is on both fields.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'UserID',N'The id of the user'
EXEC desc_field @table,N'ProcessID',N'The ID of the process.'

SET @table=N'BPAScheduleAlert'
set @desc=
    N'Keeps track of which processes are of interest to which users. Primary key '+
    N'is on both fields.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'userid',N'The id of the user'
EXEC desc_field @table,N'scheduleid',N'The ID of the schedule.'


SET @table=N'BPAAlertEvent'
SET @desc=
    N'Table of alerts to be retrieved by alerts subscribers. The rows are inserted '+
    N'half complete, and then completed when the recipient receives the alert.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'AlertEventID',N'The ID of the event.'
EXEC desc_field @table,N'AlertEventType',N'The type of alert.'
EXEC desc_field @table,N'AlertNotificationType',N'The alert notification type.'
EXEC desc_field @table,N'Message',N'The message content of the alert.'
EXEC desc_field @table,N'ProcessID',N'The ID of the process in question, NULL if this event does not refer to a process'
EXEC desc_field @table,N'ResourceID',N'The ID of the resource in question, NULL if this event does not refer to a resource'
EXEC desc_field @table,N'SessionID',N'The ID of the session in question, NULL if this event does not refer to a session'
EXEC desc_field @table,N'Date',N'The date and time at which the alert was created.'
EXEC desc_field @table,N'SubscriberUserID',N'The ID of the user to receive the alert.'
EXEC desc_field @table,N'SubscriberResourceID',N'The ID of the resource from which the recipient acknowledges the alert.'
EXEC desc_field @table,N'SubscriberDate',N'The date time at which the recipient acknowledges the alert.'
EXEC desc_field @table,N'scheduleid', N'The ID of the schedule that this event refers to, NULL if it does not refer to a schedule'
EXEC desc_field @table,N'taskid', N'The ID of the task that this event refers to, NULL if it does not refer to a task'

SET @table=N'BPAAlertsMachines'
SET @desc=
    N'Keeps track of which resources are being used to receive alerts. Every '+
    N'time a user starts listening for alerts, the machine name is entered into '+
    N'this table. Automate will restrict the number of entries in the table according '+
    N'to the number of process alerts desktops allowed by the license.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'MachineName',N'The name of the machine.'

SET @table=N'BPAAliveResources'
SET @desc=
    N'This table helps resources determine whether another resource is running '+
    N'Automate. The instance of automate on every local machine will register '+
    N'itself every two minutes in this table by adding a timestamp. When automate '+
    N'exits cleanly, the timestamp is set to null. When a user logs out, the userid '+
    N'column is set to null.'+
    NCHAR(13)+NCHAR(10)+NCHAR(13)+NCHAR(10)+
    N'This allows other resources to infer the unclean exit (or network failure etc) '+
    N'of a given resource by observing a timestamp that is more than two minutes old.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'MachineName',N'The hostname of the resource of interest.'
EXEC desc_field @table,N'UserID',N'The ID of any user logged in on this resource.'
EXEC desc_field @table,N'LastUpdated',N'A timestamp showing when this row was last updated.'


SET @table=N'BPAAuditEvents'
SET @desc=
    N'This table holds a record of important events (such as user deletion, process '+
    N'creation etc). It is only ever written to via clsAudit. It is read by Automate '+
    N'for audit log viewing from system manager.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'EventDateTime',N'Time and date of an event'
EXEC desc_field @table,N'EventId',N'Autoincrement integer for easier reference'
EXEC desc_field @table,N'sCode',N'Short string summarising event. Eg P001 = process created'
EXEC desc_field @table,N'sNarrative',N'A friendly description of the event.'
EXEC desc_field @table,N'gSrcUserId',N'The user performing the action.'
EXEC desc_field @table,N'gTgtUserId',N'If the event affects a user (eg user deletion) the user guid recorded here.'
EXEC desc_field @table,N'gTgtProcId',N'If the event affects a process (eg process deletion) the process guid recorded here.'
EXEC desc_field @table,N'gTgtResourceID',N'If the event affects a resource (eg login or logout) then the id of the resource is recorded here.'
EXEC desc_field @table,N'Comments',N'Additional information to supplement narrative'
EXEC desc_field @table,N'NewXML',N'When a process is modified/created, the new xml goes here'
EXEC desc_field @table,N'OldXML',N'Unused'
EXEC desc_field @table,N'EditSummary',N'When a process is modified, any comments supplied by the user are added here.'



SET @table=N'BPADBVersion'
SET @desc=
    N'This table tracks the current version of the database. For each database '+
    N'upgrade script that executes, a new row is added including the new version '+
    N'number and details of what changes were made. The current database version '+
    N'can therefore be found by looking at the highest version number in the table.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'dbversion',N'The database version after running the script.'
EXEC desc_field @table,N'scriptrundate',N'The date/time (UTC) the database script was run'
EXEC desc_field @table,N'scriptname',N'The name of the script that upgraded to this version'
EXEC desc_field @table,N'description',N'A description of what the script did'
EXEC desc_field @table,N'timezoneoffset',N'The timezoneoffset for when the script was run'

SET @table=N'BPADataTracker'
SET @desc=
    N'This table supports the simplistic (and fast) tracking of arbitrary data being updated. '+
    N'It consists only of a name and a version number, and is as fine-grained as its use defines.'
EXEC desc_table @table,@desc
EXEC desc_field @table, N'dataname',N'The name of the data that is being tracked by this record'
EXEC desc_field @table, N'versionno',N'The current version number corresponding to the named data'

-- =========================
-- =    Authentication     =
-- =========================

SET @table=N'BPAInternalAuth'
SET @desc=
    N'This table is used to store authorisation tokens created by the application. '+
    N'When a TCP/IP connection is created, the local user is authenticated (either by '+
    N'a Blue Prism username and password, or by single sign-on) and an authorisation '+
    N'token is deposited in this table.

'+
    N'The sender (ie the party initiating the connection) is the one to create and '+
    N'deposit the token. The sender then passes the token over to the receiver. The '+
    N'receiver then validates the token by checking it against the database.

' +
    N'Tokens must be used within the expiry time (normally 5 minutes), but in any case '+
    N'are only valid for a single use.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'UserID',N'The ID of the user to whom the token belongs. Corresponds to the ID in BPAUser.'
EXEC desc_field @table,N'Token',N'The token''s identifier / value.'
EXEC desc_field @table,N'Expiry',N'The time at which the token will expire (UTC).'
EXEC desc_field @table,N'Roles',N'The role assignments for the token''s user.'
EXEC desc_field @table,N'IsWebService',N'True if the Authorisation token is being used via a web service request.'
EXEC desc_field @table,N'LoggedInMode',N'The logged in mode of the token''s user. 1 = Native Authentication Mode, 2 = Active Directory, 3 = Anonymous, 4 = System User.'
EXEC desc_field @table,N'ProcessId',N'The ID of the process the token is to be used with.'


SET @table=N'BPAPerm'
SET @desc=
    N'Represents the permissions which are available in the product. These are version-dependent and '
    +N'hardcoded into the product against various privileged actions'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'id',N'The unique ID of the permission.'
EXEC desc_field @table,N'name',N'A short name identifying the permission'
EXEC desc_field @table,N'treeid',N'The tree type that the permission is associated with (used for group level permissions)'

set @table=N'BPATreePerm';
set @desc=N'Table linking tree types to permissions.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'Unique ID';
exec desc_field @table, 'treeid', 'The ID of the tree';
exec desc_field @table, 'permid', 'The ID of the permission';
exec desc_field @table, 'groupLevelPerm', 'If this permission is applied at the group level (1) or the record level (0)';


set @table=N'BPATreeDefaultGroup';
set @desc=N'Table linking trees to default group.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'ID column';
exec desc_field @table, 'treeid', 'The ID of the tree';
exec desc_field @table, 'groupid', 'The ID of the group';


SET @table=N'BPALicense'
SET @desc=
    N'Stores installed license keys '
EXEC desc_table @table,@desc
EXEC desc_field @table,N'id',N'The ID of the license.'
EXEC desc_field @table,N'licensekey',N'The license key.'
EXEC desc_field @table,N'installedon',N'When this key was installed'
EXEC desc_field @table,N'installedby',N'The user that installed the key'

SET @table=N'BPAPermGroup'
SET @desc=
    N'Represents a group of permissions - typically grouped by product area or functional alignment'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'id',N'The unique ID of the permission group.'
EXEC desc_field @table,N'name',N'A short name identifying the permission group'

SET @table=N'BPAPermGroupMember'
SET @desc=
    N'Linking table which determines which permission (BPAPerm record) belongs in which permission '
    +N'group (BPAPermGroup record)'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'permgroupid',N'The id of the BPAPermGroup that this member resides in. Foreign key of BPAPermGroup'
EXEC desc_field @table,N'permid',N'The id of the permission which this member represents. Foreign key of BPAPerm'

SET @table=N'BPAUserRole'
SET @desc=
    N'Represents a user role on the database. Effectively a set of permissions which can be assigned '
    +N'to a user, or associated with some other privileged entity'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'id',N'The unique identifier for the role'
EXEC desc_field @table,N'name',N'The name of the role - user modifiable'
EXEC desc_field @table,N'ssogroup',N'The Active Directory user group which maps onto this role. Null if not using Active Directory authentication or if no user group is associated with this role.'

SET @table=N'BPAUserRolePerm'
SET @desc=
    N'Linking table which models the sets of permissions which are associated with the defined user roles.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'userroleid',N'The id of the role that is assigned the permission. Foreign key of BPAUserRole'
EXEC desc_field @table,N'permid',N'The id of the permission which is assigned to the role. Foreign key of BPAPerm'

SET @table=N'BPAUserRoleAssignment'
SET @desc=
    N'Linking table which models the user roles which are assigned to users'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'userid',N'The ID of the user who is assigned the role. Foreign key of BPAUser'
EXEC desc_field @table,N'userroleid',N'The ID of the role which is assigned to the user. Foreign key of BPAUserRole'

SET @table=N'BPAUserExternalIdentity'
SET @desc=
    N'Table which allows ids from external identity providers to be mapped to BluePrim user ids. A Blue Prism user may have ids from multiple external providers.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'bpuserid',N'The Blue Prism ID of the user. Foreign key of BPAUser'
EXEC desc_field @table,N'externalproviderid',N'The ID of the external identity provider. Foreign key of BPAExternalProvider. '
EXEC desc_field @table,N'externalid',N'The external id of the user for the specific identity provider'

SET @table=N'BPAExternalProvider'
SET @desc=
    N'Table which allows external providers to be specified for external users.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'id',N'The ID of the of the external provider. Primary key.'
EXEC desc_field @table,N'name',N'The name of the external provider. e.g. Salesforce. '
EXEC desc_field @table,N'externalprovidertypeid',N'The id of the external provider type. Foreign key of BPAExternalProviderType.'

SET @table=N'BPAExternalProviderType'
SET @desc=
    N'Table which allows external provider types to be configured for external auth accounts. E.g. "SAML" and "OpenID Connect".'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'id',N'The ID of the external provider type. Primary key.'
EXEC desc_field @table,N'name',N'The name of the external provider types. '

-- =========================
-- =      Processes        =
-- =========================

SET @table=N'BPAProcess'
SET @desc=
    N'The Process table holds the information regarding each of the processes created using the process studio.'+
    N'The process will be assigned a unique ID. The user will give a name and description. The eventual Release Management will govern the status which will come from the process status table. The version will be automatically created. The processXML record contains the XML to describe the entire process.'+
    N'This table provides storage for process in the process Studio.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'ProcessId',N'The unique ID of the process automatically assigned upon creation'
EXEC desc_field @table,'Name',N'A short name identifying the process'
EXEC desc_field @table,'ProcessType',N'One of ''O'' or ''P'' to indicate whether process is a business object or a process.'
EXEC desc_field @table,'Description',N'A description of the purpose of the process'
EXEC desc_field @table,'Version','The unique version number of the process'
EXEC desc_field @table,'Createdate','The date and time that the process was created.'
EXEC desc_field @table,'Createdby','The unique identifier of the user logged on to Automate who created the process'
EXEC desc_field @table,'Lastmodifieddate','The date and time that the process was last modified'
EXEC desc_field @table,'Lastmodifiedby','The unique identifier(userid.BPAUser) who last modified the process'
EXEC desc_field @table,'ProcessXML','The xml that represents the entire process. It is created using the process studio'
EXEC desc_field @table,'AttributeID','Records what attributes the process has as independent bitwise attributes with values 1,2,4,8, etc. Maps to BPAProcessAttribute'
EXEC desc_field @table,'wspublishname','The name the process is published as a web service as, or null if one has never been defined'
EXEC desc_field @table,'runmode','The runmode for this process (on its own). 0=Exclusive, 1=Foreground, 2=Background. Only up to date when dependenciesValid is set.'
EXEC desc_field @table,'sharedObject','Indicates that this object has global scope at runtime and can share it''s application model (not set for model sharing child objects)'
EXEC desc_field @table,'compressedxml','Placeholder for the compressed XML of the process - currently unused, but may be utilised in future releases'
EXEC desc_field @table,'forceLiteralForm','If exposed as a web service, determines the encoding format. 0 = Rpc encoding. 1 = Document/literal encoding format'
EXEC desc_field @table,'useLegacyNamespace','If exposed as a web service, determines whether to use legacy namespace structure.'

SET @table=N'BPAProcessAttribute'
SET @desc=
    'Describes the available bitmask values for the AttributeID field in the BPAProcess table'
EXEC desc_table @table,@desc
EXEC desc_field @table,'AttributeID','The number enumerating the available attributes.'
EXEC desc_field @table,'AttributeName','A string naming the attribute'


SET @table=N'BPAProcessBackup'
SET @desc=
    N'This table stores backups of processes during editing so that work can be recovered in the event of a computer crash.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'ProcessId','The unique ID of the process automatically assigned upon creation'
EXEC desc_field @table,'UserID','The ID of the user who created the backup.'
EXEC desc_field @table,'ProcessXML','The xml that represents the entire process. It is created using the process studio'
EXEC desc_field @table,'BackupDate','The date the backup was created.'
EXEC desc_field @table,'compressedxml','Placeholder for the compressed XML of the process - currently unused, but may be utilised in future releases'


SET @table=N'BPAProcessLock'
SET @desc=
    N'The Process Lock table is used to identify if a process is currently being edited by another user. '+
    N'Whenever a user edits a process a record needs to be created in this table. Until database script 28 there '+
    N'was no check on the foreign key constraint (mapping to processID in BPAProcess table). The check is made from'+
    N'script 38 onwards.'+
    N'If a row cannot be created then the process is already locked.'+
    N'If the process is already locked then the lock information can be displayed i.e. who has it locked, when they '+
    N'locked it and where they locked it from.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'processid','The unique identifier of the process currently locked'
EXEC desc_field @table,'lockdatetime','The date and time stamp that the process was locked'
EXEC desc_field @table,'userid','The unique identifier of the user logged into to the product who locked the process'
EXEC desc_field @table,'machinename','The machine which the process is locked from'

SET @table=N'BPAProcessIDDependency'
SET @desc=
    N'This table tracks dependencies between this object/process and other processes that it references.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refProcessID','The unique identifier of the referenced process'


SET @table=N'BPAProcessNameDependency'
SET @desc=
    N'This table tracks dependencies between this object/process and other business objects that it references.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refProcessName','The name of the referenced business object'


SET @table=N'BPAProcessParentDependency'
SET @desc=
    N'This table tracks dependencies between this object and the parent object holding the shared model.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refParentName','The name of the object''s parent'


SET @table=N'BPAProcessActionDependency'
SET @desc=
    N'This table tracks dependencies between this object/process and business object actions that it references.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refProcessName','The name of the business object owning the action'
EXEC desc_field @table,'refActionName','The name of the referenced action'


SET @table=N'BPAProcessElementDependency'
SET @desc=
    N'This table tracks dependencies between this object and shared application model elements that it references.'
    EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refProcessName','The name of the business object owning the application model'
EXEC desc_field @table,'refElementID','The identifier of the referenced application model element'


SET @table=N'BPAProcessWebServiceDependency'
SET @desc=
    N'This table tracks dependencies between this object/process and web services that it references.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refServiceName','The name of the referenced web service'


SET @table=N'BPAProcessQueueDependency'
SET @desc=
    N'This table tracks dependencies between this object/process and work queues that it references.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refQueueName','The name of the referenced work queue'


SET @table=N'BPAProcessCredentialsDependency'
SET @desc=
    N'This table tracks dependencies between this object/process and credentials that it references.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refCredentialsName','The name of the referenced credentials'

SET @table=N'BPAProcessEnvVar'
SET @desc=
    N'This table contains the environment variables for this object/process.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'name','The name of the environment variable'
EXEC desc_field @table,'processid','The unique identifier of the process'

SET @table=N'BPAProcessEnvironmentVarDependency'
SET @desc=
    N'This table tracks dependencies between this object/process and environment variables that it references.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refVariableName','The name of the referenced environment variable'

SET @table=N'BPAProcessCalendarDependency'
SET @desc=
    N'This table tracks dependencies between this object/process and calendars that it references.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refCalendarName','The name of the referenced calendar'


SET @table=N'BPAProcessFontDependency'
SET @desc=
    N'This table tracks dependencies between this object/process and fonts that it references.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refFontName','The name of the referenced font'

SET @table=N'BPAProcessSkillDependency'
SET @desc=
    N'This table tracks dependencies between this object/process and skills that it references.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The technical primary key for the table'
EXEC desc_field @table,'ProcessID','The unique identifier of the process'
EXEC desc_field @table,'refSkillID','The unique identifier of the referenced skill'

SET @table=N'BPAProcessMITemplate'
SET @desc=
    N'This table holds the process MI templates used to analyse information about the sessions'
EXEC desc_table @table,@desc
EXEC desc_field @table,'templatename','The name of the template'
EXEC desc_field @table,'processid','The unique identifier of the process'
EXEC desc_field @table,'defaulttemplate','The default template'
EXEC desc_field @table,'templatexml','The xml for the template'


SET @table=N'BPAReport'
SET @desc=
    N'The BPAReport table holds Crystal Reports used within the application from the report console.  Standard reports are installed to this table at installation time, and further reports can be uploaded by users'
EXEC desc_table @table,@desc
EXEC desc_field @table,'Reportid','The unique identifier of the report'
EXEC desc_field @table,'Name','Report name'
EXEC desc_field @table,'Description','Report description'
EXEC desc_field @table,'Reportdata','The contents of the report file'


SET @table=N'BPAResource'
SET @desc=
    N'The BPAResource table holds the resources (pc name, terminal services virtual session etc) of where the processes are available to run.'+
    N'The resource id is an application generated guid.'+
    N'The name is the pc name or virtual session.'+
    N'The three fields unitsallocated, actionsrunning and processesrunning provide some simple real-time statistics on the current workload of the resource unit. It is intended that the Control Room be able to display this information in some future version, though it is not strictly necessary at present.'+
    N'The statusid field holds the current status of the resource - possible values are detailed on the Visio diagram "Resource PC Flow Diagram.vsd".'+
    N'Local mode is used when a Resource PC should be visible only on the PC where it is running. Typically it will not accept incoming connections from anywhere else, and so is not displayed in remote instances of Control Room. A local resource pc is indicated by the ''Local'' attribute in the AttributeID column.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'resourceid','The unique identifier of the resource.'
EXEC desc_field @table,'name','The name of the resource'
EXEC desc_field @table,'statusid','The current status of the resource'
EXEC desc_field @table,'processesrunning','The number of processes running on that resource unit'
EXEC desc_field @table,'actionsrunning','The number of actions running on the resource unit'
EXEC desc_field @table,'unitsallocated','The number of resource units allocated'
SET @desc=
    N'When the record was last updated.'+
    N'Resource PCs are required to continually update this field when active, thereby providing a means to '+
    N'identify and remove records created by "zombie" PCs - e.g. lockup or power failure.'
EXEC desc_field @table,'lastupdated',@desc
EXEC desc_field @table,'AttributeID','Attributes possessed by this resource as independent bitwise attributes (1,2,4,8, etc). Maps to BPAResourceAttribute'
EXEC desc_field @table,'pool','The resourceid of the Resource Pool this resource is a member of, or NULL'
EXEC desc_field @table,'controller','Where this resource is a Pool, this is the resourceid of the current pool controller'
EXEC desc_field @table,'userid','Where this is a private resource, this is the id of the owning user'
EXEC desc_field @table,'displaystatus','The calculated status as shown in control room and the Workforce Availability tile'
EXEC desc_field @table,'diagnostics','Diagnostics flags.  1 = Override log inhibit on key stage types, 2 = Override log inhibit on all stage types, 4 = Log memory information, 8 = Force garbage collection when collection memory info, 16 = Log Web Service communication 32 = Override log inhibit to only show errors'
EXEC desc_field @table,'FQDN','Fully qualified domain name of the PC this resource is running on.'
EXEC desc_field @table,'logtoeventlog','Determines if this resource will log to the event log or not.'
EXEC desc_field @table,'ssl','True if the Resource requires ssl connections.'

SET @table=N'BPAResourceAttribute'
SET @desc=
    N'The BPAResourceAttribute table describes the available bitmask values for the AttributeID field in the BPAResource table.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'AttributeID','The attribute value.'
EXEC desc_field @table,'AttributeName','A string naming the attribute'

SET @table=N'BPAResourceConfig'
SET @desc=
    N'This table holds the current configuration details of each configurable business object. '+
    N'This table stores the current values for the configurable variables for an object. If the XML is missing or incorrect default values from the business object are used.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'Name','The unique business object name'
EXEC desc_field @table,'Config','XML that represents the configuration details of the object'

SET @table=N'BPAScreenshot'
SET @desc=
    N'This table holds the latest exception screen capture for the resource (encrypted where required).'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'id',N'The unique ID that identifies the screenshot (automatically generated).'
EXEC desc_field @table,N'resourceid',N'The ID of the resource that the screenshot was captured from.'
EXEC desc_field @table,N'stageid',N'The ID of the stage where the exception occurred.'
EXEC desc_field @table,N'processname',N'The name of the process running at the time of the exception.'
EXEC desc_field @table,N'lastupdated',N'The date and time that the screenshot was generated.'
EXEC desc_field @table,N'timezoneoffset',N'The offset from UTC (minutes).'
EXEC desc_field @table,N'screenshot',N'The actual screenshot image (encrypted where required).'
EXEC desc_field @table,N'encryptid',N'The ID of the encryption scheme used to encrypt the screenshot (if encrypted).'

SET @table=N'BPAScenarioLink'
SET @desc=
    N'The Scenario Link table links a business process with any number of test scenarios, held in the BPAScenario table.'+
    N'This table links the business process with any number of test scenarios, held in the BPAScenario table.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'processid','The unique process ID of the source process'
EXEC desc_field @table,'scenarioid','The unique identifier for the linked scenario'
EXEC desc_field @table,'scenarioname','Scenario name'
EXEC desc_field @table,'createdate','Date scenario was created'
EXEC desc_field @table,'Userid','User who created the scenario'




SET @table=N'BPAScenario'
SET @desc=
    N'The Scenario table holds the test scenarios for a given business process test scenario.'+
    N'The table is linked to the business process by the ScenarioLink table.  Columns hold test instructions to be defined by the test builder, a passed Boolean field and a notes field to be completed by the tester.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'ScenarioID','The unique process ID of the current process'
EXEC desc_field @table,'TestNum','The number of the test in the current scenario'
EXEC desc_field @table,'Passed','An untested (0), failed(1) or passed (2) indicator'
EXEC desc_field @table,'ScenarioText','Details of the scenario to test'
EXEC desc_field @table,'ScenarioNotes','Notes for the test scenario completed by the tester'

SET @table=N'BPAScenarioDetail'
SET @desc=
    N'The Scenario Detail table holds details outlining each test scenarios for a given business process.'+
    N'The table is linked to the test scenario table by the ScenarioID  and Testnum key fields .  The testtext Column holds additional test instructions for a test scenario, to be defined by the test builder.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'scenarioid','The unique process ID of the current process'
EXEC desc_field @table,'TestNum','The number of the test in the current scenario'
EXEC desc_field @table,'detailid','An untested (0), failed(1) or passed (2) indicator'
EXEC desc_field @table,'testtext','Details of the test scenario.'

SET @table=N'BPASession'
SET @desc=
    N'This session table holds all current and historic run details of processes.'+
    N'When a process is started (in the control room) a new record is created in this table. The session Id is automatically generated to be unique. '+
    N'This session table retains details of every process run.'+
    N'It may be that a facility to purge records may be needed to be given to someone who has a system manager role.'+
    N'The pcname of where the process was started and the pcname of where the process is run is required as the process could be run remotely. The pcname is stamped in the resource table and the assigned resource id is stamped here.  Also because of this the username operating system account of where the process is running is required. All of these parameters will help keep a detailed audit trail of the process.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'Sessionid','The unique session Id that represents that run of the process - automatically created'
EXEC desc_field @table,'startdatetime','The start datetime of the session'
EXEC desc_field @table,'starttimezoneoffset','The start timezone offset of the session'
EXEC desc_field @table,'enddatetime','The end datetime of the session'
EXEC desc_field @table,'endtimezoneoffset','The end timezone offset of the session'
EXEC desc_field @table,'Processid','The unique identifier for the process run'
EXEC desc_field @table,'Starterresourceid','The resource id the process was started from.'
EXEC desc_field @table,'Starteruserid','The unique identifier of the user who started the process'
EXEC desc_field @table,'runningresourceid','The resourceid that the process is running on.'
EXEC desc_field @table,'Runningosusername','The username used to log on to the operating system of the pc that the process is running on'
EXEC desc_field @table,'Statusid','The unique identifier of the session status, i.e Running, Stopped by user, Crashed, Finished Successfully'
EXEC desc_field @table,'Sessionstatexml','This field is obsolete.'
SET @desc=
    N'The XML that represents the start parameters of the process. This is used by Control Room to maintain state '+
    N'when creating sessions, and is also set when a process is started, so is guaranteed to contain the parameters used '+
    N'to start the session, once it has been started.'
EXEC desc_field @table,'Startparamsxml',@desc
EXEC desc_field @table,'Logginglevelsxml','This field is obsolete.'
EXEC desc_field @table,'SessionNumber','ID number used to make a relation with the log table.'
exec desc_field @table,'stoprequested','The (UTC) date/time when a stop request was made for a session';
exec desc_field @table,'stoprequestack','The (UTC) date/time when a stop request was queried by a process using the appropriate function';
exec desc_field @table,'queueid','The queue that this resource is currently assigned to';
exec desc_field @table,'lastupdated','The datetime when the last stage was started';
EXEC desc_field @table,'lastupdatedtimezoneoffset','The lastupdated timezone offset'
exec desc_field @table,'laststage','The last stage that was started';
exec desc_field @table,'warningthreshold','The number of seconds since the last updated time before the session goes into a warning state.';


SET @table=N'BPASessionLog_NonUnicode'
SET @desc=
    N'The Session log table contains log messages for a session of a process (in non-unicode format)'+
    N'This table holds information regarding audit messages produced by the process. All such logging is recorded in the database; historically there was an option to log to file but that option no longer exists.'+
    N'By default, all stages are logged when a process is run or debugged. Only when the configuration option on a stage is set, does the stage not log its details.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'LogId','An incremental number containing the id of the log messages.'
EXEC desc_field @table,'Sessionnumber','The session number.'
EXEC desc_field @table,'StageID','The ID of the stage to which this log message refers.'
EXEC desc_field @table,'StageName','The name of the stage to which this log message refers'
EXEC desc_field @table,'StageType','The type of stage to which this log message refers.'
EXEC desc_field @table,'ProcessName','The name of the process to which this log message refers'
EXEC desc_field @table,'PageName','The name of the process page to which this log message refers.'
EXEC desc_field @table,'ObjectName','The name of the business object to which this log message refers.'
EXEC desc_field @table,'ActionName','The name of the business object action to which this log message refers.'
EXEC desc_field @table,'Result','The result value, if any.'
EXEC desc_field @table,'ResultType','The result data type, if any.'
EXEC desc_field @table,'StartDateTime','The log start date.'
EXEC desc_field @table,'EndDateTime','The log end date, if any.'
EXEC desc_field @table,'AttributeXML','The stage parameters stored as XML.'
EXEC desc_field @table,'automateworkingset','The working set of the Blue Prism process.'
EXEC desc_field @table,'endtimezoneoffset','The offset in seconds of the log end date from UTC.'
EXEC desc_field @table,'starttimezoneoffset','The offset in seconds of the log start date from UTC.'
EXEC desc_field @table,'targetappname','The name of the target application to which this log message refers.'
EXEC desc_field @table,'targetappworkingset','The working set of the target application process.'


SET @table=N'BPASessionLog_Unicode'
SET @desc=
    N'The Session log table contains log messages for a session of a process (in unicode format)'+
    N'This table holds information regarding audit messages produced by the process. All such logging is recorded in the database; historically there was an option to log to file but that option no longer exists.'+
    N'By default, all stages are logged when a process is run or debugged. Only when the configuration option on a stage is set, does the stage not log its details.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'LogId','An incremental number containing the id of the log messages.'
EXEC desc_field @table,'Sessionnumber','The session number.'
EXEC desc_field @table,'StageID','The ID of the stage to which this log message refers.'
EXEC desc_field @table,'StageName','The name of the stage to which this log message refers'
EXEC desc_field @table,'StageType','The type of stage to which this log message refers.'
EXEC desc_field @table,'ProcessName','The name of the process to which this log message refers'
EXEC desc_field @table,'PageName','The name of the process page to which this log message refers.'
EXEC desc_field @table,'ObjectName','The name of the business object to which this log message refers.'
EXEC desc_field @table,'ActionName','The name of the business object action to which this log message refers.'
EXEC desc_field @table,'Result','The result value, if any.'
EXEC desc_field @table,'ResultType','The result data type, if any.'
EXEC desc_field @table,'StartDateTime','The log start date.'
EXEC desc_field @table,'EndDateTime','The log end date, if any.'
EXEC desc_field @table,'AttributeXML','The stage parameters stored as XML.'
EXEC desc_field @table,'automateworkingset','The working set of the Blue Prism process.'
EXEC desc_field @table,'endtimezoneoffset','The offset in seconds of the log end date from UTC.'
EXEC desc_field @table,'starttimezoneoffset','The offset in seconds of the log start date from UTC.'
EXEC desc_field @table,'targetappname','The name of the target application to which this log message refers.'
EXEC desc_field @table,'targetappworkingset','The working set of the target application process.'

SET @table=N'BPAStatistics'
SET @desc=
    N'This table stores statistics from process runs. A running process will create an entry in this table for each statistic (i.e. a data item with the statistic flag set) defined in that process. '
EXEC desc_table @table,@desc
EXEC desc_field @table,'Sessionid','The session ID this statistic relates to'
EXEC desc_field @table,'Name','The name of the statistic'
EXEC desc_field @table,'Datatype','The data type - must be one of the Automate data types'
EXEC desc_field @table,'Value_text','The current value, for text data types'
EXEC desc_field @table,'Value_number','The current value, for number data types, or the total number of seconds for timespan data types.'
EXEC desc_field @table,'Value_date','The current value, for date, time and datetime data types'
EXEC desc_field @table,'Value_flag','The current value, for flag data types'




SET @table=N'BPAStatus'
SET @desc=
    N'The Status table is a lookup which holds all the possible values of the status of the process at design time and the session at runtime.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'statusID','The unique status ID created when the status is initialised'
EXEC desc_field @table,'Type','The type of status, eg process or session'
EXEC desc_field @table,'Description','A textual description of the status (eg Live, Test)'



SET @table=N'BPASysConfig'
SET @desc=
    N'The system configuration table holds details regarding the set-up of the product as a whole. The settings are global to all users.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'Id','Unique identifier for the table. It is envisaged only ever one row will exist. This Id is just to create a primary key instead of using every row in the table'
EXEC desc_field @table,'maxnumconcproc','Maximum number of concurrent processes allowed. When a process is started it needs to check that the current number of concurrent running processes (calculated from the runtime table) will allow the new process to be started without ever exceeding the maximum number of concurrent processes. This value will be encrypted.'
EXEC desc_field @table,'Autosaveinterval','Records in minutes the autosave interval.'
EXEC desc_field @table,'EnforceEditSummaries','Boolean variable for system manager setting. Should all users have to supply summary of changes to processes in process studio.'
EXEC desc_field @table,'ArchivingResource','The ID of the Resource that will perform automatic archiving.'
EXEC desc_field @table,'ArchiveInProgress','Used by the archiving class to indicate that archiving is in progress. The hostname of the machine performing the archiving is recorded here. When null or empty, no archiving is taking place.'
EXEC desc_field @table,'PassWordExpiryWarningInterval','The number of days before a password expiry that the user will be remainded by means of a balloon tip at the login screen. When set to zero the user will not be prompted.'
EXEC desc_field @table,'Populateusernameusing','Option set within System Manager that indicates whether Automate starts with no user name on the login screen, the previous user name or the windows user name.'
EXEC desc_field @table,'maxloginattempts','The maximum number of login attempts that a user can make before being locked out.'
EXEC desc_field @table,'ActiveDirectoryProvider','The NetBIOS name of the Active Directory domain used for logins. If this is empty, the database is not single-signon enabled, and standard Blue Prism authentication is used.'
EXEC desc_field @table,'DependencyState','Tri-state flag denoting whether the process dependency information is stale (0), being updated (1) or up to date (2)'
EXEC desc_field @table,'UnicodeLogging','Indicates that session logs are written to the Unicode version of the session log table (default is false)'
EXEC desc_field @table,'defaultencryptid','The ID of the default encryption scheme used to encrypt credential data, screen captures etc.'
EXEC desc_field @table,'ArchivingAge','The age after which session logs will be archived.'
EXEC desc_field @table,'ArchivingDelete','Determines whether archived logs will be deleted from the database.'
EXEC desc_field @table,'ArchivingFolder','The folder where logs will be archived to'
EXEC desc_field @table,'ArchivingLastAuto','The last time auto-archiving completed.'
EXEC desc_field @table,'ArchivingMode','The archiving mode. Auto = 1, Manual = 0.'
EXEC desc_field @table,'CompressProcessXML','Determines whether process xml should be compressed.'
EXEC desc_field @table,'PreventResourceRegistration','Prevents new resources being registered if set to 1.'
EXEC desc_field @table,'RequireSecuredResourceConnections','Determines if secure connections to resources are required.'
EXEC desc_field @table,'ResourceRegistrationMode','The resource registration mode. 0 = Machine'
EXEC desc_field @table,'showusernamesonlogin','Show a list of usernames on the login screen.'
EXEC desc_field @table, 'authenticationgatewayurl', 'Url specifying the location of Authentication Gateway'
EXEC desc_field @table, 'authenticationserverurl', 'Url specifying the location of Authentication Server'


SET @table=N'BPAToolPosition'
SET @desc=
    N'This table stores the preferred process studio tool position for each user.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'UserID','The ID of the user whose preference this row represents.'
EXEC desc_field @table,'Name','The name of the toolbar.'
EXEC desc_field @table,'Position','One of ''T'', ''B'', ''L'' or ''R'' to indicate top, bottom, left or right.'
EXEC desc_field @table,'X','The x coordinate of the preferred position, in pixels.'
EXEC desc_field @table,'Y','The y coordinate of the preferred position, in pixels.'
EXEC desc_field @table,'Mode','One of ''O'' or ''P'' to indicate process studio or object studio.'
EXEC desc_field @table,'Visible','Indicates whether the toolbar is visible.'


SET @table=N'BPAUser'
SET @desc=
    N'This table contains details of the automate user. For encryption details see section 5 - Encryption. '

EXEC desc_table @table,@desc
EXEC desc_field @table,'UserId','Unique identifier for the user. Automatically and uniquely assigned upon creation'
EXEC desc_field @table,'Username','The username entered to access the product - this will be NULL for system users. For Active Directory environments, it is the UPN.'
EXEC desc_field @table,'Systemusername', 'The username for this system user - for normal users, this will be NULL. For system users, this will hold the system user name'
EXEC desc_field @table,'ValidFromdate','The date that the username is valid from'
EXEC desc_field @table,'ValidTodate','The date the username becomes invalid'
EXEC desc_field @table,'PasswordExpiryDate','The date/time (UTC) the password expires and needs renewing.'
EXEC desc_field @table,'UserEmail','The email address of the user that new passwords could be sent to for security reasons.'
EXEC desc_field @table,'IsDeleted','Records whether a user has been deleted. Defaults to 0.'
EXEC desc_field @table,'UseEditSummaries','Does this user prefer to use edit summaries in process studio or not? The setting in BPASysConfig is ORed together with this preference.'
EXEC desc_field @table,'PreferredStatisticsInterval','Indicates the users refresh interval for the statistics page.'
EXEC desc_field @table,'SaveToolStripPositions','Indicates whether the user wants their process studio/object studio toolbar positions remembered.'
EXEC desc_field @table,'PasswordDurationWeeks','The number of weeks between password expiries for the user in question. Can be null in which case a software default is applied.'
EXEC desc_field @table,'AlertEventTypes','The alert event types that the user is interested in. Zero indicates no alert event types'
EXEC desc_field @table,'AlertNotificationTypes','The alert notification types that the user prefers to receive. Zero indicates no alert notification types'
EXEC desc_field @table,'LogViewerHiddenColumns','The log viewer column the user has chosen to hide.'
EXEC desc_field @table,'loginattempts','The number of failed login attempts that the user has made.'
EXEC desc_field @table,'lastsignedin','The date/time (UTC) of the last successful login for the user.'
EXEC desc_field @table,'authtype','The authentication type for each user '
EXEC desc_field @table,'authenticationServerClientId','The unique identifier of the Authentication Server service account that the user record represents. '
EXEC desc_field @table,'authenticationServerUserId','The unique identifier of the Authentication Server user that the user record represents. '
EXEC desc_field @table,'deletedLastSynchronizationDate','The datetimestamp that the user or service account was last retired or unretired on the Authentication Server.  '
EXEC desc_field @table,'hasBluePrismApiScope','Records whether the authentication server service account has permission to access the Blue Prism API'
EXEC desc_field @table,'updatedLastSynchronizationDate','The time the user was last updated by synchronisation with the authentication server '
EXEC desc_field @table,'authServerName','The user / client name of the user / service account on the authentication server. '

SET @table=N'BPAWebService'
SET @desc=
    N'This table stores details of external web services which have been configured for use via the System Manager interface. These are '+
    N'the web services that are available to be used from within processes.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'serviceid','A unique ID for the web service reference'
EXEC desc_field @table,'enabled','The web service cannot be called if this is False, thereby allowing a system admin to temporarily disable access to a web service without deleting the information.'
EXEC desc_field @table,'servicename','Descriptive name for the web service. This is the name that will be presented to the user, and also the name used for referencing within a process, to allow portability of a process between systems.'
EXEC desc_field @table,'url','The address of the web service'
EXEC desc_field @table,'wsdl','This is a cache of the web service description language.'
EXEC desc_field @table,'settingsxml','This is the configuration XML which contains the information about which services and methods are to be used.'
EXEC desc_field @table,'timeout','The timeout applied to calls to this web service, in milliseconds.'

SET @table=N'BPAWebServiceAsset'
SET @desc=
    N'This table stores details of external web service assets such as referenced wsdl files and xsd schema documents.'
EXEC desc_table @table,@desc
exec desc_field @table, 'serviceid', 'The ID service';
EXEC desc_field @table,'assettype','The type of asset. Currently 0 for unknown, 1 for wsdl and 2 for xsd.'
EXEC desc_field @table,'assetxml','The xml data of the asset document.'

SET @table=N'BPATag'
SET @desc=
    N'This table stores shared tags which can ostensibly be applied to anything, but for now are limited to '+
    N'work queue items - available for storing states, business data or anything else which might be useful.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The unique ID for the tag'
EXEC desc_field @table,'tag','The text of the tag'

SET @table=N'BPAWorkQueue'
SET @desc=
    N'This table contains information about all the queues defined on the system, and their status.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The globally unique ID for this queue'
EXEC desc_field @table,'ident','The internal ID for this queue'
EXEC desc_field @table,'name','The name of this queue'
EXEC desc_field @table,'keyfield','The name of the collection field that serves as the key for this queue. This can be blank if no key is defined for this queue.'
EXEC desc_field @table,'running','Determines whether the queue is running or not.'
EXEC desc_field @table,'maxattempts','The maximum number of attempts that should be made at a case before it is not re-presented for working'
EXEC desc_field @table,'DefaultFilterID','The ID of the default filter used with this queue. Corresponds to a filter in BPAWorkQueueFilter'
EXEC desc_field @table,'encryptid','The ID of the encryption scheme used to encrypt the item data for items on this queue - NULL indicates that the queue is not encrypted'
exec desc_field @table,'processid','The process used to work an active queue; null for non-active queues';
exec desc_field @table, 'resourcegroupid', 'The group of resources assigned to an active queue; null for non-active queues';
exec desc_field @table, 'activelock', 'A GUID representing the lock on an Active Queue. NULL if the queue is not locked.';
exec desc_field @table, 'activelockname', 'The name of the machine which acquired the lock on this queue.';
exec desc_field @table, 'activelocktime', 'The time the lock on this queue was acquired.';
exec desc_field @table, 'targetsessions', 'The target session count for this queue.';

SET @table=N'BPASnapshotConfiguration'
SET @desc=
    N'This table contains information for the snapshot configuration.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The unique ID for the snapshot configuration.'
EXEC desc_field @table,'interval','Determines the snapshot interval.'
EXEC desc_field @table,'name','The unique name for the snapshot configuration.'
EXEC desc_field @table,'timezone','Determines the timezone for the snapshot configured regardless of what timezones the appservers/resourcepcs working the queue are in.'
EXEC desc_field @table,'startsecsaftermidnight','The time of day the queue will start snapshotting.'
EXEC desc_field @table,'endsecsaftermidnight','The time of day will end snapshotting.'
EXEC desc_field @table,'sunday','Determines if the snapshot will be taken on Sunday.'
EXEC desc_field @table,'monday','Determines if the snapshot will be taken on Monday.'
EXEC desc_field @table,'tuesday','Determines if the snapshot will be taken on Tuesday.'
EXEC desc_field @table,'wednesday','Determines if the snapshot will be taken on Wednesday.'
EXEC desc_field @table,'thursday','Determines if the snapshot will be taken on Thursday.'
EXEC desc_field @table,'friday','Determines if the snapshot will be taken on Friday.'
EXEC desc_field @table,'saturday','Determines if the snapshot will be taken on Saturday.'
EXEC desc_field @table,'isenabled','Determines if the snapshot configured is enabled.'

SET @table=N'BPMIConfiguredSnapshot'
SET @desc=
    N'This table contains information for the configured snapshots.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'snapshotid','The unique ID for this snapshot'
EXEC desc_field @table,'queueident','The ID for the queue'
EXEC desc_field @table,'timeofdaysecs','The configured snapshot time in secs.'
EXEC desc_field @table,'dayofweek','The configured day of the week the snapshot will be taken.'
EXEC desc_field @table,'interval','Determines the configured snapshot interval.'

SET @table=N'BPMIQueueSnapshot'
SET @desc=
    N'This table contains information for the queue snapshots.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The unique ID for this queue.'
EXEC desc_field @table,'snapshotid','The ID for this snapshot.'
EXEC desc_field @table,'snapshotdate','The date the snapshot was taken.'
EXEC desc_field @table,'capturedatetimeutc','Actual date and time this snapshot is captured.'
EXEC desc_field @table,'totalitems','Total items in queue at snapshot.'
EXEC desc_field @table,'itemspending','Total Pending items in the queue at snapshot.'
EXEC desc_field @table,'itemscompleted','Total Completed items in the queue at snapshot.'
EXEC desc_field @table,'itemsreferred','Total Referred items in the queue at snapshot.'
EXEC desc_field @table,'newitemsdelta','Total new items since last snapshot.'
EXEC desc_field @table,'completeditemsdelta','Total completed items since last snapshot.'
EXEC desc_field @table,'referreditemsdelta','Total referred items since last snapshot.'
EXEC desc_field @table,'totalworktimecompleted','The total work time (secs) of all items completed since last snapshot.'
EXEC desc_field @table,'totalworktimereferred','The total work time (secs) of all items referred since last snapshot.'
EXEC desc_field @table,'totalidletime','The total wait time (secs) of all items completed or referred since last snapshot.'
EXEC desc_field @table,'totalnewsincemidnight','The total new items since midnight.'
EXEC desc_field @table,'totalnewlast24hours','The total new items in the last 24 hours.'
EXEC desc_field @table,'averagecompletedworktime','Total Completed Work Time / Total Completed Items.'
EXEC desc_field @table,'averagereferredworktime','Total Referred Work Time / Total Referred Items.'
EXEC desc_field @table,'averageidletime','Total Wait Time / (Total Completed Items + Total Referred Items).'


SET @table=N'BPMIQueueTrend'
SET @desc=
    N'This table contains information for queue trend data over the last 7 days, last 28 days and last 4 weekday instances.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The unique ID for the queue trend.'
EXEC desc_field @table,'snapshottimeofdaysecs','The configured snapshot time in secs.'
EXEC desc_field @table,'queueident','The ID for the queue.'
EXEC desc_field @table,'trendid','Integer corresponding to an enumeration of the 3 trends as identifier.'
EXEC desc_field @table,'capturedatetimeutc','Actual date and time this trend is captured.'
EXEC desc_field @table,'averagetotalitems','Average of the total number of items in queue at snapshot time.'
EXEC desc_field @table,'averageitemspending','Average of the total number of Pending items in the queue at snapshot.'
EXEC desc_field @table,'averageitemscompleted','Average of the total number of Completed items in the queue at snapshot.'
EXEC desc_field @table,'averageitemsreferred','Average of the total number of Referred items in the queue at snapshot.'
EXEC desc_field @table,'averagenewitemsdelta','Average of the total number of New items (added since last snapshot).'
EXEC desc_field @table,'averagecompleteditemsdelta','Average of the total number of items completed since last snapshot.'
EXEC desc_field @table,'averagereferreditemsdelta','Average of the total number of items Referred since last snapshot.'
EXEC desc_field @table,'averagetotalworktimecompleted','Average of the Completed Work Time (secs) (all items completed since last snapshot).'
EXEC desc_field @table,'averagetotalworktimereferred','Average Referral Work Time (secs) (all items referred since last snapshot).'
EXEC desc_field @table,'averagetotalidletime','Average Item Idle Time (secs) (all items completed or referred since last snapshot).'
EXEC desc_field @table,'averagetotalnewsincemidnight','The total new items since midnight.'
EXEC desc_field @table,'averagetotalnewlast24hours','The total new items in the last 24 hours.'
EXEC desc_field @table,'averageaveragecompletedworktime','Average Total Completed Work Time / Total Completed Items.'
EXEC desc_field @table,'averageaveragereferredworktime','Average Total Referred Work Time / Total Referred Items.'
EXEC desc_field @table,'averageaverageidletime','Average Total Wait Time / (Average Total Completed Items + Average Total Referred Items).'

SET @table=N'BPMISnapshotTrigger'
SET @desc=
    N'This table contains snapshot requests for work queue analysis.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'queueident','The queue that a snapshot was requested for.'
EXEC desc_field @table,'snapshotid','The id of the snapshot to create (see BPMIConfiguredSnapshots).'
EXEC desc_field @table,'lastsnapshotid','The id of the last snapshot for this queue.'
EXEC desc_field @table,'eventtype','The type of snapshot requested (1 = Snapshot, 2 = Trend Calculation, 3 = Both).'
EXEC desc_field @table,'snapshotdate','The local date/time for the snapshot to be triggered.'
EXEC desc_field @table,'snapshotdateutc','The UTC date/time for the snapshot to be triggered.'
EXEC desc_field @table,'midnightutc','The UTC representation of the previous midnight for the timezone associated with this queue.'

SET @table=N'BPAWorkQueueLog'
SET @desc=
    N'Contains an audit log for work queue operations - used in transactional charging.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'logid','The unique ID for the log entry'
EXEC desc_field @table,'eventtime','The time that the event occurred'
EXEC desc_field @table,'queueident','The identity of the queue on which the operation was performed'
EXEC desc_field @table,'queueop','The operation which was performed'
EXEC desc_field @table,'itemid','The ID of the item on which the operation was performed'
EXEC desc_field @table,'keyvalue','The key value of the item on which the operation was performed'

SET @table=N'BPAWorkQueueItem'
SET @desc=
    N'This table contains all the items in all the queues that are defined.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','The globally unique ID for this queue item'
EXEC desc_field @table,'ident','The IDENTITY field for this queue item'
EXEC desc_field @table,'queueid','The GUID of the BPAWorkQueue record, which owns this item'
EXEC desc_field @table,'queueident','The foreign key identifying the queue containing this item'
EXEC desc_field @table,'sessionid ','The last session which updated this queue item'
EXEC desc_field @table,'keyvalue','A key, used to find specific data - e.g. an account number'
EXEC desc_field @table,'status','A field that can be used by the process to record how much of the case it has completed'
EXEC desc_field @table,'attempt','The attempt number of this item - starts at 1 for the first attempt and increments for each retry clone created.'
EXEC desc_field @table,'loaded','Timestamp of when the case was loaded into the queue'
EXEC desc_field @table,'completed','Timestamp of when the case was completed. If null, it has not been completed.'
EXEC desc_field @table,'exception','Timestamp of when the case was abandonded due to an exception. Null if it has never happened.'
EXEC desc_field @table,'exceptionreason','Reason for last exception'
EXEC desc_field @table,'deferred','Timestamp of when this case is deferred until. Null if it isn''t.'
EXEC desc_field @table,'worktime','The total amount of time, in seconds, that has been spent working on this item (including time spent on prior attempts)'
EXEC desc_field @table,'data','The data associated with the queue item. This is in the form of an XML-encoded collection, which is passed to the internal business object when the item is loaded into the queue, and passed back out when the item is retrieved from the queue.'
EXEC desc_field @table,'priority','The priority of the item. Lower numbers are higher priority.'
EXEC desc_field @table,'lastupdated', 'Date that the item last changed - ie. loaded or finished. Deterministic calculated column'
EXEC desc_field @table,'exceptionreasonvarchar', 'varchar of exceptionreason to allow ordering and filtering on it. Deterministic calculated column'
EXEC desc_field @table,'exceptionreasontag', 'tag of exceptionreason'
EXEC desc_field @table,'prevworktime', 'The total amount of worktime for all attempts on the item prior this one'
EXEC desc_field @table,'attemptworktime', 'The amount of worktime spent on this attempt of the item only. Deterministic calculated column'
EXEC desc_field @table,'finished', 'The date the item was finished (ie. exceptioned or completed), null if it is not finished. Deterministic calculated column.'
EXEC desc_field @table,'encryptid','The ID of the encryption scheme used to encrypt this item''s data'
EXEC desc_field @table,'locktime','When set holds the time the item was last locked'
EXEC desc_field @table,'lockid','When set holds the unique ID used to lock the item'


SET @table=N'BPACaseLock';
SET @desc=
    N'Stores locking information for work items held in the BPAWorkQueueItem table';
EXEC desc_table @table,@desc;
EXEC desc_field @table,'id','The ID of the locked case - the work queue item identity';
EXEC desc_field @table,'locktime','The UTC date/time that the lock record was created - ie. when the case was locked';
EXEC desc_field @table,'sessionid','The ID of the session which has this record locked';
EXEC desc_field @table,'lockid','A unique GUID assigned to this ID - used to retrieve item information after locking';

SET @table=N'BPAWorkQueueItemTag'
SET @desc=
    N'Table linking the work queue items to the tags'
EXEC desc_table @table, @desc
EXEC desc_field @table, 'queueitemident', 'The internal identity field of the work queue item - used as a foreign key'
EXEC desc_field @table, 'tagid', 'The foreign key pointing to the tag record which this record represents'

SET @table=N'BPAWorkQueueFilter'
SET @desc=
    N'This table stores saved filters used for inspecting work queues. A filter limits the queue items returned by filtering on date, status, exception, etc. A default filter can be associated with a queue using the BPAWorkQueue.DefaultFilterID field.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'FilterID','A unique ID for the filter'
EXEC desc_field @table,'FilterName','The name of the filter, as supplied by the user. Must be unique.'
EXEC desc_field @table,'FilterXML','The xml representing the filter. NOT NULL.'


SET @table=N'BPACredentials'
SET @desc=
    N'This table stores credentials information.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','A unique ID for the credential'
EXEC desc_field @table,'name','The name of the credential'
EXEC desc_field @table,'description','A description of the credential'
EXEC desc_field @table,'login','The login'
EXEC desc_field @table,'password','The password, in encrypted form'
EXEC desc_field @table,'expirydate','The date on which the credential will expire.'
EXEC desc_field @table,'invalid','A flag indicating that the credentials are invalid.'
EXEC desc_field @table,'encryptid','The ID of the encryption scheme used to encrypt this credential'
EXEC desc_field @table, 'credentialType', 'The type of credential indicating its intended use within the system'

SET @table=N'BPACredentialsProcesses'
SET @desc=
    N'Records details of processes which are allowed access to a particular credential.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'credentialid','The ID of the associated credential'
EXEC desc_field @table,'processid','The ID of the process'

SET @table=N'BPACredentialsResources'
SET @desc=
    N'Records details of resources which are allowed access to a particular credential.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'credentialid','The ID of the associated credential'
EXEC desc_field @table,'resourceid','The ID of the resource'

SET @table=N'BPACredentialRole'
SET @desc=
    N'Linking table which models the user roles which have the ability to get or set credential data. '
    +N'Note that, for any given credential, a single BPACredentialRole record with a null userroleid represents "any roles", '
    +N'whereas no records represents "no roles" - ie. it cannot be accessed at all'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'credentialid',N'The ID of the credential which is accessible to the role. Foreign key of BPACredentials'
EXEC desc_field @table,N'userroleid',N'The ID of the role that has access to the credential. A single record with a null value in this field represents "any role". Foreign key of BPAUserRole'

SET @table=N'BPACredentialsProperties'
SET @desc=
    N'Holds custom properties and values for a particular credential.'
EXEC desc_table @table,@desc
EXEC desc_field @table,'id','A unique ID for the property.'
EXEC desc_field @table,'credentialid','The ID of the associated credential.'
EXEC desc_field @table,'name','The name of the property.'
EXEC desc_field @table,'value','The value of the property.'

-- Calendar stuff

set @table=N'BPAPublicHoliday';
set @desc=N'A single public holiday defined algorithmically, allowing the code to calculate the correct date for a particular year';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The unique ID of the public holiday, assigned externally';
exec desc_field @table, 'name', 
    'The name of the public holiday - note that this may not be unique since 2 groups could have public holidays with the same name which refer to different dates';
exec desc_field @table, 'dd', 'The specific date of the public holiday - set only if the public holiday falls on a specific date within the calendar.';
exec desc_field @table, 'mm', 'The 1-based month of the public holiday - set if the holiday falls on a specific date, or in a specific month';
exec desc_field @table, 'dayofweek', 'The day that the holiday falls on - Sun=0, Mon=1, Tue=2, Wed=3, Thu=4, Fri=5, Sat=6 - set for the "first Monday" or such like';
exec desc_field @table, 'nthofmonth', 'The occurrence within the month of the holiday: 1=First, 2=Second etc. Special case: -1 = Last';
exec desc_field @table, 'relativetoholiday', 'Set to the ID of the holiday that this holiday is relative to, NULL if it is not relative to another holiday';
exec desc_field @table, 'relativedaydiff', 'The number of days offset from the holiday that this holiday is relative to. eg. Boxing Day is Xmas Day + 1 day';
exec desc_field @table, 'eastersunday', 
    'Bit indicating that this holiday represents easter sunday - created primarily to allow dates relative to Easter Sunday to be specified';

set @table=N'BPAPublicHolidayGroup';
set @desc=N'Defines a group of public holiday as a discrete entity';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The unique ID of this group';
exec desc_field @table, 'name', 'The name of this public holiday group, usually the country/countries whose public holidays the group represents';

set @table=N'BPAPublicHolidayGroupMember';
set @desc=N'Binds the BPAPublicHoliday into a group, creating of it a group member';
exec desc_table @table, @desc;
exec desc_field @table, 'publicholidaygroupid', 'The group of which this record represents a member';
exec desc_field @table, 'publicholidayid', 'The public holiday which this record makes a member of the related group';

set @table=N'BPACalendar';
set @desc=
    N'Base calendar which draws together the working week, public holiday group and non-working days into a discrete entity. '+
    N'A default calendar called "Working Week / No Holidays" is created which specifies Mon-Fri inclusive as working days ';
exec desc_table @table, @desc
exec desc_field @table, 'id', 'The unique ID of the calendar within this database';
exec desc_field @table, 'name', 'The name of the calendar, again unique within this database';
exec desc_field @table, 'description', 'A long description of this calendar';
exec desc_field @table, 'publicholidaygroupid', 'The ID of the public holiday group assigned to this calendar, or NULL if none is assigned';
exec desc_field @table, 'workingweek', 
    'A bitflag integer containing a bit for each day of the week (Sun=1, Mon=2, Tue=4, Wed=8, Thu=16, Fri=32, Sat=64) in which a 1 indicates it is a working day';

set @table=N'BPANonWorkingDay';
set @desc=N'The specific days which represent a non-working day for the specified calendar';
exec desc_table @table, @desc;
exec desc_field @table, 'calendarid', 'The calendar on which this record defines a non-working day';
exec desc_field @table, 'nonworkingday', 'The actual date which represents a non-working day';

set @table=N'BPAPublicHolidayWorkingDay';
set @desc = 
    N'Public holiday overrides for a calendar - these allow a calendar to have a public holiday group assigned, '+
    N'but to have certain public holidays still be classed as working days, through the existence of a record in this table.'
exec desc_table @table, @desc;
exec desc_field @table, 'calendarid', 'The calendar on which this record overrides a public holiday to make it a working day';
exec desc_field @table, 'publicholidayid', 'The public holiday that this record is overriding';

-- Schedule proper

set @table=N'BPASchedule';
set @desc=N'The schedule itself, indicating the initial task to execute at a pre-specified time.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The ID of this schedule, unique within this database';
exec desc_field @table, 'name', 'The name of this schedule. If NULL, this represents a deleted schedule - the name is moved to "deletedname"';
exec desc_field @table, 'description', 'A full description of this schedule';
exec desc_field @table, 'initialtaskid', 'The first task to run as part of this schedule. NULL indicates that no task is assigned and nothing will run';
exec desc_field @table, 'retired', 'Flag indicating that this schedule has been retired and will not run';
exec desc_field @table, 'deletedname', 'The name of the schedule before it was deleted';
exec desc_field @table, 'versionno', 'The version number of the schedule - this is incremented every time a schedule is changed';

set @table=N'BPATask';
set @desc=N'The smaller execution unit of a schedule - these are activated in sequence depending on the exit state of the previous task';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The ID of this task, unique within this database';
exec desc_field @table, 'scheduleid', 'The schedule that this task forms a part of';
exec desc_field @table, 'name', 'The name of this task - this should be unique within a particular schedule, though that isn''t constrained by the database.';
exec desc_field @table, 'description', 'A fuller description of the task';
exec desc_field @table, 'onsuccess', 'The task to perform after successful execution of this task. NULL indicates that the schedule has completed';
exec desc_field @table, 'onfailure','The task to perform if this task has failed. NULL indicates that the schedule should be aborted';
exec desc_field @table, 'failfastonerror', 'Bit indicating that if any session in the task fails, the rest of the task should be terminated including any currently running sessions';

set @table=N'BPATaskSession';
set @desc=N'The assignation of a process / resource to a task - a "session" in scheduler parlance';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The identity of this BPATaskSession';
exec desc_field @table, 'taskid', 'The ID of the task to which this session belongs';
exec desc_field @table, 'processid', 'The process which is to run inside this session';
exec desc_field @table, 'resourcename', 'The resource on which this session will run';
exec desc_field @table, 'failonerror', 'flag indicating that a failure in this session indicates a failure of the task itself';
exec desc_field @table, 'processparams', 'XML snippet containing a collection of parameters for the process';

set @table=N'BPAScheduleTrigger';
set @desc=N'The timing data for a schedule - configuration of the times at which a schedule is repeated';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The ID of this trigger, unique within this database';
exec desc_field @table, 'scheduleid', 'The schedule that this trigger is assigned to';
exec desc_field @table, 'usertrigger', 'Flag to indicate that this trigger is user-configurable';
exec desc_field @table, 'priority', 'The priority of this trigger over other triggers - a higher number represents a higher priority';
exec desc_field @table, 'mode', 'The "mode" of this trigger - Fire = 1; Suppress = 2. Fire will initiate a schedule, Suppress will suppress the initiation of a schedule. Note that a "Suppress" trigger has a higher natural priority than a "Fire" trigger';
exec desc_field @table, 'unittype', 'The unit of repetition for this trigger: defined in BluePrism.Scheduling.IntervalType: (-1=Never; 0=Once; 1=Hour; 2=Day; 3=Week; 4=Month; 5=Year; 6=Millisecond; 7=Second; 8=Minute)';
exec desc_field @table, 'period', 'The number of time units between activations';
exec desc_field @table, 'startdate', 'The date from which this trigger is active';
exec desc_field @table, 'enddate', 'The date at which point this trigger ceases to be active';
exec desc_field @table, 'startpoint', 'The start point for a range which is valid for this trigger - currently used to contain an encoded start time in the form "hhmmss"';
exec desc_field @table, 'endpoint', 'The end point for a range which is valid for this trigger - currently used to contain an encoded end time in the form "hhmmss"';
exec desc_field @table, 'dayset', 'The days on which this trigger should activate - like BPACalendar.workingweek this is a bitset where a 1 indicates that the trigger should activate on that day. Values are: (1=Sun; 2=Mon; 4=Tue; 8=Wed; 16=Thu; 32=Fri; 64=Sat)';
exec desc_field @table, 'calendarid', 'The calendar which is assigned to this trigger, if any is assigned. NULL if not';
exec desc_field @table, 'nthofmonth', 'Indicates the "nth" occurrence of a day within a month (first Monday; last working day etc.). ';
exec desc_field @table, 'missingdatepolicy', 'Provides the guidance of what to do if a particular date does not exist within a month - eg. "31st June" or "5th Sunday in February". (0=Skip month; 1=Use last supported day of month; 2=Use first supported day of subsequent month). "Supported" means supported by assigned calendar, if one is assigned';

-- Scheduler Logs and lists
set @table=N'BPAScheduleLog';
set @desc=N'An individual log for a particular executed instance of a schedule';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The ID of this log, unique within this database';
exec desc_field @table, 'scheduleid', 'The schedule that this log is a record of';
exec desc_field @table, 'instancetime', 'The time of the instance that this log represents - note that this is not necessarily the time that the instance ran, but the time it was *scheduled* to run';
exec desc_field @table, 'firereason', 'An indication of why this instance was fired: (0=Normal Scheduler Execution; 1=Misfire:Missed while scheduler not running; 2=Misfire:Timezone changing; 3=Misfire:indeterminate trigger)';
exec desc_field @table, 'servername', 'The name of the scheduler which was last registered as running the schedule which generated this log - typically in the format "MachineName:PortNumber"';
exec desc_field @table, 'heartbeat', 'Last time that the schedule log was pulsed, indicating that the schedule is still being executed.';

set @table=N'BPAScheduleLogEntry';
set @desc=N'An entry within the specified log indicating execution activity of the schedule / task / session';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The unique identifier for this log entry';
exec desc_field @table, 'schedulelogid', 'The ID of the log that this entry forms part of';
exec desc_field @table, 'entrytype', 'The type of log entry that this record represents: (1=Schedule Started; 2=Schedule Completed; 3=Schedule Terminated; 4=Task Started; 5=Task Completed; 6=Task Terminated; 7=Session Started; 8=Session Completed; 9=Session Terminated)';
exec desc_field @table, 'taskid', 'The task that this entry represents. Note that this may point to a nonexistent task if the task is deleted and this entry remains. Also note that this is only populated on records with an entrytype of between 4 and 9 inclusive';
exec desc_field @table, 'logsessionnumber', 'The IDENTITY of the session which this log entry generated. This is only populated on records with an entrytype of between 7 and 9 inclusive';
exec desc_field @table, 'terminationreason', 'A user-presentable reason why this schedule was terminated, NULL if this entry is not a termination entry'
exec desc_field @table, 'stacktrace', 'Stack trace of any exception which caused this schedule to terminate, NULL if this entry is not a termination entry or no stack trace was recorded'
exec desc_field @table, 'entrytime', 'Time the log entry was created'

set @table=N'BPAScheduleList';
set @desc=N'Defines a list of schedule instances: either a report of executed instances or a timetable of upcoming instances';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The ID of this list, unique within this database';
exec desc_field @table, 'listtype', 'The type of list that this record represents: (1=Report; 2=Timetable)';
exec desc_field @table, 'name', 'The name of this report, unique for this type of report';
exec desc_field @table, 'description', 'A fuller description of this report';
exec desc_field @table, 'relativedate', 'Indicator of a date relative to today: (0=None; 1=Today; 2=Yesterday; 3=Tomorrow)';
exec desc_field @table, 'absolutedate', 'Absolute date for the list';
exec desc_field @table, 'daysdistance', 'The number of days from the specified date which the list should cover. This is regardless of the date itself, ie. a "daysdistance" of zero indicates that the list covers the day only';
exec desc_field @table, 'allschedules', 'Bit indicating if this list should display all schedules'

set @table=N'BPAScheduleListSchedule';
set @desc=N'Defines the schedules which should be reported on as part of the list. '+
    'Note that where no BPAScheduleListSchedule records exist for a list, it is treated as showing details for "all schedules", '+
    'note "no schedules". This allows lists to be defined for all schedules without having to be constantly maintained and updated.';
exec desc_table @table, @desc;
exec desc_field @table, 'schedulelistid', 'The list that this record represents an entry on';
exec desc_field @table, 'scheduleid', 'The schedule which makes up the entry on the parent list';

set @table=N'BPAPasswordRules';
set @desc=N'Defines the rules which passwords must conform to.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'Primary key for replication purposes. As the table has a single row, the value is always 1'
exec desc_field @table, 'uppercase', 'Whether passwords should contain at least one uppercase character';
exec desc_field @table, 'lowercase', 'Whether passwords should contain at least one lowercase character';
exec desc_field @table, 'digits', 'Whether passwords should contain at least one digit character';
exec desc_field @table, 'special', 'Whether passwords should contain at least one special character';
exec desc_field @table, 'brackets', 'Whether passwords should contain at least one bracket character';
exec desc_field @table, 'length', 'The minimum length that the password may be';
exec desc_field @table, 'additional', 'Additional characters that passwords should contain at least one of';
exec desc_field @table, 'norepeats', 'Whether a number of previously used passwords cannot be repeated';
exec desc_field @table, 'norepeatsdays', 'Whether a password used in a past number of days cannot be repeated';
exec desc_field @table, 'numberofrepeats', 'The number of previous passwords which must not be repeated';
exec desc_field @table, 'numberofdays', 'The number of days in which the password must not have been used';

set @table=N'BPAPref';
set @desc=N'Core preference table - holds preference names and scope.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'Unique identifier for a preference';
exec desc_field @table, 'name', 'The name of the preference. Must be unique within a given scope, ie. system-scope or a user-scope for a particular user id.';
exec desc_field @table, 'userid', 'The ID of the user if this preference is in user-scope; NULL if it is system-scope';

set @table=N'BPAIntegerPref';
set @desc=N'Satellite pref table holding integer preferences.';
exec desc_table @table, @desc;
exec desc_field @table, 'prefid', 'The ID of the BPAPref record that this value represents';
exec desc_field @table, 'value', 'The integer value of the preference.';

set @table=N'BPAStringPref';
set @desc=N'Satellite pref table holding string preferences.';
exec desc_table @table, @desc;
exec desc_field @table, 'prefid', 'The ID of the BPAPref record that this value represents';
exec desc_field @table, 'value', 'The string value of the preference.';

set @table=N'BPAEnvLock';
set @desc=N'Environment locking table.';
exec desc_table @table, @desc;
exec desc_field @table, 'name', 'The (unique) name of the lock record. This is used to identify the lock in the locking business object';
exec desc_field @table, 'token', 'The token used to lock the record - null if it is currently free';
exec desc_field @table, 'sessionid', 'The ID of the session which has the record locked';
exec desc_field @table, 'locktime', 'The last date/time that the record was locked';
exec desc_field @table, 'comments', 'The arbitrary comments left on the lock the last time it was acquired / released.';

set @table=N'BPAPackage';
set @desc=N'Table describing a package - a contents list for a release.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The unique ID for an individual package';
exec desc_field @table, 'name', 'The name of the package';
exec desc_field @table, 'description', 'The description of the package.';
exec desc_field @table, 'userid', 'The user who created the package (version).';
exec desc_field @table, 'created', 'The date/time that this package was created.';

set @table=N'BPARelease';
set @desc=N'Table describing a release - an export of a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The unique ID for the release.';
exec desc_field @table, 'packageid', 'The ID of the package which this release was created from';
exec desc_field @table, 'name', 'The name given to this release';
exec desc_field @table, 'created', 'The date/time this release was created on the database'
exec desc_field @table, 'userid', 'The ID of the user who created this record'
exec desc_field @table, 'notes', 'The release notes set on this release';
exec desc_field @table, 'compressedxml', 'Placeholder for the compressed XML of the release - currently unused, but may be utilised in future releases'
exec desc_field @table, 'local', 'Flag indicating if this release was created from a package on this system (1), or imported from another environment (0)'

set @table = N'BPAReleaseEntry'
set @desc = N'Table describing an entry in a release - this is specific to the release and is not dependent on the current state of the components in the database'
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The unique ID for this entry.';
exec desc_field @table, 'releaseid', 'The ID of the release that this entry is a part of.';
exec desc_field @table, 'typekey', 'The type of component that this entry represents.';
exec desc_field @table, 'entityid', 'The ID of the entry in the release - eg. for a process component, this is a string representation of the GUID process ID.';
exec desc_field @table, 'name', 'The name of the component that this entry represents.';

set @table=N'BPAPackageProcess';
set @desc=N'Table linking a process to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a process';
exec desc_field @table, 'processid', 'The ID of the process which forms part of the package';

set @table=N'BPAPackageTile';
set @desc=N'Table linking a tile to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a tile';
exec desc_field @table, 'tileid', 'The ID of the tile which forms part of the package';

set @table=N'BPAPackageDashboard';
set @desc=N'Table linking a dashboard to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a tile';
exec desc_field @table, 'dashid', 'The ID of the dashboard which forms part of the package';

set @table=N'BPAPackageWorkQueue';
set @desc=N'Table linking a work queue to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a queue';
exec desc_field @table, 'queueident', 'The identity of the work queue which forms part of the package';

set @table=N'BPAPackageCredential';
set @desc=N'Table linking a credential to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a credential';
exec desc_field @table, 'credentialid', 'The ID of the credential which forms part of the package';

set @table=N'BPAPackageSchedule';
set @desc=N'Table linking a schedule to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a schedule';
exec desc_field @table, 'scheduleid', 'The ID of the schedule which forms part of the package';

set @table=N'BPAPackageCalendar';
set @desc=N'Table linking a calendar to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a calendar';
exec desc_field @table, 'calendarid', 'The ID of the calendar which forms part of the package';

set @table=N'BPAPackageScheduleList';
set @desc=N'Table linking a schedule list to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a schedule list';
exec desc_field @table, 'schedulelistid', 'The ID of the schedule list which forms part of the package';

set @table=N'BPAPackageWebService';
set @desc=N'Table linking a web service to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a web service';
exec desc_field @table, 'webserviceid', 'The ID of the web service which forms part of the package';

set @table=N'BPAPackageEnvironmentVar';
set @desc=N'Table linking an environment variable to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a environment variable';
exec desc_field @table, 'name', 'The name of the environment variable which forms part of the package';

set @table=N'BPAPackageFont';
set @desc=N'Table linking a font to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a font';
exec desc_field @table, 'name', 'The name of the font which forms part of the package';

set @table=N'BPAValAction';
set @desc=N'Table containing definitions of all Process Validation actions.';
exec desc_table @table, @desc;
exec desc_field @table, 'actionid', 'The ID of the action';
exec desc_field @table, 'description', 'The description of the action - this will be displayed in the user interface';

set @table=N'BPAValActionMap';
set @desc=N'This table maps categories to types and actions.';
exec desc_table @table, @desc;
exec desc_field @table, 'catid', 'The ID of category';
exec desc_field @table, 'typeid', 'The ID of the type';
exec desc_field @table, 'actionid', 'The ID of the action';

set @table=N'BPAValCategory';
set @desc=N'Table containing the definitions of Process Validation categories.';
exec desc_table @table, @desc;
exec desc_field @table, 'catid', 'The ID of the category';
exec desc_field @table, 'description', 'The description of the category - this will be displayed in the user interface';

set @table=N'BPAValCheck';
set @desc=N'Table itemising all known Process Validation checks.';
exec desc_table @table, @desc;
exec desc_field @table, 'checkid', 'The ID of this check';
exec desc_field @table, 'catid', 'The ID of the category the check belongs to';
exec desc_field @table, 'typeid', 'The ID of type of the check';
exec desc_field @table, 'description', 'The description of the check';
exec desc_field @table, 'enabled', 'Determines whether or not this check is enabled';

set @table=N'BPAValType';
set @desc=N'Table containing the definitions of Process Validation check types.';
exec desc_table @table, @desc;
exec desc_field @table, 'typeid', 'The ID of the type.';
exec desc_field @table, 'description', 'The description of the type - this will be displayed in the user interface';

set @table=N'BPAFont';
set @desc=N'Table containing installed font data.';
exec desc_table @table, @desc;
exec desc_field @table, 'name', 'The name of the font.';
exec desc_field @table, 'version', 'The version number of the type. Not used internally.';
exec desc_field @table, 'fontdata', 'The XML describing the characters in the font and their relationships.';

set @table=N'BPAFontOCRPlusPlus';
set @desc=N'Table containing installed OCR Plus font data.';
exec desc_table @table, @desc;
exec desc_field @table, 'name', 'The name of the font.';
exec desc_field @table, 'version', 'The version number of the type. Not used internally.';
exec desc_field @table, 'fontdata', 'The JSON describing the characters in the font and their relationships.';

set @table=N'BPAKeyStore';
set @desc=N'Table containing encryption schemes used to encrypt Credentials and Queue items.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The ID of the encryption scheme.';
exec desc_field @table, 'name', 'The name of the encryption scheme.';
exec desc_field @table, 'location', 'The location of the secret key (0-Database, 1-Server).';
exec desc_field @table, 'isavailable', 'Flag indicating whether or not the scheme can be selected in config.';
exec desc_field @table, 'method', 'The encryption algorithm (1-TripleDES, 2-AES256), or null if key is not held in the database.';
exec desc_field @table, 'encryptkey', 'The secret key used by the algorithm, or null if key is not held in the database.';


set @table=N'BPAEnvironmentVar';
set @desc=N'Table containing environment variables.';
exec desc_table @table, @desc;
exec desc_field @table, 'name', 'The name of the environment variable.';
exec desc_field @table, 'datatype', 'The data type of the environment variable.';
exec desc_field @table, 'value', 'The value of the environment variable.';
exec desc_field @table, 'description', 'The description of the environment variable.';


set @table=N'BPAExceptionType';
set @desc=N'Table containing environment variables.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The ID of the exception type.';
exec desc_field @table, 'type', 'The type of exception.';


set @table=N'BPAGroupGroup';
set @desc=N'Table mapping groups to groups.';
exec desc_table @table, @desc;
exec desc_field @table, 'groupid', 'The ID of the containing group.';
exec desc_field @table, 'memberid', 'The ID of the group.';



set @table=N'BPAGroupQueue';
set @desc=N'Table mapping work queues to groups.';
exec desc_table @table, @desc;
exec desc_field @table, 'groupid', 'The ID of the containing group.';
exec desc_field @table, 'memberid', 'The ID of the work queue.';

set @table=N'BPAGroupResource';
set @desc=N'Table mapping resources to groups.';
exec desc_table @table, @desc;
exec desc_field @table, 'groupid', 'The ID of the containing group.';
exec desc_field @table, 'memberid', 'The ID of the resource.';


set @table=N'BPAGroupUser';
set @desc=N'Table mapping users to groups.';
exec desc_table @table, @desc;
exec desc_field @table, 'groupid', 'The ID of the containing group.';
exec desc_field @table, 'memberid', 'The ID of the user.';


set @table=N'BPAPassword';
set @desc=N'Table which stores current and previous user passwords.';
exec desc_table @table, @desc;
EXEC desc_field @table,N'id',N'The ID of the password.'
EXEC desc_field @table,N'active',N'Indicates if this is the current password (1) or a previous password (0).'
EXEC desc_field @table,N'type',N'The hash type of the password; Legacy = 0'
EXEC desc_field @table,N'hash',N'The hashed password'
EXEC desc_field @table,N'lastuseddate',N'The date the password was last used.'
EXEC desc_field @table,N'salt',N'The salt used as part of the hashing process.'

EXEC desc_field @table,N'userid',N'The ID of the user this password belongs to.'

set @table=N'BPATree';
set @desc=N'Table which stores types of trees.';
exec desc_table @table, @desc;
EXEC desc_field @table,N'id',N'The ID of the tree type.'
EXEC desc_field @table,N'name',N'The name of the tree type.'


set @table=N'BPACacheETags';
set @desc=N'Table which is used to determine when the caches need to be refreshed.';
exec desc_table @table, @desc;
exec desc_field @table, 'key', 'The key of the cache';
exec desc_field @table, 'tag', 'A value generated by the last app server which amended the values stored in the cache. Changes to this value will trigger a cache refresh.';



-- Generic groups

set @table=N'BPAGroup';
set @desc=N'Represents a group containing configuration items, e.g. processes, objects';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The ID of the group';
exec desc_field @table, 'treeid', 'An internal ID of the tree structure that the group belongs to';
exec desc_field @table, 'name', 'The group name';
exec desc_field @table, 'isrestricted', 'Indicates whether or not group level permission restrictions apply'

set @table=N'BPAGroupProcess';
set @desc=N'Represents a proces/object group item';
exec desc_table @table, @desc;
exec desc_field @table, 'groupid', 'The ID of the group';
exec desc_field @table, 'processid', 'The ID of the process/object';

set @table=N'BPAGroupTile';
set @desc=N'Represents a tile group item';
exec desc_table @table, @desc;
exec desc_field @table, 'groupid', 'The ID of the group';
exec desc_field @table, 'tileid', 'The ID of the tile';

set @table=N'BPAGroupUserRolePerm'
set @desc=N'Linking table which holds the sets of group level permissions for each user role.'
exec desc_table @table, @desc
exec desc_field @table, 'groupid', 'The ID of the group. Foreign key of BPAGroup';
exec desc_field @table, 'userroleid', 'The ID of the role that is assigned the permission. Foreign key of BPAUserRole'
exec desc_field @table, 'permid', 'The ID of the permission which is assigned to the group/role. Foreign key of BPAPerm'

-- MI Dashboards
set @table=N'BPATileDataSources';
set @desc=N'Represents a tile data source (stored procedure)';
exec desc_table @table, @desc;
exec desc_field @table, 'spname', 'The name of the stored procedure for this data source';
exec desc_field @table, 'tiletype', 'The types of tile that it is suitable for';
exec desc_field @table, 'helppage', 'The associated help page reference';

set @table=N'BPATile';
set @desc=N'Represents a dashboard tile, and contains the definition';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The ID of the tile';
exec desc_field @table, 'name', 'The tile name';
exec desc_field @table, 'tiletype', 'The tile type';
exec desc_field @table, 'description', 'The tile description';
exec desc_field @table, 'autorefresh', 'The automatic refresh rate (in seconds)';
exec desc_field @table, 'xmlproperties', 'Tile type specific properties (in XML format)';

set @table=N'BPADashboard';
set @desc=N'Represents a dashboard of tiles';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The dashboard ID';
exec desc_field @table, 'name', 'The dashboard name';
exec desc_field @table, 'dashtype', 'Indicates whether the dashboard is Personal/Global';
exec desc_field @table, 'userid', 'The user that the dashboard belongs to (personal dashboard)';
exec desc_field @table, 'sendeveryseconds', 'The frequency of sending dashboard data to DataPipelineInput(in seconds)';
exec desc_field @table, 'lastsent', 'The time when the dashboard data was last sent to DataPipelineInput';

set @table=N'BPADashboardTile';
set @desc=N'Represents a tile on a dashboard';
exec desc_table @table, @desc;
exec desc_field @table, 'dashid', 'The dashboard ID';
exec desc_field @table, 'tileid', 'The tile ID';
exec desc_field @table, 'displayorder', 'The order in which the tile is displayed';
exec desc_field @table, 'width', 'The display width of the tile';
exec desc_field @table, 'height', 'The display height of the tile';

set @table=N'BPAMIControl';
set @desc=N'Configuration settings for MI reporting';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The primary key';
exec desc_field @table, 'mienabled', 'Flag to enable/disable collection of statistics';
exec desc_field @table, 'autorefresh', 'Flag to enable automatic refreshing of MI data via the Blue Prism server';
exec desc_field @table, 'refreshat', 'The time that Blue Prism will refresh the MI statistics';
exec desc_field @table, 'lastrefresh', 'Last date/time that MI statistics were refreshed';
exec desc_field @table, 'refreshinprogress', 'Flag to indicate the MI statistics are being refreshed';
exec desc_field @table, 'dailyfor', 'The number of days to retain daily MI statistics';
exec desc_field @table, 'monthlyfor', 'The number of days to retain monthly MI statistics';

set @table=N'BPMIUtilisationShadow';
set @desc=N'Transient table used to collect information about sessions';
exec desc_table @table, @desc;
exec desc_field @table, 'sessionid', 'Session identifier';
exec desc_field @table, 'resourceid', 'Resource identifier';
exec desc_field @table, 'processid', 'Process identifier';
exec desc_field @table, 'startdatetime', 'Session start date and time';
exec desc_field @table, 'enddatetime', 'Session end date and time';

set @table=N'BPMIUtilisationDaily';
set @desc=N'Daily MI view of overall utilisation';
exec desc_table @table, @desc;
exec desc_field @table, 'reportdate', 'Date';
exec desc_field @table, 'resourceid', 'Resource identifier';
exec desc_field @table, 'processid', 'Process identifier';
exec desc_field @table, 'hr0', 'Utilisation in hour 0 (00:00 to 01:00)';
exec desc_field @table, 'hr1', 'Utilisation in hour 1 (01:00 to 02:00)';
exec desc_field @table, 'hr2', 'Utilisation in hour 2 (02:00 to 03:00)';
exec desc_field @table, 'hr3', 'Utilisation in hour 3 (03:00 to 04:00)';
exec desc_field @table, 'hr4', 'Utilisation in hour 4 (04:00 to 05:00)';
exec desc_field @table, 'hr5', 'Utilisation in hour 5 (05:00 to 06:00)';
exec desc_field @table, 'hr6', 'Utilisation in hour 6 (06:00 to 07:00)';
exec desc_field @table, 'hr7', 'Utilisation in hour 7 (07:00 to 08:00)';
exec desc_field @table, 'hr8', 'Utilisation in hour 8 (08:00 to 09:00)';
exec desc_field @table, 'hr9', 'Utilisation in hour 9 (09:00 to 10:00)';
exec desc_field @table, 'hr10', 'Utilisation in hour 10 (10:00 to 11:00)';
exec desc_field @table, 'hr11', 'Utilisation in hour 11 (11:00 to 12:00)';
exec desc_field @table, 'hr12', 'Utilisation in hour 12 (12:00 to 13:00)';
exec desc_field @table, 'hr13', 'Utilisation in hour 13 (13:00 to 14:00)';
exec desc_field @table, 'hr14', 'Utilisation in hour 14 (14:00 to 15:00)';
exec desc_field @table, 'hr15', 'Utilisation in hour 15 (15:00 to 16:00)';
exec desc_field @table, 'hr16', 'Utilisation in hour 16 (16:00 to 17:00)';
exec desc_field @table, 'hr17', 'Utilisation in hour 17 (17:00 to 18:00)';
exec desc_field @table, 'hr18', 'Utilisation in hour 18 (18:00 to 19:00)';
exec desc_field @table, 'hr19', 'Utilisation in hour 19 (19:00 to 20:00)';
exec desc_field @table, 'hr20', 'Utilisation in hour 20 (20:00 to 21:00)';
exec desc_field @table, 'hr21', 'Utilisation in hour 21 (21:00 to 22:00)';
exec desc_field @table, 'hr22', 'Utilisation in hour 22 (22:00 to 23:00)';
exec desc_field @table, 'hr23', 'Utilisation in hour 23 (23:00 to 00:00)';

set @table=N'BPMIUtilisationMonthly';
set @desc=N'Monthly MI view of overall utilisation';
exec desc_table @table, @desc;
exec desc_field @table, 'reportyear', 'Year';
exec desc_field @table, 'reportmonth', 'Month';
exec desc_field @table, 'resourceid', 'Resource identifier';
exec desc_field @table, 'processid', 'Process identifier';
exec desc_field @table, 'hr0', 'Utilisation in hour 0 (00:00 to 01:00)';
exec desc_field @table, 'hr1', 'Utilisation in hour 1 (01:00 to 02:00)';
exec desc_field @table, 'hr2', 'Utilisation in hour 2 (02:00 to 03:00)';
exec desc_field @table, 'hr3', 'Utilisation in hour 3 (03:00 to 04:00)';
exec desc_field @table, 'hr4', 'Utilisation in hour 4 (04:00 to 05:00)';
exec desc_field @table, 'hr5', 'Utilisation in hour 5 (05:00 to 06:00)';
exec desc_field @table, 'hr6', 'Utilisation in hour 6 (06:00 to 07:00)';
exec desc_field @table, 'hr7', 'Utilisation in hour 7 (07:00 to 08:00)';
exec desc_field @table, 'hr8', 'Utilisation in hour 8 (08:00 to 09:00)';
exec desc_field @table, 'hr9', 'Utilisation in hour 9 (09:00 to 10:00)';
exec desc_field @table, 'hr10', 'Utilisation in hour 10 (10:00 to 11:00)';
exec desc_field @table, 'hr11', 'Utilisation in hour 11 (11:00 to 12:00)';
exec desc_field @table, 'hr12', 'Utilisation in hour 12 (12:00 to 13:00)';
exec desc_field @table, 'hr13', 'Utilisation in hour 13 (13:00 to 14:00)';
exec desc_field @table, 'hr14', 'Utilisation in hour 14 (14:00 to 15:00)';
exec desc_field @table, 'hr15', 'Utilisation in hour 15 (15:00 to 16:00)';
exec desc_field @table, 'hr16', 'Utilisation in hour 16 (16:00 to 17:00)';
exec desc_field @table, 'hr17', 'Utilisation in hour 17 (17:00 to 18:00)';
exec desc_field @table, 'hr18', 'Utilisation in hour 18 (18:00 to 19:00)';
exec desc_field @table, 'hr19', 'Utilisation in hour 19 (19:00 to 20:00)';
exec desc_field @table, 'hr20', 'Utilisation in hour 20 (20:00 to 21:00)';
exec desc_field @table, 'hr21', 'Utilisation in hour 21 (21:00 to 22:00)';
exec desc_field @table, 'hr22', 'Utilisation in hour 22 (22:00 to 23:00)';
exec desc_field @table, 'hr23', 'Utilisation in hour 23 (23:00 to 00:00)';

set @table=N'BPMIProductivityShadow';
set @desc=N'Transient table used to collect information about work queue events';
exec desc_table @table, @desc;
exec desc_field @table, 'ident', 'Unique primary key';
exec desc_field @table, 'eventdatetime', 'Event date and time';
exec desc_field @table, 'queueident', 'Work queue identifier';
exec desc_field @table, 'itemid', 'Work item identifier';
exec desc_field @table, 'eventid', 'Event identifier';
exec desc_field @table, 'worktime', 'The total amount of time, in seconds, that has been spent working on this item';
exec desc_field @table, 'elapsedtime', 'The total amount of time, in seconds, from creation to completion';
exec desc_field @table, 'attempt', 'The attempt number';
exec desc_field @table, 'statewhendeleted', 'The state of the item at the point it was deleted';

set @table=N'BPMIProductivityDaily';
set @desc=N'Daily MI view of overall work queue productivity';
exec desc_table @table, @desc;
exec desc_field @table, 'reportdate', 'Date';
exec desc_field @table, 'queueident', 'Work queue identifier';
exec desc_field @table, 'created', 'Number of items created';
exec desc_field @table, 'deferred', 'Number of items deferred';
exec desc_field @table, 'retried', 'Number of items retried';
exec desc_field @table, 'exceptioned', 'Number of exceptions';
exec desc_field @table, 'completed', 'Number of items completed';
exec desc_field @table, 'minworktime', 'Minimum item work time (for completed items)';
exec desc_field @table, 'avgworktime', 'Average item work time (for completed items)';
exec desc_field @table, 'maxworktime', 'Maximum item work time (for completed items)';
exec desc_field @table, 'minelapsedtime', 'Minimum elapsed time (from creation to completion)';
exec desc_field @table, 'avgelapsedtime', 'Average elapsed time (from creation to completion)';
exec desc_field @table, 'maxelapsedtime', 'Maximum elapsed time (from creation to completion)';
exec desc_field @table, 'minretries', 'Minimum number of retries (for completed items)';
exec desc_field @table, 'avgretries', 'Average number of retries (for completed items)';
exec desc_field @table, 'maxretries', 'Maximum number of retries (for completed items)';

set @table=N'BPMIProductivityMonthly';
set @desc=N'Monthly MI view of overall work queue productivity';
exec desc_table @table, @desc;
exec desc_field @table, 'reportyear', 'Year';
exec desc_field @table, 'reportmonth', 'Month';
exec desc_field @table, 'queueident', 'Work queue identifier';
exec desc_field @table, 'created', 'Number of items created';
exec desc_field @table, 'deferred', 'Number of items deferred';
exec desc_field @table, 'retried', 'Number of items retried';
exec desc_field @table, 'exceptioned', 'Number of exceptions';
exec desc_field @table, 'completed', 'Number of items completed';
exec desc_field @table, 'minworktime', 'Minimum item work time (for completed items)';
exec desc_field @table, 'avgworktime', 'Average item work time (for completed items)';
exec desc_field @table, 'maxworktime', 'Maximum item work time (for completed items)';
exec desc_field @table, 'minelapsedtime', 'Minimum elapsed time (from creation to completion)';
exec desc_field @table, 'avgelapsedtime', 'Average elapsed time (from creation to completion)';
exec desc_field @table, 'maxelapsedtime', 'Maximum elapsed time (from creation to completion)';
exec desc_field @table, 'minretries', 'Minimum number of retries (for completed items)';
exec desc_field @table, 'avgretries', 'Average number of retries (for completed items)';
exec desc_field @table, 'maxretries', 'Maximum number of retries (for completed items)';

-- Web APIs

set @table=N'BPAPackageWebApi';
set @desc=N'Table linking a web api service to a package.';
exec desc_table @table, @desc;
exec desc_field @table, 'packageid', 'The ID of the package which contains a web api service';
exec desc_field @table, 'webapiid', 'The ID of the web api service which forms part of the package';
        
set @table=N'BPAProcessWebApiDependency';
set @desc=N'This table tracks dependencies between this object/process and web api services that it references';
exec desc_table @table, @desc;
exec desc_field @table,'id','The technical primary key for the table';
exec desc_field @table,'ProcessID','The unique identifier of the process';
exec desc_field @table,'refApiName','The name of the referenced web api';

set @table=N'BPAWebApiService';
set @desc=
    N'This table stores details of external web apis which have been configured for use via the System Manager interface and are '+
    N'available to be used from within processes.';
exec desc_table @table, @desc;
exec desc_field @table, 'serviceid','A unique ID for the web api';
exec desc_field @table, 'name','Descriptive name for the web api.';
exec desc_field @table, 'enabled','The web api cannot be called if this is False, thereby allowing a system admin to temporarily disable access to a web api without deleting the information.';
exec desc_field @table, 'lastupdated','The date and time the web api was last updated';
exec desc_field @table, 'baseurl','The base address of the web api, which can contain common parameter values in the form [parameter name]';
exec desc_field @table, 'authenticationtype','The type of authentication used to authenticate requests to the api' ;
exec desc_field @table, 'authenticationconfig','xml describing the authentication configured for the web api, including auth type/auth server/pre authorization settings etc.';
exec desc_field @table, 'commoncodeproperties','xml describing the common properties configured for custom code within this web api, including language, settings, references, imports and shared functionality';
exec desc_field @table, 'httpRequestConnectionTimeout','The number of seconds before the http request will time out';
exec desc_field @table, 'authServerRequestConnectionTimeout','The number of seconds before the request to the Authentication Server will time out';
    
set @table=N'BPAWebApiParameter';
set @desc=
       N'Table containing parameters configured for use in a web api, either across all actions or for individual actions.' +
       N'Parameters can be used in headers, request bodies, url paths when specified in the format [parameter name].';
exec desc_table @table, @desc;
exec desc_field @table, 'parameterid', 'A unique ID for the parameter';
exec desc_field @table, 'serviceid', 'The ID of the web api this parameter is configured for use with as a common parameter, or null if this is an action parameter';
exec desc_field @table, 'actionid', 'The ID of the web api action this parameter is configured for use with, or null if this is a common parameter';
exec desc_field @table, 'name', 'The name of the parameter';
exec desc_field @table, 'description', 'A description of the parameter';
exec desc_field @table, 'exposetoprocess', 'Whether the parameter is exposed to the process using the api, so that its value can be set within the process';
exec desc_field @table, 'datatype', 'The datatype of the parameter';
exec desc_field @table, 'initvalue', 'The initial value assigned to the parameter';


set @table=N'BPAWebApiHeader';
set @desc=
       N'Table containing http request headers configured for use with a web api, either across all actions or an individual action.';
exec desc_table @table, @desc;
exec desc_field @table, 'headerid', 'A unique identifier for the header.';
exec desc_field @table, 'serviceid', 'The ID of the web api this header is configured for use with (common headers), or null if this is an action specific header';
exec desc_field @table, 'actionid', 'The ID of the web api action this header is configured for use with, or null if this is a common header';
exec desc_field @table, 'name', 'The name of the header';
exec desc_field @table, 'value', 'The value of the header to be passed with the http request. Header values can contain common and/or action parameters when specified in the format [parameter name]';


set @table=N'BPAWebApiAction';
set @desc=N'Table containing details of specific actions of a web api which have been configured for use from within a process';
exec desc_table @table, @desc;
exec desc_field @table, 'actionid', 'A unique identifier for the api action';
exec desc_field @table, 'serviceid', 'The ID of the web api which this action is configured for use with';
exec desc_field @table, 'name', 'The name of the configured action';
exec desc_field @table, 'description', 'A description of the configured action';
exec desc_field @table, 'enabled', 'Whether the action is enabled for use from within a process/object';
exec desc_field @table, 'requesthttpmethod', 'The http request method used within this action';
exec desc_field @table, 'requesturlpath', 'The specific url path for the action which is concatenated onto the base url of the api when making the request';
exec desc_field @table, 'requestbodytypeid', 'A number specifying the type of the http request body, mapped to the WebApiRequestBodyType enumeration';
exec desc_field @table, 'requestbodycontent', 'xml containing the configured request body content to be sent with the action';
exec desc_field @table, 'enableRequestOutputParameter', 'If true the action will include an output parameter which is populated with the http request data for that action when used within a process/object. Useful for checking/troubleshooting configuration.';
exec desc_field @table, 'disableSendingOfRequest', 'If true the actual sending of the http request will be disabled within a process/object. When used with the request output parameter this can be used to prevent attempting to send the http request and getting an exception when trying to troubleshoot configuration.';
exec desc_field @table,  'outputparametercode', 'Contains any custom code which has been configured to populate output parameters from the http response';

set @table=N'BPAWebApiCustomOutputParameter';
set @desc=N'Table containing custom output parameters for use with api actions, defining a path to each value within the response content and its datatype';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'A unique identifier for the output parameter';
exec desc_field @table, 'actionid', 'The ID of the web api action which this output parameter is configured for use with';      
exec desc_field @table, 'name', 'The name of the output parameter';
exec desc_field @table, 'path', 'The path to the value for this output parameter within the response data';
exec desc_field @table, 'datatype', 'The datatype of the output data';
exec desc_field @table, 'outputparametertype', 'The type of the output parameter; json path or cusomt code';    
exec desc_field @table, 'description', 'A description of the output parameter';

set @table=N'BPASysWebConnectionSettings';
set @desc=
       N'Table containing default values for the advanced settings which can be configured to apply to service points when making web requests.' + 
       N'The defaults apply to any service point which does not have a specific uri configuration.';
exec desc_table @table, @desc;
exec desc_field @table, 'maxidletime', 'Default maximum number of seconds a connection can remain idle before it is closed. This default is applied where no uri configuration is in place for a specific service point.';
exec desc_field @table, 'connectionlimit', 'The default number of connections per service point. This default is applied where no uri configuration is in place for a specific service point.';


set @table=N'BPASysWebUrlSettings';
set @desc=N'Table containing advanced settings which can be configured to apply to service points when making web requests. The uri specific settings override any default settings configured.';
exec desc_table @table, @desc;
exec desc_field @table, 'baseuri', 'The base uri of the service point the settings will apply to';
exec desc_field @table, 'connectionlimit', 'The number of connections per service point';
exec desc_field @table, 'connectiontimeout', 'The number of seconds after which an active connection is closed. Can be left blank if not required.';
exec desc_field @table, 'maxidletime', 'The maximum number of seconds a connection can remain idle before it is closed';


-- ****************** VIEWS *********************
-- Ensure that desc_table and desc_field have the optional 'object_type'
-- param set to 'VIEW' for this to work correctly

set @view=N'BPViewWorkQueueItemTag'
set @desc=N'A view which brings together all the tags (including virtual tags) assigned to a work item'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'queueitemident', 'The identity of the work item to which the tag is assigned', 'VIEW'
exec desc_field @view, 'tag', 'The tag which is assigned to the work item', 'VIEW'

set @view=N'BPViewWorkQueueItemTagBare'
set @desc=N'A view which brings together all the tags assigned to a work item, excluding virtual tags'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'queueitemident', 'The identity of the work item to which the tag is assigned', 'VIEW'
exec desc_field @view, 'tag', 'The tag which is assigned to the work item', 'VIEW'

set @view=N'BPVWorkQueueItem'
set @desc=N'A view which provides data on work items in the form that it was before the separate locked table'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view,'ident','The IDENTITY field for this queue item', 'VIEW'
exec desc_field @view,'id','The globally unique ID for this queue item', 'VIEW'
exec desc_field @view,'queueid','The GUID of the BPAWorkQueue record, which owns this item', 'VIEW'
exec desc_field @view,'queueident','The foreign key identifying the queue containing this item', 'VIEW'
exec desc_field @view,'sessionid ','The last session which updated this queue item', 'VIEW'
exec desc_field @view,'keyvalue','A key, used to find specific data - e.g. an account number', 'VIEW'
exec desc_field @view,'status','A field that can be used by the process to record how much of the case it has completed', 'VIEW'
exec desc_field @view,'attempt','The attempt number of this item - starts at 1 for the first attempt and increments for each retry clone created.', 'VIEW'
exec desc_field @view,'loaded','Timestamp of when the case was loaded into the queue', 'VIEW'
exec desc_field @view,'locked','Timestamp of when the case was locked on retrieval from the queue. If null, it is not locked.', 'VIEW'
exec desc_field @view,'completed','Timestamp of when the case was completed. If null, it has not been completed.', 'VIEW'
exec desc_field @view,'exception','Timestamp of when the case was abandonded due to an exception. Null if it has never happened.', 'VIEW'
exec desc_field @view,'exceptionreason','Reason for last exception', 'VIEW'
exec desc_field @view,'exceptionreasontag', 'tag of exceptionreason', 'VIEW'
exec desc_field @view,'deferred','Timestamp of when this case is deferred until. Null if it isn''t.', 'VIEW'
exec desc_field @view,'worktime','The total amount of time, in seconds, that has been spent working on this item (including time spent on prior attempts)', 'VIEW'
exec desc_field @view,'data','The data associated with the queue item. This is in the form of an XML-encoded collection, which is passed to the internal business object when the item is loaded into the queue, and passed back out when the item is retrieved from the queue.', 'VIEW'
exec desc_field @view,'priority','The priority of the item. Lower numbers are higher priority.', 'VIEW'
exec desc_field @view,'lastupdated', 'Date that the item last changed - ie. loaded or finished. Deterministic calculated column', 'VIEW'
exec desc_field @view,'queuepositiondate', 'Date/time determining the item''s position in the queue. Non-deterministic calculated column', 'VIEW'
exec desc_field @view,'exceptionreasonvarchar', 'varchar of exceptionreason to allow ordering and filtering on it. Deterministic calculated column', 'VIEW'
exec desc_field @view,'prevworktime', 'The total amount of worktime for all attempts on the item prior this one', 'VIEW'
exec desc_field @view,'attemptworktime', 'The amount of worktime spent on this attempt of the item only. Deterministic calculated column', 'VIEW'
exec desc_field @view,'finished', 'The date the item was finished (ie. exceptioned or completed), null if it is not finished. Deterministic calculated column.', 'VIEW'
exec desc_field @view,'encryptid','The ID of the encryption scheme used to encrypt this item''s data', 'VIEW'
exec desc_field @view,'state','The current state of the item, e.g. Complete, Deferred, Locked etc.','VIEW'


set @view=N'BPVAnnotatedScheduleLog'
set @desc=N'A view which provides data on triggered schedules.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'id', 'The Schedule log id', 'VIEW'
exec desc_field @view, 'scheduleid', 'The ID of the schedule', 'VIEW'
exec desc_field @view, 'firereason', 'The reason the schedule was triggered.', 'VIEW'
exec desc_field @view, 'instancetime', 'The time the schedule was triggered', 'VIEW'
exec desc_field @view, 'servername', 'The name of the app server', 'VIEW'
exec desc_field @view, 'heartbeat', 'Last time that the schedule log was pulsed, indicating that the schedule is still being executed.', 'VIEW'
exec desc_field @view, 'starttime', 'The time the scheduled session started.', 'VIEW'
exec desc_field @view, 'endtime', 'The time the scheduled session ended.', 'VIEW'
exec desc_field @view, 'endtype', 'The type of log entry that this schedule log ended with: (1=Schedule Started; 2=Schedule Completed; 3=Schedule Terminated; 4=Task Started; 5=Task Completed; 6=Task Terminated; 7=Session Started; 8=Session Completed; 9=Session Terminated)', 'VIEW'
exec desc_field @view, 'endreason', 'Reason for the schedule instance to end.', 'VIEW'


set @view=N'BPVGroupedActiveObjects'
set @desc=N'A view which provides data on business objects contained in groups, and which are not retired.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the business object', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the business object', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the business object.', 'VIEW'
exec desc_field @view, 'id', 'The ID of the business object', 'VIEW'
exec desc_field @view, 'name', 'The name of the business object', 'VIEW'
exec desc_field @view, 'processtype', 'O = Object.', 'VIEW'
exec desc_field @view, 'description', 'Description of the business object', 'VIEW'
exec desc_field @view, 'createddate', 'The time the business object was created.', 'VIEW'
exec desc_field @view, 'createdby', 'The name of the user which created the business object', 'VIEW'
exec desc_field @view, 'lastmodifieddate', 'The date the business object was last modified.', 'VIEW'
exec desc_field @view, 'attributes', 'Records what attributes the process has as independent bitwise attributes with values 1,2,4,8, etc. Maps to BPAProcessAttribute.', 'VIEW'
exec desc_field @view, 'lastmodifiedby', 'Name of the user which last modified the business object', 'VIEW'

set @view=N'BPVGroupedObjects'
set @desc=N'A view which provides data on business objects contained in groups.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the business object', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the business object', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the business object.', 'VIEW'
exec desc_field @view, 'id', 'The ID of the business object', 'VIEW'
exec desc_field @view, 'name', 'The name of the business object', 'VIEW'
exec desc_field @view, 'processtype', 'O = Object.', 'VIEW'
exec desc_field @view, 'description', 'Description of the business object', 'VIEW'
exec desc_field @view, 'createddate', 'The time the business object was created.', 'VIEW'
exec desc_field @view, 'createdby', 'The name of the user which created the business object', 'VIEW'
exec desc_field @view, 'lastmodifieddate', 'The date the business object was last modified.', 'VIEW'
exec desc_field @view, 'attributes', 'Records what attributes the process has as independent bitwise attributes with values 1,2,4,8, etc. Maps to BPAProcessAttribute.', 'VIEW'
exec desc_field @view, 'lastmodifiedby', 'Name of the user which last modified the business object', 'VIEW'
exec desc_field @view, 'lockdatetime', 'If locked by a user, the time the business object was locked.', 'VIEW'
exec desc_field @view, 'lockuser', 'If locked by a user, the name of the user which has locked this process.', 'VIEW'
exec desc_field @view, 'lockmachinename', 'If locked by a user, the name of the machine which the user locked it from.', 'VIEW'
exec desc_field @view, 'webservicename', 'If exposed as a web service, this is the web service name', 'VIEW'
exec desc_field @view, 'forceDocumentLiteral', 'If exposed as a web service, determines the encoding format. 0 = Rpc encoding. 1 = Document/literal encoding format', 'VIEW'
exec desc_field @view, 'useLegacyNamespace', 'If exposed as a web service, determines whether to use legacy namespace structure.', 'VIEW'
exec desc_field @view, 'sharedObject', 'Indicates that this object has global scope at runtime and can share it''s application model (not set for model sharing child objects)', 'VIEW'


set @view=N'BPVGroupedActiveProcesses'
set @desc=N'A view which provides data on processes contained in groups, and which are not retired.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the processes', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the processes', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the processes.', 'VIEW'
exec desc_field @view, 'id', 'The ID of the processes', 'VIEW'
exec desc_field @view, 'name', 'The name of the processes', 'VIEW'
exec desc_field @view, 'processtype', 'P = Process.', 'VIEW'
exec desc_field @view, 'description', 'Description of the processes', 'VIEW'
exec desc_field @view, 'createddate', 'The time the processes was created.', 'VIEW'
exec desc_field @view, 'createdby', 'The name of the user which created the processes', 'VIEW'
exec desc_field @view, 'lastmodifieddate', 'The date the processes was last modified.', 'VIEW'
exec desc_field @view, 'attributes', 'Records what attributes the process has as independent bitwise attributes with values 1,2,4,8, etc. Maps to BPAProcessAttribute.', 'VIEW'
exec desc_field @view, 'lastmodifiedby', 'Name of the user which last modified the processes', 'VIEW'

set @view=N'BPVGroupedProcesses'
set @desc=N'A view which provides data on processes contained in groups.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the processes', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the processes', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the processes.', 'VIEW'
exec desc_field @view, 'id', 'The ID of the processes', 'VIEW'
exec desc_field @view, 'name', 'The name of the processes', 'VIEW'
exec desc_field @view, 'processtype', 'P = Process.', 'VIEW'
exec desc_field @view, 'description', 'Description of the processes', 'VIEW'
exec desc_field @view, 'createddate', 'The time the processes was created.', 'VIEW'
exec desc_field @view, 'createdby', 'The name of the user which created the processes', 'VIEW'
exec desc_field @view, 'lastmodifieddate', 'The date the processes was last modified.', 'VIEW'
exec desc_field @view, 'attributes', 'Records what attributes the process has as independent bitwise attributes with values 1,2,4,8, etc. Maps to BPAProcessAttribute.', 'VIEW'
exec desc_field @view, 'lastmodifiedby', 'Name of the user which last modified the processes', 'VIEW'
exec desc_field @view, 'lockdatetime', 'If locked by a user, the time the processes was locked.', 'VIEW'
exec desc_field @view, 'lockuser', 'If locked by a user, the name of the user which has locked this process.', 'VIEW'
exec desc_field @view, 'lockmachinename', 'If locked by a user, the name of the machine which the user locked it from.', 'VIEW'
exec desc_field @view, 'webservicename', 'If exposed as a web service, this is the web service name', 'VIEW'
exec desc_field @view, 'forceDocumentLiteral', 'If exposed as a web service, determines the encoding format. 0 = Rpc encoding. 1 = Document/literal encoding format', 'VIEW'
exec desc_field @view, 'useLegacyNamespace', 'If exposed as a web service, determines whether to use legacy namespace structure.', 'VIEW'
exec desc_field @view, 'sharedObject', 'Indicates that this object has global scope at runtime and can share it''s application model (not set for model sharing child objects)', 'VIEW'



set @view=N'BPVGroupedProcessesObjects'
set @desc=N'A view which provides data on processes and business objects contained in groups.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the process / business object', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the process / business object', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the process / business object.', 'VIEW'
exec desc_field @view, 'id', 'The ID of the process / business object', 'VIEW'
exec desc_field @view, 'name', 'The name of the process / business object', 'VIEW'
exec desc_field @view, 'processtype', 'P = process O = Object.', 'VIEW'
exec desc_field @view, 'description', 'Description of the process / business object', 'VIEW'
exec desc_field @view, 'createddate', 'The time the process / business object was created.', 'VIEW'
exec desc_field @view, 'createdby', 'The name of the user which created the ocess / business object', 'VIEW'
exec desc_field @view, 'lastmodifieddate', 'The date the process / business object was last modified.', 'VIEW'
exec desc_field @view, 'attributes', 'Records what attributes the process has as independent bitwise attributes with values 1,2,4,8, etc. Maps to BPAProcessAttribute.', 'VIEW'
exec desc_field @view, 'lastmodifiedby', 'Name of the user which last modified the ocess / business object', 'VIEW'
exec desc_field @view, 'lockdatetime', 'If locked by a user, the time the process / business object was locked.', 'VIEW'
exec desc_field @view, 'lockuser', 'If locked by a user, the name of the user which has locked this process.', 'VIEW'
exec desc_field @view, 'lockmachinename', 'If locked by a user, the name of the machine which the user locked it from.', 'VIEW'
exec desc_field @view, 'webservicename', 'If exposed as a web service, this is the web service name', 'VIEW'
exec desc_field @view, 'forceDocumentLiteral', 'If exposed as a web service, determines the encoding format. 0 = Rpc encoding. 1 = Document/literal encoding format', 'VIEW'
exec desc_field @view, 'useLegacyNamespace', 'If exposed as a web service, determines whether to use legacy namespace structure.', 'VIEW'
exec desc_field @view, 'sharedObject', 'Indicates that this object has global scope at runtime and can share it''s application model (not set for model sharing child objects)', 'VIEW'



set @view=N'BPVGroupedPublishedProcesses'
set @desc=N'A view which provides data on published processes contained in groups.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the processes', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the processes', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the processes.', 'VIEW'
exec desc_field @view, 'id', 'The ID of the processes', 'VIEW'
exec desc_field @view, 'name', 'The name of the processes', 'VIEW'
exec desc_field @view, 'processtype', 'P = Process.', 'VIEW'
exec desc_field @view, 'description', 'Description of the processes', 'VIEW'
exec desc_field @view, 'createddate', 'The time the processes was created.', 'VIEW'
exec desc_field @view, 'createdby', 'The name of the user which created the processes', 'VIEW'
exec desc_field @view, 'lastmodifieddate', 'The date the processes was last modified.', 'VIEW'
exec desc_field @view, 'attributes', 'Records what attributes the process has as independent bitwise attributes with values 1,2,4,8, etc. Maps to BPAProcessAttribute.', 'VIEW'
exec desc_field @view, 'lastmodifiedby', 'Name of the user which last modified the processes', 'VIEW'


set @view=N'BPVGroupedGroups'
set @desc=N'A view which provides data on groups contained in groups.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the group', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the group', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the group.', 'VIEW'
exec desc_field @view, 'id', 'The ID of the group', 'VIEW'
exec desc_field @view, 'name', 'The name of the group', 'VIEW'


set @view=N'BPVGroupedQueues'
set @desc=N'A view which provides data on work queues contained in groups.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the queue', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the queue', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the queue.', 'VIEW'
exec desc_field @view, 'id', 'The internal ID of the queue', 'VIEW'
exec desc_field @view, 'name', 'The name of the group', 'VIEW'
exec desc_field @view, 'guid', 'The ID of the queue', 'VIEW'
exec desc_field @view, 'running', 'If the queue is running.', 'VIEW'
exec desc_field @view,'encryptid','The ID of the encryption scheme used to encrypt the item data for items on this queue - NULL indicates that the queue is not encrypted', 'VIEW'
exec desc_field @view,'processid','The process used to work an active queue; null for non-active queues', 'VIEW'
exec desc_field @view, 'resourcegroupid', 'The group of resources assigned to an active queue; null for non-active queues', 'VIEW'
exec desc_field @view, 'isactive', 'If the queue is an active queue.', 'VIEW'



set @view=N'BPVGroupedResources'
set @desc=N'A view which provides data on resources contained in groups.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the resource', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the resource', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the resource.', 'VIEW'
exec desc_field @view, 'id', 'The internal ID of the resource', 'VIEW'
exec desc_field @view, 'name', 'The name of the group', 'VIEW'
exec desc_field @view, 'attributes', 'Attributes possessed by this resource as independent bitwise attributes (1,2,4,8, etc). Maps to BPAResourceAttribute', 'VIEW'
exec desc_field @view, 'ispoolmember', 'If the resource is a member of a resource pool.', 'VIEW'
exec desc_field @view, 'statusid','Status of the resource.', 'VIEW'


set @view=N'BPVGroupedTiles'
set @desc=N'A view which provides data on tiles contained in groups.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the tile', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the tile', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the tile.', 'VIEW'
exec desc_field @view, 'id', 'The ID of the tile', 'VIEW'
exec desc_field @view, 'name', 'The name of the tile', 'VIEW'
exec desc_field @view, 'tiletype', 'Currently only supports a value of 1 = Chart', 'VIEW'
exec desc_field @view, 'description', 'Tile description', 'VIEW'


set @view=N'BPVGroupedUsers'
set @desc=N'A view which provides data on users contained in groups.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the user', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the user', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the user.', 'VIEW'
exec desc_field @view, 'id', 'The ID of the user', 'VIEW'
exec desc_field @view, 'name', 'The name of the user', 'VIEW'
exec desc_field @view, 'authtype', 'The authentication type used by the user', 'VIEW'
exec desc_field @view, 'validfrom','The date that the username is valid from', 'VIEW'
exec desc_field @view, 'validto','The date the username becomes invalid', 'VIEW'
exec desc_field @view, 'passwordexpiry', 'The date/time (UTC) the password expires and needs renewing.', 'VIEW'
exec desc_field @view, 'lastsignedin', 'The date/time (UTC) the user last signed in.', 'VIEW'
exec desc_field @view, 'isdeleted', 'Records whether a user has been deleted. Defaults to 0.', 'VIEW'
exec desc_field @view, 'loginattempts', 'The number of failed login attempts that the user has made.', 'VIEW'
exec desc_field @view, 'maxloginattempts', 'The maximum number of login attempts that a user can make before being locked out.', 'VIEW'
exec desc_field @view, 'roleid', 'The ID of the role in the BPAUserRole table which is assigned to this user.', 'VIEW'



set @view=N'BPVPools'
set @desc=N'A view which provides data on resource pools.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'treeid', 'The ID of the tree from the BPATree table which contains the resource pool', 'VIEW'
exec desc_field @view, 'groupid', 'The ID of the group in the BPAGroup table which contains the resource pool', 'VIEW'
exec desc_field @view, 'groupname', 'The name of the group which contains the resource pool.', 'VIEW'
exec desc_field @view, 'id', 'The ID of the resource pool', 'VIEW'
exec desc_field @view, 'name', 'The name of the resource pool', 'VIEW'
exec desc_field @view, 'attributes', 'Attributes possessed by this resource pool as independent bitwise attributes (1,2,4,8, etc). Maps to BPAResourceAttribute', 'VIEW'
exec desc_field @view, 'statusid', 'The status of the resource pool', 'VIEW'


set @view=N'BPVScriptEnvironment'
set @desc=N'A view which is used internally by blue prism database upgrade scripts to determine if there is a new install or upgrade in progress.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'InstallInProgress', '1 = a new database install in progress, 0 = database upgrade.', 'VIEW'


set @view=N'BPVSession'
set @desc=N'A view which provides data on sessions.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'sessionid', 'The unique session Id that represents that run of the process - automatically created', 'VIEW'
exec desc_field @view, 'sessionnumber', 'ID number used to make a relation with the log table.', 'VIEW'
exec desc_field @view, 'startdatetime', 'The start datetime of the session', 'VIEW'
exec desc_field @view, 'enddatetime', 'The end datetime of the session', 'VIEW'
exec desc_field @view, 'processid', 'The unique identifier for the process run', 'VIEW'
exec desc_field @view, 'starterresourceid', 'The resource id the process was started from.', 'VIEW'
exec desc_field @view, 'starteruserid', 'The unique identifier of the user who started the process', 'VIEW'
exec desc_field @view, 'runningresourceid', 'The resourceid that the process is running on.', 'VIEW'
exec desc_field @view, 'runningosusername', 'The username used to log on to the operating system of the pc that the process is running on', 'VIEW'
exec desc_field @view, 'statusid', 'The unique identifier of the session status, i.e Running, Stopped by user, Crashed, Finished Successfully', 'VIEW'
exec desc_field @view, 'startparamsxml', 'The XML that represents the start parameters of the process. This is used by Control Room to maintain state when creating sessions, and is also set when a process is started, so is guaranteed to contain the parameters used to start the session, once it has been started.', 'VIEW'
exec desc_field @view, 'logginglevelsxml', 'This field is obsolete.', 'VIEW'
exec desc_field @view, 'sessionstatexml', 'This field is obsolete.', 'VIEW'
exec desc_field @view, 'queueid', 'The queue that this resource is currently assigned to', 'VIEW'
exec desc_field @view, 'stoprequested', 'The (UTC) date/time when a stop request was made for a session', 'VIEW'
exec desc_field @view, 'stoprequestack', 'The (UTC) date/time when a stop request was queried by a process using the appropriate function', 'VIEW'
exec desc_field @view, 'lastupdated', 'The datetime when the last stage was started', 'VIEW'
exec desc_field @view, 'laststage', 'The last stage that was started', 'VIEW'
exec desc_field @view, 'warningthreshold', 'The number of seconds since the last updated time before the session goes into a warning state.', 'VIEW'
exec desc_field @view, 'starttimezoneoffset', 'The start timezone offset of the session', 'VIEW'
exec desc_field @view, 'endtimezoneoffset', 'The end timezone offset of the session', 'VIEW'
exec desc_field @view, 'lastupdatedtimezoneoffset', 'The lastupdated timezone offset', 'VIEW'

set @view=N'BPVSessionInfo'
set @desc=N'A view which provides information on sessions.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'sessionid', 'The unique session Id that represents that run of the process - automatically created', 'VIEW'
exec desc_field @view, 'sessionnumber', 'ID number used to make a relation with the log table.', 'VIEW'
exec desc_field @view, 'startdatetime', 'The start datetime of the session', 'VIEW'
exec desc_field @view, 'starttimezoneoffset', 'The start timezone offset of the session', 'VIEW'
exec desc_field @view, 'enddatetime', 'The end datetime of the session', 'VIEW'
exec desc_field @view, 'endtimezoneoffset', 'The end timezone offset of the session', 'VIEW'
exec desc_field @view, 'processid', 'The unique identifier for the process run', 'VIEW'
exec desc_field @view, 'processname', 'The name for the process run', 'VIEW'
exec desc_field @view, 'starterresourceid', 'The resource id the process was started from.', 'VIEW'
exec desc_field @view, 'starterresourcename', 'The name of the resource the process was started from.', 'VIEW'
exec desc_field @view, 'starteruserid', 'The unique identifier of the user who started the process', 'VIEW'
exec desc_field @view, 'starterusername', 'The username of the user who started the process', 'VIEW'
exec desc_field @view, 'runningresourceid', 'The resourceid that the process is running on.', 'VIEW'
exec desc_field @view, 'runningresourcename', 'The name of the resource that the process is running on.', 'VIEW'
exec desc_field @view, 'runningosusername', 'The username used to log on to the operating system of the pc that the process is running on', 'VIEW'
exec desc_field @view, 'statusid', 'The unique identifier of the session status, i.e Running, Stopped by user, Crashed, Finished Successfully', 'VIEW'
exec desc_field @view, 'startparamsxml', 'The XML that represents the start parameters of the process. This is used by Control Room to maintain state when creating sessions, and is also set when a process is started, so is guaranteed to contain the parameters used to start the session, once it has been started.', 'VIEW'
exec desc_field @view, 'logginglevelsxml', 'This field is obsolete.', 'VIEW'
exec desc_field @view, 'sessionstatexml', 'This field is obsolete.', 'VIEW'
exec desc_field @view, 'lastupdated', 'The datetime when the last stage was started', 'VIEW'
exec desc_field @view, 'lastupdatedtimezoneoffset', 'The lastupdated timezone offset', 'VIEW'
exec desc_field @view, 'laststage', 'The last stage that was started', 'VIEW'
exec desc_field @view, 'warningthreshold', 'The number of seconds since the last updated time before the session goes into a warning state.', 'VIEW'
exec desc_field @view, 'queueid', 'The queue that this resource is currently assigned to', 'VIEW'


set @view=N'vw_Audit'
set @desc=N'A view which provides data on all audit events.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'eventdatetime', 'The datetime of the event', 'VIEW'
exec desc_field @view, 'eventId', 'ID of the event', 'VIEW'
exec desc_field @view, 'sCode', 'The code of the event', 'VIEW'
exec desc_field @view, 'source user', 'The username of the source user', 'VIEW'
exec desc_field @view, 'sNarrative', 'The narrative of the event', 'VIEW'
exec desc_field @view, 'comments', 'Comments about the event', 'VIEW'
exec desc_field @view, 'target user', 'The username of the target user', 'VIEW'
exec desc_field @view, 'name', 'The name of the process', 'VIEW'
exec desc_field @view, 'target resource', 'The name of the resource', 'VIEW'

set @view=N'vw_Audit_improved'
set @desc=N'An improved view which provides data on all audit events.'
exec desc_table @view, @desc, 'VIEW'
exec desc_field @view, 'Event Datetime', 'The datetime of the event', 'VIEW'
exec desc_field @view, 'Event ID', 'ID of the event', 'VIEW'
exec desc_field @view, 'Code', 'The code of the event', 'VIEW'
exec desc_field @view, 'By User', 'The username of the source user', 'VIEW'
exec desc_field @view, 'Narrative', 'The narrative of the event', 'VIEW'
exec desc_field @view, 'Comments', 'Comments about the event', 'VIEW'
exec desc_field @view, 'Target User', 'The username of the target user', 'VIEW'
exec desc_field @view, 'Target Process', 'The name of the process', 'VIEW'
exec desc_field @view, 'Target Resource', 'The name of the resource', 'VIEW'


set @table=N'BPASkill';
set @desc=N'Represents an imported robot skill';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The unique skill identifier';
exec desc_field @table, 'provider', 'The name of the skill provider';
exec desc_field @table, 'isenabled', 'True if the skill is available for use on the Studio toolbar';

set @table=N'BPASkillVersion';
set @desc=N'Represents a distinct version of an imported robot skill';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The unique skill version identifier';
exec desc_field @table, 'skillid', 'The ID of the skill';
exec desc_field @table, 'name', 'The skill name';
exec desc_field @table, 'versionnumber', 'The version number of this skill';
exec desc_field @table, 'description', 'Basic skill description';
exec desc_field @table, 'category', 'Skill category';
exec desc_field @table, 'icon', 'The image to display in Studio';
exec desc_field @table, 'bpversioncreated', 'The Blue Prism version that the skill was created with';
exec desc_field @table, 'bpversiontested', 'The Blue Prism version(s) that the skill has been tested against';
exec desc_field @table, 'importedat', 'The date the skill was imported';
exec desc_field @table, 'importedby', 'The user who imported the skill';

set @table=N'BPAWebSkillVersion';
set @desc=N'Represents a Web API based skill version';
exec desc_table @table, @desc;
exec desc_field @table, 'versionid', 'The skill version identifier';
exec desc_field @table, 'webserviceid', 'The ID of the Web API Service which implements this skill';

set @table=N'BPADataPipelineInput';
set @desc=N'Acts as a queue for events into the data pipeline.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The event identifier';
exec desc_field @table, 'eventtype', 'The type of event. 1 = Session Log, 2 = Dashboard';
exec desc_field @table, 'eventdata', 'The event data serialised to json.';
exec desc_field @table, 'publisher', 'The hostname of the machine which published the event.';
exec desc_field @table, 'inserttime', 'The time the event was added. UTC';

set @table=N'BPADataPipelineProcess';
set @desc=N'Stores processes which are consuming events from the data pipeline, which have been registered via the app server.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The ID assigned to the process';
exec desc_field @table, 'name', 'The hostname of the machine the process is running on.';
exec desc_field @table, 'status', 'Status of the process. Online = 1, Offline = 2, Error = 3';
exec desc_field @table, 'message', 'Status messages from the process. ';
exec desc_field @table, 'lastupdated', 'The time the process last updated its stautus with the app server. UTC';
exec desc_field @table, 'config', 'Reference to a configuration file in the BPADataPipelineProcessConfig table.';

set @table=N'BPADataPipelineProcessConfig';
set @desc=N'Stores configuration files for the processes which consume events from the data pipeline.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The configuration identifier';
exec desc_field @table, 'name', 'Name assigned to the configuration file.';
exec desc_field @table, 'encryptid', 'ID of the encryption scheme used to encrypt the configuration file.';
exec desc_field @table, 'configfile', 'The encrypted configuration file. ';

set @table=N'BPADataPipelineSettings';
set @desc=N'Stores settings for data gateways.';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The technical primary key (always 1)';
exec desc_field @table, 'writesessionlogstodatabase', 'Flag to indicate if session logs should be written to the DB.';
exec desc_field @table, 'emitsessionlogstodatagateways', 'Flag to indicate if session logs should be emitted to data gateways.';
exec desc_field @table, 'monitoringfrequency', 'The frequency (in seconds) that data gateway processes are monitored.';

set @table=N'BPADataPipelineOutputConfig';
set @desc=N'Contains configuration for data pipeline outputs';
exec desc_table @table, @desc;
exec desc_field @table, 'id', 'The config identifier';
exec desc_field @table, 'name', 'The config name';
exec desc_field @table, 'issessions', 'Whether the output type is sessions';
exec desc_field @table, 'isdashboards', 'Whether the output type is dashboards';
exec desc_field @table, 'iswqasnapshotdata', 'Whether the output type is WQA snapshot data';
exec desc_field @table, 'iscustomobjectdata', 'Whether the output type is custom object data';
exec desc_field @table, 'sessioncols', 'The selected session columns';
exec desc_field @table, 'dashboardcols', 'The selected dashboard columns';
exec desc_field @table, 'datecreated', 'The time the config was created. UTC';
exec desc_field @table, 'advanced', 'The advanced configuration';
exec desc_field @table, 'type', 'The output type';
exec desc_field @table, 'isadvanced', 'Whether the adanced mode is used';
exec desc_field @table, 'outputoptions', 'The output configuration options';

SET @table=N'BPAAliveAutomateC'
SET @desc=
    N'Contains the alive status of AutomateC.'
EXEC desc_table @table,@desc
EXEC desc_field @table,N'MachineName',N'The hostname of the resource of interest.'
EXEC desc_field @table,N'UserID',N'The ID of any user logged in on this resource.'
EXEC desc_field @table,N'LastUpdated',N'A timestamp showing when this row was last updated.'












--END
