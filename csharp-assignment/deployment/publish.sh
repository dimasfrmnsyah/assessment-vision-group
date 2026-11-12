#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
release="$PWD/artifacts/linux-x64"
mkdir -p "$release"
dotnet publish StudentApi/StudentApi.csproj -c Release -r linux-x64 --self-contained true -o "$release/StudentApi" -p:PublishReadyToRun=false
dotnet publish WebSocketApp/WebSocketApp.csproj -c Release -r linux-x64 --self-contained true -o "$release/WebSocketApp" -p:PublishReadyToRun=false
# Database is created in the service StateDirectory, never shipped from this Mac.
find "$release" -type f \( -name '*.db' -o -name '*.db-wal' -o -name '*.db-shm' \) -delete
mkdir -p "$release/deployment"
cp deployment/*.service deployment/nginx-csharp deployment/DEPLOY.md "$release/deployment/"
tar -czf artifacts/csharp-release-linux-x64.tar.gz -C "$release" StudentApi WebSocketApp deployment
printf 'Package: %s/artifacts/csharp-release-linux-x64.tar.gz\n' "$PWD"
