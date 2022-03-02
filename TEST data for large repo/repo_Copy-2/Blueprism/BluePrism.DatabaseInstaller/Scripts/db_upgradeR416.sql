/*
STORY:      BP-2676
PURPOSE:   Add index on enddateime and endtimezoneoffset columns in BPASession table, add status to enum table

*/

create nonclustered index [IDX_NC_BPASession_enddatetime_endtimezoneoffset] 
on BPASession
(
enddatetime, 
endtimezoneoffset
);
go

 INSERT INTO [BPAResourceAttribute] (AttributeID, AttributeName) VALUES(64,'DefaultInstance');

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('416',
 getutcdate(), 
 'db_upgradeR416.sql', 
 'Add index on enddateime and endtimezoneoffset columns in BPASession table, add status to enum table', 
 0
);
