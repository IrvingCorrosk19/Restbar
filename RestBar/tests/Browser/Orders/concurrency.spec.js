const { test, expect } = require('@playwright/test');
const { loginAsAdmin, expectNoHttp500 } = require('../helpers/auth');
const { gotoPos, selectAvailableTable, addFirstProduct, sendToKitchen } = require('../helpers/pos');

test.describe('Concurrency · waiter + kitchen contexts · POS-CONC-01', () => {
  test('POS-CONC-01 Waiter and kitchen separate contexts', async ({ browser }) => {
    test.setTimeout(120_000);
    const waiterCtx = await browser.newContext();
    const kitchenCtx = await browser.newContext();
    const waiter = await waiterCtx.newPage();
    const kitchen = await kitchenCtx.newPage();

    await loginAsAdmin(waiter);
    await loginAsAdmin(kitchen);

    const kRes = await kitchen.goto('/Order/StationOrders?stationType=kitchen', { waitUntil: 'domcontentloaded', timeout: 60000 });
    expect(kRes.status()).toBeLessThan(500);

    await gotoPos(waiter);
    const tableOk = await selectAvailableTable(waiter);
    if (tableOk) {
      await addFirstProduct(waiter);
      await sendToKitchen(waiter).catch(() => {});
    }

    await kitchen.reload({ waitUntil: 'domcontentloaded' });
    await expect(kitchen.locator('body')).not.toContainText('Exception');

    await waiterCtx.close();
    await kitchenCtx.close();
  });
});
