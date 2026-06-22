# sun-admin 技术方案

## 1. 需求分析

### 1.1 项目定位判断

sun-admin 的核心价值不是“功能大而全”，而是提供一套可复用、可二次开发、可部署的轻量级 .NET 后台管理系统基础模板。当前需求范围适合作为开源项目的首版，但需要严格控制 MVP 边界，避免一开始引入多租户、工作流、报表、插件、代码生成等复杂能力。

首版应优先证明三件事：

- 能快速启动：开发者可以通过 Docker Compose 和 README 在 10 分钟内跑起来。
- 权限模型可用：菜单可见性、按钮权限、API 访问控制形成闭环。
- 工程结构清晰：后续业务模块能按既定分层方式扩展，而不是堆在 Controller 或 ORM 查询里。

### 1.2 MVP 范围收敛

MVP 必须包含：

- 登录认证、当前用户信息、修改密码基础能力。
- 用户、角色、菜单三类核心资源管理。
- 用户-角色、角色-菜单/权限授权关系。
- 后端 API 权限校验，不能只依赖前端隐藏菜单或按钮。
- 初始超级管理员、内置基础菜单和权限种子数据。
- Swagger 或 Scalar 文档、Docker Compose、本地开发文档。
- 登录、权限、用户管理核心链路集成测试。

MVP 建议延后：

- 操作日志和登录日志查询页面：可以先实现登录日志写入，查询页放到 P1。
- 系统配置、仪表盘：对权限闭环不是关键，放到 P2。
- Refresh Token、Token 黑名单：首版只做短期 Access Token 和前端退出。
- 多租户、部门、数据字典、文件上传、国际化、插件机制。

### 1.3 当前需求需要补强的点

- 权限数据模型需要明确：`Menu` 同时承载目录、菜单、按钮权限，但 API 权限与按钮权限是否完全复用同一个 `PermissionCode` 需要定清楚。
- 超级管理员绕过规则需要明确：建议基于内置角色编码 `super_admin` 放行所有权限。
- 菜单删除规则需要明确：有子节点不允许直接删除；内置菜单不允许删除。
- 用户删除策略需要明确：后台系统建议默认软删除，避免审计和授权关系丢失。
- 密码策略需要明确：最小长度、复杂度、重置密码默认行为、首次登录改密是否强制。
- 数据库约束需要明确：用户名、邮箱、角色编码、权限编码、配置 Key 等唯一索引。
- 初始化策略需要明确：CodeFirst 建表 + Seed 初始管理员、角色、菜单、权限。
- API 响应和错误码需要统一：否则前后端联调成本会很高。

## 2. 总体架构

### 2.1 技术选型

后端：

- .NET 8 + ASP.NET Core Web API
- C# 12，开启 nullable reference types
- FreeSql + MySQL
- JWT Bearer Authentication
- 自定义 Permission AuthorizationPolicy
- FluentValidation
- Serilog
- Swagger 或 Scalar
- xUnit + WebApplicationFactory + Testcontainers MySQL

前端：

- React + TypeScript + Vite
- Ant Design
- React Router
- TanStack Query 或 ahooks 请求状态管理
- Axios 请求封装

部署：

- Dockerfile
- docker-compose.yml
- MySQL 容器
- `.env.example`

### 2.2 后端分层

推荐结构：

```text
src/
  SunAdmin.Api/
  SunAdmin.Application/
  SunAdmin.Contracts/
  SunAdmin.Domain/
  SunAdmin.Infrastructure/
tests/
  SunAdmin.UnitTests/
  SunAdmin.IntegrationTests/
web/
  sun-admin-web/
docs/
```

职责边界：

- `SunAdmin.Api`：Controller、认证授权配置、中间件、OpenAPI、HTTP 适配。
- `SunAdmin.Application`：用例服务、业务编排、校验器、权限判断接口。
- `SunAdmin.Contracts`：请求 DTO、响应 DTO、分页模型、统一响应模型。
- `SunAdmin.Domain`：实体、枚举、领域常量、领域规则。
- `SunAdmin.Infrastructure`：FreeSql 配置、仓储实现、数据库初始化、JWT、密码哈希、日志实现。

API 层不能直接访问 FreeSql；数据库实体不能直接作为接口响应返回。

## 3. 核心设计

### 3.1 统一响应与错误处理

成功响应：

```json
{
  "code": "OK",
  "message": "success",
  "data": {}
}
```

分页响应：

