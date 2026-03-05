#!/usr/bin/env bash
cd "$(dirname "$0")"
dotnet build
megadot --headless --export-pack "Windows Desktop" ./STS2Export.pck
rm -f STS2Export.zip
wget https://raw.githubusercontent.com/scriban/scriban/refs/heads/master/license.txt -O Scriban-LICENSE.txt
zip -rj STS2Export.zip .godot/mono/temp/bin/Debug/STS2Export.dll .godot/mono/temp/bin/Debug/Scriban.dll STS2Export.pck Scriban-LICENSE.txt
rm -f Scriban-LICENSE.txt