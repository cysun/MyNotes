DROP TABLE "NoteTags";
DROP TABLE "Tags";

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220720191150_InitialSchema', '6.0.7');
