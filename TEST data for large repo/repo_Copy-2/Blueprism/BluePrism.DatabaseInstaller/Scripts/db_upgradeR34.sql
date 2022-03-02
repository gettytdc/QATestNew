
--SCRIPT PURPOSE: New tables and updates to cover View Preferences in realtime stats
--NUMBER: 34
--AUTHOR: PJW
--DATE: 09/11/2005 

--This table will hold details of each view.
create table BPARealTimeStatsView (
    ViewID uniqueidentifier,
    ProcessID uniqueidentifier,
    Flipped bit NOT NULL,
    DataItemsList TEXT,
    BoldValues TEXT,
    CONSTRAINT [PK_BPARealTimeStatsView] PRIMARY KEY CLUSTERED
        (
            [ViewID]
        ),
    CONSTRAINT [FK_BPAProcess] FOREIGN KEY
        (
            [ProcessID]
        )
        REFERENCES BPAProcess
            (
                [ProcessID]
            )
            
)
GO


--This table will record which user prefers which view for which process.
if exists (select * from dbo.sysobjects where id = object_id(N'[BPAUserViewPreferencePerProcess]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAUserViewPreferencePerProcess]
GO
create table BPAUserViewPreferencePerProcess (
    ProcessID uniqueidentifier,
    UserID uniqueidentifier,
    ViewID uniqueidentifier,
    CONSTRAINT [PK_BPAUserViewPreferencePerProcess] PRIMARY KEY  CLUSTERED
    (
        [ProcessID],
        [UserID]
    ),
    CONSTRAINT [FK_BPAUserViewPreferencePerProcess_BPARealTimeStatsView] FOREIGN KEY
    (
        [ViewID]
    )
    REFERENCES BPARealTimeStatsView
    (
        [ViewID]
    )
)
GO

--We allow individual processes to have a default view
alter table bpaprocess add
    DefaultRealTimeStatsView uniqueidentifier,
    CONSTRAINT [FK_BPAProcess_DefaultRealTimeStatsView] FOREIGN KEY 
    (
        [DefaultRealTimeStatsView]
    )
    REFERENCES BPARealtimeStatsView
        (
            [ViewID]
        )
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '34',
  GETUTCDATE(),
  'db_upgradeR34.sql UTC',
  'Database amendments - New tables and updates to cover View Preferences in realtime stats.'
)
