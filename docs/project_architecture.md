# 销售信息管理系统 - 项目总体架构文档 (V1.0)

## 1. 概述

本文档旨在从宏观层面定义本系统的技术架构、项目结构和开发工作流，作为所有开发工作的顶层指导，确保团队对系统有统一的认知，并遵循一致的规范进行协作。

## 2. 系统架构

本系统采用经典的 **C/S (客户端/服务器) 架构**，并通过RESTful API进行前后端分离。

```mermaid
graph TD
    subgraph 用户端
        A[WPF 桌面客户端]
    end

    subgraph 网络
        B{HTTP/S (JSON)}
    end

    subgraph 服务器端
        C[ASP.NET Core Web API]
        D[MySQL 数据库]
    end

    A -- RESTful API 请求/响应 --> B
    B -- 内部调用 --> C
    C -- 数据库读写 --> D
```

- **客户端 (Client)**: 基于 **WPF (C#/.NET)** 的Windows桌面应用程序。负责所有UI展示、用户交互和业务逻辑的最终呈现。它是一个“瘦客户端”，自身不处理复杂的业务规则，而是通过调用后端API来完成。
- **服务器 (Server)**: 基于 **ASP.NET Core (C#/.NET)** 的Web API服务。负责处理所有核心业务逻辑、数据验证、持久化和权限控制。
- **数据库 (Database)**: 采用 **MySQL** 关系型数据库，负责所有业务数据的持久化存储。

## 3. 后端项目结构 (建议)

为了保证代码的清晰、解耦和可维护性，后端项目将遵循“洋葱架构”(Onion Architecture)思想，进行分层设计。

```
/Sellsys.Api
|-- /Controllers         // API控制器, 仅负责HTTP请求和响应
|-- /Services            // 业务逻辑核心层
|-- /DataAccess          // 数据访问层 (使用EF Core)
|   |-- /Models          // 数据库实体模型
|   |-- /Migrations      // 数据库迁移脚本
|-- /DTOs                // 数据传输对象 (用于API请求/响应)
|-- /Middleware          // 中间件 (如全局异常处理)
|-- appsettings.json     // 配置文件
|-- Program.cs           // 程序入口
```

## 4. 前端项目结构 (建议)

前端WPF项目将采用 **MVVM (Model-View-ViewModel)** 设计模式，以实现UI与逻辑的彻底分离。

```
/Sellsys.Client
|-- /Views               // 视图 (XAML文件, .xaml)
|   |-- /Windows         // 主窗口、登录窗口
|   |-- /Pages           // 各模块主页面
|   |-- /Popups          // 各类弹窗
|   |-- /UserControls    // 可复用的UI组件
|-- /ViewModels          // 视图模型 (C#文件, .cs)
|-- /Models              // 数据模型 (从API获取的数据)
|-- /Services            // 服务层 (如ApiClient)
|-- /Resources           // 资源文件 (如样式、图标)
|-- App.xaml             // 应用入口
|-- MainWindow.xaml      // 主窗口
```

## 5. 开发工作流

1.  **需求分析**: (已完成) 基于五大核心设计文档。
2.  **数据库优先**: 使用`Entity Framework Core`的`Database First`或`Code First`模式，根据`database_schema.md`生成或编写数据实体类和数据库上下文。
3.  **后端开发**: 并行开发各个模块的`Service`和`Controller`。
4.  **前端开发**: 并行开发各个模块的`View`和`ViewModel`。
5.  **联调测试**: 前后端开发人员协同测试API的调用和数据展示。
6.  **版本控制**: 所有代码通过Git管理，遵循`feature -> develop -> main`的分支模型。

本架构文档为项目的启动和实施提供了清晰的路线图。