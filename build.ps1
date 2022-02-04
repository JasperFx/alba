$ErrorActionPreference = "Stop";

dotnet run -project build/build.csproj -c Release -- $args
