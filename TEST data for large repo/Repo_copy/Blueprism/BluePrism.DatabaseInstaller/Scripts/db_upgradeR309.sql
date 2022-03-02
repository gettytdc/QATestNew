IF COL_LENGTH('BPAWorkQueue','requiredFeature') IS NULL
BEGIN
    ALTER TABLE [BPAWorkQueue]
    ADD [requiredFeature] NVARCHAR(100) NOT NULL CONSTRAINT BPAWorkQueue_default_requiredFeature DEFAULT ''
END

GO

UPDATE [BPAWorkQueue]
SET [requiredFeature] = 'DocumentProcessing'
WHERE [name] = 'Decipher Queue'

GO

ALTER view [BPVGroupedQueues] as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    q.ident as id,
    q.name as name,
    q.id as guid,
    q.running as running,
    q.encryptid as encryptid,
    q.processid as processid,
    q.resourcegroupid as resourcegroupid,
    q.requiredFeature as requiredFeature,
    case
      when q.processid is not null and q.resourcegroupid is not null then cast(1 as bit)
      else cast(0 as bit)
    end as isactive
    from BPAWorkQueue q
      left join (
        BPAGroupQueue gq
            inner join BPAGroup g on gq.groupid = g.id
      ) on gq.memberid = q.ident;

GO

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('309',
       getutcdate(),
       'db_upgradeR309.sql',
       'Add required feature column to BPAWorkQueue.',
       0);


select * from BPADBVersion order by scriptrundate