#!/usr/bin/env bash
# Compiles RushOps and runs its tests through the Unity CLI (ADR 0007).
#
# With a running Editor that has the Pipeline package (GUI or headless), everything runs
# inside that Editor and Unity stays open. Without one, it falls back to batch mode via
# `unity run` / `unity test`, which needs Unity to be closed.
#
# Usage: Tools/unity-check.sh [all|compile|editmode|playmode]   (default: all)
# Exit codes: 0 ok, 1 compile errors or warnings, 8 tests failed,
#             6 a step did not finish (timeout, Editor busy), 2 usage error.
# Requires the Unity CLI (`brew install --cask unity-cli`) and python3.
set -uo pipefail

cd "$(dirname "$0")/.."
export UNITY_NO_BANNER=1 UNITY_NO_UPDATE_CHECK=1 UNITY_NON_INTERACTIVE=1 UNITY_NO_CONSENT_PROMPT=1

MODE="${1:-all}"
LOG_DIR="Logs"
PLAYMODE_TIMEOUT=600
# The Pipeline server is briefly unreachable (exit 7) while the Editor reloads its domain.
RELOAD_RETRIES=6
RELOAD_WAIT=10

case "$MODE" in
  all|compile|editmode|playmode) ;;
  *) echo "Usage: $0 [all|compile|editmode|playmode]" >&2; exit 2 ;;
esac
mkdir -p "$LOG_DIR"

# Runs a CLI call with --json, retrying while the Editor reloads. Output goes to file $1.
cli_json() {
  local out="$1"; shift
  local rc
  for _ in $(seq 1 "$RELOAD_RETRIES"); do
    unity "$@" --project-path . --json > "$out" 2>/dev/null
    rc=$?
    if [[ $rc -ne 7 ]] && ! grep -q "No Pipeline instance found" "$out"; then
      return $rc
    fi
    sleep "$RELOAD_WAIT"
  done
  return $rc
}

# Waits until the Editor is idle again: after a recompile it reloads its domain.
wait_until_ready() {
  for _ in $(seq 1 60); do
    if cli_json "$LOG_DIR/check-editor.json" command editor status \
       && [[ "$(json_field "$LOG_DIR/check-editor.json" data.result.status)" == "ready" ]] \
       && [[ "$(json_field "$LOG_DIR/check-editor.json" data.result.compiling)" == "False" ]] \
       && [[ "$(json_field "$LOG_DIR/check-editor.json" data.result.domainReloadInProgress)" == "False" ]]; then
      return 0
    fi
    sleep 2
  done
  echo "  Editor wird nicht wieder bereit"
  return 6
}

# Prints a short report from a JSON file: kind is compile, tests or status.
report() {
  python3 - "$1" "$2" <<'EOF'
import json, sys
kind, path = sys.argv[1], sys.argv[2]
try:
    doc = json.load(open(path))
except Exception as e:
    print(f"  Keine lesbare Antwort ({e})"); sys.exit(0)
data = doc.get("data") or {}
for err in doc.get("errors") or []:
    print("  Fehler:", err.get("message", err))
if kind == "compile":
    for item in (data.get("errors") or []) + (data.get("warnings") or []):
        print("  " + item.get("raw", str(item)))
    s = data.get("summary") or {}
    print(f"  Kompilieren: {s.get('errors', '?')} Fehler, {s.get('warnings', '?')} Warnungen")
    sys.exit(0)
result = data.get("result") or {}
summary = result.get("Summary") or result.get("summary") or {}
results = result.get("Results") or result.get("results") or []
for r in results:
    status = r.get("Status") or r.get("status")
    if status not in ("Passed", "passed"):
        name = r.get("FullName") or r.get("fullName") or r.get("name")
        print(f"  {status}: {name}\n    {r.get('Message') or r.get('message') or ''}")
total = summary.get("Total", summary.get("total", "?"))
passed = summary.get("Passed", summary.get("passed", "?"))
failed = summary.get("Failed", summary.get("failed", "?"))
print(f"  Tests: {passed}/{total} bestanden, {failed} fehlgeschlagen")
EOF
}

# Reads one field from a JSON file, e.g. json_field file data.result.status
json_field() {
  python3 -c '
import json, sys
value = json.load(open(sys.argv[1]))
for key in sys.argv[2].split("."):
    value = value.get(key) if isinstance(value, dict) else None
print("" if value is None else (json.dumps(value) if isinstance(value, (dict, list)) else value))
' "$1" "$2" 2>/dev/null
}

# Number of failed tests in a JSON file (summary in data.result or data.result.summary).
failed_count() {
  local n
  n=$(json_field "$1" data.result.Summary.Failed)
  [[ -z "$n" ]] && n=$(json_field "$1" data.result.summary.failed)
  echo "${n:-0}"
}

live_compile() {
  echo "== Kompilieren (offener Editor)"
  cli_json "$LOG_DIR/check-compile.json" recompile --strict
  local rc=$?
  report compile "$LOG_DIR/check-compile.json"
  [[ $rc -eq 0 ]] || return 1
  wait_until_ready
}

