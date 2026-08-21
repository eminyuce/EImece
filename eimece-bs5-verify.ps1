param()
$ErrorActionPreference = 'Continue'
$outDir = 'C:\Users\eminy\source\repos\EImece'
$reportPath = Join-Path $outDir 'eimece-bs5-verify-report.json'
$txtPath = Join-Path $outDir 'eimece-bs5-verify-report.txt'

Add-Type -AssemblyName System.Net.Http

function New-Client([bool]$follow) {
  $h = New-Object System.Net.Http.HttpClientHandler
  $h.AllowAutoRedirect = $follow
  $h.UseCookies = $true
  $h.CookieContainer = New-Object System.Net.CookieContainer
  $c = New-Object System.Net.Http.HttpClient($h)
  $c.Timeout = [TimeSpan]::FromSeconds(45)
  $c.DefaultRequestHeaders.UserAgent.ParseAdd('EImece-BS5-Verify/1.0')
  return $c
}

$clientNoRedir = New-Client $false
$clientFollow = New-Client $true

function Get-AbsUri([string]$baseUrl, [string]$location) {
  if ([string]::IsNullOrWhiteSpace($location)) { return $null }
  $base = New-Object System.Uri($baseUrl)
  return (New-Object System.Uri($base, $location)).AbsoluteUri
}

function Probe-Url {
  param(
    [string]$Url,
    [bool]$NeedBody = $false
  )
  $result = [ordered]@{
    url = $Url
    status = 0
    finalUrl = $Url
    followStatus = $null
    length = 0
    error = $null
    body = $null
  }
  try {
    $resp = $clientNoRedir.GetAsync($Url).GetAwaiter().GetResult()
    $result.status = [int]$resp.StatusCode
    $loc = $null
    if ($resp.Headers.Location) { $loc = $resp.Headers.Location.OriginalString }
    if ($result.status -ge 300 -and $result.status -lt 400 -and $loc) {
      $next = Get-AbsUri $Url $loc
      $result.finalUrl = $next
      try {
        $resp2 = $clientFollow.GetAsync($next).GetAwaiter().GetResult()
        $result.followStatus = [int]$resp2.StatusCode
        if ($resp2.RequestMessage -and $resp2.RequestMessage.RequestUri) {
          $result.finalUrl = $resp2.RequestMessage.RequestUri.AbsoluteUri
        }
        if ($NeedBody) {
          $result.body = $resp2.Content.ReadAsStringAsync().GetAwaiter().GetResult()
          $result.length = $result.body.Length
        } else {
          $bytes = $resp2.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
          $result.length = $bytes.Length
          $resp2.Dispose()
        }
      } catch {
        $result.error = "follow: $($_.Exception.Message)"
      }
      $resp.Dispose()
    } else {
      if ($NeedBody -or ($result.status -ge 200 -and $result.status -lt 300)) {
        if ($NeedBody) {
          $result.body = $resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
          $result.length = $result.body.Length
        } else {
          $bytes = $resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
          $result.length = $bytes.Length
        }
      }
      $resp.Dispose()
    }
  } catch {
    $result.error = $_.Exception.Message
    if ($_.Exception.InnerException) { $result.error += " | " + $_.Exception.InnerException.Message }
  }
  return [pscustomobject]$result
}

function Probe-Batch {
  param([string[]]$Urls, [int]$BatchSize = 20, [bool]$NeedBody = $false)
  $all = @()
  $total = $Urls.Count
  for ($i = 0; $i -lt $total; $i += $BatchSize) {
    $end = [Math]::Min($i + $BatchSize - 1, $total - 1)
    $slice = $Urls[$i..$end]
    $tasks = @()
    foreach ($u in $slice) {
      $tasks += @{ url = $u; task = $clientNoRedir.GetAsync($u) }
    }
    foreach ($t in $tasks) {
      $item = [ordered]@{
        url = $t.url
        status = 0
        finalUrl = $t.url
        followStatus = $null
        length = 0
        error = $null
        body = $null
      }
      try {
        $resp = $t.task.GetAwaiter().GetResult()
        $item.status = [int]$resp.StatusCode
        $loc = $null
        if ($resp.Headers.Location) { $loc = $resp.Headers.Location.OriginalString }
        if ($item.status -ge 300 -and $item.status -lt 400 -and $loc) {
          $next = Get-AbsUri $t.url $loc
          $item.finalUrl = $next
          try {
            $resp2 = $clientFollow.GetAsync($next).GetAwaiter().GetResult()
            $item.followStatus = [int]$resp2.StatusCode
            if ($resp2.RequestMessage -and $resp2.RequestMessage.RequestUri) {
              $item.finalUrl = $resp2.RequestMessage.RequestUri.AbsoluteUri
            }
            if ($NeedBody) {
              $item.body = $resp2.Content.ReadAsStringAsync().GetAwaiter().GetResult()
              $item.length = $item.body.Length
            } else {
              $b = $resp2.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
              $item.length = $b.Length
            }
            $resp2.Dispose()
          } catch {
            $item.error = "follow: $($_.Exception.Message)"
          }
        } else {
          if ($NeedBody) {
            $item.body = $resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            $item.length = $item.body.Length
          } else {
            $b = $resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
            $item.length = $b.Length
          }
        }
        $resp.Dispose()
      } catch {
        $item.error = $_.Exception.Message
        if ($_.Exception.InnerException) { $item.error += " | " + $_.Exception.InnerException.Message }
      }
      $all += [pscustomobject]$item
    }
    Write-Output ("PROGRESS {0}/{1}" -f $all.Count, $total)
  }
  return $all
}

