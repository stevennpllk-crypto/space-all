#!/usr/bin/env python3
"""Download selected high-resolution images from NASA's official Image and Video Library."""
from __future__ import annotations
import json, re, sys, time, urllib.parse, urllib.request
from pathlib import Path

API = "https://images-api.nasa.gov"
OUT = Path("Assets/Art/NASA")
MAX_BYTES = 12 * 1024 * 1024
SEARCHES = [
    ("earth", "Earth Blue Marble NASA"),
    ("mars", "Mars surface NASA rover panorama"),
    ("jupiter", "Jupiter Juno NASA"),
    ("saturn", "Saturn Cassini NASA"),
    ("nebula", "nebula Hubble NASA"),
    ("galaxy", "galaxy NASA deep field"),
    ("moon", "Moon lunar surface NASA"),
]
IMAGE_EXTS = (".jpg", ".jpeg", ".png", ".webp", ".tif", ".tiff")

def get_json(url):
    req = urllib.request.Request(url, headers={"User-Agent": "GalaxyExplorer-NASA-AssetFetcher/1.0"})
    with urllib.request.urlopen(req, timeout=45) as response:
        return json.load(response)

def safe_name(value):
    value = re.sub(r"[^a-zA-Z0-9]+", "_", value.lower()).strip("_")
    return value[:64] or "nasa_image"

def is_obvious_third_party(item):
    data = item.get("data", [{}])[0]
    text = " ".join(str(data.get(k, "")) for k in ("description", "title", "photographer", "secondary_creator"))
    return bool(re.search(r"copyright\s+(?:©|\(c\)|[A-Z])|all rights reserved|used with permission", text, re.I))

def choose_asset(urls):
    candidates = []
    for entry in urls:
        url = entry.get("href") if isinstance(entry, dict) else entry
        if not isinstance(url, str):
            continue
        low = url.lower().split("?")[0]
        if low.endswith(IMAGE_EXTS) and not any(tag in low for tag in ("thumb", "small", "medium")):
            score = 100 if "~orig" in low else 0
            match = re.search(r"_(\d{3,5})\.", low)
            if match:
                score += int(match.group(1))
            candidates.append((score, url))
    return max(candidates, default=(0, None), key=lambda x: x[0])[1]

def download(url, target):
    req = urllib.request.Request(url, headers={"User-Agent": "GalaxyExplorer-NASA-AssetFetcher/1.0"})
    with urllib.request.urlopen(req, timeout=90) as response:
        content_type = response.headers.get("Content-Type", "")
        payload = response.read(MAX_BYTES + 1)
    if len(payload) > MAX_BYTES:
        raise ValueError("image exceeds 12 MiB safety limit")
    if not content_type.startswith("image/"):
        raise ValueError("unexpected content type: " + content_type)
    target.write_bytes(payload)
    return len(payload)

def main():
    OUT.mkdir(parents=True, exist_ok=True)
    records, used_ids, failures = [], set(), []
    for category, query in SEARCHES:
        try:
            params = urllib.parse.urlencode({"q": query, "media_type": "image", "page_size": 12})
            result = get_json(API + "/search?" + params)
            items = result.get("collection", {}).get("items", [])
            chosen = None
            for item in items:
                data = item.get("data", [{}])[0]
                nasa_id = data.get("nasa_id")
                if not nasa_id or nasa_id in used_ids or is_obvious_third_party(item):
                    continue
                manifest = get_json(API + "/asset/" + urllib.parse.quote(nasa_id, safe=""))
                asset_url = choose_asset(manifest.get("collection", {}).get("items", []))
                if asset_url:
                    chosen = (data, nasa_id, asset_url)
                    break
            if not chosen:
                failures.append(category + ": no suitable image found")
                continue
            data, nasa_id, asset_url = chosen
            ext = Path(urllib.parse.urlparse(asset_url).path).suffix.lower()
            if ext not in IMAGE_EXTS:
                ext = ".jpg"
            filename = category + "_" + safe_name(nasa_id) + ext
            size = download(asset_url, OUT / filename)
            used_ids.add(nasa_id)
            records.append({
                "category": category, "file": filename, "nasa_id": nasa_id,
                "title": data.get("title", ""), "date_created": data.get("date_created", ""),
                "center": data.get("center", ""), "photographer": data.get("photographer", ""),
                "secondary_creator": data.get("secondary_creator", ""),
                "source_page": "https://images.nasa.gov/details/" + nasa_id,
                "asset_url": asset_url, "bytes": size
            })
            print("Downloaded " + filename + " (" + str(size) + " bytes)")
            time.sleep(0.25)
        except Exception as exc:
            failures.append(category + ": " + type(exc).__name__ + ": " + str(exc))
    if not records:
        print("No NASA images were downloaded.", file=sys.stderr)
        for failure in failures:
            print(failure, file=sys.stderr)
        return 1
    (OUT / "NASA_ASSET_CREDITS.json").write_text(
        json.dumps({"source": "NASA Image and Video Library", "api": API, "assets": records, "warnings": failures},
                   ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (OUT / "README.md").write_text(
        "# NASA imagery\n\nOriginal image files downloaded from NASA's official Image and Video Library API. "
        "See NASA_ASSET_CREDITS.json for NASA IDs, source pages, asset URLs, and creator credits.\n\n"
        "NASA media is generally reusable under NASA's media guidelines, but individual items may contain "
        "third-party copyrighted material. Check each credit before redistribution. Do not use NASA insignia "
        "or imply NASA endorsement.\n\n"
        "Unity import compression and platform texture limits should be configured separately for Android performance.\n",
        encoding="utf-8")
    print("Saved " + str(len(records)) + " NASA images.")
    for failure in failures:
        print("Skipped: " + failure)
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
