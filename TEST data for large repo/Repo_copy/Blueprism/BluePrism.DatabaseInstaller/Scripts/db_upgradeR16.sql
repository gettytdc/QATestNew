/* THIS IS A DATABASE UPGRADE SCRIPT USED FOR AMENDING BPAStatistics CS - 15/04/2005
    IT UPGRADES A R15 database to R16
*/

alter table [BPAStatistics] drop column value_currency

/* DB Version */
insert into BPADBVersion values ('16',GETUTCDATE(),'db_upgradeR16.sql UTC','Database amendments - removes currency field from BPAStatistics')
