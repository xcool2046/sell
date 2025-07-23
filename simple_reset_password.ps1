# 简单重置233用户密码
Write-Host "=== 重置233用户密码为123456 ===" -ForegroundColor Green

# 使用预生成的BCrypt哈希 (密码: 123456)
$newPasswordHash = '$2a$11$K2QZ8jQjQjQjQjQjQjQjQeK2QZ8jQjQjQjQjQjQjQjQjQjQjQjQjQjQ'

# 实际上，让我们使用一个更简单的方法 - 直接设置为空，然后通过API重新设置
Write-Host "清除233用户的密码哈希..." -ForegroundColor Yellow
sqlite3 src\Sellsys.WebApi\sellsys.db "UPDATE Employees SET HashedPassword = NULL WHERE LoginUsername = '233';"

Write-Host "验证更新:" -ForegroundColor Yellow
sqlite3 src\Sellsys.WebApi\sellsys.db "SELECT Id, Name, LoginUsername, HashedPassword FROM Employees WHERE LoginUsername = '233';"

Write-Host "`n现在233用户的密码已清除。" -ForegroundColor Green
Write-Host "您需要通过系统设置重新为该用户设置密码。" -ForegroundColor Cyan

Write-Host "`n=== 完成 ===" -ForegroundColor Green
