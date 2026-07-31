# Delivery Readiness — 2026-07-31

## Ambiente de entrega
- **URL:** http://164.68.99.83:8084  
- **Health:** `/health/live` = Healthy · `/health/ready` = 200  
- **Git:** `main` @ `4e86dab` (deploy previo `b0a2dcb` + fix test MT-D05)  
- **Login smoke (Browser Tab):** admin → `/Home` OK · `/Customer` OK  

## Pruebas ejecutadas

| Suite | Resultado |
|-------|-----------|
| Unit Release | **98 PASS** |
| Playwright VPS chromium-desktop (Smoke, Auth, MT, Security, Cash, Inventory, Orders, Payments, Shifts, Tables, Stations, Floors, Reports, Admin) | **104/105 PASS** luego **MT-D05 corregido → PASS** |
| Browser Tab login + Home + Customer | **PASS** |

## Alcance comercial 1.0 (honesto)
- Piloto / cadena **online**, partner de implantación.  
- Exclusiones: MFA, offline POS, gateway PCI, lab hiperescala.  
- Certificaciones: RB-1000 **APPROVED WITH MINOR OBSERVATIONS** · RB-1001 **PASS WITH CONDITIONS**.

## Cómo operar
1. Abrir http://164.68.99.83:8084  
2. Login con admin de la instancia  
3. Flags Production: Cash/Purchasing/FoodCost/DI/Rules ON · Copilot OFF  

Listo para entregar como **release candidato 1.0** bajo condiciones documentadas.
