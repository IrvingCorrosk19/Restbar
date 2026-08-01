const { expect } = require('@playwright/test');
const crypto = require('crypto');

const ADMIN = {
  email: process.env.RESTBAR_ADMIN_EMAIL || 'admin@restbar.com',
  password: process.env.RESTBAR_ADMIN_PASSWORD || '123456',
  mfaSecret: process.env.RESTBAR_MFA_SECRET || 'JBSWY3DPEHPK3PXP',
};

function base32Decode(input) {
  const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ234567';
  const cleaned = String(input).replace(/=+$/g, '').replace(/\s+/g, '').toUpperCase();
  let bits = '';
  for (const c of cleaned) {
    const val = alphabet.indexOf(c);
    if (val < 0) continue;
    bits += val.toString(2).padStart(5, '0');
  }
  const bytes = [];
  for (let i = 0; i + 8 <= bits.length; i += 8) {
    bytes.push(parseInt(bits.slice(i, i + 8), 2));
  }
  return Buffer.from(bytes);
}

function generateTotp(secret, windowOffset = 0) {
  const key = base32Decode(secret);
  const timestep = Math.floor(Date.now() / 1000 / 30) + windowOffset;
  const buf = Buffer.alloc(8);
  buf.writeBigInt64BE(BigInt(timestep));
  const hmac = crypto.createHmac('sha1', key).update(buf).digest();
  const offset = hmac[hmac.length - 1] & 0xf;
  const code =
    ((hmac[offset] & 0x7f) << 24) |
    ((hmac[offset + 1] & 0xff) << 16) |
    ((hmac[offset + 2] & 0xff) << 8) |
    (hmac[offset + 3] & 0xff);
  return String(code % 1_000_000).padStart(6, '0');
}

async function completeMfaIfNeeded(page) {
  const path = new URL(page.url()).pathname;
  if (path.includes('/Auth/MfaChallenge')) {
    await page.locator('input[name="code"]').fill(generateTotp(ADMIN.mfaSecret));
    await page.locator('button.btn-login, button[type="submit"]').first().click({ noWaitAfter: true });
    await page.waitForURL(url => !url.pathname.includes('/Auth/MfaChallenge') && !url.pathname.includes('/Auth/Login'), {
      timeout: 60000,
    });
    return;
  }
  if (path.includes('/Auth/MfaSetup')) {
    const secretText = await page.locator('code').first().innerText();
    const secret = (secretText || ADMIN.mfaSecret).trim();
    await page.locator('input[name="code"]').fill(generateTotp(secret));
    await page.locator('button.btn-login, button[type="submit"]').first().click({ noWaitAfter: true });
    await page.waitForURL(url => !url.pathname.includes('/Auth/MfaSetup'), { timeout: 60000 });
  }
}

async function loginAsAdmin(page) {
  for (let attempt = 1; attempt <= 5; attempt++) {
    try {
      await page.goto('/Auth/Login', { waitUntil: 'domcontentloaded', timeout: 60000 });
      await page.locator('input[name="email"]').waitFor({ state: 'visible', timeout: 20000 });
      await page.locator('input[name="email"]').fill(ADMIN.email);
      await page.locator('input[name="password"]').fill(ADMIN.password);
      await page.locator('button.btn-login').click({ noWaitAfter: true, timeout: 15000 });
      await page.waitForURL(url => !url.pathname.includes('/Auth/Login'), { timeout: 60000 });
      await completeMfaIfNeeded(page);
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
    'aspnetcore-browser-refresh',
    'cdn.datatables.net',
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

module.exports = {
  ADMIN,
  loginAsAdmin,
  generateTotp,
  completeMfaIfNeeded,
  collectConsoleErrors,
  significantConsoleErrors,
  expectNoHttp500,
};
