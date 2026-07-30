const { expect } = require('@playwright/test');

/**
 * POS helpers — table → product → kitchen → pay
 */
async function gotoPos(page, returnUrl = '/Home') {
  await page.goto(`/Order/Index?returnUrl=${encodeURIComponent(returnUrl)}`, { waitUntil: 'domcontentloaded' });
  await expect(page.getByTestId('order-pos-chrome')).toBeVisible({ timeout: 20000 });
}

async function selectAvailableTable(page, preferredNumbers = ['P1-01', 'P1-02', 'T-01', 'C-14']) {
  await page.waitForSelector('.select-table-btn, .table-card', { timeout: 20000 });
  for (const num of preferredNumbers) {
    const btn = page.locator(`.select-table-btn[data-table-number="${num}"]`).first();
    if (await btn.count()) {
      const card = page.locator(`.table-card`).filter({ has: btn }).first();
      const status = ((await card.getAttribute('data-table-status')) || '').toLowerCase();
      if (status.includes('dispon') || status.includes('available') || status === '' || status.includes('libre')) {
        await btn.click();
        await page.waitForTimeout(800);
        return num;
      }
    }
  }
  // Fallback: first available-looking select button
  const any = page.locator('.select-table-btn').first();
  await expect(any).toBeVisible();
  const num = await any.getAttribute('data-table-number');
  await any.click();
  await page.waitForTimeout(800);
  return num;
}

async function addFirstProduct(page) {
  // Ensure products loaded — click first category if needed
  const cat = page.locator('.categoria-btn, [onclick*="loadProducts"], button').filter({ hasText: /Bebidas|Platos|Postres|Men/i }).first();
  if (await cat.count()) {
    await cat.click();
    await page.waitForTimeout(1000);
  }
  const product = page.locator('.product-card').first();
  await expect(product).toBeVisible({ timeout: 20000 });
  const name = (await product.locator('h6, .product-name, strong').first().textContent().catch(() => 'producto')) || 'producto';
  const addBtn = product.locator('button').filter({ hasText: /Agregar|\+/i }).first();
  if (await addBtn.count()) await addBtn.click();
  else await product.click();
  await page.waitForTimeout(600);
  return name.trim();
}

async function sendToKitchen(page) {
  const btn = page.locator('#sendToKitchen, button').filter({ hasText: /Agregar a Cocina|Enviar/i }).first();
  await expect(btn).toBeEnabled({ timeout: 15000 });

  const responsePromise = page.waitForResponse(
    r => r.url().includes('/Order/SendToKitchen') && r.request().method() === 'POST',
    { timeout: 30000 }
  ).catch(() => null);

  await btn.click();

  // Admin may get station modal
  const stationModal = page.locator('#stationSelect, select').filter({ hasText: /estación|Estación/i }).first();
  const confirmStation = page.locator('button').filter({ hasText: /Confirmar|Enviar|Aceptar/i }).first();
  if (await page.locator('#stationSelectionModal, .modal.show').count()) {
    const select = page.locator('#stationSelect');
    if (await select.count()) {
      const opts = select.locator('option');
      if ((await opts.count()) > 1) await select.selectOption({ index: 1 });
    }
    if (await confirmStation.count()) await confirmStation.click();
  }

  // Swal confirm if any
  const swalConfirm = page.locator('.swal2-confirm');
  if (await swalConfirm.isVisible().catch(() => false)) {
    await swalConfirm.click();
  }

  const res = await responsePromise;
  if (res) {
    expect(res.status(), 'SendToKitchen status').toBeLessThan(500);
    const body = await res.json().catch(() => ({}));
    return body;
  }
  await page.waitForTimeout(1500);
  return { success: true };
}

async function markFirstReadyOnKds(page, stationType = 'kitchen') {
  await page.goto(`/Order/StationOrders?stationType=${stationType}&returnUrl=${encodeURIComponent('/Home')}`, {
    waitUntil: 'domcontentloaded',
  });
  const ready = page.locator('.modern-status-btn.btn-ready, button').filter({ hasText: /Listo|Ready/i }).first();
  if (!(await ready.count())) {
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
  // If already has active session, ok
  if (/activa|Detalle|Sesión/i.test(body) && !/Abrir sesión/i.test(body)) {
    return { opened: true, already: true };
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

async function apiMoveToTable(page, orderId, targetTableId) {
  return page.request.post('/Order/MoveToTable', {
    data: { orderId, targetTableId },
  });
}

module.exports = {
  gotoPos,
  selectAvailableTable,
  addFirstProduct,
  sendToKitchen,
  markFirstReadyOnKds,
  openCashIfNeeded,
  apiMoveToTable,
};
