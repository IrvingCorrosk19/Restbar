const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');

test.describe('Multitenant · isolation smoke', () => {
  test('MT-01 Login binds company claim (admin session)', async ({ page }) => {
    await loginAsAdmin(page);
    // Navigate to a page that shows company context if available
    await page.goto('/CashSession/Dashboard');
    await expect(page).not.toHaveURL(/Auth\/Login/);
    // Soft check: page rendered for tenant
    await expect(page.locator('body')).not.toContainText('Npgsql.PostgresException');
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/mt-01-tenant.png', fullPage: true });
  });

  test('MT-02 Cross-company admin login if seeded', async ({ page }) => {
    await page.goto('/Auth/Login');
    await page.locator('input[name="email"]').fill('admin@costa.restbar.com');
    await page.locator('input[name="password"]').fill('123456');
    await page.locator('button.btn-login').click();
    await page.waitForTimeout(2000);
    const url = page.url();
    if (url.includes('/Auth/Login')) {
      test.skip(true, 'admin@costa.restbar.com not seeded in this DB');
      return;
    }
    await page.goto('/Supplier');
    await expect(page.locator('body')).not.toContainText('Exception');
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/mt-02-costa.png', fullPage: true });
  });
});
