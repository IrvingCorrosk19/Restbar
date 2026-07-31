const { test, expect } = require('@playwright/test');
const { loginAsAdmin, collectConsoleErrors, significantConsoleErrors, expectNoHttp500 } = require('../helpers/auth');

test.describe('Auth extended · plan AUTH-*', () => {
  test('AUTH-01 Invalid login stays on login', async ({ page }) => {
    await page.goto('/Auth/Login', { waitUntil: 'domcontentloaded' });
    await page.locator('input[name="email"]').fill('admin@restbar.com');
    await page.locator('input[name="password"]').fill('wrong-password-xyz');
    await page.locator('button.btn-login').click({ noWaitAfter: true });
    await page.waitForTimeout(1500);
    await expect(page).toHaveURL(/Auth\/Login/);
  });

  test('AUTH-02 Logout returns to login', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Home/Index', { waitUntil: 'domcontentloaded' });
    await page.locator('#userDropdown').click();
    const menu = page.locator('.dropdown-menu.show, ul.dropdown-menu[aria-labelledby="userDropdown"]');
    await expect(menu).toBeVisible({ timeout: 10000 });
    const logoutBtn = menu.locator('button.logout-btn');
    await expect(logoutBtn).toBeVisible();
    await logoutBtn.click();
    await page.waitForURL(/Auth\/Login/, { timeout: 30000 });
    await page.goto('/CashSession/Dashboard');
    await expect(page).toHaveURL(/Auth\/Login/);
  });

  test('AUTH-03 ForgotPassword reachable', async ({ page }) => {
    const res = await page.goto('/Auth/ForgotPassword', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    await expect(page.locator('body')).not.toContainText('Exception');
  });

  test('AUTH-04 Profile page', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await expectNoHttp500(page, '/Auth/Profile');
    expect(res.status()).toBe(200);
    await expect(page.locator('body')).toContainText(/Perfil|Profile|email|Usuario/i);
  });

  test('AUTH-05 Two contexts isolated sessions', async ({ browser }) => {
    const adminCtx = await browser.newContext();
    const anonCtx = await browser.newContext();
    const adminPage = await adminCtx.newPage();
    const anonPage = await anonCtx.newPage();
    await loginAsAdmin(adminPage);
    await adminPage.goto('/ExecutiveAnalytics');
    await expect(adminPage.getByRole('heading', { name: /Centro Ejecutivo/i })).toBeVisible();
    await anonPage.goto('/ExecutiveAnalytics');
    await expect(anonPage).toHaveURL(/Auth\/Login/);
    await adminCtx.close();
    await anonCtx.close();
  });
});
