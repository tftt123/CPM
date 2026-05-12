# CodeReview 修复计划

## 生成时间
2026-05-12

## 修复优先级

### P0 — 必须立即修复
1. **N+1 查询**：`GetAllStepsWithLatestCycleTimeAsync` 中 `pendingCount` 逐行查询
2. **数据库事务**：`SubmitCycleTimeChangeRequestAsync`、`ApproveAsync`、`RejectAsync` 等多 `SaveChanges` 无事务
3. **不安全解析**：`long.Parse(submitterId)` 可能抛 `FormatException`
4. **DTO 映射遗漏 Status**：已修复（第 1 轮）

### P1 — 尽快修复
5. **DateTime.Now → UtcNow**：后端全局替换
6. **Random → Random.Shared**：`GenerateTemplateCode` 线程安全
7. **PmProjectTrace 索引**：数据库性能优化
8. **前端 API baseURL 硬编码**：改为环境变量

### P2 — 优化项
9. **getPersonInCharge 性能**：使用 computed map 避免渲染期线性查找
10. **ApprovalCenter 分页**：防止大数据量加载
11. **nvarchar(max) 限制**：`StringLength` 约束
12. **ApprovalService 拆分**：单文件 1600+ 行，按职责拆分为 Template/Instance/Task Service

## 执行批次

### 批次一：后端核心修复（P0 + P1 后端部分）
- [ ] 修复 N+1 查询
- [ ] 添加数据库事务
- [ ] 替换 unsafe long.Parse
- [ ] 替换 DateTime.Now → UtcNow
- [ ] 替换 Random → Random.Shared
- [ ] 添加 PmProjectTrace 索引
- [ ] 生成 EF 迁移

### 批次二：前端修复（P1 前端部分 + P2）
- [ ] request.ts baseURL 改为环境变量
- [ ] ActualCycleTimeManage.vue getPersonInCharge 优化
- [ ] ApprovalCenter.vue 添加分页

### 批次三：重构（P2）
- [ ] ApprovalService 拆分（视时间决定是否执行）

## 验证清单
- [ ] 后端编译通过（0 errors）
- [ ] 前端编译通过（0 errors）
- [ ] EF 迁移成功应用
- [ ] 实际节拍维护页面正常加载
- [ ] 审批中心正常加载
- [ ] 提交审批流程正常执行
