# 📋 EXPLICACIÓN - OrderNumber

## 🔍 CÓMO FUNCIONA ACTUALMENTE

El `OrderNumber` **NO es único globalmente** en la base de datos. Es **secuencial por compañía**.

### Ejemplo Actual:

**Compañía A:**
- Orden 1: `OrderNumber = "000001"`
- Orden 2: `OrderNumber = "000002"`
- Orden 3: `OrderNumber = "000003"`

**Compañía B:**
- Orden 1: `OrderNumber = "000001"` ✅ (Mismo número, diferente compañía)
- Orden 2: `OrderNumber = "000002"` ✅ (Mismo número, diferente compañía)
- Orden 3: `OrderNumber = "000003"` ✅ (Mismo número, diferente compañía)

### Código Actual (`GenerateOrderNumberAsync`):

```csharp
private async Task<string> GenerateOrderNumberAsync(Guid? companyId)
{
    // Filtra órdenes por CompanyId
    if (companyId.HasValue)
    {
        query = query.Where(o => o.CompanyId == companyId.Value);
    }
    
    // Busca el último número de esa compañía
    var lastOrder = await query
        .Where(o => !string.IsNullOrEmpty(o.OrderNumber) && 
                    o.OrderNumber.All(char.IsDigit))
        .OrderByDescending(o => o.OrderNumber)
        .FirstOrDefaultAsync();
    
    // Incrementa el número
    var newOrderNumber = (lastOrderNumber + 1).ToString().PadLeft(6, '0');
    return newOrderNumber; // Ej: "000001", "000002", etc.
}
```

## ❓ ¿POR QUÉ NO ES ÚNICO GLOBALMENTE?

1. **Multi-tenancy**: Cada compañía tiene su propia secuencia de números
2. **Simplicidad**: No hay restricción de unicidad en la BD
3. **Identificación**: El `Id` (Guid) es único globalmente, el `OrderNumber` es solo para mostrar al usuario

## 🎯 OPCIONES

### OPCIÓN 1: Mantener como está (Secuencial por Compañía) ✅ ACTUAL
- ✅ Simple
- ✅ Cada compañía empieza desde 000001
- ❌ Puede haber duplicados entre compañías diferentes

### OPCIÓN 2: Único Globalmente
- ✅ Garantiza que nunca habrá duplicados
- ❌ Requiere agregar restricción UNIQUE en BD
- ❌ Números más altos (000001, 000002, ..., 999999 globalmente)

### OPCIÓN 3: Único por Compañía (Índice Compuesto)
- ✅ Garantiza unicidad dentro de cada compañía
- ✅ Cada compañía empieza desde 000001
- ❌ Requiere índice único compuesto (CompanyId, OrderNumber)

## 💡 RECOMENDACIÓN

**Mantener como está (Opción 1)** porque:
1. El `Id` (Guid) ya es único globalmente
2. El `OrderNumber` es solo para mostrar (formato amigable: "000001")
3. En un sistema multi-tenant, cada compañía debe tener su propia secuencia
4. No hay riesgo de conflictos porque se filtra por `CompanyId`

¿Quieres cambiar esto? ¿Necesitas que sea único globalmente o único por compañía?

