#!/usr/bin/env bash
# Measures frame times and memory in the editor (issue #189, decision R8): runs the explicit PlayMode fixture
# PerformanceBenchmark (stress scene, one regular level per island, main menu) and prints a Markdown table for
# docs/testprotokolle/performance.md. Raw values land in Logs/perf/<scene>.json.
#
# Runs in batch mode with graphics (Metal), so Unity must be closed. Takes about two minutes.
# Exit codes: 0 ok, 6 run did not finish, 8 a measurement failed.
set -uo pipefail

cd "$(dirname "$0")/.."
export UNITY_NO_BANNER=1 UNITY_NO_UPDATE_CHECK=1 UNITY_NON_INTERACTIVE=1 UNITY_NO_CONSENT_PROMPT=1
mkdir -p Logs
rm -rf Logs/perf

unity test . --mode PlayMode --filter "RushOps.Tests.PlayMode.PerformanceBenchmark" \
  --output Logs/perf-results.xml --timeout 900 --json > Logs/perf-run.json 2>/dev/null
rc=$?

python3 - <<'PY'
import glob, json, os
order = ["Stress_Test", "Level_1-1", "Level_2-1", "Level_3-1", "MainMenu"]
rows = {os.path.basename(p)[:-5]: json.load(open(p)) for p in glob.glob("Logs/perf/*.json")}
if not rows:
    print("Keine Messwerte unter Logs/perf/")
    raise SystemExit(0)
print("| Szene | Gegner | Spiel-CPU Ø / 1 % (ms) | Skripte Ø (ms) | Physik Ø (ms) | GC pro Frame (B) | Frames mit GC | Texturen (MB) | Draw Calls Ø / max | SetPass Ø |")
print("|---|---|---|---|---|---|---|---|---|---|")
for name in order + sorted(set(rows) - set(order)):
    d = rows.get(name)
    if not d:
        continue
    print(f"| {name} | {d['enemiesAlive']} | {d['gameMeanMs']:.2f} / {d['gameWorstPercentMs']:.2f} | "
          f"{d['scriptsMeanMs']:.2f} | {d['physicsMeanMs']:.2f} | {d['gcBytesPerFrame']:.0f} | "
          f"{d['framesWithGc']} von {d['frames']} | {d['peakTextureMemoryMb']:.0f} | "
          f"{d.get('drawCallsPerFrame', 0):.0f} / {d.get('drawCallsWorst', 0)} | {d.get('setPassCallsPerFrame', 0):.0f} |")
PY
exit $rc
