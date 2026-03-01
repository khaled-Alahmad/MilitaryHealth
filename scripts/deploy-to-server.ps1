# Build backend + frontend LOCALLY, then upload images and run on server (no build on server)
# Run from MilitaryHealth: .\scripts\deploy-to-server.ps1
# Expects MilitaryHealth.UI as sibling (e.g. P:\MilitaryHealth.UI)

$ErrorActionPreference = "Stop"
$Server = "omar@192.168.1.102"
$RemoteDir = "MilitaryHealth"
$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$FrontendRoot = Join-Path (Split-Path $ProjectRoot -Parent) "MilitaryHealth.UI"
$ApiImage = "militaryhealth-api:latest"
$FrontendImage = "militaryhealth-frontend:latest"
$ApiTar = "militaryhealth-api.tar"
$FrontendTar = "militaryhealth-frontend.tar"

Write-Host "Build locally, deploy images to server (no build on server)" -ForegroundColor Cyan

# Check Docker is running (e.g. Docker Desktop started)
$null = & docker info 2>&1
if ($LASTEXITCODE -ne 0) {
  Write-Host "`nDocker is not running." -ForegroundColor Red
  Write-Host "Start Docker Desktop, wait until it is ready, then run this script again." -ForegroundColor Yellow
  exit 1
}

# 1) Build API image locally
Write-Host "`n[1/5] Building API image locally..." -ForegroundColor Yellow
Push-Location $ProjectRoot
try {
  & docker build -t $ApiImage -f Dockerfile .
  if ($LASTEXITCODE -ne 0) { throw "API docker build failed" }
} finally { Pop-Location }

# 2) Build frontend image locally
if (-not (Test-Path $FrontendRoot)) { throw "Frontend not found at $FrontendRoot" }
Write-Host "`n[2/5] Building frontend image locally (npm install + ng build)..." -ForegroundColor Yellow
Push-Location $FrontendRoot
try {
  & docker build -t $FrontendImage -f Dockerfile .
  if ($LASTEXITCODE -ne 0) { throw "Frontend docker build failed" }
} finally { Pop-Location }

# 3) Save images to tar
Write-Host "`n[3/5] Saving images to tar..." -ForegroundColor Yellow
& docker save -o $ApiTar $ApiImage
if ($LASTEXITCODE -ne 0) { throw "docker save API failed" }
& docker save -o $FrontendTar $FrontendImage
if ($LASTEXITCODE -ne 0) { throw "docker save frontend failed" }

# 4) Upload tars + compose to server
Write-Host "`n[4/5] Uploading images and compose (enter password when prompted)..." -ForegroundColor Yellow
& ssh -o StrictHostKeyChecking=no $Server "mkdir -p ~/$RemoteDir"
if ($LASTEXITCODE -ne 0) { throw "SSH mkdir failed" }
& scp -o StrictHostKeyChecking=no "$ProjectRoot\$ApiTar" "$ProjectRoot\$FrontendTar" "${Server}:~/${RemoteDir}/"
if ($LASTEXITCODE -ne 0) { throw "SCP tars failed" }
& scp -o StrictHostKeyChecking=no "$ProjectRoot\docker-compose.images.yml" "${Server}:~/${RemoteDir}/"
if ($LASTEXITCODE -ne 0) { throw "SCP compose failed" }

# 5) On server: load images and start
Write-Host "`n[5/5] Loading images and starting containers on server (enter password again)..." -ForegroundColor Yellow
$remoteCmd = "cd ~/$RemoteDir && docker load -i $ApiTar && docker load -i $FrontendTar && docker compose -f docker-compose.images.yml up -d"
& ssh -o StrictHostKeyChecking=no $Server $remoteCmd
if ($LASTEXITCODE -ne 0) { throw "Server deploy failed" }

# Cleanup local tars
Remove-Item -Path "$ProjectRoot\$ApiTar", "$ProjectRoot\$FrontendTar" -ErrorAction SilentlyContinue

Write-Host "`nDone." -ForegroundColor Green
Write-Host "Frontend: http://192.168.1.102" -ForegroundColor Green
Write-Host "API:     http://192.168.1.102:8080" -ForegroundColor Green
Write-Host "Check:   ssh $Server 'cd $RemoteDir && docker compose -f docker-compose.images.yml ps'" -ForegroundColor Gray
