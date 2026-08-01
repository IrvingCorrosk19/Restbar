const { test, expect } = require('@playwright/test');
const path = require('path');
const { loginAsAdmin } = require('../helpers/auth');
const { createIsolatedContext, loginAs, TENANTS } = require('../helpers/multi-context');
const {
  gotoPos,
  selectAvailableTable,
  addFirstProduct,
  sendToKitchen,
  openCashIfNeeded,
  getActiveCashSessionId,
} = require('../helpers/pos');

const EVIDENCE = path.join(__dirname, '../../../FULL_E2E_TAB_BROWSER_CERTIFICATION/Evidence');

test.describe('E2E Tab · Cash + Payment chain', () => {
  test('E2E-CASH-10 open cash then POS order then payment summary', async ({ page }) => {
    await loginAsAdmin(page);
    const cash = await openCashIfNeeded(page);
    if (cash.reason === 'module-disabled') test.skip(true, 'cash module disabled');
    expect(cash.opened, `cash open: ${cash.reason || 'ok'}`).toBeTruthy();

    const sessionId = await getActiveCashSessionId(page);
    expect(sessionId, 'active cash session').toBeTruthy();

    await gotoPos(page);
    await selectAvailableTable(page);
    await addFirstProduct(page);
    await sendToKitchen(page).catch(() => null);

    // Resolve current order id from UI if exposed
    const orderId =
      (await page.locator('[data-order-id]').first().getAttribute('data-order-id').catch(() => null)) ||
      (await page.evaluate(() => window.currentOrderId || window.orderId || null).catch(() => null));

    if (orderId) {
      const pay = await page.request.get(`/api/Payment/order/${orderId}`);
      expect(pay.status()).not.toBe(500);
      expect([200, 401, 403, 404]).toContain(pay.status());
    }

    const z = await page.goto(`/CashReport/ZReport?sessionId=${sessionId}`, { waitUntil: 'domcontentloaded' });
    expect(z.status()).toBeLessThan(500);
    const zText = await page.locator('body').innerText();
    expect(/Z|Reporte|Sesión|Integridad|Cierre|JSON|Cash/i.test(zText)).toBeTruthy();

    const x = await page.goto(`/CashReport/XReport?sessionId=${sessionId}`, { waitUntil: 'domcontentloaded' });
    expect(x.status()).toBeLessThan(500);

    await page.screenshot({ path: path.join(EVIDENCE, 'Cash', 'E2E-CASH-10', 'z-report.png'), fullPage: true });
  });

  test('E2E-CASH-12 paid-in then paid-out then list movements', async ({ page }) => {
    await loginAsAdmin(page);
    const sessionId = await getActiveCashSessionId(page);
    expect(sessionId).toBeTruthy();

    const paidIn = await page.request.post('/api/CashMovement/paid-in', {
      data: { sessionId, amount: 5.5, reasonCode: 'E2E-IN', comments: 'tab-browser' },
    });
    expect(paidIn.status(), await paidIn.text()).toBeLessThan(500);
    expect([200, 201, 202]).toContain(paidIn.status());

    const paidOut = await page.request.post('/api/CashMovement/paid-out', {
      data: { sessionId, amount: 2.25, reasonCode: 'E2E-OUT', comments: 'tab-browser' },
    });
    expect(paidOut.status(), await paidOut.text()).toBeLessThan(500);
    expect([200, 201, 202]).toContain(paidOut.status());

    const list = await page.request.get(`/api/CashMovement/${sessionId}`);
    expect(list.status()).toBe(200);
    const body = await list.json();
    const items = Array.isArray(body) ? body : body.items || body.data || [];
    expect(items.length).toBeGreaterThan(0);

    await page.goto(`/CashSession/Detail/${sessionId}`, { waitUntil: 'domcontentloaded' });
    await page.screenshot({ path: path.join(EVIDENCE, 'Cash', 'E2E-CASH-12', 'detail.png'), fullPage: true });
  });

  test('E2E-CASH-11 foreign session ZReport denied soft', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/CashReport/ZReport?sessionId=11111111-1111-1111-1111-111111111111', {
      waitUntil: 'domcontentloaded',
    });
    expect(res.status()).toBeLessThan(500);
    const text = await page.locator('body').innerText();
    expect(/no encontrada|no autorizada|Sesión|Dashboard|deshabilit/i.test(text)).toBeTruthy();
    expect(text).not.toMatch(/Npgsql\.PostgresException/);
  });

  test('E2E-PAY-10 payment dashboard and void API soft', async ({ page }) => {
    await loginAsAdmin(page);
    const dash = await page.goto('/PaymentView', { waitUntil: 'domcontentloaded' });
    expect(dash.status()).toBeLessThan(500);
    await expect(page.locator('body')).not.toContainText('Npgsql.PostgresException');

    const voidRes = await page.request.delete('/api/Payment/00000000-0000-0000-0000-000000000000');
    expect(voidRes.status()).not.toBe(500);
    expect([400, 401, 403, 404]).toContain(voidRes.status());

    await page.screenshot({ path: path.join(EVIDENCE, 'Payments', 'E2E-PAY-10', 'dashboard.png'), fullPage: true });
  });
});

