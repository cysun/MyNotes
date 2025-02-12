DROP FUNCTION "SearchNotes";
DROP FUNCTION "SearchFiles";
DROP TRIGGER "NotesTsTrigger" ON "Notes";
DROP FUNCTION "NotesTsTriggerFunction"();
DROP TRIGGER "FilesTsTrigger" ON "Files";
DROP FUNCTION "FilesTsTriggerFunction"();

DROP TABLE "__EFMigrationsHistory";
DROP TABLE "FileHistories";
DROP TABLE "Notes";
DROP TABLE "Files";
