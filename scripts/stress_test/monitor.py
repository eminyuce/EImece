import subprocess
import json
import time
import threading

class SystemMonitor:
    def __init__(self, sample_interval_sec=2.0):
        self.interval = sample_interval_sec
        self.running = False
        self.thread = None
        self.samples = []
        self.lock = threading.Lock()

    def _sample(self):
        ps_cmd = (
            "$w = Get-Process -Name w3wp -ErrorAction SilentlyContinue | Select-Object -First 1 Id, WorkingSet64, CPU, Threads; "
            "$s = Get-Process -Name sqlservr -ErrorAction SilentlyContinue | Select-Object -First 1 Id, WorkingSet64, CPU; "
            "$res = @{ "
            "  w3wp_id = if($w){$w.Id}else{0}; "
            "  w3wp_ws_mb = if($w){[math]::Round($w.WorkingSet64/1MB, 2)}else{0}; "
            "  w3wp_threads = if($w){$w.Threads.Count}else{0}; "
            "  w3wp_cpu = if($w){[math]::Round($w.CPU, 2)}else{0}; "
            "  sql_id = if($s){$s.Id}else{0}; "
            "  sql_ws_mb = if($s){[math]::Round($s.WorkingSet64/1MB, 2)}else{0}; "
            "  sql_cpu = if($s){[math]::Round($s.CPU, 2)}else{0}; "
            "  timestamp = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss') "
            "}; "
            "$res | ConvertTo-Json -Compress"
        )
        try:
            out = subprocess.check_output(["powershell", "-NoProfile", "-Command", ps_cmd], text=True, timeout=5)
            data = json.loads(out.strip())
            return data
        except Exception:
            return None

    def _run_loop(self):
        while self.running:
            s = self._sample()
            if s:
                with self.lock:
                    self.samples.append(s)
            time.sleep(self.interval)

    def start(self):
        self.running = True
        self.samples = []
        self.thread = threading.Thread(target=self._run_loop, daemon=True)
        self.thread.start()

    def stop(self):
        self.running = False
        if self.thread:
            self.thread.join(timeout=3)

    def get_summary(self):
        with self.lock:
            if not self.samples:
                return {}
            w3wp_mem = [s["w3wp_ws_mb"] for s in self.samples if s["w3wp_ws_mb"] > 0]
            sql_mem = [s["sql_ws_mb"] for s in self.samples if s["sql_ws_mb"] > 0]
            threads = [s["w3wp_threads"] for s in self.samples if s["w3wp_threads"] > 0]
            
            return {
                "samples_count": len(self.samples),
                "w3wp_mem_initial_mb": w3wp_mem[0] if w3wp_mem else 0,
                "w3wp_mem_peak_mb": max(w3wp_mem) if w3wp_mem else 0,
                "w3wp_mem_final_mb": w3wp_mem[-1] if w3wp_mem else 0,
                "w3wp_threads_peak": max(threads) if threads else 0,
                "sql_mem_peak_mb": max(sql_mem) if sql_mem else 0,
                "w3wp_mem_growth_mb": (w3wp_mem[-1] - w3wp_mem[0]) if len(w3wp_mem) > 1 else 0
            }

if __name__ == "__main__":
    m = SystemMonitor(1.0)
    print("Sampling for 5 seconds...")
    m.start()
    time.sleep(5)
    m.stop()
    print("Summary:", json.dumps(m.get_summary(), indent=2))
