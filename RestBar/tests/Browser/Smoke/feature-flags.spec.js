const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');

test.describe('Feature Flags · modules enabled in Development', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('FF-01 Cash not ModuleDisabled', async ({ page }) => {
    await page.goto('/CashSession/Dashboard');
    await expect(page.locator('body')).not.toContainText('EnableCashModule');
  });

  test('FF-02 Purchasing not ModuleDisabled', async ({ page }) => {
    await page.goto('/Supplier');
    await expect(page.locator('body')).not.toContainText('EnablePurchasingModule');
  });

  test('FF-03 FoodCost not ModuleDisabled', async ({ page }) => {
    await page.goto('/FoodCostDashboard');
    await expect(page.locator('body')).not.toContainText('EnableFoodCostModule');
  });
});
