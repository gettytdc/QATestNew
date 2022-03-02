/*
SCRIPT         : 155
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds unique constraint to resource name
*/

-- First we need to ensure that there are no duplicate resources
-- This is taken from edgework 5780 - slightly modified to ensure
-- that the rename doesn't clash with previous renames.

-- Two tables used to hold prime (nominated survivor resource) and
-- clone data for duplicate resources

declare @prime table (id uniqueidentifier not null primary key)
declare @clone table (
  ident int not null identity(1,1) primary key,
  cloneid uniqueidentifier not null unique,
  primeid uniqueidentifier not null
)

-- We need to set the identity seed to the count of resources to
-- ensure there is no clash with previously renamed resources.
-- You can't set the seed manually in a table variable, so we go
-- this way. Insert all the resources. Delete all the resources,
-- which should leave the identity seed at 1 + count(resources)
insert into @clone (cloneid,primeid)
  select resourceid, newid() from BPAResource;
delete from @clone;

-- The prime table needs all the latest IDs of resources which have duplicates
-- So get all the resource IDs which have dupes with earlier last updated dates,
-- and filter out all those which are themselves earlier than other dupes.
insert into @prime
  select a.primeid from (
    select distinct r2.resourceid as primeid
    from BPAResource r1
      join BPAResource r2 on r1.name = r2.name and r1.lastupdated < r2.lastupdated
  ) a
  where not exists (
    select 1 from BPAResource ir1
      join BPAResource ir2 on ir1.name = ir2.name and ir1.lastupdated < ir2.lastupdated
    where ir1.resourceid = a.primeid
  )

-- The clone table requires all the IDs which have duplicates for which they
-- are not the prime.
insert into @clone (primeid, cloneid)
  select p.id, r2.resourceid
    from @prime p
      join BPAResource r1 on p.id = r1.resourceid
      join BPAResource r2 on r1.name = r2.name
    where r2.resourceid != p.id;

-- So we now have a table of the 'prime' resources for duplicates, and
-- a linked table containing all of its clones

-- Append an arbitrary number to the resource and retire the clones
update r set 
  r.name = r.name + '-' + cast(c.ident as varchar(31)),
  r.attributeid = (r.attributeid | 1)
from BPAResource r
  join @clone c on r.resourceid = c.cloneid;

-- Actually apply the unique constraint
alter table BPAResource
  add constraint UNQ_BPAResource_name unique (name);

--set DB version
INSERT INTO BPADBVersion VALUES (
  '155',
  GETUTCDATE(),
  'db_upgradeR155.sql UTC',
  'Adds unique constraint to resource name'
)
