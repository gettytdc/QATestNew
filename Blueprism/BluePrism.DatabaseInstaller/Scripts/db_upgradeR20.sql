/* THIS IS A DATABASE UPGRADE SCRIPT USED FOR AMENDING INDEX INDEX_SESSIONID IN BPASESSIONLOG CS - 11/05/2005 */


CREATE INDEX [Index_sessionID] ON [BPASessionLog]([sessionid], [seqnum]) with (DROP_EXISTING = on)


/* DB Version */
insert into BPADBVersion values ('20',GETUTCDATE(),'db_upgradeR20.sql UTC','Database amendments - adds seqnum to INDEX_SESSIONID in BPASESSIONLOG')
