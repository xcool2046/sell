# 重置233用户密码的脚本
Write-Host "=== 重置233用户密码 ===" -ForegroundColor Green

# 新密码
$newPassword = "123456"

# 使用BCrypt生成密码哈希
# 这里我们需要调用.NET的BCrypt库来生成哈希
$bcryptHash = ""

# 创建一个临时的C#脚本来生成BCrypt哈希
$csharpCode = @"
using System;
using BCrypt.Net;

public class PasswordHasher
{
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            string password = args[0];
            string hash = BCrypt.HashPassword(password);
            Console.WriteLine(hash);
        }
    }
}
"@

# 保存C#代码到临时文件
$tempCsFile = "temp_hasher.cs"
$csharpCode | Out-File -FilePath $tempCsFile -Encoding UTF8

try {
    # 编译并运行C#代码生成哈希
    Write-Host "生成密码哈希..." -ForegroundColor Yellow
    
    # 添加BCrypt.Net包引用并编译
    $compileResult = dotnet new console -n TempHasher -f net8.0 2>$null
    Set-Location TempHasher
    dotnet add package BCrypt.Net-Next 2>$null
    
    # 替换Program.cs
    $csharpCode | Out-File -FilePath "Program.cs" -Encoding UTF8
    
    # 编译并运行
    $hash = dotnet run $newPassword 2>$null
    Set-Location ..
    
    if ($hash) {
        Write-Host "生成的哈希: $hash" -ForegroundColor Cyan
        
        # 更新数据库中的密码
        Write-Host "更新数据库中的密码..." -ForegroundColor Yellow
        $updateQuery = "UPDATE Employees SET HashedPassword = '$hash' WHERE LoginUsername = '233';"
        sqlite3 src\Sellsys.WebApi\sellsys.db $updateQuery
        
        Write-Host "密码更新成功!" -ForegroundColor Green
        Write-Host "新密码: $newPassword" -ForegroundColor Cyan
        
        # 验证更新
        Write-Host "`n验证更新:" -ForegroundColor Yellow
        sqlite3 src\Sellsys.WebApi\sellsys.db "SELECT Id, Name, LoginUsername, HashedPassword FROM Employees WHERE LoginUsername = '233';"
    } else {
        Write-Host "生成哈希失败" -ForegroundColor Red
    }
} catch {
    Write-Host "错误: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    # 清理临时文件
    if (Test-Path $tempCsFile) { Remove-Item $tempCsFile }
    if (Test-Path "TempHasher") { Remove-Item -Recurse -Force "TempHasher" }
}

Write-Host "`n=== 完成 ===" -ForegroundColor Green
