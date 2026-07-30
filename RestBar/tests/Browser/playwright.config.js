// @ts-check
const { defineConfig, devices } = require('@playwright/test');

const baseURL = process.env.RESTBAR_BASE_URL || 'http://localhost:5001';

module.exports = defineConfig({
  testDir: './',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: 0,
  workers: 1,
  timeout: 60_000,
  expect: { timeout: 15_000 },
  reporter: [
    ['list'],
    ['html', { outputFolder: '../../RB-010_020_023_BROWSER_CERTIFICATION/playwright-report', open: 'never' }],
    ['json', { outputFile: '../../RB-010_020_023_BROWSER_CERTIFICATION/playwright-results.json' }]
  ],
  use: {
    baseURL,
    trace: 'retain-on-failure',
    screenshot: 'on',
    video: 'retain-on-failure',
    actionTimeout: 15_000,
  },
  outputDir: '../../RB-010_020_023_BROWSER_CERTIFICATION/evidence/test-output',
  projects: [
    {
      name: 'chromium-desktop',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'chromium-tablet',
      use: {
        ...devices['Desktop Chrome'],
        viewport: { width: 834, height: 1194 },
        isMobile: true,
        hasTouch: true,
      },
    },
    {
      name: 'chromium-mobile',
      use: {
        ...devices['Desktop Chrome'],
        viewport: { width: 412, height: 915 },
        isMobile: true,
        hasTouch: true,
      },
    },
  ],
});