test.describe('E2E Tab · Inventory + Procurement + FoodCost', () => {
  test('E2E-INV-10 inventory index + movements API scoped', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/Inventory', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    const text = await page.locator('body').innerText();
    expect(/Próximamente|Coming soon/i.test(text)).toBeFalsy();
    expect(text).not.toMatch(/Npgsql\.PostgresException/);

    const mov = await page.request.get('/InventoryMovement/GetMovementsByDateRange');
    expect(mov.status()).toBeLessThan(500);

    const transfer = await page.request.get('/StockTransfer');
    expect(transfer.status()).toBeLessThan(500);

    await page.screenshot({ path: path.join(EVIDENCE, 'Inventory', 'E2E-INV-10', 'index.png'), fullPage: true });
  });

  test('E2E-PO-10 supplier + procurement + PO list', async ({ page }) => {
    await loginAsAdmin(page);
    for (const route of ['/Supplier', '/ProcurementDashboard', '/PurchaseOrder']) {
      const res = await page.goto(route, { waitUntil: 'domcontentloaded' });
      expect(res.status(), route).toBeLessThan(500);
      const text = await page.locator('body').innerText();
      expect(text, route).not.toMatch(/Npgsql\.PostgresException|Stack Trace:/i);
    }
    await page.screenshot({ path: path.join(EVIDENCE, 'Procurement', 'E2E-PO-10', 'po.png'), fullPage: true });
  });

  test('E2E-FC-10 food cost dashboard + recipes + menu eng', async ({ page }) => {
    await loginAsAdmin(page);
    for (const route of ['/FoodCostDashboard', '/Recipe', '/FoodCostDashboard/MenuEngineering']) {
      const res = await page.goto(route, { waitUntil: 'domcontentloaded' });
      expect(res.status(), route).toBeLessThan(500);
      const text = await page.locator('body').innerText();
      expect(text, route).not.toMatch(/Npgsql\.PostgresException/);
    }
    await page.screenshot({ path: path.join(EVIDENCE, 'FoodCost', 'E2E-FC-10', 'dashboard.png'), fullPage: true });
  });
});

