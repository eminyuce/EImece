#!/usr/bin/env python3
"""Safe NLog -> MEL migration: inject ILogger<T> as last ctor param and pass to base()."""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SKIP = {"bin", "obj", "packages", "build-tools", "Observability/Logging/LoggingBootstrap.cs"}

STATIC_LOGGER = re.compile(
    r"^\s*(?:private|protected)\s+static\s+readonly\s+Logger\s+\w+\s*=\s*LogManager\.GetCurrentClassLogger\(\);\s*\n",
    re.MULTILINE,
)
INSTANCE_LOGGER = re.compile(
    r"^\s*(?:private|protected)\s+readonly\s+Logger\s+\w+\s*=\s*LogManager\.GetCurrentClassLogger\(\);\s*\n",
    re.MULTILINE,
)


def class_name(content: str):
    m = re.search(r"\bclass\s+(\w+)", content)
    return m.group(1) if m else None


def migrate(content: str, name: str) -> str:
    if "LogManager" not in content and "using NLog" not in content:
        return content

    cn = class_name(content)
    if not cn:
        return content

    content = STATIC_LOGGER.sub("", content)
    content = INSTANCE_LOGGER.sub("", content)
    content = re.sub(r"^using NLog;\s*\n", "", content, flags=re.MULTILINE)
    if "using Microsoft.Extensions.Logging" not in content:
        content = "using Microsoft.Extensions.Logging;\n" + content

    if f"ILogger<{cn}>" in content:
        pass
    else:
        # Find first constructor for this class
        ctor_pat = re.compile(
            rf"(public|protected)\s+{re.escape(cn)}\s*\(([^)]*)\)\s*(\:\s*base\([^)]*\))?\s*\{{",
            re.DOTALL,
        )
        m = ctor_pat.search(content)
        if m:
            params = m.group(2).strip()
            base_clause = m.group(3) or ""
            new_params = f"{params}, ILogger<{cn}> logger" if params else f"ILogger<{cn}> logger"
            if base_clause:
                base_clause = re.sub(r"\)\s*$", ", logger)", base_clause.rstrip())
            new_ctor_header = f"{m.group(1)} {cn}({new_params}) {base_clause}".strip() + " {"
            content = content[: m.start()] + new_ctor_header + content[m.end() - 1 :]

            # Add assignment if using own field pattern for non-base classes
            if "BaseController" not in content and "BaseAdminController" not in content:
                if f"ILogger<{cn}>" not in content.split("{", 1)[0]:
                    pass
            if "_logger" not in content and "protected readonly ILogger Logger" not in content:
                insert = f"        private readonly ILogger<{cn}> _logger;\n\n"
                cpos = content.find("{", content.find(f"class {cn}"))
                if cpos > 0 and insert.strip() not in content:
                    content = content[: cpos + 1] + "\n" + insert + content[cpos + 1 :]
                # assign in ctor body
                body_start = content.find("{", content.find(f"{cn}("))
                if body_start > 0 and "_logger = logger" not in content:
                    content = (
                        content[: body_start + 1]
                        + "\n            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));"
                        + content[body_start + 1 :]
                    )

    # Map common static logger names to _logger or Logger
    for old in ("HomeLogger", "ProductServiceLogger", "StoryServiceLogger", "TagServiceLogger",
                "TagCategoryServiceLogger", "StoryCategoryServiceLogger", "ProductCategoryServiceLogger",
                "BaseEntityServiceLogger", "BaseContentServiceLogger", "Logger"):
        if old != "Logger":
            content = content.replace(f"{old}.", "Logger." if "BaseController" in content or ": Base" in content else "_logger.")

    content = re.sub(r"\b(\w+)\.Info\(", r"\1.LogInformation(", content)
    content = re.sub(r"\b(\w+)\.Error\(", r"\1.LogError(", content)
    content = re.sub(r"\b(\w+)\.Debug\(", r"\1.LogDebug(", content)
    content = re.sub(r"\b(\w+)\.Warn\(", r"\1.LogWarning(", content)
    content = re.sub(r"\b(\w+)\.Trace\(", r"\1.LogTrace(", content)
    content = re.sub(r"\b(\w+)\.Fatal\(", r"\1.LogCritical(", content)

    return content


def process(path: Path) -> bool:
    if any(s in str(path) for s in SKIP):
        return False
    text = path.read_text(encoding="utf-8")
    new = migrate(text, path.name)
    if new != text:
        path.write_text(new, encoding="utf-8")
        return True
    return False


def main():
    changed = []
    for cs in ROOT.rglob("*.cs"):
        if any(p in cs.parts for p in ("bin", "obj", "packages")):
            continue
        if process(cs):
            changed.append(cs)
    print(len(changed), "files updated")


if __name__ == "__main__":
    main()
