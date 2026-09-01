import requests
import re
import json
import time
import concurrent.futures

BASE_URL = "http://127.0.0.1:81"

def get_admin_cache_metrics():
    s = requests.Session()
    s.headers.update({"Host": "localhost:81", "User-Agent": "EImece-AdminMonitor/1.0"})
    
    try:
        login_page = s.get(f"{BASE_URL}/account/adminlogin", timeout=10)
        token_match = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', login_page.text)
        if not token_match:
            return None
        token = token_match.group(1)
        
        s.post(
            f"{BASE_URL}/account/adminlogin",
            data={
                "Email": "hyuce57@gmail.com",
                "Password": "523188Hy",
                "RememberMe": "false",
                "__RequestVerificationToken": token
            },
            timeout=10,
            allow_redirects=True
        )
        
        resp = s.get(f"{BASE_URL}/admin/cache/diagnostics", timeout=10)
        if resp.status_code == 200:
            return resp.json()
    except Exception as e:
        print("Error fetching admin cache diagnostics:", e)
    return None

def run_cache_stress_and_monitor():
    print("=" * 80)
    print("       EIMECE LOCAL CACHE STRESS TEST & ADMIN/CACHE LIVE MONITOR      ")
    print("=" * 80)
    print(f"Target: http://localhost:81/  |  Admin Panel: http://localhost:81/admin/cache/")
    print("=" * 80)

    # 1. Fetch Baseline Metrics Before Load
    print("\n>>> [1/3] Reading Initial Admin Cache Baseline...", flush=True)
    before_diag = get_admin_cache_metrics()
    if before_diag:
        overview = before_diag.get("overview", {})
        combined = overview.get("combined", {})
        data_layer = overview.get("data", {})
        page_layer = overview.get("page", {})
        print(f"  Combined Hit Ratio:  {combined.get('hitRatioPercent', 0):.1f}%")
        print(f"  Data Cache Hits:     {data_layer.get('hits', 0):,}  | Misses: {data_layer.get('misses', 0):,}")
        print(f"  Page/Output Hits:    {page_layer.get('hits', 0):,}  | Misses: {page_layer.get('misses', 0):,}")
    else:
        print("  Could not fetch initial admin cache diagnostics.")

    # 2. Run High-Concurrency Load Test
    concurrency = 25
    duration_sec = 45
    print(f"\n>>> [2/3] Generating Storefront Load ({concurrency} Concurrent Users for {duration_sec}s)...", flush=True)
    
    target_urls = [
        "/",
        "/c/pc/elektronik-9a8c0j1b",
        "/c/pc/moda--giyim-1b8c0j1b",
        "/p/kulaklik--ses/copilotmicrosoft-5i0j0j0j4h1b",
        "/p/mutfak/lumina-kitchen-termos-mug-350ml-112-3f1b3f0j4h1b",
        "/products/search?search=termos",
        "/info/aboutus",
        "/info/deliveryinfo"
    ]

    total_requests = 0
    latencies = []
    statuses = {}
    stop_time = time.time() + duration_sec

    def worker(worker_id):
        nonlocal total_requests
        s = requests.Session()
        s.headers.update({"Host": "localhost:81", "User-Agent": f"EImece-CacheStress/1.0 (Worker-{worker_id})"})
        req_count = 0
        while time.time() < stop_time:
            url = target_urls[req_count % len(target_urls)]
            req_count += 1
            t0 = time.perf_counter()
            try:
                r = s.get(f"{BASE_URL}{url}", timeout=15)
                dur = (time.perf_counter() - t0) * 1000.0
                total_requests += 1
                latencies.append(dur)
                statuses[r.status_code] = statuses.get(r.status_code, 0) + 1
            except Exception:
                statuses["ERR"] = statuses.get("ERR", 0) + 1

    t_start = time.perf_counter()
    with concurrent.futures.ThreadPoolExecutor(max_workers=concurrency) as executor:
        futures = [executor.submit(worker, i) for i in range(concurrency)]
        concurrent.futures.wait(futures)
    wall_time = time.perf_counter() - t_start

    latencies.sort()
    def pct(p):
        if not latencies: return 0.0
        idx = min(int(len(latencies) * p / 100.0), len(latencies) - 1)
        return latencies[idx]

    print(f"\n  Stress Test Execution Finished:")
    print(f"  - Total Requests:    {total_requests:,}")
    print(f"  - Throughput:        {total_requests/wall_time:.2f} req/sec")
    print(f"  - HTTP Status:       {statuses}")
    print(f"  - Min Latency:       {min(latencies):.1f} ms")
    print(f"  - Avg Latency:       {sum(latencies)/len(latencies):.1f} ms")
    print(f"  - Median (p50):      {pct(50):.1f} ms")
    print(f"  - p95 Latency:       {pct(95):.1f} ms")
    print(f"  - p99 Latency:       {pct(99):.1f} ms")

    # 3. Fetch Admin Cache Diagnostics Post-Load
    print("\n>>> [3/3] Reading Post-Test Admin Cache Metrics...", flush=True)
    after_diag = get_admin_cache_metrics()
    if after_diag:
        overview = after_diag.get("overview", {})
        combined = overview.get("combined", {})
        data_layer = overview.get("data", {})
        page_layer = overview.get("page", {})
        
        print("\n================================================================================")
        print("               ADMIN / CACHE POST-TEST EVALUATION RESULTS                       ")
        print("================================================================================")
        print(f"Overall Cache Status:       {combined.get('title', 'Active')}")
        print(f"Combined Hit Ratio:         {combined.get('hitRatioPercent', 0):.2f}%")
        print(f"Total Requests Cached:      {combined.get('totalReads', 0):,}")
        print(f"Total Combined Hits:        {combined.get('hits', 0):,}")
        print(f"Total Combined Misses:      {combined.get('misses', 0):,}")
        print(f"Estimated Time Saved:       {combined.get('savedFormatted', 'N/A')}")
        print(f"Measured Speed Improvement: {combined.get('improvementFormatted', 'N/A')}")
        
        print("\n--- [A] Uygulama Veri Onbellegi (MemoryCacheProvider) ---")
        print(f"  Hits:                     {data_layer.get('hits', 0):,}")
        print(f"  Misses:                   {data_layer.get('misses', 0):,}")
        print(f"  Hit Ratio:                {data_layer.get('hitRatioPercent', 0):.2f}%")
        print(f"  Avg Cached Latency:       {data_layer.get('avgCachedMs', 0):.3f} ms")
        print(f"  Avg Uncached Latency:     {data_layer.get('avgUncachedMs', 0):.2f} ms")
        print(f"  Performance Speedup:      {data_layer.get('improvementFormatted', 'N/A')}")

        print("\n--- [B] Sayfa / Yanit Onbellegi (CustomOutputCache) ---")
        print(f"  Hits:                     {page_layer.get('hits', 0):,}")
        print(f"  Misses:                   {page_layer.get('misses', 0):,}")
        print(f"  Hit Ratio:                {page_layer.get('hitRatioPercent', 0):.2f}%")
        print(f"  Avg Cached Latency:       {page_layer.get('avgCachedMs', 0):.3f} ms")
        print(f"  Avg Uncached Latency:     {page_layer.get('avgUncachedMs', 0):.2f} ms")
        print(f"  Performance Speedup:      {page_layer.get('improvementFormatted', 'N/A')}")

        # Top 10 Hot Cache Entries
        entries = after_diag.get("entries", [])
        if entries:
            print("\n--- Top 10 Hot Cache Entries in Memory ---")
            for idx, e in enumerate(entries[:10], 1):
                avg_cached = e.get('avgCachedMs')
                avg_str = f"{avg_cached:>6.3f} ms" if avg_cached is not None else "   N/A   "
                print(f"  {idx:2d}. [{e.get('category', 'General'):<12}] Hits: {e.get('hitCount', 0):>5} | Misses: {e.get('missCount', '0'):>3} | Hit%: {e.get('hitRatio', 0):>5.1f}% | Avg Cached: {avg_str} | Key: {e.get('key', '')}")

    print("\n" + "=" * 80)

if __name__ == "__main__":
    run_cache_stress_and_monitor()
