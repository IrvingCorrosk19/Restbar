const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');
const { gotoPos, selectAvailableTable, addFirstProduct, sendToKitchen } = require('../helpers/pos');

test.describe('Inventory impact from orders', () => {
  test('INV-ORD-01 inventory page still works after kitchen send', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await selectAvailableTable(page);
    await addFirstProduct(page);
    await sendToKitchen(page);
    const res = await page.goto('/Inventory', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBe(200);
    const low = await page.request.get('/Inventory/GetLowStockProducts');
    expect(low.status()).toBe(200);
    const body = await low.json();
    expect(body.success).toBeTruthy();
  });

  test('INV-ORD-02 GetInventoryData after order activity', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/Inventory/GetInventoryData');
    expect(res.status()).toBe(200);
    const json = await res.json();
    expect(json.success).toBeTruthy();
  });
});
