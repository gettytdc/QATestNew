/*
SCRIPT         : 127
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Support for the environment locking business object
*/

create table BPAEnvLock (
    name varchar(255) not null
        constraint PK_BPAEnvLock primary key,
    token varchar(255) null,
    sessionid uniqueidentifier null
        constraint FK_BPAEnvLock_BPASession
        foreign key references BPASession(sessionid),
    locktime datetime null,
    comments varchar(1024) null
);

create index IX_BPAEnvLock_token
    on BPAEnvLock(token);

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '127',
  GETUTCDATE(),
  'db_upgradeR127.sql UTC',
  'Support for the environment locking business object'
);
