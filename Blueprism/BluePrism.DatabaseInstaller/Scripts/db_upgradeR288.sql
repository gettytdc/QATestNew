alter table BPAWorkQueueItem add
    locktime datetime NULL,
    lockid uniqueidentifier null;
GO

alter procedure usp_getnextcase
    @queuename NVARCHAR(255), 
    @keyfilter NVARCHAR(255)    = NULL, 
    @sess      UNIQUEIDENTIFIER = NULL, 
    @ontag1    NVARCHAR(255)    = NULL, 
    @ontag2    NVARCHAR(255)    = NULL, 
    @ontag3    NVARCHAR(255)    = NULL, 
    @ontag4    NVARCHAR(255)    = NULL, 
    @ontag5    NVARCHAR(255)    = NULL, 
    @ontag6    NVARCHAR(255)    = NULL, 
    @ontag7    NVARCHAR(255)    = NULL, 
    @ontag8    NVARCHAR(255)    = NULL, 
    @ontag9    NVARCHAR(255)    = NULL, 
    @offtag1   NVARCHAR(255)    = NULL, 
    @offtag2   NVARCHAR(255)    = NULL, 
    @offtag3   NVARCHAR(255)    = NULL, 
    @offtag4   NVARCHAR(255)    = NULL, 
    @offtag5   NVARCHAR(255)    = NULL, 
    @offtag6   NVARCHAR(255)    = NULL, 
    @offtag7   NVARCHAR(255)    = NULL, 
    @offtag8   NVARCHAR(255)    = NULL, 
    @offtag9   NVARCHAR(255)    = NULL
