# IIS app pool folder permissions

After publishing EImece to IIS, the site runs as the **application pool identity** (for the `Eimece` site that is usually `IIS AppPool\Eimece`). That account can read the site by default, but it **cannot write** to folders unless you grant Modify rights.

Without those rights you typically see:

- failed image / media uploads
- empty or missing NLog files under `media\logs`
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

## One writable root: `media`

Uploads (`media\images`) and application logs (`media\logs`) share the **same parent folder** so you only grant write access once.

NLog writes under `${basedir}/media/logs` (`NLog.config`). HTTP access to `/media/logs` is blocked in `media/Web.config` (hidden segment + deny handlers).

```bat
mkdir "C:\inetpub\wwwroot\Eimece\media\images" 2>nul
mkdir "C:\inetpub\wwwroot\Eimece\media\logs" 2>nul
icacls "C:\inetpub\wwwroot\Eimece\media" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
```

That single `icacls` on `media` covers:

| Path | Purpose |
| --- | --- |
| `media\images` | Product / content image uploads |
| `media\logs` | NLog / structured log files |

---

## One-shot setup after publish

Run as Administrator after each fresh deploy to a new machine (or after wiping ACLs):

```bat
mkdir "C:\inetpub\wwwroot\Eimece\media\images" 2>nul
mkdir "C:\inetpub\wwwroot\Eimece\media\logs" 2>nul

icacls "C:\inetpub\wwwroot\Eimece\media" /grant "IIS AppPool\Eimece":(OI)(CI)M /T
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
icacls "C:\inetpub\wwwroot\Eimece\media"
```

You should see a line similar to:

```text
IIS APPPOOL\Eimece:(OI)(CI)(M)
```

Smoke-test the site:

1. Open `http://localhost:81/health` — expect `{"status":"UP"}` (port may differ).
2. Upload an image in Admin and confirm a new file appears under `media\images`.
3. Confirm log files appear under `media\logs` after traffic.
4. Confirm `http://localhost:81/media/logs/EImeceLog.log` is **not** publicly downloadable (404 / blocked).

---

## Notes

- **Do not** grant Modify on the whole site (`bin`, `Views`, `Web.config`) unless you have a specific reason. Prefer write access only on `media`.
- Republishing with the Folder publish profile usually **keeps** existing ACLs on folders that already exist (`DeleteExistingFiles` is `false` in `FolderProfile.pubxml`). Re-run `icacls` if you recreate the site folder or restore from a zip that drops ACLs.
- If the app pool uses a custom identity (domain service account) instead of the default app-pool identity, grant that account instead of `IIS AppPool\Eimece`.
- Older installs may still have logs under `App_Data\logs`; after upgrading, new logs go to `media\logs`. You can delete or archive the old folder once you no longer need those files.
- Related: [BUILD_AND_RUN.md](BUILD_AND_RUN.md) (IIS setup), [SECURE_CONNECTION_STRINGS.md](SECURE_CONNECTION_STRINGS.md) (DB config for IIS).
