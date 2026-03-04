# Build backend + frontend + gateway LOCALLY, then upload images and run on server (no build on server)
# Run from MilitaryHealth: .\scripts\deploy-to-server.ps1
# Expects MilitaryHealth.UI as sibling (e.g. P:\MilitaryHealth.UI)

$ErrorActionPreference = "Stop"
$Server = "omar@192.168.1.100"
$RemoteDir = "MilitaryHealth"
$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$FrontendRoot = Join-Path (Split-Path $ProjectRoot -Parent) "MilitaryHealth.UI"
$ApiImage = "militaryhealth-api:latest"
$FrontendImage = "militaryhealth-frontend:latest"
$GatewayImage = "militaryhealth-gateway:latest"
$ApiTar = "militaryhealth-api.tar"
$FrontendTar = "militaryhealth-frontend.tar"
$GatewayTar = "militaryhealth-gateway.tar"

Write-Host "Build locally (API + frontend + gateway), deploy images to server (no build on server)" -ForegroundColor Cyan

# Check Docker is running (e.g. Docker Desktop started)
# $null = & docker info 2>&1
# if ($LASTEXITCODE -ne 0) {
#   Write-Host "`nDocker is not running." -ForegroundColor Red
#   Write-Host "Start Docker Desktop, wait until it is ready, then run this script again." -ForegroundColor Yellow
#   exit 1
# }

# # 1) Build API image locally
# Write-Host "`n[1/7] Building API image locally..." -ForegroundColor Yellow
# Push-Location $ProjectRoot
# try {
#   & docker build -t $ApiImage -f Dockerfile .
#   if ($LASTEXITCODE -ne 0) { throw "API docker build failed" }
# } finally { Pop-Location }

# # 2) Build frontend image locally
# if (-not (Test-Path $FrontendRoot)) { throw "Frontend not found at $FrontendRoot" }
# Write-Host "`n[2/7] Building frontend image locally (npm install + ng build)..." -ForegroundColor Yellow
# Push-Location $FrontendRoot
# try {
#   & docker build -t $FrontendImage -f Dockerfile .
#   if ($LASTEXITCODE -ne 0) { throw "Frontend docker build failed" }
# } finally { Pop-Location }

# # 3) Build gateway image locally
# Write-Host "`n[3/7] Building gateway image locally (nginx)..." -ForegroundColor Yellow
# Push-Location $ProjectRoot
# try {
#   & docker build -t $GatewayImage -f gateway/Dockerfile ./gateway
#   if ($LASTEXITCODE -ne 0) { throw "Gateway docker build failed" }
# } finally { Pop-Location }

# # 4) Save images to tar
# Write-Host "`n[4/7] Saving images to tar..." -ForegroundColor Yellow
# & docker save -o $ApiTar $ApiImage
# if ($LASTEXITCODE -ne 0) { throw "docker save API failed" }
# & docker save -o $FrontendTar $FrontendImage
# if ($LASTEXITCODE -ne 0) { throw "docker save frontend failed" }
# & docker save -o $GatewayTar $GatewayImage
# if ($LASTEXITCODE -ne 0) { throw "docker save gateway failed" }

# 5) Upload tars + compose + backup scripts to server
Write-Host "`n[5/7] Uploading images, compose and backup scripts (enter password when prompted)..." -ForegroundColor Yellow
& ssh -o StrictHostKeyChecking=no $Server "mkdir -p ~/$RemoteDir/scripts"
if ($LASTEXITCODE -ne 0) { throw "SSH mkdir failed" }
& scp -o StrictHostKeyChecking=no "$ProjectRoot\$ApiTar" "$ProjectRoot\$FrontendTar" "$ProjectRoot\$GatewayTar" "${Server}:~/${RemoteDir}/"
if ($LASTEXITCODE -ne 0) { throw "SCP tars failed" }
& scp -o StrictHostKeyChecking=no "$ProjectRoot\docker-compose.images.yml" "${Server}:~/${RemoteDir}/"
if ($LASTEXITCODE -ne 0) { throw "SCP compose failed" }
$backupScripts = @("$PSScriptRoot\backup-db.sh", "$PSScriptRoot\test-backup.sh", "$PSScriptRoot\setup-backup-cron.sh")
foreach ($s in $backupScripts) {
  if (Test-Path $s) {
    & scp -o StrictHostKeyChecking=no $s "${Server}:~/${RemoteDir}/scripts/"
    if ($LASTEXITCODE -ne 0) { throw "SCP backup script failed: $s" }
  }
}
# Convert Windows line endings to Unix on server so bash runs correctly
& ssh -o StrictHostKeyChecking=no $Server "sed -i 's/\r$//' ~/$RemoteDir/scripts/*.sh 2>/dev/null || true"

# 6) On server: load images and start
Write-Host "`n[6/7] Loading images and starting containers on server (enter password again)..." -ForegroundColor Yellow
$remoteCmd = "cd ~/$RemoteDir && docker load -i $ApiTar && docker load -i $FrontendTar && docker load -i $GatewayTar && docker compose -f docker-compose.images.yml up -d"
& ssh -o StrictHostKeyChecking=no $Server $remoteCmd
if ($LASTEXITCODE -ne 0) { throw "Server deploy failed" }
# صلاحيات مجلد النسخ الاحتياطي (chmod قبل chown لأن بعد chown المستخدم العادي لا يستطيع chmod)
& ssh -o StrictHostKeyChecking=no $Server "mkdir -p ~/$RemoteDir/backups && chmod 700 ~/$RemoteDir/backups && sudo chown 10001:0 ~/$RemoteDir/backups"
if ($LASTEXITCODE -ne 0) { Write-Host "تحذير: تعيين صلاحيات backups فشل (نفّذ يدوياً: sudo chown 10001:0 ~/MilitaryHealth/backups)" -ForegroundColor Yellow }

# 7) Done
Write-Host "`n[7/7] Deploy complete." -ForegroundColor Green

# Cleanup local tars
Remove-Item -Path "$ProjectRoot\$ApiTar", "$ProjectRoot\$FrontendTar", "$ProjectRoot\$GatewayTar" -ErrorAction SilentlyContinue

Write-Host "`nDone." -ForegroundColor Green
Write-Host "App (gateway): http://192.168.1.102" -ForegroundColor Green
Write-Host "API direct:   http://192.168.1.102:8080" -ForegroundColor Green
Write-Host "Check:       ssh $Server 'cd $RemoteDir && docker compose -f docker-compose.images.yml ps'" -ForegroundColor Gray
