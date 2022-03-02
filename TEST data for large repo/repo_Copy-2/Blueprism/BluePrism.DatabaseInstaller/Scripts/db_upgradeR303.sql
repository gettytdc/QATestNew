declare @docProcssing as nvarchar(25) = 'DocumentProcessing'
declare @systemAdmin as nvarchar(25) = 'System Administrators'

declare @docProcessingUserRoleId as int = (select top(1) id from BPAUserRole ur where ur.requiredFeature = @docProcssing)
declare @systemAdminRoleId as int = (select top(1) id from BPAUserRole ur where ur.name = @systemAdmin)


declare @businessObjectId as int = (select top(1) id from BPAPerm p where p.name = 'Business Objects - Management')

delete BPAUserRolePerm 
where userroleid = @docProcessingUserRoleId and permid = @businessObjectId
         
declare @idTable table(id int)

insert into @idTable
select p.id from BPAPerm p 
where p.requiredFeature = @docProcssing
and p.id not in (select p.permid from BPAUserRolePerm p where p.userroleid = @docProcessingUserRoleId)
        

insert into BPAUserRolePerm
select @docProcessingUserRoleId, i.id from @idTable i

insert into BPAUserRolePerm
select @systemAdminRoleId, i.id from @idTable i

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('303',
       getutcdate(),
       'db_upgradeR303.sql',
       'Add additional document processing permissions.',
       0);


select * from BPADBVersion order by scriptrundate