import requests
import time
import re
import concurrent.futures
from urllib.parse import urljoin

PROD_URL = "http://ledampulburada.com"

def run_prod_performance_audit():
    print(f"=== Safe Production Performance & Latency Audit: {PROD_URL} ===", flush=True)
    
    session = requests.Session()
    session.headers.update({
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Accept-Encoding": "gzip, deflate"
    })
    
    # 1. Homepage & Discovery
    t0 = time.perf_counter()
    resp = session.get(PROD_URL, timeout=15)
    lat_home = (time.perf_counter() - t0) * 1000.0
    print(f"\n1. Homepage GET: HTTP {resp.status_code} | Latency: {lat_home:.1f} ms | Size: {len(resp.content)/1024:.1f} KB", flush=True)
    
    # Extract internal links from homepage
    html = resp.text
    raw_links = set(re.findall(r'href=["\'](/[^"\'\s>]+)["\']', html))
    
    categories = [l for l in raw_links if l.startswith("/c/pc/") or "/category/" in l]
    products = [l for l in raw_links if l.startswith("/p/") or "/products/" in l]
    info_pages = [l for l in raw_links if l.startswith("/info/") or "/i/" in l]
    bundles = [l for l in raw_links if l.startswith("/bundles/")]
    
    print(f"\nDiscovered on Live Homepage:")
    print(f"  - Category Links: {len(categories)}")
    print(f"  - Product Links:  {len(products)}")
    print(f"  - Info/Static:    {len(info_pages)}")
    print(f"  - Bundles:        {len(bundles)}")
    
    # Pick target URLs for safety audit
    targets = [PROD_URL]
    if categories:
        targets.append(urljoin(PROD_URL, categories[0]))
    if products:
        targets.append(urljoin(PROD_URL, products[0]))
    if len(products) > 1:
        targets.append(urljoin(PROD_URL, products[1]))
    if info_pages:
        targets.append(urljoin(PROD_URL, info_pages[0]))
    if bundles:
        targets.append(urljoin(PROD_URL, bundles[0]))

    print("\n--- Single-Request Latency Profile ---", flush=True)
    url_results = {}
    for url in targets:
        t_start = time.perf_counter()
        try:
            r = session.get(url, timeout=15)
            d_ms = (time.perf_counter() - t_start) * 1000.0
            url_results[url] = {"status": r.status_code, "lat_ms": d_ms, "size_kb": len(r.content)/1024.0, "headers": dict(r.headers)}
            print(f"  {r.status_code} | {d_ms:6.1f} ms | {len(r.content)/1024:6.1f} KB -> {url}", flush=True)
        except Exception as e:
            print(f"  ERR | {url} -> {e}", flush=True)

    # 2. Safe Light Concurrency Test (3 Concurrent Users, 10 seconds)
    print("\n--- Safe Baseline Concurrency (3 Concurrent Users, 15 seconds) ---", flush=True)
    total_reqs = 0
    latencies = []
    statuses = {}
    stop_time = time.time() + 15
    
    def worker(worker_id):
        nonlocal total_reqs
        s = requests.Session()
        s.headers.update({"User-Agent": f"EImece-ProdAudit/1.0 (Worker-{worker_id})"})
        while time.time() < stop_time:
            target = targets[total_reqs % len(targets)]
            t_req = time.perf_counter()
            try:
                r = s.get(target, timeout=10)
                dur = (time.perf_counter() - t_req) * 1000.0
                total_reqs += 1
                latencies.append(dur)
                statuses[r.status_code] = statuses.get(r.status_code, 0) + 1
            except Exception:
                statuses["ERR"] = statuses.get("ERR", 0) + 1
            time.sleep(0.1) # Safe throttle

    start_concurrency = time.perf_counter()
    with concurrent.futures.ThreadPoolExecutor(max_workers=3) as executor:
        futures = [executor.submit(worker, i) for i in range(3)]
        concurrent.futures.wait(futures)
    
    wall_clock = time.perf_counter() - start_concurrency
    latencies.sort()
    
    def pct(p):
        if not latencies: return 0.0
        idx = min(int(len(latencies) * p / 100.0), len(latencies) - 1)
        return latencies[idx]

    print(f"\n=======================================================", flush=True)
    print(f"        PRODUCTION PERFORMANCE SUMMARY: {PROD_URL}     ", flush=True)
    print(f"=======================================================", flush=True)
    print(f"Total Requests:      {total_reqs}", flush=True)
    print(f"Duration:            {wall_clock:.2f} s", flush=True)
    print(f"Throughput (RPS):    {total_reqs/wall_clock:.2f} req/s", flush=True)
    print(f"HTTP Status Codes:   {statuses}", flush=True)
    if latencies:
        print(f"Latency Min:         {min(latencies):.1f} ms", flush=True)
        print(f"Latency Avg:         {sum(latencies)/len(latencies):.1f} ms", flush=True)
        print(f"Latency Median (p50):{pct(50):.1f} ms", flush=True)
        print(f"Latency p90:         {pct(90):.1f} ms", flush=True)
        print(f"Latency p95:         {pct(95):.1f} ms", flush=True)
        print(f"Latency Max:         {max(latencies):.1f} ms", flush=True)

if __name__ == "__main__":
    run_prod_performance_audit()
