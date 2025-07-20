#!/bin/bash

# 云端环境配置脚本
# 用于在 Linux 服务器上配置运行环境

set -e

echo "=== Sellsys 环境配置脚本 ==="

# 更新系统包
echo "更新系统包..."
sudo apt update && sudo apt upgrade -y

# 安装必要的依赖
echo "安装系统依赖..."
sudo apt install -y \
    curl \
    wget \
    unzip \
    nginx \
    ufw \
    htop \
    tree \
    sqlite3

# 配置防火墙
echo "配置防火墙..."
sudo ufw allow ssh
sudo ufw allow 80
sudo ufw allow 443
sudo ufw allow 5000
sudo ufw --force enable

# 配置 Nginx 反向代理
echo "配置 Nginx..."
sudo tee /etc/nginx/sites-available/sellsys > /dev/null <<EOF
server {
    listen 80;
    server_name _;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOF

# 启用 Nginx 站点
sudo ln -sf /etc/nginx/sites-available/sellsys /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl restart nginx
sudo systemctl enable nginx

# 创建部署目录
echo "创建部署目录..."
mkdir -p ~/sellsys-deploy

echo "=== 环境配置完成 ==="
echo "系统已准备就绪，可以部署 Sellsys 应用"
echo ""
echo "下一步:"
echo "1. 上传应用文件到 ~/sellsys-deploy/"
echo "2. 运行部署脚本: bash ~/sellsys-deploy/deploy.sh"
echo ""
echo "访问地址:"
echo "  直接访问: http://服务器IP:5000"
echo "  通过 Nginx: http://服务器IP"
