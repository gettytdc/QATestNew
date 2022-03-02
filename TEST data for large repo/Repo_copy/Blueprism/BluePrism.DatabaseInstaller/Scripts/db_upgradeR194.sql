/*
SCRIPT         : 194
CREATION DATE  : 23 March 2016
AUTHOR         : GM
PURPOSE        : Adds tables for multiple encryption key support
               : Note this was originally done in v4.2 R193 and ported to v5
*/

-- Apply v4.2 R193 changes if required
if not exists (select 1 from BPADBVersion where dbversion='193')
begin
    exec('
    -- Create new encryption key repository
    create table BPAKeyStore (
        id int identity not null,
        name nvarchar(255) not null,
        location int not null,
        isavailable bit not null,
        method int,
        encryptkey nvarchar(255),
        constraint PK_BPAKeyStore primary key (id),
        constraint Index_BPAKeyStore_name unique (name));

    -- Add encryption id foreign keys
    alter table BPAWorkQueue add encryptid int
        constraint FK_BPAWorkQueue_BPAKeyStore foreign key references BPAKeyStore (id);
    alter table BPAWorkQueueItem add encryptid int
        constraint FK_BPAWorkQueueItem_BPAKeyStore foreign key references BPAKeyStore (id);
    alter table BPASysConfig add encryptid int
        constraint FK_BPASysConfig_BPAKeyStore foreign key references BPAKeyStore (id);
    alter table BPACredentials add encryptid int
        constraint FK_BPACredentials_BPAKeyStore foreign key references BPAKeyStore (id);');

    exec('
    -- Upgrade existing data if required
    insert into BPAKeyStore (name, location, isavailable, method, encryptkey)
    select ''Credentials Key'', case when credentialkey is not null then 0 else 1 end, 1, 1, credentialkey from BPASysConfig;

    declare @id int;
    select @id = id from BPAKeyStore where name=''Credentials Key'';

    -- Point credentials key to default key
    update BPASysConfig set encryptid=@id;
    update BPACredentials set encryptid=@id;

    -- Point encrypted queues & items to default key
    update BPAWorkQueue set encryptid=@id where encryptname is not null;
    update i set i.encryptid=q.encryptid
    from BPAWorkQueue q inner join BPAWorkQueueItem i on i.queueident=q.ident
    where q.encryptname is not null;

    -- Remove redundant columns
    alter table BPAWorkQueue drop column encryptname;
    alter table BPASysConfig drop column credentialkey;');
end
GO

-- Add new permission for encryption schemes
declare @id int;
insert into BPAPerm values('Security - View Encryption Schemes');
insert into BPAPerm values('Security - Manage Encryption Schemes');

select @id = id from BPAPermGroup where name='System Manager';
insert into BPAPermGroupMember select @id, a.id from BPAPerm a
where a.name in ('Security - View Encryption Schemes', 'Security - Manage Encryption Schemes');

select @id = id from BPAUserRole where name='System Administrator';
insert into BPAUserRolePerm select @id, a.id from BPAPerm a
where a.name in ('Security - View Encryption Schemes', 'Security - Manage Encryption Schemes');
GO

-- Update the queue groups view for encryptid column
alter view BPVGroupedQueues as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    q.ident as id,
    q.name as name,
    q.id as guid,
    q.running as running,
    q.encryptid as encryptid,
    q.processid as processid,
    q.resourcegroupid as resourcegroupid,
    case
      when q.processid is not null and q.resourcegroupid is not null then cast(1 as bit)
      else cast(0 as bit)
    end as isactive
    from BPAWorkQueue q
      left join (
        BPAGroupQueue gq
            inner join BPAGroup g on gq.groupid = g.id
      ) on gq.memberid = q.ident;
GO

-- Set DB version
INSERT INTO BPADBVersion VALUES (
  '194',
  GETUTCDATE(),
  'db_upgradeR194.sql UTC',
  'Adds tables for multiple encryption key support [v5]'
);
