/*
SCRIPT         : 159
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds encryption key to work queues
*/

alter table BPAWorkQueue add 
  encryptname varchar(255) null

--set DB version
INSERT INTO BPADBVersion VALUES (
  '159',
  GETUTCDATE(),
  'db_upgradeR159.sql UTC',
  'Adds encryption key to work queues'
);

