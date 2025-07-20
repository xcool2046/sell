# Sellsys 部署指南

## 项目概述

Sellsys 是一个销售管理系统，包含以下组件：
- **WebApi**: 后端 API 服务 (ASP.NET Core 8.0)
- **WpfClient**: Windows 桌面客户端 (WPF)
- **数据库**: SQLite

## 部署架构

```
客户端 (WPF) --> 云端服务器 (WebApi + Nginx) --> SQLite 数据库
```

## 云端服务器信息

- **IP**: 8.156.69.42
- **用户**: root
- **密码**: xx20250715xx@
- **操作系统**: Linux (Ubuntu/CentOS)

## 部署步骤

### 1. 环境准备

首先连接到云端服务器：
```bash
ssh root@8.156.69.42
```

运行环境配置脚本：
```bash
bash setup-environment.sh
```

### 2. 上传文件

将以下文件上传到服务器的 `~/sellsys-deploy/` 目录：
- `webapi/` - WebApi 发布文件
- `deploy.sh` - 部署脚本
- `setup-environment.sh` - 环境配置脚本

### 3. 部署应用

运行部署脚本：
```bash
cd ~/sellsys-deploy
bash deploy.sh
```

### 4. 验证部署

检查服务状态：
```bash
sudo systemctl status sellsys-webapi
```

访问 API：
```bash
curl http://localhost:5000/health
```

## 服务管理

### 常用命令

```bash
# 查看服务状态
sudo systemctl status sellsys-webapi

# 查看实时日志
sudo journalctl -u sellsys-webapi -f

# 重启服务
sudo systemctl restart sellsys-webapi

# 停止服务
sudo systemctl stop sellsys-webapi

# 启动服务
sudo systemctl start sellsys-webapi
```

### 配置文件位置

- 应用目录: `/opt/sellsys/`
- 服务配置: `/etc/systemd/system/sellsys-webapi.service`
- Nginx 配置: `/etc/nginx/sites-available/sellsys`
- 数据库文件: `/opt/sellsys/sellsys.db`

## 访问地址

- **API 直接访问**: http://8.156.69.42:5000
- **通过 Nginx**: http://8.156.69.42
- **Swagger 文档**: http://8.156.69.42:5000/swagger
- **健康检查**: http://8.156.69.42:5000/health

## 客户端配置

WPF 客户端需要配置 API 地址为：
```
http://8.156.69.42:5000
```

## 故障排除

### 1. 服务无法启动

检查日志：
```bash
sudo journalctl -u sellsys-webapi --no-pager -n 50
```

### 2. 端口被占用

检查端口使用情况：
```bash
sudo netstat -tlnp | grep :5000
```

### 3. 权限问题

确保文件权限正确：
```bash
sudo chown -R sellsys:sellsys /opt/sellsys
sudo chmod +x /opt/sellsys/Sellsys.WebApi
```

### 4. 防火墙问题

检查防火墙状态：
```bash
sudo ufw status
```

## 备份和恢复

### 备份数据库

```bash
sudo cp /opt/sellsys/sellsys.db /opt/sellsys/backup/sellsys_$(date +%Y%m%d_%H%M%S).db
```

### 恢复数据库

```bash
sudo systemctl stop sellsys-webapi
sudo cp /opt/sellsys/backup/sellsys_backup.db /opt/sellsys/sellsys.db
sudo chown sellsys:sellsys /opt/sellsys/sellsys.db
sudo systemctl start sellsys-webapi
```

## 性能监控

### 查看系统资源

```bash
# CPU 和内存使用情况
htop

# 磁盘使用情况
df -h

# 应用进程信息
ps aux | grep Sellsys
```

### 查看应用日志

```bash
# 查看最近的错误日志
sudo journalctl -u sellsys-webapi --since "1 hour ago" | grep -i error

# 查看访问日志
sudo tail -f /var/log/nginx/access.log
```
