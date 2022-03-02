declare @docPermissions table([name] nvarchar(100))

insert into @docPermissions(name)
values('ViewBatch')

insert into @docPermissions(name)
values('DeleteBatch')

insert into @docPermissions(name)
values('VerifyBatch')

insert into @docPermissions(name)
values('ViewBatchOutput')

insert into @docPermissions(name)
values('ModifyDocument')

insert into @docPermissions(name)
values('ViewPage')

insert into @docPermissions(name)
values('ModifyFormData')

insert into @docPermissions(name)
values('CreateFieldTemplate')

insert into @docPermissions(name)
values('ModifyFieldTemplate')

insert into @docPermissions(name)
values('ViewFieldTemplate')

insert into @docPermissions(name)
values('DeleteFieldTemplate')

insert into @docPermissions(name)
values('CreateUserAccount')

insert into @docPermissions(name)
values('ViewUserAccount')

insert into @docPermissions(name)
values('CreateBatchType')

insert into @docPermissions(name)
values('ModifyBatchType')

insert into @docPermissions(name)
values('DeleteBatchType')

insert into @docPermissions(name)
values('CreateDocumentType')

insert into @docPermissions(name)
values('ModifyDocumentType')

insert into @docPermissions(name)
values('ViewDocumentType')

insert into @docPermissions(name)
values('DeleteDocumentType')

delete @docPermissions where name in (select name from BPAPerm)

-- insert document processing permissions and get id's
declare @idTable table(id int)

insert into BPAPerm (name, treeid, requiredFeature)
output inserted.id into @idTable
select name, null, 'DocumentProcessing' from @docPermissions

-- get the document processing group id
declare @permGroupId int = (select top 1 p.id 
                            from BPAPermGroup p 
                            where p.name = 'Document Processing')

-- insert the group id and the new id's into permission group table
insert into BPAPermGroupMember (permgroupid, permid)
select @permGroupId, id from @idTable

declare @roleId int = (select top 1 id 
                       from BPAUserRole 
                       where name = 'Document Processing User')

-- create a temporary table to hold userrole and permission id's
declare @userRolePerm table(userroleid int, permid int)

insert into @userRolePerm
select @roleId, i.id from @idTable i

-- remove any userrole/permission assignments already existing
delete @userRolePerm
from @userRolePerm temp 
    join BPAUserRolePerm urp 
    on temp.permid = urp.permid and temp.userroleid = urp.userroleid

insert into BPAUserRolePerm
select rp.userRoleId, rp.permId from @userRolePerm rp

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'activationdate'
          AND Object_ID = Object_ID(N'BPALicense'))
BEGIN
    ALTER TABLE BPALicense
     ADD activationdate datetime NULL
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'activatedby'
          AND Object_ID = Object_ID(N'BPALicense'))
BEGIN
    ALTER TABLE BPALicense
     ADD activatedby uniqueidentifier NULL
END
GO
-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('293',
       getutcdate(),
       'db_upgradeR293.sql',
       'Add additional document processing permissions.',
       0);
