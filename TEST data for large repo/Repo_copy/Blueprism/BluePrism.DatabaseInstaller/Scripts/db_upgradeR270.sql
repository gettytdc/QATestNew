declare @rolename nvarchar(max);
set @rolename = 'Web Service Consumers';
declare @newname nvarchar(max);
set @newname = @rolename;
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

update BPAUserRole set [name] = @rolename where [name] = 'Web Service Consumer';

-- Set DB version.
INSERT INTO BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
VALUES('270',
       getutcdate(),
       'db_upgradeR270.sql',
       'Pluralise Web Service Consumer role',
       0);
