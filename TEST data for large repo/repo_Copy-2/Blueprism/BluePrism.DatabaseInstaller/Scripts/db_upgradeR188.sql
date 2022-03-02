/*
SCRIPT         : 188
PROJECT NAME   : Automate
PURPOSE        : Adds lock for active queues' aiming operations
*/

alter table BPAWorkQueue add
  activelock uniqueidentifier null,
  activelocktime datetime null,
  activelockname nvarchar(255) null;

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '188',
  GETUTCDATE(),
  'db_upgradeR188.sql UTC',
  'Adds lock for active queues'' aiming operations'
);
