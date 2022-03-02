/*
SCRIPT         : 200
AUTHOR         : GM
PURPOSE        : Adds Shared attribute to objects
*/

-- Add shared flag to process table
alter table BPAProcess add sharedObject bit not null default 0;
GO

-- Set shared attribute on any parent objects
update BPAProcess set sharedObject = 1, processxml = STUFF(processxml, CHARINDEX('>', processxml), 0, ' shared="True"')
where processType='O' and name in (select distinct(refParentName) from BPAProcessParentDependency);

-- Set DB version
INSERT INTO BPADBVersion VALUES (
  '200',
  GETUTCDATE(),
  'db_upgradeR200.sql UTC',
  'Adds Shared attribute to objects'
);
