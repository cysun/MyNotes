ALTER TABLE "Notes" ADD COLUMN "IsPinned" boolean NOT NULL default false;
ALTER TABLE "Notes" ALTER COLUMN "IsPinned" DROP DEFAULT;

ALTER TABLE "Notes" RENAME COLUMN "Deleted" TO "IsDeleted";

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20221008233114_InitialSchema', '6.0.9');
