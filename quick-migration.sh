#!/bin/bash

# Sellsys 快速迁移脚本
# 用于快速迁移 Sellsys 系统到新服务器

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 打印函数
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

# 显示帮助信息
show_help() {
    echo "Sellsys 快速迁移脚本"
    echo ""
    echo "用法:"
    echo "  $0 backup <旧服务器IP>              # 备份旧服务器数据"
    echo "  $0 deploy <新服务器IP>              # 部署到新服务器"
    echo "  $0 update-client <新服务器IP>       # 更新客户端配置"
    echo "  $0 full-migration <旧IP> <新IP>     # 完整迁移流程"
    echo ""
    echo "示例:"
    echo "  $0 backup 8.156.69.42"
    echo "  $0 deploy 192.168.1.100"
    echo "  $0 update-client 192.168.1.100"
    echo "  $0 full-migration 8.156.69.42 192.168.1.100"
}

# 备份旧服务器数据
backup_old_server() {
    local old_ip=$1
    
    print_info "开始备份旧服务器数据..."
    print_info "服务器IP: $old_ip"
    
    # 创建备份目录
    mkdir -p backup
    
    print_info "备份数据库文件..."
    scp -o StrictHostKeyChecking=no root@$old_ip:/opt/sellsys/sellsys.db backup/sellsys_backup_$(date +%Y%m%d_%H%M%S).db
    
    print_info "备份配置文件..."
    scp -o StrictHostKeyChecking=no root@$old_ip:/opt/sellsys/appsettings.json backup/appsettings_backup.json || true
    
    print_success "数据备份完成！"
    print_info "备份文件保存在 backup/ 目录中"
}

