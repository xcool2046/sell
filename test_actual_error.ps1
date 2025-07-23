# Test actual error response from backend
$baseUrl = "http://localhost:5078/api"

Write-Host "Creating test scenario with employee..." -ForegroundColor Yellow

# Create department
$deptBody = "`"TestErrorDept`""
$dept = (Invoke-RestMethod -Uri "$baseUrl/departments" -Method Post -Body $deptBody -ContentType "application/json").data
Write-Host "Created department ID: $($dept.id)"

# Create group
$groupBody = @{ DepartmentId = $dept.id; Name = "TestGroup" } | ConvertTo-Json
$group = (Invoke-RestMethod -Uri "$baseUrl/departmentgroups" -Method Post -Body $groupBody -ContentType "application/json").data
Write-Host "Created group ID: $($group.id)"

# Create employee
$employeeBody = @{
    Name = "TestEmployee"
    LoginUsername = "testuser_$(Get-Date -Format 'HHmmss')"
    Phone = "1234567890"
    GroupId = $group.id
    RoleId = 5
    Password = "123456"
} | ConvertTo-Json

$employee = (Invoke-RestMethod -Uri "$baseUrl/employees" -Method Post -Body $employeeBody -ContentType "application/json").data
Write-Host "Created employee ID: $($employee.id)"

Write-Host "`nNow testing error response with curl..." -ForegroundColor Yellow
Write-Host "Department ID to test: $($dept.id)" -ForegroundColor Cyan
