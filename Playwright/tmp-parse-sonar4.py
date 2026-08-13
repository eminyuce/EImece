import json
import collections

p = r"C:\Users\eminy\Downloads\Eimece-sonarqube-issues-4.json"
with open(p, encoding="utf-8") as f:
    data = json.load(f)
issues = data["issues"]


def is_vendor(i):
    c = i.get("component", "")
    return any(
        x in c
        for x in [
            "tinymce",
            "mstore",
            "packages/",
            "node_modules",
            "Content/bootstrap",
            "jquery",
            "ckeditor",
            "Content/font-awesome",
        ]
    )


print("=== VULNERABILITIES ===")
for i in issues:
    if i["type"] == "VULNERABILITY":
        print(
            f"{i['severity']:8} {i['rule']:40} L{str(i.get('line', '?')):>5} {i['component'].split(':')[-1]}"
        )
        print(f"         {i['message'][:220]}")

print()
print("=== BUGS (non-vendor) ===")
bugs = [i for i in issues if i["type"] == "BUG" and not is_vendor(i)]
print(
    "count",
    len(bugs),
    "vendor bugs",
    sum(1 for i in issues if i["type"] == "BUG" and is_vendor(i)),
)
rules = collections.Counter(i["rule"] for i in bugs)
for r, c in rules.most_common():
    print(f"  {c:3d} {r}")
print()
for i in bugs:
    print(
        f"{i['severity']:8} {i['rule']:40} L{str(i.get('line', '?')):>5} {i['component'].split(':')[-1]}"
    )
    print(f"         {i['message'][:200]}")

print()
print("=== C# CODE SMELLS (non-vendor) by rule ===")
cs = [
    i
    for i in issues
    if i["type"] == "CODE_SMELL"
    and i["rule"].startswith("csharpsquid")
    and not is_vendor(i)
]
print("count", len(cs))
for r, c in collections.Counter(i["rule"] for i in cs).most_common():
    print(f"  {c:3d} {r}")
print()
for i in cs:
    print(
        f"{i['severity']:8} {i['rule']:40} L{str(i.get('line', '?')):>5} {i['component'].split(':')[-1]}"
    )
    print(f"         {i['message'][:180]}")

print()
print("=== WEB/JS/CSS non-vendor counts ===")
for lang in ["Web:", "javascript:", "css:", "external_roslyn:"]:
    items = [i for i in issues if i["rule"].startswith(lang) and not is_vendor(i)]
    print(lang, len(items))
    for r, c in collections.Counter(i["rule"] for i in items).most_common():
        print(f"  {c:3d} {r}")

print()
print("=== CRITICAL all ===")
for i in issues:
    if i["severity"] == "CRITICAL":
        print(
            f"{i['type']:15} {i['rule']:40} L{str(i.get('line', '?')):>5} {i['component'].split(':')[-1]}"
        )
        print(f"         {i['message'][:200]}")

# dump actionable json
actionable = []
for i in issues:
    if is_vendor(i):
        continue
    actionable.append(
        {
            "rule": i["rule"],
            "type": i["type"],
            "sev": i["severity"],
            "file": i["component"].split(":")[-1],
            "line": i.get("line"),
            "msg": i["message"],
            "start": (i.get("textRange") or {}).get("startLine"),
            "end": (i.get("textRange") or {}).get("endLine"),
        }
    )
out = r"C:\Users\eminy\source\repos\EImece\Playwright\tmp-sonar4-actionable.json"
with open(out, "w", encoding="utf-8") as f:
    json.dump(actionable, f, indent=2)
print("wrote", out, "count", len(actionable))
