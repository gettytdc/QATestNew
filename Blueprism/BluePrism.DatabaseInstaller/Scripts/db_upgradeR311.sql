CREATE TABLE [BPADataPipelineOutputConfig](
    [id] [INT] IDENTITY(1,1) NOT NULL,
    [uniquereference] [UNIQUEIDENTIFIER] NOT NULL,
    [name] [nvarchar](255) NOT NULL,
    [issessions] [bit] NOT NULL,
    [isdashboards] [bit] NOT NULL,
    [iscustomobjectdata] [bit] NOT NULL,
    [sessioncols] [nvarchar](max) NULL,
    [dashboardcols] [nvarchar](max) NULL,
    [datecreated] [datetime] NULL,
    [advanced] [nvarchar](max) NULL,
    [type] [nvarchar](50) NULL,
    [isadvanced] [bit] NULL,
    [outputoptions] [nvarchar](max) NULL,
 CONSTRAINT [PK_BPADataPipelineOutputConfig] PRIMARY KEY CLUSTERED (id)
)

CREATE INDEX [Index_BPADataPipelineOutputConfig_Name]
    ON [BPADataPipelineOutputConfig] ([name])

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('311',
       getutcdate(),
       'db_upgradeR311.sql',
       'Add BPADataPipelineOutputConfig table',
       0);
