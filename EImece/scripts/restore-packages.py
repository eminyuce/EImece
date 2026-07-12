#!/usr/bin/env python3
import glob
import os
import subprocess
import urllib.request
import xml.etree.ElementTree as ET

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))


def restore_package(package_id, version, destination_root):
    dest = os.path.join(destination_root, f"{package_id}.{version}")
    marker = os.path.join(dest, ".nupkg_extracted")
    if os.path.exists(marker):
        return

    url = f"https://www.nuget.org/api/v2/package/{package_id}/{version}"
    nupkg = os.path.join("/tmp", f"{package_id}.{version}.nupkg")
    print(f"Downloading {package_id} {version}...")
    urllib.request.urlretrieve(url, nupkg)
    os.makedirs(dest, exist_ok=True)
    subprocess.run(["unzip", "-q", "-o", nupkg, "-d", dest], check=True)
    open(marker, "w").close()


packages = {}

for path in glob.glob(os.path.join(ROOT, "**", "packages.config"), recursive=True):
    root = ET.parse(path).getroot()
    for pkg in root.findall("package"):
        packages[(pkg.attrib["id"], pkg.attrib["version"])] = True

packages_dir = os.path.join(ROOT, "packages")
os.makedirs(packages_dir, exist_ok=True)

for (package_id, version) in sorted(packages.keys()):
    restore_package(package_id, version, packages_dir)

build_tools_dir = os.path.join(ROOT, "build-tools")
os.makedirs(build_tools_dir, exist_ok=True)
build_tool_packages = (
    ("Microsoft.NETFramework.ReferenceAssemblies.net472", "1.0.3"),
    ("MSBuild.Microsoft.VisualStudio.Web.targets", "14.0.0.3"),
)

for package_id, version in build_tool_packages:
    restore_package(package_id, version, build_tools_dir)

print(f"Restored {len(packages)} packages and {len(build_tool_packages)} build tool packages.")
