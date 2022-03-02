declare @rolename nvarchar(max);
set @rolename = 'Release Managers';
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

insert into BPAUserRole ([name], ssogroup)
values(@rolename, '');

insert into BPAUserRolePerm(userroleid, permid)
select @@IDENTITY, permid
from BPAPermGroupMember
where permgroupid in (select id from BPAPermGroup where [name] in ('Release Manager', 'Skills'));

-- Set DB version.
INSERT INTO BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
VALUES('268',
       getutcdate(),
       'db_upgradeR268.sql',
       'Add Release Managers role to BPAUserRole and link to permissions.',
       0);
