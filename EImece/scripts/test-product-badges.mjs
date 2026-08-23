import http from 'http';

function get(url) {
    return new Promise((resolve, reject) => {
        http.get('http://localhost:81' + url, res => {
            let data = '';
            res.on('data', chunk => data += chunk);
            res.on('end', () => resolve({ status: res.statusCode, html: data }));
        }).on('error', reject);
    });
}

async function run() {
    console.log('Testing page 1 (lumina-kitchen-wireless-bluetooth-kulaklik-pro-121)...');
    const p1 = await get('/p/telefon-aksesuarlari/lumina-kitchen-wireless-bluetooth-kulaklik-pro-121-7e7e2d5i4h1b');
    console.log('Page 1 status:', p1.status);
    
    // Look for all product cards in Page 1
    const cards = p1.html.match(/<div class="product-card">[\s\S]*?<\/div>\s*<\/div>\s*<\/div>/g) || [];
    console.log(`Found ${cards.length} product-card elements in Page 1:`);
    cards.forEach((card, idx) => {
        const hasSale = card.includes('label-offer bg-red');
        const titleMatch = card.match(/<h3[^>]*><a[^>]*>(.*?)<\/a><\/h3>/);
        const title = titleMatch ? titleMatch[1] : 'Unknown';
        console.log(`Product card #${idx + 1}: "${title}" - Has Sale Badge: ${hasSale}`);
    });

    console.log('\nTesting page 2 (kitap-kosesi-sert-kapakli-defter-a5-143)...');
    const p2 = await get('/p/kirtasiye/kitap-kosesi-sert-kapakli-defter-a5-143-8c2d7e5i4h1b');
    console.log('Page 2 status:', p2.status);
    const statusBadgeMatch = p2.html.match(/class="[^"]*product-status-badge[^"]*">([^<]+)<\/span>/);
    console.log('Page 2 Product Status Badge:', statusBadgeMatch ? statusBadgeMatch[1].trim() : 'None');
    const discountBadgeMatch = p2.html.match(/class="[^"]*product-discount-badge[^"]*">([^<]+)<\/span>/);
    console.log('Page 2 Product Discount Badge:', discountBadgeMatch ? discountBadgeMatch[1].trim() : 'None');
}

run().catch(err => console.error(err));
