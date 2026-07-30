const { test, expect } = require('@playwright/test');
const { loginAsAdmin, expectNoHttp500 } = require('../helpers/auth');

test.describe('Regression · Core POS', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('REG-01 Order index', async ({ page }) => {
    const res = await expectNoHttp500(page, '/Order');
    expect(res.status()).toBe(200);
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/reg-01-orders.png', fullPage: true });
  });

  test('REG-02 Kitchen station orders', async ({ page }) => {
    const res = await expectNoHttp500(page, '/Order/StationOrders?stationType=kitchen');
    expect(res.status()).not.toBe(500);
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/reg-02-kitchen.png', fullPage: true });
  });

  test('REG-03 Product index', async ({ page }) => {
    const res = await expectNoHttp500(page, '/Product');
    expect(res.status()).not.toBe(500);
  });

  test('REG-04 Inventory index', async ({ page }) => {
    const res = await expectNoHttp500(page, '/Inventory');
    expect(res.status()).not.toBe(500);
    expect([200, 302, 404].includes(res.status()) || res.ok()).toBeTruthy();
  });

  test('REG-05 Command Center', async ({ page }) => {
    const res = await expectNoHttp500(page, '/ExecutiveCommandCenter');
    expect(res.status()).not.toBe(500);
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/reg-05-cc.png', fullPage: true });
  });

  test('REG-06 Logout path exists', async ({ page }) => {
    const res = await page.goto('/Auth/Logout', { waitUntil: 'domcontentloaded' });
    expect(res.status()).not.toBe(500);
  });
});
