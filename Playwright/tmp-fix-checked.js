const fs = require("fs");
const p = "C:/Users/eminy/source/repos/EImece/EImece/EImece/Areas/Admin/Views/AdminSettings/SystemSettings.cshtml";
let s = fs.readFileSync(p, "utf8");
if (s.charCodeAt(0) === 0xFEFF) s = s.slice(1);
const oldStr = '@(isChecked ? "checked="checked"" : "")';
const newStr = '@(isChecked ? "checked=\\"checked\\"" : null)';
if (!s.includes(oldStr)) { console.error("needle missing"); process.exit(1); }
const n = s.split(oldStr).length - 1;
s = s.replace(oldStr, newStr);
fs.writeFileSync(p, s);
console.log("replaced", n);
