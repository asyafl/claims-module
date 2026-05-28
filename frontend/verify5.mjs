import { chromium } from "playwright";
import { mkdirSync } from "fs";
import path from "path";
const SS = "C:\\Users\\asya\\AppData\\Local\\Temp\\claims_ss4";
mkdirSync(SS, { recursive: true });

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage();
const errors = [];
page.on("console", m => { if (m.type() === "error") errors.push(m.text().substring(0,100)); });

// Login
await page.goto("http://localhost:4200/login", { waitUntil: "networkidle" });
await page.locator("input[type=email]").fill("handler@claims.io");
await page.locator("input[type=password]").fill("Handler123!");
await page.locator("button[type=submit]").click();
await page.waitForURL("**/claims", { timeout: 10000 });
await page.waitForTimeout(4000);

// Claims list
const rows = await page.locator("tr.mat-mdc-row").count();
console.log(`Claims rows: ${rows}`);
await page.screenshot({ path: path.join(SS,"01_list.png"), fullPage: true });

// Click claim → detail (wait for spinner to hide)
await page.locator("tr.mat-mdc-row").first().click();
await page.locator(".loading-center mat-spinner, mat-spinner").waitFor({ state: "hidden", timeout: 10000 }).catch(() => {});
await page.waitForTimeout(2000);
await page.screenshot({ path: path.join(SS,"02_detail.png"), fullPage: true });
const tabs = await page.locator(".mat-mdc-tab-label-content").allTextContents();
const header = await page.locator(".claim-number").textContent().catch(() => "not found");
console.log(`Detail header: ${header}`);
console.log(`Tabs: [${tabs.join(", ")}]`);

// Test Reserves tab
const resTab = page.locator(".mat-mdc-tab-label-content", { hasText: /Reserves/ });
if (await resTab.count()) { await resTab.click(); await page.waitForTimeout(1000); }
await page.screenshot({ path: path.join(SS,"03_reserves.png"), fullPage: true });

// Test FNOL 
await page.goto("http://localhost:4200/claims/new", { waitUntil: "networkidle" });
await page.waitForTimeout(1500);
await page.screenshot({ path: path.join(SS,"04_fnol.png"), fullPage: true });
const steps = await page.locator(".mat-step-label").count();
console.log(`FNOL stepper steps: ${steps}`);

await browser.close();
console.log("Errors:", errors.length ? errors : "none");
console.log("SS:", SS);
