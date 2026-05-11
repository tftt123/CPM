# CPM - Customer Project Management System

机加工行业客户与产品管理系统

## 技术栈

- **后端**: .NET 8 Web API + EF Core 8 + SQL Server
- **前端**: Vue 3 + TypeScript + Element Plus + Vite + Pinia
- **认证**: JWT Bearer
- **缓存**: Redis (StackExchange.Redis)
- **后台任务**: Hangfire
- **邮件**: MailKit
- **QAD集成**: Progress OpenEdge AppServer（通过 .NET Framework 4.8 桥接程序）

## 项目结构

```
CPM_SrcCode/
├── CpmServer/              # .NET 8 后端主项目
│   ├── Controllers/        # API 控制器
│   ├── Services/           # 业务服务
│   ├── Models/             # 数据模型
│   ├── DTOs/               # 数据传输对象
│   ├── Data/               # DbContext
│   ├── Migrations/         # EF Core 迁移
│   ├── Common/             # 公共工具
│   └── cpm-web/            # Vue 3 前端
│       ├── src/
│       │   ├── views/      # 页面
│       │   ├── api/        # API 客户端
│       │   ├── stores/     # Pinia 状态
│       │   ├── router/     # 路由
│       │   └── composables/# 组合式函数
└── QadAuthBridge/          # .NET Framework 4.8 桥接程序
    ├── Program.cs          # 入口
    ├── OeAppsvHelper.cs    # Progress AppServer 调用
    └── QadAuthBridge.csproj
```

## 主要功能模块

- 用户认证（本地DB + QAD AppServer 双模式）
- 用户/角色/权限管理（多Site支持）
- 客户管理（CRM）
- 产品管理
- 工艺管理（工序/设备类型/设备）
- 报价流程（商机 → 报价单 → 报价项）
- 审批工作流（模板/规则/步骤/条件）
- 邮件服务
- 系统设置

## 开发环境

- Visual Studio 2022 / VS Code
- .NET 8 SDK
- Node.js 18+
- SQL Server 2019+
- Redis (可选)

## 运行方式

### 后端
```bash
cd CpmServer
dotnet run
```
访问 `http://localhost:5001`

### 前端
```bash
cd CpmServer/cpm-web
npm install
npm run dev
```
访问 `http://localhost:5173`

### QAD 桥接程序
需要单独编译 `QadAuthBridge` 项目（.NET Framework 4.8, x86）。

## 配置文件

`CpmServer/appsettings.json` 中的敏感字段均为占位符。请创建 `CpmServer/appsettings.Development.json`（已被 `.gitignore` 排除）填入真实值，例如：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_HOST;Database=CpmDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Secret": "your-random-32+chars-secret"
  },
  "QAD": {
    "AppServerUrl": "AppServerDC://host:port/appname",
    "AppServerName": "appname",
    "LoginProcPath": "us/xx/xxchecklogina.p"
  }
}
```

## Progress OpenEdge DLL

QAD 集成依赖 Progress 公司的商用授权 DLL，**不随仓库分发**（已在 `.gitignore` 排除）：

- `CpmServer/Lib/Progress.Messages.dll`
- `CpmServer/Lib/Progress.o4glrt.dll`

请从你本地的 Progress OpenEdge 安装目录复制到 `CpmServer/Lib/` 后再编译 `QadAuthBridge` 项目。常见路径：

```
C:\Progress\OpenEdge\bin\Progress.Messages.dll
C:\Progress\OpenEdge\bin\Progress.o4glrt.dll
```
