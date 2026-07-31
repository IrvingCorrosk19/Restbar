const { test, expect } = require('@playwright/test');
const { loginAsAdmin, collectConsoleErrors, significantConsoleErrors } = require('../helpers/auth');

test.describe('Inventory Index — functional browser', () => {
  test('INV-01 page loads after login (no 500)', async ({ page }) => {
    await loginAsAdmin(page);
    const res = await page.goto('/Inventory', { waitUntil: 'domcontentloaded' });
    expect(res.status(), '/Inventory HTTP status').toBeLessThan(500);
    expect(res.status()).toBe(200);
    await expect(page.getByRole('heading', { name: /Gestión de Inventario/i })).toBeVisible();
    await expect(page.getByRole('heading', { name: /Productos con Stock Bajo/i })).toBeVisible();
    await expect(page.getByRole('heading', { name: /Reporte de Consumo/i })).toBeVisible();
  });

  test('INV-02 dashboard card navigates to Inventory', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Home', { waitUntil: 'domcontentloaded' });
    const card = page.locator('a[href*="/Inventory"]').first();
    await expect(card).toBeVisible({ timeout: 15000 });
    await card.click();
    await page.waitForURL(/\/Inventory/i, { timeout: 15000 });
    await expect(page.getByRole('heading', { name: /Gestión de Inventario/i })).toBeVisible();
  });

  test('INV-03 low-stock alerts API + UI settle', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Inventory', { waitUntil: 'domcontentloaded' });

    const api = await page.waitForResponse(
      r => r.url().includes('/Inventory/GetLowStockProducts') && r.request().method() === 'GET',
      { timeout: 20000 }
    );
    expect(api.status()).toBe(200);
    const body = await api.json();
    expect(body.success).toBeTruthy();
    expect(Array.isArray(body.data)).toBeTruthy();

    const alerts = page.locator('#lowStockAlerts');
    await expect(alerts).not.toContainText(/Cargando alertas/i, { timeout: 15000 });
    if (body.data.length === 0) {
      await expect(alerts).toContainText(/No hay productos con stock bajo/i);
    } else {
      await expect(alerts.locator('.stock-alert').first()).toBeVisible();
    }
  });

  test('INV-04 support APIs respond 200 with success', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Inventory', { waitUntil: 'domcontentloaded' });

    const endpoints = [
      '/Inventory/GetLowStockProducts',
      '/Inventory/GetInventoryData',
      '/Inventory/GetProducts',
      '/Inventory/GetCategories',
      '/Inventory/GetBranches',
    ];

    for (const path of endpoints) {
      const res = await page.request.get(path);
      expect(res.status(), path).toBe(200);
      const json = await res.json();
      expect(json.success, path).toBeTruthy();
    }
  });

  test('INV-05 consumption report filters visible + generate', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Inventory', { waitUntil: 'domcontentloaded' });

    await expect(page.locator('#startDate')).toBeVisible();
    await expect(page.locator('#endDate')).toBeVisible();
    await expect(page.locator('#productFilter')).toBeVisible();
    await expect(page.locator('#stationFilter')).toBeVisible();
    await expect(page.getByRole('button', { name: /Generar Reporte/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /Exportar/i })).toBeVisible();

    // Product/station filters should load options beyond the default
    await expect.poll(async () => page.locator('#productFilter option').count(), { timeout: 15000 }).toBeGreaterThan(1);
    await expect.poll(async () => page.locator('#stationFilter option').count(), { timeout: 15000 }).toBeGreaterThan(1);

    const reportPromise = page.waitForResponse(
      r => r.url().includes('/Inventory/ConsumptionReport') && r.request().method() === 'GET',
      { timeout: 20000 }
    );
    await page.getByRole('button', { name: /Generar Reporte/i }).click();
    const reportRes = await reportPromise;
    expect(reportRes.status()).toBe(200);
    const reportBody = await reportRes.json();

    test.info().annotations.push({
      type: 'consumption-report',
      description: JSON.stringify({
        success: reportBody.success,
        message: reportBody.message || null,
        dataLen: Array.isArray(reportBody.data) ? reportBody.data.length : null,
      }),
    });

    // Functional requirement: report must succeed and render results
    expect(reportBody.success, reportBody.message || 'ConsumptionReport failed').toBeTruthy();
    expect(Array.isArray(reportBody.data)).toBeTruthy();
    await expect(page.locator('#consumptionReport')).not.toContainText(/Selecciona filtros/i, { timeout: 10000 });
  });

  test('INV-06 export downloads CSV / does not 500', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Inventory', { waitUntil: 'domcontentloaded' });
    const downloadPromise = page.waitForEvent('download', { timeout: 10000 }).catch(() => null);
    await page.getByRole('button', { name: /Exportar/i }).click();
    const download = await downloadPromise;
    if (download) {
      expect(download.suggestedFilename()).toMatch(/consumo_inventario_.*\.csv/i);
    } else {
      // Sin datos previos: puede avisar warning, nunca "Próximamente"
      const dialog = page.getByRole('dialog');
      if (await dialog.isVisible().catch(() => false)) {
        await expect(page.locator('.swal2-title')).not.toContainText(/Próximamente/i);
      }
    }
  });

  test('INV-07 unauthorized redirect when logged out', async ({ page }) => {
    await page.goto('/Auth/Logout').catch(() => {});
    await page.context().clearCookies();
    const res = await page.goto('/Inventory', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBeLessThan(500);
    await expect(page).toHaveURL(/Auth\/Login/i);
  });

  test('INV-08 no significant console errors on Index load', async ({ page }) => {
    const errors = await collectConsoleErrors(page);
    await loginAsAdmin(page);
    await page.goto('/Inventory', { waitUntil: 'networkidle' });
    await page.waitForTimeout(1500);
    const significant = significantConsoleErrors(errors);
    expect(significant, significant.join('\n')).toEqual([]);
  });
});
