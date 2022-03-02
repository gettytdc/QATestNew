create table BPAProcessSkillDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessSkillDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refSkillId uniqueidentifier not null,
    constraint PK_BPAProcessSkillDependency primary key (id)
);

insert into BPADBVersion values (
  '265',
  getutcdate(),
  'db_upgradeR265.sql',
  'Add dependency table for skills',
  0 -- UTC
)