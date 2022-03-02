create table BPAUserExternalIdentity
( bpuserid uniqueidentifier not null constraint FK_BPAUserExternalIdentity_BPAUser foreign key references BPAUser (userid),
  idprovider nvarchar(10) not null,
  externalid nvarchar(254) not null
);
go 

alter table BPAUserExternalIdentity
add constraint UNQ_bpuserid_idprovider unique (bpuserid, idprovider);
go

create index IX_BPAUserExternalIdentity_idprovider_externalid on BPAUserExternalIdentity(idprovider, externalid)
go

alter table BPASysConfig add
    authenticationserverurl nvarchar(2083) null;

go    
-- Set DB version.
insert into BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
values('329',
       getutcdate(),
       'db_upgradeR329.sql',
       'Add table BPAUserExternalIdentity and add authenticationserverurl field to BPASysConfig',
       0);


    

      