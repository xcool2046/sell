Write-Host "Starting login test..." -ForegroundColor Green

# Set working directory
Set-Location "e:\download-\sell-master\sell-master"

# Kill existing processes
Get-Process | Where-Object { $_.ProcessName -like "*Sellsys*" } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Build project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build src/Sellsys.WpfClient --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful!" -ForegroundColor Green
} else {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Start application
Write-Host "Starting application..." -ForegroundColor Yellow
$process = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Sellsys.WpfClient" -PassThru

Write-Host "Application started with PID: $($process.Id)" -ForegroundColor Green

# Wait for startup
Start-Sleep -Seconds 8

# Check if process is still running
$runningProcess = Get-Process -Id $process.Id -ErrorAction SilentlyContinue
if ($runningProcess) {
    Write-Host "Application is running" -ForegroundColor Green
    Write-Host "Memory usage: $([math]::Round($runningProcess.WorkingSet64/1MB, 2)) MB" -ForegroundColor Yellow
} else {
    Write-Host "Application has exited" -ForegroundColor Red
    exit 1
}

# Check for log file
$logPath = "$env:USERPROFILE\Desktop\wpf_debug.log"
if (Test-Path $logPath) {
    Write-Host "Log file found:" -ForegroundColor Yellow
    Get-Content $logPath -Tail 10
} else {
    Write-Host "No log file found" -ForegroundColor Yellow
}

Write-Host "Please manually test login with admin/admin" -ForegroundColor Cyan
Write-Host "Waiting 30 seconds for manual testing..." -ForegroundColor Yellow

# Monitor for 30 seconds
for ($i = 1; $i -le 30; $i++) {
    $currentProcess = Get-Process -Id $process.Id -ErrorAction SilentlyContinue
    if ($currentProcess) {
        Write-Host "[$i/30] App running (Memory: $([math]::Round($currentProcess.WorkingSet64/1MB, 2)) MB)" -ForegroundColor Green
    } else {
        Write-Host "[$i/30] App has exited" -ForegroundColor Red
        break
    }
    Start-Sleep -Seconds 1
}

# Cleanup
Write-Host "Cleaning up..." -ForegroundColor Yellow
if (Get-Process -Id $process.Id -ErrorAction SilentlyContinue) {
    Stop-Process -Id $process.Id -Force
    Write-Host "Process terminated" -ForegroundColor Yellow
}

Write-Host "Test completed" -ForegroundColor Green
