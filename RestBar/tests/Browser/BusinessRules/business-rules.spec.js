const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');

test.describe('Business Rules — RB-029', () => {
  test('BR-01 index loads or module disabled (no 500)', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/BusinessRules', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('BR-02 templates page soft', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/BusinessRules/Templates', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('BR-03 API templates', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/api/business-rules/templates');
    expect([200, 503]).toContain(res.status());
    expect(res.status()).not.toBe(500);
  });

  test('BR-04 API list', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/api/business-rules');
    expect([200, 403, 503]).toContain(res.status());
    expect(res.status()).not.toBe(500);
  });
});
