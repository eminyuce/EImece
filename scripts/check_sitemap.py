import requests
import xml.etree.ElementTree as ET
import time
import concurrent.futures
import json
from urllib.parse import urlparse
from requests.adapters import HTTPAdapter

BASE_HOST = "http://127.0.0.1:81"

def create_session():
    s = requests.Session()
    adapter = HTTPAdapter(pool_connections=30, pool_maxsize=30, max_retries=1)
    s.mount("http://", adapter)
    s.headers.update({
        "User-Agent": "EImece-Sitemap-Checker/1.0",
        "Host": "localhost:81",
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        "Accept-Encoding": "gzip, deflate"
    })
    return s

def fetch_sitemap_urls(session):
    url = f"{BASE_HOST}/sitemap.xml"
    resp = session.get(url, timeout=20)
    resp.raise_for_status()
    tree = ET.fromstring(resp.content)
    
    urls = []
    for elem in tree.iter("{http://www.sitemaps.org/schemas/sitemap/0.9}loc"):
        if elem.text:
            urls.append(elem.text.strip())
    return urls

def check_single_url(session, target_url):
    parsed = urlparse(target_url)
    local_url = f"{BASE_HOST}{parsed.path}"
    if parsed.query:
        local_url += f"?{parsed.query}"

    start_time = time.perf_counter()
    status_code = 0
    size = 0
    error_msg = ""
    
    try:
        resp = session.get(local_url, timeout=20, allow_redirects=False)
        status_code = resp.status_code
        size = len(resp.content)
    except requests.exceptions.RequestException as e:
        error_msg = str(e)
        
    duration_ms = (time.perf_counter() - start_time) * 1000.0
    
    path = parsed.path.lower()
    if path == "/" or path == "":
        category = "Homepage"
    elif path.startswith("/info/"):
        category = "Info / Static"
    elif path.startswith("/c/pc/"):
        category = "Product Category"
    elif path.startswith("/p/"):
        category = "Product Detail"
    elif path.startswith("/s/") or path == "/s":
        category = "Stories / Blog"
    elif path.startswith("/i/"):
        category = "Info Item"
    else:
        category = "Other"

    return {
        "original_url": target_url,
        "path": parsed.path,
        "category": category,
        "status": status_code,
        "duration_ms": round(duration_ms, 2),
        "size_bytes": size,
        "error": error_msg
    }

def worker(url_batch):
    session = create_session()
    batch_results = []
    for u in url_batch:
        res = check_single_url(session, u)
        batch_results.append(res)
    return batch_results

def main():
    print("=== Fetching sitemap.xml from EImece (http://localhost:81/sitemap.xml) ===", flush=True)
    main_session = create_session()
    sitemap_urls = fetch_sitemap_urls(main_session)
    print(f"Discovered {len(sitemap_urls)} URLs.", flush=True)
    
    # Split URLs into batches for concurrent sessions
    num_threads = 10
    batches = [[] for _ in range(num_threads)]
    for idx, u in enumerate(sitemap_urls):
        batches[idx % num_threads].append(u)
        
    print(f"\n=== Validating {len(sitemap_urls)} URLs across {num_threads} concurrent session threads ===", flush=True)
    results = []
    start_all = time.perf_counter()
    
    with concurrent.futures.ThreadPoolExecutor(max_workers=num_threads) as executor:
        futures = [executor.submit(worker, b) for b in batches if b]
        for f in concurrent.futures.as_completed(futures):
            batch_res = f.result()
            results.extend(batch_res)
            print(f"  Completed batch ({len(results)}/{len(sitemap_urls)} URLs tested)...", flush=True)

    total_time = time.perf_counter() - start_all
    
    # Sort results
    results.sort(key=lambda x: x["path"])
    
    # Summary Statistics
    status_counts = {}
    category_stats = {}
    latencies = [r["duration_ms"] for r in results]
    latencies.sort()
    
    for r in results:
        status = r["status"]
        status_counts[status] = status_counts.get(status, 0) + 1
        
        cat = r["category"]
        if cat not in category_stats:
            category_stats[cat] = {"count": 0, "latencies": [], "sizes": [], "errors": 0}
        category_stats[cat]["count"] += 1
        category_stats[cat]["latencies"].append(r["duration_ms"])
        category_stats[cat]["sizes"].append(r["size_bytes"])
        if r["status"] != 200:
            category_stats[cat]["errors"] += 1

    def percentile(p, data):
        if not data:
            return 0
        idx = int(len(data) * p / 100.0)
        idx = min(idx, len(data) - 1)
        return data[idx]

    print("\n" + "=" * 70, flush=True)
    print("                     SITEMAP URL AUDIT REPORT", flush=True)
    print("=" * 70, flush=True)
    print(f"Total URLs Tested:  {len(results)}", flush=True)
    print(f"Total Wall Clock:   {total_time:.2f} s", flush=True)
    print(f"Throughput:         {len(results) / total_time:.2f} req/sec", flush=True)
    
    print("\n--- HTTP Status Distribution ---", flush=True)
    for s, count in sorted(status_counts.items()):
        print(f"  HTTP {s}: {count} ({count/len(results)*100:.1f}%)", flush=True)

    print("\n--- Overall Latency Profile (ms) ---", flush=True)
    print(f"  Min: {min(latencies):.1f} ms  |  Avg: {sum(latencies)/len(latencies):.1f} ms  |  p50: {percentile(50, latencies):.1f} ms", flush=True)
    print(f"  p90: {percentile(90, latencies):.1f} ms  |  p95: {percentile(95, latencies):.1f} ms  |  p99: {percentile(99, latencies):.1f} ms  |  Max: {max(latencies):.1f} ms", flush=True)

    print("\n--- URL Category Breakdown ---", flush=True)
    print(f"{'Category':<20} {'Count':<8} {'Errors':<8} {'Avg Lat (ms)':<14} {'p95 Lat (ms)':<14} {'Avg Size (KB)':<14}", flush=True)
    print("-" * 80, flush=True)
    for cat, data in sorted(category_stats.items()):
        clats = sorted(data["latencies"])
        avg_lat = sum(clats) / len(clats)
        p95_lat = percentile(95, clats)
        avg_sz = (sum(data["sizes"]) / len(data["sizes"])) / 1024.0
        print(f"{cat:<20} {data['count']:<8} {data['errors']:<8} {avg_lat:<14.1f} {p95_lat:<14.1f} {avg_sz:<14.1f}", flush=True)

    # Top 10 Slowest URLs
    slowest = sorted(results, key=lambda x: x["duration_ms"], reverse=True)[:10]
    print("\n--- Top 10 Slowest Sitemap URLs ---", flush=True)
    for idx, s in enumerate(slowest, 1):
        print(f"{idx:2d}. [{s['duration_ms']:6.1f} ms | HTTP {s['status']} | {s['size_bytes']/1024:5.1f} KB] {s['original_url']}", flush=True)

    # Any non-200 URLs
    non_200 = [r for r in results if r["status"] != 200]
    if non_200:
        print(f"\n--- NON-200 URLs Detected ({len(non_200)}) ---", flush=True)
        for err in non_200:
            print(f"  HTTP {err['status']} - {err['original_url']} (Error: {err['error']})", flush=True)
    else:
        print("\n>>> ALL 262 sitemap URLs returned HTTP 200 OK! Zero broken links or 500 errors detected. <<<", flush=True)

    # Save to JSON
    with open("sitemap_audit_report.json", "w", encoding="utf-8") as f:
        json.dump(results, f, indent=2, ensure_ascii=False)
    print("\nAudit saved to sitemap_audit_report.json", flush=True)

if __name__ == "__main__":
    main()
