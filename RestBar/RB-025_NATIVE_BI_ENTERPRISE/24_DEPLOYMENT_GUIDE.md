# 24 — Deployment Guide

1. Backup DB
2. `dotnet ef database update` (or apply SQL 01+02)
3. Deploy app with ClosedXML/QuestPDF packages
4. Confirm flags EnableReportExports=true
5. Login admin → Centro Ejecutivo
6. Smoke: live cards, period, decision, export CSV/XLSX
7. Rollback: DROP SCHEMA analytics CASCADE; revert app
