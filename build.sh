#NOT WORKING!?
#!/bin/bash
config="$2"
runtime="$1"
dotnet publish src/Take.Blip.CLI/Take.Blip.CLI/Take.Blip.CLI.csproj --framework netcoreapp2.0 --runtime $1 --configuration $2