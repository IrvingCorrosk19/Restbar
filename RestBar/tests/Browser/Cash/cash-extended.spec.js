const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');
const { openCashIfNeeded } = require('../helpers/pos');

test.describe('Cash operations extended', () => {
  test('CASH-X01 module enabled dashboard', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/CashSession/Dashboard', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBe(200);
    const text = await page.locator('body').innerText();
    expect(/módulo deshabilitado|ModuleDisabled/i.test(text)).toBeFalsy();
    await expect(page.getByRole('heading', { name: /Caja/i }).first()).toBeVisible();
  });

  test('CASH-X02 open wizard and open or already open', async ({ page }) => {
    await loginAsAdmin(page);
    const result = await openCashIfNeeded(page);
    expect(result.opened || result.reason === 'module-disabled').toBeTruthy();
    if (result.reason === 'module-disabled') {
      test.fail(true, 'Cash module still disabled on target');
    }
  });

  test('CASH-X03 registers page', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/CashRegister', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('CASH-X04 double open is rejected or handled', async ({ page }) => {
    await loginAsAdmin(page);
    await openCashIfNeeded(page);
    await page.goto('/CashSession/OpenWizard');
    const opts = await page.locator('select[name="registerId"] option').count();
    if (opts === 0) {
      test.skip(true, 'no registers');
    }
    await page.locator('input[name="openingFloat"]').fill('50');
    await page.getByRole('button', { name: /Abrir sesión/i }).click();
    await page.waitForLoadState('domcontentloaded');
    expect(page.url()).not.toMatch(/error\/500/i);
    const text = await page.locator('body').innerText();
    // Controlled validation (TempData) or existing session detail — never raw crash
    expect(/Stack Trace:|at RestBar\.Controllers/i.test(text)).toBeFalsy();
    const handled =
      /already has an active session|sesión activa|Wizard|Detalle|Detail|alert-warning/i.test(text) ||
      /CashSession\/Detail/i.test(page.url());
    expect(handled).toBeTruthy();
  });

  test('CASH-X05 paid-in negative still not 500', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/api/CashMovement/paid-in', {
      data: { sessionId: '00000000-0000-0000-0000-000000000000', amount: 5, reason: 'cert-x' },
    });
    expect(res.status()).not.toBe(500);
  });
});
