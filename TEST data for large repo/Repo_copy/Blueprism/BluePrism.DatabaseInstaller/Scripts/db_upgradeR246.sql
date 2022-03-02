/*
BUG/STORY      : BG-1586
PURPOSE        : Add view environment variables permissions to Runtime Resources role
*/

-- Give the view environment variables permissions to Runtime Resources role
declare @id int;
select @id = id from BPAUserRole where name='Runtime Resources';
insert into BPAUserRolePerm (userroleid, permid) select @id, a.id from BPAPerm a
where a.name = 'Processes - View Environment Variables' 
   or a.name = 'Business Objects - View Environment Variables';

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '246',
  GETUTCDATE(),
  'db_upgradeR246.sql',
  'Add view environment variables permissions to Runtime Resources role',
  0 -- UTC
);