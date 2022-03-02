create procedure [sp_MoveDBObjectsFromDboToNewSchema]
    @newschema nvarchar(128)
as
begin

    declare @currentschema nvarchar(128), @object nvarchar(128), @sql nvarchar(max);

    declare objectcursor cursor fast_forward for
    select s.name, o.name
    from sys.objects o
    inner join sys.schemas s ON o.schema_id = s.schema_id
    where o.parent_object_id = 0 and o.type not in('IT', 'ET',  'SQ', 'S') and s.name = 'dbo'
	union all
	select s.name, o.name
    from sys.objects o
    inner join sys.schemas s ON o.schema_id = s.schema_id
    where o.type = 'PK' and o.parent_object_id <> 0 and o.type not in('IT', 'ET',  'SQ', 'S') and s.name = 'dbo'
    UNION ALL
    select s.name, o.name
    from sys.objects o
    inner join sys.schemas s ON o.schema_id = s.schema_id
    where o.parent_object_id <> 0 and o.type not in ('PK', 'IT', 'ET',  'SQ', 'S') and s.name = 'dbo';

    open objectcursor;
    fetch next from objectcursor into @currentschema, @object;

    while @@fetch_status = 0
    begin
        set @sql = 'alter schema ' + @newschema + ' transfer [' + @currentschema + '].[' + @object + '];';
        
        exec (@sql);

        fetch next from objectcursor into @currentschema, @object;
    end

    close objectcursor;
    deallocate objectcursor;

end
