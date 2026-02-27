CREATE TABLE IF NOT EXISTS event_store (
    id BIGSERIAL PRIMARY KEY,
    stream_id UUID NOT NULL,
    event_type VARCHAR(200) NOT NULL,
    event_data JSONB NOT NULL,
    metadata JSONB NOT NULL DEFAULT '{}',
    stream_version INT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    actor_user_id VARCHAR(200),
    actor_email VARCHAR(200),
    actor_name VARCHAR(200),
    correlation_id VARCHAR(200)
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_event_store_stream_version ON event_store(stream_id, stream_version);
CREATE INDEX IF NOT EXISTS idx_event_store_stream_id ON event_store(stream_id);
CREATE INDEX IF NOT EXISTS idx_event_store_created_at ON event_store(created_at);