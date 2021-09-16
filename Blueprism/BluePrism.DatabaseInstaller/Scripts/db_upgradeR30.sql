/* ABOUT THIS SCRIPT

SCRIPT         : Special script for release with V2.0 to customers who have already run Version 1.1
PROJECT NAME   : Automate
DATABASE NAME  : 
CREATION DATE  : Jan 2006
AUTHOR         : PJW
PURPOSE        : Changes layout of BPASessionLog table to allow logging of stageID in each row.
NOTES          : Originally the new stageID column was simply added to the end of the
                table. This caused a performance issue because it was after a variable-width
                column. This script is intended to insert the new column in an earlier position
                by copying the table's contents into a temporary table and then recreating the
                table in its new form.
                
                However for practical considerations, this table will only be updated if it contains
                no data. If it contains data then another script will be run manually.
                
*/

                

--only take action if there is no data in the table
IF (SELECT COUNT(a.Seqnum) FROM (SELECT TOP 1 Seqnum FROM BPASESSIONLOG) a) = 0
    BEGIN

        --remove the original table (Note that this removes existing
        --indexes, foreign keys etc so they all need to be put back)
        drop table bpasessionlog


        --now recreate the table with the columns in the correct order
        CREATE TABLE [BPASessionLog] (
            [logid] [uniqueidentifier] NOT NULL ,
            [sessionid] [uniqueidentifier] NULL ,
            [seqnum] [int] NULL ,
            [StageID] [uniqueidentifier] NULL ,
            [logdatetime] [datetime] NULL ,
            [message] [varchar] (2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
        )
        

        --recreate the index
        CREATE INDEX [Index_sessionID] ON [BPASessionLog]([sessionid], [seqnum])


        --and recreate the foreign keys
        ALTER TABLE [BPASessionLog] WITH NOCHECK ADD 
            CONSTRAINT [PK_BPASessionLog] PRIMARY KEY  CLUSTERED 
            (
                [logid]
            )

        ALTER TABLE [BPASessionLog] ADD 
            CONSTRAINT [FK_BPASessionLog_BPASession] FOREIGN KEY 
            (
                [sessionid]
            ) REFERENCES [BPASession] (
                [sessionid]
            )
        ALTER TABLE [BPASessionLog] NOCHECK CONSTRAINT [FK_BPASessionLog_BPASession]

    END


--Finally, set DB version as in all other automatic scripts
INSERT INTO BPADBVersion VALUES (
    '30',
    GETUTCDATE(),
    'db_upgradeR30.sql UTC',
    'Database amendments - Add new stageid column to BPASessionLog.'
    )
        
