-- Initialize the database if it doesn't exist
SELECT 'CREATE DATABASE edushield_backend'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'edushield_backend')\gexec

-- Connect to the database
\c edushield_backend;

-- Create extensions if they don't exist
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements";

-- Grant necessary permissions
GRANT ALL PRIVILEGES ON DATABASE edushield_backend TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA public TO postgres;


