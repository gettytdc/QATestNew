/* THIS IS A DATABASE UPGRADE SCRIPT USED FOR THE ADDITIONS TO BPASESSION GB - 28/09/2004
    IT UPGRADES A R11 database to R12
*/

ALTER TABLE [BPASession] WITH NOCHECK ADD 
    startparamsxml text NULL,
    logginglevelsxml text NULL

/* DB Version */
insert into BPADBVersion values ('12',GETUTCDATE(),'db_upgradeR12.sql UTC','Database amendments - added to BPASession startparamsxml and logginlevelsxml')