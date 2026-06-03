#!/bin/bash
set -euo pipefail

# EventHub Quick Start Script for macOS.
# Starts the API and Web projects in separate Terminal windows.

PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

run_in_terminal() {
    local command="$1"

    osascript <<EOF
tell application "Terminal"
    activate
    do script "cd \"$PROJECT_PATH\" && $command"
end tell
EOF
}

echo "Starting EventHub applications..."
echo ""

run_in_terminal "dotnet run --project EventHub.Api/EventHub.Api.csproj --launch-profile http"
sleep 2
run_in_terminal "dotnet run --project EventHub.Web/EventHub.Web.csproj --launch-profile http"

echo "Applications are starting in separate Terminal windows."
echo ""
echo "Web UI:     http://localhost:5198"
echo "API:        http://localhost:5220"
echo "Swagger:    http://localhost:5220/swagger"
echo ""
echo "If a URL does not open, check the matching Terminal window for errors."
