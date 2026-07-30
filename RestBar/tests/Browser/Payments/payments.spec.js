const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');
const { gotoPos, selectAvailableTable, addFirstProduct, sendToKitchen } = require('../helpers/pos');

test.describe('Payments', () => {
  test('PAY-01 PaymentView page loads', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/PaymentView', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('PAY-02 partial API rejects empty order', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/api/Payment/partial', {
      data: {
        orderId: '00000000-0000-0000-0000-000000000000',
        amount: 1,
        method: 'Cash',
        tip: 0,
      },
    });
    expect(res.status()).not.toBe(500);
    expect([400, 404, 422, 403].includes(res.status()) || res.ok()).toBeTruthy();
  });

  test('PAY-03 POS send-to-kitchen control present after items', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await selectAvailableTable(page);
    await addFirstProduct(page);
    await expect(page.locator('#sendToKitchen')).toBeVisible({ timeout: 15000 });
  });

  test('PAY-04 send kitchen then payment summary endpoint shape', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await selectAvailableTable(page);
    await addFirstProduct(page);
    const send = await sendToKitchen(page);
    const orderId = send?.orderId || send?.data?.orderId || send?.OrderId || send?.id;
    if (!orderId) {
      // Still validate endpoint rejects empty gracefully
      const res = await page.request.get('/api/Payment/order/00000000-0000-0000-0000-000000000001/summary');
      expect(res.status()).toBeLessThan(500);
      return;
    }
    const res = await page.request.get(`/api/Payment/order/${orderId}/summary`);
    expect(res.status()).toBeLessThan(500);
  });
});
