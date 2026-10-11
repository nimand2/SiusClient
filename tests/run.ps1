$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$installation = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
if (-not $installation) { throw 'Visual Studio MSBuild wurde nicht gefunden.' }
$msbuild = Join-Path $installation 'MSBuild/Current/Bin/MSBuild.exe'
$compiler = Join-Path $installation 'MSBuild/Current/Bin/Roslyn/csc.exe'
Push-Location $root
try {
    & $msbuild SiusClient/SiusClient.csproj /t:Build /p:Configuration=Debug /p:SignManifests=false /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build fehlgeschlagen.' }
    & $compiler /nologo /out:SiusClient/bin/Debug/DataFlowTests.exe /reference:SiusClient/bin/Debug/SiusClient.exe /reference:System.Net.Http.dll /reference:System.Windows.Forms.dll tests/DataFlowTests.cs
    if ($LASTEXITCODE -ne 0) { throw 'Testkompilierung fehlgeschlagen.' }
    & ./SiusClient/bin/Debug/DataFlowTests.exe
    if ($LASTEXITCODE -ne 0) { throw 'Regressionstests fehlgeschlagen.' }
}
finally { Pop-Location }
