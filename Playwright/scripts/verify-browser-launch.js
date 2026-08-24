const { chromium } = require('@playwright/test');

(async () => {
    console.log('Testing Playwright browser launch and context creation...');
    const browser = await chromium.launch({ headless: true });
    console.log('1. Chromium browser launched successfully.');

    const context = await browser.newContext({
        viewport: { width: 1280, height: 720 },
        userAgent: 'EImece-Verification-Agent'
    });
    console.log('2. Browser context created successfully.');

    const page = await context.newPage();
    console.log('3. Browser page created.');

    const response = await page.goto('http://localhost:81/', { waitUntil: 'domcontentloaded', timeout: 15000 });
    console.log(`4. Navigated to http://localhost:81/ - Status: ${response.status()}`);

    const title = await page.title();
    console.log(`5. Page Title: "${title}"`);

    await context.close();
    await browser.close();
    console.log('6. Browser context and process cleanly closed. ALL CHECKS PASSED.');
})();
