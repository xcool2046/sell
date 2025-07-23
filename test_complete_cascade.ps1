# Test complete cascade deletion - simple version
$baseUrl = "http://localhost:5078/api"

Write-Host "Testing complete cascade deletion..." -ForegroundColor Green

# Create test department
$deptBody = "`"CascadeTest`""
$dept = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Post -Body $deptBody -ContentType "application/json").data
Write-Host "Created department: $($dept.id)"

# Create test group
$groupBody = @{ DepartmentId = $dept.id; Name = "TestGroup" } | ConvertTo-Json
$group = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Post -Body $groupBody -ContentType "application/json").data
Write-Host "Created group: $($group.id)"

# Create test employee
$employeeBody = @{
    Name = "TestEmployee"
    LoginUsername = "testuser_$(Get-Date -Format 'HHmmss')"
    Phone = "1234567890"
    GroupId = $group.id
    RoleId = 5
    Password = "123456"
} | ConvertTo-Json

$employee = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Post -Body $employeeBody -ContentType "application/json").data
Write-Host "Created employee: $($employee.id)"

# Verify data exists
$allDepts = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Get).data
$allGroups = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get).data
$allEmployees = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data

Write-Host "Before deletion:"
Write-Host "  Departments: $($allDepts.Count)"
Write-Host "  Groups: $($allGroups.Count)"
Write-Host "  Employees: $($allEmployees.Count)"

# Delete department (should cascade delete everything)
Write-Host "`nDeleting department with cascade..." -ForegroundColor Yellow
$deleteResult = Invoke-RestMethod -Uri "$baseUrl/departments/$($dept.id)" -Method Delete

Write-Host "Delete result:"
Write-Host "  Success: $($deleteResult.isSuccess)"
Write-Host "  Message: $($deleteResult.message)"
if ($deleteResult.data) {
    Write-Host "  Deleted groups: $($deleteResult.data.deletedGroupCount)"
    Write-Host "  Deleted employees: $($deleteResult.data.deletedEmployeeCount)"
}

# Verify everything is deleted
$deptsAfter = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Get).data
$groupsAfter = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get).data
$employeesAfter = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data

Write-Host "`nAfter deletion:"
Write-Host "  Departments: $($deptsAfter.Count)"
Write-Host "  Groups: $($groupsAfter.Count)"
Write-Host "  Employees: $($employeesAfter.Count)"

$deptExists = $deptsAfter | Where-Object { $_.id -eq $dept.id }
$groupExists = $groupsAfter | Where-Object { $_.id -eq $group.id }
$employeeExists = $employeesAfter | Where-Object { $_.id -eq $employee.id }

Write-Host "`nVerification:"
if ($deptExists) { Write-Host "  ERROR: Department still exists!" -ForegroundColor Red } else { Write-Host "  OK: Department deleted" -ForegroundColor Green }
if ($groupExists) { Write-Host "  ERROR: Group still exists!" -ForegroundColor Red } else { Write-Host "  OK: Group deleted" -ForegroundColor Green }
if ($employeeExists) { Write-Host "  ERROR: Employee still exists!" -ForegroundColor Red } else { Write-Host "  OK: Employee deleted" -ForegroundColor Green }

Write-Host "`nCascade deletion test completed!" -ForegroundColor Green