```json
{
  "items": [],
  "total": 0,
  "pageIndex": 1,
  "pageSize": 20
}
```

错误响应：

```json
{
  "code": "VALIDATION_ERROR",
  "message": "参数校验失败",
  "errors": {
    "userName": ["用户名不能为空"]
  }
}
```

建议错误码：

- `VALIDATION_ERROR`
- `UNAUTHORIZED`
- `FORBIDDEN`
- `NOT_FOUND`
- `CONFLICT`
- `BUSINESS_ERROR`
- `INTERNAL_ERROR`

### 3.2 认证设计

登录输入支持用户名或邮箱：

- 查找账号。
- 校验用户状态。
- 使用密码哈希服务验证密码。
- 生成 JWT。
- 更新 `LastLoginAt`。
- 可选写入登录日志。

JWT Claims 建议：

- `sub`：用户 ID
- `name`：用户名
- `display_name`：昵称
- `roles`：角色编码列表

权限不建议全部放进 JWT。权限应在服务端按用户 ID 查询，后续可以增加本地内存缓存或 Redis。这样角色权限变更后不依赖 Token 刷新即可生效。

### 3.3 RBAC 权限设计

采用四张核心表：

- `User`
- `Role`
- `Menu`
- `UserRole`
- `RoleMenu`

`Menu` 作为统一权限资源表，字段 `Type` 区分：

- `Directory`：目录，只影响菜单结构。
- `Page`：页面菜单，可带路由和组件路径。
- `Button`：按钮或操作权限，必须有 `PermissionCode`。

API 权限建议复用 `PermissionCode`，例如：

- 创建用户接口要求 `user:create`
- 编辑用户接口要求 `user:update`
- 删除角色接口要求 `role:delete`

授权流程：

1. API 通过特性声明所需权限，例如 `[RequirePermission("user:create")]`。
2. 授权处理器读取当前用户 ID。
3. 如果用户拥有 `super_admin` 角色，直接放行。
4. 查询用户角色关联的启用菜单权限。
5. 判断是否包含接口要求的权限编码。
6. 不满足则返回 403。

菜单生成：

- `/api/auth/me` 返回用户基础信息、角色编码、权限编码和菜单树。
- 菜单树只包含 `Directory` 和 `Page`。
- 按钮权限只返回权限编码，由前端判断按钮是否展示。

### 3.4 数据库设计补充

通用字段：

- `Id`：建议使用 long 雪花 ID 或 MySQL `bigint auto_increment`。首版可用 `bigint auto_increment`，简单可靠。
- `CreatedAt`
- `UpdatedAt`
- `DeletedAt`：需要软删除的表使用。
- `CreatedBy`
- `UpdatedBy`：MVP 可选，审计增强时补齐。

建议唯一索引：

- `User.UserName`
- `User.Email`
- `Role.Code`
- `Menu.PermissionCode`，允许为空，但非空时唯一。
- `SystemConfig.Key`

建议普通索引：

- `User.Status`
- `User.CreatedAt`
- `Role.Status`
- `Menu.ParentId`
- `Menu.SortOrder`
- `LoginLog.CreatedAt`
- `OperationLog.CreatedAt`

删除策略：

- 用户、角色、菜单建议软删除。
- 内置超级管理员、超级管理员角色、系统基础菜单不允许删除。
- 菜单存在子节点时不允许删除。
- 角色已分配给用户时，删除前需要解除关系或直接禁止删除；MVP 建议禁止删除。

### 3.5 初始化策略

应用启动时执行数据库初始化：

1. 使用 FreeSql CodeFirst 同步表结构。
2. 初始化超级管理员角色 `super_admin`。
3. 初始化管理员用户。
4. 初始化系统菜单与权限。
5. 绑定超级管理员用户和角色。
6. 绑定超级管理员角色和全部菜单权限。

生产环境建议提供开关：

- `Database:SyncStructure=false`
- `Database:SeedData=true`

本地开发默认开启结构同步，生产环境默认关闭结构同步，避免误改表结构。

## 4. API 方案

### 4.1 Auth

- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `POST /api/auth/change-password`

### 4.2 Users

- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`
- `DELETE /api/users/{id}`
- `POST /api/users/{id}/enable`
- `POST /api/users/{id}/disable`
- `POST /api/users/{id}/reset-password`
- `PUT /api/users/{id}/roles`

### 4.3 Roles

- `GET /api/roles`
- `GET /api/roles/{id}`
- `POST /api/roles`
- `PUT /api/roles/{id}`
- `DELETE /api/roles/{id}`
- `PUT /api/roles/{id}/menus`

### 4.4 Menus

- `GET /api/menus/tree`
- `GET /api/menus/{id}`
- `POST /api/menus`
- `PUT /api/menus/{id}`
- `DELETE /api/menus/{id}`

接口权限映射应在代码中集中声明，文档中同步列出，避免“接口存在但权限码缺失”。

## 5. 前端方案

### 5.1 项目结构

```text
web/sun-admin-web/
  src/
    app/
    pages/
    features/
      auth/
      users/
      roles/
      menus/
    components/
    layouts/
    routes/
    services/
    stores/
    types/
```

### 5.2 登录与路由

- 登录成功后保存 access token。
- 请求 `/api/auth/me` 获取用户、权限、菜单。
- 根据菜单树生成侧边栏。
- 路由可以采用静态路由表 + 后端菜单过滤的方式，避免完全由后端返回组件路径造成安全和维护问题。
- 按钮通过 `hasPermission("user:create")` 判断展示。

### 5.3 页面优先级

MVP 页面：

- 登录页
- 主布局
- 用户管理
- 角色管理
- 菜单管理

P1 页面：

- 登录日志
- 操作日志

P2 页面：

- 系统配置
- 首页仪表盘

## 6. 测试方案

单元测试：

- 密码哈希与校验。
- 权限树构建。
- 菜单树构建。
- 业务规则，例如内置角色不可删除、菜单有子节点不可删除。

集成测试：

- 登录成功和失败。
- 禁用用户无法登录。
- 无 Token 返回 401。
- 无权限访问受保护接口返回 403。
- 超级管理员可访问受保护接口。
- 用户、角色、菜单核心 CRUD。

集成测试建议使用 Testcontainers 启动 MySQL，避免 SQLite 与 MySQL 行为差异。

## 7. 部署与配置

环境变量建议：

- `ASPNETCORE_ENVIRONMENT`
- `ConnectionStrings__Default`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Secret`
- `Jwt__AccessTokenMinutes`
- `Seed__AdminUserName`
- `Seed__AdminEmail`
- `Seed__AdminPassword`

Docker Compose 包含：

- `sun-admin-api`
- `sun-admin-web`
- `mysql`

健康检查：

- `GET /health`
- `GET /health/ready`

## 8. 里程碑建议

### Milestone 1：工程骨架

- 创建解决方案和项目结构。
- 接入 FreeSql、MySQL、Swagger、Serilog。
- 实现统一响应、异常处理、FluentValidation。
- 提供 Docker Compose 和 `.env.example`。

### Milestone 2：认证与初始化

- 实现用户、角色、菜单实体。
- 实现 CodeFirst 和种子数据。
- 实现登录、JWT、当前用户信息。
- 实现基础权限授权处理器。

### Milestone 3：核心管理 API

- 用户管理 API。
- 角色管理 API。
- 菜单管理 API。
- 用户分配角色。
- 角色分配菜单权限。

### Milestone 4：前端 MVP

- 登录页。
- 主布局、侧边栏、动态菜单。
- 用户、角色、菜单管理页。
- 权限按钮控制。

### Milestone 5：测试与发布

- 补齐集成测试。
- 补齐 README、开发文档、部署文档。
- 配置 CI。
- 发布 `v0.1.0`。

## 9. 首版验收口径

- `docker compose up` 后可以启动 API、Web 和 MySQL。
- 初始管理员可以登录。
- 管理员可以维护用户、角色、菜单。
- 管理员可以给用户分配角色、给角色分配权限。
- 普通用户只能看到授权菜单。
- 无权限访问受保护 API 返回 403。
- Swagger 或 Scalar 能查看核心 API。
- CI 可以运行后端测试。

## 10. 关键技术决策

- 使用 Web API Controller，而不是 Minimal API：后台 CRUD 接口较多，Controller 更利于分组、特性授权和文档维护。
- 使用 FreeSql CodeFirst 做本地开发初始化：符合需求中不使用 EF Core 的约束，也能降低启动门槛。
- 权限不放入 JWT：保证权限变更能更快生效，避免 Token 过期前权限不一致。
- 角色权限统一挂在 `Menu` 表：降低 MVP 数据模型复杂度，目录、页面、按钮/API 权限可以通过类型区分。
- 首版不做 Refresh Token：降低认证复杂度，后续再补完整会话管理。
