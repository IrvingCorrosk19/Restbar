const { chromium } = require('@playwright/test');
const { loginAsAdmin, completeMfaIfNeeded, generateTotp, ADMIN } = require('./auth');

/**
 * Independent browser contexts = isolated cookies/storage (Tab Browser mandate).
 */
async function createIsolatedContext(browser, label = 'ctx') {
  const context = await browser.newContext({
    locale: 'es-PA',
    viewport: { width: 1400, height: 900 },
  });
  context._restbarLabel = label;
  const page = await context.newPage();
  return { context, page, label };
}

async function loginAs(page, email, password = ADMIN.password) {
  await page.goto('/Auth/Login', { waitUntil: 'domcontentloaded', timeout: 60000 });
  await page.locator('input[name="email"]').fill(email);
  await page.locator('input[name="password"]').fill(password);
  await page.locator('button.btn-login').click({ noWaitAfter: true, timeout: 15000 });
  await page.waitForURL(
    (url) => !url.pathname.includes('/Auth/Login') || url.pathname.includes('Mfa'),
    { timeout: 60000 }
  ).catch(() => {});
  await completeMfaIfNeeded(page);
  await page.waitForTimeout(400);
  return !page.url().includes('/Auth/Login');
}

async function loginRoleOrSkip(page, email, test) {
  const ok = await loginAs(page, email);
  if (!ok) {
    test.skip(true, `${email} not available in this environment`);
    return false;
  }
  return true;
}

const TENANTS = {
  demo: { admin: ADMIN.email, label: 'Demo' },
  costa: { admin: 'admin@costa.restbar.com', mesero: 'mesero1@costa.restbar.com', chef: 'chef@costa.restbar.com', bartender: 'bartender@costa.restbar.com', cajero: 'cajero@costa.restbar.com', label: 'Costa' },
  norte: { admin: 'admin@norte.restbar.com', label: 'Norte' },
  sur: { admin: 'admin@sur.restbar.com', label: 'Sur' },
};

async function withParallelContexts(browser, specs, fn) {
  const opened = [];
  try {
    for (const spec of specs) {
      const pair = await createIsolatedContext(browser, spec.label || spec.email);
      opened.push(pair);
      const ok = await loginAs(pair.page, spec.email, spec.password || ADMIN.password);
      pair.loginOk = ok;
    }
    return await fn(opened);
  } finally {
    for (const o of opened) {
      await o.context.close().catch(() => {});
    }
  }
}

module.exports = {
  createIsolatedContext,
  loginAs,
  loginRoleOrSkip,
  TENANTS,
  withParallelContexts,
  loginAsAdmin,
  generateTotp,
};
