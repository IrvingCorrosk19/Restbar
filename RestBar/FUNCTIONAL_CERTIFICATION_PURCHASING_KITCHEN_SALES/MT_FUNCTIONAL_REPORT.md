# Multitenant Functional Cases - Report

**Fecha:** 2026-07-28 20:01:34
**Target:** http://164.68.99.83:8084
**Veredicto suite:** **FAIL**

## Totales

| Metrica | Valor |
|---------|-------|
| PASS | **47** |
| FAIL | **1** |
| BLOCKED | **3** |
| TOTAL | **51** |

## Por categoria

- **Auth**: 8 PASS / 1 FAIL / 0 BLOCKED
- **Isolation**: 5 PASS / 0 FAIL / 1 BLOCKED
- **Catalog**: 8 PASS / 0 FAIL / 0 BLOCKED
- **ParallelOps**: 4 PASS / 0 FAIL / 0 BLOCKED
- **Security**: 10 PASS / 0 FAIL / 0 BLOCKED
- **TableChange**: 2 PASS / 0 FAIL / 0 BLOCKED
- **Roles**: 5 PASS / 0 FAIL / 0 BLOCKED
- **SalesFlow**: 5 PASS / 0 FAIL / 0 BLOCKED
- **BranchIsolation**: 0 PASS / 0 FAIL / 2 BLOCKED

## Escenarios cubiertos

1. Dos empresas SaaS (A/B) en paralelo sin ver mesas/ordenes ajenas
2. Tres restaurantes independientes (Costa/Norte/Sur) con catalogo exclusivo
3. Misma empresa, dos sucursales (Centro vs Norte) - aislamiento BranchId
4. Ataques IDOR: pagar / cancelar / leer orden de otro tenant
5. Intento de mover orden a mesa de otra empresa
6. Roles: mesero/chef sin paneles admin; SuperAdmin con acceso global
7. Flujo de venta (orden a pago) en cada tenant
8. GUIDs inventados no filtran datos

## FALLOs

| MT-00-03 | Sucursal Norte (A) admin login | admin.norte@restbar.com |

## Artefacto CSV

MT_FUNCTIONAL_RESULTS_20260728_195636.csv
