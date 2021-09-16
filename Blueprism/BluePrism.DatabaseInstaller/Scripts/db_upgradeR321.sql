ALTER VIEW [BPVGroupedResources] AS
SELECT
    g.treeid AS treeid,
    (CASE WHEN r.[pool] IS NOT NULL THEN r.[pool] ELSE g.id END) AS groupid,
    g.name AS groupname,
    r.resourceid AS id,
    r.name AS name,
    r.attributeid AS attributes,
    CASE WHEN r.[pool] IS NOT NULL THEN 1 ELSE 0 END AS ispoolmember,
    1 AS statusid,
    r.diagnostics, 
    r.logtoeventlog
FROM [BPAResource] r
      LEFT JOIN (
        [BPAGroupResource] gr
            INNER JOIN BPAGroup g ON gr.groupid = g.id
      ) ON gr.memberid = r.resourceid
WHERE attributeId & 8 = 0;
GO

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('321',
       GETUTCDATE(),
       'db_upgradeR321sql',
       'Add extra columns to BPVGroupedResources.',
       0);