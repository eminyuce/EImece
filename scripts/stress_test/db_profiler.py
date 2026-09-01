import subprocess
import json

def get_sql_server_metrics():
    ps_script = (
        "$connStr = 'Server=YUCE\\SQLEXPRESS;Database=yuva8905_yuvadan;Integrated Security=True;TrustServerCertificate=True;'; "
        "$conn = New-Object System.Data.SqlClient.SqlConnection($connStr); "
        "try { "
        "  $conn.Open(); "
        "  $sql = 'SELECT COUNT(*) as cnt FROM sys.dm_exec_query_stats'; "
        "  $cmd = New-Object System.Data.SqlClient.SqlCommand($sql, $conn); "
        "  $cnt = $cmd.ExecuteScalar(); "
        "  $sql2 = 'SELECT COUNT(*) as sess FROM sys.dm_exec_sessions WHERE database_id = DB_ID()'; "
        "  $cmd2 = New-Object System.Data.SqlClient.SqlCommand($sql2, $conn); "
        "  $sess = $cmd2.ExecuteScalar(); "
        "  $res = @{ status = 'ok'; query_stats_count = [int]$cnt; active_db_sessions = [int]$sess }; "
        "  $res | ConvertTo-Json -Compress; "
        "} catch { "
        "  @{ status = 'error'; message = $_.Exception.Message } | ConvertTo-Json -Compress; "
        "} finally { "
        "  $conn.Close(); "
        "}"
    )
    try:
        out = subprocess.check_output(["powershell", "-NoProfile", "-Command", ps_script], text=True, timeout=10)
        return json.loads(out.strip())
    except Exception as e:
        return {"status": "error", "message": str(e)}

if __name__ == "__main__":
    print(get_sql_server_metrics())
