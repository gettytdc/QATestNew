/*
SCRIPT         : 215
AUTHOR         : GMB
PURPOSE        : Add additional columns for password hashes
RELATED STORY  : us-534
*/

create table [BPAPassword] (
    [id] int not null primary key identity,
    [userid] uniqueidentifier not null
        constraint FK_BPAPassword_BPAUser
        foreign key references BPAUser(userid)
            on delete cascade,
    [active] bit not null,
    [type] int not null,
    [salt] varchar(max) not null,
    [hash] varchar(max) not null,
    [lastuseddate] datetime null,
)

-- migrate passwords if not using active directory
if exists (select 1 from BPASysConfig where ActiveDirectoryProvider is null or ActiveDirectoryProvider = '') begin

  -- move current passwords to new table
  insert into BPAPassword ([active],[type],[userid],[salt],[hash],[lastuseddate])
    select 1, 0, u.[userid], '', u.[password], u.[lastsignedin] from BPAUser u where u.[password] is not null;

  -- move old passwords into the table
  insert into BPAPassword ([active],[type],[userid],[salt],[hash],[lastuseddate])
      select 0, 0, o.[userid], '', o.[password], o.[lastuseddate] from BPAOldPassword o;

end;
GO

-- clean up legacy tables
alter table [BPAUser] drop column [password];
drop table [BPAOldPassword];
GO


--set DB version
INSERT INTO BPADBVersion
VALUES
('215',
 GETUTCDATE(),
 'db_upgradeR215.sql UTC',
 'Add additional columns for password hashes'
);
