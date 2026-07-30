const { expect } = require('@playwright/test');

const ADMIN = {
  email: process.env.RESTBAR_ADMIN_EMAIL || 'admin@restbar.com',
  password: process.env.RESTBAR_ADMIN_PASSWORD || '123456',
};

async function loginAsAdmin(page) {
  for (let attempt = 1; attempt <= 5; attempt++) {
    try {
      await page.goto('/Auth/Login', { waitUntil: 'domcontentloaded', timeout: 60000 });
      await page.locator('input[name="email"]').waitFor({ state: 'visible', timeout: 20000 });
      await page.locator('input[name="email"]').fill(ADMIN.email);
      await page.locator('input[name="password"]').fill(ADMIN.password);
      await page.locator('button.btn-login').click({ noWaitAfter: true, timeout: 15000 });
      await page.waitForURL(url => !url.pathname.includes('/Auth/Login'), { timeout: 60000 });
      return;
    } catch (err) {
      if (attempt === 5) throw err;
      await page.waitForTimeout(2000 * attempt);
    }
  }
}

async function collectConsoleErrors(page) {
  const errors = [];
  page.on('console', msg => {
    if (msg.type() === 'error') errors.push(msg.text());
  });
  page.on('pageerror', err => errors.push(String(err)));
  return errors;
}

function significantConsoleErrors(errors) {
  const noise = [
    'favicon',
    'signalr',
    'websocket',
    'hubconnection',
    'failed to start the transport',
    'negotiate',
    'err_connection_refused',
    'failed to load resource',
    'status of 404',
    'net::err_',
  ];
  return errors.filter(e => {
    const lower = String(e).toLowerCase();
    return !noise.some(n => lower.includes(n));
  });
}

async function expectNoHttp500(page, path) {
  const res = await page.goto(path, { waitUntil: 'domcontentloaded' });
  expect(res, `navigation to ${path}`).not.toBeNull();
  expect(res.status(), `${path} should not be 500`).toBeLessThan(500);
  return res;
}

module.exports = { ADMIN, loginAsAdmin, collectConsoleErrors, significantConsoleErrors, expectNoHttp500 };
