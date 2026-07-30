const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');
const { gotoPos, selectAvailableTable, addFirstProduct, sendToKitchen } = require('../helpers/pos');

test.describe('Tables', () => {
  test('TBL-01 Tables management page', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/Table', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('TBL-02 POS lists multiple tables', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await expect.poll(async () => page.locator('.table-card, .select-table-btn').count(), { timeout: 20000 }).toBeGreaterThan(1);
  });

  test('TBL-03 select table enables order surface', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await selectAvailableTable(page);
    // Categories or products should appear
    await expect(page.locator('body')).toContainText(/Categor|Producto|Resumen/i);
  });

  test('TBL-04 GetActiveTables no 500', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/Order/GetActiveTables');
    expect(res.status()).toBe(200);
  });
});