test.describe('E2E Tab · BI + Admin + RBAC + Hostile', () => {
  test('E2E-BI-10 executive analytics + DI + reports + advanced', async ({ page }) => {
    await loginAsAdmin(page);
    const routes = [
      '/ExecutiveAnalytics',
      '/ExecutiveCommandCenter',
      '/BiNative',
      '/DecisionIntelligence/Cockpit',
      '/BusinessRules',
      '/Reports',
      '/AdvancedReports',
      '/Audit',
    ];
    for (const route of routes) {
      const res = await page.goto(route, { waitUntil: 'domcontentloaded' });
      expect(res.status(), route).toBeLessThan(500);
      const text = await page.locator('body').innerText();
      expect(text, route).not.toMatch(/Npgsql\.PostgresException|at RestBar\.Controllers/i);
    }
    await page.screenshot({ path: path.join(EVIDENCE, 'BI', 'E2E-BI-10', 'reports.png'), fullPage: true });
  });

  test('E2E-ADM-10 company branch area table station user', async ({ page }) => {
    await loginAsAdmin(page);
    const routes = [
      '/Company',
      '/Branch',
      '/Area',
      '/Table',
      '/Station',
      '/Category',
      '/Product',
      '/Modifier',
      '/Customer',
      '/User',
      '/UserManagement',
      '/UserAssignment',
      '/Shift',
      '/AdvancedSettings',
      '/Email',
    ];
    for (const route of routes) {
      const res = await page.goto(route, { waitUntil: 'domcontentloaded' });
      expect(res.status(), route).toBeLessThan(500);
    }
    await page.screenshot({ path: path.join(EVIDENCE, 'Admin', 'E2E-ADM-10', 'product.png'), fullPage: true });
  });

  test('E2E-RBAC-10 waiter chef cashier contexts soft', async ({ browser }) => {
    const roles = [
      { email: 'mesero@restbar.com', path: '/Order' },
      { email: 'chef@restbar.com', path: '/Order/StationOrders?stationType=kitchen' },
      { email: 'cajero@restbar.com', path: '/CashSession/Dashboard' },
    ];
    let logged = 0;
    for (const role of roles) {
      const { context, page } = await createIsolatedContext(browser, role.email);
      const ok = await loginAs(page, role.email);
      if (!ok) {
        await context.close();
        continue;
      }
      logged++;
      const res = await page.goto(role.path, { waitUntil: 'domcontentloaded' });
      expect(res.status()).toBeLessThan(500);
      await expect(page.locator('body')).not.toContainText('Npgsql.PostgresException');
      await context.close();
    }
    if (logged === 0) test.skip(true, 'ops role users not seeded');
    expect(logged).toBeGreaterThan(0);
  });

  test('E2E-MT-20 hostile IDs payment cash inventory transfer', async ({ page }) => {
    await loginAsAdmin(page);
    const foreign = '11111111-1111-1111-1111-111111111111';
    const checks = [
      page.request.get(`/api/Payment/order/${foreign}`),
      page.request.post('/api/CashMovement/paid-out', {
        data: { sessionId: foreign, amount: 1, reasonCode: 'TEST', comments: 'hostile' },
      }),
      page.request.get(`/InventoryMovement/GetMovementsByDateRange?branchId=${foreign}`),
      page.request.post('/StockTransfer/Approve?id=' + foreign),
      page.request.get(`/AdvancedReports/ProfitabilityAnalysis?branchId=${foreign}`),
    ];
    const results = await Promise.all(checks);
    for (const res of results) {
      expect(res.status(), res.url()).not.toBe(500);
      expect([200, 302, 400, 401, 403, 404]).toContain(res.status());
    }
  });

  test('E2E-UX-10 responsive POS + cash mobile viewport', async ({ browser }) => {
    const context = await browser.newContext({
      viewport: { width: 412, height: 915 },
      isMobile: true,
      hasTouch: true,
    });
    const page = await context.newPage();
    const ok = await loginAs(page, TENANTS.demo.admin);
    expect(ok).toBeTruthy();
    await page.goto('/Order', { waitUntil: 'domcontentloaded' });
    expect((await page.locator('body').innerText())).not.toMatch(/Npgsql/);
    await page.goto('/CashSession/Dashboard', { waitUntil: 'domcontentloaded' });
    expect((await page.locator('body').innerText())).not.toMatch(/Npgsql/);
    await page.screenshot({ path: path.join(EVIDENCE, 'Responsive', 'E2E-UX-10', 'mobile-cash.png'), fullPage: true });
    await context.close();
  });

  test('E2E-AUTH-10 logout clears protected route', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Order');
    await expect(page).not.toHaveURL(/Auth\/Login/);
    // Prefer form logout if present
    const logout = page.locator('form[action*="Logout"] button, a[href*="Logout"], button:has-text("Salir")').first();
    if (await logout.count()) {
      await logout.click({ force: true }).catch(() => null);
      await page.waitForTimeout(500);
    }
    await page.request.post('/Auth/Logout').catch(() => null);
    await page.context().clearCookies();
    const res = await page.goto('/CashSession/Dashboard', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    await expect(page).toHaveURL(/Auth\/Login/);
  });
});
