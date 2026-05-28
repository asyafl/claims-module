import { chromium } from "playwright";
import { mkdirSync } from "fs";
import path from "path";
const SS = "C:\\Users\\asya\\AppData\\Local\\Temp\\claims_final";
mkdirSync(SS, { recursive: true });

const browser = await chromium.launch({ headless: false, slowMo: 100 });
const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 } });
const page = await ctx.newPage();

await page.goto("http://localhost:4200/login", { waitUntil: "networkidle" });
await page.locator("input[type=email]").fill("handler@claims.io");
await page.locator("input[type=password]").fill("Handler123!");
await page.locator("button[type=submit]").click();
await page.waitForURL("**/claims", { timeout: 10000 });
await page.waitForTimeout(4000);
await page.screenshot({ path: path.join(SS,"01_dashboard.png"), fullPage: true });

// Click claim
await page.locator("tr.mat-mdc-row").first().click();
await page.waitForTimeout(4000);
await page.screenshot({ path: path.join(SS,"02_detail_overview.png"), fullPage: true });

// Reserves tab
await page.getByRole("tab", { name: /Reserves/ }).click();
await page.waitForTimeout(1500);
await page.screenshot({ path: path.join(SS,"03_reserves.png"), fullPage: true });

// Audit tab
await page.getByRole("tab", { name: /Audit/ }).click();
await page.waitForTimeout(1000);
await page.screenshot({ path: path.join(SS,"04_audit.png"), fullPage: true });

// FNOL
await page.goto("http://localhost:4200/claims/new", { waitUntil: "networkidle" });
await page.waitForTimeout(2000);
await page.screenshot({ path: path.join(SS,"05_fnol.png"), fullPage: true });

// Login as supervisor
await page.goto("http://localhost:4200/login");
await page.waitForTimeout(500);
await page.locator("input[type=email]").fill("supervisor@claims.io");
await page.locator("input[type=password]").fill("Supervisor123!");
await page.locator("button[type=submit]").click();
await page.waitForURL("**/claims", { timeout: 10000 });
await page.locator("tr.mat-mdc-row").first().click();
await page.waitForTimeout(4000);
await page.getByRole("tab", { name: /Reserves/ }).click();
await page.waitForTimeout(1500);
await page.screenshot({ path: path.join(SS,"06_reserves_supervisor.png"), fullPage: true });

await browser.close();
console.log("Screenshots:", SS);
