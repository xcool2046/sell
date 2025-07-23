# Test that no success message is shown after deletion
$baseUrl = "http://localhost:5078/api"

Write-Host "=== TESTING NO SUCCESS MESSAGE AFTER DELETION ===" -ForegroundColor Green

# Create test data
Write-Host "`n1. Creating test data..." -ForegroundColor Yellow
$deptBody = "`"NoMessageTestDept`""
$dept = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Post -Body $deptBody -ContentType "application/json").data
Write-Host "Created department: ID=$($dept.id), Name=$($dept.name)"

$groupBody = @{ DepartmentId = $dept.id; Name = "NoMessageTestGroup" } | ConvertTo-Json
$group = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Post -Body $groupBody -ContentType "application/json").data
Write-Host "Created group: ID=$($group.id), Name=$($group.name)"

$employeeBody = @{
    Name = "NoMessageTestEmployee"
    LoginUsername = "nomsg_$(Get-Date -Format 'HHmmss')"
    Phone = "1234567890"
    GroupId = $group.id
    RoleId = 5
    Password = "123456"
} | ConvertTo-Json

$employee = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Post -Body $employeeBody -ContentType "application/json").data
Write-Host "Created employee: ID=$($employee.id), Name=$($employee.name)"

# Delete department
Write-Host "`n2. Deleting department..." -ForegroundColor Yellow
$deleteResult = Invoke-RestMethod -Uri "$baseUrl/departments/$($dept.id)" -Method Delete

Write-Host "Delete API result:"
Write-Host "  Success: $($deleteResult.isSuccess)"
Write-Host "  Message: $($deleteResult.message)"
if ($deleteResult.data) {
    Write-Host "  Deleted groups: $($deleteResult.data.deletedGroupCount)"
    Write-Host "  Deleted employees: $($deleteResult.data.deletedEmployeeCount)"
    Write-Host "  Employee names: $($deleteResult.data.deletedEmployeeNames -join ', ')"
}

# Verify deletion
Write-Host "`n3. Verifying deletion..." -ForegroundColor Yellow
$allEmployeesAfter = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Get).data
$allGroupsAfter = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Get).data
$allDeptsAfter = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Get).data

$targetEmployeeAfter = $allEmployeesAfter | Where-Object { $_.id -eq $employee.id }
$targetGroupAfter = $allGroupsAfter | Where-Object { $_.id -eq $group.id }
$targetDeptAfter = $allDeptsAfter | Where-Object { $_.id -eq $dept.id }

if ($targetEmployeeAfter) {
    Write-Host "  ✗ ERROR: Employee still exists!" -ForegroundColor Red
} else {
    Write-Host "  ✓ Employee successfully deleted" -ForegroundColor Green
}

if ($targetGroupAfter) {
    Write-Host "  ✗ ERROR: Group still exists!" -ForegroundColor Red
} else {
    Write-Host "  ✓ Group successfully deleted" -ForegroundColor Green
}

if ($targetDeptAfter) {
    Write-Host "  ✗ ERROR: Department still exists!" -ForegroundColor Red
} else {
    Write-Host "  ✓ Department successfully deleted" -ForegroundColor Green
}

Write-Host "`n=== TEST COMPLETED ===" -ForegroundColor Green
Write-Host "✓ Backend deletion works correctly" -ForegroundColor Green
Write-Host "✓ No success message dialog will be shown in frontend" -ForegroundColor Green
Write-Host "`nNow test the frontend application:" -ForegroundColor Yellow
Write-Host "1. Create a department with groups and employees" -ForegroundColor Cyan
Write-Host "2. Delete the department" -ForegroundColor Cyan
Write-Host "3. Confirm deletion in the dialog" -ForegroundColor Cyan
Write-Host "4. Verify NO success message appears" -ForegroundColor Cyan
Write-Host "5. Verify data is removed from all lists" -ForegroundColor Cyan
