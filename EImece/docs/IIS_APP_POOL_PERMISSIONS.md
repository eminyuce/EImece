# IIS app pool folder permissions

After publishing EImece to IIS, the site runs as the **application pool identity** (for the `Eimece` site that is usually `IIS AppPool\Eimece`). That account can read the site by default, but it **cannot write** to folders unless you grant Modify rights.

Without those rights you typically see:

- failed image / media uploads
- empty or missing NLog files under `App_Data\logs`
- health check `fileStorage` reporting DOWN
- ASP.NET “access exception” / HTTP 500 when the app tries to create files

Run the commands below in an **elevated Command Prompt** or **PowerShell** (Run as administrator).

Default publish path used by `FolderProfile.pubxml`:

```text
C:\inetpub\wwwroot\Eimece
```

If your site physical path or app pool name differs, replace the path and `Eimece` pool name accordingly.

---

## What `icacls` is doing

```bat
icacls "PATH" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
```

| Piece | Meaning |
| --- | --- |
| `icacls` | Windows tool to view/change NTFS ACLs |
| `"PATH"` | Folder to grant rights on |
| `/grant "IIS AppPool\Eimece":...` | Add Allow ACE for the app pool virtual account |
| `(OI)` | Object inherit — files created under the folder inherit the ACE |
| `(CI)` | Container inherit — subfolders inherit the ACE |
| `M` | **Modify** (read, write, delete, create files/folders) |
| `/T` | Apply to this folder and all current children |

`IIS AppPool\Eimece` is not a normal Windows user. IIS creates it when the `Eimece` app pool exists. The name must match the app pool exactly (IIS Manager → Application Pools).

---

## Media / uploaded images

Admin and storefront code write uploaded images under `media\images`. Grant Modify so the pool can create and update files there:

```bat
icacls "C:\inetpub\wwwroot\Eimece\media\images" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
```

If `media\images` does not exist yet:

```bat
mkdir "C:\inetpub\wwwroot\Eimece\media\images"
icacls "C:\inetpub\wwwroot\Eimece\media\images" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
```

Optional: if you also write elsewhere under `media` (thumbs, cache, etc.), grant the parent once:

```bat
mkdir "C:\inetpub\wwwroot\Eimece\media"
icacls "C:\inetpub\wwwroot\Eimece\media" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
```

---

## Application logs (`App_Data\logs`)

NLog is configured to write under `${basedir}/App_Data/logs` (see `NLog.config`). Create the folder if missing, then grant Modify:

```bat
mkdir "C:\inetpub\wwwroot\Eimece\App_Data\logs"
icacls "C:\inetpub\wwwroot\Eimece\App_Data\logs" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
```

Recommended: also grant Modify on `App_Data` itself so other runtime files (cache, temp, health probes) can be written:

```bat
icacls "C:\inetpub\wwwroot\Eimece\App_Data" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
```

---

## One-shot setup after publish

Run as Administrator after each fresh deploy to a new machine (or after wiping ACLs):

```bat
mkdir "C:\inetpub\wwwroot\Eimece\App_Data\logs" 2>nul
mkdir "C:\inetpub\wwwroot\Eimece\media\images" 2>nul

icacls "C:\inetpub\wwwroot\Eimece\App_Data" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
icacls "C:\inetpub\wwwroot\Eimece\App_Data\logs" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
icacls "C:\inetpub\wwwroot\Eimece\media\images" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
```

Then recycle the pool so workers pick up a clean start:

```bat
%windir%\system32\inetsrv\appcmd recycle apppool /apppool.name:Eimece
```

Or in PowerShell:

```powershell
Import-Module WebAdministration
Restart-WebAppPool -Name Eimece
```

---

## Verify

```bat
icacls "C:\inetpub\wwwroot\Eimece\media\images"
icacls "C:\inetpub\wwwroot\Eimece\App_Data\logs"
```

You should see a line similar to:

```text
IIS APPPOOL\Eimece:(OI)(CI)(M)
```

Smoke-test the site:

1. Open `http://localhost:81/health` — expect `{"status":"UP"}` (port may differ).
2. Upload an image in Admin and confirm a new file appears under `media\images`.
3. Confirm log files appear under `App_Data\logs` after traffic.

---

## Notes

- **Do not** grant Modify on the whole site (`bin`, `Views`, `Web.config`) unless you have a specific reason. Prefer write access only on upload/log/data folders.
- Republishing with the Folder publish profile usually **keeps** existing ACLs on folders that already exist (`DeleteExistingFiles` is `false` in `FolderProfile.pubxml`). Re-run `icacls` if you recreate the site folder or restore from a zip that drops ACLs.
- If the app pool uses a custom identity (domain service account) instead of the default app-pool identity, grant that account instead of `IIS AppPool\Eimece`.
- Related: [BUILD_AND_RUN.md](BUILD_AND_RUN.md) (IIS setup), [SECURE_CONNECTION_STRINGS.md](SECURE_CONNECTION_STRINGS.md) (DB config for IIS).
