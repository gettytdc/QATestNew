/*
SCRIPT         : 53
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 04 May 2007
AUTHOR         : JC
PURPOSE        : Process alerts changes: new permission and role, new columns to BPAUser 
                 and new tables BPAAlert and BPAAlertEvent
NOTES          : 
*/

-- Two new permissions
INSERT INTO BPAPermission VALUES (128,'Subscribe to Process Alerts')
INSERT INTO BPAPermission VALUES (256,'Configure Process Alerts')
INSERT INTO BPAPermission VALUES (256+128,'Process Alerts')

--New role
INSERT INTO BPARole VALUES (64, 'Alert Subscriber', 128 + 256)

-- New columns for user config options
ALTER TABLE BPAUser ADD AlertEventTypes INTEGER NULL, AlertNotificationTypes INTEGER NULL

-- New table for alert details
CREATE TABLE [BPAAlertEvent] (
    [AlertEventID] [int] IDENTITY (1, 1) NOT NULL ,
    [AlertEventType] [int] NULL ,
    [AlertNotificationType] [int] NULL ,
    [Message] [varchar] (500) NULL ,
    [ProcessID] [uniqueidentifier] NULL ,
    [ResourceID] [uniqueidentifier] NULL ,
    [SessionID] [uniqueidentifier] NULL ,
    [Date] [datetime] NULL ,
    [SubscriberUserID] [uniqueidentifier] NULL ,
    [SubscriberResourceID] [uniqueidentifier] NULL ,
    [SubscriberDate] [datetime] NULL ,
    CONSTRAINT [PK_BPAAlertEvent] PRIMARY KEY  CLUSTERED 
    (
        [AlertEventID]
    ) 
)

-- New table for user process alert subscriptions
CREATE TABLE [BPAAlert] (
    [UserID] [uniqueidentifier] NOT NULL ,
    [ProcessID] [uniqueidentifier] NOT NULL ,
    CONSTRAINT [PK_BPAAlert] PRIMARY KEY  CLUSTERED 
    (
        [UserID],
        [ProcessID]
    ),
    CONSTRAINT [FK_BPAAlert_BPAProcess] FOREIGN KEY 
    (
        [ProcessID]
    ) REFERENCES [BPAProcess] (
        [processid]
    ),
    CONSTRAINT [FK_BPAAlert_BPAUser] FOREIGN KEY 
    (
        [UserID]
    ) REFERENCES [BPAUser] (
        [userid]
    )
)

--set DB version
INSERT INTO BPADBVersion VALUES (
  '53',
  GETUTCDATE(),
  'db_upgradeR53.sql UTC',
  'Process alerts changes: new columsn to BPAUser and new tables BPAAlert and BPAAlertEvent'
)
