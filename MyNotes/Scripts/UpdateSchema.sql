ALTER TABLE "Files" ALTER COLUMN "Created" TYPE timestamp with time zone;
ALTER TABLE "Files" ALTER COLUMN "Updated" TYPE timestamp with time zone;
ALTER TABLE "Notes" ALTER COLUMN "Created" TYPE timestamp with time zone;
ALTER TABLE "Notes" ALTER COLUMN "Updated" TYPE timestamp with time zone;
ALTER TABLE "Notes" ALTER COLUMN "Published" TYPE timestamp with time zone;
ALTER TABLE "Tags" ALTER COLUMN "LastUsed" TYPE timestamp with time zone;
ALTER TABLE "FileHistories" ALTER COLUMN "Created" TYPE timestamp with time zone;
ALTER TABLE "FileHistories" ALTER COLUMN "Updated" TYPE timestamp with time zone;

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220518020950_InitialSchema', '6.0.5');
