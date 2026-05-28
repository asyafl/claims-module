import { chromium } from "playwright";
import { mkdirSync } from "fs";
import path from "path";

const SS_DIR = "C:\\Users\\asya\\AppData\\Local\\Temp\\claims_screenshots2";
mkdirSync(SS_DIR, { recursive: true });

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage();
const errors = [];
page.on("console", m => { if (m.type() === "error") errors.push(m.text().substring(0, 120)); });

// 1. Login
await page.goto("http://localhost:4200/login", { waitUntil: "networkidle" });
await page.locator("input[type=email]").fill("handler@claims.io");
await page.locator("input[type=password]").fill("Handler123!");
await page.locator("button[type=submit]").click();
await page.waitForURL("**/claims", { timeout: 10000 });

// 2. Wait for spinner to disappear (data loaded)
await page.locator("mat-spinner, mat-progress-spinner").waitFor({ state: "hidden", timeout: 10000 }).catch(() => {});
await page.waitForTimeout(2000);
await page.screenshot({ path: path.join(SS_DIR, "01_claims_list.png"), fullPage: true });

const rows = await page.locator("tr.mat-mdc-row").count();
const claimNum = await page.locator("text=CLM-2026-0000001").count();
console.log(`Claims rows: ${rows}, CLM visible: ${claimNum}`);
console.log("URL:", page.url());

// 3. Click first row
if (rows > 0) {
  await page.locator("tr.mat-mdc-row").first().click();
  await page.waitForTimeout(3000);
  await page.screenshot({ path: path.join(SS_DIR, "02_claim_detail.png"), fullPage: true });
  const tabCount = await page.locator(".mat-mdc-tab-label-content").count();
  console.log("Tabs:", tabCount, "URL:", page.url());
  const tabTexts = await page.locator(".mat-mdc-tab-label-content").allTextContents();
  console.log("Tab labels:", tabTexts.join(", "));
}

// 4. Test FNOL form
await page.locator("button", { hasText: "Log New Claim" }).first().click();
await page.waitForURL("**/claims/new", { timeout: 5000 }).catch(() => {});
await page.waitForTimeout(1000);
await page.screenshot({ path: path.join(SS_DIR, "03_fnol.png"), fullPage: true });
console.log("FNOL URL:", page.url());

await browser.close();
console.log("\nConsole errors:", errors.length ? errors : "none");
console.log("Screenshots at:", SS_DIR);
