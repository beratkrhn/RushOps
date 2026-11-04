#!/usr/bin/env bash
# Builds RushOps for iOS and installs it on a connected iPhone (issue #13).
#
#   Tools/ios-install.sh            export the Xcode project with Unity (batch mode), build, sign, install, launch
#   Tools/ios-install.sh --no-export reuse the newest Xcode project in Builds/iOS/ (e.g. after building from the menu)
#
# Unity must be closed for the export; with the Editor open use RushOps → Build → iOS-Xcode-Projekt
# (Development) bauen and then --no-export. Only the Editor install with the iOS module can export;
# it is found automatically (here: 6000.3.10f1-arm64).
# Prerequisites: Xcode with the Apple account signed in, iPhone connected and trusted, Developer Mode on.
set -euo pipefail

cd "$(dirname "$0")/.."
TEAM_ID="${RUSHOPS_TEAM_ID:-WAM6U438Y8}"
VERSION="6000.3.10f1"
LOG_DIR="Logs"
mkdir -p "$LOG_DIR"

if [[ "${1:-}" != "--no-export" ]]; then
  UNITY=""
  for dir in /Applications/Unity/Hub/Editor/"$VERSION"*; do
    if [[ -d "$dir/PlaybackEngines/iOSSupport" || -d "$dir/Unity.app/Contents/PlaybackEngines/iOSSupport" ]]; then
      UNITY="$dir/Unity.app/Contents/MacOS/Unity"
      break
    fi
  done
  if [[ -z "$UNITY" ]]; then
    echo "Keine Unity-$VERSION-Installation mit iOS Build Support gefunden (Unity Hub → Installs → Add modules)." >&2
    exit 2
  fi

  echo "== Exportiere Xcode-Projekt mit $UNITY"
  if ! "$UNITY" -batchmode -nographics -quit -projectPath . -buildTarget iOS \
      -executeMethod RushOps.Editor.Build.BuildPipelineMenu.BuildIosDevelopment -logFile "$LOG_DIR/build-ios.log"; then
    echo "Export fehlgeschlagen (ist Unity mit dem Projekt offen?). Details: $LOG_DIR/build-ios.log" >&2
    grep -E "BuildFailed|error CS|Error building" "$LOG_DIR/build-ios.log" | head -5 >&2 || true
    exit 1
  fi
fi

PROJECT_DIR=$(ls -dt Builds/iOS/RushOps-* 2>/dev/null | head -1)
if [[ -z "$PROJECT_DIR" || ! -d "$PROJECT_DIR/Unity-iPhone.xcodeproj" ]]; then
  echo "Kein Xcode-Projekt unter Builds/iOS/ gefunden." >&2
  exit 2
fi

DERIVED="$PROJECT_DIR/DerivedData"
echo "== Baue und signiere $PROJECT_DIR (Team $TEAM_ID)"
if ! xcodebuild -quiet -project "$PROJECT_DIR/Unity-iPhone.xcodeproj" -scheme Unity-iPhone -configuration Debug \
    -destination 'generic/platform=iOS' -derivedDataPath "$DERIVED" -allowProvisioningUpdates \
    DEVELOPMENT_TEAM="$TEAM_ID" CODE_SIGN_STYLE=Automatic build > "$LOG_DIR/xcodebuild-ios.log" 2>&1; then
  echo "Xcode-Build fehlgeschlagen. Details: $LOG_DIR/xcodebuild-ios.log" >&2
  grep -E "error:" "$LOG_DIR/xcodebuild-ios.log" | head -10 >&2 || true
  exit 1
fi

APP=$(find "$DERIVED/Build/Products" -maxdepth 2 -name "*.app" -path "*iphoneos*" | head -1)
DEVICE=$(xcrun devicectl list devices 2>/dev/null | awk '/iPhone/ && /available/ {for (i = 1; i <= NF; i++) if ($i ~ /^[0-9A-F-]{36}$/) {print $i; exit}}')
if [[ -z "$DEVICE" ]]; then
  echo "Kein iPhone gefunden. Per USB verbinden, entsperren und dem Mac vertrauen. App: $APP" >&2
  exit 3
fi

echo "== Installiere auf iPhone $DEVICE"
xcrun devicectl device install app --device "$DEVICE" "$APP" > "$LOG_DIR/devicectl-ios.log" 2>&1 \
  || { echo "Installation fehlgeschlagen. Details: $LOG_DIR/devicectl-ios.log" >&2; exit 1; }
if xcrun devicectl device process launch --device "$DEVICE" com.beratkrhn.rushops >> "$LOG_DIR/devicectl-ios.log" 2>&1; then
  echo "Fertig, RushOps läuft auf dem iPhone."
else
  echo "Installiert. Zum Starten am iPhone ggf. unter Einstellungen → Allgemein → VPN & Geräteverwaltung dem Entwickler vertrauen."
fi
