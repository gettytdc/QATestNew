/*
SCRIPT         : 351
AUTHOR         : Sarah McClean
PURPOSE        : Adding External Provider and External Provider Type tables
*/

create table BPAExternalProviderType (
    id int not null identity (1,1)
		 constraint PK_BPAExternalProviderType primary key,
    name nvarchar(64) not null,
)

create table BPAExternalProvider (
    id int not null identity (1,1)
		 constraint PK_BPAExternalProvider primary key,
    name nvarchar(64) not null,
	externalprovidertypeid int not null
	)

alter table BPAExternalProvider
	add constraint FK_BPAExternalProvider_BPAExternalProviderType
		foreign key (externalprovidertypeid) references BPAExternalProviderType(id)
        on delete cascade;	

delete from BPAUserExternalIdentity;

alter table BPAUserExternalIdentity
	drop constraint UNQ_bpuserid_idprovider
		
drop index IX_BPAUserExternalIdentity_idprovider_externalid on BPAUserExternalIdentity

alter table BPAUserExternalIdentity
	drop column idprovider

alter table BPAUserExternalIdentity
	add externalproviderid int not null 
		constraint FK_BPAUserExternalIdentity_BPAExternalProvider
		foreign key (externalproviderid) references BPAExternalProvider(id)
        on delete cascade;	

alter table BPAUserExternalIdentity
	add constraint UNQ_bpuserid_externalproviderid 
	unique nonclustered (
		bpuserid asc,
		externalproviderid asc
	)

create index IX_BPAUserExternalIdentity_externalproviderid_externalid on BPAUserExternalIdentity(externalproviderid, externalid) include (bpuserid)
create index IX_BPAExternalProvider_externalproviderid on BPAExternalProviderType(id) 
	
-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('351', 
 GETUTCDATE(), 
 'db_upgradeR351.sql', 
 'Adding External Provider and External Provider Type tables', 
 0
);