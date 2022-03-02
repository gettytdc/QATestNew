CREATE TABLE [BPAScope](
    [id] int IDENTITY NOT NULL CONSTRAINT [PK_BPAScope] PRIMARY KEY,
    [scope] nvarchar(255) NOT NULL)

CREATE TABLE [BPAPermScope]
    ([id] int IDENTITY NOT NULL CONSTRAINT [PK_BPAPermScope] PRIMARY KEY,
    [permid] int NOT NULL CONSTRAINT [FK_BPAPermScope_BPAPerm] FOREIGN KEY REFERENCES BPAPerm(id),
    [scopeid] int NOT NULL CONSTRAINT [FK_BPAPermScope_BPAScope] FOREIGN KEY REFERENCES BPAScope(id))

INSERT INTO BPADBVersion(
    dbversion,
    scriptrundate,
    scriptname,
    [description],
    timezoneoffset)

VALUES('320',
    GETUTCDATE(),
    'db_upgradeR320sql',
    'Add permission and scope tables.',
    0);