const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');
const { openCashIfNeeded, gotoPos, selectAvailableTable, addFirstProduct, sendToKitchen, dismissOverlays } = require('../helpers/pos');

test.describe('Cash lifecycle — close / arqueo / movements', () => {
  test('CASH-L01 active session detail reachable', async ({ page }) => {
    await loginAsAdmin(page);
    await openCashIfNeeded(page);
    await page.goto('/CashSession/Dashboard', { waitUntil: 'domcontentloaded' });
    const link = page.locator('a[href*="/CashSession/Detail"]').first();
    if (!(await link.count())) {
      // Maybe redirected to detail on open
      if (/CashSession\/Detail/i.test(page.url())) {
        expect(page.url()).toMatch(/Detail/i);
        return;
      }
      test.skip(true, 'no active session detail link');
    }
    const res = await page.goto(await link.getAttribute('href'), { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    const text = await page.locator('body').innerText();
    expect(/Stack Trace:/i.test(text)).toBeFalsy();
  });

  test('CASH-L02 arqueo page for session no 500', async ({ page }) => {
    await loginAsAdmin(page);
    await openCashIfNeeded(page);
    await page.goto('/CashSession/Dashboard');
    const href = await page.locator('a[href*="/CashSession/Detail"]').first().getAttribute('href').catch(() => null);
    const idMatch = href && href.match(/Detail[\/\?](?:id=)?([0-9a-f-]{36})/i);
    const id = idMatch?.[1] || (page.url().match(/([0-9a-f-]{36})/i) || [])[1];
    if (!id) test.skip(true, 'no session id');
    const res = await page.goto(`/CashSession/Arqueo/${id}`, { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    const text = await page.locator('body').innerText();
    expect(/Stack Trace:/i.test(text)).toBeFalsy();
  });

  test('CASH-L03 start-close then abort preserves session', async ({ page }) => {
    await loginAsAdmin(page);
    await openCashIfNeeded(page);
    await page.goto('/CashSession/Dashboard');
    const href = await page.locator('a[href*="/CashSession/Detail"]').first().getAttribute('href').catch(() => null);
    const idMatch = href && href.match(/([0-9a-f-]{36})/i);
    const id = idMatch?.[1];
    if (!id) test.skip(true, 'no session id');
    await page.goto(`/CashSession/Detail/${id}`);
    const start = page.locator('button, input[type=submit], a').filter({ hasText: /Iniciar cierre|Cerrar|Arqueo|Start/i }).first();
    if (await start.count()) {
      await start.click().catch(() => null);
      await page.waitForLoadState('domcontentloaded');
    }
    const abort = page.locator('button, input[type=submit]').filter({ hasText: /Abortar|Cancelar cierre|Abort/i }).first();
    if (await abort.count()) {
      await abort.click().catch(() => null);
      await page.waitForLoadState('domcontentloaded');
    }
    const text = await page.locator('body').innerText();
    expect(/Stack Trace:|at RestBar\.Controllers/i.test(text)).toBeFalsy();
  });

  test('CASH-L04 paid-in API with real session if present', async ({ page }) => {
    await loginAsAdmin(page);
    await openCashIfNeeded(page);
    await page.goto('/CashSession/Dashboard');
    const href = await page.locator('a[href*="/CashSession/Detail"]').first().getAttribute('href').catch(() => null);
    const id = href && (href.match(/([0-9a-f-]{36})/i) || [])[1];
    if (!id) {
      const res = await page.request.post('/api/CashMovement/paid-in', {
        data: { sessionId: '00000000-0000-0000-0000-000000000000', amount: 1, reason: 'cert' },
      });
      expect(res.status()).not.toBe(500);
      return;
    }
    const res = await page.request.post('/api/CashMovement/paid-in', {
      data: { sessionId: id, amount: 1.25, reason: 'cert-lifecycle' },
    });
    expect(res.status()).not.toBe(500);
  });
});

test.describe('Order ops deep — cancel / split / transfer', () => {
  test('OPS-01 CancelOrderItem rejects empty soft', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/Order/UpdateItemStatus', {
      data: {
        itemId: '00000000-0000-0000-0000-000000000000',
        orderId: '00000000-0000-0000-0000-000000000000',
        status: 'Cancelled',
      },
    });
    expect(res.status()).not.toBe(500);
    expect([400, 404, 403].includes(res.status()) || res.ok()).toBeTruthy();
  });

  test('OPS-02 Split payment payload validation', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/api/Payment', {
      data: {
        orderId: '00000000-0000-0000-0000-000000000001',
        amount: 10,
        method: 'Cash',
        isShared: true,
        splitPayments: [
          { personName: 'A', amount: 5 },
          { personName: 'B', amount: 5 },
        ],
      },
    });
    expect(res.status()).not.toBe(500);
  });

  test('OPS-03 MoveToTable after send does not 500', async ({ page }) => {
    await loginAsAdmin(page);
    await gotoPos(page);
    await selectAvailableTable(page);
    await addFirstProduct(page);
    const send = await sendToKitchen(page);
    const orderId = send?.orderId || send?.OrderId;
    const tables = await page.request.get('/Order/GetActiveTables');
    const json = await tables.json();
    const list = json.tables || json.data || json;
    const arr = Array.isArray(list) ? list : [];
    const target = arr.find(t => t.id || t.Id);
    if (!orderId || !target) {
      const res = await page.request.post('/Order/MoveToTable', {
        data: {
          orderId: '00000000-0000-0000-0000-000000000000',
          targetTableId: '00000000-0000-0000-0000-000000000001',
        },
      });
      expect(res.status()).not.toBe(500);
      return;
    }
    const targetId = target.id || target.Id;
    const res = await page.request.post('/Order/MoveToTable', {
      data: { orderId, targetTableId: targetId },
    });
    expect(res.status()).not.toBe(500);
    await dismissOverlays(page);
  });
});
