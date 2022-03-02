/* THIS IS A DATABASE UPGRADE SCRIPT USED FOR ADDING a DEBUGGING STATUS TO BPAStatus GB - 30/03/2005
    IT UPGRADES A R13 database to R14
*/

insert into BPAStatus values (5,'RUN','Debugging');

/* DB Version */
insert into BPADBVersion values ('14',GETUTCDATE(),'db_upgradeR14.sql UTC','Database amendments - added debugging status to BPAStatus')