ALTER TABLE "Tag" ADD CONSTRAINT "FK_Tag_TagRecords_Label"
    FOREIGN KEY ("Label") REFERENCES "TagRecords" ("Label") ON DELETE CASCADE;

ALTER TABLE "Notes" ADD COLUMN tsv tsvector;

CREATE INDEX "NotesTsIndex" ON "Notes" USING GIN(tsv);

CREATE FUNCTION "NotesTsTriggerFunction"() RETURNS TRIGGER AS $$
BEGIN
    new.tsv := setweight(to_tsvector(new."Subject"), 'A') ||
               setweight(to_tsvector(new."Content"), 'D');
    RETURN new;
END
$$ LANGUAGE plpgsql;

CREATE TRIGGER "NotesTsTrigger"
    BEFORE INSERT OR UPDATE ON "Notes"
    FOR EACH ROW EXECUTE PROCEDURE "NotesTsTriggerFunction"();

CREATE OR REPLACE FUNCTION "SearchNotes"(q varchar) RETURNS SETOF "Notes" AS $$
BEGIN
    RETURN QUERY SELECT * FROM "Notes" WHERE plainto_tsquery(q) @@ tsv LIMIT 20;
    RETURN;
 END
$$ LANGUAGE plpgsql;
