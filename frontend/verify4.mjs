import { chromium } from "playwright";
import { mkdirSync } from "fs";
import path from "path";

const SS_DIR = "C:\\Users\\asya\\AppData\\Local\\Temp\\claims_ss3";
mkdirSync(SS_DIR, { recursive: true });

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage();
const errors = [];
page.on("console", m => { if (m.type() === "error") errors.push(m.text().substring(0, 80)); });

// Login
await page.goto("http://localhost:4200/login", { waitUntil: "networkidle" });
await page.locator("input[type=email]").fill("handler@claims.io");
await page.locator("input[type=password]").fill("Handler123!");
await page.locator("button[type=submit]").click();
await page.waitForURL("**/claims", { timeout: 10000 });
await page.waitForTimeout(4000);
await page.screenshot({ path: path.join(SS_DIR, "01_claims_list.png"), fullPage: true });

const rows = await page.locator("tr.mat-mdc-row").count();
const claimNum = await page.locator("text=CLM-2026-0000001").count();
console.log(`✅/❌ Claims loaded: rows=${rows}, CLM visible=${claimNum > 0}`);

// Screenshot claim detail
if (rows > 0) {
  await page.locator("tr.mat-mdc-row").first().click();
  await page.waitForTimeout(3000);
  await page.screenshot({ path: path.join(SS_DIR, "02_detail.png"), fullPage: true });
  const tabs = await page.locator(".mat-mdc-tab-label-content").allTextContents();
  console.log(`✅ Tabs: [${tabs.join(", ")}]`);

  // Click Reserves tab
  const reserveTab = page.locator(".mat-mdc-tab-label-content", { hasText: "Reserves" });
  if (await reserveTab.count() > 0) {
    await reserveTab.click();
    await page.waitForTimeout(1000);
    await page.screenshot({ path: path.join(SS_DIR, "03_reserves.png"), fullPage: true });
    console.log("✅ Reserves tab clicked");
  }
  
  // Click Audit tab
  const auditTab = page.locator(".mat-mdc-tab-label-content", { hasText: "Audit" });
  if (await auditTab.count() > 0) {
    await auditTab.click();
    await page.waitForTimeout(1000);
    await page.screenshot({ path: path.join(SS_DIR, "04_audit.png"), fullPage: true });
    console.log("✅ Audit tab clicked");
  }
}

// Test FNOL form
await page.goto("http://localhost:4200/claims/new", { waitUntil: "networkidle" });
await page.waitForTimeout(2000);
await page.screenshot({ path: path.join(SS_DIR, "05_fnol.png"), fullPage: true });
const steps = await page.locator(".mat-step-label").count();
console.log(`✅ FNOL steps: ${steps}`);

await browser.close();
console.log("\nErrors:", errors.length ? errors.join("\n") : "none");
console.log("Screenshots:", SS_DIR);
