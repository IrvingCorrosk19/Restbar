-- Script de prueba para verificar la integración contable
-- Ejecutar después de configurar el sistema

-- 1. Verificar que el catálogo de cuentas esté inicializado
SELECT COUNT(*) as total_accounts FROM accounts WHERE created_by = 'Sistema';

-- 2. Verificar que hay órdenes completadas
SELECT COUNT(*) as completed_orders FROM orders WHERE status = 'Completed';

-- 3. Verificar que hay pagos procesados
SELECT COUNT(*) as completed_payments FROM payments WHERE status = 'COMPLETED';

-- 4. Verificar que hay asientos contables creados automáticamente
SELECT COUNT(*) as journal_entries FROM journal_entries WHERE created_by = 'Sistema';

-- 5. Verificar asientos contables por tipo
SELECT 
    type,
    COUNT(*) as entry_count,
    SUM(total_debit) as total_debit,
    SUM(total_credit) as total_credit
FROM journal_entries 
WHERE created_by = 'Sistema'
GROUP BY type;

-- 6. Verificar detalles de asientos contables
SELECT 
    je.entry_number,
    je.description,
    je.total_debit,
    je.total_credit,
    jed.description as detail_description,
    jed.debit,
    jed.credit,
    a.code as account_code,
    a.name as account_name
FROM journal_entries je
JOIN journal_entry_details jed ON je.id = jed.journal_entry_id
JOIN accounts a ON jed.account_id = a.id
WHERE je.created_by = 'Sistema'
ORDER BY je.entry_date DESC, je.entry_number;

-- 7. Verificar balance de cuentas principales
SELECT 
    a.code,
    a.name,
    COALESCE(SUM(jed.debit), 0) as total_debit,
    COALESCE(SUM(jed.credit), 0) as total_credit,
    COALESCE(SUM(jed.debit), 0) - COALESCE(SUM(jed.credit), 0) as balance
FROM accounts a
LEFT JOIN journal_entry_details jed ON a.id = jed.account_id
LEFT JOIN journal_entries je ON jed.journal_entry_id = je.id AND je.status = 'Posted'
WHERE a.created_by = 'Sistema'
GROUP BY a.id, a.code, a.name
ORDER BY a.code;

-- 8. Verificar integración con ventas
SELECT 
    o.order_number,
    o.total_amount,
    o.status,
    o.closed_at,
    COUNT(p.id) as payment_count,
    SUM(p.amount) as total_paid,
    COUNT(je.id) as accounting_entries
FROM orders o
LEFT JOIN payments p ON o.id = p.order_id AND p.status = 'COMPLETED'
LEFT JOIN journal_entries je ON o.id = je.order_id
WHERE o.status = 'Completed'
GROUP BY o.id, o.order_number, o.total_amount, o.status, o.closed_at
ORDER BY o.closed_at DESC;

-- 9. Verificar que los asientos estén balanceados
SELECT 
    je.entry_number,
    je.description,
    je.total_debit,
    je.total_credit,
    CASE 
        WHEN je.total_debit = je.total_credit THEN '✅ Balanceado'
        ELSE '❌ Desbalanceado'
    END as balance_status
FROM journal_entries je
WHERE je.created_by = 'Sistema'
ORDER BY je.entry_date DESC;

-- 10. Resumen de integración
SELECT 
    'Catálogo de Cuentas' as item,
    COUNT(*) as count
FROM accounts WHERE created_by = 'Sistema'
UNION ALL
SELECT 
    'Órdenes Completadas',
    COUNT(*)
FROM orders WHERE status = 'Completed'
UNION ALL
SELECT 
    'Pagos Procesados',
    COUNT(*)
FROM payments WHERE status = 'COMPLETED'
UNION ALL
SELECT 
    'Asientos Contables',
    COUNT(*)
FROM journal_entries WHERE created_by = 'Sistema'
UNION ALL
SELECT 
    'Asientos Balanceados',
    COUNT(*)
FROM journal_entries 
WHERE created_by = 'Sistema' AND total_debit = total_credit; 