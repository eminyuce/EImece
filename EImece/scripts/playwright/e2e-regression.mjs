import { chromium } from 'playwright';

const BASE_URL = 'http://localhost:81';
const CUSTOMER_EMAIL = 'eminyuce1111@gmail.com';
const CUSTOMER_PASSWORD = 'V02y.qcF';

async function runE2ETests() {
    console.log('====================================================');
    console.log('STARTING PLAYWRIGHT E2E REGRESSION TEST SUITE');
    console.log(`Target: ${BASE_URL}`);
    console.log('====================================================\n');

    const browser = await chromium.launch({ channel: 'chrome', headless: true });
    let passed = 0;
    let failed = 0;
    const errors = [];

    async function testStep(name, fn) {
        process.stdout.write(`TEST: ${name} ... `);
        try {
            await fn();
            console.log('PASSED');
            passed++;
        } catch (err) {
            console.log(`FAILED: ${err.message}`);
            failed++;
            errors.push({ name, error: err.message });
        }
    }

    try {
        // ----------------------------------------------------
        // 1. DESKTOP STOREFRONT TESTS
        // ----------------------------------------------------
        const desktopContext = await browser.newContext({
            viewport: { width: 1920, height: 1080 }
        });
        const desktopPage = await desktopContext.newPage();

        await testStep('1.1 Desktop Homepage Load & Core Elements', async () => {
            const resp = await desktopPage.goto(`${BASE_URL}/`, { waitUntil: 'domcontentloaded', timeout: 30000 });
            if (resp.status() !== 200) throw new Error(`Status ${resp.status()}`);
            const title = await desktopPage.title();
            if (!title) throw new Error('Empty page title');
        });

        await testStep('1.2 Desktop Search Functionality', async () => {
            await desktopPage.goto(`${BASE_URL}/p/arama?search=lumina`, { waitUntil: 'domcontentloaded', timeout: 30000 });
            const bodyText = await desktopPage.innerText('body');
            if (!bodyText.toLowerCase().includes('lumina')) {
                throw new Error('Search results did not contain search term');
            }
        });

        await testStep('1.3 Desktop Category Page & Filters', async () => {
            const resp = await desktopPage.goto(`${BASE_URL}/c/pc/mutfak-1b3f4h1b`, { waitUntil: 'domcontentloaded', timeout: 30000 });
            if (resp.status() !== 200) throw new Error(`Category status ${resp.status()}`);
        });

        await testStep('1.4 Desktop Product Detail Page (DTO/ViewModel & Badges)', async () => {
            const resp = await desktopPage.goto(`${BASE_URL}/p/mutfak/lumina-kitchen-termos-mug-350ml-112-7e6g7e5i4h1b`, { waitUntil: 'domcontentloaded', timeout: 30000 });
            if (resp.status() !== 200) throw new Error(`Product detail status ${resp.status()}`);
            const bodyText = await desktopPage.innerText('body');
            if (!bodyText.toLowerCase().includes('termos') && !bodyText.toLowerCase().includes('lumina')) {
                throw new Error('Product details missing from page');
            }
        });

        await testStep('1.5 Desktop Stories Listing & Story Detail', async () => {
            const resp = await desktopPage.goto(`${BASE_URL}/s/`, { waitUntil: 'domcontentloaded', timeout: 30000 });
            if (resp.status() !== 200) throw new Error(`Stories root status ${resp.status()}`);
            const storyDetailResp = await desktopPage.goto(`${BASE_URL}/s/stil-rehberi/2024-sonbahar-kombin-onerileri-3f1b8c`, { waitUntil: 'domcontentloaded', timeout: 30000 });
            if (storyDetailResp.status() !== 200) throw new Error(`Story detail status ${storyDetailResp.status()}`);
        });

        // ----------------------------------------------------
        // 2. MOBILE VIEWPORT TESTS (iPhone 14)
        // ----------------------------------------------------
        const mobileContext = await browser.newContext({
            viewport: { width: 390, height: 844 },
            userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 16_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Mobile/15E148 Safari/604.1'
        });
        const mobilePage = await mobileContext.newPage();

        await testStep('2.1 Mobile Homepage & Responsive Navigation', async () => {
            const resp = await mobilePage.goto(`${BASE_URL}/`, { waitUntil: 'domcontentloaded', timeout: 30000 });
            if (resp.status() !== 200) throw new Error(`Mobile home status ${resp.status()}`);
        });

        await testStep('2.2 Mobile Product Detail Viewport', async () => {
            const resp = await mobilePage.goto(`${BASE_URL}/p/mutfak/lumina-kitchen-termos-mug-350ml-112-7e6g7e5i4h1b`, { waitUntil: 'domcontentloaded', timeout: 30000 });
            if (resp.status() !== 200) throw new Error(`Mobile product detail status ${resp.status()}`);
        });

        // ----------------------------------------------------
        // 3. CUSTOMER AUTHENTICATION FLOW
        // ----------------------------------------------------
        await testStep('3.1 Customer Login Page Rendering', async () => {
            const resp = await desktopPage.goto(`${BASE_URL}/account/login/`, { waitUntil: 'domcontentloaded', timeout: 30000 });
            if (resp.status() !== 200) throw new Error(`Customer login status ${resp.status()}`);
        });

        await testStep('3.2 Customer Authentication (Form Submit)', async () => {
            await desktopPage.goto(`${BASE_URL}/account/login/`, { waitUntil: 'domcontentloaded', timeout: 30000 });
            const emailInput = desktopPage.locator('#customer-login-form #Email, #customer-login-form input[name="Email"]').first();
            const passInput = desktopPage.locator('#customer-login-form #Password, #customer-login-form input[name="Password"]').first();
            if (await emailInput.count() > 0 && await passInput.count() > 0) {
                await emailInput.fill(CUSTOMER_EMAIL);
                await passInput.fill(CUSTOMER_PASSWORD);
                const submitBtn = desktopPage.locator('#customer-login-form button[type="submit"]').first();
                if (await submitBtn.count() > 0) {
                    await submitBtn.click();
                    await desktopPage.waitForLoadState('domcontentloaded');
                }
            }
        });

        // ----------------------------------------------------
        // 4. SHOPPING CART & CHECKOUT FLOW
        // ----------------------------------------------------
        await testStep('4.1 View Shopping Cart', async () => {
            const resp = await desktopPage.goto(`${BASE_URL}/Payment/ShoppingCart`, { waitUntil: 'domcontentloaded', timeout: 30000 });
            if (resp.status() !== 200) throw new Error(`Cart status ${resp.status()}`);
        });

        // ----------------------------------------------------
        // 5. ADMIN PANEL REGRESSION TESTS
        // ----------------------------------------------------
        const adminPages = [
            { name: 'Dashboard', url: '/admin' },
            { name: 'System Health', url: '/admin/dashboard/systemhealth' },
            { name: 'System Settings', url: '/admin/adminsettings/systemsettings' },
            { name: 'Products Grid', url: '/admin/products' },
            { name: 'Categories Grid', url: '/admin/productcategories' },
            { name: 'Brands Grid', url: '/admin/brands' },
            { name: 'Orders Grid', url: '/admin/orders' },
            { name: 'Customers Grid', url: '/admin/customers' },
            { name: 'Coupons Grid', url: '/admin/coupons' },
            { name: 'Menus Grid', url: '/admin/menus' },
            { name: 'Stories Grid', url: '/admin/stories' },
            { name: 'Story Categories', url: '/admin/storycategories' },
            { name: 'Tag Categories', url: '/admin/tagcategories' },
            { name: 'Tags Grid', url: '/admin/tags' },
            { name: 'Shopping Carts Grid', url: '/admin/shoppingcarts' },
            { name: 'Reports Index', url: '/admin/report' },
            { name: 'Coupon Usage Report', url: '/admin/report/couponusage' },
            { name: 'Fraud Analysis Report', url: '/admin/report/fraudanalysis' },
            { name: 'Regional Sales Report', url: '/admin/report/getregionalsalesreport' },
            { name: 'App Logs Viewer', url: '/admin/applogs' },
            { name: 'Metrics Snapshot', url: '/admin/metrics' }
        ];

        for (const ap of adminPages) {
            await testStep(`5. Admin: ${ap.name} (${ap.url})`, async () => {
                const resp = await desktopPage.goto(`${BASE_URL}${ap.url}`, { waitUntil: 'domcontentloaded', timeout: 30000 });
                if (resp.status() !== 200) throw new Error(`Status ${resp.status()}`);
            });
        }

        // ----------------------------------------------------
        // 6. HEALTH & API ENDPOINTS
        // ----------------------------------------------------
        await testStep('6.1 Health Endpoint (/health)', async () => {
            const resp = await desktopPage.goto(`${BASE_URL}/health`, { timeout: 15000 });
            if (resp.status() !== 200) throw new Error(`Health status ${resp.status()}`);
            const body = await resp.text();
            if (!body.includes('UP')) throw new Error('Health status is not UP');
        });

        await testStep('6.2 Robots.txt', async () => {
            const resp = await desktopPage.goto(`${BASE_URL}/robots.txt`, { timeout: 15000 });
            if (resp.status() !== 200) throw new Error(`Robots.txt status ${resp.status()}`);
        });

        await testStep('6.3 Sitemap.xml', async () => {
            const resp = await desktopPage.goto(`${BASE_URL}/sitemap.xml`, { timeout: 30000 });
            if (resp.status() !== 200) throw new Error(`Sitemap.xml status ${resp.status()}`);
            const body = await resp.text();
            if (!body.includes('<urlset') || !body.includes('<loc>')) throw new Error('Sitemap.xml is empty or invalid');
        });

        await desktopContext.close();
        await mobileContext.close();
    } finally {
        await browser.close();
    }

    console.log('\n====================================================');
    console.log(`PLAYWRIGHT TEST SUMMARY: ${passed} PASSED, ${failed} FAILED`);
    console.log('====================================================');
    if (failed > 0) {
        console.log('Failed Tests:');
        errors.forEach(e => console.log(`  - ${e.name}: ${e.error}`));
        process.exit(1);
    }
}

runE2ETests().catch(err => {
    console.error('Fatal test error:', err);
    process.exit(1);
});
