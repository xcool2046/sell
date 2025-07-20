#!/bin/bash

# Sellsys 部署脚本
# 用于在 Linux 服务器上部署 Sellsys WebApi

set -e  # 遇到错误时退出

echo "=== Sellsys WebApi 部署脚本 ==="

# 配置变量
APP_NAME="sellsys-webapi"
APP_DIR="/opt/sellsys"
SERVICE_FILE="/etc/systemd/system/${APP_NAME}.service"
USER="sellsys"
PORT="5000"

# 创建应用目录
echo "创建应用目录..."
sudo mkdir -p $APP_DIR
sudo mkdir -p $APP_DIR/logs

# 创建专用用户（如果不存在）
if ! id "$USER" &>/dev/null; then
    echo "创建用户 $USER..."
    sudo useradd -r -s /bin/false $USER
fi

# 复制应用文件
echo "复制应用文件..."
sudo cp -r ./webapi/* $APP_DIR/
sudo chown -R $USER:$USER $APP_DIR
sudo chmod +x $APP_DIR/Sellsys.WebApi

# 创建 systemd 服务文件
echo "创建 systemd 服务..."
sudo tee $SERVICE_FILE > /dev/null <<EOF
[Unit]
Description=Sellsys WebApi
After=network.target

[Service]
Type=notify
User=$USER
Group=$USER
WorkingDirectory=$APP_DIR
ExecStart=$APP_DIR/Sellsys.WebApi
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=$APP_NAME
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:$PORT

[Install]
WantedBy=multi-user.target
EOF

# 重新加载 systemd 并启动服务
echo "启动服务..."
sudo systemctl daemon-reload
sudo systemctl enable $APP_NAME
sudo systemctl start $APP_NAME

# 检查服务状态
echo "检查服务状态..."
sudo systemctl status $APP_NAME --no-pager

# 显示日志
echo "显示最近的日志..."
sudo journalctl -u $APP_NAME --no-pager -n 20

echo "=== 部署完成 ==="
echo "服务名称: $APP_NAME"
echo "应用目录: $APP_DIR"
echo "访问地址: http://服务器IP:$PORT"
echo ""
echo "常用命令:"
echo "  查看状态: sudo systemctl status $APP_NAME"
echo "  查看日志: sudo journalctl -u $APP_NAME -f"
echo "  重启服务: sudo systemctl restart $APP_NAME"
echo "  停止服务: sudo systemctl stop $APP_NAME"
