const { test, expect } = require('@playwright/test');
const { loginAsAdmin, expectNoHttp500 } = require('../helpers/auth');

test.describe('Cash reports · plan CASH-Z / CASH-XREP', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('CASH-Z-01 ZReport page', async ({ page }) => {
    const res = await page.goto('/CashReport/ZReport', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    const text = await page.locator('body').innerText();
    expect(/Z|Cierre|Reporte|deshabilit|Session|Sesión/i.test(text)).toBeTruthy();
  });

  test('CASH-XREP-01 XReport page', async ({ page }) => {
    const res = await expectNoHttp500(page, '/CashReport/XReport');
    expect(res.status()).toBeLessThan(500);
  });
});
