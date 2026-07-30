const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');
const { gotoPos, selectAvailableTable, addFirstProduct, sendToKitchen } = require('../helpers/pos');

test.describe('Negatives / integrity', () => {
  test('NEG-01 SendToKitchen empty items fails softly', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/Order/SendToKitchen', {
      data: { TableId: '00000000-0000-0000-0000-000000000001', OrderType: 'DineIn', Items: [] },
    });
    expect(res.status()).not.toBe(500);
  });

  test('NEG-02 foreign order payment summary', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/api/Payment/order/00000000-0000-0000-0000-000000000099/summary');
    expect(res.status()).not.toBe(500);
  });

  test('NEG-03 cancel item invalid ids', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/Order/UpdateItemStatus', {
      data: {
        itemId: '00000000-0000-0000-0000-000000000000',
        orderId: '00000000-0000-0000-0000-000000000000',
        status: 'Cancelled',
      },
    });
    expect(res.status()).not.toBe(500);
  });

  test('NEG-04 POS with product then home no crash', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await selectAvailableTable(page);
    await addFirstProduct(page);
    await page.getByTestId('order-nav-home').click();
    await page.waitForURL(/\/Home/i, { timeout: 20000 });
  });
});
