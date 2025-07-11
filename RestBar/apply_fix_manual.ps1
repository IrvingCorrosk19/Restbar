# Script de PowerShell para aplicar el fix manualmente
Write-Host "🔧 Aplicando fix manual para columna paid_at..." -ForegroundColor Yellow

# Ejecutar el script SQL usando dotnet ef
Write-Host "📝 Ejecutando script SQL..." -ForegroundColor Green
dotnet ef database script --sql "ALTER TABLE payments ALTER COLUMN paid_at TYPE timestamp with time zone USING paid_at AT TIME ZONE 'UTC';"

# Marcar la migración como aplicada
Write-Host "📋 Marcando migración como aplicada..." -ForegroundColor Green
dotnet ef database script --sql "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('20250711010756_ForceFixPaymentDateTimeColumn', '9.0.5') ON CONFLICT (\"MigrationId\") DO NOTHING;"

Write-Host "✅ Fix aplicado. Ahora ejecuta 'dotnet run' para probar." -ForegroundColor Green 