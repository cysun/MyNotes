ALTER TABLE "NoteTags" ADD CONSTRAINT "FK_NoteTags_Tags_Label"
    FOREIGN KEY ("Label") REFERENCES "Tags" ("Label") ON DELETE CASCADE;

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

-- Auto-Update NoteCount in Tags --

CREATE OR REPLACE FUNCTION "NoteTagsTriggerFunction"() RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'INSERT' OR TG_OP = 'UPDATE') THEN
        UPDATE "Tags" SET "NoteCount" = "NoteCount"+1, "LastUsed" = CURRENT_TIMESTAMP WHERE "Label" = NEW."Label";
    ELSIF (TG_OP = 'DELETE' OR TG_OP = 'UPDATE') THEN
        UPDATE "Tags" SET "NoteCount" = "NoteCount"-1, "LastUsed" = CURRENT_TIMESTAMP WHERE "Label" = OLD."Label";
    END IF;
    RETURN NULL;
END
$$ LANGUAGE plpgsql;

CREATE TRIGGER "NoteTagsTrigger"
    AFTER INSERT OR DELETE OR UPDATE ON "NoteTags"
    FOR EACH ROW EXECUTE PROCEDURE "NoteTagsTriggerFunction"();
