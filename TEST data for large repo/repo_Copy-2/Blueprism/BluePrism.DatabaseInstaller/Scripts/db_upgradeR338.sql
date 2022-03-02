/*
SCRIPT         : 338
AUTHOR         : Yasar Bhatti /Craig Mitchell
PURPOSE        : Adding BPAEnvironment table
*/

create table [BPAEnvironmentType](
	[Id] int not null primary key,
	[Name] nvarchar(253) not null,
);
GO

insert into [BPAEnvironmentType] (Id, [Name]) 
values (1, 'Interactive Client'), (2, 'Server'), (3, 'Runtime Resource');

create table [BPAEnvironment](
	[Id] int not null identity(1,1) primary key,
	[EnvironmentTypeId] int not null,
	[FQDN] nvarchar(253) not null,
	[Port] int null,
	[Version] nvarchar(256) not null,
	[CreatedDateTime] datetime not NULL default getutcdate(),
	[UpdatedDateTime] datetime not null default getutcDate(),
	constraint fk_BPAEnvironment_BPAEnvironmentType foreign key (EnvironmentTypeId) references BPAEnvironmentType (Id)
);
GO

create procedure [usp_CreateUpdateEnvironmentData] 
    @environmentTypeId int, 
    @fqdn nvarchar(253),
	@portNumber int = -1,
	@version nvarchar(256)
AS
if (nullif(ltrim(rtrim(@fqdn)),'') is NULL)
begin
	raiserror('invalid parameter: @fqdn cannot be blank or whitespace', 18, 0);
	return;
end
if (nullif(ltrim(rtrim(@version)),'') is null)
begin
	raiserror('invalid parameter: @version cannot be blank or whitespace', 18, 0);
	return;
end
if not exists(select 1 from [BPAEnvironmentType] where id = @environmentTypeId)
begin
	raiserror('invalid parameter: @environmentTypeId is not a valid environment type', 18, 0);
	return;
end

if not exists (select 1 from [BPAEnvironment] 
        where [FQDN]=@fqdn AND [EnvironmentTypeId]=@environmentTypeId and [port]=@portNumber)
    begin
        insert into [BPAEnvironment] ([EnvironmentTypeId], 
                    [FQDN], 
                    [Port], 
                    [Version]) 
        values (@environmentTypeId, 
                @fqdn, 
                @portNumber,
                @version)
    end
    else
    begin
        update [BPAEnvironment] 
            set [Version]=@version, 
                [UpdatedDateTime]=getutcDate() 
        where [FQDN]=@fqdn and [EnvironmentTypeId]=@environmentTypeId and [Port]=@portNumber 
    end
return;
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '338',
  GETUTCDATE(),
  'db_upgradeR338.sql',
  'Adding BPAEnvironment table',0
);