Write-Output "=== SITEMAP ==="
$sm = Probe-Url -Url "http://localhost:81/sitemap.xml" -NeedBody $true
Write-Output ("sitemap status={0} len={1}" -f $sm.status, $sm.length)
$rawLocs = [regex]::Matches($sm.body, '<loc>\s*([^<]+)\s*</loc>') | ForEach-Object { $_.Groups[1].Value.Trim() }
# filter to valid http(s) loc entries, ignore xml header/empty and dedupe preserving order
$locs = $rawLocs | Where-Object { $_ -and $_.Trim() -ne "" -and $_ -match "^https?://localhost" }
$seen = @{}
$urls = @()
foreach ($u in $locs) {
  if (-not $seen.ContainsKey($u)) { $seen[$u] = $true; $urls += $u }
}
Write-Output ("unique loc count={0} raw={1}" -f $urls.Count, $rawLocs.Count)

$sitemapResults = Probe-Batch -Urls $urls -BatchSize 20 -NeedBody $false

$hist = @{}
foreach ($r in $sitemapResults) {
  $k = [string]$r.status
  if (-not $hist.ContainsKey($k)) { $hist[$k] = 0 }
  $hist[$k]++
}
Write-Output "=== SITEMAP HISTOGRAM ==="
$hist.GetEnumerator() | Sort-Object Name | ForEach-Object { "{0}={1}" -f $_.Name, $_.Value }

$non200 = $sitemapResults | Where-Object { $_.status -ne 200 }
Write-Output ("non-200 count={0}" -f @($non200).Count)
foreach ($r in $non200) {
  Write-Output ("FAIL status={0} follow={1} final={2} url={3} err={4}" -f $r.status, $r.followStatus, $r.finalUrl, $r.url, $r.error)
}

# persist sitemap partial
$sitemapSlim = $sitemapResults | ForEach-Object {
  [pscustomobject]@{ url=$_.url; status=$_.status; followStatus=$_.followStatus; finalUrl=$_.finalUrl; length=$_.length; error=$_.error }
}

Write-Output "=== ADMIN PAGES ==="
$gridControllers = @(
  'AppLogs','Brands','Coupons','Customers','Faq','Lists','MailTemplates','MainPageImages',
  'Media','Menus','Metrics','Orders','ProductCategories','ProductComments','Products',
  'ShoppingCarts','Stories','StoryCategories','Subscribers','TagCategories','Tags','Templates','Users'
)
$otherAdmin = @(
  'http://localhost:81/admin/dashboard/',
  'http://localhost:81/admin/products/',
  'http://localhost:81/admin/brands/',
  'http://localhost:81/admin/brands/SaveOrEdit',
  'http://localhost:81/admin/orders/',
  'http://localhost:81/admin/media/',
  'http://localhost:81/Admin/Images',
  'http://localhost:81/Admin/Orders/Details/2361',
  'http://localhost:81/Admin/Dashboard/',
  'http://localhost:81/Admin/Settings/',
  'http://localhost:81/Admin/AdminSettings/',
  'http://localhost:81/Admin/FileUpload/',
  'http://localhost:81/Admin/ImportData/',
  'http://localhost:81/Admin/Report/',
  'http://localhost:81/Admin/Error/'
)

$adminUrls = @()
foreach ($c in $gridControllers) {
  $adminUrls += "http://localhost:81/Admin/$c/"
  $adminUrls += "http://localhost:81/Admin/$c/IndexGrid?gridembed=1"
}
$adminUrls += $otherAdmin
# unique
$seen2 = @{}
$adminUrlsU = @()
foreach ($u in $adminUrls) { if (-not $seen2.ContainsKey($u.ToLowerInvariant())) { $seen2[$u.ToLowerInvariant()] = $true; $adminUrlsU += $u } }

