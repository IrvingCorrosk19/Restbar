const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');

test.describe('Orders POS — navigation exit', () => {
  test('ORD-NAV-01 POS chrome shows Volver and Inicio', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Order/Index?returnUrl=' + encodeURIComponent('/Home'), { waitUntil: 'domcontentloaded' });

    await expect(page.getByTestId('order-pos-chrome')).toBeVisible({ timeout: 15000 });
    await expect(page.getByTestId('order-nav-back')).toBeVisible();
    await expect(page.getByTestId('order-nav-home')).toBeVisible();
    await expect(page.getByTestId('order-nav-home')).toHaveAttribute('href', /Home/i);
  });

  test('ORD-NAV-02 Inicio returns to Home without trap', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Order/Index?returnUrl=' + encodeURIComponent('/Home'), { waitUntil: 'domcontentloaded' });
    await expect(page.getByTestId('order-nav-home')).toBeVisible({ timeout: 15000 });

    await page.getByTestId('order-nav-home').click();
    await page.waitForURL(url => /\/Home/i.test(url.pathname) || url.pathname === '/', { timeout: 20000 });
    await expect(page.getByRole('heading', { name: /Acciones Rápidas|Dashboard|RestBar/i }).first()).toBeVisible({ timeout: 15000 });
  });

  test('ORD-NAV-03 Volver exits to safe returnUrl', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Order/Index?returnUrl=' + encodeURIComponent('/Home'), { waitUntil: 'domcontentloaded' });
    await expect(page.getByTestId('order-nav-back')).toBeVisible({ timeout: 15000 });

    await page.getByTestId('order-nav-back').click();
    await page.waitForURL(url => /\/Home/i.test(url.pathname) || url.pathname === '/', { timeout: 20000 });
  });

  test('ORD-NAV-04 open redirect returnUrl is rejected', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Order/Index?returnUrl=' + encodeURIComponent('https://evil.example/phish'), { waitUntil: 'domcontentloaded' });
    const href = await page.getByTestId('order-nav-home').getAttribute('href');
    expect(href || '').not.toMatch(/evil\.example/i);
    expect(href || '').toMatch(/Home|^\//i);
  });

  test('ORD-NAV-05 KDS Dashboard link uses tag helper / Home', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Order/StationOrders?stationType=kitchen&returnUrl=' + encodeURIComponent('/Home'), { waitUntil: 'domcontentloaded' });
    const home = page.getByTestId('kds-nav-home');
    await expect(home).toBeVisible({ timeout: 15000 });
    await home.click();
    await page.waitForURL(url => /\/Home/i.test(url.pathname) || url.pathname === '/', { timeout: 20000 });
  });

  test('ORD-NAV-06 Home Pedidos card enters POS with chrome', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Home', { waitUntil: 'domcontentloaded' });
    const card = page.getByTestId('home-card-orders');
    await expect(card).toBeVisible({ timeout: 15000 });
    await card.click();
    await page.waitForURL(/\/Order/i, { timeout: 20000 });
    await expect(page.getByTestId('order-pos-chrome')).toBeVisible({ timeout: 15000 });
  });
});
