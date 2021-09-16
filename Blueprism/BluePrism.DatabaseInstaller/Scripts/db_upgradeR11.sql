/* THIS IS A DATABASE UPGRADE SCRIPT USED FOR THE ADDITION OF BPASTATISTICS CG - 03/09/2004
    IT UPGRADES AN R10 database to R11
*/

/* The primary key was added under bug #8443. For existing databases, this
   constraint is added later if it doesn't exist. */

CREATE TABLE [BPAStatistics] (
    [sessionid] [uniqueidentifier] NOT NULL,
    [name] [varchar] (50) NOT NULL,
    [datatype] [varchar] (32) NULL,
    [value_text] [varchar] (255) NULL,
    [value_number] [float] NULL,
    [value_currency] [money] NULL,
    [value_date] [datetime] NULL,
    [value_flag] [bit] NULL,
    CONSTRAINT PK_BPAStatistics primary key (sessionid, name)
);

/* DB Version */
insert into BPADBVersion values ('11',GETUTCDATE(),'db_upgradeR11.sql UTC','Database amendments - added BPAStatistics')
