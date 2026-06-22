# sun-admin

`sun-admin` 是一个基于 .NET 8、React、MySQL 的轻量级后台管理系统，适合作为中小型管理后台的基础模板。项目已经包含登录认证、RBAC 权限、用户/角色/菜单、部门/岗位、日志审计、在线会话和系统配置等常用能力。

## 功能清单

- 登录、退出、当前用户信息、修改密码、重置密码后强制改密
- 用户管理：分页、搜索、状态/角色/部门/岗位筛选、部门岗位绑定、角色分配、批量启用/禁用/删除
- 角色管理：角色编码、状态、菜单授权、数据范围、已分配用户数
- 菜单管理：目录、页面、按钮权限码管理；前端仅限制菜单显示，接口权限由后端控制
- 部门管理：树形组织、负责人、联系方式、启停状态
- 岗位管理：岗位编码、说明、排序、启停状态
- 日志审计：操作日志、登录日志
- 在线会话：查看有效会话、强制下线
- 系统配置：密码策略、系统名称等基础配置
- 仪表盘：用户、角色、组织、菜单、今日操作和失败登录概览

## 技术栈

后端：

- .NET 8 + ASP.NET Core Web API
- FreeSql + MySQL
- JWT Bearer Authentication
- 自定义权限策略和 `RequirePermission`
- FluentValidation
- Serilog
- xUnit + WebApplicationFactory

前端：

- React + TypeScript + Vite
- Ant Design
- React Router
- Axios

部署：

- Docker
- Docker Compose
- Nginx
- MySQL

## 项目结构

```text
.
├── src/
│   ├── SunAdmin.Api/             控制器、认证、权限、异常处理、日志过滤器
│   ├── SunAdmin.Application/     应用接口、菜单树构建、通用异常
│   ├── SunAdmin.Contracts/       请求和响应 DTO
│   ├── SunAdmin.Domain/          实体、枚举、权限常量
│   └── SunAdmin.Infrastructure/  FreeSql、服务实现、种子数据、JWT
├── tests/
│   ├── SunAdmin.UnitTests/
│   └── SunAdmin.IntegrationTests/
├── web/sun-admin-web/            React 管理端
├── docs/                         需求、开发、部署和接口文档
├── docker-compose.yml
└── .env.example
```

## 本地启动

1. 复制环境变量：

```bash
cp .env.example .env
```

2. 修改 `.env` 中的敏感配置，至少需要替换：

- `Jwt__Secret`
- `Seed__AdminPassword`
- MySQL 相关密码

3. 启动完整环境：

```bash
docker compose up --build
```

默认访问地址：

- 前端：`http://localhost:5173`
- API：`http://localhost:5000`
- 健康检查：`http://localhost:5000/health`
- MySQL：`localhost:3306`

前端容器使用 Nginx 托管 Vite 构建产物，并将 `/api` 代理到后端 API 容器。

## 默认账号

初始化管理员由 `.env` 控制，示例值如下：

- 用户名：`admin`
- 邮箱：`admin@sun-admin.local`
- 密码：`ChangeMe_123456`

首次部署到共享或生产环境前必须修改默认密码。

## 权限说明

系统使用 RBAC：

- `super_admin`：内置超级管理员，后端接口拥有全部权限
- `readonly_admin`：示例只读角色，可查看基础数据
- `user_admin`：示例用户管理员，可维护用户、部门、岗位

前端只根据后端返回的菜单控制左侧菜单显示；页面路由不做强限制。真正的安全边界在后端接口权限校验，未授权接口会返回 403。

常用权限码包括：

- 用户：`user:view`、`user:create`、`user:update`、`user:delete`
- 角色：`role:view`、`role:create`、`role:update`、`role:delete`
- 菜单：`menu:view`、`menu:create`、`menu:update`、`menu:delete`
- 部门：`department:view`、`department:create`、`department:update`、`department:delete`
- 岗位：`position:view`、`position:create`、`position:update`、`position:delete`
- 日志：`operation-log:view`、`login-log:view`
- 会话：`session:view`、`session:revoke`
- 配置：`setting:view`、`setting:update`

## 常用命令

后端构建和测试：

```bash
dotnet build SunAdmin.slnx
dotnet test SunAdmin.slnx
```

前端构建：

```bash
cd web/sun-admin-web
npm install
npm run build
```

Docker 部署：

```bash
docker compose up --build -d
docker compose ps
docker compose logs -f api
```

## 文档

- [技术方案](docs/technical-solution.md)
- [开发说明](docs/development.md)
- [部署说明](docs/deployment.md)
- [接口草案](docs/api.md)
