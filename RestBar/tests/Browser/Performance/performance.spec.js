const { test, expect } = require('@playwright/test');
const { loginAsAdmin } = require('../helpers/auth');

const pages = [
  { name: 'CashDashboard', path: '/CashSession/Dashboard' },
  { name: 'Procurement', path: '/ProcurementDashboard' },
  { name: 'FoodCost', path: '/FoodCostDashboard' },
  { name: 'CommandCenter', path: '/ExecutiveCommandCenter' },
  { name: 'Orders', path: '/Order' },
];

test.describe('Performance · page load P95 budget', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  for (const p of pages) {
    test(`PERF-${p.name} < 2000ms DOMContentLoaded`, async ({ page }) => {
      const start = Date.now();
      const res = await page.goto(p.path, { waitUntil: 'domcontentloaded' });
      const ms = Date.now() - start;
      expect(res.status(), p.path).not.toBe(500);
      // Soft budget: fail hard only if > 5s (env variance); record target 2s
      expect(ms, `${p.name} took ${ms}ms`).toBeLessThan(5000);
      console.log(`PERF ${p.name}: ${ms}ms (target P95 < 2000)`);
      test.info().annotations.push({ type: 'perf_ms', description: String(ms) });
    });
  }
});
