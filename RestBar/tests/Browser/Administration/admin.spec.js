const { test, expect } = require('@playwright/test');
const { loginAsAdmin, collectConsoleErrors, significantConsoleErrors, expectNoHttp500 } = require('../helpers/auth');

test.describe('Administration · plan ADM-*', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('ADM-01 Company Index', async ({ page }) => {
    const errors = await collectConsoleErrors(page);
    const res = await expectNoHttp500(page, '/Company');
    expect(res.status()).toBeLessThan(500);
    await expect(page.locator('body')).not.toContainText('Exception');
    await page.screenshot({ path: '../../FULL_BROWSER_CERTIFICATION/Evidence/Administration/ADM-01/company.png', fullPage: true });
    expect(significantConsoleErrors(errors).length).toBe(0);
  });

  test('ADM-02 Branch Index', async ({ page }) => {
    const res = await expectNoHttp500(page, '/Branch');
    expect(res.status()).toBeLessThan(500);
    await page.screenshot({ path: '../../FULL_BROWSER_CERTIFICATION/Evidence/Administration/ADM-02/branch.png', fullPage: true });
  });

  test('ADM-03 UserManagement', async ({ page }) => {
    const res = await expectNoHttp500(page, '/User/UserManagement');
    expect(res.status()).toBeLessThan(500);
  });

  test('ADM-04 SuperAdmin denied for admin (or loads if super)', async ({ page }) => {
    const res = await page.goto('/SuperAdmin', { waitUntil: 'domcontentloaded' });
    const status = res.status();
    const denied = page.url().includes('AccessDenied') || status === 403;
    const loaded = status === 200 && /Super|Empresa|Company/i.test(await page.locator('body').innerText());
    expect(denied || loaded).toBeTruthy();
  });

  test('ADM-05 Category Index', async ({ page }) => {
    await expectNoHttp500(page, '/Category');
  });

  test('ADM-06 AdvancedSettings Index', async ({ page }) => {
    const res = await expectNoHttp500(page, '/AdvancedSettings');
    expect(res.status()).toBeLessThan(500);
  });

  test('ADM-07 Audit Index', async ({ page }) => {
    const res = await expectNoHttp500(page, '/Audit');
    expect(res.status()).toBeLessThan(500);
  });

  test('ADM-08 Email Index graceful', async ({ page }) => {
    const res = await page.goto('/Email', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('COP-01 Copilot ModuleDisabled or page', async ({ page }) => {
    const res = await page.goto('/Copilot', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    const text = await page.locator('body').innerText();
    expect(/Copilot|deshabilit|disabled|Director/i.test(text)).toBeTruthy();
  });
});
