using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace RestBar.Infrastructure.Health;

public sealed class PostgresReadyHealthCheck : IHealthCheck
{
    private readonly IConfiguration _config;
    private readonly ILogger<PostgresReadyHealthCheck> _logger;

    public PostgresReadyHealthCheck(IConfiguration config, ILogger<PostgresReadyHealthCheck> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var cs = _config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(cs))
            return HealthCheckResult.Unhealthy("Connection string missing");

        try
        {
            await using var conn = new NpgsqlConnection(cs);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1";
            cmd.CommandTimeout = 5;
            _ = await cmd.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL reachable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Postgres readiness failed");
            return HealthCheckResult.Unhealthy("PostgreSQL unreachable", ex);
        }
    }
}
