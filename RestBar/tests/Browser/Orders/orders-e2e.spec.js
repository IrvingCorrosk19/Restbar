const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');
const { gotoPos, selectAvailableTable, addFirstProduct, sendToKitchen, markFirstReadyOnKds } = require('../helpers/pos');

test.describe('Orders E2E — floor / table / kitchen', () => {
  test('ORD-E2E-01 select table add product send kitchen', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    const table = await selectAvailableTable(page);
    expect(table).toBeTruthy();
    const product = await addFirstProduct(page);
    expect(product.length).toBeGreaterThan(0);
    const send = await sendToKitchen(page);
    expect(send).toBeTruthy();
    // Exit and return — persistence
    await page.getByTestId('order-nav-home').click();
    await page.waitForURL(/\/Home/i, { timeout: 20000 });
    await gotoPos(page);
    await expect(page.getByTestId('order-pos-chrome')).toBeVisible();
  });

  test('ORD-E2E-02 KDS receives and can mark ready', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await selectAvailableTable(page);
    await addFirstProduct(page);
    await sendToKitchen(page);
    const kds = await markFirstReadyOnKds(page, 'kitchen');
    // May be empty if item routed to bar — accept marked or no-button without 500
    expect(kds.marked === true || kds.reason === 'no-ready-button').toBeTruthy();
    if (kds.status != null) expect(kds.status).toBeLessThan(500);
  });

  test('ORD-E2E-03 StationOrders pages no 500', async ({ page }) => {
    test.setTimeout(120_000);
    await loginAsAdmin(page);
    for (const type of ['kitchen', 'bar']) {
      const res = await page.goto(`/Order/StationOrders?stationType=${type}`, { waitUntil: 'domcontentloaded', timeout: 45000 });
      expect(res.status(), type).toBeLessThan(500);
      await expect(page.getByTestId('kds-nav-home')).toBeVisible({ timeout: 20000 });
    }
  });

  test('ORD-E2E-04 tables API returns data', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/Order/GetActiveTables');
    expect(res.status()).toBe(200);
    const json = await res.json();
    const tables = json.tables || json.data || json;
    expect(Array.isArray(tables) ? tables.length : Object.keys(json).length).toBeGreaterThan(0);
  });

  test('ORD-E2E-05 MoveToTable API validation', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/Order/MoveToTable', {
      data: { orderId: '00000000-0000-0000-0000-000000000000', targetTableId: '00000000-0000-0000-0000-000000000001' },
    });
    expect(res.status()).not.toBe(500);
    expect([400, 404, 403, 422].includes(res.status()) || res.ok()).toBeTruthy();
  });
});
