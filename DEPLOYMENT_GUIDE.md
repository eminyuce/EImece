# ASP.NET MVC 5 Deployment Guide: Group Policy and Razor Precompilation

## Overview

This document outlines the deployment strategy for the EImece ASP.NET MVC 5 application on production servers with Group Policy restrictions that block runtime C# compilation.

---

## Table of Contents

1. [Problem Statement](#problem-statement)
2. [Root Cause Analysis](#root-cause-analysis)
3. [Initial Configuration Error](#initial-configuration-error)
4. [Solution Approach](#solution-approach)
5. [Implementation Steps](#implementation-steps)
6. [Current Status](#current-status)
7. [Known Issues](#known-issues)
8. [Next Steps](#next-steps)

---

## Problem Statement

### Production Server Error

When the ASP.NET MVC 5 application runs on the production server, the following error occurs:

```
System.ComponentModel.Win32Exception:
This program is blocked by group policy.
For more information, contact your system administrator.
```

**Blocked Executable:**
```
C:\Inetpub\vhosts\ledampulburada.com\httpdocs\bin\roslyn\csc.exe
```

**Stack Trace Indicators:**
```
Microsoft.CodeDom.Providers.DotNetCompilerPlatform.Compiler.Compile
System.Web.Compilation.AssemblyBuilder.Compile
System.Web.Compilation.BuildManager.CompileWebFile
```

### What This Means

The error indicates that ASP.NET is attempting to compile Razor/C# code at runtime using the Roslyn C# compiler (`csc.exe`). The production server's Group Policy configuration blocks execution of this executable, preventing the application from running.

---

## Root Cause Analysis

### Execution Chain

The error occurs due to the following execution chain:

```
IIS
 ↓
ASP.NET MVC 5
 ↓
Razor View (e.g., Index.cshtml)
 ↓
System.Web.Compilation Framework
 ↓
Microsoft.CodeDom.Providers.DotNetCompilerPlatform
 ↓
Roslyn csc.exe
 ↓
Windows Group Policy (BLOCKS EXECUTION)
 ↓
ERROR
```

### What the Problem is NOT

- ❌ HomeController issue
- ❌ MVC routing problem
- ❌ Entity Framework issue
- ❌ Database connectivity error

### What the Problem IS

✅ **Production server Group Policy prevents runtime compilation of Razor views by blocking `csc.exe` execution.**

This is an infrastructure constraint, not a code defect.

---

## Initial Configuration Error

### First Error Encountered

Before the Group Policy error, a configuration error was present:

```
The 'targetFramework' attribute in the <compilation> element
of the Web.config file is used only to target version 4.0
and later of the .NET Framework.

The 'targetFramework' attribute currently references a version
that is later than the installed version of the .NET Framework.
```

**Server Information:**
```
Microsoft .NET Framework Version: 4.0.30319
ASP.NET Version: 4.8.4805.0
```

This was corrected, but the Group Policy issue persists, indicating the core problem is runtime compilation restriction.

---

## Solution Approach

### Traditional ASP.NET MVC 5 Deployment (Problematic)

In a standard deployment without precompilation:

```
Production IIS
	↓
Index.cshtml (delivered as source)
	↓
Razor Runtime Compiler
	↓
Roslyn csc.exe (BLOCKED by Group Policy)
	↓
ERROR
```

### Recommended: Precompiled Deployment

To avoid runtime compilation, we precompile all Razor views during the build process:

```
Development Machine
	↓
Visual Studio Release Build
	↓
ASP.NET Compilation Tool (aspnet_compiler.exe)
	↓
Razor Precompilation
	↓
Compiled DLLs + Binary Output
	↓
Production IIS
	↓
No runtime compilation needed
	↓
No csc.exe execution required
	↓
SUCCESS
```

### Benefits

- ✅ Eliminate runtime Razor compilation
- ✅ Eliminate dependency on Roslyn `csc.exe`
- ✅ Bypass Group Policy restrictions
- ✅ Improve startup performance
- ✅ Catch view compilation errors before deployment

---

## Implementation Steps

### Step 1: Verify Release Build

Verify that the project builds successfully in Release configuration.

**Configuration:**
- Configuration: `Release`
- Platform: `Any CPU`

**Build Command:**
```
Build → Rebuild Solution
```

**Expected Output:**
```
========== Rebuild All: 3 succeeded, 0 failed, 0 skipped ==========
```

**Project Artifacts:**
```
Resources
EImece.Domain
EImece

Output DLL: C:\Users\eminy\source\repos\EImece\EImece\EImece\bin\EImece.dll
```

### Step 2: Verify ASP.NET Compilation Tool Availability

The `aspnet_compiler.exe` tool must be available on the build machine.

**PowerShell Command:**
```powershell
Test-Path "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe"
```

**Expected Response:**
```
True
```

**Tool Version:**
```
Microsoft (R) ASP.NET Compilation Tool version 4.8.9221.0
```

### Step 3: Create Precompile Output Directory

Create the output directory for precompiled files.

**Directory Path:**
```
C:\Publish\EImece
```

**PowerShell Command:**
```powershell
New-Item -ItemType Directory -Path "C:\Publish\EImece" -Force
```

### Step 4: Execute Precompilation

Run the ASP.NET Compilation Tool to precompile the project.

**PowerShell Command:**
```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" `
  -p "C:\Users\eminy\source\repos\EImece\EImece\EImece" `
  -v / `
  -f `
  "C:\Publish\EImece"

Write-Host "Exit Code: $LASTEXITCODE"
```

**Parameters:**
- `-p`: Physical path to the ASP.NET application
- `-v`: Virtual path (root)
- `-f`: Force precompilation of all files
- Output path: Destination for precompiled files

**Success Criteria:**
- Exit Code: `0`
- Output directory contains compiled binaries

### Step 5: Verify Output

Check the precompiled output directory for compiled files.

```powershell
Get-ChildItem "C:\Publish\EImece" -Recurse | Select-Object Name, Length
```

**Expected Structure:**
```
C:\Publish\EImece\
├── bin\
│   ├── EImece.dll
│   ├── EImece.pdb
│   ├── (other assemblies)
│   └── roslyn\ (ideally minimal or empty)
├── App_Data\
├── Content\
├── Scripts\
└── (compiled view assemblies)
```

### Step 6: Deploy to Production

Copy the precompiled output to the production server.

```powershell
Copy-Item -Path "C:\Publish\EImece\*" `
		  -Destination "\\production-server\webroot\app" `
		  -Recurse -Force
```

---

## Current Status

### Release Build Status

✅ **SUCCESSFUL**

All projects build successfully in Release configuration:
- Resources
- EImece.Domain
- EImece

### ASP.NET Compiler Availability

✅ **AVAILABLE**

The ASP.NET Compilation Tool (`aspnet_compiler.exe`) version 4.8.9221.0 is present on the build machine.

### Precompilation Status

❌ **FAILED** (Exit Code: 1)

The precompilation process completes but reports failure due to actual Razor view compilation errors. These errors must be resolved before a successful precompiled deployment can be created.

---

## Known Issues

### Issue 1: Razor Compilation Error in pProductDetailToolTip.cshtml

**File:**
```
Areas\Admin\Views\Shared\pProductDetailToolTip.cshtml
```

**Error 1:**
```
error CS1061: 'ProductDetailViewModel' does not contain a definition for 'Product'
```

**Error 2 & 3:**
```
error CS0411: The type arguments for method 
'DisplayNameExtensions.DisplayNameFor<TModel, TValue>'
cannot be inferred from the usage.

error CS0411: The type arguments for method 
'DisplayExtensions.DisplayFor<TModel, TValue>'
cannot be inferred from the usage.
```

**Root Cause:**

The `pProductDetailToolTip.cshtml` view uses `ProductDetailViewModel.Product` property, but the current `ProductDetailViewModel` class does not define a `Product` property.

**Impact:**

This prevents successful precompilation (Exit Code: 1). Production deployment cannot proceed until this is resolved.

### Issue 2: Warning - Unused Variable in ProductCategories\Index.cshtml

**File:**
```
ProductCategories\Index.cshtml(19)
```

**Warning:**
```
warning CS0219: The variable 'title' is assigned but its value is never used
```

**Status:** Non-blocking (warning, not error)

### Issue 3: Warning - Unused Variable in Products\_pProductGrid.cshtml

**File:**
```
Products\_pProductGrid.cshtml(16)
```

**Warning:**
```
warning CS0219: The variable 'title' is assigned but its value is never used
```

**Status:** Non-blocking (warning, not error)

---

## Next Steps

### Immediate Actions

1. **Investigate pProductDetailToolTip.cshtml**
   ```
   File: C:\Users\eminy\source\repos\EImece\EImece\EImece\
		  Areas\Admin\Views\Shared\pProductDetailToolTip.cshtml
   ```

2. **Locate ProductDetailViewModel**
   - Find the definition of `ProductDetailViewModel`
   - Identify whether a `Product` property should exist
   - If it should exist, add it to the ViewModel
   - If the view is incorrect, correct the view references

3. **Review View Code**
   - Check all usages of `Model.Product` in the view
   - Verify property names match the ViewModel definition
   - Ensure `DisplayNameFor` and `DisplayFor` lambda expressions are correct

4. **Re-run Precompilation**
   ```powershell
   & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" `
	 -p "C:\Users\eminy\source\repos\EImece\EImece\EImece" `
	 -v / `
	 -f `
	 "C:\Publish\EImece"
   ```

   Expected result: **Exit Code: 0**

5. **Production Deployment**
   Once precompilation succeeds with Exit Code 0:
   - Copy precompiled output to production server
   - Remove `.cshtml` source files from production
   - IIS will serve pre-compiled views without runtime compilation
   - Group Policy restrictions on `csc.exe` will no longer block the application

---

## Deployment Checklist

- [ ] All projects build successfully in Release configuration
- [ ] `aspnet_compiler.exe` is available (version 4.8.9221.0 or later)
- [ ] `C:\Publish\EImece` output directory is created
- [ ] pProductDetailToolTip.cshtml compilation errors are resolved
- [ ] Precompilation runs successfully with Exit Code: 0
- [ ] No compilation errors in precompilation output
- [ ] Precompiled output is verified and contains compiled DLLs
- [ ] Production server directory is prepared
- [ ] Precompiled files are deployed to production
- [ ] `.cshtml` source files are removed from production (optional but recommended)
- [ ] Application tested in production environment
- [ ] Monitoring and logging verified

---

## Troubleshooting

### Issue: Precompilation Exit Code 1

**Solution:**
1. Review the error messages in the precompilation output
2. Identify the problematic view file(s)
3. Check ViewModel property definitions
4. Verify Lambda expressions in view helpers
5. Test the corrected view locally
6. Re-run precompilation

### Issue: aspnet_compiler.exe Not Found

**Solution:**
```powershell
# Verify the tool exists
Test-Path "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe"

# If not found, restore from .NET Framework installation
# or install/repair .NET Framework 4.8
```

### Issue: Access Denied to Output Directory

**Solution:**
```powershell
# Remove existing directory or run as Administrator
Remove-Item -Path "C:\Publish\EImece" -Recurse -Force
New-Item -ItemType Directory -Path "C:\Publish\EImece" -Force
```

---

## References

- **Microsoft Docs:** [ASP.NET Compilation Tool (aspnet_compiler.exe)]
- **Project Structure:** EImece ASP.NET MVC 5 Application
- **Target Framework:** .NET Framework 4.8.1
- **Compiler Platform:** Microsoft.CodeDom.Providers.DotNetCompilerPlatform (Roslyn)

---

## Document History

| Version | Date       | Changes                                      |
|---------|------------|----------------------------------------------|
| 1.0     | 2025       | Initial deployment guide and issue tracking |

---

**Last Updated:** 2025

**Author:** EImece Development Team

**Status:** In Progress (awaiting pProductDetailToolTip.cshtml resolution)
