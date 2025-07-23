# Test employee check logic for department deletion
$baseUrl = "http://localhost:5078/api"

Write-Host "Testing employee check logic..." -ForegroundColor Green

# 1. Get available roles first
Write-Host "1. Getting available roles..." -ForegroundColor Yellow
try {
    $roles = (Invoke-RestMethod -Uri "$baseUrl/roles" -Method Get).data
    if ($roles.Count -gt 0) {
        $roleId = $roles[0].id
        Write-Host "   Using role ID: $roleId ($($roles[0].name))" -ForegroundColor Green
    } else {
        Write-Host "   No roles available, cannot create employee" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   Failed to get roles: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 2. Create test department
Write-Host "2. Creating test department..." -ForegroundColor Yellow
$testDeptName = "TestDeptEmployee_$(Get-Date -Format 'HHmmss')"
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
    RoleId = $roleId
    Password = "123456"
} | ConvertTo-Json

try {
    $newEmployee = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Post -Body $employeeBody -ContentType "application/json").data
    Write-Host "   Created employee ID: $($newEmployee.id)" -ForegroundColor Green
    $employeeCreated = $true
} catch {
    Write-Host "   Failed to create employee: $($_.Exception.Message)" -ForegroundColor Red
    $employeeCreated = $false
}

# 5. Verify employee is in the group
if ($employeeCreated) {
    Write-Host "5. Verifying employee is in group..." -ForegroundColor Yellow
    $employees = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data
    $testEmployee = $employees | Where-Object { $_.id -eq $newEmployee.id }
    if ($testEmployee -and $testEmployee.groupId -eq $newGroup.id) {
        Write-Host "   Employee correctly assigned to group" -ForegroundColor Green
    } else {
        Write-Host "   Employee not found in group!" -ForegroundColor Red
    }
}

# 6. Try to delete department (should fail if employee exists)
Write-Host "6. Attempting to delete department..." -ForegroundColor Yellow
try {
    $deleteResult = Invoke-RestMethod -Uri "$baseUrl/departments/$($newDept.id)" -Method Delete
    if ($employeeCreated) {
        Write-Host "   ERROR: Delete succeeded when it should have failed!" -ForegroundColor Red
        Write-Host "   This indicates the employee check logic is not working" -ForegroundColor Red
    } else {
        Write-Host "   Delete succeeded (expected since no employee was created)" -ForegroundColor Green
    }
    Write-Host "   Result: $($deleteResult.message)" -ForegroundColor Cyan
} catch {
    if ($employeeCreated) {
        Write-Host "   SUCCESS: Delete failed as expected" -ForegroundColor Green
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Cyan
    } else {
        Write-Host "   Unexpected failure: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# 7. Clean up if needed
if ($employeeCreated) {
    Write-Host "7. Cleaning up..." -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$baseUrl/employees/$($newEmployee.id)" -Method Delete | Out-Null
        Write-Host "   Deleted employee" -ForegroundColor Green
        
        $deleteResult = Invoke-RestMethod -Uri "$baseUrl/departments/$($newDept.id)" -Method Delete
        Write-Host "   Deleted department" -ForegroundColor Green
    } catch {
        Write-Host "   Cleanup failed: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host "`nTest completed!" -ForegroundColor Green
