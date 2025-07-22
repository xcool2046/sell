# Test complete cascade deletion functionality
$baseUrl = "http://localhost:5078/api"

Write-Host "=== TESTING COMPLETE CASCADE DELETION ===" -ForegroundColor Green
Write-Host "Testing department deletion with automatic removal of groups and employees" -ForegroundColor Green

# 1. Check API health
Write-Host "`n1. Checking API health..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get -TimeoutSec 5
    Write-Host "   ✓ API Status: $($health.data.status)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ API Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 2. Get available roles
Write-Host "`n2. Getting available roles..." -ForegroundColor Yellow
try {
    $roles = (Invoke-RestMethod -Uri "$baseUrl/roles" -Method Get).data
    if ($roles.Count -gt 0) {
        $roleId = $roles[0].id
        Write-Host "   ✓ Using role: $($roles[0].name) (ID: $roleId)" -ForegroundColor Green
    } else {
        Write-Host "   ✗ No roles available" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ✗ Failed to get roles: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 3. Create test department with groups and employees
Write-Host "`n3. Creating test scenario..." -ForegroundColor Yellow

$testDeptName = "CascadeTestDept_$(Get-Date -Format 'HHmmss')"
$deptBody = "`"$testDeptName`""
$dept = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Post -Body $deptBody -ContentType "application/json").data
Write-Host "   Created department: $($dept.name) (ID: $($dept.id))"

# Create multiple groups
$groups = @()
for ($i = 1; $i -le 3; $i++) {
    $groupBody = @{ DepartmentId = $dept.id; Name = "Group$i" } | ConvertTo-Json
    $group = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Post -Body $groupBody -ContentType "application/json").data
    $groups += $group
    Write-Host "   Created group: $($group.name) (ID: $($group.id))"
}

# Create multiple employees
$employees = @()
for ($i = 1; $i -le 5; $i++) {
    $groupIndex = ($i - 1) % $groups.Count
    $employeeBody = @{
        Name = "Employee$i"
        LoginUsername = "emp$i_$(Get-Date -Format 'HHmmss')"
        Phone = "123456789$i"
        GroupId = $groups[$groupIndex].id
        RoleId = $roleId
        Password = "123456"
    } | ConvertTo-Json

    try {
        $employee = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Post -Body $employeeBody -ContentType "application/json").data
        $employees += $employee
        Write-Host "   Created employee: $($employee.name) (ID: $($employee.id)) in group $($groups[$groupIndex].name)"
    } catch {
        Write-Host "   Failed to create employee ${i}: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host "   ✓ Test scenario created: 1 department, $($groups.Count) groups, $($employees.Count) employees" -ForegroundColor Green

# 4. Verify data exists before deletion
Write-Host "`n4. Verifying data before deletion..." -ForegroundColor Yellow
$allDepts = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Get).data
$allGroups = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get).data
$allEmployees = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data

$testGroups = $allGroups | Where-Object { $_.departmentId -eq $dept.id }
$testEmployees = $allEmployees | Where-Object { $groups.id -contains $_.groupId }

Write-Host "   Department exists: $($allDepts | Where-Object { $_.id -eq $dept.id } | Measure-Object | Select-Object -ExpandProperty Count)" -ForegroundColor Cyan
Write-Host "   Groups exist: $($testGroups.Count)" -ForegroundColor Cyan
Write-Host "   Employees exist: $($testEmployees.Count)" -ForegroundColor Cyan

# 5. Execute cascade deletion
Write-Host "`n5. Executing cascade deletion..." -ForegroundColor Yellow
try {
    $deleteResult = Invoke-RestMethod -Uri "$baseUrl/departments/$($dept.id)" -Method Delete
    Write-Host "   ✓ DELETE SUCCESS!" -ForegroundColor Green
    Write-Host "   Message: $($deleteResult.message)" -ForegroundColor Cyan
    
    if ($deleteResult.data) {
        Write-Host "   Deletion details:" -ForegroundColor Cyan
        Write-Host "     - Department: $($deleteResult.data.departmentName)" -ForegroundColor Cyan
        Write-Host "     - Deleted groups: $($deleteResult.data.deletedGroupCount)" -ForegroundColor Cyan
        Write-Host "     - Group names: $($deleteResult.data.deletedGroupNames -join ', ')" -ForegroundColor Cyan
        Write-Host "     - Deleted employees: $($deleteResult.data.deletedEmployeeCount)" -ForegroundColor Cyan
        Write-Host "     - Employee names: $($deleteResult.data.deletedEmployeeNames -join ', ')" -ForegroundColor Cyan
    }
} catch {
    Write-Host "   ✗ DELETE FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 6. Verify complete deletion
Write-Host "`n6. Verifying complete deletion..." -ForegroundColor Yellow
$deptsAfter = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Get).data
$groupsAfter = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get).data
$employeesAfter = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data

$remainingDept = $deptsAfter | Where-Object { $_.id -eq $dept.id }
$remainingGroups = $groupsAfter | Where-Object { $_.departmentId -eq $dept.id }
$remainingEmployees = $employeesAfter | Where-Object { $groups.id -contains $_.groupId }

Write-Host "   Department remaining: $($remainingDept | Measure-Object | Select-Object -ExpandProperty Count)" -ForegroundColor Cyan
Write-Host "   Groups remaining: $($remainingGroups.Count)" -ForegroundColor Cyan
Write-Host "   Employees remaining: $($remainingEmployees.Count)" -ForegroundColor Cyan

# 7. Results
Write-Host "`n=== TEST RESULTS ===" -ForegroundColor Green
if ($remainingDept.Count -eq 0) {
    Write-Host "✓ Department successfully deleted" -ForegroundColor Green
} else {
    Write-Host "✗ Department still exists!" -ForegroundColor Red
}

if ($remainingGroups.Count -eq 0) {
    Write-Host "✓ All groups successfully deleted" -ForegroundColor Green
} else {
    Write-Host "✗ $($remainingGroups.Count) groups still exist!" -ForegroundColor Red
}

if ($remainingEmployees.Count -eq 0) {
    Write-Host "✓ All employees successfully deleted" -ForegroundColor Green
} else {
    Write-Host "✗ $($remainingEmployees.Count) employees still exist!" -ForegroundColor Red
}

Write-Host "`n🎉 CASCADE DELETION TEST COMPLETED!" -ForegroundColor Green
Write-Host "Now test the frontend to verify the UI behavior." -ForegroundColor Yellow
