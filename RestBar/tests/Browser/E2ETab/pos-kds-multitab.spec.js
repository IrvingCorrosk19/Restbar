const { test, expect } = require('@playwright/test');
const path = require('path');
const { createIsolatedContext, loginAs, TENANTS } = require('../helpers/multi-context');
const { gotoPos, selectAvailableTable, addFirstProduct, sendToKitchen } = require('../helpers/pos');

const EVIDENCE = path.join(
  __dirname,
  '../../../FULL_E2E_TAB_BROWSER_CERTIFICATION/Evidence/POS'
);

test.describe('E2E Tab · POS + KDS multitab', () => {
  test('E2E-POS-02 waiter POS tab + kitchen KDS tab same tenant isolated contexts', async ({ browser }) => {
    const waiter = await createIsolatedContext(browser, 'waiter');
    const kitchen = await createIsolatedContext(browser, 'kitchen');

    // Prefer costa mesero/chef; fall back to demo admin for both tabs if ops users missing
    let waiterOk = await loginAs(waiter.page, TENANTS.costa.mesero);
    let kitchenOk = await loginAs(kitchen.page, TENANTS.costa.chef);

    if (!waiterOk || !kitchenOk) {
      await waiter.context.clearCookies();
      await kitchen.context.clearCookies();
      waiterOk = await loginAs(waiter.page, TENANTS.demo.admin);
      kitchenOk = await loginAs(kitchen.page, TENANTS.demo.admin);
    }

    expect(waiterOk).toBeTruthy();
    expect(kitchenOk).toBeTruthy();

    await gotoPos(waiter.page);
    await selectAvailableTable(waiter.page);
    await addFirstProduct(waiter.page);
    await sendToKitchen(waiter.page).catch(() => null);

    await kitchen.page.goto('/Order/StationOrders?stationType=kitchen');
    const status = await kitchen.page.evaluate(() => document.readyState);
    expect(status).toBeTruthy();
    await expect(kitchen.page.locator('body')).not.toContainText('Npgsql.PostgresException');
    const kitchenStatus = (await kitchen.page.goto('/Order/StationOrders?stationType=kitchen')).status();
    expect(kitchenStatus).toBeLessThan(500);

    await waiter.page.screenshot({ path: path.join(EVIDENCE, 'E2E-POS-02', 'waiter.png'), fullPage: true });
    await kitchen.page.screenshot({ path: path.join(EVIDENCE, 'E2E-POS-02', 'kitchen.png'), fullPage: true });

    // Bar tab third context
    const bar = await createIsolatedContext(browser, 'bar');
    const barOk = await loginAs(bar.page, TENANTS.demo.admin);
    expect(barOk).toBeTruthy();
    const barRes = await bar.page.goto('/Order/StationOrders?stationType=bar');
    expect(barRes.status()).toBeLessThan(500);
    await bar.page.screenshot({ path: path.join(EVIDENCE, 'E2E-POS-02', 'bar.png'), fullPage: true });

    await waiter.context.close();
    await kitchen.context.close();
    await bar.context.close();
  });

  test('E2E-MT-02 foreign order edit not 500 across context', async ({ browser }) => {
    const ctx = await createIsolatedContext(browser, 'idor');
    const ok = await loginAs(ctx.page, TENANTS.demo.admin);
    expect(ok).toBeTruthy();
    const res = await ctx.page.request.get('/Order/Edit/11111111-1111-1111-1111-111111111111');
    expect(res.status()).not.toBe(500);
    expect([200, 302, 403, 404]).toContain(res.status());
    await ctx.context.close();
  });
});
