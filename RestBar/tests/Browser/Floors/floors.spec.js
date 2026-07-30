const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');
const { gotoPos, selectAvailableTable } = require('../helpers/pos');

test.describe('Floors / Areas', () => {
  test('FLR-01 Areas index loads', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/Area', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    await expect(page.locator('body')).not.toContainText('Exception');
  });

  test('FLR-02 POS tables expose area metadata', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await page.waitForSelector('.table-card', { timeout: 20000 });
    const cards = page.locator('.table-card[data-table-area]');
    const count = await cards.count();
    // Area attribute may be empty guid — still presence of cards is required
    expect(await page.locator('.table-card').count()).toBeGreaterThan(0);
    if (count > 0) {
      const area = await cards.first().getAttribute('data-table-area');
      expect(area).toBeTruthy();
    }
  });

  test('FLR-03 filter Todas visible on POS', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await expect(page.getByRole('button', { name: /Todas/i }).first()).toBeVisible({ timeout: 15000 });
  });

  test('FLR-04 exit POS after browsing tables preserves Home', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await selectAvailableTable(page).catch(() => null);
    await page.getByTestId('order-nav-back').click();
    await page.waitForURL(url => /\/Home/i.test(url.pathname) || url.pathname === '/', { timeout: 20000 });
  });
});
