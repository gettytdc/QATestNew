-- Add BPASnapshotConfiguration table
CREATE TABLE BPASnapshotConfiguration (
    id INT IDENTITY,
    interval INT NOT NULL DEFAULT 2,
    name NVARCHAR(255),
    timezone VARCHAR(255),
    startsecsaftermidnight INT NOT NULL,
    endsecsaftermidnight INT NOT NULL,
    sunday BIT,
    monday BIT,
    tuesday BIT,
    wednesday BIT,
    thursday BIT,
    friday BIT,
    saturday BIT,
    isenabled BIT,  
    CONSTRAINT PK_BPASnapshotConfiguration PRIMARY KEY (id)
)

-- Add columns to the BPAWorkQueue table
ALTER TABLE BPAWorkQueue
     ADD lastsnapshotid BIGINT,
         snapshotconfigurationid INT,
         CONSTRAINT FK_BPAWorkQueue_BPASnapshotConfiguration FOREIGN KEY (snapshotconfigurationid)
         REFERENCES BPASnapshotConfiguration (id)
GO

-- Create BPMIConfiguredSnapshot table
CREATE TABLE BPMIConfiguredSnapshot (
    snapshotid BIGINT NOT NULL IDENTITY,
    queueident INT NOT NULL,
    timeofdaysecs INT NOT NULL,
    dayofweek INT,
    interval INT NOT NULL DEFAULT 2,
    eventtype INT NOT NULL,
    CONSTRAINT PK_BPMIConfiguredSnapshot PRIMARY KEY (snapshotid),
    CONSTRAINT FK_BPMIConfiguredSnapshot_BPAWorkQueue FOREIGN KEY (queueident)
    REFERENCES BPAWorkQueue (ident)
    ON DELETE CASCADE
);

create nonclustered index Index_BPMIConfiguredSnapshot_queueident
    on BPMIConfiguredSnapshot (queueident) with (fillfactor = 90);

-- Create BPMIQueueSnapshot table
CREATE TABLE BPMIQueueSnapshot (
    id BIGINT NOT NULL IDENTITY,
    queueident INT NOT NULL,
    snapshotid BIGINT NOT NULL,
    snapshotdate DATETIMEOFFSET NOT NULL,
    capturedatetimeutc DATETIME NOT NULL DEFAULT GETUTCDATE(),
    totalitems INT NOT NULL,
    itemspending INT NOT NULL,
    itemscompleted INT NOT NULL,
    itemsreferred INT NOT NULL,
    newitemsdelta INT NOT NULL,
    completeditemsdelta INT NOT NULL,
    referreditemsdelta INT NOT NULL,
    totalworktimecompleted BIGINT NOT NULL,
    totalworktimereferred BIGINT NOT NULL,
    totalidletime BIGINT NOT NULL,
    totalnewsincemidnight INT NOT NULL,
    totalnewlast24hours INT NOT NULL,
    averagecompletedworktime INT NOT NULL,
    averagereferredworktime INT NOT NULL,
    averageidletime INT NOT NULL,
    CONSTRAINT PK_BPMIQueueSnapshot PRIMARY KEY (id),
    CONSTRAINT FK_BPMIQueueSnapshot_BPAWorkQueue FOREIGN KEY (queueident)
    REFERENCES BPAWorkQueue (ident)
    ON DELETE CASCADE
);

create nonclustered index Index_BPMIQueueSnapshot_snapshotid_queueident
    on BPMIQueueSnapshot (snapshotid, queueident) with (fillfactor = 90);

-- Create BPMIQueueTrend table
CREATE TABLE BPMIQueueTrend (
    id INT NOT NULL IDENTITY,
    snapshottimeofdaysecs INT,
    queueident INT NOT NULL,
    trendid INT NOT NULL,
    capturedatetimeutc DATETIME NOT NULL DEFAULT GETUTCDATE(),
    averagetotalitems INT NOT NULL,
    averageitemspending INT NOT NULL,
    averageitemscompleted INT NOT NULL,
    averageitemsreferred INT NOT NULL,
    averagenewitemsdelta INT NOT NULL,
    averagecompleteditemsdelta INT NOT NULL,
    averagereferreditemsdelta INT NOT NULL,
    averagetotalworktimecompleted BIGINT NOT NULL,
    averagetotalworktimereferred BIGINT NOT NULL,
    averagetotalidletime BIGINT NOT NULL,
    averagetotalnewsincemidnight INT NOT NULL,
    averagetotalnewlast24hours INT NOT NULL,
    averageaveragecompletedworktime INT NOT NULL,
    averageaveragereferredworktime INT NOT NULL,
    averageaverageidletime INT NOT NULL,
    CONSTRAINT PK_BPMIQueueTrend PRIMARY KEY (id),
    CONSTRAINT FK_BPMIQueueTrend_BPAWorkQueue FOREIGN KEY (queueident)
    REFERENCES BPAWorkQueue (ident)
    ON DELETE CASCADE
);

create nonclustered index Index_BPMIQueueTrend_queueident
    on BPMIQueueTrend (queueident) with (fillfactor = 90);

-- Create interim queue snapshot table
create table BPMIQueueInterimSnapshot(
    queueident int not null,
    snapshotdate datetimeoffset not null,
    totalitems int not null,
    itemspending int not null,
    itemscompleted int not null,
    itemsreferred int not null,
    newitemsdelta int not null,
    completeditemsdelta int not null,
    referreditemsdelta int not null,
    totalworktimecompleted bigint not null,
    totalworktimereferred bigint not null,
    totalidletime bigint not null,
constraint PK_BPMIQueueInterimSnapshot primary key clustered (queueident),
constraint FK_BPMIQueueInterimSnapshot_BPAWorkQueue foreign key (queueident)
    references BPAWorkQueue (ident) on delete cascade);

--set DB version
INSERT INTO BPADBVersion VALUES (
  '296',
  GETUTCDATE(),
  'db_upgradeR296.sql UTC',
  'Update and create tables for Work Queue Analysis',
  0)