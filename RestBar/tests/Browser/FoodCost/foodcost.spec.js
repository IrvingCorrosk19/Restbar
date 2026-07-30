const { test, expect } = require('@playwright/test');
const { loginAsAdmin, collectConsoleErrors, significantConsoleErrors, expectNoHttp500 } = require('../helpers/auth');

test.describe('RB-023 Food Cost', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('FC-01 Food Cost dashboard', async ({ page }) => {
    const errors = await collectConsoleErrors(page);
    const res = await expectNoHttp500(page, '/FoodCostDashboard');
    expect(res.status()).toBe(200);
    await expect(page.getByRole('heading', { name: /Food Cost/i })).toBeVisible();
    await expect(page.getByText(/FC%|Variance|Waste|Teórico|Teorico/i).first()).toBeVisible();
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/fc-01-dashboard.png', fullPage: true });
    expect(significantConsoleErrors(errors).length, JSON.stringify(errors)).toBe(0);
  });

  test('FC-02 Menu Engineering page', async ({ page }) => {
    await expectNoHttp500(page, '/FoodCostDashboard/MenuEngineering');
    await expect(page.locator('body')).not.toContainText('Exception');
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/fc-02-menu-eng.png', fullPage: true });
  });

  test('FC-03 Recipes index', async ({ page }) => {
    await expectNoHttp500(page, '/Recipe');
    await expect(page.getByRole('heading', { name: /Recetas/i })).toBeVisible();
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/fc-03-recipes.png', fullPage: true });
  });

  test('FC-04 Recipe cost link if products exist', async ({ page }) => {
    await page.goto('/Recipe');
    const link = page.getByRole('link', { name: /Ver costo/i }).first();
    if (await link.count() === 0) {
      test.skip(true, 'No recipe cost links');
      return;
    }
    await link.click();
    await expect(page.getByRole('heading', { name: /Costo/i })).toBeVisible();
    await expect(page.getByText(/Food Cost %|Margen|Precio/i).first()).toBeVisible();
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/fc-04-plate-cost.png', fullPage: true });
  });

  test('FC-05 PlateCost API no 500 for empty guid', async ({ page }) => {
    const res = await page.request.get('/FoodCostDashboard/PlateCost?productId=00000000-0000-0000-0000-000000000000');
    expect(res.status()).not.toBe(500);
  });
});
