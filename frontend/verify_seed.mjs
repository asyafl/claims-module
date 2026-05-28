import { chromium } from "playwright";
import { mkdirSync } from "fs";
import path from "path";
const SS = "C:\\Users\\asya\\AppData\\Local\\Temp\\seed_check";
mkdirSync(SS, { recursive: true });

const browser = await chromium.launch({ headless: false });
const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });

await page.goto("http://localhost:4200/login", { waitUntil: "networkidle" });
await page.locator("input[type=email]").fill("handler@claims.io");
await page.locator("input[type=password]").fill("Handler123!");
await page.locator("button[type=submit]").click();
await page.waitForURL("**/claims", { timeout: 10000 });
await page.waitForTimeout(4000);
await page.screenshot({ path: path.join(SS, "dashboard_with_seeds.png"), fullPage: true });
console.log("Screenshot saved");
await browser.close();
