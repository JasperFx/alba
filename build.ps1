$ErrorActionPreference = "Stop";

dotnet run -p build/build.csproj -c Release -- $args
