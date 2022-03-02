/*
SCRIPT         : 142
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds fonts to packages / releases
*/

create table BPAPackageFont (
    packageid int not null
        constraint FK_BPAPackageFont_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    name varchar(255) not null
        constraint FK_BPAPackageFont_BPAFont
            foreign key references BPAFont(name)
            on delete cascade
            on update cascade,
    constraint PK_BPAPackageFont
        primary key (packageid, name)
)

--set DB version
INSERT INTO BPADBVersion VALUES (
  '142',
  GETUTCDATE(),
  'db_upgradeR142.sql UTC',
  'Adds fonts to packages / releases'
)