AS
     DECLARE @sql NVARCHAR(MAX);
     DECLARE @params NVARCHAR(MAX);

     DECLARE @selected TABLE
     (encryptid INT, 
      id        UNIQUEIDENTIFIER, 
      ident     BIGINT, 
      keyvalue  NVARCHAR(255), 
      data      NVARCHAR(MAX), 
      STATUS    NVARCHAR(255), 
      attempt   INT, 
      lockid    UNIQUEIDENTIFIER, 
      locktime  DATETIME, 
      sessionid UNIQUEIDENTIFIER
     );

     -- This lock id ensures we can identify the lock record in
     -- BPACaseLock after we've created it
     DECLARE @lockid UNIQUEIDENTIFIER;
     SET @lockid = NEWID();

     -- Get the tags first. Note that we don't need to worry about
     -- 'virtual' tags for get next. At the moment, the only virtual tag
     -- is an exception tag: if it has an exception on it, it is not
     -- eligible for the get next call

     IF OBJECT_ID('tempdb..#ontags') IS NOT NULL
         DROP TABLE #ontags;
     IF OBJECT_ID('tempdb..#offtags') IS NOT NULL
         DROP TABLE #offtags;
     CREATE TABLE #ontags(id INT);
     CREATE TABLE #offtags(id INT);

     -- Any non-wildcard tags, we put into #ontags - wildcarded tags need to be handled separately.
     -- (Note: only for 'on' tags;
     IF @ontag1 IS NOT NULL
        AND @ontag1 NOT LIKE '%[_%]%'
         INSERT INTO #ontags
         EXEC usp_gettagids @tag = @ontag1;
     IF @ontag2 IS NOT NULL AND @ontag2 NOT LIKE '%[_%]%'
         INSERT INTO #ontags
         EXEC usp_gettagids @tag = @ontag2;
     IF @ontag3 IS NOT NULL AND @ontag3 NOT LIKE '%[_%]%'
         INSERT INTO #ontags
         EXEC usp_gettagids @tag = @ontag3;
     IF @ontag4 IS NOT NULL AND @ontag4 NOT LIKE '%[_%]%'
         INSERT INTO #ontags 
         EXEC usp_gettagids @tag = @ontag4;
     IF @ontag5 IS NOT NULL AND @ontag5 NOT LIKE '%[_%]%'
         INSERT INTO #ontags
         EXEC usp_gettagids @tag = @ontag5;
     IF @ontag6 IS NOT NULL AND @ontag6 NOT LIKE '%[_%]%'
         INSERT INTO #ontags
         EXEC usp_gettagids @tag = @ontag6;
     IF @ontag7 IS NOT NULL AND @ontag7 NOT LIKE '%[_%]%'
         INSERT INTO #ontags
         EXEC usp_gettagids @tag = @ontag7;
     IF @ontag8 IS NOT NULL AND @ontag8 NOT LIKE '%[_%]%'
         INSERT INTO #ontags
         EXEC usp_gettagids @tag = @ontag8;
     IF @ontag9 IS NOT NULL AND @ontag9 NOT LIKE '%[_%]%'
         INSERT INTO #ontags
         EXEC usp_gettagids @tag = @ontag9;

     -- If there are any nulls in #ontags - that implies the tag doesn't exist;
     -- if the tag doesn't exist, no work case can have it applied, ergo there are
     -- no cases with that tag, might as well exit now with nothing
     IF EXISTS
     (
         SELECT 1
         FROM #ontags
         WHERE id IS NULL
     )
         RETURN;

     -- The off tags all go into #offtags. Note that we don't need to separate 'static' and
     -- 'wildcarded' tags here due to the nature of the test we're doing (ie. if there are *any*
     -- tag matches we discard the item)
     IF @offtag1 IS NOT NULL
         INSERT INTO #offtags
         EXEC usp_gettagids @tag = @offtag1;
     IF @offtag2 IS NOT NULL
         INSERT INTO #offtags
         EXEC usp_gettagids @tag = @offtag2;
     IF @offtag3 IS NOT NULL
         INSERT INTO #offtags
         EXEC usp_gettagids @tag = @offtag3;
     IF @offtag4 IS NOT NULL
         INSERT INTO #offtags
         EXEC usp_gettagids @tag = @offtag4;
     IF @offtag5 IS NOT NULL
         INSERT INTO #offtags
         EXEC usp_gettagids @tag = @offtag5;
     IF @offtag6 IS NOT NULL
         INSERT INTO #offtags
         EXEC usp_gettagids @tag = @offtag6;
     IF @offtag7 IS NOT NULL
         INSERT INTO #offtags
         EXEC usp_gettagids @tag = @offtag7;
     IF @offtag8 IS NOT NULL
         INSERT INTO #offtags
         EXEC usp_gettagids @tag = @offtag8;
     IF @offtag9 IS NOT NULL
         INSERT INTO #offtags
         EXEC usp_gettagids @tag = @offtag9;

     -- if there are nulls in #offtags, we can exclude that restriction from the
     -- search - there are no cases with any tags which don't exist in the database
     DELETE FROM #offtags
     WHERE id IS NULL;

     -- Note that this disables locks for the purposes of this operation - that disabling is
     -- only in place while the dynamic sql is running and it reverts to the database default
     -- after the SQL has been completed.
     -- This is safe since we are using a 'soft lock' - ie. a lock implemented by our
     -- database schema to ensure the safety of the case lock, and we want to remove
     -- contention with other aspects of the queue management.
     SET @sql = '
    set transaction isolation level read uncommitted;
    with cte AS 
    (select top(1) i.*
      from BPAWorkQueueItem i
        join BPAWorkQueue q on i.queueident = q.ident
      where i.lockid is null
        and q.name = @queuename
        and q.running = 1
        and i.finished is null and (i.deferred is null or i.deferred < getutcdate()) /* ie. pending */';
     IF @keyfilter IS NOT NULL
         BEGIN
             SET @sql = @sql + '
        and i.keyvalue = @keyfilter';
     END;
     IF EXISTS
     (
         SELECT 1
         FROM #ontags
     )
         BEGIN
             SET @sql = @sql + '
        and (
          select count(*)
            from BPAWorkQueueItemTag it
              join #ontags ot on it.tagid = ot.id
          where it.queueitemident = i.ident
        ) = (select count(*) from #ontags)
        ';
     END;
     -- I really don't know how else to approach this - it's clunky and ugly, but
     -- at least all the clunk is in the building of the dynamic SQL and not in the
     -- execution of the query itself (my other option was using a cursor on an
     -- #onwildcardtags table, which would clunk up the query even more)
     DECLARE @onwildcardsql NVARCHAR(MAX);
     SET @onwildcardsql = '
        and exists (
            select 1
            from BPAWorkQueueItemTag it
              join BPATag t on it.tagid = t.id
            where it.queueitemident = i.ident
              and t.tag like @ontag';
     IF @ontag1 LIKE '%[_%]%'
         SET @sql = @sql + @onwildcardsql + '1)';
     IF @ontag2 LIKE '%[_%]%'
         SET @sql = @sql + @onwildcardsql + '2)';
     IF @ontag3 LIKE '%[_%]%'
         SET @sql = @sql + @onwildcardsql + '3)';
     IF @ontag4 LIKE '%[_%]%'
         SET @sql = @sql + @onwildcardsql + '4)';
     IF @ontag5 LIKE '%[_%]%'
         SET @sql = @sql + @onwildcardsql + '5)';
     IF @ontag6 LIKE '%[_%]%'
         SET @sql = @sql + @onwildcardsql + '6)';
     IF @ontag7 LIKE '%[_%]%'
         SET @sql = @sql + @onwildcardsql + '7)';
     IF @ontag8 LIKE '%[_%]%'
         SET @sql = @sql + @onwildcardsql + '8)';
     IF @ontag9 LIKE '%[_%]%'
         SET @sql = @sql + @onwildcardsql + '9)';
     IF EXISTS
     (
         SELECT 1
         FROM #offtags
     )
         BEGIN
             SET @sql = @sql + '
        and not exists (
          select 1
            from BPAWorkQueueItemTag it
              join #offtags ot on it.tagid = ot.id
          where it.queueitemident = i.ident
        )
        ';
     END;
     SET @sql = @sql + '
        order by i.priority, i.loaded, i.ident) 
        update a 
    set locktime = getutcdate(),
        lockid = @lockid
      OUTPUT inserted.encryptid, inserted.id, inserted.ident, inserted.keyvalue, inserted.data, inserted.status, inserted.attempt, inserted.lockid, inserted.locktime, inserted.sessionid 
      FROM cte a;
     ';

     -- The params that we need to pass for the above sql
     SET @params = '
        @sess uniqueidentifier,
        @queuename nvarchar(255),
        @keyfilter nvarchar(255),
        @lockid uniqueidentifier,
        @ontag1 nvarchar(255),
        @ontag2 nvarchar(255),
        @ontag3 nvarchar(255),
        @ontag4 nvarchar(255),
        @ontag5 nvarchar(255),
        @ontag6 nvarchar(255),
        @ontag7 nvarchar(255),
        @ontag8 nvarchar(255),
        @ontag9 nvarchar(255)
    ';

     -- Attempt to lock the next item a maximum of 100 times before bailing
     DECLARE @attempt INT, @maxattempts INT;
     SET @maxattempts = 100;
     SET @attempt = 1;
     WHILE @attempt <= @maxattempts
         BEGIN
             BEGIN TRY
                 -- Actually call the SQL to create the lock record
                 BEGIN TRAN;
                 INSERT INTO @selected
                 EXEC sp_executesql 
                      @sql, 
                      @params, 
                      @sess = @sess, 
                      @queuename = @queuename, 
                      @keyfilter = @keyfilter, 
                      @lockid = @lockid, 
                      @ontag1 = @ontag1, 
                      @ontag2 = @ontag2, 
                      @ontag3 = @ontag3, 
                      @ontag4 = @ontag4, 
                      @ontag5 = @ontag5, 
                      @ontag6 = @ontag6, 
                      @ontag7 = @ontag7, 
                      @ontag8 = @ontag8, 
                      @ontag9 = @ontag9;
                 IF(EXISTS
                 (
                     SELECT 1
                     FROM @selected
                 ))
                     BEGIN
                         COMMIT;
                 END;
                     ELSE
                     BEGIN
                         /*No error but no result so exit*/
                         ROLLBACK;
                         RETURN;
                 END;
                 --Before we add our lock to the CaseLocks and return the object, make sure the lockId on the item matches after the update
                 IF(EXISTS
                 (
                     SELECT 1
                     FROM BPAWorkQueueItem
                     WHERE lockID = @lockId
                 ))
                     BREAK;
             END TRY
             BEGIN CATCH
                 IF ERROR_NUMBER() = 2627       -- primary constraint error
                    OR ERROR_NUMBER() = 1205    -- deadlock victim
                     BEGIN  
                         --if we have an uncommitted transaction - roll it back
                         IF(@@trancount > 0)
                         BEGIN
                            ROLLBACK;
                         END;
                         -- Increment the attempt counter and try again
                         SET @attempt = @attempt + 1;
                         IF @attempt <= @maxattempts
                             CONTINUE;
                         -- If we've overshot the max attempts, just rethrow the last error
                 END;
                 -- Otherwise "rethrow" (which doesn't become a keyword until SQL Server 2012)

                 EXEC usp_rethrow;
                 RETURN;
             END CATCH;
         END;

     --Insert the record into The Caselocks table, but only if we have a result
     IF(EXISTS
     (
         SELECT 1
         FROM @selected
     ))
         BEGIN
             INSERT INTO BPACaseLock
             (id, 
              locktime, 
              sessionid, 
              lockid
             )
                    SELECT i.ident, 
                           i.locktime, 
                           sessionid = CASE
                                           WHEN @sess IS NULL
                                           THEN i.sessionid
                                           ELSE @sess
                                       END, 
                           @lockid
                    FROM @selected i
                    WHERE i.lockid = @lockid;
     END;

     -- Join the lock record and get the queue / case details to return
     SELECT i.encryptid, 
            i.id, 
            i.ident, 
            i.keyvalue, 
            i.data, 
            i.STATUS, 
            i.attempt
     FROM @selected i;
GO

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('288',
       getutcdate(),
       'db_upgradeR288.sql',
       'Improve robustness of work queue item locking.',
       0);
