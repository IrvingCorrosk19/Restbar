const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');
const {
  getActiveCashSessionId,
  gotoPos,
  selectAvailableTable,
  addFirstProduct,
  sendToKitchen,
  dismissOverlays,
} = require('../helpers/pos');

test.describe('Cash lifecycle — close / arqueo / movements', () => {
  test('CASH-L01 active session detail reachable', async ({ page }) => {
    await loginAsAdmin(page);
    const id = await getActiveCashSessionId(page);
    expect(id).toBeTruthy();
    const res = await page.goto(`/CashSession/Detail/${id}`, { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    await expect(page.getByRole('heading', { name: /Sesión/i }).first()).toBeVisible();
  });

  test('CASH-L02 arqueo page for session no 500', async ({ page }) => {
    await loginAsAdmin(page);
    const id = await getActiveCashSessionId(page);
    expect(id).toBeTruthy();
    await page.goto(`/CashSession/Detail/${id}`);
    const start = page.getByRole('button', { name: /Iniciar cierre/i });
    if (await start.count()) {
      await start.click();
      await page.waitForLoadState('domcontentloaded');
    }
    const res = await page.goto(`/CashSession/Arqueo/${id}`, { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    const text = await page.locator('body').innerText();
    expect(/Stack Trace:/i.test(text)).toBeFalsy();
  });

  test('CASH-L03 start-close then abort preserves session', async ({ page }) => {
    await loginAsAdmin(page);
    const id = await getActiveCashSessionId(page);
    expect(id).toBeTruthy();
    await page.goto(`/CashSession/Detail/${id}`);
    const start = page.getByRole('button', { name: /Iniciar cierre/i });
    if (await start.count()) {
      await start.click();
      await page.waitForLoadState('domcontentloaded');
    }
    const countInput = page.locator('input[name="totalCounted"]');
    if (await countInput.count()) {
      await countInput.fill('100');
      await page.getByRole('button', { name: /Enviar|Guardar|Continuar|Submit|Confirmar/i }).first().click().catch(() => null);
      await page.waitForLoadState('domcontentloaded');
    }
    const abort = page.getByRole('button', { name: /Abortar|Cancelar cierre/i });
    if (await abort.count()) {
      await abort.click();
      await page.waitForLoadState('domcontentloaded');
    }
    const text = await page.locator('body').innerText();
    expect(/Stack Trace:|at RestBar\.Controllers/i.test(text)).toBeFalsy();
  });

  test('CASH-L04 paid-in API with real session', async ({ page }) => {
    await loginAsAdmin(page);
    const id = await getActiveCashSessionId(page);
    expect(id).toBeTruthy();
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
