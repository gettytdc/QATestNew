alter table BPACredentials
    add credentialType nvarchar(50) null
go

update BPACredentials
    set credentialType = 'General'
go

alter table BPACredentials
    alter column credentialType nvarchar(50) not null
go

--set DB version
insert into BPADBVersion values (
  '257',
  GETUTCDATE(),
  'db_upgradeR257.sql UTC',
  'Add column to credentials for type',
  0 -- UTC
)