live_editmode() {
  echo "== EditMode-Tests (offener Editor)"
  cli_json "$LOG_DIR/check-editmode.json" command test run --mode editor --timeout 600 || {
    report tests "$LOG_DIR/check-editmode.json"; return 6; }
  report tests "$LOG_DIR/check-editmode.json"
  [[ "$(failed_count "$LOG_DIR/check-editmode.json")" == "0" ]] || return 8
}

live_playmode() {
  echo "== PlayMode-Tests (offener Editor)"
  # PlayMode reloads the domain, so the run is started async and polled.
  cli_json "$LOG_DIR/check-playmode.json" command test run --mode playmode --async_tests true || {
    report tests "$LOG_DIR/check-playmode.json"; return 6; }
  local status="" waited=0
  while (( waited < PLAYMODE_TIMEOUT )); do
    sleep 3; waited=$((waited + 3))
    cli_json "$LOG_DIR/check-playmode-status.json" command test status || continue
    status=$(json_field "$LOG_DIR/check-playmode-status.json" data.result.status)
    [[ "$status" != "running" ]] && break
  done
  if [[ "$status" != "completed" ]]; then
    echo "  PlayMode-Lauf nicht abgeschlossen (Status: ${status:-unbekannt})"
    unity command test cancel --project-path . --json > /dev/null 2>&1
    return 6
  fi
  report tests "$LOG_DIR/check-playmode-status.json"
  [[ "$(failed_count "$LOG_DIR/check-playmode-status.json")" == "0" ]] || return 8
}

batch_compile() {
  echo "== Kompilieren (Batchmode)"
  unity run . --log-file "$LOG_DIR/check-compile.log" --no-tail -- -nographics > /dev/null 2>&1
  local rc=$?
  grep -E "(error|warning) CS[0-9]+" "$LOG_DIR/check-compile.log" | sort -u | sed 's/^/  /'
  if [[ $rc -ne 0 ]] || grep -qE "(error|warning) CS[0-9]+" "$LOG_DIR/check-compile.log"; then
    echo "  Kompilieren fehlgeschlagen oder mit Warnungen (Log: $LOG_DIR/check-compile.log)"
    return 1
  fi
  echo "  Kompilieren: 0 Fehler, 0 Warnungen"
}

batch_tests() {
  local mode="$1" lower
  lower=$(echo "$1" | tr '[:upper:]' '[:lower:]')
  local out="$LOG_DIR/check-$lower.json" xml="$LOG_DIR/$lower-results.xml"
  echo "== $mode-Tests (Batchmode)"
  rm -f "$xml"
  unity test . --mode "$mode" --output "$xml" --timeout 900 --json -- -nographics > "$out" 2>/dev/null
  local rc=$?
  # `unity test` exits 0 / 8 / 6 itself; the counts are only in the NUnit report.
  python3 - "$out" "$xml" <<'EOF'
import json, sys
import xml.etree.ElementTree as ET
try:
    for err in json.load(open(sys.argv[1])).get("errors") or []:
        print("  Fehler:", err.get("message", err))
except Exception as e:
    print(f"  Keine lesbare Antwort ({e})")
try:
    root = ET.parse(sys.argv[2]).getroot()
except Exception:
    sys.exit(0)
for case in root.iter("test-case"):
    if case.get("result") not in ("Passed", "Skipped"):
        message = case.findtext("failure/message") or ""
        print(f"  {case.get('result')}: {case.get('fullname')}\n    {message.strip()}")
print(f"  Tests: {root.get('passed')}/{root.get('total')} bestanden, {root.get('failed')} fehlgeschlagen")
EOF
  return $rc
}

# A Pipeline Editor answers `editor status`; exit 7 or an error means none is reachable.
if unity command editor status --project-path . --json > "$LOG_DIR/check-editor.json" 2>/dev/null \
   && [[ "$(json_field "$LOG_DIR/check-editor.json" success)" == "True" ]]; then
  echo "Offener Unity-Editor gefunden, prüfe darin."
  # A background Editor throttles its frames; auto-tick keeps PlayMode tests at full speed.
  unity command editor set_auto_tick --enable true --project-path . --json > /dev/null 2>&1
  live=1
else
  echo "Kein offener Editor mit Pipeline-Paket erreichbar, nutze Batchmode (Unity muss geschlossen sein)."
  live=0
fi

overall=0
run_step() {
  "$@"
  local rc=$?
  if [[ $rc -ne 0 && $overall -eq 0 ]]; then overall=$rc; fi
  return $rc
}

if [[ $live -eq 1 ]]; then
  run_step live_compile || exit $overall
  [[ "$MODE" == all || "$MODE" == editmode ]] && run_step live_editmode
  [[ "$MODE" == all || "$MODE" == playmode ]] && run_step live_playmode
else
  run_step batch_compile || exit $overall
  [[ "$MODE" == all || "$MODE" == editmode ]] && run_step batch_tests EditMode
  [[ "$MODE" == all || "$MODE" == playmode ]] && run_step batch_tests PlayMode
fi

if [[ $overall -eq 0 ]]; then echo "OK"; else echo "FEHLGESCHLAGEN (Exit $overall)"; fi
exit $overall
