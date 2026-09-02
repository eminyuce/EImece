# Fix Bootstrap 3 admin SaveOrEdit form alignment

- **Captured:** 2026-08-07 1:05:56 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

Fix Bootstrap 3 horizontal form alignment on Admin SaveOrEdit pages

Context
ASP.NET MVC admin area. Bootstrap 3 horizontal forms use this pattern:

<div class="form-horizontal">
  <div class="form-group">
    <label class="control-label col-md-2" for="Name">...</label>
    <div class="col-md-10">
      <input class="form-control" ... />
    </div>
  </div>
</div>
Working reference (correct alignment)
http://localhost:81/admin/tags/saveoredit/2480/
View: EImece/EImece/Areas/Admin/Views/Tags/SaveOrEdit.cshtml
Fields are direct children of .form-horizontal (no panel/tabs wrapping the form-groups).
Broken pages (misaligned inputs, especially the first field)
http://localhost:81/admin/storycategories/saveoredit/56/
http://localhost:81/admin/faq/saveoredit/192/
http://localhost:81/admin/mainpageimages/saveoredit/2133/
http://localhost:81/admin/stories/saveoredit/121/
http://localhost:81/admin/brands/saveoredit/1202/
Views under EImece/EImece/Areas/Admin/Views/{StoryCategories,Faq,MainPageImages,Stories,Brands}/SaveOrEdit.cshtml

Observed problem
Rendered markup looks like a valid form-group + control-label col-md-2 + col-md-10, but layout alignment is wrong compared to Tags. Example of the first broken field:

<div class="form-group">
  <label class="control-label col-md-2" for="Name">İsim</label>
  <div class="col-md-10">
    <input class="form-control ..." id="Name" name="Name" ...>
  </div>
</div>
On broken pages, fields sit inside nested structure like: .form-horizontal → .panel → .panel-body → .tab-content → .tab-pane → .form-group

Tags does not use that nesting for the form fields.

Task
Diff Tags SaveOrEdit.cshtml against the broken ones; find why Bootstrap grid/horizontal form alignment breaks (especially the first input).
Fix layout so labels and inputs align like Tags on all listed pages.
Prefer the smallest structural fix that restores Bootstrap 3 horizontal form behavior (e.g. ensure each tab pane’s fields are under a proper .form-horizontal / correct clearfix/form-group structure — match the working Tags pattern).
Do not change business logic, field names, validation, or unrelated UI.
Ignore _publish/ copies; edit source views only.
After fixing, confirm all form-group rows with col-md-2 / col-md-10 align consistently across tabs if present.
Done when
On the broken URLs, the first and subsequent inputs align like Tags: label in col-md-2, control in col-md-10, consistent horizontal Bootstrap layout.
