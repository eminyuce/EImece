import json
from collections import defaultdict

d = json.load(
    open(
        r"C:\Users\eminy\source\repos\EImece\Playwright\tmp-sonar4-actionable.json",
        encoding="utf-8",
    )
)
cs = [
    i
    for i in d
    if i["rule"].startswith("csharpsquid") or i["rule"].startswith("external_roslyn")
]
by = defaultdict(list)
for i in cs:
    by[i["file"]].append(i)
print("C# files", len(by), "issues", len(cs))
for f, items in sorted(by.items(), key=lambda x: -len(x[1])):
    print("\n##", f, len(items))
    for i in items:
        print(f"  {i['sev']:8} {i['rule']:28} L{i['line']} {i['msg'][:160]}")

print("\n\n===== WEB remaining =====")
web = [i for i in d if i["rule"].startswith("Web:")]
byw = defaultdict(list)
for i in web:
    byw[i["file"]].append(i)
for f, items in sorted(byw.items(), key=lambda x: -len(x[1])):
    print("\n##", f, len(items))
    for i in items:
        print(f"  {i['sev']:8} {i['rule']:40} L{i['line']} {i['msg'][:140]}")

print("\n\n===== CSS/JS =====")
for i in d:
    if i["rule"].startswith("css:") or i["rule"].startswith("javascript:") or i["rule"].startswith("xml:") or i["rule"].startswith("text:"):
        print(f"  {i['sev']:8} {i['rule']:28} L{i['line']} {i['file']} | {i['msg'][:140]}")