$adminResults = @()
foreach ($u in $adminUrlsU) {
  $need = $true
  $r = Probe-Url -Url $u -NeedBody $need
  $body = $r.body
  $hasTable = $false
  $hasGriddly = $false
  $tbodyOnly = $false
  $empty = $false
  $flags = @()
  if ($r.status -eq 500 -or $r.followStatus -eq 500) { $flags += 'HTTP_500' }
  if ([string]::IsNullOrWhiteSpace($body) -and $r.status -eq 200) { $empty = $true; $flags += 'EMPTY_BODY' }
  if ($body) {
    $hasTable = $body -match '<table'
    $hasGriddly = $body -match 'griddly'
    $hasThead = $body -match '<thead'
    $hasTbody = $body -match '<tbody'
    if ($u -match 'IndexGrid' -and $u -match 'gridembed=1') {
      if (-not $hasTable) { $flags += 'NO_TABLE' }
      if (-not $hasGriddly -and -not $hasTable) { $flags += 'NO_GRIDDLY_MARKUP' }
      if ($hasTbody -and -not $hasTable) { $tbodyOnly = $true; $flags += 'TBODY_ONLY' }
      if ($hasTable -and -not $hasThead) { $flags += 'TABLE_NO_THEAD' }
    }
  }
  $snippet = $null
  if ($flags.Count -gt 0 -and $body) {
    $snippet = $body.Substring(0, [Math]::Min(400, $body.Length))
  }
  $adminResults += [pscustomobject]@{
    url = $r.url
    status = $r.status
    followStatus = $r.followStatus
    finalUrl = $r.finalUrl
    length = $r.length
    error = $r.error
    hasTable = $hasTable
    hasGriddly = $hasGriddly
    tbodyOnly = $tbodyOnly
    flags = ($flags -join ',')
    title = if ($body) { $m = [regex]::Match($body, '<title>([^<]*)</title>'); if ($m.Success) { $m.Groups[1].Value.Trim() } else { $null } } else { $null }
  }
  $flagStr = if ($flags.Count) { $flags -join ',' } else { 'ok' }
  Write-Output ("ADMIN status={0} follow={1} len={2} flags={3} url={4}" -f $r.status, $r.followStatus, $r.length, $flagStr, $r.url)
}

Write-Output "=== STOREFRONT ==="
$storeUrls = @(
  'http://localhost:81/',
  'http://localhost:81/account/login/',
  'http://localhost:81/account/adminlogin/',
  'http://localhost:81/payment/shoppingcart/',
  'http://localhost:81/c/Mutfak',
  'http://localhost:81/Error/NotFound',
  'http://localhost:81/i/iletisim-1b9a2d6g/'
)
# pick a category and product from sitemap
$cat = $urls | Where-Object { $_ -match '/c/' } | Select-Object -First 1
$prod = $urls | Where-Object { $_ -match '/p/' -or $_ -match '/products/' -or $_ -match '/urun' } | Select-Object -First 1
if (-not $prod) {
  # typical eimece product path might be /p/ or just slug
  $prod = $urls | Where-Object { $_ -match '/[a-z0-9-]+-[0-9a-z]+/$' -and $_ -notmatch '/c/' -and $_ -notmatch '/i/' -and $_ -notmatch '/s/' } | Select-Object -First 1
}
if ($cat) { $storeUrls += $cat }
if ($prod) { $storeUrls += $prod }
# extra category from homepage example
$storeUrls += 'http://localhost:81/c/pc/mutfak-1b3f4h1b/'

