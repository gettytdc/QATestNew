/*
SCRIPT         : 227
AUTHOR         : NC
PURPOSE        : Ensure all children of a BPASession record are cascade deleted when the session is deleted.
*/

--Remove orphaned rows
delete from BPAAlertEvent
       where sessionid not in (select sessionid from BPASession);

--Alter constraints 
alter table BPACaseLock 
    drop constraint FK_CaseLock_Session;

alter table BPACaseLock 
    add constraint FK_CaseLock_Session 
        foreign key(sessionid)
        references BPASession (sessionid) 
        on Delete Cascade;

alter table BPAAlertEvent 
    add constraint FK_AlertEvent_Session
        foreign key(sessionid)
        references BPASession (sessionid) 
        on delete cascade;

alter table BPAEnvLock 
    drop constraint FK_BPAEnvLock_BPASession;

alter table BPAEnvLock 
    add constraint FK_BPAEnvLock_BPASession 
        foreign key(sessionid)
        references BPASession (sessionid) 
        on delete cascade;
        
-- set DB version
INSERT INTO BPADBVersion VALUES (
  '227',
  GETUTCDATE(),
  'db_upgradeR227.sql',
  'Ensure all children of a BPASession record are cascade deleted when the session is deleted.',
  0 -- UTC
);


