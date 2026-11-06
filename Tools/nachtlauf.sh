#!/usr/bin/env bash
# Unattended issue runner ("Nachtmodus").
# Starts one fresh Claude Code session per issue, so every issue begins with a clean context.
# Stops when no issue is left, when a session writes Logs/nachtlauf.stop, when a session
# exits with an error (e.g. usage limit), or after max_issues sessions.
#
# Usage: Tools/nachtlauf.sh [max_issues] [model]
#   max_issues  default 8
#   model       default opus (any value accepted by `claude --model`, e.g. sonnet)
#
# Permissions: by default edits are accepted and only the commands listed below may run.
# NACHTLAUF_SKIP_PERMISSIONS=1 Tools/nachtlauf.sh  skips all permission checks (use with care).
set -uo pipefail

cd "$(dirname "$0")/.."

MAX_ISSUES="${1:-8}"
MODEL="${2:-opus}"
LOG_DIR="Logs"
STOP_FILE="$LOG_DIR/nachtlauf.stop"
mkdir -p "$LOG_DIR"
RUN_LOG="$LOG_DIR/nachtlauf-$(date +%Y-%m-%d_%H%M).log"

if [[ -f "$STOP_FILE" ]]; then
  echo "A previous run asked to stop:"
  cat "$STOP_FILE"
  echo "Fix the cause, then remove the file and start again: rm $STOP_FILE"
  exit 1
fi

for tool in claude gh git; do
  command -v "$tool" >/dev/null || { echo "Missing command: $tool"; exit 1; }
done

if [[ -n "$(git status --porcelain)" ]]; then
  echo "Working tree is not clean. Commit or stash first."
  exit 1
fi

# Keep the Mac awake while this script runs (lid open, power connected).
if command -v caffeinate >/dev/null; then
  caffeinate -dimsu -w $$ &
fi

PROMPT='Nachtmodus: Mach das nächste Issue. Folge CLAUDE.md, Abschnitt „Nachtmodus“. Genau ein Issue, dann beenden.'

ALLOWED_TOOLS="Read,Edit,Write,Glob,Grep,TodoWrite,\
Bash(git:*),Bash(gh:*),Bash(Tools/unity-check.sh:*),Bash(Tools/next-issue.sh:*),Bash(unity:*),\
Bash(python3:*),Bash(ls:*),Bash(cat:*),Bash(head:*),Bash(tail:*),Bash(grep:*),Bash(find:*),\
Bash(wc:*),Bash(diff:*),Bash(sort:*),Bash(mkdir:*),Bash(mv:*),Bash(cp:*),Bash(date:*),\
Bash(echo:*),Bash(sed:*),Bash(jq:*),Bash(sleep:*),Bash(touch:*)"

if [[ "${NACHTLAUF_SKIP_PERMISSIONS:-0}" == "1" ]]; then
  PERMS=(--dangerously-skip-permissions)
else
  PERMS=(--permission-mode acceptEdits --allowedTools "$ALLOWED_TOOLS")
fi

echo "Nachtlauf started $(date '+%F %T'), model $MODEL, up to $MAX_ISSUES issues. Log: $RUN_LOG" | tee -a "$RUN_LOG"

for ((i = 1; i <= MAX_ISSUES; i++)); do
  if [[ -f "$STOP_FILE" ]]; then
    echo "Stop requested:" | tee -a "$RUN_LOG"
    tee -a "$RUN_LOG" < "$STOP_FILE"
    break
  fi

  if ! next="$(Tools/next-issue.sh 2>&1)"; then
    echo "$next" | tee -a "$RUN_LOG"
    break
  fi

  echo "=== Run $i/$MAX_ISSUES $(date '+%F %T')" | tee -a "$RUN_LOG"
  echo "$next" | tee -a "$RUN_LOG"

  claude -p "$PROMPT" --model "$MODEL" --max-turns 400 "${PERMS[@]}" >>"$RUN_LOG" 2>&1
  status=$?
  echo "=== Run $i finished with exit code $status at $(date '+%F %T')" | tee -a "$RUN_LOG"

  if (( status != 0 )); then
    echo "Claude exited with code $status (see $RUN_LOG). Stopping." | tee "$STOP_FILE"
    break
  fi
done

echo "Nachtlauf finished $(date '+%F %T'). Summary: $LOG_DIR/nachtlauf.md, details: $RUN_LOG" | tee -a "$RUN_LOG"
