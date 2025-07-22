# Check specific employees from screenshot
$baseUrl = "http://localhost:5078/api"

Write-Host "=== CHECKING ORPHAN EMPLOYEES ===" -ForegroundColor Red

# Get all employees
$allEmployees = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data
$allGroups = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get).data

Write-Host "`nAll employees:"
foreach ($emp in $allEmployees) {
    $groupExists = $allGroups | Where-Object { $_.id -eq $emp.groupId }
    $status = if ($groupExists) { "OK" } else { "ORPHAN" }
    Write-Host "  ID: $($emp.id), Name: $($emp.name), GroupId: $($emp.groupId), Status: $status"
    if (-not $groupExists -and $emp.groupId) {
        Write-Host "    ERROR: Employee references non-existent group $($emp.groupId)!" -ForegroundColor Red
    }
}

# Check specific employees from screenshot (ID 14 and 15)
Write-Host "`nChecking specific employees from screenshot:"
$employee14 = $allEmployees | Where-Object { $_.id -eq 14 }
$employee15 = $allEmployees | Where-Object { $_.id -eq 15 }

if ($employee14) {
    Write-Host "Employee 14: Name=$($employee14.name), GroupId=$($employee14.groupId), GroupName=$($employee14.groupName)"
    $group14 = $allGroups | Where-Object { $_.id -eq $employee14.groupId }
    if (-not $group14) {
        Write-Host "  ERROR: Employee 14 references non-existent group!" -ForegroundColor Red
    }
} else {
    Write-Host "Employee 14: NOT FOUND (already deleted)" -ForegroundColor Green
}

if ($employee15) {
    Write-Host "Employee 15: Name=$($employee15.name), GroupId=$($employee15.groupId), GroupName=$($employee15.groupName)"
    $group15 = $allGroups | Where-Object { $_.id -eq $employee15.groupId }
    if (-not $group15) {
        Write-Host "  ERROR: Employee 15 references non-existent group!" -ForegroundColor Red
    }
} else {
    Write-Host "Employee 15: NOT FOUND (already deleted)" -ForegroundColor Green
}

# Check all groups
Write-Host "`nAll groups:"
foreach ($group in $allGroups) {
    Write-Host "  Group ID: $($group.id), Name: $($group.name), DeptId: $($group.departmentId)"
}

Write-Host "`n=== CHECK COMPLETE ===" -ForegroundColor Red
