/*
SCRIPT         : 421
AUTHOR         : Rowland Hill
PURPOSE        : Add table BPAFontOCRPlusPlus
*/


if not exists (select 1 from information_schema.columns where table_name = 'BPAFontOCRPlusPlus')
begin
    set ansi_nulls on

    set quoted_identifier on

    create table BPAFontOCRPlusPlus (
        [name] [nvarchar](255) not null,
        [version] [nvarchar](255) not null,
        [fontdata] [nvarchar](max) not null,
     constraint [pk_bpafontocrplusplus] primary key clustered 
    (
        [name] asc
    )with (pad_index = off, statistics_norecompute = off, ignore_dup_key = off, allow_row_locks = on, allow_page_locks = on) on [primary]
    ) on [primary] textimage_on [primary]
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
('421', 
 GETUTCDATE(), 
 'db_upgradeR421.sql', 
 'Add Add table BPAFontOCRPlusPlus', 
 0
);