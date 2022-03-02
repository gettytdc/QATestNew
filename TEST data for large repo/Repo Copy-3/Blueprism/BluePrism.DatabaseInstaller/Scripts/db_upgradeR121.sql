/*
SCRIPT         : 121
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Makes Scheduler use name to identify a resource rather than ID
*/

-- Initially create the table with a nullable resource name
alter table BPATaskSession add resourcename varchar(128) null; -- refer(BPAResource.name)
go

-- Set the resource name - the FK requires that a resource record exists, so
-- we know there will be no gaps
update s
    set s.resourcename = r.name
    from BPATaskSession s 
        join BPAResource r on s.resourceid = r.resourceid;

-- Set the column to be not null - we can't have a foreign key any more, but at least
-- we can check that one error condition.
alter table BPATaskSession 
    alter column resourcename varchar(128) not null;

-- Lose the foreign key
alter table BPATaskSession drop constraint FK_BPATaskProcess_BPAResource;
go

-- And now drop the resource ID column itself.
alter table BPATaskSession drop column resourceid;
go

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '121',
  GETUTCDATE(),
  'db_upgradeR121.sql UTC',
  'Makes Scheduler use name to identify a resource rather than ID'
);
