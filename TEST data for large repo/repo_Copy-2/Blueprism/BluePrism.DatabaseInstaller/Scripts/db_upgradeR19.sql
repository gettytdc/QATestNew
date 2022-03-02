/* THIS IS A DATABASE UPGRADE SCRIPT USED FOR AMENDING BPAUser JC - 06/05/2005 */

UPDATE BPAUser SET isdeleted = 0

/* DB Version */
insert into BPADBVersion values ('19',GETUTCDATE(),'db_upgradeR19.sql UTC','Database amendments - populates BPAUser.IsDeleted column with default value 0')
