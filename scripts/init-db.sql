-- Digital Stokvel Banking Database Initialization Script
-- PostgreSQL 16.x

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create schemas
CREATE SCHEMA IF NOT EXISTS stokvel;
CREATE SCHEMA IF NOT EXISTS audit;

-- Set search path
SET search_path TO stokvel, public;

-- Grant permissions
GRANT ALL PRIVILEGES ON SCHEMA stokvel TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA audit TO postgres;

-- Log initialization
DO $$
BEGIN
    RAISE NOTICE 'Digital Stokvel Banking database initialized successfully';
    RAISE NOTICE 'Schemas created: stokvel, audit';
    RAISE NOTICE 'Extensions enabled: uuid-ossp, pgcrypto';
END $$;