$storeResults = @()
foreach ($u in $storeUrls) {
  $r = Probe-Url -Url $u -NeedBody $true
  $body = $r.body
  $mentions = [ordered]@{
    bootstrap538 = $false
    jquery4 = $false
    adminJquery = $false
    adminBootstrap = $false
    siteJquery = $false
    siteBootstrap = $false
    mstoreVendorMin = $false
    mstoreThemeMin = $false
    mstoreBundle = $false
    crizalVendorJs = $false
    jquery400 = $false
  }
  if ($body) {
    $mentions.bootstrap538 = $body -match '5\.3\.8'
    $mentions.jquery4 = $body -match 'jquery-4|jquery/4|jQuery JavaScript Library v4'
    $mentions.adminJquery = $body -match 'adminJquery'
    $mentions.adminBootstrap = $body -match 'adminBootstrap'
    $mentions.siteJquery = $body -match 'siteJquery'
    $mentions.siteBootstrap = $body -match 'siteBootstrap'
    $mentions.mstoreVendorMin = $body -match 'vendor\.min\.js'
    $mentions.mstoreThemeMin = $body -match 'theme\.min\.js'
    $mentions.mstoreBundle = $body -match '/bundles/mstore'
    $mentions.crizalVendorJs = $body -match 'designs/crizal/vendor/js'
    $mentions.jquery400 = $body -match 'jquery-4\.0\.0'
  }
  $scripts = @()
  if ($body) {
    $scripts = [regex]::Matches($body, 'src="([^"]+)"') | ForEach-Object { $_.Groups[1].Value } | Where-Object { $_ -match 'js|bundle|jquery|bootstrap|mstore|vendor' }
  }
  $storeResults += [pscustomobject]@{
    url = $r.url
    status = $r.status
    followStatus = $r.followStatus
    finalUrl = $r.finalUrl
    length = $r.length
    error = $r.error
    mentions = $mentions
    scripts = $scripts
    title = if ($body) { $m = [regex]::Match($body, '<title>([^<]*)</title>'); if ($m.Success) { $m.Groups[1].Value.Trim() } else { $null } } else { $null }
  }
  Write-Output ("STORE status={0} follow={1} len={2} url={3} bs538={4} jq4={5} mstoreVendor={6} mstoreTheme={7} crizalJs={8} adminBS={9} adminJQ={10}" -f $r.status, $r.followStatus, $r.length, $r.url, $mentions.bootstrap538, $mentions.jquery4, $mentions.mstoreVendorMin, $mentions.mstoreThemeMin, $mentions.crizalVendorJs, $mentions.adminBootstrap, $mentions.adminJquery)
  if ($scripts) { $scripts | ForEach-Object { Write-Output ("  SCRIPT {0}" -f $_) } }
}

Write-Output "=== BUNDLE CONTENT CHECK ==="
# Fetch served vendor JS and look for version strings
$home = $storeResults | Where-Object { $_.url -eq 'http://localhost:81/' } | Select-Object -First 1
$bundleUrls = @()
if ($home -and $home.scripts) {
  foreach ($s in $home.scripts) {
    if ($s -match '^http') { $bundleUrls += $s }
    elseif ($s.StartsWith('/')) { $bundleUrls += ("http://localhost:81{0}" -f $s) }
  }
}
$bundleUrls += 'http://localhost:81/bundles/designs/crizal/vendor/js'
$bundleUrls += 'http://localhost:81/bundles/adminJquery'
$bundleUrls += 'http://localhost:81/bundles/adminBootstrap'
$bundleCheck = @()
$seenB = @{}
foreach ($bu in $bundleUrls) {
  $key = $bu -replace '\?.*',''
  if ($seenB.ContainsKey($key)) { continue }
  $seenB[$key] = $true
  $r = Probe-Url -Url $bu -NeedBody $true
  $head = if ($r.body) { $r.body.Substring(0, [Math]::Min(250, $r.body.Length)) -replace '\s+',' ' } else { '' }
  $has538 = $r.body -match 'Bootstrap v5\.3\.8'
  $hasJq4 = $r.body -match 'jQuery JavaScript Library v4\.0\.0'
  $hasMstoreVendor = $r.body -match 'mstore' -and $r.url -match 'vendor'
  $bundleCheck += [pscustomobject]@{
    url = $r.url
    status = $r.status
    length = $r.length
    hasBootstrap538 = $has538
    hasJquery400 = $hasJq4
    head = $head
  }
  Write-Output ("BUNDLE status={0} len={1} bs538={2} jq4={3} url={4}" -f $r.status, $r.length, $has538, $hasJq4, $r.url)
  Write-Output ("  HEAD {0}" -f $head)
}

# Admin dashboard asset check
$dash = $adminResults | Where-Object { $_.url -match 'dashboard' } | Select-Object -First 1

$report = [ordered]@{
  generated = (Get-Date).ToString('o')
  sitemap = [ordered]@{
    total = $urls.Count
    histogram = $hist
    failures = @($non200 | ForEach-Object { [ordered]@{ status=$_.status; followStatus=$_.followStatus; url=$_.url; finalUrl=$_.finalUrl; error=$_.error; length=$_.length } })
    all = $sitemapSlim
  }
  admin = $adminResults
  storefront = $storeResults
  bundles = $bundleCheck
}

$report | ConvertTo-Json -Depth 8 | Set-Content -Path $reportPath -Encoding UTF8
Write-Output ("WROTE {0}" -f $reportPath)
Write-Output "DONE"
