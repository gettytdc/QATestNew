/* SCRIPT: 238
PURPOSE:
        Adds the execute process permission to the runtime resources
        user role.
*/

declare @userRoleId int = (
    select top 1 [id]
    from [BPAUserRole]
    where [name] = 'Runtime Resources');

declare @permissionId int = (
    select top 1 [id]
    from [BPAPerm]
    where [name] = 'Execute Process');

if not exists (select * from [BPAUserRolePerm] where userroleid = @userRoleId and permid = @permissionId)
begin
    insert into [BPAUserRolePerm] (userroleid, permid)
    values (@userRoleId, @permissionId)
end

-- set DB version
insert into BPADBVersion values (
  '238',
  GETUTCDATE(),
  'db_upgradeR238.sql',
  'Adds the execute process permission to the runtime resources user role.',
  0 -- UTC
  );

