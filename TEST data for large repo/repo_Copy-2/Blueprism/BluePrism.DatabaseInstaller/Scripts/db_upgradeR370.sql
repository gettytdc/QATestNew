declare @MIReportingCommandTimeoutPreference varchar(100) = 'MIReporting.CommandTimeout.InSeconds';
declare @DefaultCommandTimeoutInSeconds int = 30;
declare @prefId int;

-- Insert preference value for MI Reporting Command Timeout.
if not exists (
		select 1
		from BPAPref
		where [name] = @MIReportingCommandTimeoutPreference
			and userid is null
		)
begin
	insert into BPAPref ([name])
	values (@MIReportingCommandTimeoutPreference);
end

set @prefId = (
		select top 1 id
		from BPAPref
		where [name] = @MIReportingCommandTimeoutPreference
			and userid is null
		);

if not exists (
		select 1
		from BPAIntegerPref
		where prefid = @prefId
		)
begin
	insert into BPAIntegerPref (
		prefid,
		[value]
		)
	values (
		@prefId,
		@DefaultCommandTimeoutInSeconds
		);
end

-- Set DB version.
insert into BPADBVersion (
	[dbversion],
	[scriptrundate],
	[scriptname],
	[description],
	[timezoneoffset]
	)
values (
	'370',
	getutcdate(),
	'db_upgradeR370.sql',
	'Add MIRefresh Command Timeout preference values to BPAPref table.',
	0
	);