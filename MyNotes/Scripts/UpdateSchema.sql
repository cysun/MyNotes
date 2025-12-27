CREATE OR REPLACE FUNCTION "SearchNotes"(q varchar) RETURNS SETOF "Notes" AS $$
BEGIN
    RETURN QUERY SELECT n.* FROM "Notes" n, plainto_tsquery(q) query WHERE query @@ tsv
      ORDER BY ts_rank_cd(tsv, query) DESC LIMIT 20;
    RETURN;
 END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION "SearchFiles"(q varchar) RETURNS SETOF "Files" AS $$
BEGIN
    RETURN QUERY SELECT f.* FROM "Files" f, plainto_tsquery(q) query WHERE query @@ tsv
      ORDER BY ts_rank_cd(tsv, query) DESC LIMIT 20;
    RETURN;
 END
$$ LANGUAGE plpgsql;
