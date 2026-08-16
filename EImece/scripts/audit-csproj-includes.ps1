$projects = @(
    @{ Path = "c:\Users\eminy\source\repos\EImece\EImece\EImece.Domain\EImece.Domain.csproj"; Dir = "c:\Users\eminy\source\repos\EImece\EImece\EImece.Domain" },
    @{ Path = "c:\Users\eminy\source\repos\EImece\EImece\EImece\EImece.csproj"; Dir = "c:\Users\eminy\source\repos\EImece\EImece\EImece" },
    @{ Path = "c:\Users\eminy\source\repos\EImece\EImece\EImece.Tests\EImece.Tests.csproj"; Dir = "c:\Users\eminy\source\repos\EImece\EImece\EImece.Tests" },
    @{ Path = "c:\Users\eminy\source\repos\EImece\EImece\Resources\Resources.csproj"; Dir = "c:\Users\eminy\source\repos\EImece\EImece\Resources" },
    @{ Path = "c:\Users\eminy\source\repos\EImece\EImece\EImece.MyConsole\EImece.MyConsole.csproj"; Dir = "c:\Users\eminy\source\repos\EImece\EImece\EImece.MyConsole" }
)

foreach ($proj in $projects) {
    if (-not (Test-Path $proj.Path)) { continue }
    [xml]$xml = Get-Content $proj.Path
    $included = $xml.Project.ItemGroup.ChildNodes | Where-Object { $_.Include } | ForEach-Object { $_.Include.Replace('/', '\') }
    
    $files = Get-ChildItem -Path $proj.Dir -Recurse -File | Where-Object {
        $_.FullName -notmatch '\\bin\\' -and 
        $_.FullName -notmatch '\\obj\\' -and 
        $_.FullName -notmatch '\\\.vs\\' -and 
        $_.FullName -notmatch '\\\.git\\' -and 
        $_.FullName -notmatch '\\media\\' -and 
        $_.FullName -notmatch '\\packages\\' -and
        $_.FullName -notmatch '\\App_Data\\' -and
        $_.Extension -in @('.cs', '.cshtml', '.config', '.asax', '.json', '.xml', '.png', '.jpg', '.svg', '.css', '.js') -and
        $_.Name -notmatch '\.csproj$' -and
        $_.Name -notmatch '\.user$'
    }
    
    $missing = @()
    foreach ($file in $files) {
        $relPath = $file.FullName.Substring($proj.Dir.Length + 1)
        if ($included -notcontains $relPath) {
            $missing += $relPath
        }
    }
    
    Write-Host "=== Project: $($proj.Path) ==="
    Write-Host "Total Disk Files: $($files.Count), Missing in csproj: $($missing.Count)"
    foreach ($m in $missing) {
        Write-Host "  [MISSING] $m" -ForegroundColor Yellow
    }
}
