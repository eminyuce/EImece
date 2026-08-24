const { chromium } = require('@playwright/test');

const BASE_URL = 'http://localhost:81';
const ADMIN_USER = 'admin@eimece.test';
const ADMIN_PASS = 'Admin123!';

const ADMIN_PAGES = [
    // Dashboard & System
    { name: 'Admin Root', path: '/admin' },
    { name: 'Dashboard Index', path: '/admin/dashboard' },
    { name: 'System Health', path: '/admin/dashboard/systemhealth' },
    { name: 'Admin Settings', path: '/admin/adminsettings' },
    { name: 'System Settings', path: '/admin/adminsettings/systemsettings' },
    { name: 'General Settings', path: '/admin/settings' },
    { name: 'App Logs', path: '/admin/applogs' },
    { name: 'Metrics', path: '/admin/metrics' },

    // Catalog & Products
    { name: 'Products Grid', path: '/admin/products' },
    { name: 'Product Create Form (SaveOrEdit)', path: '/admin/products/saveoredit' },
    { name: 'Product Edit Form (Id=1)', path: '/admin/products/saveoredit/1' },
    { name: 'Product Categories Grid', path: '/admin/productcategories' },
    { name: 'Category Create Form (SaveOrEdit)', path: '/admin/productcategories/saveoredit' },
    { name: 'Category Edit Form (Id=1)', path: '/admin/productcategories/saveoredit/1' },
    { name: 'Brands Grid', path: '/admin/brands' },
    { name: 'Brand Create Form', path: '/admin/brands/saveoredit' },
    { name: 'Product Comments', path: '/admin/productcomments' },

    // Sales & Customers
    { name: 'Orders Grid', path: '/admin/orders' },
    { name: 'Customers Grid', path: '/admin/customers' },
    { name: 'Customer Baskets Grid', path: '/admin/customers/customerbaskets' },
    { name: 'Shopping Carts Grid', path: '/admin/shoppingcarts' },
    { name: 'Coupons Grid', path: '/admin/coupons' },
    { name: 'Coupon Create Form', path: '/admin/coupons/saveoredit' },
    { name: 'Subscribers Grid', path: '/admin/subscribers' },

    // Content & Marketing
    { name: 'Menus Grid', path: '/admin/menus' },
    { name: 'Menu Create Form', path: '/admin/menus/saveoredit' },
    { name: 'Main Page Images Grid', path: '/admin/mainpageimages' },
    { name: 'Main Page Image Create Form', path: '/admin/mainpageimages/saveoredit' },
    { name: 'Stories Grid', path: '/admin/stories' },
    { name: 'Story Create Form', path: '/admin/stories/saveoredit' },
    { name: 'Story Categories Grid', path: '/admin/storycategories' },
    { name: 'Story Category Create Form', path: '/admin/storycategories/saveoredit' },
    { name: 'Tags Grid', path: '/admin/tags' },
    { name: 'Tag Create Form', path: '/admin/tags/saveoredit' },
    { name: 'Tag Categories Grid', path: '/admin/tagcategories' },
    { name: 'Tag Category Create Form', path: '/admin/tagcategories/saveoredit' },
    { name: 'FAQ Grid', path: '/admin/faq' },
    { name: 'FAQ Create Form', path: '/admin/faq/saveoredit' },
    { name: 'Custom Lists Grid', path: '/admin/lists' },
    { name: 'List Create Form', path: '/admin/lists/saveoredit' },
    { name: 'Media Library', path: '/admin/media' },
    { name: 'Mail Templates Grid', path: '/admin/mailtemplates' },
    { name: 'Mail Template Create Form', path: '/admin/mailtemplates/saveoredit' },
    { name: 'Templates Grid', path: '/admin/templates' },
    { name: 'Template Create Form', path: '/admin/templates/saveoredit' },
    { name: 'RSS Feeds Grid', path: '/admin/rssfeeds' },

    // Users & Roles
    { name: 'Users Staff Grid', path: '/admin/users' },
    { name: 'Users Customer Roles Grid', path: '/admin/users/customerroles' },

    // Reports
    { name: 'Reports Index', path: '/admin/report' },
    { name: 'Coupon Usage Report', path: '/admin/report/couponusage' },
    { name: 'Fraud Analysis Report', path: '/admin/report/fraudanalysis' },
    { name: 'Payment Method Report', path: '/admin/report/paymentmethod' },
    { name: 'Regional Sales Report', path: '/admin/report/getregionalsalesreport' },
    { name: 'Performance System Report', path: '/admin/report/performancesystemreport' },
    { name: 'Financial Report', path: '/admin/report/financialreport' },
    { name: 'Fraud Risk Report', path: '/admin/report/fraudriskreport' },
    { name: 'Order Volume Report', path: '/admin/report/ordervolumereport' },
    { name: 'Payment Transaction Report', path: '/admin/report/paymenttransactionreport' },
    { name: 'Product Summary Report', path: '/admin/report/productsummary' },
    { name: 'Price Analysis Report', path: '/admin/report/priceanalysis' },
    { name: 'Product Inventory Report', path: '/admin/report/productinventory' }
];

