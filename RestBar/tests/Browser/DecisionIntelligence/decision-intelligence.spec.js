const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');

test.describe('Decision Intelligence — RB-028', () => {
  test('DI-01 cockpit loads or shows module disabled (no 500)', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/DecisionIntelligence/Cockpit', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    await expect(page.locator('body')).not.toContainText('Npgsql.PostgresException');
  });

  test('DI-02 forecast page soft', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/DecisionIntelligence/Forecast', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('DI-03 recommendations page soft', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/DecisionIntelligence/Recommendations', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('DI-04 API executive not 500 when enabled', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/api/decision-intelligence/executive');
    expect([200, 403, 503]).toContain(res.status());
    expect(res.status()).not.toBe(500);
  });

  test('DI-05 data-quality API', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/api/decision-intelligence/data-quality');
    expect([200, 503]).toContain(res.status());
    if (res.status() === 200) {
      const json = await res.json();
      expect(json.globalScore ?? json.GlobalScore).toBeTruthy();
    }
  });

  test('DI-06 simulation does not 500', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/api/decision-intelligence/simulations/sales?pctChange=10', {
      data: { pctChange: 10 },
    });
    expect([200, 400, 403, 503]).toContain(res.status());
    expect(res.status()).not.toBe(500);
  });
});
