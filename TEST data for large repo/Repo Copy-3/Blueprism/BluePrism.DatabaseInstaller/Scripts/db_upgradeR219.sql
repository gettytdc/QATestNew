/*
SCRIPT         : 219
AUTHOR         : GB/GM
PURPOSE        : Add screenshot table and permissions to view screenshots
*/

-- Create new table to hold screenshot images
create table BPAScreenshot (
    id int not null identity
        constraint PK_BPAScreenshot primary key,
    resourceid uniqueidentifier not null,
    stageid uniqueidentifier not null,
    processname nvarchar(max) not null,
    lastupdated datetime not null,
    timezoneoffset int not null,
    screenshot nvarchar(max) not null,
    encryptid int not null
        constraint FK_BPAScreenshot_BPAKeyStore foreign key references BPAKeyStore (id)
    );

-- Rename existing credential encryption scheme ID for clarity
exec sp_rename '[BPASysConfig].[encryptid]', 'defaultencryptid', 'COLUMN';

-- Add new permission for access to screenshots
declare @id int;
insert into BPAPerm (name) values('View resource screen captures');
select @id = id from BPAPerm where name = 'View resource screen captures';

insert into BPAPermGroupMember (permgroupid, permid)
select g.id, @id from BPAPermGroup g where g.name = 'Control Room';

insert into BPAUserRolePerm (userroleid, permid)
select r.id, @id from BPAUserRole r where r.name
    in ('System Administrator', 'Tester', 'Process Administrator');

--set DB version
INSERT INTO BPADBVersion VALUES (
  '219',
  GETUTCDATE(),
  'db_upgradeR219.sql UTC',
  'Add screenshot table and permissions to view screenshots'
);
