#!/bin/bash

# Sellsys 服务器地址更新工具
# 用于快速更新客户端的服务器连接地址

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

show_help() {
    echo "Sellsys 服务器地址更新工具"
    echo ""
    echo "用法:"
    echo "  $0 <新服务器IP或域名> [端口]"
    echo ""
    echo "参数:"
    echo "  新服务器IP或域名    新服务器的IP地址或域名"
    echo "  端口              可选，默认为5000"
    echo ""
    echo "示例:"
    echo "  $0 192.168.1.100"
    echo "  $0 192.168.1.100 8080"
    echo "  $0 myserver.com"
    echo "  $0 myserver.com 5000"
}

# 验证IP地址格式
validate_ip() {
    local ip=$1
    if [[ $ip =~ ^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$ ]]; then
        return 0
    else
        return 1
    fi
}

# 验证域名格式
validate_domain() {
    local domain=$1
    if [[ $domain =~ ^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$ ]]; then
        return 0
    else
        return 1
    fi
}

# 测试服务器连接
test_server_connection() {
    local server=$1
    local port=$2
    
    print_info "测试服务器连接: $server:$port"
    
    # 测试端口连通性
    if command -v nc >/dev/null 2>&1; then
        if nc -z -w5 $server $port 2>/dev/null; then
            print_success "端口 $port 连通正常"
        else
            print_warning "端口 $port 连接失败，请检查服务器状态"
        fi
    fi
    
    # 测试HTTP连接
    if command -v curl >/dev/null 2>&1; then
        if curl -s --connect-timeout 10 http://$server:$port/health >/dev/null 2>&1; then
            print_success "HTTP服务正常"
        else
            print_warning "HTTP服务连接失败，请确认服务已启动"
        fi
    fi
}

# 更新客户端配置
update_client_config() {
    local server=$1
    local port=$2
    local new_url="http://$server:$port/api"
    
    print_info "更新客户端配置..."
    print_info "新服务器地址: $new_url"
    
    # 检查源代码文件
    local api_service_file="src/Sellsys.WpfClient/Services/ApiService.cs"
    if [ ! -f "$api_service_file" ]; then
        print_error "找不到客户端源代码文件: $api_service_file"
        print_error "请确保在项目根目录执行此脚本"
        exit 1
    fi
    
    # 备份原文件
    cp "$api_service_file" "${api_service_file}.backup.$(date +%Y%m%d_%H%M%S)"
    print_info "已备份原配置文件"
    
    # 更新生产环境URL
    sed -i "s|private const string ProductionUrl = \"http://.*:.*\/api\";|private const string ProductionUrl = \"$new_url\";|g" "$api_service_file"
    
    # 验证更新是否成功
    if grep -q "$new_url" "$api_service_file"; then
        print_success "配置文件更新成功"
    else
        print_error "配置文件更新失败"
        exit 1
    fi
    
    # 显示更新后的配置
    print_info "当前配置:"
    grep "ProductionUrl" "$api_service_file" | sed 's/^[[:space:]]*/  /'
}

# 编译新的客户端
build_client() {
    local server=$1
    local port=$2
    
    print_info "编译新的客户端..."
    
    # 清理之前的编译结果
    rm -rf publish/wpfclient-new
    
    # 编译客户端
    dotnet publish src/Sellsys.WpfClient/Sellsys.WpfClient.csproj \
        -c Release \
        -o publish/wpfclient-new \
        --self-contained true \
        -r win-x64 \
        --verbosity quiet
    
    if [ $? -eq 0 ]; then
        print_success "客户端编译成功"
    else
        print_error "客户端编译失败"
        exit 1
    fi
}

# 创建客户端发布包
create_client_package() {
    local server=$1
    local port=$2
    
    print_info "创建客户端发布包..."
    
    local package_name="Sellsys-Client-$(echo $server | tr '.' '-')-$(date +%Y%m%d_%H%M%S)"
    
    # 创建包目录
    mkdir -p "$package_name"
    
    # 复制编译结果
    cp -r publish/wpfclient-new/* "$package_name"/
    
    # 创建配置说明文件
    cat > "$package_name/服务器配置.txt" << EOF
# Sellsys 客户端 - 服务器配置信息

## 服务器信息
- 服务器地址: $server
- 端口: $port
- 完整API地址: http://$server:$port/api
- 健康检查: http://$server:$port/health

## 更新时间
$(date)

## 使用说明
1. 双击 Sellsys.WpfClient.exe 启动程序
2. 程序会自动连接到配置的服务器
3. 如有连接问题，请检查网络和服务器状态

## 测试连接
可以通过以下方式测试服务器连接：
- 浏览器访问: http://$server:$port/health
- 应该显示: Healthy

如有问题请联系技术支持。
EOF
    
    # 创建启动脚本
    cat > "$package_name/启动程序.bat" << EOF
@echo off
echo.
echo ========================================
echo   Sellsys 销售管理系统
echo   服务器: $server:$port
echo ========================================
echo.
echo 正在检查服务器连接...

ping -n 1 $server >nul 2>&1
if %errorlevel% == 0 (
    echo ✓ 服务器网络连接正常
) else (
    echo ✗ 服务器网络连接失败
    echo 请检查:
    echo 1. 网络连接是否正常
    echo 2. 服务器地址是否正确: $server
    echo 3. 服务器是否正在运行
    echo.
    pause
    exit /b 1
)

echo ✓ 正在启动客户端...
start Sellsys.WpfClient.exe

echo.
echo 程序已启动！
echo 如有问题请查看"服务器配置.txt"文件
timeout /t 3 >nul
EOF
    
    # 创建压缩包
    if command -v tar >/dev/null 2>&1; then
        tar -czf "$package_name.tar.gz" "$package_name"
        print_success "已创建压缩包: $package_name.tar.gz"
    fi
    
    print_success "客户端包创建完成: $package_name"
    
    # 显示包信息
    local package_size=$(du -sh "$package_name" | cut -f1)
    print_info "包大小: $package_size"
    print_info "包含文件数: $(find "$package_name" -type f | wc -l)"
}

# 主程序
main() {
    # 检查参数
    if [ $# -lt 1 ] || [ "$1" = "-h" ] || [ "$1" = "--help" ]; then
        show_help
        exit 0
    fi
    
    local server=$1
    local port=${2:-5000}
    
    # 验证服务器地址
    if ! validate_ip "$server" && ! validate_domain "$server"; then
        print_error "无效的服务器地址: $server"
        print_error "请提供有效的IP地址或域名"
        exit 1
    fi
    
    # 验证端口
    if ! [[ "$port" =~ ^[0-9]+$ ]] || [ "$port" -lt 1 ] || [ "$port" -gt 65535 ]; then
        print_error "无效的端口号: $port"
        print_error "端口号必须在 1-65535 范围内"
        exit 1
    fi
    
    print_info "开始更新服务器地址..."
    print_info "目标服务器: $server:$port"
    
    # 测试服务器连接
    test_server_connection "$server" "$port"
    
    # 更新配置
    update_client_config "$server" "$port"
    
    # 编译客户端
    build_client "$server" "$port"
    
    # 创建发布包
    create_client_package "$server" "$port"
    
    print_success "服务器地址更新完成！"
    print_info "请将新的客户端包分发给用户"
    
    # 显示后续步骤
    echo ""
    print_info "后续步骤:"
    echo "1. 测试新客户端是否能正常连接服务器"
    echo "2. 将客户端包发送给所有用户"
    echo "3. 指导用户卸载旧版本并安装新版本"
    echo "4. 确认所有用户都已更新完成"
}

# 执行主程序
main "$@"
