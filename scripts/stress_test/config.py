import os
import sys

BASE_URL = "http://127.0.0.1:81"
HOST_HEADER = "localhost:81"

# Database Connection (Safe read-only DMV queries)
SQL_CONNECTION_STRING = "Server=YUCE\\SQLEXPRESS;Database=yuva8905_yuvadan;Integrated Security=True;TrustServerCertificate=True;"

# Test User Accounts (From existing test fixtures)
CUSTOMER_USER = "seeduser00004@eimece.test"
CUSTOMER_PASS = "g7.nm30Z"

# Target Endpoints for Benchmark
CORE_ENDPOINTS = {
    "Homepage": "/",
    "Category_Elektronik": "/c/pc/elektronik-9a8c0j1b",
    "Category_Moda": "/c/pc/moda--giyim-1b8c0j1b",
    "Product_1": "/p/kulaklik--ses/copilotmicrosoft-5i0j0j0j4h1b",
    "Product_2": "/p/mutfak/lumina-kitchen-termos-mug-350ml-112-3f1b3f0j4h1b",
    "Search_Termos": "/products/search?search=termos",
    "Info_AboutUs": "/info/aboutus",
    "Info_Delivery": "/info/deliveryinfo",
    "Cargo_Tracking": "/payment/cargotracking",
    "Cart_View": "/payment/shoppingcart",
    "Customer_Login_Page": "/account/login",
    "Bundle_CSS_Crizal": "/bundles/designs/crizal/vendor/css",
    "Bundle_JS_Eimece": "/bundles/eimeceScripts",
    "Health_Live": "/health"
}

# Safety Limits / Circuit Breakers
MAX_ERROR_RATE_PERCENT = 5.0
MAX_LATENCY_P95_MS = 5000.0
MAX_W3WP_MEMORY_MB = 1800.0
