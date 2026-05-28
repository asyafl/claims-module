import { chromium } from "playwright";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage();

// Capture all network responses
const apiCalls = [];
page.on("response", async resp => {
  if (resp.url().includes("5000")) {
    try {
      const body = await resp.text().catch(() => "(binary)");
      apiCalls.push({ url: resp.url(), status: resp.status(), body: body.substring(0, 300) });
    } catch {}
  }
});

await page.goto("http://localhost:4200/login", { waitUntil: "networkidle" });
await page.locator("input[type=email]").fill("handler@claims.io");
await page.locator("input[type=password]").fill("Handler123!");
await page.locator("button[type=submit]").click();
await page.waitForURL("**/claims", { timeout: 10000 });
await page.waitForTimeout(5000); // wait for all API calls

console.log("\n=== API CALLS TO LOCALHOST:5000 ===");
apiCalls.forEach(c => {
  console.log(`${c.status} ${c.url}`);
  console.log("  Body preview:", c.body.substring(0, 200));
});

await browser.close();
