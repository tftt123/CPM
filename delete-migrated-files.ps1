# =============================================
# CPM 项目拆分 - 删除原项目中已迁移的重复文件
# 运行前请确保所有新模块项目已正确创建
# =============================================

$baseDir = "C:\Users\adm\Qsync\AI\CPM_SrcCode\CpmServer"
$deletedCount = 0
$notFoundCount = 0

$filesToDelete = @(
    # SharedKernel
    "Common\ApiResult.cs",
    "Common\PagedResult.cs",
    "Common\BusinessException.cs",
    "Services\ICurrentUser.cs",
    "Services\CurrentUser.cs",
    # Core
    "Data\CpmDbContext.cs",
    "Models\CrmCustomer.cs",
    "Models\CrmProduct.cs",
    "Models\MfgCategory.cs",
    "Models\MfgEquipment.cs",
    "Models\MfgProcess.cs",
    "Models\MfgSubCategory.cs",
    "Models\QuoOpportunity.cs",
    "Models\QuoQuotation.cs",
    "Models\QuoQuotationItem.cs",
    "Models\SysApprovalCondition.cs",
    "Models\SysApprovalInstance.cs",
    "Models\SysApprovalInstanceTask.cs",
    "Models\SysApprovalRecord.cs",
    "Models\SysApprovalRule.cs",
    "Models\SysApprovalStep.cs",
    "Models\SysApprovalTemplate.cs",
    "Models\SysDept.cs",
    "Models\SysEmailConfig.cs",
    "Models\SysEmailLog.cs",
    "Models\SysEmailTemplate.cs",
    "Models\SysRole.cs",
    "Models\SysUser.cs",
    "Models\SysUserRole.cs",
    "Models\SysUserSite.cs",
    # Approval
    "Services\ApprovalService.cs",
    "Services\IApprovalService.cs",
    "Services\Approvers\FixedRoleResolver.cs",
    "Services\Approvers\FixedUserResolver.cs",
    "Services\Approvers\IApproverResolver.cs",
    "Services\Approvers\OrgTreeResolver.cs",
    "Services\Approvers\SubmitterResolver.cs",
    "Controllers\ApprovalController.cs",
    "DTOs\Quotation\ApprovalTemplateDto.cs",
    "DTOs\Quotation\ApprovalActionDto.cs",
    # Quotation
    "Services\QuotationService.cs",
    "Services\IQuotationService.cs",
    "Controllers\QuotationController.cs",
    "DTOs\Quotation\OpportunityDto.cs",
    "DTOs\Quotation\QuotationDto.cs",
    # Email (已迁移到 SharedKernel)
    "Services\IEmailService.cs",
    "DTOs\Quotation\EmailConfigDto.cs"
)

foreach ($file in $filesToDelete) {
    $fullPath = Join-Path $baseDir $file
    if (Test-Path $fullPath) {
        Remove-Item $fullPath -Force
        Write-Host "Deleted: $file" -ForegroundColor Green
        $deletedCount++
    } else {
        Write-Host "Not found: $file" -ForegroundColor Yellow
        $notFoundCount++
    }
}

# 清理空目录
$emptyDirs = @("Data", "Models", "Services\Approvers")
foreach ($dir in $emptyDirs) {
    $fullDir = Join-Path $baseDir $dir
    if (Test-Path $fullDir) {
        $items = Get-ChildItem $fullDir -Recurse -ErrorAction SilentlyContinue
        if (-not $items) {
            Remove-Item $fullDir -Recurse -Force
            Write-Host "Removed empty dir: $dir" -ForegroundColor Cyan
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor White
Write-Host "Total deleted: $deletedCount" -ForegroundColor Green
Write-Host "Not found: $notFoundCount" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor White
