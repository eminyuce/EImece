import time
import json
import threading
import subprocess
import requests
from requests.adapters import HTTPAdapter
from engine import LoadEngine
from scenarios import UserScenarios
from monitor import SystemMonitor

def run_memory_soak(duration_sec=300, concurrency=15):
    print(f"=== Starting Memory Soak & GC Retention Test (Duration: {duration_sec}s, Concurrency: {concurrency} CU) ===", flush=True)
    engine = LoadEngine()
    scenarios = UserScenarios()
    monitor = SystemMonitor(sample_interval_sec=5.0)
    
    monitor.start()
    step = 0
    def anon_req(s):
        nonlocal step
        step += 1
        return scenarios.scenario_anonymous_step(s, step)

    res = engine.execute_concurrent(anon_req, concurrency=concurrency, duration_sec=duration_sec)
    monitor.stop()
    
    summary = monitor.get_summary()
    samples = monitor.samples
    
    output = {
        "duration_sec": duration_sec,
        "concurrency": concurrency,
        "total_requests": res["total_requests"],
        "rps": res["rps"],
        "error_rate_pct": res["error_rate_pct"],
        "summary": summary,
        "samples": samples
    }
    
    with open("memory_soak_results.json", "w", encoding="utf-8") as f:
        json.dump(output, f, indent=2)
        
    print("\n--- Memory Soak Results ---", flush=True)
    print(f"  Requests Processed:   {res['total_requests']} (RPS: {res['rps']})", flush=True)
    print(f"  Initial w3wp Memory:  {summary.get('w3wp_mem_initial_mb', 0)} MB", flush=True)
    print(f"  Peak w3wp Memory:     {summary.get('w3wp_mem_peak_mb', 0)} MB", flush=True)
    print(f"  Final w3wp Memory:    {summary.get('w3wp_mem_final_mb', 0)} MB", flush=True)
    print(f"  Net Memory Delta:     {summary.get('w3wp_mem_growth_mb', 0)} MB", flush=True)
    print(f"  Peak Threads:         {summary.get('w3wp_threads_peak', 0)}", flush=True)
    print("Exported to memory_soak_results.json", flush=True)
    return output

if __name__ == "__main__":
    run_memory_soak(duration_sec=60, concurrency=10)
