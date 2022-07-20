-- FTS on Notes --

ALTER TABLE "Notes" ADD COLUMN tsv tsvector;

CREATE INDEX "NotesTsIndex" ON "Notes" USING GIN(tsv);

CREATE OR REPLACE FUNCTION "NotesTsTriggerFunction"() RETURNS TRIGGER AS $$
BEGIN
    NEW.tsv := setweight(to_tsvector(coalesce(NEW."Subject",'')), 'A') ||
               setweight(to_tsvector(coalesce(NEW."Content",'')), 'D');
    RETURN NEW;
END
$$ LANGUAGE plpgsql;

CREATE TRIGGER "NotesTsTrigger"
    BEFORE INSERT OR UPDATE ON "Notes"
    FOR EACH ROW EXECUTE PROCEDURE "NotesTsTriggerFunction"();

-- Search Notes by FTS --

CREATE OR REPLACE FUNCTION "SearchNotes"(q varchar) RETURNS SETOF "Notes" AS $$
BEGIN
    RETURN QUERY SELECT * FROM "Notes" WHERE plainto_tsquery(q) @@ tsv LIMIT 20;
    RETURN;
 END
$$ LANGUAGE plpgsql;

-- Start File Id from 1000000 --

ALTER SEQUENCE "Files_Id_seq" RESTART WITH 1000000;

-- FTS on File Names --

ALTER TABLE "Files" ADD COLUMN tsv tsvector;

CREATE INDEX "FilesTsIndex" ON "Files" USING GIN(tsv);

CREATE OR REPLACE FUNCTION "FilesTsTriggerFunction"() RETURNS TRIGGER AS $$
BEGIN
    NEW.tsv := to_tsvector(NEW."Name");
    RETURN NEW;
END
$$ LANGUAGE plpgsql;

CREATE TRIGGER "FilesTsTrigger"
    BEFORE INSERT OR UPDATE ON "Files"
    FOR EACH ROW EXECUTE PROCEDURE "FilesTsTriggerFunction"();

-- Search Files by FTS --

CREATE OR REPLACE FUNCTION "SearchFiles"(q varchar) RETURNS SETOF "Files" AS $$
BEGIN
    RETURN QUERY SELECT * FROM "Files" WHERE plainto_tsquery(q) @@ tsv LIMIT 20;
    RETURN;
 END
$$ LANGUAGE plpgsql;
