/* THIS IS A DATABASE UPGRADE SCRIPT USED FOR AMENDING BPASessionLog and BPASession CS - 04/05/2005
    IT UPGRADES A R16 database to R17
*/

CREATE  INDEX [Index_processID] ON [BPASession]([processid])

CREATE  INDEX [Index_statusID] ON [BPASession]([statusid])

CREATE  INDEX [Index_sessionID] ON [BPASessionLog]([sessionid])

/* DB Version */
insert into BPADBVersion values ('17',GETUTCDATE(),'db_upgradeR17.sql UTC','Database amendments - adds index to SessionID field in BPASessionLog and StatusID & ProcessID in BPASession')
