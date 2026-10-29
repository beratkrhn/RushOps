#!/usr/bin/env bash
# Prints the next issue to work on.
# Rule: oldest open milestone (by SemVer), label "story", issues labeled "priority" first,
# then lowest issue number, skipping issues labeled "blocked", "needs-design" or "in-progress".
# Requires the GitHub CLI (gh), authenticated via `gh auth login`.
# Uses only the REST API, so it also works where GraphQL is unavailable.
set -euo pipefail

REPO="${RUSHOPS_REPO:-beratkrhn/RushOps}"

# "number<TAB>title" of open milestones, sorted by SemVer title
milestones=$(gh api "repos/$REPO/milestones?state=open&per_page=100" \
  --jq 'sort_by(.title | ltrimstr("v") | split(".") | map(tonumber? // 0)) | .[] | "\(.number)\t\(.title)"')

while IFS=$'\t' read -r ms_number ms_title; do
  [[ -z "$ms_number" ]] && continue
  next=$(gh api "repos/$REPO/issues?milestone=$ms_number&state=open&labels=story&per_page=100" \
    --jq '[.[] | select(.pull_request == null)
               | select([.labels[].name] | any(. == "blocked" or . == "needs-design" or . == "in-progress") | not)]
          | sort_by([([.labels[].name] | any(. == "priority") | not), .number])
          | .[0] // empty | "#\(.number) \(.title)"')
  if [[ -n "$next" ]]; then
    echo "Milestone: $ms_title"
    echo "Nächstes Issue: $next"
    exit 0
  fi
done <<< "$milestones"

echo "Kein offenes Issue gefunden, das bearbeitet werden kann."
exit 1
