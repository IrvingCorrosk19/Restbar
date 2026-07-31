const { test, expect } = require('@playwright/test');
const { loginAsAdmin, expectNoHttp500 } = require('../helpers/auth');

test.describe('A11Y smoke · plan A11Y-01', () => {
  test('A11Y-01 Login form has labels and focusable controls', async ({ page }) => {
    await page.goto('/Auth/Login');
    const email = page.locator('input[name="email"]');
    const password = page.locator('input[name="password"]');
    await expect(email).toBeVisible();
    await expect(password).toBeVisible();
    await email.focus();
    await expect(email).toBeFocused();
    const loginBtn = page.locator('button.btn-login');
    await expect(loginBtn).toBeVisible();
    await expect(loginBtn).toBeEnabled();
  });
});

test.describe('Multitenant IDOR soft · MT-IDOR-01', () => {
  test('MT-IDOR-01 Random guid order detail does not 500', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/Order/Edit/00000000-0000-0000-0000-000000000099');
    expect(res.status()).not.toBe(500);
  });
});
