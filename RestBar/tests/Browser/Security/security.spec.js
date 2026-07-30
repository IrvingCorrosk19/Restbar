const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');

test.describe('Security · AuthZ gates', () => {
  test('SEC-01 Anonymous CashSession → login', async ({ page }) => {
    await page.goto('/CashSession/Dashboard');
    await expect(page).toHaveURL(/Auth\/Login/);
  });

  test('SEC-02 Anonymous Supplier → login', async ({ page }) => {
    await page.goto('/Supplier');
    await expect(page).toHaveURL(/Auth\/Login/);
  });

  test('SEC-03 Anonymous FoodCost → login', async ({ page }) => {
    await page.goto('/FoodCostDashboard');
    await expect(page).toHaveURL(/Auth\/Login/);
  });

  test('SEC-04 Anonymous paid-out API not 500', async ({ page }) => {
    const res = await page.request.post('/api/CashMovement/paid-out', {
      data: { sessionId: '00000000-0000-0000-0000-000000000000', amount: 1 }
    });
    expect([401, 403, 404, 302, 200, 400].includes(res.status()) || res.status() < 500).toBeTruthy();
    expect(res.status()).not.toBe(500);
  });

  test('SEC-05 Admin can open Cash after login', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/CashSession/Dashboard');
    await expect(page).not.toHaveURL(/Auth\/Login|AccessDenied/);
  });
});
