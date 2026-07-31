# 10 — Scaling

## Current model

Single `restbar_web` + single `restbar_postgres`. Vertical scale first (CPU/RAM). Horizontal web requires:

- Shared DataProtection keys (volume/Redis) — filesystem volume ready.  
- Sticky sessions or shared session store (currently memory session).  
- Postgres connection pooling / PgBouncer for many tenants.

## Multi-restaurant

Logical multitenancy via Company/Branch — **supported**. Physical isolation per customer (DB-per-tenant) — **not implemented**.

## Load evidence

Pilot dataset PASS; 5k concurrent users **not evidenced** → cannot claim hyperscale.

**Overall scaling:** **PASS WITH CONDITIONS** (hundreds of restaurants logical; not proven thousands concurrent).
