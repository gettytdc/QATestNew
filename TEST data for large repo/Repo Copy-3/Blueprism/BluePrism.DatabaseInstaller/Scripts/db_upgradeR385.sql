/*
SCRIPT        : 385
STORY		  : BP-792
PURPOSE       : Added ApplicationServerId to BPAEnvironment and modified the SP usp_CreateUpdateEnvironmentData to insert the ApplicationServerId information into the table
AUTHOR		  : Gary Crosbie
*/

if not exists ( select 1
                from INFORMATION_SCHEMA.columns
                where table_name = 'BPAEnvironment' and column_name = 'ApplicationServerId')
begin
    alter table BPAEnvironment
	add ApplicationServerId int null
	constraint FK_BPAEnvironment_BPAEnvironment
	foreign key (ApplicationServerId) references BPAEnvironment(Id)
end
go

alter procedure usp_CreateUpdateEnvironmentData 
    @environmentTypeId int, 
    @fqdn nvarchar(253),
	@portNumber int = -1,
	@version nvarchar(256),
	@certificateExpires datetime,
	@applicationServerPortNumber int,
    @applicationServerFullyQualifiedDomainName nvarchar(253)
as
if (nullif(ltrim(rtrim(@fqdn)),'') is null)
begin
	raiserror('invalid parameter: @fqdn cannot be blank or whitespace', 18, 0);
	return;
end
if (nullif(ltrim(rtrim(@version)),'') is null)
begin
	raiserror('invalid parameter: @version cannot be blank or whitespace', 18, 0);
	return;
end
if not exists(select 1 from BPAEnvironmentType where id = @environmentTypeId)
begin
	raiserror('invalid parameter: @environmentTypeId is not a valid environment type', 18, 0);
	return;
end

declare @applicationServerId as int;
if (@environmentTypeId <> 2 and @applicationServerPortNumber > 0)
begin
select @applicationServerId = Id from BPAEnvironment where Port = @applicationServerPortNumber and FQDN = @applicationServerFullyQualifiedDomainName and EnvironmentTypeId = 2
end

if not exists (select 1 from BPAEnvironment 
        where FQDN=@fqdn and EnvironmentTypeId=@environmentTypeId and port=@portNumber)
    begin
        insert into BPAEnvironment (EnvironmentTypeId, 
                    FQDN, 
                    Port, 
                    Version,
					CertificateExpires,
					ApplicationServerId) 
        values (@environmentTypeId, 
                @fqdn, 
                @portNumber,
                @version,
				@certificateExpires,
				@applicationServerId)
    end
	else
    begin
        update BPAEnvironment 
            set Version=@version,
				CertificateExpires=@certificateExpires,
                UpdatedDateTime=getutcdate(),
				ApplicationServerId=@applicationServerId
        where FQDN=@fqdn and EnvironmentTypeId=@environmentTypeId and Port=@portNumber 
    end
return;
go

-- Set DB version.
insert into BPADBVersion (
	dbversion
	,scriptrundate
	,scriptname
	,description
	,timezoneoffset
	)
values (
	'385'
	,getutcdate()
	,'db_upgradeR385.sql'
	,'Added ApplicationServerId to BPAEnvironment and modified the SP usp_CreateUpdateEnvironmentData to insert the ApplicationServerId information into the table'
	,0
    );
