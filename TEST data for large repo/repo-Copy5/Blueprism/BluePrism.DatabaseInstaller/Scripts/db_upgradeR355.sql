/*
SCRIPT         : 355
AUTHOR         : KBW
PURPOSE        : Update process object dependant views with correct column order
*/


if not exists(select * from sys.views where name = 'BPVGroupedActiveProcesses')
  exec (N'create view BPVGroupedActiveProcesses as select 1 as placeholder');
  
if not exists(select * from sys.views where name = 'BPVGroupedPublishedProcesses')
  exec (N'create view BPVGroupedPublishedProcesses as select 1 as placeholder');
GO


  -- All active processes
  alter view BPVGroupedActiveProcesses as
select * from BPVGroupedProcesses where (attributes & 1) = 0;
GO

  alter view BPVGroupedPublishedProcesses as
select * from BPVGroupedActiveProcesses where (attributes & 2) != 0;
GO


-- set DB version
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('355', 
 GETUTCDATE(), 
 'db_upgradeR355.sql', 
 'Update process object dependant views with correct column order', 
 0
);