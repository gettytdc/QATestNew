/*
SCRIPT         : 209
AUTHOR         : CG/SW
PURPOSE        : Add multi-license table
*/

-- Create the new table to hold the licenses
create table BPALicense (
    id int identity
        constraint PK_BPALicense primary key,
    licensekey nvarchar(max) not null,
    installedon datetime not null,
    installedby uniqueidentifier null,
    constraint FK_License_User
        foreign key (installedby)
        references BPAUser (userid)
);

-- Move the old license across
insert into BPALicense (licensekey, installedon)
  select licensekey, getutcdate() from BPASysConfig;

-- And drop the column which held the old license
alter table BPASysConfig
  drop column licensekey;

GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '209',
  GETUTCDATE(),
  'db_upgradeR209.sql UTC',
  'Add multi-license table'
);
