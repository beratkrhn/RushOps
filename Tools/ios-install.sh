#!/usr/bin/env bash
# Builds the Xcode project exported by Unity, signs it with the development team and installs it on a
# connected iPhone (issue #13).
#
# Usage: Tools/ios-install.sh [xcode-project-dir]   (default: newest Builds/iOS/RushOps-*)
# Prerequisites: Xcode with the Apple account signed in (Xcode → Settings → Accounts), iPhone connected
# via USB or the same Wi-Fi, Developer Mode enabled on the iPhone (Settings → Privacy & Security).
set -euo pipefail

cd "$(dirname "$0")/.."
TEAM_ID="${RUSHOPS_TEAM_ID:-WAM6U438Y8}"
PROJECT_DIR="${1:-$(ls -dt Builds/iOS/RushOps-* 2>/dev/null | head -1)}"
if [[ -z "$PROJECT_DIR" || ! -d "$PROJECT_DIR/Unity-iPhone.xcodeproj" ]]; then
  echo "Kein Xcode-Projekt gefunden. Zuerst in Unity: RushOps → Build → iOS-Xcode-Projekt (Development) bauen" >&2
  exit 2
fi

DERIVED="$PROJECT_DIR/DerivedData"
echo "== Baue $PROJECT_DIR (Team $TEAM_ID)"
xcodebuild -project "$PROJECT_DIR/Unity-iPhone.xcodeproj" -scheme Unity-iPhone -configuration Debug \
  -destination 'generic/platform=iOS' -derivedDataPath "$DERIVED" -allowProvisioningUpdates \
  DEVELOPMENT_TEAM="$TEAM_ID" CODE_SIGN_STYLE=Automatic build | grep -E "error|warning: .*sign|BUILD" || true

APP=$(find "$DERIVED/Build/Products" -maxdepth 2 -name "*.app" -path "*iphoneos*" | head -1)
if [[ -z "$APP" ]]; then
  echo "Build fehlgeschlagen, keine .app gefunden (Details: xcodebuild ohne grep erneut ausführen)." >&2
  exit 1
fi

DEVICE=$(xcrun devicectl list devices 2>/dev/null | awk '/iPhone/ && /available|connected/ {print $3; exit}')
if [[ -z "$DEVICE" ]]; then
  echo "Kein iPhone gefunden. Per USB verbinden, entsperren und dem Mac vertrauen." >&2
  echo "Die App liegt unter: $APP" >&2
  exit 3
fi

echo "== Installiere auf $DEVICE"
xcrun devicectl device install app --device "$DEVICE" "$APP"
echo "Fertig. Beim ersten Start auf dem iPhone: Einstellungen → Allgemein → VPN & Geräteverwaltung → Entwickler-App vertrauen."
