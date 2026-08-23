# -*- coding: utf-8 -*-
"""Generate SeedDummyData_EN_Real.sql with real English (Lang=2)"""
from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).parent))
import _gen_realistic_seed as base

# Override with real English
BRANDS_EN = [
    ("Nordline", "Scandinavian style furniture and home decor.", "furniture,home,decor"),
    ("Atlas Textile", "Everyday clothing and basic textile products.", "textile,clothing,cotton"),
    ("Sportiva", "Running, fitness and outdoor equipment.", "sports,fitness,outdoor"),
    ("Lumina Kitchen", "Kitchen tools and small appliances.", "kitchen,home appliance"),
    ("Beauté Lab", "Skincare and personal care products.", "cosmetics,skincare"),
    ("TechPlus", "Electronic accessories and computer peripherals.", "electronics,accessories"),
    ("Casa Bella", "Home textiles, bedding and bathroom products.", "home textile,bedding"),
    ("MiniNest", "Baby and children's products.", "baby,children"),
    ("Meridian Outdoor", "Camping, hiking and outdoor sports.", "camping,outdoor"),
    ("Leather Workshop", "Leather bags, belts and wallets.", "leather,accessories"),
    ("AquaPure", "Water filtration and healthy living products.", "water,health"),
    ("Book Corner", "Books, stationery and hobby products.", "books,stationery"),
    ("UrbanWear", "Street style and everyday fashion.", "fashion,streetwear"),
    ("ChefPro", "Professional kitchen equipment.", "kitchen,professional"),
    ("GreenLeaf", "Organic food and natural products.", "organic,natural"),
    ("SoundWave", "Headphones, speakers and audio systems.", "audio,headphones"),
    ("FitLife", "Sportswear and active lifestyle.", "sportswear,active"),
    ("HomeGlow", "Lighting and home decoration.", "lighting,decor"),
    ("PetFriend", "Pet care and accessories.", "pet,animal"),
    ("Voyage Pack", "Luggage, backpacks and travel accessories.", "travel,luggage"),
]

CATEGORIES_EN = [
    ("Electronics", "Phones, computers and electronic accessories.", None, None),
    ("Fashion & Apparel", "Women, men and unisex clothing.", None, None),
    ("Home & Living", "Furniture, decoration and home textiles.", None, 10.0),
    ("Sports & Outdoor", "Sportswear, equipment and camping gear.", None, None),
    ("Cosmetics & Care", "Skincare, makeup and personal care.", None, None),
    ("Baby & Kids", "Baby care and children's products.", None, None),
    ("Books & Hobby", "Books, stationery and hobby supplies.", None, None),
    ("Kitchen", "Kitchen tools and small appliances.", None, None),
    ("Headphones & Audio", "Wireless headphones and speakers.", 0, None),
    ("Phone Accessories", "Cases, chargers and screen protectors.", 0, None),
    ("Women's Clothing", "Dresses, blouses and outerwear.", 1, None),
    ("Men's Clothing", "Shirts, pants and jackets.", 1, None),
    ("Shoes", "Sports and casual shoes.", 1, None),
    ("Living Room", "Sofas, armchairs and coffee tables.", 2, None),
    ("Bedroom", "Bedding, pillows and duvets.", 2, None),
    ("Lighting", "Table lamps and chandeliers.", 2, None),
    ("Running & Fitness", "Running shoes and fitness equipment.", 3, None),
    ("Camping & Nature", "Tents, mats and outdoor bags.", 3, None),
    ("Skin Care", "Cleansers, serums and moisturizers.", 4, None),
    ("Hair Care", "Shampoo, conditioner and serums.", 4, None),
    ("Baby Care", "Diapers and care sets.", 5, None),
    ("Toys", "Educational and fun toys.", 5, None),
    ("Fiction & Literature", "Contemporary and classic novels.", 6, None),
    ("Stationery", "Notebooks, pens and office supplies.", 6, None),
    ("Cookware", "Pots, pans and kitchen sets.", 7, None),
]

