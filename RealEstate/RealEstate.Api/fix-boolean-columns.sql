-- Fix auto-increment sequences and datetime columns for PostgreSQL

-- Fix DateTime columns from TEXT to timestamp with time zone
ALTER TABLE "Users" 
ALTER COLUMN "CreatedDate" TYPE timestamp with time zone 
USING "CreatedDate"::timestamp with time zone;

ALTER TABLE "Listings" 
ALTER COLUMN "CreatedDate" TYPE timestamp with time zone 
USING "CreatedDate"::timestamp with time zone;

ALTER TABLE "Listings" 
ALTER COLUMN "DeactivatedAt" TYPE timestamp with time zone 
USING "DeactivatedAt"::timestamp with time zone;

ALTER TABLE "Notifications" 
ALTER COLUMN "CreatedAt" TYPE timestamp with time zone 
USING "CreatedAt"::timestamp with time zone;

ALTER TABLE "Favorites" 
ALTER COLUMN "CreatedAt" TYPE timestamp with time zone 
USING "CreatedAt"::timestamp with time zone;

-- Fix auto-increment sequences for PostgreSQL
-- Create sequences if they don't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'users_id_seq') THEN
        CREATE SEQUENCE "Users_Id_seq";
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'listings_id_seq') THEN
        CREATE SEQUENCE "Listings_Id_seq";
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'notifications_id_seq') THEN
        CREATE SEQUENCE "Notifications_Id_seq";
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'properties_id_seq') THEN
        CREATE SEQUENCE "Properties_Id_seq";
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'favorites_id_seq') THEN
        CREATE SEQUENCE "Favorites_Id_seq";
    END IF;
END $$;

-- Set default values using sequences
ALTER TABLE "Users" ALTER COLUMN "Id" SET DEFAULT nextval('"Users_Id_seq"');
ALTER TABLE "Listings" ALTER COLUMN "Id" SET DEFAULT nextval('"Listings_Id_seq"');
ALTER TABLE "Notifications" ALTER COLUMN "Id" SET DEFAULT nextval('"Notifications_Id_seq"');
ALTER TABLE "Properties" ALTER COLUMN "Id" SET DEFAULT nextval('"Properties_Id_seq"');
ALTER TABLE "Favorites" ALTER COLUMN "Id" SET DEFAULT nextval('"Favorites_Id_seq"');

-- Reset sequences to current max ID + 1
SELECT setval('"Users_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Users"), 0) + 1, false);
SELECT setval('"Listings_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Listings"), 0) + 1, false);
SELECT setval('"Notifications_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Notifications"), 0) + 1, false);
SELECT setval('"Properties_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Properties"), 0) + 1, false);
SELECT setval('"Favorites_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Favorites"), 0) + 1, false);