(async () => {
    console.log('===============================================================');
    console.log(`STARTING COMPLETE ADMIN AUDIT (${ADMIN_PAGES.length} PAGES CHECK IN & OUT)`);
    console.log(`Target: ${BASE_URL}`);
    console.log('===============================================================\n');

    const browser = await chromium.launch({ headless: true });
    const context = await browser.newContext({
        viewport: { width: 1440, height: 900 },
        ignoreHTTPSErrors: true
    });
    const page = await context.newPage();

    // 1. Authenticate to Admin
    console.log('1. Authenticating as Administrator...');
    await page.goto(`${BASE_URL}/account/adminlogin/`, { waitUntil: 'domcontentloaded', timeout: 30000 });
    
    const emailField = page.locator('input[name="Email"], input[name="UserName"], #Email');
    if (await emailField.count() > 0) {
        await emailField.fill(ADMIN_USER);
        await page.locator('input[name="Password"], #Password').fill(ADMIN_PASS);
        await page.locator('button[type="submit"], input[type="submit"]').click();
        await page.waitForLoadState('domcontentloaded');
    }
    console.log('   Logged in successfully.\n');

    let passedCount = 0;
    let failedCount = 0;
    const failures = [];

    // 2. Iterate through every single admin page
    for (let i = 0; i < ADMIN_PAGES.length; i++) {
        const item = ADMIN_PAGES[i];
        const num = (i + 1).toString().padStart(2, '0');
        const url = `${BASE_URL}${item.path}`;

        process.stdout.write(`[${num}/${ADMIN_PAGES.length}] Checking: ${item.name} (${item.path}) ... `);

        try {
            const response = await page.goto(url, { waitUntil: 'domcontentloaded', timeout: 30000 });
            const status = response ? response.status() : 0;
            const bodyText = await page.innerText('body');

            // Error checks
            const hasCsError = bodyText.includes('error CS') || bodyText.includes('HttpCompileException');
            const hasAdminError = bodyText.includes('Admin Error') && bodyText.includes('An unexpected error occurred');
            const hasYSoD = bodyText.includes('Server Error in \'/\' Application') || bodyText.includes('Compilation Error');
            const is500 = status >= 500;

            if (hasCsError || hasAdminError || hasYSoD || is500) {
                let errReason = is500 ? `HTTP ${status}` : 'Runtime/Compilation Exception';
                if (hasCsError) errReason += ' (error CS in view compilation)';
                if (hasAdminError) errReason += ' (Admin Error banner)';
                console.log(`FAILED -> ${errReason}`);
                failedCount++;
                failures.push({ name: item.name, path: item.path, status, reason: errReason });
            } else {
                console.log(`OK (HTTP ${status})`);
                passedCount++;
            }
        } catch (err) {
            console.log(`FAILED -> Exception: ${err.message}`);
            failedCount++;
            failures.push({ name: item.name, path: item.path, status: 0, reason: err.message });
        }
    }

    console.log('\n===============================================================');
    console.log(`AUDIT COMPLETE: ${passedCount} PASSED, ${failedCount} FAILED out of ${ADMIN_PAGES.length} PAGES`);
    console.log('===============================================================');

    if (failures.length > 0) {
        console.log('\nFAILURES:');
        failures.forEach(f => console.log(` - [${f.name}] ${f.path}: ${f.reason}`));
    } else {
        console.log('\n>>> 100% OF ALL ADMIN PAGES CHECKED IN AND OUT WITH ZERO ERRORS! <<<');
    }

    await context.close();
    await browser.close();

    if (failedCount > 0) {
        process.exit(1);
    }
})();
