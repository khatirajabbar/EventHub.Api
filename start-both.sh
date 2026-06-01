#!/bin/bash

# EventHub Quick Start Script for macOS
# This script starts both the API and Web projects in separate terminal windows

echo "🚀 Starting EventHub Applications..."
echo ""

PROJECT_PATH="/Users/khatira/Desktop/EventHub.Api"

# Open API in a new terminal window
open -a Terminal "$PROJECT_PATH" --args "cd $PROJECT_PATH && dotnet run --project EventHub.Api/EventHub.Api.csproj"

# Wait 2 seconds to let API start
sleep 2

# Open Web in another terminal window
open -a Terminal "$PROJECT_PATH" --args "cd $PROJECT_PATH && dotnet run --project EventHub.Web/EventHub.Web.csproj"

echo ""
echo "✅ Applications are starting..."
echo ""
echo "📍 Web UI will be available at:     http://localhost:5198"
echo "📍 API will be available at:        http://localhost:5220"
echo "📍 API Documentation (Swagger) at: http://localhost:5220/swagger"
echo ""
echo "📌 Tip: The terminal windows may take a few seconds to open and start the apps."
echo "   If you don't see the output, check the terminal windows."

