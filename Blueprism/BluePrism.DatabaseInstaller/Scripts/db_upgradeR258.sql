alter table BPACredentials
    alter column login nvarchar(max) not null
go

--set DB version
insert into BPADBVersion values (
  '258',
  GETUTCDATE(),
  'db_upgradeR258.sql UTC',
  'Remove length restriction from the BPACredentials login column',
  0 -- UTC
)