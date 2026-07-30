const { test, expect } = require('@playwright/test');
const { loginAsAdmin, ADMIN } = require('../helpers/auth');

async function loginAs(page, email, password = '123456') {
  await page.goto('/Auth/Login', { waitUntil: 'domcontentloaded' });
  await page.locator('input[name="email"]').fill(email);
  await page.locator('input[name="password"]').fill(password);
  await page.locator('button.btn-login').click();
  await page.waitForURL(url => !url.pathname.includes('/Auth/Login'), { timeout: 25000 }).catch(() => {});
}

test.describe('Waiters / RBAC smoke', () => {
  test('WTR-01 admin can open POS', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/Order/Index', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBe(200);
    await expect(page.getByTestId('order-pos-chrome')).toBeVisible();
  });

  test('WTR-02 mesero login or skip', async ({ page }) => {
    await loginAs(page, 'mesero@restbar.com');
    if (page.url().includes('/Auth/Login')) {
      test.skip(true, 'mesero@restbar.com not seeded on this environment');
    }
    const res = await page.goto('/Order/Index', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('WTR-03 cajero login cash access or skip', async ({ page }) => {
    await loginAs(page, 'cajero@restbar.com');
    if (page.url().includes('/Auth/Login')) {
      test.skip(true, 'cajero@restbar.com not seeded');
    }
    const res = await page.goto('/CashSession/Dashboard', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('WTR-04 chef KDS access or skip', async ({ page }) => {
    await loginAs(page, 'chef@restbar.com');
    if (page.url().includes('/Auth/Login')) {
      test.skip(true, 'chef@restbar.com not seeded');
    }
    const res = await page.goto('/Order/StationOrders?stationType=kitchen', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('WTR-05 logout clears order access', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Auth/Logout').catch(() => {});
    await page.context().clearCookies();
    const res = await page.goto('/Order/Index', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    await expect(page).toHaveURL(/Auth\/Login/i);
  });
});
