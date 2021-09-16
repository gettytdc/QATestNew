/*
BUG/STORY      : us-5972
PURPOSE        : Ensure index is utilised when querying the BPAUserExternalIdentity table
*/

drop index IX_BPAUserExternalIdentity_idprovider_externalid on BPAUserExternalIdentity
go
create index IX_BPAUserExternalIdentity_idprovider_externalid on BPAUserExternalIdentity(idprovider, externalid) include (bpuserid)
go

insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('335', 
 GETUTCDATE(), 
 'db_upgradeR335.sql', 
 'Add include clause to IX_BPAUserExternalIdentity_idprovider_externalid', 
 0
);