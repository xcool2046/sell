# 销售信息管理系统 - API 接口定义文档 (V1.2 - 最终版)

## 1. 概述

- **基地址**: `https://your-api-domain.com/api`
- **认证方式**:
  - **开发阶段**: 为方便开发，暂时不启用认证。所有接口可直接访问。
  - **生产阶段**: 所有请求都需要在 Header 中携带 `Authorization: Bearer <token>`。登录接口除外。
- **数据格式**: `application/json`

---

## 2. 认证接口 (Authentication) - 开发后期实现

### 2.1. 用户登录
- **功能**: 用户通过登录账号和密码获取访问令牌 (token)。
- **路径**: `POST /auth/login`
- **请求体**: `{ "username": "...", "password": "..." }`
- **成功响应**: `{ "token": "...", "user_info": { ... } }`

---

## 3. 系统设置模块 (System Settings)

### 3.1. 员工管理 (Employees)
- `GET /employees`: 获取员工列表 (支持分页和筛选)。
- `GET /employees/{id}`: 获取单个员工详情。
- `POST /employees`: 添加新员工。
- `PUT /employees/{id}`: 更新指定员工信息。
- `DELETE /employees/{id}`: 删除指定员工。

### 3.2. 部门与分组 (Departments & Groups)
- `GET /departments`: 获取所有部门及下属分组的树形结构。
- `POST /departments`: 添加新部门。
- `PUT /departments/{id}`: 更新部门名称。
- `POST /departments/{id}/groups`: 为某部门添加新分组。
- `PUT /groups/{id}`: 更新分组名称。

### 3.3. 角色/岗位管理 (Roles & Permissions)
- `GET /roles`: 获取所有岗位职务列表 (可按部门ID筛选)。
- `GET /roles/{id}`: 获取单个岗位详情，包含其权限信息。
- `POST /roles`: 添加新岗位。
- `PUT /roles/{id}`: 更新岗位信息和关联的权限。
- `DELETE /roles/{id}`: 删除岗位。
- `GET /permissions/modules`: 获取所有可用于分配权限的模块名称列表。

---

## 4. 产品管理模块 (Products)

- `GET /products`: 获取产品列表 (支持分页和搜索)。
- `POST /products`: 添加新产品。
- `PUT /products/{id}`: 更新产品信息。
- `DELETE /products/{id}`: 删除产品。
- `GET /products/for-selection`: 获取用于下拉选择的产品简要列表 (仅含Id, Name, Specification, ListPrice)。

---

## 5. 客户管理模块 (Customers)

### 5.1. 客户 (Customers)
- `GET /customers`: 获取客户列表 (支持分页和按条件筛选)。
- `GET /customers/{id}`: 获取单个客户的完整信息，**包括其所有联系人列表**。
- `POST /customers`: 添加新客户，**请求体中包含联系人列表**。
- `PUT /customers/{id}`: 更新客户信息，**同时可以更新、添加或删除其联系人**。
- `DELETE /customers/{id}`: 删除客户。
- `POST /customers/assign-sales`: 为客户分配销售负责人。
- `POST /customers/assign-support`: 为客户分配客服。

### 5.2. 跟进记录 (Contact Logs)
- `GET /customers/{customerId}/logs`: 获取某个客户的所有跟进记录。
- `POST /customers/{customerId}/logs`: 为某个客户添加新的跟进记录。

---

## 6. 销售管理模块 (Sales Management)
- **说明**: 此模块的API主要复用客户管理和订单管理，仅提供不同的查询视图，**因此不产生新的API**。
- `GET /sales/follow-up-list`: 获取销售跟进列表 (本质是`GET /customers`接口的特定筛选和排序)。
- `GET /customers/{id}/orders`: 获取某个客户的所有订单记录 (作为`GET /orders`接口的特定筛选)。

---

## 7. 订单管理模块 (Orders)

- `GET /orders`: 获取订单列表 (支持分页和复杂筛选)。
  - **成功响应体**:
    ```json
    {
      "items": [ ... ],
      "total_count": 0,
      "summary": {
        "total_actual_price": 0.00,
        "total_quantity": 0,
        "total_order_amount": 0.00
      }
    }
    ```
- `GET /orders/{id}`: 获取单个订单详情。
- `POST /orders`: 创建新订单。
- `PUT /orders/{id}`: 更新订单信息。
- `PUT /orders/{id}/status`: 更新订单状态。
- `DELETE /orders/{id}`: 删除订单 (或标记为作废)。

---

## 8. 售后服务模块 (After-Sales)

### 8.1. 售后主列表 (After-Sales Main List)
- **功能**: 获取售后服务主界面的客户列表，通常是聚合后的视图。
- `GET /after-sales`: 获取售后服务主列表（支持分页和筛选）。
  - **查询参数**: `customerName`, `supportStaffId`, `status`。
  - **响应**: 返回聚合后的客户售后信息列表，每条记录包含最新的更新时间、客服记录总数等。

### 8.2. 客户的售后记录 (Customer-Specific Records)
- **功能**: 管理某个特定客户的所有售后记录。
- `GET /customers/{customerId}/after-sales-records`: 获取指定客户的全部售后记录列表。
- `POST /customers/{customerId}/after-sales-records`: 为指定客户添加一条新的售后记录。
- `PUT /after-sales-records/{recordId}`: 更新某条具体的售后记录。
- `DELETE /after-sales-records/{recordId}`: 删除某条具体的售后记录。

---

## 9. 财务管理模块 (Finance)
- **说明**: 此模块的API主要基于订单数据进行统计和查询，并增加了财务相关的操作。
- `GET /finance/order-list`: 获取财务订单列表。此接口返回的数据结构是`Order`的扩展，会额外包含`sales_commission_amount`, `supervisor_commission_amount`, `manager_commission_amount`, `payment_received_date`等字段。其合计行(`summary`)也会包含提成的总计。
- `POST /finance/orders/{id}/confirm-payment`: 确认收款。用于更新订单的收款状态和到账日期。
- `GET /finance/order-summary`: 获取更详细的财务摘要 (本期不做)。
