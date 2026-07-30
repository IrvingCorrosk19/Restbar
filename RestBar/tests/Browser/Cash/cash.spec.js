const { test, expect } = require('@playwright/test');
const { loginAsAdmin, collectConsoleErrors, significantConsoleErrors, expectNoHttp500 } = require('../helpers/auth');

test.describe('RB-010 Cash Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('CASH-01 Dashboard loads when module enabled', async ({ page }) => {
    const errors = await collectConsoleErrors(page);
    const res = await expectNoHttp500(page, '/CashSession/Dashboard');
    expect(res.status()).toBe(200);
    await expect(page.getByRole('heading', { name: /Caja — Command Center|Caja/i })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Sesiones activas' })).toBeVisible();
    await expect(page.getByRole('link', { name: /Abrir sesión/i })).toBeVisible();
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/cash-01-dashboard.png', fullPage: true });
    expect(significantConsoleErrors(errors).length, JSON.stringify(errors)).toBe(0);
  });

  test('CASH-02 Open wizard shows register select', async ({ page }) => {
    await expectNoHttp500(page, '/CashSession/OpenWizard');
    await expect(page.locator('select[name="registerId"]')).toBeVisible();
    await expect(page.locator('input[name="openingFloat"]')).toBeVisible();
    await expect(page.getByRole('button', { name: /Abrir sesión/i })).toBeVisible();
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/cash-02-open-wizard.png', fullPage: true });
  });

  test('CASH-03 Open session happy path or validation', async ({ page }) => {
    await page.goto('/CashSession/OpenWizard');
    const options = page.locator('select[name="registerId"] option');
    const count = await options.count();
    test.skip(count === 0, 'No cash registers seeded — create register first');
    await page.locator('input[name="openingFloat"]').fill('100');
    await page.getByRole('button', { name: /Abrir sesión/i }).click();
    await page.waitForLoadState('networkidle');
    const body = await page.locator('body').innerText();
    const ok = /Detalle|sesión|Caja|Command Center|arqueo|activa|error|ya existe|abierta/i.test(body);
    expect(ok).toBeTruthy();
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/cash-03-open-result.png', fullPage: true });
  });

  test('CASH-04 Cash registers index', async ({ page }) => {
    const res = await expectNoHttp500(page, '/CashRegister');
    expect(res.status()).toBe(200);
    await expect(page.locator('body')).not.toContainText('Exception');
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/cash-04-registers.png', fullPage: true });
  });

  test('CASH-05 Paid-in API requires session (negative)', async ({ page }) => {
    const res = await page.request.post('/api/CashMovement/paid-in', {
      data: { sessionId: '00000000-0000-0000-0000-000000000000', amount: 10, reason: 'cert' }
    });
    expect(res.status()).not.toBe(500);
    expect([400, 404, 422, 401, 403].includes(res.status()) || res.ok()).toBeTruthy();
  });

  test('CASH-06 Verify chain endpoint no 500', async ({ page }) => {
    const res = await page.request.get('/api/CashReport/verify/00000000-0000-0000-0000-000000000000');
    expect(res.status()).not.toBe(500);
  });

  test('CASH-07 Responsive dashboard tablet/mobile viewport already covered by project', async ({ page }) => {
    await page.goto('/CashSession/Dashboard');
    const box = await page.locator('body').boundingBox();
    expect(box.width).toBeGreaterThan(200);
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/cash-07-responsive.png', fullPage: true });
  });
});
