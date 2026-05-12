# 全项目修复计划（除 Docker 外全部执行）

## Batch 1: 基础设施修复（Git、配置、代码规范）
- [ ] 1. 移除 appsettings.Development.json 中的 secrets，改用 User Secrets
- [ ] 2. 清理 .gitignore 重复条目和格式错误
- [ ] 3. 添加后端 .editorconfig
- [ ] 4. 添加 CHANGELOG.md

## Batch 2: API 质量提升（版本控制、验证、健康检查）
- [ ] 5. API 版本控制（v1）
- [ ] 6. DTO 验证属性（Required、StringLength、Range）
- [ ] 7. Health Check 端点
- [ ] 8. Correlation ID 中间件

## Batch 3: 架构改进（Repository/UoW、Service 拆分）
- [ ] 9. Repository + UnitOfWork 模式基础
- [ ] 10. ApprovalService 拆分为多个 Service

## Batch 4: 安全与认证
- [ ] 11. Refresh Token 机制
- [ ] 12. 细粒度权限控制（Policy-based）

## Batch 5: 日志与可观测性
- [ ] 13. Structured Logging（Serilog）
- [ ] 14. 全局异常处理中间件提取为独立类

## Batch 6: 前端质量
- [ ] 15. 前端 API 返回类型清理（减少 any）
- [ ] 16. 切换语言/站点不刷新页面

## Batch 7: 自动化
- [ ] 17. CI/CD GitHub Actions 管道
- [ ] 18. OpenAPI 静态导出脚本

## Batch 8: 测试
- [ ] 19. 创建测试项目骨架 + 核心单元测试

## 跳过项
- [ ] ~~Docker 支持~~（用户明确要求跳过）
