# Sistema Contable RestBar

## 📊 **Descripción General**

El sistema contable de RestBar es un módulo completo que procesa datos financieros reales del restaurante, conectando con las ventas, pagos y gastos para generar reportes financieros en tiempo real.

## 🏗️ **Arquitectura del Sistema**

### **Componentes Principales:**

1. **`IAccountingService`** - Interfaz principal del servicio contable
2. **`AccountingService`** - Implementación del servicio con lógica de negocio
3. **`AccountingController`** - Controlador con endpoints REST
4. **`accounting.js`** - JavaScript del frontend
5. **`Index.cshtml`** - Vista del dashboard contable

### **Modelos de Datos:**

- **`FinancialSummaryDto`** - Resumen financiero
- **`IncomeDetailsDto`** - Detalles de ingresos
- **`ExpenseDetailsDto`** - Detalles de gastos
- **`TaxSummaryDto`** - Resumen de impuestos

## 🔄 **Flujo de Datos**

### **1. Procesamiento de Ventas**
```csharp
// Cuando se completa una orden
await _accountingService.CreateAccountingEntryFromOrderAsync(orderId);
```

**Asientos automáticos generados:**
- **Débito:** Caja (1101) - Monto total
- **Crédito:** Ventas (4101) - Subtotal
- **Crédito:** IVA por Cobrar (2101) - IVA

### **2. Procesamiento de Pagos**
```csharp
// Cuando se registra un pago
await _accountingService.CreateAccountingEntryFromPaymentAsync(paymentId);
```

**Asientos automáticos generados:**
- **Débito:** Caja (1101) - Monto del pago
- **Crédito:** Cuentas por Cobrar (1201) - Monto del pago

### **3. Cálculos Financieros**

#### **Ingresos Totales:**
```csharp
// Suma de todos los pagos completados
var totalIncome = await _context.Payments
    .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate && 
               p.Status == "COMPLETED" && !p.IsVoided.GetValueOrDefault())
    .SumAsync(p => p.Amount);
```

#### **Gastos Totales:**
```csharp
// Suma de asientos contables de tipo gasto
var totalExpenses = await _context.JournalEntries
    .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate && 
               j.Status == JournalEntryStatus.Posted &&
               j.Type == JournalEntryType.Expense)
    .SumAsync(j => j.TotalDebit);
```

#### **Cálculo de Impuestos:**
```csharp
// IVA Cobrado (16% de las ventas)
var ivaCollected = totalIncome * 0.16m / 1.16m;

// IVA a Pagar
var ivaToPay = ivaCollected - ivaPaid;

// ISR (30% del beneficio neto)
var isr = Math.Max(0, netProfit * 0.30m);
```

## 📈 **Endpoints Disponibles**

### **Dashboard y Estadísticas:**
- `GET /Accounting/FinancialSummary?period=month` - Resumen financiero
- `GET /Accounting/IncomeDetails?period=month` - Detalles de ingresos
- `GET /Accounting/ExpenseDetails?period=month` - Detalles de gastos
- `GET /Accounting/TaxSummary?period=month` - Resumen de impuestos

### **Análisis por Período:**
- `GET /Accounting/MonthlyData?year=2024` - Datos mensuales
- `GET /Accounting/DailyData?startDate=...&endDate=...` - Datos diarios

### **Reportes y Exportación:**
- `POST /Accounting/GenerateReport` - Generar reporte PDF
- `GET /Accounting/ExportData?format=xlsx` - Exportar datos Excel

### **Integración:**
- `POST /Accounting/CreateEntryFromOrder` - Crear asiento desde orden
- `POST /Accounting/CreateEntryFromPayment` - Crear asiento desde pago

## 🗂️ **Catálogo de Cuentas**

### **Estructura del Plan de Cuentas:**

