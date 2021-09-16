/*
SCRIPT         : 111
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Bug 4868 : Added a view to allow 'virtual tags' to be searched transparently
*/

-- BPViewWorkQueueItemTag brings together the item's identity and its tags, including
-- the 'virtual tag' of its exception reason (if it has one).
-- The exception reason is truncated to 1000 characters, prefixed with "Exception: "
-- and has any semi-colons (illegal characters in tags) replaced with colons.
create view BPViewWorkQueueItemTag (queueitemident, tag)
as
    select it.queueitemident, t.tag
    from BPAWorkQueueItemTag it
        join BPATag t on it.tagid = t.id
union
    select i.ident, 'Exception: ' + REPLACE(cast(i.exceptionreason as varchar(1000)),';',':')
    from BPAWorkQueueItem i
    where i.exceptionreason is not null
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '111',
  GETUTCDATE(),
  'db_upgradeR111.sql UTC',
  'Supports virtual tags by creating a view which takes them into account'
);

