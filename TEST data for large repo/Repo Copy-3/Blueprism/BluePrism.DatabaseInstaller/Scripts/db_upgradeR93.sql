/*
SCRIPT         : 93
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Currently a task cannot have the same process assigned
                 to the same resource. It rather needs it.
                 Also, renamed BPATaskProcess to BPATaskSession to match
                 the parlance we're using at higher levels of the
                 scheduler
*/

exec sp_RENAME 'BPATaskProcess', 'BPATaskSession'

alter table BPATaskSession drop constraint PK_BPATaskProcess;
alter table BPATaskSession 
    add id int not null identity
    constraint PK_BPATaskSession primary key;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '93',
  GETUTCDATE(),
  'db_upgradeR93.sql UTC',
  'Renamed BPATaskProcess to BPATaskSession, and allowed the same ' + 
  'process to run on the same resource within the same scheduler task'
);
