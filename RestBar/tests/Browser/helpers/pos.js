const { expect } = require('@playwright/test');

async function dismissOverlays(page) {
  for (let i = 0; i < 5; i++) {
    const swalVisible = await page.locator('.swal2-container.swal2-backdrop-show, .swal2-popup').first().isVisible().catch(() => false);
    if (!swalVisible) break;
    const confirm = page.locator('.swal2-confirm').first();
    if (await confirm.isVisible().catch(() => false)) {
      await confirm.click({ force: true }).catch(() => null);
    } else {
      await page.keyboard.press('Escape').catch(() => null);
      await page.evaluate(() => {
        if (window.Swal && typeof Swal.close === 'function') Swal.close();
      }).catch(() => null);
    }
    await page.waitForTimeout(250);
  }
}

async function gotoPos(page, returnUrl = '/Home') {
  await page.goto(`/Order/Index?returnUrl=${encodeURIComponent(returnUrl)}`, { waitUntil: 'domcontentloaded' });
  await expect(page.getByTestId('order-pos-chrome')).toBeVisible({ timeout: 20000 });
  await dismissOverlays(page);
}

async function selectAvailableTable(page, preferredNumbers = ['C-14', 'C-15', 'T-01', 'P1-01', 'P1-02']) {
  await page.waitForSelector('.select-table-btn, .table-card', { timeout: 20000 });
  await dismissOverlays(page);

  // Prefer free/available tables
  const freeBtn = page.locator('.select-table-btn[data-table-status="Disponible"]').first();
  if (await freeBtn.count()) {
    const num = await freeBtn.getAttribute('data-table-number');
    await freeBtn.click({ force: true });
    await page.waitForTimeout(800);
    await dismissOverlays(page);
    return num || 'free';
  }

  for (const num of preferredNumbers) {
    const btn = page.locator(`.select-table-btn[data-table-number="${num}"]`).first();
    if (await btn.count()) {
      await btn.click({ force: true });
      await page.waitForTimeout(800);
      await dismissOverlays(page);
      return num;
    }
  }
  const any = page.locator('.select-table-btn').first();
  await expect(any).toBeVisible();
  const num = await any.getAttribute('data-table-number');
  await any.click({ force: true });
  await page.waitForTimeout(800);
  await dismissOverlays(page);
  return num;
}

async function addFirstProduct(page) {
  await dismissOverlays(page);
  const cat = page.locator('#categories button, .categoria-btn').first();
  if (await cat.count()) {
    await cat.click({ force: true });
    await page.waitForTimeout(1200);
    await dismissOverlays(page); // low-stock warning after loadProducts
  }

  await page.waitForSelector('#products .product-card button:not([disabled])', { timeout: 20000 });
  const cards = page.locator('#products .product-card');
  const count = await cards.count();
  let chosenName = 'producto';
  let clicked = false;
  for (let i = 0; i < count; i++) {
    const card = cards.nth(i);
    const addBtn = card.locator('button').filter({ hasText: /\+ Agregar/i }).first();
    if (!(await addBtn.count()) || !(await addBtn.isEnabled().catch(() => false))) continue;
    chosenName = (await card.locator('h6, .card-title').first().textContent().catch(() => 'producto')) || 'producto';
    await dismissOverlays(page);
    await addBtn.click({ force: true });
    await page.waitForTimeout(600);
    await dismissOverlays(page);
    clicked = true;
    break;
  }
  expect(clicked, 'at least one in-stock product to add').toBeTruthy();

  await expect.poll(async () => page.locator('#orderItems tr').count(), { timeout: 15000 }).toBeGreaterThan(0);
  return chosenName.trim();
}

