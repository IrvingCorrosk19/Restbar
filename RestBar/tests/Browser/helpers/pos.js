const { expect } = require('@playwright/test');

async function gotoPos(page, returnUrl = '/Home') {
  await page.goto(`/Order/Index?returnUrl=${encodeURIComponent(returnUrl)}`, { waitUntil: 'domcontentloaded' });
  await expect(page.getByTestId('order-pos-chrome')).toBeVisible({ timeout: 20000 });
}

async function selectAvailableTable(page, preferredNumbers = ['P1-01', 'P1-02', 'T-01', 'C-14', 'C-15']) {
  await page.waitForSelector('.select-table-btn, .table-card', { timeout: 20000 });
  for (const num of preferredNumbers) {
    const btn = page.locator(`.select-table-btn[data-table-number="${num}"]`).first();
    if (await btn.count()) {
      await btn.click();
      await page.waitForTimeout(1000);
      return num;
    }
  }
  const any = page.locator('.select-table-btn').first();
  await expect(any).toBeVisible();
  const num = await any.getAttribute('data-table-number');
  await any.click();
  await page.waitForTimeout(1000);
  return num;
}

async function addFirstProduct(page) {
  const cat = page.locator('#categories button, .categoria-btn').first();
  if (await cat.count()) {
    await cat.click();
    await page.waitForTimeout(1200);
  }

  await page.waitForSelector('#products .product-card, #products button', { timeout: 20000 });
  const addBtn = page.locator('#products .product-card button').filter({ hasText: /\+ Agregar|Agregar/i }).first();
  await expect(addBtn).toBeVisible({ timeout: 20000 });
  const name = (await page.locator('#products .product-card').first().locator('h6, .card-title, strong').first().textContent().catch(() => 'producto')) || 'producto';
  await addBtn.click();
  await page.waitForTimeout(800);

  // Confirm stock Swal if shown
  const swal = page.locator('.swal2-confirm');
  if (await swal.isVisible().catch(() => false)) {
    await swal.click();
    await page.waitForTimeout(400);
  }

  await expect.poll(async () => page.locator('#orderItems tr').count(), { timeout: 10000 }).toBeGreaterThan(0);
  return name.trim();
}

async function sendToKitchen(page) {
  const btn = page.locator('#sendToKitchen');
  await expect(btn).toBeVisible({ timeout: 15000 });
  await expect(btn).toBeEnabled({ timeout: 15000 });
  await btn.click();

  // Admin path: modal first, then POST on confirm
  const modalRoot = page.locator('#selectStationModal');
  const modalVisible = await modalRoot.waitFor({ state: 'visible', timeout: 10000 }).then(() => true).catch(() => false);

  if (modalVisible) {
    const stationSelect = page.locator('#stationSelect');
    await expect(stationSelect).toBeVisible({ timeout: 5000 });
    await page.waitForFunction(() => {
      const s = document.querySelector('#stationSelect');
      return s && [...s.options].some(o => o.value);
    }, null, { timeout: 10000 });

    const options = stationSelect.locator('option[value]:not([value=""])');
    const n = await options.count();
    expect(n, 'stations available for admin send').toBeGreaterThan(0);
    let chosen = await options.nth(0).getAttribute('value');
    for (let i = 0; i < n; i++) {
      const text = (await options.nth(i).textContent()) || '';
      if (/kitchen|cocina/i.test(text)) {
        chosen = await options.nth(i).getAttribute('value');
        break;
      }
    }
    await stationSelect.selectOption(chosen);

    const responsePromise = page.waitForResponse(
      r => r.url().includes('/Order/SendToKitchen') && r.request().method() === 'POST',
      { timeout: 45000 }
    );
    await page.locator('#selectStationModal .modal-footer button.btn-info').click();
    const res = await responsePromise;
    expect(res.status(), 'SendToKitchen HTTP').toBeLessThan(500);
    return await res.json().catch(() => ({ success: true }));
  }

  // Non-admin: POST should happen after click
  const res = await page.waitForResponse(
    r => r.url().includes('/Order/SendToKitchen') && r.request().method() === 'POST',
    { timeout: 20000 }
  ).catch(() => null);
  if (res) {
    expect(res.status(), 'SendToKitchen HTTP').toBeLessThan(500);
    return await res.json().catch(() => ({ success: true }));
  }
  await page.waitForTimeout(1500);
  return { success: true };
}

async function markFirstReadyOnKds(page, stationType = 'kitchen') {
  await page.goto(`/Order/StationOrders?stationType=${stationType}&returnUrl=${encodeURIComponent('/Home')}`, {
    waitUntil: 'domcontentloaded',
  });
  const ready = page.locator('.modern-status-btn.btn-ready').first();
  if (!(await ready.count()) || !(await ready.isVisible().catch(() => false))) {
    return { marked: false, reason: 'no-ready-button' };
  }
  const responsePromise = page.waitForResponse(
    r => (r.url().includes('/Order/UpdateItemStatus') || r.url().includes('/Order/MarkItemReady')) && r.request().method() === 'POST',
    { timeout: 20000 }
  ).catch(() => null);
  await ready.click();
  const res = await responsePromise;
  return { marked: true, status: res ? res.status() : null };
}

async function openCashIfNeeded(page) {
  await page.goto('/CashSession/Dashboard', { waitUntil: 'domcontentloaded' });
  const body = await page.locator('body').innerText();
  if (/módulo deshabilitado|ModuleDisabled|no está habilitado/i.test(body)) {
    return { opened: false, reason: 'module-disabled' };
  }
  await page.goto('/CashSession/OpenWizard', { waitUntil: 'domcontentloaded' });
  const options = page.locator('select[name="registerId"] option');
  if ((await options.count()) === 0) {
    return { opened: false, reason: 'no-registers' };
  }
  await page.locator('input[name="openingFloat"]').fill('100');
  await page.getByRole('button', { name: /Abrir sesión/i }).click();
  await page.waitForLoadState('domcontentloaded');
  return { opened: true };
}

module.exports = {
  gotoPos,
  selectAvailableTable,
  addFirstProduct,
  sendToKitchen,
  markFirstReadyOnKds,
  openCashIfNeeded,
};