# 部署到新服务器
deploy_to_new_server() {
    local new_ip=$1
    
    print_info "开始部署到新服务器..."
    print_info "服务器IP: $new_ip"
    
    # 检查部署包是否存在
    if [ ! -d "deploy-package" ]; then
        print_error "部署包不存在！请确保 deploy-package 目录存在"
        exit 1
    fi
    
    print_info "上传部署文件到新服务器..."
    scp -r -o StrictHostKeyChecking=no deploy-package/* root@$new_ip:~/sellsys-deploy/
    
    # 上传数据库备份
    if [ -f "backup/sellsys_backup_"*".db" ]; then
        print_info "上传数据库备份..."
        latest_backup=$(ls -t backup/sellsys_backup_*.db | head -n1)
        scp -o StrictHostKeyChecking=no $latest_backup root@$new_ip:~/sellsys-deploy/sellsys_backup.db
    fi
    
    print_info "在新服务器上执行部署..."
    ssh -o StrictHostKeyChecking=no root@$new_ip << 'EOF'
        cd ~/sellsys-deploy
        
        # 配置环境
        echo "配置服务器环境..."
        bash setup-environment.sh
        
        # 部署应用
        echo "部署应用..."
        bash deploy.sh
        
        # 恢复数据库（如果存在备份）
        if [ -f "sellsys_backup.db" ]; then
            echo "恢复数据库..."
            sudo systemctl stop sellsys-webapi
            sudo cp sellsys_backup.db /opt/sellsys/sellsys.db
            sudo chown sellsys:sellsys /opt/sellsys/sellsys.db
            sudo systemctl start sellsys-webapi
        fi
        
        # 检查服务状态
        echo "检查服务状态..."
        sudo systemctl status sellsys-webapi --no-pager
        
        # 测试API
        echo "测试API接口..."
        sleep 5
        curl -f http://localhost:5000/health || echo "API测试失败"
EOF
    
    print_success "部署完成！"
    print_info "请访问 http://$new_ip:5000/health 验证服务状态"
}

# 更新客户端配置
update_client_config() {
    local new_ip=$1
    
    print_info "更新客户端配置..."
    print_info "新服务器IP: $new_ip"
    
    # 检查源代码是否存在
    if [ ! -f "src/Sellsys.WpfClient/Services/ApiService.cs" ]; then
        print_error "客户端源代码不存在！请确保在项目根目录执行"
        exit 1
    fi
    
    # 备份原文件
    cp src/Sellsys.WpfClient/Services/ApiService.cs src/Sellsys.WpfClient/Services/ApiService.cs.backup
    
    # 更新API地址
    print_info "更新API服务地址..."
    sed -i "s|private const string ProductionUrl = \"http://.*:5000/api\";|private const string ProductionUrl = \"http://$new_ip:5000/api\";|g" src/Sellsys.WpfClient/Services/ApiService.cs
    
    # 重新编译客户端
    print_info "重新编译客户端..."
    dotnet publish src/Sellsys.WpfClient/Sellsys.WpfClient.csproj \
        -c Release \
        -o publish/wpfclient-updated \
        --self-contained true \
        -r win-x64
    
    # 创建新的客户端包
    print_info "创建新的客户端包..."
    PACKAGE_NAME="Sellsys-Client-Updated-$(date +%Y%m%d_%H%M%S)"
    mkdir -p "$PACKAGE_NAME"
    cp -r publish/wpfclient-updated/* "$PACKAGE_NAME"/
    
    # 创建更新说明
    cat > "$PACKAGE_NAME/更新说明.txt" << EOF
# Sellsys 客户端更新版本

## 更新内容
- 服务器地址已更新为: http://$new_ip:5000
- 更新时间: $(date)

## 安装说明
1. 卸载旧版本客户端
2. 解压此文件包
3. 双击 Sellsys.WpfClient.exe 启动程序

## 服务器信息
- 新服务器地址: http://$new_ip:5000
- 健康检查: http://$new_ip:5000/health

如有问题请联系技术支持。
EOF
    
    # 创建启动脚本
    cat > "$PACKAGE_NAME/启动程序.bat" << EOF
@echo off
echo.
echo ========================================
echo   Sellsys 销售管理系统 (更新版)
echo   正在启动客户端...
echo ========================================
echo.
echo 新服务器地址: http://$new_ip:5000
echo 正在检查服务器状态...
echo.

ping -n 1 $new_ip >nul 2>&1
if %errorlevel% == 0 (
    echo ✓ 服务器网络连接正常
) else (
    echo ✗ 服务器网络连接失败，请检查网络设置
    pause
    exit /b 1
)

echo.
echo 正在启动客户端程序...
start Sellsys.WpfClient.exe
echo.
echo 程序已启动，如有问题请查看更新说明.txt
timeout /t 3 >nul
EOF
    
    # 创建压缩包
    tar -czf "$PACKAGE_NAME.tar.gz" "$PACKAGE_NAME"
    
    print_success "客户端更新完成！"
    print_info "新客户端包: $PACKAGE_NAME.tar.gz"
    print_warning "请将新客户端包分发给所有用户"
}

# 完整迁移流程
full_migration() {
    local old_ip=$1
    local new_ip=$2
    
    print_info "开始完整迁移流程..."
    print_info "从 $old_ip 迁移到 $new_ip"
    
    # 步骤1: 备份
    print_info "=== 步骤 1/3: 备份旧服务器数据 ==="
    backup_old_server $old_ip
    
    # 步骤2: 部署
    print_info "=== 步骤 2/3: 部署到新服务器 ==="
    deploy_to_new_server $new_ip
    
    # 步骤3: 更新客户端
    print_info "=== 步骤 3/3: 更新客户端配置 ==="
    update_client_config $new_ip
    
    print_success "迁移完成！"
    print_info "请按以下步骤完成最后的工作："
    echo "1. 测试新服务器: http://$new_ip:5000/health"
    echo "2. 分发新客户端给用户"
    echo "3. 确认所有用户更新完成后，停止旧服务器"
}

# 主程序
main() {
    case "$1" in
        "backup")
            if [ -z "$2" ]; then
                print_error "请提供旧服务器IP地址"
                show_help
                exit 1
            fi
            backup_old_server "$2"
            ;;
        "deploy")
            if [ -z "$2" ]; then
                print_error "请提供新服务器IP地址"
                show_help
                exit 1
            fi
            deploy_to_new_server "$2"
            ;;
        "update-client")
            if [ -z "$2" ]; then
                print_error "请提供新服务器IP地址"
                show_help
                exit 1
            fi
            update_client_config "$2"
            ;;
        "full-migration")
            if [ -z "$2" ] || [ -z "$3" ]; then
                print_error "请提供旧服务器IP和新服务器IP地址"
                show_help
                exit 1
            fi
            full_migration "$2" "$3"
            ;;
        *)
            show_help
            ;;
    esac
}

# 执行主程序
main "$@"
