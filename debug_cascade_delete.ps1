# Debug cascade deletion issue
$baseUrl = "http://localhost:5078/api"

Write-Host "=== DEBUGGING CASCADE DELETE ISSUE ===" -ForegroundColor Red

# Create test data
Write-Host "`n1. Creating test data..." -ForegroundColor Yellow
$deptBody = "`"DebugDept`""
$dept = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Post -Body $deptBody -ContentType "application/json").data
Write-Host "Created department: ID=$($dept.id), Name=$($dept.name)"

$groupBody = @{ DepartmentId = $dept.id; Name = "DebugGroup" } | ConvertTo-Json
$group = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Post -Body $groupBody -ContentType "application/json").data
Write-Host "Created group: ID=$($group.id), Name=$($group.name)"

$employeeBody = @{
    Name = "DebugEmployee"
    LoginUsername = "debug_$(Get-Date -Format 'HHmmss')"
    Phone = "1234567890"
    GroupId = $group.id
    RoleId = 5
    Password = "123456"
} | ConvertTo-Json

$employee = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Post -Body $employeeBody -ContentType "application/json").data
Write-Host "Created employee: ID=$($employee.id), Name=$($employee.name)"

# Verify data before deletion
Write-Host "`n2. Verifying data before deletion..." -ForegroundColor Yellow
$allEmployeesBefore = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data
$targetEmployee = $allEmployeesBefore | Where-Object { $_.id -eq $employee.id }
if ($targetEmployee) {
    Write-Host "Employee exists: ID=$($targetEmployee.id), Name=$($targetEmployee.name), GroupId=$($targetEmployee.groupId)"
} else {
    Write-Host "ERROR: Employee not found!" -ForegroundColor Red
}

# Check department with groups and employees
Write-Host "`n3. Checking department structure..." -ForegroundColor Yellow
$deptWithGroups = (Invoke-RestMethod -Uri "$baseUrl/departments/$($dept.id)" -Method Get).data
if ($deptWithGroups) {
    Write-Host "Department: $($deptWithGroups.name)"
    Write-Host "Groups count: $($deptWithGroups.groups.Count)"
    foreach ($g in $deptWithGroups.groups) {
        Write-Host "  Group: $($g.name) (ID: $($g.id))"
        Write-Host "  Employees in group: $($g.employees.Count)"
        foreach ($e in $g.employees) {
            Write-Host "    Employee: $($e.name) (ID: $($e.id))"
        }
    }
}

# Delete department
Write-Host "`n4. Deleting department..." -ForegroundColor Yellow
try {
    $deleteResult = Invoke-RestMethod -Uri "$baseUrl/departments/$($dept.id)" -Method Delete
    Write-Host "Delete result:"
    Write-Host "  Success: $($deleteResult.isSuccess)"
    Write-Host "  Message: $($deleteResult.message)"
    if ($deleteResult.data) {
        Write-Host "  Deleted groups: $($deleteResult.data.deletedGroupCount)"
        Write-Host "  Deleted employees: $($deleteResult.data.deletedEmployeeCount)"
        Write-Host "  Employee names: $($deleteResult.data.deletedEmployeeNames -join ', ')"
    }
} catch {
    Write-Host "Delete failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Verify data after deletion
Write-Host "`n5. Verifying data after deletion..." -ForegroundColor Yellow
$allEmployeesAfter = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data
$targetEmployeeAfter = $allEmployeesAfter | Where-Object { $_.id -eq $employee.id }

if ($targetEmployeeAfter) {
    Write-Host "ERROR: Employee still exists!" -ForegroundColor Red
    Write-Host "  ID: $($targetEmployeeAfter.id)"
    Write-Host "  Name: $($targetEmployeeAfter.name)"
    Write-Host "  GroupId: $($targetEmployeeAfter.groupId)"
    Write-Host "  GroupName: $($targetEmployeeAfter.groupName)"
    Write-Host "  DepartmentName: $($targetEmployeeAfter.departmentName)"
} else {
    Write-Host "OK: Employee successfully deleted" -ForegroundColor Green
}

# Check if group still exists
$allGroupsAfter = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get).data
$targetGroupAfter = $allGroupsAfter | Where-Object { $_.id -eq $group.id }

if ($targetGroupAfter) {
    Write-Host "ERROR: Group still exists!" -ForegroundColor Red
    Write-Host "  ID: $($targetGroupAfter.id)"
    Write-Host "  Name: $($targetGroupAfter.name)"
    Write-Host "  DepartmentId: $($targetGroupAfter.departmentId)"
} else {
    Write-Host "OK: Group successfully deleted" -ForegroundColor Green
}

Write-Host "`n=== DEBUG COMPLETE ===" -ForegroundColor Red
