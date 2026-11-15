param(
    [ValidateSet('win-x64', 'win-arm64')]
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot 'Aquarium90.csproj'
$publishPath = Join-Path $PSScriptRoot "obj\portable\$Runtime"
$destination = Join-Path $PSScriptRoot "dist\$Runtime"

dotnet publish $projectPath -c Release -r $Runtime --self-contained true -o $publishPath `
    -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed ($LASTEXITCODE)."
}

# Use an ASCII script file that also works with Windows PowerShell 5.1.
$russianName = (-join [char[]](0x410, 0x43A, 0x432, 0x430, 0x440, 0x438, 0x443, 0x43C)) + ' 90.scr'
$exePath = Join-Path $publishPath 'Aquarium90.exe'
$scrPath = Join-Path $destination $russianName
New-Item -ItemType Directory -Path $destination -Force | Out-Null
Copy-Item -LiteralPath $exePath -Destination $scrPath -Force
Get-Item -LiteralPath $scrPath