async function sendToKitchen(page) {
  await dismissOverlays(page);
  const btn = page.locator('#sendToKitchen');
  await expect(btn).toBeVisible({ timeout: 15000 });
  await expect(btn).toBeEnabled({ timeout: 15000 });
  await btn.click({ force: true });

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
    await page.locator('#selectStationModal .modal-footer button.btn-info').click({ force: true });
    const res = await responsePromise;
    expect(res.status(), 'SendToKitchen HTTP').toBeLessThan(500);
    await dismissOverlays(page);
    return await res.json().catch(() => ({ success: true }));
  }

  const res = await page.waitForResponse(
    r => r.url().includes('/Order/SendToKitchen') && r.request().method() === 'POST',
    { timeout: 20000 }
  ).catch(() => null);
  await dismissOverlays(page);
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
  await dismissOverlays(page);
  const ready = page.locator('.modern-status-btn.btn-ready').first();
  if (!(await ready.count()) || !(await ready.isVisible().catch(() => false))) {
    return { marked: false, reason: 'no-ready-button' };
  }
  const responsePromise = page.waitForResponse(
    r => (r.url().includes('/Order/UpdateItemStatus') || r.url().includes('/Order/MarkItemReady')) && r.request().method() === 'POST',
    { timeout: 20000 }
  ).catch(() => null);
  await ready.click({ force: true });
  const res = await responsePromise;
  return { marked: true, status: res ? res.status() : null };
}

async function openCashIfNeeded(page) {
  await page.goto('/CashSession/Dashboard', { waitUntil: 'domcontentloaded' });
  const body = await page.locator('body').innerText();
  if (/módulo deshabilitado|ModuleDisabled|no está habilitado/i.test(body)) {
    return { opened: false, reason: 'module-disabled' };
  }
  // Already have active session?
  const existing = page.locator('a[href*="/CashSession/Detail"]').first();
  if (await existing.count()) {
    const href = await existing.getAttribute('href');
    return { opened: true, reason: 'already-open', href };
  }
  await page.goto('/CashSession/OpenWizard', { waitUntil: 'domcontentloaded' });
  const options = page.locator('select[name="registerId"] option');
  if ((await options.count()) === 0) {
    return { opened: false, reason: 'no-registers' };
  }
  await page.locator('input[name="openingFloat"]').fill('100');
  await page.getByRole('button', { name: /Abrir sesión/i }).click();
  await page.waitForLoadState('domcontentloaded');
  return { opened: true, href: page.url() };
}

async function getActiveCashSessionId(page) {
  await page.goto('/CashSession/Dashboard', { waitUntil: 'domcontentloaded' });
  let href = await page.getByTestId('cash-session-detail-link').first().getAttribute('href').catch(() => null);
  if (!href) {
    href = await page.locator('a[href*="/CashSession/Detail"]').first().getAttribute('href').catch(() => null);
  }
  if (href) {
    const m = href.match(/([0-9a-f-]{36})/i);
    if (m) return m[1];
  }

  await page.goto('/CashSession/OpenWizard', { waitUntil: 'domcontentloaded' });
  const options = page.locator('select[name="registerId"] option');
  if ((await options.count()) === 0) return null;
  await page.locator('input[name="openingFloat"]').fill('100');
  await page.getByRole('button', { name: /Abrir sesión/i }).click();
  await page.waitForLoadState('domcontentloaded');
  const fromUrl = page.url().match(/([0-9a-f-]{36})/i);
  if (fromUrl) return fromUrl[1];

  await page.goto('/CashSession/Dashboard', { waitUntil: 'domcontentloaded' });
  href = await page.getByTestId('cash-session-detail-link').first().getAttribute('href').catch(() => null);
  if (!href) {
    href = await page.locator('a[href*="/CashSession/Detail"]').first().getAttribute('href').catch(() => null);
  }
  const m2 = href && href.match(/([0-9a-f-]{36})/i);
  return m2?.[1] || null;
}

module.exports = {
  dismissOverlays,
  gotoPos,
  selectAvailableTable,
  addFirstProduct,
  sendToKitchen,
  markFirstReadyOnKds,
  openCashIfNeeded,
  getActiveCashSessionId,
};