PRODUCTS_EN = [
    ("{b} Wireless Bluetooth Headset Pro", 16, 8, 1299, 800, "Black,White,Navy", None, "Wireless headset with active noise cancellation.", "<p>Long battery life, fast charging and comfortable fit. Ideal for daily commute and office use.</p>"),
    ("{b} USB-C Fast Charger 65W", 6, 9, 449, 200, "White,Black", None, "Compact GaN fast charger.", "<p>Charge your phone, tablet and laptop with one adapter. Overheat protection included.</p>"),
    ("{b} Silicone Phone Case", 6, 9, 149, 80, "Transparent,Black,Pink,Blue", None, "Shock-resistant thin silicone case.", "<p>Protects sensitive edges, compatible with wireless charging.</p>"),
    ("{b} Women's Cotton Basic T-Shirt", 2, 10, 279, 120, "White,Black,Gray,Beige", "XS,S,M,L,XL", "Breathable cotton everyday t-shirt.", "<p>100% cotton, soft texture. Machine washable.</p>"),
    ("{b} Men's Slim Fit Chino Pants", 13, 11, 599, 250, "Beige,Navy,Khaki,Black", "28,30,32,34,36", "Slim fit chinos for office and casual.", "<p>Stretch fabric, wrinkle-free. Seasonal collection.</p>"),
    ("{b} Unisex Running Shoes AirFlex", 17, 12, 1899, 600, "Black,Gray,Blue,Orange", "36,37,38,39,40,41,42,43,44", "Lightweight running shoes.", "<p>Breathable mesh upper, shock-absorbing sole. Optimized for road running.</p>"),
    ("{b} Women's Trench Coat", 13, 10, 1499, 500, "Beige,Black,Khaki", "S,M,L,XL", "Classic water-repellent trench coat.", "<p>Ideal for seasonal transitions. Lined, belted cut.</p>"),
    ("{b} Men's Oxford Shirt", 2, 11, 449, 150, "White,Light Blue,Pink", "S,M,L,XL,XXL", "Classic oxford shirt.", "<p>For work and casual combinations. Easy-iron cotton blend.</p>"),
    ("{b} Corner Sofa Set 3+1", 1, 13, 24999, 8000, "Anthracite,Cream,Green", None, "Spacious L-shaped corner sofa set.", "<p>High-density foam, removable covers. Adds spacious look to your living room.</p>"),
    ("{b} Oak Coffee Table 90cm", 1, 13, 3299, 1000, "Natural Oak,Walnut", None, "Solid wood look coffee table.", "<p>Durable surface, easy to clean. Minimal Scandinavian lines.</p>"),
    ("{b} Cotton Sateen Duvet Set", 7, 14, 899, 400, "White,Gray,Powder", "Single,Double,King", "200 TC cotton sateen duvet.", "<p>Soft texture, colorfast. Pillowcase included.</p>"),
    ("{b} LED Desk Lamp with Dimmer", 18, 15, 649, 250, "Black,White,Brass", None, "Touch dimmable LED desk lamp.", "<p>Three color temperatures, eye-friendly light. USB charging port.</p>"),
    ("{b} Yoga Mat 6mm", 17, 16, 349, 150, "Purple,Blue,Pink,Black", None, "Non-slip yoga and pilates mat.", "<p>Carrying strap included. Latex-free, easy to wipe.</p>"),
    ("{b} Dumbbell Set 2x5kg", 3, 16, 429, 200, "Black", None, "Neoprene coated dumbbell pair.", "<p>Non-slip grip for home workouts. Floor protectors.</p>"),
    ("{b} 2-Person Camping Tent", 9, 17, 2199, 700, "Green,Orange", None, "Quick-setup waterproof tent.", "<p>2000mm waterproof fabric, mosquito net. Carrying bag included.</p>"),
    ("{b} Trekking Backpack 40L", 9, 17, 1599, 500, "Anthracite,Khaki", None, "Trekking backpack with waist and chest straps.", "<p>Rain cover, hydration compatible. Breathable back panel.</p>"),
    ("{b} Vitamin C Brightening Serum 30ml", 5, 18, 389, 150, None, None, "Anti-spot vitamin C serum.", "<p>For morning routine. Use with SPF. Dermatologically tested.</p>"),
    ("{b} Moisturizing Face Cream 50ml", 5, 18, 299, 120, None, None, "24-hour moisturizing face cream.", "<p>Light formula for oily and combination skin. Paraben-free.</p>"),
    ("{b} Repair Shampoo 400ml", 5, 19, 189, 80, None, None, "Repair shampoo for damaged hair.", "<p>Keratin and argan oil complex. Suitable for daily use.</p>"),
    ("{b} Baby Care Set 5-Piece", 8, 20, 449, 150, None, None, "Baby care set for sensitive skin.", "<p>Shampoo, lotion, oil, cream and wipes. Hypoallergenic.</p>"),
    ("{b} Educational Wooden Blocks 48-Piece", 8, 21, 329, 100, "Colorful", None, "48-piece wooden block set.", "<p>Water-based paint, no sharp edges. Suitable for 3+ years.</p>"),
    ("{b} Contemporary Novel - Selection #{n}", 12, 22, 149, 80, None, None, "Curated contemporary literature.", "<p>Hardcover, local print. Top rated by readers.</p>"),
    ("{b} Hardcover Notebook A5", 12, 23, 89, 40, "Kraft,Black,Navy", None, "Dotted A5 notebook.", "<p>120 pages, 90gsm. Elastic band and bookmark.</p>"),
    ("{b} Granite Pan 28cm", 14, 24, 549, 200, "Black", None, "Non-stick granite coated pan.", "<p>Induction compatible. Oven-safe handle. PFOA-free.</p>"),
    ("{b} Stainless Steel Pot Set 6-Piece", 4, 24, 2499, 800, "Steel", None, "Stainless steel pot set.", "<p>3 pots with lids. Dishwasher safe.</p>"),
    ("{b} Glass Water Bottle 750ml", 11, 7, 249, 80, "Transparent,Smoked,Blue", None, "BPA-free glass water bottle.", "<p>Silicone sleeve, leak-proof cap. For office and sports.</p>"),
    ("{b} Organic Olive Oil 1L", 15, 7, 329, 100, None, None, "Cold-pressed organic olive oil.", "<p>Single harvest, dark glass bottle. Tasting notes on label.</p>"),
    ("{b} Cabin Suitcase 55cm", 20, 1, 1899, 600, "Black,Navy,Burgundy", None, "Lightweight hard-shell cabin suitcase.", "<p>360° wheels, TSA lock. Interior organizer compartments.</p>"),
    ("{b} Leather Shoulder Bag", 10, 1, 1299, 400, "Tan,Black,Burgundy", None, "Handcrafted look leather shoulder bag.", "<p>Adjustable strap, zip inner pocket. Daily use.</p>"),
    ("{b} Pet Food Bowl Set", 19, 2, 199, 80, "Gray,Pink,Blue", None, "Stainless steel pet bowl set.", "<p>Non-slip base, dishwasher safe.</p>"),
    ("{b} Smart LED Bulb 9W", 18, 15, 179, 60, "White", None, "App-controlled color changing bulb.", "<p>Voice assistant compatible. Timer and scene support.</p>"),
    ("{b} Thermos Mug 350ml", 4, 7, 279, 100, "Black,White,Red", None, "Stainless steel vacuum thermos mug.", "<p>Keeps hot 6h / cold 12h. Fits car holder.</p>"),
    ("{b} High-Waist Fitness Leggings", 17, 16, 449, 150, "Black,Navy,Burgundy", "XS,S,M,L", "Shaping high-waist sports leggings.", "<p>Moisture-wicking fabric, pocket detail. For training and daily use.</p>"),
    ("{b} Men's Fleece Jacket", 3, 11, 899, 300, "Anthracite,Navy,Khaki", "S,M,L,XL,XXL", "Lightweight fleece zip jacket.", "<p>As a cold weather layer or standalone.</p>"),
    ("{b} Bamboo Cutting Board Set", 14, 24, 349, 120, "Natural", None, "3-piece bamboo cutting board set.", "<p>Antibacterial natural surface. Hanging hole.</p>"),
    ("{b} Sunscreen SPF50 50ml", 5, 18, 259, 80, None, None, "Light texture sunscreen for face.", "<p>Leaves no white cast. Suitable under makeup.</p>"),
    ("{b} Baby Bodysuit 3-Pack", 8, 20, 249, 80, "White,Gray,Yellow", "0-3M,3-6M,6-9M,9-12M", "Organic cotton baby bodysuit set.", "<p>Snap buttons, tagless. For sensitive skin.</p>"),
    ("{b} Mini Bluetooth Speaker", 16, 8, 799, 300, "Black,Blue,Red", None, "Portable waterproof speaker.", "<p>12h battery, IPX7. Stereo pairing.</p>"),
    ("{b} Aluminum Laptop Stand", 6, 0, 549, 200, "Silver,Space Gray", None, "Adjustable aluminum laptop stand.", "<p>Ergonomic angle, cable management. 10-16 inch compatible.</p>"),
    ("{b} Memory Foam Pillow 2-Pack", 7, 14, 699, 250, "White", "Standard", "Visco memory foam pillow pair.", "<p>Neck support, removable cover. Anti-allergic.</p>"),
]

