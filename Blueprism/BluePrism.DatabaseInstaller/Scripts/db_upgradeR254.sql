
/*
BUG/STORY      : US-2744
PURPOSE        : Grant Execute to existing user roles
*/

if (select InstallInProgress from BPVScriptEnvironment) <> 1
begin
    declare @permissionId int
    select top 1 @permissionId = [id] from [BPAPerm] where [name] = 'Execute Business Object'

    declare @webServiceConsumerId int
    select top 1 @webServiceConsumerId = [id] from [BPAUserRole] where [name] = 'Web Service Consumer'

    declare @userRoleIds table (userRoleId int)

    insert into @userRoleIds
    select [userroleid]
    from [BPAUserRolePerm]
    where [userroleid] <> @webServiceConsumerId
    except
    select [userroleid]
    from [BPAUserRolePerm]
    where [permid] = @permissionId

    insert into [BPAUserRolePerm] ([userroleid], [permid])
    select [userRoleId], @permissionId
    from @userRoleIds
end
GO

if (select InstallInProgress from BPVScriptEnvironment) <> 1
begin
    declare @permissionId int
    select top 1 @permissionId = [id] from [BPAPerm] where [name] = 'Execute Process'

    declare @webServiceConsumerId int
    select top 1 @webServiceConsumerId = [id] from [BPAUserRole] where [name] = 'Web Service Consumer'

    declare @userRoleIds table (userRoleId int)

    insert into @userRoleIds
    select [userroleid]
    from [BPAUserRolePerm]
    where [userroleid] <> @webServiceConsumerId
    except
    select [userroleid]
    from [BPAUserRolePerm]
    where [permid] = @permissionId

    insert into [BPAUserRolePerm] ([userroleid], [permid])
    select [userRoleId], @permissionId
    from @userRoleIds
end
GO

-- set DB version
insert into BPADBVersion values (
  '254',
  getutcdate(),
  'db_upgradeR254.sql',
  'Grant Execute to existing user roles',
  0 -- UTC
);
