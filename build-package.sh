#!/bin/bash

# Sellsys 通用构建打包脚本
# 用于创建客户端包、部署包等

set -e  # 遇到错误时退出

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 打印带颜色的消息
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
    echo "Sellsys 通用构建打包脚本"
    echo ""
    echo "用法: $0 [选项] <包类型>"
    echo ""
    echo "包类型:"
    echo "  client          创建客户端包"
    echo "  client-fixed    创建修复版客户端包"
    echo "  deployment      创建完整部署包"
    echo "  all             创建所有包"
    echo ""
    echo "选项:"
    echo "  -h, --help      显示此帮助信息"
    echo "  -c, --clean     构建前清理旧的发布文件"
    echo "  -b, --build     构建前先编译项目"
    echo "  -v, --verbose   显示详细输出"
    echo ""
    echo "示例:"
    echo "  $0 client                    # 创建客户端包"
    echo "  $0 -b -c deployment          # 清理、构建并创建部署包"
    echo "  $0 --build --clean all       # 清理、构建并创建所有包"
}

# 检查必要的目录和文件
check_prerequisites() {
    print_info "检查构建环境..."
    
    if [ ! -f "Sellsys.sln" ]; then
        print_error "未找到 Sellsys.sln 文件，请在项目根目录运行此脚本"
        exit 1
    fi
    
    if [ ! -d "src" ]; then
        print_error "未找到 src 目录"
        exit 1
    fi
    
    # 检查 dotnet 命令
    if ! command -v dotnet &> /dev/null; then
        print_error "未找到 dotnet 命令，请安装 .NET SDK"
        exit 1
    fi
    
    print_success "环境检查通过"
}

# 清理旧的发布文件
clean_publish() {
    print_info "清理旧的发布文件..."
    if [ -d "publish" ]; then
        rm -rf publish
        print_success "已清理 publish 目录"
    fi
}

# 构建项目
build_project() {
    print_info "构建项目..."
    
    # 构建 WebApi
    print_info "构建 WebApi..."
    dotnet publish src/Sellsys.WebApi/Sellsys.WebApi.csproj \
        -c Release \
        -o publish/webapi \
        --self-contained false
    
    # 构建 WPF 客户端
    print_info "构建 WPF 客户端..."
    dotnet publish src/Sellsys.WpfClient/Sellsys.WpfClient.csproj \
        -c Release \
        -o publish/wpfclient \
        --self-contained true \
        -r win-x64
    
    # 创建修复版客户端（连接生产环境）
    print_info "构建修复版 WPF 客户端..."
    dotnet publish src/Sellsys.WpfClient/Sellsys.WpfClient.csproj \
        -c Release \
        -o publish/wpfclient-fixed \
        --self-contained true \
        -r win-x64
    
    print_success "项目构建完成"
}

