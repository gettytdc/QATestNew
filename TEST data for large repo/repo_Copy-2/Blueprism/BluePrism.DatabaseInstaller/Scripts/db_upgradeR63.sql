/*

SCRIPT         : 63
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : PJW
PURPOSE        : Remove relationship between BPAAliveResources and BPAResource
                    - resource PCs are for running processes and do not (necessarily)
                    correspond to physical machines used by a user.
*/


DROP TABLE BPAAliveResources
GO
 
CREATE TABLE BPAAliveResources (
    MachineName Varchar(16) NOT NULL,
    UserID uniqueidentifier NOT NULL,
    LastUpdated datetime NOT NULL,
    CONSTRAINT PK_BPAAliveResources PRIMARY KEY CLUSTERED
    (
        [MachineName],
        [UserID]
    ),
    CONSTRAINT FK_BPAAliveResources FOREIGN KEY
    (
        [UserID]
    ) REFERENCES [BPAUser] (
        [UserID]
    )
)
GO


--set DB version
INSERT INTO BPADBVersion VALUES (
  '63',
  GETUTCDATE(),
  'db_upgradeR63.sql UTC',
  'Database amendments - Remove relationship between BPAAliveResources and BPAResource tables.'
)
GO
