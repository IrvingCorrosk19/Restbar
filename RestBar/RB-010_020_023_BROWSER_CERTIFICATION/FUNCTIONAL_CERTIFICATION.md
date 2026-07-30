# FUNCTIONAL_CERTIFICATION.md

## Alcance certificado (browser real)

### RB-010 Cash
- Dashboard Command Center visible
- Open wizard (select + float + botón)
- Open session (si hay register)
- Cash registers index + create form render
- Paid-in / verify-chain no HTTP 500
- Feature flag ON en Development

### RB-020 Procurement
- Supplier index + tabla
- Procurement dashboard
- PO list + Nueva PO + Create wizard
- Create vacío no tumba página
- GetSuppliers JSON

### RB-023 Food Cost
- Food Cost dashboard métricas
- Menu Engineering
- Recipes index
- PlateCost API sin 500
- Recipe cost (si hay datos)

### Integración / Regresión
- Orders, Kitchen station, Products, Command Center, Logout

## Fuera de alcance de esta corrida (UI incompleta o sin datos)

Flujos profundos sin UI completa en v1: dual approval interactivo, hash chain visual, recepción parcial E2E con inventario, simulation UI, waste UI forms, todos los métodos de pago en caja vía browser. Quedan como backlog de certificación profunda (API + datos seed).

## Veredicto funcional browser

**PASS** sobre suite ejecutada (120/120 ejecutados con resultado PASS; 6 skip justificados).
