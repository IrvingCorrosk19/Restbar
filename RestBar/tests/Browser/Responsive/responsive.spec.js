const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');
const { gotoPos, selectAvailableTable, addFirstProduct } = require('../helpers/pos');

test.describe('Responsive POS / Cash', () => {
  test('RSP-01 POS chrome on mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 412, height: 915 });
    await loginAsAdmin(page);
    await gotoPos(page);
    await expect(page.getByTestId('order-nav-home')).toBeVisible();
    await expect(page.getByTestId('order-nav-back')).toBeVisible();
  });

  test('RSP-02 POS chrome on tablet viewport', async ({ page }) => {
    await page.setViewportSize({ width: 834, height: 1194 });
    await loginAsAdmin(page);
    await gotoPos(page);
    await selectAvailableTable(page).catch(() => null);
    await expect(page.getByTestId('order-pos-chrome')).toBeVisible();
  });

  test('RSP-03 Cash dashboard mobile', async ({ page }) => {
    await page.setViewportSize({ width: 412, height: 915 });
    await loginAsAdmin(page);
    const res = await page.goto('/CashSession/Dashboard', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });
});
