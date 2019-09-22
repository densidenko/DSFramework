function Exec {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

$root = (Get-Item -Path ".\").FullName
$artifactsPath = $root + "\artifacts"
if(Test-Path $artifactsPath) { Remove-Item $artifactsPath -Force -Recurse }

$output = gitversion /nofetch | Out-String
$versions = $output | ConvertFrom-Json
$packageVer = $versions.NuGetVersion
$buildVer = $versions.SemVer

echo "Build: Package version $packageVer"
echo "Build: Build version $buildVer"

gitversion /output buildserver /updateAssemblyInfo

# Update Appveyor version
if (Test-Path env:APPVEYOR) {      
#	echo "Build: Full version is $full"
    Update-AppveyorBuild -Version $buildVer
}

# Build
echo "`n`n----- BUILD -----`n"

exec { & dotnet build DSFrameworkCore.sln -c Release /p:Version=$buildVer }

# Test
#echo "`n`n----- TEST -----`n"

#exec { & dotnet tool install --global coverlet.console }

#$testDirs  = @(Get-ChildItem -Path tests -Include "*.Tests" -Directory -Recurse)
#$testDirs += @(Get-ChildItem -Path tests -Include "*.IntegrationTests" -Directory -Recurse)
#$testDirs += @(Get-ChildItem -Path tests -Include "*FunctionalTests" -Directory -Recurse)

#$i = 0
#ForEach ($folder in $testDirs) { 
#    echo "Testing $folder"

#    $i++
#    $format = @{ $true = "/p:CoverletOutputFormat=opencover"; $false = ""}[$i -eq $testDirs.Length ]
#
#    exec { & dotnet test $folder.FullName -c Release --no-build --no-restore /p:CollectCoverage=true /p:CoverletOutput=$root\coverage /p:MergeWith=$root\coverage.json /p:Include="[*]DSFrameworkCore.*" /p:Exclude="[*]DSFrameworkCore.Testing.*" $format }
#}

#choco install codecov --no-progress
#exec { & codecov -f "$root\coverage.opencover.xml" }

# Pack
echo "`n`n----- PACK -----`n"

exec { & dotnet pack -c Release -o $artifactsPath --include-symbols --no-build /p:VersionPrefix=$packageVer }
