/*
STORY		: bp-2322
PURPOSE		: Adds functionality to store a relogin token for use when a wcf connection is dropped and subsequently re-eastblished
*/

create table BPAUserExternalReloginToken
(
    id bigint not null identity (1,1) primary key,
    bpuserid uniqueidentifier not null 
    constraint FK_BPAUserExternalReloginToken_BPAUser 
		foreign key references BPAUser (userid)
		on delete cascade,
    machinename nvarchar(128) not null,
    processid int not null,
    token varchar(max) not null,
    salt varchar(max),
    tokenexpiry datetime not null
);
go 

alter table BPAUserExternalReloginToken
add constraint UNQ_bpuserid_machinename_processid unique (bpuserid, machinename, processid);
go

create index IX_BPAUserExternalReloginToken_bpuserid_machinename_processid on
    BPAUserExternalReloginToken(processid, tokenexpiry) include (bpuserid, machinename, salt, token)
go

-- Set DB version.
insert into BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
values('405',
       getutcdate(),
       'db_upgradeR405.sql',
       'Add table BPAUserExternalReloginToken',
       0);
