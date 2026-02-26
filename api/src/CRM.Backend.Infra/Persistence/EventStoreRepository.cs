using CRM.Backend.Domain.Events;
using CRM.Backend.Domain.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;

namespace CRM.Backend.Infra.Persistence;

public class EventStoreRepository : IEventStoreRepository
{
    private readonly DbConnectionFactory _factory;
    private readonly IEnumerable<IProjection> _projections;
    private readonly ILogger<EventStoreRepository> _logger;

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.None
    };

    public EventStoreRepository(DbConnectionFactory factory, IEnumerable<IProjection> projections, ILogger<EventStoreRepository> logger)
    {
        _factory = factory;
        _projections = projections;
        _logger = logger;
    }

    public async Task AppendEventsAsync(Guid streamId, IEnumerable<DomainEvent> events, EventMetadata metadata, CancellationToken ct = default)
    {
        using var conn = (NpgsqlConnection)_factory.CreateConnection();
        await conn.OpenAsync(ct);
        using var tx = await conn.BeginTransactionAsync(ct);

        try
        {
            var currentVersion = await GetCurrentVersionAsync(conn, streamId, tx, ct);

            foreach (var evt in events)
            {
                currentVersion++;
                var eventType = evt.GetType().Name;
                var eventData = JsonConvert.SerializeObject(evt, JsonSettings);
                var metadataJson = JsonConvert.SerializeObject(metadata, JsonSettings);

                var cmd = new NpgsqlCommand(@"
                    INSERT INTO event_store (stream_id, event_type, event_data, metadata, stream_version, actor_user_id, actor_email, actor_name, correlation_id)
                    VALUES (@streamId, @eventType, @eventData::jsonb, @metadata::jsonb, @version, @actorUserId, @actorEmail, @actorName, @correlationId)", conn, tx);

                cmd.Parameters.AddWithValue("streamId", streamId);
                cmd.Parameters.AddWithValue("eventType", eventType);
                cmd.Parameters.AddWithValue("eventData", eventData);
                cmd.Parameters.AddWithValue("metadata", metadataJson);
                cmd.Parameters.AddWithValue("version", currentVersion);
                cmd.Parameters.AddWithValue("actorUserId", (object?)metadata.ActorUserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("actorEmail", (object?)metadata.ActorEmail ?? DBNull.Value);
                cmd.Parameters.AddWithValue("actorName", (object?)metadata.ActorName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("correlationId", (object?)metadata.CorrelationId ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            foreach (var projection in _projections)
                foreach (var evt in events)
                {
                    try
                    {
                        await projection.ProjectAsync(evt, ct);
                    }
                    catch (Exception projEx)
                    {
                        _logger.LogError(projEx, "Projection {Projection} failed for event {EventType} on stream {StreamId}. Read model may be stale.",
                            projection.GetType().Name, evt.GetType().Name, streamId);
                    }
                }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append events for stream {StreamId}", streamId);
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IEnumerable<DomainEvent>> LoadEventsAsync(Guid streamId, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var rows = await conn.QueryAsync<StoredEventRow>(
            "SELECT event_type, event_data::text FROM event_store WHERE stream_id = @streamId ORDER BY stream_version",
            new { streamId });

        return rows.Select(r => DeserializeEvent(r.event_type, r.event_data)).Where(e => e != null).Select(e => e!).ToList();
    }

    public async Task<IEnumerable<StoredEvent>> GetStoredEventsAsync(Guid streamId, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var rows = await conn.QueryAsync<StoredEventRow>(
            @"SELECT id, stream_id, event_type, event_data::text as event_data, metadata::text as metadata, stream_version, created_at,
                     actor_user_id, actor_email, actor_name, correlation_id
              FROM event_store WHERE stream_id = @streamId ORDER BY stream_version",
            new { streamId });

        return rows.Select(r => new StoredEvent(r.id, r.stream_id, r.event_type, r.event_data, r.metadata ?? "{}", r.stream_version, r.created_at, r.actor_user_id, r.actor_email, r.actor_name, r.correlation_id)).ToList();
    }

    private static async Task<int> GetCurrentVersionAsync(NpgsqlConnection conn, Guid streamId, NpgsqlTransaction tx, CancellationToken ct)
    {
        var cmd = new NpgsqlCommand("SELECT COALESCE(MAX(stream_version), 0) FROM event_store WHERE stream_id = @streamId", conn, tx);
        cmd.Parameters.AddWithValue("streamId", streamId);
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    private static DomainEvent? DeserializeEvent(string eventType, string eventData)
    {
        return eventType switch
        {
            nameof(CustomerCreatedEvent) => JsonConvert.DeserializeObject<CustomerCreatedEvent>(eventData, JsonSettings),
            nameof(CustomerUpdatedEvent) => JsonConvert.DeserializeObject<CustomerUpdatedEvent>(eventData, JsonSettings),
            nameof(CustomerDeactivatedEvent) => JsonConvert.DeserializeObject<CustomerDeactivatedEvent>(eventData, JsonSettings),
            _ => null
        };
    }

    private record StoredEventRow(long id, Guid stream_id, string event_type, string event_data, string? metadata, int stream_version, DateTime created_at, string? actor_user_id, string? actor_email, string? actor_name, string? correlation_id);
}