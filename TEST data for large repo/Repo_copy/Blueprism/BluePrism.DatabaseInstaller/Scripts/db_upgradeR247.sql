/*
BUG/STORY      : US-2063
PURPOSE        : Add "Authenticate as Resource" permission to all existing roles
*/

if (select InstallInProgress from BPVScriptEnvironment) <> 1
begin
    declare @permissionId int
    select top 1 @permissionId = [id] from [BPAPerm] where [name] = 'Authenticate as Resource'

    declare @userRoleIds table (userRoleId int)

    insert into @userRoleIds
    select [userroleid]
    from [BPAUserRolePerm]
    except
    select [userroleid]
    from [BPAUserRolePerm]
    where [permid] = @permissionId

    insert into [BPAUserRolePerm] ([userroleid], [permid])
    select [userRoleId], @permissionId
    from @userRoleIds
end

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '247',
  GETUTCDATE(),
  'db_upgradeR247.sql',
  'Add "Authenticate as Resource" permission to all existing roles',
  0 -- UTC
);