# 创建客户端包
create_client_package() {
    local package_type=$1
    local source_dir=""
    local package_name=""
    local description=""
    
    if [ "$package_type" = "client" ]; then
        source_dir="publish/wpfclient"
        package_name="Sellsys-Client-$(date +%Y%m%d)"
        description="标准客户端包"
    elif [ "$package_type" = "client-fixed" ]; then
        source_dir="publish/wpfclient-fixed"
        package_name="Sellsys-Client-Fixed-$(date +%Y%m%d_%H%M%S)"
        description="修复版客户端包"
    else
        print_error "未知的客户端包类型: $package_type"
        return 1
    fi
    
    print_info "创建 $description..."
    
    if [ ! -d "$source_dir" ]; then
        print_error "源目录不存在: $source_dir"
        print_warning "请先运行构建: $0 -b $package_type"
        return 1
    fi
    
    # 创建包目录
    mkdir -p "$package_name"
    
    # 复制文件
    cp -r "$source_dir"/* "$package_name"/
    
    # 创建说明文件
    if [ "$package_type" = "client" ]; then
        create_client_readme "$package_name"
    else
        create_fixed_client_readme "$package_name"
    fi
    
    # 创建启动脚本
    create_startup_script "$package_name"
    
    # 创建压缩包
    create_archive "$package_name"
    
    print_success "$description 创建完成: $package_name"
}

# 创建客户端说明文件
create_client_readme() {
    local dir=$1
    cat > "$dir/README.txt" << 'EOF'
# Sellsys 销售管理系统 - 客户端

## 系统要求
- Windows 10 或更高版本
- .NET 8.0 运行时（已包含在程序中，无需单独安装）

## 安装说明
1. 解压所有文件到任意目录
2. 双击 Sellsys.WpfClient.exe 启动程序
3. 首次启动时需要配置服务器地址

## 服务器配置
- 开发环境: http://localhost:5078/api
- 生产环境: http://8.156.69.42:5000/api

## 主要功能
- 客户管理
- 产品管理
- 订单管理
- 销售跟进
- 售后服务
- 财务管理
- 系统设置

## 技术支持
如有问题请联系技术支持团队
EOF
}

# 创建修复版客户端说明文件
create_fixed_client_readme() {
    local dir=$1
    cat > "$dir/使用说明.txt" << 'EOF'
# Sellsys 销售管理系统 - 客户端 (修复版)

## 修复内容
- 已修复服务器连接地址
- 现在可以正常连接到云端服务器

## 系统要求
- Windows 10 或更高版本
- 无需安装 .NET 运行时（已包含）

## 使用方法
1. 双击 "Sellsys.WpfClient.exe" 启动程序
2. 程序会自动连接到云端服务器
3. 开始使用系统功能

## 服务器信息
- 服务器地址: http://8.156.69.42:5000
- 状态: 已部署并运行中

## 主要功能
- 客户管理
- 产品管理
- 订单管理
- 销售跟进
- 售后服务
- 财务管理
- 系统设置

## 如有问题
请联系技术支持团队
EOF
}

# 创建启动脚本
create_startup_script() {
    local dir=$1
    cat > "$dir/启动程序.bat" << 'EOF'
@echo off
echo.
echo ========================================
echo   Sellsys 销售管理系统
echo   正在启动客户端...
echo ========================================
echo.
start Sellsys.WpfClient.exe
EOF
}

# 创建部署包
create_deployment_package() {
    print_info "创建完整部署包..."
    
    local deploy_dir="sellsys-deployment-$(date +%Y%m%d_%H%M%S)"
    
    # 检查必要的目录
    for dir in "publish/webapi" "publish/wpfclient" "deploy"; do
        if [ ! -d "$dir" ]; then
            print_error "目录不存在: $dir"
            print_warning "请先运行构建: $0 -b deployment"
            return 1
        fi
    done
    
    # 创建部署包目录
    mkdir -p "$deploy_dir"
    
    # 复制文件
    print_info "复制 WebApi 文件..."
    cp -r publish/webapi "$deploy_dir"/
    
    print_info "复制 WPF 客户端文件..."
    cp -r publish/wpfclient "$deploy_dir"/
    
    print_info "复制部署脚本..."
    cp -r deploy "$deploy_dir"/
    
    # 设置脚本执行权限
    chmod +x "$deploy_dir"/deploy/*.sh
    
    # 创建压缩包
    create_archive "$deploy_dir"
    
    print_success "部署包创建完成: $deploy_dir"
    
    # 显示使用说明
    echo ""
    print_info "部署说明:"
    echo "1. 将 ${deploy_dir}.tar.gz 上传到云端服务器"
    echo "2. 在服务器上解压: tar -xzf ${deploy_dir}.tar.gz"
    echo "3. 运行环境配置: bash ${deploy_dir}/deploy/setup-environment.sh"
    echo "4. 运行部署脚本: bash ${deploy_dir}/deploy/deploy.sh"
}

# 创建压缩包
create_archive() {
    local dir=$1
    print_info "创建压缩包..."
    
    if command -v powershell &> /dev/null && [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
        # Windows 环境使用 PowerShell
        powershell -Command "Compress-Archive -Path '$dir' -DestinationPath '${dir}.zip' -Force"
        print_success "已创建: ${dir}.zip"
        ls -lh "${dir}.zip" 2>/dev/null || echo "文件夹: $dir/"
    else
        # Linux/Mac 环境使用 tar
        tar -czf "${dir}.tar.gz" "$dir"
        print_success "已创建: ${dir}.tar.gz"
        ls -lh "${dir}.tar.gz"
    fi
}

# 主函数
main() {
    local package_type=""
    local clean_flag=false
    local build_flag=false
    local verbose_flag=false
    
    # 解析命令行参数
    while [[ $# -gt 0 ]]; do
        case $1 in
            -h|--help)
                show_help
                exit 0
                ;;
            -c|--clean)
                clean_flag=true
                shift
                ;;
            -b|--build)
                build_flag=true
                shift
                ;;
            -v|--verbose)
                verbose_flag=true
                shift
                ;;
            client|client-fixed|deployment|all)
                package_type=$1
                shift
                ;;
            *)
                print_error "未知参数: $1"
                show_help
                exit 1
                ;;
        esac
    done
    
    # 检查包类型
    if [ -z "$package_type" ]; then
        print_error "请指定包类型"
        show_help
        exit 1
    fi
    
    # 设置详细输出
    if [ "$verbose_flag" = true ]; then
        set -x
    fi
    
    print_info "开始构建 Sellsys 包..."
    print_info "包类型: $package_type"
    
    # 检查环境
    check_prerequisites
    
    # 清理
    if [ "$clean_flag" = true ]; then
        clean_publish
    fi
    
    # 构建
    if [ "$build_flag" = true ]; then
        build_project
    fi
    
    # 创建包
    case $package_type in
        client)
            create_client_package "client"
            ;;
        client-fixed)
            create_client_package "client-fixed"
            ;;
        deployment)
            create_deployment_package
            ;;
        all)
            create_client_package "client"
            create_client_package "client-fixed"
            create_deployment_package
            ;;
        *)
            print_error "未知的包类型: $package_type"
            exit 1
            ;;
    esac
    
    print_success "所有操作完成！"
}

# 运行主函数
main "$@"
