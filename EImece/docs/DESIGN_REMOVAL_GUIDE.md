# Design Removal & Deletion Guide

This document explains the issue encountered when deleting a Razor UI design package from the EImece codebase and provides a step-by-step procedure to remove a design cleanly without breaking MSBuild publish/release pipelines.

---

## 1. The Issue / Problem Description

In ASP.NET MVC 5.3 / .NET Framework 4.8.1 projects using legacy `.csproj` MSBuild project files:

Every static file (Razor view `.cshtml`, stylesheet `.css`, script `.js`, configuration `.json`) is explicitly listed in `EImece.csproj` inside `<Content Include="..." />` nodes.

### Symptom
If a design directory (e.g., `Views/Designs/{DesignName}/`) is deleted from disk **without** removing its corresponding `<Content Include="..." />` tags from `EImece.csproj`:

1. Incremental Debug builds may initially pass.
2. **Release builds, Visual Studio Publish, IIS publishing, and CI/CD pipelines will fail** with an MSBuild error:

```text
Error Copying file Views\Designs\{DesignName}\Account\ConfirmEmail.cshtml to obj\Release\Package\PackageTmp\Views\Designs\{DesignName}\Account\ConfirmEmail.cshtml failed.
Could not find a part of the path 'Views\Designs\{DesignName}\Account\ConfirmEmail.cshtml'.
```

---

## 2. Step-by-Step Procedure to Remove a Design

When removing an unused or legacy design (e.g., `Corporate`, `Minimal`, etc.), follow these exact steps:

### Step 1: Remove Design Folders from Disk
Delete the design files from both the `Views` and `Content` directories:

```powershell
# Remove Razor Views
Remove-Item -Path "EImece\Views\Designs\{DesignName}" -Recurse -Force -ErrorAction SilentlyContinue

# Remove CSS/JS Assets
Remove-Item -Path "EImece\Content\designs\{designName}" -Recurse -Force -ErrorAction SilentlyContinue
```

### Step 2: Remove `<Content Include>` Entries from `EImece.csproj`
Open `EImece/EImece.csproj` and remove all lines matching the deleted design path:

- `Views\Designs\{DesignName}\...`
- `Content\designs\{designName}\...`

#### Automated Removal via PowerShell:
```powershell
$csprojPath = "EImece\EImece\EImece.csproj"
$designName = "Corporate"

(Get-Content $csprojPath) | Where-Object { 
    $_ -notmatch "Views\\Designs\\$designName\\" -and 
    $_ -notmatch "Content\\designs\\$($designName.ToLower())\\" 
} | Set-Content $csprojPath
```

### Step 3: Update Unit Tests & Design References
Inspect test projects (e.g., `EImece.Tests/Infrastructure/DesignSystemTests.cs`) for explicit hardcoded strings referencing `{DesignName}` and replace them with active design names or test mocks.

### Step 4: Verify Both Debug and Release Builds
Run MSBuild for both Debug and Release configurations to ensure no missing content references remain:

```powershell
# Build Debug
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" EImece\EImece.sln /t:Build /p:Configuration=Debug

# Build Release (Verifies Publish packaging targets)
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" EImece\EImece.sln /t:Build /p:Configuration=Release
```

### Step 5: Run Design System Unit Tests
Execute vstest to verify that design validation tests pass:

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" EImece\EImece.Tests\bin\Debug\EImece.Tests.dll /TestCaseFilter:ClassName=EImece.Tests.Infrastructure.DesignSystemTests
```

---

## 3. Quick Checklist

- [ ] Folder `Views/Designs/{DesignName}/` deleted.
- [ ] Folder `Content/designs/{designName}/` deleted.
- [ ] Removed all `<Content Include="Views\Designs\{DesignName}\..." />` from `EImece.csproj`.
- [ ] Removed all `<Content Include="Content\designs\{designName}\..." />` from `EImece.csproj`.
- [ ] Updated test files in `EImece.Tests`.
- [ ] `MSBuild` Debug build succeeded with 0 errors.
- [ ] `MSBuild` Release build succeeded with 0 errors.
- [ ] `DesignSystemTests` passed 100%.
