/*
SCRIPT         : 353
PURPOSE        : Isolate BPACacheETag upserts
*/

alter procedure [usp_SetCacheETag]
    @cacheKey NVARCHAR(50),
    @tag UNIQUEIDENTIFIER
as
begin
    set nocount on;
	begin try
		begin tran
		if NOT EXISTS (select * from BPACacheETags with (readpast, rowlock, updlock) where [key] = @cacheKey)
			insert into BPACacheETags ([key], [tag]) values (@cacheKey, @tag)
		else
			update BPACacheETags set [tag] = @tag where [key] = @cacheKey
		commit
	end try
	begin catch
		rollback
	end catch
end
go

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('353', 
 GETUTCDATE(), 
 'db_upgradeR353.sql', 
 'BPACacheTag upsert should be thread safe', 
 0
);