const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');

test.describe('Inventory enterprise — RB-024', () => {
  test('INV-E01 enterprise snapshot endpoint', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/Inventory/GetEnterpriseSnapshot');
    expect(res.status()).toBe(200);
    const json = await res.json();
    expect(json.success).toBeTruthy();
    expect(json.data).toBeTruthy();
    expect(typeof json.data.inventoryValue).toBe('number');
    expect(typeof json.data.criticalStockCount).toBe('number');
    expect(Array.isArray(json.data.recentMovements)).toBeTruthy();
  });

  test('INV-E02 transfer reject invalid id soft', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/StockTransfer/Reject?id=00000000-0000-0000-0000-000000000000', {
      data: { reason: 'cert' },
    });
    expect(res.status()).not.toBe(500);
  });

  test('INV-E03 movement purchase negative amount not 500', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/InventoryMovement/CreatePurchase', {
      data: {
        productId: '00000000-0000-0000-0000-000000000001',
        quantity: -5,
        unitCost: 1,
      },
    });
    const status = res.status();
    const body = await res.json().catch(() => ({}));
    expect(status, JSON.stringify(body)).toBe(400);
    expect(body.success === false || !!body.message).toBeTruthy();
  });

  test('INV-E04 adjustment invalid product soft', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/InventoryMovement/CreateAdjustment', {
      data: {
        productId: '00000000-0000-0000-0000-000000000099',
        quantity: 1,
        reason: 'cert-neg',
      },
    });
    expect([400, 404]).toContain(res.status());
    expect(res.status()).not.toBe(500);
  });

  test('INV-E05 inventory index + snapshot no console crash', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/Inventory', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    const snap = await page.request.get('/Inventory/GetEnterpriseSnapshot');
    expect((await snap.json()).success).toBeTruthy();
  });
});
