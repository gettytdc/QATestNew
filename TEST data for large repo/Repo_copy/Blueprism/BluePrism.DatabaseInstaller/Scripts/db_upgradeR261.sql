alter table BPAWebApiCustomOutputParameter
    add outputparametertype int not null default 1
go

alter table BPAWebApiAction
    add outputparametercode varchar(max) null
go


-- set DB version
insert into BPADBVersion values (
  '261',
  getutcdate(),
  'db_upgradeR261.sql',
  'Add new type column to BPAWebApiCustomOutputParameter table and outputparametercode column to BPAWebAPIAction',
  0 -- UTC
);

