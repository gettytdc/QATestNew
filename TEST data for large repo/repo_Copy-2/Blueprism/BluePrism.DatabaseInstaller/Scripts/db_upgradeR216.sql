/*
SCRIPT         : 216
AUTHOR         : GB/GM
PURPOSE        : Increase ArchiveInProgress & MachineName column sizes
RELATED BUG    : bg-308/bg-556
*/

alter table BPASysConfig alter column ArchiveInProgress nvarchar(max);

alter table BPAAliveResources drop constraint PK_BPAAliveResources;
alter table BPAAliveResources alter column MachineName nvarchar(128) not null;
alter table BPAAliveResources add constraint PK_BPAAliveResources
 primary key clustered (MachineName, UserID);

--set DB version
INSERT INTO BPADBVersion VALUES (
  '216',
  GETUTCDATE(),
  'db_upgradeR216.sql UTC',
  'Increase ArchiveInProgress & MachineName column sizes'
);
