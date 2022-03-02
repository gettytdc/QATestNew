/* THIS IS A DATABASE UPGRADE SCRIPT USED FOR RESOURCE AVAILABILTY CG - 06/05/2005
    IT UPGRADES A R14 database to R15
*/

ALTER TABLE [BPAResource] WITH NOCHECK ADD 
    availability [varchar] (16) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
    
/* DB Version */
insert into BPADBVersion values ('15',GETUTCDATE(),'db_upgradeR15.sql UTC','Database amendments - added resource availability support')