TAGS_EN = ["Daily use","Office","Sports","Travel","Gift idea","Cotton","Leather","Polyester","Organic","Metal","Summer","Winter","Spring","Autumn","Seasonal","Women","Men","Unisex","Children","Baby","Waterproof","Breathable","Fast shipping","On sale","New season","Minimal","Classic","Modern","Vintage","Scandinavian","Campaign","Bestseller","Editor's choice","Limited stock","Local production","Eco-friendly","BPA free","Machine washable","Induction compatible","TSA lock"]

STORY_CATEGORIES_EN = [
    ("Style Guide", "Fashion and style tips.", "T1"),
    ("Home Decor", "Inspiration for living spaces.", "T2"),
    ("Healthy Living", "Nutrition and wellness articles.", "T3"),
    ("Technology", "Gadget reviews and tips.", "T4"),
    ("Travel", "Route suggestions and packing guides.", "T5"),
    ("Parenting", "Baby and child care.", "T6"),
]

STORIES_EN = [
    ("2024 Autumn Outfit Ideas", "Layered autumn styles.", "Selin Arslan", "<p>How to combine trench coats, fleece and chino pants for seasonal transitions? Our editors' favorite combinations.</p>"),
    ("Furniture Tips for Small Living Rooms", "Spaciousness in tight spaces.", "Can Yilmaz", "<p>Make your living room feel larger with the right coffee table and lighting instead of a corner sofa. Examples from Nordline collection.</p>"),
    ("How to Choose Running Shoes?", "Pronation, sole and fit.", "Emre Demir", "<p>Short guide to choosing the right running shoes based on your weekly mileage and foot type.</p>"),
    ("Skincare Routine: 5 Steps", "From cleansing to moisturizing.", "Elif Kaya", "<p>A simple yet effective routine for morning and evening. Why order of serum and sunscreen matters?</p>"),
    ("Before Your Camping Holiday", "Checklist.", "Burak Sahin", "<p>Tent, mat, backpack and kitchen set: what not to forget for your first camping trip.</p>"),
    ("Using Granite Pans in the Kitchen", "Care and cooking tips.", "Ayse Celik", "<p>Don't use metal spatulas to extend non-stick life. Correct heat settings.</p>"),
    ("Baby Room Preparation List", "Essentials for first 3 months.", "Zeynep Ozturk", "<p>From bodysuit sets to care products, a realistic shopping list.</p>"),
    ("When Buying Bluetooth Headphones", "ANC, battery and compatibility.", "Kerem Aydin", "<p>Does active noise cancellation really work? Office and travel scenarios.</p>"),
    ("Reading Organic Product Labels", "What do certifications mean?", "Defne Kara", "<p>Correctly interpret organic, cold-pressed and additive statements.</p>"),
    ("Home Office Lighting", "Reduce eye strain.", "Onur Yildiz", "<p>Practical tips on desk lamp color, brightness and screen position.</p>"),
    ("What to Consider When Choosing Luggage", "Cabin rules and wheels.", "Melis Dogan", "<p>Airline cabin dimensions, TSA lock and weight balance.</p>"),
    ("Fit Check When Buying Sports Tights", "High waist and fabric.", "Irem Kilic", "<p>How to tell if sports leggings are non-slip and moisture-wicking?</p>"),
    ("A Small Corner with Books", "Creating a reading space.", "Cem Polat", "<p>Mini library at home with shelves, lighting and a cozy armchair.</p>"),
    ("Layering Winter Jackets", "Fleece + outer layer.", "Hakan Kurt", "<p>How to stay warm with breathable layers in cold weather.</p>"),
    ("Pet-Friendly Home Layout", "Feeding area and safety.", "Buse Aksoy", "<p>Safe space for your pet with non-slip bowls and cable management.</p>"),
]

