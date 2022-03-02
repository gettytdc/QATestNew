alter table BPMIProductivityShadow add statewhendeleted int null;

create nonclustered index Index_BPMIProductivityShadow_queueident_eventdatetime_eventid
    on BPMIProductivityShadow (queueident, eventdatetime, eventid)
    include (worktime, elapsedtime, statewhendeleted) with (fillfactor = 90);

--set DB version
INSERT INTO BPADBVersion VALUES (
  '298',
  GETUTCDATE(),
  'db_upgradeR298.sql',
  'Add item state at point of deletion to queue item MI shadow table.',
  0)
