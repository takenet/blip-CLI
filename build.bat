set config=%2
set runtime=%1
if "x"=="x%config%" set config=release
if "x"=="x%runtime%" set runtime=win10-x64
dotnet publish src/Take.Blip.CLI/Take.Blip.CLI/Take.Blip.CLI.csproj --framework netcoreapp2.0 --runtime %runtime% --configuration %config%