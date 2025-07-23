# Test script to verify Beijing time fix
$baseUrl = "http://localhost:5078/api"

Write-Host "=== TESTING BEIJING TIME FIX ===" -ForegroundColor Green

# Get current Beijing time for comparison
$beijingTime = [System.TimeZoneInfo]::ConvertTimeFromUtc([DateTime]::UtcNow, [System.TimeZoneInfo]::FindSystemTimeZoneById("China Standard Time"))
Write-Host "Current Beijing Time: $($beijingTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Cyan

# Test 1: Create a new customer
Write-Host "`n1. Testing customer creation time..." -ForegroundColor Yellow

$customerData = @{
    name = "TimeTest_$(Get-Date -Format 'HHmmss')"
    province = "Beijing"
    city = "Beijing"
    address = "Test Address"
    industryTypes = "Test Industry"
    salesPersonId = 1
    contacts = @(
        @{
            name = "Test Contact"
            phone = "13800138000"
            isPrimary = $true
        }
    )
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/customers" -Method Post -Body $customerData -ContentType "application/json"
    
    if ($response.isSuccess) {
        $createdTime = [DateTime]::Parse($response.data.createdAt)
        Write-Host "   Customer created successfully" -ForegroundColor Green
        Write-Host "   Created time from API: $($createdTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Cyan
        
        # Check if time is close to Beijing time (within 2 minutes)
        $timeDiff = [Math]::Abs(($beijingTime - $createdTime).TotalMinutes)
        if ($timeDiff -le 2) {
            Write-Host "   ✓ Time is correct (Beijing time)" -ForegroundColor Green
        } else {
            Write-Host "   ✗ Time difference: $([Math]::Round($timeDiff, 1)) minutes" -ForegroundColor Red
        }
        
        $customerId = $response.data.id
        
        # Test 2: Create a sales follow-up record
        Write-Host "`n2. Testing sales follow-up record creation time..." -ForegroundColor Yellow
        
        $followUpData = @{
            customerId = $customerId
            contactId = $response.data.contacts[0].id
            contactSummary = "时间测试跟进记录"
            customerIntention = "高"
            salesPersonId = 1
            nextFollowUpDate = (Get-Date).AddDays(7).ToString("yyyy-MM-dd")
        } | ConvertTo-Json
        
        $followUpResponse = Invoke-RestMethod -Uri "$baseUrl/salesfollowuplogs" -Method Post -Body $followUpData -ContentType "application/json"
        
        if ($followUpResponse.isSuccess) {
            $followUpCreatedTime = [DateTime]::Parse($followUpResponse.data.createdAt)
            Write-Host "   Follow-up record created successfully" -ForegroundColor Green
            Write-Host "   Created time from API: $($followUpCreatedTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Cyan
            
            # Check if time is close to Beijing time (within 2 minutes)
            $timeDiff2 = [Math]::Abs(($beijingTime - $followUpCreatedTime).TotalMinutes)
            if ($timeDiff2 -le 2) {
                Write-Host "   ✓ Time is correct (Beijing time)" -ForegroundColor Green
            } else {
                Write-Host "   ✗ Time difference: $([Math]::Round($timeDiff2, 1)) minutes" -ForegroundColor Red
            }
        } else {
            Write-Host "   ✗ Failed to create follow-up record: $($followUpResponse.message)" -ForegroundColor Red
        }
    } else {
        Write-Host "   ✗ Failed to create customer: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "   ✗ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== TEST COMPLETED ===" -ForegroundColor Green
Write-Host "Please check the WPF client to verify the time display shows Beijing time correctly." -ForegroundColor Yellow
