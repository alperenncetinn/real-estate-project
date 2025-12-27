-- Supabase'de verification_codes tablosunu oluşturmak için bu SQL'i çalıştırın
-- Supabase Dashboard -> SQL Editor -> New Query

CREATE TABLE IF NOT EXISTS verification_codes (
    id SERIAL PRIMARY KEY,
    email VARCHAR(256) NOT NULL,
    code VARCHAR(10) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    is_used BOOLEAN DEFAULT FALSE,
    type VARCHAR(50) DEFAULT 'email_verification'
);

-- Index for faster lookups
CREATE INDEX IF NOT EXISTS idx_verification_codes_email ON verification_codes(email);
CREATE INDEX IF NOT EXISTS idx_verification_codes_code ON verification_codes(code);

-- Optional: Auto-delete expired codes after 1 day
-- CREATE OR REPLACE FUNCTION delete_expired_verification_codes()
-- RETURNS trigger AS $$
-- BEGIN
--     DELETE FROM verification_codes WHERE expires_at < NOW() - INTERVAL '1 day';
--     RETURN NEW;
-- END;
-- $$ LANGUAGE plpgsql;
