const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');

test.describe('Shifts / users operational', () => {
  test('SHF-01 Users page loads', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/User', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('SHF-02 User assignments page', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/UserAssignment', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('SHF-03 Cash session survives navigation', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/CashSession/Dashboard');
    await page.goto('/Order/Index');
    await page.goto('/CashSession/Dashboard');
    const text = await page.locator('body').innerText();
    expect(/Exception/i.test(text)).toBeFalsy();
  });
});
