import subprocess
import json
import time
import requests
from requests.adapters import HTTPAdapter

BASE_URL = "http://127.0.0.1:81"

def get_current_theme_setting():
    ps_cmd = (
        "$connStr = 'Server=YUCE\\SQLEXPRESS;Database=yuva8905_yuvadan;Integrated Security=True;TrustServerCertificate=True;'; "
        "$conn = New-Object System.Data.SqlClient.SqlConnection($connStr); "
        "try { "
        "  $conn.Open(); "
        "  $sql = 'SELECT SettingValue FROM Settings WHERE SettingKey = ''ActiveDesign'''; "
        "  $cmd = New-Object System.Data.SqlClient.SqlCommand($sql, $conn); "
        "  $val = $cmd.ExecuteScalar(); "
        "  @{ status = 'ok'; active_design = if($val){$val.ToString()}else{'Crizal (Default)'} } | ConvertTo-Json -Compress; "
        "} catch { "
        "  @{ status = 'error'; message = $_.Exception.Message } | ConvertTo-Json -Compress; "
        "} finally { "
        "  $conn.Close(); "
        "}"
    )
    try:
        out = subprocess.check_output(["powershell", "-NoProfile", "-Command", ps_cmd], text=True, timeout=10)
        return json.loads(out.strip())
    except Exception as e:
        return {"status": "error", "message": str(e)}

def benchmark_active_theme(theme_name, count=20):
    s = requests.Session()
    s.headers.update({"User-Agent": "EImece-ThemeBenchmark/1.0", "Host": "localhost:81"})
    
    urls = [
        "/",
        "/c/pc/elektronik-9a8c0j1b",
        "/p/kulaklik--ses/copilotmicrosoft-5i0j0j0j4h1b",
        "/info/aboutus"
    ]
    
    results = {}
    for u in urls:
        latencies = []
        sizes = []
        for _ in range(count):
            t0 = time.perf_counter()
            r = s.get(f"{BASE_URL}{u}", timeout=15)
            lat = (time.perf_counter() - t0) * 1000.0
            latencies.append(lat)
            sizes.append(len(r.content))
        latencies.sort()
        results[u] = {
            "avg_ms": round(sum(latencies)/len(latencies), 2),
            "p95_ms": round(latencies[int(len(latencies)*0.95)], 2),
            "size_kb": round((sum(sizes)/len(sizes))/1024.0, 2)
        }
    return results

if __name__ == "__main__":
    current = get_current_theme_setting()
    print("Theme Setting:", current)
    print("Benchmarking currently active theme...")
    bench = benchmark_active_theme(current.get("active_design", "Crizal"))
    print(json.dumps(bench, indent=2))