```
1. ACTIVO (1000-1999)
   ├── 1101 Caja
   ├── 1102 Bancos
   ├── 1201 Cuentas por Cobrar
   ├── 1301 Inventario de Mercancías
   └── 1401 Gastos Pagados por Anticipado

2. PASIVO (2000-2999)
   ├── 2101 IVA por Cobrar
   ├── 2102 IVA por Pagar
   ├── 2201 Cuentas por Pagar
   └── 2301 Impuestos por Pagar

3. CAPITAL (3000-3999)
   ├── 3101 Capital Social
   ├── 3201 Utilidades Retenidas
   └── 3301 Utilidad del Ejercicio

4. INGRESOS (4000-4999)
   ├── 4101 Ventas de Comida
   ├── 4102 Ventas de Bebidas
   ├── 4103 Ventas de Postres
   └── 4104 Servicios de Catering

5. GASTOS (5000-5999)
   ├── 5101 Costo de Ventas - Comida
   ├── 5102 Costo de Ventas - Bebidas
   ├── 5201 Gastos de Personal
   ├── 5202 Gastos de Renta
   └── 5203 Servicios Públicos
```

## 🔧 **Configuración Inicial**

### **1. Ejecutar Script de Inicialización:**
```sql
-- Ejecutar el script para crear el catálogo de cuentas
\i Scripts/InitializeChartOfAccounts.sql
```

### **2. Registrar Servicios:**
```csharp
// En Program.cs
builder.Services.AddScoped<IAccountingService, AccountingService>();
```

### **3. Verificar Permisos:**
```csharp
// Asegurar que el usuario tenga acceso contable
[Authorize(Policy = "AccountingAccess")]
```

## 📊 **Dashboard Contable**

### **Estadísticas Principales:**
- **Ingresos Totales** - Suma de todas las ventas
- **Gastos Totales** - Suma de todos los gastos registrados
- **Beneficio Neto** - Ingresos - Gastos
- **Impuestos** - IVA + ISR calculados

### **Gráficos Interactivos:**
- **Línea de Tiempo** - Evolución mensual de ingresos, gastos y beneficios
- **Filtros por Período** - Mes, trimestre, año
- **Datos en Tiempo Real** - Actualización automática

### **Tablas de Detalle:**
- **Ingresos** - Lista de ventas con fechas y montos
- **Gastos** - Lista de gastos por categoría
- **Impuestos** - Desglose de IVA e ISR

## 🚀 **Funcionalidades Avanzadas**

### **1. Integración Automática:**
- **Ventas** → Asientos contables automáticos
- **Pagos** → Registro automático en caja
- **Gastos** → Registro manual con categorización

### **2. Cálculos Automáticos:**
- **IVA** - 16% sobre ventas
- **ISR** - 30% sobre beneficio neto
- **Margen de Beneficio** - Porcentaje de rentabilidad

### **3. Reportes:**
- **PDF** - Reportes financieros en PDF
- **Excel** - Exportación de datos en Excel
- **Gráficos** - Visualizaciones interactivas

## 🔒 **Seguridad y Permisos**

### **Políticas de Acceso:**
```csharp
[Authorize(Policy = "AccountingAccess")]
```

### **Roles con Acceso:**
- **Contador** - Acceso completo
- **Gerente** - Acceso de lectura
- **Admin** - Acceso completo

## 📝 **Notas de Implementación**

### **Consideraciones Importantes:**

1. **Datos Reales:** El sistema procesa datos reales de ventas y pagos
2. **Cálculos Automáticos:** IVA e ISR se calculan automáticamente
3. **Integración Completa:** Conecta con el sistema de ventas existente
4. **Escalabilidad:** Diseñado para manejar grandes volúmenes de datos
5. **Auditoría:** Todos los cambios quedan registrados

### **Mantenimiento:**

1. **Backup Regular:** Respaldo de datos contables
2. **Verificación Mensual:** Revisión de balances
3. **Actualización de Impuestos:** Mantener tasas actualizadas
4. **Monitoreo de Rendimiento:** Optimización de consultas

## 🎯 **Próximas Mejoras**

1. **Reportes Avanzados** - Balance general, estado de resultados
2. **Conciliación Bancaria** - Integración con bancos
3. **Presupuestos** - Planificación financiera
4. **Análisis de Rentabilidad** - Por producto/categoría
5. **Auditoría Avanzada** - Trazabilidad completa

---

**Desarrollado para RestBar - Sistema de Gestión de Restaurantes** 