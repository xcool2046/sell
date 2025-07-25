@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo.
echo ========================================
echo   Sellsys 服务器地址更新工具
echo ========================================
echo.

REM 检查是否提供了服务器地址参数
if "%1"=="" (
    echo 用法: %0 ^<服务器IP或域名^> [端口]
    echo.
    echo 示例:
    echo   %0 192.168.1.100
    echo   %0 192.168.1.100 8080
    echo   %0 myserver.com
    echo.
    pause
    exit /b 1
)

set SERVER=%1
set PORT=%2
if "%PORT%"=="" set PORT=5000

echo 目标服务器: %SERVER%:%PORT%
echo.

REM 检查源代码文件是否存在
set API_FILE=src\Sellsys.WpfClient\Services\ApiService.cs
if not exist "%API_FILE%" (
    echo [错误] 找不到客户端源代码文件: %API_FILE%
    echo 请确保在项目根目录执行此脚本
    echo.
    pause
    exit /b 1
)

echo [信息] 正在测试服务器连接...
ping -n 1 %SERVER% >nul 2>&1
if %errorlevel% == 0 (
    echo ✓ 服务器网络连接正常
) else (
    echo ✗ 服务器网络连接失败，请检查服务器地址
)

REM 测试HTTP服务
echo [信息] 测试HTTP服务...
curl -s --connect-timeout 10 http://%SERVER%:%PORT%/health >nul 2>&1
if %errorlevel% == 0 (
    echo ✓ HTTP服务正常
) else (
    echo ✗ HTTP服务连接失败，请确认服务已启动
)

echo.
echo [信息] 备份原配置文件...
set BACKUP_FILE=%API_FILE%.backup.%date:~0,4%%date:~5,2%%date:~8,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set BACKUP_FILE=%BACKUP_FILE: =0%
copy "%API_FILE%" "%BACKUP_FILE%" >nul
echo ✓ 已备份到: %BACKUP_FILE%

echo.
echo [信息] 更新服务器配置...
set NEW_URL=http://%SERVER%:%PORT%/api

REM 使用PowerShell更新配置文件
powershell -Command "(Get-Content '%API_FILE%') -replace 'private const string ProductionUrl = \"http://.*:.*/api\";', 'private const string ProductionUrl = \"%NEW_URL%\";' | Set-Content '%API_FILE%'"

REM 验证更新是否成功
findstr /C:"%NEW_URL%" "%API_FILE%" >nul
if %errorlevel% == 0 (
    echo ✓ 配置文件更新成功
    echo   新地址: %NEW_URL%
) else (
    echo ✗ 配置文件更新失败
    pause
    exit /b 1
)

echo.
echo [信息] 编译新的客户端...
if exist "publish\wpfclient-new" rmdir /s /q "publish\wpfclient-new"

dotnet publish src\Sellsys.WpfClient\Sellsys.WpfClient.csproj -c Release -o publish\wpfclient-new --self-contained true -r win-x64 --verbosity quiet

if %errorlevel% == 0 (
    echo ✓ 客户端编译成功
) else (
    echo ✗ 客户端编译失败
    pause
    exit /b 1
)

echo.
echo [信息] 创建客户端发布包...

REM 生成包名
set SERVER_CLEAN=%SERVER:.=-%
set TIMESTAMP=%date:~0,4%%date:~5,2%%date:~8,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set TIMESTAMP=%TIMESTAMP: =0%
set PACKAGE_NAME=Sellsys-Client-%SERVER_CLEAN%-%TIMESTAMP%

REM 创建包目录
if exist "%PACKAGE_NAME%" rmdir /s /q "%PACKAGE_NAME%"
mkdir "%PACKAGE_NAME%"

REM 复制文件
xcopy "publish\wpfclient-new\*" "%PACKAGE_NAME%\" /E /Q >nul

REM 创建配置说明文件
echo # Sellsys 客户端 - 服务器配置信息 > "%PACKAGE_NAME%\服务器配置.txt"
echo. >> "%PACKAGE_NAME%\服务器配置.txt"
echo ## 服务器信息 >> "%PACKAGE_NAME%\服务器配置.txt"
echo - 服务器地址: %SERVER% >> "%PACKAGE_NAME%\服务器配置.txt"
echo - 端口: %PORT% >> "%PACKAGE_NAME%\服务器配置.txt"
echo - 完整API地址: http://%SERVER%:%PORT%/api >> "%PACKAGE_NAME%\服务器配置.txt"
echo - 健康检查: http://%SERVER%:%PORT%/health >> "%PACKAGE_NAME%\服务器配置.txt"
echo. >> "%PACKAGE_NAME%\服务器配置.txt"
echo ## 更新时间 >> "%PACKAGE_NAME%\服务器配置.txt"
echo %date% %time% >> "%PACKAGE_NAME%\服务器配置.txt"
echo. >> "%PACKAGE_NAME%\服务器配置.txt"
echo ## 使用说明 >> "%PACKAGE_NAME%\服务器配置.txt"
echo 1. 双击 Sellsys.WpfClient.exe 启动程序 >> "%PACKAGE_NAME%\服务器配置.txt"
echo 2. 程序会自动连接到配置的服务器 >> "%PACKAGE_NAME%\服务器配置.txt"
echo 3. 如有连接问题，请检查网络和服务器状态 >> "%PACKAGE_NAME%\服务器配置.txt"
echo. >> "%PACKAGE_NAME%\服务器配置.txt"
echo 如有问题请联系技术支持。 >> "%PACKAGE_NAME%\服务器配置.txt"

