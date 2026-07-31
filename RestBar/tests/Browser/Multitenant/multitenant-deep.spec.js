const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');

/**
 * RB-1001 Multitenant isolation — deep IDOR / cross-company soft suite.
 * Requires ThreeCompaniesCertSeeder (admin@costa|norte|sur.restbar.com / 123456) when available.
 */

async function tryLogin(page, email, password = '123456') {
  await page.goto('/Auth/Login');
  await page.locator('input[name="email"]').fill(email);
  await page.locator('input[name="password"]').fill(password);
  await page.locator('button.btn-login').click();
  await page.waitForTimeout(2000);
  return !page.url().includes('/Auth/Login');
}

test.describe('RB-1001 Multitenant deep', () => {
  test('MT-D01 default admin session has no PostgresException', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Customer');
    await expect(page.locator('body')).not.toContainText('Npgsql.PostgresException');
    await expect(page).not.toHaveURL(/Auth\/Login/);
  });

  test('MT-D02 order edit foreign guid is not 500', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/Order/Edit/11111111-1111-1111-1111-111111111111');
    expect(res.status()).not.toBe(500);
    expect([200, 302, 404]).toContain(res.status());
  });

  test('MT-D03 customer index scoped (no exception)', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/Customer');
    expect(res.status()).toBeLessThan(500);
  });

  test('MT-D04 cross-company login costa vs norte product exclusivity', async ({ page }) => {
    const costaOk = await tryLogin(page, 'admin@costa.restbar.com');
    if (!costaOk) {
      test.skip(true, 'ThreeCompanies seed not present');
      return;
    }
    await page.goto('/Product');
    const costaBody = await page.locator('body').innerText();
    await page.goto('/Auth/Logout').catch(() => {});
    await page.context().clearCookies();

    const norteOk = await tryLogin(page, 'admin@norte.restbar.com');
    if (!norteOk) {
      test.skip(true, 'norte admin missing');
      return;
    }
    await page.goto('/Product');
    const norteBody = await page.locator('body').innerText();

    // Exclusive products should not both appear on both tenants when seeder ran
    if (costaBody.includes('Producto Exclusivo Costa') && norteBody.includes('Producto Exclusivo Norte')) {
      expect(norteBody).not.toContain('Producto Exclusivo Costa');
      expect(costaBody).not.toContain('Producto Exclusivo Norte');
    }
  });

  test('MT-D05 API decision-intelligence forbids without company claim soft', async ({ page }) => {
    await page.context().clearCookies();
    // Cookie auth often 302→login (200 HTML) when redirects are followed.
    const res = await page.request.get('/api/decision-intelligence/cockpit', { maxRedirects: 0 });
    const status = res.status();
    if ([401, 302, 403, 404].includes(status)) {
      expect([401, 302, 403, 404]).toContain(status);
      return;
    }
    expect(status).toBe(200);
    const text = await res.text();
    expect(text.toLowerCase()).toMatch(/login|iniciar sesión|access denied|unauthorized|forbid/);
    expect(text).not.toMatch(/"kpis"\s*:/i);
  });
});
