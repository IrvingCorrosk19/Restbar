# 06 — SUPPLIER ENGINE

---

# Ciclo de vida

```
Active ←→ OnHold
Active → Preferred (flag)
Any → Blacklisted (manager+reason+audit)
Blacklisted → Active (CEO/admin only + audit)
Inactive = no nuevos PO
```

---

# Capacidad

| Feature | v1 |
|---------|----|
| Código único por Company | ✅ |
| Contactos N | ✅ |
| Lead time / payment terms | ✅ |
| Preferred supplier | ✅ |
| Blacklist | ✅ |
| Catálogo SupplierProduct | ✅ |
| Score cached | ✅ |
| Contratos / docs adjuntos | v1.1 |
| Multi-branch visibility | Company-scoped; branch filtra POs |

---

# Preferred & ranking

Al sugerir proveedor para un producto:

1. SupplierProduct.active del branch company  
2. Excluir Blacklisted / OnHold  
3. Ordenar por: Preferred DESC, overall_score DESC, agreed_unit_price ASC  

Command Center usa mismo ranking.

---

# Integración UI legacy

Rutas compatibles con `wwwroot/js/supplier/supplier-management.js`:

- GET `/Supplier/GetSuppliers`  
- POST `/Supplier/CreateSupplier`  
- POST `/Supplier/Edit` / `Delete`  
- GET `/Supplier/GetSupplierProducts`  

Implementar en `SupplierController` bajo `PurchasingAccess` + feature flag.
