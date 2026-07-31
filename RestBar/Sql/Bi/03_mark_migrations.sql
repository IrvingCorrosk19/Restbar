SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY 1 DESC LIMIT 10;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT v.id, '9.0.5'
FROM (VALUES
  ('20260730190000_NativeBiAnalyticsLayer'),
  ('20260730200000_AnalyticsEnterpriseSchema'),
  ('20260731011008_SyncPendingModelChanges')
) AS v(id)
WHERE NOT EXISTS (
  SELECT 1 FROM "__EFMigrationsHistory" h WHERE h."MigrationId" = v.id
);
SELECT "MigrationId" FROM "__EFMigrationsHistory" WHERE "MigrationId" LIKE '%Analytics%' OR "MigrationId" LIKE '%NativeBi%' OR "MigrationId" LIKE '%SyncPending%' ORDER BY 1;