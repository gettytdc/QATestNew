/*
SCRIPT         : 207
AUTHOR         : GM
PURPOSE        : Restore foreign keys on Packaged Environment variable table
*/

-- Remove incorrectly defined foreign key constraint
alter table BPAPackageEnvironmentVar
drop constraint FK_BPAPackageEnvironmentVar_BPAEnvironmentVar;

-- Replace constraint with cascading options
alter table BPAPackageEnvironmentVar
add constraint FK_BPAPackageEnvironmentVar_BPAEnvironmentVar
foreign key (name) references BPAEnvironmentVar(name)
on update cascade on delete cascade;

-- Remove any orphaned packaged environment variable data
delete from BPAPackageEnvironmentVar where packageid in
(select v.packageid from BPAPackageEnvironmentVar v
left join BPAPackage p on v.packageid=p.id where p.id is null)

-- Restore cascading delete constraint to BPAPackage
alter table BPAPackageEnvironmentVar
add constraint FK_BPAPackageEnvironmentVar_BPAPackage
foreign key (packageid) references BPAPackage(id)
on delete cascade;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '207',
  GETUTCDATE(),
  'db_upgradeR207.sql UTC',
  'Restore foreign keys on Packaged Environment variable table'
);
