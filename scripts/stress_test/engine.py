import requests
import time
import concurrent.futures
import threading
from urllib.parse import urlparse
from requests.adapters import HTTPAdapter

class LoadEngine:
    def __init__(self, base_url="http://127.0.0.1:81", host_header="localhost:81"):
        self.base_url = base_url.rstrip("/")
        self.host_header = host_header

    def create_session(self, pool_size=50):
        s = requests.Session()
        adapter = HTTPAdapter(pool_connections=pool_size, pool_maxsize=pool_size, max_retries=1)
        s.mount("http://", adapter)
        s.mount("https://", adapter)
        s.headers.update({
            "User-Agent": "EImece-StressTester/1.0",
            "Host": self.host_header,
            "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
            "Accept-Encoding": "gzip, deflate"
        })
        return s

    def execute_sequential(self, endpoint_path, count=10, timeout=15):
        session = self.create_session(pool_size=10)
        url = f"{self.base_url}{endpoint_path}"
        latencies = []
        statuses = {}
        sizes = []
        errors = 0
        
        start_total = time.perf_counter()
        for _ in range(count):
            t0 = time.perf_counter()
            try:
                r = session.get(url, timeout=timeout, allow_redirects=True)
                lat = (time.perf_counter() - t0) * 1000.0
                latencies.append(lat)
                sizes.append(len(r.content))
                code = r.status_code
                statuses[code] = statuses.get(code, 0) + 1
                if code >= 400:
                    errors += 1
            except Exception as e:
                lat = (time.perf_counter() - t0) * 1000.0
                latencies.append(lat)
                errors += 1
                statuses["ERR"] = statuses.get("ERR", 0) + 1
        
        wall_time = time.perf_counter() - start_total
        latencies.sort()
        
        def pct(p):
            if not latencies:
                return 0.0
            idx = min(int(len(latencies) * p / 100.0), len(latencies) - 1)
            return round(latencies[idx], 2)
            
        return {
            "endpoint": endpoint_path,
            "count": count,
            "wall_time_sec": round(wall_time, 2),
            "rps": round(count / wall_time, 2) if wall_time > 0 else 0,
            "min_ms": round(min(latencies), 2) if latencies else 0,
            "avg_ms": round(sum(latencies) / len(latencies), 2) if latencies else 0,
            "p50_ms": pct(50),
            "p90_ms": pct(90),
            "p95_ms": pct(95),
            "p99_ms": pct(99),
            "max_ms": round(max(latencies), 2) if latencies else 0,
            "avg_size_kb": round((sum(sizes) / len(sizes)) / 1024.0, 2) if sizes else 0,
            "errors": errors,
            "error_rate_pct": round(errors / count * 100.0, 2) if count > 0 else 0,
            "status_codes": statuses
        }

    def execute_concurrent(self, request_fn, concurrency=10, duration_sec=60, timeout=15):
        """
        request_fn: function(session) -> (status_code, latency_ms, size_bytes, error_str)
        """
        latencies = []
        statuses = {}
        sizes = []
        errors = 0
        total_requests = 0
        stop_event = threading.Event()
        lock = threading.Lock()

        def worker_loop():
            nonlocal total_requests, errors
            s = self.create_session(pool_size=10)
            while not stop_event.is_set():
                status_code, lat_ms, sz_bytes, err = request_fn(s)
                with lock:
                    total_requests += 1
                    latencies.append(lat_ms)
                    sizes.append(sz_bytes)
                    statuses[status_code] = statuses.get(status_code, 0) + 1
                    if status_code >= 400 or status_code == 0 or err:
                        errors += 1

        threads = []
        start_total = time.perf_counter()
        for _ in range(concurrency):
            t = threading.Thread(target=worker_loop)
            t.daemon = True
            threads.append(t)
            t.start()

        # Wait for duration
        time.sleep(duration_sec)
        stop_event.set()

        for t in threads:
            t.join(timeout=3.0)

        wall_time = time.perf_counter() - start_total
        latencies.sort()

        def pct(p):
            if not latencies:
                return 0.0
            idx = min(int(len(latencies) * p / 100.0), len(latencies) - 1)
            return round(latencies[idx], 2)

        return {
            "concurrency": concurrency,
            "duration_sec": round(wall_time, 2),
            "total_requests": total_requests,
            "rps": round(total_requests / wall_time, 2) if wall_time > 0 else 0,
            "min_ms": round(min(latencies), 2) if latencies else 0,
            "avg_ms": round(sum(latencies) / len(latencies), 2) if latencies else 0,
            "p50_ms": pct(50),
            "p90_ms": pct(90),
            "p95_ms": pct(95),
            "p99_ms": pct(99),
            "max_ms": round(max(latencies), 2) if latencies else 0,
            "avg_size_kb": round((sum(sizes) / len(sizes)) / 1024.0, 2) if sizes else 0,
            "errors": errors,
            "error_rate_pct": round(errors / total_requests * 100.0, 2) if total_requests > 0 else 0,
            "status_codes": statuses
        }
