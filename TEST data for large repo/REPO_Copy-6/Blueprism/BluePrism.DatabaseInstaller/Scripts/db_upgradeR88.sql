/*
SCRIPT         : 88
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Work Queue Item - change 'attempts' to 'attempt' to signify that
                 each record represents a different attempt. This transfers the
                 representation of '0 attempts so far' to a combination of the 
                 attempt number 1 and a state of pending.
*/

sp_rename 'BPAWorkQueueItem.attempts','attempt','COLUMN'
GO

-- Increment the attempt number of the item if pending - there's no distinction
-- in this column between a pending attempt and a complete attempt..
-- This goes for deferred items too (which will have an attempt no. of 0)
update BPAWorkQueueItem
    set attempt = attempt + 1
    where exception is null and completed is null and locked is null;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '88',
  GETUTCDATE(),
  'db_upgradeR88.sql UTC',
  'BPAWorkQueueItem.attempts => attempt - represents attempt number, not number of attempts'
)
