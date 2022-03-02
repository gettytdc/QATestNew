/*
BUG/STORY      : US-2401
PURPOSE        : Add new permission for executing a process as a web service
*/
create procedure #usp_addPermission 
    @permissionname nvarchar(250),
    @treeparentname nvarchar(100),
    @groupname nvarchar(100),
    @roleid int
as
begin

    -- Create permission and assign to group
    declare @permissionidtable table (id int)

    insert into [BPAPerm] ([name], [treeid])
    output inserted.[id] into @permissionidtable
    values (@permissionname, null)

    declare @permissionid int

    select top 1 @permissionid = [id] from @permissionidtable

    declare @groupid int

    select top 1 @groupid = [id]
    from [BPAPermGroup]
    where [name] = @groupname

    insert into [BPAPermGroupMember] ([permgroupid], [permid])
    values (@groupid, @permissionid)

    -- Set permission to be item level in group permissions
    declare @treeid int

    select @treeid = [id]
    from [BPATree]
    where [name] = @treeparentname

    insert into [BPATreePerm] (treeid, permid, groupLevelPerm)
    values (@treeid, @permissionid, 0)

    insert into [BPAUserRolePerm] ([userroleid], [permid])
    values (@roleid, @permissionid)

    -- Apply permission to all existing users if this is an upgrade
    if (select InstallInProgress from BPVScriptEnvironment) <> 1
    begin
        declare @userroleids table (userroleid int)

        insert into @userroleids
        select [userroleid]
        from [BPAUserRolePerm]
        except
        select [userroleid]
        from [BPAUserRolePerm]
        where [permid] = @permissionid

        insert into [BPAUserRolePerm] ([userroleid], [permid])
        select [userroleid], @permissionid
        from @userroleids
    end

    -- Always give the permission to system administrators
    declare @sysadminroleid int

    select top 1 @sysadminroleid = [id] from [BPAUserRole]
    where [name] = 'System Administrators'

    delete from [BPAUserRolePerm]
    where [userroleid] = @sysadminroleid and [permid] = @permissionid

    insert into [BPAUserRolePerm] ([userroleid], [permid])
    values (@sysadminroleid, @permissionid)

end

go

-- Create role for web service consumers
declare @rolename nvarchar(max);
set @rolename = 'Web Service Consumer';
declare @newname nvarchar(max);
set @newname = @rolename
declare @suffix nvarchar(max);
set @suffix = ' (previousx)';
declare @id int;
set @id = 0;

while exists (select 1 from BPAUserRole where [name] = @newname)
begin
    set @newname = @rolename + REPLACE(@suffix, 'x', case when @id = 0 then '' else cast(@id as nvarchar(10)) end);
    set @id += 1;
end
if @newname <> @rolename
    update BPAUserRole set [name] = @newname where [name] = @rolename;

declare @roleidtable table([id] int)

insert into BPAUserRole ([name])
output inserted.id into @roleidtable
values (@rolename);

declare @webserviceroleid int

select top 1 @webserviceroleid = [id] from @roleidtable

exec #usp_addPermission 'Execute Process as Web Service', 'Processes', 'Process Studio', @webserviceroleid
exec #usp_addPermission 'Execute Business Object as Web Service', 'Objects', 'Object Studio', @webserviceroleid

-- set DB version
insert into BPADBVersion values (
  '250',
  getutcdate(),
  'db_upgradeR250.sql',
  'Add new permission for executing a process as a web service',
  0 -- UTC
);
