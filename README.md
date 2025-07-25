# Sellsys 销售管理系统

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-lightblue.svg)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![SQLite](https://img.shields.io/badge/Database-SQLite-green.svg)](https://www.sqlite.org/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

一个功能完整的企业级销售管理系统，采用现代化的 .NET 8 技术栈，提供 Web API 后端和 WPF 桌面客户端。

## 🚀 功能特性

### 核心业务模块
- **👥 客户管理** - 客户信息维护、联系人管理、客户分类
- **📦 产品管理** - 产品信息、库存管理、价格体系
- **📋 订单管理** - 订单创建、状态跟踪、订单明细
- **📈 销售跟进** - 销售活动记录、客户沟通历史
- **🔧 售后服务** - 服务记录、问题跟踪、客户反馈
- **💰 财务管理** - 销售统计、财务报表、收支分析
- **⚙️ 系统设置** - 用户管理、权限控制、系统配置

### 技术特性
- **🏗️ 清洁架构** - 采用 DDD 分层架构设计
- **🔐 权限管理** - 基于角色的访问控制 (RBAC)
- **🌐 RESTful API** - 标准化的 Web API 接口
- **📱 现代化 UI** - 响应式 WPF 界面设计
- **🔄 自动部署** - 一键部署和迁移工具
- **📊 实时监控** - 健康检查和日志记录

## 🏗️ 系统架构

```
┌─────────────────┐    ┌─────────────────┐
│   WPF Client    │    │   Web Client    │
│   (Desktop)     │    │   (Future)      │
└─────────┬───────┘    └─────────┬───────┘
          │                      │
          └──────────┬───────────┘
                     │ HTTP/REST
          ┌──────────▼───────────┐
          │     Web API          │
          │   (.NET 8)           │
          └──────────┬───────────┘
                     │
          ┌──────────▼───────────┐
          │   Business Logic     │
          │   (Application)      │
          └──────────┬───────────┘
                     │
          ┌──────────▼───────────┐
          │   Data Access        │
          │   (Infrastructure)   │
          └──────────┬───────────┘
                     │
          ┌──────────▼───────────┐
          │     SQLite DB        │
          └──────────────────────┘
```

## 🛠️ 技术栈

### 后端 (Web API)
- **.NET 8** - 现代化的 .NET 平台
- **ASP.NET Core** - Web API 框架
- **Entity Framework Core** - ORM 数据访问
- **SQLite** - 轻量级数据库
- **Swagger/OpenAPI** - API 文档
- **BCrypt** - 密码加密

### 前端 (WPF Client)
- **WPF (.NET 8)** - Windows 桌面应用框架
- **MVVM 模式** - 数据绑定和视图分离
- **HttpClient** - HTTP 通信
- **JSON 序列化** - 数据交换

### 部署和运维
- **Linux 部署** - 支持 Ubuntu/CentOS
- **Systemd 服务** - 系统服务管理
- **Nginx 反向代理** - Web 服务器
- **自动化脚本** - 一键部署和迁移

## 📁 项目结构

```
Sellsys/
├── src/                              # 源代码
│   ├── Sellsys.WebApi/              # Web API 项目
│   ├── Sellsys.Application/         # 应用服务层
│   ├── Sellsys.Domain/              # 领域模型层
│   ├── Sellsys.Infrastructure/      # 基础设施层
│   ├── Sellsys.WpfClient/          # WPF 客户端
│   └── Sellsys.CrossCutting/       # 横切关注点
├── deploy-package/                   # 部署包
│   ├── webapi-selfcontained/        # 自包含 API 程序
│   ├── deploy.sh                    # 部署脚本
│   └── setup-environment.sh        # 环境配置脚本
├── publish/                         # 编译输出
├── docs/                           # 文档
│   ├── 服务器迁移教程.md            # 迁移指南
│   ├── 开发环境配置指南.md          # 开发指南
│   └── 部署和迁移指南总结.md        # 部署总结
├── tools/                          # 工具脚本
│   ├── quick-migration.sh          # 快速迁移脚本
│   ├── update-server-address.sh    # 地址更新工具
│   └── update-server-windows.bat   # Windows 更新工具
└── README.md                       # 项目说明
```

## 🚀 快速开始

### 环境要求

#### 开发环境
- **.NET 8 SDK** - [下载地址](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** 或 **VS Code**
- **Git** - 版本控制

#### 生产环境
- **Linux 服务器** (Ubuntu 20.04+ / CentOS 8+)
- **2GB+ RAM** 和 **20GB+ 存储空间**
- **开放端口**: 80, 443, 5000

### 本地开发

1. **克隆项目**
```bash
git clone [项目地址]
cd sellsys
```

2. **还原依赖**
```bash
dotnet restore
```

3. **启动后端 API**
```bash
cd src/Sellsys.WebApi
dotnet run
```
API 将在 `http://localhost:5078` 启动

4. **启动 WPF 客户端**
- 在 Visual Studio 中打开 `Sellsys.sln`
- 设置 `Sellsys.WpfClient` 为启动项目
- 按 F5 运行

5. **默认登录账号**
- 用户名: `admin`
- 密码: `admin123`

### 生产部署

#### 一键部署 (推荐)
```bash
# 完整部署到新服务器
./quick-migration.sh deploy 服务器IP

# 示例
./quick-migration.sh deploy 192.168.1.100
```

#### 手动部署
```bash
# 1. 上传部署包
scp -r deploy-package/* root@服务器IP:~/sellsys-deploy/

# 2. 在服务器上执行
ssh root@服务器IP
cd ~/sellsys-deploy
bash setup-environment.sh
bash deploy.sh
```

## 🔧 配置说明

### API 配置
**文件**: `src/Sellsys.WebApi/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=sellsys.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### 客户端配置
**文件**: `src/Sellsys.WpfClient/Services/ApiService.cs`
```csharp
// 开发环境
private const string DevelopmentUrl = "http://localhost:5078/api";

// 生产环境 (需要修改为实际服务器地址)
private const string ProductionUrl = "http://服务器IP:5000/api";
```

## 🔄 服务器迁移

### 快速迁移
```bash
# 完整迁移流程
./quick-migration.sh full-migration 旧服务器IP 新服务器IP

# 仅更新客户端地址
./update-server-address.sh 新服务器IP
```

### 分步迁移
```bash
# 1. 备份数据
./quick-migration.sh backup 旧服务器IP

# 2. 部署到新服务器
./quick-migration.sh deploy 新服务器IP

# 3. 更新客户端
./quick-migration.sh update-client 新服务器IP
```

详细迁移指南请参考: [服务器迁移教程.md](服务器迁移教程.md)

## 📊 API 文档

### 访问地址
- **Swagger UI**: `http://服务器IP:5000/swagger`
- **健康检查**: `http://服务器IP:5000/health`

### 主要接口
- `POST /api/auth/login` - 用户登录
- `GET /api/customers` - 获取客户列表
- `GET /api/products` - 获取产品列表
- `GET /api/orders` - 获取订单列表
- `GET /api/employees` - 获取员工列表

## 🔍 故障排除

### 常见问题

#### 1. 服务无法启动
```bash
# 检查服务状态
sudo systemctl status sellsys-webapi

# 查看日志
sudo journalctl -u sellsys-webapi -f

# 检查端口占用
sudo netstat -tlnp | grep :5000
```

#### 2. 客户端连接失败
- 检查服务器 IP 地址配置
- 确认防火墙开放 5000 端口
- 测试网络连通性: `ping 服务器IP`
- 测试 API: `curl http://服务器IP:5000/health`

#### 3. 数据库问题
```bash
# 检查数据库文件
ls -la /opt/sellsys/sellsys.db

# 备份数据库
sudo cp /opt/sellsys/sellsys.db /opt/sellsys/backup/
```

## 🤝 贡献指南

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 📞 技术支持

- **文档**: 查看 `docs/` 目录下的详细文档
- **问题反馈**: 通过 GitHub Issues 提交
- **功能建议**: 欢迎提交 Feature Request

## 🎯 路线图

- [ ] Web 客户端开发
- [ ] 移动端 APP
- [ ] 高级报表功能
- [ ] 数据导入导出
- [ ] 多语言支持
- [ ] 微服务架构升级

---

**Sellsys** - 让销售管理更简单高效 🚀
