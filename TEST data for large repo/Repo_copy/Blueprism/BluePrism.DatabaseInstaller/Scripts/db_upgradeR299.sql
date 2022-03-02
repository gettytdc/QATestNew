

CREATE TABLE BPADataPipelineInput
(
    id bigint primary key identity(1,1) not null,
    eventtype integer not null,
    [eventdata] nvarchar(max) not null,
    publisher nvarchar(200) not null,
    inserttime datetime default(GETUTCDATE())
)

CREATE TABLE BPADataPipelineProcessConfig(
    id int primary key identity(1,1),
    [name] nvarchar(100) not null,
    encryptid int,
    [configfile] nvarchar(max),
    CONSTRAINT FK_BPADataPipelineProcessConfig_BPAKeyStore_ID FOREIGN KEY (encryptid) REFERENCES BPAKeyStore(id)
);


CREATE TABLE BPADataPipelineProcess (
    id int primary key identity(1,1),
    [name] nvarchar(max) not null,
    [status] int not null,
    [message] nvarchar(max),
    [lastupdated] datetime default(GETUTCDATE()),
    [config] int,
    [tcpEndpoint] nvarchar(max) not null
    CONSTRAINT FK_BPADataPipelineProcess_BPADataPipelineProcessConfig_ID FOREIGN KEY ([config]) REFERENCES BPADataPipelineProcessConfig(id)
);

insert into BPAPerm ([name]) values ('Data Gateways - Configuration'), ('Data Gateways - Advanced Configuration'), ('Data Gateways - Control Room');

declare @sysManGroupId as int
declare @controlRoomGroupId as int
select @sysManGroupId = id from BPAPermGroup where [name] = 'System Manager'
select @controlRoomGroupId = id from BPAPermGroup where [name] = 'Control Room'

insert into BPAPermGroupMember (permgroupid, permid)
    select @sysManGroupId, id from BPAPerm
    where [name] in ('Data Gateways - Configuration', 'Data Gateways - Advanced Configuration');

insert into BPAPermGroupMember (permgroupid, permid)
    select @controlRoomGroupId, id from BPAPerm
    where [name] = 'Data Gateways - Control Room';

insert into BPAUserRolePerm (userroleid, permid)
    select r.id, p.id from BPAUserRole r
        cross join BPAPerm p
    where r.name = 'System Administrators' and p.name in ('Data Gateways - Configuration', 'Data Gateways - Advanced Configuration', 'Data Gateways - Control Room');
GO

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('299',
       getutcdate(),
       'db_upgradeR299.sql',
       'Add data pipeline tables.',
       0);