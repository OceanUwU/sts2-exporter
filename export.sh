#!/usr/bin/env bash
cd "$(dirname "$0")"
dotnet build
megadot --headless --export-pack "Windows Desktop" ./STS2Export.pck