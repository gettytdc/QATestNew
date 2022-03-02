/* THIS IS A DATABASE UPGRADE SCRIPT USED FOR CREATING BPAResourceConfig GB - 04/02/2005
    IT UPGRADES A R12 database to R13
*/

CREATE TABLE BPAResourceConfig
    (
    name varchar(128) NOT NULL,
    config text NULL
    )

ALTER TABLE BPAResourceConfig ADD CONSTRAINT
    PK_BPAResourceConfig PRIMARY KEY CLUSTERED 
    (
    name
    )

/* DB Version */
insert into BPADBVersion values ('13',GETUTCDATE(),'db_upgradeR13.sql UTC','Database amendments - added BPAResourceConfig')
