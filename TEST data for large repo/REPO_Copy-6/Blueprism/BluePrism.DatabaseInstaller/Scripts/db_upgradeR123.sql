/*
SCRIPT         : 123
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Introduces a basic preferences system.
*/

-- The BPAPref table
-- A user pref is defined with a name and a user ID
-- A system pref is defined with a name and a null ID
-- You can only have one pref with a given name within
-- each scope - ie. system / specific user
create table BPAPref (
    id int identity not null primary key,
    name varchar(255) not null,
    userid uniqueidentifier null
        constraint FK_BPAPref_BPAUser
            foreign key references BPAUser(userid)
            on delete cascade,
    constraint UNQ_BPAPref_name_userid
        unique (name, userid)
);

-- A pref with an integer value.
create table BPAIntegerPref (
    prefid int not null
        constraint FK_BPAIntegerPref_BPAPref
        foreign key references BPAPref(id)
        on delete cascade,
    value int not null
);

-- A pref with a string value
create table BPAStringPref (
    prefid int not null
        constraint FK_BPAStringPref_BPAPref
        foreign key references BPAPref(id)
        on delete cascade,
    value text not null
);

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '123',
  GETUTCDATE(),
  'db_upgradeR123.sql UTC',
  'Introduces a basic preferences system'
);
