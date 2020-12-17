ALTER TABLE "Tags" ADD COLUMN "Retired" boolean NOT NULL DEFAULT FALSE;

CREATE UNIQUE INDEX "TagsLabelIndex" ON "Tags" (lower("Label"));
