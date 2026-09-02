# Admin panel Excel/CSV export

- **Captured:** 2026-08-06 2:05:06 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

# Task: Enhance Admin Panel export (Excel formatting + Excel/CSV choice)
Work in *one git branch* off master (do not mix with unrelated local changes).
Suggested branch name: feature/admin-export-excel-csv-formatting
## Context
- Admin Excel export is centralized on *NPOI 2.8.0* via EImece.Domain/Helpers/ExcelHelper.cs.
- Grid exports go through BaseAdminController.DownloadFile / DownloadFileDataTable.
- Report exports already support Excel/CSV via /Admin/Report/Export?format=excel|csv and _ReportExportButtons.cshtml.
- Grid toolbars currently only have a single Excel link in Areas/Admin/Views/Shared/pGridOperations.cshtml → ExportExcel.
- Today, CSV is only used automatically when row count ≥ 65534; users cannot choose format on grids.
- Current ExportDataTableToSheet writes everything as strings and has weak styling.
## Goals
### A) User can choose Excel or CSV on grid downloads
1. Update the grid export UI in pGridOperations.cshtml so the user can select *Excel* or *CSV* (two buttons like report export is fine; keep existing Admin toolbar look).
2. Pass a format query parameter (excel | csv) to the export action.
3. Update BaseAdminController.DownloadFile / DownloadFileDataTable to honor format:
   - excel → NPOI .xls via ExcelHelper.GetExcelByteArrayFromDataTable
   - csv → CSV via existing ExcelHelper.Export (or report-style CSV helper if more appropriate)
4. Update all ExportExcel actions (and keep action names compatible with existing routes) to accept an optional format parameter defaulting to excel.
5. Do not break Report exports; leave their existing Excel/CSV buttons working.
6. Add/reuse resource strings in Admin resources if needed (avoid hardcoding Turkish/English inconsistently with the rest of Admin UI).
### B) Professional Excel formatting (shared helper)
Implement formatting once in ExcelHelper (shared by all Admin Excel downloads: grids + reports). Do *not* duplicate formatting in each controller.
#### Header row
- Bold header text
- Light pastel yellow background
- Distinct header font (e.g. Calibri 12 Bold)
- Center-aligned
- Thin borders around header cells
#### Data rows
- Standard font (e.g. Calibri 11)
- White background
- Left-align text columns
- Right-align numeric columns
- Auto-size all columns based on content
#### Date formatting
- Turkish conventions:
  - dd.MM.yyyy for dates
  - dd.MM.yyyy HH:mm:ss for datetimes
- Excel must treat them as real date cells, not plain text
#### General sheet behavior
- Freeze the first row
- Enable AutoFilter on the header row
- Preserve correct data types (numbers, dates, booleans)
- Do *not* export everything as strings
### C) Code quality
- Follow existing project conventions
- Keep implementation clean, reusable, maintainable
- Prefer extending ExcelHelper (and BaseAdminController for download plumbing) over per-controller formatting
- Touch only files required for this feature
- Do not include unrelated dirty working-tree changes
## Key files (start here)
- EImece.Domain/Helpers/ExcelHelper.cs (CreateWorkBook, ExportDataTableToSheet, styles)
- EImece/Areas/Admin/Controllers/BaseAdminController.cs (DownloadFile, DownloadFileDataTable)
- EImece/Areas/Admin/Views/Shared/pGridOperations.cshtml
- Controllers with ExportExcel / ExportExcelAsync
- Reference for format UX: Areas/Admin/Views/Report/_ReportExportButtons.cshtml and ReportController.Export
## Acceptance criteria
- [ ] Grid toolbar lets the user download as Excel or CSV
- [ ] Choosing CSV downloads a .csv; choosing Excel downloads a .xls
- [ ] Excel headers are bold with light yellow background, centered, bordered, Calibri-like header font
- [ ] Data rows use normal font with no special fill
- [ ] Dates show in Turkish format (e.g. 06.08.2026) and are real Excel dates
- [ ] Numbers/booleans keep proper Excel types (not all strings)
- [ ] Columns auto-size
- [ ] First row is frozen
- [ ] AutoFilter is enabled on every exported Excel sheet
- [ ] Report Excel/CSV export still works
- [ ] All changes are on one feature branch with a clear commit message
## Out of scope
- Changing import Excel upload flow
- Migrating away from NPOI
- Unrelated security / Admin UI dirty changes on other branches