MENUS_EN = [
    ("Home", "<p>Welcome to EImece showcase.</p>", "home-index", None, True, None, 0),
    ("Corporate", "<p>About us and company information.</p>", "pages-index", None, True, "T1", 0),
    ("About Us", "<p>EImece is an online store bringing selected brands together under one roof.</p>", "info-aboutus", None, False, None, 0),
    ("Contact", "<h2>Contact</h2><p>Customer service and store contact information.</p><p>For orders, returns and product questions, use the form below or write to <strong>info@eimece.test</strong>.</p><p>Working hours: Weekdays 09:00–18:00</p>", "pages-index", None, False, "T8", 0),
    ("Shipping & Delivery", "<p>Shipping times, free shipping limit and return process.</p>", "info-deliveryinfo", None, True, None, 0),
    ("FAQ", "<p>FAQ about orders, payment and returns.</p>", "pages-index", None, True, "T2", 0),
    ("Campaigns", "<p>Current discounts and coupons.</p>", "pages-index", None, True, "T1", 0),
    ("Blog", "<p>Style, life and product guides.</p>", "stories-index", None, True, None, 0),
    ("Privacy Policy", "<p>Protection of personal data.</p>", "info-privacypolicy", None, False, None, 0),
    ("Distance Sales Agreement", "<p>Distance sales and consumer rights.</p>", "info-termsandconditions", None, False, None, 0),
    ("Returns & Exchange", "<p>Return and exchange conditions within 14 days.</p>", "pages-index", None, False, "T3", 0),
    ("Our Stores", "<p>Our store locations will be listed here soon.</p><p><em>Admin note:</em> Add the real map link via Admin → Menus.</p>", "pages-index", None, True, "T4", 0),
    ("Theme Examples", "<p>Page theme examples (T1–T8). Each subpage has a main image and gallery.</p>", "pages-index", None, True, "T1", 0),
    ("PT Dummy T1", "<p>This page shows <strong>PageTheme T1</strong> layout. The large image above is the menu main image. The grid below is the menu gallery.</p>", "pages-index", None, False, "T1", 12),
    ("PT Dummy T2", "<p>This page shows <strong>PageTheme T2</strong> layout.</p>", "pages-index", None, False, "T2", 12),
    ("PT Dummy T3", "<p>This page shows <strong>PageTheme T3</strong> layout.</p>", "pages-index", None, False, "T3", 12),
    ("PT Dummy T4", "<p>This page shows <strong>PageTheme T4</strong> layout.</p>", "pages-index", None, False, "T4", 12),
    ("PT Dummy T5", "<p>This page shows <strong>PageTheme T5</strong> layout.</p>", "pages-index", None, False, "T5", 12),
    ("PT Dummy T6", "<p>This page shows <strong>PageTheme T6</strong> layout.</p>", "pages-index", None, False, "T6", 12),
    ("PT Dummy T7", "<p>This page shows <strong>PageTheme T7</strong> (large gallery) layout. At least 12 menu gallery images are added.</p>", "pages-index", None, False, "T7", 12),
    ("PT Dummy T8", "<h2>Contact</h2><p>This page shows <strong>PageTheme T8</strong> contact layout.</p>", "pages-index", None, False, "T8", 12),
]

# Patch base module with English data and generate
base.BRANDS = BRANDS_EN
base.CATEGORIES = CATEGORIES_EN
base.PRODUCTS = PRODUCTS_EN
base.TAGS = TAGS_EN
base.STORY_CATEGORIES = STORY_CATEGORIES_EN
base.STORIES = STORIES_EN
base.MENUS = MENUS_EN
# Keep other lookups as is (they are already English-friendly or neutral)
# Generate to EN file
out_path = Path(__file__).with_name("SeedDummyData_EN_Real.sql")
# Temporarily override OUT
original_out = base.OUT
base.OUT = out_path
# Force English lang in generated SQL header - we will post-process
base.main()
# Post-process to set Lang=2 and keep English content
text = out_path.read_text(encoding="utf-8")
text = text.replace("DECLARE @Lang          INT          = 1;", "DECLARE @Lang          INT          = 2;")
# Keep cleanup as 0 for additive English (we already cleaned)
text = text.replace("DECLARE @CleanupFirst  BIT          = 1;", "DECLARE @CleanupFirst  BIT          = 0;")
# Patch AspNetUsers guard already in base? Ensure it stays
out_path.write_text(text, encoding="utf-8")
print(f"Generated {out_path} with real English")
