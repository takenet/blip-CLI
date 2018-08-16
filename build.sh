runtime=${1:-"ubuntu-x64"}
config=${2:-"release"}
dotnet publish src/Take.Blip.CLI/Take.Blip.CLI/Take.Blip.CLI.csproj --framework netcoreapp2.0 --runtime $runtime --configuration $config