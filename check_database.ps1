# Check database directly
$baseUrl = "http://localhost:5078/api"

Write-Host "=== CHECKING DATABASE STATE ===" -ForegroundColor Cyan

# Get all current data
Write-Host "`n1. Current database state:" -ForegroundColor Yellow
$allDepts = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Get).data
$allGroups = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get).data
$allEmployees = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data

Write-Host "Departments: $($allDepts.Count)"
foreach ($dept in $allDepts) {
    Write-Host "  Dept ID: $($dept.id), Name: $($dept.name)"
}

Write-Host "`nGroups: $($allGroups.Count)"
foreach ($group in $allGroups) {
    Write-Host "  Group ID: $($group.id), Name: $($group.name), DeptId: $($group.departmentId)"
}

Write-Host "`nEmployees: $($allEmployees.Count)"
foreach ($emp in $allEmployees) {
    Write-Host "  Emp ID: $($emp.id), Name: $($emp.name), GroupId: $($emp.groupId), GroupName: $($emp.groupName), DeptName: $($emp.departmentName)"
}

# Create new test data
Write-Host "`n2. Creating new test data..." -ForegroundColor Yellow
$deptBody = "`"TestDept_$(Get-Date -Format 'HHmmss')`""
$dept = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Post -Body $deptBody -ContentType "application/json").data
Write-Host "Created department: ID=$($dept.id), Name=$($dept.name)"

$groupBody = @{ DepartmentId = $dept.id; Name = "TestGroup_$(Get-Date -Format 'HHmmss')" } | ConvertTo-Json
$group = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Post -Body $groupBody -ContentType "application/json").data
Write-Host "Created group: ID=$($group.id), Name=$($group.name)"

$employeeBody = @{
    Name = "TestEmployee_$(Get-Date -Format 'HHmmss')"
    LoginUsername = "testuser_$(Get-Date -Format 'HHmmss')"
    Phone = "1234567890"
    GroupId = $group.id
    RoleId = 5
    Password = "123456"
} | ConvertTo-Json

$employee = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Post -Body $employeeBody -ContentType "application/json").data
Write-Host "Created employee: ID=$($employee.id), Name=$($employee.name)"

# Check state after creation
Write-Host "`n3. State after creation:" -ForegroundColor Yellow
$allEmployeesAfterCreate = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data
$newEmployee = $allEmployeesAfterCreate | Where-Object { $_.id -eq $employee.id }
if ($newEmployee) {
    Write-Host "Employee found: ID=$($newEmployee.id), Name=$($newEmployee.name), GroupId=$($newEmployee.groupId)"
} else {
    Write-Host "ERROR: Employee not found after creation!" -ForegroundColor Red
}

# Delete department
Write-Host "`n4. Deleting department..." -ForegroundColor Yellow
$deleteResult = Invoke-RestMethod -Uri "$baseUrl/departments/$($dept.id)" -Method Delete
Write-Host "Delete result: Success=$($deleteResult.isSuccess), Message=$($deleteResult.message)"
if ($deleteResult.data) {
    Write-Host "Reported deletions: Groups=$($deleteResult.data.deletedGroupCount), Employees=$($deleteResult.data.deletedEmployeeCount)"
}

# Check state after deletion
Write-Host "`n5. State after deletion:" -ForegroundColor Yellow
$allEmployeesAfterDelete = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data
$deletedEmployee = $allEmployeesAfterDelete | Where-Object { $_.id -eq $employee.id }
if ($deletedEmployee) {
    Write-Host "ERROR: Employee still exists after deletion!" -ForegroundColor Red
    Write-Host "  ID: $($deletedEmployee.id), Name: $($deletedEmployee.name), GroupId: $($deletedEmployee.groupId)"
} else {
    Write-Host "OK: Employee successfully deleted" -ForegroundColor Green
}

$allGroupsAfterDelete = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get).data
$deletedGroup = $allGroupsAfterDelete | Where-Object { $_.id -eq $group.id }
if ($deletedGroup) {
    Write-Host "ERROR: Group still exists after deletion!" -ForegroundColor Red
    Write-Host "  ID: $($deletedGroup.id), Name: $($deletedGroup.name)"
} else {
    Write-Host "OK: Group successfully deleted" -ForegroundColor Green
}

Write-Host "`n=== CHECK COMPLETE ===" -ForegroundColor Cyan
