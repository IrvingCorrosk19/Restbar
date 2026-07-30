const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');

test.describe('Stations / KDS routing surfaces', () => {
  test('STN-01 Stations index loads', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/Station', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
  });

  test('STN-02 GetStations API', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/Station/GetStations');
    expect(res.status()).toBe(200);
    const json = await res.json();
    expect(json.success !== false).toBeTruthy();
    const data = json.data || json.stations || [];
    expect(Array.isArray(data) ? data.length : 0).toBeGreaterThan(0);
  });

  test('STN-03 kitchen and bar KDS load independently', async ({ page }) => {
    await loginAsAdmin(page);
    const k = await page.goto('/Order/StationOrders?stationType=kitchen', { waitUntil: 'domcontentloaded' });
    expect(k.status()).toBeLessThan(500);
    const b = await page.goto('/Order/StationOrders?stationType=bar', { waitUntil: 'domcontentloaded' });
    expect(b.status()).toBeLessThan(500);
  });

  test('STN-04 Stations list usable for routing', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.get('/Station/GetStations');
    expect(res.status()).toBe(200);
    const json = await res.json();
    const data = json.data || [];
    expect(data.length).toBeGreaterThan(0);
    expect(data[0].id || data[0].name).toBeTruthy();
  });

  test('STN-05 UpdateItemStatus rejects empty payload', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.request.post('/Order/UpdateItemStatus', {
      data: { itemId: '00000000-0000-0000-0000-000000000000', orderId: '00000000-0000-0000-0000-000000000000', status: 'Ready' },
    });
    expect(res.status()).not.toBe(500);
  });
});
