#!/usr/bin/env python3
"""Migrate NLog static loggers to Microsoft.Extensions.Logging ILogger<T> constructor injection."""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

SKIP_DIRS = {"bin", "obj", "packages", "build-tools", "node_modules"}

NLOG_USING = re.compile(r"^using NLog;\s*$", re.MULTILINE)
STATIC_LOGGER = re.compile(
    r"^\s*(?:private|protected)\s+static\s+readonly\s+Logger\s+(\w+)\s*=\s*LogManager\.GetCurrentClassLogger\(\);\s*$",
    re.MULTILINE,
)
INSTANCE_NLOG_LOGGER = re.compile(
    r"^\s*(?:private|protected)\s+readonly\s+Logger\s+(\w+)\s*=\s*LogManager\.GetCurrentClassLogger\(\);\s*$",
    re.MULTILINE,
)

METHOD_MAP = [
    (re.compile(r"\b(\w+)\.Fatal\s*\("), r"\1.LogCritical("),
    (re.compile(r"\b(\w+)\.Error\s*\("), r"\1.LogError("),
    (re.compile(r"\b(\w+)\.Warn\s*\("), r"\1.LogWarning("),
    (re.compile(r"\b(\w+)\.Info\s*\("), r"\1.LogInformation("),
    (re.compile(r"\b(\w+)\.Debug\s*\("), r"\1.LogDebug("),
    (re.compile(r"\b(\w+)\.Trace\s*\("), r"\1.LogTrace("),
]

LOGGER_NAMES = {
    "Logger", "BaseLogger", "BaseEntityLogger", "BaseContentLogger",
    "BaseEntityServiceLogger", "BaseContentServiceLogger", "HomeLogger",
    "ProductServiceLogger", "StoryServiceLogger", "TagServiceLogger",
    "TagCategoryServiceLogger", "StoryCategoryServiceLogger",
    "ProductCategoryServiceLogger", "_logger",
}


def find_class_name(content: str) -> str | None:
    m = re.search(r"^\s*(?:public|internal|protected|private)?\s*(?:abstract|sealed|partial)?\s*class\s+(\w+)", content, re.MULTILINE)
    return m.group(1) if m else None


def find_best_constructor(content: str):
    pattern = re.compile(
        r"((?:public|protected|internal)\s+\w+\s*\([^)]*\)\s*(?::\s*base\([^)]*\))?\s*\{)",
        re.DOTALL,
    )
    ctors = []
    for m in pattern.finditer(content):
        sig_start = content.rfind("\n", 0, m.start()) + 1
        sig_text = content[sig_start:m.end()]
        if "LogManager" in sig_text:
            continue
        param_match = re.search(r"\(([^)]*)\)", sig_text)
        params = param_match.group(1).strip() if param_match else ""
        param_count = 0 if not params else len([p for p in params.split(",") if p.strip()])
        ctors.append((param_count, m.start(), m.end(), sig_text, params))
    if not ctors:
        return None
    ctors.sort(key=lambda x: (-x[0], x[1]))
    return ctors[0]


def inject_logger_into_constructor(content: str, class_name: str, logger_field: str) -> str:
    best = find_best_constructor(content)
    if best is None:
        # Create minimal constructor before first method/property
        insert_at = re.search(r"\n\s*(?:public|protected|private|internal)\s+", content)
        if not insert_at:
            return content
        ctor = (
            f"\n        private readonly ILogger<{class_name}> {logger_field};\n\n"
            f"        public {class_name}(ILogger<{class_name}> {logger_field})\n"
            f"        {{\n            this.{logger_field} = {logger_field};\n        }}\n"
        )
        return content[:insert_at.start()] + ctor + content[insert_at.start():]

    _, start, end, sig_text, params = best
    if f"ILogger<{class_name}>" in sig_text or "ILogger<" in sig_text and logger_field in sig_text:
        return content

    new_param = f"ILogger<{class_name}> {logger_field}"
    if params.strip():
        new_params = params.rstrip() + f", {new_param}"
    else:
        new_params = new_param

    new_sig = re.sub(r"\([^)]*\)", f"({new_params})", sig_text, count=1)
    # Add field if missing
    field_decl = f"private readonly ILogger<{class_name}> {logger_field};"
    if field_decl not in content:
        class_open = re.search(r"class\s+" + re.escape(class_name), content)
        if class_open:
            brace = content.find("{", class_open.end())
            content = content[: brace + 1] + f"\n        {field_decl}\n" + content[brace + 1 :]

    # Add assignment in constructor body
    if f"this.{logger_field} = {logger_field}" not in content and f"{logger_field} = {logger_field}" not in content:
        body_open = content.find("{", start + len(new_sig) - len(sig_text))
        if body_open >= 0:
            assign = f"\n            this.{logger_field} = {logger_field} ?? throw new System.ArgumentNullException(nameof({logger_field}));"
            # insert after base() call if present
            base_call = re.search(r":\s*base\([^)]*\)", new_sig)
            if base_call:
                # find opening brace of ctor
                brace_pos = content.find("{", end - 1)
                content = content[: brace_pos + 1] + assign + content[brace_pos + 1 :]
            else:
                content = content[: body_open + 1] + assign + content[body_open + 1 :]

    content = content[:start] + new_sig + content[end:]
    return content


def migrate_file(path: Path) -> bool:
    text = path.read_text(encoding="utf-8")
    if "LogManager" not in text and "using NLog" not in text:
        return False

    original = text
    class_name = find_class_name(text)
    if not class_name:
        return False

    logger_field = "_logger"
    static_removed = False

    for pat in (STATIC_LOGGER, INSTANCE_NLOG_LOGGER):
        m = pat.search(text)
        if m:
            old_name = m.group(1)
            text = pat.sub("", text, count=1)
            static_removed = True
            # map old logger variable names to _logger in method calls
            if old_name != "_logger":
                text = re.sub(rf"\b{re.escape(old_name)}\.", "_logger.", text)

    if "using Microsoft.Extensions.Logging" not in text:
        if NLOG_USING.search(text):
            text = NLOG_USING.sub("using Microsoft.Extensions.Logging;", text)
        else:
            text = "using Microsoft.Extensions.Logging;\n" + text

    text = text.replace("using NLog;\n", "")

    if static_removed and f"ILogger<{class_name}>" not in text:
        text = inject_logger_into_constructor(text, class_name, logger_field)

    for pat, repl in METHOD_MAP:
        text = pat.sub(repl, text)

    # Base class protected loggers
    text = text.replace("BaseLogger.", "Logger.")
    text = text.replace("BaseEntityLogger.", "Logger.")
    text = text.replace("BaseContentLogger.", "Logger.")
    text = text.replace("BaseEntityServiceLogger.", "Logger.")
    text = text.replace("BaseContentServiceLogger.", "Logger.")

    if text != original:
        path.write_text(text, encoding="utf-8")
        return True
    return False


def main():
    changed = []
    for cs in ROOT.rglob("*.cs"):
        if any(part in SKIP_DIRS for part in cs.parts):
            continue
        if migrate_file(cs):
            changed.append(cs)
    print(f"Migrated {len(changed)} files")
    for p in sorted(changed):
        print(f"  {p.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
