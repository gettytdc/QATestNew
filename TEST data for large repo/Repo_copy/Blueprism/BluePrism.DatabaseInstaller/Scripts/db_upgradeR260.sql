create table BPASkill (
    id uniqueidentifier not null
        constraint PK_BPASkill primary key clustered,
    [provider] nvarchar(max) not null,
    isenabled bit not null default 1
);

create table BPASkillVersion (
    id uniqueidentifier not null
        constraint PK_BPASkillVersion primary key clustered,
    skillid uniqueidentifier not null
        constraint FK_BPASkillVersion_BPASkill
            foreign key references BPASkill(id) on delete cascade,
    [name] nvarchar(max) not null,
    versionnumber nvarchar(255) not null,
    [description] nvarchar(max) not null,
    category nvarchar(max) not null,
    icon nvarchar(max) not null,
    bpversioncreated nvarchar(255),
    bpversiontested nvarchar(255),
    importedat datetime not null,
    importedby uniqueidentifier not null
        constraint FK_BPASkillVersion_BPAUser
            foreign key references BPAUser(userid)
);

create table BPAWebSkillVersion (
    versionid uniqueidentifier not null
        constraint PK_BPAWebSkillVersion primary key clustered
        constraint FK_BPAWebSkillVersion_BPASkillVersion
            foreign key references BPASkillVersion(id) on delete cascade,
    webserviceid uniqueidentifier not null
        constraint FK_BPAWebSkillVersion_BPAWebApiService
            foreign key references BPAWebApiService(serviceid)
);

-- set DB version
insert into BPADBVersion values (
  '260',
  getutcdate(),
  'db_upgradeR260.sql',
  'Add basic table structure for Skills',
  0 -- UTC
);