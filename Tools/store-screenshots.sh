#!/usr/bin/env bash
# Takes the App Store screenshots (issue #192): runs the explicit PlayMode fixture StoreScreenshots in batch mode with
# graphics (Metal) and writes five PNGs of 1320x2868 to docs/store/screenshots/. Unity must be closed; with the Editor
# open use „RushOps → Store → Screenshots aufnehmen“ instead. Takes about two minutes.
set -uo pipefail

cd "$(dirname "$0")/.."
export UNITY_NO_BANNER=1 UNITY_NO_UPDATE_CHECK=1 UNITY_NON_INTERACTIVE=1 UNITY_NO_CONSENT_PROMPT=1
mkdir -p Logs

unity test . --mode PlayMode --filter "RushOps.Tests.PlayMode.StoreScreenshots" \
  --output Logs/store-screenshots.xml --timeout 900 --json > Logs/store-screenshots.json 2>/dev/null
rc=$?
python3 - <<'PY'
import xml.etree.ElementTree as ET
try:
    root = ET.parse("Logs/store-screenshots.xml").getroot()
except Exception as e:
    raise SystemExit(f"Kein Testbericht ({e})")
for case in root.iter("test-case"):
    message = (case.findtext("failure/message") or case.findtext("reason/message") or "").strip()
    print(f"{case.get('result')}: {case.get('name')} {message}")
PY
ls -1 docs/store/screenshots/*.png 2>/dev/null
exit $rc
