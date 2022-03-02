/* THIS IS A DATABASE UPGRADE SCRIPT USED FOR AMENDING BPAUser JC - 06/05/2005 */

ALTER TABLE BPAUser ADD isdeleted BIT DEFAULT 0


/* DB Version */
insert into BPADBVersion values ('18',GETUTCDATE(),'db_upgradeR18.sql UTC','Database amendments - adds a IsDeleted column to the BPAUser table')
