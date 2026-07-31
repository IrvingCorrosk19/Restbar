const { test, expect } = require('@playwright/test');
const { loginAsAdmin, expectNoHttp500 } = require('../helpers/auth');

test.describe('Reports & Exports · plan RPT-*', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('RPT-01 Reports Index', async ({ page }) => {
    const res = await expectNoHttp500(page, '/Reports');
    expect(res.status()).toBe(200);
  });

  test('RPT-02 AdvancedReports Index', async ({ page }) => {
    const res = await expectNoHttp500(page, '/AdvancedReports');
    expect(res.status()).toBe(200);
    await expect(page.locator('body')).toContainText(/Advanced|Reporte|Análisis|Analisis/i);
  });

  test('RPT-03 AdvancedReports Export Excel non-empty', async ({ page }) => {
    const res = await page.request.get('/AdvancedReports/ExportToExcel?reportType=sales&startDate=2026-01-01&endDate=2026-12-31');
    expect(res.status()).toBe(200);
    const body = await res.body();
    expect(body.byteLength).toBeGreaterThan(100);
  });

  test('RPT-04 Reports ExportPdf stub is not 500', async ({ page }) => {
    const res = await page.request.get('/Reports/ExportPdf');
    expect(res.status()).not.toBe(500);
  });

  test('BI-01 BiNative Index', async ({ page }) => {
    await expectNoHttp500(page, '/BiNative');
  });

  test('ECC-01 Command Center Index', async ({ page }) => {
    const res = await expectNoHttp500(page, '/ExecutiveCommandCenter');
    expect(res.status()).toBeLessThan(500);
  });
});
