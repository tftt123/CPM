# VS Code 工作目录设置指南

> 适用于 CPM 项目（.NET 8 后端 + Vue 3 前端）

---

## 一、集成终端默认工作目录

在 `.vscode/settings.json` 中配置：

```json
{
  "terminal.integrated.cwd": "${workspaceFolder}"
}
```

常用选项：
- `"${workspaceFolder}"` — 工作区根目录（默认）
- `"${workspaceFolder}/CpmServer"` — 后端项目目录
- `"${workspaceFolder}/CpmServer/cpm-web"` — 前端项目目录

### 按终端类型分别设置
```json
{
  "terminal.integrated.profiles.windows": {
    "Backend Terminal": {
      "path": "cmd.exe",
      "args": ["/K", "cd /d ${workspaceFolder}\\CpmServer"]
    },
    "Frontend Terminal": {
      "path": "cmd.exe", 
      "args": ["/K", "cd /d ${workspaceFolder}\\CpmServer\\cpm-web"]
    }
  }
}
```

---

## 二、调试时的工作目录（launch.json）

在 `.vscode/launch.json` 中通过 `cwd` 字段指定：

### 1. 调试 .NET 8 后端 API
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (API)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/CpmServer/bin/Debug/net8.0/CpmServer.dll",
      "args": [],
      "cwd": "${workspaceFolder}/CpmServer",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

### 2. 调试前端（Vite + Chrome）
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch Chrome (Frontend)",
      "type": "chrome",
      "request": "launch",
      "url": "http://localhost:5173",
      "webRoot": "${workspaceFolder}/CpmServer/cpm-web/src",
      "cwd": "${workspaceFolder}/CpmServer/cpm-web"
    }
  ]
}
```

### 3. 使用 compound 同时调试前后端
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (API)",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/CpmServer/bin/Debug/net8.0/CpmServer.dll",
      "cwd": "${workspaceFolder}/CpmServer",
      "env": { "ASPNETCORE_ENVIRONMENT": "Development" }
    },
    {
      "name": "Launch Chrome (Frontend)",
      "type": "chrome",
      "request": "launch",
      "url": "http://localhost:5173",
      "webRoot": "${workspaceFolder}/CpmServer/cpm-web/src",
      "cwd": "${workspaceFolder}/CpmServer/cpm-web"
    }
  ],
  "compounds": [
    {
      "name": "Full Stack Debug",
      "configurations": [".NET Core Launch (API)", "Launch Chrome (Frontend)"],
      "stopAll": true
    }
  ]
}
```

---

## 三、任务运行时的工作目录（tasks.json）

在 `.vscode/tasks.json` 中通过 `options.cwd` 指定：

### 示例：在 CpmServer 目录执行 dotnet build
```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build-backend",
      "command": "dotnet",
      "type": "process",
      "args": ["build"],
      "options": {
        "cwd": "${workspaceFolder}/CpmServer"
      },
      "problemMatcher": "$msCompile"
    },
    {
      "label": "dev-frontend",
      "type": "shell",
      "command": "npm run dev",
      "options": {
        "cwd": "${workspaceFolder}/CpmServer/cpm-web"
      }
    }
  ]
}
```

---

## 四、变量速查表

| 变量 | 含义 |
|------|------|
| `${workspaceFolder}` | 当前打开的工作区根目录 |
| `${workspaceFolderBasename}` | 工作区目录名（不含路径）|
| `${file}` | 当前打开的文件绝对路径 |
| `${fileDirname}` | 当前打开文件所在目录 |
| `${relativeFile}` | 相对于工作区根目录的文件路径 |
| `${cwd}` | 启动时的当前工作目录 |
| `${env:NAME}` | 环境变量 |

---

## 五、快速创建配置

1. 在项目根目录下新建文件夹 `.vscode`
2. 创建 `settings.json`、`launch.json`、`tasks.json`
3. 将上述对应 JSON 复制进去
4. 按 `F5` 即可按设定的工作目录启动调试
