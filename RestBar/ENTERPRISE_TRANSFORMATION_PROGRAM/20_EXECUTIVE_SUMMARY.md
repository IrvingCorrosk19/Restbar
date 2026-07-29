# 20 — EXECUTIVE SUMMARY

**Programa:** RestBar Enterprise Transformation  
**De:** POS+KDS certificado (madurez **41/100**)  
**A:** Plataforma de gestión empresarial para restaurantes LATAM (**90+/100**)  
**Horizonte:** 24 meses · **Sin implementación en este entregable**

---

# La pregunta que gobierna todo

> ¿Esta funcionalidad hace que un restaurante **gane más**, **ahorre más**, **administre mejor** o **decida mejor**?

Si no → no entra al producto.

---

# Diagnóstico en una frase

RestBar **ya opera** el piso (órdenes + cocina + pagos + multitenant).  
**Aún no administra** el dinero ni el margen (caja, compras, food cost, fiscal, ingresos activos, inteligencia).

Por eso un restaurante pagaría un **piloto**, no una **suite enterprise**.

---

# Qué CONSTRUIR (en orden)

| Orden | Qué | Por qué |
|-------|-----|---------|
| **1º** | Caja + precuenta + export + cierre | Control diario + rotación + confianza |
| **2º** | Proveedores + PO + recetas costeo + merma | Ahorro food cost (el dinero escondido) |
| **3º** | Combos + Happy Hour + fiscal piloto | Ganar más + legal |
| **4º** | Command Center + BI + forecast + SaaS billing | Decidir + escalar comercialmente |
| **5º** | Copilot + labor + delivery + franchise pack | Ventaja competitiva sostenible |

Extender siempre: `Shift`, `Invoice`, `Recipe`, `InventoryMovement`, `DiscountPolicy`/`PriceSchedule`, `Customer`, `AdvancedReportsService`, `TenantSubscriptionMiddleware`.

---

# Qué ELIMINAR / ocultar

- Superficies stub (SupplierAnalysis vacío, exports `byte[0]`, settings sin vista)  
- Entradas de menú rotas (Supplier 404)  
- Duplicidad Payment/User admin  
- Promesas de IA/forecast vacías en UI  

---

# Qué REDISEÑAR

- Home → **Command Center**  
- Reports + AdvancedReports → una capa de **decisiones**  
- Inventario → ledger único (venta, PO, merma, transfer)  
- Shift → eje de **caja + handoff**  
- InvoiceService → precuenta + fiscal  

---

# Qué POSPONER

Hotel · Casino · Payroll nativo · Marketplace · SPA rewrite · ML profundo · Multi-fiscal simultáneo · Offline (hasta F5)

---

# Qué acelera el ROI del cliente

1. Precuenta (rotación)  
2. PO + food cost (margen)  
3. Combos/HH (ticket)  
4. Caja (fraude/pérdidas)  
5. Command Center (tiempo gerente)

---

# Qué incrementa valor percibido

- Dejar de mostrar botones que no funcionan  
- Demo “cierre de día en 5 minutos”  
- Un número: **Food Cost %** en pantalla dueño  
- Alertas accionables, no gráficos vanidosos  

---

# Qué crea ventaja competitiva sostenible

1. KDS routing multi-estación (ya)  
2. Multiempresa nativo (ya)  
3. Food cost accesible mid-market LATAM (F2)  
4. Command Center + Copilot en español (F4–F5)  
5. Datos ops+cost en un solo sistema (switching cost)

---

# Camino 41 → 90+

| Hito | Score | Estado producto |
|------|-------|-----------------|
| Hoy | 41 | Listo para Pilotos |
| Post F1 Money Ops | ~62 | Operación diaria sin Excel caja |
| Post F2 Cost | ~74 | Dueño ve margen |
| Post F3 Revenue | ~82 | Gana más + fiscal piloto |
| Post F4 Intelligence | ~88 | Comercialización regional + SaaS |
| Post F5 Advantage | **90+** | Competidor Enterprise LATAM |

---

# Veredicto estratégico

## RestBar no debe convertirse en un clon de Toast u Oracle.

Debe convertirse en la **plataforma de gestión** que une:

**Piso certificado + Margen controlado + Decisiones automáticas**

a precio y UX de Latinoamérica.

### Primero, segundo, tercero

1. **Dinero y cierre** (caja, precuenta, export)  
2. **Costo y compras** (PO, food cost, merma)  
3. **Crecimiento e inteligencia** (promos, CC, BI, Copilot, SaaS)

Con esa secuencia, RestBar deja de ser “un POS bueno” y pasa a ser **el sistema por el que un dueño sí paga — y renueva**.

---

# Entregables del programa

Ver `README.md` — documentos 01–20 en esta carpeta.

**Próximo paso operativo:** ejecutar F0 Harden + abrir epic RB-010 Caja (aún no desarrollado en este programa; solo diseñado).
