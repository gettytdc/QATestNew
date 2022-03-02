/*
SCRIPT        : 369
PURPOSE       : Added CertificateExpires to BPAEnvironment and modifies the SP usp_CreateUpdateEnvironmentData to insert the CertificateExpires information into the table
AUTHOR		  : Ian Guthrie
*/

if not exists ( select 1
                from INFORMATION_SCHEMA.columns
                where table_name = 'BPAEnvironment' and column_name = 'CertificateExpires')
begin

	alter table [BPAEnvironment] 
	add [CertificateExpires] [datetime] null
end
go

alter procedure [usp_CreateUpdateEnvironmentData] 
    @environmentTypeId int, 
    @fqdn nvarchar(253),
	@portNumber int = -1,
	@version nvarchar(256),
	@certificateExpires datetime
as
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
        where [FQDN]=@fqdn and [EnvironmentTypeId]=@environmentTypeId and [port]=@portNumber)
    begin
        insert into [BPAEnvironment] ([EnvironmentTypeId], 
                    [FQDN], 
                    [Port], 
                    [Version],
					[CertificateExpires]) 
        values (@environmentTypeId, 
                @fqdn, 
                @portNumber,
                @version,
				@certificateExpires)
    end
    else
    begin
        update [BPAEnvironment] 
            set [Version]=@version,
				[CertificateExpires]=@certificateExpires,
                [UpdatedDateTime]=getutcDate()
        where [FQDN]=@fqdn and [EnvironmentTypeId]=@environmentTypeId and [Port]=@portNumber 
    end
return;
go

-- Set DB version.
insert into BPADBVersion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'369'
	,getutcdate()
	,'db_upgradeR369.sql'
	,'Added CertificateExpires to BPAEnvironment and modifies the SP usp_CreateUpdateEnvironmentData to insert the CertificateExpires information into the table'
	,0
	);
