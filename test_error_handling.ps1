# Test error handling for department deletion with employees
$baseUrl = "http://localhost:5078/api"

Write-Host "Testing error handling for department deletion..." -ForegroundColor Green

# 1. Check API health
Write-Host "1. Checking API health..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get -TimeoutSec 5
    Write-Host "   API Status: $($health.data.status)" -ForegroundColor Green
} catch {
    Write-Host "   API Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 2. Create test department
Write-Host "2. Creating test department..." -ForegroundColor Yellow
$testDeptName = "TestDeptWithEmployee_$(Get-Date -Format 'HHmmss')"
$deptBody = "`"$testDeptName`""
$newDept = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Post -Body $deptBody -ContentType "application/json").data
Write-Host "   Created department ID: $($newDept.id)" -ForegroundColor Green

# 3. Create test group
Write-Host "3. Creating test group..." -ForegroundColor Yellow
$groupBody = @{ DepartmentId = $newDept.id; Name = "TestGroup" } | ConvertTo-Json
$newGroup = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Post -Body $groupBody -ContentType "application/json").data
Write-Host "   Created group ID: $($newGroup.id)" -ForegroundColor Green

# 4. Create test employee in the group
Write-Host "4. Creating test employee..." -ForegroundColor Yellow
$employeeBody = @{
    Name = "TestEmployee"
    LoginUsername = "testuser_$(Get-Date -Format 'HHmmss')"
    Phone = "1234567890"
    GroupId = $newGroup.id
    RoleId = 1  # Assuming role ID 1 exists
} | ConvertTo-Json

try {
    $newEmployee = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Post -Body $employeeBody -ContentType "application/json").data
    Write-Host "   Created employee ID: $($newEmployee.id)" -ForegroundColor Green
} catch {
    Write-Host "   Failed to create employee: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   This might be expected if role doesn't exist. Continuing test..." -ForegroundColor Yellow
}

# 5. Try to delete department (should fail with proper error message)
Write-Host "5. Attempting to delete department with employee (should fail)..." -ForegroundColor Yellow
try {
    $deleteResult = Invoke-RestMethod -Uri "$baseUrl/departments/$($newDept.id)" -Method Delete
    Write-Host "   ERROR: Delete succeeded when it should have failed!" -ForegroundColor Red
    Write-Host "   Result: $($deleteResult | ConvertTo-Json)" -ForegroundColor Red
} catch {
    Write-Host "   Expected failure occurred" -ForegroundColor Green
    Write-Host "   Error message: $($_.Exception.Message)" -ForegroundColor Cyan
    
    # Check if the error message is user-friendly
    if ($_.Exception.Message -like "*员工*" -or $_.Exception.Message -like "*employee*") {
        Write-Host "   SUCCESS: Error message mentions employees" -ForegroundColor Green
    } else {
        Write-Host "   WARNING: Error message might not be user-friendly" -ForegroundColor Yellow
    }
}

# 6. Clean up - delete employee first, then department
Write-Host "6. Cleaning up..." -ForegroundColor Yellow
try {
    # Try to delete employee if it was created
    if ($newEmployee) {
        Invoke-RestMethod -Uri "$baseUrl/employees/$($newEmployee.id)" -Method Delete | Out-Null
        Write-Host "   Deleted employee" -ForegroundColor Green
    }
    
    # Now delete department should succeed
    $deleteResult = Invoke-RestMethod -Uri "$baseUrl/departments/$($newDept.id)" -Method Delete
    Write-Host "   Deleted department successfully" -ForegroundColor Green
    Write-Host "   Message: $($deleteResult.message)" -ForegroundColor Cyan
} catch {
    Write-Host "   Cleanup failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`nTest completed!" -ForegroundColor Green
Write-Host "Key points to verify in frontend:" -ForegroundColor Yellow
Write-Host "1. Error message should be user-friendly, not technical" -ForegroundColor Yellow
Write-Host "2. Should mention employees need to be handled first" -ForegroundColor Yellow
Write-Host "3. Should not show HTTP status codes to users" -ForegroundColor Yellow
