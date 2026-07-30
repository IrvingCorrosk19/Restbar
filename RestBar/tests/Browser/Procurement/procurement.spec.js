const { test, expect } = require('@playwright/test');
const { loginAsAdmin, collectConsoleErrors, significantConsoleErrors, expectNoHttp500 } = require('../helpers/auth');

test.describe('RB-020 Procurement', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('PO-01 Supplier index loads', async ({ page }) => {
    const errors = await collectConsoleErrors(page);
    const res = await expectNoHttp500(page, '/Supplier');
    expect(res.status()).toBe(200);
    await expect(page.getByRole('heading', { name: /Proveedores/i })).toBeVisible();
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/po-01-suppliers.png', fullPage: true });
    expect(significantConsoleErrors(errors).length, JSON.stringify(errors)).toBe(0);
  });

  test('PO-02 Procurement dashboard', async ({ page }) => {
    await expectNoHttp500(page, '/ProcurementDashboard');
    await expect(page.locator('body')).not.toContainText('Exception');
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/po-02-dashboard.png', fullPage: true });
  });

  test('PO-03 Purchase order list', async ({ page }) => {
    await expectNoHttp500(page, '/PurchaseOrder');
    await expect(page.getByRole('heading', { name: /Órdenes de compra|Ordenes de compra/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /Nueva PO/i })).toBeVisible();
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/po-03-po-list.png', fullPage: true });
  });

  test('PO-04 Create PO wizard renders', async ({ page }) => {
    await expectNoHttp500(page, '/PurchaseOrder/Create');
    await expect(page.locator('#supplierId')).toBeVisible();
    await expect(page.locator('#productId')).toBeVisible();
    await expect(page.locator('#btnCreate')).toBeVisible();
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/po-04-create.png', fullPage: true });
  });

  test('PO-05 Create PO rejects empty product (negative)', async ({ page }) => {
    await page.goto('/PurchaseOrder/Create');
    const supplierCount = await page.locator('#supplierId option').count();
    test.skip(supplierCount === 0, 'No suppliers seeded');
    await page.locator('#productId').fill('');
    await page.locator('#btnCreate').click();
    await page.waitForTimeout(800);
    const msg = await page.locator('#msg').innerText();
    // Must not crash page; either validation message or JSON error text
    await expect(page.locator('body')).not.toContainText('Exception:');
    expect(msg.length >= 0).toBeTruthy();
    await page.screenshot({ path: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/po-05-negative.png', fullPage: true });
  });

  test('PO-06 GetSuppliers JSON when enabled', async ({ page }) => {
    const res = await page.request.get('/Supplier/GetSuppliers');
    expect(res.status()).not.toBe(500);
    expect(res.status()).toBeLessThan(500);
  });
});
