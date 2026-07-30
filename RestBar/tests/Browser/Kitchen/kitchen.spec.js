const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');
const { gotoPos, selectAvailableTable, addFirstProduct, sendToKitchen, markFirstReadyOnKds } = require('../helpers/pos');

test.describe('Kitchen KDS', () => {
  test('KDS-01 kitchen board loads', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/Order/StationOrders?stationType=kitchen', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBe(200);
    await expect(page.getByTestId('kds-nav-home')).toBeVisible();
  });

  test('KDS-02 bar board loads', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/Order/StationOrders?stationType=bar', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('KDS-03 send then open kitchen does not 500', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await selectAvailableTable(page);
    await addFirstProduct(page);
    await sendToKitchen(page);
    const res = await page.goto('/Order/StationOrders?stationType=kitchen', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    await markFirstReadyOnKds(page, 'kitchen');
  });

  test('KDS-04 kitchen API current', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/api/kitchen/current');
    expect(res.status()).toBeLessThan(500);
  });
});
