/*
SCRIPT         : 106
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GB 
PURPOSE        : Adds password rules table

*/

CREATE TABLE BPAPasswordRules
    (
    id int NOT NUll default 1
        constraint PK_BPAPasswordRules primary key,
    uppercase bit NOT NULL,
    lowercase bit NOT NULL,
    digits bit NOT NULL,
    special bit NOT NULL,
    brackets bit NOT NULL,
    length int NOT NULL,
    additional varchar(128) NOT NULL
    )

GO

INSERT INTO BPAPasswordRules VALUES (
  1,
  0,
  0,
  0,
  0,
  0,
  0,
  ''
)
    
-- set DB version
INSERT INTO BPADBVersion VALUES (
  '106',
  GETUTCDATE(),
  'db_upgradeR106.sql UTC',
  'Adds password rules table' 
);

