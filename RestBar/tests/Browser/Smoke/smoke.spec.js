const { test, expect } = require('@playwright/test');
const { loginAsAdmin, expectNoHttp500 } = require('../helpers/auth');

test.describe('Smoke · Auth + Shell', () => {
  test('SMK-01 Login admin succeeds', async ({ page }) => {
    await loginAsAdmin(page);
    await expect(page).not.toHaveURL(/Auth\/Login/);
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/smk-01-login.png', fullPage: true });
  });

  test('SMK-02 Unauthenticated protected route redirects to login', async ({ page }) => {
    await page.goto('/CashSession/Dashboard');
    await expect(page).toHaveURL(/Auth\/Login/);
  });

  test('SMK-03 Orders index loads after login (regression shell)', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await expectNoHttp500(page, '/Order');
    expect([200, 302].includes(res.status()) || res.status() < 400).toBeTruthy();
  });
});
