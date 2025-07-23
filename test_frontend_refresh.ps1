# Test frontend refresh after cascade deletion
$baseUrl = "http://localhost:5078/api"

Write-Host "=== TESTING FRONTEND REFRESH AFTER CASCADE DELETE ===" -ForegroundColor Green

# Create test data
Write-Host "`n1. Creating test data..." -ForegroundColor Yellow
$deptBody = "`"FrontendTestDept`""
$dept = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Post -Body $deptBody -ContentType "application/json").data
Write-Host "Created department: ID=$($dept.id), Name=$($dept.name)"

$groupBody = @{ DepartmentId = $dept.id; Name = "FrontendTestGroup" } | ConvertTo-Json
$group = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Post -Body $groupBody -ContentType "application/json").data
Write-Host "Created group: ID=$($group.id), Name=$($group.name)"

$employeeBody = @{
    Name = "FrontendTestEmployee"
    LoginUsername = "frontend_$(Get-Date -Format 'HHmmss')"
    Phone = "1234567890"
    GroupId = $group.id
    RoleId = 5
    Password = "123456"
} | ConvertTo-Json

$employee = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Post -Body $employeeBody -ContentType "application/json").data
Write-Host "Created employee: ID=$($employee.id), Name=$($employee.name)"

# Verify data exists before deletion
Write-Host "`n2. Verifying data before deletion..." -ForegroundColor Yellow
$allEmployeesBefore = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data
$allGroupsBefore = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get).data
$allDeptsBefore = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Get).data

Write-Host "Before deletion:"
Write-Host "  Departments: $($allDeptsBefore.Count)"
Write-Host "  Groups: $($allGroupsBefore.Count)"
Write-Host "  Employees: $($allEmployeesBefore.Count)"

$targetEmployee = $allEmployeesBefore | Where-Object { $_.id -eq $employee.id }
$targetGroup = $allGroupsBefore | Where-Object { $_.id -eq $group.id }
$targetDept = $allDeptsBefore | Where-Object { $_.id -eq $dept.id }

if ($targetEmployee) { Write-Host "  ✓ Employee exists: $($targetEmployee.name)" -ForegroundColor Green }
if ($targetGroup) { Write-Host "  ✓ Group exists: $($targetGroup.name)" -ForegroundColor Green }
if ($targetDept) { Write-Host "  ✓ Department exists: $($targetDept.name)" -ForegroundColor Green }

# Delete department
Write-Host "`n3. Deleting department..." -ForegroundColor Yellow
$deleteResult = Invoke-RestMethod -Uri "$baseUrl/departments/$($dept.id)" -Method Delete

Write-Host "Delete result:"
Write-Host "  Success: $($deleteResult.isSuccess)"
Write-Host "  Message: $($deleteResult.message)"
if ($deleteResult.data) {
    Write-Host "  Deleted groups: $($deleteResult.data.deletedGroupCount)"
    Write-Host "  Deleted employees: $($deleteResult.data.deletedEmployeeCount)"
    Write-Host "  Employee names: $($deleteResult.data.deletedEmployeeNames -join ', ')"
}

# Verify data after deletion
Write-Host "`n4. Verifying data after deletion..." -ForegroundColor Yellow
$allEmployeesAfter = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data
$allGroupsAfter = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get).data
$allDeptsAfter = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Get).data

Write-Host "After deletion:"
Write-Host "  Departments: $($allDeptsAfter.Count)"
Write-Host "  Groups: $($allGroupsAfter.Count)"
Write-Host "  Employees: $($allEmployeesAfter.Count)"

$targetEmployeeAfter = $allEmployeesAfter | Where-Object { $_.id -eq $employee.id }
$targetGroupAfter = $allGroupsAfter | Where-Object { $_.id -eq $group.id }
$targetDeptAfter = $allDeptsAfter | Where-Object { $_.id -eq $dept.id }

Write-Host "`n5. Verification results:" -ForegroundColor Yellow
if ($targetEmployeeAfter) {
    Write-Host "  ✗ ERROR: Employee still exists!" -ForegroundColor Red
    Write-Host "    ID: $($targetEmployeeAfter.id), Name: $($targetEmployeeAfter.name)"
} else {
    Write-Host "  ✓ Employee successfully deleted" -ForegroundColor Green
}

if ($targetGroupAfter) {
    Write-Host "  ✗ ERROR: Group still exists!" -ForegroundColor Red
    Write-Host "    ID: $($targetGroupAfter.id), Name: $($targetGroupAfter.name)"
} else {
    Write-Host "  ✓ Group successfully deleted" -ForegroundColor Green
}

if ($targetDeptAfter) {
    Write-Host "  ✗ ERROR: Department still exists!" -ForegroundColor Red
    Write-Host "    ID: $($targetDeptAfter.id), Name: $($targetDeptAfter.name)"
} else {
    Write-Host "  ✓ Department successfully deleted" -ForegroundColor Green
}

Write-Host "`n=== FRONTEND REFRESH TEST COMPLETED ===" -ForegroundColor Green
Write-Host "Now test the frontend application to verify UI refresh works correctly." -ForegroundColor Yellow
