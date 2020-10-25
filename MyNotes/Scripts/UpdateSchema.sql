ALTER TABLE "Notes" ADD COLUMN "Published" timestamp without time zone NULL;
UPDATE "Notes" SET "Published" = CURRENT_TIMESTAMP WHERE "IsPublic" = True;
ALTER TABLE "Notes" DROP COLUMN "IsPublic";
