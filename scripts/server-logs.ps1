# View API and SQL Server logs on the deployment server
# Run: .\scripts\server-logs.ps1
$Server = "omar@192.168.1.102"
$RemoteDir = "MilitaryHealth"
Write-Host "Fetching API container logs (last 80 lines)..." -ForegroundColor Cyan
ssh -o StrictHostKeyChecking=no $Server "cd ~/$RemoteDir && docker compose logs api --tail 80"
Write-Host "`nTo follow logs live: ssh $Server 'cd $RemoteDir && docker compose logs -f api'" -ForegroundColor Gray
