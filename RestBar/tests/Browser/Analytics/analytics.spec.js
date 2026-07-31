const { test, expect } = require('@playwright/test');
const { loginAsAdmin, collectConsoleErrors, significantConsoleErrors, expectNoHttp500 } = require('../helpers/auth');

const REPORT_KEYS = [
  'executive-summary',
  'sales-hour',
  'sales-product',
  'cash-summary',
  'inventory-health',
];

test.describe('RB-025 Executive Analytics', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('AN-01 Centro Ejecutivo loads', async ({ page }) => {
    const errors = await collectConsoleErrors(page);
    const res = await expectNoHttp500(page, '/ExecutiveAnalytics');
    expect(res.status()).toBe(200);
    await expect(page.getByRole('heading', { name: /Centro Ejecutivo/i })).toBeVisible();
    // Bootstrap may expose nav-link buttons as role=tab
    await expect(page.locator('.nav-tabs').getByText(/Ahora mismo/i)).toBeVisible();
    await expect(page.locator('.nav-tabs').getByText(/Rendimiento/i)).toBeVisible();
    await expect(page.locator('.nav-tabs').getByText(/Decisiones/i)).toBeVisible();
    await expect(page.locator('.nav-tabs').getByText(/Reportes/i)).toBeVisible();
    await page.screenshot({ path: '../../RB-025_NATIVE_BI_ENTERPRISE/evidence/an-01-center.png', fullPage: true });
    expect(significantConsoleErrors(errors).length, JSON.stringify(errors)).toBe(0);
  });

  test('AN-02 Live KPIs endpoint returns JSON', async ({ page }) => {
    await page.goto('/ExecutiveAnalytics');
    const res = await page.request.get('/ExecutiveAnalytics/Live?period=today');
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toBeTruthy();
  });

  test('AN-03 Period tabs load data', async ({ page }) => {
    await page.goto('/ExecutiveAnalytics');
    await page.locator('.nav-tabs').getByText(/Rendimiento/i).click();
    await page.waitForTimeout(1500);
    await expect(page.locator('#tab-period')).toBeVisible();
    await page.locator('.nav-tabs').getByText(/Decisiones/i).click();
    await page.waitForTimeout(1000);
    await expect(page.locator('#decisionList')).toBeVisible();
    await page.screenshot({ path: '../../RB-025_NATIVE_BI_ENTERPRISE/evidence/an-03-period-decide.png', fullPage: true });
  });

  test('AN-04 Report shell + ReportData for catalog keys', async ({ page }) => {
    for (const key of REPORT_KEYS) {
      const pageRes = await expectNoHttp500(page, `/ExecutiveAnalytics/Report?key=${key}&period=last_30`);
      expect(pageRes.status(), key).toBeLessThan(500);
      const dataRes = await page.request.get(`/ExecutiveAnalytics/ReportData?key=${key}&period=last_30`);
      expect(dataRes.status(), `ReportData ${key}`).toBe(200);
    }
  });

  test('AN-05 Export CSV + XLSX', async ({ page }) => {
    const csv = await page.request.get('/ExecutiveAnalytics/Export?key=sales-hour&format=csv&period=last_30');
    expect(csv.status()).toBe(200);
    const csvType = csv.headers()['content-type'] || '';
    expect(csvType).toMatch(/csv|octet|text/i);

    const xlsx = await page.request.get('/ExecutiveAnalytics/Export?key=sales-hour&format=xlsx&period=last_30');
    expect(xlsx.status()).toBe(200);
    expect((await xlsx.body()).byteLength).toBeGreaterThan(100);
  });

  test('AN-06 Unauthenticated redirects', async ({ browser }) => {
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    await page.goto('/ExecutiveAnalytics');
    await expect(page).toHaveURL(/Auth\/Login/);
    await ctx.close();
  });
});
