import { chromium } from 'playwright';
import { writeFileSync, mkdirSync } from 'fs';
import path from 'path';

const SS_DIR = 'C:\\Users\\asya\\AppData\\Local\\Temp\\claims_screenshots';
mkdirSync(SS_DIR, { recursive: true });

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage();
page.on('console', m => m.type() === 'error' && console.log('[CONSOLE ERROR]', m.text()));

const results = [];

function log(step, status, detail) {
  results.push({ step, status, detail });
  console.log(`${status} ${step}: ${detail}`);
}

// 1. Login page
await page.goto('http://localhost:4200', { waitUntil: 'networkidle' });
await page.waitForTimeout(2000);
const url1 = page.url();
const hasLoginForm = await page.locator('mat-card').count();
await page.screenshot({ path: path.join(SS_DIR, '01_login.png'), fullPage: true });
log('Login page loads', url1.includes('/login') || url1.includes('4200') ? '✅' : '❌', `URL: ${url1}, mat-card count: ${hasLoginForm}`);

// 2. Login
const emailInput = page.locator('input[type="email"]');
const pwInput = page.locator('input[type="password"]');
await emailInput.fill('handler@claims.io');
await pwInput.fill('Handler123!');
await page.screenshot({ path: path.join(SS_DIR, '02_filled_login.png'), fullPage: true });
await page.locator('button[type="submit"]').click();
await page.waitForURL('**/claims', { timeout: 8000 }).catch(() => {});
await page.waitForTimeout(3000);
const url2 = page.url();
await page.screenshot({ path: path.join(SS_DIR, '03_after_login.png'), fullPage: true });
log('Login succeeds', url2.includes('/claims') ? '✅' : '❌', `URL after login: ${url2}`);

// 3. Claims list
const toolbar = await page.locator('mat-toolbar').textContent().catch(() => '');
const claimRows = await page.locator('tr.mat-mdc-row, tr[mat-row]').count();
const claimNum = await page.locator('text=CLM-2026-0000001').count();
await page.screenshot({ path: path.join(SS_DIR, '04_claims_list.png'), fullPage: true });
log('Claims list shows rows', claimRows > 0 ? '✅' : '❌', `Rows: ${claimRows}, CLM-2026-0000001 visible: ${claimNum > 0}`);

// 4. Status badge colored
const badges = await page.locator('.status-badge').all();
let hasColor = false;
for (const b of badges) {
  const style = await b.getAttribute('style');
  if (style && style.includes('color')) { hasColor = true; break; }
}
log('Status badges have color', hasColor ? '✅' : '❌', `Found ${badges.length} badges with inline color styles`);

// 5. Click claim → detail
if (claimNum > 0) {
  await page.locator('text=CLM-2026-0000001').first().click();
} else if (claimRows > 0) {
  await page.locator('tr.mat-mdc-row, tr[mat-row]').first().click();
}
await page.waitForTimeout(3000);
const url5 = page.url();
const tabs = await page.locator('mat-tab-group mat-tab-header, .mat-mdc-tab-label').count();
await page.screenshot({ path: path.join(SS_DIR, '05_claim_detail.png'), fullPage: true });
log('Claim detail with tabs', url5.includes('/claims/') && !url5.endsWith('/claims/') ? '✅' : '❌', `URL: ${url5}, tab count: ${tabs}`);

// 6. Check tab headings
const tabTexts = await page.locator('mat-tab-header .mat-mdc-tab-label-content, .mat-tab-label-content').allTextContents().catch(() => []);
log('Tabs visible', tabTexts.length >= 3 ? '✅' : '⚠️', `Tabs: [${tabTexts.join(', ')}]`);

await browser.close();
console.log('\n=== RESULTS ===');
results.forEach(r => console.log(`${r.status} ${r.step}: ${r.detail}`));
console.log(`\nScreenshots saved to: ${SS_DIR}`);
