# Publishes StayPlanner for linux-x64 on this Windows machine, then runs that build in a Debian container
# with a virtual X11 desktop. Requires Docker Desktop (Linux containers).
#
#   .\run-linux.ps1                 publish, build the image and start the container
#   .\run-linux.ps1 -Screenshot x   also save a screenshot of the Linux desktop to x.png
param([string]$Screenshot)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path "$PSScriptRoot\..\.."
$publish = Join-Path $root 'publish\linux-x64'

dotnet publish "$root\Code\StayPlanner.Desktop" -c Release -r linux-x64 -o $publish --nologo
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }
docker build -t stayplanner-linux $PSScriptRoot
if ($LASTEXITCODE -ne 0) { throw 'docker build failed' }

docker rm -f stayplanner-linux 2>$null | Out-Null
# Copy the reservations saved by the Windows run next to the build, so the Linux run starts with the same data.
$seed = Join-Path $root 'publish\seed'
New-Item -ItemType Directory -Force $seed | Out-Null
$windowsData = Join-Path $env:APPDATA 'StayPlanner\reservations.json'
if (Test-Path $windowsData) { Copy-Item $windowsData $seed -Force }
$mounts = @('-v', "${publish}:/app:ro", '-v', "${seed}:/seed:ro")
docker run -d --name stayplanner-linux @mounts stayplanner-linux | Out-Null
Start-Sleep -Seconds 8

if ($Screenshot) {
    docker exec stayplanner-linux scrot -o /tmp/shot.png
    docker cp stayplanner-linux:/tmp/shot.png $Screenshot
    Write-Host "Saved $Screenshot"
}
Write-Host "Container 'stayplanner-linux' is running. Stop it with: docker rm -f stayplanner-linux"
