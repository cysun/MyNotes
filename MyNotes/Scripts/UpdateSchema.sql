ALTER TABLE "Files" RENAME COLUMN "IsFavorite" TO "IsPinned";

ALTER TABLE "Notes" ADD COLUMN "ParentId" integer;
ALTER TABLE "Notes" ADD CONSTRAINT "FK_Notes_Files_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Files" ("Id");

CREATE INDEX "IX_Notes_ParentId" ON "Notes" ("ParentId");

DELETE FROM "__EFMigrationsHistory";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250212193750_InitialSchema', '9.0.2');
