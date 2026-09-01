import re
import time
from urllib.parse import urlparse
from config import BASE_URL, CUSTOMER_USER, CUSTOMER_PASS

def extract_anti_forgery_token(html):
    m = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', html)
    if not m:
        m = re.search(r'value="([^"]+)"[^>]*name="__RequestVerificationToken"', html)
    return m.group(1) if m else None

class UserScenarios:
    def __init__(self, base_url=BASE_URL):
        self.base_url = base_url.rstrip("/")

    def scenario_anonymous_step(self, session, step_index):
        steps = [
            "/",
            "/c/pc/elektronik-9a8c0j1b",
            "/p/kulaklik--ses/copilotmicrosoft-5i0j0j0j4h1b",
            "/products/search?search=termos",
            "/p/mutfak/lumina-kitchen-termos-mug-350ml-112-3f1b3f0j4h1b",
            "/info/aboutus"
        ]
        path = steps[step_index % len(steps)]
        url = f"{self.base_url}{path}"
        t0 = time.perf_counter()
        try:
            r = session.get(url, timeout=15, allow_redirects=True)
            lat = (time.perf_counter() - t0) * 1000.0
            return r.status_code, lat, len(r.content), ""
        except Exception as e:
            lat = (time.perf_counter() - t0) * 1000.0
            return 0, lat, 0, str(e)

    def scenario_customer_session(self, session):
        login_get_url = f"{self.base_url}/account/login"
        t0 = time.perf_counter()
        try:
            r_get = session.get(login_get_url, timeout=15)
            token = extract_anti_forgery_token(r_get.text)
            
            form_data = {
                "Email": CUSTOMER_USER,
                "Password": CUSTOMER_PASS,
                "RememberMe": "false",
                "__RequestVerificationToken": token or ""
            }
            r_post = session.post(login_get_url, data=form_data, timeout=15, allow_redirects=True)
            r_cat = session.get(f"{self.base_url}/c/pc/moda--giyim-1b8c0j1b", timeout=15)
            r_prod = session.get(f"{self.base_url}/p/kulaklik--ses/copilotmicrosoft-5i0j0j0j4h1b", timeout=15)
            r_cart = session.get(f"{self.base_url}/payment/shoppingcart", timeout=15)
            r_acc = session.get(f"{self.base_url}/Customers/Home/Index", timeout=15)
            
            total_lat = (time.perf_counter() - t0) * 1000.0
            total_sz = len(r_post.content) + len(r_cat.content) + len(r_prod.content) + len(r_cart.content) + len(r_acc.content)
            status = 200 if r_acc.status_code == 200 else r_acc.status_code
            return status, total_lat, total_sz, ""
        except Exception as e:
            total_lat = (time.perf_counter() - t0) * 1000.0
            return 0, total_lat, 0, str(e)
