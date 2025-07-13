# Script PowerShell para actualizar valores de TableStatus
$connectionString = "Host=localhost;Database=restbar;Username=postgres;Password=123456"

try {
    # Crear conexión
    $connection = New-Object Npgsql.NpgsqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "🔍 Conectado a la base de datos PostgreSQL"
    
    # Script SQL para actualizar valores
    $sql = @"
-- Actualizar valores de estado de tablas de inglés a español
UPDATE tables 
SET status = CASE 
    WHEN status = 'AVAILABLE' THEN 'Disponible'
    WHEN status = 'OCCUPIED' THEN 'Ocupada'
    WHEN status = 'RESERVED' THEN 'Reservada'
    WHEN status = 'WAITING' THEN 'EnEspera'
    WHEN status = 'ATTENDED' THEN 'Atendida'
    WHEN status = 'PREPARING' THEN 'EnPreparacion'
    WHEN status = 'SERVED' THEN 'Servida'
    WHEN status = 'READY_FOR_PAYMENT' THEN 'ParaPago'
    WHEN status = 'PAID' THEN 'Pagada'
    WHEN status = 'BLOCKED' THEN 'Bloqueada'
    ELSE status -- Mantener valores que ya estén en español
END;
"@
    
    # Ejecutar el UPDATE
    $command = New-Object Npgsql.NpgsqlCommand($sql, $connection)
    $rowsAffected = $command.ExecuteNonQuery()
    
    Write-Host "✅ Actualizadas $rowsAffected filas en la tabla tables"
    
    # Verificar los valores actualizados
    $verifySql = "SELECT DISTINCT status, COUNT(*) as count FROM tables GROUP BY status ORDER BY status;"
    $verifyCommand = New-Object Npgsql.NpgsqlCommand($verifySql, $connection)
    $reader = $verifyCommand.ExecuteReader()
    
    Write-Host "📊 Valores de status actuales en la base de datos:"
    while ($reader.Read()) {
        $status = $reader["status"]
        $count = $reader["count"]
        Write-Host "  - $status : $count registros"
    }
    $reader.Close()
    
    Write-Host "✅ Script ejecutado exitosamente"
    
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)"
} finally {
    if ($connection -and $connection.State -eq 'Open') {
        $connection.Close()
        Write-Host "🔍 Conexión cerrada"
    }
}
