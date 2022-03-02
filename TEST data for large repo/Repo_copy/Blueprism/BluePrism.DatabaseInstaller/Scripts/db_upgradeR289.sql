declare @OldPermName as nvarchar(50)
       ,@NewPermName as nvarchar(50)

set @OldPermName = N'System - Web Connection Settings';
set @NewPermName = N'Business Objects - Web Connection Settings';

--Update permission name
update BPAPerm
    set name = @NewPermName
where name = @OldPermName;

GO

-- set DB version
insert into BPADBVersion values (
  '289',
  getutcdate(),
  'db_upgradeR289.sql',
  'Updated permission name for web connection settings from System to Business Objects.',
  0 -- UTC
)