REM 创建启动脚本
echo @echo off > "%PACKAGE_NAME%\启动程序.bat"
echo echo. >> "%PACKAGE_NAME%\启动程序.bat"
echo echo ======================================== >> "%PACKAGE_NAME%\启动程序.bat"
echo echo   Sellsys 销售管理系统 >> "%PACKAGE_NAME%\启动程序.bat"
echo echo   服务器: %SERVER%:%PORT% >> "%PACKAGE_NAME%\启动程序.bat"
echo echo ======================================== >> "%PACKAGE_NAME%\启动程序.bat"
echo echo. >> "%PACKAGE_NAME%\启动程序.bat"
echo echo 正在检查服务器连接... >> "%PACKAGE_NAME%\启动程序.bat"
echo echo. >> "%PACKAGE_NAME%\启动程序.bat"
echo ping -n 1 %SERVER% ^>nul 2^>^&1 >> "%PACKAGE_NAME%\启动程序.bat"
echo if %%errorlevel%% == 0 ^( >> "%PACKAGE_NAME%\启动程序.bat"
echo     echo ✓ 服务器网络连接正常 >> "%PACKAGE_NAME%\启动程序.bat"
echo ^) else ^( >> "%PACKAGE_NAME%\启动程序.bat"
echo     echo ✗ 服务器网络连接失败 >> "%PACKAGE_NAME%\启动程序.bat"
echo     echo 请检查: >> "%PACKAGE_NAME%\启动程序.bat"
echo     echo 1. 网络连接是否正常 >> "%PACKAGE_NAME%\启动程序.bat"
echo     echo 2. 服务器地址是否正确: %SERVER% >> "%PACKAGE_NAME%\启动程序.bat"
echo     echo 3. 服务器是否正在运行 >> "%PACKAGE_NAME%\启动程序.bat"
echo     echo. >> "%PACKAGE_NAME%\启动程序.bat"
echo     pause >> "%PACKAGE_NAME%\启动程序.bat"
echo     exit /b 1 >> "%PACKAGE_NAME%\启动程序.bat"
echo ^) >> "%PACKAGE_NAME%\启动程序.bat"
echo echo. >> "%PACKAGE_NAME%\启动程序.bat"
echo echo ✓ 正在启动客户端... >> "%PACKAGE_NAME%\启动程序.bat"
echo start Sellsys.WpfClient.exe >> "%PACKAGE_NAME%\启动程序.bat"
echo echo. >> "%PACKAGE_NAME%\启动程序.bat"
echo echo 程序已启动！ >> "%PACKAGE_NAME%\启动程序.bat"
echo echo 如有问题请查看"服务器配置.txt"文件 >> "%PACKAGE_NAME%\启动程序.bat"
echo timeout /t 3 ^>nul >> "%PACKAGE_NAME%\启动程序.bat"

REM 创建压缩包（如果有7zip）
where 7z >nul 2>&1
if %errorlevel% == 0 (
    echo [信息] 创建压缩包...
    7z a "%PACKAGE_NAME%.zip" "%PACKAGE_NAME%" >nul
    echo ✓ 已创建压缩包: %PACKAGE_NAME%.zip
)

echo.
echo ========================================
echo   更新完成！
echo ========================================
echo.
echo ✓ 客户端包已创建: %PACKAGE_NAME%
echo ✓ 新服务器地址: http://%SERVER%:%PORT%

REM 计算包大小
for /f "tokens=3" %%a in ('dir "%PACKAGE_NAME%" ^| findstr "个文件"') do set FILE_COUNT=%%a
echo ✓ 包含文件数: %FILE_COUNT%

echo.
echo 后续步骤:
echo 1. 测试新客户端是否能正常连接服务器
echo 2. 将客户端包发送给所有用户
echo 3. 指导用户卸载旧版本并安装新版本
echo 4. 确认所有用户都已更新完成
echo.

REM 询问是否测试新客户端
set /p TEST_CLIENT=是否现在测试新客户端？(y/n): 
if /i "%TEST_CLIENT%"=="y" (
    echo.
    echo 正在启动测试客户端...
    start "" "%PACKAGE_NAME%\Sellsys.WpfClient.exe"
)

echo.
echo 按任意键退出...
pause >nul
