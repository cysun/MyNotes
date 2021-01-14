﻿CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "Files" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "Name" character varying(1000) NOT NULL,
    "Version" integer NOT NULL,
    "ContentType" character varying(255) NULL,
    "Size" bigint NOT NULL,
    "Created" timestamp without time zone NOT NULL,
    "Updated" timestamp without time zone NOT NULL,
    "IsFolder" boolean NOT NULL,
    "ParentId" integer NULL,
    "AccessCount" integer NOT NULL,
    "IsFavorite" boolean NOT NULL,
    "IsPublic" boolean NOT NULL,
    CONSTRAINT "PK_Files" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Files_Files_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Files" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Notes" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "Subject" character varying(80) NOT NULL,
    "Content" text NULL,
    "Created" timestamp without time zone NOT NULL,
    "Updated" timestamp without time zone NOT NULL,
    "Published" timestamp without time zone NULL,
    "ViewCount" integer NOT NULL,
    "Summary" text NULL,
    "IsBlog" boolean NOT NULL DEFAULT FALSE,
    "Deleted" boolean NOT NULL,
    CONSTRAINT "PK_Notes" PRIMARY KEY ("Id")
);

CREATE TABLE "Tags" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "Label" character varying(30) NOT NULL,
    "NoteCount" integer NOT NULL,
    "LastUsed" timestamp without time zone NOT NULL,
    "Retired" boolean NOT NULL,
    CONSTRAINT "PK_Tags" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_Tags_Label" UNIQUE ("Label")
);

CREATE TABLE "FileHistories" (
    "FileId" integer NOT NULL,
    "Version" integer NOT NULL,
    "Name" character varying(1000) NOT NULL,
    "ContentType" character varying(255) NULL,
    "Size" bigint NOT NULL,
    "Created" timestamp without time zone NOT NULL,
    "Updated" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_FileHistories" PRIMARY KEY ("FileId", "Version"),
    CONSTRAINT "FK_FileHistories_Files_FileId" FOREIGN KEY ("FileId") REFERENCES "Files" ("Id") ON DELETE CASCADE
);

CREATE TABLE "NoteTags" (
    "NoteId" integer NOT NULL,
    "Label" character varying(30) NOT NULL,
    CONSTRAINT "PK_NoteTags" PRIMARY KEY ("NoteId", "Label"),
    CONSTRAINT "FK_NoteTags_Notes_NoteId" FOREIGN KEY ("NoteId") REFERENCES "Notes" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Files_ParentId" ON "Files" ("ParentId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210113051616_InitialSchema', '5.0.1');

COMMIT;

