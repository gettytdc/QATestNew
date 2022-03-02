/*
BUG/STORY      : US-1683 and other stories related to EP-76
PURPOSE        : Add Web API tables, modify the existing web service permission and
                 add new Web API permission
*/

declare @OldSoapPermName as nvarchar(50)
       ,@NewSoapPermName as nvarchar(50)
       ,@WebApiPermName as nvarchar(50)
       ,@WebApiPermId as int;

set @OldSoapPermName = N'Business Objects - Web Services';
set @NewSoapPermName = N'Business Objects - SOAP Web Services';
set @WebApiPermName = N'Business Objects - Web API Services';

--Update permission name
update BPAPerm
    set name = @NewSoapPermName
where name = @OldSoapPermName;

--Insert new permission
insert into BPAPerm (name)
    values (@WebApiPermName);

set @WebApiPermId = scope_identity();

--insert new permission into BPAUserRolePerm
insert into BPAUserRolePerm (userroleid, permid)
    select urp.userroleid
          ,@WebApiPermId
    from BPAUserRolePerm urp
        join BPAPerm p on urp.permid = p.id
    where p.name = @NewSoapPermName;

-- ... and BPAPermGroupMember
insert into BPAPermGroupMember (permgroupid, permid)
    select pgm.permgroupid
          ,@WebApiPermId
    from BPAPermGroupMember pgm
        join BPAPerm p on pgm.permid = p.id
    where p.name = @NewSoapPermName;

--Create web api service tables
create table BPAWebApiService (
    serviceid uniqueidentifier not null
        constraint PK_BPAWebApiService primary key,
    name nvarchar(128) null
        constraint UNQ_BPAWebApiService_name unique,
    enabled bit not null,
    lastupdated datetime not null,
    baseurl nvarchar(max) null,
    authenticationtype int not null,
    authenticationconfig nvarchar(max) not null,
    commoncodeproperties nvarchar(max) not null default '',
    httpRequestConnectionTimeout int not null default 10,
    authServerRequestConnectionTimeout int not null default 10
);

create table BPAWebApiAction (
    actionid int identity not null
        constraint PK_BPAWebApiAction primary key,
    serviceid uniqueidentifier not null
        constraint FK_BPAWebApiAction_BPAWebApiService
            foreign key references BPAWebApiService (serviceid),
    name nvarchar(255) not null,
    description nvarchar(max) null,
    enabled bit not null,
    requesthttpmethod nvarchar(50) not null,
    requesturlpath nvarchar(max) null,
    requestbodytypeid int not null,
    requestbodycontent nvarchar(max),
    enableRequestOutputParameter bit not null,
    disableSendingOfRequest bit not null,    
    constraint UNQ_BPAWebApiAction_serviceid_name unique (serviceid, name)
);
create index Index_BPAWebApiAction_serviced on BPAWebApiAction(serviceid);

create table BPAWebApiHeader (
    headerid int identity not null
        constraint PK_BPAWebApiHeader primary key,
    serviceid uniqueidentifier null
        constraint FK_BPAWebApiHeader_BPAWebApiService
            foreign key references BPAWebApiService (serviceid),
    actionid int null
        constraint FK_BPAWebApiHeader_BPAWebApiAction
            foreign key references BPAWebApiAction (actionid),
    name nvarchar(max) not null,
    value nvarchar(max) null
);
create index Index_BPAWebApiHeader_serviceid on BPAWebApiHeader(serviceid);
create index Index_BPAWebApiHeader_actionid on BPAWebApiHeader(actionid);

create table BPAWebApiParameter (
    parameterid int not null identity
        constraint PK_BPAWebApiParameter primary key,
    serviceid uniqueidentifier null
        constraint FK_BPAWebApiParameter_BPAWebApiService
            foreign key references BPAWebApiService (serviceid),
    actionid int null
        constraint FK_BPAWebApiParameter_BPAWebApiAction
            foreign key references BPAWebApiAction (actionid),
    name nvarchar(255) not null,
    description nvarchar(max) not null,
    exposetoprocess bit not null,
    datatype nvarchar(16) not null,
    initvalue nvarchar(max) null,
    constraint UNQ_BPAWebApiParameter_serviceid_actionid_name
        unique (serviceid, actionid, name)
);
create index Index_BPAWebApiParameter_serviceid on BPAWebApiParameter(serviceid);
create index Index_BPAWebApiParameter_actionid on BPAWebApiParameter(actionid);

create table BPAWebApiCustomOutputParameter (
    id int not null identity
        constraint PK_BPAWebApiCustomOutputParameter primary key,
    actionid int not null
        constraint FK_BPAWebApiCustomOutputParameter_BPAWebApiAction
            foreign key references BPAWebApiAction (actionid),
    name nvarchar(255) not null,
    path nvarchar(max) not null,
    datatype nvarchar(16) not null
);

create index Index_BPAWebApiCustomOutputParameter_actionid on BPAWebApiCustomOutputParameter(actionid);

-- Package support
create table BPAPackageWebApi(
    packageid int not null
        constraint FK_BPAPackageWebApi_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    webapiid uniqueidentifier not null
        constraint FK_BPAPackageWebApi_BPAWebApiService
            foreign key references BPAWebApiService(serviceid)
            on delete cascade,
    constraint PK_BPAPackageWebApi
        primary key (packageid, webapiid)
);

-- create process dependency table for web apis
create table BPAProcessWebApiDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessWebApiDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refApiName nvarchar(128) not null,
    constraint PK_BPAProcessWebApiDependency primary key (id)
);

-- Value checks
insert into BPAValCheck ([checkid], [catid], [typeid], [description], [enabled])
values
(144, 0, 0, 'Web API Business Object Action input collection mismatch{0}: {1}', 1)
GO

insert into BPAValCheck ([checkid], [catid], [typeid], [description], [enabled])
values
(145, 0, 1, 'The collection stage used{0} in the Web API Business Object Action inputs has no defined fields: {1}', 1)
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '256',
  GETUTCDATE(),
  'db_upgradeR256.sql',
  'Add Web API tables, modify the existing web service permission and add new Web API permission',
  0 -- UTC
);