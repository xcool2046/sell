# Test fixes for customer intention and creation time
Write-Host "Starting fix tests..." -ForegroundColor Green

$apiUrl = "http://localhost:5078"

try {
    Write-Host "1. Testing API connection..." -ForegroundColor Yellow
    $customers = Invoke-RestMethod -Uri "$apiUrl/api/customers" -Method Get
    if ($customers.isSuccess) {
        Write-Host "   API connection successful, found $($customers.data.Count) customers" -ForegroundColor Green
    } else {
        Write-Host "   API connection failed: $($customers.message)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   API connection error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

try {
    Write-Host "2. Testing sales follow-up logs..." -ForegroundColor Yellow
    $followUpLogs = Invoke-RestMethod -Uri "$apiUrl/api/salesfollowuplogs" -Method Get
    if ($followUpLogs.isSuccess) {
        Write-Host "   Found $($followUpLogs.data.Count) follow-up records" -ForegroundColor Green

        # Check customer intention distribution
        $intentionStats = $followUpLogs.data | Group-Object customerIntention | Select-Object Name, Count
        Write-Host "   Customer intention distribution:" -ForegroundColor Cyan
        foreach ($stat in $intentionStats) {
            Write-Host "     $($stat.Name): $($stat.Count) records" -ForegroundColor White
        }
    }
} catch {
    Write-Host "   Error getting follow-up logs: $($_.Exception.Message)" -ForegroundColor Red
}

try {
    Write-Host "3. Testing creation of new follow-up record..." -ForegroundColor Yellow

    if ($customers.data.Count -gt 0) {
        $testCustomerId = $customers.data[0].id

        $newLog = @{
            customerId = $testCustomerId
            summary = "Test record - verifying creation time and customer intention"
            customerIntention = "High"
            nextFollowUpDate = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss")
        }

        $createResponse = Invoke-RestMethod -Uri "$apiUrl/api/salesfollowuplogs" -Method Post -Body ($newLog | ConvertTo-Json) -ContentType "application/json"

        if ($createResponse.isSuccess) {
            Write-Host "   Successfully created follow-up record" -ForegroundColor Green
            Write-Host "   Record ID: $($createResponse.data.id)" -ForegroundColor Cyan
            Write-Host "   Created time: $($createResponse.data.createdAt)" -ForegroundColor Cyan
            Write-Host "   Customer intention: $($createResponse.data.customerIntention)" -ForegroundColor Cyan

            # Verify creation time is current (allow 5 minute tolerance)
            $createdTime = [DateTime]::Parse($createResponse.data.createdAt)
            $timeDiff = [Math]::Abs(((Get-Date) - $createdTime).TotalMinutes)

            if ($timeDiff -le 5) {
                Write-Host "   Creation time verification PASSED (diff: $([Math]::Round($timeDiff, 2)) minutes)" -ForegroundColor Green
            } else {
                Write-Host "   Creation time verification FAILED (diff: $([Math]::Round($timeDiff, 2)) minutes)" -ForegroundColor Red
            }

            # Verify customer intention is saved correctly
            if ($createResponse.data.customerIntention -eq "High") {
                Write-Host "   Customer intention verification PASSED" -ForegroundColor Green
            } else {
                Write-Host "   Customer intention verification FAILED" -ForegroundColor Red
            }
        } else {
            Write-Host "   Failed to create follow-up record: $($createResponse.message)" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "   Error creating follow-up record: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTest completed!" -ForegroundColor Green
Write-Host "Please verify in WPF client:" -ForegroundColor Yellow
Write-Host "1. Sales management customer intention column shows correct values (not 'Pending')" -ForegroundColor White
Write-Host "2. New records show current creation time (Beijing time)" -ForegroundColor White
Write-Host "3. Customer intention updates after creating contact records" -ForegroundColor White
Write-Host "4. Contact record dialog field labels match table headers:" -ForegroundColor White
Write-Host "   - Customer Unit, Contact Person, Contact Phone, Contact Record, Customer Intention, Contact Time, Next Appointment, Sales" -ForegroundColor White
Write-Host "5. Time display shows local time correctly (Beijing time, not UTC)" -ForegroundColor White
Write-Host "6. Contact record dialog only shows High/Medium/Low intention options (no Unknown/None)" -ForegroundColor White
Write-Host "7. All time displays across the system use consistent Beijing time format" -ForegroundColor White
