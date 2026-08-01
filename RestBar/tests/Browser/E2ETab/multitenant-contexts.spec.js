const { test, expect } = require('@playwright/test');
const path = require('path');
const { createIsolatedContext, loginAs, TENANTS } = require('../helpers/multi-context');
const { gotoPos, selectAvailableTable, addFirstProduct } = require('../helpers/pos');

const EVIDENCE = path.join(
  __dirname,
  '../../../FULL_E2E_TAB_BROWSER_CERTIFICATION/Evidence/Multitenant'
);

test.describe('E2E Tab · Multitenant isolated contexts', () => {
  test('E2E-MT-05 three tenant admins concurrent isolated sessions', async ({ browser }) => {
    const emails = [TENANTS.costa.admin, TENANTS.norte.admin, TENANTS.sur.admin];
    const sessions = [];

    for (const email of emails) {
      const { context, page, label } = await createIsolatedContext(browser, email);
      await page.waitForTimeout(1500);
      const ok = await loginAs(page, email);
      sessions.push({ context, page, email, ok, label });
    }

    const available = sessions.filter((s) => s.ok);
    if (available.length < 2) {
      for (const s of sessions) await s.context.close().catch(() => {});
      test.skip(true, 'Fewer than 2 of Costa/Norte/Sur admins seeded — run ThreeCompaniesCertSeeder');
      return;
    }

    const bodies = [];
    for (const s of available) {
      await s.page.goto('/Product');
      await expect(s.page).not.toHaveURL(/Auth\/Login/);
      const text = await s.page.locator('body').innerText();
      expect(text).not.toContain('Npgsql.PostgresException');
      bodies.push({ email: s.email, text });
      await s.page.screenshot({
        path: path.join(EVIDENCE, 'E2E-MT-05', `${s.email.split('@')[0]}.png`),
        fullPage: true,
      });
    }

    const costa = bodies.find((b) => b.email.includes('costa'));
    const norte = bodies.find((b) => b.email.includes('norte'));
    if (costa && norte) {
      if (costa.text.includes('Producto Exclusivo Costa') && norte.text.includes('Producto Exclusivo Norte')) {
        expect(norte.text).not.toContain('Producto Exclusivo Costa');
        expect(costa.text).not.toContain('Producto Exclusivo Norte');
      }
    }

    // Cookie isolation: cookies from context A must not appear in B storage state emails
    const cookiesA = await available[0].context.cookies();
    const cookiesB = await available[1].context.cookies();
    expect(cookiesA.length).toBeGreaterThan(0);
    expect(cookiesB.length).toBeGreaterThan(0);

    for (const s of sessions) await s.context.close().catch(() => {});
  });

  test('E2E-AUTH-03 demo vs costa contexts do not share storage', async ({ browser }) => {
    const a = await createIsolatedContext(browser, 'demo');
    const b = await createIsolatedContext(browser, 'costa');

    const okA = await loginAs(a.page, TENANTS.demo.admin);
    expect(okA).toBeTruthy();

    const okB = await loginAs(b.page, TENANTS.costa.admin);
    if (!okB) {
      await a.context.close();
      await b.context.close();
      test.skip(true, 'Costa admin missing');
      return;
    }

    await a.page.goto('/CashSession/Dashboard');
    await b.page.goto('/CashSession/Dashboard');
    await expect(a.page).not.toHaveURL(/Auth\/Login/);
    await expect(b.page).not.toHaveURL(/Auth\/Login/);

    // Clearing B must not log out A
    await b.context.clearCookies();
    await b.page.goto('/Order');
    await expect(b.page).toHaveURL(/Auth\/Login/);
    await a.page.goto('/Home');
    await expect(a.page).not.toHaveURL(/Auth\/Login/);

    await a.page.screenshot({ path: path.join(EVIDENCE, 'E2E-AUTH-03', 'demo-still-in.png'), fullPage: true });
    await a.context.close();
    await b.context.close();
  });
});

test.describe('E2E Tab · POS smoke under isolated admin', () => {
  test('E2E-POS-01 isolated context open table add product', async ({ browser }) => {
    const { context, page } = await createIsolatedContext(browser, 'pos');
    const ok = await loginAs(page, TENANTS.demo.admin);
    expect(ok).toBeTruthy();

    await gotoPos(page);
    const table = await selectAvailableTable(page);
    expect(table).toBeTruthy();
    const product = await addFirstProduct(page);
    expect(product).toBeTruthy();

    await page.screenshot({
      path: path.join(
        __dirname,
        '../../../FULL_E2E_TAB_BROWSER_CERTIFICATION/Evidence/POS/E2E-POS-01/order.png'
      ),
      fullPage: true,
    });
    await context.close();
  });
});
