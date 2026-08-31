#!/usr/bin/env python3
"""Fix base(...) constructor chains after ILogger migration."""

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

REPO_BASE = re.compile(r":\s*base\(\s*([^,)]+)\s*\)(\s*\{)", re.MULTILINE)
REPO_ENTITY_BASE = re.compile(r":\s*base\(\s*([^,)]+)\s*\)(\s*\{)", re.MULTILINE)

CONTENT_SERVICE_BASE = re.compile(
    r":\s*base\(\s*baseContentRepository,\s*dataCachingProvider\s*\)",
)
CONTENT_SERVICE_BASE2 = re.compile(
    r":\s*base\(\s*baseContentRepository,\s*isCachingActivated,\s*dataCachingProvider\s*\)",
)
ENTITY_SERVICE_BASE = re.compile(
    r":\s*base\(\s*baseEntityRepository,\s*dataCachingProvider\s*\)",
)
ENTITY_SERVICE_BASE2 = re.compile(
    r":\s*base\(\s*baseEntityRepository,\s*isCachingActivated,\s*dataCachingProvider\s*\)",
)
CONTROLLER_BASE = re.compile(
    r":\s*base\(\s*settingService,\s*mapper\s*\)",
)
ADMIN_BASE = re.compile(
    r":\s*base\(\s*settingService\s*\)",
)


def fix_file(path: Path) -> bool:
    text = path.read_text(encoding="utf-8")
    if "_logger" not in text and "ILogger" not in text:
        return False
    orig = text

    if "Repository" in path.name:
        if "BaseEntityRepository" not in path.name and "BaseContentRepository" not in path.name and "BaseRepository" not in path.name:
            if ": base(dbContext, _logger)" not in text and "ILogger<" in text:
                text = re.sub(
                    r":\s*base\(\s*([^,)]+)\s*\)",
                    r": base(\1, _logger)",
                    text,
                    count=1,
                )

    text = CONTENT_SERVICE_BASE.sub(": base(baseContentRepository, dataCachingProvider, _logger)", text)
    text = CONTENT_SERVICE_BASE2.sub(
        ": base(baseContentRepository, isCachingActivated, dataCachingProvider, _logger)", text
    )
    text = ENTITY_SERVICE_BASE.sub(": base(baseEntityRepository, dataCachingProvider, _logger)", text)
    text = ENTITY_SERVICE_BASE2.sub(
        ": base(baseEntityRepository, isCachingActivated, dataCachingProvider, _logger)", text
    )
    text = CONTROLLER_BASE.sub(": base(settingService, mapper, _logger)", text)
    text = ADMIN_BASE.sub(": base(settingService, _logger)", text)

    if text != orig:
        path.write_text(text, encoding="utf-8")
        return True
    return False


def main():
    changed = 0
    for cs in ROOT.rglob("*.cs"):
        if "bin" in cs.parts or "obj" in cs.parts:
            continue
        if fix_file(cs):
            changed += 1
    print(f"Fixed base calls in {changed} files")


if __name__ == "__main__":
    main()
