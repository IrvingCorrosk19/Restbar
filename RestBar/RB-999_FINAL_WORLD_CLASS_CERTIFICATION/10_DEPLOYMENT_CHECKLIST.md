# 10 — Deployment Checklist

## Infraestructura
- [ ] Docker Compose `restbar_*` aislado
- [ ] PostgreSQL healthy; user **`restbaruser`** (nunca `postgres` en VPS)
- [ ] `ASPNETCORE_ENVIRONMENT=Production`
- [ ] HTTPS / reverse proxy (nginx) cuando dominio público
- [ ] DataProtection keys volume persistente
- [ ] Feature flags revisados (Copilot **false**)

## Datos
- [ ] Backup `pg_dump` programado (`Com/backup-restbar-db.ps1`)
- [ ] Restore drill documentado
- [ ] Migraciones EF + SQL analytics/DI/Rules aplicados
- [ ] Seed solo en lab

## Observabilidad
- [ ] `/health/live` + `/health/ready` monitoreados
- [ ] Correlation ID en logs
- [ ] Alertas disco/CPU contenedor

## Comercial / soporte
- [ ] Contrato alcance (online, pagos externos)
- [ ] Capacitación roles (cajero/gerente/admin)
- [ ] Canal incidencias + RTO/RPO acordados
- [ ] Rollback: imagen Docker previa + restore DB

## Release gate
- [ ] CI **Quality Gate** verde en commit
- [ ] Smoke browser Inventory + Cash + Auth
- [ ] No secrets en git
